using System;
using XRL.World;

namespace XRL.Serialization;

public interface IFastSerializationTypeSurrogate
{
	bool SupportsType(Type type);

	void Serialize(SerializationWriter writer, object value);

	object Deserialize(SerializationReader reader, Type type);
}
