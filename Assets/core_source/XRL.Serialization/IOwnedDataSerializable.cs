using XRL.World;

namespace XRL.Serialization;

public interface IOwnedDataSerializable
{
	void SerializeOwnedData(SerializationWriter writer, object context);

	void DeserializeOwnedData(SerializationReader reader, object context);
}
