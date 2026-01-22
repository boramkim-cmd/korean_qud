# Code Analysis Report - Character Creation & Main Menu
**Date**: 2026-01-19  
**Analyst**: AI Code Review  
**Scope**: Main Menu, Character Creation screens, Core Translation Engine  
**Total Lines Analyzed**: ~2,966 lines

---

## Executive Summary

A comprehensive code analysis was performed on the Korean localization mod focusing on main menu and character creation screens. **16 potential issues** were identified:

| Severity | Count | Status |
|----------|-------|--------|
| 🔴 Critical | 3 | Requires immediate attention |
| 🟠 High | 4 | Should fix before next release |
| 🟡 Medium | 5 | Gradual improvement recommended |
| 🟢 Low | 4 | Minor improvements |

**Overall Code Health Score: 6.5/10**

---

## Files Analyzed

| File | Lines | Description |
|------|-------|-------------|
| `Scripts/00_Core/00_00_00_ModEntry.cs` | 92 | Mod entry point |
| `Scripts/00_Core/00_00_01_TranslationEngine.cs` | 176 | Tag preservation/restoration |
| `Scripts/00_Core/00_00_02_ScopeManager.cs` | 94 | Screen-based scope management |
| `Scripts/00_Core/00_00_03_LocalizationManager.cs` | 463 | JSON loading/search |
| `Scripts/00_Core/00_00_05_FontManager.cs` | 206 | Font manager & Korean utils |
| `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs` | 676 | Character creation patches |
| `Scripts/02_Patches/10_UI/02_10_00_MainMenu.cs` | 353 | Main menu & popup patches |
| `Scripts/02_Patches/10_UI/02_10_01_ChargenBackNextButton.cs` | 34 | Back/Next button patches |
| `Scripts/02_Patches/10_UI/02_10_02_GlobalTextRendering.cs` | 62 | Global text rendering patches |
| `Scripts/02_Patches/10_UI/02_10_03_UITextPatches.cs` | 36 | TMPro-based UI text patches |
| `Scripts/99_Utils/99_00_02_SafeTranslationUtils.cs` | 93 | Safe translation utilities |
| `Scripts/99_Utils/99_00_04_ChargenTranslationUtils.cs` | 216 | Chargen translation utilities |
| `Scripts/99_Utils/99_00_03_StructureTranslator.cs` | 465 | Mutation/Genotype/Subtype translation |

---

## 🔴 Critical Issues

### Issue 1: Unconditional ScopeManager.ClearAll() in Main Menu

**File:** `Scripts/02_Patches/10_UI/02_10_00_MainMenu.cs`  
**Location:** `Show_Prefix()` method  
**Severity:** 🔴 Critical

**Problem:**
`ScopeManager.ClearAll()` is called unconditionally before pushing a new scope. If another screen's scope was legitimately active (e.g., popup over game), this will corrupt the scope stack.

```csharp
// Current code:
ScopeManager.ClearAll();
_scopePushed = false;
```

**Impact:** 
- Scope corruption when transitioning between screens
- Translations from wrong category could be applied

**Recommended Fix:**
```csharp
// Only clear if depth is unexpectedly high (recovery mechanism)
if (ScopeManager.GetDepth() > 3) {
    ScopeManager.ClearAll();
    Debug.LogWarning("[Qud-KR] Scope stack was unexpectedly deep, cleared");
}
```

---

### Issue 2: Static _scopePushed Flag Without Pop Handling

**File:** `Scripts/02_Patches/10_UI/02_10_00_MainMenu.cs`  
**Location:** `Hide_Prefix()` method  
**Severity:** 🔴 Critical

**Problem:**
The `_scopePushed` static flag is set to true when pushing, but `Hide_Prefix()` does NOT pop the scope or reset the flag. The `ScopeManager.PopScope()` call is commented out.

```csharp
// Current code:
[HarmonyPrefix]
static void Hide_Prefix()
{
    // ScopeManager.PopScope(); 
    // -> 메인 메뉴가 최상위이므로 보통 놔둬도 됨...
}
```

**Impact:**
- Scope remains on stack permanently
- Causes unintended translations in other screens

**Recommended Fix:**
```csharp
[HarmonyPrefix]
static void Hide_Prefix()
{
    if (_scopePushed) {
        ScopeManager.PopScope();
        _scopePushed = false;
    }
}
```

---

### Issue 3: Data Field Modification in QudGenotypeModuleWindow

**File:** `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`  
**Location:** `QudGenotypeModuleWindow` patch  
**Severity:** 🔴 Critical

**Problem:**
The patch modifies `genotype.DisplayName` and `genotype.ExtraInfo` directly, which are **data** fields, not UI fields. This violates the "UI-Only Postfix Pattern" principle documented in `.github/copilot-instructions.md`.

```csharp
// Current code (DANGEROUS):
if (!string.IsNullOrEmpty(data.KoreanName))
    genotype.DisplayName = data.KoreanName;  // Data field modification!

if (data.LevelTextKo != null && data.LevelTextKo.Count > 0)
    genotype.ExtraInfo = new List<string>(data.LevelTextKo);  // Data field modification!
```

**Impact:**
- Could corrupt game data if saved/cached
- Game code expecting English values will fail
- Violates core project principle

**Recommended Fix:**
Only modify UI display elements, not the underlying data model. Use UI element text properties in Postfix patches.

---

## 🟠 High Issues

### Issue 4: ChoiceWithColorIcon Id Field Not Explicitly Protected

**File:** `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`  
**Location:** Multiple choice translation sections  
**Severity:** 🟠 High

**Problem:**
While `Title` is translated correctly, the code lacks explicit protection for the `Id` field. The `copilot-instructions.md` specifically lists `ChoiceWithColorIcon.Id` as a DANGEROUS FIELD.

```csharp
// Current code:
foreach (var choice in list)
{
    if (LocalizationManager.TryGetAnyTerm(choice.Title?.ToLowerInvariant(), out string tTitle, "chargen_mode", "chargen_ui"))
        choice.Title = tTitle;  // OK
    // No explicit assertion that Id is never touched
}
```

**Recommended Fix:**
```csharp
// Add explicit comment and defensive assertion
foreach (var choice in list)
{
    // CRITICAL: NEVER modify choice.Id - used for selection logic (see ERR-008)
    string originalId = choice.Id;  // Cache for verification
    
    if (LocalizationManager.TryGetAnyTerm(choice.Title?.ToLowerInvariant(), out string tTitle, "chargen_mode", "chargen_ui"))
        choice.Title = tTitle;
    
    Debug.Assert(choice.Id == originalId, "Id field was modified - this will cause crashes!");
}
```

---

### Issue 5: Traverse Field Access Without Existence Check

**File:** Multiple files  
**Location:** Various Harmony patches  
**Severity:** 🟠 High

**Problem:**
The code uses `Traverse.Create(obj).Field<T>("FieldName").Value` extensively without checking if the field exists. This bypasses validation and will silently fail on game updates.

```csharp
// Current code:
var tr = Traverse.Create(choice);
string desc = tr.Field<string>("Description").Value;
if (!string.IsNullOrEmpty(desc))
{
    tr.Field<string>("Description").Value = ChargenTranslationUtils.TranslateLongDescription(...);
}
```

**Impact:**
- Silent failure if field name changes in game update
- No error indication for debugging

**Recommended Fix:**
```csharp
var tr = Traverse.Create(choice);
var descField = tr.Field<string>("Description");
if (descField.FieldExists() && !string.IsNullOrEmpty(descField.Value))
{
    descField.Value = ChargenTranslationUtils.TranslateLongDescription(...);
}
```

---

### Issue 6: Stat Translation Format Change

**File:** `Scripts/99_Utils/99_00_04_ChargenTranslationUtils.cs`  
**Location:** `TranslateLongDescription()` method  
**Severity:** 🟠 High

**Problem:**
The stat translation pattern changes the format from English order (`+2 Agility`) to Korean order (`민첩 +2`). This format change could break any game code that parses stat strings.

```csharp
// Current transformation:
// English: "+2 Agility"
// Korean:  "민첩 +2" (order reversed!)
lines[i] = prefix + uiPrefix + bulletPrefix + translatedText + " " + numberPart + uiSuffix;
```

**Impact:**
- Game code parsing stat strings expecting "+N Text" format will fail
- Could cause issues with stat calculations or display

**Recommended Fix:**
Either preserve original format or thoroughly verify no game code depends on parsing these strings:
```csharp
// Option 1: Preserve English format (safer)
lines[i] = prefix + uiPrefix + bulletPrefix + numberPart + " " + translatedText + uiSuffix;

// Option 2: Document as intentional if tested safe
// Add comment: "Format intentionally changed for Korean grammar - verified safe"
```

---

### Issue 7: Starting Location Data Field Modification

**File:** `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`  
**Location:** `StartingLocationModuleWindow` patch  
**Severity:** 🟠 High

**Problem:**
`StartingLocation.Name` is modified directly in the patch, modifying game data instead of just UI display.

```csharp
// Current code:
if (LocalizationManager.TryGetAnyTerm(loc.Name?.ToLowerInvariant(), out string tName, "chargen_location"))
    loc.Name = tName;  // DANGEROUS: data field modification
```

**Impact:**
- Could affect save/load systems
- Other systems referencing location by name may fail

**Recommended Fix:**
Only modify UI display text in Postfix patches, not the underlying data model.

---

## 🟡 Medium Issues

### Issue 8: Null Check Before ToLowerInvariant()

**File:** `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`  
**Location:** Multiple locations  
**Severity:** 🟡 Medium

**Problem:**
Uses null-conditional `?.ToLowerInvariant()` which returns null if source is null, then passes null to dictionary lookup.

```csharp
// Current code:
if (LocalizationManager.TryGetAnyTerm(choice.Title?.ToLowerInvariant(), out string tTitle, ...))
```

**Recommended Fix:**
```csharp
if (!string.IsNullOrEmpty(choice.Title) && 
    LocalizationManager.TryGetAnyTerm(choice.Title.ToLowerInvariant(), out string tTitle, ...))
```

---

### Issue 9: Duplicate Color Tag Normalization

**File:** `TranslationEngine.cs` AND `LocalizationManager.cs`  
**Severity:** 🟡 Medium

**Problem:**
Color tag normalization (`{{C|...}}` → `{{c|...}}`) is performed in both files, causing redundant regex operations.

**Recommended Fix:**
Centralize normalization to a single utility method in `TranslationEngine.cs`.

---

### Issue 10: Case-Sensitive Bullet Prefix Check

**File:** `Scripts/99_Utils/99_00_03_StructureTranslator.cs`  
**Location:** `CombineWithLevelText()` method  
**Severity:** 🟡 Medium

**Problem:**
Bullet detection only checks lowercase `{{c|ù}}` but not uppercase `{{C|ù}}`.

```csharp
// Current code:
bool hasBullet = line.StartsWith("{{c|ù}}") ||   // lowercase only!
                 line.StartsWith("ù") || ...
```

**Impact:**
- Uppercase color-tagged bullets will get duplicate bullet prefix

**Recommended Fix:**
```csharp
bool hasBullet = line.StartsWith("{{c|ù}}", StringComparison.OrdinalIgnoreCase) ||
                 line.StartsWith("ù") || ...
```

---

### Issue 11: Silent Exception Swallowing

**File:** `Scripts/02_Patches/10_UI/02_10_03_UITextPatches.cs`  
**Severity:** 🟡 Medium

**Problem:**
```csharp
catch { }  // Silent swallow - no logging
```

**Recommended Fix:**
```csharp
catch (Exception ex) {
    #if DEBUG
    Debug.LogWarning($"[Qud-KR TMP_Text Patch] {ex.Message}");
    #endif
}
```

---

### Issue 12: TargetMethod Returns Null Without Logging

**File:** `Scripts/02_Patches/10_UI/02_10_00_MainMenu.cs`  
**Severity:** 🟡 Medium

**Problem:**
If neither `Qud.UI.MainMenu` nor `XRL.UI.MainMenu` exists, `TargetMethod()` returns null silently.

**Recommended Fix:**
```csharp
static MethodBase TargetMethod()
{
    var type = AccessTools.TypeByName("Qud.UI.MainMenu") ?? AccessTools.TypeByName("XRL.UI.MainMenu");
    if (type == null) {
        Debug.LogError("[Qud-KR] MainMenu type not found! Translation will not work.");
        return null;
    }
    return AccessTools.Method(type, "Show");
}
```

---

## 🟢 Low Issues

### Issue 13: Hardcoded Critical Type Names

**File:** `Scripts/00_Core/00_00_00_ModEntry.cs`  
**Severity:** 🟢 Low

**Problem:**
Critical type names are hardcoded strings which could break on game updates.

**Recommended Fix:**
Add version compatibility check or make types configurable.

---

### Issue 14: FontManager.ApplyKoreanFont() is Empty

**File:** `Scripts/00_Core/00_00_05_FontManager.cs`  
**Severity:** 🟢 Low

**Problem:**
Font application is disabled but the method is still called, causing unnecessary log spam.

**Recommended Fix:**
Remove the calls or use a feature flag to conditionally skip.

---

### Issue 15: Unrelated SteamGalaxyPatch Included

**File:** `Scripts/02_Patches/10_UI/02_10_00_MainMenu.cs`  
**Severity:** 🟢 Low

**Problem:**
A Steam/Galaxy platform detection patch is included which is unrelated to localization.

**Recommended Fix:**
Remove this patch from the localization mod or move to a separate utility mod.

---

### Issue 16: RecursiveJsonParser Missing Unicode Escape Handling

**File:** `Scripts/00_Core/00_00_03_LocalizationManager.cs`  
**Severity:** 🟢 Low

**Problem:**
The custom JSON parser handles `\n`, `\r`, `\t` but not `\uXXXX` unicode escapes.

**Recommended Fix:**
Add unicode escape handling or migrate to a proper JSON library.

---

## Risk Assessment

| Risk Category | Level | Justification |
|--------------|-------|---------------|
| Crash Risk | Medium-High | Data field modifications may cause unexpected behavior |
| Data Corruption | Medium | Scope stack corruption, direct data modification |
| Maintenance Risk | High | Hardcoded strings, Traverse usage, game update sensitivity |
| Performance | Low | Redundant regex operations have minimal impact |

---

## Recommended Action Plan

### Phase 1: Critical Fixes (Immediate)

1. **Fix Scope Management**
   - File: `02_10_00_MainMenu.cs`
   - Changes: Conditional `ClearAll()`, implement `PopScope()` in `Hide_Prefix`

2. **Review Data Field Modifications**
   - Files: `02_10_10_CharacterCreation.cs`
   - Changes: Document which data fields are safe to modify, or convert to UI-only approach

### Phase 2: High Priority (Before Next Release)

3. **Add Id Field Protection**
   - Add assertions and comments for dangerous fields

4. **Fix Stat Format**
   - Verify if format change is intentional, document decision

5. **Add Traverse Existence Checks**
   - Wrap all `Traverse.Field<T>()` calls with existence checks

### Phase 3: Medium Priority (Gradual Improvement)

6. **Fix Bullet Case Sensitivity**
7. **Add Error Logging**
8. **Centralize Color Tag Normalization**

### Phase 4: Low Priority (When Convenient)

9. **Remove Unrelated Patches**
10. **Improve JSON Parser**
11. **Add Version Compatibility Checks**

---

## Strengths Identified

- ✅ Good exception handling in critical patches (backup/restore pattern)
- ✅ Clear separation of concerns (Core, Patches, Utils layers)
- ✅ TranslationEngine's tag preservation logic is well-designed
- ✅ Comprehensive logging for debugging
- ✅ Good documentation headers in files
- ✅ Safe translation utilities prevent crashes on missing translations

---

## Appendix: Code Principle Violations

The following code patterns violate the documented principles in `.github/copilot-instructions.md`:

| Principle | Violation Found | Location |
|-----------|-----------------|----------|
| "UI-Only Postfix Pattern" | `genotype.DisplayName = data.KoreanName` | CharacterCreation.cs |
| "Never modify data fields" | `loc.Name = tName` | CharacterCreation.cs |
| "Check Both Namespaces" | ✅ Correctly implemented | MainMenu.cs |
| "Color Tag Duplication" | Potential with uppercase tags | StructureTranslator.cs |

---

*Report generated: 2026-01-19*  
*Next review recommended: After Phase 1 fixes completed*
