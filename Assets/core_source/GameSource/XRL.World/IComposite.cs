namespace XRL.World;

public interface IComposite
{
	bool WantFieldReflection => true;

	void Write(SerializationWriter Writer)
	{
	}

	void Read(SerializationReader Reader)
	{
	}
}
