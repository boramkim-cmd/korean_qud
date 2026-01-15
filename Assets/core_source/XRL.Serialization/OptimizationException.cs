using System;

namespace XRL.Serialization;

public class OptimizationException : Exception
{
	public OptimizationException(string message)
		: base(message)
	{
	}
}
