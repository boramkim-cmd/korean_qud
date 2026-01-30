# Caves of Qud 한글화 작업 가이드

> **버전**: 2.0 (2026-01-27)
> **주의**: 기본 규칙은 `.claude/` 폴더 참조, 이 문서는 상세 기술 레퍼런스

---

## 빠른 참조

```bash
# 세션 시작
cat .claude/session-state.md

# 배포 (검증 포함)
./deploy.sh

# 게임 내 디버그
kr:reload   # JSON 리로드
kr:stats    # 번역 통계
kr:perf     # 성능 카운터
kr:check ID # 블루프린트 확인
```

---

## 번역 시스템 구조

### 레이어 구조

| 레이어 | 담당 | 핵심 파일 |
|--------|------|-----------|
| Layer 1: UI | 메뉴, 옵션, 스탯 | `TranslationEngine.cs` |
| Layer 2: 오브젝트 | 아이템, 생물 이름 | `ObjectTranslatorV2.cs` |
| Layer 3: 설명문 | 아이템/생물 설명 | `DescriptionPatch.cs` |

### 모드 로드 순서

```
게임 시작
  ↓
ModEntry.Main() [RuntimeInitializeOnLoadMethod]
  ↓
LocalizationManager.Initialize() → JSON 로드
  ↓
JsonRepository.Load() → 번들 or 소스 모드 선택
  ↓
Harmony.PatchAll() → 패치 적용
  ↓
게임 렌더링 시 번역 적용
```

### 번들 vs 소스 모드

```
번들 모드 (기본):
  dist/data/*.json (5개 번들, 607KB)
  → 빠른 로딩, 프로덕션 권장

소스 모드 (개발):
  LOCALIZATION/**/*.json (302개 파일)
  → 개별 수정 가능, 개발 시 사용
  → LOCALIZATION 폴더 삭제하면 자동으로 번들 모드
```

---

## ObjectTranslator V2 (Pipeline 아키텍처)

### 처리 순서

```
입력: "{{w|bronze}} mace"
  ↓
1. ColorTagProcessor: 색상 태그 처리
  ↓
2. OfPatternHandler: "X of Y" → "Y의 X"
  ↓
3. PrefixSuffixHandler: 접두사/접미사 분리
  ↓
4. DirectMatchHandler: JSON 직접 매칭
  ↓
5. DynamicPatternHandler: corpse, food, parts
  ↓
6. ColorTagContentHandler: 태그 내부 번역
  ↓
7. FallbackHandler: 복합어 조합 시도
  ↓
출력: "{{w|청동}} 메이스"
```

### 핵심 핸들러

| 핸들러 | 역할 | 예시 |
|--------|------|------|
| ColorTagProcessor | 색상 태그 보존 | `{{w|x}}` → 태그 분리 |
| OfPatternHandler | 어순 변환 | sword of fire → 불의 검 |
| CompoundTranslator | 복합어 분해 | bronze mace → 청동 + 메이스 |
| BookTitleTranslator | 책 제목 전용 | On Life → 생명에 대하여 |
| FoodTranslator | 동적 음식 | bear jerky → 곰 육포 |

---

## JSON 데이터 구조

### 위치

```
LOCALIZATION/
├── CHARGEN/           # 캐릭터 생성
├── GAMEPLAY/          # 게임플레이
│   ├── MUTATIONS/     # 81개 파일
│   ├── SKILLS/        # 20개 파일
│   └── messages.json
├── OBJECTS/           # 아이템/생물
│   ├── _vocabulary/   # 어휘 사전
│   │   ├── modifiers.json (600+ 수식어)
│   │   └── nouns.json
│   ├── creatures/
│   └── items/
└── UI/                # UI 텍스트
```

### 형식

```json
// 단순 (UI)
{"Sound": "소리", "Graphics": "그래픽"}

// 구조화 (오브젝트)
{
  "Torch": {
    "names": {"torch": "횃불", "Torch": "횃불"},
    "description_ko": "빛을 제공하는 횃불."
  }
}

// 어휘 (vocabulary)
{
  "modifiers": {"bronze": "청동", "iron": "철"},
  "nouns": {"mace": "메이스", "sword": "검"}
}
```

---

## Harmony 패치 패턴

### UI 스코프 관리

```csharp
[HarmonyPatch(typeof(ScreenClass), "Show")]
static void Prefix() {
    ScopeManager.PushScope(LocalizationManager.GetCategory("category"));
}

[HarmonyPatch(typeof(ScreenClass), "Hide")]
static void Postfix() {
    ScopeManager.PopScope();  // 반드시 Pop!
}
```

### UI-Only Postfix (데이터 보호)

```csharp
[HarmonyPostfix]
static void Updated_Postfix(SomeControl __instance) {
    // ❌ 금지: 데이터 필드 수정
    // __instance.data.Field = "Korean";
    
    // ✅ 허용: UI 요소만 수정
    __instance.textElement.text = "Korean";
}
```

---

## 디버깅

### 로그 확인

```bash
# 실시간
tail -f ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | grep -i "qud-kr"

# 에러만
grep -i "error\|exception" ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | tail -30
```

### 게임 내 명령어

| 명령어 | 설명 |
|--------|------|
| `kr:reload` | JSON 리로드 (재시작 없이) |
| `kr:stats` | 번역 통계 (Mode 확인) |
| `kr:perf` | 성능 카운터 + 리셋 |
| `kr:check <id>` | 블루프린트 번역 확인 |
| `kr:untranslated` | 현재 영역 미번역 목록 |

---

## 자주 발생하는 문제

| 증상 | 원인 | 해결 |
|------|------|------|
| 전체 번역 안됨 | TypeInitializationException | Dictionary 중복 키 확인 |
| 특정 아이템만 | JSON에 없음 | 해당 카테고리 JSON 추가 |
| UI 번역 안됨 | 스코프 누락 | Push/Pop 확인 |
| 색상 태그 깨짐 | 태그 중복 포함 | JSON에서 태그 제거 |
| 게임 크래시 | 위험 필드 번역 | danger-zones.md 확인 |

---

## 상세 참조

| 문서 | 내용 |
|------|------|
| `.claude/session-state.md` | 현재 작업 상태 |
| `.claude/CONTEXT.md` | 프로젝트 전체 컨텍스트 |
| `.claude/danger-zones.md` | 절대 금지 규칙 |
| `.claude/anti-patterns.md` | 반복 실수 방지 |
| `Docs/04_TODO.md` | 작업 목록 |
| `Docs/06_ERRORS.md` | 에러 기록 |
| `Docs/terminology_standard.md` | 용어 표준 |
