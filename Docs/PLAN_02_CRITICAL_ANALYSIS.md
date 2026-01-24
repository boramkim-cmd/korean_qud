# 원본 계획에 대한 비판적 분석

> **문서 상태**: 비판적 검토 완료
> **작성일**: 2026-01-23
> **관련 문서**: [PLAN_01_ORIGINAL.md](./PLAN_01_ORIGINAL.md)

---

## 요약

원본 계획은 이론적으로 그럴듯해 보이지만, **실제 코드베이스와 맞지 않고**, **성능 문제를 야기하며**, **해결하려는 문제가 명확하지 않습니다.**

---

## 1. 근본적 문제: "해결할 문제"가 정의되지 않음

### 계획이 주장하는 문제들
- "빈 값이 표시됨"
- "null 반환"
- "Harmony 203개가 위험"
- "스코프 불일치"

### 실제로 확인된 것
- **구체적인 버그 사례가 하나도 없음**
- 어떤 화면에서 어떤 텍스트가 빈 값인지 명시 안 됨
- "203개 Harmony"가 실제로 문제를 일으킨 사례 없음
- 현재 모드가 실제로 작동하는지 여부도 불명확

### 결론
**문제를 정의하지 않고 해결책을 설계함 → 전형적인 과잉 엔지니어링**

---

## 2. 숫자 오해: "Harmony 203개" 분석

### grep 결과
```bash
grep -r "HarmonyPatch\|HarmonyPrefix\|HarmonyPostfix" | wc -l
# 결과: 203
```

### 실제 의미
- 203은 **어노테이션 수**이지 패치 클래스 수가 아님
- 하나의 패치 클래스에 `[HarmonyPatch]` + `[HarmonyPrefix]` + `[HarmonyPostfix]` = 3줄
- **실제 패치 수는 ~40-50개 수준**

### 파일 수 (실제)
```
Scripts/*.cs 파일: 28개
- 00_Core/: 7개 (패치 아님, 유틸리티)
- 02_Patches/: 17개 (실제 패치)
- 99_Utils/: 4개 (패치 아님, 유틸리티)
```

### 결론
**"203개 Harmony 패치" → 실제로는 ~17개 패치 파일**
계획의 전제 자체가 과장됨

---

## 3. 성능 재앙: TranslationResult struct

### 계획의 제안
```csharp
public struct TranslationResult
{
    public string Output;           // string 할당
    public string OriginalInput;    // string 할당
    public string StrippedInput;    // string 할당
    public string MatchedKey;       // string 할당
    public string CurrentScope;     // string 할당
    public string FoundInScope;     // string 할당
    public string CallerPatch;      // string 할당
    public ResultCode Code;
}
```

### 문제
1. **TMP_Text.Setter는 매 프레임 호출됨**
   - 60fps = 초당 60회 × UI 요소 수백 개
   - 매 호출마다 7개 string 할당 → **GC 지옥**

2. **디버그 Off일 때도 오버헤드 발생**
   - `TryTranslateWithResult()` 시그니처 자체가 struct 생성 강제
   - 기존 `TryTranslate(text, out result)` 대비 성능 저하

3. **Queue<TranslationResult> 메모리 누수**
   ```csharp
   public static Queue<TranslationResult> RecentResults = new(100);
   ```
   - `new Queue<T>(100)`은 **초기 용량**, 최대 용량 아님
   - Enqueue만 있고 제한 로직 없음 → 무한 증가

### 해결책
디버그 정보는 **반환값이 아닌 별도 로깅**으로 처리해야 함

---

## 4. API 문법 오류

### 계획의 코드
```csharp
[WishCommand("kr:debug")]
public static void ToggleDebug(string arg)
```

### 실제 기존 코드 (02_20_99_DebugWishes.cs:35)
```csharp
[WishCommand(Command = "kr:reload")]
public static void ReloadTranslations()
```

### 문제
- `WishCommand`는 `Command = "..."` 형식 사용
- 인자 받는 방식도 다름
- **계획대로 구현하면 컴파일 에러**

---

## 5. 호환성 파괴: ScopeManager 변경

### 현재 인터페이스
```csharp
public static void PushScope(params Dictionary<string, string>[] scopes)
```

### 계획의 새 인터페이스
```csharp
public static void PushScope(string name, params Dictionary<string, string>[] scopes)
```

### 영향
- **모든 PushScope 호출 위치 수정 필요**
- GlobalUI.cs:72: `ScopeManager.PushScope(pushList.ToArray());`
- 다른 파일들도 동일하게 영향

### 결론
"최소 수정"이라 했지만 **전체 수정 필요**

---

## 6. 데이터 오염 위험: 디버그 텍스트 반환

### 계획의 제안
```csharp
return $"{{g|✓}} {r.Output} {{{{y|← {r.OriginalInput}}}}}";
```

### 위험
1. **저장 파일 오염**
   - 플레이어 이름, 아이템 이름 등에 디버그 텍스트 저장
   - 디버그 끄면 저장 파일 깨짐

2. **다른 시스템 전파**
   - 번역된 텍스트가 다른 로직에서 사용됨
   - 디버그 태그가 비교/검색 로직 방해

3. **UI 레이아웃 붕괴**
   - 버튼에 긴 디버그 텍스트 → 레이아웃 깨짐
   - Minimal 모드 아이콘도 UI에 영향

### 올바른 접근
디버그 정보는 **반환값에 절대 포함하면 안 됨**
- 별도 로그 파일 출력
- 오버레이 UI (반환값과 분리)

---

## 7. 구현 불가능: ResultCode.SCOPE

### 계획의 설명
```
# 스코프 불일치
디버그: "{{o|[SCOPE]}} Ancient Crypt {{y|scope:chargen→gameplay}}"
```

### 감지하려면
1. 현재 스코프에서 번역 실패
2. **모든 다른 스코프** 검색
3. 발견되면 SCOPE 코드 반환

### 문제
- LocalizationManager에 262개 JSON 파일, 수천 개 항목
- 매 번역마다 전체 검색 → **성능 재앙**
- 현재 아키텍처로는 "다른 스코프에 있는지" 효율적으로 확인 불가능

---

## 8. Parts/Events 전환 - 검증 안 된 추측

### 계획의 주장
> "GetDisplayNameEvent를 Part로 처리하면 Harmony 감소"
> "203개 → ~80개로 감소"

### 문제점

1. **GetDisplayNameEvent.GetFor()는 static method**
   - Part 없이도 호출 가능
   - Part 추가가 필수가 아님

2. **모든 오브젝트에 Part 추가 시 성능**
   - Caves of Qud에는 수천 개의 오브젝트
   - 모든 오브젝트에 Part 추가 → 메모리/성능 영향 미지수

3. **실제 테스트 없음**
   - "예상 효과"만 있고 검증 없음
   - PoC(Proof of Concept) 없이 전면 전환 제안

### 결론
**장기 과제로 분리하고, 먼저 PoC로 검증해야 함**

---

## 9. 현재 아키텍처 분석 (계획이 무시한 것)

### 이미 잘 설계된 부분

**TranslationEngine (00_00_01)**
- 태그 처리, 대소문자 변형, 접두사 추출 모두 구현
- 단일 진입점으로 이미 작동 중

**ScopeManager (00_00_02)**
- Stack 기반 스코프 관리 구현 완료
- GetDepth(), ClearAll() 등 디버그 메서드 존재

**LocalizationManager (00_00_03)**
- 262개 JSON 파일 자동 로드
- 카테고리별 검색, 정규화 키 지원

**ObjectTranslator (02_20_00)**
- 독립 캐시, 동적 음식/시체 패턴 처리
- 상태 접미사 번역 (`[empty]` → `[비어있음]`)

### 결론
**기존 시스템이 이미 잘 구조화되어 있음**
계획은 이를 무시하고 새 시스템 제안

---

## 10. 권장 대안

### 디버그가 정말 필요하다면

```csharp
// 기존 코드에 조건부 로깅 추가 (반환값 변경 없음)
if (TranslationEngine.TryTranslate(text, out var result))
{
    value = result;
    #if DEBUG_TRANSLATION
    Debug.Log($"[OK] {text} → {result}");
    #endif
}
else
{
    #if DEBUG_TRANSLATION
    Debug.Log($"[MISS] {text}");
    #endif
}
```

**반환값은 절대 건드리지 않음**

### 실제로 필요한 것

1. **실제 문제 수집**
   - 게임 실행 → 번역 안 되는 텍스트 목록화
   - `kr:untranslated` 출력 분석

2. **기존 디버그 활용**
   - `Debug.Log` 이미 곳곳에 있음
   - Unity 콘솔에서 "[Qud-KR]" 필터링

3. **JSON 데이터 보완**
   - 번역 누락이면 JSON에 추가
   - 코드 변경 최소화

---

## 최종 권고 요약

### 하지 말 것

| 항목 | 이유 |
|------|------|
| TranslationResult struct 도입 | 성능 재앙, GC 압박 |
| ScopeManager 인터페이스 변경 | 호환성 파괴 |
| 디버그 텍스트 반환값에 포함 | 데이터 오염 |
| Parts/Events 전면 전환 | 검증 없이 위험 |

### 해야 할 것

| 순서 | 작업 | 방식 |
|------|------|------|
| 1 | 실제 버그 목록 작성 | 게임 플레이, 스크린샷 |
| 2 | JSON 데이터 보완 | 누락 번역 추가 |
| 3 | 기존 Debug.Log 활용 | Unity 콘솔 필터링 |
| 4 | Parts/Events PoC | 하나만 테스트 |

---

## 문서 활용법

### 다음 세션에서

1. 이 문서를 읽고 컨텍스트 복원
2. 실제 버그 수집 결과 추가
3. 수정된 계획 작성 (PLAN_03_REVISED.md)

### 업데이트 시

- 새로운 발견 사항은 섹션 추가
- 해결된 문제는 ~~취소선~~ 처리
- 변경 이력에 날짜와 내용 기록

---

## 변경 이력

| 날짜 | 변경 내용 |
|------|----------|
| 2026-01-23 | 초안 작성 - 원본 계획 비판적 분석 완료 |

---

## 코드베이스 현황 요약

```
/Users/ben/Desktop/qud_korean/
├── Scripts/
│   ├── 00_Core/           (7개 파일 - 핵심 시스템)
│   │   ├── 00_00_00_ModEntry.cs
│   │   ├── 00_00_01_TranslationEngine.cs    ★ 번역 핵심
│   │   ├── 00_00_02_ScopeManager.cs         ★ 스코프 관리
│   │   ├── 00_00_03_LocalizationManager.cs  ★ JSON 로딩
│   │   ├── 00_00_05_GlossaryExtensions.cs
│   │   ├── 00_00_06_G.cs
│   │   └── 00_00_99_QudKREngine.cs
│   ├── 02_Patches/        (17개 파일 - Harmony 패치)
│   │   ├── 00_Core/
│   │   ├── 10_UI/         ★ UI 번역 패치
│   │   └── 20_Objects/    ★ 오브젝트 번역 패치
│   └── 99_Utils/          (4개 파일 - 유틸리티)
├── LOCALIZATION/          (262개 JSON 파일)
│   ├── CHARGEN/
│   ├── GAMEPLAY/
│   ├── OBJECTS/
│   └── UI/
└── Docs/                  (문서)
    ├── PLAN_01_ORIGINAL.md
    └── PLAN_02_CRITICAL_ANALYSIS.md  ← 현재 문서
```
