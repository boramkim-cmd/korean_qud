/*
 * 파일명: Tooltip_Patch.cs
 * 분류: [UI Patch] 툴팁 번역 패치
 * 역할: ModelShark Tooltip 시스템의 텍스트를 번역합니다.
 */

using HarmonyLib;
using ModelShark;
using UnityEngine;
using QudKRTranslation.Utils;
using QudKRTranslation;

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

        // Postfix: SetText 호출 이후 툴팁 내부 텍스트에 한글 폰트가 적용되도록 함
        [HarmonyPostfix]
        static void SetText_Postfix(TooltipTrigger __instance)
        {
            try
            {
                var k = QudKRTranslation.Core.FontManager.GetKoreanTMPFont();
                if (k == null)
                {
                    Debug.Log("[Qud-KR] Tooltip postfix: No Korean TMP font available yet.");
                    return;
                }

                var tmps = __instance.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                int applied = 0;
                foreach (var t in tmps)
                {
                    if (t == null) continue;
                    try
                    {
                        if (t.font != null)
                        {
                            if (t.font.fallbackFontAssetTable == null)
                                t.font.fallbackFontAssetTable = new System.Collections.Generic.List<TMPro.TMP_FontAsset>();
                            if (!t.font.fallbackFontAssetTable.Contains(k))
                            {
                                t.font.fallbackFontAssetTable.Add(k);
                                applied++;
                            }
                        }
                        else
                        {
                            t.font = k;
                            applied++;
                        }
                        t.font = t.font; // trigger refresh
                        t.SetAllDirty();
                    }
                    catch { }
                }

                Debug.Log($"[Qud-KR] Tooltip postfix applied Korean fallback '{k.name}' to {applied} TMP components.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] Tooltip postfix exception: {ex.Message}");
            }
        }
    }
}
