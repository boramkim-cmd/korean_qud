/*
 * 파일명: GlossaryExtensions.cs
 * 분류: [Core] Glossary 확장 메서드
 * 역할: 문자열 보간으로 간단하게 용어 사용
 * 작성일: 2026-01-15
 */

using System;

namespace QudKRTranslation.Core
{
    /// <summary>
    /// Glossary 문자열 보간 확장
    /// </summary>
    public static class GlossaryExtensions
    {
        /// <summary>
        /// [[category.key]] 형식의 문자열을 용어로 변환
        /// </summary>
        public static string G(this string placeholder)
        {
            if (string.IsNullOrEmpty(placeholder)) return placeholder;
            
            // [[category.key]] 패턴 파싱
            if (placeholder.StartsWith("[[") && placeholder.EndsWith("]]"))
            {
                var content = placeholder.Substring(2, placeholder.Length - 4);
                var parts = content.Split('.');
                
                if (parts.Length == 2)
                {
                    string category = parts[0];
                    string key = parts[1];
                    
                    LocalizationManager.Initialize();
                    return LocalizationManager.GetTerm(category, key, placeholder);
                }
            }
            
            return placeholder;
        }
    }
}
