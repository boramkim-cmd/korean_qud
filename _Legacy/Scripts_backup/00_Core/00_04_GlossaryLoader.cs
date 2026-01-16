/*
 * 파일명: GlossaryLoader.cs
 * 분류: [Core] 레거시 호환성 래퍼
 * 역할: 기존 코드가 LocalizationManager를 사용할 수 있도록 연결해줍니다.
 *       (더 이상 직접 JSON을 로드하지 않습니다)
 */

using QudKRTranslation.Core;

namespace QudKRTranslation.Core
{
    public static class GlossaryLoader
    {
        public static void LoadGlossary() 
        {
            LocalizationManager.Initialize();
        }

        public static string GetTerm(string category, string key, string fallback = "") 
        {
            return LocalizationManager.GetTerm(category, key, fallback);
        }

        public static bool HasTerm(string category, string key) 
        {
            return LocalizationManager.HasTerm(category, key);
        }

        public static void ReloadGlossary() 
        {
            LocalizationManager.Reload();
        }
    }
}
