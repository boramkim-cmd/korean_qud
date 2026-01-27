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
            return $"{repoStats}, Cached: {TranslationContext.CacheCount}";
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
