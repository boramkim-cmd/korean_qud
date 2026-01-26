/*
 * 파일명: ColorTagProcessor.cs
 * 분류: Processing - Utility
 * 역할: 컬러 태그 처리 유틸리티
 * 작성일: 2026-01-26
 */

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using QudKorean.Objects.V2.Data;

namespace QudKorean.Objects.V2.Processing
{
    /// <summary>
    /// Utility class for handling Qud color tags in text.
    /// </summary>
    public static class ColorTagProcessor
    {
        /// <summary>
        /// Strips Qud color tags from text.
        /// Handles nested braces correctly using iterative innermost-first processing.
        /// "{{K|{{crysteel|crysteel}} mace}}" -> "crysteel mace"
        /// </summary>
        public static string Strip(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;

            // Remove simple color codes like &r, &W, &^r, &^W
            result = Regex.Replace(result, @"&[\^]?[a-zA-Z]", "");

            // Remove {{...}} tags iteratively (innermost first)
            int limit = 10;
            while (limit-- > 0 && result.Contains("{{"))
            {
                string next = Regex.Replace(result, @"\{\{([^{}|]+)\|([^{}]*)\}\}", "$2");
                if (next == result) break;
                result = next;
            }

            return result.Trim();
        }

        /// <summary>
        /// Translates possessive patterns inside color tags.
        /// "{{w|Praetorian's}} cloak" -> "{{w|프라이토리안의}} cloak"
        /// "{{Y|minstrel's token}}" -> "{{Y|음유시인의 토큰}}"
        /// </summary>
        public static string TranslatePossessivesInTags(string text, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("{{") || !text.Contains("'s"))
                return text;

            return Regex.Replace(text, @"\{\{([^|{}]+)\|([^{}]+)\}\}", match =>
            {
                string tag = match.Groups[1].Value;
                string content = match.Groups[2].Value;

                var possMatch = Regex.Match(content, @"^(.+)'s\s*(.*)$", RegexOptions.IgnoreCase);
                if (!possMatch.Success)
                    return match.Value;

                string creature = possMatch.Groups[1].Value.Trim();
                string remainder = possMatch.Groups[2].Value.Trim();

                // Species 사전에서 검색
                string creatureKo = null;
                if (!repo.Species.TryGetValue(creature, out creatureKo))
                {
                    if (!repo.Species.TryGetValue(creature.ToLower(), out creatureKo))
                    {
                        // BaseNouns에서도 검색 (merchant 등)
                        foreach (var noun in repo.BaseNouns)
                        {
                            if (noun.Key.Equals(creature, System.StringComparison.OrdinalIgnoreCase))
                            {
                                creatureKo = noun.Value;
                                break;
                            }
                        }
                    }
                }

                if (creatureKo == null)
                    return match.Value;

                if (!string.IsNullOrEmpty(remainder))
                {
                    string remainderKo = null;
                    foreach (var noun in repo.BaseNouns)
                    {
                        if (noun.Key.Equals(remainder, System.StringComparison.OrdinalIgnoreCase))
                        {
                            remainderKo = noun.Value;
                            break;
                        }
                    }
                    return remainderKo != null
                        ? $"{{{{{tag}|{creatureKo}의 {remainderKo}}}}}"
                        : $"{{{{{tag}|{creatureKo}의 {remainder}}}}}";
                }

                return $"{{{{{tag}|{creatureKo}의}}}}";
            });
        }

        /// <summary>
        /// Translates materials and vocabulary inside color tags.
        /// "{{G|hulk}} {{w|honey}} injector" -> "{{G|헐크}} {{w|꿀}} injector"
        /// </summary>
        public static string TranslateMaterials(string text, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("{{"))
                return text;

            string result = text;

            // Step 0: Handle self-referential color tags {{word|word}}
            // These are mod adjectives like {{feathered|feathered}}
            foreach (var prefix in repo.Prefixes)
            {
                string selfRefPattern = @"\{\{" + Regex.Escape(prefix.Key) + @"\|" + Regex.Escape(prefix.Key) + @"\}\}";
                if (Regex.IsMatch(result, selfRefPattern, RegexOptions.IgnoreCase))
                {
                    result = Regex.Replace(result, selfRefPattern,
                        "{{" + prefix.Key + "|" + prefix.Value + "}}",
                        RegexOptions.IgnoreCase);
                }
            }

            // Step 0.5: Handle {{shader|shader full text}} pattern
            // e.g., {{feathered|feathered leather armor}} → {{feathered|깃털 달린 leather armor}}
            // The shader name is preserved, but the prefix in content is translated
            foreach (var prefix in repo.Prefixes)
            {
                // Pattern: {{shader|shader something}} where shader matches prefix
                string extendedPattern = @"\{\{" + Regex.Escape(prefix.Key) + @"\|" + Regex.Escape(prefix.Key) + @"\s+([^{}]+)\}\}";
                result = Regex.Replace(result, extendedPattern,
                    m => "{{" + prefix.Key + "|" + prefix.Value + " " + m.Groups[1].Value + "}}",
                    RegexOptions.IgnoreCase);
            }

            // Step 1: Handle non-self-referential color tags {{shaderName|prefix}}
            // e.g., {{glittering|glitter}} → {{glittering|글리터}}
            // IMPORTANT: Shader name (first part) is NEVER translated, only display text (second part)
            foreach (var prefix in repo.Prefixes)
            {
                // Match {{anyShader|prefix}} where shader != prefix
                string pattern = @"\{\{([^|{}]+)\|" + Regex.Escape(prefix.Key) + @"\}\}";
                result = Regex.Replace(result, pattern,
                    m => "{{" + m.Groups[1].Value + "|" + prefix.Value + "}}",
                    RegexOptions.IgnoreCase);
            }

            // Process iteratively for nested tags
            int maxIterations = 5;
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                string previous = result;

                // Step 1: Translate materials/modifiers
                foreach (var mat in repo.ColorTagVocab)
                {
                    // Pattern 1: Tag content starts with material
                    string pattern1 = $@"(\{{\{{[^|{{}}]+\|)({Regex.Escape(mat.Key)})(\s|\}}\}})";
                    result = Regex.Replace(result, pattern1, m =>
                        m.Groups[1].Value + mat.Value + m.Groups[3].Value,
                        RegexOptions.IgnoreCase);

                    // Pattern 2: Material in middle of tag content
                    string pattern2 = $@"(\{{\{{[^|{{}}]+\|[^{{}}]*\s)({Regex.Escape(mat.Key)})(\s|\}}\}})";
                    result = Regex.Replace(result, pattern2, m =>
                        m.Groups[1].Value + mat.Value + m.Groups[3].Value,
                        RegexOptions.IgnoreCase);
                }

                // Step 2: Translate nouns inside tags
                foreach (var noun in repo.BaseNouns)
                {
                    // Pattern: {{X|something noun}}
                    string pattern1 = $@"(\{{\{{[^|{{}}]+\|[^{{}}]*\s)({Regex.Escape(noun.Key)})(\}}\}})";
                    result = Regex.Replace(result, pattern1, m =>
                        m.Groups[1].Value + noun.Value + m.Groups[3].Value,
                        RegexOptions.IgnoreCase);

                    // Pattern: {{X|noun}}
                    string pattern2 = $@"(\{{\{{[^|{{}}]+\|)({Regex.Escape(noun.Key)})(\}}\}})";
                    result = Regex.Replace(result, pattern2, m =>
                        m.Groups[1].Value + noun.Value + m.Groups[3].Value,
                        RegexOptions.IgnoreCase);

                    // Pattern: {{X|noun something}}
                    string pattern3 = $@"(\{{\{{[^|{{}}]+\|)({Regex.Escape(noun.Key)})(\s[^{{}}]*\}}\}})";
                    result = Regex.Replace(result, pattern3, m =>
                        m.Groups[1].Value + noun.Value + m.Groups[3].Value,
                        RegexOptions.IgnoreCase);
                }

                if (result == previous)
                    break;
            }

            // Pattern 3: Translate words outside but adjacent to tags
            foreach (var mat in repo.ColorTagVocab)
            {
                // Before tag
                string pattern3 = $@"(^|\s)({Regex.Escape(mat.Key)})(\s+\{{\{{\s*)";
                result = Regex.Replace(result, pattern3, m =>
                    m.Groups[1].Value + mat.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);

                // After tag
                string pattern4 = $@"(\}}\}}\s+)({Regex.Escape(mat.Key)})(\s|$)";
                result = Regex.Replace(result, pattern4, m =>
                    m.Groups[1].Value + mat.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }

            return result;
        }

        /// <summary>
        /// Translates base nouns outside of color tags.
        /// Uses segment-based approach to precisely distinguish tag interior/exterior.
        /// "{{w|bronze}} mace" -> "{{w|bronze}} 메이스"
        /// </summary>
        public static string TranslateNounsOutsideTags(string text, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // If no tags, direct replacement
            if (!text.Contains("{{"))
            {
                string withNouns = TranslateNounsInText(text, repo);
                return TranslatePrefixesInText(withNouns, repo);
            }

            // Segment-based processing for text with tags
            var result = new StringBuilder();
            int lastEnd = 0;
            int i = 0;

            while (i < text.Length)
            {
                if (i + 1 < text.Length && text[i] == '{' && text[i + 1] == '{')
                {
                    // Found tag start - process preceding text
                    if (i > lastEnd)
                    {
                        string segment = text.Substring(lastEnd, i - lastEnd);
                        string withNouns = TranslateNounsInText(segment, repo);
                        result.Append(TranslatePrefixesInText(withNouns, repo));
                    }

                    // Find tag end (handling nesting)
                    int depth = 1;
                    int tagStart = i;
                    i += 2;
                    while (i + 1 < text.Length && depth > 0)
                    {
                        if (text[i] == '{' && text[i + 1] == '{') { depth++; i += 2; }
                        else if (text[i] == '}' && text[i + 1] == '}') { depth--; i += 2; }
                        else i++;
                    }

                    // Append tag as-is
                    result.Append(text.Substring(tagStart, i - tagStart));
                    lastEnd = i;
                }
                else
                {
                    i++;
                }
            }

            // Process remaining text
            if (lastEnd < text.Length)
            {
                string segment = text.Substring(lastEnd);
                string withNouns = TranslateNounsInText(segment, repo);
                result.Append(TranslatePrefixesInText(withNouns, repo));
            }

            return result.ToString();
        }

        /// <summary>
        /// Translates nouns in plain text (no tags).
        /// Handles nouns after brackets like [water] or [fresh water].
        /// Also handles colons for book titles like "Fear: On..."
        /// </summary>
        private static string TranslateNounsInText(string text, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string result = text;
            foreach (var noun in repo.BaseNouns)
            {
                // Pattern: noun after start, space, [ or " and before end, space, or punctuation (including colon)
                string pattern = $@"(^|\s|\[|"")({Regex.Escape(noun.Key)})($|\s|[,.\[\]():'""!?])";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + noun.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }
            return result;
        }

        /// <summary>
        /// Translates prefixes in plain text.
        /// Handles prefixes after brackets like [fresh water] or followed by space/end.
        /// Also handles colons for book titles like "Fear: On..."
        /// </summary>
        private static string TranslatePrefixesInText(string text, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string result = text;
            foreach (var prefix in repo.Prefixes)
            {
                // Pattern: prefix after start/space/[/" and before space/end/]/:/"/!/? (supports book titles)
                // Examples: "dried la", "[fresh water]", "dried", "Fear: On...", "\"Fear\""
                string pattern = $@"(^|\s|\[|"")({Regex.Escape(prefix.Key)})(\s|$|\]|[:'""!?])";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + prefix.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }
            return result;
        }

        /// <summary>
        /// Restores color tags from original to translated text using granular replacement.
        /// </summary>
        public static string RestoreFormatting(string original, string coreName, string translatedCore, string suffix, string translatedSuffix)
        {
            if (string.IsNullOrEmpty(original)) return translatedCore + translatedSuffix;

            string result = original;

            // 1. Replace the core name
            if (!string.IsNullOrEmpty(coreName) && !string.IsNullOrEmpty(translatedCore))
            {
                result = Regex.Replace(result, Regex.Escape(coreName), translatedCore, RegexOptions.IgnoreCase);
            }

            // 2. Replace the suffix
            if (!string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(translatedSuffix) && suffix != translatedSuffix)
            {
                if (result.IndexOf(suffix, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = Regex.Replace(result, Regex.Escape(suffix), translatedSuffix, RegexOptions.IgnoreCase);
                }
                else
                {
                    // Handle complex bracket suffixes with color tags
                    string suffixContent = suffix.Trim();
                    string translatedContent = translatedSuffix.Trim();

                    if (suffixContent.StartsWith("[") && suffixContent.EndsWith("]"))
                    {
                        int lastOpenBracket = result.LastIndexOf('[');
                        int lastCloseBracket = result.LastIndexOf(']');
                        if (lastOpenBracket >= 0 && lastCloseBracket > lastOpenBracket)
                        {
                            result = result.Substring(0, lastOpenBracket) + translatedContent + result.Substring(lastCloseBracket + 1);
                        }
                    }
                    else if (suffixContent.StartsWith("(") && suffixContent.EndsWith(")"))
                    {
                        int lastOpenParen = result.LastIndexOf('(');
                        int lastCloseParen = result.LastIndexOf(')');
                        if (lastOpenParen >= 0 && lastCloseParen > lastOpenParen)
                        {
                            result = result.Substring(0, lastOpenParen) + translatedContent + result.Substring(lastCloseParen + 1);
                        }
                    }
                    else
                    {
                        string tagPattern = @"\{\{[^|]+\|" + Regex.Escape(suffixContent) + @"\}\}";
                        var tagMatch = Regex.Match(result, tagPattern, RegexOptions.IgnoreCase);
                        if (tagMatch.Success)
                        {
                            string replacement = tagMatch.Value.Replace(suffixContent, translatedContent);
                            result = result.Substring(0, tagMatch.Index) + replacement + result.Substring(tagMatch.Index + tagMatch.Length);
                        }
                        else
                        {
                            result = Regex.Replace(result, Regex.Escape(suffixContent), translatedContent, RegexOptions.IgnoreCase);
                        }
                    }
                }
            }

            return result;
        }
    }
}
