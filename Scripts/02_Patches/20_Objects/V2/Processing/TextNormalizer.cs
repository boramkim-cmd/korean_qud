/*
 * 파일명: TextNormalizer.cs
 * 분류: Processing - Utility
 * 역할: 텍스트 정규화 유틸리티
 * 작성일: 2026-01-26
 */

using System.Text.RegularExpressions;

namespace QudKorean.Objects.V2.Processing
{
    /// <summary>
    /// Utility class for text normalization operations.
    /// </summary>
    public static class TextNormalizer
    {
        /// <summary>
        /// Normalizes blueprint IDs for consistent lookup.
        /// "Witchwood Bark" -> "witchwoodbark" (lowercase, no spaces)
        /// </summary>
        public static string NormalizeBlueprintId(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            // 이미 정규화된 경우 (공백 없음 + 모두 소문자) 할당 없이 원본 반환
            bool needsNorm = false;
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                if (c == ' ' || (c >= 'A' && c <= 'Z')) { needsNorm = true; break; }
            }
            if (!needsNorm) return id;
            return id.Replace(" ", "").ToLowerInvariant();
        }

        /// <summary>
        /// Normalizes display names for cache key consistency.
        /// Ensures the same item returns the same cache key regardless of:
        /// - Color tags: {{Y|steel}} -> steel
        /// - Quantity suffixes: x15, x100 -> removed
        /// - State suffixes: [empty], (lit) -> removed
        /// - Case differences: Steel -> steel
        /// </summary>
        public static string NormalizeCacheKey(string originalName)
        {
            if (string.IsNullOrEmpty(originalName))
                return originalName;

            string normalized = originalName;

            // 1. Strip color tags
            normalized = ColorTagProcessor.Strip(normalized);

            // 2. Remove quantity suffixes
            normalized = Regex.Replace(normalized, @"\s*x\d+$", "");

            // 3. Strip state suffixes
            normalized = SuffixExtractor.StripState(normalized);

            // 4. Normalize case
            return normalized.ToLowerInvariant().Trim();
        }
    }
}
