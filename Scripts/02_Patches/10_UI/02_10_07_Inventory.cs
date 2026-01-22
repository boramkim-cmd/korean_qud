/*
 * 파일명: 10_07_P_Inventory.cs
 * 분류: [UI Patch] 인벤토리 및 장비창 번역
 * 역할: 인벤토리 화면의 메뉴, 카테고리, 도움말 텍스트를 번역합니다.
 *       (Qud.UI.InventoryAndEquipmentStatusScreen 및 관련 클래스 대상)
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Qud.UI;
using XRL.World;
using XRL.UI.Framework;
using QudKRTranslation.Core;
using UnityEngine;
using GameObject = XRL.World.GameObject;

namespace QudKRTranslation.Patches.UI
{
    // ========================================================================
    // 1. 인벤토리 카테고리 공통 번역 (Weapons, Armor 등)
    // ========================================================================
    [HarmonyPatch(typeof(GameObject), "GetInventoryCategory")]
    public static class Patch_GameObject_GetInventoryCategory
    {
        [HarmonyPostfix]
        static void Postfix(ref string __result)
        {
            if (string.IsNullOrEmpty(__result)) return;
            
            // "Weapons", "Armor" 등을 "inventory" 카테고리에서 찾음
            if (LocalizationManager.TryGetAnyTerm(__result.ToLowerInvariant(), out string translated, "inventory"))
            {
                __result = translated;
            }
        }
    }

    // ========================================================================
    // 2. 최신 UI (InventoryAndEquipmentStatusScreen) 메뉴 옵션 번역
    // ========================================================================
    [HarmonyPatch(typeof(InventoryAndEquipmentStatusScreen), "ShowScreen")]
    public static class Patch_InventoryScreen_ShowScreen
    {
        [HarmonyPrefix]
        static void Prefix(InventoryAndEquipmentStatusScreen __instance)
        {
            TranslateMenuOptions(__instance);
        }

        static void TranslateMenuOptions(InventoryAndEquipmentStatusScreen screen)
        {
            if (screen == null) return;

            // 하단 메뉴 옵션들 번역
            TranslateOption(screen.CMD_SHOWCYBERNETICS);
            TranslateOption(screen.CMD_OPTIONS);
            TranslateOption(screen.SET_PRIMARY_LIMB);
            TranslateOption(screen.SHOW_TOOLTIP);
            TranslateOption(screen.QUICK_DROP);
            TranslateOption(screen.QUICK_EAT);
            TranslateOption(screen.QUICK_DRINK);
            TranslateOption(screen.QUICK_APPLY);
        }

        static void TranslateOption(MenuOption option)
        {
            if (option == null || string.IsNullOrEmpty(option.Description)) return;
            
            // "inventory" 및 "ui" 카테고리에서 검색
            if (LocalizationManager.TryGetAnyTerm(option.Description.ToLowerInvariant(), out string translated, "inventory", "ui"))
            {
                option.Description = translated;
            }
        }
    }

    // ========================================================================
    // 3. 인벤토리 필터 탭 번역 (*All, Weapons...)
    // ========================================================================
    // UpdateViewFromData 메서드에서 filterBarCategories를 채우는 로직이 있음.
    // 하지만 GameObject.GetInventoryCategory를 이미 패치했으므로, 필터 바에도 자동으로 번역된 텍스트가 들어갈 가능성이 높음.
    // 다만 "*All" 같은 특수 항목은 별도 처리가 필요할 수 있음.

    [HarmonyPatch(typeof(InventoryAndEquipmentStatusScreen), "UpdateViewFromData")]
    public static class Patch_InventoryScreen_UpdateView
    {
        [HarmonyPostfix]
        static void Postfix(InventoryAndEquipmentStatusScreen __instance)
        {
            try
            {
                // FilterBar에 접근
                var tr = Traverse.Create(__instance);
                var filterBar = tr.Field("filterBar").GetValue<UnityEngine.Component>(); 
                // 정확한 타입은 FilterBar이지만 public이 아닐 수 있음. Component로 받고 Traverse로 다시 접근.
                
                if (filterBar != null)
                {
                    var barTr = Traverse.Create(filterBar);
                    var buttons = barTr.Field("categoryButtons").GetValue<System.Collections.IList>();
                    
                    if (buttons != null)
                    {
                        foreach (var btn in buttons)
                        {
                            if (btn == null) continue;
                            var btnTr = Traverse.Create(btn);
                            
                            // FilterBarCategoryButton has a 'public UITextSkin text;' field
                            var textSkin = btnTr.Field("text").GetValue() as XRL.UI.UITextSkin;
                            if (textSkin == null) continue;

                            string rawText = textSkin.text;
                            if (!string.IsNullOrEmpty(rawText))
                            {
                                // "*All" -> "전체"
                                // "Weapons" -> "무기"
                                if (rawText == "*All" || rawText == "*all")
                                {
                                    if (LocalizationManager.TryGetAnyTerm("*all", out string tAll, "inventory", "ui"))
                                    {
                                        textSkin.SetText(tAll);
                                    }
                                }
                                else if (LocalizationManager.TryGetAnyTerm(rawText.ToLowerInvariant(), out string translated, "inventory"))
                                {
                                    textSkin.SetText(translated);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // UI 접근 실패 시 게임 진행에는 영향 없도록
                UnityEngine.Debug.LogWarning($"[Qud-KR] Inventory FilterBar Patch Failed: {ex.Message}");
            }
        }
    }
    
    // ========================================================================
    // 4. 인벤토리 아이템 이름 번역 (InventoryLine.setData 패치)
    // ========================================================================
    [HarmonyPatch(typeof(InventoryLine), "setData")]
    public static class Patch_InventoryLine_SetData
    {
        [HarmonyPostfix]
        static void Postfix(InventoryLine __instance, FrameworkDataElement data)
        {
            try
            {
                if (data is InventoryLineData inventoryLineData && !inventoryLineData.category)
                {
                    // 아이템인 경우에만 처리
                    var go = inventoryLineData.go;
                    if (go == null) return;
                    
                    string blueprint = go.Blueprint;
                    string currentDisplayName = inventoryLineData.displayName;
                    
                    if (string.IsNullOrEmpty(currentDisplayName)) return;
                    
                    // ObjectTranslator를 통해 번역 시도
                    if (QudKorean.Objects.ObjectTranslator.TryGetDisplayName(blueprint, currentDisplayName, out string translated))
                    {
                        // UITextSkin.text를 직접 업데이트
                        if (__instance.text != null)
                        {
                            __instance.text.SetText(translated);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[Qud-KR] InventoryLine setData patch error: {ex.Message}");
            }
        }
    }
}
