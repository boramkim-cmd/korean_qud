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
    // 공통: "$123.45" → "123.45드램", "96$" → "96드램"
    internal static class UnitTranslator
    {
        // $숫자 패턴: "$12.34" → "12.34드램"
        private static readonly Regex RxDollarBefore = new Regex(@"\$(\d+(?:\.\d+)?)", RegexOptions.Compiled);
        // 숫자$ 패턴: "96$" → "96드램" (HUD 무게 바에서 사용)
        private static readonly Regex RxDollarAfter = new Regex(@"(\d+(?:\.\d+)?)\$", RegexOptions.Compiled);
        // 컬러태그로 분리된 $ 패턴: "{{B|$}}{{C|11.90}}" → "{{C|11.90드램}}"
        private static readonly Regex RxDollarColorTag = new Regex(
            @"\{\{[^{}|]*\|\$\}\}\s*\{\{([^{}|]*)\|(\d+(?:\.\d+)?)\}\}",
            RegexOptions.Compiled);

        public static string Translate(string val)
        {
            if (val == null) return val;

            // lbs. → kg
            if (val.Contains(" lbs."))
                val = val.Replace(" lbs.", " kg");

            // # → kg (무게 기호)
            if (val.Contains("#"))
                val = val.Replace("#", "kg");

            // $숫자 또는 숫자$ → 숫자드램
            if (val.Contains("$"))
            {
                // 컬러태그로 분리된 $ 패턴 우선 처리: {{B|$}}{{C|11.90}} → {{C|11.90드램}}
                val = RxDollarColorTag.Replace(val, "{{$1|$2드램}}");
                // 일반 패턴
                val = RxDollarBefore.Replace(val, "$1드램");
                val = RxDollarAfter.Replace(val, "$1드램");
            }

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
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] InventoryLine Weight 오류: {e.Message}");
            }
        }
    }

    // TradeScreen.UpdateTotals() — 거래 화면 무게/가격 단위 + drams 텍스트
    // 검증된 필드: freeDramsLabels[], totalLabels[] (TradeScreen.cs:209,211,739-742)
    [HarmonyPatch(typeof(Qud.UI.TradeScreen), "UpdateTotals")]
    public static class Patch_TradeScreen_Weight
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.TradeScreen __instance)
        {
            try
            {
                var screenType = typeof(Qud.UI.TradeScreen);

                // freeDramsLabels[]: "$96" → "96드램", "57/315 lbs." → "57/315 kg"
                TranslateAllSkins(__instance, screenType, "freeDramsLabels");

                // totalLabels[]: "0.00 drams →" → "0.00 드램 →"
                TranslateAllSkins(__instance, screenType, "totalLabels");
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

    // TradeScreen.HandleHighlightObject() — 하단 바 선택 아이템 "3# $11.90" 표시
    // 검증된 필드: detailsRightText (TradeScreen.cs:75,950-957)
    [HarmonyPatch(typeof(Qud.UI.TradeScreen), "HandleHighlightObject")]
    public static class Patch_TradeScreen_Highlight
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.TradeScreen __instance)
        {
            try
            {
                var screenType = typeof(Qud.UI.TradeScreen);
                // detailsRightText: " {{K|3#}} {{B|$}}{{C|11.90}}" → " {{K|3kg}} {{C|11.90}}드램"
                var field = AccessTools.Field(screenType, "detailsRightText");
                if (field == null) return;
                var skin = field.GetValue(__instance);
                if (skin != null)
                    UITextSkinHelper.Translate(skin, UnitTranslator.Translate);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] TradeScreen Highlight 오류: {e.Message}");
            }
        }
    }

    // TradeLine.setData() — 거래 목록 개별 아이템 가격 "[$11.90]"
    // 검증된 필드: rightFloatText (TradeLine.cs:40,487-491)
    [HarmonyPatch(typeof(Qud.UI.TradeLine), "setData")]
    public static class Patch_TradeLine_Price
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.TradeLine __instance)
        {
            try
            {
                var lineType = typeof(Qud.UI.TradeLine);
                StatusFormatExtensions.TranslateUITextSkin(__instance, lineType, "rightFloatText", UnitTranslator.Translate);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] TradeLine Price 오류: {e.Message}");
            }
        }
    }
}
