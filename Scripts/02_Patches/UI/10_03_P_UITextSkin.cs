/*
 * 파일명: UITextSkin_Patch.cs
 * 분류: [UI Patch] 모던 UI 텍스트 패치
 * 역할: UITextSkin.Apply 메서드를 패치하여 TMPro 기반 UI 텍스트를 번역합니다.
 * 작성일: 2026-01-15
 */

using HarmonyLib;
using XRL.UI;
using QudKRTranslation.Utils;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(UITextSkin), nameof(UITextSkin.Apply), new System.Type[0])]
    public static class UITextSkin_Patch
    {
        [HarmonyPrefix]
        static void Apply_Prefix(UITextSkin __instance)
        {
            if (__instance == null || string.IsNullOrEmpty(__instance.text)) return;

            // 현재 활성 Scope 가져오기
            var scope = ScopeManager.GetCurrentScope();
            if (scope == null) return;

            // 태그를 보존하며 번역 시도
            if (TranslationUtils.TryTranslatePreservingTags(__instance.text, out string translated, scope))
            {
                if (__instance.text != translated)
                {
                    __instance.text = translated;
                }
            }
        }
    }
}
