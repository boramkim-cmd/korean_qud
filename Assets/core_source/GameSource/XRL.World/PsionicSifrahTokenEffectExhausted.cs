using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenEffectExhausted : RitualSifrahTokenEffectExhausted
{
	public PsionicSifrahTokenEffectExhausted()
	{
	}

	public PsionicSifrahTokenEffectExhausted(int Chance)
		: base(Chance)
	{
	}
}
