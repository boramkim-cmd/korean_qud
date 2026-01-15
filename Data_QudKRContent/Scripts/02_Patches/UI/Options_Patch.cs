/*
 * 파일명: Options_Patch.cs
 * 분류: [UI Patch] 설정 화면 패치
 * 역할: 설정(Options) 화면이 열리고 닫힐 때 번역 범위를 관리합니다.
 *       UI가 처음 열릴 때와 검색 등으로 변경될 때 모든 UI 요소를 강제로 번역하여 유실을 방지합니다.
 */

using HarmonyLib;
using Qud.UI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using QudKRTranslation.Data;
using QudKRTranslation.Utils;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(OptionsScreen))]
    public static class Options_Patch
    {
        // Show: Scope push (옵션 전용 스코프만 사용하여 과도한 전역 번역 방지)
        [HarmonyPatch(nameof(OptionsScreen.Show), new System.Type[0])]
        [HarmonyPrefix]
        static void Show_Prefix()
        {
            if (!ScopeManager.IsScopeActive(OptionsData.Translations))
            {
                ScopeManager.PushScope(OptionsData.Translations);
                Debug.Log("[Options_Patch] Scope activated (Options only)");
            }
        }

        // Show Postfix: 최초 화면 표시 시 전체 번역 시도
        [HarmonyPatch(nameof(OptionsScreen.Show), new System.Type[0])]
        [HarmonyPostfix]
        static void Show_Postfix(OptionsScreen __instance)
        {
            TranslateAll(__instance);
        }

        // FilterItems: 검색 등으로 항목이 필터링/재구성된 후 번역 시도
        [HarmonyPatch(nameof(OptionsScreen.FilterItems))]
        [HarmonyPostfix]
        static void FilterItems_Postfix(OptionsScreen __instance)
        {
            TranslateAll(__instance);
        }

        // Finalizer: Show에서 예외가 발생했을 때 스코프가 남지 않도록 정리
        [HarmonyPatch(nameof(OptionsScreen.Show), new System.Type[0])]
        [HarmonyFinalizer]
        static void Show_Finalizer(System.Exception __exception)
        {
            if (__exception != null && ScopeManager.IsScopeActive(OptionsData.Translations))
            {
                ScopeManager.PopScope();
                Debug.LogWarning("[Options_Patch] Show finalizer popped scope after exception");
            }
        }

        // Hide: Scope pop
        [HarmonyPatch(nameof(OptionsScreen.Hide), new System.Type[0])]
        [HarmonyPostfix]
        static void Hide_Postfix()
        {
            if (ScopeManager.IsScopeActive(OptionsData.Translations))
            {
                ScopeManager.PopScope();
                Debug.Log("[Options_Patch] Scope deactivated");
            }
        }

        /// <summary>
        /// OptionsScreen 내의 모든 TMP_Text를 찾아 현재 스코프 우선순위에 따라 번역합니다.
        /// </summary>
        private static void TranslateAll(OptionsScreen screen)
        {
            try
            {
                if (screen == null) return;

                // 우선순위: OptionsData -> MainMenuData -> CommonData
                var scopes = new[] { 
                    OptionsData.Translations, 
                    QudKRTranslation.Data.MainMenuData.Translations, 
                    QudKRTranslation.Data.CommonData.Translations 
                };

                var texts = screen.GetComponentsInChildren<TMP_Text>(true);
                int applied = 0;
                foreach (var t in texts)
                {
                    if (t == null || string.IsNullOrEmpty(t.text)) continue;

                    // 제어값(숫자, On/Off, 체크박스 등)은 번역하지 않음
                    if (TranslationUtils.IsControlValue(t.text)) continue;

                    if (TranslationUtils.TryTranslatePreservingTags(t.text, out string translated, scopes))
                    {
                        if (t.text != translated)
                        {
                            t.text = translated;
                            applied++;
                        }
                    }
                }

                if (applied > 0)
                {
                    Debug.Log($"[Options_Patch] Internal translation applied to {applied}/{texts.Length} TMP_Text elements.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Options_Patch] TranslateAll exception: {ex.Message}");
            }
        }
    }
}
