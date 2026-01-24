/*
 * 파일명: 00_99_QudKREngine.cs
 * 분류: [Core] 엔진 확장 유틸리티
 * 역할: 한국어 폰트 강제 적용, 조사(Josa) 처리 로직 등 엔진 레벨의 기능을 제공합니다.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using XRL.Messages;
using XRL.Language;
using XRL.World.Parts;
using Qud.UI;

// GameObject 명시적 지정
using GameObject = XRL.World.GameObject;

namespace QudKRTranslation.Core
{
    // =================================================================
    // 1. 폰트 매니저 (레거시 - 새 시스템 TMPFallbackFontBundle과 연동)
    // =================================================================
    public static class FontManager
    {
        // 새 TMPFallbackFontBundle 시스템과 연동
        public static bool IsFontLoaded => TMPFallbackFontBundle.GetFallbackFont() != null;

        /// <summary>
        /// 한글 TMP 폰트 에셋 반환 (외부에서 접근용) - 새 시스템으로 위임
        /// </summary>
        public static TMP_FontAsset GetKoreanFont() => TMPFallbackFontBundle.GetFallbackFont();

        public static TMP_FontAsset GetKoreanTMPFont() => TMPFallbackFontBundle.GetFallbackFont();

        /// <summary>
        /// 레거시 호환용 - 새 시스템에서 자동으로 처리하므로 no-op
        /// </summary>
        public static void ApplyKoreanFont()
        {
            // 새 TMPFallbackFontBundle이 ControlManager.Update()에서 매 프레임 처리함
            // 이 메서드는 레거시 호환용으로 유지
            TMPFallbackFontBundle.EnsureFallbackAdded();
        }

        /// <summary>
        /// TMP 컴포넌트에 fallback 적용 - 레거시 호환용
        /// </summary>
        public static void ApplyFallbackToTMPComponent(TMPro.TMP_Text txt, bool forceLog = false)
        {
            if (txt == null) return;
            var k = TMPFallbackFontBundle.GetFallbackFont();
            if (k == null) return;

            try
            {
                var currentFont = txt.font;
                if (currentFont == null || currentFont == k) return;

                if (currentFont.fallbackFontAssetTable == null)
                    currentFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();

                if (!currentFont.fallbackFontAssetTable.Contains(k))
                {
                    currentFont.fallbackFontAssetTable.Add(k);
                    txt.ForceMeshUpdate(ignoreActiveState: false, forceTextReparsing: true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] ApplyFallbackToTMPComponent exception: {ex.Message}");
            }
        }

        /// <summary>
        /// 모든 TMP 컴포넌트에 fallback 적용 - 레거시 호환용
        /// </summary>
        public static void ApplyFallbackToAllTMPComponents()
        {
            TMPFallbackFontBundle.EnsureFallbackAdded();
        }
    }

    // =================================================================
    // 2. Harmony Patches
    // =================================================================

    [HarmonyPatch(typeof(Qud.UI.UIManager), "Init")]
    public static class UILoadPatch
    {
        static void Postfix()
        {
            FontManager.ApplyKoreanFont();
        }
    }

    // 메인 메뉴 표시 시 강제 적용
    [HarmonyPatch(typeof(Qud.UI.MainMenu), "Show")]
    public static class MainMenu_Show_Patch
    {
        // 1. 메뉴 표시 전: 텍스트 번역 (Show 메서드가 이 리스트를 사용함)
        static void Prefix()
        {
            TranslateMainMenuOptions();
        }

        // 2. 메뉴 표시 후: 폰트 적용 및 이벤트 등록
        static void Postfix()
        {
            // 폰트가 로드되지 않았으면 로드 시도
            if (!FontManager.IsFontLoaded)
                FontManager.ApplyKoreanFont();
            
            // 전역 Fallback 적용
            FontManager.ApplyFallbackToAllTMPComponents();

            // 메인 메뉴 Scroller 아이템 폰트 강제 적용 (이벤트 훅)
            if (FontManager.IsFontLoaded && Qud.UI.MainMenu.instance != null)
            {
                var menu = Qud.UI.MainMenu.instance;
                RegisterFontHook(menu.leftScroller);
                RegisterFontHook(menu.rightScroller);
                
                // 이미 생성된 아이템들에 대해서도 즉시 적용
                ApplyFontToScroller(menu.leftScroller);
                ApplyFontToScroller(menu.rightScroller);
            }
        }

        static void RegisterFontHook(XRL.UI.Framework.FrameworkScroller scroller)
        {
            if (scroller == null) return;
            
            // 기존 리스너 제거 (중복 방지)
            scroller.PostSetup.RemoveListener(OnPostSetup);
            // 새 리스너 등록
            scroller.PostSetup.AddListener(OnPostSetup);
            Debug.Log("[Qud-KR] Registered PostSetup hook for scroller.");
        }

        static void OnPostSetup(XRL.UI.Framework.FrameworkUnityScrollChild child, XRL.UI.Framework.ScrollChildContext context, XRL.UI.Framework.FrameworkDataElement data, int index)
        {
            if (child == null) return;
            var tmps = child.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            var krFont = FontManager.GetKoreanTMPFont();
            if (krFont == null) return;

            foreach (var tmp in tmps)
            {
                // fallback 추가 방식 (폰트 강제 교체 안 함)
                FontManager.ApplyFallbackToTMPComponent(tmp);
            }
        }

        static void ApplyFontToScroller(XRL.UI.Framework.FrameworkScroller scroller)
        {
            if (scroller == null || scroller.childRoot == null) return;
            
            var tmps = scroller.childRoot.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            var krFont = FontManager.GetKoreanTMPFont();
            if (krFont == null) return;

            foreach (var tmp in tmps)
            {
                // fallback 추가 방식 (폰트 강제 교체 안 함)
                FontManager.ApplyFallbackToTMPComponent(tmp);
            }
        }

        public static void TranslateMainMenuOptions()
        {
            Debug.Log("[Qud-KR] TranslateMainMenuOptions called.");
            var krMap = new Dictionary<string, string>
            {
                { "New Game", "새로 시작" },
                { "Continue", "이어하기" },
                { "Records", "기록" },
                { "Options", "설정" },
                { "Mods", "모드" },
                { "Redeem Code", "코드 입력" },
                { "Modding Toolkit", "모딩 툴킷" },
                { "Credits", "크레딧" },
                { "Help", "도움말" },
                { "Quit", "종료" }
            };

            bool changed = false;

            if (Qud.UI.MainMenu.LeftOptions != null)
            {
                foreach (var opt in Qud.UI.MainMenu.LeftOptions)
                {
                    if (krMap.ContainsKey(opt.Text)) 
                    {
                        opt.Text = krMap[opt.Text];
                        changed = true;
                    }
                }
            }

            if (Qud.UI.MainMenu.RightOptions != null)
            {
                foreach (var opt in Qud.UI.MainMenu.RightOptions)
                {
                    if (krMap.ContainsKey(opt.Text)) 
                    {
                        opt.Text = krMap[opt.Text];
                        changed = true;
                    }
                }
            }
            
            if (changed) Debug.Log("[Qud-KR] MainMenu options translated.");

            // 이미 메인 메뉴가 떠 있다면 강제 갱신
            try
            {
                if (Qud.UI.MainMenu.instance != null && Qud.UI.MainMenu.instance.gameObject.activeInHierarchy)
                {
                    Debug.Log("[Qud-KR] Refeshing active MainMenu instance...");
                    var menu = Qud.UI.MainMenu.instance;
                    if (menu.leftScroller != null)
                        menu.leftScroller.BeforeShow(null, Qud.UI.MainMenu.LeftOptions);
                    if (menu.rightScroller != null)
                        menu.rightScroller.BeforeShow(null, Qud.UI.MainMenu.RightOptions);
                    
                    // Force refresh layout?
                    menu.UpdateMenuBars();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] Failed to refresh MainMenu: {ex.Message}");
            }
        }
    }

    // 게임 코어 시작 시 폰트 로드 시도 (가장 빠른 시점)
    [HarmonyPatch(typeof(XRL.Core.XRLCore), "Start")]
    public static class XRLCore_Start_Patch
    {
        static void Postfix()
        {
            if (!FontManager.IsFontLoaded)
            {
                FontManager.ApplyKoreanFont();
            }
        }
    }

    // ================================================================
    // [DISABLED FOR TESTING] 기존 폰트 패치들 - TorchTest.cs로 대체 테스트 중
    // ================================================================
    /*
    // UITextSkin.Apply() 패치 - 모든 UI 텍스트에 한국어 폰트 강제 적용
    // UITextSkin은 TextMeshProUGUI를 감싸는 래퍼로, 대부분의 게임 UI 텍스트에 사용됨
    [HarmonyPatch(typeof(XRL.UI.UITextSkin), "Apply")]
    public static class UITextSkin_Apply_Patch
    {
        static void Postfix(XRL.UI.UITextSkin __instance)
        {
            if (!FontManager.IsFontLoaded) return;

            try
            {
                // UITextSkin 내부의 TMP 컴포넌트에 접근
                var tmp = __instance.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                {
                    // 직접 할당 대신 안전한 Fallback 적용 메서드 사용
                    FontManager.ApplyFallbackToTMPComponent(tmp);
                    tmp.SetAllDirty();
                }
            }
            catch { }
        }
    }

    // 모든 TMP 컴포넌트가 활성화될 때 한글 폰트 fallback 적용
    // 메인 메뉴, 동적 생성 UI 등 모든 영역에 적용됨
    [HarmonyPatch(typeof(TMPro.TextMeshProUGUI), "OnEnable")]
    public static class TextMeshProUGUI_OnEnable_Patch
    {
        static void Postfix(TMPro.TextMeshProUGUI __instance)
        {
            if (!FontManager.IsFontLoaded) return;
            FontManager.ApplyFallbackToTMPComponent(__instance);
        }
    }

    // TextMeshPro (3D) 패치도 추가
    [HarmonyPatch(typeof(TMPro.TextMeshPro), "OnEnable")]
    public static class TextMeshPro_OnEnable_Patch
    {
        static void Postfix(TMPro.TextMeshPro __instance)
        {
            if (!FontManager.IsFontLoaded) return;
            FontManager.ApplyFallbackToTMPComponent(__instance);
        }
    }

    // TextMeshProUGUI.Awake 패치 - 최초 생성 시점에도 적용
    [HarmonyPatch(typeof(TMPro.TextMeshProUGUI), "Awake")]
    public static class TextMeshProUGUI_Awake_Patch_DISABLED
    {
        static void Postfix(TMPro.TextMeshProUGUI __instance)
        {
            if (!FontManager.IsFontLoaded) return;
            FontManager.ApplyFallbackToTMPComponent(__instance);
        }
    }
    */
    // ================================================================
    // [END DISABLED FOR TESTING]
    // ================================================================

    [HarmonyPatch(typeof(MessageQueue), "AddPlayerMessage", new Type[] { typeof(string), typeof(string), typeof(bool) })]
    public static class MessageLogPatch
    {
        static void Prefix(ref string Message)
        {
            if (!string.IsNullOrEmpty(Message))
                Message = KoreanTextHelper.ResolveJosa(Message);
        }
    }

    [HarmonyPatch(typeof(Grammar), "IndefiniteArticle", new Type[] { typeof(string), typeof(bool) })]
    public static class ArticleKillerPatch
    {
        static bool Prefix(ref string __result)
        {
            __result = "";
            return false;
        }
    }

    [HarmonyPatch(typeof(Grammar), "Pluralize")]
    public static class PluralizeKillerPatch
    {
        static bool Prefix(string word, ref string __result)
        {
            __result = word;
            return false;
        }
    }

    // [DISABLED] NameOrderPatch - TargetMethod()가 null 반환 가능하여 에러 발생
    /*
    [HarmonyPatch]
    public static class NameOrderPatch
    {
        static MethodBase TargetMethod()
        {
            MethodInfo bestMatch = null;
            int bestParamCount = -1;
            foreach (MethodInfo method in typeof(GameObject).GetMethods())
            {
                if (method.Name != "GetDisplayName") continue;
                ParameterInfo[] pars = method.GetParameters();
                if (pars.Length > 0 && pars[0].ParameterType == typeof(int))
                {
                    if (pars.Length > bestParamCount)
                    {
                        bestParamCount = pars.Length;
                        bestMatch = method;
                    }
                }
            }
            return bestMatch;
        }

        static void Postfix(ref string __result)
        {
            if (__result != null && __result.Contains("에 착용") && !__result.EndsWith("에 착용"))
            {
                string[] parts = __result.Split(new string[] { "에 착용" }, StringSplitOptions.None);
                if (parts.Length > 1) __result = parts[1].Trim() + "에 착용";
            }
        }
    }
    */

    [HarmonyPatch(typeof(Description), "GetShortDescription")]
    public static class DescriptionPatch
    {
        static void Postfix(ref string __result)
        {
            if (__result != null && __result.StartsWith("You see "))
            {
                string content = __result.Substring(8).TrimEnd('.');
                __result = KoreanTextHelper.ResolveJosa(content + "{을/를} 본다.");
            }
        }
    }

    // =================================================================
    // 4. TextMeshPro 글로벌 폰트 적용 패치 [DISABLED FOR TESTING]
    // =================================================================
    /*
    [HarmonyPatch(typeof(TMPro.TextMeshProUGUI), "OnEnable")]
    public static class TMPUGUIOnEnablePatch
    {
        static void Postfix(TMPro.TextMeshProUGUI __instance)
        {
            try
            {
                QudKRTranslation.Core.FontManager.ApplyFallbackToTMPComponent(__instance);
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(TMPro.TextMeshPro), "OnEnable")]
    public static class TMPOnEnablePatch
    {
        static void Postfix(TMPro.TextMeshPro __instance)
        {
            try
            {
                QudKRTranslation.Core.FontManager.ApplyFallbackToTMPComponent(__instance);
            }
            catch { }
        }
    }
    */

    // =================================================================
    // 3. 한국어 유틸리티
    // =================================================================
    public static class KoreanTextHelper
    {
        public static bool HasJongsung(char c)
        {
            if (c < 0xAC00 || c > 0xD7A3) return false;
            return (c - 0xAC00) % 28 != 0;
        }
        public static string ResolveJosa(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('{') == -1) return text;
            StringBuilder sb = new StringBuilder(text);
            ProcessPattern(sb, "{을/를}", "을", "를");
            ProcessPattern(sb, "{이/가}", "이", "가");
            ProcessPattern(sb, "{은/는}", "은", "는");
            ProcessPattern(sb, "{와/과}", "과", "와");
            ProcessPattern(sb, "{으로/로}", "으로", "로");
            return sb.ToString();
        }
        private static void ProcessPattern(StringBuilder sb, string pattern, string josaWith, string josaWithout)
        {
            string str = sb.ToString();
            int offset = 0;
            while (true)
            {
                int idx = str.IndexOf(pattern, offset);
                if (idx == -1) break;

                // 조사 바로 앞 글자 찾기
                char target = ' ';
                if (idx > 0)
                {
                    target = str[idx - 1];
                    
                    // 만약 앞 글자가 '}'라면, 색상 태그({{...}})가 닫히는 부분인지 확인
                    if (target == '}')
                    {
                        // 색상 태그 내부의 마지막 글자를 찾는다
                        // 예: {{w|검}}{을/를} -> '검'이 target이 되어야 함
                        
                        // 정규식으로 현재 위치 바로 앞의 색상 태그 추출
                        // pattern: {{[a-zA-Z]\|([^}]+)}}
                        string sub = str.Substring(0, idx);
                        var tagMatch = Regex.Match(sub, @"\{\{[a-zA-Z]\|([^}]+)\}\}$");
                        
                        if (tagMatch.Success)
                        {
                            string innerContent = tagMatch.Groups[1].Value;
                            if (!string.IsNullOrEmpty(innerContent))
                            {
                                // 태그 내용물 중 마지막 문자를 target으로 설정
                                // (보통 마지막 글자가 한글일 가능성이 높음)
                                target = innerContent[innerContent.Length - 1];
                            }
                        }
                    }
                }

                string replacement = HasJongsung(target) ? josaWith : josaWithout;
                sb.Remove(idx, pattern.Length);
                sb.Insert(idx, replacement);
                
                // 문자열 변경됨, 다시 문자열 갱신 (비효율적이지만 안전함)
                str = sb.ToString();
                offset = idx + replacement.Length;
            }
        }
    }
}
