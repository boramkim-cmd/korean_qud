using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenCreationKnowledge : TinkeringSifrahTokenCreationKnowledge
{
	public PsionicSifrahTokenCreationKnowledge()
	{
	}

	public PsionicSifrahTokenCreationKnowledge(string Blueprint)
		: base(Blueprint)
	{
	}

	public PsionicSifrahTokenCreationKnowledge(GameObject Object)
		: base(Object)
	{
	}
}
