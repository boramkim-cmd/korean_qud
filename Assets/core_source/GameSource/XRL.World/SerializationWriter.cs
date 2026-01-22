using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Genkit;
using XRL.Collections;
using XRL.Serialization;

namespace XRL.World;

public sealed class SerializationWriter : BinaryWriter
{
	private class SingletonTypeWrapper : IEquatable<SingletonTypeWrapper>
	{
		public Type WrappedType;

		public SingletonTypeWrapper()
		{
		}

		public SingletonTypeWrapper(SingletonTypeWrapper Wrapper)
		{
			WrappedType = Wrapper.WrappedType;
		}

		public bool Equals(SingletonTypeWrapper other)
		{
			if (other != null)
			{
				return (object)WrappedType == other.WrappedType;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is SingletonTypeWrapper singletonTypeWrapper)
			{
				return (object)WrappedType == singletonTypeWrapper.WrappedType;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return WrappedType.GetHashCode();
		}
	}

	public class Block : IDisposable
	{
		private SerializationWriter Writer;

		private Stream ParentStream;

		private MemoryStream Stream = new MemoryStream();

		public void Start(SerializationWriter Writer)
		{
			this.Writer = Writer;
			ParentStream = Writer.OutStream;
			Writer.OutStream = Stream;
		}

		public void End()
		{
			if (ParentStream != null)
			{
				Writer.OutStream = ParentStream;
				int num = (int)Stream.Position;
				Writer.WriteOptimized(num);
				if (num > 0)
				{
					byte[] buffer = Stream.GetBuffer();
					ParentStream.Write(buffer, 0, num);
					Stream.Position = 0L;
				}
				ParentStream = null;
			}
		}

		public void Reset()
		{
			Stream.Position = 0L;
		}

		public void Dispose()
		{
			if (Writer != null)
			{
				End();
				Writer.Blocks.Push(this);
				Writer = null;
			}
		}
	}

	private sealed class UniqueStringList
	{
		private const float LoadFactor = 0.72f;

		private static readonly int[] primeNumberList = new int[19]
		{
			389, 1543, 6151, 24593, 98317, 196613, 393241, 786433, 1572869, 3145739,
			6291469, 12582917, 25165843, 50331653, 100663319, 201326611, 402653189, 805306457, 1610612741
		};

		private string[] stringList;

		private int[] buckets;

		private int bucketListCapacity;

		private int stringListIndex;

		private int loadLimit;

		private int primeNumberListIndex;

		public string this[int index] => stringList[index];

		public int Count => stringListIndex;

		public UniqueStringList()
		{
			bucketListCapacity = primeNumberList[primeNumberListIndex++];
			stringList = new string[bucketListCapacity];
			buckets = new int[bucketListCapacity];
			loadLimit = (int)((float)bucketListCapacity * 0.72f);
		}

		public int Add(string value)
		{
			int bucketIndex = getBucketIndex(value);
			int num = buckets[bucketIndex];
			if (num == 0)
			{
				stringList[stringListIndex++] = value;
				buckets[bucketIndex] = stringListIndex;
				if (stringListIndex > loadLimit)
				{
					expand();
				}
				return stringListIndex - 1;
			}
			return num - 1;
		}

		public void Clear()
		{
			Array.Clear(buckets, 0, buckets.Length);
			Array.Clear(stringList, 0, stringList.Length);
			stringListIndex = 0;
		}

		private void expand()
		{
			bucketListCapacity = primeNumberList[primeNumberListIndex++];
			buckets = new int[bucketListCapacity];
			string[] array = new string[bucketListCapacity];
			stringList.CopyTo(array, 0);
			stringList = array;
			reindex();
		}

		private void reindex()
		{
			loadLimit = (int)((float)bucketListCapacity * 0.72f);
			for (int i = 0; i < stringListIndex; i++)
			{
				int bucketIndex = getBucketIndex(stringList[i]);
				buckets[bucketIndex] = i + 1;
			}
		}

		private int getBucketIndex(string value)
		{
			int num = (value.GetHashCode() & 0x7FFFFFFF) % bucketListCapacity;
			int num2 = ((num <= 1) ? 1 : num);
			int num3 = bucketListCapacity;
			while (0 < num3--)
			{
				int num4 = buckets[num];
				if (num4 == 0)
				{
					return num;
				}
				if (string.CompareOrdinal(value, stringList[num4 - 1]) == 0)
				{
					return num;
				}
				num = (num + num2) % bucketListCapacity;
			}
			throw new InvalidOperationException("Failed to locate a bucket.");
		}
	}

	public static int DefaultCapacity = 1024;

	public static bool DefaultOptimizeForSize = true;

	public static bool DefaultPreserveDecimalScale = false;

	private static List<IFastSerializationTypeSurrogate> typeSurrogates = null;

	private static HashSet<Type> BinaryFormattedTypes = new HashSet<Type>();

	internal static readonly BitVector32.Section DateYearMask = BitVector32.CreateSection(127);

	internal static readonly BitVector32.Section DateMonthMask = BitVector32.CreateSection(12, DateYearMask);

	internal static readonly BitVector32.Section DateDayMask = BitVector32.CreateSection(31, DateMonthMask);

	internal static readonly BitVector32.Section DateHasTimeOrKindMask = BitVector32.CreateSection(1, DateDayMask);

	internal static readonly BitVector32.Section IsNegativeSection = BitVector32.CreateSection(1);

	internal static readonly BitVector32.Section HasDaysSection = BitVector32.CreateSection(1, IsNegativeSection);

	internal static readonly BitVector32.Section HasTimeSection = BitVector32.CreateSection(1, HasDaysSection);

	internal static readonly BitVector32.Section HasSecondsSection = BitVector32.CreateSection(1, HasTimeSection);

	internal static readonly BitVector32.Section HasMillisecondsSection = BitVector32.CreateSection(1, HasSecondsSection);

	internal static readonly BitVector32.Section HoursSection = BitVector32.CreateSection(23, HasMillisecondsSection);

	internal static readonly BitVector32.Section MinutesSection = BitVector32.CreateSection(59, HoursSection);

	internal static readonly BitVector32.Section SecondsSection = BitVector32.CreateSection(59, MinutesSection);

	internal static readonly BitVector32.Section MillisecondsSection = BitVector32.CreateSection(127, SecondsSection);

	public const short HighestOptimizable16BitValue = 127;

	public const int HighestOptimizable32BitValue = 2097151;

	public const long HighestOptimizable64BitValue = 562949953421311L;

	internal const short OptimizationFailure16BitValue = 16384;

	internal const int OptimizationFailure32BitValue = 268435456;

	internal const long OptimizationFailure64BitValue = 72057594037927936L;

	internal const ushort GameObjectStartValue = 64206;

	internal const ushort GameObjectEndValue = 44203;

	internal const ushort GameObjectFinishValue = 52958;

	private static readonly BitArray FullyOptimizableTypedArray = new BitArray(0);

	private static SerializationWriter Instance = new SerializationWriter(FastSerialization.SharedCache);

	public MemoryStream Stream;

	private byte[] Buffer;

	private Stack<Block> Blocks;

	private HashSet<Assembly> LocalAssemblies;

	private UniqueStringList TokenStrings;

	private Rack<object> TokenObjects;

	private Dictionary<object, int> TokenByObject;

	private Rack<ITokenized> Tokenized;

	private Rack<Type> TokenTypes;

	private Dictionary<Type, int> TokenByType;

	private Rack<GameObject> TokenGameObjects;

	private Dictionary<GameObject, int> TokenByGameObject;

	private Rack<EventRegistry> EventRegistries;

	private Rack<GameObjectReference> GameObjectReferences;

	private long StringPosition;

	private long ObjectPosition;

	private long TokenizedPosition;

	private long TypePosition;

	private long GameObjectPosition;

	private long GameObjectReferencePosition;

	private long EventRegistryPosition;

	public int FileVersion;

	public bool SerializePlayer = true;

	private bool optimizeForSize = DefaultOptimizeForSize;

	private bool preserveDecimalScale = DefaultPreserveDecimalScale;

	private SingletonTypeWrapper WrapperCache;

	private static BinaryFormatter formatter = null;

	public static List<IFastSerializationTypeSurrogate> TypeSurrogates
	{
		get
		{
			if (typeSurrogates == null)
			{
				typeSurrogates = new List<IFastSerializationTypeSurrogate>();
				typeSurrogates.Add(new ColorSerializationSurrogate());
			}
			return typeSurrogates;
		}
	}

	public bool OptimizeForSize
	{
		get
		{
			return optimizeForSize;
		}
		set
		{
			optimizeForSize = value;
		}
	}

	public bool PreserveDecimalScale
	{
		get
		{
			return preserveDecimalScale;
		}
		set
		{
			preserveDecimalScale = value;
		}
	}

	public long Position
	{
		get
		{
			return OutStream.Position;
		}
		set
		{
			OutStream.Position = value;
		}
	}

	public static bool TryGetShared(out SerializationWriter Reader)
	{
		if (!GameManager.IsOnGameContext())
		{
			MetricsManager.LogException("SerializationWriter::TryGetShared not on game context!", new Exception("SerializationWriter::TryGetShared not on game context!"));
		}
		if (FastSerialization.SharedCache.Reserved)
		{
			Reader = null;
			return false;
		}
		Reader = Instance;
		return FastSerialization.SharedCache.Reserved = true;
	}

	public static void ReleaseShared()
	{
		Instance.Clear();
		FastSerialization.SharedCache.Reserved = false;
	}

	public static SerializationWriter Get()
	{
		if (TryGetShared(out var Reader))
		{
			return Reader;
		}
		return new SerializationWriter(new FastSerialization.Cache(0));
	}

	public static void Release(SerializationWriter Writer)
	{
		if (Writer == Instance)
		{
			ReleaseShared();
		}
		else
		{
			Writer.Dispose();
		}
	}

	public SerializationWriter(MemoryStream Stream)
		: base(Stream, FastSerialization.Encoding)
	{
		this.Stream = Stream;
		if (!Stream.CanSeek)
		{
			throw new InvalidOperationException("Stream must be seekable");
		}
	}

	public SerializationWriter(FastSerialization.Cache Cache)
		: this(Cache.MemoryStream)
	{
		Initialize(Cache);
	}

	public void Start(int FileVersion, bool SerializePlayer = false)
	{
		this.FileVersion = FileVersion;
		this.SerializePlayer = SerializePlayer;
		ReserveHeader();
		LocalAssemblies.Add(typeof(int).Assembly);
		LocalAssemblies.Add(typeof(GameObject).Assembly);
		foreach (Assembly modAssembly in ModManager.ModAssemblies)
		{
			LocalAssemblies.Add(modAssembly);
		}
		SetPlayer(The.Player);
		WriteModVersions();
	}

	public void Initialize(FastSerialization.Cache Cache)
	{
		if (Buffer == null)
		{
			Buffer = new byte[16];
			Blocks = new Stack<Block>();
			TokenObjects = Cache.Objects;
			TokenByObject = new Dictionary<object, int>();
			TokenStrings = new UniqueStringList();
			TokenTypes = Cache.Types;
			TokenByType = new Dictionary<Type, int>(768);
			TokenGameObjects = Cache.GameObjects;
			GameObjectReferences = Cache.GameObjectReferences;
			TokenByGameObject = new Dictionary<GameObject, int>(2560);
			EventRegistries = Cache.EventRegistries;
			Tokenized = Cache.Tokenized;
			formatter = null;
			LocalAssemblies = new HashSet<Assembly>(ModManager.ModAssemblies.Count + 2);
		}
	}

	public void Clear()
	{
		Stream.SetLength(0L);
		TokenObjects.Clear();
		TokenByObject.Clear();
		TokenStrings.Clear();
		TokenTypes.Clear();
		TokenByType.Clear();
		TokenGameObjects.Clear();
		GameObjectReferences.Clear();
		TokenByGameObject.Clear();
		EventRegistries.Clear();
		Tokenized.Clear();
		LocalAssemblies.Clear();
		formatter = null;
	}

	public void SetPlayer(GameObject Object)
	{
		if (TokenGameObjects.Count == 0)
		{
			TokenGameObjects.Add(Object);
		}
		else
		{
			TokenGameObjects[0] = Object;
		}
		if (Object != null)
		{
			TokenByGameObject[Object] = 0;
		}
	}

	public void Write(ArrayList value)
	{
		if (value == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		writeTypeCode(SerializedType.ArrayListType);
		WriteOptimized(value);
	}

	public void Write(BitArray value)
	{
		if (value == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		writeTypeCode(SerializedType.BitArrayType);
		WriteOptimized(value);
	}

	public void Write(BitVector32 value)
	{
		base.Write(value.Data);
	}

	public void Write(DateTime value)
	{
		Write(value.ToBinary());
	}

	public void Write(Guid value)
	{
		Span<byte> destination = Buffer.AsSpan(0, 16);
		if (value.TryWriteBytes(destination))
		{
			OutStream.Write(Buffer, 0, 16);
		}
		else
		{
			base.Write(value.ToByteArray());
		}
	}

	public void Write(IOwnedDataSerializable target, object context)
	{
		target.SerializeOwnedData(this, context);
	}

	public void WriteObject(object value)
	{
		try
		{
			if (value == null)
			{
				writeTypeCode(SerializedType.NullType);
				return;
			}
			if (value is string)
			{
				WriteOptimized((string)value);
				return;
			}
			if (value is int num)
			{
				switch (num)
				{
				case 0:
					writeTypeCode(SerializedType.ZeroInt32Type);
					return;
				case -1:
					writeTypeCode(SerializedType.MinusOneInt32Type);
					return;
				case 1:
					writeTypeCode(SerializedType.OneInt32Type);
					return;
				}
				if (optimizeForSize)
				{
					if (num > 0)
					{
						if (num <= 2097151)
						{
							writeTypeCode(SerializedType.OptimizedInt32Type);
							write7bitEncodedSigned32BitValue(num);
							return;
						}
					}
					else
					{
						int num2 = -(num + 1);
						if (num2 <= 2097151)
						{
							writeTypeCode(SerializedType.OptimizedInt32NegativeType);
							write7bitEncodedSigned32BitValue(num2);
							return;
						}
					}
				}
				writeTypeCode(SerializedType.Int32Type);
				Write(num);
				return;
			}
			if (value == DBNull.Value)
			{
				writeTypeCode(SerializedType.DBNullType);
				return;
			}
			if (value is bool)
			{
				writeTypeCode(((bool)value) ? SerializedType.BooleanTrueType : SerializedType.BooleanFalseType);
				return;
			}
			if (value is decimal num3)
			{
				if (num3 == 0m)
				{
					writeTypeCode(SerializedType.ZeroDecimalType);
					return;
				}
				if (num3 == 1m)
				{
					writeTypeCode(SerializedType.OneDecimalType);
					return;
				}
				writeTypeCode(SerializedType.DecimalType);
				WriteOptimized(num3);
				return;
			}
			if (value is DateTime dateTime)
			{
				if (dateTime == DateTime.MinValue)
				{
					writeTypeCode(SerializedType.MinDateTimeType);
				}
				else if (dateTime == DateTime.MaxValue)
				{
					writeTypeCode(SerializedType.MaxDateTimeType);
				}
				else if (optimizeForSize && dateTime.Ticks % 10000 == 0L)
				{
					writeTypeCode(SerializedType.OptimizedDateTimeType);
					WriteOptimized(dateTime);
				}
				else
				{
					writeTypeCode(SerializedType.DateTimeType);
					Write(dateTime);
				}
				return;
			}
			if (value is double num4)
			{
				if (num4 == 0.0)
				{
					writeTypeCode(SerializedType.ZeroDoubleType);
					return;
				}
				if (num4 == 1.0)
				{
					writeTypeCode(SerializedType.OneDoubleType);
					return;
				}
				writeTypeCode(SerializedType.DoubleType);
				Write(num4);
				return;
			}
			if (value is float num5)
			{
				if (num5 == 0f)
				{
					writeTypeCode(SerializedType.ZeroSingleType);
					return;
				}
				if (num5 == 1f)
				{
					writeTypeCode(SerializedType.OneSingleType);
					return;
				}
				writeTypeCode(SerializedType.SingleType);
				Write(num5);
				return;
			}
			if (value is short num6)
			{
				switch (num6)
				{
				case 0:
					writeTypeCode(SerializedType.ZeroInt16Type);
					return;
				case -1:
					writeTypeCode(SerializedType.MinusOneInt16Type);
					return;
				case 1:
					writeTypeCode(SerializedType.OneInt16Type);
					return;
				}
				if (optimizeForSize)
				{
					if (num6 > 0)
					{
						if (num6 <= 127)
						{
							writeTypeCode(SerializedType.OptimizedInt16Type);
							write7bitEncodedSigned32BitValue(num6);
							return;
						}
					}
					else
					{
						int num7 = -(num6 + 1);
						if (num7 <= 127)
						{
							writeTypeCode(SerializedType.OptimizedInt16NegativeType);
							write7bitEncodedSigned32BitValue(num7);
							return;
						}
					}
				}
				writeTypeCode(SerializedType.Int16Type);
				Write(num6);
				return;
			}
			if (value is Guid guid)
			{
				if (guid == Guid.Empty)
				{
					writeTypeCode(SerializedType.EmptyGuidType);
					return;
				}
				writeTypeCode(SerializedType.GuidType);
				Write(guid);
				return;
			}
			if (value is long num8)
			{
				switch (num8)
				{
				case 0L:
					writeTypeCode(SerializedType.ZeroInt64Type);
					return;
				case -1L:
					writeTypeCode(SerializedType.MinusOneInt64Type);
					return;
				case 1L:
					writeTypeCode(SerializedType.OneInt64Type);
					return;
				}
				if (optimizeForSize)
				{
					if (num8 > 0)
					{
						if (num8 <= 562949953421311L)
						{
							writeTypeCode(SerializedType.OptimizedInt64Type);
							write7bitEncodedSigned64BitValue(num8);
							return;
						}
					}
					else
					{
						long num9 = -(num8 + 1);
						if (num9 <= 562949953421311L)
						{
							writeTypeCode(SerializedType.OptimizedInt64NegativeType);
							write7bitEncodedSigned64BitValue(num9);
							return;
						}
					}
				}
				writeTypeCode(SerializedType.Int64Type);
				Write(num8);
				return;
			}
			if (value is byte b)
			{
				switch (b)
				{
				case 0:
					writeTypeCode(SerializedType.ZeroByteType);
					break;
				case 1:
					writeTypeCode(SerializedType.OneByteType);
					break;
				default:
					writeTypeCode(SerializedType.ByteType);
					Write(b);
					break;
				}
				return;
			}
			if (value is char c)
			{
				switch (c)
				{
				case '\0':
					writeTypeCode(SerializedType.ZeroCharType);
					break;
				case '\u0001':
					writeTypeCode(SerializedType.OneCharType);
					break;
				default:
					writeTypeCode(SerializedType.CharType);
					Write(c);
					break;
				}
				return;
			}
			if (value is sbyte b2)
			{
				switch (b2)
				{
				case 0:
					writeTypeCode(SerializedType.ZeroSByteType);
					break;
				case 1:
					writeTypeCode(SerializedType.OneSByteType);
					break;
				default:
					writeTypeCode(SerializedType.SByteType);
					Write(b2);
					break;
				}
				return;
			}
			if (value is uint num10)
			{
				switch (num10)
				{
				case 0u:
					writeTypeCode(SerializedType.ZeroUInt32Type);
					return;
				case 1u:
					writeTypeCode(SerializedType.OneUInt32Type);
					return;
				}
				if (optimizeForSize && num10 <= 2097151)
				{
					writeTypeCode(SerializedType.OptimizedUInt32Type);
					write7bitEncodedUnsigned32BitValue(num10);
				}
				else
				{
					writeTypeCode(SerializedType.UInt32Type);
					Write(num10);
				}
				return;
			}
			if (value is ushort num11)
			{
				switch (num11)
				{
				case 0:
					writeTypeCode(SerializedType.ZeroUInt16Type);
					return;
				case 1:
					writeTypeCode(SerializedType.OneUInt16Type);
					return;
				}
				if (optimizeForSize && num11 <= 127)
				{
					writeTypeCode(SerializedType.OptimizedUInt16Type);
					write7bitEncodedUnsigned32BitValue(num11);
				}
				else
				{
					writeTypeCode(SerializedType.UInt16Type);
					Write(num11);
				}
				return;
			}
			if (value is ulong num12)
			{
				switch (num12)
				{
				case 0uL:
					writeTypeCode(SerializedType.ZeroUInt64Type);
					return;
				case 1uL:
					writeTypeCode(SerializedType.OneUInt64Type);
					return;
				}
				if (optimizeForSize && num12 <= 562949953421311L)
				{
					writeTypeCode(SerializedType.OptimizedUInt64Type);
					WriteOptimized(num12);
				}
				else
				{
					writeTypeCode(SerializedType.UInt64Type);
					Write(num12);
				}
				return;
			}
			if (value is TimeSpan timeSpan)
			{
				if (timeSpan == TimeSpan.Zero)
				{
					writeTypeCode(SerializedType.ZeroTimeSpanType);
				}
				else if (optimizeForSize && timeSpan.Ticks % 10000 == 0L)
				{
					writeTypeCode(SerializedType.OptimizedTimeSpanType);
					WriteOptimized(timeSpan);
				}
				else
				{
					writeTypeCode(SerializedType.TimeSpanType);
					Write(timeSpan);
				}
				return;
			}
			if (value is GameObject gameObject)
			{
				writeTypeCode(SerializedType.GameObjectType);
				WriteGameObject(gameObject);
				return;
			}
			if (value is IComposite value2)
			{
				SerializeComposite(value2);
				return;
			}
			if (value is Array { Rank: 1 } array)
			{
				writeTypedArray(array, storeType: true);
				return;
			}
			if (value is Type type)
			{
				writeTypeCode(SerializedType.TypeType);
				WriteTokenized(type);
				return;
			}
			if (value is BitArray)
			{
				writeTypeCode(SerializedType.BitArrayType);
				WriteOptimized((BitArray)value);
				return;
			}
			if (value is BitVector32)
			{
				writeTypeCode(SerializedType.BitVector32Type);
				Write((BitVector32)value);
				return;
			}
			if (value is SingletonTypeWrapper)
			{
				writeTypeCode(SerializedType.SingleInstanceType);
				WriteTokenized(((SingletonTypeWrapper)value).WrappedType);
				return;
			}
			if (value is List<string>)
			{
				writeTypeCode(SerializedType.StringListType);
				Write(value as List<string>);
				return;
			}
			if (value is ArrayList)
			{
				writeTypeCode(SerializedType.ArrayListType);
				WriteOptimized(value as ArrayList);
				return;
			}
			if (value is IDictionary)
			{
				writeTypeCode(SerializedType.IDictionaryType);
				WriteTokenized(value.GetType());
				Write((IDictionary)value);
				return;
			}
			if (value is IList && !(value is Array))
			{
				writeTypeCode(SerializedType.IListType);
				WriteTokenized(value.GetType());
				Write((IList)value);
				return;
			}
			if (value is Enum)
			{
				Type type2 = value.GetType();
				Type underlyingType = Enum.GetUnderlyingType(type2);
				if (underlyingType == typeof(int) || underlyingType == typeof(uint))
				{
					uint num13 = ((underlyingType == typeof(int)) ? ((uint)(int)value) : ((uint)value));
					if (num13 <= 2097151)
					{
						writeTypeCode(SerializedType.OptimizedEnumType);
						WriteTokenized(type2);
						write7bitEncodedUnsigned32BitValue(num13);
					}
					else
					{
						writeTypeCode(SerializedType.EnumType);
						WriteTokenized(type2);
						Write(num13);
					}
					return;
				}
				if (underlyingType == typeof(long) || underlyingType == typeof(ulong))
				{
					ulong num14 = ((underlyingType == typeof(long)) ? ((ulong)(long)value) : ((ulong)value));
					if (num14 <= 562949953421311L)
					{
						writeTypeCode(SerializedType.OptimizedEnumType);
						WriteTokenized(type2);
						write7bitEncodedUnsigned64BitValue(num14);
					}
					else
					{
						writeTypeCode(SerializedType.EnumType);
						WriteTokenized(type2);
						Write(num14);
					}
					return;
				}
				writeTypeCode(SerializedType.EnumType);
				WriteTokenized(type2);
				if (underlyingType == typeof(byte))
				{
					Write((byte)value);
				}
				else if (underlyingType == typeof(sbyte))
				{
					Write((sbyte)value);
				}
				else if (underlyingType == typeof(short))
				{
					Write((short)value);
				}
				else
				{
					Write((ushort)value);
				}
				return;
			}
			Type type3 = value.GetType();
			if (isTypeRecreatable(type3))
			{
				writeTypeCode(SerializedType.OwnedDataSerializableAndRecreatableType);
				WriteTokenized(type3);
				Write((IOwnedDataSerializable)value, null);
				return;
			}
			IFastSerializationTypeSurrogate fastSerializationTypeSurrogate;
			if ((fastSerializationTypeSurrogate = findSurrogateForType(type3)) != null)
			{
				writeTypeCode(SerializedType.SurrogateHandledType);
				WriteTokenized(type3);
				fastSerializationTypeSurrogate.Serialize(this, value);
				return;
			}
			try
			{
				writeTypeCode(SerializedType.OtherType);
				createBinaryFormatter().Serialize(OutStream, value);
				if (BinaryFormattedTypes.Add(type3) && type3.Assembly != Assembly.GetExecutingAssembly())
				{
					MetricsManager.LogAssemblyError(type3, $"Using binary formatter for type '{type3}', convert it to an IComposite or serialize it manually.");
				}
			}
			catch (Exception innerException)
			{
				throw new Exception($"exception serializing value '{value}' of type '{type3}'", innerException);
			}
		}
		catch (Exception innerException2)
		{
			throw new Exception("exception serializing object " + value.ToString() + " of " + value.GetType().Name + " : ", innerException2);
		}
	}

	public override void Write(string value)
	{
		WriteOptimized(value);
	}

	public void Write(TimeSpan value)
	{
		Write(value.Ticks);
	}

	public void Write(Type value, bool fullyQualified)
	{
		if (value == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		writeTypeCode(SerializedType.TypeType);
		WriteTokenized(value);
	}

	public void WriteOptimized(ArrayList value)
	{
		writeObjectArray(value.ToArray());
	}

	public void WriteOptimized(BitArray value)
	{
		write7bitEncodedSigned32BitValue(value.Length);
		if (value.Length > 0)
		{
			byte[] array = new byte[(value.Length + 7) / 8];
			value.CopyTo(array, 0);
			base.Write(array, 0, array.Length);
		}
	}

	public void WriteOptimized(BitVector32 value)
	{
		write7bitEncodedSigned32BitValue(value.Data);
	}

	public void WriteOptimized(DateTime value)
	{
		BitVector32 bitVector = new BitVector32
		{
			[DateYearMask] = value.Year,
			[DateMonthMask] = value.Month,
			[DateDayMask] = value.Day
		};
		int num = 0;
		bool flag = value != value.Date;
		num = (int)value.Kind;
		flag = flag || num != 0;
		bitVector[DateHasTimeOrKindMask] = (flag ? 1 : 0);
		int data = bitVector.Data;
		Write((byte)data);
		Write((byte)(data >> 8));
		Write((byte)(data >> 16));
		if (flag)
		{
			encodeTimeSpan(value.TimeOfDay, partOfDateTime: true, num);
		}
	}

	public void WriteOptimized(decimal value)
	{
		int[] bits = decimal.GetBits(value);
		byte b = (byte)(bits[3] >> 16);
		byte b2 = 0;
		if (b != 0 && !preserveDecimalScale && optimizeForSize)
		{
			decimal num = decimal.Truncate(value);
			if (num == value)
			{
				bits = decimal.GetBits(num);
				b = 0;
			}
		}
		if ((bits[3] & int.MinValue) != 0)
		{
			b2 |= 1;
		}
		if (b != 0)
		{
			b2 |= 2;
		}
		if (bits[0] == 0)
		{
			b2 |= 4;
		}
		else if (bits[0] <= 2097151 && bits[0] >= 0)
		{
			b2 |= 0x20;
		}
		if (bits[1] == 0)
		{
			b2 |= 8;
		}
		else if (bits[1] <= 2097151 && bits[1] >= 0)
		{
			b2 |= 0x40;
		}
		if (bits[2] == 0)
		{
			b2 |= 0x10;
		}
		else if (bits[2] <= 2097151 && bits[2] >= 0)
		{
			b2 |= 0x80;
		}
		Write(b2);
		if (b != 0)
		{
			Write(b);
		}
		if ((b2 & 4) == 0)
		{
			if ((b2 & 0x20) != 0)
			{
				write7bitEncodedSigned32BitValue(bits[0]);
			}
			else
			{
				Write(bits[0]);
			}
		}
		if ((b2 & 8) == 0)
		{
			if ((b2 & 0x40) != 0)
			{
				write7bitEncodedSigned32BitValue(bits[1]);
			}
			else
			{
				Write(bits[1]);
			}
		}
		if ((b2 & 0x10) == 0)
		{
			if ((b2 & 0x80) != 0)
			{
				write7bitEncodedSigned32BitValue(bits[2]);
			}
			else
			{
				Write(bits[2]);
			}
		}
	}

	public void WriteOptimized(short value)
	{
		write7bitEncodedSigned32BitValue(value);
	}

	public void WriteOptimized(int value)
	{
		write7bitEncodedSigned32BitValue(value);
	}

	public void WriteOptimized(long value)
	{
		write7bitEncodedSigned64BitValue(value);
	}

	public void WriteOptimized(string value)
	{
		if (value == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (value.Length == 1)
		{
			char c = value[0];
			switch (c)
			{
			case 'Y':
				writeTypeCode(SerializedType.YStringType);
				break;
			case 'N':
				writeTypeCode(SerializedType.NStringType);
				break;
			case ' ':
				writeTypeCode(SerializedType.SingleSpaceType);
				break;
			default:
				writeTypeCode(SerializedType.SingleCharStringType);
				Write(c);
				break;
			}
		}
		else if (value.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyStringType);
		}
		else
		{
			int num = TokenStrings.Add(value);
			Write((byte)(num % 128));
			write7bitEncodedSigned32BitValue(num >> 7);
		}
	}

	public void WriteOptimized(TimeSpan value)
	{
		encodeTimeSpan(value, partOfDateTime: false, 0);
	}

	public void WriteOptimized(Type value)
	{
		Assembly assembly = value.Assembly;
		if (LocalAssemblies.Contains(assembly))
		{
			WriteOptimized(value.FullName);
		}
		else
		{
			WriteOptimized(value.AssemblyQualifiedName);
		}
	}

	public void WriteDirect(Type value)
	{
		Assembly assembly = value.Assembly;
		if (LocalAssemblies.Contains(assembly))
		{
			base.Write(value.FullName);
		}
		else
		{
			base.Write(value.AssemblyQualifiedName);
		}
	}

	[CLSCompliant(false)]
	public void WriteOptimized(ushort value)
	{
		write7bitEncodedUnsigned32BitValue(value);
	}

	[CLSCompliant(false)]
	public void WriteOptimized(uint value)
	{
		write7bitEncodedUnsigned32BitValue(value);
	}

	[CLSCompliant(false)]
	public void WriteOptimized(ulong value)
	{
		write7bitEncodedUnsigned64BitValue(value);
	}

	public void Write(bool[] values)
	{
		WriteOptimized(values);
	}

	public override void Write(byte[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	public override void Write(char[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	public void Write(DateTime[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	public void Write(decimal[] values)
	{
		WriteOptimized(values);
	}

	public void Write(double[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	public void Write(float[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	public void Write(Guid[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	public void Write(int[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	public void Write(long[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	public void Write(object[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyObjectArrayType);
			return;
		}
		writeTypeCode(SerializedType.ObjectArrayType);
		writeObjectArray(values);
	}

	[CLSCompliant(false)]
	public void Write(sbyte[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	public void Write(short[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	public void Write(TimeSpan[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	[CLSCompliant(false)]
	public void Write(uint[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	[CLSCompliant(false)]
	public void Write(ulong[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
		}
		else
		{
			writeArray(values, null);
		}
	}

	[CLSCompliant(false)]
	public void Write(ushort[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		writeArray(values);
	}

	public void WriteOptimized(bool[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.FullyOptimizedTypedArrayType);
		writeArray(values);
	}

	public void WriteOptimized(DateTime[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i].Ticks % 10000 != 0L)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	public void WriteOptimized(decimal[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		writeTypeCode(SerializedType.FullyOptimizedTypedArrayType);
		writeArray(values);
	}

	public void WriteOptimized(object[] values)
	{
		writeObjectArray(values);
	}

	public void WriteOptimized(object[] values1, object[] values2)
	{
		writeObjectArray(values1);
		int num = values2.Length - 1;
		for (int i = 0; i < values2.Length; i++)
		{
			object obj = values2[i];
			if (obj?.Equals(values1[i]) ?? (values1[i] == null))
			{
				int num2 = 0;
				for (; i < num && ((values2[i + 1] == null) ? (values1[i + 1] == null) : values2[i + 1].Equals(values1[i + 1])); i++)
				{
					num2++;
				}
				if (num2 == 0)
				{
					writeTypeCode(SerializedType.DuplicateValueType);
					continue;
				}
				writeTypeCode(SerializedType.DuplicateValueSequenceType);
				write7bitEncodedSigned32BitValue(num2);
			}
			else if (obj == null)
			{
				int num3 = 0;
				for (; i < num && values2[i + 1] == null; i++)
				{
					num3++;
				}
				if (num3 == 0)
				{
					writeTypeCode(SerializedType.NullType);
					continue;
				}
				writeTypeCode(SerializedType.NullSequenceType);
				write7bitEncodedSigned32BitValue(num3);
			}
			else if (obj == DBNull.Value)
			{
				int num4 = 0;
				for (; i < num && values2[i + 1] == DBNull.Value; i++)
				{
					num4++;
				}
				if (num4 == 0)
				{
					writeTypeCode(SerializedType.DBNullType);
					continue;
				}
				writeTypeCode(SerializedType.DBNullSequenceType);
				write7bitEncodedSigned32BitValue(num4);
			}
			else
			{
				WriteObject(obj);
			}
		}
	}

	public void WriteOptimized(short[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] < 0 || values[i] > 127)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	public void WriteOptimized(int[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] < 0 || values[i] > 2097151)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	public void WriteOptimized(long[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] < 0 || values[i] > 562949953421311L)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	public void WriteOptimized(TimeSpan[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i].Ticks % 10000 != 0L)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	[CLSCompliant(false)]
	public void WriteOptimized(ushort[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] > 127)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	[CLSCompliant(false)]
	public void WriteOptimized(uint[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] > 2097151)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	[CLSCompliant(false)]
	public void WriteOptimized(ulong[] values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		if (values.Length == 0)
		{
			writeTypeCode(SerializedType.EmptyTypedArrayType);
			return;
		}
		BitArray bitArray = null;
		int num = 0;
		int num2 = 1 + (int)((float)values.Length * (optimizeForSize ? 0.8f : 0.6f));
		for (int i = 0; i < values.Length; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (values[i] > 562949953421311L)
			{
				num++;
				continue;
			}
			if (bitArray == null)
			{
				bitArray = new BitArray(values.Length);
			}
			bitArray[i] = true;
		}
		if (num == 0)
		{
			bitArray = FullyOptimizableTypedArray;
		}
		else if (num >= num2)
		{
			bitArray = null;
		}
		writeArray(values, bitArray);
	}

	public void WriteOptimized(Version Value)
	{
		write7bitEncodedUnsigned32BitValue((uint)Value.Major);
		write7bitEncodedUnsigned32BitValue((uint)Value.Minor);
		write7bitEncodedUnsigned32BitValue((uint)Value.Build);
		write7bitEncodedUnsigned32BitValue((uint)Value.Revision);
	}

	public void WriteNullable(ValueType value)
	{
		WriteObject(value);
	}

	public void Write<K, V>(Dictionary<K, V> value)
	{
		if (value == null)
		{
			Write(-1);
			return;
		}
		Write(value.Keys.Count);
		foreach (KeyValuePair<K, V> item in value)
		{
			WriteObject(item.Key);
			WriteObject(item.Value);
		}
	}

	public void Write(IDictionary Value)
	{
		if (Value == null)
		{
			Write(-1);
			return;
		}
		Write(Value.Keys.Count);
		foreach (DictionaryEntry item in Value)
		{
			WriteObject(item.Key);
			WriteObject(item.Value);
		}
	}

	public void Write<T>(List<T> value)
	{
		if (value == null)
		{
			Write(-1);
			return;
		}
		Write(value.Count);
		for (int i = 0; i < value.Count; i++)
		{
			WriteObject(value[i]);
		}
	}

	public void Write(List<string> value)
	{
		if (value == null)
		{
			Write(-1);
			return;
		}
		Write(value.Count);
		for (int i = 0; i < value.Count; i++)
		{
			Write(value[i]);
		}
	}

	public void Write(IList Value)
	{
		if (Value == null)
		{
			Write(-1);
			return;
		}
		Write(Value.Count);
		foreach (object item in Value)
		{
			WriteObject(item);
		}
	}

	public void Write(Location2D Value)
	{
		if (Value == null)
		{
			Write(-1);
			return;
		}
		WriteOptimized(Value.X);
		WriteOptimized(Value.Y);
	}

	public void Write(List<Location2D> Value)
	{
		if (Value == null)
		{
			Write(-1);
			return;
		}
		Write(Value.Count);
		foreach (Location2D item in Value)
		{
			Write(item);
		}
	}

	public void Write(Cell Value)
	{
		string text = Value?.ParentZone?.ZoneID;
		WriteOptimized(text);
		if (text != null)
		{
			WriteOptimized(Value.X);
			WriteOptimized(Value.Y);
		}
	}

	public void Write(EventRegistry Registry)
	{
		if (Registry == null || !Registry.Serialize)
		{
			write7bitEncodedSigned32BitValue(0);
			return;
		}
		write7bitEncodedSigned32BitValue(EventRegistries.Count + 1);
		EventRegistries.Add(Registry);
	}

	public void Write(GameObjectReference Reference)
	{
		if (Reference == null)
		{
			write7bitEncodedSigned32BitValue(0);
			return;
		}
		write7bitEncodedSigned32BitValue(GameObjectReferences.Count + 1);
		GameObjectReferences.Add(Reference);
	}

	public void Write(IComposite Value)
	{
		if (Value == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else
		{
			SerializeComposite(Value);
		}
	}

	private void SerializeComposite(IComposite Value)
	{
		bool wantFieldReflection = Value.WantFieldReflection;
		writeTypeCode(wantFieldReflection ? SerializedType.ICompositeFieldType : SerializedType.ICompositeType);
		Type type = Value.GetType();
		using (StartBlock())
		{
			WriteTokenized(type);
			if (wantFieldReflection)
			{
				WriteTypeFields(Value, type);
			}
			Value.Write(this);
		}
	}

	public void WriteComposite<T>(T Value) where T : IComposite, new()
	{
		if (Value == null)
		{
			writeTypeCode(SerializedType.NullType);
			return;
		}
		bool wantFieldReflection = Value.WantFieldReflection;
		writeTypeCode(wantFieldReflection ? SerializedType.ICompositeFieldType : SerializedType.ICompositeType);
		using (StartBlock())
		{
			if (wantFieldReflection)
			{
				WriteFields(Value);
			}
			Value.Write(this);
		}
	}

	public void WriteComposite<T>(List<T> Value) where T : IComposite
	{
		if (Value == null)
		{
			WriteOptimized(0);
			return;
		}
		int count = Value.Count;
		WriteOptimized(count + 1);
		for (int i = 0; i < count; i++)
		{
			Write(Value[i]);
		}
	}

	public void WriteTypedArray(Array values)
	{
		if (values == null)
		{
			writeTypeCode(SerializedType.NullType);
		}
		else
		{
			writeTypedArray(values, storeType: true);
		}
	}

	public int AppendTokenTables()
	{
		GameObjectReferencePosition = OutStream.Position;
		int count = GameObjectReferences.Count;
		write7bitEncodedSigned32BitValue(count);
		for (int i = 0; i < count; i++)
		{
			GameObjectReferences[i].Write(this);
		}
		ObjectPosition = OutStream.Position;
		count = TokenObjects.Count;
		write7bitEncodedSigned32BitValue(count);
		for (int j = 0; j < count; j++)
		{
			WriteObject(TokenObjects[j]);
		}
		TokenizedPosition = OutStream.Position;
		count = Tokenized.Count;
		write7bitEncodedSigned32BitValue(count);
		for (int k = 0; k < count; k++)
		{
			Write(Tokenized[k]);
			Tokenized[k].Token = 0;
		}
		TypePosition = OutStream.Position;
		count = TokenTypes.Count;
		write7bitEncodedSigned32BitValue(count);
		for (int l = 0; l < count; l++)
		{
			WriteDirect(TokenTypes[l]);
		}
		StringPosition = OutStream.Position;
		count = TokenStrings.Count;
		write7bitEncodedSigned32BitValue(count);
		for (int m = 0; m < count; m++)
		{
			base.Write(TokenStrings[m]);
		}
		formatter = null;
		return (int)(OutStream.Position - ObjectPosition);
	}

	public void FinalizeWrite()
	{
		WriteGameObjects();
		WriteEventRegistries();
		AppendTokenTables();
		WriteHeader();
	}

	public void ReserveHeader()
	{
		for (int i = 0; i < 68; i++)
		{
			OutStream.WriteByte(0);
		}
	}

	public void WriteHeader()
	{
		long position = OutStream.Position;
		OutStream.Position = 0L;
		Write(FileVersion);
		Write(GameObjectPosition);
		Write(GameObjectReferencePosition);
		Write(EventRegistryPosition);
		Write(TokenizedPosition);
		Write(ObjectPosition);
		Write(TypePosition);
		Write(StringPosition);
		Write(TokenGameObjects.Count);
		Write(EventRegistries.Count);
		OutStream.Position = position;
	}

	public byte[] ToArray()
	{
		AppendTokenTables();
		return Stream.ToArray();
	}

	public void WriteBytesDirect(byte[] value)
	{
		base.Write(value);
	}

	public void WriteStringDirect(string value)
	{
		base.Write(value);
	}

	public void WriteFields<T>(T Value, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		WriteTypeFields(Value, typeof(T), Flags, Mask);
	}

	public void WriteObjectFields(object Value, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		WriteTypeFields(Value, Value.GetType(), Flags, Mask);
	}

	public void WriteTypeFields(object Value, Type Type, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		FieldInfo[] array = ((Flags == (BindingFlags.Instance | BindingFlags.Public)) ? Type.GetCachedFields() : Type.GetFields(Flags));
		foreach (FieldInfo fieldInfo in array)
		{
			if ((fieldInfo.Attributes & Mask) == 0)
			{
				WriteObject(fieldInfo.GetValue(Value));
			}
		}
	}

	public void WriteNamedFields(object Value, Type Type, BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public, FieldAttributes Mask = FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.NotSerialized)
	{
		FieldInfo[] array = ((Flags == (BindingFlags.Instance | BindingFlags.Public)) ? Type.GetCachedFields() : Type.GetFields(Flags));
		int num = array.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if ((array[i].Attributes & Mask) == 0)
			{
				num2++;
			}
		}
		WriteOptimized(num2);
		for (int j = 0; j < num; j++)
		{
			if (num2 <= 0)
			{
				break;
			}
			FieldInfo fieldInfo = array[j];
			if ((fieldInfo.Attributes & Mask) == 0)
			{
				WriteOptimized(fieldInfo.Name);
				WriteObject(fieldInfo.GetValue(Value));
				num2--;
			}
		}
	}

	public void WriteGameObjectList(List<GameObject> List)
	{
		Write(List.Count);
		for (int i = 0; i < List.Count; i++)
		{
			WriteGameObject(List[i]);
		}
	}

	public void WriteGameObject(GameObject Object)
	{
		if (Object == null)
		{
			write7bitEncodedSigned32BitValue(0);
			return;
		}
		if (TokenByGameObject.TryGetValue(Object, out var value))
		{
			write7bitEncodedSigned32BitValue(value + 2);
			return;
		}
		Object.Flags |= 32;
		value = TokenGameObjects.Count;
		TokenByGameObject[Object] = value;
		TokenGameObjects.Add(Object);
		write7bitEncodedSigned32BitValue(value + 2);
	}

	public void WriteGameObject(GameObject Object, bool Reference)
	{
		int value;
		string value2;
		if (Object == null)
		{
			write7bitEncodedSigned32BitValue(0);
		}
		else if (TokenByGameObject.TryGetValue(Object, out value))
		{
			write7bitEncodedSigned32BitValue(value + 2);
		}
		else if (!Reference)
		{
			Object.Flags |= 32;
			value = TokenGameObjects.Count;
			TokenByGameObject[Object] = value;
			TokenGameObjects.Add(Object);
			write7bitEncodedSigned32BitValue(value + 2);
		}
		else if (Object._Property != null && Object._Property.TryGetValue("id", out value2))
		{
			write7bitEncodedSigned32BitValue(1);
			WriteOptimized(value2);
		}
		else
		{
			write7bitEncodedSigned32BitValue(0);
		}
	}

	public void WriteGameObjects()
	{
		GameObjectPosition = OutStream.Position;
		for (int i = ((!SerializePlayer) ? 1 : 0); i < TokenGameObjects.Count; i++)
		{
			Write((ushort)64206);
			write7bitEncodedSigned32BitValue(i);
			GameObject gameObject = TokenGameObjects[i];
			gameObject.Save(this);
			Write((ushort)44203);
			gameObject.Flags &= -33;
		}
		for (int j = 0; j < TokenGameObjects.Count; j++)
		{
			List<GameObject> list = TokenGameObjects[j]?.Inventory?.Objects;
			if (list.IsNullOrEmpty())
			{
				continue;
			}
			foreach (GameObject item in list)
			{
				if (item.Flags.HasBit(32))
				{
					MetricsManager.LogError("!POOLEDOBJECT Inventory object '" + item.DebugName + "' in '" + TokenGameObjects[j].DebugName + "' was tokenized but never wrote its data.");
				}
			}
		}
		Write((ushort)52958);
	}

	public void WriteEventRegistries()
	{
		EventRegistryPosition = OutStream.Position;
		int i = 0;
		for (int count = EventRegistries.Count; i < count; i++)
		{
			EventRegistries[i].Write(this);
		}
	}

	public void WriteTokenizedObject(object value)
	{
		WriteTokenizedObject(value, recreateFromType: false);
	}

	public void WriteTokenizedObject(object value, bool recreateFromType)
	{
		if (recreateFromType)
		{
			if (WrapperCache == null)
			{
				WrapperCache = new SingletonTypeWrapper();
			}
			WrapperCache.WrappedType = value.GetType();
			value = WrapperCache;
		}
		object obj = TokenByObject[value];
		if (obj != null)
		{
			write7bitEncodedSigned32BitValue((int)obj);
			return;
		}
		if (recreateFromType)
		{
			value = new SingletonTypeWrapper(WrapperCache);
		}
		int count = TokenObjects.Count;
		TokenObjects.Add(value);
		TokenByObject[value] = count;
		write7bitEncodedSigned32BitValue(count);
	}

	public void WriteTokenized(Type Type)
	{
		if ((object)Type == null)
		{
			write7bitEncodedSigned32BitValue(0);
			return;
		}
		if (!TokenByType.TryGetValue(Type, out var value))
		{
			value = TokenTypes.Count + 1;
			TokenTypes.Add(Type);
			TokenByType[Type] = value;
		}
		write7bitEncodedSigned32BitValue(value);
	}

	public void WriteTokenized(ITokenized Value)
	{
		int num = 0;
		if (Value != null)
		{
			num = Value.Token;
			if (num <= 0)
			{
				num = (Value.Token = Tokenized.Count + 1);
				Tokenized.Add(Value);
			}
		}
		write7bitEncodedSigned32BitValue(num);
	}

	public void WriteModVersions()
	{
		int num = ModManager.ActiveMods?.Count ?? 0;
		WriteOptimized(num);
		for (int i = 0; i < num; i++)
		{
			ModInfo modInfo = ModManager.ActiveMods[i];
			WriteOptimized(modInfo.ID);
			WriteOptimized(modInfo.Manifest?.Version ?? Version.Zero);
		}
	}

	internal static IFastSerializationTypeSurrogate findSurrogateForType(Type type)
	{
		foreach (IFastSerializationTypeSurrogate typeSurrogate in TypeSurrogates)
		{
			if (typeSurrogate.SupportsType(type))
			{
				return typeSurrogate;
			}
		}
		return null;
	}

	private static IFormatter createBinaryFormatter()
	{
		if (formatter == null)
		{
			Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
			formatter = new BinaryFormatter();
			formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			formatter.TypeFormat = FormatterTypeStyle.TypesWhenNeeded;
		}
		return formatter;
	}

	private void encodeTimeSpan(TimeSpan value, bool partOfDateTime, int initialData)
	{
		BitVector32 bitVector = new BitVector32(initialData);
		int num = Math.Abs(value.Hours);
		int num2 = Math.Abs(value.Minutes);
		int num3 = Math.Abs(value.Seconds);
		int num4 = Math.Abs(value.Milliseconds);
		bool flag = num != 0 || num2 != 0;
		int num5 = 0;
		int num6;
		if (partOfDateTime)
		{
			num6 = 0;
		}
		else
		{
			num6 = Math.Abs(value.Days);
			bitVector[IsNegativeSection] = ((value.Ticks < 0) ? 1 : 0);
			bitVector[HasDaysSection] = ((num6 != 0) ? 1 : 0);
		}
		if (flag)
		{
			bitVector[HasTimeSection] = 1;
			bitVector[HoursSection] = num;
			bitVector[MinutesSection] = num2;
		}
		if (num3 != 0)
		{
			bitVector[HasSecondsSection] = 1;
			if (!flag && num4 == 0)
			{
				bitVector[MinutesSection] = num3;
			}
			else
			{
				bitVector[SecondsSection] = num3;
				num5++;
			}
		}
		if (num4 != 0)
		{
			bitVector[HasMillisecondsSection] = 1;
			bitVector[MillisecondsSection] = num4;
			num5 = 2;
		}
		int data = bitVector.Data;
		Write((byte)data);
		Write((byte)(data >> 8));
		if (num5 > 0)
		{
			Write((byte)(data >> 16));
		}
		if (num5 > 1)
		{
			Write((byte)(data >> 24));
		}
		if (num6 != 0)
		{
			write7bitEncodedSigned32BitValue(num6);
		}
	}

	[Conditional("THROW_IF_NOT_OPTIMIZABLE")]
	private static void checkOptimizable(bool condition, string message)
	{
		if (!condition)
		{
			throw new OptimizationException(message);
		}
	}

	private void write7bitEncodedSigned32BitValue(int value)
	{
		uint num;
		for (num = (uint)value; num >= 128; num >>= 7)
		{
			Write((byte)(num | 0x80));
		}
		Write((byte)num);
	}

	private void write7bitEncodedSigned64BitValue(long value)
	{
		ulong num;
		for (num = (ulong)value; num >= 128; num >>= 7)
		{
			Write((byte)(num | 0x80));
		}
		Write((byte)num);
	}

	private void write7bitEncodedUnsigned32BitValue(uint value)
	{
		while (value >= 128)
		{
			Write((byte)(value | 0x80));
			value >>= 7;
		}
		Write((byte)value);
	}

	private void write7bitEncodedUnsigned64BitValue(ulong value)
	{
		while (value >= 128)
		{
			Write((byte)(value | 0x80));
			value >>= 7;
		}
		Write((byte)value);
	}

	private void writeArray(bool[] values)
	{
		WriteOptimized(new BitArray(values));
	}

	private void writeArray(byte[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		if (values.Length != 0)
		{
			base.Write(values);
		}
	}

	private void writeArray(char[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		if (values.Length != 0)
		{
			base.Write(values);
		}
	}

	private void writeArray(DateTime[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				WriteOptimized(values[i]);
			}
		}
	}

	private void writeArray(decimal[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			WriteOptimized(values[i]);
		}
	}

	private void writeArray(double[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (double value in values)
		{
			Write(value);
		}
	}

	private void writeArray(float[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (float value in values)
		{
			Write(value);
		}
	}

	private void writeArray(Guid[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (Guid value in values)
		{
			Write(value);
		}
	}

	private void writeArray(short[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedSigned32BitValue(values[i]);
			}
		}
	}

	private void writeArray(int[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedSigned32BitValue(values[i]);
			}
		}
	}

	private void writeArray(long[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedSigned64BitValue(values[i]);
			}
		}
	}

	private void writeArray(sbyte[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (sbyte value in values)
		{
			Write(value);
		}
	}

	private void writeArray(short[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (short value in values)
		{
			Write(value);
		}
	}

	private void writeArray(TimeSpan[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				WriteOptimized(values[i]);
			}
		}
	}

	private void writeArray(ushort[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedUnsigned32BitValue(values[i]);
			}
		}
	}

	private void writeArray(uint[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedUnsigned32BitValue(values[i]);
			}
		}
	}

	private void writeArray(ushort[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		foreach (ushort value in values)
		{
			Write(value);
		}
	}

	private void writeArray(ulong[] values, BitArray optimizeFlags)
	{
		writeTypedArrayTypeCode(optimizeFlags, values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			if (optimizeFlags == null || (optimizeFlags != FullyOptimizableTypedArray && !optimizeFlags[i]))
			{
				Write(values[i]);
			}
			else
			{
				write7bitEncodedUnsigned64BitValue(values[i]);
			}
		}
	}

	private void writeObjectArray(object[] values)
	{
		write7bitEncodedSigned32BitValue(values.Length);
		int num = values.Length - 1;
		for (int i = 0; i < values.Length; i++)
		{
			object obj = values[i];
			if (i < num && (obj?.Equals(values[i + 1]) ?? (values[i + 1] == null)))
			{
				int num2 = 1;
				if (obj == null)
				{
					writeTypeCode(SerializedType.NullSequenceType);
					for (i++; i < num && values[i + 1] == null; i++)
					{
						num2++;
					}
				}
				else if (obj == DBNull.Value)
				{
					writeTypeCode(SerializedType.DBNullSequenceType);
					for (i++; i < num && values[i + 1] == DBNull.Value; i++)
					{
						num2++;
					}
				}
				else
				{
					writeTypeCode(SerializedType.DuplicateValueSequenceType);
					for (i++; i < num && obj.Equals(values[i + 1]); i++)
					{
						num2++;
					}
					WriteObject(obj);
				}
				write7bitEncodedSigned32BitValue(num2);
			}
			else
			{
				WriteObject(obj);
			}
		}
	}

	private void writeTypeCode(SerializedType typeCode)
	{
		Write((byte)typeCode);
	}

	private void writeTypedArray(Array value, bool storeType)
	{
		Type elementType = value.GetType().GetElementType();
		if (elementType == typeof(object))
		{
			storeType = false;
		}
		if (elementType == typeof(string))
		{
			writeTypeCode(SerializedType.StringArrayType);
			WriteOptimized((object[])value);
			return;
		}
		if (elementType == typeof(int))
		{
			writeTypeCode(SerializedType.Int32ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((int[])value);
			}
			else
			{
				Write((int[])value);
			}
			return;
		}
		if (elementType == typeof(short))
		{
			writeTypeCode(SerializedType.Int16ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((short[])value);
			}
			else
			{
				Write((short[])value);
			}
			return;
		}
		if (elementType == typeof(long))
		{
			writeTypeCode(SerializedType.Int64ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((long[])value);
			}
			else
			{
				Write((long[])value);
			}
			return;
		}
		if (elementType == typeof(uint))
		{
			writeTypeCode(SerializedType.UInt32ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((uint[])value);
			}
			else
			{
				Write((uint[])value);
			}
			return;
		}
		if (elementType == typeof(ushort))
		{
			writeTypeCode(SerializedType.UInt16ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((ushort[])value);
			}
			else
			{
				Write((ushort[])value);
			}
			return;
		}
		if (elementType == typeof(ulong))
		{
			writeTypeCode(SerializedType.UInt64ArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((ulong[])value);
			}
			else
			{
				Write((ulong[])value);
			}
			return;
		}
		if (elementType == typeof(float))
		{
			writeTypeCode(SerializedType.SingleArrayType);
			writeArray((float[])value);
			return;
		}
		if (elementType == typeof(double))
		{
			writeTypeCode(SerializedType.DoubleArrayType);
			writeArray((double[])value);
			return;
		}
		if (elementType == typeof(decimal))
		{
			writeTypeCode(SerializedType.DecimalArrayType);
			writeArray((decimal[])value);
			return;
		}
		if (elementType == typeof(DateTime))
		{
			writeTypeCode(SerializedType.DateTimeArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((DateTime[])value);
			}
			else
			{
				Write((DateTime[])value);
			}
			return;
		}
		if (elementType == typeof(TimeSpan))
		{
			writeTypeCode(SerializedType.TimeSpanArrayType);
			if (optimizeForSize)
			{
				WriteOptimized((TimeSpan[])value);
			}
			else
			{
				Write((TimeSpan[])value);
			}
			return;
		}
		if (elementType == typeof(Guid))
		{
			writeTypeCode(SerializedType.GuidArrayType);
			writeArray((Guid[])value);
			return;
		}
		if (elementType == typeof(sbyte))
		{
			writeTypeCode(SerializedType.SByteArrayType);
			writeArray((sbyte[])value);
			return;
		}
		if (elementType == typeof(bool))
		{
			writeTypeCode(SerializedType.BooleanArrayType);
			writeArray((bool[])value);
			return;
		}
		if (elementType == typeof(byte))
		{
			writeTypeCode(SerializedType.ByteArrayType);
			writeArray((byte[])value);
			return;
		}
		if (elementType == typeof(char))
		{
			writeTypeCode(SerializedType.CharArrayType);
			writeArray((char[])value);
			return;
		}
		if (value.Length == 0)
		{
			writeTypeCode((elementType == typeof(object)) ? SerializedType.EmptyObjectArrayType : SerializedType.EmptyTypedArrayType);
			if (storeType)
			{
				WriteTokenized(elementType);
			}
			return;
		}
		if (elementType == typeof(object))
		{
			writeTypeCode(SerializedType.ObjectArrayType);
			writeObjectArray((object[])value);
			return;
		}
		BitArray bitArray = (isTypeRecreatable(elementType) ? FullyOptimizableTypedArray : null);
		if (!elementType.IsValueType)
		{
			if (bitArray == null || !arrayElementsAreSameType((object[])value, elementType))
			{
				if (!storeType)
				{
					writeTypeCode(SerializedType.ObjectArrayType);
				}
				else
				{
					writeTypeCode(SerializedType.OtherTypedArrayType);
					WriteTokenized(elementType);
				}
				writeObjectArray((object[])value);
				return;
			}
			for (int i = 0; i < value.Length; i++)
			{
				if (value.GetValue(i) == null)
				{
					if (bitArray == FullyOptimizableTypedArray)
					{
						bitArray = new BitArray(value.Length);
					}
					bitArray[i] = true;
				}
			}
		}
		writeTypedArrayTypeCode(bitArray, value.Length);
		if (storeType)
		{
			WriteTokenized(elementType);
		}
		for (int j = 0; j < value.Length; j++)
		{
			if (bitArray == null)
			{
				WriteObject(value.GetValue(j));
			}
			else if (bitArray == FullyOptimizableTypedArray || !bitArray[j])
			{
				Write((IOwnedDataSerializable)value.GetValue(j), null);
			}
		}
	}

	private static bool isTypeRecreatable(Type type)
	{
		if (type.IsValueType)
		{
			return typeof(IOwnedDataSerializable).IsAssignableFrom(type);
		}
		if (typeof(IOwnedDataSerializableAndRecreatable).IsAssignableFrom(type))
		{
			return type.GetConstructor(Type.EmptyTypes) != null;
		}
		return false;
	}

	private static bool arrayElementsAreSameType(object[] values, Type elementType)
	{
		foreach (object obj in values)
		{
			if (obj != null && obj.GetType() != elementType)
			{
				return false;
			}
		}
		return true;
	}

	private void writeTypedArrayTypeCode(BitArray optimizeFlags, int length)
	{
		if (optimizeFlags == null)
		{
			writeTypeCode(SerializedType.NonOptimizedTypedArrayType);
		}
		else if (optimizeFlags == FullyOptimizableTypedArray)
		{
			writeTypeCode(SerializedType.FullyOptimizedTypedArrayType);
		}
		else
		{
			writeTypeCode(SerializedType.PartiallyOptimizedTypedArrayType);
			WriteOptimized(optimizeFlags);
		}
		write7bitEncodedSigned32BitValue(length);
	}

	[Conditional("DEBUG")]
	public void DumpTypeUsage()
	{
		StringBuilder value = new StringBuilder("Type Usage Dump\r\n---------------\r\n");
		for (int i = 0; i < 256; i++)
		{
		}
		Console.WriteLine(value);
	}

	public Block StartBlock()
	{
		if (!Blocks.TryPop(out var result))
		{
			result = new Block();
		}
		result.Start(this);
		return result;
	}
}
