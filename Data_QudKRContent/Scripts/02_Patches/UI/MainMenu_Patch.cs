/*
 * 파일명: MainMenu_Patch.cs
 * 분류: [UI Patch] 메인 메뉴 패치
 * 역할: 메인 메뉴 화면이 열릴 때 번역 범위를 설정합니다.
 * 작성일: 2026-01-15
 */

using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    /// <summary>
    /// 메인 메뉴 패치
    /// 메인 메뉴 화면이 열릴 때 MainMenuData + CommonData 범위를 활성화합니다.
    /// </summary>
    [HarmonyPatch]
    public static class MainMenu_Patch
    {
        /// <summary>
        /// 패치 대상 메서드를 동적으로 결정합니다.
        /// 게임 버전에 따라 메인 메뉴 클래스명이 다를 수 있으므로 여러 시도를 합니다.
        /// </summary>
        static System.Reflection.MethodBase TargetMethod()
        {
            // 시도 1: XRL.UI.MainMenu
            var type1 = AccessTools.TypeByName("XRL.UI.MainMenu");
            if (type1 != null)
            {
                var method = AccessTools.Method(type1, "Show");
                if (method != null)
                {
                    Debug.Log("[MainMenu_Patch] Target: XRL.UI.MainMenu.Show");
                    return method;
                }
            }
            
            // 시도 2: Qud.UI.MainMenuScreen
            var type2 = AccessTools.TypeByName("Qud.UI.MainMenuScreen");
            if (type2 != null)
            {
                var method = AccessTools.Method(type2, "Show");
                if (method != null)
                {
                    Debug.Log("[MainMenu_Patch] Target: Qud.UI.MainMenuScreen.Show");
                    return method;
                }
            }
            
            Debug.LogWarning("[MainMenu_Patch] Could not find main menu target method");
            return null;
        }
        
        /// <summary>
        /// 메인 메뉴 열릴 때: 범위 설정
        /// </summary>
        [HarmonyPrefix]
        static void Show_Prefix()
        {
            // MainMenuData를 우선, CommonData를 보조로 사용
            ScopeManager.PushScope(Data.MainMenuData.Translations, Data.CommonData.Translations);
            Debug.Log("[MainMenu_Patch] Scope activated");
        }
        
        /// <summary>
        /// 메인 메뉴 닫힐 때: 범위 해제
        /// </summary>
        [HarmonyPostfix]
        static void Show_Postfix()
        {
            ScopeManager.PopScope();
            Debug.Log("[MainMenu_Patch] Scope deactivated");
        }
        
        /// <summary>
        /// 예외 발생 시에도 범위 정리
        /// </summary>
        [HarmonyFinalizer]
        static void Show_Finalizer()
        {
            // Postfix가 호출되지 않은 경우에만 정리
            if (ScopeManager.GetDepth() > 0)
            {
                Debug.LogWarning("[MainMenu_Patch] Finalizer cleaning up scope");
                ScopeManager.PopScope();
            }
        }
    }
}
