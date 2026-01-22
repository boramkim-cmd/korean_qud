using System;

namespace XRL.World.Conversations;

[AttributeUsage(AttributeTargets.Method)]
public class ConversationDelegate : Attribute
{
	public string Key;

	public bool Inverse = true;

	public string InverseKey;

	public bool Speaker;

	public string SpeakerKey;

	public string SpeakerInverseKey;

	public bool Require = true;

	public string RequireKey;
}
