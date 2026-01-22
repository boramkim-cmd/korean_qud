# Caves of Qud Korean Localization - Changelog

> **Version**: 6.0 | **Last Updated**: 2026-01-22

---

## Recent Changes

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

### Phase 2: Gameplay (25%)
| Date | Work |
|------|------|
| 01-22 | Object localization system (Phases 0-4) |

---

## Statistics
- Total translation entries: 3,938
- Mutation files: 81
- Object files: 51
- Build status: Success

---

*Full history: _archive/04_CHANGELOG_full_20260122.md*
