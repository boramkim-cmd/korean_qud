# Caves of Qud Korean Localization - Changelog

> **Version**: 3.0 | **Last Updated**: 2026-01-19

> [!NOTE]
> **AI Agent**: This document is for completion records. Read `00_PRINCIPLES.md` first!

Official changelog for all completed work.
Completed items from `03_TODO.md` are moved here.

---

## [2026-01-19] - Documentation System English Conversion

### 📋 Documentation
- **All documentation converted to English**
  - Primary language: English (for AI reasoning quality)
  - Korean versions: `*_KO.md` suffix (preserved for reference)
  - Code comments: English only
  - Commit messages: English

- **Copilot Instructions Enhanced**
  - `.github/copilot-instructions.md`: Complete rewrite in English
  - LAYER 0: Language rules (think in English, report in Korean)
  - Auto-sync script updated for English format

### Changed Files
- `Docs/00_PRINCIPLES.md`: English version (Korean backup: `00_PRINCIPLES_KO.md`)
- `Docs/04_CHANGELOG.md`: English version (Korean backup: `04_CHANGELOG_KO.md`)
- `Docs/05_ERROR_LOG.md`: English version (Korean backup: `05_ERROR_LOG_KO.md`)
- `.github/copilot-instructions.md`: English with SSOT structure

---

## [2026-01-19] - Character Creation Screen Critical Bug Fixes

### 🔴 Critical Bug Fixes
- **[ERR-008] Attribute Screen Crash Fixed**
  - **Cause**: Game source `AttributeSelectionControl.Updated()` calls `Attribute.Substring(0,3)`, Korean translation ("힘") less than 3 chars causes `ArgumentOutOfRangeException`
  - **Resolution**: Removed direct `attr.Attribute` translation, use Postfix patch on `AttributeSelectionControl.Updated()` to translate UI text only
  - **Impact**: Fixed inability to proceed after caste/calling selection

### 🔧 Changed
- **[ERR-009] Bullet (Dot) Auto-Add Logic**
  - Improved `StructureTranslator.CombineWithLevelText()`
  - Auto-add `{{c|ù}} ` to each LevelTextKo item (with duplicate prevention)
  - Fixed double dot and missing dot issues

- **[ERR-011] Reputation Translation Logic**
  - `ChargenTranslationUtils.TranslateLongDescription()`: Strip color tags before faction lookup
  - Added case-insensitive matching
  - `factions.json`: Added faction name variations (30 → 52 entries)

### 🗑️ Removed
- **[ERR-010] Removed English Parenthetical in Caste Names**
  - Removed English notation from 5 Castes JSON files
  - Unified format: `"영사(Consul)"` → `"영사"`
  - Files: Artifex, Consul, Eunuch, Praetorian, Syzygyrior

### 📊 Impact
- **Modified Files**: 9
- **Resolved Issues**: 5 (double dot, missing dot, hardcoding/order, caste selection, English mixed)
- **Severity**: Critical → Resolved

### ⚠️ Lessons Learned
1. **Data vs UI Separation**: NEVER translate fields that game source processes with `Substring()`, `Split()`, etc. Instead, use Postfix patch at UI display point.
2. **Check Player.log**: Always check for `ArgumentOutOfRangeException` and similar runtime errors.
3. **Dynamic Text Handling**: Use Regex pattern matching for runtime-generated text like reputation.

---

## [2026-01-18] - LOCALIZATION Folder Structure Reorganization

### 🏗️ Refactored
- **Translation File Structure Overhaul**
  - Introduced context-based hierarchy (CHARGEN/, GAMEPLAY/, UI/)
  - Moved and renamed 12 Layer 1 files
  - Relocated 3 Layer 2 folders (MUTATIONS, GENOTYPES, SUBTYPES)
  - Integrated `glossary_proto.json` into GENOTYPES/SUBTYPES, marked deprecated

- **Code Updates**
  - `LocalizationManager.cs`: Added recursive JSON loading (`SearchOption.AllDirectories`)
  - `StructureTranslator.cs`: Updated folder paths

- **Documentation Improvements**
  - `LOCALIZATION/README.md`: Complete overhaul
  - Added `README.md` to each subfolder (CHARGEN, GAMEPLAY, UI)
  - Updated file paths in `Docs/10_DEVELOPMENT_GUIDE.md`

### ✨ Added
- **New Folder Structure**:
  - `CHARGEN/`: Character creation (modes, stats, ui, presets, locations, factions + GENOTYPES, SUBTYPES)
  - `GAMEPLAY/`: Gameplay features (skills, cybernetics + MUTATIONS)
  - `UI/`: User interface (common, options, terms)

---

## [2026-01-17] - Translation Engine Improvements

### 🔧 Changed
- **Tag Handling Improvements**
  - Color tag normalization: `{{C|...}}` → `{{c|...}}`
  - Bullet prefix preservation in translations
  - Fallback logic for fragmented tags

- **StructureTranslator Enhancements**
  - Added `GetTranslationData()` for MUTATIONS/GENOTYPES/SUBTYPES
  - Implemented `CombineDescriptionAndLevelText()` method
  - Support for multi-variant mutations (Stinger variants)

### 📋 Documentation
- Added ERR-006 and ERR-007 to error log
- Updated architecture documentation

---

## [2026-01-16] - Phase 1 Stabilization

### ✨ Added
- **Inventory Screen Patches**
  - Filter bar translation (*All → 전체)
  - Category translations (Weapons, Armor, Tools, etc.)

- **Options Screen**
  - Completed 50 missing option translations
  - Added HelpText translations

### 🔧 Changed
- **Korean Particle Processing**
  - Color tag-aware particle selection (은/는, 이/가, 을/를)
  - Handles tags inside word boundaries

---

## Template for New Entries

```markdown
## [YYYY-MM-DD] - Brief Description

### ✨ Added
- New feature or file

### 🔧 Changed
- Modification to existing functionality

### 🗑️ Removed
- Removed feature or file

### 🐛 Fixed
- Bug fix (reference ERR-XXX if applicable)

### 📋 Documentation
- Documentation updates

### 📊 Impact
- **Modified Files**: N
- **Resolved Issues**: N
```
