/*
 * 파일명: LocalizationManager.cs
 * 분류: [Core] 통합 번역 관리자
 * 역할: LOCALIZATION 폴더 내의 모든 JSON 번역 데이터를 로드하고 관리합니다.
 *       기존 GlossaryLoader를 대체/확장하는 상위 개념의 매니저입니다.
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
        // 통합 번역 저장소: <Category, <Key, Value>>
        private static Dictionary<string, Dictionary<string, string>> _translationDB = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        
        private static bool _isLoaded = false;

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
            try
            {
                string modDirectory = GetModDirectory();
                if (string.IsNullOrEmpty(modDirectory))
                {
                    Debug.LogError("[LocalizationManager] 모드 디렉토리를 찾을 수 없습니다.");
                    return;
                }

                string locPath = Path.Combine(modDirectory, "LOCALIZATION");
                if (!Directory.Exists(locPath))
                {
                    Debug.LogWarning($"[LocalizationManager] LOCALIZATION 폴더 없음: {locPath}");
                    return;
                }

                // 모든 .json 파일 검색 (하위 폴더 포함 여부는 정책에 따라, 일단 루트만)
                string[] files = Directory.GetFiles(locPath, "*.json", SearchOption.TopDirectoryOnly);
                
                Debug.Log($"[LocalizationManager] {files.Length}개의 JSON 파일 발견. 로드 시작...");

                foreach (string file in files)
                {
                    LoadJsonFile(file);
                }

                Debug.Log($"[LocalizationManager] 로드 완료. 총 카테고리 수: {_translationDB.Count}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalizationManager] 초기화 중 치명적 오류: {ex.Message}");
            }
        }

        private static string GetModDirectory()
        {
            // 1. ModManager API 시도 (가장 정확)
            try
            {
                var mod = ModManager.GetMod("KoreanLocalization");
                if (mod != null && !string.IsNullOrEmpty(mod.Path)) return mod.Path;
            }
            catch { }

            // 2. Mods 루트 경로 계산
            string modsRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"),
                "com.FreeholdGames.CavesOfQud",
                "Mods"
            );

            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                 modsRoot = Path.Combine(homeDir, "Library/Application Support/com.FreeholdGames.CavesOfQud/Mods");
            }

            // 3. "KoreanLocalization" 폴더 확인 (기본)
            string targetPath = Path.Combine(modsRoot, "KoreanLocalization");
            if (Directory.Exists(targetPath)) return targetPath;

            // 4. Auto-Detect: 이름이 바뀌었을 경우를 대비해 LOCALIZATION 폴더를 가진 디렉토리 검색
            if (Directory.Exists(modsRoot))
            {
                try 
                {
                    foreach (var dir in Directory.GetDirectories(modsRoot))
                    {
                        if (Directory.Exists(Path.Combine(dir, "LOCALIZATION")))
                        {
                            Debug.Log($"[LocalizationManager] Auto-detected mod directory: {dir}");
                            return dir;
                        }
                    }
                }
                catch { }
            }
            
            return targetPath; // 없으면 기본 경로 반환 (에러 유도)
        }

        /// <summary>
        /// 용어집 키를 정규화합니다 (색상 태그 제거 + 특수문자 제거 + 소문자 변환).
        /// 이를 통해 게임에서 색상 태그가 제거된 텍스트로 검색해도 매칭됩니다.
        /// </summary>
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

        private static void LoadJsonFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var data = SimpleJsonParser.Parse(json); // 하단에 파서 구현

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
                        // 키 중복 시 덮어쓰기 (나중에 로드된 파일 우선)
                        _translationDB[category][termPair.Key] = termPair.Value;
                        
                        // 정규화된 키도 저장 (색상 태그 제거 + 소문자)
                        // 이를 통해 게임에서 "Most creatures..."로 검색해도
                        // 용어집의 "{{c|ù}} most creatures..." 항목과 매칭됩니다.
                        string normalizedKey = NormalizeKey(termPair.Key);
                        if (!string.IsNullOrEmpty(normalizedKey) && 
                            normalizedKey != termPair.Key.ToLowerInvariant() &&
                            !_translationDB[category].ContainsKey(normalizedKey))
                        {
                            _translationDB[category][normalizedKey] = termPair.Value;
                            
                            // [DEBUG] 정규화 키 저장 로그 (중요한 키만)
                            if (termPair.Key.Contains("bonus skill") || termPair.Key.Contains("putus"))
                            {
                                Debug.Log($"[LocalizationManager] 정규화 키 저장: [{category}] '{termPair.Key}' → '{normalizedKey}'");
                            }
                        }
                    }
                }
                Debug.Log($"[LocalizationManager] 파일 로드됨: {Path.GetFileName(path)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalizationManager] 파일 파싱 실패 ({Path.GetFileName(path)}): {ex.Message}");
            }
        }

        // ========================================================================
        // 조회 API
        // ========================================================================

        /// <summary>
        /// 특정 카테고리의 전체 번역 딕셔너리를 반환합니다. (ScopeManager 등에서 사용)
        /// 존재하지 않으면 null을 반환합니다.
        /// </summary>
        public static Dictionary<string, string> GetCategory(string category)
        {
            if (!_isLoaded) Initialize();
            
            if (_translationDB.TryGetValue(category, out var dict))
            {
                return dict;
            }
            return null;
        }

        /// <summary>
        /// 특정 카테고리의 특정 키에 대한 번역을 가져옵니다.
        /// </summary>
        public static string GetTerm(string category, string key, string fallback = "")
        {
            if (!_isLoaded) Initialize();

            if (_translationDB.TryGetValue(category, out var dict))
            {
                if (dict.TryGetValue(key, out string val)) return val;
            }
            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }

        /// <summary>
        /// 해당 용어가 존재하는지 확인합니다.
        /// </summary>
        public static bool HasTerm(string category, string key)
        {
            if (!_isLoaded) Initialize();

            if (_translationDB.TryGetValue(category, out var dict))
            {
                return dict.ContainsKey(key);
            }
            return false;
        }

        /// <summary>
        /// 여러 카테고리를 순차적으로 검색하여 번역을 찾습니다. (UI용)
        /// </summary>
        public static bool TryGetAnyTerm(string key, out string result, params string[] categories)
        {
            if (!_isLoaded) Initialize();
            
            // 카테고리 지정이 없으면 전체 검색 (성능 주의)
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
                    if (_translationDB.TryGetValue(cat, out var dict))
                    {
                        if (dict.TryGetValue(key, out result)) return true;
                    }
                }
            }

            result = null;
            return false;
        }
    }

    // ========================================================================
    // 내부 유틸리티: JSON 파서 (외부 라이브러리 의존성 제거를 위함)
    // ========================================================================
    internal static class SimpleJsonParser
    {
        public static Dictionary<string, Dictionary<string, string>> Parse(string json)
        {
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            
            // 매우 단순화된 파서: 정규식이나 문자열 처리로 구현 
            // * 주의: 복잡한 중첩 구조는 지원하지 않음. 오직 "Category": { "Key": "Value" } 형태만 지원
            
            json = json.Trim();
            // 주석 제거 등 전처리 필요할 수 있음
            
            // 1. 카테고리 분리
            // "key": { ... } 패턴 찾기
            // 실제로는 기존 GlossaryLoader의 파싱 로직을 재사용/개선
            
            // 기존 Parser 로직 재사용
            try
            {
                // 전체 중괄호 제거
                if (json.StartsWith("{")) json = json.Substring(1);
                if (json.EndsWith("}")) json = json.Substring(0, json.Length - 1);
                
                var lines = json.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                string currentCategory = null;
                Dictionary<string, string> currentDict = null;
                int braceDepth = 0;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//")) continue;

                    // { 개수 카운트 (매우 순진한 방식)
                    braceDepth += trimmed.Count(c => c == '{');
                    braceDepth -= trimmed.Count(c => c == '}');

                    // "Category": {
                    if (trimmed.Contains(": {") || (trimmed.EndsWith("{") && trimmed.Contains("\"")))
                    {
                        int colonIndex = trimmed.IndexOf(':');
                        if (colonIndex > 0)
                        {
                            currentCategory = trimmed.Substring(0, colonIndex).Trim().Trim('"');
                            currentDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            result[currentCategory] = currentDict;
                        }
                    }
                    else if (currentDict != null && trimmed.Contains(":"))
                    {
                        // "Key": "Value"
                        // 1. Key 추출
                        int keyStart = trimmed.IndexOf('"');
                        int keyEnd = -1;
                        
                        // 키 닫는 따옴표 찾기 (이스케이프 안 함 가정)
                        for (int i = keyStart + 1; i < trimmed.Length; i++)
                        {
                            if (trimmed[i] == '"' && trimmed[i-1] != '\\') 
                            {
                                keyEnd = i;
                                break;
                            }
                        }

                        if (keyStart >= 0 && keyEnd > keyStart)
                        {
                            string key = trimmed.Substring(keyStart + 1, keyEnd - keyStart - 1);
                            
                            // 2. 콜론 찾기 (키 뒤에서부터)
                            int colonIndex = trimmed.IndexOf(':', keyEnd + 1);
                            
                            if (colonIndex > 0)
                            {
                                // 3. Value 추출
                                int valStart = trimmed.IndexOf('"', colonIndex + 1);
                                int valEnd = trimmed.LastIndexOf('"'); 
                                // LastIndexOf는 끝에 콤마가 있을 경우 주의 필요.
                                // 가장 마지막 따옴표를 찾되, 콤마가 있다면 그 앞의 따옴표일 것임.
                                
                                // 더 정확하게: valStart 이후의 "를 찾음
                                if (valStart > 0 && valEnd > valStart)
                                {
                                    string value = trimmed.Substring(valStart + 1, valEnd - valStart - 1);
                                    
                                    // 이스케이프 문자 처리 (간단)
                                    value = value.Replace("\\\"", "\"").Replace("\\\\", "\\");
                                    
                                    currentDict[key] = value;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { /* Parsing Error */ }

            return result;
        }
    }
}
