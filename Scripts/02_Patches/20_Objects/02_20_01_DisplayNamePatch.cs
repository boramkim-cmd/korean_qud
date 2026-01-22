/*
 * 파일명: 02_20_01_DisplayNamePatch.cs
 * 분류: [Patch] 오브젝트 이름 번역
 * 역할: GetDisplayNameEvent.GetFor() 패치로 생물/아이템 이름 한글화
 * 작성일: 2026-01-22
 * 비고: ForSort, ColorOnly 모드에서는 번역 스킵
 * 수정: 2026-01-22 - Harmony 패치 시그니처 수정 - 파라미터 타입 명시 제거로 모든 호출 패치
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
    /// NOTE: We don't specify parameter types so this matches ALL GetFor calls,
    /// including short calls like GetFor(this, DisplayNameBase) that use default params.
    /// </summary>
    [HarmonyPatch(typeof(GetDisplayNameEvent), "GetFor")]
    public static class Patch_ObjectDisplayName
    {
        private const string LOG_PREFIX = "[QudKR-Objects]";
        
        // Static constructor to verify patch is loaded
        static Patch_ObjectDisplayName()
        {
            UnityEngine.Debug.Log($"{LOG_PREFIX} DisplayName patch class loaded!");
        }
        
        /// <summary>
        /// Postfix patch for GetDisplayNameEvent.GetFor().
        /// CRITICAL: Must check ForSort and ColorOnly parameters to avoid breaking sorting/color-only operations.
        /// </summary>
        [HarmonyPostfix]
        static void GetFor_Postfix(
            ref string __result,
            GameObject Object,
            bool ColorOnly = false,   // Default value to handle calls that don't specify
            bool ForSort = false)     // Default value to handle calls that don't specify
        {
            try
            {
                // CRITICAL: Skip translation for special modes
                if (ForSort || ColorOnly) return;
                
                // Basic validation
                if (Object == null || string.IsNullOrEmpty(__result)) return;
                
                string blueprint = Object.Blueprint;
                if (string.IsNullOrEmpty(blueprint)) return;
                
                // DEBUG: Log first few translation attempts
                if (_logCount < 10)
                {
                    _logCount++;
                    UnityEngine.Debug.Log($"{LOG_PREFIX} TryTranslate: blueprint='{blueprint}', result='{__result}'");
                }
                
                // Attempt translation
                if (ObjectTranslator.TryGetDisplayName(blueprint, __result, out string translated))
                {
                    // CRITICAL: Final safety check - never replace with empty string
                    if (!string.IsNullOrEmpty(translated))
                    {
                        UnityEngine.Debug.Log($"{LOG_PREFIX} Translated: '{__result}' -> '{translated}'");
                        __result = translated;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"{LOG_PREFIX} Empty translation for '{blueprint}', keeping original: '{__result}'");
                    }
                }
            }
            catch (Exception ex)
            {
                // Never crash the game - log and continue
                UnityEngine.Debug.LogError($"{LOG_PREFIX} GetFor_Postfix error: {ex.Message}");
            }
        }
        
        private static int _logCount = 0;
        
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
