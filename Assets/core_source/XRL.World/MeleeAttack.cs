using System;
using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World;

public class MeleeAttack : IDisposable
{
	public int Chance;

	public int HitModifier;

	public int PenModifier;

	public object Source;

	public string Type;

	public string Properties;

	public GameObject Weapon;

	public BodyPart BodyPart;

	public Predicate<BodyPart> Filter;

	public bool Intrinsic;

	public bool? Primary;

	private static Stack<MeleeAttack> Pool = new Stack<MeleeAttack>();

	public bool IsValidFor(BodyPart BodyPart)
	{
		if (this.BodyPart != null && BodyPart != this.BodyPart)
		{
			return false;
		}
		if (Filter != null && !Filter(BodyPart))
		{
			return false;
		}
		return true;
	}

	public void Reset()
	{
		Chance = 0;
		HitModifier = 0;
		PenModifier = 0;
		Type = null;
		Properties = null;
		BodyPart = null;
		Source = null;
		Filter = null;
		Primary = null;
	}

	private static MeleeAttack GetInternal()
	{
		if (Pool.Count <= 0)
		{
			return new MeleeAttack();
		}
		return Pool.Pop();
	}

	public static MeleeAttack Get(int Chance = 0, int HitModifier = 0, int PenModifier = 0, object Source = null, string Type = null, string Properties = null, GameObject Weapon = null, BodyPart BodyPart = null, Predicate<BodyPart> Filter = null, bool Intrinsic = false, bool? Primary = null)
	{
		MeleeAttack meleeAttack = GetInternal();
		meleeAttack.Chance = Chance;
		meleeAttack.HitModifier = HitModifier;
		meleeAttack.PenModifier = PenModifier;
		meleeAttack.Source = Source;
		meleeAttack.Type = Type;
		meleeAttack.Properties = Properties;
		meleeAttack.BodyPart = BodyPart;
		meleeAttack.Filter = Filter;
		meleeAttack.Primary = Primary;
		return meleeAttack;
	}

	public static void Return(MeleeAttack Attack)
	{
		Attack.Reset();
		Pool.Push(Attack);
	}

	public void Dispose()
	{
		Return(this);
	}
}
