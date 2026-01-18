# Gameplay Features Context

In-game mechanics, abilities, and character progression systems.

## Layer 1 (Simple Glossaries)

- `skills.json` - Skill and power names/descriptions (218 entries)
  - Combat skills, crafting skills, social skills
  - Power descriptions (mental mutations, activated abilities)
- `cybernetics.json` - Cybernetic implant translations (54 entries)
  - Implant names, effects, installation descriptions

## Layer 2 (Structured Data)

- `MUTATIONS/` - Genetic mutations and defects (81 files total)
  - `Physical_Mutations/` - Body modifications (31 files: Stinger, Wings, etc.)
  - `Mental_Mutations/` - Psionic powers (27 files: Telepathy, Pyrokinesis, etc.)
  - `Physical_Defects/` - Physical drawbacks (12 files: Albino, Brittle Bones, etc.)
  - `Mental_Defects/` - Mental drawbacks (8 files: Amnesia, Narcolepsy, etc.)
  - `Morphotypes/` - Transformation types (3 files: Chimera, Esper, Unstable Genome)
  - Each file contains: names, description, leveltext (progression details)

## Usage

These files are loaded by:
- `LocalizationManager` (Layer 1 glossaries)
- `StructureTranslator` (Layer 2 mutations)

Translations appear in:
- Character creation (mutation selection)
- Skills screen (S key)
- Character status (examining mutations/implants)
- Mutation gain/loss messages
