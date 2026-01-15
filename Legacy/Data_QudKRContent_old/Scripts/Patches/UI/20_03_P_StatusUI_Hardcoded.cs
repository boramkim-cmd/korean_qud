/*
 * 파일명: 20_03_P_StatusUI_Hardcoded.cs
 * 분류: [UI] 상태창 하드코딩 텍스트 번역
 * 역할: 
 *   1. QuestsLine: "You have no active quests."
 *   2. JournalScreen: 탭 이름 (Locations, Sultans 등)
 *   3. JournalStatusScreen: "No entries found."
 *   4. FactionsLine: "Reputation: "
 *   5. StatusScreensScreen: 네비게이션 메뉴 (navigation, Accept)
 * 수정일: 2026-01-14
 */

using HarmonyLib;
using System.Collections.Generic;
using Qud.UI;
using XRL.UI;
using XRL.World;

namespace QudKRContent
{
    // 1. 퀘스트 라인: "You have no active quests."
    [HarmonyPatch(typeof(Qud.UI.QuestsLine))]
    public static class Patch_QuestsLine
    {
        [HarmonyPatch("setData")]
        [HarmonyPostfix]
        static void setData_Postfix(Qud.UI.QuestsLine __instance, XRL.UI.Framework.FrameworkDataElement data)
        {
            if (data is QuestsLineData qData && qData.quest == null)
            {
                __instance.titleText.SetText("진행 중인 퀘스트가 없습니다.");
            }
        }
    }

    // 2. 저널 스크린: 탭 이름 (Locations, Sultans 등)
    [HarmonyPatch(typeof(XRL.UI.JournalScreen))]
    public static class Patch_JournalScreen
    {
        [HarmonyPatch("GetTabDisplayName")]
        [HarmonyPrefix]
        static bool GetTabDisplayName_Prefix(string Tab, ref string __result)
        {
            if (Tab == JournalScreen.STR_LOCATIONS) __result = "장소";
            else if (Tab == JournalScreen.STR_CHRONOLOGY) __result = "연대기";
            else if (Tab == JournalScreen.STR_OBSERVATIONS) __result = "가십 및 설화";
            else if (Tab == JournalScreen.STR_SULTANS) __result = "술탄의 역사";
            else if (Tab == JournalScreen.STR_VILLAGES) __result = "마을의 역사";
            else if (Tab == JournalScreen.STR_GENERAL) __result = "일반 노트";
            else if (Tab == JournalScreen.STR_RECIPES) __result = "레시피";
            else return true; // 원래 로직 실행 (Sultan의 경우 동적 처리 등)

            // Sultan 탭 등은 동적으로 이름이 바뀔 수 있으므로(GetSultansDisplayName), 
            // 단순 번역이 아닌 경우 원래 로직을 타게 하거나 추가 처리가 필요할 수 있음.
            // 여기서는 STR_SULTANS가 "Sultan Histories"로 고정되어 있다고 가정하고 번역.
            // 만약 동적 이름이 필요하면 추가 로직 필요.
            
            return false;
        }
    }

    // 3. 저널 상태창: "No entries found."
    [HarmonyPatch(typeof(Qud.UI.JournalStatusScreen))]
    public static class Patch_JournalStatusScreen
    {
        [HarmonyPatch("ShowScreen")]
        [HarmonyPrefix]
        static void ShowScreen_Prefix()
        {
            // 정적 필드 값을 덮어씁니다.
            Qud.UI.JournalStatusScreen.NO_ENTRIES_TEXT = " 기록 없음.";
        }
    }

    // 4. 평판 라인: "Reputation: "
    [HarmonyPatch(typeof(Qud.UI.FactionsLine))]
    public static class Patch_FactionsLine
    {
        [HarmonyPatch("setData")]
        [HarmonyPostfix]
        static void setData_Postfix(Qud.UI.FactionsLine __instance, XRL.UI.Framework.FrameworkDataElement data)
        {
            if (data is FactionsLineData fData)
            {
                // FormatFactionReputation은 Public Static이어야 접근 가능.
                // 만약 접근 불가라면 Reflection 필요.
                // 보통 XRL.UI.FactionsScreen.FormatFactionReputation 는 public static.
                string repVal = XRL.UI.FactionsScreen.FormatFactionReputation(fData.id);
                __instance.barReputationText.SetText("평판: " + repVal);
            }
        }
    }

    // 5. 상태창 스크린: 네비게이션 메뉴 (navigation, Accept)
    [HarmonyPatch(typeof(Qud.UI.StatusScreensScreen))]
    public static class Patch_StatusScreensScreen
    {
        [HarmonyPatch("showScreen")]
        [HarmonyPrefix]
        static void showScreen_Prefix(Qud.UI.StatusScreensScreen __instance)
        {
            if (__instance.defaultMenuOptionOrder != null)
            {
                foreach (var option in __instance.defaultMenuOptionOrder)
                {
                    if (option.Description == "navigation") option.Description = "이동";
                    if (option.Description == "Accept") option.Description = "선택";
                }
            }
        }
    }
}
