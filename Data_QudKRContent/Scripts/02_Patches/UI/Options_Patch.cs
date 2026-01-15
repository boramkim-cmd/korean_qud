/*
 * 파일명: Options_Patch.cs
 * 분류: [UI Patch] 설정 화면 패치
 * 역할: 설정(Options) 화면이 열리고 닫힐 때 번역 범위를 관리합니다.
 */

using HarmonyLib;
using Qud.UI;
using UnityEngine;
using QudKRTranslation.Data;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(OptionsScreen))]
    public static class Options_Patch
    {
        [HarmonyPatch(nameof(OptionsScreen.Show), new System.Type[0])]
        [HarmonyPrefix]
        static void Show_Prefix()
        {
            // OptionsScope가 이미 활성화되어 있지 않다면 푸시
            if (!ScopeManager.IsScopeActive(OptionsData.Translations))
            {
                ScopeManager.PushScope(OptionsData.Translations, CommonData.Translations);
                Debug.Log("[Options_Patch] Scope activated");
            }
        }

        [HarmonyPatch(nameof(OptionsScreen.Hide), new System.Type[0])]
        [HarmonyPostfix]
        static void Hide_Postfix()
        {
            // 현재 스코프가 OptionsData면 제거
            if (ScopeManager.IsScopeActive(OptionsData.Translations))
            {
                ScopeManager.PopScope();
                Debug.Log("[Options_Patch] Scope deactivated");
            }
        }
    }
}
