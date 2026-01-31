/*
 * 파일명: ObjectTranslatorV2.cs
 * 분류: Facade
 * 역할: ObjectTranslator V2 Public API
 * 작성일: 2026-01-26
 *
 * 리팩토링된 ObjectTranslator의 Facade 클래스입니다.
 * 기존 ObjectTranslator와 동일한 API를 유지하면서 내부 구현은 완전히 새로운 아키텍처를 사용합니다.
 *
 * 아키텍처:
 * - Chain of Responsibility: 번역 파이프라인 (Cache -> DirectMatch -> PrefixSuffix -> Pattern -> Fallback)
 * - Strategy Pattern: 패턴 번역기 (Corpse, Food, Parts, Possessive, OfPattern)
 * - Repository Pattern: 데이터 접근 추상화 (JsonRepository)
 */

using System;
using System.Collections.Generic;
using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Data;
using QudKorean.Objects.V2.Pipeline;
using QudKorean.Objects.V2.Processing;

namespace QudKorean.Objects.V2
{
    /// <summary>
    /// Facade class providing the same public API as the original ObjectTranslator.
    /// Internally uses the refactored architecture with Chain of Responsibility,
    /// Strategy, and Repository patterns.
    /// </summary>
    public static class ObjectTranslatorV2
    {
        private const string LOG_PREFIX = "[QudKR-V2]";

        private static ITranslationRepository _repository;
        private static TranslationPipeline _pipeline;
        private static bool _initialized;

        // XML DisplayName → 한글 1:1 매핑 (빌드타임 생성, 컬러태그 포함 원문 key)
        private static Dictionary<string, string> _displayLookup;

        // 빠른 경로: blueprint → {영어이름 → 한글이름} 프리빌드 캐시
        // 고정 오브젝트(1838개)는 Dictionary 조회만으로 O(1) 번역
        private static Dictionary<string, Dictionary<string, string>> _fastCache;

        // 고정 오브젝트 블루프린트 집합 (데이터에 존재하는 모든 블루프린트)
        // 이 집합에 없는 블루프린트 = 절차적 생성물 → 파이프라인 실행
        // 이 집합에 있는데 빠른 캐시 미스 = 이름 변형 → 파이프라인 실행
        private static HashSet<string> _knownBlueprints;

        // 성능 카운터
        private static int _lookupHit;
        private static int _fastHit;
        private static int _fastSkip;
        private static int _pipelineFallback;
        private static int _totalCalls;

        // 핫스팟 감지: 블루프린트별 호출 횟수 (상위 반복 호출 추적)
        // 디버깅용 — EnableHotspotTracking = false로 프로덕션 오버헤드 제거
        internal static bool EnableHotspotTracking = false;
        private static Dictionary<string, int> _callFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static bool _hotspotWarned;

        #region Public API (Compatible with original ObjectTranslator)

        /// <summary>
        /// Ensures JSON data is loaded. Called automatically on first use.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (_initialized) return;

            try
            {
                Initialize();
                _initialized = true;
                UnityEngine.Debug.Log($"{LOG_PREFIX} Initialized with new architecture");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Reloads JSON files without restarting the game. Used by kr:reload wish command.
        /// </summary>
        public static void ReloadJson()
        {
            _repository?.Reload();
            TranslationContext.ClearCache();

            // Rebuild display lookup from reloaded repository
            var lookupData = _repository?.DisplayLookup;
            if (lookupData != null && lookupData.Count > 0)
                _displayLookup = new Dictionary<string, string>(lookupData, StringComparer.OrdinalIgnoreCase);

            // Rebuild fast cache
            BuildFastCache();

            UnityEngine.Debug.Log($"{LOG_PREFIX} Reloaded!");
        }

        /// <summary>
        /// Clears the display name cache. Called on game load/save.
        /// </summary>
        public static void ClearCache()
        {
            TranslationContext.ClearCache();
        }

        /// <summary>
        /// Attempts to get a translated display name for a blueprint.
        /// </summary>
        /// <param name="blueprint">The blueprint ID (e.g., "Bear", "Dagger")</param>
        /// <param name="originalName">The original English display name</param>
        /// <param name="translated">The translated Korean name, if found</param>
        /// <returns>True if translation was found</returns>
        public static bool TryGetDisplayName(string blueprint, string originalName, out string translated)
        {
            translated = null;
            if (string.IsNullOrEmpty(blueprint)) return false;

            EnsureInitialized();

            _totalCalls++;

            // 핫스팟 추적 (디버깅 전용 — 프로덕션에서는 EnableHotspotTracking = false)
            if (EnableHotspotTracking)
            {
                if (_callFrequency.TryGetValue(blueprint, out int freq))
                    _callFrequency[blueprint] = freq + 1;
                else
                    _callFrequency[blueprint] = 1;

                if (!_hotspotWarned && _totalCalls % 5000 == 0 && _totalCalls > 0)
                {
                    _hotspotWarned = true;
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"{LOG_PREFIX} Hotspot report at {_totalCalls} calls (Lookup:{_lookupHit} FastHit:{_fastHit} Skip:{_fastSkip} Pipeline:{_pipelineFallback}):");
                    var sorted = new List<KeyValuePair<string, int>>(_callFrequency);
                    sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
                    for (int i = 0; i < Math.Min(10, sorted.Count); i++)
                        sb.AppendLine($"  {sorted[i].Key}: {sorted[i].Value}x");
                    UnityEngine.Debug.Log(sb.ToString());
                }
            }

            // 최우선 경로: XML DisplayName 1:1 lookup (컬러태그 포함 원문 그대로 매칭)
            if (_displayLookup != null)
            {
                // 1) 원문 그대로 매칭
                if (_displayLookup.TryGetValue(originalName, out translated) && !string.IsNullOrEmpty(translated))
                {
                    _lookupHit++;
                    return true;
                }

                // 2) 컬러태그 strip 후 매칭
                string dlStripped = ColorTagProcessor.Strip(originalName);
                if (dlStripped != originalName && _displayLookup.TryGetValue(dlStripped, out translated) && !string.IsNullOrEmpty(translated))
                {
                    _lookupHit++;
                    return true;
                }

                // 3) suffix strip 후 매칭 (e.g. "torch x10 (unburnt)" → "torch")
                string dlInput = dlStripped != originalName ? dlStripped : originalName;
                string dlBase = StripSuffixFast(dlInput);
                if (dlBase != null && dlBase != dlInput && _displayLookup.TryGetValue(dlBase, out translated) && !string.IsNullOrEmpty(translated))
                {
                    string suffix = dlInput.Substring(dlBase.Length);
                    string suffixKo = SuffixExtractor.TranslateState(suffix, _repository);
                    translated = translated + suffixKo;
                    _lookupHit++;
                    return true;
                }

                // 4) 한글 베이스 + 영어 접미사 재진입 (e.g., "횃불 (unburnt)" → "횃불 (미사용)")
                string reentryInput = dlStripped != originalName ? dlStripped : originalName;
                string reentryBase = StripSuffixFast(reentryInput);
                if (reentryBase != null && reentryBase != reentryInput && ContainsKorean(reentryBase))
                {
                    string suffix = reentryInput.Substring(reentryBase.Length);
                    string suffixKo = SuffixExtractor.TranslateState(suffix, _repository);
                    if (suffixKo != suffix)
                    {
                        translated = reentryBase + suffixKo;
                        _lookupHit++;
                        return true;
                    }
                }
            }

            // 빠른 경로: 고정 오브젝트는 프리빌드 캐시에서 O(1) 조회
            if (_fastCache != null)
            {
                string normalizedBp = TextNormalizer.NormalizeBlueprintId(blueprint);
                if (_fastCache.TryGetValue(normalizedBp, out var nameMap))
                {
                    // 정확한 이름 매칭
                    if (nameMap.TryGetValue(originalName, out translated) && !string.IsNullOrEmpty(translated))
                    {
                        _fastHit++;
                        return true;
                    }

                    // 컬러태그 제거 후 매칭
                    string stripped = ColorTagProcessor.Strip(originalName);
                    if (stripped != originalName && nameMap.TryGetValue(stripped, out translated) && !string.IsNullOrEmpty(translated))
                    {
                        _fastHit++;
                        return true;
                    }

                    // 접미사(수량, 상태, 스탯) 제거 후 기본 이름으로 매칭 (Regex 없이)
                    string strippedInput = stripped != originalName ? stripped : originalName;
                    string baseName = StripSuffixFast(strippedInput);
                    if (baseName != null && nameMap.TryGetValue(baseName, out translated) && !string.IsNullOrEmpty(translated))
                    {
                        // 접미사 번역 후 재결합
                        string suffix = strippedInput.Substring(baseName.Length);
                        if (!string.IsNullOrEmpty(suffix))
                        {
                            string suffixKo = SuffixExtractor.TranslateState(suffix, _repository);
                            translated = translated + suffixKo;
                        }
                        _fastHit++;
                        return true;
                    }

                    // 고정 오브젝트인데 이름 변형이 다름 → 파이프라인으로 폴스루
                    _pipelineFallback++;
                    if (_pipelineFallback <= 50)
                        UnityEngine.Debug.Log($"{LOG_PREFIX} PipelineFallback: bp={blueprint} name=\"{originalName}\"");
                }
                else if (_knownBlueprints != null && !_knownBlueprints.Contains(normalizedBp))
                {
                    // 데이터에 없는 블루프린트 → GlobalNameIndex에서 display name으로 O(1) 조회 시도
                    string stripped = ColorTagProcessor.Strip(originalName);
                    string baseName = StripSuffixFast(stripped) ?? stripped;

                    if (_repository.GlobalNameIndex.TryGetValue(baseName, out translated) && !string.IsNullOrEmpty(translated))
                    {
                        // 접미사가 있었으면 번역 추가
                        if (baseName != stripped)
                        {
                            string suffix = stripped.Substring(baseName.Length);
                            string suffixKo = SuffixExtractor.TranslateState(suffix, _repository);
                            translated = translated + suffixKo;
                        }
                        _fastHit++;
                        return true;
                    }

                    // GlobalNameIndex에도 없음 → 스킵
                    _fastSkip++;
                    return false;
                }
            }

            // 느린 경로: 이름 변형이 있는 고정 오브젝트 또는 절차적 생성물 → 전체 파이프라인
            try
            {
                var context = new TranslationContext(_repository, blueprint, originalName);
                var result = _pipeline.Execute(context);

                translated = result.Translated;
                return result.Success;
            }
            catch (Exception ex)
            {
                LogTranslationError(blueprint, originalName, ex);
                return false;
            }
        }

        /// <summary>
        /// Attempts to get a translated description for a blueprint.
        /// </summary>
        public static bool TryGetDescription(string blueprint, out string translated)
        {
            translated = null;
            if (string.IsNullOrEmpty(blueprint)) return false;

            EnsureInitialized();

            string normalizedBlueprint = TextNormalizer.NormalizeBlueprintId(blueprint);
            var data = _repository.GetCreature(normalizedBlueprint) ??
                       _repository.GetItem(normalizedBlueprint) ??
                       _repository.GetCreature(blueprint) ??
                       _repository.GetItem(blueprint);

            if (data != null && !string.IsNullOrEmpty(data.DescriptionKo))
            {
                translated = data.DescriptionKo;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to translate a description ONLY if the current text matches the known English description.
        /// Use this to safely replace text in Tooltips without context.
        /// </summary>
        public static bool TryTranslateDescriptionExact(string blueprint, string currentText, out string translated)
        {
            translated = null;
            if (string.IsNullOrEmpty(blueprint) || string.IsNullOrEmpty(currentText)) return false;

            EnsureInitialized();

            string normalizedBlueprint = TextNormalizer.NormalizeBlueprintId(blueprint);
            var data = _repository.GetCreature(normalizedBlueprint) ??
                       _repository.GetItem(normalizedBlueprint) ??
                       _repository.GetCreature(blueprint) ??
                       _repository.GetItem(blueprint);

            if (data != null &&
                !string.IsNullOrEmpty(data.Description) &&
                !string.IsNullOrEmpty(data.DescriptionKo))
            {
                if (currentText.Trim().Equals(data.Description.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    translated = data.DescriptionKo;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a blueprint has any translation data.
        /// </summary>
        public static bool HasTranslation(string blueprint)
        {
            if (string.IsNullOrEmpty(blueprint)) return false;
            EnsureInitialized();
            return _repository.GetCreature(blueprint) != null ||
                   _repository.GetItem(blueprint) != null;
        }

        /// <summary>
        /// Gets statistics about loaded translations.
        /// </summary>
        public static string GetStats()
        {
            EnsureInitialized();
            string repoStats = _repository.GetStats();
            return $"{repoStats}, Cached: {TranslationContext.CacheCount}, LookupHit: {_lookupHit}, FastHit: {_fastHit}, FastSkip: {_fastSkip}, Pipeline: {_pipelineFallback}, Total: {_totalCalls}";
        }

        #endregion

        #region Internal Initialization

        private static void Initialize()
        {
            // Create repository
            _repository = new JsonRepository();
            ((JsonRepository)_repository).EnsureInitialized();

            // Create pipeline with default handlers
            _pipeline = TranslationPipeline.CreateDefault(_repository);

            // XML DisplayName → 한글 1:1 lookup 로드
            var lookupData = _repository.DisplayLookup;
            if (lookupData != null && lookupData.Count > 0)
            {
                _displayLookup = new Dictionary<string, string>(lookupData, StringComparer.OrdinalIgnoreCase);
                UnityEngine.Debug.Log($"{LOG_PREFIX} Display lookup loaded: {_displayLookup.Count} entries");
            }

            // 고정 오브젝트 빠른 캐시 프리빌드
            BuildFastCache();

        }

        private static void BuildFastCache()
        {
            _fastCache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            int count = 0;

            foreach (var data in _repository.AllCreatures)
            {
                if (data.Names != null && data.Names.Count > 0)
                {
                    string key = TextNormalizer.NormalizeBlueprintId(data.BlueprintId);
                    _fastCache[key] = new Dictionary<string, string>(data.Names, StringComparer.OrdinalIgnoreCase);
                    count++;
                }
            }

            foreach (var data in _repository.AllItems)
            {
                if (data.Names != null && data.Names.Count > 0)
                {
                    string key = TextNormalizer.NormalizeBlueprintId(data.BlueprintId);
                    _fastCache[key] = new Dictionary<string, string>(data.Names, StringComparer.OrdinalIgnoreCase);
                    count++;
                }
            }

            _knownBlueprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in _fastCache.Keys)
                _knownBlueprints.Add(key);

            UnityEngine.Debug.Log($"{LOG_PREFIX} Fast cache built: {count} blueprints preloaded, {_knownBlueprints.Count} known");
        }

        #endregion

        #region Fast Suffix Strip

        /// <summary>
        /// Regex 없이 접미사를 빠르게 제거.
        /// "torch x14 (unburnt)" → "torch"
        /// "leather cloak ◆0 ○1" → "leather cloak"
        /// "musket →8 ♥1d8 [empty]" → "musket"
        /// </summary>
        private static string StripSuffixFast(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length < 2) return null;

            // 끝에서부터 접미사 패턴 제거
            int end = name.Length;

            // 1. 괄호/대괄호 제거: (unburnt), [empty], [3 servings]
            while (end > 0)
            {
                char last = name[end - 1];
                if (last == ')')
                {
                    int open = name.LastIndexOf('(', end - 2);
                    if (open > 0) { end = open; while (end > 0 && name[end - 1] == ' ') end--; continue; }
                }
                if (last == ']')
                {
                    int open = name.LastIndexOf('[', end - 2);
                    if (open > 0) { end = open; while (end > 0 && name[end - 1] == ' ') end--; continue; }
                }
                break;
            }

            // 2. 수량 제거: x14, x3
            if (end > 2 && name[end - 1] >= '0' && name[end - 1] <= '9')
            {
                int i = end - 1;
                while (i > 0 && name[i - 1] >= '0' && name[i - 1] <= '9') i--;
                if (i > 0 && name[i - 1] == 'x' && (i < 2 || name[i - 2] == ' '))
                {
                    end = i - 1;
                    while (end > 0 && name[end - 1] == ' ') end--;
                }
            }

            // 3+4. 스탯 제거: 유니코드 마커(→♦○♥) 또는 평문 숫자 패턴을 통합 루프로 처리
            // "iron buckler ♦2 \t-3" → ♦2도 제거, "musket 8 1d8" → 둘 다 제거
            while (end > 2)
            {
                if (name[end - 1] >= '0' && name[end - 1] <= '9')
                {
                    int i = end - 1;
                    while (i > 0 && (char.IsDigit(name[i - 1]) || name[i - 1] == 'd'
                           || name[i - 1] == '+' || name[i - 1] == '-')) i--;
                    // 유니코드 스탯 마커 (→◆♦●○♥♠♣)
                    if (i > 0 && "→◆♦●○♥♠♣".IndexOf(name[i - 1]) >= 0)
                    {
                        end = i - 1;
                        while (end > 0 && char.IsWhiteSpace(name[end - 1])) end--;
                        continue;
                    }
                    // 평문 스탯 (컬러태그 strip 후 남은 숫자)
                    if (i > 0 && i < end && char.IsWhiteSpace(name[i - 1]))
                    {
                        end = i - 1;
                        while (end > 0 && char.IsWhiteSpace(name[end - 1])) end--;
                        continue;
                    }
                }
                break;
            }

            if (end <= 0 || end == name.Length) return null;
            return name.Substring(0, end).Trim();
        }

        #endregion

        #region Helpers

        private static bool ContainsKorean(string s)
        {
            for (int i = 0; i < s.Length; i++)
                if (s[i] >= '\uAC00' && s[i] <= '\uD7AF') return true;
            return false;
        }

        #endregion

        #region Error Logging

        /// <summary>
        /// Logs translation errors with source location information for debugging.
        /// </summary>
        private static void LogTranslationError(string blueprint, string originalName, Exception ex)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{LOG_PREFIX} Translation error");
            sb.AppendLine($"  Blueprint: {blueprint}");
            sb.AppendLine($"  Original: {originalName}");

            // Get source information from repository if available
            var jsonRepo = _repository as JsonRepository;
            if (jsonRepo != null)
            {
                var source = jsonRepo.GetSourceInfo(blueprint);
                if (source != null)
                {
                    sb.AppendLine($"  Source: {source.File}:{source.Line}");
                    if (!string.IsNullOrEmpty(source.Category))
                        sb.AppendLine($"  Category: {source.Category}");
                    if (!string.IsNullOrEmpty(source.Pattern))
                        sb.AppendLine($"  Pattern: {source.Pattern}");
                    if (source.Prefixes != null && source.Prefixes.Count > 0)
                        sb.AppendLine($"  Prefixes: [{string.Join(", ", source.Prefixes)}]");
                    if (!string.IsNullOrEmpty(source.BaseNoun))
                        sb.AppendLine($"  BaseNoun: {source.BaseNoun}");
                }
                else
                {
                    sb.AppendLine($"  Source: (not in sourcemap - check if blueprint exists)");
                }
            }

            sb.AppendLine($"  Error: {ex.Message}");

            UnityEngine.Debug.LogError(sb.ToString());
        }

        #endregion
    }
}
