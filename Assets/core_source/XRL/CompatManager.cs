using System;
using System.Collections.Generic;
using XRL.Collections;
using XRL.UI;

namespace XRL;

[HasModSensitiveStaticCache]
public static class CompatManager
{
	public static bool Loaded;

	[ModSensitiveStaticCache(false, CreateEmptyInstance = true)]
	public static StringMap<StringMap<string>> CompatEntries;

	[ModSensitiveStaticCache(false, CreateEmptyInstance = false)]
	public static StringMap<string> Skills;

	[ModSensitiveStaticCache(false, CreateEmptyInstance = false)]
	public static StringMap<string> Mutations;

	[ModSensitiveStaticCache(false, CreateEmptyInstance = false)]
	public static StringMap<string> Factions;

	private static readonly Dictionary<string, Action<XmlDataHelper>> _outerNodes = new Dictionary<string, Action<XmlDataHelper>> { { "compat", HandleInnerNode } };

	private static readonly Dictionary<string, Action<XmlDataHelper>> _innerNodes = new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "skills", HandleDeprecatedNode },
		{ "mutations", HandleDeprecatedNode },
		{ "factions", HandleDeprecatedNode }
	};

	public static void CheckInit()
	{
		if (!Loaded || Skills == null)
		{
			Init();
		}
	}

	[ModSensitiveCacheInit]
	private static void Init()
	{
		Loaded = false;
		CompatEntries.Clear();
		if (Skills == null)
		{
			Skills = new StringMap<string>();
		}
		Skills.Clear();
		if (Mutations == null)
		{
			Mutations = new StringMap<string>();
		}
		Mutations.Clear();
		if (Factions == null)
		{
			Factions = new StringMap<string>();
		}
		Factions.Clear();
		CompatEntries.Add("skill", Skills);
		CompatEntries.Add("mutation", Mutations);
		CompatEntries.Add("faction", Factions);
		Loading.LoadTask("Loading Compat.xml", delegate
		{
			Loaded = true;
			foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("compat"))
			{
				item.HandleNodes(_outerNodes);
			}
		});
	}

	public static void HandleDeprecatedNode(XmlDataHelper Reader)
	{
		Reader.ParseWarning(Reader.Name + " node deprecated, the children of this node are parsed outside of the container anyway.");
		Reader.HandleNodes(_innerNodes, HandleGenericCompatNode);
	}

	public static void HandleInnerNode(XmlDataHelper xml)
	{
		xml.HandleNodes(_innerNodes, HandleGenericCompatNode);
	}

	public static void HandleGenericCompatNode(XmlDataHelper Reader)
	{
		string text = Reader.ParseAttribute<string>("Old", null, required: true);
		if (text.IsNullOrEmpty())
		{
			throw new Exception(Reader.Name + " tag had missing or empty Old attribute");
		}
		string text2 = Reader.ParseAttribute<string>("New", null, required: true);
		if (text2.IsNullOrEmpty())
		{
			throw new Exception(Reader.Name + " tag had missing or empty New attribute");
		}
		SetCompatEntry(Reader.Name.ToLower(), text, text2);
		Reader.DoneWithElement();
	}

	public static void SetCompatEntry(string Type, string Old, string New)
	{
		CheckInit();
		if (!CompatEntries.TryGetValue(Type, out var Value))
		{
			Value = new StringMap<string>();
			CompatEntries.Add(Type, Value);
		}
		Value[Old] = New;
	}

	public static bool TryGetCompatEntry(string Type, string ID, out string NewID)
	{
		CheckInit();
		NewID = null;
		if (CompatEntries.TryGetValue(Type, out var Value))
		{
			return Value.TryGetValue(ID, out NewID);
		}
		return false;
	}

	public static bool ProcessCompatEntry(string Type, ref string Name)
	{
		if (TryGetCompatEntry(Type, Name, out var NewID))
		{
			Name = NewID;
			return true;
		}
		return false;
	}

	public static string ProcessCompatEntry(string Type, string Name)
	{
		if (!TryGetCompatEntry(Type, Name, out var NewID))
		{
			return Name;
		}
		return NewID;
	}

	public static void ProcessPart(ref string Part)
	{
		ProcessSkill(ref Part);
		ProcessMutation(ref Part);
	}

	public static string ProcessPart(string Part)
	{
		ProcessPart(ref Part);
		return Part;
	}

	public static string GetNewPart(string Part)
	{
		string Part2 = Part;
		ProcessPart(ref Part2);
		if (Part2 == Part)
		{
			return null;
		}
		return Part2;
	}

	public static bool TryGetPart(string Part, out string NewPart, out string Type)
	{
		CheckInit();
		if (Skills.TryGetValue(Part, out NewPart))
		{
			Type = "Skill";
			return true;
		}
		if (Mutations.TryGetValue(Part, out NewPart))
		{
			Type = "Mutation";
			return true;
		}
		Type = null;
		return false;
	}

	public static bool TryGetPart(string Part, out string NewPart)
	{
		string Type;
		return TryGetPart(Part, out NewPart, out Type);
	}

	public static string GetNewSkill(string Skill)
	{
		CheckInit();
		if (Skills.TryGetValue(Skill, out var Value))
		{
			return Value;
		}
		return null;
	}

	public static void ProcessSkill(ref string Skill)
	{
		CheckInit();
		if (Skills.TryGetValue(Skill, out var Value))
		{
			Skill = Value;
		}
	}

	public static string ProcessSkill(string Skill)
	{
		CheckInit();
		if (Skills.TryGetValue(Skill, out var Value))
		{
			Skill = Value;
		}
		return Skill;
	}

	public static string GetNewMutation(string Mutation)
	{
		CheckInit();
		if (Mutations.TryGetValue(Mutation, out var Value))
		{
			return Value;
		}
		return null;
	}

	public static void ProcessMutation(ref string Mutation)
	{
		CheckInit();
		if (Mutations.TryGetValue(Mutation, out var Value))
		{
			Mutation = Value;
		}
	}

	public static string ProcessMutation(string Mutation)
	{
		CheckInit();
		if (Mutations.TryGetValue(Mutation, out var Value))
		{
			Mutation = Value;
		}
		return Mutation;
	}

	public static string GetNewFaction(string Faction)
	{
		CheckInit();
		if (Factions.TryGetValue(Faction, out var Value))
		{
			return Value;
		}
		return null;
	}

	public static void ProcessFaction(ref string Faction)
	{
		CheckInit();
		if (Factions.TryGetValue(Faction, out var Value))
		{
			Faction = Value;
		}
	}

	public static string ProcessFaction(string Faction)
	{
		CheckInit();
		if (Factions.TryGetValue(Faction, out var Value))
		{
			Faction = Value;
		}
		return Faction;
	}
}
