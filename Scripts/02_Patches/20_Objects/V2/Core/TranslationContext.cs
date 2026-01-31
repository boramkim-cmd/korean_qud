/*
 * 파일명: TranslationContext.cs
 * 분류: Core - Implementation
 * 역할: 번역 컨텍스트 구현
 * 작성일: 2026-01-26
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using QudKorean.Objects.V2.Data;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2.Core
{
    /// <summary>
    /// Implementation of translation context that holds state for a single translation operation.
    /// </summary>
    public class TranslationContext : ITranslationContext
    {
        private static readonly ConcurrentDictionary<string, string> _globalCache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ITranslationRepository Repository { get; }
        public string Blueprint { get; }
        public string OriginalName { get; }
        public string CacheKey { get; }

        public TranslationContext(ITranslationRepository repository, string blueprint, string originalName)
        {
            Repository = repository;
            Blueprint = blueprint;
            OriginalName = originalName;
            CacheKey = $"{blueprint}:{TextNormalizer.NormalizeCacheKey(originalName)}";
        }

        public bool TryGetCached(string key, out string value)
        {
            return _globalCache.TryGetValue(key, out value);
        }

        public void SetCached(string key, string value)
        {
            _globalCache.TryAdd(key, value);
        }

        /// <summary>
        /// Clears the global translation cache.
        /// </summary>
        public static void ClearCache()
        {
            _globalCache.Clear();
        }

        /// <summary>
        /// Gets the current cache count for statistics.
        /// </summary>
        public static int CacheCount => _globalCache.Count;
    }
}
