/*
 * 파일명: GlossaryLoader.cs
 * 분류: [Core] 용어집 로더
 * 역할: JSON 용어집을 로드하여 번역에 사용합니다.
 * 작성일: 2026-01-15
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QudKRTranslation.Core
{
    /// <summary>
    /// JSON 기반 용어집 로더
    /// </summary>
    public static class GlossaryLoader
    {
        private static Dictionary<string, object> _glossary = null;

        /// <summary>
        /// 용어집 로드 (최초 1회만)
        /// </summary>
        public static void LoadGlossary()
        {
            if (_glossary != null) return;

            try
            {
                // 1. 공식 API 사용 (XRL.ModManager.GetMod)
                // 소스 코드 분석 결과: XRL.ModManager.GetMod(string ID)가 ModInfo를 반환하며, ModInfo에 Path 속성이 있음.
                string modPath = null;
                var modInfo = XRL.ModManager.GetMod("KoreanLocalization");
                
                if (modInfo != null)
                {
                    modPath = modInfo.Path;
                    Debug.Log($"[GlossaryLoader] ModManager API로 경로 확인: {modPath}");
                }
                
                // 2. API 실패 시 Fallback (직접 경로 구성)
                if (string.IsNullOrEmpty(modPath))
                {
                    Debug.LogWarning("[GlossaryLoader] ModManager.GetMod 실패, 수동 경로 탐색 시도");
                    
                    string modsPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"),
                        "com.FreeholdGames.CavesOfQud",
                        "Mods",
                        "KoreanLocalization"
                    );
                    
                    if (Application.platform == RuntimePlatform.OSXPlayer)
                    {
                        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                        modsPath = Path.Combine(
                            homeDir,
                            "Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization"
                        );
                    }
                    
                    if (Directory.Exists(modsPath))
                    {
                        modPath = modsPath;
                    }
                }

                if (string.IsNullOrEmpty(modPath))
                {
                    Debug.LogWarning("[GlossaryLoader] 모드 폴더를 찾을 수 없습니다");
                    _glossary = new Dictionary<string, object>();
                    return;
                }

                // local path/Mod/KoreanLocalization/LOCALIZATION/glossary.json
                string fullPath = Path.Combine(modPath, "LOCALIZATION", "glossary.json");
                
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"[GlossaryLoader] 용어집 파일을 찾을 수 없습니다: {fullPath}");
                    _glossary = new Dictionary<string, object>();
                    return;
                }

                string json = File.ReadAllText(fullPath);
                
                // 간단한 수동 JSON 파싱
                _glossary = ParseGlossaryJson(json);
                
                Debug.Log($"[GlossaryLoader] 용어집 로드 완료: {fullPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GlossaryLoader] 용어집 로드 실패: {ex.Message}\n{ex.StackTrace}");
                _glossary = new Dictionary<string, object>();
            }
        }
        
        /// <summary>
        /// Glossary JSON 파싱 (개선된 버전)
        /// </summary>
        private static Dictionary<string, object> ParseGlossaryJson(string json)
        {
            var result = new Dictionary<string, object>();
            
            try
            {
                json = json.Trim();
                if (!json.StartsWith("{") || !json.EndsWith("}"))
                {
                    Debug.LogError("[GlossaryLoader] Invalid JSON format");
                    return result;
                }
                
                json = json.Substring(1, json.Length - 2).Trim();
                
                string currentCategory = null;
                Dictionary<string, object> currentDict = null;
                int braceDepth = 0;
                
                var lines = json.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;
                    
                    // 중괄호 카운팅
                    foreach (char c in trimmed)
                    {
                        if (c == '{') braceDepth++;
                        else if (c == '}') braceDepth--;
                    }
                    
                    // 카테고리 시작: "category": {
                    if (trimmed.Contains(": {") && braceDepth == 1)
                    {
                        int colonIndex = trimmed.IndexOf(':');
                        if (colonIndex > 0)
                        {
                            currentCategory = trimmed.Substring(0, colonIndex).Trim().Trim('"');
                            currentDict = new Dictionary<string, object>();
                            result[currentCategory] = currentDict;
                        }
                    }
                    // 카테고리 종료
                    else if (trimmed.StartsWith("}") && braceDepth == 0)
                    {
                        currentCategory = null;
                        currentDict = null;
                    }
                    // key-value 파싱
                    else if (currentDict != null && braceDepth == 1 && trimmed.Contains(":"))
                    {
                        // "key": "value" 파싱
                        int firstQuote = trimmed.IndexOf('"');
                        int secondQuote = trimmed.IndexOf('"', firstQuote + 1);
                        int thirdQuote = trimmed.IndexOf('"', secondQuote + 1);
                        int fourthQuote = trimmed.IndexOf('"', thirdQuote + 1);
                        
                        if (firstQuote >= 0 && secondQuote > firstQuote && 
                            thirdQuote > secondQuote && fourthQuote > thirdQuote)
                        {
                            string key = trimmed.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
                            string value = trimmed.Substring(thirdQuote + 1, fourthQuote - thirdQuote - 1);
                            currentDict[key] = value;
                        }
                    }
                }
                
                Debug.Log($"[GlossaryLoader] 파싱 완료: {result.Count}개 카테고리");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GlossaryLoader] JSON 파싱 오류: {ex.Message}\n{ex.StackTrace}");
            }
            
            return result;
        }

        /// <summary>
        /// 용어 가져오기
        /// </summary>
        /// <param name="category">카테고리 (예: "attributes")</param>
        /// <param name="key">키 (예: "strength")</param>
        /// <param name="fallback">기본값</param>
        /// <returns>번역된 용어</returns>
        public static string GetTerm(string category, string key, string fallback = "")
        {
            if (_glossary == null) LoadGlossary();

            try
            {
                if (_glossary.ContainsKey(category))
                {
                    var categoryDict = _glossary[category] as Dictionary<string, object>;
                    if (categoryDict != null && categoryDict.ContainsKey(key))
                    {
                        return categoryDict[key].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GlossaryLoader] 용어 조회 실패 ({category}.{key}): {ex.Message}");
            }

            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }

        /// <summary>
        /// 용어 존재 여부 확인
        /// </summary>
        public static bool HasTerm(string category, string key)
        {
            if (_glossary == null) LoadGlossary();

            try
            {
                if (_glossary.ContainsKey(category))
                {
                    var categoryDict = _glossary[category] as Dictionary<string, object>;
                    return categoryDict != null && categoryDict.ContainsKey(key);
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 용어집 리로드 (개발/디버깅용)
        /// </summary>
        public static void ReloadGlossary()
        {
            _glossary = null;
            LoadGlossary();
        }
    }
}
