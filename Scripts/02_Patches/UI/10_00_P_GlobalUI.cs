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
using QudKRTranslation.Data;
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

        [HarmonyPrefix]
        static void Show_Prefix()
        {
            if (!ScopeManager.IsScopeActive(Data.MainMenuData.Translations))
            {
                ScopeManager.PushScope(Data.MainMenuData.Translations, Data.CommonData.Translations);
            }
            Patch_MainMenu_Data.TranslateMenuData();
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
            if (ScopeManager.IsScopeActive(Data.MainMenuData.Translations))
            {
                ScopeManager.PopScope();
            }
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
                    ownerInstance = UnityEngine.Object.FindObjectOfType(menuType);
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

                        // UI 데이터 우선 검색 (glossary_ui.json)
                        if (LocalizationManager.TryGetAnyTerm(original.ToLowerInvariant(), out string translated, "ui", "common"))
                        {
                            SetTextToItem(item, member, translated);
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
                var scope = ScopeManager.GetCurrentScope();
                if (scope == null) return;

                if (TranslationUtils.TryTranslatePreservingTags(value, out string translated, scope))
                {
                    if (value != translated) value = translated;
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
            var lists = new List<List<QudMenuItem>>[] {
                PopupMessage._YesNoButton, PopupMessage._YesNoCancelButton, PopupMessage._SingleButton,
                PopupMessage._CancelButton, PopupMessage._CopyButton, PopupMessage._AcceptCancelButton,
                PopupMessage._SubmitCancelButton, PopupMessage._AcceptCancelColorButton,
                PopupMessage._SubmitCancelColorButton, PopupMessage.AcceptButton
            };

            foreach (var list in lists)
            {
                if (list != null) list.ForEach(TranslateMenuItem);
            }
            TranslateMenuItem(PopupMessage.LookButton);
        }

        static void TranslateMenuItem(QudMenuItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.text)) return;
            
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
                if (LocalizationManager.TryGetAnyTerm(option.Description.ToLowerInvariant(), out string translated, "ui", "common"))
                    option.Description = translated;
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
                if (LocalizationManager.TryGetAnyTerm(option.Description.ToLowerInvariant(), out string translated, "ui", "common"))
                    option.Description = translated;
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
                if (item is MenuOption menuOption && !string.IsNullOrEmpty(menuOption.Description))
                {
                    if (LocalizationManager.TryGetAnyTerm(menuOption.Description.ToLowerInvariant(), out string translated, "ui", "common"))
                    {
                        menuOption.Description = translated;
                    }
                }
            }
        }
    }
}
