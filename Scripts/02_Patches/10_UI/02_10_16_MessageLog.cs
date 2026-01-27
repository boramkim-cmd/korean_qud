// ============================================================
// 분류: 02_Patches/10_UI
// 역할: 메시지 로그 번역 패치
// 설명: MessageQueue.AddPlayerMessage를 가로채서 메시지를 한글로 번역
// 참조: XRL.Messages.MessageQueue
// ============================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using XRL.Messages;
using UnityEngine;
using QudKRTranslation.Core;

namespace QudKoreanMod.Patches
{
    /// <summary>
    /// 메시지 로그 번역 패치
    /// </summary>
    [HarmonyPatch]
    public static class Patch_MessageQueue
    {
        private static Dictionary<string, string> _messagePatterns;
        private static bool _initialized = false;

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            
            _messagePatterns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            LoadMessageTranslations();
        }

        private static void LoadMessageTranslations()
        {
            try
            {
                // LocalizationManager가 이미 초기화되어 있어야 함
                // messages.json은 LOCALIZATION/GAMEPLAY/messages.json에 있음
                Debug.Log($"[Qud-KR][Messages] Message translation ready via LocalizationManager");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Qud-KR][Messages] Init error: {ex.Message}");
            }
        }

        /// <summary>
        /// AddPlayerMessage Prefix - 메시지 번역 시도
        /// </summary>
        [HarmonyPatch(typeof(MessageQueue), nameof(MessageQueue.AddPlayerMessage), 
            new Type[] { typeof(string), typeof(string), typeof(bool) })]
        [HarmonyPrefix]
        public static void AddPlayerMessage_Prefix(ref string Message)
        {
            if (string.IsNullOrEmpty(Message)) return;
            
            EnsureInitialized();
            
            string translated = TryTranslateMessage(Message);
            if (translated != null)
            {
                Message = translated;
            }
        }

        /// <summary>
        /// 메시지 번역 시도
        /// </summary>
        private static string TryTranslateMessage(string message)
        {
            // 1. 정확한 매치 시도 - LocalizationManager 사용
            string normalized = NormalizeMessage(message);
            
            // messages 카테고리들에서 검색
            string[] categories = { "flight", "movement", "items", "combat", "status", "interaction", "system" };
            foreach (var cat in categories)
            {
                if (LocalizationManager.TryGetAnyTerm(normalized, out string translation, cat))
                {
                    return translation;
                }
            }

            // 2. _messagePatterns 캐시에서 검색 (하위 호환)
            if (_messagePatterns != null && _messagePatterns.TryGetValue(normalized, out string cached))
            {
                return cached;
            }

            // 3. 패턴 매칭 시도 (You verb...)
            string patternResult = TryPatternMatch(message);
            if (patternResult != null)
            {
                return patternResult;
            }

            return null;
        }

        /// <summary>
        /// 메시지 정규화 (색상 태그 제거, 공백 정리)
        /// </summary>
        private static string NormalizeMessage(string message)
        {
            // 색상 태그 제거
            string result = Regex.Replace(message, @"\{\{[a-zA-Z\|]+\|?", "");
            result = result.Replace("}}", "");
            return result.Trim();
        }

        /// <summary>
        /// 패턴 매칭 시도
        /// </summary>
        private static string TryPatternMatch(string message)
        {
            // "You verb..." 패턴
            var youMatch = Regex.Match(message, @"^You\s+(\w+)(.*)$", RegexOptions.IgnoreCase);
            if (youMatch.Success)
            {
                string verb = youMatch.Groups[1].Value.ToLower();
                string rest = youMatch.Groups[2].Value;
                
                // 동사별 번역 패턴
                string koreanVerb = TranslateVerb(verb);
                if (koreanVerb != null)
                {
                    string koreanRest = TranslateRest(rest);
                    return $"당신{은는(koreanVerb)} {koreanVerb}{koreanRest}";
                }
            }

            return null;
        }

        /// <summary>
        /// 동사 번역
        /// </summary>
        private static string TranslateVerb(string verb)
        {
            var verbMap = new Dictionary<string, string>
            {
                { "begin", "시작한다" },
                { "return", "돌아온다" },
                { "fall", "떨어진다" },
                { "pick", "집어든다" },
                { "drop", "떨어뜨린다" },
                { "attack", "공격한다" },
                { "hit", "명중시킨다" },
                { "miss", "빗나간다" },
                { "dodge", "회피한다" },
                { "block", "막는다" },
                { "kill", "처치한다" },
                { "die", "죽는다" },
                { "drink", "마신다" },
                { "eat", "먹는다" },
                { "equip", "장착한다" },
                { "unequip", "해제한다" },
                { "use", "사용한다" },
                { "open", "연다" },
                { "close", "닫는다" },
                { "move", "이동한다" },
                { "rest", "휴식한다" },
                { "wait", "기다린다" },
                { "see", "본다" },
                { "hear", "듣는다" },
                { "feel", "느낀다" },
            };

            return verbMap.TryGetValue(verb, out string korean) ? korean : null;
        }

        /// <summary>
        /// 나머지 문장 번역
        /// </summary>
        private static string TranslateRest(string rest)
        {
            if (string.IsNullOrEmpty(rest)) return ".";

            // 간단한 패턴 치환
            rest = rest.Replace(" to the ground", " 땅으로");
            rest = rest.Replace(" flying", " 비행을");
            rest = rest.Replace("!", "!");
            rest = rest.Replace(".", ".");

            return rest;
        }

        /// <summary>
        /// 은/는 조사 선택 (간단 버전)
        /// </summary>
        private static string 은는(string word)
        {
            if (string.IsNullOrEmpty(word)) return "은";
            char last = word[word.Length - 1];
            // 받침 있으면 "은", 없으면 "는"
            return HasFinalConsonant(last) ? "은" : "는";
        }

        private static bool HasFinalConsonant(char c)
        {
            if (c < 0xAC00 || c > 0xD7A3) return false;
            return ((c - 0xAC00) % 28) != 0;
        }
    }
}
