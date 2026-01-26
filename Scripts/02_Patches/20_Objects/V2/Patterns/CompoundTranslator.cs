/*
 * 파일명: CompoundTranslator.cs
 * 분류: Patterns - Translator
 * 역할: 복합어 패턴 번역기 (bear golem → 곰 골렘)
 * 작성일: 2026-01-26
 *
 * 공백으로 분리된 복합어를 각 단어별로 번역 후 조합합니다.
 * 컬러 태그가 있는 경우 태그 구조를 보존하면서 내용을 번역합니다.
 * 예: "bear golem" → "곰" + "골렘" → "곰 골렘"
 *     "{{c|bear}} golem" → "{{c|곰}} 골렘"
 *     "antelope cherub" → "영양" + "체루브" → "영양 체루브"
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Data;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Patterns
{
    /// <summary>
    /// Translates compound words by splitting and translating each part.
    /// Handles patterns like "{modifier} {noun}" or "{creature} {type}".
    /// Preserves color tag structure while translating contents.
    /// </summary>
    public class CompoundTranslator : IPatternTranslator
    {
        public string Name => "Compound";
        public int Priority => 60; // After other specific patterns

        public bool CanHandle(string name)
        {
            string stripped = ColorTagProcessor.Strip(name);

            // Must have at least one space (compound) and 2-4 words
            if (!stripped.Contains(" "))
                return false;

            // Skip "of" patterns - handled by OfPatternTranslator
            if (Regex.IsMatch(stripped, @"\bof\b", RegexOptions.IgnoreCase))
                return false;

            // Skip patterns with "the" - usually proper names
            if (Regex.IsMatch(stripped, @"\bthe\b", RegexOptions.IgnoreCase))
                return false;

            string[] parts = stripped.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Only handle 2-4 word compounds to avoid over-matching
            return parts.Length >= 2 && parts.Length <= 4;
        }

        public TranslationResult Translate(string name, ITranslationContext context)
        {
            // Extract suffixes first (접미사 먼저 분리)
            string stripped = ColorTagProcessor.Strip(name);
            string baseName = SuffixExtractor.ExtractAll(stripped, out string suffixes);

            // Check if base name (without suffixes) is a valid compound
            string[] parts = baseName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return TranslationResult.Miss();

            // Build translation map for all words
            var translationMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string part in parts)
            {
                if (!TryTranslatePart(context.Repository, part, out string translated))
                {
                    // If any part fails, we can't complete the compound translation
                    return TranslationResult.Miss();
                }
                if (!translationMap.ContainsKey(part))
                {
                    translationMap[part] = translated;
                }
            }

            // Translate the base compound (with or without color tags)
            string translatedBase;

            // For tag handling, we need to work with original but strip suffixes
            string nameWithoutSuffix = name;
            if (!string.IsNullOrEmpty(suffixes))
            {
                // Remove suffix from original name (preserving tags)
                int suffixStartIndex = name.LastIndexOf(suffixes.TrimStart());
                if (suffixStartIndex > 0)
                {
                    nameWithoutSuffix = name.Substring(0, suffixStartIndex).TrimEnd();
                }
            }

            if (nameWithoutSuffix.Contains("{{"))
            {
                translatedBase = TranslateWithTags(nameWithoutSuffix, translationMap);
            }
            else
            {
                // Simple case: no tags, just replace words
                translatedBase = string.Join(" ", parts.Select(p => translationMap[p]));
            }

            // Translate and append suffixes
            string result = translatedBase;
            if (!string.IsNullOrEmpty(suffixes))
            {
                string translatedSuffix = SuffixExtractor.TranslateAll(suffixes, context.Repository);
                result = translatedBase + translatedSuffix;
            }

            return TranslationResult.Hit(result, Name);
        }

        /// <summary>
        /// Translates text containing color tags, preserving tag structure.
        /// Handles: "{{c|bear}} golem" → "{{c|곰}} 골렘"
        /// </summary>
        private string TranslateWithTags(string text, Dictionary<string, string> translationMap)
        {
            var result = new StringBuilder();
            int i = 0;

            while (i < text.Length)
            {
                // Check for tag start
                if (i + 1 < text.Length && text[i] == '{' && text[i + 1] == '{')
                {
                    // Find tag end
                    int tagStart = i;
                    int depth = 1;
                    i += 2;
                    int pipePos = -1;

                    while (i + 1 < text.Length && depth > 0)
                    {
                        if (text[i] == '{' && text[i + 1] == '{')
                        {
                            depth++;
                            i += 2;
                        }
                        else if (text[i] == '}' && text[i + 1] == '}')
                        {
                            depth--;
                            if (depth > 0) i += 2;
                        }
                        else
                        {
                            if (depth == 1 && text[i] == '|' && pipePos == -1)
                            {
                                pipePos = i;
                            }
                            i++;
                        }
                    }

                    if (depth == 0)
                    {
                        // Complete tag found
                        int tagEnd = i + 2;
                        string fullTag = text.Substring(tagStart, tagEnd - tagStart);

                        if (pipePos > tagStart)
                        {
                            // Extract tag name and content
                            string tagName = text.Substring(tagStart + 2, pipePos - tagStart - 2);
                            string content = text.Substring(pipePos + 1, i - pipePos - 1);

                            // Translate words in content
                            string translatedContent = TranslateWordsInText(content, translationMap);

                            // Reconstruct tag (preserve tag name, translate content)
                            result.Append("{{");
                            result.Append(tagName);
                            result.Append("|");
                            result.Append(translatedContent);
                            result.Append("}}");
                        }
                        else
                        {
                            // Malformed tag, keep as-is
                            result.Append(fullTag);
                        }

                        i = tagEnd;
                    }
                    else
                    {
                        // Incomplete tag, keep as-is
                        result.Append(text.Substring(tagStart, i - tagStart));
                    }
                }
                else
                {
                    // Find next tag or end of string
                    int segmentStart = i;
                    while (i < text.Length)
                    {
                        if (i + 1 < text.Length && text[i] == '{' && text[i + 1] == '{')
                            break;
                        i++;
                    }

                    // Translate words in this plain text segment
                    string segment = text.Substring(segmentStart, i - segmentStart);
                    result.Append(TranslateWordsInText(segment, translationMap));
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Translates individual words in plain text using the translation map.
        /// </summary>
        private string TranslateWordsInText(string text, Dictionary<string, string> translationMap)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Split by whitespace while preserving separators
            var result = new StringBuilder();
            var wordBuffer = new StringBuilder();

            for (int i = 0; i <= text.Length; i++)
            {
                char c = i < text.Length ? text[i] : ' ';
                bool isWordChar = char.IsLetterOrDigit(c) || c == '-' || c == '\'';

                if (isWordChar && i < text.Length)
                {
                    wordBuffer.Append(c);
                }
                else
                {
                    // End of word
                    if (wordBuffer.Length > 0)
                    {
                        string word = wordBuffer.ToString();
                        if (translationMap.TryGetValue(word, out string translated))
                        {
                            result.Append(translated);
                        }
                        else
                        {
                            result.Append(word);
                        }
                        wordBuffer.Clear();
                    }

                    // Append separator
                    if (i < text.Length)
                    {
                        result.Append(c);
                    }
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Tries to translate a single word using all available vocabularies.
        /// Handles possessive forms: "merchant's" → "상인의"
        /// </summary>
        private bool TryTranslatePart(ITranslationRepository repo, string word, out string translated)
        {
            translated = null;

            // Handle possessive forms: "word's" → "word_ko의"
            bool isPossessive = word.EndsWith("'s", StringComparison.OrdinalIgnoreCase);
            string baseWord = isPossessive ? word.Substring(0, word.Length - 2) : word;
            string lowerWord = baseWord.ToLower();

            // 1. Check Species dictionary (creatures)
            if (repo.Species.TryGetValue(lowerWord, out translated) ||
                repo.Species.TryGetValue(baseWord, out translated))
            {
                if (isPossessive) translated += "의";
                return true;
            }

            // 2. Check BaseNouns (items, types)
            foreach (var kv in repo.BaseNouns)
            {
                if (kv.Key.Equals(baseWord, StringComparison.OrdinalIgnoreCase))
                {
                    translated = kv.Value;
                    if (isPossessive) translated += "의";
                    return true;
                }
            }

            // 3. Check Prefixes (modifiers, materials, qualities)
            foreach (var kv in repo.Prefixes)
            {
                if (kv.Key.Equals(baseWord, StringComparison.OrdinalIgnoreCase))
                {
                    translated = kv.Value;
                    if (isPossessive) translated += "의";
                    return true;
                }
            }

            // 4. Check ColorTagVocab (broader vocabulary)
            foreach (var kv in repo.ColorTagVocab)
            {
                if (kv.Key.Equals(baseWord, StringComparison.OrdinalIgnoreCase))
                {
                    translated = kv.Value;
                    if (isPossessive) translated += "의";
                    return true;
                }
            }

            // 5. Check BodyParts
            if (repo.BodyParts.TryGetValue(lowerWord, out translated) ||
                repo.BodyParts.TryGetValue(baseWord, out translated))
            {
                if (isPossessive) translated += "의";
                return true;
            }

            // 6. Check Liquids
            if (repo.Liquids.TryGetValue(lowerWord, out translated) ||
                repo.Liquids.TryGetValue(baseWord, out translated))
            {
                if (isPossessive) translated += "의";
                return true;
            }

            // 7. Check creature Names directly
            foreach (var creature in repo.AllCreatures)
            {
                foreach (var namePair in creature.Names)
                {
                    if (namePair.Key.Equals(baseWord, StringComparison.OrdinalIgnoreCase))
                    {
                        translated = namePair.Value;
                        if (isPossessive) translated += "의";
                        return true;
                    }
                }
            }

            // 8. Check item Names directly
            foreach (var item in repo.AllItems)
            {
                foreach (var namePair in item.Names)
                {
                    if (namePair.Key.Equals(baseWord, StringComparison.OrdinalIgnoreCase))
                    {
                        translated = namePair.Value;
                        if (isPossessive) translated += "의";
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
