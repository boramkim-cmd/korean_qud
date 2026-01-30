# QUD_KOREAN 프로젝트 컨텍스트
> **버전**: 2.0 | **SSOT** (Single Source of Truth)
> 다른 문서(CLAUDE.md, skills.md, copilot-instructions.md)는 이 파일을 참조

---

## 세션 시작 체크리스트

```bash
# 1. 현재 상태 확인 (필수!)
cat .claude/session-state.md

# 2. 최근 커밋 확인
git log --oneline -5

# 3. 필요시 상세 문서
cat Docs/04_TODO.md | head -100
```

---

## 프로젝트 요약

| 항목 | 값 |
|------|-----|
| 프로젝트 | Caves of Qud 한글화 모드 |
| 기술 스택 | C# + Harmony 2.x + Unity |
| 번역 진행률 | 99% (1,617/1,634 복합어) |
| 테스트 커버리지 | 197개 케이스, 100% 통과 |

---

## 핵심 명령어

```bash
# 배포
./deploy.sh

# 검증
python3 tools/project_tool.py

# 로그 확인
tail -f ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | grep -i "qud-kr\|error"

# 게임 내 디버그 (Ctrl+W)
kr:reload    # JSON 리로드
kr:stats     # 번역 통계
kr:perf      # 성능 카운터
kr:check <id> # 블루프린트 확인
```

---

## 작업 흐름 (필수 준수)

```
코드/JSON 수정
    ↓
Dictionary 수정 시: grep -n "키" 파일.cs (중복 확인)
    ↓
./deploy.sh
    ↓
게임 실행 + 테스트 (생략 금지!)
    ↓
로그 확인 (에러 없는지)
    ↓
즉시 커밋 (나중에 하지 않기!)
    ↓
.claude/session-state.md 업데이트
```

---

## 파일 구조 (핵심만)

```
Scripts/
├── 00_Core/                    # 번역 엔진
│   ├── 00_00_00_ModEntry.cs    # 진입점
│   ├── 00_00_01_TranslationEngine.cs
│   └── 00_00_03_LocalizationManager.cs
├── 02_Patches/
│   ├── 10_UI/                  # UI 패치
│   └── 20_Objects/V2/          # ObjectTranslator V2 (Pipeline)
└── 99_Utils/

LOCALIZATION/                   # 번역 JSON
├── CHARGEN/                    # 캐릭터 생성
├── GAMEPLAY/                   # 게임플레이
├── OBJECTS/                    # 아이템/생물
│   └── _vocabulary/            # 어휘 사전
└── UI/                         # UI 텍스트

.claude/
├── session-state.md            # 현재 세션 (필수!)
├── danger-zones.md             # 절대 금지 규칙
└── anti-patterns.md            # 반복 실수 방지
```

---

## 번역 API 사용법

```csharp
// 1. 단순 번역 (현재 스코프)
if (TranslationEngine.TryTranslate(text, out var tr))
    element.text = tr;

// 2. 카테고리 지정
if (LocalizationManager.TryGetAnyTerm(key, out var val, "chargen", "ui"))
    element.text = val;

// 3. 구조화 데이터 (뮤테이션 등)
var data = StructureTranslator.GetTranslationData("Clairvoyance", "MUTATIONS");
```

---

## 참조 문서

| 문서 | 용도 |
|------|------|
| `.claude/session-state.md` | 현재 작업 상태 |
| `.claude/danger-zones.md` | 절대 금지 규칙 |
| `.claude/anti-patterns.md` | 반복 실수 방지 |
| `Docs/04_TODO.md` | 작업 목록 |
| `Docs/06_ERRORS.md` | 에러 기록 |
| `Docs/terminology_standard.md` | 용어 표준 |
