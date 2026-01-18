# User Interface Context

Common UI elements, menus, and system messages used throughout the game.

## Files

- `common.json` - Universal UI elements (148 entries)
  - Buttons: "New Game", "Continue", "Load", "Save"
  - Menu items: "Inventory", "Skills", "Character", "Journal"
  - Common actions: "Confirm", "Cancel", "Back", "Next"
  - Status indicators: "Health", "Energy", "Turn"
  
- `options.json` - Settings screen translations (362 entries)
  - Game options: difficulty, permadeath, autosave
  - Audio options: volume, music, sound effects
  - Control options: keybindings, mouse sensitivity
  - Accessibility options: colorblind modes, text size

- `display.json` - Display-related options (16 entries)
  - Resolution and fullscreen settings
  - UI scale and graphics options
  - Visual effects toggles
  
- `terms.json` - General game terminology (53 entries)
  - Attributes: "Strength", "Agility", "Intelligence"
  - Items: "Artifact", "Relic", "Blueprint"
  - Locations: "Joppa", "Grit Gate", "Bethesda Susa"
  - Factions: "Barathrumites", "Mechanimists", "Putus Templar"
  - Common phrases: "Water Ritual", "Greetings", "Farewell"

## Usage

These files are loaded by `LocalizationManager` and used throughout the game:
- `common.json` - Accessed globally by all UI screens
- `options.json` - Loaded when entering settings menu
- `display.json` - Display-specific settings
- `terms.json` - Referenced for consistent terminology across all contexts

## Scope Management

- `common.json` is typically in the base scope (always accessible)
- `options.json` is pushed onto the scope stack when opening settings
- `display.json` supplements display options
- `terms.json` provides fallback translations for core concepts
