/*
 * 파일명: PatternTranslatorRegistry.cs
 * 분류: Patterns - Registry
 * 역할: 패턴 번역기 등록 및 관리
 * 작성일: 2026-01-26
 */

using System.Collections.Generic;
using System.Linq;
using QudKorean.Objects.V2.Core;

namespace QudKorean.Objects.V2.Patterns
{
    /// <summary>
    /// Registry for pattern translators.
    /// Manages registration and execution of pattern translators in priority order.
    /// </summary>
    public class PatternTranslatorRegistry
    {
        private readonly List<IPatternTranslator> _translators = new List<IPatternTranslator>();
        private bool _sorted = false;

        /// <summary>
        /// Registers a new pattern translator.
        /// </summary>
        public void Register(IPatternTranslator translator)
        {
            _translators.Add(translator);
            _sorted = false;
        }

        /// <summary>
        /// Tries to translate using all registered pattern translators.
        /// Returns on first successful translation.
        /// </summary>
        public TranslationResult TryTranslate(string name, ITranslationContext context)
        {
            EnsureSorted();

            foreach (var translator in _translators)
            {
                if (translator.CanHandle(name))
                {
                    var result = translator.Translate(name, context);
                    if (result.Success)
                    {
                        return result;
                    }
                }
            }

            return TranslationResult.Miss();
        }

        private void EnsureSorted()
        {
            if (!_sorted)
            {
                _translators.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                _sorted = true;
            }
        }

        /// <summary>
        /// Creates a registry with all default pattern translators.
        /// </summary>
        public static PatternTranslatorRegistry CreateDefault()
        {
            var registry = new PatternTranslatorRegistry();

            // Register translators in priority order (lower = earlier)
            registry.Register(new CorpseTranslator());       // Priority 10
            registry.Register(new FoodTranslator());         // Priority 20
            registry.Register(new PartsTranslator());        // Priority 30
            registry.Register(new PossessiveTranslator());   // Priority 40
            registry.Register(new BookTitleTranslator());    // Priority 45
            registry.Register(new OfPatternTranslator());    // Priority 50
            registry.Register(new CompoundTranslator());     // Priority 60

            return registry;
        }
    }
}
