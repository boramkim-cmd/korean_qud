/*
 * 파일명: 02_20_00_ObjectTranslator.cs
 * 분류: [Patch] 오브젝트 번역
 * 역할: 생물/아이템 이름 및 설명을 번역하는 독립 시스템
 * 작성일: 2026-01-22
 * 비고: 기존 TranslationEngine/StructureTranslator와 분리된 캐시 사용
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using XRL;

namespace QudKorean.Objects
{
    /// <summary>
    /// Isolated translator for creature and item names/descriptions.
    /// Has its own cache separate from StructureTranslator to prevent collisions.
    /// </summary>
    public static class ObjectTranslator
    {
        #region Data Structures
        
        public class ObjectData
        {
            public string BlueprintId { get; set; }
            public Dictionary<string, string> Names { get; set; } = new(StringComparer.OrdinalIgnoreCase);
            public string Description { get; set; }
            public string DescriptionKo { get; set; }
        }
        
        #endregion
        
        #region Private Fields (Isolated Caches)
        
        // Separate caches - NO collision with StructureTranslator possible
        private static Dictionary<string, ObjectData> _creatureCache = new();
        private static Dictionary<string, ObjectData> _itemCache = new();
        private static Dictionary<string, string> _displayNameCache = new();
        
        private static bool _initialized = false;
        private static string _modDirectory = null;

        private const string LOG_PREFIX = "[QudKR-Objects]";

        // JSON에서 로드된 어휘 사전
        private static Dictionary<string, string> _materialsLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _qualitiesLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _modifiersLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _processingLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _tonicsLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _grenadesLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _marksLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _colorsLoaded = new(StringComparer.OrdinalIgnoreCase);

        // creatures/_common.json에서 로드된 species 사전
        private static Dictionary<string, string> _speciesLoaded = new(StringComparer.OrdinalIgnoreCase);

        // items/_nouns.json에서 로드된 base noun 사전
        private static Dictionary<string, string> _baseNounsLoaded = new(StringComparer.OrdinalIgnoreCase);

        // _suffixes.json에서 로드된 접미사 사전들
        private static Dictionary<string, string> _statesLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _liquidsLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _ofPatternsLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> _bodyPartsLoaded = new(StringComparer.OrdinalIgnoreCase);
        private static List<KeyValuePair<string, string>> _partSuffixesLoaded = null;

        // 통합 접두사 사전 (긴 것 우선 정렬) - JSON에서 로드
        private static List<KeyValuePair<string, string>> _allPrefixesSortedLoaded = null;

        // 컬러 태그 내부용 (materials + qualities + tonics + grenades) - JSON에서 로드
        private static List<KeyValuePair<string, string>> _colorTagVocabSortedLoaded = null;

        // JSON에서 로드된 base nouns 정렬 캐시
        private static List<KeyValuePair<string, string>> _baseNounsSortedLoaded = null;

        #endregion

        #region Prefix/Suffix Dictionaries

        // NOTE: Hardcoded prefix dictionaries removed - now loaded from items/_common.json
        // See: LOCALIZATION/OBJECTS/items/_common.json
        // Removed: _materialPrefixes, _qualityPrefixes, _processingPrefixes, _descriptivePrefixes

        // Cached list of all prefixes sorted by length (longest first)
        private static List<KeyValuePair<string, string>> _allPrefixesSorted = null;

        /// <summary>
        /// Gets all prefix dictionaries combined and sorted by key length (longest first).
        /// This ensures "folded carbide" is matched before "carbide".
        /// </summary>
        private static List<KeyValuePair<string, string>> GetAllPrefixesSorted()
        {
            // Use JSON-loaded data
            if (_allPrefixesSortedLoaded != null && _allPrefixesSortedLoaded.Count > 0)
            {
                return _allPrefixesSortedLoaded;
            }

            // Fallback: return empty list with warning
            if (_allPrefixesSorted == null)
            {
                UnityEngine.Debug.LogWarning($"{LOG_PREFIX} GetAllPrefixesSorted: items/_common.json not loaded, prefix translation disabled");
                _allPrefixesSorted = new List<KeyValuePair<string, string>>();
            }
            return _allPrefixesSorted;
        }

        /// <summary>
        /// Extracts and translates all prefixes from a name.
        /// "wooden arrow" → ("나무", "arrow")
        /// "flawless crysteel dagger" → ("완벽한 크리스틸", "dagger")
        /// </summary>
        private static bool TryExtractAndTranslatePrefixes(string name, out string prefixKo, out string remainder)
        {
            prefixKo = null;
            remainder = name;

            var allPrefixes = GetAllPrefixesSorted();
            // Debug: Log prefix extraction attempt
            // UnityEngine.Debug.Log($"{LOG_PREFIX} TryExtractAndTranslatePrefixes: input='{name}', prefixCount={allPrefixes.Count}");

            List<string> translatedPrefixes = new List<string>();
            string current = name;

            // Iteratively extract prefixes (there may be multiple)
            bool foundAny = true;
            while (foundAny)
            {
                foundAny = false;
                foreach (var prefix in allPrefixes)
                {
                    if (current.StartsWith(prefix.Key + " ", StringComparison.OrdinalIgnoreCase))
                    {
                        translatedPrefixes.Add(prefix.Value);
                        current = current.Substring(prefix.Key.Length + 1);
                        foundAny = true;
                        // Debug: Log successful prefix match
                        // UnityEngine.Debug.Log($"{LOG_PREFIX} TryExtractAndTranslatePrefixes: found prefix '{prefix.Key}' -> '{prefix.Value}', remainder='{current}'");
                        break; // Restart search with longest prefixes first
                    }
                }
            }

            if (translatedPrefixes.Count > 0)
            {
                prefixKo = string.Join(" ", translatedPrefixes);
                remainder = current;
                // Debug: Log result
                // UnityEngine.Debug.Log($"{LOG_PREFIX} TryExtractAndTranslatePrefixes: SUCCESS prefixKo='{prefixKo}', remainder='{remainder}'");
                return true;
            }

            // Debug: Log failure
            // UnityEngine.Debug.Log($"{LOG_PREFIX} TryExtractAndTranslatePrefixes: FAILED for '{name}'");
            return false;
        }

        /// <summary>
        /// Extracts all suffixes from a name (quantity, state brackets, parentheses, "of X", "+X", stats).
        /// "torch x14 (unburnt)" → ("torch", " x14 (unburnt)")
        /// "sword of fire" → ("sword", " of fire")
        /// "dagger +3" → ("dagger", " +3")
        /// "musket →8 ♥1d8 [empty]" → ("musket", " →8 ♥1d8 [empty]")
        /// </summary>
        private static string ExtractAllSuffixes(string name, out string suffixes)
        {
            suffixes = "";
            if (string.IsNullOrEmpty(name)) return name;

            string result = name;
            List<string> extractedSuffixes = new List<string>();

            // 1. Extract parenthesis suffixes: (lit), (unlit), (unburnt), etc.
            var parenMatch = Regex.Match(result, @"(\s*\([^)]+\))$");
            if (parenMatch.Success)
            {
                extractedSuffixes.Insert(0, parenMatch.Value);
                result = result.Substring(0, parenMatch.Index);
            }

            // 2. Extract bracket suffixes: [empty], [full], [32 drams of water], etc.
            var bracketMatch = Regex.Match(result, @"(\s*\[[^\]]+\])$");
            if (bracketMatch.Success)
            {
                extractedSuffixes.Insert(0, bracketMatch.Value);
                result = result.Substring(0, bracketMatch.Index);
            }

            // 3. Extract quantity suffixes: x3, x14, x15 etc. (can be in the middle now)
            var quantityMatch = Regex.Match(result, @"(\s*x\d+)$");
            if (quantityMatch.Success)
            {
                extractedSuffixes.Insert(0, quantityMatch.Value);
                result = result.Substring(0, quantityMatch.Index);
            }

            // 4. Extract weapon/armor stats at end: →4 ♥1d2, ◆3 ○0, etc.
            // Pattern: stats start with special char followed by number/dice, may have multiple
            var statsMatch = Regex.Match(result, @"(\s+[→◆♦●○]-?\d+(?:\s+[♥♠♣]\d+d\d+(?:\+\d+)?)?)$");
            if (statsMatch.Success)
            {
                extractedSuffixes.Insert(0, statsMatch.Value);
                result = result.Substring(0, statsMatch.Index);
            }
            // Also try armor stats pattern: ◆3 ○0
            var armorStatsMatch = Regex.Match(result, @"(\s+[◆♦]\d+\s+[○●]-?\d+)$");
            if (armorStatsMatch.Success)
            {
                extractedSuffixes.Insert(0, armorStatsMatch.Value);
                result = result.Substring(0, armorStatsMatch.Index);
            }

            // 5. Extract "+X" suffixes: +1, +2, +3, etc. (Phase 2.2)
            var plusMatch = Regex.Match(result, @"(\s*\+\d+)$");
            if (plusMatch.Success)
            {
                extractedSuffixes.Insert(0, plusMatch.Value);
                result = result.Substring(0, plusMatch.Index);
            }

            // 5. Extract "of X" suffixes: "of fire", "of frost", etc. (Phase 2.2)
            var ofMatch = Regex.Match(result, @"(\s+of\s+[\w\s]+)$", RegexOptions.IgnoreCase);
            if (ofMatch.Success)
            {
                extractedSuffixes.Insert(0, ofMatch.Value);
                result = result.Substring(0, ofMatch.Index);
            }

            suffixes = string.Concat(extractedSuffixes);
            return result.Trim();
        }

        /// <summary>
        /// Translates all suffix patterns to Korean.
        /// Handles compound suffixes like " x15 (unburnt)" → " x15 (미사용)"
        /// Also handles "of X" patterns like " of fire" → "의 불"
        /// Uses loaded dictionaries from _suffixes.json
        /// </summary>
        private static string TranslateAllSuffixes(string suffixes)
        {
            if (string.IsNullOrEmpty(suffixes)) return "";

            string result = suffixes;

            // Apply state translations from _suffixes.json
            foreach (var kvp in _statesLoaded)
            {
                if (result.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = Regex.Replace(result, Regex.Escape(kvp.Key), kvp.Value, RegexOptions.IgnoreCase);
                }
            }

            // [X drams of Y] pattern → [Y X드램]
            result = Regex.Replace(result, @"\[(\d+) drams? of ([^\]]+)\]", m => {
                string amount = m.Groups[1].Value;
                string liquid = m.Groups[2].Value.Trim();
                string liquidKo = _liquidsLoaded.TryGetValue(liquid, out var ko) ? ko : liquid;
                return $"[{liquidKo} {amount}드램]";
            }, RegexOptions.IgnoreCase);

            // [X servings] pattern → [X인분]
            result = Regex.Replace(result, @"\[(\d+) servings?\]", "[$1인분]", RegexOptions.IgnoreCase);

            // "of X" pattern → "의 X번역" (Phase 2.2)
            result = Regex.Replace(result, @"\s+of\s+([\w\s]+)$", m => {
                string element = m.Groups[1].Value.Trim();
                string elementKo = _ofPatternsLoaded.TryGetValue(element, out var ko) ? ko : element;
                return $"의 {elementKo}";
            }, RegexOptions.IgnoreCase);

            return result;
        }

        #endregion

        #region Blueprint ID Normalization

        /// <summary>
        /// Normalizes blueprint IDs for consistent lookup.
        /// "Witchwood Bark" → "witchwoodbark" (lowercase, no spaces)
        /// </summary>
        private static string NormalizeBlueprintId(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            return id.Replace(" ", "").ToLowerInvariant();
        }

        /// <summary>
        /// Normalizes display names for cache key consistency.
        /// This ensures the same item returns the same cache key regardless of:
        /// - Color tags: {{Y|steel}} → steel
        /// - Quantity suffixes: x15, x100 → removed
        /// - State suffixes: [empty], (lit) → removed
        /// - Case differences: Steel → steel
        /// </summary>
        private static string NormalizeCacheKey(string originalName)
        {
            if (string.IsNullOrEmpty(originalName))
                return originalName;

            string normalized = originalName;

            // 1. Strip color tags: {{Y|steel}} → steel
            normalized = StripColorTags(normalized);

            // 2. Remove quantity suffixes: "x15", "x100"
            normalized = Regex.Replace(normalized, @"\s*x\d+$", "");

            // 3. Strip state suffixes: [empty], (lit), weapon stats
            normalized = StripStateSuffix(normalized);

            // 4. Normalize case
            return normalized.ToLowerInvariant().Trim();
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Ensures JSON data is loaded. Called automatically on first use.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (_initialized) return;
            
            try
            {
                LoadAllJsonFiles();
                _initialized = true;
                UnityEngine.Debug.Log($"{LOG_PREFIX} Initialized: {_creatureCache.Count} creatures, {_itemCache.Count} items");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Initialization failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Reloads JSON files without restarting the game. Used by kr:reload wish command.
        /// </summary>
        public static void ReloadJson()
        {
            _creatureCache.Clear();
            _itemCache.Clear();
            _displayNameCache.Clear();
            _allPrefixesSorted = null; // Clear prefix cache
            _colorTagMaterialsSorted = null; // Clear color tag materials cache
            _baseNounsSorted = null; // Clear base nouns cache (Phase 3.2)

            // Clear JSON-loaded vocabulary
            _materialsLoaded.Clear();
            _qualitiesLoaded.Clear();
            _modifiersLoaded.Clear();
            _processingLoaded.Clear();
            _tonicsLoaded.Clear();
            _grenadesLoaded.Clear();
            _marksLoaded.Clear();
            _colorsLoaded.Clear();
            _allPrefixesSortedLoaded = null;
            _colorTagVocabSortedLoaded = null;

            // Clear JSON-loaded species, nouns, suffixes
            _speciesLoaded.Clear();
            _baseNounsLoaded.Clear();
            _statesLoaded.Clear();
            _liquidsLoaded.Clear();
            _ofPatternsLoaded.Clear();
            _bodyPartsLoaded.Clear();
            _partSuffixesLoaded = null;
            _baseNounsSortedLoaded = null;

            _initialized = false;
            EnsureInitialized();
            UnityEngine.Debug.Log($"{LOG_PREFIX} Reloaded!");
        }
        
        /// <summary>
        /// Clears the display name cache. Called on game load/save.
        /// </summary>
        public static void ClearCache()
        {
            _displayNameCache.Clear();
        }
        
        /// <summary>
        /// Attempts to get a translated display name for a blueprint.
        /// </summary>
        /// <param name="blueprint">The blueprint ID (e.g., "Bear", "Dagger")</param>
        /// <param name="originalName">The original English display name</param>
        /// <param name="translated">The translated Korean name, if found</param>
        /// <returns>True if translation was found</returns>
        public static bool TryGetDisplayName(string blueprint, string originalName, out string translated)
        {
            translated = null;
            if (string.IsNullOrEmpty(blueprint)) return false;

            EnsureInitialized();

            // Fast path: display name cache
            // Use normalized cache key for consistency across different contexts
            // (inventory, tooltip, shop - may have different color tags, quantities, etc.)
            string normalizedName = NormalizeCacheKey(originalName);
            string cacheKey = $"{blueprint}:{normalizedName}";
            if (_displayNameCache.TryGetValue(cacheKey, out translated))
            {
                // CRITICAL: Don't return empty strings as successful translations
                if (!string.IsNullOrEmpty(translated))
                {
                    return true;
                }
                // Remove invalid cache entry
                _displayNameCache.Remove(cacheKey);
                translated = null;
            }

            // STEP 1: 색상 태그 내 재료 번역 시도
            // "{{w|bronze}} mace" → "{{w|청동}} mace"
            string withTranslatedMaterials = TranslateMaterialsInColorTags(originalName);

            // Try creature cache first, then item cache
            // Use normalized blueprint for lookup to handle key variations
            string normalizedBlueprint = NormalizeBlueprintId(blueprint);
            ObjectData data = null;
            if (_creatureCache.TryGetValue(normalizedBlueprint, out data) ||
                _itemCache.TryGetValue(normalizedBlueprint, out data) ||
                _creatureCache.TryGetValue(blueprint, out data) ||
                _itemCache.TryGetValue(blueprint, out data))
            {
                // Try exact match with color-tagged version first
                if (data.Names.TryGetValue(originalName, out string exactMatch) && !string.IsNullOrEmpty(exactMatch))
                {
                    translated = exactMatch;
                    _displayNameCache[cacheKey] = translated;
                    return true;
                }

                // Try exact match with material-translated version
                if (withTranslatedMaterials != originalName &&
                    data.Names.TryGetValue(withTranslatedMaterials, out string materialMatch) &&
                    !string.IsNullOrEmpty(materialMatch))
                {
                    translated = materialMatch;
                    _displayNameCache[cacheKey] = translated;
                    return true;
                }

                // Try stripped version (no color tags)
                string strippedOriginal = StripColorTags(withTranslatedMaterials);
                if (data.Names.TryGetValue(strippedOriginal, out string koreanName) && !string.IsNullOrEmpty(koreanName))
                {
                    // Full match including suffix if present in the key
                    // In this case, strippedOriginal IS the core name found in DB
                    translated = RestoreFormatting(originalName, strippedOriginal, koreanName, "", "");
                    if (string.IsNullOrEmpty(translated)) return false;

                    // Also translate any remaining prefixes/nouns for consistency
                    translated = TranslateBaseNounsOutsideTags(translated);

                    _displayNameCache[cacheKey] = translated;
                    return true;
                }

                // PRIORITY: Check state suffix BEFORE partial matching
                // This ensures "waterskin [empty]" -> "물주머니 [비어있음]" not "물주머니 [empty]"
                string noStateSuffix = StripStateSuffix(strippedOriginal);
                if (noStateSuffix != strippedOriginal)
                {
                    string suffix = strippedOriginal.Substring(noStateSuffix.Length);

                    if (data.Names.TryGetValue(noStateSuffix, out string baseNameKo) && !string.IsNullOrEmpty(baseNameKo))
                    {
                        string suffixKo = TranslateStateSuffix(suffix);
                        translated = RestoreFormatting(originalName, noStateSuffix, baseNameKo, suffix, suffixKo);

                        if (!string.IsNullOrEmpty(translated))
                        {
                            // Also translate any remaining prefixes/nouns for consistency
                            translated = TranslateBaseNounsOutsideTags(translated);

                            _displayNameCache[cacheKey] = translated;
                            return true;
                        }
                    }
                }
                
                // Try any name in the names dictionary (partial match fallback)
                foreach (var kvp in data.Names)
                {
                    if (!string.IsNullOrEmpty(kvp.Value))
                    {
                        // Check if key exists in original or stripped version
                        bool inOriginal = originalName.Contains(kvp.Key);
                        bool inStripped = strippedOriginal.Contains(kvp.Key);

                        if (inOriginal || inStripped)
                        {
                            // FIX: If key is in stripped but not original (due to color tags),
                            // do replacement on stripped version instead
                            if (inOriginal)
                            {
                                translated = originalName.Replace(kvp.Key, kvp.Value);
                            }
                            else
                            {
                                // Key is in stripped version only (color tags interfered)
                                translated = strippedOriginal.Replace(kvp.Key, kvp.Value);
                            }

                            if (string.IsNullOrEmpty(translated)) continue;

                            // CRITICAL FIX: Also translate remaining prefixes and nouns
                            // "painted 정육점 칼" → "칠해진 정육점 칼"
                            translated = TranslateBaseNounsOutsideTags(translated);

                            _displayNameCache[cacheKey] = translated;
                            return true;
                        }
                    }
                }
            }
            
            // Try with state suffix stripped for items NOT in blueprint cache
            // Use material-translated version for better matching
            string globalStripped = StripColorTags(withTranslatedMaterials);
            string globalNoSuffix = StripStateSuffix(globalStripped);
            if (globalNoSuffix != globalStripped)
            {
                string globalSuffix = globalStripped.Substring(globalNoSuffix.Length);

                // Try finding translation for base name without state suffix in ALL caches
                foreach (var cache in new[] { _creatureCache, _itemCache })
                {
                    foreach (var kvp in cache)
                    {
                        foreach (var namePair in kvp.Value.Names)
                        {
                            if (namePair.Key.Equals(globalNoSuffix, StringComparison.OrdinalIgnoreCase))
                            {
                                string suffixKo = TranslateStateSuffix(globalSuffix);
                                // Use withTranslatedMaterials to preserve Korean materials in color tags
                                translated = RestoreFormatting(withTranslatedMaterials, globalNoSuffix, namePair.Value, globalSuffix, suffixKo);

                                // CRITICAL FIX: Also translate remaining prefixes and nouns
                                translated = TranslateBaseNounsOutsideTags(translated);

                                _displayNameCache[cacheKey] = translated;
                                return true;
                            }
                        }
                    }
                }
            }

            // === Prefix/Suffix System ===
            // Extract all suffixes first, then try to match prefixes
            // Use original English name for prefix matching (keys are English)
            string strippedForPrefix = StripColorTags(originalName);
            string baseNameForPrefix = ExtractAllSuffixes(strippedForPrefix, out string allSuffixes);

            // Try with prefixes
            if (TryExtractAndTranslatePrefixes(baseNameForPrefix, out string prefixKo, out string remainder))
            {
                // Try to find translation for the remainder (base item name)
                if (TryGetItemTranslation(remainder, out string baseKo) ||
                    TryGetCreatureTranslation(remainder, out baseKo))
                {
                    string suffixKo = TranslateAllSuffixes(allSuffixes);
                    // Korean word order: prefix + base + suffix
                    translated = $"{prefixKo} {baseKo}{suffixKo}";
                    _displayNameCache[cacheKey] = translated;
                    return true;
                }

                // BUG #1 FIX: remainder에서 추가 접두사(재료) 추출 시도
                // "engraved bronze mace" → prefixKo="새겨진", remainder="bronze mace"
                // → materialKo="청동", baseOnly="mace" → "새겨진 청동 메이스"
                if (TryExtractAndTranslatePrefixes(remainder, out string materialKo, out string baseOnly))
                {
                    if (TryGetItemTranslation(baseOnly, out string baseKo2) ||
                        TryGetCreatureTranslation(baseOnly, out baseKo2))
                    {
                        string suffixKo = TranslateAllSuffixes(allSuffixes);
                        translated = $"{prefixKo} {materialKo} {baseKo2}{suffixKo}";
                        _displayNameCache[cacheKey] = translated;
                        return true;
                    }
                }
            }

            // Try base item name lookup (handles both simple items and items with suffixes)
            // This covers: "torch" -> "횃불", "torch (unburnt)" -> "횃불 (미사용)"
            if (TryGetItemTranslation(baseNameForPrefix, out string baseKo3) ||
                TryGetCreatureTranslation(baseNameForPrefix, out baseKo3))
            {
                string suffixKo = TranslateAllSuffixes(allSuffixes);
                translated = string.IsNullOrEmpty(suffixKo) ? baseKo3 : $"{baseKo3}{suffixKo}";
                _displayNameCache[cacheKey] = translated;
                return true;
            }

            // Corpse pattern handling: "{creature} corpse" -> "{creature_ko} 시체"
            if (TryTranslateCorpse(originalName, out translated))
            {
                _displayNameCache[cacheKey] = translated;
                return true;
            }
            
            // Dynamic food patterns: jerky, meat, haunch
            if (TryTranslateDynamicFood(originalName, out translated))
            {
                _displayNameCache[cacheKey] = translated;
                return true;
            }

            // Dynamic parts patterns: egg, hide, bone, skull, horn, feather, scale (Phase 1.1)
            if (TryTranslateDynamicParts(originalName, out translated))
            {
                _displayNameCache[cacheKey] = translated;
                return true;
            }

            // "of X" pattern: "sandals of the river-wives" → "강 아내들의 샌들" (Step 5 어순 처리)
            if (TryTranslateOfPattern(originalName, out translated))
            {
                _displayNameCache[cacheKey] = translated;
                return true;
            }

            // Possessive pattern: "panther's claw" → "표범의 발톱" (Phase 소유격)
            if (TryTranslatePossessive(originalName, out translated))
            {
                _displayNameCache[cacheKey] = translated;
                return true;
            }

            // === Final Fallback: 색상 태그 내 재료 및 외부 명사 번역 (Phase 3.2) ===
            // If materials in color tags were translated, also try to translate base nouns outside
            // "{{w|bronze}} mace" → "{{w|청동}} 메이스"
            if (withTranslatedMaterials != originalName)
            {
                // BUG #2 FIX: "of X" 패턴 먼저 확인 (원본 영어 사용)
                // "{{C|sandals}} of the river-wives" → "강 아내들의 샌들"
                string strippedForOf = StripColorTags(originalName);
                if (strippedForOf.Contains(" of "))
                {
                    if (TryTranslateOfPattern(strippedForOf, out translated))
                    {
                        _displayNameCache[cacheKey] = translated;
                        return true;
                    }
                }

                // Also translate base nouns outside the color tags
                translated = TranslateBaseNounsOutsideTags(withTranslatedMaterials);
                _displayNameCache[cacheKey] = translated;
                return true;
            }

            // Last resort: Try translating just the base nouns even without color tags
            string withBaseNouns = TranslateBaseNounsOutsideTags(originalName);
            if (withBaseNouns != originalName)
            {
                translated = withBaseNouns;
                _displayNameCache[cacheKey] = translated;
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Strips state suffixes like [empty], [full], (lit), (unlit), x4, and stats like →8 ♥1d8.
        /// All patterns use $ anchor to only match at end of string to avoid corrupting names.
        /// </summary>
        private static string StripStateSuffix(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            string result = name;

            // Remove weapon/armor stats at end: →4 ♥1d2, ◆0 ○0, etc.
            // Must be done FIRST before other suffixes, and as a complete block
            // Pattern: optional stats like →4, then optional damage like ♥1d2 or ♥1d4+1
            result = Regex.Replace(result, @"\s+[→◆♦●○]\s*-?\d+(\s+[♥♠♣]\d+d\d+(\+\d+)?)?$", "");
            // Also handle standalone damage dice at end
            result = Regex.Replace(result, @"\s+[♥♠♣]\d+d\d+(\+\d+)?$", "");

            // Remove bracket suffixes at end: [empty], [full], [loaded], etc.
            result = Regex.Replace(result, @"\s*\[[^\]]+\]$", "");

            // Remove parenthesis suffixes at end: (lit), (unlit), (unburnt), etc.
            result = Regex.Replace(result, @"\s*\([^)]+\)$", "");

            // Remove count suffixes at end: x4, x10, etc.
            result = Regex.Replace(result, @"\s*x\d+$", "");

            return result.Trim();
        }
        
        /// <summary>
        /// Translates common state suffixes to Korean.
        /// Supports compound suffixes like " x15 (unburnt)" -> " x15 (미사용)"
        /// Uses loaded dictionaries from _suffixes.json
        /// </summary>
        private static string TranslateStateSuffix(string suffix)
        {
            if (string.IsNullOrEmpty(suffix)) return "";

            string result = suffix;

            // State translations from _suffixes.json
            foreach (var kvp in _statesLoaded)
            {
                if (result.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = Regex.Replace(result, Regex.Escape(kvp.Key), kvp.Value, RegexOptions.IgnoreCase);
                }
            }

            // [X drams of Y] pattern → [Y X드램] using loaded liquids
            result = Regex.Replace(result, @"\[(\d+) drams? of ([^\]]+)\]", m => {
                string amount = m.Groups[1].Value;
                string liquid = m.Groups[2].Value.Trim();
                string liquidKo = _liquidsLoaded.TryGetValue(liquid, out var ko) ? ko : liquid;
                return $"[{liquidKo} {amount}드램]";
            }, RegexOptions.IgnoreCase);

            // [X servings] pattern → [X인분]
            result = Regex.Replace(result, @"\[(\d+) servings?\]", "[$1인분]", RegexOptions.IgnoreCase);

            return result;
        }
        
        /// <summary>
        /// Attempts to translate dynamic food items using patterns:
        /// - "{creature} jerky" -> "{creature_ko} 육포"
        /// - "{creature} meat" -> "{creature_ko} 고기"
        /// - "{creature} haunch" -> "{creature_ko} 넓적다리"
        /// - "preserved {ingredient}" -> "절임 {ingredient_ko}"
        /// </summary>
        private static bool TryTranslateDynamicFood(string originalName, out string translated)
        {
            translated = null;
            string stripped = StripColorTags(originalName);
            
            // Pattern: "{creature} jerky"
            if (stripped.EndsWith(" jerky", StringComparison.OrdinalIgnoreCase))
            {
                string creaturePart = stripped.Substring(0, stripped.Length - " jerky".Length);
                if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                {
                    translated = $"{creatureKo} 육포";
                    return true;
                }
            }
            
            // Pattern: "{creature} meat"
            if (stripped.EndsWith(" meat", StringComparison.OrdinalIgnoreCase))
            {
                string creaturePart = stripped.Substring(0, stripped.Length - " meat".Length);
                if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                {
                    translated = $"{creatureKo} 고기";
                    return true;
                }
            }
            
            // Pattern: "{creature} haunch"
            if (stripped.EndsWith(" haunch", StringComparison.OrdinalIgnoreCase))
            {
                string creaturePart = stripped.Substring(0, stripped.Length - " haunch".Length);
                if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                {
                    translated = $"{creatureKo} 넓적다리";
                    return true;
                }
            }
            
            // Pattern: "preserved {creature/ingredient}"
            if (stripped.StartsWith("preserved ", StringComparison.OrdinalIgnoreCase))
            {
                string ingredientPart = stripped.Substring("preserved ".Length);
                if (TryGetCreatureTranslation(ingredientPart, out string ingredientKo))
                {
                    translated = $"절임 {ingredientKo}";
                    return true;
                }
                // Try item translation as well
                if (TryGetItemTranslation(ingredientPart, out ingredientKo))
                {
                    translated = $"절임 {ingredientKo}";
                    return true;
                }
            }
            
            // Pattern: "cooked {ingredient}"
            if (stripped.StartsWith("cooked ", StringComparison.OrdinalIgnoreCase))
            {
                string ingredientPart = stripped.Substring("cooked ".Length);
                if (TryGetItemTranslation(ingredientPart, out string ingredientKo))
                {
                    translated = $"조리된 {ingredientKo}";
                    return true;
                }
                if (TryGetCreatureTranslation(ingredientPart, out ingredientKo))
                {
                    translated = $"조리된 {ingredientKo}";
                    return true;
                }
            }

            // Pattern: "{creature} gland paste" -> "{creature_ko} 분비샘 반죽" (Phase 6.3)
            if (stripped.EndsWith(" gland paste", StringComparison.OrdinalIgnoreCase))
            {
                string creaturePart = stripped.Substring(0, stripped.Length - " gland paste".Length);
                // "elder" 접두사 처리
                if (creaturePart.StartsWith("elder ", StringComparison.OrdinalIgnoreCase))
                {
                    creaturePart = creaturePart.Substring("elder ".Length);
                    if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                    {
                        translated = $"장로 {creatureKo} 분비샘 반죽";
                        return true;
                    }
                }
                else if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                {
                    translated = $"{creatureKo} 분비샘 반죽";
                    return true;
                }
            }

            // Pattern: "{creature} gland" -> "{creature_ko} 분비샘"
            if (stripped.EndsWith(" gland", StringComparison.OrdinalIgnoreCase))
            {
                string creaturePart = stripped.Substring(0, stripped.Length - " gland".Length);
                // "elder" 접두사 처리
                if (creaturePart.StartsWith("elder ", StringComparison.OrdinalIgnoreCase))
                {
                    creaturePart = creaturePart.Substring("elder ".Length);
                    if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                    {
                        translated = $"장로 {creatureKo} 분비샘";
                        return true;
                    }
                }
                else if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                {
                    translated = $"{creatureKo} 분비샘";
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 생물 부위 패턴 번역 (Phase 1.1)
        /// - "{creature} egg" -> "{creature_ko} 알"
        /// - "{creature} hide" -> "{creature_ko} 가죽"
        /// - "{creature} bone" -> "{creature_ko} 뼈"
        /// - "raw {creature} {part}" -> "생 {creature_ko} {part_ko}"
        /// </summary>
        private static bool TryTranslateDynamicParts(string originalName, out string translated)
        {
            translated = null;
            string stripped = StripColorTags(originalName);

            // 부위 패턴 목록 - _suffixes.json의 part_suffixes에서 로드
            var partPatterns = GetPartSuffixesSorted();
            if (partPatterns == null || partPatterns.Count == 0)
            {
                return false;
            }

            // "raw {creature} {part}" 패턴 처리
            if (stripped.StartsWith("raw ", StringComparison.OrdinalIgnoreCase))
            {
                string remainder = stripped.Substring("raw ".Length);
                foreach (var kvp in partPatterns)
                {
                    if (remainder.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        string creaturePart = remainder.Substring(0, remainder.Length - kvp.Key.Length);
                        if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                        {
                            translated = $"생 {creatureKo}{kvp.Value}";
                            return true;
                        }
                    }
                }
            }

            // 일반 "{creature} {part}" 패턴 처리
            foreach (var kvp in partPatterns)
            {
                if (stripped.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    string creaturePart = stripped.Substring(0, stripped.Length - kvp.Key.Length);
                    if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                    {
                        translated = $"{creatureKo}{kvp.Value}";
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 소유격 패턴 번역 (Phase 소유격)
        /// - "{creature}'s {part}" → "{creature_ko}의 {part_ko}"
        /// - "panther's claw" → "표범의 발톱"
        /// </summary>
        private static bool TryTranslatePossessive(string originalName, out string translated)
        {
            translated = null;
            string stripped = StripColorTags(originalName);

            // Pattern: "{creature}'s {part}"
            var match = Regex.Match(stripped, @"^(.+)'s\s+(.+)$", RegexOptions.IgnoreCase);
            if (!match.Success) return false;

            string creature = match.Groups[1].Value.Trim();
            string part = match.Groups[2].Value.Trim();

            // Try to translate creature
            if (!TryGetCreatureTranslation(creature, out string creatureKo))
            {
                return false;
            }

            // Try to translate part (using item or part patterns)
            string partKo = null;

            // Check base noun translations first (from items/_nouns.json)
            if (_baseNounsLoaded.TryGetValue(part, out partKo))
            {
                translated = $"{creatureKo}의 {partKo}";
                return true;
            }

            // Check item cache
            if (TryGetItemTranslation(part, out partKo))
            {
                translated = $"{creatureKo}의 {partKo}";
                return true;
            }

            // Try body parts from JSON (_suffixes.json)
            if (_bodyPartsLoaded.TryGetValue(part, out partKo))
            {
                translated = $"{creatureKo}의 {partKo}";
                return true;
            }

            return false;
        }

        /// <summary>
        /// "X of Y" 패턴을 한국어 어순 "Y의 X"로 변환 (Step 5)
        /// "sandals of the river-wives" → "강 아내들의 샌들"
        /// "banner of the Holy Rhombus" → "성스러운 마름모의 깃발"
        /// </summary>
        private static bool TryTranslateOfPattern(string originalName, out string translated)
        {
            translated = null;
            string stripped = StripColorTags(originalName);

            // "X of Y" 패턴 매칭 - "of the Y" 또는 "of Y" 형태
            var match = Regex.Match(stripped, @"^(.+?)\s+of\s+(?:the\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return false;

            string itemPart = match.Groups[1].Value.Trim();  // "sandals"
            string ofPart = match.Groups[2].Value.Trim();    // "river-wives" or "Holy Rhombus"

            // 1. of_patterns 사전에서 전체 "of X" 매칭 시도
            string ofKo = null;
            string fullOfPatternWithThe = $"of the {ofPart}";
            string fullOfPattern = $"of {ofPart}";

            if (_ofPatternsLoaded.TryGetValue(fullOfPatternWithThe, out ofKo))
            {
                // ofKo = "강 아내들의" (이미 "의" 포함)
            }
            else if (_ofPatternsLoaded.TryGetValue(fullOfPattern, out ofKo))
            {
                // ofKo = "~의" 형태
            }
            else if (_ofPatternsLoaded.TryGetValue(ofPart, out ofKo))
            {
                // 기존 of_patterns (원소만 있는 경우)
                ofKo = $"{ofKo}의";
            }
            else
            {
                // 사전에 없으면 기본 번역 시도
                string ofPartTranslated = TranslateNounsInText(ofPart);
                ofPartTranslated = TranslatePrefixesInText(ofPartTranslated);

                // 번역이 안 됐으면 실패
                if (ofPartTranslated == ofPart)
                    return false;

                ofKo = $"{ofPartTranslated}의";
            }

            // 2. 아이템 부분 번역
            string itemKo = itemPart;
            if (TryGetItemTranslation(itemPart, out string itemTranslated))
            {
                itemKo = itemTranslated;
            }
            else
            {
                string nounTranslated = TranslateNounsInText(itemPart);
                nounTranslated = TranslatePrefixesInText(nounTranslated);

                // 아이템 부분도 번역 안 됐으면 부분 번역이라도 시도
                if (nounTranslated != itemPart)
                {
                    itemKo = nounTranslated;
                }
            }

            // 3. 한국어 어순으로 조합: "Y의 X"
            translated = $"{ofKo} {itemKo}".Trim();

            // 원본 색상 태그 복원은 하지 않음 (태그 제거 후 번역)
            return true;
        }

        /// <summary>
        /// Tries to find a creature translation from cache or common species list
        /// </summary>
        private static bool TryGetCreatureTranslation(string creatureName, out string translated)
        {
            translated = null;
            
            // Try creature cache first
            foreach (var kvp in _creatureCache)
            {
                foreach (var namePair in kvp.Value.Names)
                {
                    if (namePair.Key.Equals(creatureName, StringComparison.OrdinalIgnoreCase))
                    {
                        translated = namePair.Value;
                        return true;
                    }
                }
            }
            
            // Fallback: species from creatures/_common.json
            if (_speciesLoaded.TryGetValue(creatureName, out translated))
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Tries to find an item translation from cache or base noun dictionary
        /// </summary>
        private static bool TryGetItemTranslation(string itemName, out string translated)
        {
            translated = null;
            // Debug: Log item translation attempt
            // UnityEngine.Debug.Log($"{LOG_PREFIX} TryGetItemTranslation: looking for '{itemName}'");

            // First try JSON cache
            foreach (var kvp in _itemCache)
            {
                foreach (var namePair in kvp.Value.Names)
                {
                    if (namePair.Key.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                    {
                        translated = namePair.Value;
                        // Debug: Log success
                        // UnityEngine.Debug.Log($"{LOG_PREFIX} TryGetItemTranslation: FOUND in itemCache '{itemName}' -> '{translated}'");
                        return true;
                    }
                }
            }

            // Fallback: check base noun translations from items/_nouns.json
            if (_baseNounsLoaded.TryGetValue(itemName, out translated))
            {
                // Debug: Log success
                // UnityEngine.Debug.Log($"{LOG_PREFIX} TryGetItemTranslation: FOUND in baseNouns '{itemName}' -> '{translated}'");
                return true;
            }

            // Debug: Log failure
            // UnityEngine.Debug.Log($"{LOG_PREFIX} TryGetItemTranslation: NOT FOUND '{itemName}', baseNounsLoaded.Count={_baseNounsLoaded.Count}");
            return false;
        }

        // NOTE: _commonSpeciesTranslations removed - now loaded from creatures/_common.json
        // See: LOCALIZATION/OBJECTS/creatures/_common.json "species" section

        /// <summary>
        /// Attempts to translate corpse names using pattern: "{creature} corpse" -> "{creature_ko} 시체"
        /// </summary>
        private static bool TryTranslateCorpse(string originalName, out string translated)
        {
            translated = null;
            string stripped = StripColorTags(originalName);
            
            // Check if it ends with "corpse"
            if (!stripped.EndsWith(" corpse", StringComparison.OrdinalIgnoreCase))
                return false;
            
            // Extract creature part
            string creaturePart = stripped.Substring(0, stripped.Length - " corpse".Length);
            if (string.IsNullOrEmpty(creaturePart))
                return false;
            
            // Try to find creature translation using shared method
            if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
            {
                translated = $"{creatureKo} 시체";
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Attempts to get a translated description for a blueprint.
        /// </summary>
        public static bool TryGetDescription(string blueprint, out string translated)
        {
            translated = null;
            if (string.IsNullOrEmpty(blueprint)) return false;
            
            EnsureInitialized();
            
            // Normalize blueprint ID for consistent lookup (fixes "Torch" vs "torch" mismatch)
            string normalizedBlueprint = NormalizeBlueprintId(blueprint);
            ObjectData data = null;
            if (_creatureCache.TryGetValue(normalizedBlueprint, out data) || 
                _itemCache.TryGetValue(normalizedBlueprint, out data) ||
                _creatureCache.TryGetValue(blueprint, out data) || 
                _itemCache.TryGetValue(blueprint, out data))
            {
                if (!string.IsNullOrEmpty(data.DescriptionKo))
                {
                    translated = data.DescriptionKo;
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Attempts to translate a description ONLY if the current text matches the known English description.
        /// Use this to safely replace text in Tooltips without context.
        /// </summary>
        public static bool TryTranslateDescriptionExact(string blueprint, string currentText, out string translated)
        {
            translated = null;
            if (string.IsNullOrEmpty(blueprint) || string.IsNullOrEmpty(currentText)) return false;

            EnsureInitialized();
            
            string normalizedBlueprint = NormalizeBlueprintId(blueprint);
            ObjectData data = null;
            if (_creatureCache.TryGetValue(normalizedBlueprint, out data) || 
                _itemCache.TryGetValue(normalizedBlueprint, out data) ||
                _creatureCache.TryGetValue(blueprint, out data) || 
                _itemCache.TryGetValue(blueprint, out data))
            {
                // strict or trimmed match of English description
                if (!string.IsNullOrEmpty(data.Description) && !string.IsNullOrEmpty(data.DescriptionKo))
                {
                    if (currentText.Trim().Equals(data.Description.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        translated = data.DescriptionKo;
                        return true;
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks if a blueprint has any translation data.
        /// </summary>
        public static bool HasTranslation(string blueprint)
        {
            if (string.IsNullOrEmpty(blueprint)) return false;
            EnsureInitialized();
            return _creatureCache.ContainsKey(blueprint) || _itemCache.ContainsKey(blueprint);
        }
        
        /// <summary>
        /// Gets statistics about loaded translations.
        /// </summary>
        public static string GetStats()
        {
            EnsureInitialized();
            int vocabCount = _allPrefixesSortedLoaded?.Count ?? 0;
            int speciesCount = _speciesLoaded?.Count ?? 0;
            int nounsCount = _baseNounsLoaded?.Count ?? 0;
            int suffixesCount = (_statesLoaded?.Count ?? 0) + (_liquidsLoaded?.Count ?? 0) +
                               (_ofPatternsLoaded?.Count ?? 0) + (_bodyPartsLoaded?.Count ?? 0) +
                               (_partSuffixesLoaded?.Count ?? 0);
            return $"Creatures: {_creatureCache.Count}, Items: {_itemCache.Count}, Vocab: {vocabCount}, Species: {speciesCount}, Nouns: {nounsCount}, Suffixes: {suffixesCount}, Cached: {_displayNameCache.Count}";
        }
        
        #endregion
        
        #region Private Methods
        
        private static string GetModDirectory()
        {
            if (_modDirectory != null) return _modDirectory;
            
            try
            {
                // Use ModManager to get our mod's path - this is the official way
                var modInfo = XRL.ModManager.GetMod("KoreanLocalization");
                if (modInfo != null && !string.IsNullOrEmpty(modInfo.Path))
                {
                    string objectsPath = Path.Combine(modInfo.Path, "LOCALIZATION", "OBJECTS");
                    if (Directory.Exists(objectsPath))
                    {
                        _modDirectory = modInfo.Path;
                        UnityEngine.Debug.Log($"{LOG_PREFIX} Found mod directory via ModManager: {_modDirectory}");
                        return _modDirectory;
                    }
                }
                
                // Fallback: try common mod locations (macOS and Windows)
                string[] possiblePaths = new[]
                {
                    // macOS - Unity standalone
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Library", "Application Support", "com.FreeholdGames.CavesOfQud", "Mods", "KoreanLocalization"),
                    // Windows - Steam
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "..", "LocalLow", "Freehold Games", "CavesOfQud", "Mods", "KoreanLocalization"),
                    // Windows - GOG
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "AppData", "LocalLow", "Freehold Games", "CavesOfQud", "Mods", "KoreanLocalization")
                };
                
                foreach (var path in possiblePaths)
                {
                    if (Directory.Exists(Path.Combine(path, "LOCALIZATION", "OBJECTS")))
                    {
                        _modDirectory = path;
                        UnityEngine.Debug.Log($"{LOG_PREFIX} Found mod directory via fallback: {_modDirectory}");
                        return _modDirectory;
                    }
                }
                
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Could not find mod directory in any known location");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} GetModDirectory failed: {ex.Message}");
            }
            
            return null;
        }
        
        private static void LoadAllJsonFiles()
        {
            string modDir = GetModDirectory();
            if (string.IsNullOrEmpty(modDir))
            {
                UnityEngine.Debug.LogWarning($"{LOG_PREFIX} Could not find mod directory");
                return;
            }
            
            string objectsPath = Path.Combine(modDir, "LOCALIZATION", "OBJECTS");
            
            // Load creatures
            string creaturesPath = Path.Combine(objectsPath, "creatures");
            if (Directory.Exists(creaturesPath))
            {
                foreach (var file in Directory.GetFiles(creaturesPath, "*.json", SearchOption.AllDirectories))
                {
                    LoadJsonFile(file, _creatureCache);
                }
            }
            
            // Load items
            string itemsPath = Path.Combine(objectsPath, "items");
            if (Directory.Exists(itemsPath))
            {
                foreach (var file in Directory.GetFiles(itemsPath, "*.json", SearchOption.AllDirectories))
                {
                    // Skip _common.json - it has a different structure and is loaded separately
                    if (Path.GetFileName(file).StartsWith("_"))
                        continue;
                    LoadJsonFile(file, _itemCache);
                }
            }

            // Load common vocabulary (materials, qualities, etc.)
            LoadItemCommon();

            // Load species from creatures/_common.json
            LoadCreatureCommon();

            // Load base nouns from items/_nouns.json
            LoadItemNouns();

            // Load suffixes from _suffixes.json
            LoadSuffixes();
        }
        
        private static void LoadJsonFile(string filePath, Dictionary<string, ObjectData> cache)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                JObject root = JObject.Parse(json);
                
                foreach (var prop in root.Properties())
                {
                    // Skip metadata
                    if (prop.Name.StartsWith("_")) continue;
                    
                    string blueprintId = prop.Name;
                    string normalizedId = NormalizeBlueprintId(blueprintId);
                    JObject entry = prop.Value as JObject;
                    if (entry == null) continue;
                    
                    var data = new ObjectData
                    {
                        BlueprintId = blueprintId,  // Keep original for reference
                        Description = entry["description"]?.ToString(),
                        DescriptionKo = entry["description_ko"]?.ToString()
                    };
                    
                    // Parse names dictionary
                    JObject names = entry["names"] as JObject;
                    if (names != null)
                    {
                        foreach (var nameProp in names.Properties())
                        {
                            data.Names[nameProp.Name] = nameProp.Value.ToString();
                        }
                    }
                    
                    // Store with normalized key for consistent lookup
                    cache[normalizedId] = data;
                    // Also store with original key for direct matches
                    if (normalizedId != blueprintId.ToLowerInvariant())
                    {
                        cache[blueprintId] = data;
                    }
                }
                
                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load {filePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// items/_common.json에서 공통 어휘를 로드합니다.
        /// </summary>
        private static void LoadItemCommon()
        {
            string modDir = GetModDirectory();
            if (string.IsNullOrEmpty(modDir)) return;

            string path = Path.Combine(modDir, "LOCALIZATION", "OBJECTS", "items", "_common.json");
            if (!File.Exists(path))
            {
                UnityEngine.Debug.Log($"{LOG_PREFIX} items/_common.json not found, using hardcoded dictionaries");
                return;
            }

            try
            {
                var root = JObject.Parse(File.ReadAllText(path));

                LoadSection(root["materials"] as JObject, _materialsLoaded);
                LoadSection(root["qualities"] as JObject, _qualitiesLoaded);
                LoadSection(root["modifiers"] as JObject, _modifiersLoaded);
                LoadSection(root["processing"] as JObject, _processingLoaded);
                LoadSection(root["tonics"] as JObject, _tonicsLoaded);
                LoadSection(root["grenades"] as JObject, _grenadesLoaded);
                LoadSection(root["marks"] as JObject, _marksLoaded);
                LoadSection(root["colors"] as JObject, _colorsLoaded);  // BUG #2 수정: 색상 형용사 로드

                // 통합 접두사 사전 구축 (모든 카테고리)
                var allPrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                MergeInto(allPrefixes, _materialsLoaded);
                MergeInto(allPrefixes, _qualitiesLoaded);
                MergeInto(allPrefixes, _modifiersLoaded);
                MergeInto(allPrefixes, _processingLoaded);
                MergeInto(allPrefixes, _tonicsLoaded);
                MergeInto(allPrefixes, _grenadesLoaded);
                MergeInto(allPrefixes, _marksLoaded);
                MergeInto(allPrefixes, _colorsLoaded);  // BUG #2: colors도 접두사로 사용 가능

                _allPrefixesSortedLoaded = allPrefixes.ToList();
                _allPrefixesSortedLoaded.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));

                // 컬러 태그 내부용 (재료 + 품질 + 토닉 + 수류탄 + 색상)
                var colorTagVocab = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                MergeInto(colorTagVocab, _materialsLoaded);
                MergeInto(colorTagVocab, _qualitiesLoaded);
                MergeInto(colorTagVocab, _tonicsLoaded);
                MergeInto(colorTagVocab, _grenadesLoaded);
                MergeInto(colorTagVocab, _modifiersLoaded); // modifiers에도 컬러 태그에서 쓰이는 것들 있음
                MergeInto(colorTagVocab, _colorsLoaded);    // BUG #2: 색상 형용사 추가 (violet tube 등)

                _colorTagVocabSortedLoaded = colorTagVocab.ToList();
                _colorTagVocabSortedLoaded.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));

                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded items/_common.json: {allPrefixes.Count} prefixes, {colorTagVocab.Count} color tag vocab, {_colorsLoaded.Count} colors");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load items/_common.json: {ex.Message}");
            }
        }

        private static void LoadSection(JObject section, Dictionary<string, string> target)
        {
            if (section == null) return;
            foreach (var prop in section.Properties())
            {
                if (prop.Name.StartsWith("_")) continue; // Skip comments
                target[prop.Name] = prop.Value.ToString();
            }
        }

        private static void MergeInto(Dictionary<string, string> target, Dictionary<string, string> source)
        {
            foreach (var kvp in source)
            {
                if (!target.ContainsKey(kvp.Key))
                {
                    target[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// creatures/_common.json에서 species를 로드합니다.
        /// </summary>
        private static void LoadCreatureCommon()
        {
            string modDir = GetModDirectory();
            if (string.IsNullOrEmpty(modDir)) return;

            string path = Path.Combine(modDir, "LOCALIZATION", "OBJECTS", "creatures", "_common.json");
            if (!File.Exists(path))
            {
                UnityEngine.Debug.Log($"{LOG_PREFIX} creatures/_common.json not found, species translation disabled");
                return;
            }

            try
            {
                var root = JObject.Parse(File.ReadAllText(path));
                LoadSection(root["species"] as JObject, _speciesLoaded);

                // Step 4: species를 color tag vocab 및 allPrefixes에 병합
                // _colorTagVocabSortedLoaded와 _allPrefixesSortedLoaded는 LoadItemCommon()에서 이미 생성됨
                if (_speciesLoaded.Count > 0)
                {
                    // colorTagVocab에 병합 (Issachari 등 파벌명 번역)
                    if (_colorTagVocabSortedLoaded != null)
                    {
                        var colorTagVocab = _colorTagVocabSortedLoaded.ToDictionary(
                            kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
                        MergeInto(colorTagVocab, _speciesLoaded);
                        _colorTagVocabSortedLoaded = colorTagVocab.ToList();
                        _colorTagVocabSortedLoaded.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
                    }

                    // allPrefixes에도 병합 (ape fur cloak → 유인원 모피 망토)
                    if (_allPrefixesSortedLoaded != null)
                    {
                        var allPrefixes = _allPrefixesSortedLoaded.ToDictionary(
                            kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
                        MergeInto(allPrefixes, _speciesLoaded);
                        _allPrefixesSortedLoaded = allPrefixes.ToList();
                        _allPrefixesSortedLoaded.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
                    }

                    UnityEngine.Debug.Log($"{LOG_PREFIX} Merged {_speciesLoaded.Count} species into vocabularies");
                }

                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded creatures/_common.json: {_speciesLoaded.Count} species");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load creatures/_common.json: {ex.Message}");
            }
        }

        /// <summary>
        /// items/_nouns.json에서 기본 명사를 로드합니다.
        /// </summary>
        private static void LoadItemNouns()
        {
            string modDir = GetModDirectory();
            if (string.IsNullOrEmpty(modDir)) return;

            string path = Path.Combine(modDir, "LOCALIZATION", "OBJECTS", "items", "_nouns.json");
            if (!File.Exists(path))
            {
                UnityEngine.Debug.Log($"{LOG_PREFIX} items/_nouns.json not found, base noun translation disabled");
                return;
            }

            try
            {
                var root = JObject.Parse(File.ReadAllText(path));

                // 모든 카테고리에서 명사 로드
                foreach (var prop in root.Properties())
                {
                    if (prop.Name.StartsWith("_")) continue;
                    LoadSection(prop.Value as JObject, _baseNounsLoaded);
                }

                // 정렬된 캐시 생성 (긴 것 우선)
                _baseNounsSortedLoaded = _baseNounsLoaded.ToList();
                _baseNounsSortedLoaded.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));

                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded items/_nouns.json: {_baseNounsLoaded.Count} nouns");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load items/_nouns.json: {ex.Message}");
            }
        }

        /// <summary>
        /// _suffixes.json에서 접미사 사전들을 로드합니다.
        /// </summary>
        private static void LoadSuffixes()
        {
            string modDir = GetModDirectory();
            if (string.IsNullOrEmpty(modDir)) return;

            string path = Path.Combine(modDir, "LOCALIZATION", "OBJECTS", "_suffixes.json");
            if (!File.Exists(path))
            {
                UnityEngine.Debug.Log($"{LOG_PREFIX} _suffixes.json not found, suffix translation disabled");
                return;
            }

            try
            {
                var root = JObject.Parse(File.ReadAllText(path));

                LoadSection(root["states"] as JObject, _statesLoaded);
                LoadSection(root["liquids"] as JObject, _liquidsLoaded);
                LoadSection(root["of_patterns"] as JObject, _ofPatternsLoaded);
                LoadSection(root["body_parts"] as JObject, _bodyPartsLoaded);

                // part_suffixes 로드 및 정렬 (긴 것 우선)
                var partSuffixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                LoadSection(root["part_suffixes"] as JObject, partSuffixes);
                _partSuffixesLoaded = partSuffixes.ToList();
                _partSuffixesLoaded.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));

                int totalLoaded = _statesLoaded.Count + _liquidsLoaded.Count + _ofPatternsLoaded.Count +
                                  _bodyPartsLoaded.Count + _partSuffixesLoaded.Count;
                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded _suffixes.json: {totalLoaded} entries");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load _suffixes.json: {ex.Message}");
            }
        }

        /// <summary>
        /// part_suffixes의 정렬된 목록을 반환합니다.
        /// </summary>
        private static List<KeyValuePair<string, string>> GetPartSuffixesSorted()
        {
            return _partSuffixesLoaded;
        }

        #endregion

        #region Tag Handling (Own implementation - not shared)

        #region Material Translation in Color Tags

        // NOTE: _colorTagMaterialTranslations removed - now loaded from items/_common.json
        // See: LOCALIZATION/OBJECTS/items/_common.json

        // Cached sorted list for longest-first matching
        private static List<KeyValuePair<string, string>> _colorTagMaterialsSorted = null;

        private static List<KeyValuePair<string, string>> GetColorTagMaterialsSorted()
        {
            // Use JSON-loaded data
            if (_colorTagVocabSortedLoaded != null && _colorTagVocabSortedLoaded.Count > 0)
            {
                return _colorTagVocabSortedLoaded;
            }

            // Fallback: return empty list with warning
            if (_colorTagMaterialsSorted == null)
            {
                UnityEngine.Debug.LogWarning($"{LOG_PREFIX} GetColorTagMaterialsSorted: items/_common.json not loaded, color tag translation disabled");
                _colorTagMaterialsSorted = new List<KeyValuePair<string, string>>();
            }
            return _colorTagMaterialsSorted;
        }

        /// <summary>
        /// 색상 태그 내의 재료와 명사를 번역합니다. (Phase 3.1 개선 + BUG #1 수정)
        /// Handles: {{color|material}}, {{shader|text}}, nested tags, and multiple sequential tags.
        /// Also translates nouns inside tags: {{c|basic toolkit}} → {{c|기본 공구함}}
        /// "{{G|hulk}} {{w|honey}} injector" → "{{G|헐크}} {{w|꿀}} injector"
        /// </summary>
        private static string TranslateMaterialsInColorTags(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("{{"))
                return text;

            string result = text;

            // 먼저 중첩 태그 내부부터 처리 (innermost first)
            // {{K|{{crysteel|crysteel}}}} → {{K|{{crysteel|크리스틸}}}}
            int maxIterations = 5;
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                string previous = result;

                // Step 1: 재료/수식어 번역
                foreach (var mat in GetColorTagMaterialsSorted())
                {
                    // Pattern 1: 태그 내용 시작 부분에서 매칭: {{X|material}} or {{X|material word...}}
                    // 이 패턴은 태그 내용이 material로 시작하는 경우를 처리
                    string pattern1 = $@"(\{{\{{[^|{{}}]+\|)({Regex.Escape(mat.Key)})(\s|\}}\}})";
                    result = Regex.Replace(result, pattern1, m =>
                        m.Groups[1].Value + mat.Value + m.Groups[3].Value,
                        RegexOptions.IgnoreCase);

                    // Pattern 2: 태그 내용 중간에서 공백 뒤에 매칭: {{X|something material}}
                    string pattern2 = $@"(\{{\{{[^|{{}}]+\|[^{{}}]*\s)({Regex.Escape(mat.Key)})(\s|\}}\}})";
                    result = Regex.Replace(result, pattern2, m =>
                        m.Groups[1].Value + mat.Value + m.Groups[3].Value,
                        RegexOptions.IgnoreCase);
                }

                // Step 2: 명사 번역 (BUG #1 수정 - 태그 내부 명사도 번역)
                // {{c|basic toolkit}} → {{c|기본 toolkit}} → {{c|기본 공구함}}
                foreach (var noun in GetBaseNounsSorted())
                {
                    // Pattern: {{X|something noun}} - 태그 내용 끝에서 명사 매칭
                    string pattern1 = $@"(\{{\{{[^|{{}}]+\|[^{{}}]*\s)({Regex.Escape(noun.Key)})(\}}\}})";
                    result = Regex.Replace(result, pattern1, m =>
                        m.Groups[1].Value + noun.Value + m.Groups[3].Value,
                        RegexOptions.IgnoreCase);

                    // Pattern: {{X|noun}} - 명사만 있는 경우 (태그 시작 부분)
                    string pattern2 = $@"(\{{\{{[^|{{}}]+\|)({Regex.Escape(noun.Key)})(\}}\}})";
                    result = Regex.Replace(result, pattern2, m =>
                        m.Groups[1].Value + noun.Value + m.Groups[3].Value,
                        RegexOptions.IgnoreCase);

                    // Pattern: {{X|noun something}} - 명사가 시작 부분에 있는 경우
                    string pattern3 = $@"(\{{\{{[^|{{}}]+\|)({Regex.Escape(noun.Key)})(\s[^{{}}]*\}}\}})";
                    result = Regex.Replace(result, pattern3, m =>
                        m.Groups[1].Value + noun.Value + m.Groups[3].Value,
                        RegexOptions.IgnoreCase);
                }

                // 더 이상 변화가 없으면 중단
                if (result == previous)
                    break;
            }

            // Pattern 3: 태그 외부에서 태그 앞에 있는 prefix 처리 (Phase 3.2 부분)
            // e.g., "two-handed {{B|carbide}} hammer" → "양손 {{B|카바이드}} hammer"
            foreach (var mat in GetColorTagMaterialsSorted())
            {
                // 태그 앞의 단어 매칭
                string pattern3 = $@"(^|\s)({Regex.Escape(mat.Key)})(\s+\{{\{{\s*)";
                result = Regex.Replace(result, pattern3, m =>
                    m.Groups[1].Value + mat.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);

                // 태그 뒤의 단어 매칭
                string pattern4 = $@"(\}}\}}\s+)({Regex.Escape(mat.Key)})(\s|$)";
                result = Regex.Replace(result, pattern4, m =>
                    m.Groups[1].Value + mat.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }

            return result;
        }

        // NOTE: _baseNounTranslations removed - now loaded from items/_nouns.json
        // See: LOCALIZATION/OBJECTS/items/_nouns.json

        // Cached sorted list for longest-first matching
        private static List<KeyValuePair<string, string>> _baseNounsSorted = null;

        private static List<KeyValuePair<string, string>> GetBaseNounsSorted()
        {
            // Use JSON-loaded data
            if (_baseNounsSortedLoaded != null && _baseNounsSortedLoaded.Count > 0)
            {
                return _baseNounsSortedLoaded;
            }

            // Fallback: return empty list with warning
            if (_baseNounsSorted == null)
            {
                UnityEngine.Debug.LogWarning($"{LOG_PREFIX} GetBaseNounsSorted: items/_nouns.json not loaded, base noun translation disabled");
                _baseNounsSorted = new List<KeyValuePair<string, string>>();
            }
            return _baseNounsSorted;
        }

        /// <summary>
        /// 색상 태그 외부의 기본 명사와 접두사를 번역합니다. (Phase 3.2 - 버그 수정, Phase 미번역 버그 수정)
        /// "{{w|bronze}} mace" → "{{w|bronze}} 메이스"
        /// "leather moccasins" → "가죽 모카신"
        /// 세그먼트 분할 방식으로 태그 내부/외부를 정확히 구분합니다.
        /// </summary>
        private static string TranslateBaseNounsOutsideTags(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // 태그가 없으면 직접 교체 - 명사 먼저, 그 다음 접두사
            if (!text.Contains("{{"))
            {
                string withNouns = TranslateNounsInText(text);
                return TranslatePrefixesInText(withNouns);
            }

            // 태그가 있으면: 세그먼트로 분할 후 외부만 처리
            var result = new System.Text.StringBuilder();
            int lastEnd = 0;

            // 중첩 태그도 처리할 수 있도록 균형 잡힌 매칭 사용
            int i = 0;
            while (i < text.Length)
            {
                if (i + 1 < text.Length && text[i] == '{' && text[i + 1] == '{')
                {
                    // 태그 시작 발견 - 이전 텍스트 처리 (명사 + 접두사)
                    if (i > lastEnd)
                    {
                        string segment = text.Substring(lastEnd, i - lastEnd);
                        string withNouns = TranslateNounsInText(segment);
                        result.Append(TranslatePrefixesInText(withNouns));
                    }

                    // 태그 끝 찾기 (중첩 고려)
                    int depth = 1;
                    int tagStart = i;
                    i += 2;
                    while (i + 1 < text.Length && depth > 0)
                    {
                        if (text[i] == '{' && text[i + 1] == '{') { depth++; i += 2; }
                        else if (text[i] == '}' && text[i + 1] == '}') { depth--; i += 2; }
                        else i++;
                    }

                    // 태그 그대로 추가 (내부는 건드리지 않음)
                    result.Append(text.Substring(tagStart, i - tagStart));
                    lastEnd = i;
                }
                else
                {
                    i++;
                }
            }

            // 남은 텍스트 처리 (명사 + 접두사)
            if (lastEnd < text.Length)
            {
                string segment = text.Substring(lastEnd);
                string withNouns = TranslateNounsInText(segment);
                result.Append(TranslatePrefixesInText(withNouns));
            }

            return result.ToString();
        }

        /// <summary>
        /// 텍스트 내 명사를 번역합니다 (태그 없는 텍스트용).
        /// </summary>
        private static string TranslateNounsInText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string result = text;
            foreach (var noun in GetBaseNounsSorted())
            {
                // 단어 경계에서만 매칭 (간단하고 안전한 패턴)
                string pattern = $@"(^|\s)({Regex.Escape(noun.Key)})($|\s|[,.\[\]()])";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + noun.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }
            return result;
        }

        /// <summary>
        /// 텍스트 내 접두사를 번역합니다 (Phase 미번역 버그 수정).
        /// Final fallback에서 명사 번역 후 접두사도 번역합니다.
        /// "leather 모카신" → "가죽 모카신"
        /// </summary>
        private static string TranslatePrefixesInText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string result = text;
            foreach (var prefix in GetAllPrefixesSorted())
            {
                // 단어 경계에서만 매칭 - 접두사는 다음에 공백이 와야 함
                string pattern = $@"(^|\s)({Regex.Escape(prefix.Key)})(\s)";
                result = Regex.Replace(result, pattern, m =>
                    m.Groups[1].Value + prefix.Value + m.Groups[3].Value,
                    RegexOptions.IgnoreCase);
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Strips Qud color tags from text. Own implementation, not using TranslationEngine.
        /// Updated to handle nested braces correctly using iterative innermost-first processing.
        /// "{{K|{{crysteel|crysteel}} mace}}" → "crysteel mace"
        /// </summary>
        private static string StripColorTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;

            // Remove simple color codes like &r, &W, &^r, &^W
            result = Regex.Replace(result, @"&[\^]?[a-zA-Z]", "");

            // Remove {{...}} tags iteratively (innermost first)
            // Process from innermost tags outward to handle nesting like {{K|{{crysteel|crysteel}} mace}}
            int limit = 10;
            while (limit-- > 0 && result.Contains("{{"))
            {
                // Match innermost tags: {{tag|content}} where content has no {{ or }}
                // This ensures we process nested tags from inside out
                string next = Regex.Replace(result, @"\{\{([^{}|]+)\|([^{}]*)\}\}", "$2");
                if (next == result) break; // No more replacements made
                result = next;
            }

            return result.Trim();
        }
        
        /// <summary>
        /// Restores color tags from original to translated text using granular replacement.
        /// Handles cases like "{{r|Torch}} (lit)" where "Torch (lit)" is not a contiguous string in original.
        /// </summary>
        private static string RestoreFormatting(string original, string coreName, string translatedCore, string suffix, string translatedSuffix)
        {
            if (string.IsNullOrEmpty(original)) return translatedCore + translatedSuffix;

            string result = original;

            // 1. Replace the core name (e.g. "Torch" -> "횃불")
            if (!string.IsNullOrEmpty(coreName) && !string.IsNullOrEmpty(translatedCore))
            {
                // Case-insensitive replace for robustness
                result = Regex.Replace(result, Regex.Escape(coreName), translatedCore, RegexOptions.IgnoreCase);
            }

            // 2. Replace the suffix (e.g. " (lit)" -> " (점화됨)")
            if (!string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(translatedSuffix) && suffix != translatedSuffix)
            {
                // First try direct replacement
                if (result.IndexOf(suffix, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = Regex.Replace(result, Regex.Escape(suffix), translatedSuffix, RegexOptions.IgnoreCase);
                }
                else
                {
                    // Suffix might be inside color tags like {{c|[empty]}}
                    // Try to handle complex bracket suffixes with color tags inside
                    string suffixContent = suffix.Trim();
                    string translatedContent = translatedSuffix.Trim();

                    // For bracket suffixes like "[32 drams of fresh water]", the original might have
                    // color tags inside: "[32 drams of {{G|fresh water}}]"
                    // Strategy: Find bracket boundaries and replace the entire bracketed section
                    if (suffixContent.StartsWith("[") && suffixContent.EndsWith("]"))
                    {
                        // Find the last [ and ] in result and replace everything between
                        int lastOpenBracket = result.LastIndexOf('[');
                        int lastCloseBracket = result.LastIndexOf(']');
                        if (lastOpenBracket >= 0 && lastCloseBracket > lastOpenBracket)
                        {
                            result = result.Substring(0, lastOpenBracket) + translatedContent + result.Substring(lastCloseBracket + 1);
                        }
                    }
                    else if (suffixContent.StartsWith("(") && suffixContent.EndsWith(")"))
                    {
                        // Same for parenthesis suffixes
                        int lastOpenParen = result.LastIndexOf('(');
                        int lastCloseParen = result.LastIndexOf(')');
                        if (lastOpenParen >= 0 && lastCloseParen > lastOpenParen)
                        {
                            result = result.Substring(0, lastOpenParen) + translatedContent + result.Substring(lastCloseParen + 1);
                        }
                    }
                    else
                    {
                        // Fallback: Match suffix inside color tags: {{X|suffix}}
                        string tagPattern = @"\{\{[^|]+\|" + Regex.Escape(suffixContent) + @"\}\}";
                        var tagMatch = Regex.Match(result, tagPattern, RegexOptions.IgnoreCase);
                        if (tagMatch.Success)
                        {
                            string replacement = tagMatch.Value.Replace(suffixContent, translatedContent);
                            result = result.Substring(0, tagMatch.Index) + replacement + result.Substring(tagMatch.Index + tagMatch.Length);
                        }
                        else
                        {
                            // Last resort: just replace the content if found
                            result = Regex.Replace(result, Regex.Escape(suffixContent), translatedContent, RegexOptions.IgnoreCase);
                        }
                    }
                }
            }

            return result;
        }
        
        #endregion
    }
}
