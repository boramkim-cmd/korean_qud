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
            if (ContainsWord(stripped, "of"))
                return false;

            // Skip patterns with "the" - usually proper names
            if (ContainsWord(stripped, "the"))
                return false;

            string[] parts = stripped.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Only handle 2-4 word compounds to avoid over-matching
            return parts.Length >= 2 && parts.Length <= 4;
        }

        /// <summary>
        /// Checks if text contains a whole word (space-delimited) without Regex.
        /// </summary>
        private static bool ContainsWord(string text, string word)
        {
            int idx = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                bool leftOk = idx == 0 || text[idx - 1] == ' ';
                bool rightOk = idx + word.Length == text.Length || text[idx + word.Length] == ' ';
                if (leftOk && rightOk) return true;
                idx = text.IndexOf(word, idx + 1, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public TranslationResult Translate(string name, ITranslationContext context)
        {
            // Extract suffixes first (접미사 먼저 분리)
            string stripped = ColorTagProcessor.Strip(name);
            string baseName = SuffixExtractor.ExtractAll(stripped, out string suffixes);

            // 1) 전체 이름을 GlobalNameIndex에서 먼저 조회 (복합어가 이미 등록된 경우)
            var repo = context.Repository;
            if (repo.GlobalNameIndex.TryGetValue(baseName, out string fullMatch) && !string.IsNullOrEmpty(fullMatch))
            {
                string sfxKo = SuffixExtractor.TranslateAll(suffixes, repo);
                string fullResult = string.IsNullOrEmpty(sfxKo) ? fullMatch : fullMatch + sfxKo;
                return TranslationResult.Hit(fullResult, Name);
            }

            // Check if base name (without suffixes) is a valid compound
            string[] parts = baseName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return TranslationResult.Miss();

            // Build translation map for all words
            var translationMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int translatedCount = 0;
            int unknownCount = 0;

            foreach (string part in parts)
            {
                if (TryTranslatePart(repo, part, out string translated))
                {
                    if (!translationMap.ContainsKey(part))
                    {
                        translationMap[part] = translated;
                        translatedCount++;
                    }
                }
                else if (ShouldKeepAsIs(part))
                {
                    if (!translationMap.ContainsKey(part))
                    {
                        translationMap[part] = part;
                    }
                }
                else
                {
                    // 미번역 단어: 원문 유지하되 카운트
                    if (!translationMap.ContainsKey(part))
                    {
                        translationMap[part] = part;
                        unknownCount++;
                    }
                }
            }

            // Need at least one actual translation
            if (translatedCount == 0)
                return TranslationResult.Miss();

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
                string translatedSuffix = SuffixExtractor.TranslateAll(suffixes, repo);
                result = translatedBase + translatedSuffix;
            }

            return unknownCount > 0
                ? TranslationResult.Partial(result, Name)
                : TranslationResult.Hit(result, Name);
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

            // 1. GlobalNameIndex (모든 creature/item 이름 O(1))
            if (repo.GlobalNameIndex.TryGetValue(baseWord, out translated))
            {
                if (isPossessive) translated += "의";
                return true;
            }

            // 2. Species (creature 종 이름)
            if (repo.Species.TryGetValue(baseWord, out translated))
            {
                if (isPossessive) translated += "의";
                return true;
            }

            // 3. BaseNouns O(1)
            if (repo.BaseNounsDict.TryGetValue(baseWord, out translated))
            {
                if (isPossessive) translated += "의";
                return true;
            }

            // 4. Prefixes O(1)
            if (repo.PrefixesDict.TryGetValue(baseWord, out translated))
            {
                if (isPossessive) translated += "의";
                return true;
            }

            // 5. ColorTagVocab O(1)
            if (repo.ColorTagVocabDict.TryGetValue(baseWord, out translated))
            {
                if (isPossessive) translated += "의";
                return true;
            }

            // 6. BodyParts
            if (repo.BodyParts.TryGetValue(baseWord, out translated))
            {
                if (isPossessive) translated += "의";
                return true;
            }

            // 7. Liquids
            if (repo.Liquids.TryGetValue(baseWord, out translated))
            {
                if (isPossessive) translated += "의";
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a word should be kept as-is without translation.
        /// Includes: numbers, roman numerals, MK abbreviations, proper nouns, single letters, placeholders.
        /// </summary>
        private bool ShouldKeepAsIs(string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;

            // Numbers (including negative): 1, 2, -1, 100, etc.
            if (IsNumber(word))
                return true;

            // Roman numerals: I, II, III, IV, V, VI, VII, VIII, IX, X, etc.
            if (word.Length <= 8 && IsRomanNumeral(word))
                return true;

            // MK abbreviations: Mk, Mk., MK, mk
            if (word.Length >= 2 && word.Length <= 3 &&
                (word[0] == 'M' || word[0] == 'm') &&
                (word[1] == 'K' || word[1] == 'k') &&
                (word.Length == 2 || word[2] == '.'))
                return true;

            // Single letters: a, b, q, y, etc. (often used as identifiers)
            if (word.Length == 1 && char.IsLetter(word[0]))
                return true;

            // Proper nouns: starts with uppercase (and more than 1 char, not all-caps)
            if (word.Length > 1 && char.IsUpper(word[0]) && !AllUpper(word))
                return true;

            // All-caps abbreviations (2-4 letters): HE, AP, HP, etc.
            if (word.Length >= 2 && word.Length <= 4 && AllUpper(word))
                return true;

            // Placeholders: *creature*, *item*, etc.
            if (word.Length >= 2 && word[0] == '*' && word[word.Length - 1] == '*')
                return true;

            return false;
        }

        private static bool IsNumber(string s)
        {
            int start = (s.Length > 1 && s[0] == '-') ? 1 : 0;
            if (start >= s.Length) return false;
            for (int i = start; i < s.Length; i++)
                if (!char.IsDigit(s[i])) return false;
            return true;
        }

        private static bool IsRomanNumeral(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char c = char.ToUpper(s[i]);
                if (c != 'I' && c != 'V' && c != 'X' && c != 'L' && c != 'C' && c != 'D' && c != 'M')
                    return false;
            }
            return s.Length > 0;
        }

        private static bool AllUpper(string s)
        {
            for (int i = 0; i < s.Length; i++)
                if (!char.IsUpper(s[i])) return false;
            return true;
        }
    }
}
