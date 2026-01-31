/*
 * 파일명: ColorTagProcessor.cs
 * 분류: Processing - Utility
 * 역할: 컬러 태그 처리 유틸리티
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
    /// Utility class for handling Qud color tags in text.
    /// </summary>
    public static class ColorTagProcessor
    {
        // Compiled regex for Strip()
        private static readonly Regex RxColorCode = new Regex(@"&[\^]?[a-zA-Z]", RegexOptions.Compiled);
        private static readonly Regex RxInnerTag = new Regex(@"\{\{([^{}|]+)\|([^{}]*)\}\}", RegexOptions.Compiled);

        // Compiled regex for TranslatePossessivesInTags
        private static readonly Regex RxTagBlock = new Regex(@"\{\{([^|{}]+)\|([^{}]+)\}\}", RegexOptions.Compiled);
        private static readonly Regex RxPossessive = new Regex(@"^(.+)'s\s*(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Strips Qud color tags from text.
        /// Handles nested braces correctly using iterative innermost-first processing.
        /// </summary>
        public static string Strip(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;

            result = RxColorCode.Replace(result, "");

            int limit = 20;
            while (limit-- > 0 && result.Contains("{{"))
            {
                string next = RxInnerTag.Replace(result, "$2");
                if (next == result) break;
                result = next;
            }

            return result.Trim();
        }

        /// <summary>
        /// Translates possessive patterns inside color tags.
        /// </summary>
        public static string TranslatePossessivesInTags(string text, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("{{") || !text.Contains("'s"))
                return text;

            return RxTagBlock.Replace(text, match =>
            {
                string tag = match.Groups[1].Value;
                string content = match.Groups[2].Value;

                var possMatch = RxPossessive.Match(content);
                if (!possMatch.Success)
                    return match.Value;

                string creature = possMatch.Groups[1].Value.Trim();
                string remainder = possMatch.Groups[2].Value.Trim();

                string creatureKo = null;
                if (!repo.Species.TryGetValue(creature, out creatureKo))
                {
                    repo.BaseNounsDict.TryGetValue(creature, out creatureKo);
                }

                if (creatureKo == null)
                    return match.Value;

                if (!string.IsNullOrEmpty(remainder))
                {
                    repo.BaseNounsDict.TryGetValue(remainder, out var remainderKo);
                    return remainderKo != null
                        ? $"{{{{{tag}|{creatureKo}의 {remainderKo}}}}}"
                        : $"{{{{{tag}|{creatureKo}의 {remainder}}}}}";
                }

                return $"{{{{{tag}|{creatureKo}의}}}}";
            });
        }

        /// <summary>
        /// Translates materials and vocabulary inside color tags using dictionary lookup.
        /// Replaces O(N×M) regex loops with O(word_count) dictionary lookups.
        /// </summary>
        public static string TranslateMaterials(string text, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("{{"))
                return text;

            var prefixDict = repo.PrefixesDict;
            var vocabDict = repo.ColorTagVocabDict;
            var nounDict = repo.BaseNounsDict;

            var result = new StringBuilder(text.Length);
            int pos = 0;

            while (pos < text.Length)
            {
                // Find next tag opening
                int tagStart = text.IndexOf("{{", pos, StringComparison.Ordinal);
                if (tagStart < 0)
                {
                    // No more tags — translate remaining outside text
                    if (pos < text.Length)
                        result.Append(TranslateWordsInSegment(text.Substring(pos), prefixDict, vocabDict, nounDict));
                    break;
                }

                // Translate text before tag
                if (tagStart > pos)
                    result.Append(TranslateWordsInSegment(text.Substring(pos, tagStart - pos), prefixDict, vocabDict, nounDict));

                // Find pipe separator
                int pipeIdx = text.IndexOf('|', tagStart + 2);
                if (pipeIdx < 0)
                {
                    // Malformed tag — output as-is
                    result.Append("{{");
                    pos = tagStart + 2;
                    continue;
                }

                // Find tag closing (handle nesting)
                int closeIdx = FindTagClose(text, pipeIdx + 1);
                if (closeIdx < 0)
                {
                    // Malformed — output rest as-is
                    result.Append(text.Substring(tagStart));
                    break;
                }

                string shaderName = text.Substring(tagStart + 2, pipeIdx - tagStart - 2);
                string content = text.Substring(pipeIdx + 1, closeIdx - pipeIdx - 1);

                // Translate content words via dictionary lookup
                string translatedContent = TranslateTagContent(shaderName, content, prefixDict, vocabDict, nounDict);

                result.Append("{{");
                result.Append(shaderName);
                result.Append('|');
                result.Append(translatedContent);
                result.Append("}}");

                pos = closeIdx + 2;
            }

            return result.ToString();
        }

        /// <summary>
        /// Finds the closing }} for a tag, handling nested tags.
        /// </summary>
        private static int FindTagClose(string text, int startAfterPipe)
        {
            int depth = 1;
            int i = startAfterPipe;
            while (i + 1 < text.Length)
            {
                if (text[i] == '{' && text[i + 1] == '{') { depth++; i += 2; }
                else if (text[i] == '}' && text[i + 1] == '}')
                {
                    depth--;
                    if (depth == 0) return i;
                    i += 2;
                }
                else i++;
            }
            return -1;
        }

        /// <summary>
        /// Translates words inside a tag content using dictionary lookup.
        /// Handles self-referential tags (shader == content).
        /// </summary>
        private static string TranslateTagContent(string shaderName, string content,
            IReadOnlyDictionary<string, string> prefixes,
            IReadOnlyDictionary<string, string> vocab,
            IReadOnlyDictionary<string, string> nouns)
        {
            return TranslateWordsInSegment(content, prefixes, vocab, nouns);
        }

        /// <summary>
        /// Translates individual words in a text segment via dictionary lookup.
        /// Preserves punctuation and whitespace boundaries.
        /// O(word_count) instead of O(vocab_size).
        /// </summary>
        private static string TranslateWordsInSegment(string segment,
            IReadOnlyDictionary<string, string> prefixes,
            IReadOnlyDictionary<string, string> vocab,
            IReadOnlyDictionary<string, string> nouns)
        {
            if (string.IsNullOrEmpty(segment))
                return segment;

            string[] parts = segment.Split(' ');
            bool changed = false;

            for (int i = 0; i < parts.Length; i++)
            {
                string word = parts[i];
                if (word.Length == 0) continue;

                // Separate leading/trailing punctuation
                int leadLen = 0;
                while (leadLen < word.Length && IsBoundaryChar(word[leadLen]))
                    leadLen++;

                int trailLen = 0;
                while (trailLen < word.Length - leadLen && IsBoundaryChar(word[word.Length - 1 - trailLen]))
                    trailLen++;

                string lead = leadLen > 0 ? word.Substring(0, leadLen) : "";
                string trail = trailLen > 0 ? word.Substring(word.Length - trailLen) : "";
                string core = word.Substring(leadLen, word.Length - leadLen - trailLen);

                if (core.Length == 0) continue;

                string translated;
                if (prefixes.TryGetValue(core, out translated) ||
                    vocab.TryGetValue(core, out translated) ||
                    nouns.TryGetValue(core, out translated))
                {
                    parts[i] = lead + translated + trail;
                    changed = true;
                }
            }

            return changed ? string.Join(" ", parts) : segment;
        }

        private static bool IsBoundaryChar(char c)
        {
            return c == '[' || c == ']' || c == '(' || c == ')' ||
                   c == '"' || c == '\'' || c == ',' || c == '.' ||
                   c == ':' || c == ';' || c == '!' || c == '?';
        }

        /// <summary>
        /// Translates base nouns outside of color tags.
        /// Uses segment-based approach to precisely distinguish tag interior/exterior.
        /// </summary>
        public static string TranslateNounsOutsideTags(string text, ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var prefixDict = repo.PrefixesDict;
            var vocabDict = repo.ColorTagVocabDict;
            var nounDict = repo.BaseNounsDict;

            // If no tags, direct dictionary replacement
            if (!text.Contains("{{"))
            {
                return TranslateWordsInSegment(text, prefixDict, vocabDict, nounDict);
            }

            // Segment-based processing for text with tags
            var result = new StringBuilder();
            int lastEnd = 0;
            int i = 0;

            while (i < text.Length)
            {
                if (i + 1 < text.Length && text[i] == '{' && text[i + 1] == '{')
                {
                    // Process preceding text via dictionary lookup
                    if (i > lastEnd)
                    {
                        string segment = text.Substring(lastEnd, i - lastEnd);
                        result.Append(TranslateWordsInSegment(segment, prefixDict, vocabDict, nounDict));
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
                result.Append(TranslateWordsInSegment(segment, prefixDict, vocabDict, nounDict));
            }

            return result.ToString();
        }

        /// <summary>
        /// Restores color tags from original to translated text using granular replacement.
        /// </summary>
        public static string RestoreFormatting(string original, string coreName, string translatedCore, string suffix, string translatedSuffix)
        {
            if (string.IsNullOrEmpty(original)) return translatedCore + translatedSuffix;

            string result = original;

            // 1. Replace the core name (case-insensitive, no Regex)
            if (!string.IsNullOrEmpty(coreName) && !string.IsNullOrEmpty(translatedCore))
            {
                result = ReplaceIgnoreCase(result, coreName, translatedCore);
            }

            // 2. Replace the suffix
            if (!string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(translatedSuffix) && suffix != translatedSuffix)
            {
                if (result.IndexOf(suffix, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = ReplaceIgnoreCase(result, suffix, translatedSuffix);
                }
                else
                {
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
                        // Try to find suffix inside a color tag
                        int tagIdx = FindSuffixInTag(result, suffixContent);
                        if (tagIdx >= 0)
                        {
                            result = ReplaceIgnoreCase(result, suffixContent, translatedContent);
                        }
                        else
                        {
                            result = ReplaceIgnoreCase(result, suffixContent, translatedContent);
                        }
                    }
                }
            }

            return result;
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

        /// <summary>
        /// Finds suffix content inside a {{tag|...}} block. Returns index or -1.
        /// </summary>
        private static int FindSuffixInTag(string text, string suffixContent)
        {
            int searchFrom = 0;
            while (true)
            {
                int tagStart = text.IndexOf("{{", searchFrom, StringComparison.Ordinal);
                if (tagStart < 0) return -1;
                int pipeIdx = text.IndexOf('|', tagStart + 2);
                if (pipeIdx < 0) return -1;
                int closeIdx = text.IndexOf("}}", pipeIdx, StringComparison.Ordinal);
                if (closeIdx < 0) return -1;

                string content = text.Substring(pipeIdx + 1, closeIdx - pipeIdx - 1);
                int innerIdx = content.IndexOf(suffixContent, StringComparison.OrdinalIgnoreCase);
                if (innerIdx >= 0) return pipeIdx + 1 + innerIdx;

                searchFrom = closeIdx + 2;
            }
        }
    }
}
