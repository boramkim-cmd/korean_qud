# Item Tooltip Localization Analysis
# Deep Investigation Report

**Analysis Date**: 2026-01-22  
**Status**: Investigation Complete  
**Priority**: High  

---

## 1. Problem Summary (Screenshots Analysis)

### Observed Issues in Game:

| Issue | Screenshot Evidence | Severity |
|-------|---------------------|----------|
| "This Item" / "Equipped Item" headers in English | All 4 screenshots | High |
| "waterskin" item name in English | Screenshot 2 | High |
| "bear jerky" item name in English | Screenshot 3 | High |
| Item descriptions in English | Screenshots 2, 3 | High |
| Some items correctly translated (마녀나무 껍질, 햇불) | Screenshots 1, 4 | Working |

### Pattern Analysis:
- **Translated**: Items with exact Blueprint match in JSON (torch → 횃불, witchwood bark → 마녀나무 껍질)
- **Not Translated**: Dynamic/compound items (bear jerky), items with state suffix ([empty])
- **Headers**: Unity Prefab hardcoded text, not covered by any patch

---

## 2. Code Architecture Analysis

### 2.1 Tooltip Rendering Flow

```
User hovers over item in inventory
       ↓
BaseLineWithTooltip.Update() detects hover
       ↓
BaseLineWithTooltip.StartTooltip(go, compareGo)
       ↓
Look.GenerateTooltipInformation(go)
  → Returns: DisplayName, LongDescription, SubHeader, WoundLevel, IconRenderable
       ↓
GameManager.Instance.compareLookerTooltip (Unity Prefab)
       ↓
ParameterizedTextField loop fills values:
  - "DisplayName" → tooltipInformation.DisplayName
  - "LongDescription" → tooltipInformation.LongDescription
  - "DisplayName2" → comparison item name
  - "LongDescription2" → comparison item description
       ↓
TooltipTrigger.ShowManually() displays tooltip
```

### 2.2 Key Source Files

| File | Path | Role |
|------|------|------|
| BaseLineWithTooltip.cs | `Assets/core_source/GameSource/Qud.UI/` | Tooltip trigger, StartTooltip() method |
| Look.cs | `Assets/core_source/GameSource/XRL.World.Parts/` | GenerateTooltipInformation() |
| Description.cs | `Assets/core_source/GameSource/XRL.World.Parts/` | GetShortDescription(), GetLongDescription() |
| InventoryLine.cs | `Assets/core_source/GameSource/Qud.UI/` | Inventory row, extends BaseLineWithTooltip |
| GetDisplayNameEvent.cs | `Assets/core_source/GameSource/XRL.World/` | GetFor() - display name generation |

### 2.3 Critical Code: BaseLineWithTooltip.StartTooltip()

```csharp
// Location: Assets/core_source/GameSource/Qud.UI/BaseLineWithTooltip.cs#L109-L150

public void StartTooltip(XRL.World.GameObject go, XRL.World.GameObject compareGo, ...)
{
    Look.TooltipInformation tooltipInformation = Look.GenerateTooltipInformation(go);
    Look.TooltipInformation tooltipInformation2 = default;
    
    if (compareGo != null && compareGo.IsValid() && compareGo.HasPart<Description>())
    {
        tooltipInformation2 = Look.GenerateTooltipInformation(compareGo);
    }
    
    // KEY: compareLookerTooltip is a Unity Prefab
    tooltip = GameManager.Instance.compareLookerTooltip;
    
    // KEY: ParameterizedTextField loop - THIS IS WHERE TRANSLATION SHOULD HAPPEN
    foreach (ParameterizedTextField parameterizedTextField2 in tooltip.parameterizedTextFields)
    {
        current.value = RTF.FormatToRTF(Markup.Color("y", parameterizedTextField.name switch
        {
            "DisplayName" => tooltipInformation.DisplayName,      // NEEDS TRANSLATION
            "ConText" => tooltipInformation.SubHeader,
            "WoundLevel" => tooltipInformation.WoundLevel,
            "LongDescription" => tooltipInformation.LongDescription.Trim(),  // NEEDS TRANSLATION
            "DisplayName2" => tooltipInformation2.DisplayName,    // NEEDS TRANSLATION
            "LongDescription2" => tooltipInformation2.LongDescription.Trim(), // NEEDS TRANSLATION
            _ => "",
        }), "FF", 60) ?? "";
    }
    
    tooltip.ShowManually(bForceDisplay: true, ...);
}
```

**Key Insight**: The `ParameterizedTextField.name` values are field identifiers (DisplayName, LongDescription, etc.), NOT the "This Item" / "Equipped Item" headers. Headers are separate UI elements in the Unity Prefab.

---

## 3. Existing Patch Analysis

### 3.1 Current Patches

| File | Patch Target | Status |
|------|--------------|--------|
| [02_10_02_Tooltip.cs](../../../Scripts/02_Patches/10_UI/02_10_02_Tooltip.cs) | TooltipTrigger.SetText | ✅ Active - Font fallback |
| [02_10_07_Inventory.cs](../../../Scripts/02_Patches/10_UI/02_10_07_Inventory.cs) | InventoryAndEquipmentStatusScreen | ✅ Active - Category translation |
| [02_20_01_DisplayNamePatch.cs](../../../Scripts/02_Patches/20_Objects/02_20_01_DisplayNamePatch.cs) | GetDisplayNameEvent.GetFor | ✅ Active - Name translation |
| [02_20_02_DescriptionPatch.cs](../../../Scripts/02_Patches/20_Objects/02_20_02_DescriptionPatch.cs) | Description.GetShortDescription | ✅ Active - Description translation |

### 3.2 Missing Patches

| Target | Why Needed |
|--------|------------|
| `BaseLineWithTooltip.StartTooltip` Postfix | Tooltip header translation + ensure ParameterizedTextField values are translated |
| Unity Prefab text elements | "This Item" / "Equipped Item" headers |

### 3.3 ObjectTranslator Analysis

**Current Implementation** ([02_20_00_ObjectTranslator.cs](../../../Scripts/02_Patches/20_Objects/02_20_00_ObjectTranslator.cs)):

```csharp
public static bool TryGetDisplayName(string blueprint, string originalName, out string translated)
{
    // 1. Fast path: display name cache
    // 2. Try creature cache, then item cache
    // 3. Exact match in Names dictionary
    // 4. Partial match (contains)
    // 5. Corpse pattern: "{creature} corpse" → "{creature_ko} 시체"
}
```

**Gap**: No pattern for `{creature} jerky`, `{creature} meat`, or other dynamic food items.

---

## 4. JSON Translation Files Analysis

### 4.1 File Structure

```
LOCALIZATION/OBJECTS/
├── creatures/
│   └── tutorial.json          # Snapjaw, Bear, Clockwork Beetle
├── items/
│   ├── _common.json
│   ├── tutorial.json          # Dagger, Torch, Waterskin, Chem Cell
│   ├── artifacts/
│   │   └── misc.json          # Waterskin, Canteen, Books
│   ├── consumables/
│   │   ├── drinks.json
│   │   ├── food.json          # Witchwood Bark, Yuckwheat, etc.
│   │   └── injectors.json
│   ├── weapons/
│   ├── armor/
│   └── tools/
```

### 4.2 Translation Coverage

| Item | JSON Entry | Translation | Status |
|------|------------|-------------|--------|
| witchwood bark | food.json | 마녀나무 껍질 | ✅ Working |
| torch | tutorial.json | 횃불 | ✅ Working |
| waterskin | tutorial.json, misc.json | 물주머니, 물통 | ❌ Not Applied |
| bear jerky | ❌ Missing | - | ❌ Not Translated |
| bronze dagger | tutorial.json | 청동 단검 | ✅ Working |

### 4.3 Missing Translations

```json
// Items NOT in any JSON file:
- "bear jerky" (and all "{creature} jerky" patterns)
- "preserved {creature} meat"
- "{creature} haunch"
- "cooked {ingredient}"
- Items with state suffixes: "waterskin [empty]", "torch (unburnt)"
```

---

## 5. Root Cause Analysis

### 5.1 Why "waterskin" Shows in English

1. JSON has translation: `"waterskin": "물주머니"`
2. Game displays: `"waterskin [empty]"` (with state suffix)
3. ObjectTranslator tries exact match: `"waterskin [empty]"` ≠ `"waterskin"`
4. Partial match check: `originalName.Contains(key)` → `"waterskin [empty]".Contains("waterskin")` = true
5. **Bug**: But the replacement logic may fail due to state suffix handling

### 5.2 Why "bear jerky" Shows in English

1. No JSON entry for "bear jerky" or dynamic food patterns
2. ObjectTranslator has corpse pattern but NO jerky/meat pattern
3. Item is dynamically generated from cooking/butchering

### 5.3 Why "This Item" / "Equipped Item" Show in English

1. These are **NOT** in `ParameterizedTextField` - they're separate UI Text components
2. Unity Prefab `compareLookerTooltip` has hardcoded English text
3. No existing patch targets these UI elements
4. `TooltipTrigger.SetText` patch only handles tooltip content, not headers

---

## 6. Recommended Solutions

### 6.1 Priority 1: StartTooltip Postfix Patch

**Target**: `Qud.UI.BaseLineWithTooltip.StartTooltip`

```csharp
[HarmonyPatch(typeof(BaseLineWithTooltip), "StartTooltip")]
[HarmonyPostfix]
static void StartTooltip_Postfix(TooltipTrigger ___tooltip)
{
    if (___tooltip == null) return;
    
    // 1. Translate ParameterizedTextField values
    foreach (var field in ___tooltip.parameterizedTextFields)
    {
        if (field.name == "DisplayName" || field.name == "DisplayName2")
        {
            // Apply ObjectTranslator
        }
        if (field.name == "LongDescription" || field.name == "LongDescription2")
        {
            // Apply description translation
        }
    }
    
    // 2. Find and translate header text components
    var textComponents = ___tooltip.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
    foreach (var text in textComponents)
    {
        if (text.text == "This Item") text.text = "현재 아이템";
        if (text.text == "Equipped Item") text.text = "장착 아이템";
    }
}
```

### 6.2 Priority 2: Dynamic Item Pattern Support

**Add to ObjectTranslator.cs**:

```csharp
// Pattern: "{creature} jerky" → "{creature_ko} 육포"
private static bool TryTranslateJerky(string originalName, out string translated)
{
    if (!originalName.EndsWith(" jerky", StringComparison.OrdinalIgnoreCase))
        return false;
    
    string creaturePart = originalName.Substring(0, originalName.Length - " jerky".Length);
    // Find creature translation...
    translated = $"{creatureKo} 육포";
    return true;
}

// Also add patterns for:
// - "{creature} meat" → "{creature_ko} 고기"
// - "{creature} haunch" → "{creature_ko} 넓적다리"
// - "preserved {ingredient}" → "절임 {ingredient_ko}"
```

### 6.3 Priority 3: State Suffix Handling

**Improve name matching to strip state suffixes**:

```csharp
// Strip common suffixes before matching
private static string StripStateSuffix(string name)
{
    // Remove: [empty], [full], (unburnt), (lit), x4, etc.
    string result = Regex.Replace(name, @"\s*\[.*?\]$", "");
    result = Regex.Replace(result, @"\s*\(.*?\)$", "");
    result = Regex.Replace(result, @"\s*x\d+$", "");
    return result.Trim();
}
```

### 6.4 Priority 4: Add Missing JSON Translations

**Add to food.json**:

```json
"Bear Jerky": {
  "names": {
    "bear jerky": "곰 육포"
  },
  "description": "Bear flesh was cut into neat strips, salted, and smoked.",
  "description_ko": "곰고기를 깔끔한 조각으로 잘라 소금에 절이고 훈제했다."
}
```

**Add to common.json (tooltips section)**:

```json
"tooltips": {
  "this item": "현재 아이템",
  "equipped item": "장착 아이템"
}
```

---

## 7. Implementation Checklist

- [x] Create `BaseLineWithTooltip.StartTooltip` Postfix patch
- [x] Add header text translation ("This Item" → "현재 아이템")
- [x] Add jerky/meat/haunch pattern to ObjectTranslator
- [x] Improve state suffix stripping in name matching
- [x] Add missing food items to JSON (jerky, preserved meat, etc.)
- [x] Add tooltip headers to common.json
- [ ] Test with various item types:
  - [ ] Static items (dagger, torch)
  - [ ] Items with state ([empty], (unburnt))
  - [ ] Dynamic items (bear jerky, corpses)
  - [ ] Comparison tooltip (both items)

---

## 8. Bugs Found During Implementation (2026-01-22)

### 8.1 BUG: TooltipTrigger vs Tooltip.GameObject Confusion (FIXED)

**Problem**: Original code accessed TMP components from wrong object.

```csharp
// WRONG - TooltipTrigger is the trigger, not the tooltip UI!
var textComponents = tooltip.GetComponentsInChildren<TextMeshProUGUI>(true);

// CORRECT - Tooltip.GameObject is the actual tooltip UI
var textComponents = trigger.Tooltip.GameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
```

**Impact**: Header translation and font application failed silently.

### 8.2 BUG: State Suffix Processing Order (FIXED)

**Problem**: Partial matching executed before suffix stripping.

```
Input: "waterskin [empty]"
1. Exact match: "waterskin [empty]" ≠ "waterskin" → FAIL
2. Partial match: "waterskin [empty]".Contains("waterskin") → TRUE
3. Result: "물주머니 [empty]" (English suffix kept!)
```

**Fix**: Reordered to check suffix stripping BEFORE partial matching.

```
Input: "waterskin [empty]"
1. Exact match: FAIL
2. Suffix strip: "waterskin" + " [empty]"
3. Exact match stripped: "waterskin" → "물주머니"
4. Translate suffix: " [empty]" → " [비어있음]"
5. Result: "물주머니 [비어있음]" ✓
```

### 8.3 BUG: Look.QueueLookerTooltip Path Not Covered (FIXED)

**Problem**: Multiple tooltip rendering paths exist:

| Path | Trigger | Original Patch |
|------|---------|----------------|
| `BaseLineWithTooltip.StartTooltip` | Inventory item hover | ✅ Patched |
| `Look.QueueLookerTooltip` | World map object click | ❌ Missing |
| `Look.ShowItemTooltipAsync` | General item tooltip | ❌ Missing |

**Fix**: Consolidated all patches into `TooltipTrigger.ShowManually` Postfix, which is the common endpoint for all tooltip display paths.

---

## 9. Technical Notes

### 9.1 Unity Prefab Access Pattern

The `compareLookerTooltip` is accessed via `GameManager.Instance`. Text children can be found using:

```csharp
var textComponents = tooltip.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
```

**Caution**: Prefab might be pooled/reused. Always check text content before replacing.

### 9.2 Performance Considerations

- Header translation should use string comparison, not regex
- Cache translated display names (already implemented in ObjectTranslator)
- Avoid calling GetComponentsInChildren every frame - only on tooltip show

### 9.3 Tooltip Architecture Summary

```
┌─────────────────────────────────────────────────────────────────┐
│                        TOOLTIP SYSTEM                           │
├─────────────────────────────────────────────────────────────────┤
│  Entry Points (3 different paths):                              │
│  ┌─────────────────────────┐                                    │
│  │ BaseLineWithTooltip     │ → Inventory item comparison        │
│  │ .StartTooltip()         │                                    │
│  └───────────┬─────────────┘                                    │
│              │                                                   │
│  ┌───────────┴─────────────┐                                    │
│  │ Look.QueueLookerTooltip │ → World map tile/object click      │
│  └───────────┬─────────────┘                                    │
│              │                                                   │
│  ┌───────────┴─────────────┐                                    │
│  │ Look.ShowItemTooltipAsync│ → General item tooltip            │
│  └───────────┬─────────────┘                                    │
│              │                                                   │
│              ▼                                                   │
│  ┌─────────────────────────┐                                    │
│  │ TooltipTrigger          │ ← OUR PATCH POINT (ShowManually)   │
│  │ .ShowManually()         │   Covers ALL paths!                │
│  └───────────┬─────────────┘                                    │
│              │                                                   │
│              ▼                                                   │
│  ┌─────────────────────────┐                                    │
│  │ Tooltip.GameObject      │ ← Actual UI (TMP components here)  │
│  │ (Unity Prefab)          │                                    │
│  └─────────────────────────┘                                    │
└─────────────────────────────────────────────────────────────────┘
```

### 9.4 Related Documentation

- [09_OBJECT_REVIEW.md](09_OBJECT_REVIEW.md) - Object localization analysis
- [05_ERROR_LOG.md](05_ERROR_LOG.md) - Known issues
- [04_CHANGELOG.md](04_CHANGELOG.md) - Recent changes

---

## 10. Remaining Risks (Monitor)

| Risk | Description | Mitigation |
|------|-------------|------------|
| RTF Double Wrapping | If translated name already has color tags, `Markup.Color("y", ...)` may double-wrap | Test with colored item names |
| JosaHandler Patch Mismatch | `GenerateTooltipInformation` returns struct, not string - legacy patch may not work | Verify in Base-Work folder |
| Different Prefab Structures | lookerTooltip, tileTooltip, compareLookerTooltip may have different structures | Test all tooltip types |

---

## 11. References

### Source Files Analyzed

1. `Assets/core_source/GameSource/Qud.UI/BaseLineWithTooltip.cs` (Lines 1-185)
2. `Assets/core_source/GameSource/XRL.UI/Look.cs` (Lines 240-400)
3. `Assets/core_source/ThirdParty/ModelShark/TooltipTrigger.cs` (Full file)
4. `Assets/core_source/ThirdParty/ModelShark/TooltipManager.cs` (Lines 130-250)
5. `Assets/core_source/ThirdParty/ModelShark/Tooltip.cs` (Lines 1-80)
6. `Assets/core_source/GameSource/Qud.UI/RTF.cs` (Full file)
7. `Scripts/02_Patches/10_UI/02_10_02_Tooltip.cs` (Full file)
8. `Scripts/02_Patches/20_Objects/02_20_00_ObjectTranslator.cs` (Full file)

### JSON Files Modified

1. `LOCALIZATION/UI/common.json` - Added tooltips section
2. `LOCALIZATION/OBJECTS/items/consumables/food.json` - Added jerky, haunch, preserved meat
