/*
 * 파일명: PrefixExtractor.cs
 * 분류: Processing - Utility
 * 역할: 접두사 추출 및 번역 유틸리티
 * 작성일: 2026-01-26
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QudKorean.Objects.V2.Data;

namespace QudKorean.Objects.V2.Processing
{
    /// <summary>
    /// Utility class for prefix extraction and translation.
    /// </summary>
    public static class PrefixExtractor
    {
        /// <summary>
        /// Extracts and translates all prefixes from a name.
        /// "wooden arrow" -> ("나무", "arrow")
        /// "flawless crysteel dagger" -> ("완벽한 크리스틸", "dagger")
        /// </summary>
        public static bool TryExtract(string name, ITranslationRepository repo, out string prefixKo, out string remainder)
        {
            prefixKo = null;
            remainder = name;

            var allPrefixes = repo.Prefixes;
            if (allPrefixes == null || allPrefixes.Count == 0)
                return false;

            List<string> translatedPrefixes = new List<string>();
            string current = name;

            // Iteratively extract prefixes (there may be multiple)
            bool foundAny = true;
            while (foundAny)
            {
                foundAny = false;
                foreach (var prefix in allPrefixes)
                {
                    // Check for color tag pattern: {{prefix|prefix}}
                    string colorTagPattern = "{{" + prefix.Key + "|" + prefix.Key + "}}";
                    if (current.StartsWith(colorTagPattern + " ", StringComparison.OrdinalIgnoreCase))
                    {
                        // Preserve color tag structure with translated content
                        translatedPrefixes.Add("{{" + prefix.Value + "|" + prefix.Value + "}}");
                        current = current.Substring(colorTagPattern.Length + 1);
                        foundAny = true;
                        break;
                    }

                    // Standard prefix check
                    if (current.StartsWith(prefix.Key + " ", StringComparison.OrdinalIgnoreCase))
                    {
                        translatedPrefixes.Add(prefix.Value);
                        current = current.Substring(prefix.Key.Length + 1);
                        foundAny = true;
                        break; // Restart search with longest prefixes first
                    }
                }
            }

            if (translatedPrefixes.Count > 0)
            {
                prefixKo = string.Join(" ", translatedPrefixes);
                remainder = current;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Translates prefixes in text (for final fallback).
        /// "leather 모카신" -> "가죽 모카신"
        /// </summary>
        public static string TranslateInText(string text, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string result = text;
            foreach (var prefix in repo.Prefixes)
            {
                // Match prefix followed by space at word boundaries
                string pattern = $@"(^|\s)({Regex.Escape(prefix.Key)})(\s)";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + prefix.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }
            return result;
        }
    }
}
