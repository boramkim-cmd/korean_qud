/*
 * 파일명: 02_20_01_DisplayNamePatch.cs
 * 분류: [Patch] 오브젝트 이름 번역
 * 역할: GetDisplayNameEvent.GetFor() 패치로 생물/아이템 이름 한글화
 * 작성일: 2026-01-22
 * 비고: ForSort, ColorOnly 모드에서는 번역 스킵
 * 수정: 2026-01-22 - Harmony 패치 시그니처 수정 (파라미터 타입 명시)
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using XRL;
using XRL.World;

namespace QudKorean.Objects
{
    /// <summary>
    /// Harmony patch for GetDisplayNameEvent.GetFor() method.
    /// Translates creature and item display names to Korean.
    /// </summary>
    [HarmonyPatch(typeof(GetDisplayNameEvent))]
    [HarmonyPatch("GetFor")]
    [HarmonyPatch(new Type[] { 
        typeof(GameObject),  // Object
        typeof(string),      // Base
        typeof(int),         // Cutoff
        typeof(string),      // Context
        typeof(bool),        // AsIfKnown
        typeof(bool),        // Single
        typeof(bool),        // NoConfusion
        typeof(bool),        // NoColor
        typeof(bool),        // ColorOnly
        typeof(bool),        // Visible
        typeof(bool),        // BaseOnly
        typeof(bool),        // UsingAdjunctNoun
        typeof(bool),        // WithoutTitles
        typeof(bool),        // ForSort
        typeof(bool),        // Reference
        typeof(bool)         // IncludeImplantPrefix
    })]
    public static class Patch_ObjectDisplayName
    {
        private const string LOG_PREFIX = "[QudKR-Objects]";
        
        /// <summary>
        /// Postfix patch for GetDisplayNameEvent.GetFor().
        /// CRITICAL: Must check ForSort and ColorOnly parameters to avoid breaking sorting/color-only operations.
        /// </summary>
        [HarmonyPostfix]
        static void GetFor_Postfix(
            ref string __result,
            GameObject Object,
            bool ColorOnly,    // If true, only returns color code - DO NOT translate
            bool ForSort)      // If true, used for sorting - DO NOT translate
        {
            try
            {
                // CRITICAL: Skip translation for special modes
                if (ForSort || ColorOnly) return;
                
                // Basic validation
                if (Object == null || string.IsNullOrEmpty(__result)) return;
                
                string blueprint = Object.Blueprint;
                if (string.IsNullOrEmpty(blueprint)) return;
                
                // Attempt translation
                if (ObjectTranslator.TryGetDisplayName(blueprint, __result, out string translated))
                {
                    __result = translated;
                }
            }
            catch (Exception ex)
            {
                // Never crash the game - log and continue
                UnityEngine.Debug.LogError($"{LOG_PREFIX} GetFor_Postfix error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Clears the translation cache. Called on game load.
        /// </summary>
        public static void ClearCache()
        {
            ObjectTranslator.ClearCache();
        }
        
        /// <summary>
        /// Reloads JSON files and clears cache. Used by kr:reload wish command.
        /// </summary>
        public static void ReloadAndClear()
        {
            ObjectTranslator.ReloadJson();
        }
    }
    
    /// <summary>
    /// Invalidates cache on game load to ensure fresh translations.
    /// Uses CallAfterGameLoaded attribute which is the official Qud mod API for this purpose.
    /// </summary>
    [HasCallAfterGameLoaded]
    public static class CacheInvalidation
    {
        [CallAfterGameLoaded]
        public static void OnGameLoaded()
        {
            Patch_ObjectDisplayName.ClearCache();
        }
    }
}
