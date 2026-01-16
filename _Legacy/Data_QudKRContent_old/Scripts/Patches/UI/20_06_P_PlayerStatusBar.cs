/*
 * 파일명: 20_06_P_PlayerStatusBar.cs
 * 분류: [UI] 상단 HUD 및 상태바 번역
 * 역할: 화면 상단의 HP, 레벨, 경험치, 무게, 배고픔/목마름 상태 등을 번역합니다.
 * 수정일: 2026-01-14
 */

using HarmonyLib;
using Qud.UI;
using System.Text;
using UnityEngine;
using XRL.UI;
using System;
using System.Reflection;

namespace QudKRContent
{
    // StringDataType 사용 부분은 리플렉션으로 해결해야 하므로 별도 클래스로 분리
    // TargetMethod를 사용하여 private/nested type 파라미터를 가진 메서드를 타겟팅
    [HarmonyPatch]
    public static class Patch_PlayerStatusBar_UpdateString_SB
    {
        static MethodBase TargetMethod()
        {
            Type pbType = typeof(PlayerStatusBar);
            Type enumType = pbType.GetNestedType("StringDataType", BindingFlags.NonPublic | BindingFlags.Public);
            if (enumType == null) return null;

            return AccessTools.Method(pbType, "UpdateString", new Type[] { enumType, typeof(StringBuilder), typeof(bool) });
        }

        static void Prefix(object type, StringBuilder data)
        {
            if (data == null) return;
            Patch_PlayerStatusBar_Helper.TranslateStatusBarData(type.ToString(), data);
        }
    }

    [HarmonyPatch]
    public static class Patch_PlayerStatusBar_UpdateString_String
    {
        static MethodBase TargetMethod()
        {
            Type pbType = typeof(PlayerStatusBar);
            Type enumType = pbType.GetNestedType("StringDataType", BindingFlags.NonPublic | BindingFlags.Public);
            if (enumType == null) return null;

            return AccessTools.Method(pbType, "UpdateString", new Type[] { enumType, typeof(string), typeof(bool) });
        }

        static void Prefix(object type, ref string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            
            // string data 처리 로직 (필요 시 구현)
            // 여기서는 StringBuilder 로직만 중요하므로 패스
        }
    }

    [HarmonyPatch(typeof(PlayerStatusBar))]
    public static class Patch_PlayerStatusBar_Update
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void Update_Postfix(PlayerStatusBar __instance)
        {
            if (__instance.XPBar != null && __instance.XPBar.text != null)
            {
                string text = __instance.XPBar.text.text;
                if (text.Contains("LVL:"))
                {
                    string newText = text.Replace("LVL:", "레벨:")
                                         .Replace("Exp:", "경험치:");
                    
                    if (newText != text)
                    {
                        __instance.XPBar.text.SetText(newText);
                    }
                }
            }
        }
    }

    public static class Patch_PlayerStatusBar_Helper
    {
        public static void TranslateStatusBarData(string typeName, StringBuilder sb)
        {
            switch (typeName)
            {
                case "HPBar":
                    sb.Replace("HP:", "체력:");
                    break;
                case "FoodWater":
                    sb.Replace("Sated", "포만")
                      .Replace("Hungry", "배고픔")
                      .Replace("Famished", "기아")
                      .Replace("Starving", "아사 직전")
                      .Replace("Bloated", "과식")
                      .Replace("Hydrated", "수분 충분")
                      .Replace("Thirsty", "목마름")
                      .Replace("Dehydrated", "탈수")
                      .Replace("Parched", "갈증 심함")
                      .Replace("Wet", "젖음")
                      .Replace("Slimy", "점액");
                    break;
                case "Temp":
                    sb.Replace("T:", "온도:");
                    break;
                case "Weight":
                    // sb.Replace("#", " lbs"); 
                    break;
            }
        }
    }
}
