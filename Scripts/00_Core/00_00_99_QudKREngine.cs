/*
 * 파일명: 00_99_QudKREngine.cs
 * 분류: [Core] 엔진 확장 유틸리티
 * 역할: 한국어 폰트 강제 적용, 조사(Josa) 처리 로직 등 엔진 레벨의 기능을 제공합니다.
 */

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

namespace QudKRTranslation.Core
{
    // =================================================================
    // 1. 폰트 매니저
    // =================================================================
    public static class FontManager
    {
        public static bool IsFontLoaded { get; private set; } = false;

        public static string[] TargetFontNames = { 
            "NeoDunggeunmo-Regular", 
            "NeoDunggeunmo",         
            "neodgm",                
            "AppleGothic",           
            "Arial" 
        };
        private static bool _patched = false;

        public static void ApplyKoreanFont()
        {
            Debug.Log("[Qud-KR] 폰트 적용 로직 비활성화 (UI 깨짐 방지)");
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