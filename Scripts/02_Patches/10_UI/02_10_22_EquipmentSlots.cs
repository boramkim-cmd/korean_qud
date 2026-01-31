// 분류: UI 패치
// 역할: 장비 슬롯 표시명 (BodyPart.GetCardinalDescription 결과)을 한글로 교체

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using QudKRTranslation.Core;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(XRL.World.Anatomy.BodyPart), nameof(XRL.World.Anatomy.BodyPart.GetCardinalDescription))]
    public static class Patch_BodyPart_GetCardinalDescription
    {
        private static Dictionary<string, string> _bodyParts;

        // Laterality adjectives from Laterality.BuildLateralityAdjective
        private static readonly Dictionary<string, string> _laterality = new Dictionary<string, string>
        {
            { "Left", "왼쪽" },
            { "Right", "오른쪽" },
            { "Upper", "상부" },
            { "Lower", "하부" },
            { "Fore", "전방" },
            { "Mid", "중앙" },
            { "Hind", "후방" },
            { "Inside", "안쪽" },
            { "Outside", "바깥쪽" },
            { "Inner", "내부" },
            { "Outer", "외부" }
        };

        [HarmonyPostfix]
        static void Postfix(XRL.World.Anatomy.BodyPart __instance, ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(__result)) return;
                if (_bodyParts == null)
                    _bodyParts = LocalizationManager.GetCategory("body_parts");
                if (_bodyParts == null) return;

                // Strip color tags to check content, but apply to colored result
                // GetCardinalDescription returns: {{color|[DescriptionPrefix] [Laterality] Description (N)}}
                // We translate inside the color wrapper

                // Try direct match first (handles simple cases like "Head", "Body")
                // Need to handle color-wrapped strings: extract inner text
                string inner = __result;
                string prefix = "";
                string suffix = "";

                int pipeIdx = inner.IndexOf('|');
                int closeIdx = inner.LastIndexOf("}}");
                if (pipeIdx >= 0 && closeIdx > pipeIdx && inner.StartsWith("{{"))
                {
                    prefix = inner.Substring(0, pipeIdx + 1);
                    suffix = inner.Substring(closeIdx);
                    inner = inner.Substring(pipeIdx + 1, closeIdx - pipeIdx - 1);
                }

                // Extract position suffix like " (2)"
                string posSuffix = "";
                var posMatch = System.Text.RegularExpressions.Regex.Match(inner, @" \(\d+\)$");
                if (posMatch.Success)
                {
                    posSuffix = posMatch.Value;
                    inner = inner.Substring(0, inner.Length - posMatch.Length);
                }

                // Try full match
                if (_bodyParts.TryGetValue(inner, out var fullKo))
                {
                    __result = prefix + fullKo + posSuffix + suffix;
                    return;
                }

                // Component translation: split by spaces and translate parts
                string translated = inner;

                // Translate DescriptionPrefix (e.g., "Missile Weapon")
                if (__instance.DescriptionPrefix != null && _bodyParts.TryGetValue(__instance.DescriptionPrefix, out var prefixKo))
                {
                    translated = translated.Replace(__instance.DescriptionPrefix, prefixKo);
                }

                // Translate laterality adjectives
                foreach (var kv in _laterality)
                {
                    if (translated.Contains(kv.Key + " ") || translated.Contains(kv.Key + "-"))
                    {
                        translated = translated.Replace(kv.Key + " ", kv.Value + " ");
                        translated = translated.Replace(kv.Key + "-", kv.Value + "-");
                    }
                }

                // Translate body part description (the base type)
                var typeModel = __instance.VariantTypeModel();
                if (typeModel != null && _bodyParts.TryGetValue(typeModel.Description, out var typeKo))
                {
                    // Replace the base description, but be careful not to replace parts already translated
                    if (translated.Contains(typeModel.Description))
                        translated = translated.Replace(typeModel.Description, typeKo);
                }

                if (translated != inner)
                {
                    __result = prefix + translated + posSuffix + suffix;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] GetCardinalDescription Postfix 오류: {e.Message}");
            }
        }
    }
}
