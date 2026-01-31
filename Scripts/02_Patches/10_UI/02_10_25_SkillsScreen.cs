// 분류: UI 패치
// 역할: 스킬 화면 UI 텍스트 한글화 (헤더 스탯 바, SP 텍스트, 스킬 항목 텍스트, 속성 요구치)

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    // Task 2a+2b: SkillsAndPowersStatusScreen — 스탯 요약 바 + SP 텍스트
    [HarmonyPatch(typeof(Qud.UI.SkillsAndPowersStatusScreen), "UpdateData")]
    public static class Patch_SkillsScreen_UpdateData
    {
        private static readonly Dictionary<string, string> _statLabels = new Dictionary<string, string>
        {
            { "STR:", "힘:" },
            { "AGI:", "민첩:" },
            { "TOU:", "건강:" },
            { "INT:", "지능:" },
            { "WIL:", "의지:" },
            { "EGO:", "자아:" }
        };

        [HarmonyPostfix]
        static void Postfix(Qud.UI.SkillsAndPowersStatusScreen __instance)
        {
            try
            {
                var screenType = typeof(Qud.UI.SkillsAndPowersStatusScreen);

                // statBlockText — 스탯 요약 바
                StatusFormatExtensions.TranslateUITextSkin(__instance, screenType, "statBlockText", val =>
                {
                    foreach (var kv in _statLabels)
                    {
                        if (val.Contains(kv.Key))
                            val = val.Replace(kv.Key, kv.Value);
                    }
                    return val;
                });

                // spText — "Skill Points (SP): X"
                StatusFormatExtensions.TranslateUITextSkin(__instance, screenType, "spText", val =>
                {
                    if (val.Contains("Skill Points (SP):"))
                        val = val.Replace("Skill Points (SP):", "스킬 포인트 (SP):");
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] SkillsScreen UpdateData 오류: {e.Message}");
            }
        }
    }

    // Task 3 + Task 4: SkillsAndPowersLine.setData() — Starting Cost / Learned / 속성 요구치
    [HarmonyPatch(typeof(Qud.UI.SkillsAndPowersLine), "setData")]
    public static class Patch_SkillsLine_SetData
    {
        private static readonly Dictionary<string, string> _attrNames = new Dictionary<string, string>
        {
            { "Strength", "힘" },
            { "Agility", "민첩" },
            { "Toughness", "건강" },
            { "Intelligence", "지능" },
            { "Willpower", "의지" },
            { "Ego", "자아" }
        };

        [HarmonyPostfix]
        static void Postfix(Qud.UI.SkillsAndPowersLine __instance)
        {
            try
            {
                var lineType = typeof(Qud.UI.SkillsAndPowersLine);

                // skillRightText — "Starting Cost [X sp]" / "Learned [X/Y]" / "[Learned]" / "[Unlearned]"
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "skillRightText", val =>
                {
                    // Cost/Learned 번역
                    val = val.Replace("Starting Cost", "습득 비용");
                    val = val.Replace("[Learned]", "[습득]");
                    val = val.Replace("[Unlearned]", "[미습득]");
                    val = val.Replace("Learned", "습득"); // "Learned [X/Y]" 처리

                    // 속성 요구치 이름 번역 (Task 4)
                    foreach (var kv in _attrNames)
                    {
                        if (val.Contains(kv.Key))
                            val = val.Replace(kv.Key, kv.Value);
                    }

                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] SkillsLine setData 오류: {e.Message}");
            }
        }
    }
}
