using System;

namespace XRL.World;

[AttributeUsage(AttributeTargets.Class)]
public class GameEventAttribute : Attribute
{
	public string Seed;

	private int? _Cascade;

	public bool Base;

	public MinEvent.Cache Cache;

	public int Cascade
	{
		get
		{
			return _Cascade.GetValueOrDefault();
		}
		set
		{
			_Cascade = value;
		}
	}

	public bool HasCascade => _Cascade.HasValue;
}
