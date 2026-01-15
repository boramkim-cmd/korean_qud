using System;
using System.Linq;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class BurnOffGas : IPart
{
	public int DamageTaken;

	public int DamagePer = 10;

	public int Chance = 100;

	public string Number = "1";

	public string DamageTriggerTypes = "Heat;Fire";

	public string Blueprint;

	public bool PopulationRollsAreStatic = true;

	public bool SpawnAsDropColor = true;

	public override bool SameAs(IPart p)
	{
		BurnOffGas burnOffGas = p as BurnOffGas;
		if (burnOffGas.DamagePer != DamagePer)
		{
			return false;
		}
		if (burnOffGas.Chance != Chance)
		{
			return false;
		}
		if (burnOffGas.Number != Number)
		{
			return false;
		}
		if (burnOffGas.Blueprint != Blueprint)
		{
			return false;
		}
		if (burnOffGas.PopulationRollsAreStatic != PopulationRollsAreStatic)
		{
			return false;
		}
		if (burnOffGas.SpawnAsDropColor != SpawnAsDropColor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeTookDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTookDamage")
		{
			Physics physics = ParentObject.Physics;
			if (physics == null || physics.CurrentCell == null)
			{
				return true;
			}
			if (!E.GetParameter<Damage>("Damage").Attributes.Any((string s) => DamageTriggerTypes.Contains(s)))
			{
				return true;
			}
			DamageTaken += E.GetParameter<Damage>("Damage").Amount;
			while (DamageTaken >= DamagePer)
			{
				DamageTaken -= DamagePer;
				if (!Chance.in100() || Blueprint.IsNullOrEmpty() || ParentObject.CurrentCell == null)
				{
					continue;
				}
				int num = 0;
				for (int num2 = Stat.Roll(Number); num < num2; num++)
				{
					string blueprint = Blueprint;
					if (blueprint.StartsWith("@"))
					{
						blueprint = PopulationManager.RollOneFrom(blueprint.Substring(1)).Blueprint;
						if (PopulationRollsAreStatic)
						{
							Blueprint = blueprint;
						}
					}
					GameObject gameObject = GameObject.Create(blueprint);
					ParentObject.CurrentCell.AddObject(gameObject);
					if (ParentObject.IsVisible())
					{
						IComponent<GameObject>.XDidY(ParentObject, "burn", "off " + gameObject.an());
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
