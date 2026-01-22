using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenEffectLost : RitualSifrahTokenEffectLost
{
	public PsionicSifrahTokenEffectLost()
	{
	}

	public PsionicSifrahTokenEffectLost(int Chance)
		: base(Chance)
	{
	}
}
