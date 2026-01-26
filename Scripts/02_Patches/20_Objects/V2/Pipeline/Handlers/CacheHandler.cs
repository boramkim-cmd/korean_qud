/*
 * 파일명: CacheHandler.cs
 * 분류: Pipeline - Handler
 * 역할: 캐시 조회 핸들러
 * 작성일: 2026-01-26
 */

using QudKorean.Objects.V2.Core;

namespace QudKorean.Objects.V2.Pipeline.Handlers
{
    /// <summary>
    /// First handler in the pipeline - checks the cache for existing translations.
    /// </summary>
    public class CacheHandler : ITranslationHandler
    {
        public string Name => "Cache";
        public ITranslationHandler Next { get; set; }

        public TranslationResult Handle(ITranslationContext context)
        {
            // Try to get from cache
            if (context.TryGetCached(context.CacheKey, out string cached))
            {
                // Don't return empty strings as successful translations
                if (!string.IsNullOrEmpty(cached))
                {
                    return TranslationResult.Hit(cached, Name);
                }
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
