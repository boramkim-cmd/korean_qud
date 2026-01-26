/*
 * 파일명: FoodTranslator.cs
 * 분류: Patterns - Translator
 * 역할: 음식 패턴 번역기
 * 작성일: 2026-01-26
 */

using System;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Patterns
{
    /// <summary>
    /// Translates dynamic food patterns:
    /// - "{creature} jerky" -> "{creature_ko} 육포"
    /// - "{creature} meat" -> "{creature_ko} 고기"
    /// - "{creature} haunch" -> "{creature_ko} 넓적다리"
    /// - "preserved {ingredient}" -> "절임 {ingredient_ko}"
    /// - "cooked {ingredient}" -> "조리된 {ingredient_ko}"
    /// - "{creature} gland" -> "{creature_ko} 분비샘"
    /// - "{creature} gland paste" -> "{creature_ko} 분비샘 반죽"
    /// </summary>
    public class FoodTranslator : IPatternTranslator
    {
        public string Name => "Food";
        public int Priority => 20;

        public bool CanHandle(string name)
        {
            string stripped = ColorTagProcessor.Strip(name);
            return stripped.EndsWith(" jerky", StringComparison.OrdinalIgnoreCase) ||
                   stripped.EndsWith(" meat", StringComparison.OrdinalIgnoreCase) ||
                   stripped.EndsWith(" haunch", StringComparison.OrdinalIgnoreCase) ||
                   stripped.StartsWith("preserved ", StringComparison.OrdinalIgnoreCase) ||
                   stripped.StartsWith("cooked ", StringComparison.OrdinalIgnoreCase) ||
                   stripped.StartsWith("congealed ", StringComparison.OrdinalIgnoreCase) ||
                   stripped.StartsWith("concentrated ", StringComparison.OrdinalIgnoreCase) ||
                   stripped.StartsWith("raw ", StringComparison.OrdinalIgnoreCase) ||
                   stripped.StartsWith("dried ", StringComparison.OrdinalIgnoreCase) ||
                   stripped.StartsWith("canned ", StringComparison.OrdinalIgnoreCase) ||
                   stripped.EndsWith(" gland paste", StringComparison.OrdinalIgnoreCase) ||
                   stripped.EndsWith(" gland", StringComparison.OrdinalIgnoreCase);
        }

        public TranslationResult Translate(string name, ITranslationContext context)
        {
            string stripped = ColorTagProcessor.Strip(name);
            var repo = context.Repository;

            // Pattern: "{creature} jerky"
            if (stripped.EndsWith(" jerky", StringComparison.OrdinalIgnoreCase))
            {
                string creaturePart = stripped.Substring(0, stripped.Length - " jerky".Length);
                if (TryGetCreatureTranslation(repo, creaturePart, out string creatureKo))
                {
                    return TranslationResult.Hit($"{creatureKo} 육포", Name);
                }
            }

            // Pattern: "{creature} meat"
            if (stripped.EndsWith(" meat", StringComparison.OrdinalIgnoreCase))
            {
                string creaturePart = stripped.Substring(0, stripped.Length - " meat".Length);
                if (TryGetCreatureTranslation(repo, creaturePart, out string creatureKo))
                {
                    return TranslationResult.Hit($"{creatureKo} 고기", Name);
                }
            }

            // Pattern: "{creature} haunch"
            if (stripped.EndsWith(" haunch", StringComparison.OrdinalIgnoreCase))
            {
                string creaturePart = stripped.Substring(0, stripped.Length - " haunch".Length);
                if (TryGetCreatureTranslation(repo, creaturePart, out string creatureKo))
                {
                    return TranslationResult.Hit($"{creatureKo} 넓적다리", Name);
                }
            }

            // Pattern: "{creature} gland paste"
            if (stripped.EndsWith(" gland paste", StringComparison.OrdinalIgnoreCase))
            {
                string creaturePart = stripped.Substring(0, stripped.Length - " gland paste".Length);

                // Handle "elder" prefix
                if (creaturePart.StartsWith("elder ", StringComparison.OrdinalIgnoreCase))
                {
                    creaturePart = creaturePart.Substring("elder ".Length);
                    if (TryGetCreatureTranslation(repo, creaturePart, out string creatureKo))
                    {
                        return TranslationResult.Hit($"장로 {creatureKo} 분비샘 반죽", Name);
                    }
                }
                else if (TryGetCreatureTranslation(repo, creaturePart, out string creatureKo))
                {
                    return TranslationResult.Hit($"{creatureKo} 분비샘 반죽", Name);
                }
            }

            // Pattern: "{creature} gland"
            if (stripped.EndsWith(" gland", StringComparison.OrdinalIgnoreCase))
            {
                string creaturePart = stripped.Substring(0, stripped.Length - " gland".Length);

                // Handle "elder" prefix
                if (creaturePart.StartsWith("elder ", StringComparison.OrdinalIgnoreCase))
                {
                    creaturePart = creaturePart.Substring("elder ".Length);
                    if (TryGetCreatureTranslation(repo, creaturePart, out string creatureKo))
                    {
                        return TranslationResult.Hit($"장로 {creatureKo} 분비샘", Name);
                    }
                }
                else if (TryGetCreatureTranslation(repo, creaturePart, out string creatureKo))
                {
                    return TranslationResult.Hit($"{creatureKo} 분비샘", Name);
                }
            }

            // Pattern: "preserved {creature/ingredient}"
            if (stripped.StartsWith("preserved ", StringComparison.OrdinalIgnoreCase))
            {
                string ingredientPart = stripped.Substring("preserved ".Length);
                if (TryGetCreatureTranslation(repo, ingredientPart, out string ingredientKo))
                {
                    return TranslationResult.Hit($"절임 {ingredientKo}", Name);
                }
                if (TryGetItemTranslation(repo, ingredientPart, out ingredientKo))
                {
                    return TranslationResult.Hit($"절임 {ingredientKo}", Name);
                }
            }

            // Pattern: "cooked {ingredient}"
            if (stripped.StartsWith("cooked ", StringComparison.OrdinalIgnoreCase))
            {
                string ingredientPart = stripped.Substring("cooked ".Length);
                if (TryGetItemTranslation(repo, ingredientPart, out string ingredientKo))
                {
                    return TranslationResult.Hit($"조리된 {ingredientKo}", Name);
                }
                if (TryGetCreatureTranslation(repo, ingredientPart, out ingredientKo))
                {
                    return TranslationResult.Hit($"조리된 {ingredientKo}", Name);
                }
            }

            // Pattern: "congealed {tonic/liquid}"
            if (stripped.StartsWith("congealed ", StringComparison.OrdinalIgnoreCase))
            {
                string ingredientPart = stripped.Substring("congealed ".Length);
                if (TryGetTonicTranslation(repo, ingredientPart, out string ingredientKo))
                {
                    return TranslationResult.Hit($"응고된 {ingredientKo}", Name);
                }
                if (TryGetItemTranslation(repo, ingredientPart, out ingredientKo))
                {
                    return TranslationResult.Hit($"응고된 {ingredientKo}", Name);
                }
            }

            // Pattern: "raw {creature} {suffix}" - e.g., "raw bear meat"
            if (stripped.StartsWith("raw ", StringComparison.OrdinalIgnoreCase))
            {
                string remainder = stripped.Substring("raw ".Length);

                // Check for "X meat" pattern
                if (remainder.EndsWith(" meat", StringComparison.OrdinalIgnoreCase))
                {
                    string creaturePart = remainder.Substring(0, remainder.Length - " meat".Length);
                    if (TryGetCreatureTranslation(repo, creaturePart, out string creatureKo))
                        return TranslationResult.Hit($"생 {creatureKo} 고기", Name);
                }

                // Generic item translation
                if (TryGetItemTranslation(repo, remainder, out string itemKo))
                    return TranslationResult.Hit($"생 {itemKo}", Name);
                if (TryGetCreatureTranslation(repo, remainder, out itemKo))
                    return TranslationResult.Hit($"생 {itemKo}", Name);
            }

            // Pattern: "dried {item}" - e.g., "dried lah petals"
            if (stripped.StartsWith("dried ", StringComparison.OrdinalIgnoreCase))
            {
                string ingredientPart = stripped.Substring("dried ".Length);
                if (TryGetItemTranslation(repo, ingredientPart, out string ingredientKo))
                {
                    return TranslationResult.Hit($"말린 {ingredientKo}", Name);
                }
                if (TryGetCreatureTranslation(repo, ingredientPart, out ingredientKo))
                {
                    return TranslationResult.Hit($"말린 {ingredientKo}", Name);
                }
            }

            // Pattern: "canned {item}" - e.g., "canned have-it-all"
            if (stripped.StartsWith("canned ", StringComparison.OrdinalIgnoreCase))
            {
                string ingredientPart = stripped.Substring("canned ".Length);
                if (TryGetItemTranslation(repo, ingredientPart, out string ingredientKo))
                {
                    return TranslationResult.Hit($"통조림 {ingredientKo}", Name);
                }
                if (TryGetCreatureTranslation(repo, ingredientPart, out ingredientKo))
                {
                    return TranslationResult.Hit($"통조림 {ingredientKo}", Name);
                }
            }

            // Pattern: "concentrated {creature} gland paste"
            if (stripped.StartsWith("concentrated ", StringComparison.OrdinalIgnoreCase) &&
                stripped.EndsWith(" gland paste", StringComparison.OrdinalIgnoreCase))
            {
                string middle = stripped.Substring("concentrated ".Length);
                middle = middle.Substring(0, middle.Length - " gland paste".Length);
                if (TryGetCreatureTranslation(repo, middle, out string creatureKo))
                {
                    return TranslationResult.Hit($"농축 {creatureKo} 분비샘 반죽", Name);
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

            foreach (var noun in repo.BaseNouns)
            {
                if (noun.Key.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                {
                    translated = noun.Value;
                    return true;
                }
            }

            return false;
        }

        private bool TryGetTonicTranslation(Data.ITranslationRepository repo, string tonicName, out string translated)
        {
            translated = null;

            // Check tonics dictionary
            if (repo.Tonics.TryGetValue(tonicName, out translated))
            {
                return true;
            }

            // Check shaders (for tonic color tags like "blaze", "love", etc.)
            if (repo.Shaders.TryGetValue(tonicName, out translated))
            {
                return true;
            }

            return false;
        }
    }
}
