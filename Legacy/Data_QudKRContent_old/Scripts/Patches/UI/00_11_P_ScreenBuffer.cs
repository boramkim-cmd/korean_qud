/*
 * 파일명: 00_11_P_ScreenBuffer.cs
 * 분류: [UI] 클래식 UI 텍스트 번역
 * 역할: 클래식(콘솔) UI의 텍스트를 관리하는 ScreenBuffer 클래스를 패치하여
 *       화면에 표시되기 직전에 한글로 번역합니다.
 * 수정일: 2026-01-14
 */

using HarmonyLib;
using ConsoleLib.Console;

namespace QudKRContent
{
    [HarmonyPatch(typeof(ScreenBuffer))]
    public static class Patch_ScreenBuffer
    {
        // 1. 일반적인 Write 호출 번역
        [HarmonyPatch("Write", new[] { typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(System.Collections.Generic.List<string>), typeof(int) })]
        [HarmonyPrefix]
        static void Write_Prefix(ref string s)
        {
            if (string.IsNullOrEmpty(s)) return;

            // 현재 스코프가 지정되어 있으면 사용, 없으면 Common 사용
            var scopes = TranslationScopeState.CurrentScope ?? new[] { DictDB.Common };
            if (DictDB.TryGetScopedTranslation(s, out string translated, scopes))
            {
                s = translated;
            }
        }

        // 2. 줄바꿈을 포함한 블록 단위 Write 번역
        [HarmonyPatch("WriteBlockWithNewlines", new[] { typeof(string), typeof(int), typeof(int), typeof(bool) })]
        [HarmonyPrefix]
        static void WriteBlockWithNewlines_Prefix(ref string s)
        {
            if (string.IsNullOrEmpty(s)) return;

            var scopes = TranslationScopeState.CurrentScope ?? new[] { DictDB.Common };
            if (DictDB.TryGetScopedTranslation(s, out string translated, scopes))
            {
                s = translated;
            }
        }
    }
}
