/*
 * 파일명: 10_01_P_MainMenu.cs
 * 분류: [Menu] 메인 메뉴
 * 역할: 메인 메뉴 항목을 한글로 번역합니다.
 * 수정일: 2026-01-14
 */

using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Qud.UI;

namespace QudKRContent
{
    [HarmonyPatch(typeof(MainMenu), "Show")]
    public static class Patch_MainMenu
    {
        static void Prefix()
        {
            TranslateMenuOptions();
        }

        // ModEntry에서 호출하는 메서드
        public static void OverwriteMenuData()
        {
            TranslateMenuOptions();
        }

        private static void TranslateMenuOptions()
        {
            try
            {
                Type menuType = typeof(MainMenu);
                
                FieldInfo leftField = AccessTools.Field(menuType, "LeftOptions");
                FieldInfo rightField = AccessTools.Field(menuType, "RightOptions");

                TranslateList(leftField);
                TranslateList(rightField);
                
                Debug.Log("[Qud-KR] 메인 메뉴 번역 완료");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Qud-KR] 메뉴 번역 중 예외: {e.Message}");
            }
        }

        private static void TranslateList(FieldInfo field)
        {
            if (field == null) return;
            
            try
            {
                var list = field.GetValue(null) as IList;
                if (list == null) return;

                for (int i = 0; i < list.Count; i++)
                {
                    object item = list[i];
                    if (item == null) continue;

                    var textField = AccessTools.Field(item.GetType(), "Text");
                    if (textField != null)
                    {
                        string original = textField.GetValue(item) as string;
                        if (original != null && MenuTranslations.TryGetValue(original.Trim(), out string translated))
                        {
                            textField.SetValue(item, translated);
                        }
                    }
                }
            }
            catch { }
        }

        private static System.Collections.Generic.Dictionary<string, string> MenuTranslations = 
            new System.Collections.Generic.Dictionary<string, string>()
        {
            // 왼쪽 메뉴
            { "New Game", "새 게임" },
            { "Continue", "이어하기" },
            { "Records", "기록실" },
            { "Load Game", "불러오기" },
            { "Options", "설정" },
            { "Mods", "모드 관리" },
            { "Daily Challenge", "일일 도전" },
            { "Weekly Challenge", "주간 도전" },
            
            // 오른쪽 메뉴
            { "Redeem Code", "코드 입력" },
            { "Modding Toolkit", "모딩 도구" },
            { "Credits", "제작진" },
            { "Help", "도움말" },
            { "Overlay UI", "오버레이 UI" },
            { "System", "시스템" },
            { "Library", "라이브러리" },
            { "Quit", "종료" }
        };
    }
}
