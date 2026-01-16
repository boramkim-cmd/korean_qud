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
        private static readonly string GLOSSARY_PATH = "Mods/KoreanLocalization/LOCALIZATION/glossary.json";

        /// <summary>
        /// 용어집 로드 (최초 1회만)
        /// </summary>
        public static void LoadGlossary()
        {
            if (_glossary != null) return;

            try
            {
                string fullPath = Path.Combine(Application.dataPath, GLOSSARY_PATH);
                
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"[GlossaryLoader] 용어집 파일을 찾을 수 없습니다: {fullPath}");
                    _glossary = new Dictionary<string, object>();
                    return;
                }

                string json = File.ReadAllText(fullPath);
                _glossary = JsonUtility.FromJson<Dictionary<string, object>>(json);
                
                Debug.Log($"[GlossaryLoader] 용어집 로드 완료: {_glossary.Count}개 카테고리");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GlossaryLoader] 용어집 로드 실패: {ex.Message}");
                _glossary = new Dictionary<string, object>();
            }
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
