/*
 * 파일명: TranslationUtils.cs
 * 분류: [Utils] 안전 번역 및 태그 관리
 * 역할: UI 태그(<...>, {{...}})를 보존하고, 숫구나 제어값을 번역에서 제외합니다.
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QudKRTranslation.Utils
{
    public static class TranslationUtils
    {
        // 태그를 추출하기 위한 정규표현식 (Unity Rich Text 및 게임 커스텀 태그)
        private static readonly Regex TagRegex = new Regex(@"(<[^>]+>|\{\{[^}]+\}\})", RegexOptions.Compiled);

        /// <summary>
        /// 태그를 보존하면서 번역을 시도합니다.
        /// 예: "<color=red>Fire</color>" -> "{0}화염{1}" -> "<color=red>화염</color>"
        /// </summary>
        public static bool TryTranslatePreservingTags(string input, out string output, Dictionary<string, string> scope)
        {
            output = input;
            if (string.IsNullOrEmpty(input) || SeemsLikeControlValue(input)) return false;

            // 태그가 없는 경우 일반 번역 시도
            if (!TagRegex.IsMatch(input))
            {
                return TranslationEngine.TryTranslate(input, out output, scope);
            }

            // 태그 분리 및 플레이스홀더화
            var tags = new List<string>();
            string template = TagRegex.Replace(input, m => {
                tags.Add(m.Value);
                return $"[[TAG_{tags.Count - 1}]]";
            });

            // 텍스트 부분만 번역 시도
            if (TranslationEngine.TryTranslate(template, out string translatedTemplate, scope))
            {
                // 태그 복원
                output = Regex.Replace(translatedTemplate, @"\[\[TAG_(\d+)\]\]", m => {
                    int index = int.Parse(m.Groups[1].Value);
                    return index < tags.Count ? tags[index] : m.Value;
                });
                return true;
            }

            return false;
        }

        /// <summary>
        /// 숫자, On/Off 등 번역하면 안 되는 제어값인지 확인합니다.
        /// </summary>
        public static bool SeemsLikeControlValue(string s)
        {
            if (string.IsNullOrEmpty(s)) return true;
            
            s = s.Trim();
            // 숫자만 있는 경우
            if (double.TryParse(s, out _)) return true;
            
            // 일반적인 UI 제어어 (대소문자 무시)
            string lower = s.ToLower();
            if (lower == "on" || lower == "off" || lower == "yes" || lower == "no" || lower == "true" || lower == "false")
                return true;

            return false;
        }
    }
}
