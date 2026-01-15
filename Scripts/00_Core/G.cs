/*
 * 파일명: G.cs
 * 분류: [Core] Glossary 간편 접근
 * 역할: 초간단 glossary 접근을 위한 헬퍼
 * 작성일: 2026-01-15
 */

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
            if (string.IsNullOrEmpty(placeholder)) return placeholder;
            
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
                return GlossaryLoader.GetTerm(category, key, placeholder);
            }
            
            return placeholder;
        }
    }
}
