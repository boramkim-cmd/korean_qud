using System;

namespace XRL.World.Text.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class VariableReplacerAttribute : Attribute
{
	public string[] Keys = Array.Empty<string>();

	public string Default;

	public bool Capitalization;

	public bool Override;

	public int Flags;

	public VariableReplacerAttribute()
	{
	}

	public VariableReplacerAttribute(params string[] Keys)
	{
		this.Keys = Keys;
	}
}
