# Remaining English UI Translation Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Translate all remaining English UI text visible during gameplay into Korean, across 6 phases.

**Architecture:** Harmony postfix patches intercept game methods at runtime and replace English strings with Korean from JSON lookup files. The `LocalizationManager.GetCategory()` API loads `Dictionary<string, string>` from JSON files under `LOCALIZATION/`. Some items only need JSON additions to existing categories (picked up by existing patches); others need new C# patch files.

**Tech Stack:** C# with Harmony 2.0, Newtonsoft.Json, Unity TextMeshPro. JSON translation data. Python validation tooling.

---

### Task 1: Phase 1 — Add 5 menu items to common.json

**Files:**
- Modify: `LOCALIZATION/UI/common.json`

**Step 1: Add the 5 new entries to common.json**

Open `LOCALIZATION/UI/common.json` and add these entries to the `"common"` object (alphabetical order):

```json
"Buy Mutation": "변이 구매",
"navigation": "이동",
"Set Primary Limb": "주 사용 부위 설정",
"Show Effects": "효과 보기",
"Show Tooltip": "도움말 보기"
```

**Step 2: Run validation**

Run: `python3 tools/project_tool.py validate`
Expected: PASS, no duplicate key errors

**Step 3: Commit**

```bash
git add LOCALIZATION/UI/common.json
git commit -m "feat: add 5 status screen menu items to common.json"
git push
```

---

### Task 2: Phase 2 — Create stat_help.json with 16 attribute descriptions

**Files:**
- Create: `LOCALIZATION/UI/stat_help.json`

**Step 1: Create the JSON file**

Create `LOCALIZATION/UI/stat_help.json` with the following content. Keys are `Statistic.Name` values. Translations follow existing `stats.json` tone ("~을 결정합니다", "~을 나타냅니다") and use canonical attribute names from `_SHARED/attributes.json`.

```json
{
  "stat_help": {
    "Strength": "{{W|힘}}은 근접 피해량(방어 관통력 향상), 강제 이동 저항력, 운반 용량을 결정합니다.",
    "Agility": "{{W|민첩}}은 근접 및 원거리 무기의 명중률과 공격 회피 확률을 결정합니다.",
    "Toughness": "{{W|건강}}은 체력, 체력 재생 속도, 독과 질병에 대한 저항력을 결정합니다.",
    "Intelligence": "{{W|지능}}은 기술 포인트 수와 유물 감정 능력을 결정합니다.",
    "Willpower": "{{W|의지}}는 활성 능력의 재사용 대기시간, 정신 공격 저항력, 체력 재생 속도를 조절합니다.",
    "Ego": "{{W|자아}}는 정신 변이의 위력, 상인과의 흥정 능력, 다른 생명체의 의지를 지배하는 능력을 결정합니다.",
    "AV": "{{W|방어력(AV)}}은 물리 공격에 대한 방어 수준을 나타냅니다. 수치가 높을수록 적의 공격이 방어구를 관통하여 피해를 입히는 횟수가 줄어듭니다. 기본 AV는 0입니다.",
    "DV": "{{W|회피력(DV)}}은 물리 공격에 맞을 확률을 나타냅니다. 수치가 높을수록 적의 공격이 빗나갈 확률이 높아집니다. DV는 민첩 보정치의 영향을 받습니다. 기본 DV는 6입니다.",
    "MA": "{{W|정신 방어력(MA)}}은 정신 공격에 대한 방어 수준을 나타냅니다. 수치가 높을수록 적의 정신 공격이 방어를 뚫고 피해를 입힐 확률이 낮아집니다. MA는 의지 보정치의 영향을 받습니다. 기본 MA는 4입니다.",
    "Speed": "{{W|속도}}는 모든 행동을 수행하는 빠르기를 나타냅니다. 기본 속도는 100입니다.",
    "MoveSpeed": "{{W|이동 속도}}는 걷거나 나는 빠르기를 나타냅니다. 기본 이동 속도는 100입니다.",
    "T": "{{W|온도}}는 현재 체온 상태를 나타냅니다. 너무 차가우면 속도가 감소하고 물리 행동이 제한될 수 있습니다. 너무 뜨거우면 불이 붙어 매 턴 피해를 받습니다.",
    "AcidResistance": "{{W|산 저항}}은 산성 피해 경감량을 결정합니다. 기본 산 저항 수치는 0입니다. 100이면 산성 피해에 면역입니다.",
    "ColdResistance": "{{W|냉기 저항}}은 냉기 피해 경감량과 체온 감소 효과에 대한 절연력을 결정합니다. 기본 냉기 저항 수치는 0입니다. 100이면 냉기 피해에 면역이며 체온이 감소하지 않습니다.",
    "ElectricResistance": "{{W|전기 저항}}은 전기 피해 경감량과 전류에 대한 저항력을 결정합니다. 기본 전기 저항 수치는 0입니다. 100이면 전기 피해에 면역이며 전기를 전도하지 않습니다.",
    "HeatResistance": "{{W|열 저항}}은 열 피해 경감량과 체온 상승 효과에 대한 절연력을 결정합니다. 기본 열 저항 수치는 0입니다. 100이면 열 피해에 면역이며 체온이 상승하지 않습니다."
  }
}
```

**Step 2: Run validation**

Run: `python3 tools/project_tool.py validate`
Expected: PASS

**Step 3: Commit**

```bash
git add LOCALIZATION/UI/stat_help.json
git commit -m "feat: add 16 stat help text translations"
git push
```

---

### Task 3: Phase 2 — Create StatHelpText Harmony patch

**Files:**
- Create: `Scripts/02_Patches/10_UI/02_10_20_StatHelpText.cs`

**Step 1: Write the patch file**

Create `Scripts/02_Patches/10_UI/02_10_20_StatHelpText.cs`:

```csharp
// 분류: UI 패치
// 역할: Statistic.GetHelpText() 반환값을 한글로 교체

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using QudKRTranslation.Core;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(XRL.World.Statistic), nameof(XRL.World.Statistic.GetHelpText))]
    public static class Patch_Statistic_GetHelpText
    {
        private static Dictionary<string, string> _helpTexts;

        [HarmonyPostfix]
        static void Postfix(XRL.World.Statistic __instance, ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(__result)) return;
                if (_helpTexts == null)
                    _helpTexts = LocalizationManager.GetCategory("stat_help");
                if (_helpTexts != null && _helpTexts.TryGetValue(__instance.Name, out var ko))
                    __result = ko;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] GetHelpText Postfix 오류: {e.Message}");
            }
        }
    }
}
```

**Step 2: Run validation**

Run: `python3 tools/project_tool.py validate`
Expected: PASS (header check: `분류:` and `역할:` present, brace balance OK)

**Step 3: Commit**

```bash
git add Scripts/02_Patches/10_UI/02_10_20_StatHelpText.cs
git commit -m "feat: add Harmony patch for stat help text translation"
git push
```

---

### Task 4: Phase 3 — Create AbilityBar Harmony patch

**Files:**
- Create: `Scripts/02_Patches/10_UI/02_10_19_AbilityBar.cs`

**Step 1: Write the patch file**

Create `Scripts/02_Patches/10_UI/02_10_19_AbilityBar.cs`. This patches void methods via field/property reflection since `ref string __result` is not available.

```csharp
// 분류: UI 패치
// 역할: AbilityBar의 영어 UI 텍스트를 한글로 교체 (ACTIVE EFFECTS, TARGET, ABILITIES, 토글 상태)

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using XRL.UI;

namespace QudKRTranslation.Patches
{
    // ACTIVE EFFECTS: → 활성 효과:
    [HarmonyPatch(typeof(AbilityBar), "InternalUpdateActiveEffects")]
    public static class Patch_AbilityBar_ActiveEffects
    {
        [HarmonyPostfix]
        static void Postfix(AbilityBar __instance)
        {
            try
            {
                var field = AccessTools.Field(typeof(AbilityBar), "effectText");
                if (field == null) return;
                string val = field.GetValue(__instance) as string;
                if (val != null && val.Contains("ACTIVE EFFECTS:"))
                    field.SetValue(__instance, val.Replace("ACTIVE EFFECTS:", "활성 효과:"));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] AbilityBar ActiveEffects 오류: {e.Message}");
            }
        }
    }

    // TARGET: {name} → 대상: {name}, TARGET: [none] → 대상: [없음]
    [HarmonyPatch(typeof(AbilityBar), "AfterRender")]
    public static class Patch_AbilityBar_Target
    {
        [HarmonyPostfix]
        static void Postfix(AbilityBar __instance)
        {
            try
            {
                var field = AccessTools.Field(typeof(AbilityBar), "targetText");
                if (field == null) return;
                string val = field.GetValue(__instance) as string;
                if (val == null) return;
                if (val.Contains("TARGET:"))
                {
                    val = val.Replace("TARGET:", "대상:");
                    val = val.Replace("[none]", "[없음]");
                    field.SetValue(__instance, val);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] AbilityBar Target 오류: {e.Message}");
            }
        }
    }

    // ABILITIES, page X of Y, [disabled], [on], [off]
    [HarmonyPatch(typeof(AbilityBar), "UpdateAbilitiesText")]
    public static class Patch_AbilityBar_Abilities
    {
        private static readonly Dictionary<string, string> _replacements = new Dictionary<string, string>
        {
            { "ABILITIES", "능력" },
            { "[disabled]", "[비활성]" },
            { "[on]", "[켜짐]" },
            { "[off]", "[꺼짐]" }
        };

        [HarmonyPostfix]
        static void Postfix(AbilityBar __instance)
        {
            try
            {
                var field = AccessTools.Field(typeof(AbilityBar), "abilitiesText");
                if (field == null) return;
                string val = field.GetValue(__instance) as string;
                if (val == null) return;

                foreach (var kv in _replacements)
                    val = val.Replace(kv.Key, kv.Value);

                // "page X of Y" → "X/Y 페이지"
                if (val.Contains("page "))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(val, @"page (\d+) of (\d+)");
                    if (match.Success)
                        val = val.Replace(match.Value, $"{match.Groups[1].Value}/{match.Groups[2].Value} 페이지");
                }

                field.SetValue(__instance, val);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] AbilityBar Abilities 오류: {e.Message}");
            }
        }
    }
}
```

**Step 2: Run validation**

Run: `python3 tools/project_tool.py validate`
Expected: PASS

**Important caveat:** The field names (`effectText`, `targetText`, `abilitiesText`) are guesses based on the design document. Before implementation, verify actual field names by searching `Assets/core_source/AbilityBar.cs` for the string assignments at the lines referenced in the design doc (lines 260, 380, 412, 723). If field names differ, update accordingly. The patch may need to target `UITextSkin.text` or `TMP_Text.text` properties instead of string fields.

**Step 3: Commit**

```bash
git add Scripts/02_Patches/10_UI/02_10_19_AbilityBar.cs
git commit -m "feat: add AbilityBar Korean translation patch"
git push
```

---

### Task 5: Phase 4 — Create ability_names.json

**Files:**
- Create: `LOCALIZATION/GAMEPLAY/ability_names.json`

**Step 1: Create the JSON file**

Create `LOCALIZATION/GAMEPLAY/ability_names.json`:

```json
{
  "ability_names": {
    "Sprint": "질주",
    "Make Camp": "캠프 설치",
    "Rebuke Robot": "로봇 복종",
    "Deploy Turret": "포탑 배치",
    "Recharge": "충전"
  }
}
```

**Step 2: Run validation**

Run: `python3 tools/project_tool.py validate`
Expected: PASS

**Step 3: Commit**

```bash
git add LOCALIZATION/GAMEPLAY/ability_names.json
git commit -m "feat: add ability name translations"
git push
```

---

### Task 6: Phase 4 — Create ActivatedAbility patch for ability names

**Files:**
- Create: `Scripts/02_Patches/10_UI/02_10_21_ActivatedAbilities.cs`

**Step 1: Write the patch file**

Create `Scripts/02_Patches/10_UI/02_10_21_ActivatedAbilities.cs`. This patches the point where ability display names are set so they render in Korean on the ability bar.

```csharp
// 분류: UI 패치
// 역할: ActivatedAbilityEntry.DisplayName을 한글로 교체

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using QudKRTranslation.Core;
using XRL.World.Parts;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(ActivatedAbilities), nameof(ActivatedAbilities.AddAbility))]
    public static class Patch_ActivatedAbilities_AddAbility
    {
        private static Dictionary<string, string> _abilityNames;

        [HarmonyPostfix]
        static void Postfix(ActivatedAbilities __instance, string Name, ref Guid __result)
        {
            try
            {
                if (_abilityNames == null)
                    _abilityNames = LocalizationManager.GetCategory("ability_names");
                if (_abilityNames == null) return;

                if (__instance.AbilityByGuid.TryGetValue(__result, out var entry))
                {
                    if (_abilityNames.TryGetValue(entry.DisplayName, out var ko))
                        entry.DisplayName = ko;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] AddAbility Postfix 오류: {e.Message}");
            }
        }
    }
}
```

**Step 2: Run validation**

Run: `python3 tools/project_tool.py validate`
Expected: PASS

**Important caveat:** The exact method signature of `AddAbility` must be verified against `Assets/core_source/`. The method name, parameter list, and return type may differ. Check `ActivatedAbilities.cs` for the actual signature. Also verify `AbilityByGuid` is the correct dictionary property name.

**Step 3: Commit**

```bash
git add Scripts/02_Patches/10_UI/02_10_21_ActivatedAbilities.cs
git commit -m "feat: add activated ability name translation patch"
git push
```

---

### Task 7: Phase 5 — Investigate equipment slot composition method

**Files:**
- Search: `Assets/core_source/` for `DescriptionPrefix` usage

**Step 1: Search for the slot name composition code**

Run:
```bash
cd /Users/ben/Desktop/qud_korean
grep -rn "DescriptionPrefix" Assets/core_source/ | head -20
```

Look for the method that combines `DescriptionPrefix + " " + Type` (and Laterality like "Left"/"Right"). Document the exact method name, class, and line numbers.

**Step 2: Search for Laterality rendering**

Run:
```bash
grep -rn "Laterality" Assets/core_source/ | grep -i "left\|right\|display\|render\|text" | head -20
```

**Step 3: Document findings**

Record the exact method(s) to patch in a comment at the top of the future patch file. This task is **research only** — implementation depends on findings.

**Step 4: Commit findings as code comment or update design doc**

No commit if research only. Proceed to Task 8 based on findings.

---

### Task 8: Phase 5 — Implement equipment slot translation patch

**Files:**
- Modify: `LOCALIZATION/_SHARED/body_parts.json` (add Laterality + prefix entries)
- Create: `Scripts/02_Patches/10_UI/02_10_22_EquipmentSlots.cs`

**Step 1: Add laterality and prefix translations to body_parts.json**

Add to the body_parts category in `LOCALIZATION/_SHARED/body_parts.json`:

```json
"Worn on": "착용:",
"Left": "왼쪽",
"Right": "오른쪽",
"Missile Weapon": "투사 무기"
```

**Step 2: Write the patch**

The exact patch depends on Task 7 findings. The patch should intercept the slot name composition method and replace components:
- `"Worn on"` → `"착용:"`
- `"Left"` / `"Right"` → `"왼쪽"` / `"오른쪽"`
- Body part type → from existing `body_parts.json`

Template:

```csharp
// 분류: UI 패치
// 역할: 장비 슬롯 표시명의 Laterality + DescriptionPrefix를 한글로 교체

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using QudKRTranslation.Core;

namespace QudKRTranslation.Patches
{
    // Target class and method to be determined by Task 7 research
    // [HarmonyPatch(typeof(TARGET_CLASS), "TARGET_METHOD")]
    public static class Patch_EquipmentSlotDisplay
    {
        private static Dictionary<string, string> _bodyParts;

        [HarmonyPostfix]
        static void Postfix(ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(__result)) return;
                if (_bodyParts == null)
                    _bodyParts = LocalizationManager.GetCategory("body_parts");
                if (_bodyParts == null) return;

                // Direct match first
                if (_bodyParts.TryGetValue(__result, out var ko))
                {
                    __result = ko;
                    return;
                }

                // Component replacement
                __result = __result.Replace("Worn on ", "");
                __result = __result.Replace("Left ", "왼쪽 ");
                __result = __result.Replace("Right ", "오른쪽 ");

                // Translate remaining body part type
                string remaining = __result.Replace("왼쪽 ", "").Replace("오른쪽 ", "").Trim();
                if (_bodyParts.TryGetValue(remaining, out var partKo))
                {
                    __result = __result.Replace(remaining, partKo);
                }

                // Add back "착용" prefix if it was "Worn on"
                // (Logic depends on actual composition pattern found in Task 7)
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] EquipmentSlot 오류: {e.Message}");
            }
        }
    }
}
```

**Step 3: Run validation**

Run: `python3 tools/project_tool.py validate`
Expected: PASS

**Step 4: Commit**

```bash
git add LOCALIZATION/_SHARED/body_parts.json Scripts/02_Patches/10_UI/02_10_22_EquipmentSlots.cs
git commit -m "feat: add equipment slot name Korean translation"
git push
```

---

### Task 9: Phase 6 — Patch status screen format strings

**Files:**
- Create: `Scripts/02_Patches/10_UI/02_10_23_StatusFormat.cs`
- Modify: `LOCALIZATION/UI/common.json`

**Step 1: Add format string labels to common.json**

Add to `LOCALIZATION/UI/common.json`:

```json
"Level:": "레벨:",
"HP:": "체력:",
"XP:": "경험치:",
"Weight:": "무게:"
```

**Step 2: Write status format patch**

Create `Scripts/02_Patches/10_UI/02_10_23_StatusFormat.cs`:

```csharp
// 분류: UI 패치
// 역할: 상태 화면 포맷 문자열 (Level/HP/XP/Weight, 유전형+하위유형)을 한글로 교체

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using QudKRTranslation.Core;

namespace QudKRTranslation.Patches
{
    // Genotype + Subtype display (e.g., "True Kin Artifex" → "순수 인간 기술자")
    // Target the method in CharacterStatusScreen that builds the genotype/subtype line
    // Exact target TBD — search for GetGenotype()/GetSubtype() usage in CharacterStatusScreen

    // Status bar labels
    // Target: the method that formats "Level: X ¯ HP: X/X ¯ XP: X/X ¯ Weight: X"
    // Apply string.Replace on the formatted result

    public static class Patch_StatusFormat
    {
        private static readonly Dictionary<string, string> _labels = new Dictionary<string, string>
        {
            { "Level:", "레벨:" },
            { "HP:", "체력:" },
            { "XP:", "경험치:" },
            { "Weight:", "무게:" }
        };

        public static string TranslateStatusLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return line;
            foreach (var kv in _labels)
                line = line.Replace(kv.Key, kv.Value);
            return line;
        }
    }

    // show cybernetics — needs dedicated patch on InventoryAndEquipmentStatusScreen
    // The {{hotkey}} tag strips to "[~Toggle] show cybernetics"
    // Patch: replace "show cybernetics" → "사이버네틱스 보기" after hotkey rendering
}
```

**Step 3: Run validation**

Run: `python3 tools/project_tool.py validate`
Expected: PASS

**Important caveat:** This task is partially a template. The exact `[HarmonyPatch]` targets for status line formatting and genotype display must be identified by searching `Assets/core_source/CharacterStatusScreen.cs` and `InventoryAndEquipmentStatusScreen.cs`. The helper method `TranslateStatusLine` is ready to use once the correct target is found.

**Step 4: Commit**

```bash
git add LOCALIZATION/UI/common.json Scripts/02_Patches/10_UI/02_10_23_StatusFormat.cs
git commit -m "feat: add status screen format string translation"
git push
```

---

### Task 10: Phase 6 — Patch "show cybernetics" hotkey text

**Files:**
- Modify: `Scripts/02_Patches/10_UI/02_10_23_StatusFormat.cs` (add patch class)

**Step 1: Add cybernetics patch to existing file**

Add to `02_10_23_StatusFormat.cs`:

```csharp
[HarmonyPatch(typeof(XRL.UI.InventoryAndEquipmentStatusScreen), "Show")]
public static class Patch_ShowCybernetics
{
    [HarmonyPostfix]
    static void Postfix()
    {
        try
        {
            // After the screen renders, find and replace the cybernetics toggle text
            // This targets the post-hotkey-rendered string
            // Exact field/property TBD — search for "show cybernetics" in InventoryAndEquipmentStatusScreen
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Qud-KR] ShowCybernetics 오류: {e.Message}");
        }
    }
}
```

**Step 2: Run validation**

Run: `python3 tools/project_tool.py validate`
Expected: PASS

**Step 3: Commit**

```bash
git add Scripts/02_Patches/10_UI/02_10_23_StatusFormat.cs
git commit -m "feat: add show cybernetics translation patch"
git push
```

---

### Task 11: In-game verification checklist

**Files:** None (manual testing)

**Step 1: Deploy mod**

Run: `./deploy.sh`

**Step 2: Phase 1 verification**

Launch game → Status screen → Verify:
- [ ] `효과 보기` visible (was `Show Effects`)
- [ ] `변이 구매` visible (was `Buy Mutation`)
- [ ] `이동` visible (was `navigation`)
- [ ] `주 사용 부위 설정` visible (was `Set Primary Limb`)
- [ ] `도움말 보기` visible (was `Show Tooltip`)

**Step 3: Phase 2 verification**

Status screen → Select each attribute → Verify:
- [ ] Korean help text displays for all 16 stats
- [ ] Color tags `{{W|...}}` render correctly
- [ ] No layout overflow

**Step 4: Phase 3 verification**

Gameplay HUD → Verify:
- [ ] `활성 효과:` (was `ACTIVE EFFECTS:`)
- [ ] `대상: [없음]` (was `TARGET: [none]`)
- [ ] `[켜짐]`/`[꺼짐]`/`[비활성]` on ability toggles
- [ ] `능력` header and `X/Y 페이지` pagination

**Step 5: Phase 4 verification**

Ability bar → Verify:
- [ ] `질주` (was `Sprint`)
- [ ] `캠프 설치` (was `Make Camp`)
- [ ] `충전` (was `Recharge`)

**Step 6: Phase 5 verification**

Inventory → Equipment tab → Verify:
- [ ] `왼쪽 팔`, `오른쪽 팔` slot names
- [ ] No layout overflow from Korean text

**Step 7: Phase 6 verification**

Status screen header → Verify:
- [ ] `레벨:`, `체력:`, `경험치:`, `무게:` labels
- [ ] `사이버네틱스 보기` toggle text
