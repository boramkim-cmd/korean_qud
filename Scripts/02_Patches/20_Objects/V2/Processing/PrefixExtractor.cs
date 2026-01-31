/*
 * 파일명: PrefixExtractor.cs
 * 분류: Processing - Utility
 * 역할: 접두사 추출 및 번역 유틸리티
 * 작성일: 2026-01-26
 */

using System;
using System.Collections.Generic;
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
                    if (current.Length > prefix.Key.Length &&
                        current[prefix.Key.Length] == ' ' &&
                        current.StartsWith(prefix.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        translatedPrefixes.Add(prefix.Value);
                        current = current.Substring(prefix.Key.Length + 1);
                        foundAny = true;
                        break;
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
        /// Translates prefixes in text using dictionary lookup instead of regex loop.
        /// "leather 모카신" -> "가죽 모카신"
        /// </summary>
        public static string TranslateInText(string text, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var dict = repo.PrefixesDict;
            if (dict == null || dict.Count == 0)
                return text;

            // Split by spaces, look up each word in the prefix dictionary
            string[] words = text.Split(' ');
            bool changed = false;
            for (int i = 0; i < words.Length; i++)
            {
                if (dict.TryGetValue(words[i], out var translated))
                {
                    words[i] = translated;
                    changed = true;
                }
            }

            return changed ? string.Join(" ", words) : text;
        }
    }
}
