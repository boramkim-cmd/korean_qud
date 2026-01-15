/*
 * 파일명: 20_04_P_StatusUI_Specifics.cs
 * 분류: [Play] 상태창 정밀 번역 (스탯 이름 & 설명)
 * 역할: 
 * 1. AttributeRow: "Strength" 같은 스탯 이름 번역 (색상 태그 유지)
 * 2. HandleHighlightObject: 마우스 호버 시 나오는 상세 설명 번역
 * 수정일: 2026-01-14 (Update)
 */

using HarmonyLib;
using Qud.UI;
using XRL.UI;
using XRL.UI.Framework;
using UnityEngine;

namespace QudKRContent
{
    // 1. [스탯 이름] 화면에 스탯이 그려질 때(SetData) 가로채기
    [HarmonyPatch(typeof(AttributeRow), "SetData")]
    public static class Patch_AttributeRow
    {
        [HarmonyPostfix]
        static void Postfix(AttributeRow __instance)
        {
            if (__instance == null || __instance.title == null) return;

            // 원본 텍스트 (예: "{{W|Strength}}")
            string raw = __instance.title.text; 
            // 색상 태그 제거 (예: "Strength")
            string clean = ColorUtility.StripColorTags(raw).Trim();

            // 사전에서 "Strength" -> "힘" 찾기
            if (DictDB.Status.TryGetValue(clean, out string translated))
            {
                // 원본에 색상 코드(|)가 있었으면, 번역문에도 적용
                if (raw.Contains("|"))
                {
                    // 단순 교체: "{{W|Strength}}" -> "{{W|힘}}"
                    __instance.title.text = raw.Replace(clean, translated);
                }
                else
                {
                    __instance.title.text = translated;
                }
            }
        }
    }

    // 2. [설명 텍스트] 마우스로 항목을 가리킬 때(Hover) 설명 가로채기
    [HarmonyPatch(typeof(SkillsAndPowersStatusScreen), "HandleHighlightObject")]
    public static class Patch_StatusDescription
    {
        [HarmonyPostfix]
        static void Postfix(SkillsAndPowersStatusScreen __instance)
        {
            if (__instance == null || __instance.detailsText == null) return;

            // 현재 화면에 뜬 설명 가져오기
            string currentDesc = __instance.detailsText.text;
            string cleanDesc = ColorUtility.StripColorTags(currentDesc).Trim();

            // 설명 사전(StatusDescriptions) 뒤지기
            foreach (var kvp in DictDB.StatusDescriptions)
            {
                // "Strength determines" 문구가 포함되어 있으면 -> 한글 설명으로 덮어쓰기
                if (cleanDesc.Contains(kvp.Key))
                {
                     __instance.detailsText.SetText(kvp.Value);
                     return; // 하나 찾으면 끝
                }
            }
        }
    }
}
