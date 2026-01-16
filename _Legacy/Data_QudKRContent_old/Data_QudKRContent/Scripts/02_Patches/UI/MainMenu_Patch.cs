/*
 * 파일명: MainMenu_Patch.cs
 * 분류: [UI Patch] 메인 메뉴 패치 (Part 1: Show)
 * 역할: 메인 메뉴가 열릴 때(Show) 번역 범위를 설정합니다.
 *       이 클래스는 TargetMethod를 사용한 동적 타겟팅만 담당합니다.
 * 
 * 변경사항: Harmony 스타일 혼용 문제(TargetMethod + [HarmonyPatch])를 해결하기 위해
 *          Hide 관련 패치는 MainMenu_Hide_Patch.cs로 분리되었습니다.
 * 작성일: 2026-01-15
 */

using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch]
    public static class MainMenu_Patch
    {
        static MethodBase TargetMethod()
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

            // 시도 2: Qud.UI.MainMenu
            var type2 = AccessTools.TypeByName("Qud.UI.MainMenu");
            if (type2 != null)
            {
                var method = AccessTools.Method(type2, "Show");
                if (method != null)
                {
                    Debug.Log("[MainMenu_Patch] Target: Qud.UI.MainMenu.Show");
                    return method;
                }
            }

            Debug.LogWarning("[MainMenu_Patch] Could not find main menu target method");
            return null;
        }

        [HarmonyPrefix]
        static void Show_Prefix()
        {
            // Note: Uses TranslationEngine indirectly via TranslationUtils in other patches.
            if (!ScopeManager.IsScopeActive(Data.MainMenuData.Translations))
            {
                ScopeManager.PushScope(Data.MainMenuData.Translations, Data.CommonData.Translations);
                Debug.Log("[MainMenu_Patch] Scope activated (Persistent)");
            }
        }
    }
}
