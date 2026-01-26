/*
 * 파일명: IPatternTranslator.cs
 * 분류: Patterns - Interface
 * 역할: Strategy 패턴 인터페이스 정의
 * 작성일: 2026-01-26
 */

using QudKorean.Objects.V2.Core;

namespace QudKorean.Objects.V2.Patterns
{
    /// <summary>
    /// Strategy interface for pattern-based translators.
    /// Each pattern translator handles a specific naming pattern (corpse, food, parts, etc.).
    /// </summary>
    public interface IPatternTranslator
    {
        /// <summary>
        /// The name of this pattern translator (for debugging and logging).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Priority for execution order (lower = earlier).
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Checks if this translator can handle the given name.
        /// </summary>
        bool CanHandle(string name);

        /// <summary>
        /// Attempts to translate the name using this pattern.
        /// </summary>
        TranslationResult Translate(string name, ITranslationContext context);
    }
}
