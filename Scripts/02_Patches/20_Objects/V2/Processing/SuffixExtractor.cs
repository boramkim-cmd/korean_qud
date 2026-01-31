/*
 * 파일명: SuffixExtractor.cs
 * 분류: Processing - Utility
 * 역할: 접미사 추출 및 번역 유틸리티
 * 작성일: 2026-01-26
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using QudKorean.Objects.V2.Data;

namespace QudKorean.Objects.V2.Processing
{
    /// <summary>
    /// Utility class for suffix extraction and translation.
    /// </summary>
    public static class SuffixExtractor
    {
        // Compiled regex for ExtractAll
        private static readonly Regex RxParen = new Regex(@"(\s*\([^)]+\))$", RegexOptions.Compiled);
        private static readonly Regex RxBracket = new Regex(@"(\s*\[[^\]]+\])$", RegexOptions.Compiled);
        private static readonly Regex RxQuantity = new Regex(@"(\s*x\d+)$", RegexOptions.Compiled);
        private static readonly Regex RxWeaponStats = new Regex(@"(\s+[→◆♦●○]-?\d+(?:\s+[♥♠♣]\d+d\d+(?:\+\d+)?)?)$", RegexOptions.Compiled);
        private static readonly Regex RxArmorStats = new Regex(@"(\s+[◆♦]\d+\s+[○●]-?\d+)$", RegexOptions.Compiled);
        private static readonly Regex RxPlus = new Regex(@"(\s*[+-]\d+)$", RegexOptions.Compiled);
        private static readonly Regex RxOf = new Regex(@"(\s+of\s+[\w\s\-']+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Compiled regex for StripState
        private static readonly Regex RxStripWeaponStats = new Regex(@"\s+[→◆♦●○]\s*-?\d+(\s+[♥♠♣]\d+d\d+(\+\d+)?)?$", RegexOptions.Compiled);
        private static readonly Regex RxStripDiceStats = new Regex(@"\s+[♥♠♣]\d+d\d+(\+\d+)?$", RegexOptions.Compiled);
        private static readonly Regex RxStripBracket = new Regex(@"\s*\[[^\]]+\]$", RegexOptions.Compiled);
        private static readonly Regex RxStripParen = new Regex(@"\s*\([^)]+\)$", RegexOptions.Compiled);
        private static readonly Regex RxStripCount = new Regex(@"\s*x\d+$", RegexOptions.Compiled);

        // Compiled regex for TranslateAll / TranslateState
        private static readonly Regex RxDrams = new Regex(@"\[(\d+) drams? of ([^\]]+)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RxServings = new Regex(@"\[(\d+) servings?\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RxOfTranslate = new Regex(@"\s+of\s+([\w\s\-']+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Extracts all suffixes from a name (quantity, state brackets, parentheses, "of X", "+X", stats).
        /// </summary>
        public static string ExtractAll(string name, out string suffixes)
        {
            suffixes = "";
            if (string.IsNullOrEmpty(name)) return name;

            string result = name;
            List<string> extractedSuffixes = new List<string>();

            var parenMatch = RxParen.Match(result);
            if (parenMatch.Success)
            {
                extractedSuffixes.Insert(0, parenMatch.Value);
                result = result.Substring(0, parenMatch.Index);
            }

            var bracketMatch = RxBracket.Match(result);
            if (bracketMatch.Success)
            {
                extractedSuffixes.Insert(0, bracketMatch.Value);
                result = result.Substring(0, bracketMatch.Index);
            }

            var quantityMatch = RxQuantity.Match(result);
            if (quantityMatch.Success)
            {
                extractedSuffixes.Insert(0, quantityMatch.Value);
                result = result.Substring(0, quantityMatch.Index);
            }

            var statsMatch = RxWeaponStats.Match(result);
            if (statsMatch.Success)
            {
                extractedSuffixes.Insert(0, statsMatch.Value);
                result = result.Substring(0, statsMatch.Index);
            }

            var armorStatsMatch = RxArmorStats.Match(result);
            if (armorStatsMatch.Success)
            {
                extractedSuffixes.Insert(0, armorStatsMatch.Value);
                result = result.Substring(0, armorStatsMatch.Index);
            }

            var plusMatch = RxPlus.Match(result);
            if (plusMatch.Success)
            {
                extractedSuffixes.Insert(0, plusMatch.Value);
                result = result.Substring(0, plusMatch.Index);
            }

            var ofMatch = RxOf.Match(result);
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
        /// </summary>
        public static string TranslateAll(string suffixes, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(suffixes)) return "";

            string result = suffixes;

            foreach (var kvp in repo.States)
            {
                if (result.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = ReplaceIgnoreCase(result, kvp.Key, kvp.Value);
                }
            }

            result = RxDrams.Replace(result, m => {
                string amount = m.Groups[1].Value;
                string liquid = m.Groups[2].Value.Trim();
                string liquidStripped = ColorTagProcessor.Strip(liquid);
                string liquidKo;
                if (repo.Liquids.TryGetValue(liquidStripped, out var ko))
                    liquidKo = ko;
                else
                    liquidKo = TranslateLiquidPhrase(liquidStripped, repo);
                return $"[{liquidKo} {amount}드램]";
            });

            result = RxServings.Replace(result, "[$1인분]");

            result = RxOfTranslate.Replace(result, m => {
                string element = m.Groups[1].Value.Trim();
                string elementKo = repo.OfPatterns.TryGetValue(element, out var ko) ? ko : element;
                return $"의 {elementKo}";
            });

            return result;
        }

        /// <summary>
        /// Strips state suffixes like [empty], [full], (lit), (unlit), x4, and stats.
        /// </summary>
        public static string StripState(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            string result = name;

            result = RxStripWeaponStats.Replace(result, "");
            result = RxStripDiceStats.Replace(result, "");
            result = RxStripBracket.Replace(result, "");
            result = RxStripParen.Replace(result, "");
            result = RxStripCount.Replace(result, "");

            return result.Trim();
        }

        /// <summary>
        /// Translates state suffix to Korean.
        /// </summary>
        public static string TranslateState(string suffix, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(suffix)) return "";

            string result = suffix;

            foreach (var kvp in repo.States)
            {
                if (result.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = ReplaceIgnoreCase(result, kvp.Key, kvp.Value);
                }
            }

            result = RxDrams.Replace(result, m => {
                string amount = m.Groups[1].Value;
                string liquid = m.Groups[2].Value.Trim();
                string liquidStripped = ColorTagProcessor.Strip(liquid);
                string liquidKo;
                if (repo.Liquids.TryGetValue(liquidStripped, out var ko))
                    liquidKo = ko;
                else
                    liquidKo = TranslateLiquidPhrase(liquidStripped, repo);
                return $"[{liquidKo} {amount}드램]";
            });

            result = RxServings.Replace(result, "[$1인분]");

            return result;
        }

        /// <summary>
        /// Translates compound liquid phrases by individual word lookup.
        /// E.g., "inky water" → tries "inky" in Liquids/Prefixes, "water" in Liquids.
        /// </summary>
        private static string TranslateLiquidPhrase(string liquid, ITranslationRepository repo)
        {
            string[] words = liquid.Split(' ');
            bool changed = false;
            for (int i = 0; i < words.Length; i++)
            {
                if (repo.Liquids.TryGetValue(words[i], out var wko))
                    { words[i] = wko; changed = true; }
                else if (repo.PrefixesDict.TryGetValue(words[i], out wko))
                    { words[i] = wko; changed = true; }
            }
            return changed ? string.Join(" ", words) : liquid;
        }

        /// <summary>
        /// Case-insensitive string replace without Regex allocation.
        /// </summary>
        private static string ReplaceIgnoreCase(string source, string oldValue, string newValue)
        {
            int idx = source.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return source;

            var sb = new StringBuilder(source.Length + newValue.Length - oldValue.Length);
            int lastIdx = 0;
            while (idx >= 0)
            {
                sb.Append(source, lastIdx, idx - lastIdx);
                sb.Append(newValue);
                lastIdx = idx + oldValue.Length;
                idx = source.IndexOf(oldValue, lastIdx, StringComparison.OrdinalIgnoreCase);
            }
            sb.Append(source, lastIdx, source.Length - lastIdx);
            return sb.ToString();
        }
    }
}
