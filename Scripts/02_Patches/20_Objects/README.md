# Object Localization System

This folder contains the **ISOLATED** Object (Creature/Item) Localization System.

## ⚠️ ISOLATION POLICY

This system is **completely separate** from:
- `TranslationEngine.cs` (CharacterCreation translations)
- `StructureTranslator.cs` (Mutation translations)
- `LocalizationManager.cs` (except read-only `GetModDirectory()`)

**DO NOT** modify any Core files when working on this system.

## Files

| File | Purpose |
|------|---------|
| `02_20_00_ObjectTranslator.cs` | Core translator with isolated cache |
| `02_20_01_DisplayNamePatch.cs` | Patches `GetDisplayNameEvent.GetFor()` |
| `02_20_02_DescriptionPatch.cs` | Patches `Description.GetShortDescription()` |
| `02_20_99_DebugWishes.cs` | Debug commands (`kr:reload`, `kr:check`, etc.) |

## JSON Data Location

Translation data is stored in `LOCALIZATION/OBJECTS/`:
- `creatures/` - Creature translations
- `items/` - Item translations

## Debug Commands (Ctrl+W)

| Command | Description |
|---------|-------------|
| `kr:reload` | Reload all JSON files (no restart needed) |
| `kr:check <blueprint>` | Check translation for specific blueprint |
| `kr:untranslated` | List untranslated objects in current zone |
| `kr:stats` | Show translation statistics |
| `kr:clearcache` | Clear display name cache |

## Rollback

If something breaks, delete these folders to restore:
1. `Scripts/02_Patches/20_Objects/`
2. `LOCALIZATION/OBJECTS/`

## Version History

| Version | Date | Time | Changes |
|---------|------|------|----------|
| 2.1 | 2026-01-22 | 09:30 | Phase 2 implemented: melee_weapons.json, armor.json, tools.json |
| 2.0 | 2026-01-22 | 08:08 | Phase 0-1 implemented, tutorial JSON files created |
| 1.0 | 2026-01-22 | - | Initial structure created |

**Last Modified**: 2026-01-22 09:30:00

See: `Docs/en/guides/OBJECT_LOCALIZATION_PLAN.md`
