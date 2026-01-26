/*
 * 파일명: TranslationResult.cs
 * 분류: Core - Data Object
 * 역할: 번역 결과 객체
 * 작성일: 2026-01-26
 */

namespace QudKorean.Objects.V2.Core
{
    /// <summary>
    /// Represents the result of a translation operation.
    /// </summary>
    public class TranslationResult
    {
        /// <summary>
        /// Whether the translation was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// The translated text (null if translation failed).
        /// </summary>
        public string Translated { get; }

        /// <summary>
        /// The name of the handler that produced this result.
        /// </summary>
        public string HandlerName { get; }

        private TranslationResult(bool success, string translated, string handlerName)
        {
            Success = success;
            Translated = translated;
            HandlerName = handlerName;
        }

        /// <summary>
        /// Creates a successful translation result.
        /// </summary>
        public static TranslationResult Hit(string translated, string handler)
        {
            return new TranslationResult(true, translated, handler);
        }

        /// <summary>
        /// Creates a failed translation result (no translation found).
        /// </summary>
        public static TranslationResult Miss()
        {
            return new TranslationResult(false, null, null);
        }

        /// <summary>
        /// Creates a partial translation result (some parts translated).
        /// Used when only partial translation is possible.
        /// </summary>
        public static TranslationResult Partial(string partialResult, string handler)
        {
            return new TranslationResult(true, partialResult, handler);
        }
    }
}
