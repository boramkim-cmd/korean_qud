/*
 * 파일명: 20_01_P_TradeUI.cs
 * 분류: [UI] 거래 화면 번역
 * 역할: 거래 화면(TradeScreen)의 고유 UI 텍스트를 번역합니다.
 *       전역 패치인 ScreenBuffer/UITextSkin의 범위를 Trade 딕셔너리로 한정합니다.
 * 수정일: 2026-01-14
 */

using HarmonyLib;
using Qud.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using XRL.World;

namespace QudKRContent
{
    [HarmonyPatch(typeof(TradeScreen))]
    public static class Patch_TradeUI
    {
        private static readonly Dictionary<string, string>[] Scopes = new[] { DictDB.Trade, DictDB.Common };

        [HarmonyPatch("showScreen")]
        [HarmonyPrefix]
        static void showScreen_Prefix()
        {
            TranslationScopeState.CurrentScope = Scopes;
        }

        [HarmonyPatch("showScreen")]
        [HarmonyPostfix]
        static void showScreen_Postfix()
        {
            TranslationScopeState.CurrentScope = null;
        }

        // 추가적으로 TradeScreen의 특수 프로퍼티나 텍스트 필드를 직접 번역할 수 있습니다.
        [HarmonyPatch("UpdateTitleBars")]
        [HarmonyPrefix]
        static void UpdateTitleBars_Prefix(TradeScreen __instance)
        {
            if (__instance == null) return;
        }
    }
}
