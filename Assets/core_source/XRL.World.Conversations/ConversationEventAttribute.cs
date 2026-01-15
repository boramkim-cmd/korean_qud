using System;

namespace XRL.World.Conversations;

[AttributeUsage(AttributeTargets.Class)]
public class ConversationEventAttribute : Attribute
{
	public bool Base;

	public ConversationEvent.Action Action;

	public ConversationEvent.Instantiation Instantiation = ConversationEvent.Instantiation.Pooling;
}
