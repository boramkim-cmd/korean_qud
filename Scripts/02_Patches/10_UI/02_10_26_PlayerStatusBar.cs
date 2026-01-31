// 분류: UI 패치
// 역할: 상단 HUD 바 (HP, 온도, 날짜, LVL, Exp, 무게) + 배고픔/갈증 상태 텍스트 한글화
// 구조: PlayerStatusBar는 BeginEndTurn()에서 playerStringData dict에 문자열 저장,
//       Update()에서 dict → UI 반영. Prefix로 dict 값을 번역하여 UI에 한글 적용.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    // PlayerStatusBar.Update() Prefix — private playerStringData dict 값을 한글로 변환
    [HarmonyPatch(typeof(Qud.UI.PlayerStatusBar), "Update")]
    public static class Patch_PlayerStatusBar_Update
    {
        private static FieldInfo _dictField;
        private static FieldInfo _xpBarField;
        private static object _hpKey, _tempKey, _timeKey, _weightKey;
        private static bool _initialized;

        static void Init()
        {
            if (_initialized) return;
            var barType = typeof(Qud.UI.PlayerStatusBar);
            _dictField = AccessTools.Field(barType, "playerStringData");
            _xpBarField = AccessTools.Field(barType, "XPBar");

            var enumType = barType.GetNestedType("StringDataType", BindingFlags.NonPublic);
            if (enumType != null)
            {
                _hpKey = Enum.Parse(enumType, "HPBar");
                _tempKey = Enum.Parse(enumType, "Temp");
                _timeKey = Enum.Parse(enumType, "Time");
                _weightKey = Enum.Parse(enumType, "Weight");
            }
            _initialized = true;
        }

        // Prefix: dict 값 번역 (Update가 UI에 반영하기 전)
        [HarmonyPrefix]
        static void Prefix(Qud.UI.PlayerStatusBar __instance)
        {
            try
            {
                Init();
                if (_dictField == null || _hpKey == null) return;

                var dict = _dictField.GetValue(__instance) as IDictionary;
                if (dict == null) return;

                // HP: "{{Y|HP: {{G|19}} / 19}}" → "{{Y|체력: {{G|19}} / 19}}"
                TranslateEntry(dict, _hpKey, "HP: ", "체력: ");

                // 온도: "T:25ø" → "온도:25ø"
                TranslateEntry(dict, _tempKey, "T:", "온도:");

                // 날짜: "Harvest Dawn 22nd of Tuum Ut" → "Harvest Dawn 22nd Tuum Ut"
                TranslateEntry(dict, _timeKey, " of ", " ");

                // 무게: "68/285# {{blue|96$}}" → "68/285kg {{blue|96드램}}"
                TranslateEntry(dict, _weightKey, "#", "kg");
                TranslateEntry(dict, _weightKey, "$", "드램");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] PlayerStatusBar Prefix 오류: {e.Message}");
            }
        }

        // Postfix: XPBar.text (inline 빌드라 dict에 없음)
        [HarmonyPostfix]
        static void Postfix(Qud.UI.PlayerStatusBar __instance)
        {
            try
            {
                // XPBar는 HPBar 타입, text는 UITextSkin 필드
                var xpBar = __instance.XPBar;
                if (xpBar == null) return;

                var textSkin = xpBar.text;
                if (textSkin == null) return;

                string val = textSkin.text;
                if (val == null || !val.Contains("LVL:")) return;

                val = val.Replace("LVL: ", "레벨: ");
                val = val.Replace("Exp: ", "경험: ");
                textSkin.SetText(val);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] PlayerStatusBar XPBar 오류: {e.Message}");
            }
        }

        private static void TranslateEntry(IDictionary dict, object key, string from, string to)
        {
            if (!dict.Contains(key)) return;
            var val = dict[key] as string;
            if (val != null && val.Contains(from))
                dict[key] = val.Replace(from, to);
        }
    }

    // Stomach — 배고픔/갈증 상태 텍스트
    [HarmonyPatch(typeof(XRL.World.Parts.Stomach), "FoodStatus")]
    public static class Patch_Stomach_FoodStatus
    {
        private static readonly Dictionary<string, string> _foodStatus = new Dictionary<string, string>
        {
            { "Sated", "포만" },
            { "Hungry", "배고픔" },
            { "Famished!", "굶주림!" },
            { "Wilted!", "시들음!" }
        };

        [HarmonyPostfix]
        static void Postfix(ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(__result)) return;
                foreach (var kv in _foodStatus)
                {
                    if (__result.Contains(kv.Key))
                    {
                        __result = __result.Replace(kv.Key, kv.Value);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] FoodStatus 오류: {e.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(XRL.World.Parts.Stomach), "WaterStatus")]
    public static class Patch_Stomach_WaterStatus
    {
        private static readonly Dictionary<string, string> _waterStatus = new Dictionary<string, string>
        {
            { "Quenched", "해갈" },
            { "Thirsty", "목마름" },
            { "Parched", "극갈" },
            { "Dehydrated!", "탈수!" },
            { "Tumescent", "과수분" }
        };

        [HarmonyPostfix]
        static void Postfix(ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(__result)) return;
                foreach (var kv in _waterStatus)
                {
                    if (__result.Contains(kv.Key))
                    {
                        __result = __result.Replace(kv.Key, kv.Value);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] WaterStatus 오류: {e.Message}");
            }
        }
    }
}
