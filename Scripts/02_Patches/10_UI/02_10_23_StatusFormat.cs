// 분류: UI 패치
// 역할: 상태 화면 포맷 문자열 (Level/HP/XP/Weight, Attribute Points, show cybernetics/equipment)을 한글로 교체
// 참고: UITextSkin.text, UIHotkeySkin.text는 property가 아닌 public field임.
//       값 변경 후 SetText()를 호출해야 formattedText 캐시가 리셋됨.

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    // CharacterStatusScreen.UpdateViewFromData() — Level/HP/XP/Weight line + Attribute Points
    [HarmonyPatch(typeof(Qud.UI.CharacterStatusScreen), "UpdateViewFromData")]
    public static class Patch_CharacterStatusScreen_UpdateViewFromData
    {
        [HarmonyPostfix]
        static void Postfix(Qud.UI.CharacterStatusScreen __instance)
        {
            try
            {
                // levelText: "Level: X ¯ HP: X/X ¯ XP: X/X ¯ Weight: X#"
                StatusFormatExtensions.TranslateUITextSkin(__instance, typeof(Qud.UI.CharacterStatusScreen), "levelText", val =>
                {
                    val = val.Replace("Level:", "레벨:");
                    val = val.Replace("HP:", "체력:");
                    val = val.Replace("XP:", "경험치:");
                    val = val.Replace("Weight:", "무게:");
                    return val;
                });

                // attributePointsText: "Attribute Points: {{G|X}}"
                StatusFormatExtensions.TranslateUITextSkin(__instance, typeof(Qud.UI.CharacterStatusScreen), "attributePointsText", val =>
                {
                    if (val.Contains("Attribute Points:"))
                        val = val.Replace("Attribute Points:", "속성 포인트:");
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] CharacterStatusScreen 오류: {e.Message}");
            }
        }
    }

    // InventoryAndEquipmentStatusScreen.UpdateViewFromData() — "show cybernetics" / "show equipment" / "lbs."
    [HarmonyPatch(typeof(Qud.UI.InventoryAndEquipmentStatusScreen), "UpdateViewFromData")]
    public static class Patch_InventoryEquipment_UpdateViewFromData
    {
        private static readonly Dictionary<string, string> _cyberReplacements = new Dictionary<string, string>
        {
            { "show cybernetics", "사이버네틱스 보기" },
            { "show equipment", "장비 보기" }
        };

        [HarmonyPostfix]
        static void Postfix(Qud.UI.InventoryAndEquipmentStatusScreen __instance)
        {
            try
            {
                var screenType = typeof(Qud.UI.InventoryAndEquipmentStatusScreen);

                // cyberneticsHotkeySkin / cyberneticsHotkeySkinForList (UIHotkeySkin)
                StatusFormatExtensions.TranslateUITextSkin(__instance, screenType, "cyberneticsHotkeySkin", TranslateCyberText);
                StatusFormatExtensions.TranslateUITextSkin(__instance, screenType, "cyberneticsHotkeySkinForList", TranslateCyberText);

                // weightText (UITextSkin): "lbs." → "kg"
                StatusFormatExtensions.TranslateUITextSkin(__instance, screenType, "weightText", val =>
                {
                    if (val.Contains("lbs."))
                        val = val.Replace("lbs.", "kg");
                    return val;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] InventoryEquipment 오류: {e.Message}");
            }
        }

        private static string TranslateCyberText(string val)
        {
            foreach (var kv in _cyberReplacements)
            {
                if (val.Contains(kv.Key))
                    val = val.Replace(kv.Key, kv.Value);
            }
            return val;
        }
    }

    // 공통 헬퍼: UITextSkin/UIHotkeySkin의 text field를 읽고 SetText()로 업데이트
    internal static class UITextSkinHelper
    {
        // text는 UITextSkin/UIHotkeySkin 모두 public field
        // SetText(string)은 두 클래스 모두 구현함
        private static Dictionary<Type, FieldInfo> _textFields = new Dictionary<Type, FieldInfo>();
        private static Dictionary<Type, MethodInfo> _setTextMethods = new Dictionary<Type, MethodInfo>();

        public static void Translate(object skin, Func<string, string> translator)
        {
            if (skin == null) return;
            var skinType = skin.GetType();

            if (!_textFields.TryGetValue(skinType, out var textField))
            {
                textField = AccessTools.Field(skinType, "text");
                _textFields[skinType] = textField;
            }
            if (textField == null) return;

            string val = textField.GetValue(skin) as string;
            if (val == null) return;

            string translated = translator(val);
            if (translated == val) return;

            if (!_setTextMethods.TryGetValue(skinType, out var setTextMethod))
            {
                setTextMethod = AccessTools.Method(skinType, "SetText", new Type[] { typeof(string) });
                _setTextMethods[skinType] = setTextMethod;
            }

            if (setTextMethod != null)
                setTextMethod.Invoke(skin, new object[] { translated });
            else
                textField.SetValue(skin, translated); // fallback
        }
    }

    internal static class StatusFormatExtensions
    {
        public static void TranslateUITextSkin(object instance, Type declaringType, string fieldName, Func<string, string> translator)
        {
            var field = AccessTools.Field(declaringType, fieldName);
            if (field == null) return;
            var skin = field.GetValue(instance);
            UITextSkinHelper.Translate(skin, translator);
        }
    }
}
