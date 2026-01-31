// 분류: UI 패치
// 역할: 상단 HUD 바 (HP, 온도, 날짜, LVL, Exp) + 배고픔/갈증 상태 텍스트 한글화

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    // Task 5a: PlayerStatusBar.BeginEndTurn() — HP 바, 온도, 날짜
    [HarmonyPatch(typeof(Qud.UI.PlayerStatusBar), "BeginEndTurn")]
    public static class Patch_PlayerStatusBar_BeginEndTurn
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.PlayerStatusBar __instance)
        {
            try
            {
                var barType = typeof(Qud.UI.PlayerStatusBar);

                // HP 바 텍스트
                StatusFormatExtensions.TranslateUITextSkin(__instance, barType, "hpText", val =>
                {
                    if (val.Contains("HP: "))
                        val = val.Replace("HP: ", "체력: ");
                    return val;
                });

                // 온도 텍스트
                StatusFormatExtensions.TranslateUITextSkin(__instance, barType, "tempText", val =>
                {
                    if (val.StartsWith("T:"))
                        val = "온도:" + val.Substring(2);
                    return val;
                });

                // 날짜 텍스트 — "X of Y" → "X Y"
                StatusFormatExtensions.TranslateUITextSkin(__instance, barType, "dateText", val =>
                {
                    if (val.Contains(" of "))
                        val = val.Replace(" of ", " ");
                    return val;
                });

                // LVL/Exp 바 (Update()는 매 프레임이므로 여기서 처리)
                StatusFormatExtensions.TranslateUITextSkin(__instance, barType, "xpBarText", val =>
                {
                    val = val.Replace("LVL: ", "레벨: ");
                    val = val.Replace("Exp: ", "경험: ");
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] PlayerStatusBar BeginEndTurn 오류: {e.Message}");
            }
        }
    }

    // Task 5b: PlayerStatusBar — LVL/Exp 바
    // BeginEndTurn()에서 함께 처리 (Update()는 매 프레임 호출되므로 성능 문제)
    // XP 바는 레벨업 시에만 변경되므로 BeginEndTurn으로 충분

    // Task 6: Stomach — 배고픔/갈증 상태 텍스트
    [HarmonyPatch(typeof(XRL.World.Parts.Stomach), "FoodStatus")]
    public static class Patch_Stomach_FoodStatus
    {
        private static readonly Dictionary<string, string> _foodStatus = new Dictionary<string, string>
        {
            { "Sated", "포만" },
            { "Hungry", "배고픔" },
            { "Famished!", "굶주림!" },
            { "Wilted!", "시들음!" }
        };

        [HarmonyPostfix]
        static void Postfix(ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(__result)) return;

                foreach (var kv in _foodStatus)
                {
                    if (__result.Contains(kv.Key))
                    {
                        __result = __result.Replace(kv.Key, kv.Value);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] FoodStatus Postfix 오류: {e.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(XRL.World.Parts.Stomach), "WaterStatus")]
    public static class Patch_Stomach_WaterStatus
    {
        private static readonly Dictionary<string, string> _waterStatus = new Dictionary<string, string>
        {
            { "Quenched", "해갈" },
            { "Thirsty", "목마름" },
            { "Parched", "극갈" },
            { "Dehydrated!", "탈수!" },
            { "Tumescent", "과수분" }
        };

        [HarmonyPostfix]
        static void Postfix(ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(__result)) return;

                foreach (var kv in _waterStatus)
                {
                    if (__result.Contains(kv.Key))
                    {
                        __result = __result.Replace(kv.Key, kv.Value);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] WaterStatus Postfix 오류: {e.Message}");
            }
        }
    }
}
