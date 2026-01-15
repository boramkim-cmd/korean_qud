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
            // 1. ModManager API 시도
            try
            {
                var mod = ModManager.GetMod("KoreanLocalization");
                if (mod != null && !string.IsNullOrEmpty(mod.Path)) return mod.Path;
            }
            catch { }

            // 2. Fallback: 수동 경로 찾기
            string userPath = XRL.XRLCore.CorePath; // 보통 Save 경로 등
            // * 정확한 경로는 기존 GlossaryLoader의 로직 재사용
             string modsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"),
                "com.FreeholdGames.CavesOfQud",
                "Mods",
                "KoreanLocalization"
            );

            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                 modsPath = Path.Combine(homeDir, "Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization");
            }
            
            return modsPath;
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
                        int firstQuote = trimmed.IndexOf('"');
                        int secondQuote = trimmed.IndexOf('"', firstQuote + 1);
                        // Value 찾기 (이스케이프 문자 처리 등은 생략된 기본형)
                        int partsIndex = trimmed.IndexOf(':');
                        int thirdQuote = trimmed.IndexOf('"', partsIndex + 1);
                        int fourthQuote = trimmed.LastIndexOf('"'); // 마지막 따옴표

                        if (firstQuote >= 0 && secondQuote > firstQuote && thirdQuote > partsIndex && fourthQuote > thirdQuote)
                        {
                            string key = trimmed.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
                            string value = trimmed.Substring(thirdQuote + 1, fourthQuote - thirdQuote - 1);
                            
                            // 콤마 제거
                            // (위 로직은 파싱이 취약할 수 있으므로, 키/값을 정확히 추출하는 것이 중요)
                            
                            currentDict[key] = value;
                        }
                    }
                }
            }
            catch (Exception) { /* Parsing Error */ }

            return result;
        }
    }
}
