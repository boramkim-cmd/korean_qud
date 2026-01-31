/*
 * 파일명: 02_20_02_DescriptionPatch.cs
 * 분류: [Patch] 오브젝트 설명 번역
 * 역할: Description.GetShortDescription() 패치로 설명 한글화
 * 작성일: 2026-01-22
 * 비고: 툴팁 및 Look 팝업의 설명 번역
 */

using System;
using System.Text;
using HarmonyLib;
using XRL.World.Parts;
using QudKorean.Objects.V2;

namespace QudKorean.Objects
{
    /// <summary>
    /// Harmony patch for Description part methods.
    /// Translates creature and item descriptions to Korean.
    /// </summary>
    [HarmonyPatch(typeof(Description))]
    public static class Patch_ObjectDescription
    {
        private const string LOG_PREFIX = "[QudKR-Objects]";
        
        /// <summary>
        /// Postfix patch for Description.GetShortDescription().
        /// Translates the short description shown in tooltips and Look popups.
        /// </summary>
        [HarmonyPatch(nameof(Description.GetShortDescription))]
        [HarmonyPostfix]
        static void GetShortDescription_Postfix(ref string __result, Description __instance)
        {
            try
            {
                if (__instance?.ParentObject == null) return;
                
                string blueprint = __instance.ParentObject.Blueprint;
                if (string.IsNullOrEmpty(blueprint)) return;
                
                if (ObjectTranslatorV2.TryGetDescription(blueprint, out string translated))
                {
                    __result = translated;
                }

                // "Weight: X lbs." → "무게: X kg"
                if (__result.Contains(" lbs."))
                {
                    __result = __result.Replace("Weight: ", "무게: ");
                    __result = __result.Replace(" lbs.", " kg");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} GetShortDescription_Postfix error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Postfix patch for Description.GetLongDescription(StringBuilder).
        /// Translates hardcoded UI strings in the long description.
        /// </summary>
        [HarmonyPatch("GetLongDescription", typeof(StringBuilder))]
        [HarmonyPostfix]
        static void GetLongDescription_Postfix(StringBuilder SB)
        {
            try
            {
                // Replace hardcoded English strings with Korean
                SB.Replace("Physical features: ", "신체적 특징: ");
                SB.Replace("Equipped: ", "장착: ");
                SB.Replace("Gender: ", "성별: ");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} GetLongDescription_Postfix error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Postfix patch for Description.GetFeelingDescription().
        /// Translates "Friendly", "Hostile", "Neutral" strings.
        /// </summary>
        [HarmonyPatch(nameof(Description.GetFeelingDescription))]
        [HarmonyPostfix]
        static void GetFeelingDescription_Postfix(ref string __result)
        {
            try
            {
                __result = __result switch
                {
                    "{{G|Friendly}}" => "{{G|우호적}}",
                    "{{R|Hostile}}" => "{{R|적대적}}",
                    "Neutral" => "중립",
                    _ => __result
                };
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} GetFeelingDescription_Postfix error: {ex.Message}");
            }
        }
    }
}
