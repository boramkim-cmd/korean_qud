# Caves of Qud 한글화 - 공식 모딩 시스템 기반 재설계

> **문서 상태**: 원본 계획 (비판적 검토 필요)
> **작성일**: 2026-01-23
> **관련 문서**: [PLAN_02_CRITICAL_ANALYSIS.md](./PLAN_02_CRITICAL_ANALYSIS.md)

---

## 공식 문서에서 배운 것

### 모딩 3단계 (권장 순서)
1. **Data Mods (XML)** - 가장 안정적, 게임 업데이트에 강함
2. **Script Mods (C#)** - Parts/Events 시스템 활용
3. **Harmony Mods** - **최후의 수단** (공식 문서: "last resort only")

### 현재 한글화 모드의 문제
- Harmony 패치 203개 사용 → 공식 권장과 정반대
- Parts/Events 시스템 미활용
- 게임 업데이트마다 깨질 위험

### 공식 권장 대안
```
GetDisplayNameEvent  → 오브젝트 이름 변경에 사용 가능
GetShortDescriptionEvent → 설명 변경에 사용 가능
IPart 상속 → 커스텀 동작 추가
WishCommand → 디버그 명령어
```

---

## 핵심 전략

1. **디버그 시스템 먼저** - 문제 파악
2. **Parts/Events로 전환 검토** - Harmony 의존도 감소
3. **XML 오버라이드 활용** - 정적 텍스트는 XML로

---

## 1. 디버그 시스템 설계

### 1.1 표시 형식

```
# 성공
일반:   "횃불"
디버그: "{{g|[OK]}} 횃불 {{y|← Torch}}"

# 번역 없음
일반:   "Wyrm"
디버그: "{{r|[NO_TR]}} Wyrm"

# 스코프 불일치
일반:   "" (빈 값)
디버그: "{{o|[SCOPE]}} Ancient Crypt {{y|scope:chargen→gameplay}}"

# 태그 복원 실패
일반:   "{{r|Torch}}" (원문 그대로)
디버그: "{{o|[TAG]}} Torch {{y|restore failed}}"

# null 반환됨
일반:   ""
디버그: "{{r|[NULL]}} ← SomeText"
```

### 1.2 고려해야 할 문제들

**문제 1: 디버그 텍스트가 UI를 깨뜨림**
- 버튼에 긴 디버그 텍스트가 들어가면 레이아웃 붕괴
- 해결: 짧은 코드만 표시, 상세 정보는 별도 오버레이

**문제 2: 색상 태그 중첩**
- 원본이 `{{r|text}}`인데 디버그가 `{{g|[OK]}} {{r|text}}`가 되면?
- 해결: 디버그 모드에서는 원본 태그 strip 후 디버그 태그만 사용

**문제 3: 성능**
- 매 프레임 TMP_Text.Setter가 호출됨
- 디버그 체크가 느리면 게임 렉
- 해결: `TranslationDebug.Enabled` 체크를 가장 먼저, 비활성화 시 즉시 리턴

**문제 4: 어디서 실패했는지 추적**
- 같은 텍스트가 여러 경로로 번역 시도됨
- 해결: 호출 스택 또는 패치 이름 기록

### 1.3 디버그 레벨

```csharp
public enum DebugLevel
{
    Off,      // 일반 모드
    Minimal,  // 실패한 것만 표시: [NO_TR], [SCOPE], [NULL]
    Normal,   // 성공도 표시: [OK]
    Verbose   // 상세 정보: 원문, 스코프, 호출 위치
}
```

---

## 2. 아키텍처 재설계

### 2.1 현재 문제: 번역 시스템 4개 분산

```
TMP_Text.Setter
    ├→ TranslationEngine.TryTranslate()
    ├→ LocalizationManager.TryGetAnyTerm()
    ├→ TranslationUtils.TryTranslatePreservingTags()
    └→ Patch_UITextSkin_SetText.TryGetHardcodedTranslation()

GetDisplayNameEvent
    └→ ObjectTranslator.TryGetDisplayName()
```

### 2.2 해결: 단일 진입점 + 디버그 래퍼

```csharp
// 모든 번역이 이 함수를 통과
public static class TranslationHub
{
    public static string Translate(
        string input,
        string context,      // "TMP_Text", "DisplayName", "Popup" 등
        string[] categories  // ["ui", "common"] 등
    )
    {
        var result = TryTranslateInternal(input, categories, out var code, out var details);

        return TranslationDebug.Format(input, result, code, context, details);
    }
}
```

### 2.3 TranslationResult 구조체

```csharp
public struct TranslationResult
{
    public string Output;           // 번역된 텍스트 (또는 원문)
    public ResultCode Code;         // OK, NO_ENTRY, SCOPE, TAG, NULL
    public string OriginalInput;    // 원본 입력
    public string StrippedInput;    // 태그 제거된 입력
    public string MatchedKey;       // 실제 매칭된 키 (있으면)
    public string CurrentScope;     // 현재 스코프
    public string FoundInScope;     // 번역이 발견된 스코프 (있으면)
    public string CallerPatch;      // 호출한 패치 이름
}
```

---

## 3. 구현 계획

### Phase 1: 디버그 인프라 (읽기 전용, 기존 코드 안 건드림)

**파일: `Scripts/00_Core/00_00_04_TranslationDebug.cs` (신규)**

```csharp
public static class TranslationDebug
{
    public static DebugLevel Level = DebugLevel.Off;

    // 최근 번역 결과 저장 (오버레이 표시용)
    public static Queue<TranslationResult> RecentResults = new(100);

    public static string Format(TranslationResult r)
    {
        if (Level == DebugLevel.Off)
            return r.Output;

        // 실패만 표시
        if (Level == DebugLevel.Minimal && r.Code == ResultCode.OK)
            return r.Output;

        return FormatDebugOutput(r);
    }

    private static string FormatDebugOutput(TranslationResult r)
    {
        string prefix = r.Code switch
        {
            ResultCode.OK => "{{g|✓}}",
            ResultCode.NO_ENTRY => "{{r|✗}}",
            ResultCode.SCOPE => "{{o|⚠}}",
            ResultCode.TAG => "{{y|⚡}}",
            ResultCode.NULL => "{{r|∅}}",
            _ => "{{w|?}}"
        };

        if (Level == DebugLevel.Verbose)
            return $"{prefix} {r.Output} {{{{y|← {r.OriginalInput} @{r.CallerPatch}}}}}";

        return $"{prefix} {r.Output}";
    }
}
```

**파일: `Scripts/02_Patches/20_Objects/02_20_99_DebugWishes.cs` (수정)**

```csharp
// 기존 위시에 추가
[WishCommand("kr:debug")]
public static void ToggleDebug(string arg)
{
    if (arg == "off") TranslationDebug.Level = DebugLevel.Off;
    else if (arg == "min") TranslationDebug.Level = DebugLevel.Minimal;
    else if (arg == "verbose") TranslationDebug.Level = DebugLevel.Verbose;
    else TranslationDebug.Level = TranslationDebug.Level == DebugLevel.Off
        ? DebugLevel.Normal : DebugLevel.Off;

    Popup.Show($"Debug: {TranslationDebug.Level}");
}

[WishCommand("kr:scope")]
public static void ShowScope()
{
    var depth = ScopeManager.GetDepth();
    var info = ScopeManager.GetDebugInfo(); // 새로 추가할 메서드
    Popup.Show($"Scope depth: {depth}\n{info}");
}

[WishCommand("kr:recent")]
public static void ShowRecentMisses()
{
    var misses = TranslationDebug.RecentResults
        .Where(r => r.Code != ResultCode.OK)
        .Take(20);

    var text = string.Join("\n", misses.Select(r =>
        $"[{r.Code}] {r.OriginalInput} @{r.CallerPatch}"));

    Popup.Show(text);
}
```

### Phase 2: TranslationEngine에 ResultCode 추가

**파일: `Scripts/00_Core/00_00_01_TranslationEngine.cs`**

```csharp
// 기존 메서드 유지 (호환성)
public static bool TryTranslate(string text, out string translated)
{
    var result = TryTranslateWithResult(text, ScopeManager.GetCurrentScope(), "Unknown");
    translated = result.Output;
    return result.Code == ResultCode.OK;
}

// 새 메서드 (디버그 정보 포함)
public static TranslationResult TryTranslateWithResult(
    string text,
    Dictionary<string, string>[] scopes,
    string callerPatch)
{
    var result = new TranslationResult
    {
        OriginalInput = text,
        CallerPatch = callerPatch,
        CurrentScope = ScopeManager.GetCurrentScopeName()
    };

    if (string.IsNullOrEmpty(text))
    {
        result.Code = ResultCode.NULL;
        result.Output = TranslationDebug.Level != DebugLevel.Off
            ? "{{r|[NULL:EMPTY_INPUT]}}"
            : "";
        return result;
    }

    // ... 기존 로직 ...

    // 번역 실패 시
    result.Code = ResultCode.NO_ENTRY;
    result.Output = text;  // null 대신 원문 반환!

    TranslationDebug.RecentResults.Enqueue(result);
    return result;
}
```

### Phase 3: 각 패치에 디버그 연동

**패턴: 기존 코드를 감싸는 방식 (최소 수정)**

```csharp
// 기존
[HarmonyPrefix]
static void Prefix(ref string value)
{
    if (TranslationEngine.TryTranslate(value, out var result))
    {
        value = result;
    }
}

// 변경
[HarmonyPrefix]
static void Prefix(ref string value)
{
    var result = TranslationEngine.TryTranslateWithResult(
        value,
        ScopeManager.GetCurrentScope(),
        "TMP_Text.Setter"  // 호출 위치 기록
    );
    value = TranslationDebug.Format(result);
}
```

### Phase 4: 스코프 디버그 정보 추가

**파일: `Scripts/00_Core/00_00_02_ScopeManager.cs`**

```csharp
// 스코프에 이름 추가
public class ScopeEntry
{
    public string Name { get; set; }          // "MainMenu", "CharGen"
    public Dictionary<string, string>[] Scopes { get; set; }
    public DateTime PushedAt { get; set; }    // 언제 Push됐는지
}

private static Stack<ScopeEntry> scopeStack = new();

public static void PushScope(string name, params Dictionary<string, string>[] scopes)
{
    scopeStack.Push(new ScopeEntry
    {
        Name = name,
        Scopes = scopes,
        PushedAt = DateTime.Now
    });
}

public static string GetDebugInfo()
{
    return string.Join(" → ", scopeStack.Select(s => s.Name));
}

public static string GetCurrentScopeName()
{
    return scopeStack.Count > 0 ? scopeStack.Peek().Name : "(none)";
}
```

---

## 4. 문제 해결 순서

```
1. kr:debug 입력
   ↓
2. 화면에 [✗], [⚠], [∅] 가 보임
   ↓
3. kr:recent 입력 → 최근 실패 목록 확인
   ↓
4. [NO_ENTRY]가 많음 → JSON에 번역 추가
   [SCOPE]가 많음 → 스코프 Push/Pop 순서 확인
   [TAG]가 많음 → 태그 처리 로직 수정
   [NULL]이 많음 → null 반환 지점 수정
```

---

## 5. 파일 수정 목록

| 우선순위 | 파일 | 작업 |
|---------|------|------|
| 1 | `00_Core/00_00_04_TranslationDebug.cs` | **신규** - 디버그 시스템 |
| 1 | `02_Patches/20_Objects/02_20_99_DebugWishes.cs` | 위시 명령어 추가 |
| 2 | `00_Core/00_00_01_TranslationEngine.cs` | ResultCode 추가, null→원문 |
| 2 | `00_Core/00_00_02_ScopeManager.cs` | 스코프 이름 추가 |
| 3 | `02_Patches/10_UI/02_10_00_GlobalUI.cs` | 디버그 연동 |
| 3 | `02_Patches/20_Objects/02_20_00_ObjectTranslator.cs` | 디버그 연동 |

---

## 6. 검증 방법

1. 게임 실행 → `kr:debug` 입력
2. 메인 메뉴에서 `[✓]`, `[✗]` 등 표시 확인
3. `kr:debug verbose` → 원문과 호출 위치 확인
4. `kr:recent` → 실패한 번역 목록 확인
5. `kr:scope` → 현재 스코프 스택 확인
6. 가장 많은 에러 유형 파악 후 해당 부분 수정

---

## 7. 주의사항

### 7.1 UI 깨짐 방지
- 디버그 텍스트가 너무 길면 버튼/라벨이 깨짐
- 해결: Minimal 모드에서는 아이콘만 (✓, ✗, ⚠)
- Verbose 모드는 넓은 UI (팝업, 메시지 로그)에서만 사용

### 7.2 성능
```csharp
// 모든 번역 함수 시작에:
if (TranslationDebug.Level == DebugLevel.Off)
{
    // 기존 빠른 경로
}
```

### 7.3 기존 코드 호환성
- 기존 `TryTranslate(text, out translated)` 시그니처 유지
- 새 메서드 `TryTranslateWithResult()` 추가
- 점진적으로 새 메서드로 마이그레이션

---

## 8. 장기 개선: Parts/Events 시스템 활용 (Harmony 대체)

### 8.1 공식 문서의 권장사항

> "Harmony should be treated as a last resort—only when desired functionality
> cannot be achieved through the game's existing part and event infrastructure"

현재 모드: Harmony 패치 203개 → **공식 권장의 정반대**

### 8.2 GetDisplayNameEvent 활용 예시

**현재 방식 (Harmony):**
```csharp
[HarmonyPatch(typeof(GetDisplayNameEvent), "GetFor")]
public static class Patch_ObjectDisplayName
{
    [HarmonyPostfix]
    static void GetFor_Postfix(ref string __result, GameObject Object, ...)
    {
        // 번역 로직
    }
}
```

**공식 방식 (Parts/Events):**
```csharp
[Serializable]
public class KoreanTranslationPart : IPart
{
    public override bool WantEvent(int ID, int cascade)
    {
        return ID == GetDisplayNameEvent.ID || ID == GetShortDescriptionEvent.ID;
    }

    public override bool HandleEvent(GetDisplayNameEvent E)
    {
        // E.DB.PrimaryBase에 번역 적용
        if (TryTranslate(E.DB.PrimaryBase, out var translated))
        {
            E.DB.PrimaryBase = translated;
        }
        return base.HandleEvent(E);
    }
}
```

**장점:**
- 게임 내부 시스템과 자연스럽게 통합
- 다른 모드와 충돌 가능성 낮음
- 게임 업데이트에 더 안정적

**단점:**
- 모든 오브젝트에 Part를 추가해야 함 (mixin 사용 가능)
- UI 텍스트는 여전히 Harmony 필요

### 8.3 XML Mixin으로 Part 자동 추가

```xml
<!-- ObjectBlueprints.xml -->
<object Name="Object" Load="Merge">
    <mixin Include="KoreanLocalization_TranslationPart" />
</object>
```

이렇게 하면 모든 오브젝트에 한글화 Part가 자동 추가됨.

### 8.4 하이브리드 접근법 (권장)

| 대상 | 방식 | 이유 |
|------|------|------|
| 오브젝트 이름 | Parts + GetDisplayNameEvent | 공식 API |
| 오브젝트 설명 | Parts + GetShortDescriptionEvent | 공식 API |
| UI 텍스트 (TMP_Text) | Harmony (유지) | 대안 없음 |
| 팝업 메시지 | Harmony (유지) | 대안 없음 |
| 메뉴 텍스트 | Harmony (유지) | 대안 없음 |

**예상 효과:**
- Harmony 패치: 203개 → ~80개로 감소
- 오브젝트 번역 안정성 대폭 향상
- 유지보수 부담 분산

### 8.5 WishCommand 구현 (공식 API)

현재 DebugWishes.cs가 이미 WishCommand를 사용하고 있음 - 좋은 패턴!

```csharp
[HasWishCommand]
public class KoreanWishes
{
    [WishCommand(Command = "kr")]
    public static bool HandleKoreanWish(string rest)
    {
        if (rest == "debug") { /* 토글 */ return true; }
        if (rest == "scope") { /* 스코프 표시 */ return true; }
        if (rest == "reload") { /* 리로드 */ return true; }
        return false;
    }
}
```

---

## 9. 실행 계획

### Phase 1: 디버그 시스템 (즉시)
1. TranslationDebug.cs 생성
2. kr:debug, kr:scope, kr:recent 위시 추가
3. 기존 코드에 ResultCode 추가

### Phase 2: 문제 파악 (디버그 활용)
4. 게임 실행 → kr:debug로 문제 유형 파악
5. 가장 많은 에러 유형 확인
6. null 반환 문제 즉시 수정

### Phase 3: Parts/Events 전환 (장기)
7. KoreanTranslationPart 구현
8. GetDisplayNameEvent 핸들링
9. XML mixin으로 자동 적용
10. Harmony 패치 점진적 제거

---

## 10. 우선순위 정리

| 순위 | 작업 | 방식 | 효과 |
|-----|------|------|------|
| 1 | 디버그 시스템 | C# (Wish) | 문제 가시화 |
| 2 | null → 원문 반환 | 기존 코드 수정 | 빈 값 해결 |
| 3 | 스코프 이름 추가 | 기존 코드 수정 | 디버그 개선 |
| 4 | KoreanTranslationPart | Parts/Events | Harmony 감소 |
| 5 | XML mixin 적용 | XML | 자동화 |

---

## 변경 이력

| 날짜 | 변경 내용 |
|------|----------|
| 2026-01-23 | 초안 작성 |
