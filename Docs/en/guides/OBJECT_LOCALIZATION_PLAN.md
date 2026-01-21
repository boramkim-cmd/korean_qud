# Object Localization System Plan
# Creatures & Items Translation Architecture

**Version**: 2.0  
**Created**: 2026-01-22  
**Updated**: 2026-01-22  
**Status**: Ready for Implementation  

---

> ⚠️ **CRITICAL: ISOLATION STRATEGY**  
> This system is **completely isolated** from existing translation infrastructure.  
> **DO NOT modify**: TranslationEngine.cs, StructureTranslator.cs, LocalizationManager.cs  
> If something breaks, delete `Scripts/02_Patches/20_Objects/` + `LOCALIZATION/OBJECTS/` to restore.

---

## TL;DR (Executive Summary)

| Aspect | Detail |
|--------|--------|
| **Goal** | Translate creature/item DisplayNames to Korean |
| **Method** | Harmony Postfix on `GetDisplayNameEvent.GetFor()` |
| **Isolation** | 100% separate from existing CharacterCreation/Mutation systems |
| **Start Point** | Phase 0 (Foundation) → Phase 1 (Tutorial) |
| **Time Estimate** | Phase 0: 4-6h, Phase 1: 2-3h |
| **Key Risk** | `GetFor()` has 16 parameters - must guard `ForSort`/`ColorOnly` modes |

---

## Table of Contents

1. [Overview](#1-overview)
2. [Architecture](#2-architecture) - Hybrid Isolation Strategy
3. [File Structure](#3-folder-structure) - Isolated folders
4. [JSON Schema](#4-json-schema)
5. [Patch Implementation](#5-patch-implementation-details) - Correct signatures
6. [Edge Cases](#6-edge-case-handling)
7. [Performance](#7-performance-optimization)
8. [Phase Plan](#8-implementation-phases) - Realistic estimates
9. [Debugging Tools](#9-debugging-tools) - Wish commands ⭐ NEW
10. [Testing Checklist](#10-testing-checklist)
11. [Risk Assessment](#11-risk-assessment)
12. [Dependencies](#12-dependencies)
- [Appendix A-C](#appendix) - Reference data

> **Note**: Sections 13-22 (Effects, Combat, Grammar) have been moved to a separate document:  
> `EFFECT_COMBAT_LOCALIZATION_PLAN.md` (to be created after Phase 2 completion)

---

## 1. Overview

This document describes the implementation plan for creature and item localization in the Caves of Qud Korean mod. 

### Scope
**This document covers Objects (Creatures/Items) ONLY.**  
Effects, Combat Messages, and Grammar systems are documented separately.

### Architecture Principle
> **"Read-only reuse of existing infrastructure, complete isolation for caching/patching"**

The system uses Harmony Postfix patches on `GetDisplayNameEvent.GetFor()` to translate display names at the UI layer only, preserving game data integrity.

### Goals
- Translate all creature and item DisplayNames to Korean
- Translate descriptions shown in tooltips and examine screens
- Handle edge cases: color tags, dynamic prefixes, corpses, unknown items
- Maintain performance with aggressive caching
- **Zero interference with existing CharacterCreation/Mutation translations**

### Non-Goals (Out of Scope)
- Modifying game data directly
- Translating internal IDs or blueprint names
- Changing game mechanics
- Modifying existing Core files (TranslationEngine, StructureTranslator)

---

## 2. Architecture

### 2.1 Hybrid Isolation Strategy

**CRITICAL**: This system is completely isolated from existing translation infrastructure.

| Component | Reuse? | Reason |
|-----------|--------|--------|
| `LocalizationManager.GetModDirectory()` | ✅ Read-only | Path lookup only, no modification |
| `TranslationEngine` | ❌ No | Risk of breaking CharacterCreation |
| `StructureTranslator` | ❌ No | Cache collision with Mutations |
| `ScopeManager` | ❌ No | Not needed for Objects |

### 2.2 Safety Verification

| Existing System | Impact from Object System |
|-----------------|---------------------------|
| CharacterCreation patches | 🟢 **None** - completely separate |
| Mutation translations | 🟢 **None** - separate cache |
| UI translations | 🟢 **None** - separate JSON folder |
| Options/Inventory patches | 🟢 **None** - different events |

**Rollback**: If issues occur, delete these folders to restore:
- `Scripts/02_Patches/20_Objects/`
- `LOCALIZATION/OBJECTS/`

### 2.3 Data Flow

```
[Game Engine]
     │
     ▼
GetDisplayNameEvent.GetFor(Object, Base, ...14 more params)
     │
     ├─── ForSort=true? ──► SKIP (return unchanged)
     ├─── ColorOnly=true? ─► SKIP (return unchanged)
     │
     ▼
[Harmony Postfix Patch] ◄─── ObjectTranslator (ISOLATED)
     │                            │
     │                    ┌───────┴───────┐
     │                    ▼               ▼
     │              Cache Lookup    JSON Lookup
     │              (O(1) fast)    (if cache miss)
     │                    │               │
     │                    └───────┬───────┘
     ▼                            ▼
[Translated DisplayName] ◄── Translation Result
     │
     ▼
[UI Rendering]
```

### 2.4 Key Components (Isolated)

| Component | File | Responsibility |
|-----------|------|----------------|
| ObjectTranslator | `Scripts/02_Patches/20_Objects/02_20_00_ObjectTranslator.cs` | **Isolated** JSON loading, cache, lookup |
| DisplayNamePatch | `Scripts/02_Patches/20_Objects/02_20_01_DisplayNamePatch.cs` | Patch GetDisplayNameEvent.GetFor() |
| DescriptionPatch | `Scripts/02_Patches/20_Objects/02_20_02_DescriptionPatch.cs` | Patch Description.GetShortDescription() |
| DebugWishes | `Scripts/02_Patches/20_Objects/02_20_99_DebugWishes.cs` | kr:reload, kr:check commands |
| JSON Data | `LOCALIZATION/OBJECTS/creatures/`, `items/` | Translation data storage |

### 2.5 Class Diagram

```
┌─────────────────────────────────────────────────────┐
│           ObjectTranslator (ISOLATED)                │
├─────────────────────────────────────────────────────┤
│ - _creatureCache: Dictionary<string, ObjectData>     │  ◄─ Separate from StructureTranslator
│ - _itemCache: Dictionary<string, ObjectData>         │  ◄─ No collision possible
│ - _displayNameCache: Dictionary<string, string>      │
│ - _initialized: bool                                 │
├─────────────────────────────────────────────────────┤
│ + Initialize()                                       │
│ + TryGetDisplayName(blueprint, original, out result) │
│ + TryGetDescription(blueprint, out result)           │
│ + ReloadJson()  ◄─ For debugging (kr:reload)         │
│ + ClearCache()                                       │
│ - LoadJsonFiles()                                    │
│ - StripColorTags(text)  ◄─ Own copy, not shared      │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│                    ObjectData                        │
├─────────────────────────────────────────────────────┤
│ + BlueprintId: string                                │
│ + Names: Dictionary<string, string>                  │
│ + Description: string                                │
│ + DescriptionKo: string                              │
└─────────────────────────────────────────────────────┘
```

---

## 3. Folder Structure (Isolated)

### 3.1 New Folders (Completely Separate)

```
LOCALIZATION/
├── OBJECTS/                              # 🆕 NEW - Isolated from GAMEPLAY/
│   ├── creatures/
│   │   ├── _common.json                  # Common terms (corpse, species)
│   │   ├── tutorial.json                 # Phase 1
│   │   ├── tier1_humanoids.json          # Phase 3 (Snapjaws)
│   │   ├── tier1_animals.json            # Phase 3
│   │   └── npcs_joppa.json               # Phase 3
│   │
│   └── items/
│       ├── _common.json                  # Materials, prefixes, modifiers
│       ├── tutorial.json                 # Phase 1
│       ├── melee_weapons.json            # Phase 2
│       ├── armor.json                    # Phase 2
│       └── tools.json                    # Phase 2
│
├── GAMEPLAY/                             # ⚠️ EXISTING - DO NOT MODIFY
│   ├── MUTATIONS/                        # Used by StructureTranslator
│   ├── CYBERNETICS/
│   └── TUTORIAL/
│
├── CHARGEN/                              # ⚠️ EXISTING - DO NOT MODIFY
└── UI/                                   # ⚠️ EXISTING - DO NOT MODIFY
```

### 3.2 Source Code Structure (Isolated)

```
Scripts/
├── 00_Core/                              # ⚠️ EXISTING - DO NOT MODIFY
│   ├── 00_00_00_ModEntry.cs              # Auto-registers new patches ✅
│   ├── 00_00_01_TranslationEngine.cs     # DO NOT MODIFY
│   └── 00_00_03_LocalizationManager.cs   # Only use GetModDirectory() ✅
│
├── 99_Utils/
│   └── 99_00_03_StructureTranslator.cs   # ⚠️ DO NOT MODIFY
│
└── 02_Patches/
    ├── 10_UI/                            # ⚠️ EXISTING - DO NOT MODIFY
    │
    └── 20_Objects/                       # 🆕 NEW - Isolated folder
        ├── 02_20_00_ObjectTranslator.cs  # Isolated cache + JSON loading
        ├── 02_20_01_DisplayNamePatch.cs  # GetDisplayNameEvent patch
        ├── 02_20_02_DescriptionPatch.cs  # Description patch
        └── 02_20_99_DebugWishes.cs       # kr:reload, kr:check commands
```

---

## 4. JSON Schema

### 4.1 Creature Schema

**File**: `CREATURES/tier1_humanoids.json`

```json
{
  "SnapjawScavenger": {
    "names": {
      "snapjaw scavenger": "스냅조 청소부"
    },
    "description": "Armed with scraps, a muscular and yellow-scaled humanoid takes a fighting stance.",
    "description_ko": "파편으로 무장한 노란 비늘의 근육질 휴머노이드가 전투 자세를 취하고 있다."
  },
  "SnapjawHunter": {
    "names": {
      "snapjaw hunter": "스냅조 사냥꾼"
    },
    "description": "...",
    "description_ko": "..."
  }
}
```

### 4.2 Item Schema

**File**: `ITEMS/melee_weapons.json`

```json
{
  "Dagger": {
    "names": {
      "dagger": "단검"
    },
    "description": "A short, pointed blade ideal for close combat.",
    "description_ko": "근접전에 적합한 짧고 뾰족한 칼날이다."
  },
  "Mace2": {
    "names": {
      "{{w|bronze}} mace": "{{w|청동}} 메이스"
    },
    "description": "A corm of bell bronze is screw-fitted to a nicked wooden haft.",
    "description_ko": "벨 청동 덩어리가 흠집 난 나무 자루에 나사로 고정되어 있다."
  }
}
```

### 4.3 Common Data Schema

**File**: `ITEMS/_common.json`

```json
{
  "materials": {
    "bronze": "청동",
    "iron": "철",
    "steel": "강철",
    "carbide": "카바이드",
    "fullerite": "풀러라이트",
    "crysteel": "크리스틸",
    "zetachrome": "제타크롬",
    "flawless crysteel": "완벽한 크리스틸"
  },
  "prefixes": {
    "rusty": "녹슨",
    "masterwork": "명품",
    "worn": "낡은",
    "flawless": "완벽한",
    "serrated": "톱니 달린",
    "electrified": "전기가 흐르는"
  },
  "suffixes": {
    "of fire": "화염의",
    "of ice": "냉기의"
  }
}
```

**File**: `CREATURES/_common.json`

```json
{
  "common_terms": {
    "corpse": "시체",
    "remains": "잔해",
    "hostile": "적대적"
  },
  "species": {
    "snapjaw": "스냅조",
    "goatfolk": "염소인",
    "bear": "곰"
  }
}
```

---

## 5. Patch Implementation Details

### 5.1 GetDisplayNameEvent.GetFor() Patch

**Target**: `XRL.World.GetDisplayNameEvent.GetFor()`

```csharp
[HarmonyPatch(typeof(GetDisplayNameEvent))]
public static class Patch_ObjectDisplayName
{
    private static readonly Dictionary<string, string> _cache = new();
    
    [HarmonyPatch(nameof(GetDisplayNameEvent.GetFor))]
    [HarmonyPostfix]
    static void GetFor_Postfix(ref string __result, GameObject Object, string Base)
    {
        if (Object == null || string.IsNullOrEmpty(__result)) return;
        
        string blueprint = Object.Blueprint;
        string cacheKey = $"{blueprint}:{Base}";
        
        // Fast path: cache hit
        if (_cache.TryGetValue(cacheKey, out string cached))
        {
            __result = cached;
            return;
        }
        
        // Slow path: lookup and cache
        if (ObjectTranslator.TryGetDisplayName(blueprint, Base, out string translated))
        {
            // Preserve color tag structure, replace inner text only
            __result = ReplacePreservingTags(__result, Base, translated);
            _cache[cacheKey] = __result;
        }
    }
    
    public static void ClearCache() => _cache.Clear();
}
```

### 5.2 Description.GetShortDescription() Patch

**Target**: `XRL.World.Parts.Description.GetShortDescription()`

```csharp
[HarmonyPatch(typeof(Description))]
public static class Patch_ObjectDescription
{
    [HarmonyPatch(nameof(Description.GetShortDescription))]
    [HarmonyPostfix]
    static void GetShortDescription_Postfix(ref string __result, Description __instance)
    {
        if (__instance?.ParentObject == null) return;
        
        string blueprint = __instance.ParentObject.Blueprint;
        
        if (ObjectTranslator.TryGetDescription(blueprint, out string translated))
        {
            __result = translated;
        }
    }
}
```

### 5.3 Cache Invalidation

**Event Hook**: Scene transition or game load

```csharp
// In ModEntry.cs or dedicated event handler
[HarmonyPatch(typeof(XRLCore))]
public static class Patch_CacheInvalidation
{
    [HarmonyPatch("LoadGame")]
    [HarmonyPostfix]
    static void LoadGame_Postfix()
    {
        Patch_ObjectDisplayName.ClearCache();
    }
}
```

---

## 6. Edge Case Handling

### 6.1 Color Tags

**Problem**: DisplayNames contain color tags like `{{w|bronze}} mace`

**Solution**: Parse tags, translate inner text, reconstruct

| Original | Process | Result |
|----------|---------|--------|
| `dagger` | Direct translate | `단검` |
| `{{c|dagger}}` | Preserve tag, translate inner | `{{c|단검}}` |
| `{{w|bronze}} mace` | Translate each part | `{{w|청동}} 메이스` |
| `{{K|{{crysteel|crysteel}} mace}}` | Recursive processing | `{{K|{{crysteel|크리스틸}} 메이스}}` |

**Implementation**:
```csharp
static string ReplacePreservingTags(string original, string baseText, string translated)
{
    // Use regex to find and replace text outside/inside tags
    // Reuse TranslationEngine pattern if applicable
}
```

### 6.2 Dynamic Prefixes (Mods)

**Problem**: Items can have runtime-generated prefixes like "rusty", "masterwork"

**Solution**: Use `_common.json` prefix table + composition

```
"rusty iron mace" decomposition:
  1. prefix: "rusty" → "녹슨"
  2. material: "iron" → "철"  
  3. base: "mace" → "메이스"
  Result: "녹슨 철 메이스"
```

**Implementation Strategy**:
1. First attempt: Exact match lookup in JSON
2. Fallback: Decompose name, translate parts, recompose

### 6.3 Corpses

**Problem**: Corpse names are dynamically generated: `"{creature} corpse"`

**Solution**: Pattern-based translation

```csharp
if (originalName.EndsWith(" corpse"))
{
    string creaturePart = originalName.Replace(" corpse", "");
    if (TryGetCreatureName(creaturePart, out string translatedCreature))
    {
        return $"{translatedCreature} 시체";
    }
}
```

**Common Terms** (`_common.json`):
```json
{
  "common_terms": {
    "corpse": "시체",
    "remains": "잔해",
    "husk": "껍데기"
  }
}
```

### 6.4 Unknown Items (Examiner Part)

**Problem**: Unidentified items show alternate names like "odd trinket"

**XML Example**:
```xml
<part Name="Examiner" Complexity="2" Alternate="UnknownPistol" />
```

**Solution**: Translate `Unknown*` blueprints in separate file

**File**: `ITEMS/unknown_items.json`
```json
{
  "UnknownPistol": {
    "names": { "odd pistol": "이상한 권총" }
  },
  "UnknownRifle": {
    "names": { "odd rifle": "이상한 소총" }
  },
  "UnknownTrinket": {
    "names": { "odd trinket": "이상한 장신구" }
  }
}
```

**Priority**: Phase 4 (after core items are translated)

### 6.5 Proper Nouns

**Detection**: Check `HasProperName` property or `<tag Name="ProperNoun" />`

**Handling**: 
- Proper nouns (NPC names): Transliterate or keep original
- Common nouns (species, items): Full translation

```csharp
if (Object.HasProperName)
{
    // Use transliteration table or keep original
    return TryGetProperNounTranslation(blueprint, out translated);
}
```

---

## 7. Performance Optimization

### 7.1 Caching Strategy

| Level | Storage | Lookup Time | Purpose |
|-------|---------|-------------|---------|
| L1 | `Dictionary<string, string>` | O(1) | Hot path cache |
| L2 | JSON parsed data | O(1) | Cold lookup |
| L3 | Disk JSON files | N/A | Initial load only |

### 7.2 Cache Key Design

```
CacheKey = "{BlueprintID}:{BaseDisplayName}"

Examples:
- "SnapjawScavenger:snapjaw scavenger"
- "Mace2:{{w|bronze}} mace"
```

### 7.3 Memory Management

**Cache Size Limit**: Consider LRU eviction if cache grows too large (unlikely for game objects)

**Clear Conditions**:
1. Game load/save
2. Scene transition
3. Mod reload (if supported)

### 7.4 Lazy Loading

```csharp
public static class ObjectTranslator
{
    private static bool _initialized = false;
    
    public static void EnsureInitialized()
    {
        if (_initialized) return;
        LoadJsonFiles();
        _initialized = true;
    }
    
    public static bool TryGetDisplayName(...)
    {
        EnsureInitialized();
        // ... lookup logic
    }
}
```

---

## 8. Implementation Phases

### Phase 0: Foundation (Estimated: 1.5 hours)

| ID | Task | File | Time |
|----|------|------|------|
| OBJ-001 | Create folder structure | `LOCALIZATION/GAMEPLAY/CREATURES/`, `ITEMS/` | 5 min |
| OBJ-002 | Create ObjectTranslator.cs | `Scripts/99_Utils/99_00_04_ObjectTranslator.cs` | 30 min |
| OBJ-003 | Create DisplayNamePatch.cs | `Scripts/02_Patches/20_Objects/02_20_01_ObjectDisplayNamePatch.cs` | 30 min |
| OBJ-004 | Create DescriptionPatch.cs | `Scripts/02_Patches/20_Objects/02_20_02_ObjectDescriptionPatch.cs` | 15 min |
| OBJ-005 | Update ModEntry registration | `Scripts/00_Core/00_00_00_ModEntry.cs` | 10 min |

### Phase 1: Tutorial (Estimated: 1 hour) 🔴 PRIORITY

| ID | Task | Items | Time |
|----|------|-------|------|
| OBJ-006 | Create CREATURES/tutorial.json | 3 creatures | 20 min |
| OBJ-007 | Create ITEMS/tutorial.json | 9 items | 20 min |
| OBJ-008 | Test tutorial flow | - | 20 min |

**Tutorial Creatures**:
- TutorialSnapjaw (→ Snapjaw Scavenger)
- TutorialBear (→ Bear)
- TutorialClockworkBeetlePariah

**Tutorial Items**:
- TutorialDagger, TutorialTorch, TutorialLeatherArmor
- TutorialChemCell, TutorialBattleAxe, TutorialAphorisms
- TutorialMarkovBook, TutorialHalfFullWaterskin, TutorialChest1

### Phase 2: Basic Equipment (Estimated: 1.5 hours) 🟠

| ID | Task | Items | Time |
|----|------|-------|------|
| OBJ-009 | Create ITEMS/_common.json | Materials, prefixes | 15 min |
| OBJ-010 | Create ITEMS/melee_weapons.json | ~15 items | 30 min |
| OBJ-011 | Create ITEMS/armor_body.json | ~10 items | 20 min |
| OBJ-012 | Create ITEMS/tools.json | ~5 items | 10 min |
| OBJ-013 | Test inventory/equipment | - | 15 min |

### Phase 3: Joppa Area (Estimated: 2 hours) 🟡

| ID | Task | Items | Time |
|----|------|-------|------|
| OBJ-014 | Create CREATURES/_common.json | Common terms | 10 min |
| OBJ-015 | Create CREATURES/tier1_humanoids.json | Snapjaws (4) | 30 min |
| OBJ-016 | Create CREATURES/tier1_animals.json | ~15 animals | 40 min |
| OBJ-017 | Create CREATURES/npcs_joppa.json | ~10 NPCs | 30 min |
| OBJ-018 | Test Joppa exploration | - | 20 min |

### Phase 4: Extended Content (Future)

| ID | Task | Scope |
|----|------|-------|
| OBJ-019 | Unknown items | `ITEMS/unknown_items.json` |
| OBJ-020 | Tier 2+ creatures | All remaining tiers |
| OBJ-021 | Missile weapons | Bows, guns, etc. |
| OBJ-022 | Books & artifacts | Special items |
| OBJ-023 | Merchants | Trading NPCs |

---

## 9. Testing Checklist

### 9.1 Display Name Tests

- [ ] Inventory screen shows Korean names
- [ ] Look popup shows Korean names
- [ ] Combat log shows Korean names
- [ ] Equipment screen shows Korean names
- [ ] Trade screen shows Korean names

### 9.2 Description Tests

- [ ] Tooltip shows Korean description
- [ ] Examine (look) shows Korean description
- [ ] Journal entries show Korean names

### 9.3 Edge Case Tests

- [ ] Color-tagged items display correctly
- [ ] Rusty/masterwork prefixes translate
- [ ] Corpse names translate
- [ ] Unknown items show alternate translation
- [ ] Stacked items display correctly

### 9.4 Performance Tests

- [ ] No noticeable lag in inventory
- [ ] Combat log updates smoothly
- [ ] Scene transitions don't cause issues

---

## 10. Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Performance degradation | High | Aggressive caching, lazy loading |
| Color tag corruption | Medium | Preserve tag structure, unit tests |
| Missing translations | Low | Fallback to original English |
| Cache memory bloat | Low | Clear on scene transition |
| Mod conflicts | Medium | Use unique namespace, late patching |

---

## 11. Dependencies

### Required Files (Read)
- `Assets/core_source/XRL.World/GetDisplayNameEvent.cs`
- `Assets/core_source/XRL.World.Parts/Render.cs`
- `Assets/core_source/XRL.World.Parts/Description.cs`
- `Assets/StreamingAssets/Base/ObjectBlueprints/Creatures.xml`
- `Assets/StreamingAssets/Base/ObjectBlueprints/Items.xml`

### Existing Code (Reuse)
- `Scripts/00_Core/00_00_01_TranslationEngine.cs` - Tag preservation
- `Scripts/00_Core/00_00_03_LocalizationManager.cs` - JSON loading
- `Scripts/99_Utils/99_00_03_StructureTranslator.cs` - Pattern reference

---

## 12. Appendix

### A. Joppa Tier 1 Creature List

| Blueprint | DisplayName | Korean |
|-----------|-------------|--------|
| SnapjawScavenger | snapjaw scavenger | 스냅조 청소부 |
| SnapjawHunter | snapjaw hunter | 스냅조 사냥꾼 |
| SnapjawWarrior | snapjaw warrior | 스냅조 전사 |
| SnapjawShotgunner | snapjaw shotgunner | 스냅조 산탄총수 |
| Bear | bear | 곰 |
| Boar | boar | 멧돼지 |
| Pig | pig | 돼지 |
| Salthopper | salthopper | 솔트호퍼 |
| Saltback | saltback | 솔트백 |
| Girshling | girshling | 거슐링 |

### B. Basic Equipment List

| Blueprint | DisplayName | Korean |
|-----------|-------------|--------|
| Dagger | dagger | 단검 |
| Club | club | 곤봉 |
| BattleAxe2 | battle axe | 전투 도끼 |
| Mace2 | {{w|bronze}} mace | {{w|청동}} 메이스 |
| Torch | torch | 횃불 |
| LeatherArmor | leather armor | 가죽 갑옷 |
| WovenTunic | woven tunic | 직조 튜닉 |
| Sandals | sandals | 샌들 |

### C. Joppa NPC List

| Blueprint | DisplayName | Korean |
|-----------|-------------|--------|
| Mehmet | Mehmet | 메흐멧 |
| Argyve | Argyve | 아르기브 |
| ElderIrudad | Elder Irudad | 장로 이루다드 |
| WatervineFarmerJoppa | watervine farmer | 워터바인 농부 |

---

## 13. Status Effects System

### 13.1 Effect Classes Location

**Source**: `Assets/core_source/GameSource/XRL.World.Effects/`

### 13.2 Status Effect Categories

#### Combat Effects
| File | DisplayName | Korean | Priority |
|------|-------------|--------|----------|
| Confused.cs | `confused` | 혼란 | Phase 1 |
| Stunned.cs | `stunned` | 기절 | Phase 1 |
| Dazed.cs | `dazed` | 혼미 | Phase 2 |
| Paralyzed.cs | `paralyzed` | 마비 | Phase 2 |
| Prone.cs | `prone` | 넘어짐 | Phase 2 |
| Blind.cs | `blind` | 실명 | Phase 2 |
| Terrified.cs | `terrified` | 공포 | Phase 2 |
| Berserk.cs | `berserk` | 광폭화 | Phase 3 |

#### Damage Over Time
| File | DisplayName | Korean | Priority |
|------|-------------|--------|----------|
| Bleeding.cs | `bleeding` | 출혈 | Phase 1 |
| Burning.cs | `burning` | 화상 | Phase 1 |
| Poisoned.cs | `poisoned` | 중독 | Phase 1 |
| Frozen.cs | `{{freezing|frozen}}` | 동결 | Phase 1 |

#### Temperature Effects
| File | DisplayName | Korean |
|------|-------------|--------|
| Frozen.cs | `{{freezing|frozen}}` | 동결 |
| Shivering.cs | `shivering` | 떨림 |
| Warm.cs | `warm` | 따뜻함 |
| Overheated.cs | `overheated` | 과열 |
| OnFire.cs | `on fire` | 불붙음 |

#### Mental Effects
| File | DisplayName | Korean |
|------|-------------|--------|
| Confused.cs | `confused` | 혼란 |
| Dominated.cs | `dominated` | 지배됨 |
| Asleep.cs | `asleep` | 수면 |
| Meditating.cs | `meditating` | 명상 |
| Trance.cs | `trance` | 황홀경 |
| Shaken.cs | `shaken` | 동요 |
| Psionically_Cleaved.cs | `{{psionic|psionically cleaved}}` | 초능력 분열 |

#### Diseases
| File | DisplayName | Korean |
|------|-------------|--------|
| Glotrot.cs | `glotrot` | 글로트롯 |
| Ironshank.cs | `ironshank` | 아이언생크 |
| Monochrome.cs | `monochrome` | 모노크롬 |
| Stiff_Legs.cs | `stiff legs` | 경직된 다리 |
| Blurry_Vision.cs | `blurry vision` | 흐릿한 시야 |

#### Tonic Effects
| File | DisplayName | Korean |
|------|-------------|--------|
| LoveTonic.cs | `{{amorous|love}} tonic` | 사랑의 토닉 |
| Tonicked.cs | various | 토닉 효과 |

#### Special Effects
| File | DisplayName | Korean |
|------|-------------|--------|
| Flying.cs | `flying` | 비행 |
| Sprinting.cs | `sprinting` | 질주 |
| Phased.cs | `phased` | 위상 변환 |
| Invisible.cs | `invisible` | 투명 |
| Glitching.cs | `{{entropic|glitching}}` | 글리칭 |

### 13.3 Effect GetDetails() Patterns

Each effect has a `GetDetails()` method returning description:

```csharp
// Confused.cs
"Acts semi-randomly.\n-" + Level + " DV\n-" + Level + " MA"

// Bleeding.cs  
Damage + " damage per turn."

// Stunned.cs
"Can't take actions.\nDV set to 0."
```

**Translation Strategy**: Patch `Effect.GetDetails()` or create effect-specific patches.

---

## 14. Damage Types & Attributes

### 14.1 Source Location

**File**: `Assets/core_source/GameSource/XRL.World/Damage.cs`

### 14.2 Damage Type List

| English | Korean | Color Code |
|---------|--------|------------|
| Fire / Heat | 화염 | `{{r|...}}` |
| Cold / Ice / Freeze | 냉기 | `{{freezing|...}}` |
| Electric / Shock / Lightning | 전기 | `{{W|...}}` |
| Acid | 산성 | `{{g|...}}` |
| Light / Laser | 빛 | `{{Y|...}}` |
| Poison | 독 | `{{G|...}}` |
| Bleeding | 출혈 | `{{r|...}}` |
| Mental / Psionic | 정신 | `{{psionic|...}}` |
| Explosion / Explosive | 폭발 | `{{O|...}}` |
| Asphyxiation | 질식 | `{{B|...}}` |
| Cudgel / Bludgeoning | 둔기 | `{{w|...}}` |
| Disintegrate | 분해 | `{{K|...}}` |

### 14.3 Stat Attributes

| Attribute | English | Korean |
|-----------|---------|--------|
| AV | Armor Value | 방어력 |
| DV | Dodge Value | 회피력 |
| MA | Mental Armor | 정신 방어력 |
| HP | Hit Points | 체력 |
| QN | Quickness | 민첩성 |
| MS | Move Speed | 이동 속도 |

### 14.4 Resistance Types

| English | Korean |
|---------|--------|
| Heat Resistance | 열 저항 |
| Cold Resistance | 냉기 저항 |
| Electric Resistance | 전기 저항 |
| Acid Resistance | 산성 저항 |

---

## 15. Color Tag System

### 15.1 Basic Color Codes

| Code | Color | Usage |
|------|-------|-------|
| `k/K` | Black | Shadows, dark |
| `b/B` | Blue | Water, cold |
| `g/G` | Green | Poison, nature |
| `c/C` | Cyan | Tech, artifacts |
| `r/R` | Red | Fire, damage |
| `m/M` | Magenta | Psionic |
| `w/W` | White/Gray | Common items |
| `y/Y` | Yellow/Brown | Earth, gold |
| `o/O` | Orange | Explosions |

### 15.2 Special Color Aliases

| Alias | Purpose | Example |
|-------|---------|---------|
| `{{rules|...}}` | Stats/mechanics | `{{rules|+5}}` |
| `{{freezing|...}}` | Cold effects | `{{freezing|frozen}}` |
| `{{psionic|...}}` | Mental/psi | `{{psionic|dominated}}` |
| `{{amorous|...}}` | Love effects | `{{amorous|charmed}}` |
| `{{entropic|...}}` | Entropy | `{{entropic|glitching}}` |
| `{{hint|...}}` | Tutorial hints | `{{hint|Press Space}}` |
| `{{crysteel|...}}` | Crysteel material | `{{crysteel|crysteel}}` |
| `{{zetachrome|...}}` | Zetachrome | alternating colors |

### 15.3 Tag Format Patterns

```
Long format:   {{color|text}}
Short fore:    &color
Short back:    ^color
Nested:        {{K|{{crysteel|crysteel}} mace}}
```

### 15.4 Translation Rule

**CRITICAL**: Translate text INSIDE tags, preserve tag structure:
```
WRONG: "{{r|fire}}" → "화염"
RIGHT: "{{r|fire}}" → "{{r|화염}}"
```

---

## 16. Hardcoded Text Fragmentation

### 16.1 Combat Message System

**File**: `Assets/core_source/GameSource/XRL.World/GameObject.cs`

```csharp
// Pattern 1: DidX - single action
DidX("become", "confused", "!");
DidX("are", "stunned", "!");
DidX("die", null, "!");

// Pattern 2: DidXToY - action with target
DidXToY("hit", gameObject, ...);
DidXToY("charge", combatTarget, ...);
```

**Problem**: These are scattered across 100+ files.

### 16.2 ParticleText Messages

**Location**: Various combat/effect files

```csharp
ParticleText("*stunned*", ...);
ParticleText("*miss*", ...);
ParticleText("-5", ...);  // damage numbers
```

### 16.3 Effect Detail Strings

**Location**: Each `XRL.World.Effects/*.cs` file

| Class | Hardcoded String |
|-------|------------------|
| Confused | `"Acts semi-randomly.\n-X DV\n-X MA"` |
| Bleeding | `"X damage per turn."` |
| Burning | `"X damage per turn."` |
| Stunned | `"Can't take actions.\nDV set to 0."` |

### 16.4 UI Screen Hardcoding

| Screen | File | Hardcoded Examples |
|--------|------|-------------------|
| Inventory | `InventoryScreen.cs` | "Weight", "Value" |
| Equipment | `EquipmentScreen.cs` | "Equipped", slot names |
| Look | `LookUI.cs` | "You see..." |
| Popup | `Popup.cs` | Button labels |

### 16.5 Patch Strategy for Fragmented Text

```
Approach 1: Patch each source file (accurate but tedious)
Approach 2: Patch display layer only (easier but may miss some)
Approach 3: Hybrid - patch common patterns + specific overrides

RECOMMENDED: Approach 3
- Patch DidX/DidXToY at GameObject level
- Patch ParticleText at rendering level  
- Patch Effect.GetDetails() at base class
- Add specific patches for important UI screens
```

---

## 17. Item Modifier System

### 17.1 Source Location

**Path**: `Assets/core_source/GameSource/XRL.World.Parts/Mod*.cs`

### 17.2 Quality Modifiers

| File | English | Korean | Effect |
|------|---------|--------|--------|
| ModMasterwork.cs | `masterwork` | 명품 | Crit chance |
| ModLegendary.cs | `Legendary` | 전설 | Multiple bonuses |
| ModReinforced.cs | `reinforced` | 강화됨 | Durability |
| ModSharp.cs | `sharp` | 날카로운 | PV bonus |

### 17.3 Elemental Modifiers

| File | English | Korean | Damage Type |
|------|---------|--------|-------------|
| ModFlaming.cs | `flaming` | 불타는 | Fire |
| ModFreezing.cs | `{{freezing|freezing}}` | 얼어붙는 | Cold |
| ModElectrified.cs | `electrified` | 전기화된 | Electric |
| ModSerrated.cs | `serrated` | 톱니 달린 | Bleeding |
| ModPsionic.cs | `{{psionic|psionic}}` | 초능력의 | Mental |

### 17.4 Condition Modifiers

| English | Korean |
|---------|--------|
| rusty | 녹슨 |
| worn | 낡은 |
| cracked | 금간 |
| broken | 부서진 |
| pristine | 완전한 |

---

## 18. Grammar System (Korean Adaptation)

### 18.1 Source Location

**File**: `Assets/core_source/GameSource/Grammar.cs` (2883 lines)

### 18.2 English Grammar Functions

```csharp
Grammar.Pluralize(word)      // attack → attacks
Grammar.ThirdPerson(verb)    // attack → attacks  
Grammar.PastTenseOf(verb)    // attack → attacked
Grammar.Cardinal(num)        // 1 → "one"
Grammar.Ordinal(num)         // 1 → "first"
Grammar.A(word)              // apple → "an apple"
Grammar.MakePossessive(w)    // you → "your"
```

### 18.3 Korean Grammar Challenges

**Problem 1: Postpositions (조사)**
Korean particles change based on final consonant:
- 은/는 (topic): 고블린**은** vs 곰**은**
- 이/가 (subject): 단검**이** vs 메이스**가**
- 을/를 (object): 곰**을** vs 단검**를** (X) → **을**

**Problem 2: Word Order**
- English: `Subject + Verb + Object` ("You hit the goblin")
- Korean: `Subject + Object + Verb` ("당신이 고블린을 공격합니다")

**Problem 3: Verb Conjugation**
Korean verbs conjugate differently:
- 공격하다 → 공격합니다/공격했습니다/공격해요

### 18.4 Recommended Solution

```json
// GRAMMAR/korean_postpositions.json
{
  "rules": {
    "은/는": "Check final consonant → 은 if consonant, 는 if vowel",
    "이/가": "Check final consonant → 이 if consonant, 가 if vowel",
    "을/를": "Check final consonant → 을 if consonant, 를 if vowel"
  },
  "examples": {
    "곰": { "topic": "곰은", "subject": "곰이", "object": "곰을" },
    "단검": { "topic": "단검은", "subject": "단검이", "object": "단검을" }
  }
}
```

**Implementation**: Create `KoreanGrammar` utility class with postposition logic.

---

## 19. Expanded Folder Structure

### 19.1 Complete Localization Structure

```
LOCALIZATION/
├── GAMEPLAY/
│   ├── CREATURES/              # 크리쳐 (기존)
│   │   ├── _common.json
│   │   ├── tutorial.json
│   │   ├── tier1_animals.json
│   │   ├── tier1_humanoids.json
│   │   └── npcs_joppa.json
│   │
│   ├── ITEMS/                  # 아이템 (기존)
│   │   ├── _common.json        # materials, prefixes
│   │   ├── tutorial.json
│   │   ├── melee_weapons.json
│   │   ├── armor_body.json
│   │   ├── tools.json
│   │   └── unknown_items.json
│   │
│   ├── EFFECTS/                # 상태 효과 (신규)
│   │   ├── _index.json         # Effect class → DisplayName mapping
│   │   ├── combat_effects.json # stunned, confused, prone...
│   │   ├── damage_effects.json # bleeding, burning, poisoned...
│   │   ├── mental_effects.json # dominated, asleep, trance...
│   │   ├── temperature.json    # frozen, warm, overheated...
│   │   ├── diseases.json       # glotrot, ironshank...
│   │   └── effect_details.json # GetDetails() descriptions
│   │
│   ├── COMBAT/                 # 전투 메시지 (신규)
│   │   ├── verbs.json          # hit, miss, kill, dodge...
│   │   ├── messages.json       # "X attacks Y", "X misses"...
│   │   ├── damage_types.json   # fire, cold, electric...
│   │   └── particle_text.json  # *stunned*, *miss*...
│   │
│   ├── ATTRIBUTES/             # 속성/스탯 (신규)
│   │   ├── stats.json          # AV, DV, MA, HP...
│   │   ├── resistances.json    # heat res, cold res...
│   │   └── skills.json         # (기존에서 이동)
│   │
│   ├── MODIFIERS/              # 아이템 수식어 (신규)
│   │   ├── quality.json        # masterwork, legendary...
│   │   ├── elemental.json      # flaming, freezing...
│   │   ├── condition.json      # rusty, worn, broken...
│   │   └── materials.json      # (ITEMS에서 분리)
│   │
│   ├── LIQUIDS/                # 액체 (신규)
│   │   └── liquid_names.json   # water, acid, blood...
│   │
│   └── TUTORIAL/               # 튜토리얼 (기존)
│
├── GRAMMAR/                    # 문법 시스템 (신규)
│   ├── korean_postpositions.json  # 조사 규칙
│   ├── verb_conjugation.json   # 동사 활용
│   └── number_words.json       # 수사 번역
│
└── UI/                         # UI (기존)
```

### 19.2 New Files Summary

| Folder | File | Content | Priority |
|--------|------|---------|----------|
| EFFECTS/ | combat_effects.json | 15+ effects | Phase 2 |
| EFFECTS/ | damage_effects.json | 5+ effects | Phase 1 |
| EFFECTS/ | effect_details.json | GetDetails() text | Phase 3 |
| COMBAT/ | verbs.json | 30+ verbs | Phase 2 |
| COMBAT/ | messages.json | Combat templates | Phase 3 |
| COMBAT/ | damage_types.json | 12+ types | Phase 2 |
| ATTRIBUTES/ | stats.json | 10+ stats | Phase 2 |
| MODIFIERS/ | quality.json | 10+ modifiers | Phase 2 |
| MODIFIERS/ | elemental.json | 8+ modifiers | Phase 2 |
| GRAMMAR/ | korean_postpositions.json | Grammar rules | Phase 1 |

---

## 20. Additional Patch Classes Needed

### 20.1 Effect System Patches

| Target | Method | Purpose |
|--------|--------|---------|
| `Effect` (base) | `GetDisplayName()` | Status effect names |
| `Effect` (base) | `GetDetails()` | Effect descriptions |
| `Effect` (base) | `GetDescription()` | Long descriptions |

### 20.2 Combat Message Patches

| Target | Method | Purpose |
|--------|--------|---------|
| `GameObject` | `DidX()` | Single action messages |
| `GameObject` | `DidXToY()` | Target action messages |
| `ParticleText` | (renderer) | Floating text |

### 20.3 Grammar Patches

| Target | Method | Purpose |
|--------|--------|---------|
| `Grammar` | `Pluralize()` | Skip for Korean |
| `Grammar` | `ThirdPerson()` | Skip for Korean |
| `Grammar` | `A()` | Remove articles |

### 20.4 Recommended Patch File Structure

```
Scripts/02_Patches/
├── 20_Objects/
│   ├── 02_20_01_ObjectDisplayNamePatch.cs
│   └── 02_20_02_ObjectDescriptionPatch.cs
│
├── 21_Effects/                              # 신규
│   ├── 02_21_01_EffectDisplayNamePatch.cs
│   └── 02_21_02_EffectDetailsPatch.cs
│
├── 22_Combat/                               # 신규
│   ├── 02_22_01_CombatMessagePatch.cs
│   └── 02_22_02_ParticleTextPatch.cs
│
└── 23_Grammar/                              # 신규
    └── 02_23_01_KoreanGrammarPatch.cs
```

---

## 21. Updated Phase Plan

### Phase 0: Foundation (2 hours)
- OBJ-001 ~ OBJ-005: Base structure + translators

### Phase 1: Tutorial + Core Effects (2 hours) 🔴
- OBJ-006 ~ OBJ-008: Tutorial creatures/items
- EFF-001: EFFECTS/damage_effects.json (bleeding, burning, poisoned, frozen)
- GRM-001: GRAMMAR/korean_postpositions.json (basic rules)

### Phase 2: Basic Equipment + Attributes (2.5 hours) 🟠
- OBJ-009 ~ OBJ-013: Basic weapons/armor
- EFF-002: EFFECTS/combat_effects.json (stunned, confused, etc.)
- ATR-001: ATTRIBUTES/stats.json
- CMB-001: COMBAT/damage_types.json
- MOD-001: MODIFIERS/quality.json, elemental.json

### Phase 3: Joppa + Combat Messages (3 hours) 🟡
- OBJ-014 ~ OBJ-018: Joppa creatures/NPCs
- EFF-003: EFFECTS/effect_details.json
- CMB-002: COMBAT/verbs.json, messages.json
- Patch: CombatMessagePatch.cs

### Phase 4: Extended (Future)
- Remaining tiers, diseases, tonics, liquids
- Full grammar system
- All particle text

---

## 22. Updated Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Combat message fragmentation | High | Patch at DidX/DidXToY level |
| Effect text hardcoding | Medium | Patch Effect base class |
| Korean grammar complexity | Medium | Postposition utility class |
| Color tag corruption | Medium | Preserve tag structure |
| Performance (frequent calls) | High | Aggressive caching |

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-22 | Initial plan created |
| 1.1 | 2026-01-22 | Added: Status effects, damage types, color tags, hardcoded text analysis, grammar system, expanded folder structure, additional patches |
