/*
 * 파일명: MainMenuTranslator.cs
 * 분류: [UI Patch] 메인 메뉴 데이터 기반 번역
 * 역할: 메인 메뉴의 UI 데이터(LeftOptions, RightOptions)를 직접 수정하고,
 *       TMP_Text 세터를 통해 실시간 번역을 보장합니다.
 *       기존 MainMenu_Patch가 활성화한 Scope를 활용합니다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using TMPro;
using QudKRTranslation;
using QudKRTranslation.Data;
using QudKRTranslation.Utils;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch]
    public static class MainMenuTranslator
    {
        // 1. TMP_Text.text 세터 패치: 텍스트가 바뀔 때마다 스코프가 있으면 번역 시도
        [HarmonyPatch(typeof(TMP_Text), "text", MethodType.Setter)]
        public static class TMP_Text_Setter_Patch
        {
            static void Prefix(TMP_Text __instance, ref string value)
            {
                try
                {
                    if (string.IsNullOrEmpty(value)) return;
                    
                    // 현재 활성 스코프 확인
                    var scope = ScopeManager.GetCurrentScope();
                    if (scope == null) return;

                    // 태그 보존 번역 시도
                    if (TranslationUtils.TryTranslatePreservingTags(value, out string translated, scope))
                    {
                        if (value != translated)
                        {
                            value = translated;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 최상위 UI에서 에러가 나면 게임이 멈출 수 있으므로 경고만 로그
                    Debug.LogWarning($"[Qud-KR] MainMenuTranslator.TMP_Text_Setter_Patch Exception: {ex.Message}");
                }
            }
        }

        // 2. 메인 메뉴가 열리기 직전에 데이터 리스트 번역 시도
        [HarmonyPatch]
        public static class MainMenu_Show_Patch
        {
            static MethodBase TargetMethod()
            {
                var t = AccessTools.TypeByName("Qud.UI.MainMenu") ?? AccessTools.TypeByName("XRL.UI.MainMenu");
                return t != null ? AccessTools.Method(t, "Show") : null;
            }

            [HarmonyPrefix]
            static void Prefix()
            {
                TranslateMenuData();
            }
        }

        /// <summary>
        /// 메인 메뉴의 LeftOptions/RightOptions 리스트를 찾아 한국어로 직접 치환합니다.
        /// </summary>
        private static void TranslateMenuData()
        {
            try
            {
                var menuType = AccessTools.TypeByName("Qud.UI.MainMenu") ?? AccessTools.TypeByName("XRL.UI.MainMenu");
                if (menuType == null) return;

                // 정적/인스턴스 및 Public/Private 모두 검색
                FieldInfo leftField = menuType.GetField("LeftOptions", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                FieldInfo rightField = menuType.GetField("RightOptions", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                if (leftField == null && rightField == null) return;

                // 인스턴스 필드일 경우 현재 씬의 MainMenu 객체 확보 시도
                object ownerInstance = null;
                if ((leftField != null && !leftField.IsStatic) || (rightField != null && !rightField.IsStatic))
                {
                    ownerInstance = UnityEngine.Object.FindObjectOfType(menuType);
                }

                if (leftField != null) TranslateList(leftField, ownerInstance, "LeftOptions");
                if (rightField != null) TranslateList(rightField, ownerInstance, "RightOptions");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Qud-KR] TranslateMenuData exception: " + ex.Message);
            }
        }

        private static void TranslateList(FieldInfo field, object owner, string fieldName)
        {
            try
            {
                object raw = field.GetValue(field.IsStatic ? null : owner);
                if (raw == null) return;

                var list = raw as IList;
                if (list == null) return;

                int changed = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    object item = list[i];
                    if (item == null) continue;

                    // "Text", "Description", "Title" 순서대로 텍스트 필드 탐색
                    string original = null;
                    FieldInfo textField = item.GetType().GetField("Text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                       ?? item.GetType().GetField("Description", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                       ?? item.GetType().GetField("Title", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (textField != null)
                    {
                        original = textField.GetValue(item) as string;
                    }
                    else
                    {
                        // 프로퍼티로 존재할 경우 대응
                        PropertyInfo textProp = item.GetType().GetProperty("Text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                             ?? item.GetType().GetProperty("Description", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (textProp != null) original = textProp.GetValue(item, null) as string;
                    }

                    if (string.IsNullOrEmpty(original)) continue;

                    // MainMenuData를 사용하여 번역 시도
                    if (TranslationEngine.TryTranslate(original, out string translated, new[] { MainMenuData.Translations }))
                    {
                        if (textField != null)
                        {
                            textField.SetValue(item, translated);
                            changed++;
                        }
                        else
                        {
                            PropertyInfo textProp = item.GetType().GetProperty("Text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (textProp != null) { textProp.SetValue(item, translated, null); changed++; }
                        }
                    }
                }

                if (changed > 0)
                {
                    Debug.Log($"[Qud-KR] MainMenuTranslator: {fieldName} {changed}개 항목 번역 적용 완료");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] TranslateList({fieldName}) exception: {ex.Message}");
            }
        }
    }
}
