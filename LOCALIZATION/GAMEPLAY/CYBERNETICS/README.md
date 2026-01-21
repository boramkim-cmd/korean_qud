# Cybernetics Localization

Structured translation files for True Kin cybernetic implants.

## Structure

Each JSON file contains:
- `names`: English name -> Korean name mapping
- `description`: Original English short description
- `behaviorDescription`: Original English behavior/effect description
- `description_ko`: Korean translation of description
- `behaviorDescription_ko`: Korean translation of behavior description
- `slot`: Body slot(s) where the implant can be installed
- `cost`: Cybernetic point cost

## Categories

Files are organized by point cost:
- `1_Point/` - Basic implants (optical bioscanner, cherubic visage, etc.)
- `2_Point/` - Standard implants (dermal insulation, night vision, etc.)
- `3_Point/` - Advanced implants (bionic heart, giant hands, etc.)
- `4_Point/` - High-grade implants (motorized treads, gun rack, etc.)
- `5_Point/` - Elite implants (micromanipulator array, grafted mirror arm)
- `6_Point/` - Premium implants (penetrating radar, biodynamic power plant, etc.)
- `8_Point/` - Top-tier implants (optical multiscanner, crysteel hand bones, etc.)
- `12_Point/` - Legendary implants (cathedra variants)

## Usage

These files are loaded by:
- `LocalizationManager` for glossary lookups
- `StructureTranslator.GetTranslationData()` for structured data access

Translations appear in:
- Character creation (True Kin cybernetics selection)
- Cybernetics terminal (installation/removal)
- Equipment screen (examining implants)
- Status effects and descriptions
