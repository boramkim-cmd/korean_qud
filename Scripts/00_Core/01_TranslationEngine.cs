/*
 * 파일명: 01_TranslationEngine.cs
 * 분류: [Core] 번역 엔진
 * 역할: 색상 태그, 체크박스, 대소문자를 무시하고 번역을 찾아주는 핵심 로직
 * 작성일: 2026-01-15
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QudKRTranslation
{
    /// <summary>
    /// 번역 엔진 - 다양한 형식의 텍스트를 처리하여 번역을 찾습니다.
    /// </summary>
    public static class TranslationEngine
    {
        /// <summary>
        /// 텍스트를 번역합니다. 현재 활성 Scope를 사용합니다.
        /// </summary>
        public static bool TryTranslate(string text, out string translated)
        {
            var scope = ScopeManager.GetCurrentScope();
            return TryTranslate(text, out translated, scope);
        }
        
        /// <summary>
        /// 텍스트를 번역합니다. 지정된 Scope를 사용합니다.
        /// </summary>
        public static bool TryTranslate(string text, out string translated, Dictionary<string, string>[] scopes)
        {
            if (string.IsNullOrEmpty(text))
            {
                translated = null;
                return false;
            }
            
            // 1. 전처리: 앞뒤 공백 제거
            string working = text.Trim();
            
            // 2. 체크박스/접두사 패턴 추출
            string prefix = ExtractPrefix(ref working);
            
            // 3. 색상 태그 제거 (Qud 형식 + Unity 형식)
            string stripped = StripColorTags(working);
            bool hasColorTags = (working != stripped);
            
            // 4. 핵심 텍스트 추출
            string core = stripped.Trim();
            
            // 5. 번역 찾기 (대소문자 변형 시도)
            string result = null;
            if (FindInScopes(core, out result, scopes) ||                    // 1) 원본 그대로
                FindInScopes(core.ToUpper(), out result, scopes) ||          // 2) 전체 대문자
                FindInScopes(ToTitleCase(core), out result, scopes) ||       // 3) 첫 글자만 대문자
                FindInScopes(core.ToLower(), out result, scopes))            // 4) 전체 소문자
            {
                // 6. 번역 성공: 색상 태그 복원
                if (hasColorTags)
                {
                    result = RestoreColorTags(working, stripped, result);
                }
                
                // 7. 접두사 복원
                translated = prefix + result;
                return true;
            }
            
            translated = null;
            return false;
        }
        
        /// <summary>
        /// 체크박스 등의 접두사를 추출하고 제거합니다.
        /// </summary>
        private static string ExtractPrefix(ref string text)
        {
            string[] prefixes = { 
                "[■] ", "[ ] ", "[*] ", "[X] ", "[x] ", 
                "[Space] ", "[-] ", "[+] ", 
                "( ) ", "(X) ", "(x) ", "(*) ", "(-) ", "(+) " 
            };
            
            foreach (var p in prefixes)
            {
                if (text.StartsWith(p))
                {
                    text = text.Substring(p.Length).TrimStart();
                    return p;
                }
            }
            
            return "";
        }
        
        /// <summary>
        /// 색상 태그를 제거합니다 (Qud 형식 + Unity 형식).
        /// </summary>
        private static string StripColorTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            string result = text;
            
            // 1. Qud 형식: {{w|text}}, {{R|text}} 등
            // 패턴: {{[a-zA-Z]|...}}
            result = Regex.Replace(result, @"\{\{[a-zA-Z]\|([^}]+)\}\}", "$1");
            
            // 2. Unity 형식: <color=red>text</color>
            result = Regex.Replace(result, @"<color=[^>]+>([^<]+)</color>", "$1");
            
            return result;
        }
        
        /// <summary>
        /// 원본의 색상 태그를 번역된 텍스트에 복원합니다.
        /// </summary>
        private static string RestoreColorTags(string original, string stripped, string translated)
        {
            // 간단한 방법: 원본에서 stripped를 translated로 교체
            // 색상 태그는 그대로 유지됨
            return original.Replace(stripped, translated);
        }
        
        /// <summary>
        /// 지정된 Scope 배열에서 번역을 찾습니다.
        /// </summary>
        private static bool FindInScopes(string key, out string val, Dictionary<string, string>[] scopes)
        {
            if (scopes != null)
            {
                // 우선순위 순서대로 검색
                foreach (var dict in scopes)
                {
                    if (dict != null && dict.TryGetValue(key, out val))
                    {
                        // [중요] 빈 문자열이면 번역하지 않은 것으로 간주하고 계속 검색하거나 실패 처리
                        if (!string.IsNullOrEmpty(val))
                        {
                            return true;
                        }
                    }
                }
            }
            
            val = null;
            return false;
        }
        
        /// <summary>
        /// 첫 글자만 대문자로 변환합니다.
        /// </summary>
        private static string ToTitleCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Length == 1) return text.ToUpper();
            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }
    }
}
