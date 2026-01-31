// 분류: UI 패치
// 역할: 상태 화면 포맷 문자열 (Level/HP/XP/Weight, Attribute Points, show cybernetics/equipment)을 한글로 교체

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    // CharacterStatusScreen.UpdateViewFromData() — Level/HP/XP/Weight line + Attribute Points + Genotype/Subtype
    [HarmonyPatch(typeof(Qud.UI.CharacterStatusScreen), "UpdateViewFromData")]
    public static class Patch_CharacterStatusScreen_UpdateViewFromData
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.CharacterStatusScreen __instance)
        {
            try
            {
                // levelText: "Level: X ¯ HP: X/X ¯ XP: X/X ¯ Weight: X#"
                var levelTextField = AccessTools.Field(typeof(Qud.UI.CharacterStatusScreen), "levelText");
                if (levelTextField != null)
                {
                    var skin = levelTextField.GetValue(__instance);
                    if (skin != null)
                    {
                        var textProp = AccessTools.Property(skin.GetType(), "text");
                        if (textProp != null)
                        {
                            string val = textProp.GetValue(skin) as string;
                            if (val != null)
                            {
                                val = val.Replace("Level:", "레벨:");
                                val = val.Replace("HP:", "체력:");
                                val = val.Replace("XP:", "경험치:");
                                val = val.Replace("Weight:", "무게:");
                                textProp.SetValue(skin, val);
                            }
                        }
                    }
                }

                // attributePointsText: "Attribute Points: {{G|X}}"
                var apField = AccessTools.Field(typeof(Qud.UI.CharacterStatusScreen), "attributePointsText");
                if (apField != null)
                {
                    var skin = apField.GetValue(__instance);
                    if (skin != null)
                    {
                        var textProp = AccessTools.Property(skin.GetType(), "text");
                        if (textProp != null)
                        {
                            string val = textProp.GetValue(skin) as string;
                            if (val != null && val.Contains("Attribute Points:"))
                            {
                                val = val.Replace("Attribute Points:", "속성 포인트:");
                                textProp.SetValue(skin, val);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] CharacterStatusScreen 오류: {e.Message}");
            }
        }
    }

    // InventoryAndEquipmentStatusScreen.UpdateViewFromData() — "show cybernetics" / "show equipment"
    [HarmonyPatch(typeof(Qud.UI.InventoryAndEquipmentStatusScreen), "UpdateViewFromData")]
    public static class Patch_InventoryEquipment_UpdateViewFromData
    {
        private static readonly Dictionary<string, string> _replacements = new Dictionary<string, string>
        {
            { "show cybernetics", "사이버네틱스 보기" },
            { "show equipment", "장비 보기" }
        };

        [HarmonyPostfix]
        static void Postfix(Qud.UI.InventoryAndEquipmentStatusScreen __instance)
        {
            try
            {
                TranslateSkinField(__instance, "cyberneticsHotkeySkin");
                TranslateSkinField(__instance, "cyberneticsHotkeySkinForList");

                // Also translate "lbs." in weight text
                var weightField = AccessTools.Field(typeof(Qud.UI.InventoryAndEquipmentStatusScreen), "weightText");
                if (weightField != null)
                {
                    var skin = weightField.GetValue(__instance);
                    if (skin != null)
                    {
                        var textProp = AccessTools.Property(skin.GetType(), "text");
                        if (textProp != null)
                        {
                            string val = textProp.GetValue(skin) as string;
                            if (val != null && val.Contains("lbs."))
                            {
                                val = val.Replace("lbs.", "파운드");
                                textProp.SetValue(skin, val);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] InventoryEquipment 오류: {e.Message}");
            }
        }

        private static void TranslateSkinField(object instance, string fieldName)
        {
            var field = AccessTools.Field(typeof(Qud.UI.InventoryAndEquipmentStatusScreen), fieldName);
            if (field == null) return;
            var skin = field.GetValue(instance);
            if (skin == null) return;
            var textProp = AccessTools.Property(skin.GetType(), "text");
            if (textProp == null) return;
            string val = textProp.GetValue(skin) as string;
            if (val == null) return;

            foreach (var kv in _replacements)
            {
                if (val.Contains(kv.Key))
                    val = val.Replace(kv.Key, kv.Value);
            }
            textProp.SetValue(skin, val);
        }
    }
}
