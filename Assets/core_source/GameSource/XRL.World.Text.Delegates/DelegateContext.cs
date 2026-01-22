using System.Collections.Generic;
using System.Text;

namespace XRL.World.Text.Delegates;

public class DelegateContext
{
	public StringBuilder Value;

	public GameObject Target;

	public IPronounProvider Pronouns = Gender.DefaultNeuter;

	public TargetType Type;

	public string Explicit;

	public string Default;

	public bool Capitalize;

	public int Flags;

	public List<string> Parameters;

	internal static DelegateContext Instance = new DelegateContext();

	internal static DelegateContext Set(StringBuilder Value, List<string> Parameters, GameObject Target, IPronounProvider Pronouns, TargetType Type, string Explicit, string Default, bool Capitalize, int Flags)
	{
		Instance.Value = Value;
		Instance.Parameters = Parameters;
		Instance.Target = Target;
		Instance.Type = Type;
		Instance.Pronouns = Pronouns ?? Target?.GetPronounProvider() ?? Gender.DefaultNeuter;
		Instance.Explicit = Explicit;
		Instance.Default = Default;
		Instance.Capitalize = Capitalize;
		Instance.Flags = Flags;
		return Instance;
	}
}
