/*
 * 파일명: 99_00_03_MutationTranslator.cs
 * 분류: [Util] Mutation 전용 번역기
 * 역할: description + leveltext 구조의 mutation JSON을 로드하고 번역 제공
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QudKRTranslation.Utils
{
    public static class MutationTranslator
    {
        public class MutationData
        {
            public string EnglishName { get; set; }
            public string KoreanName { get; set; }
            public string Description { get; set; }
            public string DescriptionKo { get; set; }
            public List<string> LevelText { get; set; }
            public List<string> LevelTextKo { get; set; }

            /// <summary>
            /// GetDescription() + "\n\n" + GetLevelText() 형식으로 조합 (한글 우선)
            /// </summary>
            public string GetCombinedLongDescription()
            {
                // Use Korean if available, fallback to English
                string desc = !string.IsNullOrEmpty(DescriptionKo) ? DescriptionKo : Description;
                List<string> leveltext = (LevelTextKo != null && LevelTextKo.Count > 0) ? LevelTextKo : LevelText;
                
                if (string.IsNullOrEmpty(desc))
                    return leveltext != null ? string.Join("\n", leveltext) : "";
                
                if (leveltext == null || leveltext.Count == 0)
                    return desc;
                
                return desc + "\n\n" + string.Join("\n", leveltext);
            }
        }

        private static Dictionary<string, MutationData> _mutations = new Dictionary<string, MutationData>(StringComparer.OrdinalIgnoreCase);
        private static bool _isLoaded = false;

        /// <summary>
        /// Lazy initialization - automatically finds and loads mutation JSON files
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_isLoaded) return;

            try
            {
                // Try to find mod directory
                string modPath = null;
                
                // Option 1: Get from ModManager
                var mod = XRL.ModManager.GetMod("KoreanLocalization");
                if (mod != null && !string.IsNullOrEmpty(mod.Path))
                {
                    modPath = mod.Path;
                }
                
                // Option 2: OSX fallback
                if (modPath == null && Application.platform == RuntimePlatform.OSXPlayer)
                {
                    string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    modPath = Path.Combine(homeDir, "Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization");
                }
                
                if (modPath != null)
                {
                    string mutationsDir = Path.Combine(modPath, "LOCALIZATION", "MUTATIONS");
                    Initialize(mutationsDir);
                }
                else
                {
                    Debug.LogWarning("[MutationTranslator] Could not find mod directory for auto-initialization");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MutationTranslator] Auto-initialization failed: {e.Message}");
            }
        }

        public static void Initialize(string mutationsDirectory)
        {
            if (_isLoaded) return;

            if (!Directory.Exists(mutationsDirectory))
            {
                Debug.LogWarning($"[MutationTranslator] Directory not found: {mutationsDirectory}");
                return;
            }

            foreach (var jsonFile in Directory.GetFiles(mutationsDirectory, "*.json", SearchOption.AllDirectories))
            {
                LoadMutationFile(jsonFile);
            }

            _isLoaded = true;
            Debug.Log($"[MutationTranslator] Loaded {_mutations.Count} mutations");
        }

        private static void LoadMutationFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var mutData = ParseMutationJson(json);
                
                if (mutData != null && !string.IsNullOrEmpty(mutData.EnglishName))
                {
                    _mutations[mutData.EnglishName] = mutData;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MutationTranslator] Failed to load {Path.GetFileName(path)}: {e.Message}");
            }
        }

        private static MutationData ParseMutationJson(string json)
        {
            var data = new MutationData();
            var levelTextList = new List<string>();
            var levelTextKoList = new List<string>();
            
            try
            {
                string[] lines = json.Split('\n');
                string currentSection = null;
                
                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    
                    // Section headers
                    if (trimmed.Contains("\"names\":"))
                    {
                        currentSection = "names";
                    }
                    else if (trimmed.Contains("\"description_ko\":"))
                    {
                        currentSection = "description_ko";
                        data.DescriptionKo = ExtractStringValue(trimmed);
                    }
                    else if (trimmed.Contains("\"description\":"))
                    {
                        currentSection = "description";
                        data.Description = ExtractStringValue(trimmed);
                    }
                    else if (trimmed.Contains("\"leveltext_ko\":"))
                    {
                        currentSection = "leveltext_ko";
                    }
                    else if (trimmed.Contains("\"leveltext\":"))
                    {
                        currentSection = "leveltext";
                    }
                    // Parse entries
                    else if (currentSection == "names" && trimmed.Contains("\":"))
                    {
                        // "English Name": "한글 이름"
                        int q1 = trimmed.IndexOf('\"');
                        int q2 = trimmed.IndexOf('\"', q1 + 1);
                        int q3 = trimmed.IndexOf('\"', q2 + 1);
                        int q4 = trimmed.LastIndexOf('\"');
                        
                        if (q1 >= 0 && q2 > q1 && q3 > q2 && q4 > q3)
                        {
                            data.EnglishName = trimmed.Substring(q1 + 1, q2 - q1 - 1);
                            data.KoreanName = Unescape(trimmed.Substring(q3 + 1, q4 - q3 - 1));
                        }
                    }
                    else if (currentSection == "leveltext" && trimmed.StartsWith("\""))
                    {
                        string levelLine = ExtractArrayItem(trimmed);
                        if (levelLine != null) levelTextList.Add(levelLine);
                    }
                    else if (currentSection == "leveltext_ko" && trimmed.StartsWith("\""))
                    {
                        string levelLine = ExtractArrayItem(trimmed);
                        if (levelLine != null) levelTextKoList.Add(levelLine);
                    }
                }
                
                data.LevelText = levelTextList;
                data.LevelTextKo = levelTextKoList.Count > 0 ? levelTextKoList : null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MutationTranslator] Parse error: {e.Message}");
            }
            
            return data;
        }

        private static string ExtractStringValue(string trimmed)
        {
            int colonIdx = trimmed.IndexOf(':');
            if (colonIdx >= 0)
            {
                string valuepart = trimmed.Substring(colonIdx + 1).Trim();
                if (valuepart.StartsWith("\""))
                {
                    int q1 = 0;
                    int q2 = valuepart.LastIndexOf('\"');
                    if (valuepart.EndsWith("\","))
                        q2 = valuepart.Length - 2;
                    else if (valuepart.EndsWith("\""))
                        q2 = valuepart.Length - 1;
                    
                    if (q2 > 0)
                    {
                        return Unescape(valuepart.Substring(q1 + 1, q2 - q1 - 1));
                    }
                }
            }
            return null;
        }

        private static string ExtractArrayItem(string trimmed)
        {
            int q1 = 0;
            int q2 = trimmed.LastIndexOf('\"');
            
            if (trimmed.EndsWith("\","))
                q2 = trimmed.LastIndexOf('\"', trimmed.Length - 2);
                
            if (q2 > 0)
            {
                return Unescape(trimmed.Substring(q1 + 1, q2 - q1 - 1));
            }
            return null;
        }

        private static string Unescape(string text)
        {
            return text.Replace("\\\"", "\"").Replace("\\\\", "\\").Replace("\\n", "\n");
        }

        public static bool TryGetMutation(string englishName, out MutationData data)
        {
            EnsureInitialized();
            return _mutations.TryGetValue(englishName, out data);
        }

        public static string TranslateName(string englishName)
        {
            EnsureInitialized();
            if (_mutations.TryGetValue(englishName, out var data))
                return data.KoreanName;
            return englishName;
        }

        public static string TranslateLongDescription(string englishName)
        {
            EnsureInitialized();
            if (_mutations.TryGetValue(englishName, out var data))
                return data.GetCombinedLongDescription();
            return "";
        }
    }
}
