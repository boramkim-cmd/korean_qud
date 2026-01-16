/*
 * 파일명: TranslationUtils.cs
 * 분류: [Utils] 안전 번역 및 태그 관리
 * 역할: UI 태그(<...>, {{...}})를 보존하고, 숫구나 제어값을 번역에서 제외합니다.
 *       단일 딕셔너리와 배열 형태의 Scope 모두를 지원하도록 오버로드되었습니다.
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace QudKRTranslation.Utils
{
    public static class TranslationUtils
    {
        // 태그(색상/게임 커스텀 마크업)를 추출하기 위한 정규식
        private static readonly Regex TagRegex = new Regex(@"(<[^>]+>|\{\{[^}]+\}\})", RegexOptions.Compiled);

        /// <summary>
        /// 기존 단일-dictionary 시그니처를 유지하되 내부에서 배열 오버로드를 호출합니다.
        /// (호환성 유지)
        /// </summary>
        public static bool TryTranslatePreservingTags(string input, out string output, Dictionary<string, string> scope)
        {
            output = input;
            if (scope == null) return false;
            // 단일 dict 를 배열 형태로 래핑하여 배열 오버로드로 위임
            return TryTranslatePreservingTags(input, out output, new[] { scope });
        }

        /// <summary>
        /// 배열(s) 형태의 scope를 받는 오버로드.
        /// 우선순위(배열 순서)대로 번역을 시도합니다.
        /// 태그(예: 색상, {{...}})를 플레이스홀더로 치환한 뒤 번역 -> 태그 복원.
        /// </summary>
        public static bool TryTranslatePreservingTags(string input, out string output, Dictionary<string, string>[] scopes)
        {
            output = input;
            if (string.IsNullOrEmpty(input) || scopes == null || scopes.Length == 0) return false;
            if (IsControlValue(input)) return false;

            // 태그가 없는 경우 TranslationEngine의 배열 시그니처를 바로 사용
            if (!TagRegex.IsMatch(input))
            {
                return TranslationEngine.TryTranslate(input, out output, scopes);
            }

            // 태그가 있을 때: 태그들을 플레이스홀더로 교체
            var tags = new List<string>();
            string template = TagRegex.Replace(input, m =>
            {
                tags.Add(m.Value);
                return $"[[TAG_{tags.Count - 1}]]";
            });

            // 템플릿(태그 제거된 형태)을 TranslationEngine에 배열 스코프와 함께 전달
            if (TranslationEngine.TryTranslate(template, out string translatedTemplate, scopes))
            {
                // 태그 복원
                output = Regex.Replace(translatedTemplate, @"\[\[TAG_(\d+)\]\]", m =>
                {
                    int idx = int.Parse(m.Groups[1].Value);
                    return idx >= 0 && idx < tags.Count ? tags[idx] : m.Value;
                });
                return true;
            }

            return false;
        }

        /// <summary>
        /// 체크박스/숫자/On/Off 등 번역하면 안 되는 '제어값'인지 판단합니다.
        /// Options/툴팁에서 숫자나 On/Off 값이 번역되는 것을 막기 위해 public으로 노출합니다.
        /// </summary>
        public static bool IsControlValue(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;

            string trimmed = s.TrimStart();
            string[] prefixes = { "[ ]", "[X]", "[x]", "[*]", "( )", "(X)", "(x)", "(*)", "[■]" };
            foreach (var p in prefixes)
            {
                if (trimmed.StartsWith(p, StringComparison.InvariantCulture)) return true;
            }

            // 숫자만 있는 경우 (예: "123")
            if (Regex.IsMatch(s.Trim(), @"^\d+$")) return true;

            // On/Off(대소문자 무시)만 있는 경우
            var w = s.Trim();
            if (w.Equals("On", StringComparison.InvariantCultureIgnoreCase) || w.Equals("Off", StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }
    }
}
