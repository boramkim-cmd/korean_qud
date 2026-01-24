# Gameplay Features Context

In-game mechanics, abilities, and character progression systems.

## Layer 1 (Simple Glossaries)

- `cybernetics.json` - Cybernetic implant translations (54 entries) [DEPRECATED - use CYBERNETICS/ folder]
  - Implant names, effects, installation descriptions

## Layer 2 (Structured Data)

### SKILLS/ - Combat and Utility Skills (20 files)

**구조**: 각 스킬별 개별 파일
```
LOCALIZATION/GAMEPLAY/SKILLS/
├── Acrobatics.json      (4 powers)
├── Axe.json             (7 powers)
├── Bow_and_Rifle.json   (9 powers)
├── Cooking_and_Gathering.json (5 powers)
├── Cudgel.json          (6 powers)
├── Customs_and_Folklore.json (2 powers)
├── Endurance.json       (7 powers)
├── Heavy_Weapon.json    (3 powers)
├── Long_Blade.json      (8 powers)
├── Multiweapon_Fighting.json (4 powers)
├── Persuasion.json      (6 powers)
├── Physic.json          (4 powers)
├── Pistol.json          (8 powers)
├── Self_Discipline.json (6 powers)
├── Shield.json          (6 powers)
├── Short_Blade.json     (7 powers)
├── Single_Weapon_Fighting.json (4 powers)
├── Tactics.json         (5 powers)
├── Tinkering.json       (10 powers)
└── Wayfaring.json       (11 powers)
```

**파일 구조**:
```json
{
  "names": { "Axe": "도끼" },
  "description": "You are skilled with axes.",
  "description_ko": "당신은 도끼에 숙달되어 있습니다.",
  "powers": {
    "axe proficiency": {
      "name": "도끼 숙련",
      "desc": "도끼로 공격할 때 명중에 +2 보너스를 받습니다."
    },
    "cleave": {
      "name": "쪼개기",
      "desc": "도끼로 적을 공격해 명중할 때마다 75% 확률로..."
    }
  }
}
```

**총 파워 수**: 122개

### MUTATIONS/ - Genetic Mutations and Defects (81 files)

- `Physical_Mutations/` - Body modifications (31 files: Stinger, Wings, etc.)
- `Mental_Mutations/` - Psionic powers (27 files: Telepathy, Pyrokinesis, etc.)
- `Physical_Defects/` - Physical drawbacks (12 files: Albino, Brittle Bones, etc.)
- `Mental_Defects/` - Mental drawbacks (8 files: Amnesia, Narcolepsy, etc.)
- `Morphotypes/` - Transformation types (3 files: Chimera, Esper, Unstable Genome)
- Each file contains: names, description, leveltext (progression details)

### CYBERNETICS/ - True Kin Cybernetic Implants (55+ files)

- `1_Point/` - Basic implants (9 files: optical bioscanner, cherubic visage, etc.)
- `2_Point/` - Standard implants (22 files: dermal insulation, night vision, etc.)
- `3_Point/` - Advanced implants (19 files: bionic heart, giant hands, etc.)
- `4_Point/` - High-grade implants (8 files: motorized treads, gun rack, etc.)
- `5_Point/` - Elite implants (2 files: micromanipulator array, grafted mirror arm)
- `6_Point/` - Premium implants (4 files: biodynamic power plant, etc.)
- `8_Point/` - Top-tier implants (5 files: optical multiscanner, crysteel hand bones, etc.)
- `12_Point/` - Legendary implants (4 files: cathedra variants)
- Each file contains: names, description, behaviorDescription, slot, cost

## Usage

These files are loaded by:
- `SkillLocalizationManager` (Skills - direct Harmony patch to SkillFactory)
- `LocalizationManager` (Layer 1 glossaries)
- `StructureTranslator` (Layer 2 mutations and cybernetics)

Translations appear in:
- Skills screen (S key) - via `Patch_SkillFactory`
- Character creation (mutation/cybernetics selection)
- Character status (examining mutations/implants)
- Mutation gain/loss messages
- Cybernetics terminal (installation/removal)

## Patch Files

- `02_10_12_Skills.cs` - SkillFactory postfix patch for skill/power translation
