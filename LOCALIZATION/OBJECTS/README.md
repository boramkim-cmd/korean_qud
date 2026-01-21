# Object Translations

> **Last Modified**: 2026-01-22 08:08:46

This folder contains translation data for creatures and items.

## ⚠️ ISOLATION POLICY

This folder is **completely separate** from `LOCALIZATION/GAMEPLAY/`.
Do not move files between these folders.

## Structure

```
OBJECTS/
├── creatures/          # Creature translations
│   ├── _common.json    # Common terms (corpse, species)
│   ├── tutorial.json   # Tutorial creatures
│   └── *.json          # Other creature files
│
└── items/              # Item translations
    ├── _common.json    # Materials, prefixes, modifiers
    ├── tutorial.json   # Tutorial items
    └── *.json          # Other item files
```

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

- Version: 2.0
- Created: 2026-01-22
