/*
 * 파일명: ITranslationContext.cs
 * 분류: Core - Interface
 * 역할: 번역 컨텍스트 인터페이스 정의
 * 작성일: 2026-01-26
 */

namespace QudKorean.Objects.V2.Core
{
    /// <summary>
    /// Translation context interface that provides access to repository
    /// and manages cache for a single translation operation.
    /// </summary>
    public interface ITranslationContext
    {
        /// <summary>
        /// The translation repository for data access.
        /// </summary>
        Data.ITranslationRepository Repository { get; }

        /// <summary>
        /// The blueprint ID being translated.
        /// </summary>
        string Blueprint { get; }

        /// <summary>
        /// The original English name to translate.
        /// </summary>
        string OriginalName { get; }

        /// <summary>
        /// Tries to get a cached translation result.
        /// </summary>
        bool TryGetCached(string key, out string value);

        /// <summary>
        /// Sets a cached translation result.
        /// </summary>
        void SetCached(string key, string value);

        /// <summary>
        /// The normalized cache key for this context.
        /// </summary>
        string CacheKey { get; }
    }
}
