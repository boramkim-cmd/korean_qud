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
using System.Text.RegularExpressions;
using HarmonyLib;
using XRL;
using XRL.World;
using QudKorean.Objects.V2;
using QudKorean.Objects.V2.Processing;
using QudKRTranslation.Patches;

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

        // 최종 안전망: "[X dram(s) of Y]" 패턴을 한글로 변환
        private static readonly Regex RxDramFinal = new Regex(
            @"\[(\d+) drams? of ([^\]]+)\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

                // Skip during world generation — no UI visible, pipeline is O(n) expensive
                if (WorldGenActivityIndicator.IsWorldGenActive) return;

                // Basic validation
                if (Object == null || string.IsNullOrEmpty(__result)) return;
                
                string blueprint = Object.Blueprint;
                if (string.IsNullOrEmpty(blueprint)) return;

                // 모든 오브젝트 번역 (테스트 모드 해제됨)

                // Attempt translation
                bool translationSucceeded = ObjectTranslatorV2.TryGetDisplayName(blueprint, __result, out string translated);

                if (translationSucceeded)
                {
                    // CRITICAL: Final safety check - never replace with empty string
                    if (!string.IsNullOrEmpty(translated))
                    {
                        __result = translated;
                    }
                }

                // 후처리 (번역 성공/실패 무관)

                // "data disk:" → "데이터 디스크:"
                if (__result.Contains("data disk:"))
                    __result = __result.Replace("data disk:", "데이터 디스크:");

                // "[X dram(s) of Y]" 최종 안전망 — 어떤 번역 경로를 탔든 dram 패턴 한글화
                if (__result.Contains(" dram"))
                {
                    __result = RxDramFinal.Replace(__result, m =>
                    {
                        string amount = m.Groups[1].Value;
                        string liquid = m.Groups[2].Value.Trim();
                        // 색상태그 strip 후 액체명 (이미 부분 번역되어 있을 수 있음)
                        string liquidClean = ColorTagProcessor.Strip(liquid);
                        return $"[{liquidClean} {amount}드램]";
                    });
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
            ObjectTranslatorV2.ClearCache();
        }
        
        /// <summary>
        /// Reloads JSON files and clears cache. Used by kr:reload wish command.
        /// </summary>
        public static void ReloadAndClear()
        {
            ObjectTranslatorV2.ReloadJson();
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
