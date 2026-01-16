/*
 * 파일명: 10_01_P_OptionsUI.cs
 * 분류: [Menu] 설정창 UI 번역
 * 역할: 데이터 레벨에서 번역할 수 없는 'Category' 이름을 UI 출력 시점에 번역합니다.
 *       (Category는 데이터 레벨에서 번역 시 내부 딕셔너리 키 충돌로 크래시가 발생함)
 * 수정일: 2026-01-14
 */

using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using XRL.UI.Framework;

namespace QudKRContent
{
    // 1. 설정창 메인 리스트의 카테고리 헤더 번역
    [HarmonyPatch]
    public static class Patch_OptionsUI
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method("Qud.UI.OptionsScreen:Update");
        }

        [HarmonyPostfix]
        static void Postfix(Component __instance)
        {
            if (__instance == null || !__instance.gameObject.activeInHierarchy) return;

            // 모든 설정 관련 딕셔너리를 포함하는 스코프 구성
            var scopes = new[] { 
                DictDB.Options, DictDB.Options_Sound, DictDB.Options_Display, DictDB.Options_Controls,
                DictDB.Options_Accessibility, DictDB.Options_UI, DictDB.Options_Automation,
                DictDB.Options_Autoget, DictDB.Options_Prompts, DictDB.Options_LegacyUI,
                DictDB.Options_Mods, DictDB.Options_AppSettings, DictDB.Options_Performance,
                DictDB.Options_Debug, DictDB.Common 
            };

            var texts = __instance.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
            {
                if (string.IsNullOrEmpty(t.text)) continue;

                if (DictDB.TryGetScopedTranslation(t.text, out string translated, scopes))
                {
                    if (t.text != translated)
                    {
                        t.text = translated;
                    }
                }
            }
        }
    }

    // 2. 왼쪽 카테고리 사이드바 메뉴 번역
    [HarmonyPatch(typeof(CategoryMenuController), "setData")]
    public static class Patch_CategoryMenuUI
    {
        [HarmonyPostfix]
        static void Postfix(CategoryMenuController __instance)
        {
            if (__instance == null) return;

            var scopes = new[] { 
                DictDB.Options, DictDB.Options_Sound, DictDB.Options_Display, DictDB.Options_Controls,
                DictDB.Options_Accessibility, DictDB.Options_UI, DictDB.Options_Automation,
                DictDB.Options_Autoget, DictDB.Options_Prompts, DictDB.Options_LegacyUI,
                DictDB.Options_Mods, DictDB.Options_AppSettings, DictDB.Options_Performance,
                DictDB.Options_Debug, DictDB.Common 
            };

            try
            {
                var texts = __instance.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in texts)
                {
                    if (string.IsNullOrEmpty(t.text)) continue;

                    if (DictDB.TryGetScopedTranslation(t.text, out string translated, scopes))
                    {
                        if (t.text != translated)
                        {
                            t.text = translated;
                        }
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
