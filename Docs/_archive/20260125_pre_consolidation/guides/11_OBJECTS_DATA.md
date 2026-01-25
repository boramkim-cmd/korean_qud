# Object Translations

> **Last Modified**: 2026-01-24 | **Total Entries**: 6,169

This folder contains translation data for creatures, items, furniture, terrain, and widgets.

## ⚠️ ISOLATION POLICY

This folder is **completely separate** from `LOCALIZATION/GAMEPLAY/`.
Do not move files between these folders.

## Structure

```
OBJECTS/
├── creatures/              # Creature translations
│   ├── _common.json        # Common terms (corpse, species)
│   ├── tutorial.json       # Tutorial creatures
│   ├── animals/            # Bears, mammals, etc.
│   ├── humanoids/          # Goatfolk, others (Baetyl, golems, trolls)
│   └── insects/            # Ants, beetles, crabs, hoppers, moths, spiders, worms
│
├── items/                  # Item translations
│   ├── _common.json        # Materials, prefixes, modifiers
│   ├── tutorial.json       # Tutorial items
│   ├── armor/              # head, hands, feet (all tiers)
│   ├── weapons/            # melee (axes, cudgels), ranged (guns)
│   ├── ammo/               # Ammunition
│   └── books/              # Books and scrolls
│
├── furniture/              # Furniture translations
│   └── widgets.json        # 44 widget entries
│
└── terrain/                # Terrain translations
    └── zone.json           # 52 zone entries
```

**Total Files**: 67 JSON files

## JSON Schema

```json
{
  "BlueprintId": {
    "names": {
      "english display name": "한글 표시명"
    },
    "description": "English description",
    "description_ko": "한글 설명"
  }
}
```

## Example

```json
{
  "Bear": {
    "names": {
      "bear": "곰"
    },
    "description": "A large, powerful creature covered in thick fur.",
    "description_ko": "두꺼운 털로 덮인 크고 강력한 생물이다."
  }
}
```

## Hot Reload

Use `Ctrl+W` → `kr:reload` to reload all JSON files without restarting the game.

## Version

- Version: 3.0
- Created: 2026-01-22
- Updated: 2026-01-24 (Major expansion: 67 files, 6,169 entries)
