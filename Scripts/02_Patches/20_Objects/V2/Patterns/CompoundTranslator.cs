/*
 * 파일명: CompoundTranslator.cs
 * 분류: Patterns - Translator
 * 역할: 복합어 패턴 번역기 (bear golem → 곰 골렘)
 * 작성일: 2026-01-26
 *
 * 공백으로 분리된 복합어를 각 단어별로 번역 후 조합합니다.
 * 예: "bear golem" → "곰" + "골렘" → "곰 골렘"
 *     "antelope cherub" → "영양" + "체루브" → "영양 체루브"
 */

using System;
using System.Collections.Generic;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Data;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Patterns
{
    /// <summary>
    /// Translates compound words by splitting and translating each part.
    /// Handles patterns like "{modifier} {noun}" or "{creature} {type}".
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

            string[] parts = stripped.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Only handle 2-4 word compounds to avoid over-matching
            return parts.Length >= 2 && parts.Length <= 4;
        }

        public TranslationResult Translate(string name, ITranslationContext context)
        {
            string stripped = ColorTagProcessor.Strip(name);
            string[] parts = stripped.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return TranslationResult.Miss();

            var translatedParts = new List<string>();
            bool allTranslated = true;

            foreach (string part in parts)
            {
                if (TryTranslatePart(context.Repository, part, out string translated))
                {
                    translatedParts.Add(translated);
                }
                else
                {
                    // If any part fails, we can't complete the compound translation
                    allTranslated = false;
                    break;
                }
            }

            if (allTranslated && translatedParts.Count == parts.Length)
            {
                // Combine translated parts (Korean typically keeps same order for compounds)
                string result = string.Join(" ", translatedParts);
                return TranslationResult.Hit(result, Name);
            }

            return TranslationResult.Miss();
        }

        /// <summary>
        /// Tries to translate a single word using all available vocabularies.
        /// </summary>
        private bool TryTranslatePart(ITranslationRepository repo, string word, out string translated)
        {
            translated = null;
            string lowerWord = word.ToLower();

            // 1. Check Species dictionary (creatures)
            if (repo.Species.TryGetValue(lowerWord, out translated) ||
                repo.Species.TryGetValue(word, out translated))
            {
                return true;
            }

            // 2. Check BaseNouns (items, types)
            foreach (var kv in repo.BaseNouns)
            {
                if (kv.Key.Equals(word, StringComparison.OrdinalIgnoreCase))
                {
                    translated = kv.Value;
                    return true;
                }
            }

            // 3. Check Prefixes (modifiers, materials, qualities)
            foreach (var kv in repo.Prefixes)
            {
                if (kv.Key.Equals(word, StringComparison.OrdinalIgnoreCase))
                {
                    translated = kv.Value;
                    return true;
                }
            }

            // 4. Check ColorTagVocab (broader vocabulary)
            foreach (var kv in repo.ColorTagVocab)
            {
                if (kv.Key.Equals(word, StringComparison.OrdinalIgnoreCase))
                {
                    translated = kv.Value;
                    return true;
                }
            }

            // 5. Check BodyParts
            if (repo.BodyParts.TryGetValue(lowerWord, out translated) ||
                repo.BodyParts.TryGetValue(word, out translated))
            {
                return true;
            }

            // 6. Check Liquids
            if (repo.Liquids.TryGetValue(lowerWord, out translated) ||
                repo.Liquids.TryGetValue(word, out translated))
            {
                return true;
            }

            // 7. Check creature Names directly
            foreach (var creature in repo.AllCreatures)
            {
                foreach (var namePair in creature.Names)
                {
                    if (namePair.Key.Equals(word, StringComparison.OrdinalIgnoreCase))
                    {
                        translated = namePair.Value;
                        return true;
                    }
                }
            }

            // 8. Check item Names directly
            foreach (var item in repo.AllItems)
            {
                foreach (var namePair in item.Names)
                {
                    if (namePair.Key.Equals(word, StringComparison.OrdinalIgnoreCase))
                    {
                        translated = namePair.Value;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
