/*
 * 파일명: Tooltip_Patch.cs
 * 분류: [UI Patch] 툴팁 번역 패치
 * 역할: ModelShark Tooltip 시스템의 텍스트를 번역합니다.
 */

using System;
using HarmonyLib;
using ModelShark;
using UnityEngine;
using QudKRTranslation.Utils;
using QudKRTranslation;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(TooltipTrigger))]
    public static class Tooltip_Patch
    {
        [HarmonyPatch(nameof(TooltipTrigger.SetText), new System.Type[] { typeof(string), typeof(string) })]
        [HarmonyPrefix]
        static void SetText_Prefix(ref string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            // 현재 활성 Scope 가져오기
            var scope = ScopeManager.GetCurrentScope();
            if (scope == null) return;

            // 태그를 보존하며 번역 시도
            if (TranslationUtils.TryTranslatePreservingTags(text, out string translated, scope))
            {
                text = translated;
            }
        }

        // Postfix: SetText 호출 이후 툴팁 내부 텍스트에 한글 폰트가 적용되도록 함
        [HarmonyPatch(nameof(TooltipTrigger.SetText), new System.Type[] { typeof(string), typeof(string) })]
        [HarmonyPostfix]
        static void SetText_Postfix(TooltipTrigger __instance)
        {
            ApplyKoreanFontToTooltip(__instance);
        }
        
        /// <summary>
        /// 툴팁 팝업 GameObject의 모든 TMP 컴포넌트에 한글 폰트 fallback 적용
        /// 주의: TooltipTrigger != Tooltip - 트리거와 팝업은 별도 계층
        /// </summary>
        public static void ApplyKoreanFontToTooltip(TooltipTrigger trigger)
        {
            try
            {
                var koreanFont = QudKRTranslation.Core.FontManager.GetKoreanTMPFont();
                if (koreanFont == null)
                {
                    return;
                }

                // 1. 트리거의 Tooltip 프로퍼티에서 실제 팝업 GameObject 가져오기
                var tooltip = trigger?.Tooltip;
                if (tooltip?.GameObject == null)
                {
                    return;
                }

                // 2. 툴팁 팝업 내부의 모든 TMP 컴포넌트에 한글 폰트 fallback 적용
                var tmps = tooltip.GameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                int applied = 0;
                foreach (var tmp in tmps)
                {
                    if (tmp == null) continue;
                    try
                    {
                        if (tmp.font != null)
                        {
                            if (tmp.font.fallbackFontAssetTable == null)
                                tmp.font.fallbackFontAssetTable = new System.Collections.Generic.List<TMPro.TMP_FontAsset>();
                            if (!tmp.font.fallbackFontAssetTable.Contains(koreanFont))
                            {
                                tmp.font.fallbackFontAssetTable.Insert(0, koreanFont);
                                applied++;
                            }
                        }
                        else
                        {
                            tmp.font = koreanFont;
                            applied++;
                        }
                        tmp.SetAllDirty();
                    }
                    catch { }
                }

                if (applied > 0)
                {
                    Debug.Log($"[Qud-KR] Tooltip: Applied Korean font to {applied} TMP components in popup.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] Tooltip font exception: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// TooltipTrigger.ShowManually 패치 - 툴팁 표시 시점에 한글 폰트 적용
    /// </summary>
    [HarmonyPatch(typeof(TooltipTrigger))]
    public static class Tooltip_ShowManually_Patch
    {
        [HarmonyPatch(nameof(TooltipTrigger.ShowManually), new System.Type[] { typeof(bool), typeof(UnityEngine.Vector3), typeof(bool), typeof(bool) })]
        [HarmonyPostfix]
        static void ShowManually_Postfix(TooltipTrigger __instance)
        {
            Tooltip_Patch.ApplyKoreanFontToTooltip(__instance);
        }
    }
}
