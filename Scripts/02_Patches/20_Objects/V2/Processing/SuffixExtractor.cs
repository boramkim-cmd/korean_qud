/*
 * 파일명: SuffixExtractor.cs
 * 분류: Processing - Utility
 * 역할: 접미사 추출 및 번역 유틸리티
 * 작성일: 2026-01-26
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QudKorean.Objects.V2.Data;

namespace QudKorean.Objects.V2.Processing
{
    /// <summary>
    /// Utility class for suffix extraction and translation.
    /// </summary>
    public static class SuffixExtractor
    {
        /// <summary>
        /// Extracts all suffixes from a name (quantity, state brackets, parentheses, "of X", "+X", stats).
        /// "torch x14 (unburnt)" -> ("torch", " x14 (unburnt)")
        /// "sword of fire" -> ("sword", " of fire")
        /// "dagger +3" -> ("dagger", " +3")
        /// "musket →8 ♥1d8 [empty]" -> ("musket", " →8 ♥1d8 [empty]")
        /// </summary>
        public static string ExtractAll(string name, out string suffixes)
        {
            suffixes = "";
            if (string.IsNullOrEmpty(name)) return name;

            string result = name;
            List<string> extractedSuffixes = new List<string>();

            // 1. Extract parenthesis suffixes: (lit), (unlit), (unburnt), etc.
            var parenMatch = Regex.Match(result, @"(\s*\([^)]+\))$");
            if (parenMatch.Success)
            {
                extractedSuffixes.Insert(0, parenMatch.Value);
                result = result.Substring(0, parenMatch.Index);
            }

            // 2. Extract bracket suffixes: [empty], [full], [32 drams of water], etc.
            var bracketMatch = Regex.Match(result, @"(\s*\[[^\]]+\])$");
            if (bracketMatch.Success)
            {
                extractedSuffixes.Insert(0, bracketMatch.Value);
                result = result.Substring(0, bracketMatch.Index);
            }

            // 3. Extract quantity suffixes: x3, x14, x15 etc.
            var quantityMatch = Regex.Match(result, @"(\s*x\d+)$");
            if (quantityMatch.Success)
            {
                extractedSuffixes.Insert(0, quantityMatch.Value);
                result = result.Substring(0, quantityMatch.Index);
            }

            // 4. Extract weapon/armor stats at end: →4 ♥1d2, ◆3 ○0, etc.
            var statsMatch = Regex.Match(result, @"(\s+[→◆♦●○]-?\d+(?:\s+[♥♠♣]\d+d\d+(?:\+\d+)?)?)$");
            if (statsMatch.Success)
            {
                extractedSuffixes.Insert(0, statsMatch.Value);
                result = result.Substring(0, statsMatch.Index);
            }

            // Also try armor stats pattern: ◆3 ○0
            var armorStatsMatch = Regex.Match(result, @"(\s+[◆♦]\d+\s+[○●]-?\d+)$");
            if (armorStatsMatch.Success)
            {
                extractedSuffixes.Insert(0, armorStatsMatch.Value);
                result = result.Substring(0, armorStatsMatch.Index);
            }

            // 5. Extract "+X" or "-X" suffixes: +1, +2, -1, etc.
            var plusMatch = Regex.Match(result, @"(\s*[+-]\d+)$");
            if (plusMatch.Success)
            {
                extractedSuffixes.Insert(0, plusMatch.Value);
                result = result.Substring(0, plusMatch.Index);
            }

            // 6. Extract "of X" suffixes: "of fire", "of frost", "of the river-wives", etc.
            var ofMatch = Regex.Match(result, @"(\s+of\s+[\w\s\-']+)$", RegexOptions.IgnoreCase);
            if (ofMatch.Success)
            {
                extractedSuffixes.Insert(0, ofMatch.Value);
                result = result.Substring(0, ofMatch.Index);
            }

            suffixes = string.Concat(extractedSuffixes);
            return result.Trim();
        }

        /// <summary>
        /// Translates all suffix patterns to Korean.
        /// " x15 (unburnt)" -> " x15 (미사용)"
        /// " of fire" -> "의 불"
        /// </summary>
        public static string TranslateAll(string suffixes, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(suffixes)) return "";

            string result = suffixes;

            // Apply state translations
            foreach (var kvp in repo.States)
            {
                if (result.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = Regex.Replace(result, Regex.Escape(kvp.Key), kvp.Value, RegexOptions.IgnoreCase);
                }
            }

            // [X drams of Y] pattern -> [Y X드램]
            result = Regex.Replace(result, @"\[(\d+) drams? of ([^\]]+)\]", m => {
                string amount = m.Groups[1].Value;
                string liquid = m.Groups[2].Value.Trim();
                string liquidStripped = ColorTagProcessor.Strip(liquid);
                string liquidKo = repo.Liquids.TryGetValue(liquidStripped, out var ko) ? ko : liquid;
                return $"[{liquidKo} {amount}드램]";
            }, RegexOptions.IgnoreCase);

            // [X servings] pattern -> [X인분]
            result = Regex.Replace(result, @"\[(\d+) servings?\]", "[$1인분]", RegexOptions.IgnoreCase);

            // "of X" pattern -> "의 X번역"
            result = Regex.Replace(result, @"\s+of\s+([\w\s\-']+)$", m => {
                string element = m.Groups[1].Value.Trim();
                string elementKo = repo.OfPatterns.TryGetValue(element, out var ko) ? ko : element;
                return $"의 {elementKo}";
            }, RegexOptions.IgnoreCase);

            return result;
        }

        /// <summary>
        /// Strips state suffixes like [empty], [full], (lit), (unlit), x4, and stats.
        /// </summary>
        public static string StripState(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            string result = name;

            // Remove weapon/armor stats at end
            result = Regex.Replace(result, @"\s+[→◆♦●○]\s*-?\d+(\s+[♥♠♣]\d+d\d+(\+\d+)?)?$", "");
            result = Regex.Replace(result, @"\s+[♥♠♣]\d+d\d+(\+\d+)?$", "");

            // Remove bracket suffixes
            result = Regex.Replace(result, @"\s*\[[^\]]+\]$", "");

            // Remove parenthesis suffixes
            result = Regex.Replace(result, @"\s*\([^)]+\)$", "");

            // Remove count suffixes
            result = Regex.Replace(result, @"\s*x\d+$", "");

            return result.Trim();
        }

        /// <summary>
        /// Translates state suffix to Korean.
        /// </summary>
        public static string TranslateState(string suffix, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(suffix)) return "";

            string result = suffix;

            // State translations
            foreach (var kvp in repo.States)
            {
                if (result.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = Regex.Replace(result, Regex.Escape(kvp.Key), kvp.Value, RegexOptions.IgnoreCase);
                }
            }

            // [X drams of Y] pattern
            result = Regex.Replace(result, @"\[(\d+) drams? of ([^\]]+)\]", m => {
                string amount = m.Groups[1].Value;
                string liquid = m.Groups[2].Value.Trim();
                string liquidStripped = ColorTagProcessor.Strip(liquid);
                string liquidKo = repo.Liquids.TryGetValue(liquidStripped, out var ko) ? ko : liquid;
                return $"[{liquidKo} {amount}드램]";
            }, RegexOptions.IgnoreCase);

            // [X servings] pattern
            result = Regex.Replace(result, @"\[(\d+) servings?\]", "[$1인분]", RegexOptions.IgnoreCase);

            return result;
        }
    }
}
