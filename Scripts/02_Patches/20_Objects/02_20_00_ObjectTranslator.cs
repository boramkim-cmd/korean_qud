// Object Localization System - ObjectTranslator
// ISOLATED from existing translation infrastructure
// DO NOT modify TranslationEngine.cs or StructureTranslator.cs
// Version: 2.0 | Created: 2026-01-22

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
                
                // Try any name in the names dictionary
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
            
            // Use LocalizationManager's method (read-only reuse)
            try
            {
                // Find the mod directory by looking for our LOCALIZATION folder
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string modDir = Path.GetDirectoryName(assemblyPath);
                
                // Walk up to find LOCALIZATION/OBJECTS folder
                while (!string.IsNullOrEmpty(modDir))
                {
                    string objectsPath = Path.Combine(modDir, "LOCALIZATION", "OBJECTS");
                    if (Directory.Exists(objectsPath))
                    {
                        _modDirectory = modDir;
                        return _modDirectory;
                    }
                    modDir = Path.GetDirectoryName(modDir);
                }
                
                // Fallback: try common mod locations
                string[] possiblePaths = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                        "Caves of Qud", "Mods", "QudKorean"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Library", "Application Support", "Steam", "steamapps", "workshop", "content", "333640")
                };
                
                foreach (var path in possiblePaths)
                {
                    if (Directory.Exists(Path.Combine(path, "LOCALIZATION", "OBJECTS")))
                    {
                        _modDirectory = path;
                        return _modDirectory;
                    }
                }
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
