namespace XRL.World;

public interface IModEventHandler<T> where T : MinEvent
{
	bool HandleEvent(T E)
	{
		return true;
	}
}
