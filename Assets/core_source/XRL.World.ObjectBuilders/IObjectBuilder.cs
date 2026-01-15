namespace XRL.World.ObjectBuilders;

public abstract class IObjectBuilder
{
	public virtual void Initialize()
	{
	}

	public abstract void Apply(GameObject Object, string Context);
}
