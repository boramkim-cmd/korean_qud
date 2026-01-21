/*
 * 파일명: 02_10_15_Tutorial.cs
 * 분류: [UI Patch] 튜토리얼 번역 패치
 * 역할: TutorialManager의 텍스트를 번역합니다.
 */

using System;
using HarmonyLib;
using UnityEngine;
using QudKRTranslation.Core;
using QudKRTranslation.Utils;

namespace QudKRTranslation.Patches
{
    /// <summary>
    /// TutorialManager 패치 - 튜토리얼 팝업 및 하이라이트 텍스트 번역
    /// 
    /// 대상 메서드:
    /// - ShowCellPopup: 셀 위치에 팝업 표시
    /// - ShowCIDPopupAsync: Control ID 위치에 팝업 표시
    /// - ShowIntermissionPopupAsync: 중앙 팝업 표시
    /// - Highlight: UI 요소 하이라이트
    /// - HighlightByCID: Control ID로 하이라이트
    /// - HighlightCell: 게임 셀 하이라이트
    /// </summary>
    [HarmonyPatch(typeof(TutorialManager))]
    public static class Patch_TutorialManager
    {
        /// <summary>
        /// 튜토리얼 텍스트 번역 시도
        /// </summary>
        /// <param name="originalText">원본 영어 텍스트</param>
        /// <param name="translated">번역된 한글 텍스트</param>
        /// <returns>번역 성공 여부</returns>
        private static bool TryTranslateTutorial(string originalText, out string translated)
        {
            translated = originalText;
            
            if (string.IsNullOrEmpty(originalText))
                return false;
            
            Debug.Log($"[Qud-KR][Tutorial] TryTranslate called with: '{originalText.Substring(0, Math.Min(80, originalText.Length))}...'");
            
            // LocalizationManager에서 "tutorial" 카테고리의 딕셔너리를 가져옴
            LocalizationManager.Initialize();
            var tutorialScope = LocalizationManager.GetCategory("tutorial");
            
            if (tutorialScope == null)
            {
                Debug.LogWarning("[Qud-KR][Tutorial] Tutorial category not found! Check LOCALIZATION/GAMEPLAY/tutorial.json");
                return false;
            }
            
            Debug.Log($"[Qud-KR][Tutorial] Tutorial scope loaded with {tutorialScope.Count} entries");
            
            // ~Command 플레이스홀더와 색상 태그 보존하며 번역
            if (TranslationUtils.TryTranslatePreservingTags(originalText, out string result, tutorialScope))
            {
                translated = result;
                Debug.Log($"[Qud-KR][Tutorial] SUCCESS: '{originalText.Substring(0, Math.Min(50, originalText.Length))}...' -> '{result.Substring(0, Math.Min(50, result.Length))}...'");
                return true;
            }
            
            Debug.Log($"[Qud-KR][Tutorial] No translation found for: '{originalText.Substring(0, Math.Min(80, originalText.Length))}...'");
            return false;
        }
        
        // ========================================================================
        // ShowCellPopup - 셀 위치에 팝업 표시
        // ========================================================================
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialManager.ShowCellPopup))]
        static void ShowCellPopup_Prefix(ref string text)
        {
            if (TryTranslateTutorial(text, out var translated))
                text = translated;
        }
        
        // ========================================================================
        // ShowCIDPopupAsync - Control ID 위치에 비동기 팝업 표시
        // ========================================================================
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialManager.ShowCIDPopupAsync))]
        static void ShowCIDPopupAsync_Prefix(ref string text, ref string buttonText)
        {
            // 메시지 본문 번역
            if (TryTranslateTutorial(text, out var translatedText))
                text = translatedText;
            
            // 버튼 텍스트 번역 (예: "[~Accept] Continue" -> "[~Accept] 계속")
            if (TryTranslateTutorial(buttonText, out var translatedButton))
                buttonText = translatedButton;
        }
        
        // ========================================================================
        // ShowIntermissionPopupAsync - 중앙에 비동기 팝업 표시  
        // ========================================================================
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialManager.ShowIntermissionPopupAsync))]
        static void ShowIntermissionPopupAsync_Prefix(ref string message)
        {
            if (TryTranslateTutorial(message, out var translated))
                message = translated;
        }
        
        // ========================================================================
        // Highlight - UI 요소 하이라이트
        // ========================================================================
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialManager.Highlight))]
        static void Highlight_Prefix(ref string text)
        {
            // 특수 마커 체크 (<noframe>, <no message>, <nohighlight>)
            if (!string.IsNullOrEmpty(text) && 
                (text.Contains("<noframe>") || text.Contains("<no message>") || text.Contains("<nohighlight>")))
            {
                // 특수 마커는 번역하지 않음
                return;
            }
            
            if (TryTranslateTutorial(text, out var translated))
                text = translated;
        }
        
        // ========================================================================
        // HighlightByCID - Control ID로 하이라이트
        // ========================================================================
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialManager.HighlightByCID))]
        static void HighlightByCID_Prefix(ref string text)
        {
            // 특수 마커 체크
            if (!string.IsNullOrEmpty(text) && 
                (text.Contains("<noframe>") || text.Contains("<no message>") || text.Contains("<nohighlight>")))
            {
                return;
            }
            
            if (TryTranslateTutorial(text, out var translated))
                text = translated;
        }
        
        // ========================================================================
        // HighlightCell - 게임 셀 하이라이트
        // ========================================================================
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialManager.HighlightCell))]
        static void HighlightCell_Prefix(ref string text)
        {
            // 특수 마커 체크
            if (!string.IsNullOrEmpty(text) && 
                (text.Contains("<noframe>") || text.Contains("<no message>")))
            {
                return;
            }
            
            if (TryTranslateTutorial(text, out var translated))
                text = translated;
        }
    }
}
