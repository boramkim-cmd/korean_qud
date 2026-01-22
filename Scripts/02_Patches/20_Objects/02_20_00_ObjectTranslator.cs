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
            public Dictionary<string, string> Names { get; set; } = new();
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
                return true;
            }
            
            // Try creature cache first, then item cache
            ObjectData data = null;
            if (_creatureCache.TryGetValue(blueprint, out data) || 
                _itemCache.TryGetValue(blueprint, out data))
            {
                // Try exact match
                string strippedOriginal = StripColorTags(originalName);
                if (data.Names.TryGetValue(strippedOriginal, out string koreanName))
                {
                    // Restore color tags if present in original
                    translated = RestoreColorTags(originalName, strippedOriginal, koreanName);
                    _displayNameCache[cacheKey] = translated;
                    return true;
                }
                
                // PRIORITY: Check state suffix BEFORE partial matching
                // This ensures "waterskin [empty]" -> "물주머니 [비어있음]" not "물주머니 [empty]"
                string noStateSuffix = StripStateSuffix(strippedOriginal);
                if (noStateSuffix != strippedOriginal)
                {
                    foreach (var namePair in data.Names)
                    {
                        if (namePair.Key.Equals(noStateSuffix, StringComparison.OrdinalIgnoreCase))
                        {
                            string suffix = strippedOriginal.Substring(noStateSuffix.Length);
                            translated = namePair.Value + TranslateStateSuffix(suffix);
                            _displayNameCache[cacheKey] = translated;
                            return true;
                        }
                    }
                }
                
                // Try any name in the names dictionary (partial match fallback)
                foreach (var kvp in data.Names)
                {
                    if (originalName.Contains(kvp.Key) || strippedOriginal.Contains(kvp.Key))
                    {
                        translated = originalName.Replace(kvp.Key, kvp.Value);
                        _displayNameCache[cacheKey] = translated;
                        return true;
                    }
                }
            }
            
            // Try with state suffix stripped for items NOT in blueprint cache
            string globalStripped = StripColorTags(originalName);
            string globalNoSuffix = StripStateSuffix(globalStripped);
            if (globalNoSuffix != globalStripped)
            {
                // Try finding translation for base name without state suffix in ALL caches
                foreach (var cache in new[] { _creatureCache, _itemCache })
                {
                    foreach (var kvp in cache)
                    {
                        foreach (var namePair in kvp.Value.Names)
                        {
                            if (namePair.Key.Equals(globalNoSuffix, StringComparison.OrdinalIgnoreCase))
                            {
                                string suffix = globalStripped.Substring(globalNoSuffix.Length);
                                translated = namePair.Value + TranslateStateSuffix(suffix);
                                _displayNameCache[cacheKey] = translated;
                                return true;
                            }
                        }
                    }
                }
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
            
            return false;
        }
        
        /// <summary>
        /// Strips state suffixes like [empty], [full], (lit), (unlit), x4, etc.
        /// </summary>
        private static string StripStateSuffix(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            
            string result = name;
            
            // Remove bracket suffixes: [empty], [full], [loaded], etc.
            result = Regex.Replace(result, @"\s*\[[^\]]+\]$", "");
            
            // Remove parenthesis suffixes: (lit), (unlit), (unburnt), etc.
            result = Regex.Replace(result, @"\s*\([^)]+\)$", "");
            
            // Remove count suffixes: x4, x10, etc.
            result = Regex.Replace(result, @"\s*x\d+$", "");
            
            return result.Trim();
        }
        
        /// <summary>
        /// Translates common state suffixes to Korean
        /// </summary>
        private static string TranslateStateSuffix(string suffix)
        {
            if (string.IsNullOrEmpty(suffix)) return "";
            
            // Common state translations
            var stateTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { " [empty]", " [비어있음]" },
                { " [full]", " [가득 참]" },
                { " [loaded]", " [장전됨]" },
                { " (lit)", " (점화됨)" },
                { " (unlit)", " (꺼짐)" },
                { " (unburnt)", " (미사용)" }
            };
            
            string trimmedSuffix = suffix.Trim();
            foreach (var kvp in stateTranslations)
            {
                if (kvp.Key.Trim().Equals(trimmedSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
            
            // Return original suffix if no translation found
            return suffix;
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
        /// Tries to find an item translation from cache
        /// </summary>
        private static bool TryGetItemTranslation(string itemName, out string translated)
        {
            translated = null;
            
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
            
            return false;
        }
        
        // Common species translations for dynamic food items
        private static readonly Dictionary<string, string> _commonSpeciesTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "bear", "곰" }, { "bat", "박쥐" }, { "pig", "돼지" }, { "boar", "멧돼지" },
            { "baboon", "비비" }, { "crab", "게" }, { "spider", "거미" }, { "beetle", "딱정벌레" },
            { "ant", "개미" }, { "fish", "물고기" }, { "worm", "벌레" }, { "bird", "새" },
            { "dog", "개" }, { "cat", "고양이" }, { "snapjaw", "스냅조" }, { "goatfolk", "염소인" },
            { "dromad", "드로마드" }, { "hindren", "힌드렌" }, { "leech", "거머리" },
            { "glowmoth", "발광나방" }, { "salthopper", "소금메뚜기" }, { "knollworm", "구릉지렁이" },
            { "electrofuge", "전기거미" }, { "eyeless crab", "눈먼 게" }, { "slug", "민달팽이" },
            { "girshling", "거슐링" }, { "tortoise", "거북" }, { "issachari", "이사차리" },
            { "albino ape", "흰알비노 유인원" }, { "ape", "유인원" }, { "croc", "악어" },
            { "crocodile", "악어" }, { "segmented mirthworm", "마디 웃음벌레" }
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
            
            ObjectData data = null;
            if (_creatureCache.TryGetValue(blueprint, out data) || 
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
            return $"Creatures: {_creatureCache.Count}, Items: {_itemCache.Count}, Cached: {_displayNameCache.Count}";
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
                    LoadJsonFile(file, _itemCache);
                }
            }
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
                    JObject entry = prop.Value as JObject;
                    if (entry == null) continue;
                    
                    var data = new ObjectData
                    {
                        BlueprintId = blueprintId,
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
                    
                    cache[blueprintId] = data;
                }
                
                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load {filePath}: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Tag Handling (Own implementation - not shared)
        
        /// <summary>
        /// Strips Qud color tags from text. Own implementation, not using TranslationEngine.
        /// </summary>
        private static string StripColorTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            // {{color|text}} format
            string result = Regex.Replace(text, @"\{\{[a-zA-Z]+\|([^}]+)\}\}", "$1");
            
            // Short format &color
            result = Regex.Replace(result, @"&[a-zA-Z]", "");
            
            return result.Trim();
        }
        
        /// <summary>
        /// Restores color tags from original to translated text.
        /// </summary>
        private static string RestoreColorTags(string original, string stripped, string translated)
        {
            if (string.IsNullOrEmpty(original) || original == stripped)
                return translated;
            
            // Try simple replacement
            return original.Replace(stripped, translated);
        }
        
        #endregion
    }
}
