/*
 * 파일명: 99_SystemCapabilityTest.cs
 * 분류: [Test] 시스템 기능 테스트
 * 역할: 현재 Mono/Unity 환경에서 사용 가능한 .NET 기능을 파악합니다.
 * 수정일: 2026-01-14
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace QudKRContent
{
    public static class SystemCapabilityTest
    {
        public static void RunTests()
        {
            Debug.Log("========================================");
            Debug.Log("[Qud-KR] 시스템 기능 테스트 시작");
            Debug.Log("========================================");
            
            // 1. .NET Framework 버전
            TestFrameworkVersion();
            
            // 2. LINQ 사용 가능 여부
            TestLinq();
            
            // 3. Async/Await 사용 가능 여부
            TestAsync();
            
            // 4. 람다 표현식
            TestLambda();
            
            // 5. 제네릭 컬렉션
            TestGenerics();
            
            // 6. String 메서드
            TestStringMethods();
            
            // 7. Reflection 기능
            TestReflection();
            
            Debug.Log("========================================");
            Debug.Log("[Qud-KR] 시스템 기능 테스트 완료");
            Debug.Log("========================================");
        }
        
        private static void TestFrameworkVersion()
        {
            try
            {
                var version = Environment.Version;
                Debug.Log($"[Test] .NET Version: {version}");
                Debug.Log($"[Test] OS Version: {Environment.OSVersion}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Test] Framework Version 테스트 실패: {e.Message}");
            }
        }
        
        private static void TestLinq()
        {
            try
            {
                // System.Linq 없이 테스트
                var list = new List<int> { 1, 2, 3, 4, 5 };
                
                // LINQ 사용 시도
                #if false
                var result = list.Where(x => x > 3).ToList();
                Debug.Log($"[Test] LINQ 사용 가능: {result.Count}개 항목");
                #else
                Debug.Log("[Test] LINQ: 테스트 스킵 (System.Linq 미포함)");
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"[Test] LINQ 테스트 실패: {e.Message}");
            }
        }
        
        private static void TestAsync()
        {
            try
            {
                // async/await는 Unity에서 제한적
                Debug.Log("[Test] Async/Await: Unity에서 제한적 지원 (Coroutine 사용 권장)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Test] Async 테스트 실패: {e.Message}");
            }
        }
        
        private static void TestLambda()
        {
            try
            {
                // 람다 표현식
                Func<int, int> square = x => x * x;
                int result = square(5);
                Debug.Log($"[Test] 람다 표현식 사용 가능: 5^2 = {result}");
                
                // Action
                Action<string> print = msg => Debug.Log($"[Test] Action: {msg}");
                print("테스트 성공");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Test] 람다 테스트 실패: {e.Message}");
            }
        }
        
        private static void TestGenerics()
        {
            try
            {
                // Dictionary
                var dict = new Dictionary<string, int>();
                dict["test"] = 123;
                Debug.Log($"[Test] Dictionary 사용 가능: {dict["test"]}");
                
                // List
                var list = new List<string> { "a", "b", "c" };
                Debug.Log($"[Test] List 사용 가능: {list.Count}개 항목");
                
                // HashSet
                var set = new HashSet<int> { 1, 2, 3, 2, 1 };
                Debug.Log($"[Test] HashSet 사용 가능: {set.Count}개 고유 항목");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Test] Generics 테스트 실패: {e.Message}");
            }
        }
        
        private static void TestStringMethods()
        {
            try
            {
                string test = "Hello World";
                
                // String.Join
                string joined = string.Join(", ", new string[] { "a", "b", "c" });
                Debug.Log($"[Test] String.Join 사용 가능: {joined}");
                
                // String interpolation
                string name = "Qud";
                string interpolated = $"Game: {name}";
                Debug.Log($"[Test] String interpolation 사용 가능: {interpolated}");
                
                // Contains, StartsWith, EndsWith
                bool contains = test.Contains("World");
                bool starts = test.StartsWith("Hello");
                bool ends = test.EndsWith("World");
                Debug.Log($"[Test] String 메서드 사용 가능: Contains={contains}, StartsWith={starts}, EndsWith={ends}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Test] String 테스트 실패: {e.Message}");
            }
        }
        
        private static void TestReflection()
        {
            try
            {
                // Type 정보
                var type = typeof(SystemCapabilityTest);
                Debug.Log($"[Test] Reflection 사용 가능: {type.Name}");
                
                // AccessTools (Harmony)
                var optionsType = AccessTools.TypeByName("Qud.UI.OptionsScreen");
                if (optionsType != null)
                {
                    var methods = optionsType.GetMethods();
                    Debug.Log($"[Test] AccessTools 사용 가능: OptionsScreen에 {methods.Length}개 메서드");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Test] Reflection 테스트 실패: {e.Message}");
            }
        }
    }
}
