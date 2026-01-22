# Caves of Qud Korean Localization - Changelog

> **Version**: 6.3 | **Last Updated**: 2026-01-22

---

## Recent Changes

### [2026-01-22] Item Tooltip Localization Complete
- **Patches Created/Modified**:
  - `02_10_02_Tooltip.cs` - ShowManually unified patch for all tooltip paths
  - `02_20_00_ObjectTranslator.cs` - Dynamic food patterns + state suffix handling
- **Features Added**:
  - Tooltip header translation ("This Item" → "현재 아이템", "Equipped Item" → "장착 아이템")
  - Dynamic food patterns: `{creature} jerky/meat/haunch` → Korean
  - State suffix translation: `[empty]` → `[비어있음]`, `(lit)` → `(점화됨)`
- **Bugs Fixed**:
  - TooltipTrigger vs Tooltip.GameObject confusion (critical)
  - State suffix processing order (waterskin [empty] now properly translated)
  - Look.QueueLookerTooltip path not covered (world map tooltips)
- **JSON Added**:
  - `common.json`: tooltips section
  - `food.json`: bear jerky, haunch, preserved meat entries
- See: [10_ITEM_TOOLTIP_ANALYSIS.md](10_ITEM_TOOLTIP_ANALYSIS.md)

### [2026-01-22] P3-06 Tool Scripts Consolidation
- Upgraded `project_tool.py` to v2.0 with CLI subcommands
  - `validate`, `build`, `glossary`, `stats`, `help`
- Moved 3 legacy scripts to `tools/_legacy/`:
  - `check_missing_translations.py` (XML-based, obsolete)
  - `verify_structure_data.py` (integrated into project_tool)
  - `fix_empty_descriptions.py` (one-time fix, complete)
- Created `tools/README.md` documentation
- Total legacy scripts: 13 (safely archived)

### [2026-01-22] Object Translation Expansion
- Added new creature categories: birds, reptiles, farmers, seedsprout
- Added new item categories: ammo, books
- Final stats: 57 JSON files, 321+ translation entries
- Git synced: commit a6d9cf2

### [2026-01-22] P2-01 Message Log Patch Complete
- Created `02_10_16_MessageLog.cs` - Harmony patch for MessageQueue.AddPlayerMessage
- Created `LOCALIZATION/GAMEPLAY/messages.json` - 50+ message patterns
- Features: verb translation dictionary, Korean josa handling, pattern matching
- Categories: flight, movement, items, combat, status, interaction, system

### [2026-01-22] Mutation & Object Systems Complete
- Verified all 81 mutation files translated (Physical/Mental/Defects/Morphotypes)
- Object JSON reorganized: 51 files, 300+ entries (type-based structure)
- Validation fixes: 0 empty translations, 0 duplicate keys

### [2026-01-21] Tutorial Translation (ERR-018)
- Fixed smart quote mismatch (U+2019 vs U+0027)
- Added Korean text skip logic
- Added missing plain text variations

### [2026-01-20] Font System
- Korean font bundle loading from StreamingAssets
- TMP_FontAsset fallback registration
- Tooltip Korean glyph support

### [2026-01-19] Character Creation Fixes
- ERR-017: Attribute screen multi-fix (breadcrumb, tooltip, descriptions)
- ERR-016: Attribute tooltip/description localization
- ERR-015: Chargen overlay scope fix
- ERR-014: Toughness terminology (건강 not 지구력)
- ERR-013: Caste stat/save modifiers
- ERR-008: Attribute crash (Substring fix)
- ERR-009: Bullet dot issues

---

## Summary by Phase

### Phase 1: Stabilization (100%)
| Date | Work |
|------|------|
| 01-16 | Inventory filter, Options values, Josa support, Mutation desc |
| 01-17~22 | Mutation JSON restructure (81 files) |
| 01-21 | Tutorial translation system |

### Phase 2: Gameplay (75%)
| Date | Work |
|------|----- |
| 01-22 | Object localization system (Phases 0-4) |
| 01-22 | Message Log Patch (P2-01) |

---

## Statistics
- Total translation entries: 4,130+
- Mutation files: 81
- Object files: 57
- Message patterns: 50+
- Build status: Success

---

*Full history: _archive/04_CHANGELOG_full_20260122.md*
