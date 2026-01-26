/*
 * 파일명: ITranslationHandler.cs
 * 분류: Pipeline - Interface
 * 역할: Chain of Responsibility 핸들러 인터페이스
 * 작성일: 2026-01-26
 */

using QudKorean.Objects.V2.Core;

namespace QudKorean.Objects.V2.Pipeline
{
    /// <summary>
    /// Handler interface for Chain of Responsibility pattern.
    /// Each handler tries to translate the input, passing to the next handler if unsuccessful.
    /// </summary>
    public interface ITranslationHandler
    {
        /// <summary>
        /// The name of this handler (for debugging and logging).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The next handler in the chain.
        /// </summary>
        ITranslationHandler Next { get; set; }

        /// <summary>
        /// Handles the translation request.
        /// Returns a successful result if this handler can translate, otherwise passes to next handler.
        /// </summary>
        TranslationResult Handle(ITranslationContext context);
    }
}
