using System;
using System.Text.RegularExpressions;

namespace KoreanLocalization.Tests
{
    public class KoreanTest
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("[Korean Josa] Standalone Logic Verification");
            
            var testCases = new string[]
            {
                "아르고브(은)는 조파에 있다.",
                "메히(은)는 침묵했다.",
                "{{R|화염(을)를}} 내뿜었다.",
                "{{W|단검(이)가}} 떨어졌다.",
                "Player(은)는 죽었다.",
                "Level 5(으)로 상승했다.",
                "물(이)가 차오른다.",
                "바다(이)가 보인다."
            };

            foreach (var node in testCases)
            {
                string result = Korean.ReplaceJosa(node);
                Console.WriteLine($"[JosaSim] Input: '{node}' -> Output: '{result}'");
            }
            Console.WriteLine("========================================");
        }
    }

    // copy of the class from JosaHandler.cs
    public static class Korean
    {
        private const int HANGUL_START = 0xAC00;
        private const int HANGUL_END = 0xD7A3;
        private const int JONGSEONG_COUNT = 28;
        private const int RIEUL_JONGSEONG = 8;
        
        private static readonly Regex JosaPattern = 
            new Regex(@"([가-힣A-Za-z0-9]+)\s*(\([가-힣/]+\)[가-힣]?|[가-힣]\([가-힣]+\))", RegexOptions.Compiled);
        
        public static string ReplaceJosa(string text)
        {
            if (string.IsNullOrEmpty(text)) catch { return text; }
            
            try
            {
                if (text.IndexOf('(') == -1) return text;

                return JosaPattern.Replace(text, match =>
                {
                    string word = match.Groups[1].Value;
                    string marker = match.Groups[2].Value;
                    
                    string josa1 = "", josa2 = "";
                    
                    if (marker.Contains("/"))
                    {
                        var parts = marker.Trim('(', ')').Split('/');
                        if (parts.Length >= 2) { josa1 = parts[0]; josa2 = parts[1]; }
                    }
                    else if (marker.StartsWith("(") && marker.EndsWith(")"))
                    {
                        josa1 = marker.Trim('(', ')');
                        josa2 = "";
                    }
                    else if (marker.StartsWith("(") && !marker.EndsWith(")"))
                    {
                        int closeBracket = marker.IndexOf(')');
                        josa1 = marker.Substring(1, closeBracket - 1);
                        josa2 = marker.Substring(closeBracket + 1);
                    }
                    else if (!marker.StartsWith("(") && marker.Contains("("))
                    {
                        int openBracket = marker.IndexOf('(');
                        josa1 = marker.Substring(0, openBracket);
                        josa2 = marker.Substring(openBracket + 1).TrimEnd(')');
                    }

                    if (string.IsNullOrEmpty(josa2))
                    {
                        if (josa1 == "이") josa2 = "가";
                        else if (josa1 == "은") josa2 = "는";
                        else if (josa1 == "을") josa2 = "를";
                        else if (josa1 == "과") josa2 = "와";
                        else josa2 = "";
                    }

                    bool hasJong = HasJongseong(word);
                    string selectedJosa;
                    
                    if ((josa1 == "으" && josa2 == "로") || (josa1 == "으로" && josa2 == "로"))
                    {
                        if (HasRieulJongseong(word))
                            selectedJosa = "로";
                        else
                            selectedJosa = hasJong ? "으로" : "로";
                    }
                    else
                    {
                        selectedJosa = hasJong ? josa1 : josa2;
                        if (string.IsNullOrEmpty(selectedJosa)) return word + marker;
                    }
                    
                    return word + selectedJosa;
                });
            }
            catch
            {
                return text;
            }
        }
        
        private static bool HasJongseong(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;
            char lastChar = GetLastKoreanChar(word);
            if (lastChar == '\0' || lastChar < HANGUL_START || lastChar > HANGUL_END) return false;
            return (lastChar - HANGUL_START) % JONGSEONG_COUNT > 0;
        }
        
        private static bool HasRieulJongseong(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;
            char lastChar = GetLastKoreanChar(word);
            if (lastChar == '\0' || lastChar < HANGUL_START || lastChar > HANGUL_END) return false;
            return (lastChar - HANGUL_START) % JONGSEONG_COUNT == RIEUL_JONGSEONG;
        }
        
        private static char GetLastKoreanChar(string word)
        {
            for (int i = word.Length - 1; i >= 0; i--)
            {
                char c = word[i];
                if (c >= HANGUL_START && c <= HANGUL_END) return c;
            }
            return '\0';
        }
    }
}
