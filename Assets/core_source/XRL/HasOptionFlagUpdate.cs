using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Class)]
public class HasOptionFlagUpdate : Attribute
{
	public string Prefix;

	public bool FieldFlags;
}
