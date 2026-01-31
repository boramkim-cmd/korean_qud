// 분류: UI 패치
// 역할: 스킬 화면 UI 텍스트 한글화
//   - 스탯 요약 바 (ShowScreen에서 설정)
//   - SP 텍스트 (UpdateData에서 설정)
//   - 스킬 항목 텍스트 (setData)
//   - 속성 요구치 (PowerEntry.Render)
//   - 디테일 패널 ([Learned]/[Unlearned], 요구 속성)

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    // 속성 이름 번역용 공통 dictionary
    internal static class StatNameTranslator
    {
        public static readonly Dictionary<string, string> AttrNames = new Dictionary<string, string>
        {
            { "Strength", "힘" },
            { "Agility", "민첩" },
            { "Toughness", "건강" },
            { "Intelligence", "지능" },
            { "Willpower", "의지" },
            { "Ego", "자아" }
        };

        public static string TranslateAttributes(string val)
        {
            foreach (var kv in AttrNames)
            {
                if (val.Contains(kv.Key))
                    val = val.Replace(kv.Key, kv.Value);
            }
            return val;
        }
    }

    // SkillsAndPowersStatusScreen.ShowScreen() — 스탯 요약 바
    // 소스: statBlockText.SetText("{{K|{{g|STR:}}19 ■ {{g|AGI}}: 19 ■ ...}}")
    // STR:는 색상 태그 내부, 나머지는 AGI}}:처럼 :가 태그 밖
    [HarmonyPatch(typeof(Qud.UI.SkillsAndPowersStatusScreen), "ShowScreen")]
    public static class Patch_SkillsScreen_ShowScreen
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.SkillsAndPowersStatusScreen __instance)
        {
            try
            {
                var screenType = typeof(Qud.UI.SkillsAndPowersStatusScreen);

                StatusFormatExtensions.TranslateUITextSkin(__instance, screenType, "statBlockText", val =>
                {
                    // STR:는 태그 내부라 그대로 매칭
                    val = val.Replace("STR:", "힘:");
                    // 나머지는 태그 내부 약어만 교체 (:|는 태그 밖이므로 약어만)
                    val = val.Replace("|AGI}}", "|민첩}}");
                    val = val.Replace("|TOU}}", "|건강}}");
                    val = val.Replace("|INT}}", "|지능}}");
                    val = val.Replace("|WIL}}", "|의지}}");
                    val = val.Replace("|EGO}}", "|자아}}");
                    return val;
                });

                // nameBlockText: "Player's Skills" → 한글
                StatusFormatExtensions.TranslateUITextSkin(__instance, screenType, "nameBlockText", val =>
                {
                    if (val.EndsWith("Skills") || val.EndsWith("' Skills") || val.EndsWith("'s Skills"))
                    {
                        int idx = val.LastIndexOf("'");
                        if (idx > 0)
                            val = val.Substring(0, idx) + "의 스킬";
                        else
                            val = val.Replace("Skills", "스킬");
                    }
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] SkillsScreen ShowScreen 오류: {e.Message}");
            }
        }
    }

    // SkillsAndPowersStatusScreen.UpdateData() — SP 텍스트
    [HarmonyPatch(typeof(Qud.UI.SkillsAndPowersStatusScreen), "UpdateData")]
    public static class Patch_SkillsScreen_UpdateData
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.SkillsAndPowersStatusScreen __instance)
        {
            try
            {
                var screenType = typeof(Qud.UI.SkillsAndPowersStatusScreen);

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

    // SkillsAndPowersLine.setData() — Starting Cost / Learned 텍스트
    [HarmonyPatch(typeof(Qud.UI.SkillsAndPowersLine), "setData")]
    public static class Patch_SkillsLine_SetData
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.SkillsAndPowersLine __instance)
        {
            try
            {
                var lineType = typeof(Qud.UI.SkillsAndPowersLine);

                // skillRightText: "Starting Cost [X sp]" / "Learned [X/Y]"
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "skillRightText", val =>
                {
                    // 순서 중요: [Learned]/[Unlearned] 먼저, 그 다음 bare Learned
                    val = val.Replace("Starting Cost", "습득 비용");
                    val = val.Replace("[Learned]", "[습득]");
                    val = val.Replace("[Unlearned]", "[미습득]");
                    val = val.Replace("Learned", "습득");
                    return val;
                });

                // powerText: SPNode.ModernUIText 결과 — 속성 요구치 포함
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "powerText", val =>
                {
                    return StatNameTranslator.TranslateAttributes(val);
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] SkillsLine setData 오류: {e.Message}");
            }
        }
    }

    // SkillsAndPowersStatusScreen.UpdateDetailsFromNode() — 디테일 패널
    // learnedText: "[Learned]" / "[Unlearned]"
    // requirementsText: ":: 19 Strength ::" 등
    [HarmonyPatch(typeof(Qud.UI.SkillsAndPowersStatusScreen), "UpdateDetailsFromNode")]
    public static class Patch_SkillsScreen_UpdateDetails
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.SkillsAndPowersStatusScreen __instance)
        {
            try
            {
                var screenType = typeof(Qud.UI.SkillsAndPowersStatusScreen);

                // learnedText: "[Learned]" / "[Unlearned]"
                StatusFormatExtensions.TranslateUITextSkin(__instance, screenType, "learnedText", val =>
                {
                    val = val.Replace("[Learned]", "[습득]");
                    val = val.Replace("[Unlearned]", "[미습득]");
                    return val;
                });

                // requirementsText: ":: 19 Strength ::" 등
                StatusFormatExtensions.TranslateUITextSkin(__instance, screenType, "requirementsText", val =>
                {
                    return StatNameTranslator.TranslateAttributes(val);
                });

                // requiredSkillsText: "[none]" 등
                StatusFormatExtensions.TranslateUITextSkin(__instance, screenType, "requiredSkillsText", val =>
                {
                    val = val.Replace("[none]", "[없음]");
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] SkillsScreen UpdateDetails 오류: {e.Message}");
            }
        }
    }

    // PowerEntry.Render() — 파워 목록에서 속성 요구치 번역
    // 반환값: "power_name [Xsp] 19 Strength" 등
    [HarmonyPatch(typeof(XRL.World.Skills.PowerEntry), "Render", new Type[] { typeof(XRL.World.GameObject) })]
    public static class Patch_PowerEntry_Render
    {
        [HarmonyPostfix]
        static void Postfix(ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(__result)) return;
                __result = StatNameTranslator.TranslateAttributes(__result);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] PowerEntry.Render 오류: {e.Message}");
            }
        }
    }
}
