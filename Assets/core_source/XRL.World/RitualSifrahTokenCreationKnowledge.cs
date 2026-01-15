using System;

namespace XRL.World;

[Serializable]
public class RitualSifrahTokenCreationKnowledge : TinkeringSifrahTokenCreationKnowledge
{
	public RitualSifrahTokenCreationKnowledge()
	{
	}

	public RitualSifrahTokenCreationKnowledge(string Blueprint)
		: base(Blueprint)
	{
	}

	public RitualSifrahTokenCreationKnowledge(GameObject Object)
		: base(Object)
	{
	}
}
