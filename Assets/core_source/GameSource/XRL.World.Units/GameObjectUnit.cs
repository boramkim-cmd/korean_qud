using System;

namespace XRL.World.Units;

[Serializable]
public abstract class GameObjectUnit
{
	public virtual void Initialize(GameObject Object)
	{
	}

	public virtual void Apply(GameObject Object)
	{
	}

	public virtual void Remove(GameObject Object)
	{
	}

	public virtual void Reset()
	{
	}

	public virtual bool CanInscribe()
	{
		return true;
	}

	public virtual string GetDescription(bool Inscription = false)
	{
		return "";
	}

	public override string ToString()
	{
		return base.ToString();
	}
}
