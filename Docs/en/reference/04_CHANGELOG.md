# Caves of Qud Korean Localization - Changelog

> **Version**: 4.0 | **Last Updated**: 2026-01-22 17:32:00

> [!NOTE]
> **AI Agent**: This document is for completion records. Read `00_PRINCIPLES.md` first!

Official changelog for all completed work.
Completed items from `03_TODO.md` are moved here.

---

## [2026-01-22 17:32] - Object Localization System Phase 3 Implementation

### Summary
Implemented Phase 3 of Object Localization System: Joppa Area.
Added comprehensive translations for Tier 1 creatures, Snapjaws, and Joppa NPCs.

### ✅ JSON Data Created

**Tier 1 Humanoids** (`LOCALIZATION/OBJECTS/creatures/tier1_humanoids.json`):
- 25+ Snapjaw variants
  - Base: 스냅조
  - Scavenger variants (0-2): 스냅조 청소부
  - Hunter variants (0-2): 스냅조 사냥꾼
  - Shotgunner variants (0-2): 스냅조 산탄총잡이
  - Brute: 스냅조 무법자
  - Warlord/Hero: 스냅조 전쟁군주
  - Troglodyte variants: 피더, 창마, 덫사냥꾼, 하울러
  - Golem: 스냅조 골렘
- Cannibal: 식인종

**Tier 1 Animals** (`LOCALIZATION/OBJECTS/creatures/tier1_animals.json`):
- 20+ animals covering early game encounters
- Bear: 곰, Bat: 박쥐, Pig: 돼지, Boar: 멧돼지
- Ray Cat: 레이 캣, Cave Spider: 동굴 거미
- Baboon variants: 비비, 거대 비비, 약삭빠른 비비
- Clockwork Beetle: 클락워크 비틀
- Additional fauna: 소금메뚜기, 발광어, 불개미, etc.

**Joppa NPCs** (`LOCALIZATION/OBJECTS/creatures/npcs_joppa.json`):
- 15 named NPCs and roles
  - Elder Irudad (ElderBob): 이루다드
  - Argyve: 아르기브
  - Mehmet: 메흐메트
  - Tam: 탐
  - Warden Yrame: 이라메
  - Nima Ruda: 니마 루다
  - Ctesiphus: 크테시푸스
- Village roles: 수초 농부, 사과 농부, 메카님스트 개종자

**Common Terms Update** (`LOCALIZATION/OBJECTS/creatures/_common.json`):
- Version 2.0 with expanded species (cat, dog, spider, crab, etc.)
- Creature classes: 거미류, 연체동물, 갑각류, etc.
- Roles: 청소부, 사냥꾼, 무법자, 전쟁군주, etc.
- Honorifics: 장로, 경비관

### Statistics
- Total new JSON entries: ~60 creatures
- Tier 1 humanoids: 25 entries
- Tier 1 animals: 20 entries
- Joppa NPCs: 15 entries

### Deploy Status
- Build: ✅ Success (1 warning - unrelated)
- Deploy: ✅ Success

---

## [2026-01-22 09:30] - Object Localization System Phase 2 Implementation

### Summary
Implemented Phase 2 of Object Localization System: Basic Equipment.
Added comprehensive translations for melee weapons, armor, and tools.

### ✅ JSON Data Created

**Melee Weapons** (`LOCALIZATION/OBJECTS/items/melee_weapons.json`):
- 37 melee weapons covering all material tiers
- Cudgels: club, wrench, pestle, staff, walking stick, maces, war hammers
- Short Blades: daggers (bronze/iron/steel/carbide), kukri, kris, utility knives
- Axes: battle axes (all tiers), hand axes, vinereaper, halberds
- Full descriptions translated for all items

**Armor** (`LOCALIZATION/OBJECTS/items/armor.json`):
- 26 armor pieces covering Tiers 1-6
- Body armor: leather, bark, studded, chain, plate, crysteel, fullerite
- Helmets: steel, chain coif, armet, skull caps
- Boots: leather, chain, steel, carbide, magnetized, bounding
- Specialty: flexivest, recycling suit, rubber suit, nanoweave

**Tools** (`LOCALIZATION/OBJECTS/items/tools.json`):
- 15 utility items
- Light sources: torch, glowsphere, headlamp
- Energy cells: chem, fidget, solar, nuclear, antimatter
- Tools: bandage, toolkit, advanced toolkit, pickaxe, jackhammer
- Containers: waterskin, canteen, hoversled

**Common Terms Update** (`LOCALIZATION/OBJECTS/items/_common.json`):
- Version 2.0 with expanded materials (chain, bark, chitin, folded carbide)
- New prefixes: two-handed, opal-pommeled, magnetized, greased, bounding
- Expanded item types: 30+ new entries

### Statistics
- Total new JSON entries: ~78 items
- Melee weapons: 37 entries
- Armor: 26 entries  
- Tools: 15 entries

### Deploy Status
- Build: ✅ Success
- Deploy: ✅ Success

---

## [2026-01-22 08:08] - Object Localization System Phase 0-1 Implementation

### Summary
Implemented isolated Object (Creature/Item) Localization System Phase 0-1.
Complete separation from existing CharacterCreation/Mutation translation infrastructure.

### ✅ Code Implementation

**Verified Files (already existed, validated)**:
- `Scripts/02_Patches/20_Objects/02_20_00_ObjectTranslator.cs` - Isolated JSON loader + cache
- `Scripts/02_Patches/20_Objects/02_20_01_DisplayNamePatch.cs` - `GetDisplayNameEvent.GetFor()` patch (fixed XRL.Core namespace)
- `Scripts/02_Patches/20_Objects/02_20_02_DescriptionPatch.cs` - Description patches
- `Scripts/02_Patches/20_Objects/02_20_99_DebugWishes.cs` - `kr:reload`, `kr:check`, `kr:untranslated`

### ✅ JSON Data Created

**Tutorial Creatures** (`LOCALIZATION/OBJECTS/creatures/tutorial.json`):
- TutorialSnapjaw → 스냅조 청소부
- TutorialBear → 곰
- TutorialClockworkBeetlePariah → 클락워크 비틀 - 동족의 패리아
- Base versions (Bear, Snapjaw Scavenger, ClockworkBeetle)

**Tutorial Items** (`LOCALIZATION/OBJECTS/items/tutorial.json`):
- TutorialDagger → {{w|청동}} 단검
- TutorialTorch → 횃불
- TutorialLeatherArmor → 가죽 갑옷
- TutorialChemCell → {{c|화학 전지}}
- TutorialBattleAxe → {{w|청동}} 전투 도끼
- TutorialHalfFullWaterskin → 물주머니
- Base item versions included

**Common Terms**:
- `creatures/_common.json` - species, corpse terms, classes
- `items/_common.json` - materials, prefixes, suffixes, item types

### Bug Fix
- Fixed `XRLCore` → `XRL.Core.XRLCore` namespace in DisplayNamePatch.cs

### Testing
- Build: ✅ Success (1 warning - unrelated)
- Deploy: ✅ Success

### Next Steps
- Phase 2: Basic equipment (melee_weapons.json, armor.json)
- Phase 3: Joppa area creatures and NPCs

---

## [2026-01-22] - Debug Tools Documentation

### Summary
Comprehensive documentation created for all in-game debugging tools useful for object/creature translation testing.

### ✅ Documentation Added

**New File: `Docs/en/guides/DEBUG_TOOLS_REFERENCE.md`**
- Complete Wish command system documentation
- Object/creature spawning commands (`<name>`, `item:<name>`, `testhero:<name>`)
- Debug options reference (`DebugInternals`, `DebugShowConversationNode`, etc.)
- Game logging locations and monitoring commands
- Look/Examine system analysis
- Proposed custom wish commands for mod (`kr:check`, `kr:reload`, `kr:verbose`)
- Recommended testing workflow

### Key Findings

**Wish System Access:**
- Always available via **Ctrl+W** (no special mode needed)
- Fuzzy search using Levenshtein distance
- Supports `item:<name>:<count>` for batch spawning

**Useful Test Commands:**
- `testpets` - Tests all creature DisplayNames for `[bracket]` issues
- `testobjects` - Tests all object DisplayNames for issues
- `blueprint` - Shows blueprint name of adjacent object
- `showcharset` - Displays character set (useful for font testing)

**Custom Wish Support:**
- Mods can add custom wishes using `[HasWishCommand]` class attribute
- Method attribute `[WishCommand(Command = "name")]` registers command
- Supports string parameters and regex matching

### Files Created
- `Docs/en/guides/DEBUG_TOOLS_REFERENCE.md`

---

## [2026-01-21] - Tutorial Translation Key Matching Fix (ERR-018)

### Summary
Fixed critical issue where tutorial text was not being translated despite having translations in JSON files. Two root causes identified and resolved:
1. Infinite loop from re-translating already-translated Korean text
2. Smart quote mismatch between game text and JSON keys

### ✅ Changes

**Korean Text Skip Logic**
- Added `ContainsKorean()` helper function to detect already-translated text
- Prevents infinite translation attempts on Korean text (e.g., `{{y|야영 하기.}}`)

**Smart Quote Normalization**
- Game uses smart quotes: `'` (U+2019), `'` (U+2018), `"` (U+201C), `"` (U+201D)
- JSON uses straight quotes: `'` (U+0027), `"` (U+0022)
- Added character replacement in `NormalizeKey()` function

**NormalizeKey() Improvements**
- Added trim before other normalizations
- Added full-text wrapping color tag removal (`{{y|entire text}}` → `entire text`)
- Improved debug logging to show both original and normalized text

**Missing Translation Keys Added**
- `"There's a village to the north called Joppa. Let's go there."` (plain text version)
- `"Press 8 or ◎."` variation
- `"TUTORIAL GUIDE"` UI title
- `"[Space] Continue"` button text

### Files Modified
- `Scripts/02_Patches/10_UI/02_10_15_Tutorial.cs`
- `LOCALIZATION/GAMEPLAY/tutorial/04_surface.json`
- `LOCALIZATION/GAMEPLAY/tutorial/_common.json`

### Related
- Error Log: `05_ERROR_LOG.md` → ERR-018

---

## [2026-01-20] - Korean Font Bundle Loading & Tooltip Font Fallback

### Summary
Added runtime loading of the `qudkoreanfont` AssetBundle from `StreamingAssets/Mods/*/Fonts` and registered its `TMP_FontAsset` as a global fallback so Korean glyphs render in UI (including tooltips).

### ✅ Changes
- Load `qudkoreanfont` AssetBundle at UI initialization and extract `TMP_FontAsset`.
- Insert the loaded font into `TMP_Settings.fallbackFontAssets` (front) and add it as a fallback to existing `TMP_FontAsset` instances.
- If the bundle is not present or contains no usable font, detect an existing `TMP_FontAsset` that supports Korean and use it as a fallback (preferred target order defined by `TargetFontNames`).
- Ensure `TooltipTrigger.SetText` applies the fallback to its internal `TextMeshProUGUI` components so tooltips display Korean characters.

### Files Modified
- `Scripts/00_Core/00_00_99_QudKREngine.cs` (Font loading + fallback registration)
- `Scripts/02_Patches/10_UI/02_10_02_Tooltip.cs` (Tooltip Postfix: apply fallback to internal TMP components)

---

## [2026-01-19] - Attribute Screen Multi-Issue Fix (ERR-017)

### Summary
Fixed multiple issues in the attribute distribution screen:
1. Breadcrumb (top bar) showing English caste/calling names
2. Empty bonus source tooltips
3. Attribute descriptions remaining in English

### ✅ Changes

**Breadcrumb Translation Enhancement**
- `ChargenTranslationUtils.TranslateBreadcrumb()` now uses StructureTranslator first for subtype names
- Added lowercase fallback for translation lookup
- Added `chargen_attributes` scope for better coverage

**BonusSource Tooltip Fix**
- Fixed regex pattern to properly capture source names with Qud color tags `{{important|...}}`
- Added StructureTranslator lookup for calling names (not just castes)
- Changed non-greedy `(.+?)` to greedy `(.+)` to capture full source string

**Attribute Description JSON Update**
- Added full ChargenDescription strings from Genotypes.xml to `attributes.json`
- Previous JSON had abbreviated versions that didn't match actual game strings
- Added Willpower descriptions for both Mutant and True Kin (different descriptions)
- Added Ego variations (with/without mental mutation potency mention)

### Files Modified
- `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`
- `Scripts/99_Utils/99_00_02_ChargenTranslationUtils.cs`
- `LOCALIZATION/CHARGEN/attributes.json`

### Known Issue (WONTFIX)
Point display overlap (`[1점]` with description) is a UI prefab layout issue, cannot be fixed via code patches.

### Related
- Error Log: `05_ERROR_LOG.md` → ERR-017

---

## [2026-01-19] - Attribute Tooltip BonusSource Fallback

### Summary
Improved attribute bonus tooltips to handle BonusSource lines without explicit type tokens and prioritized AppleGothic for tooltip fonts on macOS.

### ✅ Changes
- Added fallback parsing for BonusSource lines that omit `caste/calling/genotype/subtype`.
- Prioritized AppleGothic in tooltip font selection for Korean glyph coverage.

### Files Modified
- `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`

---

## [2026-01-19] - Chargen Overlay Scope Fix (ERR-015)

### Summary
Fixed untranslated "character creation" header in the character creation overlay by ensuring UITextSkin has the proper localization scope.

### ✅ Changes
- Added scope management for EmbarkBuilder overlay (`BeforeShowWithWindow` push, `Hide` pop)
- Ensured Back/Next static labels translate under chargen scopes

### Files Modified
- `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`

### Related
- Error Log: `05_ERROR_LOG.md` → ERR-015

---

## [2026-01-19] - Attribute Screen Tooltip/Description Localization (ERR-016)

### Summary
Fixed untranslated attribute descriptions and caste bonus tooltips in the attribute distribution screen, and ensured layout height is recalculated after translation.

### ✅ Changes
- Translate `AttributeDataElement.Description` during `QudAttributesModuleWindow.UpdateControls()` using `chargen_attributes` scope.
- Strip Qud color tags from `BonusSource` and map source types (caste/calling/genotype/subtype) to Korean labels.
- Apply localized bonus source lines to the attribute tooltip.

### Files Modified
- `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`

### Related
- Error Log: `05_ERROR_LOG.md` → ERR-016

---

## [2026-01-19] - Toughness Translation Consistency Fix (ERR-014)

### Summary
Fixed inconsistent translation of "Toughness" attribute across codebase. The attribute was incorrectly translated as "지구력" (endurance) instead of "건강" (health), causing confusion with the "Endurance" skill.

### 🔴 Critical Issue: Term Confusion
- **Problem**: "Toughness" (attribute) and "Endurance" (skill) are different concepts
  - Toughness = HP, healing rate, poison/disease resistance
  - Endurance = Skill for stamina-related abilities
- Both were translated as "지구력" in some files, causing mixed display

### ✅ Translation Rules Established
| English | Korean | Context |
|---------|--------|---------|
| Toughness | 건강 | Attribute (character stat) |
| Endurance | 지구력 | Skill name |

### Files Modified (8 files)
**Callings:**
- `Nomad.json`: "지구력 +2" → "건강 +2"
- `Watervine_Farmer.json`: "지구력 +2" → "건강 +2"

**Castes:**
- `Priest_of_All_Moons.json`: "지구력 +2" → "건강 +2"
- `Child_of_the_Deep.json`: "지구력 +3" → "건강 +3" (Endurance skill kept as "지구력")
- `Praetorian.json`: "지구력 +1" → "건강 +1"

**Mutations:**
- `Two-Hearted.json`: "+2 지구력(Toughness)" → "+2 건강(Toughness)"

**UI Terms:**
- `terms.json`: "toughness": "강인함" → "건강"
- `common.json`: "toughness": "강인함" → "건강"

### Related
- Error Log: `05_ERROR_LOG.md` → ERR-014

---

## [2026-01-19] - Attribute Selection Screen Full Translation

### Summary
Complete translation of the attribute/stat selection screen in character creation:
- Attribute names shortened to 1-2 characters (Korean gaming standard)
- Caste bonus tooltips translated
- Point cost display translated
- Bottom menu bar translated

### 🟢 Attribute Names (1-2 characters)
| English | Korean |
|---------|--------|
| Strength | 힘 |
| Agility | 민첩 |
| Toughness | 건강 |
| Intelligence | 지능 |
| Willpower | 의지 |
| Ego | 자아 |

### 🟢 Caste Short Names (≤6 characters, no spaces)
| English | Korean |
|---------|--------|
| Priest of All Moons | 달의사제 |
| Priest of All Suns | 태양사제 |
| Horticulturist | 원예가 |
| Child of the Deep | 심연의자녀 |
| Child of the Hearth | 화로의자녀 |
| Child of the Wheel | 수레의자녀 |
| Fuming God-Child | 연신의자녀 |
| Artifex | 기술자 |
| Consul | 영사 |
| Eunuch | 환관 |
| Praetorian | 근위병 |
| Syzygyrior | 합위전사 |

### 🟢 UI Elements
- `[1pts]` → `[1점]`
- `Points Remaining: 38` → `남은 포인트: 38`
- `+2 from Priest of All Moons caste` → `달의사제 계급 +2`

### Files Created
- `LOCALIZATION/CHARGEN/attributes.json` - Attribute translation data

### Files Modified
- `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs` - Extended `Patch_AttributeSelectionControl`, added `Patch_QudAttributesModuleWindow`
- `LOCALIZATION/CHARGEN/stats.json` - Updated attribute names

### Technical Details
- Added `CasteShortNames` dictionary for tooltip translation
- Added `AttributeShortNames` dictionary for 1-2 char names
- Added `TranslateBonusSource()` helper with regex parsing
- Added `GetKeyMenuBar` Postfix for "Points Remaining" translation

---

## [2026-01-19] - True Kin Caste Stat Translation Fix (ERR-013)

### Summary
Fixed untranslated stat modifiers and save bonuses in True Kin caste selection screen.

### 🔴 Critical Fix: Stat/Save Modifier Translation
- **Problem**: 3 types of text appearing in English:
  - `"+15 heat resistance"` / `"+15 cold resistance"`
  - `"+2 to saves vs. bleeding"`

- **Root Cause**: 
  - JSON `leveltext` format (`"HeatResistance +15"`) didn't match game's output format (`"+15 heat resistance"`)
  - `NormalizeLine()` couldn't convert CamelCase to space-separated
  - Save modifiers completely missing from JSON files

- **Resolution**:
  1. Added CamelCase→space conversion to `StructureTranslator.NormalizeLine()`
  2. Updated 8 Caste JSON files to use game's stat format
  3. Added save modifier entries to 4 Castes with bleeding resistance
  4. Added fallback translations to `chargen_ui.json`

### Files Modified
- `Scripts/99_Utils/99_00_03_StructureTranslator.cs`
- `LOCALIZATION/CHARGEN/ui.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Child_of_the_Deep.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Child_of_the_Wheel.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Child_of_the_Hearth.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Fuming_God-Child.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Consul.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Artifex.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Praetorian.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Eunuch.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Horticulturist.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Priest_of_All_Suns.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Priest_of_All_Moons.json`
- `LOCALIZATION/CHARGEN/SUBTYPES/Castes/Syzygyrior.json`

### Related
- Error Log: `05_ERROR_LOG.md` → ERR-013

---

## [2026-01-19] - UI Translation Consistency Fixes

### Summary
Fixed three major UI translation issues:
1. Missing colons in subtitle translations (double dot issue)
2. Missing periods in calling descriptions
3. Fixed sync script path error

### 🟢 UI Translation Fixes
- **Subtitle Format Fixed**
  - Fixed all subtitle translations to include proper colons on both sides
  - Example: `:choose game mode:` → `:게임 모드 선택:` (was `게임 모드 선택`)
  - Example: `:choose calling:` → `:직업 선택:` (was `직업 선택`)
  - Affected: All 14 subtitle translations in `LOCALIZATION/CHARGEN/ui.json`
  - This fixes the "double dot issue" where colons were appearing from English original + Korean translation without colons

- **Calling Descriptions Consistency**
  - Added periods to all bullet points in all 12 calling leveltext_ko entries
  - Ensures consistent punctuation across all callings
  - Files affected:
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Warden.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Gunslinger.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Scholar.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Marauder.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Arconaut.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Nomad.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Watervine_Farmer.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Water_Merchant.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Apostle.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Pilgrim.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Tinker.json`
    - `LOCALIZATION/CHARGEN/SUBTYPES/Callings/Greybeard.json`

### 🔧 Tool Fixes
- **Sync Script Path Update**
  - Fixed `sync_copilot_instructions.py` to use correct ERROR_LOG path
  - Changed from `Docs/05_ERROR_LOG.md` to `Docs/en/reference/05_ERROR_LOG.md`
  - File: `tools/sync_copilot_instructions.py`

---

## [2026-01-19] - Code Analysis Report Fixes (16 Issues Resolved)

### 🔴 Critical Fixes
- **[Issue 1] ScopeManager.ClearAll() Conditional**
  - Changed from unconditional clear to conditional (only if depth > 3)
  - Prevents scope corruption when transitioning between screens
  - File: `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs`

- **[Issue 2] _scopePushed Pop Handling**
  - Implemented proper `PopScope()` in `Hide_Prefix` method
  - Made `_scopePushed` internal for cross-class access
  - File: `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs`

- **[Issue 3] Data Field Modification Pattern**
  - Changed `QudGenotypeModuleWindow` from `BeforeShow_Prefix` to `GetSelections_Postfix`
  - Now modifies UI objects (`ChoiceWithColorIcon`) instead of data fields (`genotype.DisplayName`)
  - Follows "UI-Only Postfix Pattern" principle
  - File: `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`

### 🟠 High Priority Fixes
- **[Issue 4] Id Field Protection**
  - Added explicit assertions to verify `choice.Id` is never modified
  - Added cache of originalId for verification
  - Files: Multiple patches in CharacterCreation.cs

- **[Issue 5] Traverse Field Access Pattern**
  - Changed from generic `Traverse<T>.Field()` to non-generic `Traverse.Field()` where `FieldExists()` is needed
  - Use `.Value` property for getting/setting values
  - Files: Multiple patches in CharacterCreation.cs

- **[Issue 6] Stat Translation Format Documentation**
  - Documented intentional format change for Korean grammar
  - English: "+2 Agility" → Korean: "민첩 +2"
  - Added comment explaining this is display-only and safe
  - File: `Scripts/99_Utils/99_00_02_ChargenTranslationUtils.cs`

- **[Issue 7] StartingLocation Data Fix**
  - Changed from `BeforeShow_Prefix` to `GetSelections_Postfix`
  - Now modifies `StartingLocationData` UI fields instead of data fields
  - File: `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`

### 🟡 Medium Priority Fixes
- **[Issue 8] Null Check Before ToLowerInvariant()**
  - Added proper null checks: `!string.IsNullOrEmpty(choice.Title) &&`
  - Prevents null reference when Title is null

- **[Issue 10] Case-Sensitive Bullet Check**
  - Changed `line.StartsWith("{{c|ù}}")` to use `StringComparison.OrdinalIgnoreCase`
  - Now handles both `{{c|ù}}` and `{{C|ù}}`
  - File: `Scripts/99_Utils/99_00_03_StructureTranslator.cs`

- **[Issue 11] Silent Exception Logging**
  - Replaced `catch { }` with `catch (Exception ex) { Debug.LogWarning(...) }`
  - Conditional on `#if DEBUG` to avoid production spam
  - File: `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs`

- **[Issue 12] TargetMethod Null Logging**
  - Added `Debug.LogError()` when MainMenu type is not found
  - Helps diagnose translation failures
  - File: `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs`

### 🟢 Low Priority Fixes
- **[Issue 13] Hardcoded Type Names Documentation**
  - Added version notes and documentation for critical type names
  - Listed last verified game version
  - File: `Scripts/00_Core/00_00_00_ModEntry.cs`

- **[Issue 14] FontManager Log Spam Prevention**
  - Added feature flag `_hasLoggedDisabled` to log only once
  - Reduces log spam from repeated calls
  - File: `Scripts/00_Core/00_00_99_QudKREngine.cs`

- **[Issue 15] SteamGalaxyPatch Documentation**
  - Added documentation explaining this patch is NOT related to localization
  - Added note about potential separation to utility mod
  - File: `Scripts/02_Patches/00_Core/02_00_01_SteamGalaxy.cs`

- **[Issue 16] Unicode Escape Handling**
  - Added `\uXXXX` unicode escape sequence handling to JSON parser
  - Supports standard JSON unicode escapes
  - File: `Scripts/00_Core/00_00_03_LocalizationManager.cs`

### Changed Files Summary
- `Scripts/00_Core/00_00_00_ModEntry.cs`
- `Scripts/00_Core/00_00_03_LocalizationManager.cs`
- `Scripts/00_Core/00_00_99_QudKREngine.cs`
- `Scripts/02_Patches/00_Core/02_00_01_SteamGalaxy.cs`
- `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs`
- `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`
- `Scripts/99_Utils/99_00_02_ChargenTranslationUtils.cs`
- `Scripts/99_Utils/99_00_03_StructureTranslator.cs`

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
