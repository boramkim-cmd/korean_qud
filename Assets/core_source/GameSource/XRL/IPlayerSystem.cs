using System;
using XRL.World;

namespace XRL;

[Serializable]
public abstract class IPlayerSystem : IGameSystem
{
	public override void ApplyRegistrar(XRLGame Game)
	{
		EventRegistrar eventRegistrar = EventRegistrar.Get(Game, this);
		eventRegistrar.Register(PooledEvent<AfterPlayerBodyChangeEvent>.ID);
		Register(Game, eventRegistrar);
		GameObject body = Game.Player._Body;
		if (body != null)
		{
			eventRegistrar.Source = body;
			RegisterPlayer(body, eventRegistrar);
		}
	}

	public override void ApplyUnregistrar(XRLGame Game)
	{
		EventUnregistrar eventUnregistrar = EventUnregistrar.Get(Game, this);
		eventUnregistrar.Register(PooledEvent<AfterPlayerBodyChangeEvent>.ID);
		Register(Game, eventUnregistrar);
		GameObject body = Game.Player._Body;
		if (body != null)
		{
			eventUnregistrar.Source = body;
			RegisterPlayer(body, eventUnregistrar);
		}
	}

	public virtual void RegisterPlayer(GameObject Player, IEventRegistrar Registrar)
	{
	}

	public override bool HandleEvent(AfterPlayerBodyChangeEvent E)
	{
		if (E.OldBody != null)
		{
			RegisterPlayer(E.OldBody, EventUnregistrar.Get(E.OldBody, this));
		}
		if (E.NewBody != null)
		{
			RegisterPlayer(E.NewBody, EventRegistrar.Get(E.NewBody, this));
		}
		return base.HandleEvent(E);
	}
}
