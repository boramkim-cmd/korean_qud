/*
 * 파일명: G.cs
 * 분류: [Core] Glossary 간편 접근
 * 역할: 초간단 glossary 접근을 위한 헬퍼
 * 작성일: 2026-01-15
 */

using UnityEngine;

namespace QudKRTranslation.Core
{
    /// <summary>
    /// Glossary 초간단 접근 (G 클래스)
    /// </summary>
    public static class G
    {
        /// <summary>
        /// category.key 또는 [[category.key]] 형식을 파싱하여 용어 반환
        /// </summary>
        public static string _(string placeholder)
        {
            if (string.IsNullOrEmpty(placeholder)) return "";
            
            // [[category.key]] 형식이면 괄호 제거
            string content = placeholder;
            if (placeholder.StartsWith("[[") && placeholder.EndsWith("]]"))
            {
                content = placeholder.Substring(2, placeholder.Length - 4);
            }
            
            // category.key 파싱
            var parts = content.Split('.');
            if (parts.Length == 2)
            {
                string category = parts[0];
                string key = parts[1];
                
                GlossaryLoader.LoadGlossary();
                string result = GlossaryLoader.GetTerm(category, key, "");
                
                // 용어를 찾지 못하면 빈 문자열 대신 key만 반환
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning($"[Glossary] 용어를 찾을 수 없음: {category}.{key}");
                    return key; // "newGame" 같은 key만 반환 (placeholder 전체가 아님)
                }
                
                return result;
            }
            
            return "";
        }
    }
}
