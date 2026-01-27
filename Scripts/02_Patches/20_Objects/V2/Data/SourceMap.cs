/*
 * 파일명: SourceMap.cs
 * 분류: Data - Source Tracking
 * 역할: 빌드된 번들에서 원본 소스 파일/라인 추적
 * 작성일: 2026-01-27
 *
 * 빌드 시 생성된 sourcemap.json을 로드하여
 * 런타임 에러 발생 시 원본 파일 위치를 제공합니다.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace QudKorean.Objects.V2.Data
{
    /// <summary>
    /// 소스 위치 정보
    /// </summary>
    public class SourceInfo
    {
        /// <summary>원본 파일 경로 (LOCALIZATION/...)</summary>
        public string File { get; set; }

        /// <summary>원본 파일 내 라인 번호</summary>
        public int Line { get; set; }

        /// <summary>카테고리 (creatures, items, etc.)</summary>
        public string Category { get; set; }

        /// <summary>분석된 패턴 (prefix+noun 등)</summary>
        public string Pattern { get; set; }

        /// <summary>분석된 접두사 목록</summary>
        public List<string> Prefixes { get; set; }

        /// <summary>분석된 기본 명사</summary>
        public string BaseNoun { get; set; }

        /// <summary>한글 번역 (어휘의 경우)</summary>
        public string Korean { get; set; }

        /// <summary>포맷된 위치 문자열 (File:Line)</summary>
        public string Location => $"{File}:{Line}";

        public override string ToString()
        {
            return $"{File}:{Line} [{Category}] {Pattern ?? "direct"}";
        }
    }

    /// <summary>
    /// 소스맵 관리자
    /// </summary>
    public class SourceMap
    {
        private const string LOG_PREFIX = "[QudKR-SourceMap]";

        private Dictionary<string, SourceInfo> _blueprints;
        private Dictionary<string, SourceInfo> _vocabulary;
        private string _sourceDir;
        private DateTime _buildTime;
        private bool _loaded;

        public bool IsLoaded => _loaded;
        public string SourceDirectory => _sourceDir;
        public DateTime BuildTime => _buildTime;
        public int BlueprintCount => _blueprints?.Count ?? 0;
        public int VocabularyCount => _vocabulary?.Count ?? 0;

        /// <summary>
        /// 소스맵 파일 로드
        /// </summary>
        /// <param name="path">sourcemap.json 파일 경로</param>
        public void Load(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                UnityEngine.Debug.LogWarning($"{LOG_PREFIX} SourceMap not found: {path}");
                return;
            }

            try
            {
                var root = JObject.Parse(System.IO.File.ReadAllText(path));

                // 메타데이터
                var meta = root["_meta"] as JObject;
                if (meta != null)
                {
                    _sourceDir = meta["sourceDir"]?.ToString();
                    if (DateTime.TryParse(meta["buildTime"]?.ToString(), out var bt))
                        _buildTime = bt;
                }

                // 블루프린트 로드
                _blueprints = new Dictionary<string, SourceInfo>(StringComparer.OrdinalIgnoreCase);
                var blueprints = root["blueprints"] as JObject;
                if (blueprints != null)
                {
                    foreach (var prop in blueprints.Properties())
                    {
                        var entry = prop.Value as JObject;
                        if (entry == null) continue;

                        var info = new SourceInfo
                        {
                            File = entry["file"]?.ToString(),
                            Line = entry["line"]?.Value<int>() ?? 0,
                            Category = entry["category"]?.ToString(),
                        };

                        // analysis 섹션
                        var analysis = entry["analysis"] as JObject;
                        if (analysis != null)
                        {
                            info.Pattern = analysis["pattern"]?.ToString();
                            info.BaseNoun = analysis["baseNoun"]?.ToString();

                            var prefixes = analysis["prefixes"] as JArray;
                            if (prefixes != null)
                            {
                                info.Prefixes = new List<string>();
                                foreach (var p in prefixes)
                                    info.Prefixes.Add(p.ToString());
                            }
                        }

                        _blueprints[prop.Name] = info;
                    }
                }

                // 어휘 로드
                _vocabulary = new Dictionary<string, SourceInfo>(StringComparer.OrdinalIgnoreCase);
                var vocabulary = root["vocabulary"] as JObject;
                if (vocabulary != null)
                {
                    foreach (var prop in vocabulary.Properties())
                    {
                        var entry = prop.Value as JObject;
                        if (entry == null) continue;

                        _vocabulary[prop.Name] = new SourceInfo
                        {
                            File = entry["file"]?.ToString(),
                            Line = entry["line"]?.Value<int>() ?? 0,
                            Korean = entry["korean"]?.ToString(),
                        };
                    }
                }

                _loaded = true;
                UnityEngine.Debug.Log($"{LOG_PREFIX} Loaded: {_blueprints.Count} blueprints, {_vocabulary.Count} vocabulary");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Failed to load: {ex.Message}");
            }
        }

        /// <summary>
        /// 블루프린트의 소스 정보 조회
        /// </summary>
        public SourceInfo GetBlueprintSource(string blueprintId)
        {
            if (_blueprints != null && _blueprints.TryGetValue(blueprintId, out var info))
                return info;
            return null;
        }

        /// <summary>
        /// 어휘의 소스 정보 조회
        /// </summary>
        public SourceInfo GetVocabularySource(string term)
        {
            if (_vocabulary != null && _vocabulary.TryGetValue(term, out var info))
                return info;
            return null;
        }

        /// <summary>
        /// 에러 로그용 포맷된 소스 정보 생성
        /// </summary>
        public string FormatErrorSource(string blueprintId, string term = null)
        {
            var parts = new List<string>();

            var bpInfo = GetBlueprintSource(blueprintId);
            if (bpInfo != null)
            {
                parts.Add($"Blueprint: {bpInfo.Location}");
                if (!string.IsNullOrEmpty(bpInfo.Category))
                    parts.Add($"Category: {bpInfo.Category}");
                if (!string.IsNullOrEmpty(bpInfo.Pattern))
                    parts.Add($"Pattern: {bpInfo.Pattern}");
                if (bpInfo.Prefixes != null && bpInfo.Prefixes.Count > 0)
                    parts.Add($"Prefixes: [{string.Join(", ", bpInfo.Prefixes)}]");
                if (!string.IsNullOrEmpty(bpInfo.BaseNoun))
                    parts.Add($"BaseNoun: {bpInfo.BaseNoun}");
            }

            if (!string.IsNullOrEmpty(term))
            {
                var vocabInfo = GetVocabularySource(term);
                if (vocabInfo != null)
                    parts.Add($"Vocabulary '{term}': {vocabInfo.Location}");
            }

            return parts.Count > 0 ? string.Join("\n  ", parts) : "Source unknown";
        }
    }
}
