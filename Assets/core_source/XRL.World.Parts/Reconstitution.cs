using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Reconstitution : IPart
{
	public string Marker = "Widget";

	public string Obliterate;

	public string Object;

	public string Turns;

	public long CompleteTurn = -1L;

	public bool RemoveDeathState = true;

	public bool RemoveXP = true;

	public bool StopFighting = true;

	public bool Restore = true;

	public bool Tick;

	public string DropMessage;

	public bool DropMessageUsePopup;

	public bool DropMessageAlwaysVisible;

	public bool Activated;

	[NonSerialized]
	private bool Destroyed;

	public override bool WantEvent(int ID, int cascade)
	{
		if (Activated)
		{
			if (ID != ZoneActivatedEvent.ID && ID != ZoneDeactivatedEvent.ID)
			{
				return ID == ZoneThawedEvent.ID;
			}
			return true;
		}
		if (ID == BeforeDestroyObjectEvent.ID)
		{
			return true;
		}
		return base.WantEvent(ID, cascade);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		TryReconstitute(Tick);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneDeactivatedEvent E)
	{
		TryReconstitute();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		TryReconstitute();
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		if (Activated)
		{
			return Tick;
		}
		return false;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		TryReconstitute(WithEffect: true);
		base.TurnTick(TimeTick, Amount);
	}

	public bool TryReconstitute(bool WithEffect = false)
	{
		if (!Activated)
		{
			return false;
		}
		if (The.Game.TimeTicks < CompleteTurn)
		{
			return false;
		}
		if (Object.IsNullOrEmpty())
		{
			ParentObject.Obliterate(null, Silent: true);
			return false;
		}
		GameObject gameObject = (Restore ? The.ZoneManager.CachedObjects.PullValue(Object) : GameObjectFactory.Factory.CreateObject(Object));
		if (gameObject == null)
		{
			MetricsManager.LogError("Reconstitution", "No '" + Object + "' object found to " + (Restore ? "restore" : "create") + ".");
			return false;
		}
		if (gameObject.TryGetPart<GameUnique>(out var Part))
		{
			string stringGameState = The.Game.GetStringGameState(Part.State);
			if (stringGameState.IsNullOrEmpty() || stringGameState.HasDelimitedSubstring(',', "Dead"))
			{
				The.Game.SetStringGameState(Part.State, gameObject.ID);
			}
			if (RemoveDeathState && !Part.DeathState.IsNullOrEmpty())
			{
				The.Game.RemoveBooleanGameState(Part.DeathState);
			}
		}
		if (Restore)
		{
			gameObject.RestorePristineHealth();
			gameObject.FireEvent(Event.New("Regenera", "Level", 10, "Source", ParentObject));
			RepairedEvent.Send(ParentObject, gameObject, ParentObject);
			if (StopFighting)
			{
				gameObject.StopFighting();
			}
		}
		if (RemoveXP && gameObject.HasStat("XPValue"))
		{
			gameObject.Statistics.Remove("XPValue");
		}
		try
		{
			ParseObliterate(gameObject);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Reconstitution", x);
		}
		Object = null;
		ParentObject.CurrentCell.AddObject(gameObject);
		ParentObject.Obliterate(null, Silent: true);
		if (WithEffect && gameObject.IsVisible())
		{
			gameObject.DustPuff();
			string clip = gameObject.GetPropertyOrTag("AmbientIdleSound").Coalesce("Sounds/StatusEffects/sfx_statusEffect_budding");
			gameObject.PlayWorldSound(clip);
		}
		return true;
	}

	public void ParseObliterate(GameObject Object)
	{
		if (Obliterate.IsNullOrEmpty() || Object.Inventory == null)
		{
			return;
		}
		if (Obliterate == "*")
		{
			Object.Inventory.Clear();
			return;
		}
		string[] array = Obliterate.Split(',');
		foreach (string text in array)
		{
			string[] parts = text.Split(':');
			if (parts.Length > 1)
			{
				switch (parts[0])
				{
				case "Descends":
					Object.Inventory.RemoveAll((GameObject x) => x.GetBlueprint().DescendsFrom(parts[1]));
					break;
				case "Inherits":
					Object.Inventory.RemoveAll((GameObject x) => x.GetBlueprint().InheritsFrom(parts[1]));
					break;
				case "Tag":
					Object.Inventory.RemoveAll((GameObject x) => x.HasTag(parts[1]));
					break;
				case "Property":
					Object.Inventory.RemoveAll((GameObject x) => x.HasProperty(parts[1]));
					break;
				case "TagOrProperty":
					Object.Inventory.RemoveAll((GameObject x) => x.HasTagOrProperty(parts[1]));
					break;
				case "Part":
					Object.Inventory.RemoveAll((GameObject x) => x.HasPart(parts[1]));
					break;
				}
			}
			else if (parts.Length != 0)
			{
				Object.Inventory.RemoveAll((GameObject x) => x.Blueprint == parts[1]);
			}
		}
	}

	public override bool HandleEvent(BeforeDestroyObjectEvent E)
	{
		if (!Activated && !ParentObject.IsPlayer())
		{
			if (Destroyed && !E.Obliterate)
			{
				return !Restore;
			}
			Destroyed = true;
			DropMarker();
			if (Restore)
			{
				ParentObject.Physics.TeardownForDestroy(MoveToGraveyard: false, Silent: true);
				The.ZoneManager.CachedObjects[ParentObject.ID] = ParentObject;
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public void DropMarker(Cell Cell = null)
	{
		GameObject gameObject = GameObject.Create(Marker);
		if ((gameObject.Render == null || !gameObject.Render.Visible) && Cell == null)
		{
			Cell = ParentObject.Brain?.StartingCell?.ResolveCell();
		}
		Reconstitution obj = (Reconstitution)gameObject.AddPart(DeepCopy(gameObject));
		obj.Activated = true;
		obj.Object = (Restore ? ParentObject.ID : ParentObject.Blueprint);
		obj.CompleteTurn = (Turns.IsNullOrEmpty() ? (-1) : (The.Game.TimeTicks + Stat.Roll(Turns)));
		if (Cell == null)
		{
			Cell = ParentObject.CurrentCell;
		}
		Cell.AddObject(gameObject);
		if (!DropMessage.IsNullOrEmpty())
		{
			string text = GameText.VariableReplace(DropMessage, ParentObject, gameObject);
			if (!text.IsNullOrEmpty())
			{
				EmitMessage(text, ' ', FromDialog: false, DropMessageUsePopup, DropMessageAlwaysVisible);
			}
		}
	}
}
