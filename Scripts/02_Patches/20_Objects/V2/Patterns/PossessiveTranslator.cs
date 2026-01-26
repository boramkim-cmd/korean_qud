/*
 * 파일명: PossessiveTranslator.cs
 * 분류: Patterns - Translator
 * 역할: 소유격 패턴 번역기
 * 작성일: 2026-01-26
 */

using System;
using System.Text.RegularExpressions;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Patterns
{
    /// <summary>
    /// Translates possessive patterns:
    /// - "{creature}'s {part}" -> "{creature_ko}의 {part_ko}"
    /// - "panther's claw" -> "표범의 발톱"
    /// </summary>
    public class PossessiveTranslator : IPatternTranslator
    {
        public string Name => "Possessive";
        public int Priority => 40;

        public bool CanHandle(string name)
        {
            return name.Contains("'s ");
        }

        public TranslationResult Translate(string name, ITranslationContext context)
        {
            string stripped = ColorTagProcessor.Strip(name);
            var repo = context.Repository;

            // Pattern: "{creature}'s {part}"
            var match = Regex.Match(stripped, @"^(.+)'s\s+(.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return TranslationResult.Miss();

            string creature = match.Groups[1].Value.Trim();
            string part = match.Groups[2].Value.Trim();

            // Try to translate creature
            if (!TryGetCreatureTranslation(repo, creature, out string creatureKo))
            {
                return TranslationResult.Miss();
            }

            // Try to translate part
            string partKo = null;

            // Check base noun translations first
            foreach (var noun in repo.BaseNouns)
            {
                if (noun.Key.Equals(part, StringComparison.OrdinalIgnoreCase))
                {
                    partKo = noun.Value;
                    break;
                }
            }

            if (partKo != null)
            {
                return TranslationResult.Hit($"{creatureKo}의 {partKo}", Name);
            }

            // Check item cache
            if (TryGetItemTranslation(repo, part, out partKo))
            {
                return TranslationResult.Hit($"{creatureKo}의 {partKo}", Name);
            }

            // Try body parts from JSON
            if (repo.BodyParts.TryGetValue(part, out partKo))
            {
                return TranslationResult.Hit($"{creatureKo}의 {partKo}", Name);
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

            return false;
        }
    }
}
