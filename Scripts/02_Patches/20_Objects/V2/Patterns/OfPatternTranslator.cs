/*
 * 파일명: OfPatternTranslator.cs
 * 분류: Patterns - Translator
 * 역할: "of X" 패턴 번역기
 * 작성일: 2026-01-26
 */

using System;
using System.Text.RegularExpressions;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Patterns
{
    /// <summary>
    /// Translates "X of Y" patterns to Korean word order "Y의 X":
    /// - "sandals of the river-wives" -> "강 아내들의 샌들"
    /// - "banner of the Holy Rhombus" -> "성스러운 마름모의 깃발"
    /// </summary>
    public class OfPatternTranslator : IPatternTranslator
    {
        public string Name => "OfPattern";
        public int Priority => 50;

        public bool CanHandle(string name)
        {
            return name.Contains(" of ", StringComparison.OrdinalIgnoreCase);
        }

        public TranslationResult Translate(string name, ITranslationContext context)
        {
            string stripped = ColorTagProcessor.Strip(name);
            var repo = context.Repository;

            // "X of Y" pattern matching - "of the Y" or "of Y" form
            var match = Regex.Match(stripped, @"^(.+?)\s+of\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return TranslationResult.Miss();

            string itemPart = match.Groups[1].Value.Trim();  // "sandals"
            string ofPart = match.Groups[2].Value.Trim();    // "river-wives" or "Holy Rhombus"

            // 1. Try of_patterns dictionary for full "of X" match
            string ofKo = null;
            string fullOfPatternWithThe = $"of the {ofPart}";
            string fullOfPattern = $"of {ofPart}";

            if (repo.OfPatterns.TryGetValue(fullOfPatternWithThe, out ofKo))
            {
                // ofKo = "강 아내들의" (already includes "의")
            }
            else if (repo.OfPatterns.TryGetValue(fullOfPattern, out ofKo))
            {
                // ofKo = "~의" form
            }
            else if (repo.OfPatterns.TryGetValue(ofPart, out ofKo))
            {
                // Existing of_patterns (element only)
                ofKo = $"{ofKo}의";
            }
            else
            {
                // Try to translate of part with prefixes/nouns
                string ofPartTranslated = TranslateWithDictionaries(ofPart, repo);

                // If no translation, fail
                if (ofPartTranslated == ofPart)
                    return TranslationResult.Miss();

                ofKo = $"{ofPartTranslated}의";
            }

            // 2. Translate item part
            string itemKo = itemPart;
            if (TryGetItemTranslation(repo, itemPart, out string itemTranslated))
            {
                itemKo = itemTranslated;
            }
            else
            {
                string nounTranslated = TranslateWithDictionaries(itemPart, repo);

                // Even if item part is not fully translated, try partial
                if (nounTranslated != itemPart)
                {
                    itemKo = nounTranslated;
                }
            }

            // 3. Combine in Korean word order: "Y의 X"
            string translated = $"{ofKo} {itemKo}".Trim();

            return TranslationResult.Hit(translated, Name);
        }

        private string TranslateWithDictionaries(string text, Data.ITranslationRepository repo)
        {
            string result = text;

            // Try nouns
            foreach (var noun in repo.BaseNouns)
            {
                string pattern = $@"(^|\s)({Regex.Escape(noun.Key)})($|\s|[,.\[\]()])";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + noun.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }

            // Try prefixes
            foreach (var prefix in repo.Prefixes)
            {
                string pattern = $@"(^|\s)({Regex.Escape(prefix.Key)})(\s)";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + prefix.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }

            // Try ColorTagVocab (materials, colors, modifiers, qualities, etc.)
            foreach (var vocab in repo.ColorTagVocab)
            {
                string pattern = $@"(^|\s)({Regex.Escape(vocab.Key)})($|\s|[,.\[\]()])";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + vocab.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }

            return result;
        }

        private bool TryGetItemTranslation(Data.ITranslationRepository repo, string itemName, out string translated)
        {
            translated = null;

            foreach (var item in repo.AllItems)
            {
                foreach (var namePair in item.Names)
                {
                    if (namePair.Key.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                    {
                        translated = namePair.Value;
                        return true;
                    }
                }
            }

            foreach (var noun in repo.BaseNouns)
            {
                if (noun.Key.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                {
                    translated = noun.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
