/*
 * 파일명: DirectMatchHandler.cs
 * 분류: Pipeline - Handler
 * 역할: 직접 매칭 핸들러
 * 작성일: 2026-01-26
 */

using System;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Data;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Pipeline.Handlers
{
    /// <summary>
    /// Handler that tries direct blueprint/name matching from the cache.
    /// </summary>
    public class DirectMatchHandler : ITranslationHandler
    {
        public string Name => "DirectMatch";
        public ITranslationHandler Next { get; set; }

        public TranslationResult Handle(ITranslationContext context)
        {
            var repo = context.Repository;
            string blueprint = context.Blueprint;
            string originalName = context.OriginalName;

            // Translate materials in color tags first
            string withTranslatedMaterials = ColorTagProcessor.TranslateMaterials(originalName, repo);

            // Try creature cache first, then item cache
            string normalizedBlueprint = TextNormalizer.NormalizeBlueprintId(blueprint);
            ObjectData data = repo.GetCreature(normalizedBlueprint) ??
                              repo.GetItem(normalizedBlueprint) ??
                              repo.GetCreature(blueprint) ??
                              repo.GetItem(blueprint);

            if (data != null)
            {
                // Try exact match with color-tagged version
                if (data.Names.TryGetValue(originalName, out string exactMatch) && !string.IsNullOrEmpty(exactMatch))
                {
                    CacheAndReturn(context, exactMatch);
                    return TranslationResult.Hit(exactMatch, Name);
                }

                // Try exact match with material-translated version
                if (withTranslatedMaterials != originalName &&
                    data.Names.TryGetValue(withTranslatedMaterials, out string materialMatch) &&
                    !string.IsNullOrEmpty(materialMatch))
                {
                    CacheAndReturn(context, materialMatch);
                    return TranslationResult.Hit(materialMatch, Name);
                }

                // Try stripped version (no color tags)
                string strippedOriginal = ColorTagProcessor.Strip(withTranslatedMaterials);
                if (data.Names.TryGetValue(strippedOriginal, out string koreanName) && !string.IsNullOrEmpty(koreanName))
                {
                    string translated = ColorTagProcessor.RestoreFormatting(originalName, strippedOriginal, koreanName, "", "");
                    if (!string.IsNullOrEmpty(translated))
                    {
                        translated = ColorTagProcessor.TranslateNounsOutsideTags(translated, repo);
                        CacheAndReturn(context, translated);
                        return TranslationResult.Hit(translated, Name);
                    }
                }

                // Check state suffix BEFORE partial matching
                string noStateSuffix = SuffixExtractor.StripState(strippedOriginal);
                if (noStateSuffix != strippedOriginal)
                {
                    string suffix = strippedOriginal.Substring(noStateSuffix.Length);

                    if (data.Names.TryGetValue(noStateSuffix, out string baseNameKo) && !string.IsNullOrEmpty(baseNameKo))
                    {
                        string suffixKo = SuffixExtractor.TranslateState(suffix, repo);
                        string translated = ColorTagProcessor.RestoreFormatting(originalName, noStateSuffix, baseNameKo, suffix, suffixKo);

                        if (!string.IsNullOrEmpty(translated))
                        {
                            translated = ColorTagProcessor.TranslateNounsOutsideTags(translated, repo);
                            CacheAndReturn(context, translated);
                            return TranslationResult.Hit(translated, Name);
                        }
                    }
                }

                // Try partial match fallback
                string strippedFromOriginal = ColorTagProcessor.Strip(originalName);
                foreach (var kvp in data.Names)
                {
                    if (!string.IsNullOrEmpty(kvp.Value))
                    {
                        // Use withTranslatedMaterials to preserve color tag translations
                        bool inTranslated = withTranslatedMaterials.Contains(kvp.Key);
                        bool inStripped = strippedOriginal.Contains(kvp.Key);
                        bool inOriginalStripped = strippedFromOriginal.Contains(kvp.Key);

                        if (inTranslated || inStripped || inOriginalStripped)
                        {
                            string translated;
                            if (inTranslated)
                                translated = withTranslatedMaterials.Replace(kvp.Key, kvp.Value);
                            else if (inOriginalStripped)
                                translated = strippedFromOriginal.Replace(kvp.Key, kvp.Value);
                            else
                                translated = strippedOriginal.Replace(kvp.Key, kvp.Value);

                            if (!string.IsNullOrEmpty(translated))
                            {
                                translated = ColorTagProcessor.TranslateNounsOutsideTags(translated, repo);
                                CacheAndReturn(context, translated);
                                return TranslationResult.Hit(translated, Name);
                            }
                        }
                    }
                }
            }

            // Try global search with state suffix stripped (O(1) via GlobalNameIndex)
            string globalStripped = ColorTagProcessor.Strip(withTranslatedMaterials);
            string globalNoSuffix = SuffixExtractor.StripState(globalStripped);
            if (globalNoSuffix != globalStripped)
            {
                string globalSuffix = globalStripped.Substring(globalNoSuffix.Length);

                if (repo.GlobalNameIndex.TryGetValue(globalNoSuffix, out string globalMatch))
                {
                    string suffixKo = SuffixExtractor.TranslateState(globalSuffix, repo);
                    string translated = ColorTagProcessor.RestoreFormatting(withTranslatedMaterials, globalNoSuffix, globalMatch, globalSuffix, suffixKo);
                    translated = ColorTagProcessor.TranslateNounsOutsideTags(translated, repo);
                    CacheAndReturn(context, translated);
                    return TranslationResult.Hit(translated, Name);
                }
            }

            // Pass to next handler
            if (Next != null)
            {
                return Next.Handle(context);
            }

            return TranslationResult.Miss();
        }

        private void CacheAndReturn(ITranslationContext context, string translated)
        {
            context.SetCached(context.CacheKey, translated);
        }
    }
}
