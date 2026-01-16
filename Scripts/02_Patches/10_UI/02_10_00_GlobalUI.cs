/*
 * 파일명: 10_00_P_GlobalUI.cs
 * 분류: [UI Patch] 전역 UI 통합 패치
 * 역할: 메인 메뉴, 팝업 메시지, 네비게이션 바, 공용 버튼 등 전반적인 UI 번역을 담당합니다.
 *       LocalizationManager를 사용하여 glossary_ui.json 데이터를 참조합니다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using TMPro;
using Qud.UI;
using XRL.UI;
using XRL.UI.Framework;
using XRL.CharacterBuilds;
using QudKRTranslation.Core;
using QudKRTranslation.Utils;

namespace QudKRTranslation.Patches
{
    // ========================================================================
    // 1. 메인 메뉴 스코프 관리 (Show/Hide)
    // ========================================================================
    [HarmonyPatch]
    public static class Patch_MainMenu_Scope
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("Qud.UI.MainMenu") ?? AccessTools.TypeByName("XRL.UI.MainMenu");
            return type != null ? AccessTools.Method(type, "Show") : null;
        }

        private static bool _scopePushed = false;
        private static bool _fontWarned = false;

        [HarmonyPrefix]
        static void Show_Prefix()
        {
            // [Essential] 폰트 로드 보장 (UIManager.Init 시점 문제 해결)
            FontManager.ApplyKoreanFont();

            // 메인 메뉴 진입 시 스택 초기화 (안전장치)
            ScopeManager.ClearAll();
            _scopePushed = false;

            if (!_scopePushed)
            {
                var uiDict = LocalizationManager.GetCategory("ui");
                var commonDict = LocalizationManager.GetCategory("common");

                var pushList = new List<Dictionary<string, string>>();
                if (uiDict != null) pushList.Add(uiDict);
                if (commonDict != null) pushList.Add(commonDict);

                if (pushList.Count > 0)
                {
                    ScopeManager.PushScope(pushList.ToArray());
                    _scopePushed = true;
                }
            }
            Patch_MainMenu_Data.TranslateMenuData();
        }

        [HarmonyPostfix]
        static void Show_Postfix()
        {
            // 폰트 안내 팝업 비활성화 (사용자 요청)
            /*
            if (!FontManager.IsFontLoaded && !_fontWarned)
            {
                _fontWarned = true;
                Popup.Show("한국어 폰트(NeoDunggeunmo)가 설치되지 않았습니다.\n\n글자가 깨져 보일 수 있습니다.\n운영체제에 해당 폰트를 설치해주세요.");
            }
            */
        }
    }

    [HarmonyPatch]
    public static class Patch_MainMenu_Hide
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("Qud.UI.MainMenu") ?? AccessTools.TypeByName("XRL.UI.MainMenu");
            return type != null ? AccessTools.Method(type, "Hide") : null;
        }

        [HarmonyPrefix]
        static void Hide_Prefix()
        {
            // MainMenu는 닫힐 때 명확히 Pop을 하기엔 싱글톤/다중 호출 등의 이슈가 있을 수 있어
            // ScopeManager의 자동 관리나 명시적 Clear를 기대해야 할 수도 있지만,
            // 여기서는 일단 PopScope 시도. (하지만 _scopePushed 상태 공유가 안되므로 주의)
            
            // ScopeManager.PopScope(); 
            // -> 메인 메뉴가 최상위이므로 보통 놔둬도 됨. 또는 다른 화면으로 갈 때 덮어씌워짐.
        }
    }

    // ========================================================================
    // 2. 메인 메뉴 데이터 항목 번역
    // ========================================================================
    public static class Patch_MainMenu_Data
    {
        public static void TranslateMenuData()
        {
            try
            {
                var menuType = AccessTools.TypeByName("Qud.UI.MainMenu") ?? AccessTools.TypeByName("XRL.UI.MainMenu");
                if (menuType == null) return;

                FieldInfo leftField = menuType.GetField("LeftOptions", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                FieldInfo rightField = menuType.GetField("RightOptions", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                if (leftField == null && rightField == null) return;

                object ownerInstance = null;
                if ((leftField != null && !leftField.IsStatic) || (rightField != null && !rightField.IsStatic))
                {
                    ownerInstance = UnityEngine.Object.FindFirstObjectByType(menuType);
                }

                if (leftField != null) TranslateList(leftField, ownerInstance);
                if (rightField != null) TranslateList(rightField, ownerInstance);
            }
            catch (Exception) { /* Ignore */ }
        }

        private static void TranslateList(FieldInfo field, object owner)
        {
            try
            {
                object raw = field.GetValue(field.IsStatic ? null : owner);
                if (raw is IList list)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        object item = list[i];
                        if (item == null) continue;

                        string original = GetTextFromItem(item, out MemberInfo member);
                        if (string.IsNullOrEmpty(original)) continue;

                        // [Fix] TranslationUtils 오버로드 타입 불일치 수정
                        var scopes = new List<Dictionary<string, string>>();
                        var uiDict = LocalizationManager.GetCategory("ui");
                        if (uiDict != null) scopes.Add(uiDict);
                        var commonDict = LocalizationManager.GetCategory("common");
                        if (commonDict != null) scopes.Add(commonDict);

                        // UI 데이터 우선 검색 (glossary_ui.json) - 태그 보존 번역 시도
                        if (scopes.Count > 0 && TranslationUtils.TryTranslatePreservingTags(original, out string translated, scopes.ToArray()))
                        {
                            SetTextToItem(item, member, translated);
                        }
                        // Fallback: 단순 소문자 검색
                        else if (LocalizationManager.TryGetAnyTerm(original.ToLowerInvariant(), out string t2, "ui", "common"))
                        {
                            SetTextToItem(item, member, t2);
                        }
                        else
                        {
                            // 디버깅: 번역 실패한 중요 항목 로그 출력
                            if (original.IndexOf("new", StringComparison.OrdinalIgnoreCase) >= 0 || original.IndexOf("game", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Debug.Log($"[Qud-KR] Failed to translate: '{original}'");
                            }
                        }
                    }
                }
            }
            catch (Exception) { /* Ignore */ }
        }

        private static string GetTextFromItem(object item, out MemberInfo member)
        {
            member = null;
            var type = item.GetType();
            
            var fields = new[] { "Text", "Description", "Title" };
            foreach (var name in fields)
            {
                var f = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null) { member = f; return f.GetValue(item) as string; }
                
                var p = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null) { member = p; return p.GetValue(item, null) as string; }
            }
            return null;
        }

        private static void SetTextToItem(object item, MemberInfo member, string value)
        {
            if (member is FieldInfo f) f.SetValue(item, value);
            else if (member is PropertyInfo p) p.SetValue(item, value, null);
        }
    }

    // ========================================================================
    // 3. TMP_Text Setter 전역 패치
    // ========================================================================
    [HarmonyPatch(typeof(TMP_Text), "text", MethodType.Setter)]
    public static class Patch_TMP_Text_Setter
    {
        static void Prefix(TMP_Text __instance, ref string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return;
                
                // 1. 활성 스코프 우선
                var scope = ScopeManager.GetCurrentScope();
                if (scope != null)
                {
                    if (TranslationUtils.TryTranslatePreservingTags(value, out string translated, scope))
                    {
                        if (value != translated) 
                        {
                            value = translated;
                            return; // 스코프에서 찾았으면 종료
                        }
                    }
                }

                // 2. 기본 UI/Common 딕셔너리에서 검색 (Fallback)
                // 활성 스코프가 없거나 거기서 못 찾았을 때
                // 매번 GetCategory 호출은 오버헤드가 있으므로 캐싱 고려 가능하지만 일단 직접 호출
                var uiDict = LocalizationManager.GetCategory("ui");
                var commonDict = LocalizationManager.GetCategory("common");
                
                var fallbackScopes = new List<Dictionary<string, string>>();
                if (uiDict != null) fallbackScopes.Add(uiDict);
                if (commonDict != null) fallbackScopes.Add(commonDict);

                if (fallbackScopes.Count > 0)
                {
                    if (TranslationUtils.TryTranslatePreservingTags(value, out string t2, fallbackScopes.ToArray()))
                    {
                        if (value != t2) value = t2;
                    }
                }
            }
            catch { }
        }
    }

    // ========================================================================
    // 4. 팝업 메시지 및 버튼 번역 (LocalizationManager 사용)
    // ========================================================================
    [HarmonyPatch(typeof(PopupMessage), "ShowPopup")]
    public static class Patch_PopupMessage_Buttons
    {
        private static bool _buttonsTranslated = false;

        [HarmonyPrefix]
        static void Prefix()
        {
            if (_buttonsTranslated) return;
            TranslateStaticButtons();
            _buttonsTranslated = true;
        }

        static void TranslateStaticButtons()
        {
            var lists = new List<QudMenuItem>[] {
                PopupMessage._YesNoButton, PopupMessage._YesNoCancelButton, PopupMessage._SingleButton,
                PopupMessage._CancelButton, PopupMessage._CopyButton, PopupMessage._AcceptCancelButton,
                PopupMessage._SubmitCancelButton, PopupMessage._AcceptCancelColorButton,
                PopupMessage._SubmitCancelColorButton, PopupMessage.AcceptButton
            };

            foreach (var list in lists)
            {
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        TranslateMenuItem(ref item);
                        list[i] = item;
                    }
                }
            }

        }

        static void TranslateMenuItem(ref QudMenuItem item)
        {
            if (string.IsNullOrEmpty(item.text)) return;
            
            string[] keywords = { "Yes", "No", "Cancel", "Accept", "Submit", "Copy", "Look", "Continue", "Color", "Hold to Accept", "Trade" };
            foreach (var key in keywords)
            {
                if (item.text.Contains(key))
                {
                    // "ui" 및 "common" 카테고리를 대상으로 검색
                    if (LocalizationManager.TryGetAnyTerm(key.ToLowerInvariant(), out string translated, "common", "ui"))
                    {
                        item.text = item.text.Replace($"|{key}}}", $"|{translated}}}").Replace($" {key}", $" {translated}");
                    }
                }
            }
        }
    }

    // ========================================================================
    // 5. 종료 팝업 및 일반 팝업 메시지
    // ========================================================================
    [HarmonyPatch(typeof(Popup), "ShowYesNoAsync")]
    public static class Patch_Popup_ShowYesNoAsync
    {
        [HarmonyPrefix]
        static void Prefix(ref string Message)
        {
             // "Are you sure..." 같은 문장 전체 검색 ("common", "ui" 카테고리)
            if (LocalizationManager.TryGetAnyTerm(Message.ToLowerInvariant(), out string translated, "common", "ui"))
            {
                Message = translated;
            }
        }
    }

    // ========================================================================
    // 6. 네비게이션 바 (캐릭터 생성 및 메인 메뉴)
    // ========================================================================
    [HarmonyPatch(typeof(AbstractBuilderModuleWindowBase), "GetKeyLegend")]
    public static class Patch_AbstractBuilder_GetKeyLegend
    {
        [HarmonyPostfix]
        static IEnumerable<MenuOption> Postfix(IEnumerable<MenuOption> __result)
        {
            foreach (var option in __result)
            {
                var tr = Traverse.Create(option);
                string desc = tr.Field<string>("Description").Value;
                if (LocalizationManager.TryGetAnyTerm(desc?.ToLowerInvariant(), out string translated, "ui", "common"))
                    tr.Field<string>("Description").Value = translated;
                yield return option;
            }
        }
    }

    [HarmonyPatch(typeof(AbstractBuilderModuleWindowBase), "GetKeyMenuBar")]
    public static class Patch_AbstractBuilder_GetKeyMenuBar
    {
        [HarmonyPostfix]
        static IEnumerable<MenuOption> Postfix(IEnumerable<MenuOption> __result)
        {
            foreach (var option in __result)
            {
                var tr = Traverse.Create(option);
                string desc = tr.Field<string>("Description").Value;
                if (LocalizationManager.TryGetAnyTerm(desc?.ToLowerInvariant(), out string translated, "ui", "common"))
                    tr.Field<string>("Description").Value = translated;
                yield return option;
            }
        }
    }

    [HarmonyPatch(typeof(FrameworkScroller), "BeforeShow", new Type[] { typeof(EmbarkBuilderModuleWindowDescriptor), typeof(IEnumerable<FrameworkDataElement>) })]
    public static class Patch_FrameworkScroller_BeforeShow
    {
        [HarmonyPrefix]
        static void Prefix(EmbarkBuilderModuleWindowDescriptor descriptor, IEnumerable<FrameworkDataElement> selections)
        {
            if (selections == null) return;
            
            foreach (var item in selections)
            {
                if (item is MenuOption menuOption)
                {
                    var tr = Traverse.Create(menuOption);
                    string desc = tr.Field<string>("Description").Value;
                    if (!string.IsNullOrEmpty(desc))
                    {
                        if (LocalizationManager.TryGetAnyTerm(desc.ToLowerInvariant(), out string translated, "ui", "common"))
                        {
                            tr.Field<string>("Description").Value = translated;
                        }
                    }
                }
            }
        }
    }
}
