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

                // rightText: 거래 화면에서 "X# $Y.YY" 형식의 가격/무게
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "rightText", val =>
                {
                    if (val == null) return val;
                    if (val.Contains("#"))
                        val = val.Replace("#", "kg");
                    if (val.Contains("$"))
                        val = val.Replace("$", "드램");
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] InventoryLine Weight 오류: {e.Message}");
            }
        }
    }

    // TradeScreen.UpdateTotals() — 거래 화면 무게/가격 단위 + drams 텍스트
    [HarmonyPatch(typeof(Qud.UI.TradeScreen), "UpdateTotals")]
    public static class Patch_TradeScreen_Weight
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.TradeScreen __instance)
        {
            try
            {
                var screenType = typeof(Qud.UI.TradeScreen);

                // freeDramsLabels: 배열 전체 순회 (drams/weight 라벨)
                TranslateAllSkins(__instance, screenType, "freeDramsLabels");

                // 기타 거래 화면 텍스트 필드들
                TranslateSkinField(__instance, screenType, "playerDramsLabel");
                TranslateSkinField(__instance, screenType, "traderDramsLabel");
                TranslateSkinField(__instance, screenType, "playerWeightLabel");
                TranslateSkinField(__instance, screenType, "traderWeightLabel");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] TradeScreen Weight 오류: {e.Message}");
            }
        }

        private static string TranslateUnits(string val)
        {
            if (val == null) return val;
            if (val.Contains(" lbs."))
                val = val.Replace(" lbs.", " kg");
            if (val.Contains("drams"))
                val = val.Replace("drams", "드램");
            if (val.Contains("#"))
                val = val.Replace("#", "kg");
            if (val.Contains("$"))
                val = val.Replace("$", "드램");
            return val;
        }

        private static void TranslateAllSkins(object instance, Type type, string fieldName)
        {
            var field = AccessTools.Field(type, fieldName);
            if (field == null) return;
            var arr = field.GetValue(instance) as Array;
            if (arr == null) return;
            for (int i = 0; i < arr.Length; i++)
            {
                var skin = arr.GetValue(i);
                if (skin != null)
                    UITextSkinHelper.Translate(skin, TranslateUnits);
            }
        }

        private static void TranslateSkinField(object instance, Type type, string fieldName)
        {
            var field = AccessTools.Field(type, fieldName);
            if (field == null) return;
            var skin = field.GetValue(instance);
            if (skin != null)
                UITextSkinHelper.Translate(skin, TranslateUnits);
        }
    }
}
