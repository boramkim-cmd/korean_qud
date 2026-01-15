using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Class)]
public class GameStateSingleton : Attribute
{
	public string ID;

	public GameStateSingleton()
	{
	}

	public GameStateSingleton(string ID)
	{
		this.ID = ID;
	}
}
[Obsolete("Use GameStateSingleton")]
[AttributeUsage(AttributeTargets.Class)]
public class GamestateSingleton : Attribute
{
	public string id;

	public GamestateSingleton(string ID)
	{
	}
}
