/*
 * 파일명: DictionaryCache.cs
 * 분류: Data - Utility
 * 역할: 정렬된 사전 캐시 관리
 * 작성일: 2026-01-26
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace QudKorean.Objects.V2.Data
{
    /// <summary>
    /// Utility class for managing sorted dictionary caches.
    /// Ensures longest-first matching for greedy prefix/suffix matching.
    /// </summary>
    public static class DictionaryCache
    {
        /// <summary>
        /// Converts a dictionary to a list sorted by key length (longest first).
        /// This enables greedy matching where "folded carbide" is matched before "carbide".
        /// </summary>
        public static List<KeyValuePair<string, string>> SortByKeyLength(Dictionary<string, string> dict)
        {
            if (dict == null || dict.Count == 0)
                return new List<KeyValuePair<string, string>>();

            var list = dict.ToList();
            list.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
            return list;
        }

        /// <summary>
        /// Merges source dictionary into target, without overwriting existing keys.
        /// </summary>
        public static void MergeInto(Dictionary<string, string> target, Dictionary<string, string> source)
        {
            if (source == null) return;
            foreach (var kvp in source)
            {
                if (!target.ContainsKey(kvp.Key))
                {
                    target[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Merges multiple dictionaries into one, sorted by key length.
        /// First dictionary has priority for duplicate keys.
        /// </summary>
        public static List<KeyValuePair<string, string>> MergeAndSort(params Dictionary<string, string>[] dictionaries)
        {
            var combined = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dict in dictionaries)
            {
                if (dict == null) continue;
                foreach (var kvp in dict)
                {
                    if (!combined.ContainsKey(kvp.Key))
                    {
                        combined[kvp.Key] = kvp.Value;
                    }
                }
            }

            return SortByKeyLength(combined);
        }

        /// <summary>
        /// Creates a case-insensitive dictionary from key-value pairs.
        /// </summary>
        public static Dictionary<string, string> ToIgnoreCaseDictionary(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in pairs)
            {
                dict[kvp.Key] = kvp.Value;
            }
            return dict;
        }
    }
}
