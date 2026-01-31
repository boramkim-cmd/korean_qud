// 분류: UI 패치
// 역할: Statistic.GetStatShortName() 반환값을 한글 약어로 교체
// 범위: 속성 박스, HUD 보조 스탯(QN/MS/AV/DV/MA), 저항 박스 등

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(XRL.World.Statistic), "GetStatShortName", new Type[] { typeof(string) })]
    public static class Patch_Statistic_GetStatShortName
    {
        private static readonly Dictionary<string, string> _shortNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // 주요 속성
            { "Strength", "힘" },
            { "Agility", "민첩" },
            { "Toughness", "건강" },
            { "Intelligence", "지능" },
            { "Willpower", "의지" },
            { "Ego", "자아" },
            // 보조 속성
            { "Speed", "속도" },
            { "MoveSpeed", "이속" },
            { "AV", "방어" },
            { "DV", "회피" },
            { "MA", "정방" },
            { "Hitpoints", "체력" },
            // 저항
            { "AcidResistance", "산저" },
            { "ElectricResistance", "전저" },
            { "ColdResistance", "냉저" },
            { "HeatResistance", "열저" }
        };

        [HarmonyPostfix]
        static void Postfix(string Name, ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(Name)) return;
                if (_shortNames.TryGetValue(Name, out var ko))
                    __result = ko;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] GetStatShortName Postfix 오류: {e.Message}");
            }
        }
    }
}
