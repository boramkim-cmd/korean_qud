/*
 * 파일명: BookTitleTranslator.cs
 * 분류: Patterns - Translator
 * 역할: 책 제목 번역기 - 한글 어순 변환
 * 작성일: 2026-01-27
 */

using System;
using System.Text.RegularExpressions;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Patterns
{
    /// <summary>
    /// Translates book titles with proper Korean word order:
    /// - "Blood and Fear: On the Life Cycle of La" -> "피와 공포: 라의 생명 주기에 대하여"
    /// - "On X" -> "X에 대하여"
    /// - "X of Y" -> "Y의 X"
    /// - "X and Y" -> "X와 Y"
    /// </summary>
    public class BookTitleTranslator : IPatternTranslator
    {
        public string Name => "BookTitle";
        public int Priority => 45; // Higher priority than OfPattern (50)

        public bool CanHandle(string name)
        {
            // Book titles often have colons or "On the" pattern
            if (name.Contains(":")) return true;
            if (name.StartsWith("On ", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.StartsWith("The ", StringComparison.OrdinalIgnoreCase) && name.Contains(" of ")) return true;
            return false;
        }

        public TranslationResult Translate(string name, ITranslationContext context)
        {
            string stripped = ColorTagProcessor.Strip(name);
            var repo = context.Repository;

            // Handle colon-separated titles (Title: Subtitle)
            if (stripped.Contains(":"))
            {
                int colonIdx = stripped.IndexOf(':');
                string title = stripped.Substring(0, colonIdx).Trim();
                string subtitle = stripped.Substring(colonIdx + 1).Trim();

                string titleKo = TranslatePhrase(title, repo);
                string subtitleKo = TranslatePhrase(subtitle, repo);

                string result = $"{titleKo}: {subtitleKo}";
                if (result != stripped)
                    return TranslationResult.Hit(result, Name);
            }

            // Try translating as single phrase
            string translated = TranslatePhrase(stripped, repo);
            if (translated != stripped)
                return TranslationResult.Hit(translated, Name);

            return TranslationResult.Miss();
        }

        private string TranslatePhrase(string phrase, Data.ITranslationRepository repo)
        {
            string result = phrase;

            // 0. Handle possessive "X's Y" or "X' Y" -> "X의 Y"
            var possMatch = Regex.Match(result, @"^(?:The\s+)?(.+?)'s?\s+(.+)$", RegexOptions.IgnoreCase);
            if (possMatch.Success)
            {
                string owner = possMatch.Groups[1].Value.Trim();
                string owned = possMatch.Groups[2].Value.Trim();

                string ownerKo = TranslateWords(owner, repo);
                string ownedKo = TranslateWords(owned, repo);

                result = $"{ownerKo}의 {ownedKo}";
                return result;
            }

            // 1. Handle "On the X of Y" -> "Y의 X에 대하여"
            var onOfMatch = Regex.Match(result, @"^On\s+(?:the\s+)?(.+?)\s+of\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (onOfMatch.Success)
            {
                string subject = onOfMatch.Groups[1].Value.Trim();  // "Life Cycle"
                string ofPart = onOfMatch.Groups[2].Value.Trim();   // "La"

                string subjectKo = TranslateWords(subject, repo);
                string ofPartKo = TranslateWords(ofPart, repo);

                result = $"{ofPartKo}의 {subjectKo}에 대하여";
                return result;
            }

            // 2. Handle "On the X" -> "X에 대하여"
            var onMatch = Regex.Match(result, @"^On\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (onMatch.Success)
            {
                string subject = onMatch.Groups[1].Value.Trim();
                string subjectKo = TranslateWords(subject, repo);
                result = $"{subjectKo}에 대하여";
                return result;
            }

            // 3. Handle "X of Y" -> "Y의 X"
            var ofMatch = Regex.Match(result, @"^(.+?)\s+of\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (ofMatch.Success)
            {
                string itemPart = ofMatch.Groups[1].Value.Trim();
                string ofPart = ofMatch.Groups[2].Value.Trim();

                string itemKo = TranslateWords(itemPart, repo);
                string ofPartKo = TranslateWords(ofPart, repo);

                result = $"{ofPartKo}의 {itemKo}";
                return result;
            }

            // 4. Handle "X and Y" -> "X와 Y" (after other patterns)
            result = TranslateAndPattern(result, repo);

            // 5. Translate remaining words
            result = TranslateWords(result, repo);

            return result;
        }

        private string TranslateAndPattern(string text, Data.ITranslationRepository repo)
        {
            // "X and Y" -> "X와 Y"
            var match = Regex.Match(text, @"^(.+?)\s+and\s+(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string first = match.Groups[1].Value.Trim();
                string second = match.Groups[2].Value.Trim();

                string firstKo = TranslateWords(first, repo);
                string secondKo = TranslateWords(second, repo);

                // Korean uses 와/과 based on final consonant
                // For simplicity, use 와 (more common)
                return $"{firstKo}와 {secondKo}";
            }
            return text;
        }

        private string TranslateWords(string text, Data.ITranslationRepository repo)
        {
            string result = text;

            // Remove "the" articles
            result = Regex.Replace(result, @"\bthe\s+", "", RegexOptions.IgnoreCase);

            // Translate nouns (longest first)
            foreach (var noun in repo.BaseNouns)
            {
                string pattern = $@"(^|\s|\[|"")({Regex.Escape(noun.Key)})($|\s|[,.\[\]():'""!?])";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + noun.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }

            // Translate prefixes/modifiers
            foreach (var prefix in repo.Prefixes)
            {
                string pattern = $@"(^|\s|\[|"")({Regex.Escape(prefix.Key)})(\s|$|\]|[:'""!?])";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + prefix.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }

            // Translate from ColorTagVocab (materials, liquids, etc.)
            foreach (var vocab in repo.ColorTagVocab)
            {
                string pattern = $@"(^|\s|\[|"")({Regex.Escape(vocab.Key)})($|\s|[,.\[\]():'""!?])";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + vocab.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }

            return result.Trim();
        }
    }
}
