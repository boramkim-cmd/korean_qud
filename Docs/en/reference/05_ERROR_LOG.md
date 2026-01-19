# Caves of Qud Korean Localization - Error/Issue Log

> **Version**: 2.1 | **Last Updated**: 2026-01-19

> [!WARNING]
> **AI Agent**: Check unresolved issues (🔴 OPEN) before starting work!
> Read `00_PRINCIPLES.md` first, then log errors in this document when they occur.

This document records errors encountered during development and their solutions.
**All errors must be logged here** to prevent the same issues from recurring.

---

## 📋 Documentation System Integration

### Integration Structure
```
10_DEVELOPMENT_GUIDE.md (immutable reference)
          ↓
03_TODO.md (dynamic tracking)
          ↓
04_CHANGELOG.md (completion log)
          ↓
05_ERROR_LOG.md (this document - error/issue tracking)
```

### Logging Principles
1. **Log All Errors**: Even minor errors
2. **Document Resolution Process**: All attempted methods
3. **Highlight Final Solution**: Clearly mark successful approach
4. **Write Prevention Guide**: Document how to avoid same issue

---

## 📊 Issue Classification

### Status Markers
| Status | Meaning |
|--------|---------|
| 🔴 **OPEN** | Unresolved - work needed |
| 🟡 **IN PROGRESS** | Being resolved |
| 🟢 **RESOLVED** | Resolution complete |
| ⚪ **WONTFIX** | Cannot fix or no fix needed |

### Severity Classification
| Severity | Meaning | Example |
|----------|---------|---------|
| 🔴 **Critical** | Game crash or mod load failure | Harmony patch error |
| 🟠 **High** | Major feature not working | Translation not showing |
| 🟡 **Medium** | Partial function issue | Specific screen translation missing |
| 🟢 **Low** | Minor problem | Typo, style inconsistency |

---

## ERR-014: Toughness Attribute Translation Inconsistency

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🟢 RESOLVED |
| **Severity** | 🟡 Medium |
| **Discovered** | 2026-01-19 |
| **Resolved** | 2026-01-19 |

### Symptoms
1. Attribute screen shows "지구력" instead of expected "건강" for Toughness attribute
2. Three different translations used across codebase:
   - Code patch: "건강" (correct)
   - JSON files: "지구력" (incorrect - mixed with Endurance skill)
   - terms.json: "강인함" (incorrect)
3. Screenshots confirm "지구력 +2" showing in-game instead of "건강 +2"

### Root Cause Analysis
**Translation Term Confusion**: 
- **Toughness (Attribute)** and **Endurance (Skill)** are DIFFERENT game concepts
- Both were incorrectly translated as "지구력" in some files
- Caused mixed display in character creation screens

**Files with Wrong Translations**:
| File | Wrong Translation | Correct |
|------|------------------|---------|
| `Callings/Nomad.json` | "지구력 +2" | "건강 +2" |
| `Callings/Watervine_Farmer.json` | "지구력 +2" | "건강 +2" |
| `Castes/Priest_of_All_Moons.json` | "지구력 +2" | "건강 +2" |
| `Castes/Child_of_the_Deep.json` | "지구력 +3" | "건강 +3" |
| `Castes/Praetorian.json` | "지구력 +1" | "건강 +1" |
| `MUTATIONS/Two-Hearted.json` | "+2 지구력(Toughness)" | "+2 건강(Toughness)" |
| `UI/terms.json` | "toughness": "강인함" | "toughness": "건강" |
| `UI/common.json` | "toughness": "강인함" | "toughness": "건강" |

### ✅ Final Resolution
Established clear translation rules:
- **Toughness (Attribute)** → "건강" (health/constitution)
- **Endurance (Skill)** → "지구력" (endurance/stamina)

Updated 8 JSON files to use correct translation:
1. Fixed all "지구력 +N" to "건강 +N" in Caste/Calling files
2. Updated `terms.json` and `common.json`
3. Fixed mutation leveltext

### Related Files
- `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Nomad.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Watervine_Farmer.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Priest_of_All_Moons.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Child_of_the_Deep.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Praetorian.json`
- `LOCALIZATION/GAMEPLAY/MUTATIONS/Physical_Mutations/Two-Hearted.json`
- `LOCALIZATION/UI/terms.json`
- `LOCALIZATION/UI/common.json`

### Prevention Guide
⚠️ **RULE**: Always distinguish between Attribute names and Skill names in translation glossary.
1. Maintain consistent terminology: Create a translation glossary for attributes vs skills
2. Use English parenthetical notation when ambiguous (e.g., "건강(Toughness)" vs "지구력(Endurance)")
3. Cross-reference code patch dictionaries with JSON data before deployment

---

## ERR-013: Caste Stat/Save Modifiers Not Translated

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🟢 RESOLVED |
| **Severity** | 🟠 High |
| **Discovered** | 2026-01-19 |
| **Resolved** | 2026-01-19 |

### Symptoms
1. True Kin caste selection screen shows untranslated stat modifiers:
   - `"+15 heat resistance"` (should be `"열 저항 +15"`)
   - `"+15 cold resistance"` (should be `"냉기 저항 +15"`)
   - `"+2 to saves vs. bleeding"` (should be `"출혈 저항 +2"`)
2. These appear alongside properly translated Korean items, creating mixed-language display.

### Root Cause Analysis
1. **Stat Format Mismatch**:
   - JSON `leveltext`: `"HeatResistance +15"` (CamelCase, stat first)
   - Game output: `"+15 heat resistance"` (lowercase, number first)
   - `StructureTranslator.NormalizeLine()` couldn't match these as duplicates

2. **CamelCase Not Handled**:
   - `NormalizeLine()` converts `"+15 heat resistance"` → `"heat resistance 15"`
   - But `"HeatResistance +15"` → `"heatresistance 15"` (no space between words)
   - These don't match, so Korean leveltext not applied

3. **Missing Save Modifiers**:
   - XML: `<savemodifier Vs="Bleeding" Amount="2" />`
   - Game generates: `"+2 to saves vs. bleeding"`
   - This wasn't in JSON `leveltext`, so no Korean translation existed

### ✅ Final Resolution

#### 1. NormalizeLine() Enhancement
Added CamelCase to space-separated conversion:
```csharp
// In StructureTranslator.NormalizeLine()
// 3. Normalize CamelCase to space-separated (e.g., "HeatResistance" -> "Heat Resistance")
result = Regex.Replace(result, @"([a-z])([A-Z])", "$1 $2");
```

#### 2. JSON leveltext Format Updated
Changed to match game's output format:
- `"HeatResistance +15"` → `"+15 heat resistance"`
- `"ColdResistance +15"` → `"+15 cold resistance"`

Files modified (8 Castes):
- `Child_of_the_Deep.json`, `Child_of_the_Wheel.json`
- `Child_of_the_Hearth.json`, `Fuming_God-Child.json`
- `Consul.json`, `Artifex.json`, `Praetorian.json`, `Eunuch.json`

#### 3. Added Save Modifier Translations
Added `"+2 to saves vs. bleeding"` to JSON for 4 Castes:
- `Horticulturist.json`, `Priest_of_All_Suns.json`
- `Priest_of_All_Moons.json`, `Syzygyrior.json`

#### 4. chargen_ui.json Fallback Translations
Added common patterns to handle any edge cases:
```json
"Resistance Stats": {
  "+15 heat resistance": "열 저항 +15",
  "+15 cold resistance": "냉기 저항 +15"
},
"Save Bonuses": {
  "+2 to saves vs. bleeding": "출혈 저항 +2"
}
```

### Related Files
- `Scripts/99_Utils/99_00_03_StructureTranslator.cs`
- `LOCALIZATION/CHARGEN/ui.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/*.json` (12 files)

### Prevention Guide
⚠️ **RULE**: JSON `leveltext` must match game's runtime output format exactly.
1. Check game source for how stats are displayed (e.g., `Statistic.GetStatDisplayName()`)
2. Use game's format in JSON: `"+N stat_name"` not `"StatName +N"`
3. Include ALL dynamic content in JSON (stats, save modifiers, reputation)
4. Test with actual game to verify string matching

---

*No unresolved issues at this time.*

---

## ERR-008: Character Creation Attribute Screen Crash (Substring Index Error)

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🟢 RESOLVED |
| **Severity** | 🔴 Critical |
| **Discovered** | 2026-01-19 |
| **Resolved** | 2026-01-19 |

### Symptoms
1. Cannot proceed to next step after caste/calling selection in character creation.
2. Multiple `ArgumentOutOfRangeException: Index and length must refer to a location within the string.` errors in Player.log.
3. Crash on Attributes screen.

### Root Cause Analysis
1. Game source code `AttributeSelectionControl.Updated()` extracts first 3 characters of attribute name:
   ```csharp
   attribute.text = data.Attribute.Substring(0, 3).ToUpper();  // "Strength" → "STR"
   ```
2. When Korean patch translates `attr.Attribute` to Korean (e.g., "힘"), it's less than 3 characters, so `Substring(0, 3)` throws `ArgumentOutOfRangeException`.
3. This causes `EmbarkBuilder.advance()` to fail, preventing progression.

### ✅ Final Resolution
1. **Removed existing patch**: Deleted direct `attr.Attribute` translation from `Patch_QudAttributesModuleWindow`.
2. **Added new patch**: Postfix patch on `AttributeSelectionControl.Updated()` to translate only UI text (`attribute.text`).
3. **Key insight**: Keep data fields in original English, translate only UI display.

### Related Files
- `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`

### Prevention Guide
⚠️ **WARNING**: When game source processes a data field (Substring, Split, etc.), NEVER translate that field directly. Instead, use Postfix patch at the UI display point after processing.

---

## ERR-009: Character Creation Description Bullet (Dot) Missing and Duplicated

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🟢 RESOLVED (Updated 2026-01-19) |
| **Severity** | 🟠 High |
| **Discovered** | 2026-01-19 |
| **Resolved** | 2026-01-19 (re-fixed) |

### Symptoms
1. Bullet (`{{c|ù}}`) prefix missing in calling/caste selection descriptions.
2. **Double dot issue** (e.g., `힘 +2..`) in skill/stat entries.
3. English and Korean descriptions appearing duplicated (separated by newlines).

### Root Cause Analysis (Updated)
1. **Original Issue**: `leveltext_ko` arrays missing bullet prefixes → Fixed by auto-add logic.
2. **NEW: Double dot source**: `leveltext_ko` entries had trailing periods (`.`) while English `leveltext` did not.
   - English: `"Strength +2"` (no dot)
   - Korean: `"힘 +2."` (has dot)
   - When game adds its own formatting, double dots appeared.
3. **Inconsistency in JSON data**: Some translators added periods, others didn't.

### Why Previous "Fix" Failed
The 2026-01-19 initial fix only addressed:
- Missing bullet prefix auto-addition in `CombineWithLevelText()`
- Case-insensitive bullet detection

But it did NOT address:
- **Data-level inconsistency**: Periods in `leveltext_ko` that shouldn't exist
- Translation data validation was not enforced

### ✅ Final Resolution (Re-fixed 2026-01-19)
1. **Removed trailing periods from simple entries** in all 12 Callings JSON files:
   - Skill/stat entries: `"힘 +2."` → `"힘 +2"`
   - Simple nouns: `"도끼."` → `"도끼"`
   
2. **Kept periods for complete sentences**:
   - `"{{b|재활용 슈트}}를 착용한 상태로 시작합니다."` (verb ending → keep period)
   
3. **Rule established**:
   | Type | Example EN | Example KO | Period? |
   |------|-----------|-----------|---------|
   | Stat modifier | `Strength +2` | `힘 +2` | ❌ No |
   | Skill name | `Long Blade` | `롱 블레이드` | ❌ No |
   | Complete sentence | `Starts with...` | `~시작합니다` | ✅ Yes |

### Related Files
- `LOCALIZATION/CHARGEN/SUBTYPES/Callings/*.json` (12 files modified)
- `Scripts/99_Utils/99_00_03_StructureTranslator.cs`

### Prevention Guide
⚠️ **RULE**: Match punctuation style with English source.
- If English has no trailing period → Korean should not have one either
- Always verify both `leveltext` and `leveltext_ko` arrays for consistency
- Run `tools/verify_structure_data.py` before deployment

---

## ERR-010: Caste Names with English in Parentheses

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🟢 RESOLVED |
| **Severity** | 🟢 Low |
| **Discovered** | 2026-01-19 |
| **Resolved** | 2026-01-19 |

### Symptoms
Caste selection screen shows `"영사(Consul)"`, `"프라이토리아(Praetorian)"` with English in parentheses, exceeding UI width.

### Root Cause Analysis
Castes JSON files' `names` section written in dual English format.

### ✅ Final Resolution
Removed English parenthetical notation from 5 Castes JSON files:
- `Artifex.json`: `"아르티펙스(Artifex)"` → `"아르티펙스"`
- `Consul.json`: `"영사(Consul)"` → `"영사"`
- `Eunuch.json`: `"환관(Eunuch)"` → `"환관"`
- `Praetorian.json`: `"프라이토리아(Praetorian)"` → `"프라이토리아"`
- `Syzygyrior.json`: Similar fix

### Related Files
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/*.json`

---

## ERR-011: Reputation Text Not Translated

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🟢 RESOLVED |
| **Severity** | 🟠 High |
| **Discovered** | 2026-01-19 |
| **Resolved** | 2026-01-19 |

### Symptoms
Reputation text like `"+100 reputation with Apes"` shows in English in calling/caste descriptions.

### Root Cause Analysis
1. Game source `SubtypeEntry.GetChargenInfo()` generates reputation text at runtime.
2. Cannot define in static JSON; requires dynamic translation.

### ✅ Final Resolution
1. `ChargenTranslationUtils.TranslateLongDescription()` enhanced:
   - Regex pattern to match reputation format: `^([+-]?\d+)\s+reputation with\s+(.+)$`
   - Strip color tags before faction lookup
   - Translate faction name from `factions.json`
   - Reconstruct: `{factionKo}와(과)의 평판 {sign}{value}`

2. Expanded `factions.json` from 30 to 52 entries with case variations.

### Related Files
- `Scripts/99_Utils/99_00_02_ChargenTranslationUtils.cs`
- `LOCALIZATION/CHARGEN/factions.json`

---

## ERR-006: Stinger Mutation Description Structure Parsing Issue

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🟢 RESOLVED |
| **Severity** | 🟠 High |
| **Discovered** | 2026-01-17 |
| **Resolved** | 2026-01-17 |

### Symptoms
`Stinger`: Mutation sub-type (Venom) description shown mixed with main description.

### Root Cause Analysis
`02_10_10_CharacterCreation.cs` used single `text_ko` field for both description and level text.

### ✅ Final Resolution
Split into structured format:
- `description` (core one-liner) + `leveltext` (array) structure
- Use `description_ko` + `leveltext_ko` for Korean
- `CombineDescriptionAndLevelText()` method for combination

### Related Files
- `Scripts/99_Utils/99_00_03_StructureTranslator.cs`

---

## ERR-007: Color Tag Double Display

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🟢 RESOLVED |
| **Severity** | 🟡 Medium |
| **Discovered** | 2026-01-17 |
| **Resolved** | 2026-01-17 |

### Symptoms
Color tags like `{{c|ù}}` appearing twice in output.

### Root Cause Analysis
Translation JSON included color tags, but `TranslationEngine` also auto-restores them.

### ✅ Final Resolution
- Remove tags from translation values in JSON
- Let `TranslationEngine` handle tag restoration automatically

### Prevention Guide
⚠️ **NEVER include color tags in translation JSON values** - the engine restores them automatically.

---

## ERR-012: Persistent Translation Issues Due to Code vs Data Confusion

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🟢 RESOLVED (Meta-Analysis) |
| **Severity** | 🟠 High (Process Issue) |
| **Discovered** | 2026-01-19 |
| **Resolved** | 2026-01-19 |

### Symptoms
Issues marked as "RESOLVED" kept reappearing:
1. Options screen: "Interface sounds", "Fire crackling sounds" still in English
2. Character creation: Double dots, separated lines in callings/castes
3. AI models reporting "fix complete" but no actual change visible

### Root Cause Analysis: Why "Fixed" Issues Persisted

#### Problem 1: Code vs Data Confusion
| Aspect | What AI Fixed | What Actually Needed Fixing |
|--------|---------------|----------------------------|
| Options | Harmony patch logic (C#) | **Translation JSON data missing entries** |
| Callings | `CombineWithLevelText()` algorithm | **`leveltext_ko` data had inconsistent periods** |

**Lesson**: Code patches are useless if translation data doesn't exist or is malformed.

#### Problem 2: Validation Gap
- `project_tool.py` validates JSON syntax, not **semantic correctness**
- No tool verified that `leveltext` and `leveltext_ko` had matching formats
- No tool checked if options.json covered all game options

#### Problem 3: Scope Mismatch
- Previous session analyzed `CODE_ANALYSIS_REPORT.md` (code issues)
- But user's screenshots showed **data issues** (missing translations)
- AI focused on wrong problem domain

### ✅ Resolution: Process Improvements

1. **Always check BOTH code AND data**:
   ```bash
   # Before claiming "fix complete":
   grep -r "English text" LOCALIZATION/  # Check if translation exists
   grep -r "TranslateMethod" Scripts/     # Check if patch exists
   ```

2. **Match source vs translation formatting**:
   - Compare `leveltext` array (EN) with `leveltext_ko` (KO)
   - Periods, bullets, color tags must match

3. **Screenshot-driven debugging**:
   - When user provides screenshot with English text, search for that EXACT text
   - Don't assume code fix will resolve data issue

### Checklist for Future Sessions

- [ ] Does the translation JSON contain the exact English key shown in screenshot?
- [ ] Does the patch code actually load and use that JSON category?
- [ ] Are there formatting differences between EN/KO that could cause display issues?
- [ ] Did you TEST in game after deploying?

### Related Files
- This is a meta-analysis, applies to all translation files

---

## ERR-001: Inventory "*All" Filter Not Translated

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🟢 RESOLVED |
| **Severity** | 🟡 Medium |
| **Discovered** | 2026-01-16 |
| **Resolved** | 2026-01-16 |

### Symptoms
Inventory screen filter bar shows "*All" in English.

### ✅ Final Resolution
Implemented `InventoryAndEquipmentStatusScreen.UpdateViewFromData` Postfix patch to translate filter texts.

### Related Files
- `Scripts/02_Patches/10_UI/02_10_07_Inventory.cs`

---

## Template for New Errors

```markdown
## ERR-XXX: [Brief Title]

### Basic Info
| Item | Content |
|------|---------|
| **Status** | 🔴 OPEN / 🟡 IN PROGRESS / 🟢 RESOLVED |
| **Severity** | 🔴 Critical / 🟠 High / 🟡 Medium / 🟢 Low |
| **Discovered** | YYYY-MM-DD |
| **Resolved** | YYYY-MM-DD (if resolved) |

### Symptoms
1. Description of observable problem

### Root Cause Analysis
1. Technical explanation

### ✅ Final Resolution (if resolved)
1. What was done to fix it

### Related Files
- Path to relevant files

### Prevention Guide
⚠️ How to avoid this in future
```
