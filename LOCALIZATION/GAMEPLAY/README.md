# Gameplay Features Context

In-game mechanics, abilities, and character progression systems.

## Layer 1 (Simple Glossaries)

- `skills.json` - Skill and power names/descriptions (218 entries)
  - Combat skills, crafting skills, social skills
  - Power descriptions (mental mutations, activated abilities)
- `cybernetics.json` - Cybernetic implant translations (54 entries) [DEPRECATED - use CYBERNETICS/ folder]
  - Implant names, effects, installation descriptions

## Layer 2 (Structured Data)

- `MUTATIONS/` - Genetic mutations and defects (81 files total)
  - `Physical_Mutations/` - Body modifications (31 files: Stinger, Wings, etc.)
  - `Mental_Mutations/` - Psionic powers (27 files: Telepathy, Pyrokinesis, etc.)
  - `Physical_Defects/` - Physical drawbacks (12 files: Albino, Brittle Bones, etc.)
  - `Mental_Defects/` - Mental drawbacks (8 files: Amnesia, Narcolepsy, etc.)
  - `Morphotypes/` - Transformation types (3 files: Chimera, Esper, Unstable Genome)
  - Each file contains: names, description, leveltext (progression details)

- `CYBERNETICS/` - True Kin cybernetic implants (55+ files total)
  - `1_Point/` - Basic implants (9 files: optical bioscanner, cherubic visage, etc.)
  - `2_Point/` - Standard implants (22 files: dermal insulation, night vision, etc.)
  - `3_Point/` - Advanced implants (19 files: bionic heart, giant hands, etc.)
  - `4_Point/` - High-grade implants (8 files: motorized treads, gun rack, etc.)
  - `5_Point/` - Elite implants (2 files: micromanipulator array, grafted mirror arm)
  - `6_Point/` - Premium implants (4 files: penetrating radar, biodynamic power plant, etc.)
  - `8_Point/` - Top-tier implants (5 files: optical multiscanner, crysteel hand bones, etc.)
  - `12_Point/` - Legendary implants (4 files: cathedra variants)
  - Each file contains: names, description, behaviorDescription, slot, cost

## Usage

These files are loaded by:
- `LocalizationManager` (Layer 1 glossaries)
- `StructureTranslator` (Layer 2 mutations and cybernetics)

Translations appear in:
- Character creation (mutation/cybernetics selection)
- Skills screen (S key)
- Character status (examining mutations/implants)
- Mutation gain/loss messages
- Cybernetics terminal (installation/removal)
