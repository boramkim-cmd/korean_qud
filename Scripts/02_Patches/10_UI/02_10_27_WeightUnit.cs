// 분류: UI 패치
// 역할: 모든 UI에서 무게 단위 lbs → kg 통일
//   - InventoryLine.setData(): 카테고리/아이템 무게
//   - TradeScreen.UpdateTotals(): 거래 화면 무게

using System;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    // InventoryLine.setData() — 인벤토리 카테고리 및 아이템 무게
    // "X items|Y lbs.|" → "X 개|Y kg|", "[X lbs.]" → "[X kg]"
    [HarmonyPatch(typeof(Qud.UI.InventoryLine), "setData")]
    public static class Patch_InventoryLine_Weight
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.InventoryLine __instance)
        {
            try
            {
                var lineType = typeof(Qud.UI.InventoryLine);

                // categoryWeightText: "|X items|Y lbs.|" or "|X items|Y#|"
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "categoryWeightText", val =>
                {
                    val = val.Replace(" lbs.", " kg");
                    val = val.Replace(" items", " 개");
                    if (val.Contains("#"))
                        val = val.Replace("#", "kg");
                    return val;
                });

                // itemWeightText: "[X lbs.]" or "X#"
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "itemWeightText", val =>
                {
                    val = val.Replace(" lbs.", " kg");
                    if (val.Contains("#"))
                        val = val.Replace("#", "kg");
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] InventoryLine Weight 오류: {e.Message}");
            }
        }
    }

    // TradeScreen.UpdateTotals() — 거래 화면 무게 + drams
    [HarmonyPatch(typeof(Qud.UI.TradeScreen), "UpdateTotals")]
    public static class Patch_TradeScreen_Weight
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.TradeScreen __instance)
        {
            try
            {
                var screenType = typeof(Qud.UI.TradeScreen);

                // freeDramsLabels[1]: "...|X/Y lbs.}}"
                var field = AccessTools.Field(screenType, "freeDramsLabels");
                if (field == null) return;

                var labels = field.GetValue(__instance) as Array;
                if (labels == null || labels.Length < 2) return;

                var label = labels.GetValue(1);
                if (label == null) return;

                UITextSkinHelper.Translate(label, val =>
                {
                    if (val == null) return val;
                    if (val.Contains(" lbs."))
                        val = val.Replace(" lbs.", " kg");
                    if (val.Contains("#"))
                        val = val.Replace("#", "kg");
                    if (val.Contains("$"))
                        val = val.Replace("$", "드램");
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] TradeScreen Weight 오류: {e.Message}");
            }
        }
    }
}
