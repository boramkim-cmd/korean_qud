/*
 * 파일명: FrameworkScroller_Patch.cs
 * 분류: [UI Patch] 프레임워크 스크롤러 패치
 * 역할: FrameworkScroller가 프리팹(각 줄의 UI)을 설정할 때 즉시 번역을 적용합니다.
 *       이를 통해 옵션 리스트나 메뉴 항목이 생성되는 시점에 한글화를 보장합니다.
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using XRL.UI.Framework;
using QudKRTranslation;
using QudKRTranslation.Utils;
using QudKRTranslation.Data;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(FrameworkScroller), nameof(FrameworkScroller.SetupPrefab))]
    public static class FrameworkScroller_SetupPrefab_Patch
    {
        // Postfix: 프리팹 설정 직후 해당 요소 내의 모든 텍스트를 현재 스코프에 맞춰 번역
        static void Postfix(FrameworkUnityScrollChild newChild, ScrollChildContext context, FrameworkDataElement data, int index)
        {
            try
            {
                if (newChild == null) return;

                // 현재 활성 스코프 가져오기
                var currentScope = ScopeManager.GetCurrentScope();

                // 우선순위 결정: 현재 스코프가 있다면 그것을 사용, 없다면 옵션->메인메뉴->공통 순서로 시도
                Dictionary<string, string>[] scopesToTry;
                if (currentScope != null)
                {
                    scopesToTry = currentScope;
                }
                else
                {
                    scopesToTry = new[] {
                        OptionsData.Translations,
                        Data.MainMenuData.Translations,
                        Data.CommonData.Translations
                    };
                }

                // 1) TMP_Text 번역 (일반적인 UI 텍스트)
                var tmps = newChild.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in tmps)
                {
                    if (t == null || string.IsNullOrEmpty(t.text)) continue;

                    // 제어값(숫자, On/Off, 체크박스 등)은 보호
                    if (TranslationUtils.IsControlValue(t.text)) continue;

                    if (TranslationUtils.TryTranslatePreservingTags(t.text, out string translated, scopesToTry))
                    {
                        if (t.text != translated)
                        {
                            t.text = translated;
                        }
                    }
                }

                // 2) UITextSkin 번역 (게임 엔진 커스텀 텍스트 스킨)
                var uiTextSkins = newChild.GetComponentsInChildren(typeof(XRL.UI.UITextSkin), true);
                foreach (var comp in uiTextSkins)
                {
                    if (comp == null) continue;
                    var uiSkin = comp as XRL.UI.UITextSkin;
                    if (uiSkin == null || string.IsNullOrEmpty(uiSkin.text)) continue;

                    if (TranslationUtils.IsControlValue(uiSkin.text)) continue;

                    if (TranslationUtils.TryTranslatePreservingTags(uiSkin.text, out string translated, scopesToTry))
                    {
                        if (uiSkin.text != translated)
                        {
                            uiSkin.text = translated;
                            // UITextSkin의 text를 변경하면 내부 Apply 로직에 의해 TMP에 반영됨
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 패치 도중 에러가 나더라도 게임 흐름에 영향을 주지 않도록 경고만 남김
                Debug.LogWarning("[Qud-KR] FrameworkScroller.SetupPrefab Patch Exception: " + ex.Message);
            }
        }
    }
}
