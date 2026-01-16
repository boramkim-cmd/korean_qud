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
        
        private static string GetModDirectory()
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
                var data = SimpleJsonParser.Parse(json);

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

            // 1. 정확한 매칭
            if (_translationDB.TryGetValue(category, out var dict) && dict.TryGetValue(key, out string val))
                return val;

            // 2. 자동 서브 검색
            var subCats = _translationDB.Keys.Where(k => k.StartsWith(category + "_"));
            foreach (var cat in subCats)
            {
                if (_translationDB[cat].TryGetValue(key, out val)) return val;
            }

            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }

        public static bool TryGetAnyTerm(string key, out string result, params string[] categories)
        {
            if (!_isLoaded) Initialize();

            if (categories == null || categories.Length == 0)
            {
                foreach (var dict in _translationDB.Values)
                {
                    if (dict.TryGetValue(key, out result)) return true;
                }
            }
            else
            {
                foreach (var cat in categories)
                {
                    if (_translationDB.TryGetValue(cat, out var dict) && dict.TryGetValue(key, out result))
                        return true;

                    // 서브 카테고리 검색
                    string prefix = cat + "_";
                    var subCats = _translationDB.Keys.Where(k => k.StartsWith(prefix));
                    foreach (var sc in subCats)
                    {
                        if (_translationDB[sc].TryGetValue(key, out result)) return true;
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

    internal static class SimpleJsonParser
    {
        public static Dictionary<string, Dictionary<string, string>> Parse(string json)
        {
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                // 매우 단순한 형태의 JSON 파서 (카테고리 기반)
                string[] lines = json.Split('\n');
                string currentCategory = null;
                var currentDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (trimmed.EndsWith("{"))
                    {
                        int quote1 = trimmed.IndexOf('"');
                        int quote2 = trimmed.IndexOf('"', quote1 + 1);
                        if (quote1 >= 0 && quote2 > quote1)
                        {
                            currentCategory = trimmed.Substring(quote1 + 1, quote2 - quote1 - 1);
                            currentDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            result[currentCategory] = currentDict;
                        }
                    }
                    else if (trimmed.Contains("\":"))
                    {
                        int q1 = trimmed.IndexOf('"');
                        int q2 = trimmed.IndexOf('"', q1 + 1);
                        int q3 = trimmed.IndexOf('"', q2 + 1);
                        int q4 = trimmed.LastIndexOf('"');

                        if (q1 >= 0 && q2 > q1 && q3 > q2 && q4 > q3)
                        {
                            string key = trimmed.Substring(q1 + 1, q2 - q1 - 1);
                            string val = trimmed.Substring(q3 + 1, q4 - q3 - 1);
                            val = val.Replace("\\\"", "\"").Replace("\\\\", "\\");
                            if (currentCategory != null) result[currentCategory][key] = val;
                        }
                    }
                }
            }
            catch { }
            return result;
        }
    }
}
