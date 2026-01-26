/*
 * 파일명: PartsTranslator.cs
 * 분류: Patterns - Translator
 * 역할: 부위 패턴 번역기
 * 작성일: 2026-01-26
 */

using System;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Patterns
{
    /// <summary>
    /// Translates dynamic parts patterns:
    /// - "{creature} egg" -> "{creature_ko} 알"
    /// - "{creature} hide" -> "{creature_ko} 가죽"
    /// - "{creature} bone" -> "{creature_ko} 뼈"
    /// - "raw {creature} {part}" -> "생 {creature_ko} {part_ko}"
    /// Uses part_suffixes from _suffixes.json.
    /// </summary>
    public class PartsTranslator : IPatternTranslator
    {
        public string Name => "Parts";
        public int Priority => 30;

        public bool CanHandle(string name)
        {
            // Quick check without full pattern matching
            string stripped = ColorTagProcessor.Strip(name);
            return stripped.Contains(" ") &&
                   (stripped.StartsWith("raw ", StringComparison.OrdinalIgnoreCase) ||
                    !stripped.StartsWith("elder ", StringComparison.OrdinalIgnoreCase) ||
                    stripped.Contains(" "));
        }

        public TranslationResult Translate(string name, ITranslationContext context)
        {
            string stripped = ColorTagProcessor.Strip(name);
            var repo = context.Repository;

            var partPatterns = repo.PartSuffixes;
            if (partPatterns == null || partPatterns.Count == 0)
                return TranslationResult.Miss();

            // "raw {creature} {part}" pattern
            if (stripped.StartsWith("raw ", StringComparison.OrdinalIgnoreCase))
            {
                string remainder = stripped.Substring("raw ".Length);
                foreach (var kvp in partPatterns)
                {
                    if (remainder.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        string creaturePart = remainder.Substring(0, remainder.Length - kvp.Key.Length);
                        if (TryGetCreatureTranslation(repo, creaturePart, out string creatureKo))
                        {
                            return TranslationResult.Hit($"생 {creatureKo}{kvp.Value}", Name);
                        }
                    }
                }
            }

            // General "{creature} {part}" pattern (with elder prefix support)
            foreach (var kvp in partPatterns)
            {
                if (stripped.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    string creaturePart = stripped.Substring(0, stripped.Length - kvp.Key.Length);

                    // Handle "elder" prefix
                    if (creaturePart.StartsWith("elder ", StringComparison.OrdinalIgnoreCase))
                    {
                        string actualCreature = creaturePart.Substring("elder ".Length);
                        if (TryGetCreatureTranslation(repo, actualCreature, out string creatureKo))
                        {
                            return TranslationResult.Hit($"장로 {creatureKo}{kvp.Value}", Name);
                        }
                    }

                    if (TryGetCreatureTranslation(repo, creaturePart, out string creatureKo2))
                    {
                        return TranslationResult.Hit($"{creatureKo2}{kvp.Value}", Name);
                    }
                }
            }

            return TranslationResult.Miss();
        }

        private bool TryGetCreatureTranslation(Data.ITranslationRepository repo, string creatureName, out string translated)
        {
            translated = null;

            foreach (var creature in repo.AllCreatures)
            {
                foreach (var namePair in creature.Names)
                {
                    if (namePair.Key.Equals(creatureName, StringComparison.OrdinalIgnoreCase))
                    {
                        translated = namePair.Value;
                        return true;
                    }
                }
            }

            if (repo.Species.TryGetValue(creatureName, out translated))
            {
                return true;
            }

            return false;
        }
    }
}
