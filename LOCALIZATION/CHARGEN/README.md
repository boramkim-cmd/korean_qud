# Character Creation Context

This folder contains all localization data related to the character creation screen.

## Layer 1 (Simple Glossaries)

- `modes.json` - Game modes (Classic, Roleplay, Wander, Daily, Tutorial)
- `stats.json` - Attribute descriptions (Strength, Agility, Toughness, etc.)
- `ui.json` - Character creation UI text (buttons, labels, prompts)
- `presets.json` - Preset character descriptions and traits
- `locations.json` - Starting location names and descriptions
- `factions.json` - Faction names (Fellowship of Wardens, etc.)

## Layer 2 (Structured Data)

- `GENOTYPES/` - Species selection (Mutated Human, True Kin)
  - Each file contains: names, description, leveltext (benefits/drawbacks)
- `SUBTYPES/` - Character classes and roles
  - `Callings/` - Mutated Human professions (12 files: Apostle, Arconaut, etc.)
  - `Castes/` - True Kin social classes (12 files: Artifex, Consul, etc.)
  - Each file contains: names, skill bonuses, starting abilities

## Usage

These files are loaded by:
- `LocalizationManager` (Layer 1 glossaries)
- `StructureTranslator` (Layer 2 structured data)

Translations appear in the character creation screen when selecting game mode, genotype, calling/caste, and customizing attributes.
