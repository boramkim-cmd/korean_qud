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
using QudKRTranslation.Core;

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
        /// 체크박스, 핫키 등 접두사를 추출하고 제거합니다.
        /// </summary>
        private static string ExtractPrefix(ref string text)
        {
            // 1. Common Checkboxes
            string[] checkboxes = { "[■] ", "[ ] ", "[*] ", "[X] ", "[x] ", "( ) ", "(X) ", "(x) ", "(*) " };
            foreach (var p in checkboxes)
            {
                if (text.StartsWith(p))
                {
                    text = text.Substring(p.Length).TrimStart();
                    return p;
                }
            }

            // 2. Hotkeys: [A], [9], [Esc], [Tab], [Delete], [~], [Space]
            // Pattern: Starts with [ ... ] followed by space
            var match = Regex.Match(text, @"^(\[[A-Za-z0-9~\+\-\.]+\]\s+)");
            if (match.Success)
            {
                string p = match.Groups[1].Value;
                // Avoid stripping [c]olor tags if they somehow leaked here (unlikely due to {{}} usage in Qud)
                // But Qud sometimes uses [P] for something?
                
                text = text.Substring(p.Length).TrimStart();
                return p;
            }

            // 3. Parenthesis keys: (-) (+)
            string[] parens = { "(-) ", "(+) " };
            foreach (var p in parens)
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
            
            // 0. 색상 태그를 소문자로 통일: {{C|text}} → {{c|text}}
            result = Regex.Replace(result, @"\{\{([a-zA-Z])\|", m => $"{{{{{m.Groups[1].Value.ToLower()}|", RegexOptions.IgnoreCase);
            
            // 1. Qud 형식: {{w|text}}, {{R|text}} 등
            // 패턴: {{[a-zA-Z]|...}}
            result = Regex.Replace(result, @"\{\{[a-zA-Z]\|([^}]+)\}\}", "$1");
            
            // 2. Unity 형식: <color=red>text</color>
            result = Regex.Replace(result, @"<color=[^>]+>([^<]+)</color>", "$1");
            
            // 3. 특수 bullet 문자 제거 (ù 등 - 색상 태그 내부에서 사용됨)
            // LocalizationManager.NormalizeKey와 동일한 처리
            result = Regex.Replace(result, @"^[ùúûü·•◦‣⁃]\s*", "");
            
            return result;
        }
        
        /// <summary>
        /// 원본의 색상 태그를 번역된 텍스트에 복원합니다.
        /// </summary>
        private static string RestoreColorTags(string original, string stripped, string translated)
        {
            // 간단한 방법: 원본에서 stripped를 translated로 교체
            // 색상 태그는 그대로 유지됨
            string restored = original.Replace(stripped, translated);
            
            // 만약 Replacement가 실패했거나(원본에 stripped가 없음) 의미가 없다면
            // 그리고 translated가 유효하다면, 태그 복원보다 번역된 텍스트(이미 태그가 있을 수 있음)를 우선합니다.
            if (!original.Contains(stripped))
            {
                // 원본이 stripped를 포함하지 않음 (태그로 인해 끊겨있을 수 있음: {{C|2}}0 등)
                // 이 경우 번역된 텍스트를 그대로 반환합니다.
                return translated;
            }
            
            return restored;
        }
        
        /// <summary>
        /// 지정된 Scope 배열에서 번역을 찾습니다.
        /// 공백 정규화(trailing space 등)도 시도합니다.
        /// </summary>
        private static bool FindInScopes(string key, out string val, Dictionary<string, string>[] scopes)
        {
            if (scopes != null)
            {
                // 우선순위 순서대로 검색
                foreach (var dict in scopes)
                {
                    if (dict == null) continue;
                    
                    // 1) 원본 키 그대로
                    if (dict.TryGetValue(key, out val) && !string.IsNullOrEmpty(val))
                    {
                        return true;
                    }
                    
                    // 2) 공백 정규화: trim 시도
                    string trimmedKey = key.Trim();
                    if (trimmedKey != key && dict.TryGetValue(trimmedKey, out val) && !string.IsNullOrEmpty(val))
                    {
                        return true;
                    }
                    
                    // 3) 다중 공백을 단일 공백으로 정규화
                    string normalizedKey = System.Text.RegularExpressions.Regex.Replace(trimmedKey, @"\s+", " ");
                    if (normalizedKey != trimmedKey && dict.TryGetValue(normalizedKey, out val) && !string.IsNullOrEmpty(val))
                    {
                        return true;
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
