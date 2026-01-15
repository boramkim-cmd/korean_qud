using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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

namespace QudKREngine
{
    // =================================================================
    // 1. 폰트 매니저
    // =================================================================
    public static class FontManager
    {
        // [중요] 찾아주신 정확한 이름을 1순위에 넣었습니다.
        public static string[] TargetFontNames = { 
            "NeoDunggeunmo-Regular", // 1순위: 정확한 이름
            "NeoDunggeunmo",         // 2순위: 패밀리 이름
            "neodgm",                // 3순위: 파일명
            "AppleGothic",           // 4순위: 맥 기본
            "Arial" 
        };
        private static bool _patched = false;

        public static void ApplyKoreanFont()
        {
            if (_patched) return;

            Font osFont = null;
            string loadedName = "";

            // 1. 폰트 찾기
            foreach (string fontName in TargetFontNames)
            {
                Font tempFont = Font.CreateDynamicFontFromOSFont(fontName, 32);
                // 폰트 유효성 검사
                if (tempFont != null && tempFont.fontNames != null && tempFont.fontNames.Length > 0)
                {
                    osFont = tempFont;
                    loadedName = fontName;
                    Debug.Log($"[Qud-KR] 폰트 발견 성공: '{fontName}'");
                    break;
                }
            }

            if (osFont == null)
            {
                Debug.LogError($"[Qud-KR] 폰트 로드 실패. 'NeoDunggeunmo-Regular'가 설치되었는지 확인하세요.");
                return;
            }

            // 2. TMPro 폰트 에셋 생성 및 연결
            TMP_FontAsset koreanTMPFont = TMP_FontAsset.CreateFontAsset(osFont);
            koreanTMPFont.name = "QudKR_Fallback_" + loadedName;
            
            var allTMPFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            int count = 0;
            foreach (var fontAsset in allTMPFonts)
            {
                if (fontAsset == null || fontAsset.name.Contains("QudKR_Fallback")) continue;
                if (fontAsset.fallbackFontAssetTable == null)
                    fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();

                bool alreadyHas = false;
                foreach (var fb in fontAsset.fallbackFontAssetTable)
                {
                    if (fb != null && fb.name.Contains("QudKR_Fallback")) { alreadyHas = true; break; }
                }

                if (!alreadyHas)
                {
                    fontAsset.fallbackFontAssetTable.Add(koreanTMPFont);
                    count++;
                }
            }
            Debug.Log($"[Qud-KR] UI 폰트 {count}개에 '{loadedName}' 적용 완료.");
            _patched = true;
        }
    }

    // =================================================================
    // 2. Harmony Patches
    // =================================================================

    // UI 초기화 시점 후킹 (폰트 적용 타이밍)
    [HarmonyPatch(typeof(Qud.UI.UIManager), "Init")]
    public static class UILoadPatch
    {
        static void Postfix()
        {
            FontManager.ApplyKoreanFont();
        }
    }

    // 로그 한글화 (조사 처리)
    [HarmonyPatch(typeof(MessageQueue), "AddPlayerMessage", new Type[] { typeof(string), typeof(string), typeof(bool) })]
    public static class MessageLogPatch
    {
        static void Prefix(ref string Message)
        {
            if (!string.IsNullOrEmpty(Message))
                Message = KoreanTextHelper.ResolveJosa(Message);
        }
    }

    // 관사(a/an) 제거
    [HarmonyPatch(typeof(Grammar), "IndefiniteArticle", new Type[] { typeof(string), typeof(bool) })]
    public static class ArticleKillerPatch
    {
        static bool Prefix(ref string __result)
        {
            __result = "";
            return false;
        }
    }

    // 복수형(s) 제거
    [HarmonyPatch(typeof(Grammar), "Pluralize")]
    public static class PluralizeKillerPatch
    {
        static bool Prefix(string word, ref string __result)
        {
            __result = word;
            return false;
        }
    }

    // 이름 어순 교정
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

    // 설명문 교정
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
            while (true)
            {
                string current = sb.ToString();
                int idx = current.IndexOf(pattern);
                if (idx == -1) break;
                char prevChar = (idx > 0) ? current[idx - 1] : ' ';
                sb.Replace(pattern, HasJongsung(prevChar) ? josaWith : josaWithout, idx, pattern.Length);
            }
        }
    }
}