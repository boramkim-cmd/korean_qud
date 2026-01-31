/*
 * 파일명: FallbackHandler.cs
 * 분류: Pipeline - Handler
 * 역할: 최종 폴백 핸들러
 * 작성일: 2026-01-26
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Pipeline.Handlers
{
    /// <summary>
    /// Final fallback handler that attempts color tag material translation
    /// and base noun translation as a last resort.
    /// </summary>
    public class FallbackHandler : ITranslationHandler
    {
        private static readonly Regex RxOfPattern = new Regex(@"^(.+?)\s+of\s+(?:the\s+)?(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Name => "Fallback";
        public ITranslationHandler Next { get; set; }

        public TranslationResult Handle(ITranslationContext context)
        {
            var repo = context.Repository;
            string originalName = context.OriginalName;

            // NEW: 색상 태그 내 소유격 패턴 먼저 처리
            string withPossessives = ColorTagProcessor.TranslatePossessivesInTags(originalName, repo);

            // Step 1: Translate materials in color tags
            string withTranslatedMaterials = ColorTagProcessor.TranslateMaterials(withPossessives, repo);

            // If materials were translated, also try to translate base nouns outside
            if (withTranslatedMaterials != originalName)
            {
                // Check "of X" pattern first (using original English)
                string strippedForOf = ColorTagProcessor.Strip(originalName);
                if (strippedForOf.Contains(" of "))
                {
                    if (TryTranslateOfPattern(strippedForOf, repo, out string ofTranslated))
                    {
                        CacheAndReturn(context, ofTranslated);
                        return TranslationResult.Hit(ofTranslated, Name);
                    }
                }

                // Translate base nouns outside the color tags
                string translated = ColorTagProcessor.TranslateNounsOutsideTags(withTranslatedMaterials, repo);
                CacheAndReturn(context, translated);
                return TranslationResult.Partial(translated, Name);
            }

            // Last resort: Try translating just the base nouns even without color tags
            string withBaseNouns = ColorTagProcessor.TranslateNounsOutsideTags(originalName, repo);
            if (withBaseNouns != originalName)
            {
                CacheAndReturn(context, withBaseNouns);
                return TranslationResult.Partial(withBaseNouns, Name);
            }

            return TranslationResult.Miss();
        }

        private bool TryTranslateOfPattern(string stripped, Data.ITranslationRepository repo, out string translated)
        {
            translated = null;

            // "X of Y" pattern matching
            var match = RxOfPattern.Match(stripped);
            if (!match.Success)
                return false;

            string itemPart = match.Groups[1].Value.Trim();
            string ofPart = match.Groups[2].Value.Trim();

            // Try of_patterns dictionary
            string ofKo = null;
            string fullOfPatternWithThe = $"of the {ofPart}";
            string fullOfPattern = $"of {ofPart}";

            if (repo.OfPatterns.TryGetValue(fullOfPatternWithThe, out ofKo))
            {
                // ofKo already includes "의"
            }
            else if (repo.OfPatterns.TryGetValue(fullOfPattern, out ofKo))
            {
                // ofKo already includes "의"
            }
            else if (repo.OfPatterns.TryGetValue(ofPart, out ofKo))
            {
                ofKo = $"{ofKo}의";
            }
            else
            {
                // Try to translate the of part
                string ofPartTranslated = TranslateWithPrefixesAndNouns(ofPart, repo);
                if (ofPartTranslated == ofPart)
                    return false;
                ofKo = $"{ofPartTranslated}의";
            }

            // Translate item part
            string itemKo = TranslateWithPrefixesAndNouns(itemPart, repo);

            // Korean word order: "Y의 X"
            translated = $"{ofKo} {itemKo}".Trim();
            return true;
        }

        private string TranslateWithPrefixesAndNouns(string text, Data.ITranslationRepository repo)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var nounDict = repo.BaseNounsDict;
            var prefixDict = repo.PrefixesDict;

            string[] words = text.Split(' ');
            bool changed = false;

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (word.Length == 0) continue;

                // Separate leading/trailing punctuation
                int leadLen = 0;
                while (leadLen < word.Length && IsBoundary(word[leadLen]))
                    leadLen++;

                int trailLen = 0;
                while (trailLen < word.Length - leadLen && IsBoundary(word[word.Length - 1 - trailLen]))
                    trailLen++;

                string core = word.Substring(leadLen, word.Length - leadLen - trailLen);
                if (core.Length == 0) continue;

                string translated;
                if (nounDict.TryGetValue(core, out translated) ||
                    prefixDict.TryGetValue(core, out translated))
                {
                    string lead = leadLen > 0 ? word.Substring(0, leadLen) : "";
                    string trail = trailLen > 0 ? word.Substring(word.Length - trailLen) : "";
                    words[i] = lead + translated + trail;
                    changed = true;
                }
            }

            return changed ? string.Join(" ", words) : text;
        }

        private static bool IsBoundary(char c)
        {
            return c == '[' || c == ']' || c == '(' || c == ')' ||
                   c == '"' || c == '\'' || c == ',' || c == '.' ||
                   c == ':' || c == ';' || c == '!' || c == '?';
        }

        private void CacheAndReturn(ITranslationContext context, string translated)
        {
            context.SetCached(context.CacheKey, translated);
        }
    }
}
