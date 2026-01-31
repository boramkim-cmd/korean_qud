// 분류: UI 패치
// 역할: AbilityBar의 영어 UI 텍스트를 한글로 교체 (ACTIVE EFFECTS, TARGET, ABILITIES, 토글 상태)
// 참고: effectText/targetText는 AfterRender에서 빌드 후 dirty flag → Update()에서 SetText() 호출
//       따라서 AfterRender postfix에서 string field를 번역하면 Update()가 번역된 값을 UI에 적용함

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using XRL.UI;

namespace QudKRTranslation.Patches
{
    // AfterRender에서 effectText + targetText 모두 번역
    // AfterRender가 effectText를 매 프레임 재빌드하므로 여기서 처리해야 함
    [HarmonyPatch(typeof(AbilityBar), "AfterRender")]
    public static class Patch_AbilityBar_AfterRender
    {
        private static FieldInfo _effectTextField;
        private static FieldInfo _targetTextField;

        [HarmonyPostfix]
        static void Postfix(AbilityBar __instance)
        {
            try
            {
                // ACTIVE EFFECTS: → 활성 효과:
                if (_effectTextField == null)
                    _effectTextField = AccessTools.Field(typeof(AbilityBar), "effectText");
                if (_effectTextField != null)
                {
                    string val = _effectTextField.GetValue(__instance) as string;
                    if (val != null && val.Contains("ACTIVE EFFECTS:"))
                        _effectTextField.SetValue(__instance, val.Replace("ACTIVE EFFECTS:", "활성 효과:"));
                }

                // TARGET: → 대상:, [none] → [없음]
                if (_targetTextField == null)
                    _targetTextField = AccessTools.Field(typeof(AbilityBar), "targetText");
                if (_targetTextField != null)
                {
                    string val = _targetTextField.GetValue(__instance) as string;
                    if (val != null && val.Contains("TARGET:"))
                    {
                        val = val.Replace("TARGET:", "대상:");
                        val = val.Replace("[none]", "[없음]");
                        _targetTextField.SetValue(__instance, val);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] AbilityBar AfterRender 오류: {e.Message}");
            }
        }
    }

    // ABILITIES, page X of Y, [disabled]
    [HarmonyPatch(typeof(AbilityBar), "UpdateAbilitiesText")]
    public static class Patch_AbilityBar_Abilities
    {
        private static FieldInfo _abilityCommandTextField;
        private static MethodInfo _setTextMethod;

        private static readonly Dictionary<string, string> _replacements = new Dictionary<string, string>
        {
            { "ABILITIES", "능력" },
            { "[disabled]", "[비활성]" }
        };

        [HarmonyPostfix]
        static void Postfix(AbilityBar __instance)
        {
            try
            {
                // AbilityCommandText는 public UITextSkin field
                if (_abilityCommandTextField == null)
                    _abilityCommandTextField = AccessTools.Field(typeof(AbilityBar), "AbilityCommandText");
                if (_abilityCommandTextField == null) return;

                var skin = _abilityCommandTextField.GetValue(__instance);
                if (skin == null) return;

                // UITextSkin.text는 public field (property 아님)
                var textField = AccessTools.Field(skin.GetType(), "text");
                if (textField == null) return;
                string val = textField.GetValue(skin) as string;
                if (val == null) return;

                string original = val;
                foreach (var kv in _replacements)
                    val = val.Replace(kv.Key, kv.Value);

                // "page X of Y" → "X/Y 페이지"
                if (val.Contains("page "))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(val, @"page (\d+) of (\d+)");
                    if (match.Success)
                        val = val.Replace(match.Value, $"{match.Groups[1].Value}/{match.Groups[2].Value} 페이지");
                }

                if (val != original)
                {
                    // SetText()를 호출해야 formattedText 캐시가 리셋됨
                    if (_setTextMethod == null)
                        _setTextMethod = AccessTools.Method(skin.GetType(), "SetText", new Type[] { typeof(string) });
                    if (_setTextMethod != null)
                        _setTextMethod.Invoke(skin, new object[] { val });
                    else
                        textField.SetValue(skin, val); // fallback
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] AbilityBar Abilities 오류: {e.Message}");
            }
        }
    }
}
