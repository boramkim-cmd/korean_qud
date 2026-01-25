# Font Patch Tracker
# Version: 1.0 | Created: 2026-01-21
# Purpose: Track all UI screens requiring Korean font patches

---

## Overview

Caves of Qud uses TWO rendering systems:
| System | Technology | Korean Support | Solution |
|--------|------------|----------------|----------|
| **Modern UI** | TMP_Text (TextMeshPro) | ✅ via fallback | `EnsureFontFallback()` in TMP_Text.text setter |
| **Legacy UI** | ScreenBuffer → SpriteManager | ❌ 256-char limit | Force ModernUI OR TMP overlay |

---

## Global Font Solutions Applied

### 1. TMP_Text.text Setter Patch (Global)
- **File**: `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs`
- **Method**: `Patch_TMP_Text_Setter.Prefix()` → `EnsureFontFallback()`
- **Coverage**: ALL TMP_Text components
- **Status**: ✅ Working

### 2. FontManager Bi-directional Fallback
- **File**: `Scripts/00_Core/00_00_99_QudKREngine.cs`
- **Method**: `FontManager.ApplyKoreanFont()`
- **Coverage**: All TMP fonts at startup
- **Status**: ✅ Working

---

## Individual Screen Patches

### Legend
- ✅ Patched & Working
- 🔄 Patched, Needs Testing
- ❌ Not Patched
- ⚠️ Patched but Issues

---

### CHARACTER CREATION SCREENS

| Screen | Render Type | Status | Patch File | Notes |
|--------|-------------|--------|------------|-------|
| Main Title "character creation" | TMP (color tag) | ✅ | GlobalUI.cs | Color tag stripping required |
| Game Mode Selection | TMP | ✅ | Auto (global) | |
| Build Library | TMP | ✅ | Auto (global) | |
| Genotype Selection | TMP | ✅ | CharacterCreation.cs | |
| Subtype Selection | TMP | ✅ | CharacterCreation.cs | |
| Attributes Window | TMP | ✅ | CharacterCreation.cs | |
| Mutation Selection | TMP | ✅ | CharacterCreation.cs | |
| Cybernetics Selection | TMP | ✅ | CharacterCreation.cs | |

### WORLD GENERATION SCREENS

| Screen | Render Type | Status | Patch File | Notes |
|--------|-------------|--------|------------|-------|
| "Creating World" title | Legacy/TMP | 🔄 | WorldCreation.cs | ModernUI force + overlay |
| Progress messages | Legacy/TMP | 🔄 | WorldCreation.cs | Translation patch |
| Quote text | TMP | 🔄 | WorldCreation.cs | SetQuote patch |

### GAMEPLAY SCREENS

| Screen | Render Type | Status | Patch File | Notes |
|--------|-------------|--------|------------|-------|
| Main game console | Legacy | ❌ | - | Core gameplay, complex |
| Inventory | TMP | ✅ | Auto (global) | |
| Equipment | TMP | ✅ | Auto (global) | |
| Journal | TMP | ✅ | Auto (global) | |
| Map | Mixed | ❓ | - | Needs investigation |
| Conversation | TMP | ✅ | Auto (global) | |
| Trade | TMP | ✅ | Auto (global) | |

### MENU SCREENS

| Screen | Render Type | Status | Patch File | Notes |
|--------|-------------|--------|------------|-------|
| Main Menu | TMP | ✅ | MainMenu patches | |
| Options | TMP | ✅ | Options patches | |
| Keybindings | TMP | ✅ | Auto (global) | |
| Mods | TMP | ✅ | Auto (global) | |
| Credits | Legacy? | ❓ | - | Needs investigation |

---

## Patching Methods

### Method 1: Global TMP Fallback (Recommended for TMP screens)

**When to use**: Screen uses TMP_Text components

```csharp
// Already applied globally in Patch_TMP_Text_Setter
// No additional code needed for TMP screens
```

**Pros**: Automatic, no per-screen work
**Cons**: Only works for TMP_Text

---

### Method 2: Force ModernUI Option

**When to use**: Screen has both Modern and Legacy implementations

```csharp
[HarmonyPatch]
public static class Patch_Options_ModernUI
{
    private static bool _forceModernUI = false;
    
    public static void EnableForceModernUI() => _forceModernUI = true;
    public static void DisableForceModernUI() => _forceModernUI = false;
    
    static MethodBase TargetMethod()
    {
        var optionsType = AccessTools.TypeByName("XRL.UI.Options");
        var prop = optionsType.GetProperty("ModernUI", BindingFlags.Public | BindingFlags.Static);
        return prop?.GetGetMethod();
    }
    
    [HarmonyPostfix]
    static void Postfix(ref bool __result)
    {
        if (_forceModernUI) __result = true;
    }
}

// Usage in target screen patch:
[HarmonyPrefix]
static void ScreenShow_Prefix()
{
    Patch_Options_ModernUI.EnableForceModernUI();
}

[HarmonyPostfix]
static void ScreenHide_Postfix()
{
    Patch_Options_ModernUI.DisableForceModernUI();
}
```

**Pros**: Uses game's existing Modern UI
**Cons**: Only works if Modern UI exists for that screen

---

### Method 3: TMP Overlay (For Legacy-only screens)

**When to use**: Screen ONLY has Legacy implementation (ScreenBuffer)

```csharp
private static GameObject _overlayCanvas;
private static TextMeshProUGUI _overlayText;

private static void CreateOverlay()
{
    // 1. Create Canvas
    _overlayCanvas = new GameObject("KoreanOverlay");
    var canvas = _overlayCanvas.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = 9999; // On top of everything
    
    _overlayCanvas.AddComponent<CanvasScaler>();
    _overlayCanvas.AddComponent<GraphicRaycaster>();
    
    // 2. Create TMP Text
    var textObj = new GameObject("Text");
    textObj.transform.SetParent(_overlayCanvas.transform, false);
    
    _overlayText = textObj.AddComponent<TextMeshProUGUI>();
    _overlayText.fontSize = 24;
    _overlayText.alignment = TextAlignmentOptions.Center;
    
    // 3. Position (adjust per screen)
    var rect = textObj.GetComponent<RectTransform>();
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.sizeDelta = new Vector2(800, 100);
    
    // 4. Apply Korean font
    if (FontManager.IsFontLoaded)
    {
        var koreanFont = FontManager.GetKoreanFont();
        if (koreanFont != null && _overlayText.font != null)
        {
            if (_overlayText.font.fallbackFontAssetTable == null)
                _overlayText.font.fallbackFontAssetTable = new List<TMP_FontAsset>();
            _overlayText.font.fallbackFontAssetTable.Insert(0, koreanFont);
        }
    }
}

public static void DestroyOverlay()
{
    if (_overlayCanvas != null)
    {
        UnityEngine.Object.Destroy(_overlayCanvas);
        _overlayCanvas = null;
        _overlayText = null;
    }
}
```

**Pros**: Works for any screen
**Cons**: Complex, need to match positioning, may conflict with game UI

---

### Method 4: ScreenBuffer Interception (Advanced)

**When to use**: Need to intercept text BEFORE it goes to ScreenBuffer

```csharp
[HarmonyPatch(typeof(ScreenBuffer), nameof(ScreenBuffer.Write))]
public static class Patch_ScreenBuffer_Write
{
    [HarmonyPrefix]
    static void Prefix(ref string Text)
    {
        // Translate text before it reaches the 256-char limit
        if (ContainsKorean(Text))
        {
            // Option 1: Replace with ASCII approximation
            // Option 2: Flag for overlay rendering
            // Option 3: Skip and render separately
        }
    }
}
```

**Pros**: Catches all ScreenBuffer text
**Cons**: Korean still can't render in ScreenBuffer, only useful for flagging/routing

---

## ⚠️ Critical Warnings

### 1. Never Modify Data Fields
```csharp
// ❌ BAD - Crashes game
__instance.data.SomeField = "한글";

// ✅ GOOD - Only modify UI elements
__instance.textComponent.text = "한글";
```

### 2. Check Font Loading State
```csharp
// ❌ BAD - May crash if font not loaded
var font = FontManager.GetKoreanFont();
text.font = font;

// ✅ GOOD - Always check
if (FontManager.IsFontLoaded)
{
    var font = FontManager.GetKoreanFont();
    if (font != null) { /* use font */ }
}
```

### 3. Clean Up Overlays
```csharp
// ❌ BAD - Memory leak
void OnScreenShow() { CreateOverlay(); }

// ✅ GOOD - Always clean up
void OnScreenShow() { CreateOverlay(); }
void OnScreenHide() { DestroyOverlay(); }
```

### 4. Handle Color Tags
```csharp
// ❌ BAD - Won't match
if (text == "character creation") // Actual: "<color=#CFC041FF>character creation </color>"

// ✅ GOOD - Strip tags for matching
string stripped = Regex.Replace(text, "<[^>]+>", "").Trim();
if (stripped == "character creation") { /* translate */ }
```

---

## Success Cases

### Case 1: "character creation" Header
- **Problem**: Text wrapped in `<color>` tags, not matching
- **Solution**: Regex strip tags, match content, replace preserving tags
- **Result**: ✅ "캐릭터 생성" displays correctly
- **File**: `02_10_00_GlobalUI.cs`

### Case 2: Tooltip Font
- **Problem**: Tooltips not showing Korean
- **Solution**: Dedicated `Patch_Tooltip_Awake` adding fallback
- **Result**: ✅ Tooltips render Korean
- **File**: `02_10_02_Tooltip.cs`

### Case 3: Attribute Window
- **Problem**: Attribute names truncated (game uses Substring)
- **Solution**: Translate ONLY UI display text, not data fields
- **Result**: ✅ Full Korean names display
- **File**: `02_10_10_CharacterCreation.cs`

---

## Failure Cases & Lessons

### Case 1: ERR-008 Attribute Crash
- **Problem**: Translated `attr.Attribute` data field
- **Cause**: Game code does `Attribute.Substring(0,3)` for abbreviation
- **Lesson**: NEVER modify data fields, only UI text elements
- **Solution**: Patch display method postfix, not data

### Case 2: ERR-013 Caste Selection
- **Problem**: `NormalizeLine()` broke with Korean
- **Cause**: String processing assumed ASCII
- **Lesson**: Check game's string processing methods
- **Solution**: Translate after normalization

### Case 3: Creating World Font
- **Problem**: Korean shows as boxes/missing
- **Cause**: Legacy ScreenBuffer uses 256-char sprite atlas
- **Lesson**: Identify render system BEFORE patching
- **Solution**: Force ModernUI or TMP overlay

---

## Investigation Checklist for New Screens

When patching a new screen:

- [ ] **Identify render system**: TMP_Text or ScreenBuffer?
  ```bash
  grep -r "ScreenName" Assets/core_source/ | grep -i "tmp\|text\|buffer"
  ```

- [ ] **Check for ModernUI variant**: Does `Options.ModernUI` affect it?
  ```bash
  grep -r "ModernUI" Assets/core_source/ | grep -i "screenname"
  ```

- [ ] **Find all text sources**: Hardcoded? XML? Data field?
  ```bash
  grep -rn "\"TextToTranslate\"" Assets/core_source/
  ```

- [ ] **Check string processing**: Any Substring/Split/Trim calls?
  ```bash
  grep -A5 -B5 "FieldName" Assets/core_source/ClassName.cs
  ```

- [ ] **Test with Korean**: Does it display? Crash? Truncate?

- [ ] **Document in this file**: Update the screen table above

---

## TODO: Screens Needing Investigation

1. [ ] Main game console (core gameplay text)
2. [ ] Map screen labels
3. [ ] Credits screen
4. [ ] Loading screen tips
5. [ ] Death screen
6. [ ] Achievement popups
7. [ ] Steam overlay integration

---

## Related Files

- `Scripts/00_Core/00_00_99_QudKREngine.cs` - FontManager
- `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs` - Global TMP patch
- `Scripts/02_Patches/10_UI/02_10_11_WorldCreation.cs` - World creation patches
- `Docs/reference/03_ERROR_LOG.md` - Error history

---

## Changelog

| Date | Change |
|------|--------|
| 2026-01-21 | Created document |
| 2026-01-21 | Added WorldCreation patches (Methods 2, 3) |
| 2026-01-21 | Documented "character creation" success case |
