/*
 * 파일명: 02_10_17_TooltipFallback.cs
 * 분류: [Patches/UI] 툴팁 폰트 패치
 * 역할: 툴팁(ModelShark.Tooltip)이 표시될 때 한글 fallback 폰트를 적용
 */

using System.Collections.Generic;
using HarmonyLib;
using ModelShark;
using TMPro;
using QudKRTranslation.Core;

namespace QudKRTranslation.Patches.UI
{
    [HarmonyPatch(typeof(Tooltip), nameof(Tooltip.Display))]
    public static class TooltipTMPFallbackApply
    {
        static void Postfix(Tooltip __instance)
        {
            if (__instance?.GameObject == null)
            {
                return;
            }

            TMP_FontAsset fallback = TMPFallbackFontBundle.GetFallbackFont();
            if (fallback == null)
            {
                return;
            }

            TextMeshProUGUI[] texts = __instance.GameObject.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
            for (int i = 0; i < texts.Length; i++)
            {
                TextMeshProUGUI tmp = texts[i];
                if (tmp == null || tmp.font == null)
                {
                    continue;
                }

                List<TMP_FontAsset> fallbacks = tmp.font.fallbackFontAssetTable;
                if (fallbacks == null)
                {
                    fallbacks = new List<TMP_FontAsset>();
                    tmp.font.fallbackFontAssetTable = fallbacks;
                }

                if (!fallbacks.Contains(fallback))
                {
                    fallbacks.Add(fallback);
                }
                tmp.ForceMeshUpdate(ignoreActiveState: false, forceTextReparsing: true);
            }
        }
    }
}
