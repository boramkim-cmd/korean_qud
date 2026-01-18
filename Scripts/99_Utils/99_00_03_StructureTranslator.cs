/*
 * 파일명: 99_00_03_StructureTranslator.cs
 * 분류: [Util] 구조화된 번역 데이터 처리기 (통합)
 * 역할: MUTATIONS, GENOTYPES, SUBTYPES 등의 폴더에 있는 구조화된 JSON(이름, 설명, 레벨텍스트)을 로드하고 번역을 제공합니다.
 * 기존 MutationTranslator를 대체합니다.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using XRL;

namespace QudKRTranslation.Utils
{
    public static class StructureTranslator
    {
        public class TranslationData
        {
            public string EnglishName { get; set; }
            public string KoreanName { get; set; }
            public string Description { get; set; }
            public string DescriptionKo { get; set; }
            public List<string> LevelText { get; set; }
            public List<string> LevelTextKo { get; set; }

            /// <summary>
            /// GetDescription() + "\n\n" + GetLevelText() 형식으로 조합 (한글 우선)
            /// fallbackOriginal이 제공되면, Description/DescriptionKo가 없을 때 이를 사용
            /// </summary>
            public string GetCombinedLongDescription(string fallbackOriginal = null)
            {
                // Use Korean Description if available
                if (!string.IsNullOrEmpty(DescriptionKo))
                    return CombineWithLevelText(DescriptionKo, LevelTextKo);

                // Use English Description if available
                if (!string.IsNullOrEmpty(Description))
                     return CombineWithLevelText(Description, LevelTextKo);

                // Fallback to original
                string desc = fallbackOriginal ?? "";
                
                // Filter out lines from desc that are present in LevelText (to avoid duplicates)
                if (!string.IsNullOrEmpty(desc) && LevelText != null && LevelText.Count > 0)
                {
                    var lines = desc.Split('\n').ToList();
                    var filteredLines = new List<string>();
                    
                    foreach (var line in lines)
                    {
                        bool isDuplicate = false;
                        string cleanLine = Unformat(line); // Remove color tags/bullets for comparison
                        
                        foreach (var matchText in LevelText)
                        {
                            if (string.IsNullOrEmpty(matchText)) continue;
                            
                            // Check if line contains the text (ignoring case? strictly?)
                            // Line usually has "{{c|ù}} " prefix. Match if the core text is present.
                            if (cleanLine.IndexOf(matchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                isDuplicate = true;
                                break;
                            }
                        }
                        
                        if (!isDuplicate)
                            filteredLines.Add(line);
                    }
                    
                    desc = string.Join("\n", filteredLines);
                }

                return CombineWithLevelText(desc, LevelTextKo);
            }

            private string CombineWithLevelText(string desc, List<string> levelText)
            {
                List<string> extras = (levelText != null && levelText.Count > 0) ? levelText : null;
                
                if (string.IsNullOrEmpty(desc))
                    return extras != null ? string.Join("\n", extras) : "";
                
                if (extras == null || extras.Count == 0)
                    return desc;
                
                return desc + "\n\n" + string.Join("\n", extras);
            }

            private static string Unformat(string text)
            {
                // Simple tag stripper: remove {{...}} sequences
                if (string.IsNullOrEmpty(text)) return "";
                return System.Text.RegularExpressions.Regex.Replace(text, @"\{\{[^}]+\}\}", "").Trim();
            }
        }

        private static Dictionary<string, TranslationData> _data = new Dictionary<string, TranslationData>(StringComparer.OrdinalIgnoreCase);
        private static bool _isLoaded = false;
        private static readonly string[] TargetDirectories = { "GAMEPLAY/MUTATIONS", "CHARGEN/GENOTYPES", "CHARGEN/SUBTYPES" };

        /// <summary>
        /// Lazy initialization - automatically finds and loads JSON files from target directories
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_isLoaded) return;

            try
            {
                string modPath = GetModDirectory();
                
                if (modPath != null)
                {
                    foreach (var dirName in TargetDirectories)
                    {
                        string dirPath = Path.Combine(modPath, "LOCALIZATION", dirName);
                        InitializeDirectory(dirPath);
                    }
                    _isLoaded = true;
                    Debug.Log($"[StructureTranslator] Loaded {_data.Count} entries from {string.Join(", ", TargetDirectories)}");
                }
                else
                {
                    Debug.LogWarning("[StructureTranslator] Could not find mod directory for auto-initialization");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[StructureTranslator] Auto-initialization failed: {e.Message}");
            }
        }

        private static string GetModDirectory()
        {
             // Option 1: Get from ModManager
            var mod = ModManager.GetMod("KoreanLocalization");
            if (mod != null && !string.IsNullOrEmpty(mod.Path))
            {
                return mod.Path;
            }
            
            // Option 2: OSX fallback
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(homeDir, "Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization");
            }
            return null;
        }

        public static void InitializeDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                // 폴더가 없는 것은 정상이므로 경고 없이 리턴 (아직 해당 카테고리 작업을 안했을 수 있음)
                return;
            }

            foreach (var jsonFile in Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories))
            {
                LoadFile(jsonFile);
            }
        }

        private static void LoadFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var itemData = ParseJson(json);
                
                if (itemData != null && !string.IsNullOrEmpty(itemData.EnglishName))
                {
                    _data[itemData.EnglishName] = itemData;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[StructureTranslator] Failed to load {Path.GetFileName(path)}: {e.Message}");
            }
        }

        private static TranslationData ParseJson(string json)
        {
            var data = new TranslationData();
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
                Debug.LogError($"[StructureTranslator] Parse error: {e.Message}");
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
                    
                    // Handle trailing comma
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

        public static bool TryGetData(string englishName, out TranslationData data)
        {
            EnsureInitialized();
            return _data.TryGetValue(englishName, out data);
        }

        public static string TranslateName(string englishName)
        {
            EnsureInitialized();
            if (_data.TryGetValue(englishName, out var data))
                return data.KoreanName;
            return englishName;
        }

        public static string TranslateLongDescription(string englishName, string fallbackOriginal = null)
        {
            EnsureInitialized();
            if (_data.TryGetValue(englishName, out var data))
                return data.GetCombinedLongDescription(fallbackOriginal);
                
            return fallbackOriginal ?? "";
        }
        
        /// <summary>
        /// 레벨 텍스트(extrainfo 등)만 리스트로 반환 (한글 우선)
        /// </summary>
        public static List<string> TranslateLevelText(string englishName)
        {
            EnsureInitialized();
            if (_data.TryGetValue(englishName, out var data))
            {
                if (data.LevelTextKo != null && data.LevelTextKo.Count > 0)
                    return data.LevelTextKo;
                return data.LevelText;
            }
            return null;
        }
    }
}
