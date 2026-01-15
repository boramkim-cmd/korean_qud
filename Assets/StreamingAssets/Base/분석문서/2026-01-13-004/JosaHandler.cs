// ==================================================
// Caves of Qud 한글 조사 처리 시스템
// 기반: Josa.js (https://github.com/e-/Josa.js)
// 최적화: Caves of Qud 게임 환경
// 라이선스: MIT
// ==================================================

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CavesOfQud.KoreanJosa
{
    /// <summary>
    /// 한글 조사 자동 처리 핸들러
    /// Josa.js의 검증된 알고리즘을 C#으로 포팅하고 게임 환경에 최적화
    /// </summary>
    public static class JosaHandler
    {
        // ==================== 상수 ====================
        
        private const int HANGUL_START = 0xAC00;  // '가'
        private const int HANGUL_END = 0xD7A3;    // '힣'
        private const int JONGSEONG_COUNT = 28;
        private const int RIEUL_JONGSEONG = 8;    // ㄹ 받침
        
        // ==================== 캐시 ====================
        
        private static readonly Dictionary<string, string> ProcessCache = 
            new Dictionary<string, string>(1000);
        
        private static readonly Dictionary<(string, string), string> JosaCache = 
            new Dictionary<(string, string), string>(100);
        
        // ==================== 정규표현식 ====================
        
        // 패턴: <변수><josa_type>
        // 예: <item.name><josa_eul_reul>
        private static readonly Regex JosaPattern = 
            new Regex(@"<([^>]+)><josa_(\w+)>", RegexOptions.Compiled);
        
        // ==================== 공개 API ====================
        
        /// <summary>
        /// 텍스트 내 모든 조사 태그 처리
        /// </summary>
        /// <param name="text">처리할 텍스트</param>
        /// <returns>조사가 처리된 텍스트</returns>
        public static string Process(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            // 캐시 확인
            if (ProcessCache.TryGetValue(text, out string cached))
                return cached;
            
            // 조사 태그 처리
            string result = JosaPattern.Replace(text, match =>
            {
                string variable = match.Groups[1].Value;
                string josaType = match.Groups[2].Value;
                
                // 변수 값 가져오기
                string value = ResolveVariable(variable);
                
                // 조사 선택
                string josa = SelectJosa(value, josaType);
                
                return value + josa;
            });
            
            // 캐시 저장 (최대 1000개)
            if (ProcessCache.Count < 1000)
                ProcessCache[text] = result;
            
            return result;
        }
        
        /// <summary>
        /// 조사만 선택 (Josa.js의 josa.c와 동일)
        /// </summary>
        /// <param name="word">단어</param>
        /// <param name="format">조사 형식 (예: "을/를", "이/가")</param>
        /// <returns>선택된 조사</returns>
        public static string Choose(string word, string format)
        {
            return SelectJosa(word, format);
        }
        
        /// <summary>
        /// 단어 + 조사 반환 (Josa.js의 josa.r과 동일)
        /// </summary>
        /// <param name="word">단어</param>
        /// <param name="format">조사 형식</param>
        /// <returns>단어 + 조사</returns>
        public static string Result(string word, string format)
        {
            return word + SelectJosa(word, format);
        }
        
        /// <summary>
        /// 캐시 초기화 (메모리 관리)
        /// </summary>
        public static void ClearCache()
        {
            ProcessCache.Clear();
            JosaCache.Clear();
        }
        
        // ==================== 핵심 알고리즘 ====================
        
        /// <summary>
        /// 조사 선택 (Josa.js 알고리즘 기반)
        /// </summary>
        private static string SelectJosa(string word, string type)
        {
            if (string.IsNullOrEmpty(word))
                return GetDefaultJosa(type);
            
            // 캐시 확인
            var cacheKey = (word, type);
            if (JosaCache.TryGetValue(cacheKey, out string cachedJosa))
                return cachedJosa;
            
            bool hasJong = HasJongseong(word);
            string josa;
            
            // 조사 타입별 선택
            switch (type.ToLower())
            {
                // 을/를
                case "eul_reul":
                case "을/를":
                case "을":
                case "를":
                case "을를":
                    josa = hasJong ? "을" : "를";
                    break;
                
                // 이/가
                case "i_ga":
                case "subject":
                case "이/가":
                case "이":
                case "가":
                case "이가":
                    josa = hasJong ? "이" : "가";
                    break;
                
                // 은/는
                case "eun_neun":
                case "topic":
                case "은/는":
                case "은":
                case "는":
                case "은는":
                    josa = hasJong ? "은" : "는";
                    break;
                
                // 으로/로 (ㄹ 받침 특수 처리)
                case "euro_ro":
                case "direction":
                case "으로/로":
                case "으로":
                case "로":
                case "으로로":
                    if (HasRieulJongseong(word))
                        josa = "로";
                    else
                        josa = hasJong ? "으로" : "로";
                    break;
                
                // 아/야
                case "a_ya":
                case "vocative":
                case "아/야":
                case "아":
                case "야":
                    josa = hasJong ? "아" : "야";
                    break;
                
                // 와/과
                case "wa_gwa":
                case "와/과":
                case "와":
                case "과":
                case "와과":
                    josa = hasJong ? "과" : "와";
                    break;
                
                default:
                    josa = "";
                    break;
            }
            
            // 캐시 저장 (최대 100개)
            if (JosaCache.Count < 100)
                JosaCache[cacheKey] = josa;
            
            return josa;
        }
        
        /// <summary>
        /// 받침 확인 (Josa.js 알고리즘 + 개선)
        /// </summary>
        /// <param name="word">확인할 단어</param>
        /// <returns>받침 있으면 true</returns>
        private static bool HasJongseong(string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;
            
            // 특수문자 제거 후 마지막 한글 찾기
            char lastChar = GetLastKoreanChar(word);
            
            if (lastChar == '\0')
                return false;
            
            // 한글 범위 체크 (Josa.js 개선)
            if (lastChar < HANGUL_START || lastChar > HANGUL_END)
                return false;
            
            // Josa.js 핵심 알고리즘
            // (코드 - 0xAC00) % 28 > 0 → 받침 있음
            return (lastChar - HANGUL_START) % JONGSEONG_COUNT > 0;
        }
        
        /// <summary>
        /// ㄹ 받침 확인 (으로/로 특수 케이스)
        /// </summary>
        private static bool HasRieulJongseong(string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;
            
            char lastChar = GetLastKoreanChar(word);
            
            if (lastChar == '\0' || lastChar < HANGUL_START || lastChar > HANGUL_END)
                return false;
            
            // ㄹ 받침은 8번
            return (lastChar - HANGUL_START) % JONGSEONG_COUNT == RIEUL_JONGSEONG;
        }
        
        /// <summary>
        /// 마지막 한글 글자 추출 (특수문자 무시)
        /// </summary>
        private static char GetLastKoreanChar(string word)
        {
            for (int i = word.Length - 1; i >= 0; i--)
            {
                char c = word[i];
                if (c >= HANGUL_START && c <= HANGUL_END)
                    return c;
            }
            return '\0';
        }
        
        /// <summary>
        /// 기본 조사 (빈 문자열일 때)
        /// </summary>
        private static string GetDefaultJosa(string type)
        {
            switch (type.ToLower())
            {
                case "eul_reul":
                case "을/를":
                    return "를";
                case "i_ga":
                case "subject":
                case "이/가":
                    return "가";
                case "eun_neun":
                case "topic":
                case "은/는":
                    return "는";
                case "euro_ro":
                case "direction":
                case "으로/로":
                    return "로";
                case "a_ya":
                case "vocative":
                case "아/야":
                    return "야";
                case "wa_gwa":
                case "와/과":
                    return "와";
                default:
                    return "";
            }
        }
        
        /// <summary>
        /// 변수 값 가져오기 (게임 API 연동)
        /// TODO: Caves of Qud API와 연동 필요
        /// </summary>
        private static string ResolveVariable(string variable)
        {
            // 간단한 변수 처리 예시
            // 실제로는 게임 API를 사용해야 함
            
            /*
            // 예시 구현:
            if (variable.StartsWith("player."))
            {
                string property = variable.Substring(7);
                return GetPlayerProperty(property);
            }
            else if (variable.StartsWith("item."))
            {
                string property = variable.Substring(5);
                return GetItemProperty(property);
            }
            */
            
            // 기본값: 변수명 그대로 반환
            return variable;
        }
    }
}
