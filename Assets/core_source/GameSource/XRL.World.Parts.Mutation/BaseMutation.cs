using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class BaseMutation : IPart
{
	public struct LevelCalculation
	{
		public int bonus;

		public bool temporary;

		public string reason;

		public char sigil;
	}

	public string Variant;

	[Obsolete("Preserves SaveCompat, use GetDisplayName() or Set DisplayName on XML Node.")]
	public string _DisplayName = "";

	public int BaseLevel = 1;

	public int LastLevel;

	public Guid ActivatedAbilityID;

	[NonSerialized]
	private List<LevelCalculation> _levelCalcWorkspace = new List<LevelCalculation>();

	[NonSerialized]
	[Obsolete("Use GetMaxLevel()")]
	protected int MaxLevel = -1;

	[NonSerialized]
	[Obsolete("Cache Value Only, use GetMutationType()")]
	public string _Type;

	[NonSerialized]
	private MutationEntry _Entry;

	private string EnergyUseType;

	public int CapOverride = -1;

	[NonSerialized]
	private string _RapidKey;

	public string DisplayName
	{
		[Obsolete("Use GetDisplayName()")]
		get
		{
			return GetDisplayName();
		}
		[Obsolete("All mutations should now have MutationEntries in XML. Set DisplayName on XML node.")]
		set
		{
			_DisplayName = value;
		}
	}

	public virtual int BaseElementWeight
	{
		get
		{
			if (BaseLevel > 0)
			{
				return 3 + BaseLevel;
			}
			return 0;
		}
	}

	public virtual bool HasVariants
	{
		get
		{
			List<string> variants = GetVariants();
			if (variants != null)
			{
				return variants.Count > 1;
			}
			return false;
		}
	}

	public virtual bool CanSelectVariant => true;

	public virtual bool UseVariantName => true;

	public int Level
	{
		get
		{
			return CalcLevel();
		}
		set
		{
			BaseLevel = value;
			if (ParentObject != null)
			{
				SyncMutationLevelsEvent.Send(ParentObject);
			}
		}
	}

	public string Type
	{
		get
		{
			return GetMutationType();
		}
		[Obsolete("Should not need to set Type. Defaults to mutation Type in Mutations.xml, or the mutation category's Name.")]
		set
		{
			_Type = value;
		}
	}

	public new StatShifter StatShifter
	{
		get
		{
			StatShifter statShifter = base.StatShifter;
			if (string.IsNullOrEmpty(statShifter.DefaultDisplayName))
			{
				statShifter.DefaultDisplayName = GetDisplayName(WithAnnotations: false);
			}
			return statShifter;
		}
	}

	protected virtual string GetBaseDisplayName()
	{
		return GetMutationEntry().GetDisplayName();
	}

	public string GetDisplayName(bool WithAnnotations = true)
	{
		if (!string.IsNullOrEmpty(_DisplayName))
		{
			return _DisplayName;
		}
		if (UseVariantName)
		{
			_DisplayName = GetVariantName(Variant);
		}
		if (string.IsNullOrEmpty(_DisplayName))
		{
			_DisplayName = GetBaseDisplayName() ?? "";
		}
		if (WithAnnotations && IsDefect())
		{
			return _DisplayName + " ({{r|D}})";
		}
		return _DisplayName;
	}

	public void SetDisplayName(string DisplayName)
	{
		_DisplayName = DisplayName;
	}

	public void ResetDisplayName()
	{
		_DisplayName = null;
	}

	public int CalcLevel()
	{
		return CalcLevel(storeWork: false);
	}

	public List<LevelCalculation> GetLevelCalculations()
	{
		CalcLevel(storeWork: true);
		return _levelCalcWorkspace;
	}

	public virtual int GetUIDisplayLevel()
	{
		return Level;
	}

	private int CalcLevel(bool storeWork = false)
	{
		if (storeWork)
		{
			_levelCalcWorkspace.Clear();
		}
		if (!CanLevel() || ParentObject == null)
		{
			return BaseLevel;
		}
		string text = "mutation";
		int num = BaseLevel;
		if (storeWork)
		{
			text = GetMutationTermEvent.GetFor(ParentObject, this);
			if (num <= 0)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = num,
					sigil = '\0',
					temporary = false,
					reason = "* You do not possess this " + text + " inherently, and so you cannot advance its rank."
				});
			}
			else
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = num,
					sigil = '\0',
					temporary = false,
					reason = "* This " + Grammar.MakePossessive(text) + " base rank is " + num + "."
				});
			}
		}
		MutationEntry mutationEntry = GetMutationEntry();
		Statistic value = null;
		Dictionary<string, Statistic> statistics = ParentObject.Statistics;
		if (statistics != null && statistics.TryGetValue(mutationEntry?.GetStat() ?? "", out value))
		{
			num += value.Modifier;
			if (value.Modifier > 0 && storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = value.Modifier,
					sigil = '+',
					temporary = false,
					reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + value.Modifier + " due to your high " + value.Name + "."
				});
			}
			if (value.Modifier < 0 && storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = value.Modifier,
					sigil = '-',
					temporary = false,
					reason = "- This " + Grammar.MakePossessive(text) + " rank is decreased by " + -value.Modifier + " due to your low " + value.Name + "."
				});
			}
		}
		int intProperty = ParentObject.GetIntProperty(mutationEntry?.Class);
		num += intProperty;
		if (intProperty != 0)
		{
			MetricsManager.LogWarning("Using old ClassName based IntProperty to adjust mutation level, use RequirePart<Mutations>().AddMutationMod(...):" + DebugName);
		}
		intProperty = ParentObject.GetIntProperty(mutationEntry?.GetProperty());
		num += intProperty;
		if (intProperty != 0)
		{
			MetricsManager.LogWarning("Using old MutationEntry.Property based IntProperty to adjust mutation level, use RequirePart<Mutations>().AddMutationMod(...):" + DebugName);
		}
		intProperty = ParentObject.GetIntProperty("AllMutationLevelModifier");
		num += intProperty;
		if (intProperty > 0 && storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '+',
				reason = "+ All your " + Grammar.Pluralize(text) + "' ranks are increased by " + intProperty + "."
			});
		}
		if (intProperty < 0 && storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '-',
				reason = "- All your " + Grammar.Pluralize(text) + "' ranks are decreased by " + -intProperty + "."
			});
		}
		intProperty = ParentObject.GetIntProperty(mutationEntry?.Category?.CategoryModifierName ?? "UnknownMutationLevelModifier");
		num += intProperty;
		if (intProperty > 0 && storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '+',
				reason = "+ All your " + mutationEntry?.Category?.DisplayName + " " + Grammar.MakePossessive(Grammar.Pluralize(text)) + " ranks are increased by " + intProperty + "."
			});
		}
		if (intProperty < 0 && storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '-',
				reason = "- All your " + mutationEntry?.Category?.DisplayName + " " + Grammar.MakePossessive(Grammar.Pluralize(text)) + " ranks are decreased by " + -intProperty + "."
			});
		}
		if (GetType() != typeof(AdrenalControl2) && IsPhysical())
		{
			intProperty = ParentObject.GetIntProperty("AdrenalLevelModifier");
			num += intProperty;
			if (intProperty > 0 && storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					temporary = true,
					bonus = intProperty,
					sigil = '+',
					reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + intProperty + " due to your high adrenaline."
				});
			}
		}
		intProperty = GetRapidLevelAmount();
		num += intProperty;
		if (intProperty > 0 && storeWork)
		{
			_levelCalcWorkspace.Add(new LevelCalculation
			{
				temporary = false,
				bonus = intProperty,
				sigil = '+',
				reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + intProperty + " due to being rapidly advanced " + intProperty / 3 + " time" + ((intProperty > 3) ? "s" : "") + "."
			});
		}
		Mutations part = ParentObject.GetPart<Mutations>();
		if (part != null)
		{
			for (int i = 0; i < part.MutationMods.Count; i++)
			{
				Mutations.MutationModifierTracker mutationModifierTracker = part.MutationMods[i];
				if (mutationModifierTracker.mutationName != base.Name)
				{
					continue;
				}
				num += mutationModifierTracker.bonus;
				if (storeWork)
				{
					switch (mutationModifierTracker.sourceType)
					{
					case Mutations.MutationModifierTracker.SourceType.Cooking:
						_levelCalcWorkspace.Add(new LevelCalculation
						{
							temporary = true,
							bonus = mutationModifierTracker.bonus,
							sigil = '+',
							reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + mutationModifierTracker.bonus + " due to a metabolizing effect."
						});
						break;
					case Mutations.MutationModifierTracker.SourceType.Tonic:
						_levelCalcWorkspace.Add(new LevelCalculation
						{
							temporary = true,
							bonus = mutationModifierTracker.bonus,
							sigil = '+',
							reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + mutationModifierTracker.bonus + " due to a tonic effect."
						});
						break;
					case Mutations.MutationModifierTracker.SourceType.Equipment:
						_levelCalcWorkspace.Add(new LevelCalculation
						{
							temporary = true,
							bonus = mutationModifierTracker.bonus,
							sigil = '+',
							reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + mutationModifierTracker.bonus + " due to your equipped item, " + mutationModifierTracker.sourceName + "."
						});
						break;
					case Mutations.MutationModifierTracker.SourceType.External:
						_levelCalcWorkspace.Add(new LevelCalculation
						{
							temporary = true,
							bonus = mutationModifierTracker.bonus,
							sigil = '+',
							reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + mutationModifierTracker.bonus + " due to " + mutationModifierTracker.sourceName + "."
						});
						break;
					default:
						_levelCalcWorkspace.Add(new LevelCalculation
						{
							temporary = true,
							bonus = mutationModifierTracker.bonus,
							sigil = ((mutationModifierTracker.bonus > 0) ? '+' : '-'),
							reason = "+ This " + Grammar.MakePossessive(text) + " rank is increased by " + Math.Abs(mutationModifierTracker.bonus) + " due to your " + mutationModifierTracker.sourceName + "."
						});
						break;
					}
				}
			}
		}
		if (num < 1)
		{
			if (storeWork)
			{
				_levelCalcWorkspace.Add(new LevelCalculation
				{
					bonus = 1 - num,
					temporary = false,
					sigil = '+',
					reason = "+ " + ColorUtility.CapitalizeExceptFormatting(text) + " ranks cannot be reduced below 1."
				});
			}
			num = 1;
		}
		int mutationCap = GetMutationCap();
		if (mutationCap != -1 && mutationCap < num)
		{
			if (storeWork)
			{
				bool flag = false;
				int num2 = num - mutationCap;
				int num3 = _levelCalcWorkspace.Where((LevelCalculation c) => c.temporary).Aggregate(0, (int a, LevelCalculation b) => a + b.bonus);
				if (num3 > 0)
				{
					int num4 = Math.Min(num2, num3);
					_levelCalcWorkspace.Add(new LevelCalculation
					{
						bonus = -num4,
						temporary = true,
						sigil = '-',
						reason = "- This " + Grammar.MakePossessive(text) + " rank is capped at " + mutationCap + " due to your level."
					});
					num2 -= num4;
					flag = true;
				}
				if (num2 > 0)
				{
					_levelCalcWorkspace.Add(new LevelCalculation
					{
						bonus = -num2,
						temporary = false,
						sigil = ((!flag) ? '-' : '\0'),
						reason = (flag ? null : ("- This " + Grammar.MakePossessive(text) + " rank is capped at " + mutationCap + " due to your level."))
					});
				}
			}
			num = mutationCap;
		}
		intProperty = ParentObject.GetIntProperty(mutationEntry?.GetCategoryForceProperty());
		intProperty += ParentObject.GetIntProperty(mutationEntry?.GetForceProperty());
		return num + intProperty;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != PooledEvent<GetPsychicGlimmerEvent>.ID || !(Type == "Mental")) && (ID != PooledEvent<IsSensableAsPsychicEvent>.ID || !(Type == "Mental")))
		{
			if (ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID && ActivatedAbilityID != Guid.Empty)
			{
				_ = ActivatedAbilityID;
				return true;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPsychicGlimmerEvent E)
	{
		if (Type == "Mental" && E.Subject == ParentObject && !IsDefect())
		{
			E.Level += Level;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		if (Type == "Mental")
		{
			E.Sensable = true;
		}
		return base.HandleEvent(E);
	}

	public void SyncLevel()
	{
		Mutations.SyncMutation(this, SyncGlimmer: true);
	}

	public virtual bool CompatibleWith(GameObject go)
	{
		List<MutationEntry> mutationEntries = MutationFactory.GetMutationEntries(this);
		if (mutationEntries == null)
		{
			return true;
		}
		foreach (MutationEntry item in mutationEntries)
		{
			string[] exclusions = item.GetExclusions();
			foreach (string name in exclusions)
			{
				if (MutationFactory.HasMutation(name))
				{
					string name2 = MutationFactory.GetMutationEntryByName(name).Class;
					if (go.HasPart(name2))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public int GetTemporaryLevels()
	{
		return (from v in GetLevelCalculations()
			where v.temporary
			select v).Aggregate(0, (int memo, LevelCalculation value) => memo + value.bonus);
	}

	public bool IsPhysical()
	{
		return GetMutationEntry()?.IsPhysical() ?? false;
	}

	public bool IsMental()
	{
		return GetMutationEntry()?.IsMental() ?? false;
	}

	public bool IsDefect()
	{
		return GetMutationEntry()?.IsDefect() ?? false;
	}

	public virtual int GetMaxLevel()
	{
		if (MaxLevel == -1)
		{
			MaxLevel = 10;
			string name = GetType().Name;
			foreach (MutationEntry item in MutationFactory.AllMutationEntries())
			{
				if (item.Class.EqualsNoCase(name) && item.MaxLevel > -1)
				{
					MaxLevel = item.MaxLevel;
					break;
				}
			}
		}
		return MaxLevel;
	}

	public bool CanIncreaseLevel()
	{
		if (CanLevel() && BaseLevel < GetMaxLevel())
		{
			return Level < GetMutationCap();
		}
		return false;
	}

	public virtual string GetMutationType()
	{
		return _Type ?? (_Type = GetMutationEntry()?.Type ?? "Physical");
	}

	public bool isCategory(string category)
	{
		return GetMutationEntry()?.Category?.Name == category;
	}

	public MutationEntry GetMutationEntry()
	{
		if (_Entry != null)
		{
			return _Entry;
		}
		List<MutationEntry> mutationEntries = MutationFactory.GetMutationEntries(this);
		if (mutationEntries.IsNullOrEmpty())
		{
			string text = (string.IsNullOrEmpty(_DisplayName) ? "..." : _DisplayName);
			MetricsManager.LogError("Mutation entry not found for '" + GetType().Name + "'. Please add `<mutation Name=\"" + text + "\" Class=\"" + GetType().Name + "\" />` node to a mutations xml file.");
			return _Entry = MutationFactory.CreateMutationEntryForMutation(this);
		}
		if (mutationEntries.Count == 1)
		{
			return _Entry = mutationEntries[0];
		}
		string variant = Variant;
		MutationEntry mutationEntry;
		if (variant == null)
		{
			foreach (MutationEntry item in mutationEntries)
			{
				if (item.Variant != null || item.HasVariants)
				{
					continue;
				}
				mutationEntry = (_Entry = item);
				mutationEntry = mutationEntry;
				goto IL_0182;
			}
		}
		else
		{
			foreach (MutationEntry item2 in mutationEntries)
			{
				if (!(item2.Variant == variant) && (!item2.HasVariants || !item2.GetVariants().Contains(variant)))
				{
					continue;
				}
				mutationEntry = (_Entry = item2);
				goto IL_0182;
			}
		}
		return _Entry = mutationEntries[0];
		IL_0182:
		return mutationEntry;
	}

	public string GetMutationClass()
	{
		return GetMutationEntry()?.Class;
	}

	public string GetBearerDescription()
	{
		return GetMutationEntry()?.Snippet ?? "";
	}

	public void UseEnergy(int Amount)
	{
		if (EnergyUseType == null)
		{
			EnergyUseType = Type + " Mutation";
		}
		ParentObject.UseEnergy(Amount, EnergyUseType);
	}

	public void UseEnergy(int Amount, string Type)
	{
		ParentObject.UseEnergy(Amount, Type);
	}

	public static int GetMutationCapForLevel(int level)
	{
		return level / 2 + 1;
	}

	public virtual int GetMutationCap()
	{
		GameObject parentObject = ParentObject;
		if (parentObject != null && parentObject.HasStat("Level"))
		{
			return Math.Max(CapOverride, GetMutationCapForLevel(ParentObject.Stat("Level")));
		}
		return CapOverride;
	}

	public string GetStat()
	{
		MutationEntry mutationEntry = GetMutationEntry();
		if (mutationEntry != null)
		{
			if (string.IsNullOrEmpty(mutationEntry.Stat) && mutationEntry.Category != null && !string.IsNullOrEmpty(mutationEntry.Category.Stat))
			{
				mutationEntry.Stat = mutationEntry.Category.Stat;
			}
			return mutationEntry.Stat;
		}
		return null;
	}

	public virtual bool ShouldShowLevel()
	{
		return CanLevel();
	}

	public virtual bool CanLevel()
	{
		return true;
	}

	public virtual bool AffectsBodyParts()
	{
		return false;
	}

	public virtual bool GeneratesEquipment()
	{
		return false;
	}

	public virtual string GetCreateCharacterDisplayName()
	{
		return GetDisplayName();
	}

	public virtual void CollectStats(Templates.StatCollector stats)
	{
		CollectStats(stats, Level);
	}

	public virtual void CollectStats(Templates.StatCollector stats, int Level)
	{
	}

	public virtual string GetDescription()
	{
		if (Templates.TemplateByID.TryGetValue("Mutation." + GetMutationClass() + ".Description", out var value))
		{
			return value.Build(CollectStats, "mutation description");
		}
		return "<description>";
	}

	public virtual string GetLevelText(int Level)
	{
		if (Templates.TemplateByID.TryGetValue("Mutation." + GetMutationClass() + ".LevelText", out var value))
		{
			return value.Build(delegate(Templates.StatCollector stats)
			{
				CollectStats(stats, Level);
			}, "mutation leveltext").Trim('\n');
		}
		return "<Does level " + Level + " stuff>";
	}

	public virtual bool Mutate(GameObject GO, int Level = 1)
	{
		BaseLevel = Level;
		ChangeLevel(this.Level);
		return true;
	}

	public virtual void AfterMutate()
	{
	}

	public virtual bool Unmutate(GameObject GO)
	{
		LastLevel = 0;
		return true;
	}

	public virtual void AfterUnmutate(GameObject GO)
	{
	}

	public int GetRapidLevelAmount()
	{
		if (_RapidKey == null)
		{
			_RapidKey = "RapidLevel_" + base.Name;
		}
		return ParentObject.GetIntProperty(_RapidKey);
	}

	public void SetRapidLevelAmount(int Amount, bool Sync = true)
	{
		if (_RapidKey == null)
		{
			_RapidKey = "RapidLevel_" + base.Name;
		}
		ParentObject.SetIntProperty(_RapidKey, Amount, RemoveIfZero: true);
		ChangeLevel(Level);
		if (Sync)
		{
			ParentObject.SyncMutationLevelAndGlimmer();
		}
	}

	public virtual void RapidLevel(int Amount, bool Sync = true)
	{
		if (_RapidKey == null)
		{
			_RapidKey = "RapidLevel_" + base.Name;
		}
		ParentObject.ModIntProperty(_RapidKey, Amount, RemoveIfZero: true);
		ChangeLevel(Level);
		if (Sync)
		{
			ParentObject.SyncMutationLevelAndGlimmer();
		}
	}

	public virtual bool ChangeLevel(int NewLevel)
	{
		if (NewLevel >= 15 && LastLevel < 15 && ParentObject != null && ParentObject.IsPlayer() && !ParentObject.HasEffect<Dominated>())
		{
			Achievement.GET_MUTATION_LEVEL_15.Unlock();
		}
		LastLevel = NewLevel;
		return true;
	}

	protected void CleanUpMutationEquipment(GameObject who, ref GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		if (who != null)
		{
			BodyPart bodyPart = obj.EquippedOn();
			if (bodyPart != null)
			{
				who.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", bodyPart).SetSilent(Silent: true));
			}
			else
			{
				bodyPart = who.Body?.FindDefaultOrEquippedItem(obj);
				if (bodyPart != null && bodyPart.DefaultBehavior == obj)
				{
					bodyPart.DefaultBehavior = null;
				}
			}
		}
		obj.Obliterate();
		obj = null;
	}

	public virtual List<string> GetVariants()
	{
		return Mutations.GetVariants(base.Name);
	}

	public virtual List<string> CreateVariants()
	{
		List<string> list = new List<string>();
		string name = base.Name;
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (blueprint.Tags.TryGetValue("MutationEquipment", out var value) && value == name && !blueprint.IsBaseBlueprint())
			{
				list.Add(blueprint.Name);
			}
		}
		return list;
	}

	public virtual string GetVariantName()
	{
		return GetVariantName(Variant);
	}

	public virtual string GetVariantName(string Blueprint)
	{
		return GetVariantName(GameObjectFactory.Factory.GetBlueprintIfExists(Blueprint));
	}

	public virtual string GetVariantName(GameObjectBlueprint Blueprint)
	{
		if (Blueprint == null)
		{
			return null;
		}
		if (!Blueprint.Tags.TryGetValue("VariantName", out var value))
		{
			return Blueprint.CachedDisplayNameStrippedTitleCase;
		}
		return value;
	}

	public virtual void SetVariant(string Variant)
	{
		this.Variant = Variant;
		if (UseVariantName)
		{
			ResetDisplayName();
		}
	}

	public virtual IRenderable GetIcon(string Variant)
	{
		GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(Variant);
		if (blueprintIfExists != null)
		{
			return new Renderable(blueprintIfExists);
		}
		return null;
	}

	public virtual IRenderable GetIcon()
	{
		if (Variant == null)
		{
			if (MutationFactory.TryGetMutationEntry(this, out var Entry))
			{
				return Entry.GetRenderable();
			}
			return null;
		}
		return GetIcon(Variant);
	}

	public virtual bool TryGetVariantValidity(GameObject Object, string Variant, out string Message)
	{
		Message = null;
		return true;
	}

	public bool SelectVariant(GameObject Object, bool AllowEscape = true)
	{
		List<string> variants = GetVariants();
		if (variants.IsNullOrEmpty())
		{
			return false;
		}
		string[] array = new string[variants.Count];
		IRenderable[] array2 = new IRenderable[variants.Count];
		bool[] array3 = new bool[variants.Count];
		for (int i = 0; i < variants.Count; i++)
		{
			string text = variants[i];
			array[i] = GetVariantName(text);
			array2[i] = GetIcon(text);
			array3[i] = TryGetVariantValidity(Object, text, out var Message);
			if (!array3[i])
			{
				array[i] = array[i].WithColor("K") + "\n" + Message;
			}
		}
		int num = 0;
		while (true)
		{
			num = Popup.PickOption("Choose variant", null, "", "Sounds/UI/ui_notification", array, null, array2, null, null, null, null, 0, 60, num, -1, AllowEscape);
			if (num < 0)
			{
				break;
			}
			if (array3[num])
			{
				SetVariant(variants[num]);
				return true;
			}
		}
		return false;
	}

	public string GetMutationTerm(GameObject who = null)
	{
		return GetMutationTermEvent.GetFor(who ?? ParentObject, this);
	}

	protected void CleanUpMutationEquipment(GameObject who, GameObject obj)
	{
		CleanUpMutationEquipment(who, ref obj);
	}

	public static BaseMutation Create(string Mutation, string Variant = null)
	{
		BaseMutation genericMutation = Mutations.GetGenericMutation(Mutation, Variant);
		if (genericMutation != null)
		{
			return Create(genericMutation.GetType(), Variant);
		}
		return null;
	}

	public static BaseMutation Create(Type Mutation, string Variant = null)
	{
		BaseMutation baseMutation = (BaseMutation)GamePartBlueprint.PartReflectionCache.Get(Mutation).GetNewInstance();
		if (!Variant.IsNullOrEmpty())
		{
			baseMutation.SetVariant(Variant);
		}
		return baseMutation;
	}

	public static BaseMutation Create(MutationEntry Mutation)
	{
		return Create(Mutation.Class, Mutation.Variant);
	}

	public static T Create<T>(string Variant = null) where T : BaseMutation
	{
		return (T)Create(typeof(T), Variant);
	}
}
