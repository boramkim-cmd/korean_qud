using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Field)]
public class GameBasedStaticCacheAttribute : Attribute
{
	public bool CreateInstance = true;

	public bool ClearInstance;

	public GameBasedStaticCacheAttribute(bool CreateInstance = true, bool ClearInstance = false)
	{
		this.CreateInstance = CreateInstance;
		this.ClearInstance = ClearInstance;
	}
}
