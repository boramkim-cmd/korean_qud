/*
 * 파일명: Tooltip_Patch.cs
 * 분류: [UI Patch] 툴팁 번역 패치
 * 역할: ModelShark Tooltip 시스템의 텍스트를 번역합니다.
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using ModelShark;
using UnityEngine;
using TMPro;
using QudKRTranslation.Utils;
using QudKRTranslation;
using Qud.UI;
using QudKorean.Objects;

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
    
    /// <summary>
    /// BaseLineWithTooltip.StartTooltip 패치 - 아이템 비교 툴팁 헤더 번역
    /// </summary>
    [HarmonyPatch(typeof(BaseLineWithTooltip))]
    public static class BaseLineWithTooltip_StartTooltip_Patch
    {
        // Tooltip header translations
        private static readonly Dictionary<string, string> HeaderTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "This Item", "현재 아이템" },
            { "Equipped Item", "장착 아이템" },
            { "this item", "현재 아이템" },
            { "equipped item", "장착 아이템" }
        };
        
        [HarmonyPatch("StartTooltip", new System.Type[] { 
            typeof(XRL.World.GameObject), 
            typeof(XRL.World.GameObject), 
            typeof(bool), 
            typeof(RectTransform) })]
        [HarmonyPostfix]
        static void StartTooltip_Postfix(TooltipTrigger ___tooltip)
        {
            if (___tooltip == null) return;
            
            try
            {
                // 1. Translate header text components ("This Item", "Equipped Item")
                TranslateTooltipHeaders(___tooltip);
                
                // 2. Apply Korean font fallback to all text components
                ApplyKoreanFontToTooltipChildren(___tooltip);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] StartTooltip patch error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Find and translate header text elements in the tooltip
        /// </summary>
        private static void TranslateTooltipHeaders(TooltipTrigger trigger)
        {
            // CRITICAL: TooltipTrigger is the trigger component, actual tooltip UI is in Tooltip.GameObject
            var tooltipObj = trigger?.Tooltip?.GameObject;
            if (tooltipObj == null) return;
            
            var textComponents = tooltipObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            
            foreach (var textComponent in textComponents)
            {
                if (textComponent == null || string.IsNullOrEmpty(textComponent.text))
                    continue;
                
                string trimmedText = textComponent.text.Trim();
                
                // Check if this is a header that needs translation
                if (HeaderTranslations.TryGetValue(trimmedText, out string koreanText))
                {
                    textComponent.text = koreanText;
                    textComponent.SetAllDirty();
                }
            }
        }
        
        /// <summary>
        /// Apply Korean font fallback to all TMP components in the tooltip
        /// </summary>
        private static void ApplyKoreanFontToTooltipChildren(TooltipTrigger trigger)
        {
            var koreanFont = QudKRTranslation.Core.FontManager.GetKoreanTMPFont();
            if (koreanFont == null) return;
            
            // CRITICAL: TooltipTrigger is the trigger component, actual tooltip UI is in Tooltip.GameObject
            var tooltipObj = trigger?.Tooltip?.GameObject;
            if (tooltipObj == null) return;
            
            var textComponents = tooltipObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            
            foreach (var tmp in textComponents)
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
                        }
                    }
                    else
                    {
                        tmp.font = koreanFont;
                    }
                    tmp.SetAllDirty();
                }
                catch { }
            }
        }
    }
}
