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
            // Handles: "counterweighted(2) carbide long sword" → prefix="counterweighted", modifier="(2)"
            bool foundAny = true;
            while (foundAny)
            {
                foundAny = false;
                foreach (var prefix in allPrefixes)
                {
                    if (!current.StartsWith(prefix.Key, StringComparison.OrdinalIgnoreCase))
                        continue;

                    int afterPrefix = prefix.Key.Length;
                    if (afterPrefix >= current.Length)
                        continue;

                    // 접두사 바로 뒤가 공백이면 일반 매칭
                    if (current[afterPrefix] == ' ')
                    {
                        translatedPrefixes.Add(prefix.Value);
                        current = current.Substring(afterPrefix + 1);
                        foundAny = true;
                        break;
                    }

                    // 접두사 뒤에 (숫자) 수정치가 붙은 경우: "counterweighted(2) ..."
                    if (current[afterPrefix] == '(' && afterPrefix + 2 < current.Length)
                    {
                        int closeParen = current.IndexOf(')', afterPrefix);
                        if (closeParen > afterPrefix && closeParen + 1 < current.Length && current[closeParen + 1] == ' ')
                        {
                            string modifier = current.Substring(afterPrefix, closeParen - afterPrefix + 1);
                            translatedPrefixes.Add(prefix.Value + modifier);
                            current = current.Substring(closeParen + 2);
                            foundAny = true;
                            break;
                        }
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
