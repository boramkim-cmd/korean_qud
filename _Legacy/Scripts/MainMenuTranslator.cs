using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro; 
using UnityEngine;
using Qud.UI;

namespace QudKRContent
{
    // =================================================================
    // 1. 모드 시동 키
    // =================================================================
    public class ModEntry
    {
        public static void Main()
        {
            try
            {
                var harmony = new Harmony("com.boram.qud.content");
                harmony.PatchAll();
                Debug.Log("[Qud-KR] 스마트 버튼 번역 모드 로드 (문장 오염 방지)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Qud-KR] 로드 실패: {e.ToString()}");
            }
        }
    }

    // =================================================================
    // 2. 번역 로직
    // =================================================================
    [HarmonyPatch]
    public static class MainMenuTranslator
    {
        // ✅ [버튼/UI 단어장]
        // 문장에 섞여 나올 걱정 없이, UI 단어들을 마음껏 넣으세요.
        public static Dictionary<string, string> UiDict = new Dictionary<string, string>()
        {
            // [짧은 버튼들] (이제 문장 속에서 오작동하지 않습니다!)
            { "Yes", "예" },
            { "No", "아니오" },
            { "OK", "확인" },
            { "Back", "뒤로" },
            { "Next", "다음" },
            { "Done", "완료" },
            { "Quit", "종료" },
            { "Help", "도움말" },
            { "Accept", "수락" },
            { "Cancel", "취소" },
            { "Buy", "구매" },
            { "Sell", "판매" },
            { "Look", "살펴보기" },
            { "Get", "줍기" },
            
            // [메인 메뉴 & 긴 단어]
            { "New Game", "새 게임" },
            { "Continue", "이어하기" },
            { "Records", "기록실" },
            { "Load Game", "불러오기" },
            { "Options", "설정" },
            { "Mods", "모드 관리" },
            { "Daily Challenge", "일일 도전" },
            { "Weekly Challenge", "주간 도전" },
            { "Travel to a Shared World", "공유 세계로 여행" },
            { "Credits", "제작진" },
            { "System", "시스템" },
            { "Library", "라이브러리" },
            { "Overlay UI", "오버레이 UI" },
            { "Redeem Code", "코드 입력" },
            { "Modding Toolkit", "모딩 도구" },

            // [캐릭터 생성]
            { "character creation", "캐릭터 생성" },
            { "Name:", "이름:" },
            { "Restore Defaults", "기본값 복원" },
            { "Randomize", "무작위" },
            
            // [설정 탭]
            { "Video", "비디오" },
            { "Audio", "오디오" },
            { "Controls", "조작" },
            { "Interface", "인터페이스" },
            { "Automation", "자동화" },
            { "Prompts", "알림" },
            { "Prerelease Content", "베타 콘텐츠" },
            { "Debug", "디버그" }
        };

        // 🟢 [스마트 실시간 번역]
        // 텍스트가 화면에 나올 때마다 검사하지만, '버튼처럼 생긴 것'만 골라서 번역합니다.
        [HarmonyPatch(typeof(TMP_Text), "text", MethodType.Setter)]
        public static class TMP_Text_Setter_Patch
        {
            static void Prefix(ref string value)
            {
                if (string.IsNullOrEmpty(value)) return;
                
                string cleanText = value.Trim();

                // 1. [정확한 일치] (가장 빠르고 정확함)
                if (UiDict.TryGetValue(cleanText, out string translated))
                {
                    value = translated;
                    return;
                }

                // 2. [스마트 포함 검사]
                foreach (var kvp in UiDict)
                {
                    // 텍스트에 키워드가 들어있는가? (예: "&WNext" 안에 "Next"가 있는가?)
                    if (cleanText.Contains(kvp.Key) && !cleanText.Contains(kvp.Value))
                    {
                        // ★핵심 안전장치: 길이 차이 검사★
                        // 원본 텍스트 길이 - 키워드 길이 = 군더더기 길이
                        // 군더더기가 10글자 미만이면 -> "아, 이건 색깔 코드나 괄호가 붙은 버튼이구나" -> 번역 OK
                        // 군더더기가 10글자 이상이면 -> "아, 이건 긴 문장이구나" -> 번역 PASS
                        
                        int diff = cleanText.Length - kvp.Key.Length;
                        
                        if (diff < 10) 
                        {
                            value = value.Replace(kvp.Key, kvp.Value);
                            return; // 하나 찾으면 종료
                        }
                    }
                }
            }
        }

        // 🟣 [보조] 메인 메뉴 데이터 원본 수정
        // 이건 게임 켤 때 메뉴판 자체를 바꾸는 거라 무조건 안전합니다.
        [HarmonyPatch(typeof(MainMenu), "Show")]
        public static class MainMenu_Show_Patch
        {
            static void Prefix() { TranslateMenuData(); }
        }

        private static void TranslateMenuData()
        {
            try
            {
                Type menuType = AccessTools.TypeByName("Qud.UI.MainMenu");
                if (menuType == null) return;
                TranslateList(AccessTools.Field(menuType, "LeftOptions"));
                TranslateList(AccessTools.Field(menuType, "RightOptions"));
            }
            catch { }
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

                    var textMember = AccessTools.Field(item.GetType(), "Text");
                    var textProp = AccessTools.Property(item.GetType(), "Text");
                    
                    string original = null;
                    if (textMember != null) original = textMember.GetValue(item) as string;
                    else if (textProp != null) original = textProp.GetValue(item, null) as string;

                    if (original != null)
                    {
                        string trimmed = original.Trim();
                        // 메뉴 데이터는 정확도 100%이므로 바로 교체
                        if (UiDict.TryGetValue(trimmed, out string translated))
                        {
                            if (textMember != null) textMember.SetValue(item, translated);
                            else textProp?.SetValue(item, translated, null);
                        }
                    }
                }
            }
            catch { }
        }
    }
}