/*
 * 파일명: TranslationPipeline.cs
 * 분류: Pipeline - Core
 * 역할: 번역 파이프라인 관리자
 * 작성일: 2026-01-26
 */

using QudKorean.Objects.V2.Core;
using QudKorean.Objects.V2.Data;
using QudKorean.Objects.V2.Pipeline.Handlers;

namespace QudKorean.Objects.V2.Pipeline
{
    /// <summary>
    /// Manages the translation pipeline using Chain of Responsibility pattern.
    /// </summary>
    public class TranslationPipeline
    {
        private ITranslationHandler _firstHandler;
        private ITranslationHandler _lastHandler;

        /// <summary>
        /// Adds a handler to the end of the pipeline chain.
        /// </summary>
        public TranslationPipeline AddHandler(ITranslationHandler handler)
        {
            if (_firstHandler == null)
            {
                _firstHandler = handler;
                _lastHandler = handler;
            }
            else
            {
                _lastHandler.Next = handler;
                _lastHandler = handler;
            }
            return this;
        }

        /// <summary>
        /// Executes the pipeline for the given context.
        /// </summary>
        public TranslationResult Execute(ITranslationContext context)
        {
            if (_firstHandler == null)
            {
                return TranslationResult.Miss();
            }

            return _firstHandler.Handle(context);
        }

        /// <summary>
        /// Creates the default translation pipeline with all standard handlers.
        /// Pipeline order:
        /// 1. CacheHandler - Check cache first
        /// 2. DirectMatchHandler - Try direct blueprint match
        /// 3. PrefixSuffixHandler - Extract and handle prefixes/suffixes
        /// 4. PatternHandler - Try dynamic pattern matching
        /// 5. FallbackHandler - Final fallback with color tag translation
        /// </summary>
        public static TranslationPipeline CreateDefault(ITranslationRepository repo)
        {
            var pipeline = new TranslationPipeline();

            pipeline
                .AddHandler(new CacheHandler())
                .AddHandler(new DirectMatchHandler())
                .AddHandler(new PrefixSuffixHandler())
                .AddHandler(new PatternHandler())
                .AddHandler(new FallbackHandler());

            return pipeline;
        }
    }
}
