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
using XRL.UI;

namespace QudKRTranslation.Patches
{
    /// <summary>
    /// TooltipManager.SetTextAndSize 패치 - 핵심 번역 지점
    /// 이 메서드는 툴팁 텍스트가 설정된 직후 동기적으로 실행됩니다.
    /// ShowManually는 코루틴을 시작하므로 Postfix 타이밍이 맞지 않습니다.
    /// </summary>
    [HarmonyPatch(typeof(TooltipManager))]
    public static class TooltipManager_SetTextAndSize_Patch
    {
        // Tooltip header translations (case-insensitive, no duplicates)
        private static readonly Dictionary<string, string> HeaderTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "This Item", "현재 아이템" },
            { "Equipped Item", "장착 아이템" }
        };
        
        [HarmonyPatch(nameof(TooltipManager.SetTextAndSize))]
        [HarmonyPostfix]
        static void SetTextAndSize_Postfix(TooltipTrigger trigger)
        {
            try
            {
                if (trigger?.Tooltip?.GameObject == null) return;
                
                // 1. Translate static header text (This Item / Equipped Item)
                TranslateTooltipHeaders(trigger.Tooltip.GameObject);

                // 2. Item Name/Description translation is now handled by Look_GenerateTooltipInformation_Patch
                //    (The old TranslateInventoryTooltip method was removed because it always got null InventoryLine)

                // 3. Apply Korean font fallback
                ApplyKoreanFontToTooltip(trigger.Tooltip.GameObject);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] SetTextAndSize patch error: {ex.Message}");
            }
        }
        
        private static void TranslateTooltipHeaders(GameObject tooltipObj)
        {
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
        
        private static void ApplyKoreanFontToTooltip(GameObject tooltipObj)
        {
            var koreanFont = QudKRTranslation.Core.FontManager.GetKoreanTMPFont();
            if (koreanFont == null) return;
            
            var tmps = tooltipObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                if (tmp?.font == null) continue;
                
                if (tmp.font.fallbackFontAssetTable == null)
                    tmp.font.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    
                if (!tmp.font.fallbackFontAssetTable.Contains(koreanFont))
                {
                    tmp.font.fallbackFontAssetTable.Insert(0, koreanFont);
                    tmp.SetAllDirty();
                }
            }
        }
    }

    /// <summary>
    /// Look.GenerateTooltipInformation 패치 - 핵심 툴팁 번역 지점
    /// 이 메서드는 툴팁에 표시될 아이템 정보(이름, 설명)를 생성합니다.
    /// BaseLineWithTooltip.StartTooltip()이 이 메서드를 호출하여 TooltipInformation을 얻습니다.
    /// RTF 변환 전에 한글 번역을 적용하므로, 모든 툴팁 경로에서 동작합니다.
    /// </summary>
    [HarmonyPatch(typeof(Look), "GenerateTooltipInformation")]
    public static class Look_GenerateTooltipInformation_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref Look.TooltipInformation __result, XRL.World.GameObject go)
        {
            try
            {
                if (go == null) return;

                string blueprint = go.Blueprint;
                if (string.IsNullOrEmpty(blueprint)) return;

                // 1. 이름 번역 (DisplayName)
                if (!string.IsNullOrEmpty(__result.DisplayName))
                {
                    if (ObjectTranslator.TryGetDisplayName(blueprint, __result.DisplayName, out string nameKo))
                    {
                        __result.DisplayName = nameKo;
                        Debug.Log($"[Qud-KR][TooltipInfo] Translated DisplayName: '{nameKo}' (Blueprint: {blueprint})");
                    }
                }

                // 2. 설명 번역 (LongDescription)
                if (!string.IsNullOrEmpty(__result.LongDescription))
                {
                    if (ObjectTranslator.TryTranslateDescriptionExact(blueprint, __result.LongDescription, out string descKo))
                    {
                        __result.LongDescription = descKo;
                        Debug.Log($"[Qud-KR][TooltipInfo] Translated LongDescription for: {blueprint}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] Look.GenerateTooltipInformation patch error: {ex.Message}");
            }
        }
    }

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
    }
}
