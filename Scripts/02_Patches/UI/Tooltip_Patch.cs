/*
 * 파일명: Tooltip_Patch.cs
 * 분류: [UI Patch] 툴팁 번역 패치
 * 역할: ModelShark Tooltip 시스템의 텍스트를 번역합니다.
 */

using HarmonyLib;
using ModelShark;
using UnityEngine;
using QudKRTranslation.Utils;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(TooltipTrigger))]
    public static class Tooltip_Patch
    {
        [HarmonyPatch(nameof(TooltipTrigger.SetText), new System.Type[] { typeof(string), typeof(string) })]
        [HarmonyPrefix]
        static void SetText_Prefix(ref string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            // 현재 활성 Scope 가져오기
            var scope = ScopeManager.GetCurrentScope();
            if (scope == null) return;

            // 태그를 보존하며 번역 시도
            if (TranslationUtils.TryTranslatePreservingTags(text, out string translated, scope))
            {
                text = translated;
            }
        }
    }
}
