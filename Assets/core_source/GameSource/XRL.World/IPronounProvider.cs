namespace XRL.World;

public interface IPronounProvider
{
	string Name { get; }

	string CapitalizedName { get; }

	bool Generic { get; }

	bool Generated { get; }

	bool Plural { get; }

	bool PseudoPlural { get; }

	string Subjective { get; }

	string CapitalizedSubjective { get; }

	string Objective { get; }

	string CapitalizedObjective { get; }

	string PossessiveAdjective { get; }

	string CapitalizedPossessiveAdjective { get; }

	string SubstantivePossessive { get; }

	string CapitalizedSubstantivePossessive { get; }

	string Reflexive { get; }

	string CapitalizedReflexive { get; }

	string PersonTerm { get; }

	string CapitalizedPersonTerm { get; }

	string ImmaturePersonTerm { get; }

	string CapitalizedImmaturePersonTerm { get; }

	string FormalAddressTerm { get; }

	string CapitalizedFormalAddressTerm { get; }

	string OffspringTerm { get; }

	string CapitalizedOffspringTerm { get; }

	string SiblingTerm { get; }

	string CapitalizedSiblingTerm { get; }

	string ParentTerm { get; }

	string CapitalizedParentTerm { get; }

	string IndicativeProximal { get; }

	string CapitalizedIndicativeProximal { get; }

	string IndicativeDistal { get; }

	string CapitalizedIndicativeDistal { get; }

	bool UseBareIndicative { get; }
}
