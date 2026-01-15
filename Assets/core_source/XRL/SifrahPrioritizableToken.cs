namespace XRL;

public abstract class SifrahPrioritizableToken : SifrahToken, SifrahPrioritizable
{
	public abstract int GetPriority();

	public abstract int GetTiebreakerPriority();
}
