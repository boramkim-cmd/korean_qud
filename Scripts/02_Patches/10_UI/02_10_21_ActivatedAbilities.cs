// 분류: UI 패치
// 역할: ActivatedAbilityEntry.DisplayName을 한글로 교체

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using QudKRTranslation.Core;
using XRL.World.Parts;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(ActivatedAbilities), nameof(ActivatedAbilities.AddAbility))]
    public static class Patch_ActivatedAbilities_AddAbility
    {
        private static Dictionary<string, string> _abilityNames;

        [HarmonyPostfix]
        static void Postfix(ActivatedAbilities __instance, string Name, ref Guid __result)
        {
            try
            {
                if (_abilityNames == null)
                    _abilityNames = LocalizationManager.GetCategory("ability_names");
                if (_abilityNames == null) return;

                if (__instance.AbilityByGuid.TryGetValue(__result, out var entry))
                {
                    if (_abilityNames.TryGetValue(entry.DisplayName, out var ko))
                        entry.DisplayName = ko;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] AddAbility Postfix 오류: {e.Message}");
            }
        }
    }
}
