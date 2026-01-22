using System;
using System.Reflection;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class WantToAutoexplore : IPart
{
	public string AdjacentAction;

	public bool AllowRetry;

	public bool Override;

	public string TriggeredEvent;

	[NonSerialized]
	private int? TriggeredMinEvent;

	public override bool WantEvent(int ID, int cascade)
	{
		if (ID == TriggeredMinEvent)
		{
			ParentObject.RemovePart(this);
		}
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AutoexploreObjectEvent.ID;
		}
		return true;
	}

	public override void Register(GameObject GO, IEventRegistrar Registrar)
	{
		Type type = ModManager.ResolveType("XRL.World", TriggeredEvent);
		if ((object)type != null)
		{
			PropertyInfo property = type.GetProperty("ID", BindingFlags.Static | BindingFlags.Public);
			if ((object)property != null)
			{
				TriggeredMinEvent = (int)property.GetValue(null);
			}
		}
		if (!TriggeredMinEvent.HasValue && !TriggeredEvent.IsNullOrEmpty())
		{
			GO.RegisterPartEvent(this, TriggeredEvent);
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == TriggeredEvent)
		{
			ParentObject.RemovePart(this);
		}
		return base.FireEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!AdjacentAction.IsNullOrEmpty() && (E.Command == null || (Override && E.Command != AdjacentAction)) && (!E.AutogetOnlyMode || AdjacentAction == "Autoget" || AdjacentAction == "CollectLiquid") && (AllowRetry || AutoAct.GetAutoexploreActionProperty(ParentObject, AdjacentAction) <= 0))
		{
			E.Command = AdjacentAction;
		}
		return base.HandleEvent(E);
	}
}
