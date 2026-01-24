# CLAUDE.md - Caves of Qud Korean Localization

> **Project**: Complete Korean (Hangul) localization mod for Caves of Qud
> **Version**: 1.0.0 | **Author**: boram
> **Updated**: 2026-01-24

---

## Quick Start for AI Assistants

### Before Starting Work (Required)
```bash
# 1. Check session state for handoff context
cat Docs/SESSION_STATE.md

# 2. Check current tasks
cat Docs/en/reference/03_TODO.md | head -60

# 3. Check known issues
cat Docs/en/reference/05_ERROR_LOG.md | head -50

# 4. Validate project state
python3 tools/project_tool.py
```

### Documentation Language
- **All documentation**: English (primary)
- **Code comments**: English only
- **Commit messages**: English
- Korean only for translation content in JSON files

---

## Project Overview

This is a Unity/C# mod using **Harmony 2.x runtime patching** to provide Korean translation for Caves of Qud. The mod intercepts game text at UI display points and replaces English text with Korean translations.

### Current Progress
| Phase | Status | Progress |
|-------|--------|----------|
| Phase 1: Stabilization | Complete | 100% |
| Phase 2: Gameplay | In Progress | 75% |
| Phase 3: Optimization | Not Started | 17% |
| Phase 4: Community | Not Started | 0% |

---

## Directory Structure

```
korean_qud/
├── Scripts/                    # C# Source Code (main codebase)
│   ├── 00_Core/               # Core localization engine
│   │   ├── 00_00_00_ModEntry.cs          # Mod entry + Harmony patching
│   │   ├── 00_00_01_TranslationEngine.cs # Main translation logic
│   │   ├── 00_00_02_ScopeManager.cs      # Screen context stacking
│   │   ├── 00_00_03_LocalizationManager.cs # JSON data loading
│   │   └── 00_00_99_QudKREngine.cs       # Korean-specific features
│   ├── 02_Patches/            # Harmony runtime patches
│   │   ├── 00_Core/           # Platform patches
│   │   ├── 10_UI/             # UI patches (menus, tooltips, etc.)
│   │   └── 20_Objects/        # Object/creature translation patches
│   └── 99_Utils/              # Utility functions
│
├── LOCALIZATION/              # Translation Data (262 JSON files)
│   ├── CHARGEN/               # Character creation
│   │   ├── GENOTYPES/         # Race/species data
│   │   └── SUBTYPES/          # Callings & Castes
│   ├── GAMEPLAY/              # In-game content
│   │   ├── MUTATIONS/         # 81 mutation files
│   │   ├── CYBERNETICS/       # Implant data
│   │   └── tutorial/          # Tutorial text
│   ├── OBJECTS/               # Items & creatures (57+ files)
│   │   ├── creatures/         # NPCs, animals, etc.
│   │   └── items/             # Weapons, armor, etc.
│   └── UI/                    # Common UI elements
│
├── Docs/                      # Documentation
│   ├── SESSION_STATE.md       # Current session handoff context
│   └── en/
│       ├── guides/            # Development principles & guides
│       └── reference/         # TODO, changelog, error log
│
├── tools/                     # Development scripts
│   ├── project_tool.py        # Main validation tool (REQUIRED)
│   ├── deploy-mods.sh         # Mod deployment
│   └── session_manager.py     # Session state management
│
├── Assets/                    # Game references
│   └── core_source/           # Decompiled game code for reference
│
├── mod_info.json              # Mod metadata
├── manifest.json              # Mod manifest + preload scripts
└── QudKorean.csproj           # .NET project file
```

---

## Build Commands

```bash
# Validate C# compilation
dotnet build QudKorean.csproj

# Validate project integrity (REQUIRED before deployment)
python3 tools/project_tool.py

# Deploy to game
./tools/deploy-mods.sh "commit message"

# Quick save (git commit)
./tools/quick-save.sh

# Session state save (for handoff)
python3 tools/session_manager.py save "description"
```

---

## Seven Core Principles

```
1. Documentation First    - If not documented, it doesn't exist
2. No Guessing           - Always verify in actual code (use grep)
3. Reuse First           - Search existing code before writing new
4. Validation Required   - Never deploy without project_tool.py
5. Log Errors            - All issues → Docs/en/reference/05_ERROR_LOG.md
6. Log Changes           - All changes → Docs/en/reference/04_CHANGELOG.md
7. Check Both Namespaces - Most screens have XRL.UI + Qud.UI implementations
```

---

## Critical Rules (NEVER Violate)

### Dangerous Fields - NEVER Translate Directly

These fields are processed by game source code. Direct translation causes crashes!

| Class | Field | Processing | Safe Patch Point |
|-------|-------|------------|------------------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` | UI display Postfix |
| `ChoiceWithColorIcon` | `Id` | Selection logic | Translate `Title` only |

**Rule**: If game source uses `Substring()`, `Split()`, or regex on a field, translate at UI display point only.

### Color Tag Rules

**NEVER include color tags in translations** - `TranslationEngine` auto-restores them.

```json
// BAD - causes duplication
{"{{c|u}} text": "{{c|u}} 번역"}

// GOOD - engine restores tags automatically
{"{{c|u}} text": "번역"}
```

### Forbidden Actions

| Forbidden | Reason | Correct Approach |
|-----------|--------|------------------|
| Use `_Legacy/` folder | Outdated code | Use only `Scripts/` |
| Guess method signatures | Harmony fails | Verify with grep |
| Deploy without validation | Runtime errors | Run `project_tool.py` first |
| Translate special tags | Game breaks | Keep `%var%`, `{{tag}}` as-is |
| Check only XRL.UI | Qud.UI may differ | Check BOTH namespaces |

---

## Translation Architecture

### Three-Layer System

```
Layer 1: Simple Glossaries (LocalizationManager)
  - Direct JSON lookup: {"English": "한국어"}
  - Files: CHARGEN/*.json, GAMEPLAY/*.json, UI/*.json

Layer 2: Structured Data (StructureTranslator)
  - Directory-based hierarchical structure
  - Files: CHARGEN/GENOTYPES/, CHARGEN/SUBTYPES/, GAMEPLAY/MUTATIONS/

Layer 3: Runtime Generation (ChargenTranslationUtils)
  - Regex pattern matching for dynamic text
  - Reputation, health display, item states, etc.
```

### Translation Pipeline

```
Raw Text: "{{C|20}} bonus skill points"
  ↓ [1] Trim whitespace
  ↓ [2] Normalize color tags
  ↓ [3] Extract prefix (bullets, checkboxes)
  ↓ [4] Strip tags for lookup
  ↓ [5] Dictionary search
  ↓ [6] Restore color tags automatically
  ↓ [7] Restore prefix
Final: "{{c|20}} 레벨당 보너스 기술 포인트"
```

---

## Harmony Patch Patterns

### Standard UI Patch Template

```csharp
[HarmonyPatch]
public static class Patch_ClassName
{
    static MethodBase TargetMethod()
    {
        // Check BOTH namespaces
        var type = AccessTools.TypeByName("Qud.UI.ClassName")
            ?? AccessTools.TypeByName("XRL.UI.ClassName");
        return type != null ? AccessTools.Method(type, "MethodName") : null;
    }

    [HarmonyPostfix]
    static void MethodName_Postfix(ref string __result)
    {
        if (TranslationEngine.TryTranslate(__result, out var tr))
            __result = tr;
    }
}
```

### Scope Management Pattern

```csharp
[HarmonyPatch(typeof(TargetClass))]
public static class Patch_ScreenName
{
    private static bool _scopePushed = false;

    [HarmonyPrefix]
    static void Show_Prefix() {
        if (!_scopePushed) {
            ScopeManager.PushScope(LocalizationManager.GetCategory("category"));
            _scopePushed = true;
        }
    }

    [HarmonyPostfix]
    static void Hide_Postfix() {
        if (_scopePushed) {
            ScopeManager.PopScope();
            _scopePushed = false;
        }
    }
}
```

### UI-Only Postfix Pattern

```csharp
[HarmonyPostfix]
static void Updated_Postfix(SomeControl __instance) {
    // BAD - modifying data breaks game logic
    // __instance.data.Field = "Korean";

    // GOOD - only modify UI elements
    __instance.textElement.text = "Korean";
}
```

---

## Translation API Usage

```csharp
// 1. Simple translation (uses current scope)
if (TranslationEngine.TryTranslate(text, out string translated))
    element.text = translated;

// 2. Category-specific translation
if (LocalizationManager.TryGetAnyTerm(key, out string value, "chargen_ui", "ui"))
    element.text = value;

// 3. Structured data (mutations, genotypes, subtypes)
var data = StructureTranslator.GetTranslationData("Clairvoyance", "MUTATIONS");
if (data != null)
    element.text = data.GetCombinedLongDescription();
```

---

## File Naming Convention

```
[Priority]_[Type]_[Number]_[Name].cs

00_00_00_ModEntry.cs           # Core layer, index 0
02_10_00_GlobalUI.cs           # Patch layer 2, UI category 10
02_20_01_DisplayNamePatch.cs   # Patch layer 2, Objects category 20
99_00_01_TranslationUtils.cs   # Utils layer 99
```

---

## In-Game Debug Commands

When testing in-game, use these "wish" commands:

```
kr_reload        # Reload JSON without restart
kr_check         # Check translation for blueprint
kr_stats         # Show translation statistics
kr_untranslated  # List untranslated items in current zone
```

**Player Log Location**:
- macOS: `~/Library/Logs/Freehold Games/CavesOfQud/Player.log`
- Linux: `~/.config/unity3d/Freehold Games/Caves of Qud/Player.log`

---

## Key Patterns to Know

### Smart Quote Normalization
```csharp
// Game uses curly quotes, JSON uses straight quotes
text.Replace('\u2019', '\'').Replace('\u2018', '\'')
```

### Korean Text Detection
```csharp
// Skip already-translated Korean text
if (c >= 0xAC00 && c <= 0xD7A3) return true; // Hangul Syllables
```

### BonusSource Parsing
```csharp
// Use greedy match for color tags
@"(.+)\s+(caste|calling|genotype|subtype)\s*([+-]?\d+)"
```

---

## Terminology Standards

| English | Korean | Note |
|---------|--------|------|
| Toughness (Attr) | 건강 | NOT 지구력 |
| Endurance (Skill) | 지구력 | Different from Toughness |
| Strength | 힘 | |
| Agility | 민첩 | |
| Intelligence | 지능 | |
| Willpower | 의지 | |
| Ego | 자아 | |

---

## Common Workflows

### Adding a Translation Entry
1. Find appropriate JSON in `LOCALIZATION/`
2. Add `{"English": "한국어"}` entry
3. Run `python3 tools/project_tool.py` to validate JSON
4. Deploy and test in-game

### Adding/Modifying a Patch
1. Find game class in `Assets/core_source/` or grep game DLLs
2. Create/modify patch in `Scripts/02_Patches/`
3. Follow Harmony pattern: `TargetMethod` → `Prefix/Postfix`
4. Build: `dotnet build QudKorean.csproj`
5. Run `python3 tools/project_tool.py`
6. Deploy and test

### Session End Workflow
```bash
python3 tools/project_tool.py                    # Validate
python3 tools/session_manager.py save "description"  # Save state
git add -A && git commit -m "description"        # Commit
```

---

## UI Fragmentation Warning

UI elements are often fragmented across multiple files. Never assume a single patch covers everything.

**Example**: Options Screen has FOUR rendering paths:
- `OptionsScreen.cs` - Main panel
- `OptionsCategoryControl.cs` - Right panel rows
- `LeftSideCategory.cs` - Left panel categories
- `MenuOption` - Bottom buttons

**Always**:
1. Search ALL classes touching the target UI
2. Check both XRL.UI and Qud.UI namespaces
3. Verify each path is patched independently
4. Test each UI area after patching

---

## Key Documentation Files

| File | Purpose |
|------|---------|
| `Docs/SESSION_STATE.md` | Session handoff context |
| `Docs/en/reference/03_TODO.md` | Current task tracking |
| `Docs/en/reference/04_CHANGELOG.md` | Completed work log |
| `Docs/en/reference/05_ERROR_LOG.md` | Known issues & solutions |
| `Docs/en/guides/01_PRINCIPLES.md` | Detailed principles (read first) |
| `Docs/en/guides/02_ARCHITECTURE.md` | System architecture |

---

## Pre-Deploy Checklist

```
☐ python3 tools/project_tool.py validation passed
☐ dotnet build QudKorean.csproj succeeded
☐ In-game testing completed
☐ 03_TODO.md status updated
☐ Errors logged in 05_ERROR_LOG.md (if any)
☐ Changes documented appropriately
```

---

## Current Test Items (as of 2026-01-22)

See `Docs/en/reference/03_TODO.md` for the latest. Key items:

- [ ] Item Tooltip testing (comparison headers, item states)
- [ ] Message Log testing (movement/flight messages)
- [ ] Attribute Screen verification (names, costs, bonuses)

---

## Contact & Resources

- **Repository**: https://github.com/boramkim-cmd/korean_qud
- **Game**: Caves of Qud (Steam/GOG)
- **Framework**: Harmony 2.x, .NET Framework 4.8, Unity
