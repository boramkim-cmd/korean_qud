# Caves of Qud 한글화 프로젝트 마스터 문서

## 프로젝트 개요
- 저장소: https://github.com/boramkim-cmd/korean_qud
- 작업 폴더: /Users/ben/Desktop/qud_korean
- 모드 위치: ~/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/qud_korean

---

## 진행 현황 (58%)

| Phase | 진행률 | 상태 |
|-------|--------|------|
| Phase 1: 안정화 | 100% | 완료 |
| Phase 2: 게임플레이 | 90% | 진행 중 |
| Phase 3: 최적화 | 17% | 대기 |
| Phase 4: 커뮤니티 | 0% | 대기 |

### 완료된 기능
- 폰트 시스템 (d2coding.bundle)
- JSON 기반 로컬라이제이션
- 캐릭터 생성 UI
- 옵션 화면
- 튜토리얼 팝업
- 메시지 로그 패치

### 테스트 필요 항목
- [ ] 툴팁 헤더: "현재 아이템" / "장착 아이템"
- [ ] 아이템 이름: "횃불", "물주머니 [비어있음]"
- [ ] 속성 화면: 힘, 민첩, 건강, 지능, 의지, 자아

---

## 모드 구조

```
qud_korean/
├── d2coding.bundle              # 한글 폰트
├── manifest.json                # 모드 메타데이터
├── deploy.sh                    # 배포 스크립트
│
├── Scripts/
│   ├── 00_Core/                 # 핵심 시스템 (7개)
│   │   ├── 00_00_00_ModEntry.cs           # 진입점
│   │   ├── 00_00_01_TranslationEngine.cs  # 번역 엔진
│   │   ├── 00_00_02_ScopeManager.cs       # 스코프 관리
│   │   ├── 00_00_03_LocalizationManager.cs # JSON 로더
│   │   └── 00_00_99_QudKREngine.cs        # 조사 처리
│   │
│   ├── 02_Patches/              # Harmony 패치 (17개)
│   │   ├── 00_Core/             # 코어 패치
│   │   ├── 10_UI/               # UI 패치
│   │   └── 20_Objects/          # 오브젝트 패치
│   │
│   └── 99_Utils/                # 유틸리티 (4개)
│
├── LOCALIZATION/                # JSON 번역 데이터
│   ├── CHARGEN/                 # 캐릭터 생성
│   ├── GAMEPLAY/                # 게임플레이
│   ├── OBJECTS/                 # 오브젝트 번역
│   └── UI/                      # UI 텍스트
│
└── docs/                        # 문서
```

---

## 핵심 파일 인덱스

### Core Layer
| 파일 | 역할 | 주요 메서드 |
|------|------|-------------|
| `00_00_01_TranslationEngine.cs` | 번역 엔진 | `TryTranslate()` |
| `00_00_02_ScopeManager.cs` | 스코프 관리 | `PushScope()`, `PopScope()` |
| `00_00_03_LocalizationManager.cs` | JSON 로더 | `GetTerm()`, `TryGetAnyTerm()` |
| `00_00_99_QudKREngine.cs` | 조사 처리 | `HasJongsung()`, `ResolveJosa()` |

### Patch Layer
| 파일 | 대상 | 역할 |
|------|------|------|
| `02_10_00_GlobalUI.cs` | TMP_Text | 전역 UI 번역 |
| `02_10_10_CharacterCreation.cs` | 캐릭터 생성 | 12개 모듈 패치 |
| `02_20_00_ObjectTranslator.cs` | 오브젝트 | 아이템/생물 번역 |
| `02_20_01_DisplayNamePatch.cs` | 이름 표시 | GetDisplayName 패치 |

---

## 번역 데이터 현황

| 카테고리 | 파일 수 | 항목 수 |
|----------|---------|---------|
| CHARGEN | 15+ | 200+ |
| GAMEPLAY | 20+ | 500+ |
| OBJECTS | 67 | 6,956+ |
| UI | 10+ | 700+ |
| **합계** | **112+** | **8,356+** |

---

## 명령어

### 개발
```bash
python3 tools/project_tool.py    # 검증
bash tools/sync-and-deploy.sh    # 배포
./deploy.sh                       # 빠른 배포
```

### 디버그 (게임 내 Wish)
```
kr:reload       # JSON 리로드
kr:check <id>   # 특정 블루프린트 확인
kr:untranslated # 미번역 목록
kr:stats        # 번역 통계
kr:clearcache   # 캐시 클리어
```

---

## 작업 흐름

```
1. 코드/JSON 수정
       ↓
2. python3 tools/project_tool.py (검증)
       ↓
3. ./deploy.sh (모드 폴더에 배포)
       ↓
4. 게임 실행 → 테스트
       ↓
5. git add . && git commit -m "설명" && git push
```

---

## 위험 필드 (번역 금지)

| 클래스 | 필드 | 이유 |
|--------|------|------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` 사용 → 크래시 |
| `ChoiceWithColorIcon` | `Id` | 선택 로직에 사용 |

---

## 용어 기준

| 영문 | 한글 | 비고 |
|------|------|------|
| Toughness | 건강 | NOT 지구력 |
| Strength | 힘 | |
| Agility | 민첩 | |
| Intelligence | 지능 | |
| Willpower | 의지 | |
| Ego | 자아 | |

---

## 문서 참조

| 문서 | 용도 |
|------|------|
| `reference/01_TODO.md` | 작업 목록 (권위 문서) |
| `reference/02_CHANGELOG.md` | 변경 이력 |
| `reference/03_ERROR_LOG.md` | 에러 기록 |
| `guides/01_PRINCIPLES.md` | 개발 원칙 |
| `guides/02_ARCHITECTURE.md` | 시스템 구조 |
