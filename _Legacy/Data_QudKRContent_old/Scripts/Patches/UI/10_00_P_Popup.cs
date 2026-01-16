/*
 * 파일명: 10_00_P_Popup.cs
 * 분류: [UI] 팝업창 번역
 * 역할: 게임 내의 각종 알림, 질문, 선택지형 팝업(Show, AskString, PickOption 등)을 
 *       호출 직전에 가로채어 인자값을 한글로 번역합니다.
 * 수정일: 2026-01-14
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using XRL.UI;
using UnityEngine;

namespace QudKRContent
{
    [HarmonyPatch(typeof(Popup))]
    public static class Patch_Popup
    {
        // 1. 일반 알림 팝업 (Show)
        [HarmonyPatch("Show")]
        [HarmonyPrefix]
        static void Show_Prefix(ref string Message, ref string Title)
        {
            var scopes = new[] { DictDB.Popup, DictDB.Common };
            if (DictDB.TryGetScopedTranslation(Message, out string translatedMessage, scopes))
            {
                Message = translatedMessage;
            }
            if (DictDB.TryGetScopedTranslation(Title, out string translatedTitle, scopes))
            {
                Title = translatedTitle;
            }
        }

        // 2. 문자열 입력 팝업 (AskString)
        [HarmonyPatch("AskString")]
        [HarmonyPrefix]
        static void AskString_Prefix(ref string Message)
        {
            var scopes = new[] { DictDB.Popup, DictDB.Common };
            if (DictDB.TryGetScopedTranslation(Message, out string translated, scopes))
            {
                Message = translated;
            }
        }

        // 3. 선택지형 팝업 (PickOption)
        [HarmonyPatch("PickOption")]
        [HarmonyPrefix]
        static void PickOption_Prefix(ref string Title, ref string Intro, ref string[] Options)
        {
            var scopes = new[] { DictDB.Popup, DictDB.Common };
            // 제목 번역
            if (DictDB.TryGetScopedTranslation(Title, out string translatedTitle, scopes))
            {
                Title = translatedTitle;
            }
            // 설명문 번역
            if (DictDB.TryGetScopedTranslation(Intro, out string translatedIntro, scopes))
            {
                Intro = translatedIntro;
            }
            // 선택지 목록 번역 (새 리스트 생성하여 교체)
            if (Options != null && Options.Length > 0)
            {
                string[] translatedOptions = new string[Options.Length];
                bool anyChange = false;
                for (int i = 0; i < Options.Length; i++)
                {
                    if (DictDB.TryGetScopedTranslation(Options[i], out string translatedOption, scopes))
                    {
                        translatedOptions[i] = translatedOption;
                        anyChange = true;
                    }
                    else
                    {
                        translatedOptions[i] = Options[i];
                    }
                }
                if (anyChange)
                {
                    Options = translatedOptions;
                }
            }
        }
    }
}
