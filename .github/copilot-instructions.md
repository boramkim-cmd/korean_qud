폰트 이슈는 아닐건데 appleGothic 사용 해# Caves of Qud Korean Localization - AI Instructions
# Version: 2.1 | Updated: 2026-01-22
# This file is automatically read by GitHub Copilot at every session.
# SSOT (Single Source of Truth) - AI_SESSION_START.md is deprecated

################################################################################
# LAYER 0: SESSION START CHECKLIST (Read First!)
################################################################################

## 🚨 BEFORE STARTING ANY WORK, DO THIS:
# 1. Read TODO for pending tasks and test requirements:
#    cat Docs/en/reference/03_TODO.md | head -150
# 2. Read CHANGELOG for recent changes:
#    cat Docs/en/reference/04_CHANGELOG.md | head -100
# 3. Read ERROR_LOG for known issues:
#    cat Docs/en/reference/05_ERROR_LOG.md | head -100
# 4. Check if previous session has UNTESTED changes that need verification

################################################################################
# LAYER 0.5: LANGUAGE RULES (Highest Priority)
################################################################################

## ALL DOCUMENTATION AND CODE IN ENGLISH
# - All reasoning, code, comments, documentation: ENGLISH
# - Only user-facing reports/responses: KOREAN (when user speaks Korean)
# - User may ask in Korean, but you MUST think and work in English
# - This improves AI reasoning quality and code consistency

## DOCUMENTATION LANGUAGE POLICY
# - All project documents: ENGLISH (primary)
# - Korean translations: Only when explicitly requested
# - Code comments: ENGLISH only
# - Commit messages: ENGLISH

################################################################################
# LAYER 1: CRITICAL RULES (Never Violate)
################################################################################

## DANGEROUS FIELDS - NEVER TRANSLATE DIRECTLY
# Game source uses Substring(), Split() on these fields - direct translation causes crash!

| Class | Field | Processing | Safe Patch Point |
|-------|-------|------------|------------------|
| AttributeDataElement | Attribute | Substring(0,3) | AttributeSelectionControl.Updated() Postfix |
| ChoiceWithColorIcon | Id | Selection logic comparison | Translate Title only, NEVER modify Id |

## COLOR TAG DUPLICATION - NEVER INCLUDE IN TRANSLATIONS
# TranslationEngine auto-restores tags, including them causes duplication
# BAD:  {"{{c|u}} text": "{{c|u}} translated"}
# GOOD: {"{{c|u}} text": "translated"}

## MANDATORY VALIDATION BEFORE DEPLOYMENT
# Always run: python3 tools/project_tool.py

################################################################################
# LAYER 2: SEVEN CORE PRINCIPLES
################################################################################

1. Documentation First: If not documented, it doesn't exist
2. No Guessing: Always verify in actual code (grep -r "keyword" Assets/core_source/)
3. Reuse First: Search existing code before writing new
4. Validate Required: Never deploy without project_tool.py
5. Log Errors: All issues -> Docs/en/reference/05_ERROR_LOG.md
6. Log Changes: All changes -> Docs/en/reference/04_CHANGELOG.md
7. Check Both Namespaces: Most screens have dual implementation (XRL.UI + Qud.UI)

## UI FRAGMENTATION PRINCIPLE (Critical for UI Patches)
# UI elements are often fragmented across multiple files and hardcoded in unexpected places.
# Never assume a single patch point covers all UI elements on a screen.
#
# Example: Options Screen has FOUR separate rendering paths:
#   - OptionsScreen.cs          -> Main panel options
#   - OptionsCategoryControl.cs -> Right panel category rows  
#   - LeftSideCategory.cs       -> LEFT panel category names (separate class!)
#   - MenuOption / overlay      -> Bottom action buttons
#
# ALWAYS:
# 1. Search for ALL classes that touch the target UI element
# 2. Check both XRL.UI and Qud.UI namespaces
# 3. Look for: SetText(), text.text, Title, DisplayText, Description
# 4. Verify each rendering path is patched independently
# 5. Test each UI area separately after patching

################################################################################
# LAYER 3: PAST CRITICAL ERRORS (Never Repeat!)
################################################################################

| ID | Symptom | Root Cause | Resolution |
|----|---------|------------|------------|
| ERR-018 | Tutorial popup text remai... | **Problem 1: Korean Text ... |  |
| ERR-013 | True Kin caste selection ... |  | #### 1. NormalizeLine() E... |
| ERR-008 | Cannot proceed to next st... | Game source code `Attribu... | Deleted direct `attr.Attr... |
| ERR-009 | Bullet (`{{c|ù}}`) prefix... | Unknown | See error log |
| ERR-011 | Game source `SubtypeEntry... | Game source `SubtypeEntry... | `ChargenTranslationUtils.... |
| ERR-006 | Stinger Mutation Descript... | `02_10_10_CharacterCreati... | Split into structured for... |
| ERR-012 | Options screen: "Interfac... | Unknown | See error log |

# Full details: Docs/05_ERROR_LOG.md

################################################################################
# LAYER 4: KEY FILE PATHS
################################################################################

## Core Layer (Translation Engine)
# Scripts/00_Core/00_00_00_ModEntry.cs          - Mod entry point
# Scripts/00_Core/00_00_01_TranslationEngine.cs - Tag preservation/restoration
# Scripts/00_Core/00_00_02_ScopeManager.cs      - Screen-based scope management
# Scripts/00_Core/00_00_03_LocalizationManager.cs - JSON loading/search

## Patch Layer (UI Patches)
# Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs - Character creation 12 modules

## Utils Layer
# Scripts/99_Utils/99_00_03_StructureTranslator.cs - MUTATIONS/GENOTYPES/SUBTYPES

## Data Layer (Translation JSON)
# LOCALIZATION/CHARGEN/   - Character creation
# LOCALIZATION/GAMEPLAY/  - Gameplay (includes MUTATIONS/)
# LOCALIZATION/UI/        - Common UI

## Documentation (Reorganized by language/purpose)
# Docs/en/guides/00_PRINCIPLES.md      - Detailed principles (required reading)
# Docs/en/guides/06_ARCHITECTURE.md    - System architecture
# Docs/en/guides/10_DEVELOPMENT_GUIDE.md - Development guide
# Docs/en/reference/03_TODO.md         - Task tracking
# Docs/en/reference/04_CHANGELOG.md    - Change history
# Docs/en/reference/05_ERROR_LOG.md    - Error history
# Docs/ko/guides/                       - Korean guides (workflow, style)
# Docs/ko/reference/                    - Korean reference (TODO, changelog)

################################################################################
# LAYER 5: ESSENTIAL COMMANDS
################################################################################

## Pre-work Verification
# grep -r "ClassName" Assets/core_source/     # Check game source
# grep -r "keyword" Scripts/                   # Check existing patches

## Debugging
# macOS: tail -f ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log
# Windows: %APPDATA%\..\LocalLow\Freehold Games\CavesOfQud\Player.log

## Deployment
# python3 tools/project_tool.py   # Validation (required!)
# bash tools/sync-and-deploy.sh   # Deploy

################################################################################
# LAYER 6: HARMONY PATCH PATTERNS (Must Follow)
################################################################################

## Scope Management Pattern:
# [HarmonyPatch(typeof(TargetClass))]
# public static class Patch_ScreenName {
#     private static bool _scopePushed = false;
#     [HarmonyPrefix] void Show_Prefix() {
#         if (!_scopePushed) {
#             ScopeManager.PushScope(LocalizationManager.GetCategory("category"));
#             _scopePushed = true;
#         }
#     }
#     [HarmonyPostfix] void Hide_Postfix() {
#         if (_scopePushed) { ScopeManager.PopScope(); _scopePushed = false; }
#     }
# }

## UI-Only Postfix Pattern (Protect Data Fields):
# [HarmonyPostfix]
# static void Updated_Postfix(SomeControl __instance) {
#     // BAD:  __instance.data.Field = "Korean";  // Never modify data fields!
#     // GOOD: __instance.textElement.text = "Korean";  // Only modify UI
# }

################################################################################
# LAYER 7: TRANSLATION API USAGE
################################################################################

## 1. Simple Translation (Current Scope)
# if (TranslationEngine.TryTranslate(text, out string translated))
#     element.text = translated;

## 2. Category-Specific Translation
# if (LocalizationManager.TryGetAnyTerm(key, out string value, "chargen_ui", "ui"))
#     element.text = value;

## 3. Structured Data (MUTATIONS, GENOTYPES, SUBTYPES)
# var data = StructureTranslator.GetTranslationData("Clairvoyance", "MUTATIONS");
# if (data != null) element.text = data.GetCombinedLongDescription();

################################################################################
# COMPLETION CHECKLIST
################################################################################

# [ ] python3 tools/project_tool.py validation passed
# [ ] Errors logged in Docs/en/reference/05_ERROR_LOG.md (if any)
# [ ] Changes logged in Docs/en/reference/04_CHANGELOG.md
# [ ] bash tools/sync-and-deploy.sh deployment complete
