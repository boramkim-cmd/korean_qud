# Critical Review: OBJECT_LOCALIZATION_PLAN.md
# Comparison Against Actual Codebase Implementation

**Review Date**: 2026-01-22  
**Reviewer**: AI Code Review  
**Document Version**: 1.0 / 1.1  

---

## Executive Summary

The OBJECT_LOCALIZATION_PLAN.md is a well-structured document that covers most fundamental concepts. However, code analysis reveals several **critical errors**, **significant gaps**, and **unrealistic assumptions** that must be addressed before implementation.

| Category | Count | Severity |
|----------|-------|----------|
| ERRORS | 6 | High |
| GAPS | 8 | Medium-High |
| UNREALISTIC | 4 | Medium |
| IMPROVEMENTS | 5 | - |
| VALIDATION | 7 | ✓ |

---

## 1. ERRORS: Factual Mistakes in the Document

### ERR-R01: GetFor() Signature Mismatch (CRITICAL)

**Document Section 5.1 claims**:
```csharp
static void GetFor_Postfix(ref string __result, GameObject Object, string Base)
```

**Actual source** ([GetDisplayNameEvent.cs#L74](Assets/core_source/GameSource/XRL.World/GetDisplayNameEvent.cs#L74)):
```csharp
public static string GetFor(GameObject Object, string Base, 
    int Cutoff = int.MaxValue, string Context = null, 
    bool AsIfKnown = false, bool Single = false, 
    bool NoConfusion = false, bool NoColor = false, 
    bool ColorOnly = false, bool Visible = true, 
    bool BaseOnly = false, bool UsingAdjunctNoun = false, 
    bool WithoutTitles = false, bool ForSort = false, 
    bool Reference = false, bool IncludeImplantPrefix = true)
```

**Problems**:
1. **16 parameters** - Document only shows 2
2. Parameters like `AsIfKnown`, `NoColor`, `ColorOnly` affect translation decisions
3. `ForSort` returns text for sorting (should NOT translate)
4. `ColorOnly` returns just color info (no text to translate)

**Impact**: Patch will incorrectly translate sort keys and color-only queries.

---

### ERR-R02: GetShortDescription() is NOT the Main Description Path

**Document Section 5.2 claims patching `Description.GetShortDescription()` is sufficient**

**Actual source** ([Description.cs#L70-L97](Assets/core_source/GameSource/XRL.World.Parts/Description.cs#L70)):
```csharp
public string GetShortDescription(bool AsIfKnown = false, bool NoConfusion = false, string Context = null)
{
    // ...
    int num = (AsIfKnown ? 2 : ParentObject.GetEpistemicStatus());
    switch (num)
    {
    case 0: // UNKNOWN
        shortDescriptionEvent = GetUnknownShortDescriptionEvent.FromPool(...);
        break;
    case 1: // PARTIAL
        shortDescriptionEvent = GetUnknownShortDescriptionEvent.FromPool(...);
        break;
    default: // KNOWN
        shortDescriptionEvent = GetShortDescriptionEvent.FromPool(..., _Short, ...);
        break;
    }
}
```

**Problems**:
1. Unknown items use `GetUnknownShortDescriptionEvent` - separate event class
2. `Examiner.GetActiveShortDescription()` is called for unknown items
3. Three different paths: known, unknown, partial epistemic states

**Impact**: Unknown/unidentified items will NOT be translated.

---

### ERR-R03: Corpse Name Generation Not As Described

**Document Section 6.3 claims**:
> Corpse names are dynamically generated: "{creature} corpse"

**Actual source** ([Corpse.cs#L140-L154](Assets/core_source/GameSource/XRL.World.Parts/Corpse.cs#L140)):
```csharp
if (ParentObject.HasProperName)
{
    gameObject.SetStringProperty("CreatureName", ParentObject.BaseDisplayName);
}
else
{
    string text = NameMaker.MakeName(ParentObject, ...);
    if (text != null)
    {
        gameObject.SetStringProperty("CreatureName", text);
    }
}
```

**Problems**:
1. Corpses store `CreatureName` as StringProperty, not in DisplayName
2. `NameMaker.MakeName()` generates procedural names
3. Corpse blueprint determines base display name (e.g., "Fresh Corpse")
4. Full name composition happens elsewhere

**Impact**: Simple string suffix replacement won't work.

---

### ERR-R04: Missing DescriptionBuilder Complexity

**Document does not mention DescriptionBuilder at all**

**Actual source** ([GetDisplayNameEvent.cs#L13-L15](Assets/core_source/GameSource/XRL.World/GetDisplayNameEvent.cs#L13)):
```csharp
public class GetDisplayNameEvent : PooledEvent<GetDisplayNameEvent>
{
    public DescriptionBuilder DB = new DescriptionBuilder();
```

**The `DescriptionBuilder` handles**:
- `AddBase()` - base name
- `AddAdjective()` - prefixes like "rusty", "masterwork"
- `AddClause()` - postfixes
- `AddHonorific()`, `AddEpithet()`, `AddTitle()` - creature titles
- `AddTag()` - status tags
- Color management

**Impact**: Document's simple `ReplacePreservingTags()` approach is insufficient.

---

### ERR-R05: ProcessFor() Is The Actual Entry Point

**Document patches `GetFor()` but misses `ProcessFor()`**

**Actual source** ([GetDisplayNameEvent.cs#L195-L250](Assets/core_source/GameSource/XRL.World/GetDisplayNameEvent.cs#L195)):
```csharp
public string ProcessFor(GameObject obj, bool NoReturn = false)
{
    obj.HandleEvent(this);
    if (obj.HasRegisteredEvent("GetDisplayName"))
    {
        // Legacy event handler support
        Event obj2 = Event.New("GetDisplayName");
        // ... fires legacy event
    }
}
```

**Problems**:
1. `ProcessFor()` is where all Part handlers contribute
2. `HandleEvent(GetDisplayNameEvent)` on each Part modifies the name
3. Legacy `"GetDisplayName"` event also modifies names

**Impact**: Patching only `GetFor()` postfix sees final result, but can't intercept parts.

---

### ERR-R06: PooledEvent Reset Destroys Cached References

**Document proposes caching but ignores pooling**

**Actual source** ([GetDisplayNameEvent.cs#L58-L71](Assets/core_source/GameSource/XRL.World/GetDisplayNameEvent.cs#L58)):
```csharp
[GameEvent(Cache = Cache.Pool)]
public class GetDisplayNameEvent : PooledEvent<GetDisplayNameEvent>
{
    public override void Reset()
    {
        Object = null;
        DB.Clear();
        // ... resets all fields
    }
}
```

And in `GetFor()`:
```csharp
GetDisplayNameEvent E = PooledEvent<GetDisplayNameEvent>.FromPool();
// ... use E ...
PooledEvent<GetDisplayNameEvent>.ResetTo(ref E);  // Returns to pool!
```

**Impact**: Cannot store event object references; must copy data immediately.

---

## 2. GAPS: Important Missing Information

### GAP-R01: Examiner Part Unknown Items System

**Document mentions "UnknownPistol" but misses critical details**

**Actual source** ([Examiner.cs#L46-L51](Assets/core_source/GameSource/XRL.World.Parts/Examiner.cs#L46)):
```csharp
public string Alternate = "BaseUnknown";
public string Unknown = "BaseUnknown";
public int EpistemicStatus = -1;  // -1=uninitialized, 0=unknown, 1=partial, 2=known
```

**Missing from document**:
1. `BaseUnknown` is the default unknown type - must be translated
2. Epistemic status system determines which name to show
3. `Examiner.HandleEvent(GetDisplayNameEvent)` replaces name entirely for unknown items
4. `GetActiveSample()` creates proxy GameObject with alternate name

---

### GAP-R02: Multiple DisplayName Event Handlers

**Document assumes single patch point**

**Parts that handle GetDisplayNameEvent** (non-exhaustive):
- `Examiner` - unknown item names
- `ModLegendary` - adds "legendary" prefix
- `ModMasterwork` - adds "masterwork" prefix  
- `Cursed` - adds cursed indicators
- `LiquidVolume` - adds liquid contents
- `Titles` - adds creature titles
- `Epithets` - adds epithets
- Many Mod*.cs parts

**Impact**: A single postfix patch must handle all these modifications.

---

### GAP-R03: Look.GenerateTooltipInformation() 

**Document doesn't mention the Look tooltip system**

**Actual source** ([Description.cs#L310](Assets/core_source/GameSource/XRL.World.Parts/Description.cs#L310)):
```csharp
Look.TooltipInformation tooltipInformation = Look.GenerateTooltipInformation(ParentObject);
```

**This generates**:
- `LongDescription`
- `WoundLevel`
- `DisplayName`
- `SubHeader`
- `IconRenderable`

**Impact**: Need to patch `Look.GenerateTooltipInformation()` for tooltip translations.

---

### GAP-R04: Hardcoded UI Labels in Description.cs

**Actual source** ([Description.cs#L231-L240](Assets/core_source/GameSource/XRL.World.Parts/Description.cs#L231)):
```csharp
if (Gender.EnableDisplay)
{
    SB.Append("\n\nGender: ").Append(value);  // HARDCODED
}
if (FeatureItems.Count > 0)
{
    SB.Append("\n\nPhysical features: ").Append(FeatureItems[0]);  // HARDCODED
}
if (EquipItems.Count > 0)
{
    SB.Append("\nEquipped: ").Append(EquipItems[0]);  // HARDCODED
}
```

And feeling descriptions:
```csharp
return brain.GetFeelingLevel(who) switch
{
    Brain.FeelingLevel.Allied => "{{G|Friendly}}",  // HARDCODED
    Brain.FeelingLevel.Hostile => "{{R|Hostile}}", // HARDCODED
    _ => "Neutral",  // HARDCODED
};
```

**Impact**: These require separate patches or string replacement.

---

### GAP-R05: Weight Label Hardcoding

**Actual source** ([Description.cs#L161](Assets/core_source/GameSource/XRL.World.Parts/Description.cs#L161)):
```csharp
stringBuilder.Append("\n{{K|Weight: ").Append(ParentObject.Weight).Append(" lbs.}}");
```

**Impact**: "Weight:" and "lbs." need translation.

---

### GAP-R06: NameMaker Procedural Generation

**Not mentioned in document**

**Referenced in Corpse.cs**:
```csharp
string text = NameMaker.MakeName(ParentObject, ...);
```

**NameMaker** generates procedural names like:
- "Ptoh"
- "Resheph"
- "Eschelstadt II"

**Impact**: Proper nouns from NameMaker should NOT be translated.

---

### GAP-R07: Material/Mod Application Order

**Document assumes simple prefix replacement**

**Actual flow**:
1. Base name from blueprint
2. `DescriptionBuilder.AddBase()` sets core name
3. Material mods add via `AddAdjective()` 
4. Quality mods add via `AddAdjective()` at different priorities
5. `DB.ToString()` composes final name with ordering

**Example**: "worn bronze short sword"
- Priority -10: "worn" (condition)
- Priority 0: "bronze" (material)
- Priority 0: "short sword" (base)

**Impact**: Translation must respect composition order.

---

### GAP-R08: Thread Safety Not Analyzed

**Document mentions caching but not thread safety**

**Questions to verify**:
1. Is rendering multi-threaded?
2. Can `GetFor()` be called from multiple threads?
3. Are static dictionaries safe?

**Based on Unity + Qud patterns**: Likely single-threaded for UI rendering, but async operations exist.

**Recommendation**: Use `ConcurrentDictionary` or lock patterns for safety.

---

## 3. UNREALISTIC: Assumptions That Won't Work

### UNR-R01: Phase Timing Estimates

**Document claims**:
| Phase | Time |
|-------|------|
| Phase 0: Foundation | 1.5 hours |
| Phase 1: Tutorial | 1 hour |
| Phase 2: Basic Equipment | 1.5 hours |

**Reality check**:
- Character Creation patch (similar complexity): **1,696 lines**
- StructureTranslator (simpler system): **550 lines**
- 81 mutation JSON files exist - each took time

**Realistic estimates**:
| Phase | Realistic Time |
|-------|----------------|
| Phase 0: Foundation | 4-6 hours |
| Phase 1: Tutorial | 2-3 hours |
| Phase 2: Basic Equipment | 4-6 hours |

**Reason**: GetDisplayNameEvent is MORE complex than CharacterCreation due to:
- Multiple event handlers
- Pooled events
- DescriptionBuilder complexity
- Unknown item system

---

### UNR-R02: Simple Tag Preservation

**Document proposes**:
```csharp
__result = ReplacePreservingTags(__result, Base, translated);
```

**Reality**: 
- Tags can be nested: `{{K|{{crysteel|crysteel}} mace}}`
- Tags can wrap partial text: `{{r|fire}}ball`
- Material tags use custom colors: `{{zetachrome|zetachrome}}`
- Some tags are dynamic: `{{w|bronze}}` vs `{{W|bronze}}`

**Existing TranslationEngine** has this logic but needs verification.

---

### UNR-R03: Cache Key Design

**Document proposes**:
```
CacheKey = "{BlueprintID}:{BaseDisplayName}"
```

**Problems**:
1. Same blueprint can have different names based on:
   - Epistemic status (known vs unknown)
   - Mods applied (rusty, masterwork)
   - Liquid contents
   - Context (inventory vs combat log)
2. `Base` parameter varies at call sites

**Better key**:
```
CacheKey = "{BlueprintID}:{EpistemicStatus}:{FinalDisplayName}"
```

---

### UNR-R04: Corpse Pattern Matching

**Document proposes**:
```csharp
if (originalName.EndsWith(" corpse"))
{
    string creaturePart = originalName.Replace(" corpse", "");
```

**Problems**:
1. Corpses are separate blueprint objects
2. Corpse name includes creature via `StringProperty["CreatureName"]`
3. Full name is composed by Render or Description parts
4. Pattern: "fresh bear corpse", "burnt snapjaw scavenger remains"

**Better approach**: 
1. Patch where corpse name is composed
2. Or detect by `HasPart("Corpse")` and translate creature portion

---

## 4. IMPROVEMENTS: Specific Suggestions

### IMP-R01: Add ForSort Guard

```csharp
[HarmonyPostfix]
static void GetFor_Postfix(ref string __result, GameObject Object, string Base,
    bool ForSort, bool ColorOnly, bool AsIfKnown)
{
    // Skip non-translatable calls
    if (ForSort || ColorOnly) return;
    
    // Skip if explicitly English context
    if (AsIfKnown && ShouldPreserveEnglish(Object)) return;
    
    // ... translation logic
}
```

---

### IMP-R02: Patch At Multiple Levels

**Recommended patch points**:
1. `GetDisplayNameEvent.GetFor()` - final output (postfix)
2. `DescriptionBuilder.AddAdjective()` - intercept mod prefixes
3. `Look.GenerateTooltipInformation()` - tooltip text
4. `Brain.GetFeelingLevel()` - Hostile/Friendly labels

---

### IMP-R03: Use Existing TranslationEngine

**Don't reinvent tag handling**:
```csharp
if (TranslationEngine.TryTranslate(baseText, out string translated))
{
    // Tags already preserved by TranslationEngine
    __result = TranslationEngine.RestoreTags(original, translated);
}
```

---

### IMP-R04: Handle Epistemic States

```csharp
// Check if item is identified
int epistemicStatus = Object.GetEpistemicStatus();
string lookupKey = epistemicStatus switch
{
    0 => Object.GetPart<Examiner>()?.Unknown ?? Object.Blueprint,
    1 => Object.GetPart<Examiner>()?.Alternate ?? Object.Blueprint,
    _ => Object.Blueprint
};
```

---

### IMP-R05: Add Validation Tests

**Create unit test JSON**:
```json
{
  "test_cases": [
    {"input": "dagger", "expected": "단검"},
    {"input": "{{w|bronze}} mace", "expected": "{{w|청동}} 메이스"},
    {"input": "rusty iron sword", "expected": "녹슨 철 검"},
    {"input": "odd trinket", "expected": "이상한 장신구"},
    {"input": "bear corpse", "expected": "곰 시체"}
  ]
}
```

---

## 5. VALIDATION: Things the Document Got Right

### VAL-R01: UI-Only Patch Approach ✓
Correctly identifies that data fields should NOT be modified.

### VAL-R02: Caching Strategy ✓
Caching is necessary - just needs better key design.

### VAL-R03: Color Tag Awareness ✓
Document correctly identifies color tags as a challenge.

### VAL-R04: JSON-Based Translation Data ✓
Separation of translation data from code is correct.

### VAL-R05: Folder Structure ✓
CREATURES/ and ITEMS/ organization is sensible.

### VAL-R06: Material/Prefix Tables ✓
`_common.json` for shared terms is a good pattern.

### VAL-R07: Phase-Based Implementation ✓
Incremental approach (Tutorial → Basic → Extended) is correct.

---

## 6. Recommended Action Items

### Immediate (Before Implementation)

| ID | Action | Priority |
|----|--------|----------|
| A1 | Fix GetFor() signature in document | Critical |
| A2 | Add epistemic status handling | Critical |
| A3 | Research DescriptionBuilder integration | High |
| A4 | Verify thread safety requirements | High |
| A5 | Update time estimates (3x current) | Medium |

### During Implementation

| ID | Action | Priority |
|----|--------|----------|
| A6 | Add ForSort/ColorOnly guards | Critical |
| A7 | Handle unknown item event path | Critical |
| A8 | Integrate with existing TranslationEngine | High |
| A9 | Add Look.GenerateTooltipInformation patch | High |
| A10 | Add hardcoded string patches for Description.cs | Medium |

### Post-Implementation

| ID | Action | Priority |
|----|--------|----------|
| A11 | Create test cases for edge cases | High |
| A12 | Profile cache hit rates | Medium |
| A13 | Document actual implementation vs plan | Medium |

---

## Appendix: Code References

| Document Section | Actual Source File | Line Numbers |
|-----------------|-------------------|--------------|
| 5.1 GetFor() | GetDisplayNameEvent.cs | 74-109 |
| 5.2 GetShortDescription() | Description.cs | 70-109 |
| 6.3 Corpses | Corpse.cs | 100-203 |
| 6.4 Unknown Items | Examiner.cs | 1-400 |
| - DescriptionBuilder | GetDisplayNameEvent.cs | 13-15, 117-190 |
| - ProcessFor | GetDisplayNameEvent.cs | 195-250 |
| - Tooltip | Description.cs | 305-340 |

---

**Review Conclusion**: The document provides a good conceptual foundation but requires significant revisions before implementation. The primary risks are incorrect patch targets and underestimated complexity.
