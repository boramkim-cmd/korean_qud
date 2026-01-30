/*
 * 파일명: PrefixSuffixHandler.cs
 * 분류: Pipeline - Handler
 * 역할: 접두사/접미사 처리 핸들러
 * 작성일: 2026-01-26
 */

using System;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Pipeline.Handlers
{
    /// <summary>
    /// Handler that extracts and translates prefixes/suffixes.
    /// Handles patterns like "wooden arrow", "dagger +3", "torch x14 (unburnt)".
    /// </summary>
    public class PrefixSuffixHandler : ITranslationHandler
    {
        public string Name => "PrefixSuffix";
        public ITranslationHandler Next { get; set; }

        public TranslationResult Handle(ITranslationContext context)
        {
            var repo = context.Repository;
            string originalName = context.OriginalName;

            // First translate prefixes/materials in color tags (for later restoration)
            string withTranslatedTags = ColorTagProcessor.TranslateMaterials(originalName, repo);

            // Strip ORIGINAL (not translated) for prefix matching - need English keys!
            string strippedForPrefix = ColorTagProcessor.Strip(originalName);
            string baseNameForPrefix = SuffixExtractor.ExtractAll(strippedForPrefix, out string allSuffixes);

            // Try with prefixes
            if (PrefixExtractor.TryExtract(baseNameForPrefix, repo, out string prefixKo, out string remainder))
            {
                // Try to find translation for the remainder (base item name)
                if (TryGetItemTranslation(repo, remainder, out string baseKo) ||
                    TryGetCreatureTranslation(repo, remainder, out baseKo))
                {
                    string suffixKo = SuffixExtractor.TranslateAll(allSuffixes, repo);
                    string translated;

                    // Restore color tags if present
                    if (withTranslatedTags.Contains("{{"))
                    {
                        translated = ColorTagProcessor.RestoreFormatting(
                            withTranslatedTags, remainder, baseKo, allSuffixes, suffixKo);
                    }
                    else
                    {
                        translated = $"{prefixKo} {baseKo}{suffixKo}";
                    }

                    CacheAndReturn(context, translated);
                    return TranslationResult.Hit(translated, Name);
                }

                // Try extracting additional prefix (material) from remainder
                if (PrefixExtractor.TryExtract(remainder, repo, out string materialKo, out string baseOnly))
                {
                    if (TryGetItemTranslation(repo, baseOnly, out string baseKo2) ||
                        TryGetCreatureTranslation(repo, baseOnly, out baseKo2))
                    {
                        string suffixKo = SuffixExtractor.TranslateAll(allSuffixes, repo);
                        string translated;

                        if (withTranslatedTags.Contains("{{"))
                        {
                            translated = ColorTagProcessor.RestoreFormatting(
                                withTranslatedTags, baseOnly, baseKo2, allSuffixes, suffixKo);
                        }
                        else
                        {
                            translated = $"{prefixKo} {materialKo} {baseKo2}{suffixKo}";
                        }

                        CacheAndReturn(context, translated);
                        return TranslationResult.Hit(translated, Name);
                    }
                }

                // Try extracting base noun from end of remainder
                // Pattern: "<creature_or_modifier> <base_noun>" e.g., "진눈깨비수염 gland paste"
                if (TryExtractBaseNounFromEnd(repo, remainder, out string modifierPart, out string baseNounKo))
                {
                    // Try translating modifier as creature
                    if (!TryGetCreatureTranslation(repo, modifierPart, out string modifierKo))
                    {
                        modifierKo = modifierPart; // Keep as-is if not a known creature
                    }
                    string suffixKo = SuffixExtractor.TranslateAll(allSuffixes, repo);
                    string translated;

                    if (withTranslatedTags.Contains("{{"))
                    {
                        translated = ColorTagProcessor.RestoreFormatting(
                            withTranslatedTags, modifierPart, modifierKo, allSuffixes, suffixKo);
                        // Also need to translate the base noun in the result
                        // The restoration may not handle this case well, so do simple assembly
                        translated = $"{prefixKo} {modifierKo} {baseNounKo}{suffixKo}";
                    }
                    else
                    {
                        translated = $"{prefixKo} {modifierKo} {baseNounKo}{suffixKo}";
                    }

                    CacheAndReturn(context, translated);
                    return TranslationResult.Hit(translated, Name);
                }
            }

            // Try base item name lookup (handles both simple items and items with suffixes)
            if (TryGetItemTranslation(repo, baseNameForPrefix, out string baseKo3) ||
                TryGetCreatureTranslation(repo, baseNameForPrefix, out baseKo3))
            {
                string suffixKo = SuffixExtractor.TranslateAll(allSuffixes, repo);
                string translated = string.IsNullOrEmpty(suffixKo) ? baseKo3 : $"{baseKo3}{suffixKo}";
                CacheAndReturn(context, translated);
                return TranslationResult.Hit(translated, Name);
            }

            // Pass to next handler
            if (Next != null)
            {
                return Next.Handle(context);
            }

            return TranslationResult.Miss();
        }

        private bool TryGetCreatureTranslation(Data.ITranslationRepository repo, string creatureName, out string translated)
        {
            translated = null;

            // O(1) lookup via GlobalNameIndex (replaces O(n) AllCreatures scan)
            if (repo.GlobalNameIndex.TryGetValue(creatureName, out translated) && !string.IsNullOrEmpty(translated))
                return true;

            // Fallback: species dictionary
            if (repo.Species.TryGetValue(creatureName, out translated))
                return true;

            return false;
        }

        private bool TryGetItemTranslation(Data.ITranslationRepository repo, string itemName, out string translated)
        {
            translated = null;

            // O(1) lookup via GlobalNameIndex (replaces O(n) AllItems scan)
            if (repo.GlobalNameIndex.TryGetValue(itemName, out translated) && !string.IsNullOrEmpty(translated))
                return true;

            // Fallback: base nouns dictionary (sorted list, still O(n) but small)
            var baseNouns = repo.BaseNouns;
            foreach (var noun in baseNouns)
            {
                if (noun.Key.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                {
                    translated = noun.Value;
                    return true;
                }
            }

            return false;
        }

        private void CacheAndReturn(ITranslationContext context, string translated)
        {
            context.SetCached(context.CacheKey, translated);
        }

        /// <summary>
        /// Tries to extract a base noun from the end of a string.
        /// Pattern: "modifier baseNoun" e.g., "진눈깨비수염 gland paste" -> ("진눈깨비수염", "분비샘 페이스트")
        /// </summary>
        private bool TryExtractBaseNounFromEnd(Data.ITranslationRepository repo, string text, out string modifier, out string baseNounKo)
        {
            modifier = null;
            baseNounKo = null;

            if (string.IsNullOrEmpty(text) || !text.Contains(" "))
                return false;

            // Check base nouns (sorted by length, longest first)
            foreach (var noun in repo.BaseNouns)
            {
                string suffix = " " + noun.Key;
                if (text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    modifier = text.Substring(0, text.Length - suffix.Length).Trim();
                    baseNounKo = noun.Value;
                    return true;
                }
            }

            return false;
        }
    }
}
