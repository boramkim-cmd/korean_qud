using UnityEngine;

namespace XRL.World.AI;

public abstract class IOpinion : IComposite
{
	public float Magnitude;

	public long Time;

	public int Value => Mathf.RoundToInt((float)BaseValue * Magnitude);

	public virtual bool WantFieldReflection => true;

	public abstract int BaseValue { get; }

	public virtual int Duration
	{
		get
		{
			if (BaseValue >= 0)
			{
				return 0;
			}
			return 16800;
		}
	}

	public virtual int Cooldown => 1200;

	public virtual float Limit => 1f;

	public virtual void Write(SerializationWriter Writer)
	{
	}

	public virtual void Read(SerializationReader Reader)
	{
	}

	public virtual string GetText(GameObject Actor)
	{
		return null;
	}

	public virtual bool HandleEvent(AfterAddOpinionEvent E)
	{
		return true;
	}
}
