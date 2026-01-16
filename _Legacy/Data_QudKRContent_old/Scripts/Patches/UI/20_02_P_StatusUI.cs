/*
 * 파일명: 20_02_P_StatusUI.cs
 * 분류: [UI] 상태창 화면 번역
 * 역할: 상태창 화면(StatusScreensScreen)의 고유 UI 텍스트를 번역합니다.
 *       전역 패치인 ScreenBuffer/UITextSkin의 범위를 Status 딕셔너리로 한정합니다.
 * 수정일: 2026-01-14
 */

using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using Qud.UI;
using XRL.UI;
using XRL.World; // For GameObject
using XRL.World.Parts; // For Leveler
using UnityEngine;

namespace QudKRContent
{
    // 1. 모던 상태창 (StatusScreensScreen)
    [HarmonyPatch(typeof(StatusScreensScreen))]
    public static class Patch_StatusScreensUI
    {
        private static readonly Dictionary<string, string>[] Scopes = new[] { DictDB.Status, DictDB.UICommon };

        [HarmonyPatch("showScreen")]
        [HarmonyPrefix]
        static void Show_Prefix()
        {
            TranslationScopeState.CurrentScope = Scopes;
        }

        [HarmonyPatch("showScreen")]
        [HarmonyPostfix]
        static void Show_Postfix()
        {
            TranslationScopeState.CurrentScope = null;
        }
    }

    // 2. 클래식 상태창 (StatusScreen)
    [HarmonyPatch(typeof(StatusScreen))]
    public static class Patch_ClassicStatusUI
    {
        private static readonly Dictionary<string, string>[] Scopes = new[] { DictDB.Status, DictDB.UICommon };

        [HarmonyPatch("Show")]
        [HarmonyPrefix]
        static void Show_Prefix(XRL.World.GameObject GO)
        {
            TranslationScopeState.CurrentScope = Scopes;
        }

        [HarmonyPatch("Show")]
        [HarmonyPostfix]
        static void Show_Postfix()
        {
            TranslationScopeState.CurrentScope = null;
        }
    }

    // 3. 캐릭터 상세 상태창 (CharacterStatusScreen)
    [HarmonyPatch(typeof(CharacterStatusScreen))]
    public static class Patch_CharacterStatusUI
    {
        [HarmonyPatch("GetTabString")]
        [HarmonyPrefix]
        static bool GetTabString_Prefix(ref string __result)
        {
            if (XRL.UI.Media.sizeClass < XRL.UI.Media.SizeClass.Medium)
                __result = "속성";
            else
                __result = "속성 및 권능";
            return false;
        }

        [HarmonyPatch("UpdateViewFromData")]
        [HarmonyPostfix]
        static void UpdateView_Postfix(CharacterStatusScreen __instance)
        {

            var go = Traverse.Create(__instance).Field("GO").GetValue<XRL.World.GameObject>();
            if (go == null) return;

            // Level Header
            // Original: "Level: {0} \u00af HP: {1}/{2} \u00af XP: {3}/{4} \u00af Weight: {5}#"
            string lvl = "레벨";
            string hp = "체력";
            string xp = "경험치";
            string wgt = "무게";

            // Safe access to stats
            int level = go.Stat("Level");
            int hitpoints = go.Stat("Hitpoints");
            int baseHitpoints = go.GetStat("Hitpoints").BaseValue;
            int currentXP = go.Stat("XP");
            int nextLevelXP = XRL.World.Parts.Leveler.GetXPForLevel(level + 1);
            int weight = go.Weight;

            string txt = string.Format("{0}: {1} \u00af {2}: {3}/{4} \u00af {5}: {6}/{7} \u00af {8}: {9}#",
                lvl, level,
                hp, hitpoints, baseHitpoints,
                xp, currentXP, nextLevelXP,
                wgt, weight
            );
            __instance.levelText.SetText(txt);

            // Attribute Points
            // Original: "Attribute Points: {0}{1}}}}}"
            string ap = "속성 포인트";
            int apVal = go.Stat("AP");
            __instance.attributePointsText.SetText(string.Format("{0}: {1}{2}}}}}", ap, (apVal > 0) ? "{{G|" : "{{K|", apVal));

            // Mutation Points
            // Original: "{0} Points: {1}{2}}}}}" or "MP: {0}{1}}}}}"
            int mpVal = go.Stat("MP");
            string mpColor = (mpVal > 0) ? "{{G|" : "{{K|";
            
            if (XRL.UI.Media.sizeClass < XRL.UI.Media.SizeClass.Medium)
            {
               __instance.mutationPointsText.SetText(string.Format("MP: {0}{1}}}}}", mpColor, mpVal));
            }
            else
            {
                string mutationTerm = "변이";
                if (DictDB.TryGetAnyTranslation(__instance.mutationTermCapital, out string t)) mutationTerm = t;
                else if (__instance.mutationTermCapital == "Cybernetics") mutationTerm = "사이버네틱스";

                __instance.mutationPointsText.SetText(string.Format("{0} 포인트: {1}{2}}}}}", mutationTerm, mpColor, mpVal));
            }
        }
    }

    // 4. 스킬/권능 창 (SkillsAndPowersStatusScreen)
    [HarmonyPatch(typeof(Qud.UI.SkillsAndPowersStatusScreen))]
    public static class Patch_SkillsUI
    {
        [HarmonyPatch("GetTabString")]
        [HarmonyPrefix]
        static bool GetTabString_Prefix(ref string __result)
        {
            __result = "스킬";
            return false;
        }

        [HarmonyPatch("ShowScreen")]
        [HarmonyPostfix]
        static void ShowScreen_Postfix(Qud.UI.SkillsAndPowersStatusScreen __instance)
        {
            var go = __instance.GO;
            if (go == null) return;

            // nameBlockText: "{DisplayName}의 스킬"
            __instance.nameBlockText.SetText(go.DisplayName + "의 스킬");

            // statBlockText: STR, AGI... 
            // Original: "{{{{K|{{{{g|STR:}}}}{0} ■ {{{{g|AGI}}}}: {1} ...}}}}"
            var sb = new System.Text.StringBuilder();
            sb.Append("{{K|");
            sb.AppendFormat("{{{{g|힘:}}}}{0} ■ ", go.GetStat("Strength").Value);
            sb.AppendFormat("{{{{g|민첩:}}}}{0} ■ ", go.GetStat("Agility").Value);
            sb.AppendFormat("{{{{g|건강:}}}}{0} ■ ", go.GetStat("Toughness").Value);
            sb.AppendFormat("{{{{g|지능:}}}}{0} ■ ", go.GetStat("Intelligence").Value);
            sb.AppendFormat("{{{{g|의지:}}}}{0} ■ ", go.GetStat("Willpower").Value);
            sb.AppendFormat("{{{{g|자아:}}}}{0}", go.GetStat("Ego").Value);
            sb.Append("}}}}");
            
            __instance.statBlockText.SetText(sb.ToString());
        }

        [HarmonyPatch("UpdateData")]
        [HarmonyPostfix]
        static void UpdateData_Postfix(Qud.UI.SkillsAndPowersStatusScreen __instance)
        {
             // spText: "Skill Points (SP): {{C|{0}}}"
             __instance.spText.SetText(string.Format("스킬 포인트 (SP): {{{{C|{0}}}}}", __instance.GO.GetStat("SP").Value));
        }
    }

    // 5. 기타 상태창 탭 이름 (Quests, Factions, Journal, etc.)
    [HarmonyPatch(typeof(Qud.UI.QuestsStatusScreen), "GetTabString")]
    public static class Patch_QuestsTab { static bool Prefix(ref string __result) { __result = "퀘스트"; return false; } }
    
    [HarmonyPatch(typeof(Qud.UI.FactionsStatusScreen), "GetTabString")]
    public static class Patch_FactionsTab { static bool Prefix(ref string __result) { __result = "평판"; return false; } }
    
    [HarmonyPatch(typeof(Qud.UI.JournalStatusScreen), "GetTabString")]
    public static class Patch_JournalTab { static bool Prefix(ref string __result) { __result = "일지"; return false; } }

    [HarmonyPatch(typeof(Qud.UI.TinkeringStatusScreen), "GetTabString")]
    public static class Patch_TinkeringTab { static bool Prefix(ref string __result) { __result = "팅커링"; return false; } }
    
    [HarmonyPatch(typeof(Qud.UI.MessageLogStatusScreen), "GetTabString")]
    public static class Patch_MessageLogTab { static bool Prefix(ref string __result) { __result = "메시지 기록"; return false; } }

    // 상태창 열릴 때 하단 메뉴(Buy Mutation 등) 번역
    [HarmonyPatch(typeof(CharacterStatusScreen), "Show")]
    public static class Patch_StatusMenu
    {
        [HarmonyPostfix]
        public static void Postfix(CharacterStatusScreen __instance)
        {
            if (__instance == null || __instance.horizNav == null) return;
            
            if (__instance.horizNav.menuOptionDescriptions != null)
            {
                foreach (var option in __instance.horizNav.menuOptionDescriptions)
                {
                    if (option != null && DictDB.Status.TryGetValue(option.DisplayName ?? "", out string translated))
                    {
                        option.DisplayName = translated;
                    }
                }
            }
        }
    }
}
