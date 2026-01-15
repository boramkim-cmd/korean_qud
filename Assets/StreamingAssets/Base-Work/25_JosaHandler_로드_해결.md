# JosaHandler.cs 로드 문제 해결

**문제:** C# 스크립트가 게임에 로드되지 않음  
**원인:** Caves of Qud 모드 시스템의 C# 스크립트 로딩 방식 문제

---

## 🔍 문제 진단

### 현재 상태
```
게임 로그: "[Korean Josa]" 메시지 없음
→ JosaHandler.cs가 컴파일/실행되지 않음
→ Harmony 패치 적용 안 됨
→ 조사 처리 안 됨
```

### Caves of Qud 모드 C# 로딩 방식

**정상 작동 시:**
1. 게임 시작
2. `manifest.json` 읽기
3. `preloadScripts`의 C# 파일 컴파일
4. 컴파일된 DLL 로드
5. Harmony 패치 적용
6. 초기화 코드 실행

**현재 상태:**
- 3단계에서 실패 (컴파일 에러 또는 로딩 실패)

---

## 🎯 해결 방법

### 방법 1: 초기화 코드 수정 (가장 가능성 높음)

**문제:** `KoreanJosaInit` 클래스가 호출되지 않음

**현재 코드 (Line 131-140):**
```csharp
public class KoreanJosaInit
{
    static KoreanJosaInit()
    {
        UnityEngine.Debug.Log("========================================");
        UnityEngine.Debug.Log("[Korean Josa] v10 - Hotfix");
        UnityEngine.Debug.Log("[Korean Josa] MessageQueue Patch Active");
        UnityEngine.Debug.Log("========================================");
    }
}
```

**문제점:**
- 이 클래스가 자동으로 인스턴스화되지 않음
- static 생성자가 호출되지 않음

**해결책:** Harmony 패치에 직접 로그 추가

```csharp
[HarmonyPatch(typeof(XRL.Messages.MessageQueue))]
class MessageQueue_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch("Add")]
    static void Prefix(ref string Message)
    {
        // 첫 실행 시 로그 출력
        if (!_initialized)
        {
            UnityEngine.Debug.Log("[Korean Josa] v10 - Loaded!");
            _initialized = true;
        }
        
        if (!string.IsNullOrEmpty(Message))
        {
            Message = Korean.ReplaceJosa(Message);
        }
    }
    
    private static bool _initialized = false;
}
```

---

### 방법 2: Harmony 패치 타겟 확인

**문제:** `XRL.Messages.MessageQueue` 클래스가 존재하지 않을 수 있음

**확인 방법:**
1. 게임 DLL 디컴파일 필요
2. 정확한 클래스 이름 확인

**임시 해결:**
- 더 안전한 패치 타겟 사용
- 예: `XRL.UI.TextConsole` 등

---

### 방법 3: 조사 마커 제거 (즉시 해결)

**가장 빠른 방법:**
1. `Conversations.xml`에서 모든 `(이)가`, `(을)를` 제거
2. 자연스러운 한글로 수정
3. 즉시 작동

**장점:**
- C# 문제 해결 불필요
- 즉시 적용 가능
- 안정적

**단점:**
- 수동 작업 필요
- 변수와 함께 사용 불가

---

## 🔧 즉시 적용 가능한 수정

### JosaHandler.cs 수정안

```csharp
// ==================================================
// Caves of Qud 한글 조사 처리 시스템 v11
// 수정: 초기화 로그를 Harmony 패치 내부로 이동
// ==================================================

using System;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace KoreanLocalization.HarmonyPatches
{
    public static class Korean
    {
        private const int HANGUL_START = 0xAC00;
        private const int HANGUL_END = 0xD7A3;
        private const int JONGSEONG_COUNT = 28;
        private const int RIEUL_JONGSEONG = 8;
        
        private static readonly Regex JosaPattern = 
            new Regex(@"(\S+)\(([^)]+)\)([^(\s]*)", RegexOptions.Compiled);
        
        public static string ReplaceJosa(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            try
            {
                if (text.IndexOf('(') == -1)
                    return text;

                return JosaPattern.Replace(text, match =>
                {
                    string word = match.Groups[1].Value;
                    string josa1 = match.Groups[2].Value;
                    string josa2 = match.Groups[3].Value;
                    
                    bool hasJong = HasJongseong(word);
                    
                    string selectedJosa;
                    
                    if (josa1 == "으" && josa2 == "로")
                    {
                        if (HasRieulJongseong(word))
                            selectedJosa = josa2;
                        else
                            selectedJosa = hasJong ? josa1 + josa2 : josa2;
                    }
                    else
                    {
                        selectedJosa = hasJong ? josa1 : josa2;
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
            if (string.IsNullOrEmpty(word))
                return false;
            
            char lastChar = GetLastKoreanChar(word);
            
            if (lastChar == '\0' || lastChar < HANGUL_START || lastChar > HANGUL_END)
                return false;
            
            return (lastChar - HANGUL_START) % JONGSEONG_COUNT > 0;
        }
        
        private static bool HasRieulJongseong(string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;
            
            char lastChar = GetLastKoreanChar(word);
            
            if (lastChar == '\0' || lastChar < HANGUL_START || lastChar > HANGUL_END)
                return false;
            
            return (lastChar - HANGUL_START) % JONGSEONG_COUNT == RIEUL_JONGSEONG;
        }
        
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
    }
    
    [HarmonyPatch(typeof(XRL.Messages.MessageQueue))]
    class MessageQueue_Patch
    {
        private static bool _initialized = false;
        
        [HarmonyPrefix]
        [HarmonyPatch("Add")]
        static void Prefix(ref string Message)
        {
            // 첫 실행 시 로그 출력
            if (!_initialized)
            {
                try
                {
                    UnityEngine.Debug.Log("========================================");
                    UnityEngine.Debug.Log("[Korean Josa] v11 - Successfully Loaded!");
                    UnityEngine.Debug.Log("[Korean Josa] MessageQueue Patch Active");
                    UnityEngine.Debug.Log("========================================");
                    _initialized = true;
                }
                catch
                {
                    // 로그 실패해도 계속 진행
                }
            }
            
            if (!string.IsNullOrEmpty(Message))
            {
                Message = Korean.ReplaceJosa(Message);
            }
        }
    }
}
```

---

## 📋 적용 방법

### 1. JosaHandler.cs 교체
```bash
# 수정된 파일로 교체
cp /path/to/new/JosaHandler.cs \
   ~/Library/Application\ Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization/Scripts/
```

### 2. 게임 완전 재시작
```
1. Caves of Qud 완전 종료
2. 재실행
3. Mods 메뉴에서 Korean Localization 비활성화
4. 다시 활성화
5. 게임 재시작
```

### 3. 로그 확인
```bash
tail -f ~/Library/Application\ Support/com.FreeholdGames.CavesOfQud/Player.log
```

**찾아야 할 메시지:**
```
[Korean Josa] v11 - Successfully Loaded!
[Korean Josa] MessageQueue Patch Active
```

---

## 🚨 여전히 안 되면

### 대안: 조사 마커 완전 제거

**Conversations.xml 수정:**
- 모든 `(이)가` → `이` 또는 `가`
- 모든 `(을)를` → `을` 또는 `를`
- 모든 `(으)로` → `로` 또는 `으로`

**장점:**
- 100% 작동 보장
- C# 문제 무관
- 즉시 적용

---

**결론:**
1. 수정된 JosaHandler.cs 적용 (v11)
2. 게임 재시작
3. 로그 확인
4. 안 되면 조사 마커 제거
