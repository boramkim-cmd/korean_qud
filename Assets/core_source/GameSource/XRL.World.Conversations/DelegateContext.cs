namespace XRL.World.Conversations;

public class DelegateContext
{
	public IConversationElement Element;

	public GameObject Target;

	public string Value;

	public object[] Separated;

	public static DelegateContext Instance = new DelegateContext();

	internal static DelegateContext Set(IConversationElement Element, string Value, GameObject Target)
	{
		Instance.Element = Element;
		Instance.Value = Value;
		Instance.Target = Target;
		return Instance;
	}
}
