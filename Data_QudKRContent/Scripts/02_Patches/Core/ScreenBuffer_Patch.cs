/*
 * 파일명: ScreenBuffer_Patch.cs
 * 분류: [Core Patch] 전역 텍스트 렌더링 패치
 * 역할: ScreenBuffer.Write 메서드를 패치하여 모든 화면의 텍스트를 번역합니다.
 *       ⚠️ 전역 패치이므로 ScopeManager를 통해 범위를 엄격히 제어합니다.
 * 작성일: 2026-01-15
 */

using HarmonyLib;
using XRL.UI;

namespace QudKRTranslation.Patches
{
    /// <summary>
    /// ScreenBuffer 전역 패치
    /// 주의: 이 패치는 모든 화면에 영향을 주므로 Scope가 설정된 경우에만 번역합니다.
    /// </summary>
    [HarmonyPatch(typeof(ScreenBuffer))]
    public static class ScreenBuffer_Patch
    {
        /// <summary>
        /// ScreenBuffer.Write 메서드 패치
        /// 현재 활성 Scope가 있을 때만 번역을 시도합니다.
        /// </summary>
        [HarmonyPatch("Write", new System.Type[] { typeof(string) })]
        [HarmonyPrefix]
        static void Write_Prefix(ref string Text)
        {
            // Scope가 설정되지 않았으면 번역하지 않음 (안전)
            var scope = ScopeManager.GetCurrentScope();
            if (scope == null) return;
            
            // 번역 시도
            if (TranslationEngine.TryTranslate(Text, out string translated, scope))
            {
                Text = translated;
            }
        }
    }
}
