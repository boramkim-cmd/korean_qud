/*
 * 파일명: 10_05_P_OptionsData.cs
 * 분류: [Data] 옵션 데이터 번역
 * 역할: 게임 옵션 데이터가 로드될 때(LoadOptionNode) 즉시 번역을 적용하여
 *       UI 딜레이와 호버 시 원문 회귀 현상을 근본적으로 방지합니다.
 * 수정일: 2026-01-14
 */

using System;
using HarmonyLib;
using XRL.UI;
using UnityEngine;
using System.Collections.Generic;

namespace QudKRContent
{
    [HarmonyPatch(typeof(Options), "LoadOptionNode")]
    public static class Patch_OptionsData
    {
        [HarmonyPostfix]
        static void LoadOptionNode_Postfix(GameOption __result)
        {
            if (__result == null) return;

            try
            {
                TranslateOption(__result);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] Patch_OptionsData 오류 (ID: {__result.ID}): {e.Message}");
            }
        }

        public static void TranslateOption(GameOption opt)
        {
            if (opt == null) return;

            // 1. 표시 텍스트 번역
            if (!string.IsNullOrEmpty(opt.DisplayText) && DictDB.TryGetAnyTranslation(opt.DisplayText, out string t1))
            {
                opt.DisplayText = t1;
            }

            // 2. 설명 텍스트 번역
            if (!string.IsNullOrEmpty(opt.HelpText) && DictDB.TryGetAnyTranslation(opt.HelpText, out string t2))
            {
                opt.HelpText = t2;
            }

            // 3. 카테고리 이름 번역 (데이터 레벨에서는 생략 - UI 패치에서 처리)
            // opt.Category는 내부 딕셔너리의 키로 사용되므로 여기서 번역하면 KeyNotFoundException이 발생합니다.

            // 4. 표시 값들 번역
            if (opt.DisplayValues != null)
            {
                for (int i = 0; i < opt.DisplayValues.Length; i++)
                {
                    if (DictDB.TryGetAnyTranslation(opt.DisplayValues[i], out string t4))
                    {
                        opt.DisplayValues[i] = t4;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Options), "LoadAllOptions")]
    public static class Patch_OptionsData_Initial
    {
        [HarmonyPostfix]
        static void LoadAllOptions_Postfix()
        {
            try
            {
                if (Options.OptionsByID == null) return;

                Debug.Log("[Qud-KR] 모든 옵션 데이터에 대한 일괄 번역 시작...");
                int count = 0;
                foreach (var kv in Options.OptionsByID)
                {
                    if (kv.Value != null)
                    {
                        Patch_OptionsData.TranslateOption(kv.Value);
                        count++;
                    }
                }
                Debug.Log($"[Qud-KR] 일괄 번역 완료: 총 {count}개의 옵션이 번역되었습니다.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] LoadAllOptions_Postfix 오류: {e.Message}");
            }
        }
    }
}
