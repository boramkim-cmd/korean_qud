# 색상태그 접두사 번역 근본 수정 Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 인벤토리에서 `{{feathered|feathered}} leather armor`가 `깃털 달린 가죽 갑옷`으로 완전히 번역되도록 수정

**Architecture:** `PrefixSuffixHandler`가 색상태그를 먼저 스트립하여 정보가 손실되는 문제를 해결. `ColorTagProcessor.TranslateMaterials()`를 확장하여 `{{prefix|prefix}}` 패턴의 접두사도 번역하도록 함.

**Tech Stack:** C# / Harmony Patches / Regex

---

## 문제 근본 원인

### 현재 코드 흐름

```
입력: "{{feathered|feathered}} leather armor"
    ↓
[PrefixSuffixHandler Line 29]
ColorTagProcessor.Strip(originalName)
    → "feathered leather armor"  ← 색상태그 정보 손실!
    ↓
[PrefixSuffixHandler Line 33]
PrefixExtractor.TryExtract("feathered leather armor", ...)
    → prefixKo = "깃털 달린"  (색상태그 없음!)
    → remainder = "leather armor"
    ↓
[PrefixSuffixHandler Line 40]
translated = $"{prefixKo} {baseKo}{suffixKo}"
    → "깃털 달린 가죽 갑옷"  (색상태그 복원 안됨!)
```

### 이전 수정의 문제

`PrefixExtractor`에 `{{prefix|prefix}}` 체크를 추가했지만, `PrefixSuffixHandler`가 **스트립된 문자열을 전달**하기 때문에 그 코드가 실행되지 않음.

### 해결 전략

**선택한 접근법:** `ColorTagProcessor.TranslateMaterials()`가 `{{prefix|prefix}}` 패턴도 처리

이유:
1. `DirectMatchHandler`와 `FallbackHandler`는 이미 `TranslateMaterials()`를 호출함
2. 색상태그 내부 번역 로직이 이미 존재하므로 확장만 필요
3. `PrefixSuffixHandler` 구조를 크게 변경하지 않아도 됨

---

## Tasks

### Task 1: ColorTagProcessor에 자기참조 색상태그 번역 추가

**Files:**
- Modify: `/Users/ben/Desktop/qud_korean/Scripts/02_Patches/20_Objects/V2/Processing/ColorTagProcessor.cs`

**문제:**
`{{feathered|feathered}}`는 색상코드와 내용이 동일한 "자기참조" 패턴.
현재 regex는 이를 색상코드로 처리하여 내용만 추출 (`feathered`).

**Step 1:** `TranslateMaterials()` 시작 부분에 자기참조 패턴 처리 추가 (Line 55 근처)

```csharp
// Step 0: Handle self-referential color tags {{word|word}} where color code equals content
// These are typically mod adjectives like {{feathered|feathered}}
foreach (var prefix in repo.Prefixes)
{
    // Pattern: {{word|word}} where both parts are the same prefix
    string selfRefPattern = $@"\{{\{{{Regex.Escape(prefix.Key)}\|{Regex.Escape(prefix.Key)}\}}\}}";
    if (Regex.IsMatch(result, selfRefPattern, RegexOptions.IgnoreCase))
    {
        result = Regex.Replace(result, selfRefPattern,
            "{{" + prefix.Value + "|" + prefix.Value + "}}",
            RegexOptions.IgnoreCase);
    }
}
```

**Step 2:** 빌드 및 배포

```bash
cd /Users/ben/Desktop/qud_korean && ./deploy.sh
```

**Step 3:** 커밋

```bash
git add Scripts/02_Patches/20_Objects/V2/Processing/ColorTagProcessor.cs
git commit -m "fix: ColorTagProcessor에 자기참조 색상태그 번역 추가

{{feathered|feathered}} 같은 패턴을 {{깃털 달린|깃털 달린}}으로 번역

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

### Task 2: PrefixExtractor 색상태그 로직 제거 (불필요해짐)

**Files:**
- Modify: `/Users/ben/Desktop/qud_korean/Scripts/02_Patches/20_Objects/V2/Processing/PrefixExtractor.cs`

**이유:**
- Task 1의 수정으로 `TranslateMaterials()`가 `{{prefix|prefix}}` → `{{prefix_ko|prefix_ko}}`를 처리
- `PrefixSuffixHandler`에 입력되는 시점에 이미 번역된 상태
- `PrefixExtractor`의 색상태그 체크는 불필요해지고 코드 복잡도만 증가

**Step 1:** 이전에 추가한 색상태그 패턴 체크 제거 (Line 44-53)

**Before (현재):**
```csharp
foreach (var prefix in allPrefixes)
{
    // Check for color tag pattern: {{prefix|prefix}}
    string colorTagPattern = "{{" + prefix.Key + "|" + prefix.Key + "}}";
    if (current.StartsWith(colorTagPattern + " ", StringComparison.OrdinalIgnoreCase))
    {
        // Preserve color tag structure with translated content
        translatedPrefixes.Add("{{" + prefix.Value + "|" + prefix.Value + "}}");
        current = current.Substring(colorTagPattern.Length + 1);
        foundAny = true;
        break;
    }

    // Standard prefix check
    if (current.StartsWith(prefix.Key + " ", StringComparison.OrdinalIgnoreCase))
    ...
```

**After:**
```csharp
foreach (var prefix in allPrefixes)
{
    if (current.StartsWith(prefix.Key + " ", StringComparison.OrdinalIgnoreCase))
    {
        translatedPrefixes.Add(prefix.Value);
        current = current.Substring(prefix.Key.Length + 1);
        foundAny = true;
        break; // Restart search with longest prefixes first
    }
}
```

**Step 2:** 커밋

```bash
git add Scripts/02_Patches/20_Objects/V2/Processing/PrefixExtractor.cs
git commit -m "refactor: PrefixExtractor에서 불필요한 색상태그 로직 제거

ColorTagProcessor.TranslateMaterials()가 처리하므로 중복 로직 제거

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

### Task 3: PrefixSuffixHandler에서 색상태그 번역 먼저 적용

**Files:**
- Modify: `/Users/ben/Desktop/qud_korean/Scripts/02_Patches/20_Objects/V2/Pipeline/Handlers/PrefixSuffixHandler.cs`

**문제:**
현재 `PrefixSuffixHandler`는 `ColorTagProcessor.TranslateMaterials()`를 호출하지 않음.
Task 1에서 추가한 로직이 실행되지 않음.

**Step 1:** Handler 시작 부분에 재료/접두사 번역 추가 (Line 27 근처)

**Before:**
```csharp
public TranslationResult Handle(ITranslationContext context)
{
    var repo = context.Repository;
    string originalName = context.OriginalName;

    // Strip color tags for prefix matching
    string strippedForPrefix = ColorTagProcessor.Strip(originalName);
```

**After:**
```csharp
public TranslationResult Handle(ITranslationContext context)
{
    var repo = context.Repository;
    string originalName = context.OriginalName;

    // First translate materials/prefixes in color tags
    string withTranslatedTags = ColorTagProcessor.TranslateMaterials(originalName, repo);

    // Strip color tags for prefix matching
    string strippedForPrefix = ColorTagProcessor.Strip(withTranslatedTags);
```

**Step 2:** 결과 생성 시 번역된 색상태그 복원 (Line 40 근처)

**문제:** 현재 결과가 색상태그 없이 생성됨

**수정 방향:** 원본의 색상태그를 번역된 버전으로 대체

```csharp
// 기존 코드
string translated = $"{prefixKo} {baseKo}{suffixKo}";

// 수정: 색상태그가 있었다면 복원
if (withTranslatedTags != originalName && withTranslatedTags.Contains("{{"))
{
    // 색상태그가 번역되었으면 그 버전 사용
    translated = ColorTagProcessor.RestoreFormatting(
        withTranslatedTags,
        remainder,  // 영어 base name
        baseKo,     // 한국어 base name
        allSuffixes,
        suffixKo
    );
}
else
{
    translated = $"{prefixKo} {baseKo}{suffixKo}";
}
```

**Step 3:** 빌드 및 배포

```bash
cd /Users/ben/Desktop/qud_korean && ./deploy.sh
```

**Step 4:** 커밋

```bash
git add Scripts/02_Patches/20_Objects/V2/Pipeline/Handlers/PrefixSuffixHandler.cs
git commit -m "fix: PrefixSuffixHandler에서 색상태그 접두사 복원

- TranslateMaterials() 먼저 호출하여 {{prefix|prefix}} 번역
- 결과 생성 시 색상태그 구조 유지

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
```

---

### Task 4: 게임 테스트 및 검증

**Step 1:** 게임 실행 및 테스트 항목

| 테스트 | 입력 | 예상 결과 |
|--------|------|-----------|
| 인벤토리 | `{{feathered|feathered}} leather armor` | `{{깃털 달린|깃털 달린}} 가죽 갑옷` |
| 툴팁 | 동일 | 동일 |
| glitter grenade | `glitter grenade` | `글리터 수류탄` |
| advanced toolbox | `advanced toolbox` | `고급 공구함` |

**Step 2:** 로그 확인

```bash
grep -i "error\|exception" "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | tail -20
```

**Step 3:** 문제 발견 시 롤백 또는 추가 수정

---

### Task 5: 최종 정리 및 문서 업데이트

**Files:**
- Update: `/Users/ben/Desktop/qud_korean/Docs/00_CONTEXT.md`

**Step 1:** 변경사항 문서화

```markdown
## 최근 변경 (2026-01-26)

### 색상태그 접두사 번역 수정
- **문제:** 인벤토리에서 `feathered 가죽 갑옷` 표시 (접두사 미번역)
- **원인:** `PrefixSuffixHandler`가 색상태그를 먼저 스트립하여 정보 손실
- **해결:**
  - `ColorTagProcessor.TranslateMaterials()`에 자기참조 패턴 처리 추가
  - `PrefixSuffixHandler`에서 스트립 전 색상태그 번역 적용
```

---

## 검증 체크리스트

- [ ] `./deploy.sh` 빌드 성공
- [ ] 게임 로딩 시 에러 없음
- [ ] 인벤토리: `{{feathered|feathered}} leather armor` → 색상태그 유지하며 한글화
- [ ] 툴팁: 동일 항목 동일하게 번역
- [ ] 기존 번역 (단순 접두사) 여전히 작동
- [ ] 컬러 표시 정상 (색상태그 구조 유지)

---

## 아키텍처 참고

```
번역 파이프라인 (수정 후):

DirectMatchHandler          ← TranslateMaterials() 이미 호출 ✓
    ↓
PrefixSuffixHandler         ← TranslateMaterials() 추가 (Task 3)
    ↓
    → TranslateMaterials("{{feathered|feathered}} leather armor")
    → "{{깃털 달린|깃털 달린}} leather armor"
    → Strip() → "깃털 달린 leather armor"
    → PrefixExtractor.TryExtract() → prefix="깃털 달린", remainder="leather armor"
    → baseKo="가죽 갑옷"
    → RestoreFormatting() → "{{깃털 달린|깃털 달린}} 가죽 갑옷"
    ↓
PatternHandler
    ↓
FallbackHandler             ← TranslateMaterials() 이미 호출 ✓
```

## 범위 외

1. 카테고리 UI 번역 (`Grenades`, `Miscellaneous`) - 별도 UI 패치 필요
2. 색상태그 중첩 케이스 (`{{K|{{crysteel|crysteel}} mace}}`) - 기존 로직으로 처리됨
