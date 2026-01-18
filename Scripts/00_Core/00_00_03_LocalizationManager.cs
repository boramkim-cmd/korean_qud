/*
 * 파일명: 00_03_LocalizationManager.cs
 * 분류: [Core] 로컬라이제이션 데이터 관리자
 * 역할: JSON 번역 파일을 로드하고 카테고리별로 관리하며, 세분화된 카테고리 병합 기능을 제공합니다.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using XRL;

namespace QudKRTranslation.Core
{
    public static class LocalizationManager
    {
        private static Dictionary<string, Dictionary<string, string>> _translationDB = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static bool _isLoaded = false;
        private static string _modPath = null;

        public static void Initialize()
        {
            if (_isLoaded) return;
            LoadAllJsonFiles();
            _isLoaded = true;
        }

        public static void Reload()
        {
            _translationDB.Clear();
            _isLoaded = false;
            Initialize();
        }

        public static void LoadGlossary()
        {
            string modDir = GetModDirectory();
            if (string.IsNullOrEmpty(modDir))
            {
                Debug.LogError("[LocalizationManager] Mod directory not found, cannot load glossary.");
                return;
            }

            // CHARGEN
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/CHARGEN/modes.json"));
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/CHARGEN/stats.json"));
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/CHARGEN/ui.json"));
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/CHARGEN/presets.json"));
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/CHARGEN/locations.json"));
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/CHARGEN/factions.json"));

            // GAMEPLAY
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/GAMEPLAY/skills.json"));
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/GAMEPLAY/cybernetics.json"));

            // UI
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/UI/common.json"));
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/UI/options.json"));
            LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/UI/terms.json"));

            // Load legacy if needed (Deprecated)
            // LoadJsonFile(Path.Combine(modDir, "LOCALIZATION/_DEPRECATED/glossary_proto.json")); 
        }

        private static void LoadAllJsonFiles()
        {
            string locDir = Path.Combine(GetModDirectory(), "LOCALIZATION");
            if (!Directory.Exists(locDir))
            {
                Debug.LogError($"[LocalizationManager] Localization directory not found: {locDir}");
                return;
            }

            foreach (var file in Directory.GetFiles(locDir, "*.json"))
            {
                LoadJsonFile(file);
            }
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;
            
            // 0. 색상 태그를 소문자로 통일: {{C|text}} → {{c|text}}
            string result = Regex.Replace(key, @"\{\{([a-zA-Z])\|", m => $"{{{{{m.Groups[1].Value.ToLower()}|", RegexOptions.IgnoreCase);
            
            // 1. Qud 색상 태그 제거: {{X|text}} → text
            result = Regex.Replace(result, @"\{\{[a-zA-Z]\|([^}]+)\}\}", "$1");
            
            // 2. 특수 bullet 문자 제거 (ù 등 - 색상 태그 내부에서 사용됨)
            result = Regex.Replace(result, @"^[ùúûü·•◦‣⁃]\s*", "");
            
            // 3. 소문자 변환 및 앞뒤 공백 제거
            return result.Trim().ToLowerInvariant();
        }
        
        public static string GetModDirectory()
        {
            if (_modPath != null) return _modPath;

            try
            {
                var mod = ModManager.GetMod("KoreanLocalization");
                if (mod != null && !string.IsNullOrEmpty(mod.Path))
                {
                    _modPath = mod.Path;
                    return _modPath;
                }
            }
            catch { }

            // Fallback for OSX
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string modsRoot = Path.Combine(homeDir, "Library/Application Support/com.FreeholdGames.CavesOfQud/Mods");
                string target = Path.Combine(modsRoot, "KoreanLocalization");
                if (Directory.Exists(target)) return target;
            }

            return "";
        }

        private static void LoadJsonFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var data = RecursiveJsonParser.Parse(json);

                foreach (var categoryPair in data)
                {
                    string category = categoryPair.Key;
                    var terms = categoryPair.Value;

                    if (!_translationDB.ContainsKey(category))
                    {
                        _translationDB[category] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }

                    foreach (var termPair in terms)
                    {
                        _translationDB[category][termPair.Key] = termPair.Value;

                        // NormalizeKey를 사용하여 정규화된 키도 함께 저장
                        // 예: "{{c|ù}} most creatures..."와 "most creatures..." 모두 검색 가능하게 함
                        string normalized = NormalizeKey(termPair.Key);
                        if (!string.IsNullOrEmpty(normalized) && !normalized.Equals(termPair.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!_translationDB[category].ContainsKey(normalized))
                            {
                                _translationDB[category][normalized] = termPair.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationManager] Failed to load {path}: {e.Message}");
            }
        }

        // ========================================================================
        // 조회 API (서브 카테고리 지원)
        // ========================================================================

        public static Dictionary<string, string> GetCategory(string category)
        {
            if (!_isLoaded) Initialize();

            if (category.EndsWith("*"))
            {
                return GetCategoryGroup(category.Substring(0, category.Length - 1));
            }

            if (_translationDB.TryGetValue(category, out var dict)) return dict;

            // 자동 병합 (예: "power" 호출 시 "power_*" 병합)
            var subCats = _translationDB.Keys.Where(k => k.StartsWith(category + "_")).ToList();
            if (subCats.Count > 0) return GetCategoryGroup(category + "_");

            return null;
        }

        public static Dictionary<string, string> GetCategoryGroup(string prefix)
        {
            if (!_isLoaded) Initialize();
            var combined = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var targets = _translationDB.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

            foreach (var cat in targets)
            {
                foreach (var kv in _translationDB[cat])
                {
                    combined[kv.Key] = kv.Value;
                }
            }
            return combined.Count > 0 ? combined : null;
        }

        public static string GetTerm(string category, string key, string fallback = "")
        {
            if (!_isLoaded) Initialize();

            if (string.IsNullOrEmpty(key)) return fallback;
            string normalizedKey = NormalizeKey(key);

            if (_translationDB.TryGetValue(category, out var dict))
            {
                // 1. Exact match
                if (dict.TryGetValue(key, out string val)) return val;
                // 2. Normalized match
                if (dict.TryGetValue(normalizedKey, out val)) return val;
            }

            // 3. Auto sub-category search
            var subCats = _translationDB.Keys.Where(k => k.StartsWith(category + "_"));
            foreach (var cat in subCats)
            {
                var subDict = _translationDB[cat];
                if (subDict.TryGetValue(key, out string val)) return val;
                if (subDict.TryGetValue(normalizedKey, out val)) return val;
            }

            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }

        public static bool TryGetAnyTerm(string key, out string result, params string[] categories)
        {
            if (!_isLoaded) Initialize();

            if (string.IsNullOrEmpty(key))
            {
                result = null;
                return false;
            }
            string normalizedKey = NormalizeKey(key);

            if (categories == null || categories.Length == 0)
            {
                foreach (var dict in _translationDB.Values)
                {
                    if (dict.TryGetValue(key, out result)) return true;
                    if (dict.TryGetValue(normalizedKey, out result)) return true;
                }
            }
            else
            {
                foreach (var cat in categories)
                {
                    if (_translationDB.TryGetValue(cat, out var dict))
                    {
                        if (dict.TryGetValue(key, out result)) return true;
                        if (dict.TryGetValue(normalizedKey, out result)) return true;
                    }

                    // Sub-category search
                    string prefix = cat + "_";
                    var subCats = _translationDB.Keys.Where(k => k.StartsWith(prefix));
                    foreach (var sc in subCats)
                    {
                        var subDict = _translationDB[sc];
                        if (subDict.TryGetValue(key, out result)) return true;
                        if (subDict.TryGetValue(normalizedKey, out result)) return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public static bool HasTerm(string category, string key)
        {
            return GetTerm(category, key, null) != null;
        }
    }

    internal static class RecursiveJsonParser
    {
        public static Dictionary<string, Dictionary<string, string>> Parse(string json)
        {
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(json)) return result;

            int index = 0;
            SkipWhitespace(json, ref index);

            if (index >= json.Length || json[index] != '{') return result;
            index++; // skip {

            while (index < json.Length)
            {
                SkipWhitespace(json, ref index);
                if (index >= json.Length || json[index] == '}') break;

                // Top Level Key (Category)
                string category = ParseString(json, ref index);
                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == ':') index++;
                SkipWhitespace(json, ref index);

                // Value must be an object (Category contents)
                if (index < json.Length && json[index] == '{')
                {
                    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    ParseObjectRecursive(json, ref index, dict);
                    result[category] = dict;
                }
                else
                {
                    // Skip invalid top level value
                    ParseValue(json, ref index);
                }

                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == ',') index++;
            }

            return result;
        }

        private static void ParseObjectRecursive(string json, ref int index, Dictionary<string, string> targetDict)
        {
            index++; // skip {
            while (index < json.Length)
            {
                SkipWhitespace(json, ref index);
                if (index >= json.Length || json[index] == '}')
                {
                    index++;
                    return;
                }

                string key = ParseString(json, ref index);
                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == ':') index++;
                SkipWhitespace(json, ref index);

                if (index < json.Length)
                {
                    if (json[index] == '{')
                    {
                        // Recursive: flatten nested object into current dict
                        ParseObjectRecursive(json, ref index, targetDict);
                    }
                    else if (json[index] == '\u0022')
                    {
                        // Leaf: String value
                        string val = ParseString(json, ref index);
                        targetDict[key] = val;
                    }
                    else
                    {
                        // Primitive/Array (ignore for localization strings)
                        ParseValue(json, ref index);
                    }
                }

                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == ',') index++;
            }
        }

        private static string ParseString(string json, ref int index)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (index < json.Length && json[index] == '\u0022') index++; // skip starting quote

            while (index < json.Length)
            {
                char c = json[index++];
                if (c == '\u0022') break;
                if (c == '\\')
                {
                    if (index < json.Length)
                    {
                        char next = json[index++];
                        switch (next)
                        {
                            case '\u0022': sb.Append('\u0022'); break;
                            case '\\': sb.Append('\\'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            default: sb.Append(next); break;
                        }
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static void ParseValue(string json, ref int index)
        {
            SkipWhitespace(json, ref index);
            if (index >= json.Length) return;

            char c = json[index];
            if (c == '{')
            {
                // Skip object depth
                int depth = 1;
                index++;
                while (index < json.Length && depth > 0)
                {
                    if (json[index] == '{') depth++;
                    else if (json[index] == '}') depth--;
                    index++;
                }
            }
            else if (c == '[')
            {
                // Skip array depth
                int depth = 1;
                index++;
                while (index < json.Length && depth > 0)
                {
                    if (json[index] == '[') depth++;
                    else if (json[index] == ']') depth--;
                    index++;
                }
            }
            else if (c == '\u0022')
            {
                ParseString(json, ref index);
            }
            else
            {
                // Number/Bool/Null: read until delimiter
                while (index < json.Length && !IsDelimiter(json[index]))
                {
                    index++;
                }
            }
        }

        private static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }
        }

        private static bool IsDelimiter(char c)
        {
            return c == ',' || c == '}' || c == ']' || char.IsWhiteSpace(c);
        }
    }
}
