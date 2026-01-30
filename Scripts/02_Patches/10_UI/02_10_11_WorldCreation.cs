/*
 * 파일명: 02_10_11_WorldCreation.cs
 * 분류: [UI Patch] 월드 생성 화면
 * 역할: "Creating World" 화면의 진행 메시지를 번역하고, 활성 표시기(점 애니메이션 + 경과 시간)를 표시합니다.
 * 작성일: 2026-01-21
 *
 * 구조:
 * 1. Modern UI (WorldGenerationScreen) 메시지 번역
 * 2. Legacy UI (WorldCreationProgress) 메시지 번역
 * 3. WorldGenActivityIndicator - ControlManager.Update 기반 점 애니메이션 오버레이
 *    (Draw()는 워커 스레드에서 호출되어 Unity UI 생성 불가 → 메인 스레드인 Update()에서 처리)
 *
 * 성능 주의:
 * IsWorldGenActive 플래그를 통해 DisplayNamePatch, GlobalUI 등 무거운 패치가
 * 세계 생성 중 스킵되도록 합니다.
 */

using System;
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

            LocalizationManager.Initialize();

            string key = message.Trim().TrimEnd('.');

            if (LocalizationManager.TryGetAnyTerm(key, out string translated, "worldgen", "ui"))
            {
                message = translated;
            }

            WorldGenActivityIndicator.OnMessage(message);
        }
    }

    // ========================================================================
    // STEP 2: Legacy UI (XRL.UI.WorldCreationProgress) 패치
    // ========================================================================

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
            return AccessTools.Method(type, "Begin", new Type[] { typeof(int) });
        }

        [HarmonyPrefix]
        static void Prefix()
        {
            WorldGenActivityIndicator.SetActive(true);
        }
    }

    [HarmonyPatch]
    public static class Patch_WorldCreationProgress_NextStep
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("XRL.UI.WorldCreationProgress");
            if (type == null) return null;
            return AccessTools.Method(type, "NextStep", new Type[] { typeof(string), typeof(int) });
        }

        [HarmonyPrefix]
        static void Prefix(ref string Text)
        {
            if (string.IsNullOrEmpty(Text)) return;

            LocalizationManager.Initialize();

            string key = Text.Trim().TrimEnd('.');

            if (LocalizationManager.TryGetAnyTerm(key, out string translated, "worldgen", "ui"))
            {
                Text = translated;
            }
        }
    }

    [HarmonyPatch]
    public static class Patch_WorldCreationProgress_StepProgress
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("XRL.UI.WorldCreationProgress");
            if (type == null) return null;
            return AccessTools.Method(type, "StepProgress", new Type[] { typeof(string), typeof(bool) });
        }

        [HarmonyPrefix]
        static void Prefix(ref string StepText)
        {
            if (string.IsNullOrEmpty(StepText) || StepText.Trim().Length == 0) return;

            LocalizationManager.Initialize();

            string key = StepText.Trim().TrimEnd('.');

            if (LocalizationManager.TryGetAnyTerm(key, out string translated, "worldgen", "ui"))
            {
                StepText = translated;
            }
        }
    }

    // ========================================================================
    // STEP 3: 활성 표시기 - ControlManager.Update() 메인 스레드에서 오버레이 관리
    // + IsWorldGenActive 플래그로 다른 무거운 패치들 스킵
    // ========================================================================

    [HarmonyPatch(typeof(ControlManager), "Update")]
    public static class WorldGenActivityIndicator
    {
        private static bool _worldGenActive;
        private static float _startTime;
        private static float _lastDotUpdate;
        private static int _dotPhase;

        private static GameObject _overlayCanvas;
        private static TextMeshProUGUI _statusText;

        private const float DOT_INTERVAL = 0.4f;
        private static readonly string[] DOT_FRAMES = { "●", "● ●", "● ● ●" };

        /// <summary>
        /// 다른 패치에서 세계 생성 중인지 확인하여 무거운 작업을 스킵하는 데 사용
        /// </summary>
        public static bool IsWorldGenActive => _worldGenActive;

        public static void SetActive(bool active)
        {
            _worldGenActive = active;
            if (active)
            {
                _startTime = Time.realtimeSinceStartup;
                _lastDotUpdate = 0f;
                _dotPhase = 0;
                Debug.Log("[Qud-KR] World generation started - heavy patches suspended");
            }
            else
            {
                float duration = Time.realtimeSinceStartup - _startTime;
                Debug.Log($"[Qud-KR] World generation ended ({duration:F1}s) - stats: {QudKorean.Objects.V2.ObjectTranslatorV2.GetStats()}");
            }
        }

        public static void OnMessage(string message)
        {
            if (!_worldGenActive) return;
            if (string.IsNullOrEmpty(message)) return;

            string lower = message.ToLowerInvariant();
            if (lower.Contains("complete") || lower.Contains("done") || lower.Contains("finished") ||
                lower.Contains("완료"))
            {
                SetActive(false);
            }
        }

        [HarmonyPostfix]
        static void Postfix()
        {
            if (!_worldGenActive)
            {
                if (_overlayCanvas != null)
                    DestroyOverlay();
                return;
            }

            // 플레이어 오브젝트 존재 = 세계 생성 완료
            try
            {
                if (XRL.The.Player != null)
                {
                    SetActive(false);
                    return;
                }
            }
            catch { }

            float elapsed = Time.realtimeSinceStartup - _startTime;

            if (_overlayCanvas == null)
            {
                CreateOverlay();
                if (_overlayCanvas == null) return;
            }

            if (Time.realtimeSinceStartup - _lastDotUpdate >= DOT_INTERVAL)
            {
                _lastDotUpdate = Time.realtimeSinceStartup;
                _dotPhase = (_dotPhase + 1) % DOT_FRAMES.Length;
                int seconds = Mathf.FloorToInt(elapsed);
                _statusText.text = $"세계 생성 중 {DOT_FRAMES[_dotPhase]}  ({seconds}초)";
            }
        }

        private static void CreateOverlay()
        {
            try
            {
                _overlayCanvas = new GameObject("WorldGenActivityOverlay");
                var canvas = _overlayCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999;

                _overlayCanvas.AddComponent<CanvasScaler>();

                var textObj = new GameObject("StatusText");
                textObj.transform.SetParent(_overlayCanvas.transform, false);

                _statusText = textObj.AddComponent<TextMeshProUGUI>();
                _statusText.text = "세계 생성 중 ●  (0초)";
                _statusText.fontSize = 20;
                _statusText.alignment = TextAlignmentOptions.BottomRight;
                _statusText.color = new Color(0.5f, 1f, 0.5f, 0.8f);

                var rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(1f, 0f);
                rt.anchoredPosition = new Vector2(-20f, 20f);
                rt.sizeDelta = new Vector2(400, 40);

                Debug.Log("[Qud-KR] WorldGen activity indicator created");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Qud-KR] Failed to create WorldGen activity overlay: {ex.Message}");
                _overlayCanvas = null;
            }
        }

        private static void DestroyOverlay()
        {
            if (_overlayCanvas != null)
            {
                UnityEngine.Object.Destroy(_overlayCanvas);
                _overlayCanvas = null;
                _statusText = null;
                Debug.Log("[Qud-KR] WorldGen activity indicator destroyed");
            }
        }
    }
}
