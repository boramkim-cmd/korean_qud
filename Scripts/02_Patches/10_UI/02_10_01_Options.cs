/*
 * 파일명: 10_01_P_Options.cs
 * 분류: [UI Patch] 설정(Options) 화면 통합 패치
 * 역할: 데이터 로딩(LoadOptionNode) 및 UI 표시(OptionsScreen) 시점을 모두 패치하여 완벽한 번역을 제공합니다.
 *       기존 하드코딩 데이터 대신 LocalizationManager를 사용합니다.
 */

using HarmonyLib;
using Qud.UI;
using XRL.UI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using QudKRTranslation.Core; // LocalizationManager
using QudKRTranslation.Utils;
using System;
using QudKRTranslation;

namespace QudKRTranslation.Patches
{
    /// <summary>
    /// [1] 데이터 레벨 패치: GameOption 객체가 생성될 때 내용물(제목, 설명, 값)을 번역합니다.
    /// </summary>
    public static class Patch_OptionsData
    {
        // 개별 노드가 로드될 때마다 번역 적용
        [HarmonyPatch(typeof(Options), "LoadOptionNode")]
        public static class LoadOptionNode_Patch
        {
            [HarmonyPostfix]
            static void Postfix(GameOption __result)
            {
                try
                {
                    if (__result == null) return;
                    TranslateOption(__result);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Qud-KR] LoadOptionNode_Postfix 오류 (ID: {__result?.ID}): {e.Message}");
                }
            }
        }

        // 전체 옵션을 한 번에 로드한 이후 일괄 번역 (로딩 순서 안전)
        [HarmonyPatch(typeof(Options), "LoadAllOptions")]
        public static class LoadAllOptions_Patch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                try
                {
                    if (Options.OptionsByID == null) return;

                    int count = 0;
                    foreach (var kv in Options.OptionsByID)
                    {
                        var opt = kv.Value;
                        if (opt == null) continue;
                        TranslateOption(opt);
                        count++;
                    }
                    Debug.Log($"[Qud-KR] 모든 옵션 데이터 번역 완료: {count}개");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Qud-KR] LoadAllOptions_Postfix 오류: {e.Message}");
                }
            }
        }

        // 실제 번역 로직: DisplayText, HelpText, DisplayValues(콤보 표시값) 등
        public static void TranslateOption(GameOption opt)
        {
            if (opt == null) return;

            // 우선순위 스코프: options -> common -> ui
            var optionsDict = LocalizationManager.GetCategory("options");
            var displayDict = LocalizationManager.GetCategory("display"); // display 옵션 분리된 경우 대비
            var commonDict = LocalizationManager.GetCategory("common");

            var scopes = new List<Dictionary<string, string>>();
            if (optionsDict != null) scopes.Add(optionsDict);
            if (displayDict != null) scopes.Add(displayDict);
            if (commonDict != null) scopes.Add(commonDict);

            var scopeArray = scopes.ToArray();

            // DisplayText (타이틀)
            if (!string.IsNullOrEmpty(opt.DisplayText))
            {
                if (TranslationUtils.TryTranslatePreservingTags(opt.DisplayText, out string t, scopeArray))
                    opt.DisplayText = t;
            }

            // HelpText (설명 / 툴팁)
            if (!string.IsNullOrEmpty(opt.HelpText))
            {
                if (TranslationUtils.TryTranslatePreservingTags(opt.HelpText, out string h, scopeArray))
                    opt.HelpText = h;
            }

            // DisplayValues (콤보 박스의 표시값들)
            try
            {
                if (opt.Values != null && opt.Values.Length > 0 && opt.DisplayValues != null)
                {
                    for (int i = 0; i < opt.DisplayValues.Length; i++)
                    {
                        if (string.IsNullOrEmpty(opt.DisplayValues[i])) continue;
                        if (TranslationUtils.TryTranslatePreservingTags(opt.DisplayValues[i], out string dv, scopeArray))
                            opt.DisplayValues[i] = dv;
                    }
                }
            }
            catch { /* Ignore array index errors */ }
        }
    }

    /// <summary>
    /// [2] UI 레벨 패치: 화면이 표시될 때(Show) UI 요소들을 스캔하여 미처 번역되지 않은 텍스트를 처리합니다.
    /// </summary>
    [HarmonyPatch(typeof(OptionsScreen))]
    public static class Patch_OptionsUI
    {
        private static bool _scopePushed = false;

        // Show: Scope push (옵션 전용 스코프)
        [HarmonyPatch(nameof(OptionsScreen.Show), new System.Type[0])]
        [HarmonyPrefix]
        static void Show_Prefix()
        {
            if (!_scopePushed)
            {
                var optionsDict = LocalizationManager.GetCategory("options");
                var displayDict = LocalizationManager.GetCategory("display");

                var pushList = new List<Dictionary<string, string>>();
                if (optionsDict != null) pushList.Add(optionsDict);
                if (displayDict != null) pushList.Add(displayDict);

                if (pushList.Count > 0)
                {
                    ScopeManager.PushScope(pushList.ToArray());
                    _scopePushed = true;
                }
            }
        }

        // Show Postfix: UI 요소 강제 번역
        [HarmonyPatch(nameof(OptionsScreen.Show), new System.Type[0])]
        [HarmonyPostfix]
        static void Show_Postfix(OptionsScreen __instance)
        {
            TranslateAll(__instance);
        }

        // FilterItems: 검색 등으로 재구성될 때 번역
        [HarmonyPatch(nameof(OptionsScreen.FilterItems))]
        [HarmonyPostfix]
        static void FilterItems_Postfix(OptionsScreen __instance)
        {
            TranslateAll(__instance);
        }

        // Finalizer: 예외 발생 시 스코프 정리
        [HarmonyPatch(nameof(OptionsScreen.Show), new System.Type[0])]
        [HarmonyFinalizer]
        static void Show_Finalizer(System.Exception __exception)
        {
            if (__exception != null && _scopePushed)
            {
                ScopeManager.PopScope();
                _scopePushed = false;
            }
        }

        // Hide: Scope pop
        [HarmonyPatch(nameof(OptionsScreen.Hide), new System.Type[0])]
        [HarmonyPostfix]
        static void Hide_Postfix()
        {
            if (_scopePushed)
            {
                ScopeManager.PopScope();
                _scopePushed = false;
            }
        }

        private static void TranslateAll(OptionsScreen screen)
        {
            try
            {
                if (screen == null) return;

                // UI 스캔 시에는 options, display, ui, common 모두 사용
                var optionsDict = LocalizationManager.GetCategory("options");
                var displayDict = LocalizationManager.GetCategory("display");
                var uiDict = LocalizationManager.GetCategory("ui");
                var commonDict = LocalizationManager.GetCategory("common");

                var scopes = new List<Dictionary<string, string>>();
                if (optionsDict != null) scopes.Add(optionsDict);
                if (displayDict != null) scopes.Add(displayDict);
                if (uiDict != null) scopes.Add(uiDict);
                if (commonDict != null) scopes.Add(commonDict);

                if (scopes.Count == 0) return;
                var scopeArray = scopes.ToArray();

                var texts = screen.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in texts)
                {
                    if (t == null || string.IsNullOrEmpty(t.text)) continue;

                    if (TranslationUtils.IsControlValue(t.text)) continue;

                    if (TranslationUtils.TryTranslatePreservingTags(t.text, out string translated, scopeArray))
                    {
                        if (t.text != translated) t.text = translated;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Options_Patch] TranslateAll exception: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// [3] OptionsCategoryControl 패치: 오른쪽 패널의 카테고리 행(Sound, Display 등)을 번역합니다.
    /// </summary>
    [HarmonyPatch(typeof(OptionsCategoryControl))]
    public static class Patch_OptionsCategoryControl
    {
        private static Dictionary<string, string>[] _scopeArray = null;

        private static Dictionary<string, string>[] GetScopes()
        {
            if (_scopeArray == null)
            {
                var optionsDict = LocalizationManager.GetCategory("options");
                var commonDict = LocalizationManager.GetCategory("common");
                var scopes = new List<Dictionary<string, string>>();
                if (optionsDict != null) scopes.Add(optionsDict);
                if (commonDict != null) scopes.Add(commonDict);
                _scopeArray = scopes.ToArray();
            }
            return _scopeArray;
        }

        [HarmonyPatch(nameof(OptionsCategoryControl.Render))]
        [HarmonyPostfix]
        static void Render_Postfix(OptionsCategoryControl __instance)
        {
            try
            {
                if (__instance == null || __instance.data == null) return;
                if (__instance.title == null) return;

                // 원본 카테고리 이름 가져오기
                string originalTitle = __instance.data.Title;
                if (string.IsNullOrEmpty(originalTitle)) return;

                // 번역 시도
                var scopes = GetScopes();
                if (scopes == null || scopes.Length == 0) return;

                if (TranslationUtils.TryTranslatePreservingTags(originalTitle, out string translated, scopes))
                {
                    // 원본 Render는 "{{C|TITLE}}" 형식으로 설정하므로 동일하게 적용
                    __instance.title.SetText("{{C|" + translated.ToUpper() + "}}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] OptionsCategoryControl.Render_Postfix 오류: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// [4] LeftSideCategory 패치: 왼쪽 패널의 카테고리 이름(Sound, Display 등)을 번역합니다.
    /// 게임 원본: LeftSideCategory.setData()에서 optionsCategoryRow.CategoryId를 직접 텍스트로 설정
    /// 문제점: CategoryId는 XML Options.xml의 Category 속성에서 직접 가져온 영문 문자열
    /// 해결: setData Postfix에서 text.SetText() 호출 후 번역 적용
    /// </summary>
    [HarmonyPatch(typeof(LeftSideCategory))]
    public static class Patch_LeftSideCategory
    {
        private static Dictionary<string, string>[] _scopeArray = null;

        private static Dictionary<string, string>[] GetScopes()
        {
            if (_scopeArray == null)
            {
                var optionsDict = LocalizationManager.GetCategory("options");
                var commonDict = LocalizationManager.GetCategory("common");
                var scopes = new List<Dictionary<string, string>>();
                if (optionsDict != null) scopes.Add(optionsDict);
                if (commonDict != null) scopes.Add(commonDict);
                _scopeArray = scopes.ToArray();
            }
            return _scopeArray;
        }

        /// <summary>
        /// setData Postfix: 원본 메서드가 text.SetText("{{C|CategoryId}}")를 호출한 후
        /// 해당 텍스트를 번역된 텍스트로 교체합니다.
        /// </summary>
        [HarmonyPatch("setData")]
        [HarmonyPostfix]
        static void setData_Postfix(LeftSideCategory __instance, XRL.UI.Framework.FrameworkDataElement data)
        {
            try
            {
                if (__instance == null || __instance.text == null) return;

                // OptionsCategoryRow인 경우만 처리 (왼쪽 패널의 옵션 카테고리)
                if (data is OptionsCategoryRow optionsCategoryRow)
                {
                    string categoryId = optionsCategoryRow.CategoryId;
                    if (string.IsNullOrEmpty(categoryId)) return;

                    var scopes = GetScopes();
                    if (scopes == null || scopes.Length == 0) return;

                    // CategoryId 번역 시도 (예: "Sound" -> "사운드")
                    if (TranslationUtils.TryTranslatePreservingTags(categoryId, out string translated, scopes))
                    {
                        // 원본과 동일한 형식 유지: {{C|번역된텍스트}}
                        __instance.text.SetText("{{C|" + translated + "}}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] LeftSideCategory.setData_Postfix 오류: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// [5] KeyMenuOption 패치: 하단 바 버튼들(Collapse All, Expand All, Select, Help 등)을 번역합니다.
    /// 게임 원본: KeyMenuOption.setDataMenuOption()에서 data.Description을 직접 렌더링
    /// 해결: Render() 호출 후 textSkin의 텍스트를 번역
    /// </summary>
    [HarmonyPatch(typeof(XRL.UI.Framework.KeyMenuOption))]
    public static class Patch_KeyMenuOption
    {
        private static Dictionary<string, string>[] _scopeArray = null;

        private static Dictionary<string, string>[] GetScopes()
        {
            if (_scopeArray == null)
            {
                var optionsDict = LocalizationManager.GetCategory("options");
                var commonDict = LocalizationManager.GetCategory("common");
                var scopes = new List<Dictionary<string, string>>();
                if (optionsDict != null) scopes.Add(optionsDict);
                if (commonDict != null) scopes.Add(commonDict);
                _scopeArray = scopes.ToArray();
            }
            return _scopeArray;
        }

        [HarmonyPatch("setDataMenuOption")]
        [HarmonyPostfix]
        static void setDataMenuOption_Postfix(XRL.UI.Framework.KeyMenuOption __instance, XRL.UI.Framework.MenuOption data)
        {
            try
            {
                if (__instance == null || __instance.textSkin == null) return;
                if (data == null || string.IsNullOrEmpty(data.Description)) return;

                var scopes = GetScopes();
                if (scopes == null || scopes.Length == 0) return;

                // Description 번역 (예: "Collapse All" -> "모두 접기")
                if (TranslationUtils.TryTranslatePreservingTags(data.Description, out string translated, scopes))
                {
                    __instance.textSkin.SetText(translated);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] KeyMenuOption.setDataMenuOption_Postfix 오류: {ex.Message}");
            }
        }

        [HarmonyPatch("setDataPrefixMenuOption")]
        [HarmonyPostfix]
        static void setDataPrefixMenuOption_Postfix(XRL.UI.Framework.KeyMenuOption __instance, XRL.UI.Framework.PrefixMenuOption data)
        {
            try
            {
                if (__instance == null || __instance.textSkin == null) return;
                if (data == null || string.IsNullOrEmpty(data.Description)) return;

                var scopes = GetScopes();
                if (scopes == null || scopes.Length == 0) return;

                if (TranslationUtils.TryTranslatePreservingTags(data.Description, out string translated, scopes))
                {
                    __instance.textSkin.SetText(translated);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] KeyMenuOption.setDataPrefixMenuOption_Postfix 오류: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// [6] EmbarkBuilderModuleBackButton 패치: Back 버튼을 번역합니다.
    /// 게임 원본: EmbarkBuilderModuleBackButton.Update()에서 menuOption.getMenuText()를 렌더링
    /// 해결: Update() 호출 후 TextSkin.text를 번역
    /// </summary>
    [HarmonyPatch(typeof(XRL.CharacterBuilds.UI.EmbarkBuilderModuleBackButton))]
    public static class Patch_EmbarkBuilderModuleBackButton
    {
        private static Dictionary<string, string>[] _scopeArray = null;

        private static Dictionary<string, string>[] GetScopes()
        {
            if (_scopeArray == null)
            {
                var optionsDict = LocalizationManager.GetCategory("options");
                var commonDict = LocalizationManager.GetCategory("common");
                var scopes = new List<Dictionary<string, string>>();
                if (optionsDict != null) scopes.Add(optionsDict);
                if (commonDict != null) scopes.Add(commonDict);
                _scopeArray = scopes.ToArray();
            }
            return _scopeArray;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void Update_Postfix(XRL.CharacterBuilds.UI.EmbarkBuilderModuleBackButton __instance)
        {
            try
            {
                if (__instance == null || __instance.TextSkin == null) return;
                if (__instance.menuOption == null) return;

                string description = __instance.menuOption.Description;
                if (string.IsNullOrEmpty(description)) return;

                var scopes = GetScopes();
                if (scopes == null || scopes.Length == 0) return;

                // Description 번역 (예: "Back" -> "뒤로")
                if (TranslationUtils.TryTranslatePreservingTags(description, out string translated, scopes))
                {
                    // getMenuText() 형식 유지: "[{{W|Esc}}] Back" -> "[{{W|Esc}}] 뒤로"
                    string keyDesc = __instance.menuOption.getKeyDescription();
                    string newText;
                    if (!string.IsNullOrEmpty(keyDesc))
                    {
                        newText = "[{{W|" + keyDesc + "}}] " + translated;
                    }
                    else
                    {
                        newText = translated;
                    }
                    
                    // 현재 텍스트와 다른 경우에만 업데이트 (무한 루프 방지)
                    if (__instance.TextSkin.text != newText)
                    {
                        __instance.TextSkin.text = newText;
                        __instance.TextSkin.Apply();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] EmbarkBuilderModuleBackButton.Update_Postfix 오류: {ex.Message}");
            }
        }
    }
}
