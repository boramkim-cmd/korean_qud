/*
 * 파일명: 10_04_P_ListScroller.cs
 * 분류: [UI Patch] 프레임워크 스크롤러 패치
 * 역할: FrameworkScroller가 프리팹(각 줄의 UI)을 설정할 때 즉시 번역을 적용합니다.
 *       이를 통해 옵션 리스트나 메뉴 항목이 생성되는 시점에 한글화를 보장합니다.
 *       기존 데이터 클래스 대신 LocalizationManager를 사용합니다.
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using XRL.UI.Framework;
using QudKRTranslation;
using QudKRTranslation.Utils;
using QudKRTranslation.Core;

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

                // 우선순위 결정: 현재 스코프가 있다면 사용, 없다면 options/ui/common 순으로 fallback
                Dictionary<string, string>[] scopesToTry;
                if (currentScope != null)
                {
                    scopesToTry = currentScope;
                }
                else
                {
                    var optionsDict = LocalizationManager.GetCategory("options");
                    var uiDict = LocalizationManager.GetCategory("ui");
                    var commonDict = LocalizationManager.GetCategory("common");

                    var list = new List<Dictionary<string, string>>();
                    if (optionsDict != null) list.Add(optionsDict);
                    if (uiDict != null) list.Add(uiDict);
                    if (commonDict != null) list.Add(commonDict);

                    scopesToTry = list.ToArray();
                }

                if (scopesToTry == null || scopesToTry.Length == 0) return;

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
                // UITextSkin 타입이 없을 수도 있으니 리플렉션이나 문자열로 찾을 수도 있음. 
                // 하지만 XRL.UI 네임스페이스가 있으므로 직접 참조 시도.
                // 만약 빌드 에러가 난다면 리플렉션 사용. (여기서는 직접 참조 유지)
                
                // Reflection to avoid hard dependency if the class is tricky, 
                // but usually it's fine. If XRL.UI.UITextSkin is accessible:
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
