/*
 * 파일명: CorpseTranslator.cs
 * 분류: Patterns - Translator
 * 역할: 시체 패턴 번역기
 * 작성일: 2026-01-26
 */

using System;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Patterns
{
    /// <summary>
    /// Translates corpse patterns: "{creature} corpse" -> "{creature_ko} 시체"
    /// </summary>
    public class CorpseTranslator : IPatternTranslator
    {
        public string Name => "Corpse";
        public int Priority => 10;

        public bool CanHandle(string name)
        {
            string stripped = ColorTagProcessor.Strip(name);
            return stripped.EndsWith(" corpse", StringComparison.OrdinalIgnoreCase);
        }

        public TranslationResult Translate(string name, ITranslationContext context)
        {
            string stripped = ColorTagProcessor.Strip(name);

            // Extract creature part
            string creaturePart = stripped.Substring(0, stripped.Length - " corpse".Length);
            if (string.IsNullOrEmpty(creaturePart))
                return TranslationResult.Miss();

            // Try to find creature translation
            if (TryGetCreatureTranslation(context.Repository, creaturePart, out string creatureKo))
            {
                string translated = $"{creatureKo} 시체";
                return TranslationResult.Hit(translated, Name);
            }

            return TranslationResult.Miss();
        }

        private bool TryGetCreatureTranslation(Data.ITranslationRepository repo, string creatureName, out string translated)
        {
            translated = null;

            // Try creature cache
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

            // Fallback: species dictionary
            if (repo.Species.TryGetValue(creatureName, out translated))
            {
                return true;
            }

            return false;
        }
    }
}
