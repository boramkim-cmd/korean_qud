/*
 * 파일명: 10_15_P_EmbarkOverlay.cs
 * 분류: [UI Patch] 캐릭터 생성 오버레이 (Back/Next 버튼)
 * 역할: 캐릭터 생성 화면 하단의 'Back', 'Next' 공통 버튼 텍스트를 번역합니다.
 */

using HarmonyLib;
using XRL.CharacterBuilds.UI;
using QudKRTranslation.Core;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(EmbarkBuilderOverlayWindow))]
    public static class Patch_EmbarkBuilderOverlayWindow
    {
        [HarmonyPatch(nameof(EmbarkBuilderOverlayWindow.BeforeShowWithWindow))]
        [HarmonyPrefix]
        static void BeforeShowWithWindow_Prefix()
        {
            // Static MenuOption들을 번역
            if (LocalizationManager.TryGetAnyTerm("back", out string backText, "chargen_ui", "common", "ui"))
            {
                EmbarkBuilderOverlayWindow.BackMenuOption.Description = backText;
            }
            
            if (LocalizationManager.TryGetAnyTerm("next", out string nextText, "chargen_ui", "common", "ui"))
            {
                EmbarkBuilderOverlayWindow.NextMenuOption.Description = nextText;
            }
        }
    }
}
