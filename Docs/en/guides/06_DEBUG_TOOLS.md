# Caves of Qud - Debug Tools Reference for Translation Testing

> **Version**: 1.0 | **Created**: 2026-01-22
> **Purpose**: Document all in-game debugging tools useful for testing object/creature translations

---

## Table of Contents
1. [Enabling Debug/Wish Mode](#1-enabling-debugwish-mode)
2. [Wish Command System](#2-wish-command-system)
3. [Spawning Objects/Creatures](#3-spawning-objectscreatures)
4. [Debug Options](#4-debug-options)
5. [Game Logging](#5-game-logging)
6. [Look/Examine System](#6-lookexamine-system)
7. [Custom Debug Tools for Mod](#7-custom-debug-tools-for-mod)
8. [Recommended Workflow](#8-recommended-workflow)

---

## 1. Enabling Debug/Wish Mode

### How to Access Wish Menu
The wish system is ALWAYS available in Caves of Qud - no special mode needed.

**Access Methods:**
1. Press **Ctrl+W** (default keybind) during gameplay
2. Type your wish command in the popup dialog
3. Press Enter to execute

### Relevant Game Settings (Options.cs)
| Option | Description |
|--------|-------------|
| `DebugInternals` | Shows internal object IDs in Look mode (press N) |
| `DebugShowConversationNode` | Shows conversation node IDs |
| `DebugDamagePenetrations` | Shows damage calculation details |
| `DebugSavingThrows` | Shows saving throw details |
| `DebugStatShift` | Shows stat modification details |
| `DebugAttitude` | Shows faction attitude calculations |
| `HarmonyDebug` | Enables Harmony mod debug logging |

**Note:** These can be toggled via wish commands (see below).

---

## 2. Wish Command System

### Core Architecture
- **WishManager** (`XRL.Wish.WishManager`) - Handles wish command routing
- **Wishing.cs** (`XRL.World.Capabilities.Wishing`) - Contains built-in wish commands (~4500 lines!)
- **WishSearcher** (`XRL.WishSearcher`) - Fuzzy search for blueprints/zones/quests

### Wish Command Format
```
command                    # Simple command
command:parameter          # With parameter
command:param1:param2      # Multiple parameters
command param              # Space-separated (some commands)
```

### How Mods Can Add Custom Wish Commands
```csharp
using XRL.Wish;

[HasWishCommand]  // Class attribute required
public class MyWishHandler
{
    [WishCommand(Command = "mycommand")]  // Method attribute
    public static void HandleMyCommand()
    {
        // Handle wish
    }
    
    [WishCommand(Command = "mycommand")]  // With parameter
    public static void HandleWithParam(string param)
    {
        // param = text after ":"
    }
    
    [WishCommand(Regex = @"^mypattern\s+(.*)$")]  // Regex matching
    public static void HandleRegex(System.Text.RegularExpressions.Match match)
    {
        // Custom parsing
    }
}
```

---

## 3. Spawning Objects/Creatures

### Primary Spawn Commands

| Command | Description | Example |
|---------|-------------|---------|
| `<blueprint>` | Spawn by blueprint name (fuzzy search) | `snapjaw` → spawns Snapjaw |
| `item:<name>` | Spawn item specifically | `item:dagger` |
| `item:<name>:<count>` | Spawn multiple items | `item:lead slug:100` |
| `item:<name>:here` | Spawn at player's cell | `item:torch:here` |
| `testhero:<blueprint>` | Spawn as hero (named + buffed) | `testhero:Goatfolk` |
| `testwarden:<blueprint>` | Spawn as warden NPC | `testwarden:Goatfolk` |

### Blueprint Search (WishSearcher)
- Uses **Levenshtein distance** for fuzzy matching
- Searches both `Blueprint.Name` and `DisplayName`
- Prioritizes "real" objects over base objects
- Case insensitive

### Test Display Name Issues
```
# Spawn and check display name
snapjaw
# Then press L (Look) to examine

# Check for [brackets] in names
testpets     # Tests all creature DisplayNames
testobjects  # Tests all object DisplayNames
```

### Zone/Location Wishes
| Command | Description |
|---------|-------------|
| `<zone_name>` | Teleport to zone (fuzzy) |
| `goto:<zone_id>` | Teleport to exact zone ID |
| `xy` | Show current coordinates |
| `where` | Show current zone info |
| `zonenamedata` | Show zone name details |

---

## 4. Debug Options

### Toggle Debug Options via Wish
Many options can be toggled with `option:<name>` or `setoption:<name>:<value>`

### Useful Debug-Related Wishes

| Wish | Description |
|------|-------------|
| `DebugInternals` | Toggle showing internal IDs |
| `what` | Show info about objects at cursor |
| `blueprint` | Pick direction, show blueprint name |
| `showcharset` | Display character set (font testing!) |
| `testmarkup` | Test color markup rendering |

### Internal Data Wishes
| Wish | Description |
|------|-------------|
| `showstringproperty:<name>` | Show string property on player |
| `showintproperty:<name>` | Show int property on player |
| `getstringgamestate:<name>` | Get game state string |
| `getboolgamestate:<name>` | Get game state bool |
| `checkforpart:<partname>` | Check if player has part |

---

## 5. Game Logging

### Log File Locations
| Platform | Path |
|----------|------|
| macOS | `~/Library/Logs/Freehold Games/CavesOfQud/Player.log` |
| Windows | `%APPDATA%\..\LocalLow\Freehold Games\CavesOfQud\Player.log` |
| Linux | `~/.config/unity3d/Freehold Games/CavesOfQud/Player.log` |

### Real-time Log Monitoring
```bash
# macOS
tail -f ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log

# Filter mod logs only
tail -f ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | grep "\[Qud-KR"
```

### Mod Logging (Current Implementation)
```csharp
// From 00_ModEntry.cs
Debug.Log("[Qud-KR Translation] message");
Debug.LogWarning("[Qud-KR Translation] warning");
Debug.LogError("[Qud-KR Translation] error");
```

### Log Tags in Use
- `[Qud-KR Translation]` - General mod messages
- `[Qud-KR]` - Short prefix for frequent logs
- `[Qud-KR][TMPFont]` - Font system logs

---

## 6. Look/Examine System

### Look Mode (Press L)
- Shows object `DisplayName` 
- Shows `Description._Short` (long description)
- Shows difficulty level
- Shows faction feelings

### Key Information Displayed
From `Look.cs`:
```csharp
// Display name with colors
gameObject.GetDisplayName(...);

// Long description
description.GetLongDescription(LookSB);

// Difficulty
description.GetDifficultyDescription();

// Feelings (faction)
description.GetFeelingDescription();
```

### Debug Internals Mode
When `Options.DebugInternals` is enabled:
- Press **N** in Look mode to show navigation weights
- Shows internal IDs in some UIs

### GetDisplayNameEvent
The display name is constructed through `GetDisplayNameEvent`:
```csharp
// Event flow
GetDisplayNameEvent.GetFor(Object, Base, ...)
// -> Object.HandleEvent(GetDisplayNameEvent)
// -> All parts can modify the name
```

---

## 7. Custom Debug Tools for Mod

### Proposed Custom Wish Commands

```csharp
[HasWishCommand]
public static class KoreanTranslationDebugWishes
{
    // Show translation status for object
    [WishCommand(Command = "kr:check")]
    public static void CheckTranslation(string blueprintName)
    {
        var result = WishSearcher.SearchForBlueprint(blueprintName);
        var obj = GameObjectFactory.Factory.CreateObject(result.Result);
        
        Debug.Log($"[Qud-KR Debug] Blueprint: {obj.Blueprint}");
        Debug.Log($"[Qud-KR Debug] DisplayName: {obj.DisplayName}");
        Debug.Log($"[Qud-KR Debug] DisplayNameStripped: {obj.DisplayNameStripped}");
        
        var desc = obj.GetPart<Description>();
        if (desc != null)
        {
            Debug.Log($"[Qud-KR Debug] Description._Short: {desc._Short}");
        }
        
        // Check if translation exists
        // ... lookup in LocalizationManager
    }
    
    // Dump all untranslated objects in zone
    [WishCommand(Command = "kr:untranslated")]
    public static void ListUntranslated()
    {
        var zone = The.Player.CurrentZone;
        foreach (var obj in zone.GetObjects())
        {
            if (!IsTranslated(obj.DisplayName))
            {
                Debug.Log($"[Qud-KR] Untranslated: {obj.Blueprint} -> {obj.DisplayName}");
            }
        }
    }
    
    // Show translation lookup path
    [WishCommand(Command = "kr:trace")]
    public static void TraceTranslation(string key)
    {
        Debug.Log($"[Qud-KR Debug] Tracing translation for: {key}");
        // Log which JSON files are checked
        // Log if found and what the translation is
    }
    
    // Reload all translation JSON
    [WishCommand(Command = "kr:reload")]
    public static void ReloadTranslations()
    {
        LocalizationManager.Initialize();
        Debug.Log("[Qud-KR] Translations reloaded!");
    }
    
    // Toggle verbose translation logging
    [WishCommand(Command = "kr:verbose")]
    public static void ToggleVerbose()
    {
        // Toggle a flag that enables detailed logging
    }
}
```

### Implementation Priority
1. **kr:reload** - Hot-reload translations without restarting game (HIGH)
2. **kr:check:<name>** - Check translation status for specific object (HIGH)
3. **kr:verbose** - Toggle detailed logging (MEDIUM)
4. **kr:untranslated** - List missing translations in zone (MEDIUM)
5. **kr:trace:<key>** - Debug translation lookup (LOW)

---

## 8. Recommended Workflow

### Testing Object/Creature Translations

```
1. Start game with mod loaded
2. Create new character (or load existing)
3. Open terminal for log monitoring:
   tail -f ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | grep "\[Qud-KR"

4. Spawn test object:
   Ctrl+W → "snapjaw" → Enter
   
5. Look at object:
   Press L → move cursor to object
   
6. Check displayed name and description

7. If issues found:
   - Note the Blueprint name
   - Check JSON files for translation
   - Check log for any errors
```

### Translation Verification Checklist
- [ ] DisplayName shows Korean
- [ ] Description shows Korean  
- [ ] No `[Untranslated]` brackets
- [ ] Color tags preserved correctly
- [ ] No duplicate color tags
- [ ] Font renders Korean characters

### Quick Test Commands
```
# Test basic creature
snapjaw

# Test item
dagger

# Test with specific blueprint
item:Steel Long Sword

# Test multiple
item:torch:5

# Check all objects for issues
testobjects

# Check all creatures for issues  
testpets
```

---

## Appendix: Complete Wish Command Reference

### Object Testing
| Command | Description |
|---------|-------------|
| `<name>` | Spawn object (fuzzy search) |
| `item:<name>` | Spawn item |
| `item:<name>:<count>` | Spawn multiple |
| `testpets` | Test all creature DisplayNames |
| `testobjects` | Test all object DisplayNames |
| `blueprint` | Show blueprint of adjacent object |

### Quest/Debug
| Command | Description |
|---------|-------------|
| `questdebug` | Quest debug menu |
| `what` | Info about cursor location |
| `where` | Current zone info |

### Character
| Command | Description |
|---------|-------------|
| `xp:<amount>` | Give XP |
| `levelup` | Level up |
| `skill:<name>` | Add skill |
| `mutation:<name>` | Add mutation |

### Utility
| Command | Description |
|---------|-------------|
| `reload` | Hotload configuration |
| `rebuild` | Rebuild current zone |
| `showcharset` | Show character set |

---

## Related Files

- [Wishing.cs](../../../Assets/core_source/GameSource/XRL.World.Capabilities/Wishing.cs) - Main wish handler (~4500 lines)
- [WishManager.cs](../../../Assets/core_source/GameSource/XRL.Wish/WishManager.cs) - Wish routing
- [WishSearcher.cs](../../../Assets/core_source/GameSource/XRL/WishSearcher.cs) - Blueprint search
- [Look.cs](../../../Assets/core_source/GameSource/XRL.UI/Look.cs) - Look mode UI
- [Options.cs](../../../Assets/core_source/GameSource/XRL.UI/Options.cs) - Debug options
