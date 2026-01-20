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
    // 1. 폰트 매니저
    // =================================================================
    public static class FontManager
    {
        public static bool IsFontLoaded { get; private set; } = false;
        
        private static bool _patched = false;
        private static TMP_FontAsset _koreanTMPFont = null;

        public static string[] TargetFontNames = { 
            "AppleGothic",
            "NeoDunggeunmo-Regular", 
            "NeoDunggeunmo",         
            "neodgm",                
            "Apple SD Gothic Neo",
            "Noto Sans CJK KR",
            "Arial" 
        };

        public static void ApplyKoreanFont()
        {
            if (_patched) return;

            // Prevent re-entry
            _patched = true;

            // 1. 모든 TMP 폰트 이름, 한글 지원 여부, fallback 목록을 로그로 출력
            var allTMPFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            int fontIdx = 0;
            foreach (var fontAsset in allTMPFonts)
            {
                if (fontAsset == null) continue;
                bool hasKorean = false;
                try { hasKorean = fontAsset.HasCharacter('가'); } catch { }
                Debug.Log($"[Qud-KR][TMPFont] #{fontIdx}: '{fontAsset.name}' (Korean: {hasKorean})" );
                fontIdx++;
            }

            // Try to find and load our AssetBundle (qudkoreanfont) under StreamingAssets/Mods/*/Fonts
            try
            {
                string streaming = Application.streamingAssetsPath;
                string modsPath = System.IO.Path.Combine(streaming, "Mods");
                TMP_FontAsset loadedFont = null;

                if (System.IO.Directory.Exists(modsPath))
                {
                    var candidates = System.IO.Directory.GetFiles(modsPath, "qudkoreanfont*", System.IO.SearchOption.AllDirectories);
                    foreach (var candidate in candidates)
                    {
                        Debug.Log($"[Qud-KR] Attempting to load font bundle: {candidate}");
                        var bundle = AssetBundle.LoadFromFile(candidate);
                        if (bundle == null) continue;

                        try
                        {
                            var fonts = bundle.LoadAllAssets<TMP_FontAsset>();
                            if (fonts != null && fonts.Length > 0)
                            {
                                loadedFont = fonts[0];
                                Debug.Log($"[Qud-KR] Loaded TMP_FontAsset '{loadedFont.name}' from bundle.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[Qud-KR] Error reading assets from bundle: {ex.Message}");
                        }
                        // Keep assets in memory but unload bundle container
                        bundle.Unload(false);

                        if (loadedFont != null) break;
                    }
                }

                if (loadedFont != null)
                {
                    _koreanTMPFont = loadedFont;

                    // Ensure TMP_Settings fallback list exists and contains our font (insert at front)
                    if (TMP_Settings.fallbackFontAssets == null)
                        TMP_Settings.fallbackFontAssets = new System.Collections.Generic.List<TMP_FontAsset>();

                    if (!TMP_Settings.fallbackFontAssets.Contains(_koreanTMPFont))
                    {
                        TMP_Settings.fallbackFontAssets.Insert(0, _koreanTMPFont);
                        Debug.Log($"[Qud-KR] Inserted '{_koreanTMPFont.name}' into TMP_Settings.fallbackFontAssets.");
                    }

                    // Add as fallback to all existing TMP font assets
                    foreach (var fontAsset in allTMPFonts)
                    {
                        if (fontAsset == null || fontAsset == _koreanTMPFont) continue;
                        if (fontAsset.fallbackFontAssetTable == null)
                            fontAsset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                        if (!fontAsset.fallbackFontAssetTable.Contains(_koreanTMPFont))
                            fontAsset.fallbackFontAssetTable.Add(_koreanTMPFont);
                    }

                    // Force refresh of existing TMP components so fallback takes effect immediately
                    var allTMPTexts = Resources.FindObjectsOfTypeAll<TMPro.TextMeshProUGUI>();
                    foreach (var txt in allTMPTexts)
                    {
                        if (txt == null) continue;
                        try
                        {
                            // reassign font to trigger internal updates
                            txt.font = txt.font;
                            txt.SetAllDirty();
                        }
                        catch { }
                    }

                    IsFontLoaded = true;
                }
                else
                {
                    Debug.LogWarning("[Qud-KR] Korean font bundle not found under StreamingAssets/Mods/*/Fonts or it contained no TMP_FontAsset.");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] Exception while loading font bundle: {e.Message}");
            }

            // 2. TMP_Settings.fallbackFontAssets 목록 출력
            if (TMP_Settings.fallbackFontAssets != null)
            {
                int fbIdx = 0;
                foreach (var fb in TMP_Settings.fallbackFontAssets)
                {
                    if (fb == null) continue;
                    bool hasKorean = false;
                    try { hasKorean = fb.HasCharacter('가'); } catch { }
                    Debug.Log($"[Qud-KR][Fallback] #{fbIdx}: '{fb.name}' (Korean: {hasKorean})");
                    fbIdx++;
                }
            }

            Debug.Log($"[Qud-KR] Font diagnostic complete. (총 TMP 폰트: {fontIdx})");
        }
        
        public static TMP_FontAsset GetKoreanTMPFont()
        {
            return _koreanTMPFont;
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