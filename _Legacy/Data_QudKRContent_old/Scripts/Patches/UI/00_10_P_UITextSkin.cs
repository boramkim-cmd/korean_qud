/*
 * 파일명: 00_10_P_UITextSkin.cs
 * 분류: [UI] 모던 UI 텍스트 번역
 * 역할: 모던 UI(TextMeshPro 기반)의 텍스트를 관리하는 UITextSkin 클래스를 패치하여
 *       화면에 표시되기 직전에 한글로 번역합니다.
 * 수정일: 2026-01-14
 */

using HarmonyLib;
using XRL.UI;
using Qud.UI;

namespace QudKRContent
{
    [HarmonyPatch(typeof(UITextSkin), "Apply")]
    public static class Patch_UITextSkin
    {
        [HarmonyPrefix]
        static void Prefix(UITextSkin __instance)
        {
            if (__instance == null) return;

            // 이미 번역된 텍스트이거나 비어있으면 스킵
            if (string.IsNullOrEmpty(__instance.text)) return;

            // 현재 스코프가 지정되어 있으면 사용, 없으면 Common 사용
            var scopes = TranslationScopeState.CurrentScope ?? new[] { DictDB.Common };
            if (DictDB.TryGetScopedTranslation(__instance.text, out string translated, scopes))
            {
                if (__instance.text != translated)
                {
                    __instance.text = translated;
                }
            }
        }
    }
}
