/*
 * 파일명: MainMenu_Hide_Patch.cs
 * 분류: [UI Patch] 메인 메뉴 패치 (Part 2: Hide)
 * 역할: 메인 메뉴가 닫힐 때(Hide) 번역 범위를 해제합니다.
 *       Harmony 스타일 혼용 문제를 피하기 위해 별도 클래스로 분리되었습니다.
 * 작성일: 2026-01-15
 */

using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    // Hide 메서드는 명확하므로 정적 타겟팅 사용
    [HarmonyPatch(typeof(Qud.UI.MainMenu), "Hide")]
    public static class MainMenu_Hide_Patch
    {
        [HarmonyPostfix]
        static void Hide_Postfix()
        {
            if (ScopeManager.IsScopeActive(Data.MainMenuData.Translations))
            {
                ScopeManager.PopScope();
                Debug.Log("[MainMenu_Hide_Patch] Scope deactivated");
            }
        }
    }
}
