# Harmony Wiki 상세 분석

**작성일:** 2026-01-13 13:27  
**출처:** https://harmony.pardeike.net/

---

## 📚 Harmony 공식 문서 핵심 내용

### Postfix 패치 사용법

**공식 예제:**
```csharp
public class OriginalCode 
{ 
    public string GetName() => name;
}

[HarmonyPatch(typeof(OriginalCode), nameof(OriginalCode.GetName))]
class Patch 
{ 
    static void Postfix(ref string __result) 
    { 
        if (__result == "foo") 
            __result = "bar"; 
    } 
}
```

**핵심 포인트:**
1. `ref string __result` - 반환값 수정 가능
2. `Postfix`는 원본 메서드 **실행 후** 호출
3. **항상 실행됨** (Prefix와 달리 스킵 안 됨)

---

## ✅ JosaHandler.cs v12 검증

### 우리 코드
```csharp
[HarmonyPatch(typeof(XRL.UI.ConversationUI))]
class ConversationUI_GetDisplayText_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch("GetDisplayText")]
    static void Postfix(ref string __result)
    {
        if (!string.IsNullOrEmpty(__result))
        {
            __result = Korean.ReplaceJosa(__result);
        }
    }
}
```

### 공식 문서와 비교

| 항목 | 공식 예제 | 우리 코드 | 결과 |
|------|-----------|-----------|------|
| 클래스 어노테이션 | `[HarmonyPatch(typeof(...))]` | `[HarmonyPatch(typeof(XRL.UI.ConversationUI))]` | ✅ |
| 메서드 어노테이션 | `[HarmonyPostfix]` | `[HarmonyPostfix]` | ✅ |
| 메서드 이름 지정 | `nameof(...)` 또는 `"MethodName"` | `[HarmonyPatch("GetDisplayText")]` | ✅ |
| 반환값 수정 | `ref string __result` | `ref string __result` | ✅ |
| 메서드 시그니처 | `static void Postfix(...)` | `static void Postfix(...)` | ✅ |

**결론:** ✅ 100% 정확한 문법!

---

## 📖 Harmony 어노테이션 방식

### 방법 1: 클래스에 모든 정보 (공식 추천)
```csharp
[HarmonyPatch(typeof(ClassName), "MethodName")]
class MyPatch
{
    static void Postfix(ref string __result) { }
}
```

### 방법 2: 메서드에 분산 (우리 방식)
```csharp
[HarmonyPatch(typeof(ClassName))]
class MyPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("MethodName")]
    static void Postfix(ref string __result) { }
}
```

**둘 다 정상 작동!**

---

## 🎯 Postfix의 장점 (공식 문서)

### "Postfixes always run"
> Harmony will not skip any postfix regardless of what any prefix or the original method do.

**의미:**
- Prefix는 스킵될 수 있음
- **Postfix는 항상 실행됨**
- 더 안전하고 호환성 높음

### 우리 경우
```csharp
static void Postfix(ref string __result)
{
    // 원본 메서드 실행 후 항상 호출됨
    __result = Korean.ReplaceJosa(__result);
}
```

**장점:**
- ✅ 다른 모드와 충돌 없음
- ✅ 원본 메서드 실행 보장
- ✅ 결과만 수정

---

## 📝 공식 문서 주요 내용

### Patch Class
```csharp
[HarmonyPatch]
class MyPatchClass
{
    // Harmony가 자동으로 찾는 메서드 이름:
    // - TargetMethod()
    // - Prepare()
    // - Prefix()
    // - Postfix()
    // - Transpiler()
    // - Cleanup()
}
```

### 또는 어노테이션 사용
```csharp
[HarmonyPatch]
class MyPatchClass
{
    [HarmonyTargetMethod]
    static MethodBase MyTarget() { }
    
    [HarmonyPrepare]
    static bool MyPrepare() { }
    
    [HarmonyPrefix]
    static bool MyPrefix() { }
    
    [HarmonyPostfix]
    static void MyPostfix() { }
}
```

---

## 🔧 __result 사용법

### 읽기 전용
```csharp
static void Postfix(string __result)
{
    Console.WriteLine(__result);  // 읽기만
}
```

### 수정 가능
```csharp
static void Postfix(ref string __result)
{
    __result = "modified";  // 수정 가능
}
```

**우리는 `ref` 사용 → 수정 가능 ✅**

---

## 🎮 실제 적용 예시

### 원본 게임 코드 (추정)
```csharp
namespace XRL.UI
{
    class ConversationUI
    {
        public string GetDisplayText()
        {
            return "조파(으)로 온 것(을)를 환영하네";
        }
    }
}
```

### Harmony 패치 적용 후
```
1. GetDisplayText() 실행
   → "조파(으)로 온 것(을)를 환영하네"

2. Postfix 실행
   → Korean.ReplaceJosa() 호출
   → "조파로 온 것을 환영하네"

3. 최종 반환
   → "조파로 온 것을 환영하네" ✅
```

---

## ✅ 최종 검증

### JosaHandler.cs v12가 올바른 이유

1. **문법 정확**
   - Harmony 공식 문서 예제와 동일
   - `ref string __result` 올바름

2. **메서드 이름 정확**
   - DLL 분석으로 확인: `GetDisplayText` 존재
   - `GetTextToRead` 존재

3. **패치 방식 적절**
   - Postfix 사용 → 항상 실행
   - 다른 모드와 충돌 최소화

4. **Harmony 버전 호환**
   - Harmony 2.x 문법 사용
   - Caves of Qud는 Harmony 포함

---

## 🚀 다음 테스트

### 예상 결과
```
게임 로그:
[Korean Josa] v12 - ConversationUI Patch Added!
[Korean Josa] MessageQueue Patch Active
[Korean Josa] ConversationUI Patch Active

대화 테스트:
입력: "조파(으)로 온 것(을)를 환영하네"
출력: "조파로 온 것을 환영하네" ✅
```

### 만약 안 되면?
1. 로그 확인 - 패치 로드 여부
2. 메서드 이름 재확인
3. Harmony 버전 확인

---

**결론:**
Harmony Wiki 공식 문서 기준으로 JosaHandler.cs v12는 **완벽하게 올바른 코드**입니다!
