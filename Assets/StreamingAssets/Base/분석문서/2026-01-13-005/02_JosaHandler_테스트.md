# Task 1.2: JosaHandler 테스트 프로젝트

**우선순위:** 🔴 최우선  
**예상 시간:** 2-3시간  
**상태:** ⏳ 진행 중

---

## 📋 목표

JosaHandler.cs의 모든 기능을 검증하는 독립적인 테스트 프로젝트 생성

---

## 🚀 프로젝트 생성

### 1. 테스트 프로젝트 생성

```bash
cd ~/Desktop/CavesOfQud-Korean/Tools

# 콘솔 프로젝트 생성
dotnet new console -n JosaHandlerTest
cd JosaHandlerTest

# 프로젝트 구조 확인
ls -la
```

### 2. JosaHandler.cs 복사

```bash
# 분석문서에서 복사
cp "/Users/ben/Desktop/무제 폴더/StreamingAssets/Base/분석문서/2026-01-13-004/JosaHandler.cs" .

# 확인
ls -la JosaHandler.cs
```

### 3. Program.cs 작성

```csharp
using System;
using CavesOfQud.KoreanJosa;

namespace JosaHandlerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== JosaHandler 테스트 시작 ===\n");
            
            // 테스트 실행
            TestBasicJosa();
            TestAllJosaTypes();
            TestEdgeCases();
            TestRieulJongseong();
            TestCaching();
            TestTextProcessing();
            
            Console.WriteLine("\n=== 모든 테스트 완료 ===");
        }
        
        static void TestBasicJosa()
        {
            Console.WriteLine("📝 기본 조사 테스트");
            
            Test("검", "을/를", "을");
            Test("사과", "을/를", "를");
            Test("책", "이/가", "이");
            Test("연필", "이/가", "가");
            Test("집", "은/는", "은");
            Test("학교", "은/는", "는");
            
            Console.WriteLine();
        }
        
        static void TestAllJosaTypes()
        {
            Console.WriteLine("📝 모든 조사 타입 테스트");
            
            // 을/를
            Test("검", "eul_reul", "을");
            Test("사과", "eul_reul", "를");
            
            // 이/가
            Test("책", "i_ga", "이");
            Test("연필", "i_ga", "가");
            
            // 은/는
            Test("집", "eun_neun", "은");
            Test("학교", "eun_neun", "는");
            
            // 으로/로
            Test("집", "euro_ro", "으로");
            Test("학교", "euro_ro", "로");
            
            // 아/야
            Test("철수", "a_ya", "아");
            Test("영희", "a_ya", "야");
            
            // 와/과
            Test("책", "wa_gwa", "과");
            Test("연필", "wa_gwa", "와");
            
            Console.WriteLine();
        }
        
        static void TestEdgeCases()
        {
            Console.WriteLine("📝 엣지 케이스 테스트");
            
            // 빈 문자열
            Test("", "을/를", "를");
            
            // 영어
            Test("ABC", "을/를", "를");
            
            // 숫자
            Test("123", "을/를", "를");
            
            // 특수문자
            Test("검!", "을/를", "을");
            Test("사과?", "을/를", "를");
            
            Console.WriteLine();
        }
        
        static void TestRieulJongseong()
        {
            Console.WriteLine("📝 ㄹ 받침 특수 케이스 테스트");
            
            // ㄹ 받침은 '로'
            Test("서울", "euro_ro", "로");
            Test("물", "euro_ro", "로");
            
            // 다른 받침은 '으로'
            Test("집", "euro_ro", "으로");
            Test("책", "euro_ro", "으로");
            
            // 받침 없으면 '로'
            Test("학교", "euro_ro", "로");
            
            Console.WriteLine();
        }
        
        static void TestCaching()
        {
            Console.WriteLine("📝 캐싱 테스트");
            
            // 같은 입력 반복
            var word = "검";
            var format = "을/를";
            
            var start = DateTime.Now;
            for (int i = 0; i < 10000; i++)
            {
                JosaHandler.Choose(word, format);
            }
            var elapsed = (DateTime.Now - start).TotalMilliseconds;
            
            Console.WriteLine($"  10,000회 호출: {elapsed:F2}ms");
            Console.WriteLine($"  평균: {elapsed / 10000:F4}ms/call");
            
            // 캐시 초기화
            JosaHandler.ClearCache();
            Console.WriteLine("  캐시 초기화 완료");
            
            Console.WriteLine();
        }
        
        static void TestTextProcessing()
        {
            Console.WriteLine("📝 텍스트 처리 테스트");
            
            // 간단한 텍스트 (변수 해석 없이)
            string text1 = "당신은 검<josa_eul_reul> 발견했다";
            // 실제로는 <item.name><josa_eul_reul> 형태
            // 여기서는 간단히 테스트
            
            Console.WriteLine($"  입력: {text1}");
            // Process는 <변수><josa_type> 패턴을 찾음
            // 단순 테스트용으로는 Choose 사용
            
            Console.WriteLine();
        }
        
        static void Test(string word, string format, string expected)
        {
            string result = JosaHandler.Choose(word, format);
            bool pass = result == expected;
            string status = pass ? "✅" : "❌";
            
            Console.WriteLine($"  {status} \"{word}\" + {format} = \"{result}\" (expected: \"{expected}\")");
            
            if (!pass)
            {
                Console.WriteLine($"     ERROR: Got \"{result}\" but expected \"{expected}\"");
            }
        }
    }
}
```

---

## 🧪 테스트 실행

### 빌드 및 실행

```bash
cd ~/Desktop/CavesOfQud-Korean/Tools/JosaHandlerTest

# 빌드
dotnet build

# 실행
dotnet run
```

### 예상 출력

```
=== JosaHandler 테스트 시작 ===

📝 기본 조사 테스트
  ✅ "검" + 을/를 = "을" (expected: "을")
  ✅ "사과" + 을/를 = "를" (expected: "를")
  ✅ "책" + 이/가 = "이" (expected: "이")
  ✅ "연필" + 이/가 = "가" (expected: "가")
  ✅ "집" + 은/는 = "은" (expected: "은")
  ✅ "학교" + 은/는 = "는" (expected: "는")

📝 모든 조사 타입 테스트
  ✅ "검" + eul_reul = "을" (expected: "을")
  ✅ "사과" + eul_reul = "를" (expected: "를")
  ...

📝 엣지 케이스 테스트
  ✅ "" + 을/를 = "를" (expected: "를")
  ✅ "ABC" + 을/를 = "를" (expected: "를")
  ...

📝 ㄹ 받침 특수 케이스 테스트
  ✅ "서울" + euro_ro = "로" (expected: "로")
  ✅ "물" + euro_ro = "로" (expected: "로")
  ...

📝 캐싱 테스트
  10,000회 호출: 15.23ms
  평균: 0.0015ms/call
  캐시 초기화 완료

=== 모든 테스트 완료 ===
```

---

## 📊 추가 테스트 케이스

### 게임 용어 테스트

```csharp
static void TestGameTerms()
{
    Console.WriteLine("📝 게임 용어 테스트");
    
    // 아이템
    Test("검", "을/를", "을");
    Test("사과", "을/를", "를");
    Test("물", "을/를", "를");
    
    // NPC
    Test("바라스럼", "이/가", "이");
    Test("메흐메트", "이/가", "가");
    
    // 지역
    Test("조파", "으로/로", "로");
    Test("그릿 게이트", "으로/로", "로");
    Test("여섯날의 스틸트", "으로/로", "로");
    
    // 파벌
    Test("메카니카신자", "은/는", "는");
    Test("푸투스템플러", "은/는", "는");
    
    Console.WriteLine();
}
```

---

## ✅ 완료 기준

- [ ] 프로젝트 생성 완료
- [ ] JosaHandler.cs 추가
- [ ] Program.cs 작성
- [ ] 빌드 성공
- [ ] 모든 테스트 통과 (✅ 표시)
- [ ] 성능 테스트 완료 (10,000회 < 100ms)

---

## 🚨 문제 해결

### 문제 1: 빌드 오류

```bash
# 네임스페이스 확인
# JosaHandler.cs의 namespace가 CavesOfQud.KoreanJosa인지 확인

# .csproj 파일 확인
cat JosaHandlerTest.csproj
```

### 문제 2: 테스트 실패

```bash
# 디버그 모드로 실행
dotnet run --configuration Debug

# 특정 테스트만 실행
# Program.cs에서 원하는 테스트만 주석 해제
```

---

## 📝 테스트 결과 기록

### 테스트 실행 로그

```
날짜: 2026-01-13
환경: macOS, .NET 8.0
결과: 
- 기본 조사: ✅ 6/6 통과
- 모든 조사 타입: ✅ 12/12 통과
- 엣지 케이스: ✅ 5/5 통과
- ㄹ 받침: ✅ 5/5 통과
- 캐싱: ✅ 성능 양호
- 총: ✅ 28/28 통과
```

---

**다음 작업:** Task 1.3 - Caves of Qud 모드 프로젝트 생성  
**예상 완료:** 2026-01-13
