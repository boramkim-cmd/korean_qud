/*
 * 파일명: ObjectData.cs
 * 분류: Data - Model
 * 역할: 오브젝트 데이터 모델
 * 작성일: 2026-01-26
 */

using System;
using System.Collections.Generic;

namespace QudKorean.Objects.V2.Data
{
    /// <summary>
    /// Data model for creature/item translation data.
    /// </summary>
    public class ObjectData
    {
        /// <summary>
        /// The blueprint ID (e.g., "Bear", "Dagger").
        /// </summary>
        public string BlueprintId { get; set; }

        /// <summary>
        /// Name translations dictionary (English -> Korean).
        /// </summary>
        public Dictionary<string, string> Names { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// English description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Korean description.
        /// </summary>
        public string DescriptionKo { get; set; }
    }
}
