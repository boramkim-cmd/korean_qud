/*
 * 파일명: 99_99_TorchTest.cs
 * 분류: [Test] Torch 한글 표시 테스트 및 Tooltip 패치
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using XRL.UI;
using Qud.UI;
using QudKRTranslation.Core;

namespace QudKRTranslation.Test
{
    /// <summary>
    /// UITextSkin.Apply() 패치 - 안전한 방식
    /// </summary>
    [HarmonyPatch(typeof(UITextSkin), "Apply")]
    public static class Patch_UITextSkin_Apply_Test
    {
        [HarmonyPostfix]
        static void Postfix(UITextSkin __instance)
        {
            try
            {
                var tmp = __instance.GetComponent<TextMeshProUGUI>();
                if (tmp == null) return;

                string text = tmp.text;
                if (string.IsNullOrEmpty(text)) return;

                // 한글 체크
                bool hasKorean = false;
                foreach (char c in text)
                {
                    if (c >= 0xAC00 && c <= 0xD7A3)
                    {
                        hasKorean = true;
                        break;
                    }
                }

                if (!hasKorean) return;

                var koreanFont = FontManager.GetKoreanFont();
                if (koreanFont == null)
                {
                    Debug.LogWarning("[TorchTest] Korean font NULL");
                    return;
                }

                // Fallback 추가
                var currentFont = tmp.font;
                if (currentFont != null && currentFont != koreanFont)
                {
                    if (currentFont.fallbackFontAssetTable == null)
                        currentFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();

                    if (!currentFont.fallbackFontAssetTable.Contains(koreanFont))
                    {
                        currentFont.fallbackFontAssetTable.Insert(0, koreanFont);
                        Debug.Log($"[TorchTest] Fallback added to {currentFont.name}");
                    }
                }

                // 강제 갱신
                tmp.SetAllDirty();
                tmp.ForceMeshUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TorchTest] Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// TooltipManager.SetTextAndSize() 패치
    /// Tooltip은 UITextSkin을 거치지 않고 직접 TMP에 텍스트를 설정하므로
    /// 별도 패치가 필요함
    /// </summary>
    [HarmonyPatch(typeof(ModelShark.TooltipManager), "SetTextAndSize")]
    public static class Patch_TooltipManager_SetTextAndSize
    {
        [HarmonyPostfix]
        static void Postfix(ModelShark.TooltipTrigger trigger)
        {
            try
            {
                var tooltip = trigger?.Tooltip;
                if (tooltip?.TMPFields == null) return;

                var koreanFont = FontManager.GetKoreanFont();
                if (koreanFont == null) return;

                foreach (var field in tooltip.TMPFields)
                {
                    var tmp = field?.Text;
                    if (tmp == null) continue;

                    string text = tmp.text;
                    if (string.IsNullOrEmpty(text)) continue;

                    // 한글 포함 여부 확인
                    if (!ContainsKorean(text)) continue;

                    var currentFont = tmp.font;
                    if (currentFont == null || currentFont == koreanFont) continue;

                    // Fallback 추가
                    if (currentFont.fallbackFontAssetTable == null)
                        currentFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

                    if (!currentFont.fallbackFontAssetTable.Contains(koreanFont))
                    {
                        currentFont.fallbackFontAssetTable.Insert(0, koreanFont);
                        Debug.Log($"[TooltipPatch] Fallback added to {currentFont.name}");
                    }

                    // 강제 갱신
                    tmp.SetAllDirty();
                    tmp.ForceMeshUpdate();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[TooltipPatch] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// 문자열에 한글이 포함되어 있는지 확인
        /// </summary>
        static bool ContainsKorean(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                // 한글 완성형 범위: 0xAC00 ~ 0xD7A3
                if (c >= 0xAC00 && c <= 0xD7A3) return true;
            }
            return false;
        }
    }

    /// <summary>
    /// InventoryLine.setData() 패치 - 인벤토리 항목에 직접 한글 폰트 강제 적용
    /// </summary>
    [HarmonyPatch(typeof(Qud.UI.InventoryLine), "setData")]
    public static class Patch_InventoryLine_setData
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.InventoryLine __instance)
        {
            try
            {
                var koreanFont = FontManager.GetKoreanFont();
                if (koreanFont == null) return;

                // 인벤토리 라인의 모든 TMP 컴포넌트에 강제로 한글 폰트 fallback 적용
                var allTMPs = __instance.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in allTMPs)
                {
                    if (tmp == null) continue;

                    var currentFont = tmp.font;
                    if (currentFont == null)
                    {
                        tmp.font = koreanFont;
                        tmp.SetAllDirty();
                        tmp.ForceMeshUpdate();
                        continue;
                    }

                    if (currentFont == koreanFont) continue;

                    // Fallback 추가
                    if (currentFont.fallbackFontAssetTable == null)
                        currentFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

                    if (!currentFont.fallbackFontAssetTable.Contains(koreanFont))
                    {
                        currentFont.fallbackFontAssetTable.Insert(0, koreanFont);
                        Debug.Log($"[InventoryPatch] Fallback added to {currentFont.name} on {tmp.gameObject.name}");
                    }

                    // 강제 갱신
                    tmp.SetAllDirty();
                    tmp.ForceMeshUpdate();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InventoryPatch] Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// TextMeshProUGUI.SetText() 패치 - 모든 TMP 텍스트 설정 시 한글 폰트 강제 적용
    /// </summary>
    [HarmonyPatch(typeof(TextMeshProUGUI), "SetText", new Type[] { typeof(string) })]
    public static class Patch_TMP_SetText
    {
        [HarmonyPostfix]
        static void Postfix(TextMeshProUGUI __instance, string sourceText)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceText)) return;
                KoreanFontHelper.ApplyKoreanFontIfNeeded(__instance, sourceText);
            }
            catch { }
        }
    }

    /// <summary>
    /// TMP_Text.text setter 패치 - tmp.text = "한글" 형태로 설정될 때도 처리
    /// UITextSkin.Apply()에서 tmp.text = formattedText 로 설정하는 경우 처리
    /// </summary>
    [HarmonyPatch(typeof(TMP_Text), "set_text")]
    public static class Patch_TMP_Text_Setter
    {
        [HarmonyPostfix]
        static void Postfix(TMP_Text __instance)
        {
            try
            {
                string text = __instance.text;
                if (string.IsNullOrEmpty(text)) return;
                KoreanFontHelper.ApplyKoreanFontIfNeeded(__instance, text);
            }
            catch { }
        }
    }

    /// <summary>
    /// 공통 헬퍼 클래스: TMP에 한글 폰트 fallback 적용
    /// </summary>
    public static class KoreanFontHelper
    {
        /// <summary>
        /// TMP에 한글이 포함되어 있으면 폰트 fallback 적용
        /// </summary>
        public static void ApplyKoreanFontIfNeeded(TMP_Text tmp, string text)
        {
            if (tmp == null || string.IsNullOrEmpty(text)) return;

            // 한글 체크
            bool hasKorean = false;
            foreach (char c in text)
            {
                if (c >= 0xAC00 && c <= 0xD7A3)
                {
                    hasKorean = true;
                    break;
                }
            }
            if (!hasKorean) return;

            var koreanFont = FontManager.GetKoreanFont();
            if (koreanFont == null) return;

            var currentFont = tmp.font;
            if (currentFont == null || currentFont == koreanFont) return;

            // Fallback 추가
            if (currentFont.fallbackFontAssetTable == null)
                currentFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

            if (!currentFont.fallbackFontAssetTable.Contains(koreanFont))
            {
                currentFont.fallbackFontAssetTable.Insert(0, koreanFont);
            }

            tmp.SetAllDirty();
            tmp.ForceMeshUpdate();
        }
    }
}
