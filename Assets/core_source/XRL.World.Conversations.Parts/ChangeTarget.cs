namespace XRL.World.Conversations.Parts;

public class ChangeTarget : IPredicatePart
{
	public string Target;

	public new bool Any;

	public bool Not;

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation))
		{
			return ID == GetTargetElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTargetElementEvent E)
	{
		if (Check(Any) != Not)
		{
			E.Target = Target;
		}
		return base.HandleEvent(E);
	}
}
