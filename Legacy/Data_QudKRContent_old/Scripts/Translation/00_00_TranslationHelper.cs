/*
 * 파일명: 00_00_TranslationHelper.cs
 * 분류: [System] 번역 헬퍼
 * 역할: 색상 태그, 대소문자, 체크박스 등을 무시하고 범용적으로 번역을 찾아주는 핵심 로직을 담당합니다.
 * 수정일: 2026-01-14
 */

using System;
using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        /// <summary>
        /// 어떤 텍스트가 들어와도 (색상, 체크박스, 대소문자 무시) 최적의 번역을 찾아 반환합니다.
        /// </summary>
        /// <summary>
        /// 어떤 텍스트가 들어와도 (색상, 체크박스, 대소문자 무시) 최적의 번역을 찾아 반환합니다.
        /// 기본적으로 모든 딕셔너리를 순회합니다.
        /// </summary>
        public static bool TryGetAnyTranslation(string text, out string translated)
        {
            return TryGetScopedTranslation(text, out translated, null);
        }

        /// <summary>
        /// 특정 딕셔너리 범위(Scope) 내에서만 번역을 시도합니다.
        /// </summary>
        /// <param name="scopes">검색할 딕셔너리 배열. null이면 모든 딕셔너리를 검색합니다.</param>
        public static bool TryGetScopedTranslation(string text, out string translated, Dictionary<string, string>[] scopes = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                translated = null;
                return false;
            }

            // 1. 기본 전처리: 앞뒤 공백 제거
            string working = text.Trim();
            string prefix = "";
            
            // 2. 다양한 접두사 패턴 확인 및 제거
            string[] prefixes = { "[■] ", "[ ] ", "[*] ", "[X] ", "[x] ", "[Space] ", "[-] ", "[+] ", "( ) ", "(X) ", "(x) ", "(*) ", "(-) ", "(+) " };
            foreach (var p in prefixes)
            {
                if (working.StartsWith(p))
                {
                    prefix = p;
                    working = working.Substring(p.Length);
                    break;
                }
            }

            working = working.TrimStart();

            // 3. 색상 태그 추출 및 제거
            string strippedColor = StripColorTags(working);
            bool hasColor = working != strippedColor;
            string core = strippedColor.Trim();

            // 4. 범위 내에서 번역 찾기
            string result = null;
            if (FindInScopes(core, out result, scopes) ||            // 1) 그대로 시도
                FindInScopes(core.ToUpper(), out result, scopes) ||  // 2) 대문자로 시도
                FindInScopes(ToTitleCase(core), out result, scopes)) // 3) 첫글자 대문자로 시도
            {
                // 5. 번역 성공: 복원
                if (hasColor)
                {
                    result = working.Replace(strippedColor, result);
                }
                
                translated = prefix + result;
                return true;
            }

            translated = null;
            return false;
        }

        private static bool FindInScopes(string key, out string val, Dictionary<string, string>[] scopes)
        {
            if (scopes != null)
            {
                foreach (var dict in scopes)
                {
                    if (dict != null && dict.TryGetValue(key, out val)) return true;
                }
                // Scopes에 없더라도 공통(Common) 딕셔너리는 항상 확인 (편의성)
                if (Common.TryGetValue(key, out val)) return true;
                return false;
            }

            // Scopes가 지정되지 않은 경우 전체 순회 (호환성 유지)
            return FindInAllDicts(key, out val);
        }

        // 모든 딕셔너리를 순회하며 번역 찾기
        private static bool FindInAllDicts(string key, out string val)
        {
            if (Options.TryGetValue(key, out val)) return true;
            if (Options_Sound.TryGetValue(key, out val)) return true;
            if (Options_Display.TryGetValue(key, out val)) return true;
            if (Options_Controls.TryGetValue(key, out val)) return true;
            if (Options_Accessibility.TryGetValue(key, out val)) return true;
            if (Options_UI.TryGetValue(key, out val)) return true;
            if (Options_Automation.TryGetValue(key, out val)) return true;
            if (Options_Autoget.TryGetValue(key, out val)) return true;
            if (Options_Prompts.TryGetValue(key, out val)) return true;
            if (Options_LegacyUI.TryGetValue(key, out val)) return true;
            if (Options_Mods.TryGetValue(key, out val)) return true;
            if (Options_AppSettings.TryGetValue(key, out val)) return true;
            if (Options_Performance.TryGetValue(key, out val)) return true;
            if (Options_Debug.TryGetValue(key, out val)) return true;
            if (Popup.TryGetValue(key, out val)) return true;
            if (Inventory.TryGetValue(key, out val)) return true;
            if (Trade.TryGetValue(key, out val)) return true;
            if (Status.TryGetValue(key, out val)) return true;
            if (Common.TryGetValue(key, out val)) return true;
            return false;
        }

        private static string StripColorTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string result = text;
            int startIndex = 0;
            while (true)
            {
                int colorStart = result.IndexOf("<color=", startIndex);
                if (colorStart == -1) break;
                int colorEnd = result.IndexOf(">", colorStart);
                if (colorEnd == -1) break;
                int closeStart = result.IndexOf("</color>", colorEnd);
                if (closeStart == -1) break;
                
                string beforeTag = result.Substring(0, colorStart);
                string content = result.Substring(colorEnd + 1, closeStart - colorEnd - 1);
                string afterTag = result.Substring(closeStart + 8);
                result = beforeTag + content + afterTag;
                startIndex = beforeTag.Length + content.Length;
            }
            return result;
        }

        private static string ToTitleCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Length == 1) return text.ToUpper();
            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }
    }

    /// <summary>
    /// 현재 실행 중인 UI 컨텍스트에 따라 검색 범위를 동적으로 지정하기 위한 헬퍼
    /// </summary>
    public static class TranslationScopeState
    {
        public static Dictionary<string, string>[] CurrentScope = null;
    }
}
