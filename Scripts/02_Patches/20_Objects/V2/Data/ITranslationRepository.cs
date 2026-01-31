/*
 * 파일명: ITranslationRepository.cs
 * 분류: Data - Interface
 * 역할: Repository 패턴 인터페이스 정의
 * 작성일: 2026-01-26
 */

using System.Collections.Generic;

namespace QudKorean.Objects.V2.Data
{
    /// <summary>
    /// Repository interface for translation data access.
    /// Abstracts the data source (JSON files) from the translation logic.
    /// </summary>
    public interface ITranslationRepository
    {
        #region Object Data Access

        /// <summary>
        /// Gets creature data by blueprint ID.
        /// </summary>
        ObjectData GetCreature(string id);

        /// <summary>
        /// Gets item data by blueprint ID.
        /// </summary>
        ObjectData GetItem(string id);

        /// <summary>
        /// All loaded creature data.
        /// </summary>
        IEnumerable<ObjectData> AllCreatures { get; }

        /// <summary>
        /// All loaded item data.
        /// </summary>
        IEnumerable<ObjectData> AllItems { get; }

        #endregion

        #region Vocabulary Dictionaries

        /// <summary>
        /// All prefixes sorted by length (longest first) for greedy matching.
        /// Includes materials, qualities, modifiers, processing, tonics, grenades, marks, colors.
        /// </summary>
        IReadOnlyList<KeyValuePair<string, string>> Prefixes { get; }

        /// <summary>
        /// Prefixes as case-insensitive dictionary for O(1) lookup.
        /// </summary>
        IReadOnlyDictionary<string, string> PrefixesDict { get; }

        /// <summary>
        /// Vocabulary for color tag content translation.
        /// Includes materials, qualities, tonics, grenades, modifiers, colors, liquids, of patterns, body parts.
        /// </summary>
        IReadOnlyList<KeyValuePair<string, string>> ColorTagVocab { get; }

        /// <summary>
        /// ColorTagVocab as case-insensitive dictionary for O(1) lookup.
        /// </summary>
        IReadOnlyDictionary<string, string> ColorTagVocabDict { get; }

        /// <summary>
        /// Base nouns sorted by length (longest first).
        /// </summary>
        IReadOnlyList<KeyValuePair<string, string>> BaseNouns { get; }

        /// <summary>
        /// BaseNouns as case-insensitive dictionary for O(1) lookup.
        /// </summary>
        IReadOnlyDictionary<string, string> BaseNounsDict { get; }

        /// <summary>
        /// Species dictionary (creature names to Korean).
        /// </summary>
        IReadOnlyDictionary<string, string> Species { get; }

        /// <summary>
        /// State suffixes (lit, empty, etc.).
        /// </summary>
        IReadOnlyDictionary<string, string> States { get; }

        /// <summary>
        /// Liquid names.
        /// </summary>
        IReadOnlyDictionary<string, string> Liquids { get; }

        /// <summary>
        /// "of X" pattern translations.
        /// </summary>
        IReadOnlyDictionary<string, string> OfPatterns { get; }

        /// <summary>
        /// Body parts dictionary.
        /// </summary>
        IReadOnlyDictionary<string, string> BodyParts { get; }

        /// <summary>
        /// Part suffixes sorted by length (longest first).
        /// E.g., " egg" -> " 알", " hide" -> " 가죽"
        /// </summary>
        IReadOnlyList<KeyValuePair<string, string>> PartSuffixes { get; }

        /// <summary>
        /// Tonics dictionary (tonic names to Korean).
        /// </summary>
        IReadOnlyDictionary<string, string> Tonics { get; }

        /// <summary>
        /// Shaders dictionary (shader/color names to Korean).
        /// </summary>
        IReadOnlyDictionary<string, string> Shaders { get; }

        /// <summary>
        /// Global name index: English name -> Korean translation.
        /// Built from all creature and item Names dictionaries for O(1) lookup.
        /// </summary>
        IReadOnlyDictionary<string, string> GlobalNameIndex { get; }

        /// <summary>
        /// Display lookup: original DisplayName (with color tags) -> Korean translation.
        /// Built at build time from XML assets for O(1) direct lookup.
        /// </summary>
        IReadOnlyDictionary<string, string> DisplayLookup { get; }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Reloads all JSON files.
        /// </summary>
        void Reload();

        /// <summary>
        /// Gets statistics about loaded data.
        /// </summary>
        string GetStats();

        #endregion
    }
}
