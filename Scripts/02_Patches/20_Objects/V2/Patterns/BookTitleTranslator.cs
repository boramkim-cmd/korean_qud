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
    /// - "X and Y" -> "X와 Y"
    /// - "X of Y" -> "Y의 X"
    /// - "On X" -> "X에 대하여"
    /// - "X's Y" -> "X의 Y"
    /// - "X with Y" -> "Y가 있는 X"
    /// - "X without Y" -> "Y 없는 X"
    /// - "X for Y" -> "Y를 위한 X"
    /// - "X from Y" -> "Y로부터의 X"
    /// - "X by Y" -> "Y의 X"
    /// - "X in Y" -> "Y의 X"
    /// - "X to Y" -> "Y로의 X"
    /// - "X against Y" -> "Y에 대항하는 X"
    /// - "X through Y" -> "Y를 통한 X"
    /// - "X under Y" -> "Y 아래의 X"
    /// - "X beyond Y" -> "Y 너머의 X"
    /// - "X among Y" -> "Y 사이의 X"
    /// - "A Guide to X" -> "X 안내서"
    /// - "Introduction to X" -> "X 입문"
    /// </summary>
    public class BookTitleTranslator : IPatternTranslator
    {
        public string Name => "BookTitle";
        public int Priority => 45; // Higher priority than OfPattern (50)

        // Prepositions that trigger word order transformation
        private static readonly string[] Prepositions = {
            " of ", " with ", " without ", " for ", " from ", " by ",
            " in ", " to ", " against ", " through ", " under ", " beyond ", " among "
        };

        public bool CanHandle(string name)
        {
            // Book titles often have colons
            if (name.Contains(":")) return true;

            // "On the X" pattern
            if (name.StartsWith("On ", StringComparison.OrdinalIgnoreCase)) return true;

            // "A Guide to X", "Introduction to X" patterns
            if (name.Contains(" Guide to ", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Contains("Introduction to ", StringComparison.OrdinalIgnoreCase)) return true;

            // "The X of Y" pattern
            if (name.StartsWith("The ", StringComparison.OrdinalIgnoreCase) && name.Contains(" of ")) return true;

            // Any preposition pattern
            foreach (var prep in Prepositions)
            {
                if (name.Contains(prep, StringComparison.OrdinalIgnoreCase)) return true;
            }

            // Possessive pattern
            if (name.Contains("'s ") || name.Contains("' ")) return true;

            // "X and Y" pattern
            if (name.Contains(" and ", StringComparison.OrdinalIgnoreCase)) return true;

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
                if (result != stripped && !IsMixedScript(result))
                    return TranslationResult.Hit(result, Name);
            }

            // Try translating as single phrase
            string translated = TranslatePhrase(stripped, repo);
            if (translated != stripped && !IsMixedScript(translated))
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
                return $"{TranslateWords(owner, repo)}의 {TranslateWords(owned, repo)}";
            }

            // 1. "A Guide to X" / "Guide to X" -> "X 안내서"
            var guideMatch = Regex.Match(result, @"^(?:A\s+)?Guide\s+to\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (guideMatch.Success)
            {
                return $"{TranslateWords(guideMatch.Groups[1].Value.Trim(), repo)} 안내서";
            }

            // 2. "Introduction to X" -> "X 입문"
            var introMatch = Regex.Match(result, @"^(?:An?\s+)?Introduction\s+to\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (introMatch.Success)
            {
                return $"{TranslateWords(introMatch.Groups[1].Value.Trim(), repo)} 입문";
            }

            // 3. "On the X of Y" -> "Y의 X에 대하여"
            var onOfMatch = Regex.Match(result, @"^On\s+(?:the\s+)?(.+?)\s+of\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (onOfMatch.Success)
            {
                string subject = TranslateWords(onOfMatch.Groups[1].Value.Trim(), repo);
                string ofPart = TranslateWords(onOfMatch.Groups[2].Value.Trim(), repo);
                return $"{ofPart}의 {subject}에 대하여";
            }

            // 4. "On the X" -> "X에 대하여"
            var onMatch = Regex.Match(result, @"^On\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (onMatch.Success)
            {
                return $"{TranslateWords(onMatch.Groups[1].Value.Trim(), repo)}에 대하여";
            }

            // 5. "X with Y" -> "Y가 있는 X"
            var withMatch = Regex.Match(result, @"^(.+?)\s+with\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (withMatch.Success)
            {
                string x = TranslateWords(withMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(withMatch.Groups[2].Value.Trim(), repo);
                return $"{y}가 있는 {x}";
            }

            // 6. "X without Y" -> "Y 없는 X"
            var withoutMatch = Regex.Match(result, @"^(.+?)\s+without\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (withoutMatch.Success)
            {
                string x = TranslateWords(withoutMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(withoutMatch.Groups[2].Value.Trim(), repo);
                return $"{y} 없는 {x}";
            }

            // 7. "X for Y" -> "Y를 위한 X"
            var forMatch = Regex.Match(result, @"^(.+?)\s+for\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (forMatch.Success)
            {
                string x = TranslateWords(forMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(forMatch.Groups[2].Value.Trim(), repo);
                return $"{y}를 위한 {x}";
            }

            // 8. "X from Y" -> "Y로부터의 X"
            var fromMatch = Regex.Match(result, @"^(.+?)\s+from\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (fromMatch.Success)
            {
                string x = TranslateWords(fromMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(fromMatch.Groups[2].Value.Trim(), repo);
                return $"{y}로부터의 {x}";
            }

            // 9. "X by Y" -> "Y의 X"
            var byMatch = Regex.Match(result, @"^(.+?)\s+by\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (byMatch.Success)
            {
                string x = TranslateWords(byMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(byMatch.Groups[2].Value.Trim(), repo);
                return $"{y}의 {x}";
            }

            // 10. "X in Y" -> "Y의 X" or "Y 안의 X"
            var inMatch = Regex.Match(result, @"^(.+?)\s+in\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (inMatch.Success)
            {
                string x = TranslateWords(inMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(inMatch.Groups[2].Value.Trim(), repo);
                return $"{y}의 {x}";
            }

            // 11. "X to Y" -> "Y로의 X"
            var toMatch = Regex.Match(result, @"^(.+?)\s+to\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (toMatch.Success)
            {
                string x = TranslateWords(toMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(toMatch.Groups[2].Value.Trim(), repo);
                return $"{y}로의 {x}";
            }

            // 12. "X against Y" -> "Y에 대항하는 X"
            var againstMatch = Regex.Match(result, @"^(.+?)\s+against\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (againstMatch.Success)
            {
                string x = TranslateWords(againstMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(againstMatch.Groups[2].Value.Trim(), repo);
                return $"{y}에 대항하는 {x}";
            }

            // 13. "X through Y" -> "Y를 통한 X"
            var throughMatch = Regex.Match(result, @"^(.+?)\s+through\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (throughMatch.Success)
            {
                string x = TranslateWords(throughMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(throughMatch.Groups[2].Value.Trim(), repo);
                return $"{y}를 통한 {x}";
            }

            // 14. "X under Y" -> "Y 아래의 X"
            var underMatch = Regex.Match(result, @"^(.+?)\s+under\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (underMatch.Success)
            {
                string x = TranslateWords(underMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(underMatch.Groups[2].Value.Trim(), repo);
                return $"{y} 아래의 {x}";
            }

            // 15. "X beyond Y" -> "Y 너머의 X"
            var beyondMatch = Regex.Match(result, @"^(.+?)\s+beyond\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (beyondMatch.Success)
            {
                string x = TranslateWords(beyondMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(beyondMatch.Groups[2].Value.Trim(), repo);
                return $"{y} 너머의 {x}";
            }

            // 16. "X among Y" -> "Y 사이의 X"
            var amongMatch = Regex.Match(result, @"^(.+?)\s+among\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (amongMatch.Success)
            {
                string x = TranslateWords(amongMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(amongMatch.Groups[2].Value.Trim(), repo);
                return $"{y} 사이의 {x}";
            }

            // 17. "X of Y" -> "Y의 X" (most common, check last among prepositions)
            var ofMatch = Regex.Match(result, @"^(.+?)\s+of\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (ofMatch.Success)
            {
                string x = TranslateWords(ofMatch.Groups[1].Value.Trim(), repo);
                string y = TranslateWords(ofMatch.Groups[2].Value.Trim(), repo);
                return $"{y}의 {x}";
            }

            // 18. Handle "X and Y" -> "X와 Y"
            result = TranslateAndPattern(result, repo);

            // 19. Translate remaining words
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

        /// <summary>
        /// 영한 혼합 스크립트 감지 — 마르코프 책 등 부분 번역 방지
        /// </summary>
        private static bool IsMixedScript(string s)
        {
            bool hasKorean = false, hasLatin = false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= '\uAC00' && c <= '\uD7AF') hasKorean = true;
                else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) hasLatin = true;
                if (hasKorean && hasLatin) return true;
            }
            return false;
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
