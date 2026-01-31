// 분류: UI 패치
// 역할: 모든 UI에서 무게/가격 단위 한글화
//   - "#" → "kg", "$X.XX" → "X.XX드램", "X lbs." → "X kg"
//   - InventoryLine.setData(): 카테고리/아이템 무게 + 가격
//   - TradeScreen.UpdateTotals(): 거래 화면 무게/가격/drams

using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    // 공통: "$123.45" → "123.45드램", "[$5.71]" → "[5.71드램]"
    internal static class UnitTranslator
    {
        // $숫자 패턴: "$12.34" → "12.34드램"
        private static readonly Regex RxDollar = new Regex(@"\$(\d+(?:\.\d+)?)", RegexOptions.Compiled);

        public static string Translate(string val)
        {
            if (val == null) return val;

            // lbs. → kg
            if (val.Contains(" lbs."))
                val = val.Replace(" lbs.", " kg");

            // # → kg (무게 기호)
            if (val.Contains("#"))
                val = val.Replace("#", "kg");

            // $숫자 → 숫자드램 (Regex로 순서 보장)
            if (val.Contains("$"))
                val = RxDollar.Replace(val, "$1드램");

            // "drams" → "드램"
            if (val.Contains("drams"))
                val = val.Replace("drams", "드램");

            return val;
        }
    }

    // InventoryLine.setData() — 인벤토리 카테고리/아이템 무게 + 가격
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
                    val = UnitTranslator.Translate(val);
                    if (val != null && val.Contains(" items"))
                        val = val.Replace(" items", " 개");
                    return val;
                });

                // itemWeightText: "[X lbs.]" or "X#"
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "itemWeightText", UnitTranslator.Translate);

                // rightText: 거래 화면에서 "X# $Y.YY" 또는 "[$Y.YY]"
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "rightText", UnitTranslator.Translate);
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

                // freeDramsLabels: 배열 전체 순회
                TranslateAllSkins(__instance, screenType, "freeDramsLabels");

                // 기타 거래 화면 텍스트 필드들
                TranslateSkinField(__instance, screenType, "playerDramsLabel");
                TranslateSkinField(__instance, screenType, "traderDramsLabel");
                TranslateSkinField(__instance, screenType, "playerWeightLabel");
                TranslateSkinField(__instance, screenType, "traderWeightLabel");
                TranslateSkinField(__instance, screenType, "selectedItemWeight");
                TranslateSkinField(__instance, screenType, "selectedItemPrice");
                TranslateSkinField(__instance, screenType, "selectedItemInfo");
                TranslateSkinField(__instance, screenType, "itemInfoText");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] TradeScreen Weight 오류: {e.Message}");
            }
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
                    UITextSkinHelper.Translate(skin, UnitTranslator.Translate);
            }
        }

        private static void TranslateSkinField(object instance, Type type, string fieldName)
        {
            var field = AccessTools.Field(type, fieldName);
            if (field == null) return;
            var skin = field.GetValue(instance);
            if (skin != null)
                UITextSkinHelper.Translate(skin, UnitTranslator.Translate);
        }
    }
}
