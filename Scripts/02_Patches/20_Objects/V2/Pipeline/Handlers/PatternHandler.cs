/*
 * 파일명: PatternHandler.cs
 * 분류: Pipeline - Handler
 * 역할: 패턴 번역기 위임 핸들러
 * 작성일: 2026-01-26
 */

using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Patterns;

namespace QudKorean.Objects.V2.Pipeline.Handlers
{
    /// <summary>
    /// Handler that delegates to the pattern translator registry.
    /// Tries all registered pattern translators (corpse, food, parts, possessive, of-pattern).
    /// </summary>
    public class PatternHandler : ITranslationHandler
    {
        public string Name => "Pattern";
        public ITranslationHandler Next { get; set; }

        private PatternTranslatorRegistry _registry;

        public PatternHandler()
        {
            _registry = PatternTranslatorRegistry.CreateDefault();
        }

        public TranslationResult Handle(ITranslationContext context)
        {
            // Try all pattern translators
            var result = _registry.TryTranslate(context.OriginalName, context);

            if (result.Success)
            {
                context.SetCached(context.CacheKey, result.Translated);
                return result;
            }

            // Pass to next handler
            if (Next != null)
            {
                return Next.Handle(context);
            }

            return TranslationResult.Miss();
        }
    }
}
