# File Structure and Deployment Guide for Qud-KR Translation

This document is automatically generated to assist other AI agents and developers in understanding the file structure and deployment mechanism of the Qud-KR Translation mod.

## 1. Directory Structure

The mod is developed in an external workspace (`/Users/ben/Desktop/qud_korean`) and deployed to the game's mod directory (`/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/QudKR_Translation`).

### Source Workspace (`/Users/ben/Desktop/qud_korean`)
The root directory for development.

- **Data_QudKRContent/**: The main content folder that mirrors the structure of the deployed mod.
    - **Scripts/**: C# source code.
        - **00_Core/**: Core system logic (Entry point, Translation Engine, Scope Manager).
        - **01_Data/**: Translation data (Dictionary<string, string>).
        - **02_Patches/**: Harmony patches (UI, Core).
        - **99_Utils/**: Utility scripts.
    - **Docs/**: Documentation (Development, Bug Reports).
    - **mod_info.json**: Metadata for the mod (Title, Version, Description).
    - **README.md**: General information.
    - **AUTO_SYNC.md**: Guide for auto-sync feature.
    - **CHANGELOG.md**: History of changes.
    - **GITHUB_SETUP.md**: Git configuration guide.
    - **INSTALLATION.md**: Installation instructions.
    - **KNOWN_ISSUES.md**: List of known bugs and limitations.
    - **quick-save.sh**: Helper script for quick git commits.
    - **sync.sh**: Script for manually syncing files to the game directory.

- **Assets/**: Decompiled game source code (Reference only).
    - **core_source/**: Decompiled C# classes from `Assembly-CSharp.dll`.
    - **StreamingAssets/**: Game data XMLs and blueprints.

- **HarmonyAnalyzer/**: Tool for analyzing Harmony patch targets (Development only).

- **Docs/**: Project-level documentation (Development process, API reference).

- **watch-and-sync.sh**: **CRITICAL**. This script monitors `Data_QudKRContent` and automatically deploys changes to the game mod directory in real-time.

### Deployed Mod Directory (`.../Mods/QudKR_Translation`)
Only the contents of `Data_QudKRContent` are deployed here.

- **mod_info.json**: Required by the game to recognize the mod.
- **Scripts/**: Compiled by the game at runtime.
    - **00_Core/**
    - **01_Data/**
    - **02_Patches/**
    - **99_Utils/**
- **Docs/**: Included for reference.
- **README.md**, **INSTALLATION.md**, etc.

## 2. Deployment Mechanism

We use a **selective synchronization** approach. We do not copy the entire workspace.

### The `watch-and-sync.sh` Script
This script uses `fswatch` (or a fallback loop) to monitor changes in `/Users/ben/Desktop/qud_korean/Data_QudKRContent`.

**Rules:**
1. **Source**: `/Users/ben/Desktop/qud_korean/Data_QudKRContent/`
2. **Destination**: `/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/QudKR_Translation/`
3. **Behavior**:
    - When a file in `Data_QudKRContent` is modified, it is immediately copied to the destination.
    - Files outside `Data_QudKRContent` (e.g., `Assets/`, `Docs/` in root, root scripts) are **NOT** deployed.
    - This ensures only necessary mod files are present in the game's directory, keeping it clean.

### What is Copied?
- `Scripts/**/*.cs` (Logic, Data, Patches)
- `mod_info.json` (Metadata)
- `Docs/**/*.md` (Documentation)
- `*.md` inside `Data_QudKRContent`

### What is NOT Copied?
- `Assets/` (Original game code)
- `HarmonyAnalyzer/` (Dev tools)
- `watch-and-sync.sh` (Deployment script itself)
- `validate-mod.sh` (Correction script)

## 3. Key File Paths for Agents

- **Mod Entry Point**: `Data_QudKRContent/Scripts/00_Core/00_ModEntry.cs`
- **Translation Engine**: `Data_QudKRContent/Scripts/00_Core/01_TranslationEngine.cs`
- **Main Menu Data**: `Data_QudKRContent/Scripts/01_Data/MainMenu.cs`
- **Main Menu Patch**: `Data_QudKRContent/Scripts/02_Patches/UI/MainMenu_Patch.cs`
- **Game Logs**: `/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/game_log.txt` (or `Player.log` on some setups)

## 4. How to Verify Deployment

1. Check if `watch-and-sync.sh` is running.
2. Modify a file in `Data_QudKRContent`.
3. Check the timestamp of the corresponding file in the game's mod directory.
4. Check `game_log.txt` for "[QudKR]" logs after restarting the game.
