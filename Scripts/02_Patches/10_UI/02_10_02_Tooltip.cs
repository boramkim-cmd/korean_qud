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
    /// TooltipTrigger.ShowManually 패치 - 툴팁 표시 시점에 한글 폰트 적용 및 헤더 번역
    /// 이 패치는 모든 툴팁 표시 경로를 커버합니다:
    /// - BaseLineWithTooltip.StartTooltip (인벤토리 아이템 비교)
    /// - Look.QueueLookerTooltip (월드맵 타일/오브젝트 클릭)
    /// - Look.ShowItemTooltipAsync (일반 아이템 툴팁)
    /// </summary>
    [HarmonyPatch(typeof(TooltipTrigger))]
    public static class Tooltip_ShowManually_Patch
    {
        // Tooltip header translations - centralized here for all tooltip types
        private static readonly Dictionary<string, string> HeaderTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "This Item", "현재 아이템" },
            { "Equipped Item", "장착 아이템" },
            { "this item", "현재 아이템" },
            { "equipped item", "장착 아이템" }
        };
        
        [HarmonyPatch(nameof(TooltipTrigger.ShowManually), new System.Type[] { typeof(bool), typeof(UnityEngine.Vector3), typeof(bool), typeof(bool) })]
        [HarmonyPostfix]
        static void ShowManually_Postfix(TooltipTrigger __instance)
        {
            try
            {
                Debug.Log("[Qud-KR] ShowManually_Postfix called");
                
                // 1. Translate headers if present (for compareLookerTooltip)
                TranslateTooltipHeaders(__instance);
                
                // 2. Apply Korean font fallback to all text components
                Tooltip_Patch.ApplyKoreanFontToTooltip(__instance);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] ShowManually patch error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Find and translate header text elements in the tooltip.
        /// Safe to call on any tooltip - will only translate if matching text found.
        /// </summary>
        private static void TranslateTooltipHeaders(TooltipTrigger trigger)
        {
            var tooltipObj = trigger?.Tooltip?.GameObject;
            if (tooltipObj == null)
            {
                Debug.Log("[Qud-KR] TranslateTooltipHeaders: tooltipObj is null");
                return;
            }
            
            var textComponents = tooltipObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            Debug.Log($"[Qud-KR] TranslateTooltipHeaders: found {textComponents.Length} text components");
            
            foreach (var textComponent in textComponents)
            {
                if (textComponent == null || string.IsNullOrEmpty(textComponent.text))
                    continue;
                
                string trimmedText = textComponent.text.Trim();
                
                // Check if this is a header that needs translation
                if (HeaderTranslations.TryGetValue(trimmedText, out string koreanText))
                {
                    Debug.Log($"[Qud-KR] Translating header: '{trimmedText}' -> '{koreanText}'");
                    textComponent.text = koreanText;
                    textComponent.SetAllDirty();
                }
            }
        }
    }
    
    /// <summary>
    /// BaseLineWithTooltip.StartTooltip 패치 - 보조 안전망
    /// NOTE: ShowManually_Patch에서 이미 처리하지만, StartTooltip이 ShowManually 전에 
    /// 텍스트를 설정하므로 여기서도 한 번 더 적용합니다.
    /// 중복 적용되어도 안전합니다 (idempotent).
    /// </summary>
    [HarmonyPatch(typeof(BaseLineWithTooltip))]
    public static class BaseLineWithTooltip_StartTooltip_Patch
    {
        [HarmonyPatch("StartTooltip", new System.Type[] { 
            typeof(XRL.World.GameObject), 
            typeof(XRL.World.GameObject), 
            typeof(bool), 
            typeof(RectTransform) })]
        [HarmonyPostfix]
        static void StartTooltip_Postfix(TooltipTrigger ___tooltip)
        {
            // ShowManually_Patch에서 처리하므로 여기서는 추가 작업 불필요
            // StartTooltip 끝에서 ShowManually가 호출되어 해당 패치가 실행됨
        }
    }
}
