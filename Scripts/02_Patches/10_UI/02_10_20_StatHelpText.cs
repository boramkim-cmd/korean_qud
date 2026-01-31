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
