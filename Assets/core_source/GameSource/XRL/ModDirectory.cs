using System.Collections.Generic;
using Newtonsoft.Json;
using XRL.UI;

namespace XRL;

public class ModDirectory
{
	public string[] Paths;

	public string Version;

	public string Build;

	public GameOption.RequiresSpec Options;

	public Dictionary<string, string> Dependencies;

	public Dictionary<string, string> Exclusions;

	[JsonProperty]
	private string Dependency
	{
		init
		{
			Dependencies = new Dictionary<string, string> { { value, "*" } };
		}
	}

	[JsonProperty]
	private string Exclusion
	{
		init
		{
			Exclusions = new Dictionary<string, string> { { value, "*" } };
		}
	}

	[JsonProperty]
	private string Path
	{
		init
		{
			Paths = new string[1] { value };
		}
	}

	[JsonProperty]
	private GameOption.RequiresSpec Option
	{
		init
		{
			Options = value;
		}
	}
}
