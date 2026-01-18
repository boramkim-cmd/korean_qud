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

                // Fallback to original - but we need to filter out leveltext lines to avoid duplicates
                string desc = fallbackOriginal ?? "";
                
                // If we have LevelTextKo, filter out matching lines from desc to avoid English+Korean duplication
                if (!string.IsNullOrEmpty(desc) && LevelText != null && LevelText.Count > 0)
                {
                    var lines = desc.Split('\n').ToList();
                    var filteredLines = new List<string>();
                    
                    // Build a set of normalized leveltext entries for comparison
                    var normalizedLevelTexts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var lt in LevelText)
                    {
                        if (string.IsNullOrEmpty(lt)) continue;
                        normalizedLevelTexts.Add(NormalizeLine(lt));
                    }
                    
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            filteredLines.Add(line);
                            continue;
                        }
                        
                        string normalizedLine = NormalizeLine(line);
                        
                        // Check if this line matches any leveltext entry
                        bool isDuplicate = normalizedLevelTexts.Contains(normalizedLine);
                        
                        // If not found, also try partial match for lines with prefixes like "{{c|ù}}"
                        if (!isDuplicate)
                        {
                            foreach (var nlt in normalizedLevelTexts)
                            {
                                if (normalizedLine.Contains(nlt) || nlt.Contains(normalizedLine))
                                {
                                    isDuplicate = true;
                                    break;
                                }
                            }
                        }
                        
                        if (!isDuplicate)
                            filteredLines.Add(line);
                    }
                    
                    // 필터링된 라인들을 번역 시도 (평판 텍스트 등)
                    for (int i = 0; i < filteredLines.Count; i++)
                    {
                        string line = filteredLines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        
                        // LocalizationManager를 통해 번역 시도
                        if (QudKRTranslation.Core.LocalizationManager.TryGetAnyTerm(line, out string translated, "chargen_ui"))
                        {
                            filteredLines[i] = translated;
                        }
                        else
                        {
                            // 불렛 제거 후 다시 시도
                            string stripped = System.Text.RegularExpressions.Regex.Replace(line, @"^\{\{[a-zA-Z]\|[ùúûü]\}\}\s*", "");
                            stripped = System.Text.RegularExpressions.Regex.Replace(stripped, @"^[ùúûü·•]\s*", "");
                            if (!string.IsNullOrEmpty(stripped) && QudKRTranslation.Core.LocalizationManager.TryGetAnyTerm(stripped, out translated, "chargen_ui"))
                            {
                                // 불렛 복원
                                if (line.StartsWith("{{c|ù}}"))
                                    filteredLines[i] = "{{c|ù}} " + translated;
                                else if (line.StartsWith("ù"))
                                    filteredLines[i] = "ù " + translated;
                                else
                                    filteredLines[i] = translated;
                            }
                        }
                    }
                    
                    desc = string.Join("\n", filteredLines);
                }

                return CombineWithLevelText(desc, LevelTextKo);
            }
            
            /// <summary>
            /// Normalize a line for duplicate detection - strips tags, bullets, and normalizes stat format
            /// </summary>
            private static string NormalizeLine(string line)
            {
                if (string.IsNullOrEmpty(line)) return "";
                
                // 1. Strip color tags {{X|...}} -> content
                string result = System.Text.RegularExpressions.Regex.Replace(line, @"\{\{[a-zA-Z]\|([^}]+)\}\}", "$1");
                
                // 2. Remove bullet prefixes
                result = System.Text.RegularExpressions.Regex.Replace(result, @"^[ùúûü·•◦‣⁃]\s*", "");
                
                // 3. Normalize stat format: "+2 Toughness" <-> "Toughness +2"
                // Convert both to canonical form: "toughness 2" (text then number, no sign)
                var statMatch = System.Text.RegularExpressions.Regex.Match(result.Trim(), @"^([+-]?\d+)\s+(.+)$");
                if (statMatch.Success)
                {
                    string text = statMatch.Groups[2].Value.Trim().ToLowerInvariant();
                    string num = statMatch.Groups[1].Value.TrimStart('+');
                    result = text + " " + num;
                }
                else
                {
                    var reverseMatch = System.Text.RegularExpressions.Regex.Match(result.Trim(), @"^(.+)\s+([+-]?\d+)$");
                    if (reverseMatch.Success)
                    {
                        string text = reverseMatch.Groups[1].Value.Trim().ToLowerInvariant();
                        string num = reverseMatch.Groups[2].Value.TrimStart('+');
                        result = text + " " + num;
                    }
                    else
                    {
                        result = result.Trim().ToLowerInvariant();
                    }
                }
                
                return result;
            }

            private string CombineWithLevelText(string desc, List<string> levelText)
            {
                if (levelText == null || levelText.Count == 0)
                {
                    return string.IsNullOrEmpty(desc) ? "" : desc;
                }
                
                // LevelText 각 항목에 불렛 프리픽스 추가 (이미 있으면 스킵)
                var formattedExtras = new List<string>();
                foreach (var line in levelText)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    // 이미 불렛이 있는지 확인
                    bool hasBullet = line.StartsWith("{{c|ù}}") || 
                                     line.StartsWith("ù") || 
                                     line.StartsWith("•") ||
                                     line.StartsWith("·");
                    
                    if (hasBullet)
                    {
                        formattedExtras.Add(line);
                    }
                    else
                    {
                        // 불렛 추가
                        formattedExtras.Add("{{c|ù}} " + line);
                    }
                }
                
                if (string.IsNullOrEmpty(desc))
                    return string.Join("\n", formattedExtras);
                
                return desc + "\n\n" + string.Join("\n", formattedExtras);
            }


        }


        private static string Unformat(string text)
        {
            // Simple tag stripper: remove {{...}} sequences
            if (string.IsNullOrEmpty(text)) return "";
            return System.Text.RegularExpressions.Regex.Replace(text, @"\{\{[^}]+\}\}", "").Trim();
        }

        private static Dictionary<string, TranslationData> _data = new Dictionary<string, TranslationData>(StringComparer.OrdinalIgnoreCase);
        private static bool _isLoaded = false;
        private static readonly string[] TargetDirectories = { 
            "GAMEPLAY/MUTATIONS", 
            "CHARGEN/GENOTYPES", 
            "CHARGEN/SUBTYPES" 
        };

        /// <summary>
        /// Lazy initialization - automatically finds and loads JSON files from target directories
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_isLoaded) return;

            try
            {
                string modPath = QudKRTranslation.Core.LocalizationManager.GetModDirectory();
                
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
                        int q1 = trimmed.IndexOf('\u0022');
                        int q2 = trimmed.IndexOf('\u0022', q1 + 1);
                        int q3 = trimmed.IndexOf('\u0022', q2 + 1);
                        int q4 = trimmed.LastIndexOf('\u0022');
                        
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
                if (valuepart.StartsWith("\u0022"))
                {
                    int q1 = 0;
                    int q2 = valuepart.LastIndexOf('\u0022');
                    
                    // Handle trailing comma
                    if (valuepart.EndsWith("\","))
                        q2 = valuepart.Length - 2;
                    else if (valuepart.EndsWith("\u0022"))
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
            int q2 = trimmed.LastIndexOf('\u0022');
            
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
            
            // 1. Exact match
            if (_data.TryGetValue(englishName, out data)) return true;
            
            // 2. Normalized match (strip tags, trim)
            string normalized = Unformat(englishName);
            if (_data.TryGetValue(normalized, out data)) return true;
            
            // 3. Case-insensitive fallback (대소문자 무시)
            foreach (var kvp in _data)
            {
                if (kvp.Key.Equals(englishName, StringComparison.OrdinalIgnoreCase) ||
                    kvp.Key.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                {
                    data = kvp.Value;
                    return true;
                }
            }
            
            return false;
        }

        public static string TranslateName(string englishName)
        {
            EnsureInitialized();
            if (_data.TryGetValue(englishName, out var data))
                return data.KoreanName;
            return englishName;
        }

        public static string GetLongDescription(string englishName, string fallbackOriginal = null)
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
