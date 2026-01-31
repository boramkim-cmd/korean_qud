// 분류: UI 패치
// 역할: 모든 UI에서 무게 단위 lbs → kg 통일
//   - InventoryLine.setData(): 카테고리/아이템 무게
//   - NearbyItemsWindow.UpdateGameContext(): 근처 아이템 무게
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

                // categoryWeightText: "|X items|Y lbs.|"
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "categoryWeightText", val =>
                {
                    val = val.Replace(" lbs.", " kg");
                    val = val.Replace(" items", " 개");
                    return val;
                });

                // itemWeightText: "[X lbs.]"
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "itemWeightText", val =>
                {
                    val = val.Replace(" lbs.", " kg");
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] InventoryLine Weight 오류: {e.Message}");
            }
        }
    }

    // NearbyItemsWindow.UpdateGameContext() — 근처 아이템 무게
    // RightText는 ObjectFinderLine.Data에 설정되므로, 결과적으로
    // ObjectFinderLine에서 표시될 때 번역. UpdateGameContext Postfix로 불가하므로
    // ObjectFinderLine.setData()를 패치하여 표시 시점에 교체.
    [HarmonyPatch(typeof(Qud.UI.ObjectFinderLine), "setData")]
    public static class Patch_ObjectFinderLine_Weight
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.ObjectFinderLine __instance)
        {
            try
            {
                var lineType = typeof(Qud.UI.ObjectFinderLine);

                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "rightText", val =>
                {
                    if (val.Contains("lbs."))
                        val = val.Replace("lbs.", "kg");
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] ObjectFinderLine Weight 오류: {e.Message}");
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

                var labels = field.GetValue(__instance) as Qud.UI.UITextSkin[];
                if (labels == null || labels.Length < 2 || labels[1] == null) return;

                string val = labels[1].text;
                if (val != null && val.Contains(" lbs."))
                {
                    labels[1].SetText(val.Replace(" lbs.", " kg"));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] TradeScreen Weight 오류: {e.Message}");
            }
        }
    }
}
