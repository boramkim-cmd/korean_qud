/*
 * 파일명: 10_00_P_GlobalUI.cs
 * 분류: [UI Patch] 전역 UI 통합 패치
 * 역할: 메인 메뉴, 팝업 메시지, 네비게이션 바, 공용 버튼 등 전반적인 UI 번역을 담당합니다.
 *       LocalizationManager를 사용하여 glossary_ui.json 데이터를 참조합니다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using QudKRTranslation; // ScopeManager

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
            // [FIX Issue 12] TargetMethod null logging
            if (type == null)
            {
                Debug.LogError("[Qud-KR] MainMenu type not found! Translation will not work.");
                return null;
            }
            return AccessTools.Method(type, "Show");
        }

        // [FIX Issue 2] internal to allow access from Patch_MainMenu_Hide for proper Pop handling
        internal static bool _scopePushed = false;

        [HarmonyPrefix]
        static void Show_Prefix()
        {
            // [Essential] 폰트 로드 보장 (UIManager.Init 시점 문제 해결)
            FontManager.ApplyKoreanFont();

            // [FIX Issue 1] 조건부 스택 초기화 - 비정상적으로 깊을 때만 초기화 (복구 메커니즘)
            if (ScopeManager.GetDepth() > 3)
            {
                Debug.LogWarning("[Qud-KR] Scope stack was unexpectedly deep, cleared");
                ScopeManager.ClearAll();
            }
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

        // [FIX Issue 2] _scopePushed 상태 공유를 위한 static 참조
        private static bool _hideNeedsPop => Patch_MainMenu_Scope._scopePushed;
        
        [HarmonyPrefix]
        static void Hide_Prefix()
        {
            // [FIX Issue 2] 메인 메뉴가 닫힐 때 스코프 정리
            if (Patch_MainMenu_Scope._scopePushed)
            {
                ScopeManager.PopScope();
                Patch_MainMenu_Scope._scopePushed = false;
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
    // 3. TMP_Text Setter 전역 패치 - Unity 리치 텍스트 태그 지원 + 폰트 적용
    // ========================================================================
    [HarmonyPatch(typeof(TMP_Text), "text", MethodType.Setter)]
    public static class Patch_TMP_Text_Setter
    {
        // Unity 리치 텍스트 태그 패턴
        private static readonly System.Text.RegularExpressions.Regex UnityTagPattern = 
            new System.Text.RegularExpressions.Regex(@"<[^>]+>", System.Text.RegularExpressions.RegexOptions.Compiled);
        
        static void Prefix(TMP_Text __instance, ref string value)
        {
            try
            {
                // ★ 폰트 fallback 항상 적용 (번역 여부와 관계없이)
                EnsureFontFallback(__instance);
                
                if (string.IsNullOrEmpty(value)) return;
                
                // 0. Unity 리치 텍스트 태그 strip하여 순수 텍스트 추출
                string stripped = UnityTagPattern.Replace(value, "").Trim();
                if (string.IsNullOrEmpty(stripped)) return;
                
                // 1. hardcoded 텍스트 매칭 (태그 제거된 텍스트로)
                if (Patch_UITextSkin_SetText.TryGetHardcodedTranslation(stripped, out string hardcodedTranslation))
                {
                    // 원본에 태그가 있으면 태그 구조 보존
                    if (value != stripped)
                    {
                        value = value.Replace(stripped, hardcodedTranslation);
                    }
                    else
                    {
                        value = hardcodedTranslation;
                    }
                    return;
                }
                
                // 2. LocalizationManager로 직접 검색 (태그 제거된 텍스트로)
                string lowerStripped = stripped.ToLowerInvariant();
                if (LocalizationManager.TryGetAnyTerm(lowerStripped, out string translated, "chargen_ui", "ui", "common"))
                {
                    if (value != stripped)
                    {
                        value = value.Replace(stripped, translated);
                    }
                    else
                    {
                        value = translated;
                    }
                    return;
                }
                
                // 3. TranslationEngine 사용 (Qud 태그 + 대소문자 변형 지원)
                var scope = ScopeManager.GetCurrentScope();
                if (scope != null)
                {
                    if (TranslationUtils.TryTranslatePreservingTags(value, out string t1, scope))
                    {
                        if (value != t1) 
                        {
                            value = t1;
                            return;
                        }
                    }
                }

                // 4. Fallback: UI/Common 카테고리
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
            catch (System.Exception ex)
            {
                #if DEBUG
                Debug.LogWarning($"[Qud-KR TMP_Text Patch] {ex.Message}");
                #endif
            }
        }
        
        /// <summary>
        /// TMP 컴포넌트에 한글 폰트 fallback이 적용되어 있는지 확인하고, 없으면 적용
        /// </summary>
        private static void EnsureFontFallback(TMP_Text tmp)
        {
            if (tmp == null) return;
            if (!FontManager.IsFontLoaded) return;
            
            var koreanFont = FontManager.GetKoreanFont();
            if (koreanFont == null) return;
            
            // 현재 폰트에 한글 fallback이 없으면 추가
            var currentFont = tmp.font;
            if (currentFont != null && currentFont != koreanFont)
            {
                if (currentFont.fallbackFontAssetTable == null)
                    currentFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                
                if (!currentFont.fallbackFontAssetTable.Contains(koreanFont))
                {
                    currentFont.fallbackFontAssetTable.Insert(0, koreanFont);
                    // 새로 추가되었을 때만 dirty 플래그 설정
                    tmp.SetAllDirty();
                }
            }
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

    // [REMOVED] Redundant GetKeyLegend and GetKeyMenuBar patches (Already handled in 02_10_10_CharacterCreation.cs)

    [HarmonyPatch(typeof(FrameworkScroller), "BeforeShow", new Type[] { typeof(EmbarkBuilderModuleWindowDescriptor), typeof(IEnumerable<FrameworkDataElement>) })]
    public static class Patch_FrameworkScroller_BeforeShow
    {
        [HarmonyPrefix]
        static void Prefix(EmbarkBuilderModuleWindowDescriptor descriptor, IEnumerable<FrameworkDataElement> selections)
        {
            // Translate descriptor.title (e.g., "character creation" -> "캐릭터 생성")
            // This title is displayed by titleText.SetText(descriptor.title) in FrameworkScroller.BeforeShow
            if (descriptor != null && !string.IsNullOrEmpty(descriptor.title))
            {
                if (LocalizationManager.TryGetAnyTerm(descriptor.title.ToLowerInvariant(), out string translatedTitle, "chargen_ui", "ui", "common"))
                {
                    descriptor.title = translatedTitle;
                }
            }
            
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

    // ========================================================================
    // UITextSkin.SetText Patch - Translate hardcoded prefab texts
    // ========================================================================
    [HarmonyPatch(typeof(UITextSkin), nameof(UITextSkin.SetText))]
    public static class Patch_UITextSkin_SetText
    {
        // Known hardcoded texts in Unity Prefabs that need translation
        private static readonly Dictionary<string, string> HardcodedTexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "character creation", null },  // Will be loaded from localization
            { "build library", null },
            { "build summary", null },
            { "choose game mode", null },
            { "choose genotype", null },
            { "choose subtype", null },
            { "choose mutations", null },
            { "choose attributes", null },
            { "choose cybernetic implant", null },
            { "customize character", null },
            { "choose starting location", null },
            { "choose preset", null },
            { "choose character type", null }
        };
        
        private static bool _initialized = false;
        
        private static void InitializeTranslations()
        {
            if (_initialized) return;
            _initialized = true;
            
            // Load translations for all hardcoded texts
            var keys = new List<string>(HardcodedTexts.Keys);
            foreach (var key in keys)
            {
                if (LocalizationManager.TryGetAnyTerm(key.ToLowerInvariant(), out string translated, "chargen_ui", "ui", "common"))
                {
                    HardcodedTexts[key] = translated;
                }
            }
            
            Debug.Log($"[Qud-KR] UITextSkin patch initialized with {HardcodedTexts.Count(kv => kv.Value != null)} translations");
        }
        
        // Public method for other patches to use
        public static bool TryGetHardcodedTranslation(string text, out string translated)
        {
            translated = null;
            if (string.IsNullOrEmpty(text)) return false;
            
            InitializeTranslations();
            
            if (HardcodedTexts.TryGetValue(text, out translated) && !string.IsNullOrEmpty(translated))
            {
                return true;
            }
            return false;
        }

        [HarmonyPrefix]
        static void Prefix(ref string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            // Strip color/rich text tags for matching, but preserve in replacement
            string stripped = System.Text.RegularExpressions.Regex.Replace(text, @"<[^>]+>", "").Trim();
            
            if (TryGetHardcodedTranslation(stripped, out string translated))
            {
                // If text has color tags, replace only the content
                if (text.Contains("<color"))
                {
                    text = System.Text.RegularExpressions.Regex.Replace(
                        text,
                        System.Text.RegularExpressions.Regex.Escape(stripped) + @"\s*",
                        translated + " ",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                else
                {
                    text = translated;
                }
            }
        }
    }
}
