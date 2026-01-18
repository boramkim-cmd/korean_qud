# Caves of Qud Korean Localization - Error/Issue Log

> **Version**: 2.0 | **Last Updated**: 2026-01-19

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
| **Status** | 🟢 RESOLVED |
| **Severity** | 🟠 High |
| **Discovered** | 2026-01-19 |
| **Resolved** | 2026-01-19 |

### Symptoms
1. Bullet (`{{c|ù}}`) prefix missing in calling/caste selection descriptions.
2. Double dot issue where bullets appear duplicated on some screens.

### Root Cause Analysis
1. `CHARGEN/SUBTYPES/` JSON files' `leveltext_ko` arrays missing bullet prefixes.
2. No auto-add bullet logic in `StructureTranslator.CombineWithLevelText()`.

### ✅ Final Resolution
1. Improved `CombineWithLevelText()` method:
   - Auto-add bullet (`{{c|ù}} `) to each LevelTextKo item
   - Prevent duplicate addition if bullet already exists
   
```csharp
// Check if bullet already exists
bool hasBullet = line.StartsWith("{{c|ù}}") || line.StartsWith("ù") || ...;
if (!hasBullet) formattedExtras.Add("{{c|ù}} " + line);
```

### Related Files
- `Scripts/99_Utils/99_00_03_StructureTranslator.cs`

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
