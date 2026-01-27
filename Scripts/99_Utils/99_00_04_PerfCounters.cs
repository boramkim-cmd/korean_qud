/*
 * 파일명: 99_00_04_PerfCounters.cs
 * 분류: [Utils] 성능 카운터
 * 역할: 번역 시스템의 성능 측정을 위한 카운터를 제공합니다.
 * 작성일: 2026-01-27
 */

namespace QudKRTranslation.Utils
{
    public static class PerfCounters
    {
        public static long TmpSetterCalls;
        public static long TmpSetterSkipped;
        public static long FontCacheHits;
        public static long TranslationCacheHits;
        public static long TranslationCacheMisses;

        public static void Reset()
        {
            TmpSetterCalls = 0;
            TmpSetterSkipped = 0;
            FontCacheHits = 0;
            TranslationCacheHits = 0;
            TranslationCacheMisses = 0;
        }

        public static string Report()
        {
            long total = TmpSetterCalls;
            double skipPct = total > 0 ? (double)TmpSetterSkipped / total * 100 : 0;
            return $"[Qud-KR Performance]\n" +
                   $"  TMP setter: {total} calls, {TmpSetterSkipped} skipped ({skipPct:F1}%)\n" +
                   $"  Font cache hits: {FontCacheHits}\n" +
                   $"  Translation cache: {TranslationCacheHits} hits, {TranslationCacheMisses} misses";
        }
    }
}
