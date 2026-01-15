/*
 * 파일명: 20_00_P_InventoryUI.cs
 * 분류: [UI] 인벤토리 화면 번역
 * 역할: 인벤토리 화면(InventoryScreen)의 카테고리 및 고유 UI 텍스트를 번역합니다.
 *       전역 패치인 ScreenBuffer의 범위를 제한하는 대신, 인벤토리 전용 로직에서 번역을 처리합니다.
 * 수정일: 2026-01-14
 */

using HarmonyLib;
using UnityEngine;
using XRL.UI;
using System.Collections.Generic;
using ConsoleLib.Console;

namespace QudKRContent
{
    [HarmonyPatch(typeof(InventoryScreen))]
    public static class Patch_InventoryUI
    {
        private static readonly Dictionary<string, string>[] Scopes = new[] { DictDB.Inventory, DictDB.Common };

        // 1. 인벤토리 카테고리 이름 번역
        [HarmonyPatch("RebuildLists")]
        [HarmonyPrefix]
        static void RebuildLists_Prefix(XRL.World.GameObject GO)
        {
        }

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

    // 2. 모던 인벤토리/장비창 (InventoryAndEquipmentStatusScreen)
    [HarmonyPatch(typeof(Qud.UI.InventoryAndEquipmentStatusScreen))]
    public static class Patch_InventoryAndEquipmentUI
    {
        private static readonly Dictionary<string, string>[] Scopes = new[] { DictDB.Inventory, DictDB.Common };

        [HarmonyPatch("ShowScreen", new System.Type[] { typeof(XRL.World.GameObject), typeof(Qud.UI.StatusScreensScreen) })]
        [HarmonyPrefix]
        static void ShowScreen_Prefix()
        {
            TranslationScopeState.CurrentScope = Scopes;
        }

        [HarmonyPatch("ShowScreen", new System.Type[] { typeof(XRL.World.GameObject), typeof(Qud.UI.StatusScreensScreen) })]
        [HarmonyPostfix]
        static void ShowScreen_Postfix()
        {
            TranslationScopeState.CurrentScope = null;
        }

        // 탭 이름 번역 (Equipment -> 장비)
        [HarmonyPatch("GetTabString")]
        [HarmonyPrefix]
        static bool GetTabString_Prefix(Qud.UI.InventoryAndEquipmentStatusScreen __instance, ref string __result)
        {
            if (XRL.UI.Media.sizeClass < XRL.UI.Media.SizeClass.Medium)
            {
                __result = "장비"; // Eq
            }
            else
            {
                __result = "장비"; // Equipment
            }
            return false; // Skip original
        }

        // 무게 및 가격 표시 형식 번역
        [HarmonyPatch("UpdateViewFromData")]
        [HarmonyPostfix]
        static void UpdateView_Postfix(Qud.UI.InventoryAndEquipmentStatusScreen __instance)
        {
            var go = __instance.GO;
            if (go == null) return;

            // Price Text
            // Original: "{{{{B|${GO.GetFreeDrams()}}}}}"
            // $ is fine, Drams are currency.
            // Maybe we want "{{{{B|{GO.GetFreeDrams()} 드램}}}}" ? 
            // Original keeps $. Let's keep it for now as it's iconic or simple.
            // But Weight needs translation.
            
            // Weight Text
            // Original: "{{{{C|{GO.GetCarriedWeight()}{{{{K|/{GO.GetMaxCarriedWeight()}}}}} lbs. }}}}"
            // Translate "lbs." -> "파운드"
            
            __instance.weightText.SetText($"{{{{C|{go.GetCarriedWeight()}{{{{K|/{go.GetMaxCarriedWeight()}}}}} 파운드 }}}}");
        }
    }
}
