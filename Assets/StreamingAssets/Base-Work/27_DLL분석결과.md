# Assembly-CSharp.dll 분석 결과

**작성일:** 2026-01-13 13:21  
**목적:** 대화 텍스트 표시 메서드 찾기

---

## 🎯 발견한 핵심 클래스/메서드

### 1. XRL.UI.ConversationUI
**대화 UI 렌더링 클래스**

```
발견된 메서드:
- GetDisplayText          ← 핵심! 표시할 텍스트 가져오기
- GetTextToRead           ← 텍스트 읽기
- GetTextNode             ← 텍스트 노드 가져오기
- RenderableLines         ← 렌더링 가능한 라인
- RenderableSelection     ← 렌더링 가능한 선택지
```

### 2. XRL.World.Conversations.DisplayTextEvent
**대화 텍스트 표시 이벤트**

```
발견된 이벤트:
- DisplayTextEvent        ← 텍스트 표시 시 발생
- GetTextElementEvent     ← 텍스트 요소 가져올 때
- ColorTextEvent          ← 텍스트 색상 지정
```

### 3. XRL.World.Conversations.ConversationEvent
**대화 이벤트 기본 클래스**

---

## 📋 Harmony 패치 전략

### Harmony 문서 요약

**Postfix 패치 (추천):**
```csharp
[HarmonyPostfix]
static void Postfix(ref string __result)
{
    __result = Korean.ReplaceJosa(__result);
}
```

**특징:**
- 원본 메서드 실행 후 호출
- `__result`로 반환값 수정 가능
- 가장 안전한 패치 방식

---

## 🔧 정확한 Harmony 패치 코드

### 패치 1: ConversationUI.GetDisplayText

```csharp
/// <summary>
/// 대화 텍스트 표시 패치 - 핵심!
/// </summary>
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

### 패치 2: ConversationUI.GetTextToRead

```csharp
/// <summary>
/// 대화 텍스트 읽기 패치 - 보조
/// </summary>
[HarmonyPatch(typeof(XRL.UI.ConversationUI))]
class ConversationUI_GetTextToRead_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch("GetTextToRead")]
    static void Postfix(ref string __result)
    {
        if (!string.IsNullOrEmpty(__result))
        {
            __result = Korean.ReplaceJosa(__result);
        }
    }
}
```

### 패치 3: DisplayTextEvent (선택)

```csharp
/// <summary>
/// 대화 텍스트 이벤트 패치 - 추가 보장
/// </summary>
[HarmonyPatch(typeof(XRL.World.Conversations.DisplayTextEvent))]
class DisplayTextEvent_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch("GetText")]
    static void Postfix(ref string __result)
    {
        if (!string.IsNullOrEmpty(__result))
        {
            __result = Korean.ReplaceJosa(__result);
        }
    }
}
```

---

## ✅ 검증된 정보

### DLL 위치
```
/Users/ben/Library/Application Support/Steam/steamapps/common/Caves of Qud/CoQ.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll
```

### 확인된 클래스
- ✅ `XRL.UI.ConversationUI` 존재
- ✅ `GetDisplayText` 메서드 존재
- ✅ `XRL.World.Conversations.DisplayTextEvent` 존재

### Harmony 문서 확인
- ✅ Postfix 패치 방식 확인
- ✅ `ref string __result` 사용법 확인
- ✅ 여러 메서드 패치 가능 확인

---

## 🚀 다음 단계

1. JosaHandler.cs 업데이트
2. 3개 패치 모두 추가
3. 게임 재시작
4. 테스트

---

**예상 결과:**
- ✅ 대화 텍스트 조사 처리
- ✅ 대화 선택지 조사 처리
- ✅ 모든 대화 시스템 작동
