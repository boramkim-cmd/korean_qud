/*
 * 파일명: 10_05_P_OptionsData.cs
 * 분류: [UI Patch] 설정 데이터 번역
 * 역할: Options 데이터가 로드될 때(LoadOptionNode / LoadAllOptions) GameOption을 번역합니다.
 *       데이터 레벨에서 번역하여 UI 갱신 타이밍과 무관하게 안정적인 한글화를 보장합니다.
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using XRL.UI;
using UnityEngine;
using QudKRTranslation.Data;
using QudKRTranslation.Utils;

namespace QudKRTranslation.Patches
{
    /// <summary>
    /// Options 데이터가 로드될 때(LoadOptionNode / LoadAllOptions) GameOption을 번역합니다.
    /// </summary>
    public static class Patch_OptionsData
    {
        // 개별 노드가 로드될 때마다 번역 적용
        [HarmonyPatch(typeof(Options), "LoadOptionNode")]
        public static class LoadOptionNode_Patch
        {
            [HarmonyPostfix]
            static void Postfix(GameOption __result)
            {
                try
                {
                    if (__result == null) return;
                    TranslateOption(__result);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Qud-KR] LoadOptionNode_Postfix 오류 (ID: {__result?.ID}): {e.Message}");
                }
            }
        }

        // 전체 옵션을 한 번에 로드한 이후 일괄 번역 (로딩 순서 안전)
        [HarmonyPatch(typeof(Options), "LoadAllOptions")]
        public static class LoadAllOptions_Patch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                try
                {
                    if (Options.OptionsByID == null) return;

                    Debug.Log("[Qud-KR] 모든 옵션 데이터에 대한 일괄 번역 시작...");
                    int count = 0;
                    foreach (var kv in Options.OptionsByID)
                    {
                        var opt = kv.Value;
                        if (opt == null) continue;
                        TranslateOption(opt);
                        count++;
                    }
                    Debug.Log($"[Qud-KR] 일괄 번역 완료: 총 {count}개의 옵션 번역 적용");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Qud-KR] LoadAllOptions_Postfix 오류: {e.Message}");
                }
            }
        }

        // 실제 번역 로직: DisplayText, HelpText, DisplayValues(콤보 표시값) 등
        public static void TranslateOption(GameOption opt)
        {
            if (opt == null) return;

            // 우선순위 스코프: 옵션 전용 데이터 -> 공통 데이터
            var scopes = new[] { OptionsData.Translations, QudKRTranslation.Data.CommonData.Translations };

            // DisplayText (타이틀)
            if (!string.IsNullOrEmpty(opt.DisplayText))
            {
                if (TranslationUtils.TryTranslatePreservingTags(opt.DisplayText, out string t, scopes))
                    opt.DisplayText = t;
            }

            // HelpText (설명 / 툴팁)
            if (!string.IsNullOrEmpty(opt.HelpText))
            {
                if (TranslationUtils.TryTranslatePreservingTags(opt.HelpText, out string h, scopes))
                    opt.HelpText = h;
            }

            // DisplayValues (콤보 박스의 표시값들) — 배열이 있는 경우 각각 번역
            try
            {
                if (opt.Values != null && opt.Values.Length > 0)
                {
                    if (opt.DisplayValues != null && opt.DisplayValues.Length == opt.Values.Length)
                    {
                        for (int i = 0; i < opt.DisplayValues.Length; i++)
                        {
                            if (string.IsNullOrEmpty(opt.DisplayValues[i])) continue;
                            if (TranslationUtils.TryTranslatePreservingTags(opt.DisplayValues[i], out string dv, scopes))
                                opt.DisplayValues[i] = dv;
                        }
                    }
                    else if (opt.DisplayValues != null)
                    {
                        for (int i = 0; i < opt.DisplayValues.Length; i++)
                        {
                            if (string.IsNullOrEmpty(opt.DisplayValues[i])) continue;
                            if (TranslationUtils.TryTranslatePreservingTags(opt.DisplayValues[i], out string dv, scopes))
                                opt.DisplayValues[i] = dv;
                        }
                    }
                }
            }
            catch { /* 안전하게 무시 */ }
        }
    }
}
