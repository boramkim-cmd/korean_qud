/*
 * 파일명: 02_10_11_WorldCreation.cs
 * 분류: [UI Patch] 월드 생성 화면
 * 역할: "Creating World" 화면의 진행 메시지를 번역하고 한글 폰트를 적용합니다.
 * 작성일: 2026-01-21
 * 
 * 구조:
 * 1. Modern UI (WorldGenerationScreen) 메시지 번역
 * 2. Legacy UI (WorldCreationProgress) 메시지 번역 + TMP 오버레이
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QudKRTranslation.Core;

namespace QudKRTranslation.Patches
{
    // ========================================================================
    // STEP 1: Modern UI (Qud.UI.WorldGenerationScreen) 메시지 번역
    // TMP_Text를 사용하므로 기존 폰트 패치가 자동 적용됨
    // ========================================================================
    
    /// <summary>
    /// WorldGenerationScreen._AddMessage 패치 - 진행 메시지 번역
    /// </summary>
    [HarmonyPatch]
    public static class Patch_WorldGenerationScreen_AddMessage
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("Qud.UI.WorldGenerationScreen");
            if (type == null)
            {
                Debug.LogWarning("[Qud-KR] WorldGenerationScreen type not found");
                return null;
            }
            return AccessTools.Method(type, "_AddMessage");
        }
        
        [HarmonyPrefix]
        static void Prefix(ref string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            
            // Ensure localization is loaded
            LocalizationManager.Initialize();
            
            string originalMessage = message;
            string key = message.Trim().TrimEnd('.').ToLowerInvariant();
            
            Debug.Log($"[Qud-KR WorldGen] _AddMessage called with: '{originalMessage}', key='{key}'");
            
            if (LocalizationManager.TryGetAnyTerm(key, out string translated, "worldgen", "ui"))
            {
                message = translated;
                Debug.Log($"[Qud-KR WorldGen] Translated: '{key}' -> '{translated}'");
            }
            else
            {
                Debug.Log($"[Qud-KR WorldGen] No translation found for key: '{key}'");
            }
        }
    }
    
    /// <summary>
    /// WorldGenerationScreen.SetQuote 패치 - 인용문 번역
    /// </summary>
    [HarmonyPatch]
    public static class Patch_WorldGenerationScreen_SetQuote
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("Qud.UI.WorldGenerationScreen");
            if (type == null) return null;
            return AccessTools.Method(type, "SetQuote");
        }
        
        [HarmonyPrefix]
        static void Prefix(ref string quote, ref string source)
        {
            // Quote 번역
            if (!string.IsNullOrEmpty(quote))
            {
                if (LocalizationManager.TryGetAnyTerm(quote, out string translatedQuote, "worldgen", "quotes"))
                {
                    quote = translatedQuote;
                }
            }
            
            // Source 번역 (예: "-Ekuemekiyyen inscription")
            if (!string.IsNullOrEmpty(source))
            {
                if (LocalizationManager.TryGetAnyTerm(source, out string translatedSource, "worldgen", "quotes"))
                {
                    source = translatedSource;
                }
            }
        }
    }
    
    // ========================================================================
    // STEP 3: Legacy UI (XRL.UI.WorldCreationProgress) 패치 + TMP 오버레이
    // 스프라이트 기반이므로 TMP 오버레이를 생성하여 한글 표시
    // ========================================================================
    
    /// <summary>
    /// WorldCreationProgress.Begin 패치 - 시작 시 ModernUI 강제 활성화
    /// </summary>
    [HarmonyPatch]
    public static class Patch_WorldCreationProgress_Begin
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("XRL.UI.WorldCreationProgress");
            if (type == null)
            {
                Debug.LogWarning("[Qud-KR] WorldCreationProgress type not found");
                return null;
            }
            return AccessTools.Method(type, "Begin");
        }
        
        [HarmonyPrefix]
        static void Prefix()
        {
            Debug.Log("[Qud-KR] WorldCreation: Begin called");
        }
    }
    
    /// <summary>
    /// WorldCreationProgress.End 패치 - 종료 시 ModernUI 강제 해제
    /// </summary>
    [HarmonyPatch]
    public static class Patch_WorldCreationProgress_End
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("XRL.UI.WorldCreationProgress");
            if (type == null) return null;
            return AccessTools.Method(type, "End");
        }
        
        [HarmonyPostfix]
        static void Postfix()
        {
            Debug.Log("[Qud-KR] WorldCreation: End called");
        }
    }
    
    /// <summary>
    /// WorldCreationProgress.NextStep 패치 - 진행 단계 텍스트 번역
    /// </summary>
    [HarmonyPatch]
    public static class Patch_WorldCreationProgress_NextStep
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("XRL.UI.WorldCreationProgress");
            if (type == null) return null;
            return AccessTools.Method(type, "NextStep");
        }
        
        [HarmonyPrefix]
        static void Prefix(ref string Text)
        {
            if (string.IsNullOrEmpty(Text)) return;
            
            LocalizationManager.Initialize();
            
            string originalText = Text;
            string key = Text.Trim().TrimEnd('.').ToLowerInvariant();
            
            Debug.Log($"[Qud-KR WorldGen] NextStep called with: '{originalText}', key='{key}'");
            
            if (LocalizationManager.TryGetAnyTerm(key, out string translated, "worldgen", "ui"))
            {
                Text = translated;
                Debug.Log($"[Qud-KR WorldGen] NextStep translated: '{key}' -> '{translated}'");
            }
        }
    }
    
    /// <summary>
    /// WorldCreationProgress.StepProgress 패치 - 세부 진행 텍스트 번역
    /// </summary>
    [HarmonyPatch]
    public static class Patch_WorldCreationProgress_StepProgress
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("XRL.UI.WorldCreationProgress");
            if (type == null) return null;
            return AccessTools.Method(type, "StepProgress");
        }
        
        [HarmonyPrefix]
        static void Prefix(ref string StepText)
        {
            if (string.IsNullOrEmpty(StepText) || StepText.Trim().Length == 0) return;
            
            LocalizationManager.Initialize();
            
            string originalText = StepText;
            string key = StepText.Trim().TrimEnd('.').ToLowerInvariant();
            
            Debug.Log($"[Qud-KR WorldGen] StepProgress called with: '{originalText}', key='{key}'");
            
            if (LocalizationManager.TryGetAnyTerm(key, out string translated, "worldgen", "ui"))
            {
                StepText = translated;
                Debug.Log($"[Qud-KR WorldGen] StepProgress translated: '{key}' -> '{translated}'");
            }
        }
    }
    
    /// <summary>
    /// WorldCreationProgress.Draw 패치 - 타이틀 "Creating World" 번역
    /// TMP 오버레이 생성하여 한글 표시
    /// </summary>
    [HarmonyPatch]
    public static class Patch_WorldCreationProgress_Draw
    {
        private static GameObject _overlayCanvas;
        private static TextMeshProUGUI _titleText;
        private static TextMeshProUGUI _progressText;
        
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("XRL.UI.WorldCreationProgress");
            if (type == null) return null;
            return AccessTools.Method(type, "Draw");
        }
        
        [HarmonyPostfix]
        static void Postfix()
        {
            // TMP 오버레이가 없으면 생성
            if (_overlayCanvas == null)
            {
                CreateOverlay();
            }
            
            // 타이틀 번역 적용
            if (_titleText != null)
            {
                string translatedTitle;
                if (LocalizationManager.TryGetAnyTerm("creating world", out translatedTitle, "worldgen", "ui"))
                {
                    _titleText.text = translatedTitle;
                }
                else
                {
                    _titleText.text = "세계 생성 중";
                }
            }
        }
        
        private static void CreateOverlay()
        {
            try
            {
                // Canvas 생성
                _overlayCanvas = new GameObject("WorldCreationKoreanOverlay");
                var canvas = _overlayCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999; // 최상위에 표시
                
                _overlayCanvas.AddComponent<CanvasScaler>();
                _overlayCanvas.AddComponent<GraphicRaycaster>();
                
                // 타이틀 텍스트 생성
                var titleObj = new GameObject("TitleText");
                titleObj.transform.SetParent(_overlayCanvas.transform, false);
                
                _titleText = titleObj.AddComponent<TextMeshProUGUI>();
                _titleText.text = "세계 생성 중";
                _titleText.fontSize = 48;
                _titleText.alignment = TextAlignmentOptions.Center;
                _titleText.color = new Color(0.81f, 0.75f, 0.25f, 1f); // #CFC041
                
                // 위치 설정 (화면 상단 중앙)
                var rectTransform = titleObj.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.8f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.8f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(800, 100);
                
                // 한글 폰트 적용
                if (FontManager.IsFontLoaded)
                {
                    var koreanFont = FontManager.GetKoreanFont();
                    if (koreanFont != null)
                    {
                        if (_titleText.font != null && _titleText.font.fallbackFontAssetTable != null)
                        {
                            if (!_titleText.font.fallbackFontAssetTable.Contains(koreanFont))
                            {
                                _titleText.font.fallbackFontAssetTable.Insert(0, koreanFont);
                            }
                        }
                    }
                }
                
                Debug.Log("[Qud-KR] Created WorldCreation TMP overlay for Korean text");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Qud-KR] Failed to create WorldCreation overlay: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 오버레이 정리 (월드 생성 완료 시)
        /// </summary>
        public static void DestroyOverlay()
        {
            if (_overlayCanvas != null)
            {
                UnityEngine.Object.Destroy(_overlayCanvas);
                _overlayCanvas = null;
                _titleText = null;
                _progressText = null;
            }
        }
    }
    
    /// <summary>
    /// 월드 생성 완료 시 오버레이 정리
    /// </summary>
    [HarmonyPatch]
    public static class Patch_WorldCreationProgress_Cleanup
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("XRL.UI.WorldCreationProgress");
            if (type == null) return null;
            return AccessTools.Method(type, "End");
        }
        
        [HarmonyPostfix]
        static void Postfix_Cleanup()
        {
            Patch_WorldCreationProgress_Draw.DestroyOverlay();
        }
    }
}
