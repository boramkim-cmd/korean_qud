/*
 * 파일명: JsonRepository.cs
 * 분류: Data - Implementation
 * 역할: JSON 파일 기반 Repository 구현
 * 작성일: 2026-01-26
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using QudKorean.Objects.V2.Processing;
using XRL;

namespace QudKorean.Objects.V2.Data
{
    /// <summary>
    /// JSON file-based implementation of ITranslationRepository.
    /// Loads translation data from LOCALIZATION/OBJECTS directory.
    /// </summary>
    public class JsonRepository : ITranslationRepository
    {
        private const string LOG_PREFIX = "[QudKR-V2-Repo]";

        #region Private Fields

        private readonly Dictionary<string, ObjectData> _creatureCache = new Dictionary<string, ObjectData>();
        private readonly Dictionary<string, ObjectData> _itemCache = new Dictionary<string, ObjectData>();

        // Vocabulary dictionaries loaded from JSON
        private Dictionary<string, string> _materials = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _qualities = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _modifiers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _processing = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _tonics = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _grenades = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _marks = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _colors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _shaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _species = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _baseNouns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _states = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _liquids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _ofPatterns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _bodyParts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Cached sorted lists
        private List<KeyValuePair<string, string>> _prefixesSorted;
        private List<KeyValuePair<string, string>> _colorTagVocabSorted;
        private List<KeyValuePair<string, string>> _baseNounsSorted;
        private List<KeyValuePair<string, string>> _partSuffixesSorted;
        private Dictionary<string, string> _globalNameIndex;
        private Dictionary<string, string> _displayLookup;

        // Dictionary versions for O(1) lookup (used by optimized pipeline)
        private Dictionary<string, string> _prefixesDict;
        private Dictionary<string, string> _colorTagVocabDict;
        private Dictionary<string, string> _baseNounsDict;

        private string _modDirectory;
        private bool _initialized;
        private bool _loadedFromBundle;

        // Source map for error tracking
        private SourceMap _sourceMap = new SourceMap();

        #endregion

        #region ITranslationRepository Implementation

        public ObjectData GetCreature(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string normalizedId = TextNormalizer.NormalizeBlueprintId(id);

            if (_creatureCache.TryGetValue(normalizedId, out var data))
                return data;
            if (_creatureCache.TryGetValue(id, out data))
                return data;
            return null;
        }

        public ObjectData GetItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string normalizedId = TextNormalizer.NormalizeBlueprintId(id);

            if (_itemCache.TryGetValue(normalizedId, out var data))
                return data;
            if (_itemCache.TryGetValue(id, out data))
                return data;
            return null;
        }

        public IEnumerable<ObjectData> AllCreatures => _creatureCache.Values;
        public IEnumerable<ObjectData> AllItems => _itemCache.Values;

        public IReadOnlyList<KeyValuePair<string, string>> Prefixes
        {
            get
            {
                EnsureInitialized();
                return _prefixesSorted ?? new List<KeyValuePair<string, string>>();
            }
        }

        public IReadOnlyList<KeyValuePair<string, string>> ColorTagVocab
        {
            get
            {
                EnsureInitialized();
                return _colorTagVocabSorted ?? new List<KeyValuePair<string, string>>();
            }
        }

        public IReadOnlyList<KeyValuePair<string, string>> BaseNouns
        {
            get
            {
                EnsureInitialized();
                return _baseNounsSorted ?? new List<KeyValuePair<string, string>>();
            }
        }

        public IReadOnlyDictionary<string, string> PrefixesDict
        {
            get
            {
                EnsureInitialized();
                return _prefixesDict ?? (IReadOnlyDictionary<string, string>)new Dictionary<string, string>();
            }
        }

        public IReadOnlyDictionary<string, string> ColorTagVocabDict
        {
            get
            {
                EnsureInitialized();
                return _colorTagVocabDict ?? (IReadOnlyDictionary<string, string>)new Dictionary<string, string>();
            }
        }

        public IReadOnlyDictionary<string, string> BaseNounsDict
        {
            get
            {
                EnsureInitialized();
                return _baseNounsDict ?? (IReadOnlyDictionary<string, string>)new Dictionary<string, string>();
            }
        }

        public IReadOnlyDictionary<string, string> Species
        {
            get
            {
                EnsureInitialized();
                return _species;
            }
        }

        public IReadOnlyDictionary<string, string> States
        {
            get
            {
                EnsureInitialized();
                return _states;
            }
        }

        public IReadOnlyDictionary<string, string> Liquids
        {
            get
            {
                EnsureInitialized();
                return _liquids;
            }
        }

        public IReadOnlyDictionary<string, string> OfPatterns
        {
            get
            {
                EnsureInitialized();
                return _ofPatterns;
            }
        }

        public IReadOnlyDictionary<string, string> BodyParts
        {
            get
            {
                EnsureInitialized();
                return _bodyParts;
            }
        }

        public IReadOnlyList<KeyValuePair<string, string>> PartSuffixes
        {
            get
            {
                EnsureInitialized();
                return _partSuffixesSorted ?? new List<KeyValuePair<string, string>>();
            }
        }

        public IReadOnlyDictionary<string, string> Tonics
        {
            get
            {
                EnsureInitialized();
                return _tonics;
            }
        }

        public IReadOnlyDictionary<string, string> Shaders
        {
            get
            {
                EnsureInitialized();
                return _shaders;
            }
        }

        public IReadOnlyDictionary<string, string> GlobalNameIndex
        {
            get
            {
                EnsureInitialized();
                return _globalNameIndex ?? (IReadOnlyDictionary<string, string>)new Dictionary<string, string>();
            }
        }

        public IReadOnlyDictionary<string, string> DisplayLookup
        {
            get
            {
                EnsureInitialized();
                return _displayLookup ?? (IReadOnlyDictionary<string, string>)new Dictionary<string, string>();
            }
        }

        public void Reload()
        {
            ClearAll();
            _initialized = false;
            EnsureInitialized();
            UnityEngine.Debug.Log($"{LOG_PREFIX} Reloaded!");
        }

        public string GetStats()
        {
            EnsureInitialized();
            int vocabCount = _prefixesSorted?.Count ?? 0;
            int speciesCount = _species?.Count ?? 0;
            int nounsCount = _baseNouns?.Count ?? 0;
            int suffixesCount = (_states?.Count ?? 0) + (_liquids?.Count ?? 0) +
                               (_ofPatterns?.Count ?? 0) + (_bodyParts?.Count ?? 0) +
                               (_partSuffixesSorted?.Count ?? 0);
            int lookupCount = _displayLookup?.Count ?? 0;
            string mode = _loadedFromBundle ? "bundle" : "source";
            return $"Creatures: {_creatureCache.Count}, Items: {_itemCache.Count}, Vocab: {vocabCount}, Species: {speciesCount}, Nouns: {nounsCount}, Suffixes: {suffixesCount}, Lookup: {lookupCount}, Mode: {mode}";
        }

        /// <summary>
        /// Gets source information for a blueprint (for error tracking)
        /// </summary>
        public SourceInfo GetSourceInfo(string blueprintId)
        {
            return _sourceMap?.GetBlueprintSource(blueprintId);
        }

        /// <summary>
        /// Gets formatted source location for a blueprint
        /// </summary>
        public string GetSourceLocation(string blueprintId)
        {
            var info = _sourceMap?.GetBlueprintSource(blueprintId);
            return info?.Location ?? "unknown";
        }

        /// <summary>
        /// Formats error source information for logging
        /// </summary>
        public string FormatErrorSource(string blueprintId, string term = null)
        {
            return _sourceMap?.FormatErrorSource(blueprintId, term) ?? "Source unknown";
        }

        #endregion

        #region Initialization

        public void EnsureInitialized()
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

        private void ClearAll()
        {
            _creatureCache.Clear();
            _itemCache.Clear();
            _materials.Clear();
            _qualities.Clear();
            _modifiers.Clear();
            _processing.Clear();
            _tonics.Clear();
            _grenades.Clear();
            _marks.Clear();
            _colors.Clear();
            _species.Clear();
            _baseNouns.Clear();
            _states.Clear();
            _liquids.Clear();
            _ofPatterns.Clear();
            _bodyParts.Clear();
            _prefixesSorted = null;
            _colorTagVocabSorted = null;
            _baseNounsSorted = null;
            _partSuffixesSorted = null;
            _globalNameIndex = null;
            _displayLookup = null;
            _prefixesDict = null;
            _colorTagVocabDict = null;
            _baseNounsDict = null;
        }

        #endregion

        #region JSON Loading

        private string GetModDirectory()
        {
            if (_modDirectory != null) return _modDirectory;

            try
            {
                var modInfo = ModManager.GetMod("KoreanLocalization");
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

                // Fallback paths
                string[] possiblePaths = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Library", "Application Support", "com.FreeholdGames.CavesOfQud", "Mods", "KoreanLocalization"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "..", "LocalLow", "Freehold Games", "CavesOfQud", "Mods", "KoreanLocalization"),
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

        private void LoadAllJsonFiles()
        {
            string modDir = GetModDirectory();
            if (string.IsNullOrEmpty(modDir))
            {
                UnityEngine.Debug.LogWarning($"{LOG_PREFIX} Could not find mod directory");
                return;
            }

            // Check for bundled data first (optimized path)
            string bundleDir = Path.Combine(modDir, "data");
            string objectsBundle = Path.Combine(bundleDir, "objects.json");

            if (File.Exists(objectsBundle))
            {
                // Load from bundles (optimized path)
                LoadBundledData(modDir, bundleDir);

                // 번들에 포함되지 않는 보충 사전 로드
                // (tonics, grenades, marks, colors, shaders, nouns, species, suffixes, vocabulary)
                string objectsPath = Path.Combine(modDir, "LOCALIZATION", "OBJECTS");
                LoadItemCommon(objectsPath);
                LoadCreatureCommon(objectsPath);
                LoadItemNouns(objectsPath);
                LoadSuffixes(objectsPath);
                LoadVocabulary(objectsPath);
                LoadShared(modDir);

                // Load source map for error tracking
                string sourcemapPath = Path.Combine(modDir, "sourcemap.json");
                _sourceMap.Load(sourcemapPath);

                _loadedFromBundle = true;
                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded from bundles (optimized)");
            }
            else
            {
                // Load from source files (development path)
                LoadOriginalJsonFiles(modDir);
                _loadedFromBundle = false;
                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded from source files (dev mode)");
            }

            // Build sorted caches
            BuildSortedCaches();
        }

        /// <summary>
        /// Loads data from bundled JSON files (optimized path)
        /// </summary>
        private void LoadBundledData(string modDir, string bundleDir)
        {
            // Load objects.json bundle
            string objectsPath = Path.Combine(bundleDir, "objects.json");
            if (File.Exists(objectsPath))
            {
                try
                {
                    var root = JObject.Parse(File.ReadAllText(objectsPath));

                    // Load creatures section
                    var creatures = root["creatures"] as JObject;
                    if (creatures != null)
                    {
                        foreach (var prop in creatures.Properties())
                        {
                            LoadBlueprintEntry(prop.Name, prop.Value as JObject, _creatureCache);
                        }
                    }

                    // Load items section
                    var items = root["items"] as JObject;
                    if (items != null)
                    {
                        foreach (var prop in items.Properties())
                        {
                            LoadBlueprintEntry(prop.Name, prop.Value as JObject, _itemCache);
                        }
                    }

                    // Load furniture section
                    var furniture = root["furniture"] as JObject;
                    if (furniture != null)
                    {
                        foreach (var prop in furniture.Properties())
                        {
                            LoadBlueprintEntry(prop.Name, prop.Value as JObject, _itemCache);
                        }
                    }

                    // Load terrain section
                    var terrain = root["terrain"] as JObject;
                    if (terrain != null)
                    {
                        foreach (var prop in terrain.Properties())
                        {
                            LoadBlueprintEntry(prop.Name, prop.Value as JObject, _itemCache);
                        }
                    }

                    // Load hidden section
                    var hidden = root["hidden"] as JObject;
                    if (hidden != null)
                    {
                        foreach (var prop in hidden.Properties())
                        {
                            LoadBlueprintEntry(prop.Name, prop.Value as JObject, _itemCache);
                        }
                    }

                    // Load widgets section
                    var widgets = root["widgets"] as JObject;
                    if (widgets != null)
                    {
                        foreach (var prop in widgets.Properties())
                        {
                            LoadBlueprintEntry(prop.Name, prop.Value as JObject, _itemCache);
                        }
                    }

                    // Load vocabulary section
                    var vocabulary = root["vocabulary"] as JObject;
                    if (vocabulary != null)
                    {
                        LoadBundledVocabulary(vocabulary);
                    }

                    UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded objects bundle: {_creatureCache.Count} creatures, {_itemCache.Count} items");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load objects bundle: {ex.Message}");
                }
            }

            // Load display_lookup.json
            string lookupPath = Path.Combine(bundleDir, "display_lookup.json");
            if (File.Exists(lookupPath))
            {
                try
                {
                    var lookupRoot = JObject.Parse(File.ReadAllText(lookupPath));
                    _displayLookup = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (var prop in lookupRoot.Properties())
                    {
                        _displayLookup[prop.Name] = prop.Value.ToString();
                    }
                    UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded display_lookup: {_displayLookup.Count} entries");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load display_lookup: {ex.Message}");
                }
            }

            // Load shared.json bundle
            string sharedPath = Path.Combine(bundleDir, "shared.json");
            if (File.Exists(sharedPath))
            {
                try
                {
                    var root = JObject.Parse(File.ReadAllText(sharedPath));
                    LoadBundledShared(root);
                    UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded shared bundle");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load shared bundle: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads a blueprint entry from bundled data
        /// </summary>
        private void LoadBlueprintEntry(string blueprintId, JObject entry, Dictionary<string, ObjectData> cache)
        {
            if (entry == null) return;

            string normalizedId = TextNormalizer.NormalizeBlueprintId(blueprintId);

            var data = new ObjectData
            {
                BlueprintId = blueprintId,
                Description = entry["description"]?.ToString(),
                DescriptionKo = entry["description_ko"]?.ToString()
            };

            JObject names = entry["names"] as JObject;
            if (names != null)
            {
                foreach (var nameProp in names.Properties())
                {
                    data.Names[nameProp.Name] = nameProp.Value.ToString();
                }
            }

            cache[normalizedId] = data;
            if (normalizedId != blueprintId.ToLowerInvariant())
            {
                cache[blueprintId] = data;
            }
        }

        /// <summary>
        /// Loads vocabulary from bundled objects.json
        /// </summary>
        private void LoadBundledVocabulary(JObject vocabulary)
        {
            foreach (var prop in vocabulary.Properties())
            {
                if (prop.Name.StartsWith("_")) continue;

                string key = prop.Name;
                string value = prop.Value.ToString();

                // Add to modifiers as the catch-all vocabulary
                _modifiers[key] = value;
            }
        }

        /// <summary>
        /// Loads shared vocabulary from shared.json bundle
        /// </summary>
        private void LoadBundledShared(JObject root)
        {
            // Load _SHARED section
            var sharedSection = root["_SHARED"] as JObject;
            if (sharedSection != null)
            {
                foreach (var categoryProp in sharedSection.Properties())
                {
                    if (categoryProp.Name.StartsWith("_")) continue;

                    var category = categoryProp.Value as JObject;
                    if (category == null) continue;

                    foreach (var itemProp in category.Properties())
                    {
                        if (itemProp.Name.StartsWith("_")) continue;

                        string key = itemProp.Name;
                        string value = null;

                        if (itemProp.Value.Type == JTokenType.String)
                        {
                            value = itemProp.Value.ToString();
                        }
                        else if (itemProp.Value is JObject obj && obj["ko"] != null)
                        {
                            value = obj["ko"].ToString();

                            // Also add aliases
                            var aliases = obj["aliases"] as JArray;
                            if (aliases != null)
                            {
                                foreach (var alias in aliases)
                                {
                                    AddToVocabularyByCategory(categoryProp.Name, alias.ToString(), value);
                                }
                            }
                        }

                        if (value != null)
                        {
                            AddToVocabularyByCategory(categoryProp.Name, key, value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a vocabulary entry to the appropriate dictionary based on category
        /// </summary>
        private void AddToVocabularyByCategory(string category, string key, string value)
        {
            switch (category.ToLower())
            {
                case "materials":
                    _materials[key] = value;
                    break;
                case "qualities":
                    _qualities[key] = value;
                    break;
                case "modifiers":
                    _modifiers[key] = value;
                    break;
                case "processing":
                    _processing[key] = value;
                    break;
                case "species":
                    _species[key] = value;
                    break;
                case "body_parts":
                    _bodyParts[key] = value;
                    break;
                case "attributes":
                    // Skip attributes - not used in translation
                    break;
                case "factions":
                    // Skip factions - not used in translation
                    break;
                default:
                    _modifiers[key] = value;
                    break;
            }
        }

        /// <summary>
        /// Original JSON file loading logic (development fallback)
        /// </summary>
        private void LoadOriginalJsonFiles(string modDir)
        {
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
                    if (Path.GetFileName(file).StartsWith("_"))
                        continue;
                    LoadJsonFile(file, _itemCache);
                }
            }

            // Load furniture
            string furniturePath = Path.Combine(objectsPath, "furniture");
            if (Directory.Exists(furniturePath))
            {
                foreach (var file in Directory.GetFiles(furniturePath, "*.json", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(file).StartsWith("_"))
                        continue;
                    LoadJsonFile(file, _itemCache);
                }
            }

            // Load terrain
            string terrainPath = Path.Combine(objectsPath, "terrain");
            if (Directory.Exists(terrainPath))
            {
                foreach (var file in Directory.GetFiles(terrainPath, "*.json", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(file).StartsWith("_"))
                        continue;
                    LoadJsonFile(file, _itemCache);
                }
            }

            // Load standalone JSON files (hidden, widgets)
            string hiddenPath = Path.Combine(objectsPath, "hidden.json");
            if (File.Exists(hiddenPath))
            {
                LoadJsonFile(hiddenPath, _itemCache);
            }

            string widgetsPath = Path.Combine(objectsPath, "widgets.json");
            if (File.Exists(widgetsPath))
            {
                LoadJsonFile(widgetsPath, _itemCache);
            }

            // Load vocabulary files
            LoadItemCommon(objectsPath);
            LoadCreatureCommon(objectsPath);
            LoadItemNouns(objectsPath);
            LoadSuffixes(objectsPath);
            LoadVocabulary(objectsPath);
            LoadShared(modDir);

            // Try loading display_lookup.json from data/ even in dev mode
            string devLookupPath = Path.Combine(modDir, "data", "display_lookup.json");
            if (File.Exists(devLookupPath))
            {
                try
                {
                    var lookupRoot = JObject.Parse(File.ReadAllText(devLookupPath));
                    _displayLookup = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (var prop in lookupRoot.Properties())
                    {
                        _displayLookup[prop.Name] = prop.Value.ToString();
                    }
                    UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded display_lookup (dev): {_displayLookup.Count} entries");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load display_lookup (dev): {ex.Message}");
                }
            }
        }

        private void LoadJsonFile(string filePath, Dictionary<string, ObjectData> cache)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                JObject root = JObject.Parse(json);

                foreach (var prop in root.Properties())
                {
                    if (prop.Name.StartsWith("_")) continue;

                    string blueprintId = prop.Name;
                    string normalizedId = TextNormalizer.NormalizeBlueprintId(blueprintId);
                    JObject entry = prop.Value as JObject;
                    if (entry == null) continue;

                    var data = new ObjectData
                    {
                        BlueprintId = blueprintId,
                        Description = entry["description"]?.ToString(),
                        DescriptionKo = entry["description_ko"]?.ToString()
                    };

                    JObject names = entry["names"] as JObject;
                    if (names != null)
                    {
                        foreach (var nameProp in names.Properties())
                        {
                            data.Names[nameProp.Name] = nameProp.Value.ToString();
                        }
                    }

                    cache[normalizedId] = data;
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

        private void LoadSection(JObject section, Dictionary<string, string> target)
        {
            if (section == null) return;
            foreach (var prop in section.Properties())
            {
                if (prop.Name.StartsWith("_")) continue;
                target[prop.Name] = prop.Value.ToString();
            }
        }

        private void LoadItemCommon(string objectsPath)
        {
            string path = Path.Combine(objectsPath, "items", "_common.json");
            if (!File.Exists(path))
            {
                UnityEngine.Debug.Log($"{LOG_PREFIX} items/_common.json not found");
                return;
            }

            try
            {
                var root = JObject.Parse(File.ReadAllText(path));

                LoadSection(root["materials"] as JObject, _materials);
                LoadSection(root["qualities"] as JObject, _qualities);
                LoadSection(root["modifiers"] as JObject, _modifiers);
                LoadSection(root["processing"] as JObject, _processing);
                LoadSection(root["tonics"] as JObject, _tonics);
                LoadSection(root["grenades"] as JObject, _grenades);
                LoadSection(root["marks"] as JObject, _marks);
                LoadSection(root["colors"] as JObject, _colors);
                LoadSection(root["shaders"] as JObject, _shaders);

                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded items/_common.json: {_shaders.Count} shaders");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load items/_common.json: {ex.Message}");
            }
        }

        private void LoadCreatureCommon(string objectsPath)
        {
            string path = Path.Combine(objectsPath, "creatures", "_common.json");
            if (!File.Exists(path))
            {
                UnityEngine.Debug.Log($"{LOG_PREFIX} creatures/_common.json not found");
                return;
            }

            try
            {
                var root = JObject.Parse(File.ReadAllText(path));
                LoadSection(root["species"] as JObject, _species);

                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded creatures/_common.json: {_species.Count} species");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load creatures/_common.json: {ex.Message}");
            }
        }

        private void LoadItemNouns(string objectsPath)
        {
            string path = Path.Combine(objectsPath, "items", "_nouns.json");
            if (!File.Exists(path))
            {
                UnityEngine.Debug.Log($"{LOG_PREFIX} items/_nouns.json not found");
                return;
            }

            try
            {
                var root = JObject.Parse(File.ReadAllText(path));

                foreach (var prop in root.Properties())
                {
                    if (prop.Name.StartsWith("_")) continue;
                    LoadSection(prop.Value as JObject, _baseNouns);
                }

                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded items/_nouns.json: {_baseNouns.Count} nouns");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load items/_nouns.json: {ex.Message}");
            }
        }

        private void LoadSuffixes(string objectsPath)
        {
            string path = Path.Combine(objectsPath, "_suffixes.json");
            if (!File.Exists(path))
            {
                UnityEngine.Debug.Log($"{LOG_PREFIX} _suffixes.json not found");
                return;
            }

            try
            {
                var root = JObject.Parse(File.ReadAllText(path));

                LoadSection(root["states"] as JObject, _states);
                LoadSection(root["liquids"] as JObject, _liquids);
                LoadSection(root["of_patterns"] as JObject, _ofPatterns);
                LoadSection(root["body_parts"] as JObject, _bodyParts);

                // Load part_suffixes
                var partSuffixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                LoadSection(root["part_suffixes"] as JObject, partSuffixes);
                _partSuffixesSorted = DictionaryCache.SortByKeyLength(partSuffixes);

                int totalLoaded = _states.Count + _liquids.Count + _ofPatterns.Count +
                                  _bodyParts.Count + _partSuffixesSorted.Count;
                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded _suffixes.json: {totalLoaded} entries");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load _suffixes.json: {ex.Message}");
            }
        }

        private void LoadVocabulary(string objectsPath)
        {
            string vocabPath = Path.Combine(objectsPath, "_vocabulary");
            if (!Directory.Exists(vocabPath))
            {
                UnityEngine.Debug.Log($"{LOG_PREFIX} _vocabulary directory not found");
                return;
            }

            // Load modifiers.json (nested structure)
            string modifiersPath = Path.Combine(vocabPath, "modifiers.json");
            if (File.Exists(modifiersPath))
            {
                try
                {
                    var root = JObject.Parse(File.ReadAllText(modifiersPath));
                    int count = LoadNestedVocabulary(root, _modifiers);
                    UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded _vocabulary/modifiers.json: {count} entries");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load _vocabulary/modifiers.json: {ex.Message}");
                }
            }

            // Load processing.json (nested structure)
            string processingPath = Path.Combine(vocabPath, "processing.json");
            if (File.Exists(processingPath))
            {
                try
                {
                    var root = JObject.Parse(File.ReadAllText(processingPath));
                    int count = LoadNestedVocabulary(root, _processing);
                    UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded _vocabulary/processing.json: {count} entries");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load _vocabulary/processing.json: {ex.Message}");
                }
            }
        }

        private int LoadNestedVocabulary(JToken token, Dictionary<string, string> target)
        {
            int count = 0;
            if (token is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    if (prop.Name.StartsWith("_")) continue;

                    if (prop.Value is JObject nested)
                    {
                        // Check if it's a _SHARED format: { "ko": "번역", "aliases": [...] }
                        if (nested["ko"] != null)
                        {
                            string ko = nested["ko"].ToString();
                            target[prop.Name] = ko;
                            count++;

                            // Also add aliases
                            var aliases = nested["aliases"] as JArray;
                            if (aliases != null)
                            {
                                foreach (var alias in aliases)
                                {
                                    target[alias.ToString()] = ko;
                                    count++;
                                }
                            }
                        }
                        else
                        {
                            // Recurse into nested objects
                            count += LoadNestedVocabulary(nested, target);
                        }
                    }
                    else if (prop.Value.Type == JTokenType.String)
                    {
                        // Leaf node - actual translation pair
                        target[prop.Name] = prop.Value.ToString();
                        count++;
                    }
                }
            }
            return count;
        }

        private void LoadShared(string modDir)
        {
            string sharedPath = Path.Combine(modDir, "LOCALIZATION", "_SHARED");
            if (!Directory.Exists(sharedPath))
            {
                UnityEngine.Debug.Log($"{LOG_PREFIX} _SHARED directory not found");
                return;
            }

            // Load materials.json
            string materialsPath = Path.Combine(sharedPath, "materials.json");
            if (File.Exists(materialsPath))
            {
                try
                {
                    var root = JObject.Parse(File.ReadAllText(materialsPath));
                    int count = LoadNestedVocabulary(root["materials"], _materials);
                    UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded _SHARED/materials.json: {count} entries");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load _SHARED/materials.json: {ex.Message}");
                }
            }

            // Load qualities.json
            string qualitiesPath = Path.Combine(sharedPath, "qualities.json");
            if (File.Exists(qualitiesPath))
            {
                try
                {
                    var root = JObject.Parse(File.ReadAllText(qualitiesPath));
                    int count = LoadNestedVocabulary(root["qualities"], _qualities);
                    UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded _SHARED/qualities.json: {count} entries");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load _SHARED/qualities.json: {ex.Message}");
                }
            }

            // Load body_parts.json
            string bodyPartsPath = Path.Combine(sharedPath, "body_parts.json");
            if (File.Exists(bodyPartsPath))
            {
                try
                {
                    var root = JObject.Parse(File.ReadAllText(bodyPartsPath));
                    int count = LoadNestedVocabulary(root["body_parts"], _bodyParts);
                    UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded _SHARED/body_parts.json: {count} entries");
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load _SHARED/body_parts.json: {ex.Message}");
                }
            }
        }

        private void BuildSortedCaches()
        {
            // Build all prefixes sorted
            var allPrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            DictionaryCache.MergeInto(allPrefixes, _materials);
            DictionaryCache.MergeInto(allPrefixes, _qualities);
            DictionaryCache.MergeInto(allPrefixes, _modifiers);
            DictionaryCache.MergeInto(allPrefixes, _processing);
            DictionaryCache.MergeInto(allPrefixes, _tonics);
            DictionaryCache.MergeInto(allPrefixes, _grenades);
            DictionaryCache.MergeInto(allPrefixes, _marks);
            DictionaryCache.MergeInto(allPrefixes, _colors);
            DictionaryCache.MergeInto(allPrefixes, _shaders);
            DictionaryCache.MergeInto(allPrefixes, _species);
            _prefixesSorted = DictionaryCache.SortByKeyLength(allPrefixes);
            _prefixesDict = allPrefixes;

            // Build color tag vocab sorted
            var colorTagVocab = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            DictionaryCache.MergeInto(colorTagVocab, _materials);
            DictionaryCache.MergeInto(colorTagVocab, _qualities);
            DictionaryCache.MergeInto(colorTagVocab, _tonics);
            DictionaryCache.MergeInto(colorTagVocab, _grenades);
            DictionaryCache.MergeInto(colorTagVocab, _modifiers);
            DictionaryCache.MergeInto(colorTagVocab, _colors);
            DictionaryCache.MergeInto(colorTagVocab, _shaders);
            DictionaryCache.MergeInto(colorTagVocab, _liquids);
            DictionaryCache.MergeInto(colorTagVocab, _ofPatterns);
            DictionaryCache.MergeInto(colorTagVocab, _bodyParts);
            DictionaryCache.MergeInto(colorTagVocab, _species);
            _colorTagVocabSorted = DictionaryCache.SortByKeyLength(colorTagVocab);
            _colorTagVocabDict = colorTagVocab;

            // Build base nouns sorted
            _baseNounsSorted = DictionaryCache.SortByKeyLength(_baseNouns);
            _baseNounsDict = new Dictionary<string, string>(_baseNouns, StringComparer.OrdinalIgnoreCase);

            // Build global name index from all creatures and items
            _globalNameIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var creature in _creatureCache.Values)
            {
                foreach (var namePair in creature.Names)
                {
                    if (!string.IsNullOrEmpty(namePair.Value) && !_globalNameIndex.ContainsKey(namePair.Key))
                        _globalNameIndex[namePair.Key] = namePair.Value;
                }
            }
            foreach (var item in _itemCache.Values)
            {
                foreach (var namePair in item.Names)
                {
                    if (!string.IsNullOrEmpty(namePair.Value) && !_globalNameIndex.ContainsKey(namePair.Key))
                        _globalNameIndex[namePair.Key] = namePair.Value;
                }
            }

            UnityEngine.Debug.Log($"{LOG_PREFIX} Built caches: {_prefixesSorted.Count} prefixes, {_colorTagVocabSorted.Count} color tag vocab, {_baseNounsSorted.Count} nouns, {_globalNameIndex.Count} global names");
        }

        #endregion
    }
}
