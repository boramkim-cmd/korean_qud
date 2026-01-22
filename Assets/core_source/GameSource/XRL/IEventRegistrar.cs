using System;
using XRL.World;

namespace XRL;

public interface IEventRegistrar
{
	bool IsUnregister { get; }

	void Register(IEventSource Source, IEventHandler Handler, int EventID, int Order = 0, bool Serialize = false);

	void Register(IEventSource Source, int EventID, int Order = 0, bool Serialize = false);

	void Register(int EventID, int Order = 0, bool Serialize = false);

	void Register(string EventID);

	[Obsolete("Use Register(string EventId)")]
	void RegisterPartEvent(IPart Ef, string Event)
	{
		Register(Event);
	}
}
