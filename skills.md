# Caves of Qud 한글화 작업 가이드

## 현재 상태 (2026-01-25)

- 기본 번역 시스템 동작 중
- ObjectTranslator (아이템명 번역) 정상 작동
- UI 패치 시스템 정상 작동

---

## 주요 명령어

### 배포
```bash
./deploy.sh
```

### 로그 확인
```bash
# 실시간 로그
tail -f "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | grep -i "qud-kr\|error\|exception"

# 최근 에러만
grep -i "error\|exception\|Qud-KR" "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | tail -30
```

### 검증 도구
```bash
python3 tools/project_tool.py
```

---

## 주의사항

### 1. C# Dictionary 중복 키 금지

```csharp
// 잘못된 예 - 런타임 에러 발생!
private static readonly Dictionary<string, string> _prefixes = new Dictionary<string, string>
{
    { "worn", "낡은" },      // 첫 번째
    // ... 100줄 후 ...
    { "worn", "낡은" },      // 중복! TypeInitializationException 발생
};
```

**증상**: `TypeInitializationException` 으로 클래스 전체 로드 실패
**해결**: 추가 전 `grep "키이름" 파일.cs`로 중복 확인

### 2. Static 필드 초기화 실패 = 전체 실패

- C#에서 `static readonly` 필드 초기화 실패 시 해당 클래스 전체 사용 불가
- 에러 메시지에 원인이 명확히 안 나올 수 있음
- `TypeInitializationException` 보이면 static 필드부터 점검

### 3. 번역 태그 보존 필수

```
{{tag}}  - 게임 내부 변수, 절대 번역 금지
%var%    - 동적 값, 절대 번역 금지
=text=   - 색상/서식 태그, 구조 유지
```

### 4. 접두사 추가 시 체크리스트

ObjectTranslator.cs의 `_descriptivePrefixes` 등에 새 항목 추가 시:

1. `grep -n "추가할키" ObjectTranslator.cs` 로 중복 확인
2. 알파벳 순서나 카테고리 맞춰서 추가
3. `./deploy.sh` 후 게임 실행 테스트
4. 로그에서 `TypeInitializationException` 없는지 확인

---

## 버그 이력

### 2026-01-25: Dictionary 중복 키 버그

**커밋**: `a3651bf`
**증상**: 번역이 전혀 안 됨
**원인**: `_descriptivePrefixes`에 중복 키 3개 (`worn`, `polished`, `weird`)
**해결**: 중복 항목 삭제

---

## 한글 적용 시스템 구조

### 1. 모드 로드 순서 (ModEntry.cs)

```
게임 시작
    ↓
[RuntimeInitializeOnLoadMethod] ModEntry.Main()
    ↓
LocalizationManager.Initialize()  ← JSON 번역 데이터 로드
    ↓
Harmony.PatchAll()  ← 모든 패치 클래스 적용
    ↓
게임 UI/오브젝트 렌더링 시 패치 작동
```

### 2. 번역 레이어 구조

| 레이어 | 담당 | 파일 |
|--------|------|------|
| **Layer 1: UI 텍스트** | 메뉴, 옵션, 스탯 화면 | `TranslationEngine.cs` + `ScopeManager.cs` |
| **Layer 2: 오브젝트명** | 아이템, 생물 이름 | `ObjectTranslator.cs` + `DisplayNamePatch.cs` |
| **Layer 3: 설명문** | 아이템/생물 설명 | `DescriptionPatch.cs` |

### 3. TranslationEngine (UI 번역)

```csharp
// 핵심 로직: TryTranslate()
1. 전처리: 앞뒤 공백 제거
2. 체크박스/핫키 접두사 추출: "[A] Options" → 접두사 "[A] ", 본문 "Options"
3. 색상 태그 제거: "{{c|text}}" → "text"
4. 대소문자 변형 시도: 원본 → UPPER → Title → lower
5. ScopeManager의 현재 스코프에서 번역 검색
6. 색상 태그 복원: 번역된 텍스트에 원래 색상 태그 적용
```

**ScopeManager 사용법**:
```csharp
ScopeManager.Push("options", "stats");  // 스코프 진입
// ... UI 렌더링 코드 (이 안에서 TryTranslate 호출됨)
ScopeManager.Pop();  // 반드시 Pop! (안 하면 스코프 꼬임)
```

### 4. ObjectTranslator (오브젝트명 번역)

**번역 우선순위**:
```
1. 캐시 확인 (빠른 경로)
2. JSON 정확히 일치 검색 (creatures/*.json, items/*.json)
3. 접두사 분리 후 검색: "wooden arrow" → "나무" + "arrow" → "나무 화살"
4. 접미사 분리 후 검색: "torch (unburnt)" → "횃불" + "(미사용)"
5. 동적 패턴: "{creature} corpse" → "{생물명} 시체"
6. 기본 명사 폴백: "mace" → "메이스"
```

**접두사 딕셔너리** (길이순 정렬 중요!):
```csharp
_materialPrefixes    // 재료: wooden, iron, steel, crysteel...
_qualityPrefixes     // 품질: flawless, masterwork, crude...
_processingPrefixes  // 가공: freeze-dried, raw, preserved...
_descriptivePrefixes // 설명: broken, rusted, luminous...
```

**동적 패턴 처리**:
```csharp
// TryTranslateDynamicFood()
"bear jerky" → "곰 육포"
"preserved snapjaw" → "절임 스냅조"

// TryTranslateDynamicParts()
"bear hide" → "곰 가죽"
"snapjaw skull" → "스냅조 두개골"

// TryTranslateCorpse()
"bear corpse" → "곰 시체"
```

### 5. Harmony 패치 방식

```csharp
// Postfix 패치: 원본 메서드 실행 후 결과 수정
[HarmonyPatch(typeof(GetDisplayNameEvent), "GetFor")]
static void GetFor_Postfix(ref string __result, GameObject Object)
{
    // __result = 원래 영어 이름
    if (ObjectTranslator.TryGetDisplayName(blueprint, __result, out string translated))
    {
        __result = translated;  // 한글로 교체
    }
}
```

**중요 파라미터**:
- `ForSort = true`: 정렬용 호출, 번역 스킵 (안 하면 정렬 깨짐)
- `ColorOnly = true`: 색상만 필요, 번역 스킵

### 6. 색상 태그 처리

**Qud 색상 태그 형식**:
```
{{c|cyan text}}     ← 단일 문자 = 색상
{{crysteel|text}}   ← 여러 문자 = 셰이더/재료
{{K|{{crysteel|crysteel}} mace}}  ← 중첩 가능
```

**처리 흐름**:
```
입력: "{{w|bronze}} mace"
    ↓
TranslateMaterialsInColorTags(): "{{w|청동}} mace"
    ↓
TranslateBaseNounsOutsideTags(): "{{w|청동}} 메이스"
    ↓
출력: "{{w|청동}} 메이스"
```

### 7. JSON 데이터 구조

**UI 번역** (`LOCALIZATION/UI/*.json`):
```json
{
  "options": {
    "Sound": "소리",
    "Graphics": "그래픽"
  }
}
```

**오브젝트 번역** (`LOCALIZATION/OBJECTS/items/*.json`):
```json
{
  "Torch": {
    "names": {
      "torch": "횃불",
      "Torch": "횃불"
    },
    "description": "A torch that provides light.",
    "description_ko": "빛을 제공하는 횃불."
  }
}
```

### 8. 디버그 명령어 (Wish)

게임 내 `Ctrl+W`로 Wish 입력:
```
kr:reload    - JSON 리로드 (게임 재시작 없이 번역 갱신)
kr:stats     - 로드된 번역 통계 출력
kr:test      - 특정 아이템 번역 테스트
```

---

## 파일 구조

```
qud_korean/
├── deploy.sh                              # 배포 스크립트
├── skills.md                              # 이 문서
│
├── Scripts/
│   ├── 00_Core/                           # 핵심 시스템
│   │   ├── 00_00_00_ModEntry.cs           # 모드 진입점, Harmony 패치 적용
│   │   ├── 00_00_01_TranslationEngine.cs  # UI 번역 핵심 로직
│   │   ├── 00_00_02_ScopeManager.cs       # 번역 스코프 관리
│   │   ├── 00_00_03_LocalizationManager.cs # JSON 로더
│   │   └── 00_00_04_TMPFallbackFontBundle.cs # 한글 폰트 로더
│   │
│   ├── 02_Patches/
│   │   ├── 00_Core/
│   │   │   └── 02_00_02_ScreenBuffer.cs   # 콘솔 렌더링 패치
│   │   ├── 10_UI/                         # UI 화면별 패치
│   │   │   ├── 02_10_00_GlobalUI.cs       # 전역 UI
│   │   │   ├── 02_10_01_Options.cs        # 옵션 화면
│   │   │   ├── 02_10_10_CharacterCreation.cs # 캐릭터 생성
│   │   │   └── ...
│   │   └── 20_Objects/                    # 오브젝트 번역
│   │       ├── 02_20_00_ObjectTranslator.cs   # 핵심: 접두사/패턴 사전
│   │       ├── 02_20_01_DisplayNamePatch.cs   # Harmony 패치
│   │       ├── 02_20_02_DescriptionPatch.cs   # 설명문 패치
│   │       └── 02_20_99_DebugWishes.cs        # 디버그 명령어
│   │
│   └── 99_Utils/                          # 유틸리티
│       ├── 99_00_01_TranslationUtils.cs
│       └── 99_00_02_ChargenTranslationUtils.cs
│
├── LOCALIZATION/                          # 번역 JSON
│   ├── UI/                                # UI 텍스트
│   │   ├── common.json
│   │   └── options.json
│   ├── CHARGEN/                           # 캐릭터 생성
│   │   ├── stats.json
│   │   └── modes.json
│   ├── GAMEPLAY/                          # 게임플레이
│   │   └── cybernetics.json
│   └── OBJECTS/                           # 오브젝트 (아이템/생물)
│       ├── creatures/
│       │   └── *.json
│       └── items/
│           └── *.json
│
└── Docs/                                  # 상세 문서
    ├── MASTER.md                          # 마스터 문서
    └── guides/                            # 작업 가이드
```

---

## 작업 플로우

1. 코드 수정
2. `./deploy.sh` 실행
3. 게임 재시작
4. 로그 확인 (에러 없는지)
5. 인게임 테스트
6. **커밋** (매 작업 단위마다)

---

## Git 커밋 규칙

### 자주 커밋하기
- **매 기능/버그 수정 완료 시** 즉시 커밋
- 대화 한 세션이 끝날 때 반드시 커밋
- "나중에 한꺼번에 커밋" 금지

### 커밋 타이밍
```
코드 변경 → 테스트 → 정상 작동 확인 → 즉시 커밋
```

### 커밋 메시지 형식
```bash
# 타입: 설명
git commit -m "fix: Dictionary 중복 키 버그 수정"
git commit -m "feat: 새 접두사 추가 (frozen, burnt)"
git commit -m "docs: skills.md 작업 가이드 추가"
git commit -m "chore: 불필요한 파일 정리"
```

### 왜 자주 커밋해야 하는가?
1. **롤백 용이**: 문제 발생 시 특정 시점으로 되돌리기 쉬움
2. **변경 추적**: 어떤 커밋에서 버그가 도입됐는지 파악 가능
3. **작업 손실 방지**: 예기치 못한 상황에서 작업물 보호
4. **협업**: 다른 사람이 변경 이력 이해하기 쉬움

---

## 자주 발생하는 문제

### 번역이 전혀 안 됨

1. **로그 확인**: `TypeInitializationException` 있는지
2. **원인 90%**: Dictionary 중복 키
3. **해결**: 중복 키 찾아서 삭제

### 특정 아이템만 번역 안 됨

1. **JSON 확인**: `LOCALIZATION/OBJECTS/items/`에 해당 아이템 있는지
2. **키 확인**: JSON의 키가 Blueprint ID와 일치하는지 (대소문자 주의)
3. **names 필드**: 다양한 변형 포함했는지 (`torch`, `Torch` 둘 다)

### UI 번역 안 됨

1. **스코프 확인**: 해당 화면에서 `ScopeManager.Push()` 호출하는지
2. **JSON 확인**: 올바른 카테고리에 번역 있는지
3. **대소문자**: 원본 텍스트와 JSON 키 일치하는지

### 색상 태그가 깨짐

1. **중첩 태그**: `{{K|{{crysteel|...}}}}` 형태 제대로 처리하는지
2. **StripColorTags**: 태그 제거 후 복원 과정 확인
3. **테스트**: `kr:test 아이템명`으로 번역 결과 확인

### 게임이 크래시됨

1. **Harmony 패치 문제**: 특정 패치 비활성화 후 테스트
2. **null 체크**: `__result`, `Object` 등 null 확인
3. **try-catch**: 패치 내부에 예외 처리 있는지

---

## 새 번역 추가 방법

### 새 접두사 추가

```csharp
// ObjectTranslator.cs의 적절한 딕셔너리에 추가
// 1. 중복 확인
grep -n "새접두사" 02_20_00_ObjectTranslator.cs

// 2. 적절한 카테고리에 추가
_materialPrefixes      // 재료 (bronze, steel...)
_qualityPrefixes       // 품질 (flawless, crude...)
_processingPrefixes    // 가공 (raw, dried...)
_descriptivePrefixes   // 기타 설명 (broken, luminous...)

// 3. 복합 접두사는 먼저! (길이순 정렬됨)
{ "folded carbide", "접힌 카바이드" },  // 먼저
{ "carbide", "카바이드" },              // 나중
```

### 새 오브젝트 추가

```json
// LOCALIZATION/OBJECTS/items/weapons.json
{
  "NewWeapon": {
    "names": {
      "new weapon": "새 무기",
      "New Weapon": "새 무기"
    },
    "description": "A new weapon.",
    "description_ko": "새로운 무기."
  }
}
```

### 새 UI 화면 패치

```csharp
// Scripts/02_Patches/10_UI/02_10_XX_NewScreen.cs
[HarmonyPatch(typeof(NewScreen), "Show")]
static void Prefix()
{
    ScopeManager.Push("newscreen", "common");
}

[HarmonyPatch(typeof(NewScreen), "Hide")]
static void Postfix()
{
    ScopeManager.Pop();
}
```
