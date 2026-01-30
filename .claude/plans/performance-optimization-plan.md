# 한글화 최적화 — 성능 + 번역 품질 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 파이프라인 fallback 성능 최적화, TMP setter 최적화, 번역 품질 개선으로 버퍼 없는 쾌적한 한글화 경험 달성

**이전 세션 완료 사항:** 카테고리 헤더 번역, display_lookup 빈값 덮어쓰기 버그 수정, 95개 아이템 번역 추가, 코드 리뷰 피드백 반영 (커밋 358908c, 576d95a)

---

## 병목 분석 요약

### 현재 번역 경로 (TryGetDisplayName)
```
1. DisplayLookup (O(1)) → 96.9% 히트 ✅
2. FastCache (O(1))     → blueprint별 프리빌드 ✅
3. GlobalNameIndex (O(1)) → 미지 블루프린트 ✅
4. Pipeline (5단계)      → 여기가 병목 ⚠️
```

### Pipeline 내부 병목 (3.1%가 여기 진입)
```
DirectMatchHandler → ColorTagProcessor.TranslateMaterials() 호출
  └─ foreach prefix (766개) × Regex.Replace = O(766) regex
  └─ foreach colorTagVocab (924개) × Regex.Replace × 5 iterations = O(4620) regex
  └─ foreach baseNouns (612개) × Regex.Replace = O(612) regex

FallbackHandler.TranslateWithPrefixesAndNouns()
  └─ BaseNouns (612) + Prefixes (766) = 1,378번 Regex.Replace

SuffixExtractor.ExtractAll()
  └─ 6개 비컴파일 Regex.Match 순차 실행
```

**총 worst-case: 한 아이템당 ~7,000번 regex 실행**

---

## Task 1: SuffixExtractor — Regex를 static compiled로 변환

**Files:**
- Modify: `Scripts/02_Patches/20_Objects/V2/Processing/SuffixExtractor.cs`

**변경:**
- `ExtractAll()` 내 6개 `Regex.Match(result, pattern)` → `private static readonly Regex` 필드로 변환
- `StripState()` 내 5개 `Regex.Replace(result, pattern, ...)` → 동일하게 static compiled
- `TranslateAll()`, `TranslateState()` 내 패턴도 동일 처리

```csharp
// Before (매 호출마다 regex 컴파일):
var parenMatch = Regex.Match(result, @"(\s*\([^)]+\))$");

// After (한 번만 컴파일):
private static readonly Regex RxParen = new Regex(@"(\s*\([^)]+\))$", RegexOptions.Compiled);
// ...
var parenMatch = RxParen.Match(result);
```

**예상 효과:** ExtractAll 호출 시 regex 컴파일 비용 제거 (6→0 컴파일)

---

## Task 2: PrefixExtractor — string concat 제거 + compiled regex

**Files:**
- Modify: `Scripts/02_Patches/20_Objects/V2/Processing/PrefixExtractor.cs`

**변경 A:** `TryExtract()` 내 `prefix.Key + " "` string concat 제거
```csharp
// Before (매 iteration 할당):
if (current.StartsWith(prefix.Key + " ", StringComparison.OrdinalIgnoreCase))

// After (할당 없음):
if (current.Length > prefix.Key.Length &&
    current[prefix.Key.Length] == ' ' &&
    current.StartsWith(prefix.Key, StringComparison.OrdinalIgnoreCase))
```

**변경 B:** `TranslateInText()` 내 766번 Regex.Replace → Dictionary 기반 단일 패스
- 모든 prefix를 `|` 로 연결한 하나의 compiled regex 빌드 (초기화 시 1회)
- 또는 단순 word-by-word 치환 (공백 split → dictionary lookup → join)

---

## Task 3: ColorTagProcessor.TranslateMaterials() — O(N×M) regex 제거

**Files:**
- Modify: `Scripts/02_Patches/20_Objects/V2/Processing/ColorTagProcessor.cs`

**현재 문제 (라인 113-222):**
```
Step 0: foreach prefix (766) → Regex.IsMatch + Regex.Replace (self-ref)
Step 0.5: foreach prefix (766) → Regex.Replace (extended)
Step 1: foreach prefix (766) → Regex.Replace (non-self-ref)
Inner loop × 5 iterations:
  Step 1: foreach colorTagVocab (924) × 2 patterns → Regex.Replace
  Step 2: foreach baseNouns (612) × 3 patterns → Regex.Replace
Step 3: foreach colorTagVocab (924) × 2 patterns → Regex.Replace
```
**Total: ~15,000+ regex 실행 per call**

**해결 전략:**
태그 내용을 직접 파싱하여 Dictionary lookup으로 대체:
1. `{{tag|content}}` 패턴을 수동 파싱 (indexOf 기반)
2. content를 공백으로 split
3. 각 word를 Prefixes/ColorTagVocab/BaseNouns Dictionary에서 O(1) lookup
4. 결과 조립

```csharp
// 핵심 아이디어: regex 루프 대신 Dictionary lookup
public static string TranslateMaterialsFast(string text, ITranslationRepository repo)
{
    // 1. {{tag|content}} 블록을 찾아서
    // 2. content 내 각 단어를 dictionary lookup
    // 3. 태그 밖 단어도 동일 처리
}
```

**예상 효과:** ~15,000 regex → ~50 dictionary lookups (300x 개선)

---

## Task 4: FallbackHandler — regex 루프를 dictionary 치환으로 변경

**Files:**
- Modify: `Scripts/02_Patches/20_Objects/V2/Pipeline/Handlers/FallbackHandler.cs`

**현재 문제 (라인 108-131):**
```csharp
// 612번 regex
foreach (var noun in repo.BaseNouns)
    result = Regex.Replace(result, pattern, ...);
// 766번 regex
foreach (var prefix in repo.Prefixes)
    result = Regex.Replace(result, pattern, ...);
```

**해결:** Task 3과 동일 전략 — word-by-word dictionary lookup
```csharp
private string TranslateWithPrefixesAndNouns(string text, ITranslationRepository repo)
{
    // 공백/구두점 기준 split → 각 token을 BaseNouns/Prefixes dict에서 lookup
    // O(N) where N = token count (보통 2-5개)
}
```

**예상 효과:** 1,378 regex → ~5 dictionary lookups

---

## Task 5: ColorTagProcessor.Strip() — compiled regex + 안전 한도 증가

**Files:**
- Modify: `Scripts/02_Patches/20_Objects/V2/Processing/ColorTagProcessor.cs`

**변경:**
1. `Regex.Replace(result, ...)` → `static readonly Regex` compiled 버전
2. iteration limit 10 → 20 (실용적 안전 한도)
3. `&[\^]?[a-zA-Z]` 패턴도 compiled로

---

## Task 6: TranslateNounsOutsideTags/TranslateNounsInText — dictionary 치환

**Files:**
- Modify: `Scripts/02_Patches/20_Objects/V2/Processing/ColorTagProcessor.cs` (라인 295-333)

**현재 문제:**
```csharp
// TranslateNounsInText: 612번 regex
foreach (var noun in repo.BaseNouns)
    result = Regex.Replace(result, pattern, ...);

// TranslatePrefixesInText: 766번 regex
foreach (var prefix in repo.Prefixes)
    result = Regex.Replace(result, pattern, ...);
```

**해결:** word boundary를 수동 파싱하여 dictionary lookup
- 텍스트를 word boundary로 split
- 각 word를 BaseNouns dict, Prefixes dict에서 O(1) lookup
- 결과 조립

---

## Task 7: Repository에 Dictionary 버전 추가

**Files:**
- Modify: `Scripts/02_Patches/20_Objects/V2/Data/ITranslationRepository.cs`
- Modify: `Scripts/02_Patches/20_Objects/V2/Data/JsonRepository.cs`

**변경:**
현재 Prefixes, ColorTagVocab, BaseNouns가 `IReadOnlyList<KeyValuePair<string, string>>` (sorted list for greedy matching). Task 3-6에서 dictionary lookup을 쓰려면 `IReadOnlyDictionary` 버전도 필요.

```csharp
// 인터페이스에 추가:
IReadOnlyDictionary<string, string> PrefixesDict { get; }
IReadOnlyDictionary<string, string> ColorTagVocabDict { get; }
IReadOnlyDictionary<string, string> BaseNounsDict { get; }
```

BuildSortedCaches()에서 sorted list와 함께 dictionary도 보존.

---

## Task 8: Pipeline pre-warming (첫 프레임 스파이크 제거)

**Files:**
- Modify: `Scripts/02_Patches/20_Objects/V2/ObjectTranslatorV2.cs`

**변경:**
게임 로드 후 자주 등장하는 블루프린트를 미리 번역 캐시에 넣기:
```csharp
private static void PreWarmCommonItems()
{
    // FastCache에 있는 모든 blueprint의 첫 번째 name을
    // TryGetDisplayName으로 한 번씩 호출하여 TranslationContext._globalCache에 채움
    foreach (var bp in _fastCache)
    {
        foreach (var name in bp.Value)
        {
            string dummy;
            TryGetDisplayName(bp.Key, name.Key, out dummy);
            break; // 첫 번째 이름만
        }
    }
}
```

Initialize() 끝에서 호출. 1838개 blueprint × O(1) lookup = 빠름.

---

## 실행 순서

```
Task 7 (Dictionary 인터페이스) → Task 1 (SuffixExtractor) → Task 2 (PrefixExtractor)
→ Task 5 (Strip compiled) → Task 3 (TranslateMaterials) → Task 6 (NounsOutsideTags)
→ Task 4 (FallbackHandler) → Task 8 (Pre-warming)
```

## 검증

1. `python3 -m pytest tools/ -q` — 기존 테스트 통과
2. `python3 tools/build_optimized.py` — 빌드 성공
3. 게임 테스트: `kr:stats`로 LookupHit/FastHit/Pipeline 비율 확인
4. 상점 열기 체감 속도 확인
5. 번역 품질 확인 (기존 번역이 깨지지 않았는지)
