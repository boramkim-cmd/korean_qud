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

        private string _modDirectory;
        private bool _initialized;

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
            return $"Creatures: {_creatureCache.Count}, Items: {_itemCache.Count}, Vocab: {vocabCount}, Species: {speciesCount}, Nouns: {nounsCount}, Suffixes: {suffixesCount}";
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

            // Load vocabulary files
            LoadItemCommon(objectsPath);
            LoadCreatureCommon(objectsPath);
            LoadItemNouns(objectsPath);
            LoadSuffixes(objectsPath);

            // Build sorted caches
            BuildSortedCaches();
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

            // Build base nouns sorted
            _baseNounsSorted = DictionaryCache.SortByKeyLength(_baseNouns);

            UnityEngine.Debug.Log($"{LOG_PREFIX} Built caches: {_prefixesSorted.Count} prefixes, {_colorTagVocabSorted.Count} color tag vocab, {_baseNounsSorted.Count} nouns");
        }

        #endregion
    }
}
