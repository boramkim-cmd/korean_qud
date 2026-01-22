---
name: LOCALIZATION Folder Reorganization
overview: Reorganize LOCALIZATION folder into screen-context based subfolders, resolve duplicates between Layer 1 (glossary files) and Layer 2 (structured folders), and improve maintainability through better categorization.
todos:
  - id: backup-localization
    content: Backup entire LOCALIZATION folder and document baseline
    status: completed
  - id: create-folders
    content: Create CHARGEN/, GAMEPLAY/, UI/, _DEPRECATED/ folders with READMEs
    status: completed
  - id: merge-proto-data
    content: Merge glossary_proto.json into GENOTYPES/SUBTYPES files (26 files)
    status: completed
  - id: move-rename-files
    content: Rename and move 12 glossary files to new locations
    status: completed
  - id: update-code-refs
    content: Update code references in LocalizationManager, StructureTranslator, and docs
    status: completed
  - id: validate-testing
    content: Run project_tool.py, deploy to game, test all screens
    status: completed
  - id: update-docs
    content: Update TODO.md, CHANGELOG.md, and archive deprecated files
    status: completed
---

# LOCALIZATION Folder Reorganization Plan

## Current State Analysis

### Root-Level Files (12 glossary files)

```
glossary_chargen_modes.json    → Character creation: Game modes
glossary_chargen_stats.json    → Character creation: Stat descriptions
glossary_chargen_ui.json       → Character creation: UI text
glossary_proto.json            → Character creation: Names (DUPLICATES GENOTYPES/SUBTYPES)
glossary_pregen.json           → Character creation: Preset descriptions
glossary_location.json         → Character creation: Starting locations
glossary_factions.json         → Character creation: Faction names
glossary_skills.json           → Gameplay: Skills/powers
glossary_cybernetics.json      → Gameplay: Cybernetic implants
glossary_ui.json               → UI: Common interface elements
glossary_options.json          → UI: Settings screen
glossary_terms.json            → General: Core game terms
```

### Existing Subfolders (Layer 2)

```
MUTATIONS/    → Physical/Mental mutations and defects (81 files)
GENOTYPES/    → Mutated Human, True Kin (2 files)
SUBTYPES/     → Callings (12) and Castes (12) = 24 files
```

### Critical Issue: Duplication

`glossary_proto.json` contains 43 entries where **names overlap** with GENOTYPES/SUBTYPES:

- "mutated human", "true kin" → duplicates `GENOTYPES/*.json` names
- "apostle", "tinker", etc. → duplicates `SUBTYPES/Callings/*.json` names
- "artifex", "consul", etc. → duplicates `SUBTYPES/Castes/*.json` names

However, `glossary_proto.json` contains **leveltext descriptions** (e.g., "starts with {{b|recycling suit}}") that are **missing** from the SUBTYPES files.

---

## Proposed Structure

### New Folder Organization

```
LOCALIZATION/
├── README.md                    # Updated structure guide
├── integrity_report.md          # Keep (generated report)
│
├── CHARGEN/                     # 🆕 Character Creation Context
│   ├── README.md                # Subfolder guide
│   ├── modes.json               # ← glossary_chargen_modes.json (renamed)
│   ├── stats.json               # ← glossary_chargen_stats.json (renamed)
│   ├── ui.json                  # ← glossary_chargen_ui.json (renamed)
│   ├── presets.json             # ← glossary_pregen.json (renamed)
│   ├── locations.json           # ← glossary_location.json (renamed)
│   ├── factions.json            # ← glossary_factions.json (renamed)
│   ├── GENOTYPES/               # ← Move from root
│   │   ├── Mutated_Human.json  # Enhanced with proto data
│   │   └── True_Kin.json       # Enhanced with proto data
│   └── SUBTYPES/                # ← Move from root
│       ├── Callings/            # Enhanced with proto data
│       └── Castes/              # Enhanced with proto data
│
├── GAMEPLAY/                    # 🆕 In-Game Features
│   ├── README.md
│   ├── skills.json              # ← glossary_skills.json (renamed)
│   ├── cybernetics.json         # ← glossary_cybernetics.json (renamed)
│   └── MUTATIONS/               # ← Move from root
│       ├── Physical_Mutations/
│       ├── Mental_Mutations/
│       ├── Physical_Defects/
│       ├── Mental_Defects/
│       └── Morphotypes/
│
├── UI/                          # 🆕 Interface Context
│   ├── README.md
│   ├── common.json              # ← glossary_ui.json (renamed)
│   ├── options.json             # ← glossary_options.json (renamed)
│   └── terms.json               # ← glossary_terms.json (consolidated)
│
└── _DEPRECATED/                 # 🆕 Archive old files
    └── glossary_proto.json      # To be deleted after migration
```

---

## Reorganization Steps

### Phase 1: Create New Folder Structure

1. Create `CHARGEN/`, `GAMEPLAY/`, `UI/` folders
2. Create `README.md` in each subfolder explaining purpose
3. Create `_DEPRECATED/` for old files

### Phase 2: Resolve glossary_proto.json Duplication

**Strategy**: Merge proto data into GENOTYPES/SUBTYPES files

#### Example: Merge into SUBTYPES/Callings/Apostle.json

**Before** (current):

```json
{
  "names": {"Apostle": "사도"},
  "description": "",
  "description_ko": "",
  "leveltext": ["Ego +2", "Intimidate", ...],
  "leveltext_ko": ["자아 +2", "위협", ...]
}
```

**After** (enhanced):

```json
{
  "names": {"Apostle": "사도"},
  "description": "",
  "description_ko": "",
  "leveltext": [
    "Ego +2",
    "Intimidate",
    "Proselytize",
    "Customs and Folklore"
  ],
  "leveltext_ko": [
    "자아 +2",
    "위협",
    "개종",
    "풍습 및 민속"
  ],
  "extrainfo": [
    "may rebuke robots"
  ],
  "extrainfo_ko": [
    "로봇을 꾸짖을 수 있음(Rebuke)"
  ]
}
```

**Process**:

1. For each GENOTYPES/SUBTYPES file:

   - Find matching entries in `glossary_proto.json`
   - Add missing `extrainfo` / `extrainfo_ko` fields

2. Validate that all proto entries are accounted for
3. Move `glossary_proto.json` to `_DEPRECATED/`

### Phase 3: Rename and Move Files

```bash
# CHARGEN
mv glossary_chargen_modes.json → CHARGEN/modes.json
mv glossary_chargen_stats.json → CHARGEN/stats.json
mv glossary_chargen_ui.json → CHARGEN/ui.json
mv glossary_pregen.json → CHARGEN/presets.json
mv glossary_location.json → CHARGEN/locations.json
mv glossary_factions.json → CHARGEN/factions.json
mv GENOTYPES/ → CHARGEN/GENOTYPES/
mv SUBTYPES/ → CHARGEN/SUBTYPES/

# GAMEPLAY
mv glossary_skills.json → GAMEPLAY/skills.json
mv glossary_cybernetics.json → GAMEPLAY/cybernetics.json
mv MUTATIONS/ → GAMEPLAY/MUTATIONS/

# UI
mv glossary_ui.json → UI/common.json
mv glossary_options.json → UI/options.json
mv glossary_terms.json → UI/terms.json

# Archive
mv glossary_proto.json → _DEPRECATED/glossary_proto.json
```

### Phase 4: Update Code References

**Files to update**:

1. **`Scripts/00_Core/00_00_03_LocalizationManager.cs`**

   - Update file paths in `LoadGlossary()` method
   - Change `"glossary_*.json"` patterns to new paths
   - Example: `"CHARGEN/modes.json"`, `"UI/common.json"`

2. **`Scripts/99_Utils/99_00_03_StructureTranslator.cs`**

   - Update folder paths: `"MUTATIONS/"` → `"GAMEPLAY/MUTATIONS/"`
   - Update: `"GENOTYPES/"` → `"CHARGEN/GENOTYPES/"`
   - Update: `"SUBTYPES/"` → `"CHARGEN/SUBTYPES/"`

3. **`LOCALIZATION/README.md`**

   - Update folder structure documentation
   - Update Layer 1 vs Layer 2 examples
   - Add new subfolder descriptions

4. **`Docs/01_DEVELOPMENT_GUIDE.md`**

   - Update glossary file paths in Part B.2
   - Update references throughout document

5. **`Docs/02_CORE_QUICK_REFERENCE.md`** (auto-generated)

   - Will auto-update via `tools/project_tool.py`

### Phase 5: Create Subfolder READMEs

**CHARGEN/README.md**:

```markdown
# Character Creation Context

This folder contains all localization data related to the character creation screen.

## Layer 1 (Simple Glossaries)
- `modes.json` - Game modes (Classic, Roleplay, Wander, etc.)
- `stats.json` - Attribute descriptions (Strength, Agility, etc.)
- `ui.json` - Character creation UI text
- `presets.json` - Preset character descriptions
- `locations.json` - Starting location names
- `factions.json` - Faction names

## Layer 2 (Structured Data)
- `GENOTYPES/` - Mutated Human, True Kin (with descriptions)
- `SUBTYPES/` - Callings and Castes (with skill lists)
```

**GAMEPLAY/README.md**:

```markdown
# Gameplay Features Context

In-game mechanics, abilities, and character progression.

## Layer 1
- `skills.json` - Skill and power names/descriptions
- `cybernetics.json` - Cybernetic implant translations

## Layer 2
- `MUTATIONS/` - Physical/Mental mutations and defects (81 files)
```

**UI/README.md**:

```markdown
# User Interface Context

Common UI elements, menus, and system messages.

## Files
- `common.json` - Buttons, menus, common UI text
- `options.json` - Settings screen (362 entries)
- `terms.json` - General game terminology
```

### Phase 6: Validation and Testing

```bash
# 1. Validate JSON structure
python3 tools/project_tool.py

# 2. Deploy to game
./tools/deploy-mods.sh

# 3. Test in-game
- Character creation screen (all modes, genotypes, callings, castes)
- Skills screen
- Mutations screen
- Options menu
- General UI elements
```

---

## Benefits of Reorganization

### 1. Context-Based Organization

- **Before**: All glossaries mixed at root level
- **After**: Grouped by game screen/feature
- **Result**: Easier to find relevant files when translating specific screens

### 2. Reduced Duplication

- **Before**: `glossary_proto.json` duplicates GENOTYPES/SUBTYPES names
- **After**: Single source of truth in Layer 2 structured files
- **Result**: No conflicting translations, easier maintenance

### 3. Clearer Layer 1 vs Layer 2 Distinction

- **Before**: Both types mixed at root
- **After**: Subfolders clearly show Layer 1 (simple) vs Layer 2 (structured)
- **Result**: New translators understand structure faster

### 4. Improved Scalability

- **Before**: 12 root-level glossary files (hard to navigate)
- **After**: 3 context folders, each with clear purpose
- **Result**: Easy to add new categories (e.g., COMBAT/, WORLD/)

---

## Migration Checklist

### Pre-Migration

- [ ] Backup entire LOCALIZATION folder
- [ ] Document current file sizes and entry counts
- [ ] Run `project_tool.py` to capture baseline

### Migration

- [ ] Create new folder structure (Phase 1)
- [ ] Merge proto data into GENOTYPES/SUBTYPES (Phase 2)
- [ ] Rename and move files (Phase 3)
- [ ] Update code references in 5 files (Phase 4)
- [ ] Create subfolder READMEs (Phase 5)

### Post-Migration

- [ ] Run `python3 tools/project_tool.py` - should pass
- [ ] Deploy to game and test all screens
- [ ] Update `Docs/02_TODO.md` with completion
- [ ] Update `Docs/03_CHANGELOG.md`
- [ ] Delete `_DEPRECATED/glossary_proto.json` after validation

---

## Risk Assessment

### Low Risk

- Moving files (paths updated in code)
- Creating new folders and READMEs
- Renaming glossary files (pattern-based loading)

### Medium Risk

- Merging proto data into SUBTYPES/GENOTYPES
  - **Mitigation**: Validate JSON schema, test character creation screen thoroughly

### High Risk

- Deleting `glossary_proto.json`
  - **Mitigation**: Keep in `_DEPRECATED/` initially, delete only after 100% validation

---

## Estimated Time

- Phase 1 (Folders): 30 minutes
- Phase 2 (Proto merge): 2 hours (24 SUBTYPES + 2 GENOTYPES = 26 files)
- Phase 3 (Move files): 15 minutes
- Phase 4 (Code updates): 1 hour (5 files)
- Phase 5 (READMEs): 30 minutes
- Phase 6 (Testing): 1 hour

**Total**: ~5-6 hours

---

## Future Extensions

After this reorganization, the structure supports:

- `COMBAT/` - Combat-related translations (weapons, damage types)
- `WORLD/` - Locations, creatures, NPCs
- `QUESTS/` - Quest and conversation text
- `ITEMS/` - Item descriptions and categories

Each can follow the same Layer 1 (simple) + Layer 2 (structured) pattern.