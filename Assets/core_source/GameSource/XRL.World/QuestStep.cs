using System;

namespace XRL.World;

[Serializable]
public class QuestStep
{
	public const int FLAG_BASE = 1;

	public const int FLAG_FINISHED = 2;

	public const int FLAG_FAILED = 4;

	public const int FLAG_COLLAPSE = 8;

	public const int FLAG_HIDDEN = 16;

	public const int FLAG_AWARDED = 32;

	public const int FLAG_OPTIONAL = 64;

	public string ID;

	public string Name;

	public string Text;

	public string Value;

	public int XP;

	public int Ordinal;

	public int Flags = 8;

	public bool Base
	{
		get
		{
			return Flags.HasBit(1);
		}
		set
		{
			Flags.SetBit(1, value);
		}
	}

	public bool Finished
	{
		get
		{
			return Flags.HasBit(2);
		}
		set
		{
			Flags.SetBit(2, value);
		}
	}

	public bool Awarded
	{
		get
		{
			return Flags.HasBit(32);
		}
		set
		{
			Flags.SetBit(32, value);
		}
	}

	public bool Failed
	{
		get
		{
			return Flags.HasBit(4);
		}
		set
		{
			Flags.SetBit(4, value);
		}
	}

	public bool Collapse
	{
		get
		{
			return Flags.HasBit(8);
		}
		set
		{
			Flags.SetBit(8, value);
		}
	}

	public bool Hidden
	{
		get
		{
			return Flags.HasBit(16);
		}
		set
		{
			Flags.SetBit(16, value);
		}
	}

	public bool Optional
	{
		get
		{
			return Flags.HasBit(64);
		}
		set
		{
			Flags.SetBit(64, value);
		}
	}

	public QuestStep()
	{
	}

	public QuestStep(SerializationReader Reader)
		: this()
	{
		Load(Reader);
	}

	public override string ToString()
	{
		return ID + " n=" + Name + " t=" + Text + " xp=" + XP + " finished=" + Finished;
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.Write(ID);
		Writer.Write(Name);
		Writer.Write(Text);
		Writer.Write(Value);
		Writer.WriteOptimized(XP);
		Writer.WriteOptimized(Ordinal);
		Writer.WriteOptimized(Flags);
	}

	public void Load(SerializationReader Reader)
	{
		ID = Reader.ReadString();
		Name = Reader.ReadString();
		Text = Reader.ReadString();
		Value = Reader.ReadString();
		XP = Reader.ReadOptimizedInt32();
		Ordinal = Reader.ReadOptimizedInt32();
		Flags = Reader.ReadOptimizedInt32();
	}
}
