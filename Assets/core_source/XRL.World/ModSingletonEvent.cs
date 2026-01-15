namespace XRL.World;

public abstract class ModSingletonEvent<T> : SingletonEvent<T> where T : ModSingletonEvent<T>, new()
{
	public override bool Dispatch(IEventHandler Handler)
	{
		if (Handler is IModEventHandler<T> modEventHandler)
		{
			return modEventHandler.HandleEvent((T)this);
		}
		if (Handler is IEventSource)
		{
			return Handler.HandleEvent(this);
		}
		return true;
	}
}
