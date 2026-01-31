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
                var skinField = AccessTools.Field(typeof(AbilityBar), "AbilityCommandText");
                if (skinField == null) return;
                var skin = skinField.GetValue(__instance);
                if (skin == null) return;

                var textProp = AccessTools.Property(skin.GetType(), "text");
                if (textProp == null) return;
                string val = textProp.GetValue(skin) as string;
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

                textProp.SetValue(skin, val);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] AbilityBar Abilities 오류: {e.Message}");
            }
        }
    }
}
