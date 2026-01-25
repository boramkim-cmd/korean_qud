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

        // 통합 접두사 사전 (긴 것 우선 정렬) - JSON에서 로드
        private static List<KeyValuePair<string, string>> _allPrefixesSortedLoaded = null;

        // 컬러 태그 내부용 (materials + qualities + tonics + grenades) - JSON에서 로드
        private static List<KeyValuePair<string, string>> _colorTagVocabSortedLoaded = null;

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

            List<string> translatedPrefixes = new List<string>();
            string current = name;

            // Iteratively extract prefixes (there may be multiple)
            bool foundAny = true;
            while (foundAny)
            {
                foundAny = false;
                foreach (var prefix in GetAllPrefixesSorted())
                {
                    if (current.StartsWith(prefix.Key + " ", StringComparison.OrdinalIgnoreCase))
                    {
                        translatedPrefixes.Add(prefix.Value);
                        current = current.Substring(prefix.Key.Length + 1);
                        foundAny = true;
                        break; // Restart search with longest prefixes first
                    }
                }
            }

            if (translatedPrefixes.Count > 0)
            {
                prefixKo = string.Join(" ", translatedPrefixes);
                remainder = current;
                return true;
            }

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
        /// </summary>
        private static string TranslateAllSuffixes(string suffixes)
        {
            if (string.IsNullOrEmpty(suffixes)) return "";

            string result = suffixes;

            // State translations
            var stateTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Bracket states
                { "[empty]", "[비어있음]" },
                { "[full]", "[가득 참]" },
                { "[loaded]", "[장전됨]" },

                // Parenthesis states
                { "(lit)", "(점화됨)" },
                { "(unlit)", "(꺼짐)" },
                { "(unburnt)", "(미사용)" }
            };

            // Liquid translations for [X drams of Y] pattern
            var liquidTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "fresh water", "신선한 물" },
                { "salt water", "소금물" },
                { "acid", "산" },
                { "oil", "기름" },
                { "water", "물" },
                { "blood", "피" },
                { "slime", "점액" },
                { "honey", "꿀" },
                { "wine", "와인" },
                { "cider", "사과주" },
                { "sap", "수액" },
                { "gel", "젤" },
                { "ink", "잉크" },
                { "lava", "용암" },
                { "putrid", "부패액" },
                { "cloning draught", "복제 음료" },
                { "brain brine", "뇌 염수" },
                { "neutron flux", "중성자 플럭스" }
            };

            // "of X" element/attribute translations (Phase 2.2)
            var ofTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // 원소 (Elements)
                { "fire", "불" },
                { "flame", "불꽃" },
                { "frost", "서리" },
                { "ice", "얼음" },
                { "cold", "냉기" },
                { "lightning", "번개" },
                { "thunder", "천둥" },
                { "electricity", "전기" },
                { "acid", "산" },
                { "poison", "독" },
                { "venom", "독액" },
                // 속성 (Attributes)
                { "strength", "힘" },
                { "agility", "민첩" },
                { "toughness", "강인함" },
                { "intelligence", "지능" },
                { "willpower", "의지력" },
                { "ego", "자아" },
                // 효과 (Effects)
                { "bleeding", "출혈" },
                { "confusion", "혼란" },
                { "stunning", "기절" },
                { "dismemberment", "절단" },
                { "slaying", "처치" },
                { "speed", "속도" },
                { "healing", "치유" },
                { "regeneration", "재생" },
                // 기타 (Misc)
                { "light", "빛" },
                { "darkness", "어둠" },
                { "shadow", "그림자" },
                { "the void", "공허" },
                { "the ancients", "고대인" }
            };

            // Apply simple state translations
            foreach (var kvp in stateTranslations)
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
                string liquidKo = liquidTranslations.TryGetValue(liquid, out var ko) ? ko : liquid;
                return $"[{liquidKo} {amount}드램]";
            }, RegexOptions.IgnoreCase);

            // [X servings] pattern → [X인분]
            result = Regex.Replace(result, @"\[(\d+) servings?\]", "[$1인분]", RegexOptions.IgnoreCase);

            // "of X" pattern → "의 X번역" (Phase 2.2)
            result = Regex.Replace(result, @"\s+of\s+([\w\s]+)$", m => {
                string element = m.Groups[1].Value.Trim();
                string elementKo = ofTranslations.TryGetValue(element, out var ko) ? ko : element;
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
            _allPrefixesSortedLoaded = null;
            _colorTagVocabSortedLoaded = null;

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
            string cacheKey = $"{blueprint}:{originalName}";
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
                                _displayNameCache[cacheKey] = translated;
                                return true;
                            }
                        }
                    }
                }
            }

            // === Prefix/Suffix System ===
            // Extract all suffixes first, then try to match prefixes
            // Use material-translated version so "{{w|청동}} mace" gets processed correctly
            string strippedForPrefix = StripColorTags(withTranslatedMaterials);
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
            }

            // Try base item name lookup (handles both simple items and items with suffixes)
            // This covers: "torch" -> "횃불", "torch (unburnt)" -> "횃불 (미사용)"
            if (TryGetItemTranslation(baseNameForPrefix, out string baseKo2) ||
                TryGetCreatureTranslation(baseNameForPrefix, out baseKo2))
            {
                string suffixKo = TranslateAllSuffixes(allSuffixes);
                translated = string.IsNullOrEmpty(suffixKo) ? baseKo2 : $"{baseKo2}{suffixKo}";
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
        /// </summary>
        private static string TranslateStateSuffix(string suffix)
        {
            if (string.IsNullOrEmpty(suffix)) return "";

            string result = suffix;

            // State translations - keys without leading space for Contains() matching
            var stateTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "[empty]", "[비어있음]" },
                { "[full]", "[가득 참]" },
                { "[loaded]", "[장전됨]" },
                { "(lit)", "(점화됨)" },
                { "(unlit)", "(꺼짐)" },
                { "(unburnt)", "(미사용)" }
            };

            // Iterate through patterns and replace any that are contained (supports compound suffixes)
            foreach (var kvp in stateTranslations)
            {
                if (result.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = Regex.Replace(result, Regex.Escape(kvp.Key), kvp.Value, RegexOptions.IgnoreCase);
                }
            }

            // Liquid translations for [X drams of Y] pattern
            var liquidTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "fresh water", "신선한 물" },
                { "salt water", "소금물" },
                { "acid", "산" },
                { "oil", "기름" },
                { "water", "물" },
                { "blood", "피" },
                { "slime", "점액" },
                { "honey", "꿀" },
                { "wine", "와인" },
                { "cider", "사과주" },
                { "sap", "수액" },
                { "gel", "젤" },
                { "ink", "잉크" },
                { "lava", "용암" },
                { "putrid", "부패액" },
                { "convalessence", "회복액" },
                { "cloning draught", "복제 음료" },
                { "brain brine", "뇌 염수" },
                { "neutron flux", "중성자 플럭스" }
            };

            // [X drams of Y] pattern → [Y X드램]
            result = Regex.Replace(result, @"\[(\d+) drams? of ([^\]]+)\]", m => {
                string amount = m.Groups[1].Value;
                string liquid = m.Groups[2].Value.Trim();
                string liquidKo = liquidTranslations.TryGetValue(liquid, out var ko) ? ko : liquid;
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

            // 부위 패턴 목록 (Part patterns) - 길이순 정렬 필요
            var partPatterns = new (string suffix, string korean)[]
            {
                // 복수형 먼저 (longer matches first)
                (" feathers", " 깃털"),
                (" scales", " 비늘"),
                (" horns", " 뿔"),
                (" bones", " 뼈"),
                (" teeth", " 이빨"),
                (" claws", " 발톱"),
                (" wings", " 날개"),
                // 단수형
                (" feather", " 깃털"),
                (" scale", " 비늘"),
                (" skull", " 두개골"),
                (" horn", " 뿔"),
                (" bone", " 뼈"),
                (" hide", " 가죽"),
                (" pelt", " 모피"),
                (" skin", " 피부"),
                (" tooth", " 이빨"),
                (" fang", " 송곳니"),
                (" claw", " 발톱"),
                (" talon", " 발톱"),
                (" tail", " 꼬리"),
                (" wing", " 날개"),
                (" beak", " 부리"),
                (" shell", " 껍데기"),
                (" carapace", " 갑각"),
                (" egg", " 알"),
                (" eggs", " 알")
            };

            // "raw {creature} {part}" 패턴 처리
            if (stripped.StartsWith("raw ", StringComparison.OrdinalIgnoreCase))
            {
                string remainder = stripped.Substring("raw ".Length);
                foreach (var (suffix, korean) in partPatterns)
                {
                    if (remainder.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        string creaturePart = remainder.Substring(0, remainder.Length - suffix.Length);
                        if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                        {
                            translated = $"생 {creatureKo}{korean}";
                            return true;
                        }
                    }
                }
            }

            // 일반 "{creature} {part}" 패턴 처리
            foreach (var (suffix, korean) in partPatterns)
            {
                if (stripped.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    string creaturePart = stripped.Substring(0, stripped.Length - suffix.Length);
                    if (TryGetCreatureTranslation(creaturePart, out string creatureKo))
                    {
                        translated = $"{creatureKo}{korean}";
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

            // Check base noun translations first
            if (_baseNounTranslations.TryGetValue(part, out partKo))
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

            // Try common body parts directly
            var bodyParts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "claw", "발톱" }, { "claws", "발톱" },
                { "fang", "송곳니" }, { "fangs", "송곳니" },
                { "tooth", "이빨" }, { "teeth", "이빨" },
                { "horn", "뿔" }, { "horns", "뿔" },
                { "tail", "꼬리" }, { "tails", "꼬리" },
                { "hide", "가죽" }, { "pelt", "모피" },
                { "bone", "뼈" }, { "bones", "뼈" },
                { "skull", "두개골" },
                { "eye", "눈" }, { "eyes", "눈" },
                { "heart", "심장" },
                { "blood", "피" },
                { "liver", "간" },
                { "brain", "뇌" },
                { "tongue", "혀" },
                { "ear", "귀" }, { "ears", "귀" },
                { "wing", "날개" }, { "wings", "날개" },
                { "feather", "깃털" }, { "feathers", "깃털" },
                { "scale", "비늘" }, { "scales", "비늘" },
                { "shell", "껍데기" },
                { "beak", "부리" },
                { "talon", "발톱" }, { "talons", "발톱" },
                { "mane", "갈기" },
                { "fur", "털" },
                { "whisker", "수염" }, { "whiskers", "수염" },
                { "tusk", "엄니" }, { "tusks", "엄니" }
            };

            if (bodyParts.TryGetValue(part, out partKo))
            {
                translated = $"{creatureKo}의 {partKo}";
                return true;
            }

            return false;
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
            
            // Fallback: common species names
            if (_commonSpeciesTranslations.TryGetValue(creatureName.ToLowerInvariant(), out translated))
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

            // First try JSON cache
            foreach (var kvp in _itemCache)
            {
                foreach (var namePair in kvp.Value.Names)
                {
                    if (namePair.Key.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                    {
                        translated = namePair.Value;
                        return true;
                    }
                }
            }

            // Fallback: check base noun translations (for arrow, mace, dagger, etc.)
            if (_baseNounTranslations.TryGetValue(itemName, out translated))
            {
                return true;
            }

            return false;
        }
        
        // Common species translations for dynamic food items and parts
        private static readonly Dictionary<string, string> _commonSpeciesTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // 기존 생물 (Existing creatures)
            { "bear", "곰" }, { "bat", "박쥐" }, { "pig", "돼지" }, { "boar", "멧돼지" },
            { "baboon", "비비" }, { "crab", "게" }, { "spider", "거미" }, { "beetle", "딱정벌레" },
            { "ant", "개미" }, { "fish", "물고기" }, { "worm", "벌레" }, { "bird", "새" },
            { "dog", "개" }, { "cat", "고양이" }, { "snapjaw", "스냅조" }, { "goatfolk", "염소인" },
            { "dromad", "드로마드" }, { "hindren", "힌드렌" }, { "leech", "거머리" },
            { "glowmoth", "발광나방" }, { "salthopper", "소금메뚜기" }, { "knollworm", "구릉지렁이" },
            { "electrofuge", "전기거미" }, { "eyeless crab", "눈먼 게" }, { "slug", "민달팽이" },
            { "girshling", "거슐링" }, { "tortoise", "거북" }, { "issachari", "이사차리" },
            { "albino ape", "흰알비노 유인원" }, { "ape", "유인원" }, { "croc", "악어" },
            { "crocodile", "악어" }, { "segmented mirthworm", "마디 웃음벌레" },

            // 추가 생물 (Additional creatures) - Phase 1.2
            { "rhinox", "라이녹스" },
            { "phase spider", "위상 거미" },
            { "great saltback", "대형 소금등" },
            { "saltback", "소금등" },
            { "dawnglider", "새벽활강꾼" },
            { "eyeless king crab", "눈먼 왕게" },
            { "bone worm", "뼈 벌레" },
            { "traipsing mortar", "배회하는 박격포" },
            { "chrome pyramid", "크롬 피라미드" },
            { "madpole", "미친극지" },
            { "qudzu", "쿠드주" },
            { "seedsprout", "싹틔움" },
            { "voider", "보이더" },
            { "waydrotter", "길부패꾼" },
            { "worm of the earth", "대지의 벌레" },
            { "cave spider", "동굴 거미" },
            { "jilted lover", "버림받은 연인" },
            { "lurking beth", "잠복 베스" },
            { "fire ant", "불개미" },
            { "ice frog", "얼음 개구리" },
            { "frog", "개구리" },
            { "quillipede", "가시지네" },
            { "slumberling", "졸음이" },
            { "snapjaw scavenger", "스냅조 청소부" },
            { "snapjaw hunter", "스냅조 사냥꾼" },
            { "snapjaw warrior", "스냅조 전사" },
            { "snapjaw brute", "스냅조 야수" },
            { "snapjaw warlord", "스냅조 전쟁군주" },
            { "chitinous puma", "키틴질 퓨마" },
            { "puma", "퓨마" },
            { "sawhander", "톱손이" },
            { "chainsaw", "전기톱" },
            { "yempuris", "옘푸리스" },
            { "agolfly", "아골파리" },
            { "plasmafly", "플라즈마파리" },
            { "urshiib", "우르시브" },
            { "naphtaali", "나프탈리" },
            { "watervine", "물덩굴" },
            { "young ivory", "어린 상아" },
            { "ivory", "상아" },
            { "slog", "슬로그" },
            { "troll", "트롤" },
            { "ogre", "오우거" },
            { "goat", "염소" },
            { "horse", "말" },
            { "mule", "노새" },
            { "donkey", "당나귀" },
            { "hawk", "매" },
            { "eagle", "독수리" },
            { "owl", "올빼미" },
            { "raven", "까마귀" },
            { "crow", "까마귀" },
            { "chicken", "닭" },
            { "turkey", "칠면조" },
            { "duck", "오리" },
            { "swan", "백조" },
            { "rat", "쥐" },
            { "mouse", "생쥐" },
            { "hare", "산토끼" },
            { "rabbit", "토끼" },
            { "fox", "여우" },
            { "wolf", "늑대" },
            { "deer", "사슴" },
            { "stag", "수사슴" },
            { "elk", "엘크" },
            { "moose", "무스" },
            { "snake", "뱀" },
            { "serpent", "뱀" },
            { "lizard", "도마뱀" },
            { "gecko", "게코" },
            { "salamander", "도롱뇽" },
            { "newt", "영원" },

            // 대형 포식자 (Large predators) - Phase 소유격 패턴
            { "panther", "표범" },
            { "lion", "사자" },
            { "tiger", "호랑이" },
            { "leopard", "표범" },
            { "jaguar", "재규어" },
            { "cougar", "쿠거" },
            { "lynx", "스라소니" },
            { "hyena", "하이에나" },
            { "jackal", "자칼" },
            { "wolverine", "울버린" },
            { "badger", "오소리" },
            { "weasel", "족제비" },
            { "otter", "수달" },
            { "mink", "밍크" },
            { "beaver", "비버" },
            { "buffalo", "물소" },
            { "bison", "들소" },
            { "rhino", "코뿔소" },
            { "hippo", "하마" },
            { "elephant", "코끼리" },
            { "mammoth", "매머드" },

            // 세력/고유명사 (Factions/Proper Names) - Phase 2차 번역 확장
            { "Praetorian", "프라이토리안" },
            { "Issachar", "이사카르" },

            // 수염 생물 (Beard creatures for glands) - Phase 6.3
            { "flamebeard", "불수염" },
            { "sleetbeard", "진눈깨비수염" },
            { "tartbeard", "타르수염" },
            { "nullbeard", "허무수염" },
            { "gallbeard", "담즙수염" },
            { "dreambeard", "꿈수염" },
            { "stillbeard", "고요수염" },
            { "mazebeard", "미로수염" }
        };
        
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
            return $"Creatures: {_creatureCache.Count}, Items: {_itemCache.Count}, Vocab: {vocabCount}, Cached: {_displayNameCache.Count}";
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

                // 통합 접두사 사전 구축 (모든 카테고리)
                var allPrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                MergeInto(allPrefixes, _materialsLoaded);
                MergeInto(allPrefixes, _qualitiesLoaded);
                MergeInto(allPrefixes, _modifiersLoaded);
                MergeInto(allPrefixes, _processingLoaded);
                MergeInto(allPrefixes, _tonicsLoaded);
                MergeInto(allPrefixes, _grenadesLoaded);
                MergeInto(allPrefixes, _marksLoaded);

                _allPrefixesSortedLoaded = allPrefixes.ToList();
                _allPrefixesSortedLoaded.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));

                // 컬러 태그 내부용 (재료 + 품질 + 토닉 + 수류탄)
                var colorTagVocab = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                MergeInto(colorTagVocab, _materialsLoaded);
                MergeInto(colorTagVocab, _qualitiesLoaded);
                MergeInto(colorTagVocab, _tonicsLoaded);
                MergeInto(colorTagVocab, _grenadesLoaded);
                MergeInto(colorTagVocab, _modifiersLoaded); // modifiers에도 컬러 태그에서 쓰이는 것들 있음

                _colorTagVocabSortedLoaded = colorTagVocab.ToList();
                _colorTagVocabSortedLoaded.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));

                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded items/_common.json: {allPrefixes.Count} prefixes, {colorTagVocab.Count} color tag vocab");
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
        /// 색상 태그 내의 재료를 번역합니다. (Phase 3.1 개선)
        /// Handles: {{color|material}}, {{shader|text}}, nested tags, and multiple sequential tags.
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

        /// <summary>
        /// 기본 명사 번역 사전 (Phase 3.2)
        /// 색상 태그 외부의 아이템 명사를 번역합니다.
        /// </summary>
        private static readonly Dictionary<string, string> _baseNounTranslations = new(StringComparer.OrdinalIgnoreCase)
        {
            // 무기 (Weapons)
            { "dagger", "단검" },
            { "sword", "검" },
            { "long sword", "장검" },
            { "short sword", "단검" },
            { "mace", "메이스" },
            { "hammer", "해머" },
            { "axe", "도끼" },
            { "spear", "창" },
            { "staff", "지팡이" },
            { "bow", "활" },
            { "crossbow", "석궁" },
            { "arrow", "화살" },
            { "bolt", "볼트" },
            { "club", "곤봉" },
            { "flail", "도리깨" },
            { "halberd", "미늘창" },
            { "pike", "장창" },
            { "cudgel", "몽둥이" },

            // 방어구 (Armor)
            { "helmet", "투구" },
            { "helm", "투구" },
            { "shield", "방패" },
            { "buckler", "버클러" },
            { "armor", "갑옷" },
            { "mail", "사슬갑옷" },
            { "plate", "판금갑옷" },
            { "gloves", "장갑" },
            { "gauntlets", "건틀릿" },
            { "boots", "부츠" },
            { "greaves", "정강이받이" },
            { "cloak", "망토" },
            { "cape", "케이프" },
            { "robe", "로브" },

            // 소비품 (Consumables)
            { "injector", "주사기" },
            { "tonic", "토닉" },
            { "grenade", "수류탄" },
            { "torch", "횃불" },
            { "canteen", "수통" },
            { "waterskin", "물주머니" },
            { "ration", "식량" },
            { "bandage", "붕대" },
            { "salve", "연고" },

            // 도구 (Tools)
            { "pick", "곡괭이" },
            { "pickaxe", "곡괭이" },
            { "shovel", "삽" },
            { "rope", "밧줄" },
            { "lantern", "랜턴" },
            { "lamp", "램프" },
            { "key", "열쇠" },
            { "lockpick", "자물쇠따개" },

            // 기타 (Misc)
            { "book", "책" },
            { "scroll", "두루마리" },
            { "ring", "반지" },
            { "amulet", "부적" },
            { "bracelet", "팔찌" },
            { "necklace", "목걸이" },
            { "gem", "보석" },
            { "coin", "동전" },
            { "corpse", "시체" },
            { "nugget", "덩어리" },
            { "ingot", "주괴" },
            { "bar", "바" },
            { "chunk", "덩어리" },
            { "lump", "덩어리" },

            // 추가 방어구/의류 (Additional armor/clothing)
            { "moccasins", "모카신" },
            { "sandals", "샌들" },
            { "shoes", "신발" },
            { "slippers", "슬리퍼" },
            { "wreath", "화환" },
            { "crown", "왕관" },
            { "circlet", "관" },
            { "headband", "머리띠" },
            { "mask", "가면" },
            { "goggles", "고글" },
            { "vest", "조끼" },
            { "tunic", "튜닉" },
            { "shirt", "셔츠" },
            { "pants", "바지" },
            { "skirt", "치마" },
            { "belt", "벨트" },
            { "sash", "허리띠" },

            // 기계/전자 (Mechanical/Electronic)
            { "cell", "전지" },
            { "battery", "배터리" },
            { "headlamp", "헤드램프" },
            { "flashlight", "손전등" },
            { "tube", "튜브" },
            { "canister", "용기" },
            { "tank", "탱크" },
            { "pack", "팩" },
            { "bodypack", "바디팩" },
            { "backpack", "배낭" },

            // 유물/특수 (Artifacts/Special)
            { "artifact", "유물" },
            { "relic", "유물" },
            { "charm", "부적" },
            { "talisman", "부적" },
            { "totem", "토템" },
            { "idol", "우상" },
            { "food cube", "식품 큐브" },
            { "cube", "큐브" },
            { "sphere", "구체" },
            { "orb", "오브" },
            { "crystal", "수정" },
            { "shard", "조각" },
            { "fragment", "파편" },
            { "pill", "알약" },
            { "capsule", "캡슐" },
            { "vial", "약병" },
            { "bottle", "병" },
            { "jar", "단지" },
            { "flask", "플라스크" },
            { "phial", "소병" },

            // 총기류 (Firearms) - Phase 2차 번역 확장
            { "revolver", "리볼버" },
            { "rifle", "라이플" },
            { "pistol", "권총" },
            { "shotgun", "산탄총" },
            { "carbine", "카빈총" },
            { "blunderbuss", "나팔총" },
            { "musket", "머스킷" },
            { "arquebus", "아르케부스" },

            // 추가 명사 (Additional Nouns) - Phase 2차 번역 확장
            { "tubes", "튜브" },
            { "processing core", "처리 코어" },
            { "core", "코어" }
        };

        // Cached sorted list for longest-first matching
        private static List<KeyValuePair<string, string>> _baseNounsSorted = null;

        private static List<KeyValuePair<string, string>> GetBaseNounsSorted()
        {
            if (_baseNounsSorted == null)
            {
                _baseNounsSorted = new List<KeyValuePair<string, string>>(_baseNounTranslations);
                _baseNounsSorted.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
            }
            return _baseNounsSorted;
        }

        /// <summary>
        /// 색상 태그 외부의 기본 명사를 번역합니다. (Phase 3.2 - 버그 수정)
        /// "{{w|bronze}} mace" → "{{w|bronze}} 메이스"
        /// 세그먼트 분할 방식으로 태그 내부/외부를 정확히 구분합니다.
        /// </summary>
        private static string TranslateBaseNounsOutsideTags(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // 태그가 없으면 직접 교체
            if (!text.Contains("{{"))
            {
                return TranslateNounsInText(text);
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
                    // 태그 시작 발견 - 이전 텍스트 처리
                    if (i > lastEnd)
                    {
                        result.Append(TranslateNounsInText(text.Substring(lastEnd, i - lastEnd)));
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

            // 남은 텍스트 처리
            if (lastEnd < text.Length)
            {
                result.Append(TranslateNounsInText(text.Substring(lastEnd)));
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
