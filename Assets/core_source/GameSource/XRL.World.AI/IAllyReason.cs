namespace XRL.World.AI;

public abstract class IAllyReason : IComposite
{
	public enum ReplaceTarget
	{
		None,
		Source,
		Type
	}

	public long Time;

	public virtual bool WantFieldReflection => false;

	public virtual ReplaceTarget Replace => ReplaceTarget.None;

	public virtual void Initialize(GameObject Actor, GameObject Source, AllegianceSet Set)
	{
	}

	public virtual string GetText(GameObject Actor)
	{
		return null;
	}

	public virtual void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Time);
	}

	public virtual void Read(SerializationReader Reader)
	{
		Time = Reader.ReadOptimizedInt64();
	}
}
