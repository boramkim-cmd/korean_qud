# QUD_KOREAN 프로젝트 컨텍스트

> **이 파일은 Claude Code가 세션 시작 시 반드시 읽어야 하는 핵심 문서입니다.**
> 최종 업데이트: 2026-01-27 00:30

---

## 프로젝트 개요

| 항목 | 값 |
|------|-----|
| 프로젝트 | Caves of Qud 한글화 모드 |
| 저장소 | https://github.com/boramkim-cmd/korean_qud |
| 작업 폴더 | `/Users/ben/Desktop/qud_korean` |
| 모드 위치 | `~/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/qud_korean` |
| 진행률 | 번역 대상 2,948개 중 ~1,927개 번역 가능 (65.4%) |

---

## 현재 상태

### 동작 중인 기능
- 폰트 시스템 (d2coding.bundle)
- JSON 기반 로컬라이제이션
- 캐릭터 생성 UI
- 옵션 화면
- 튜토리얼 팝업
- **ObjectTranslator V2** (Pipeline 아키텍처, 26개 모듈)
- 메시지 로그 패치
- **ObjectTranslator 테스트 스크립트** (111개 테스트 케이스, 100% 통과)
- **비자기참조 색상태그 번역** (`{{glittering|glitter}}` → `{{glittering|글리터}}`)

### 번역 현황 (2026-01-27)
| 항목 | 개수 |
|------|------|
| XML DisplayName (총) | 3,006 |
| 템플릿 변수 (제외) | 58 |
| **실제 번역 대상** | **2,948** |
| JSON 직접 번역 | 927 |
| 패턴 번역 가능 | ~1,000 |
| **총 번역 가능** | **~1,927 (65.4%)** |
| 용어 불일치 | **0개** (모두 해결됨) |

> 템플릿 변수 (`=creatureRegionNoun=` 등)는 게임이 런타임에 동적으로 채우므로 번역 대상에서 제외

### 어휘 현황 (2026-01-27)
| 항목 | 개수 |
|------|------|
| 총 어휘 | 1,411 |
| Materials | 67 |
| Modifiers | 280+ |
| Base Nouns | 340+ |
| Species | 260+ |
| Body Parts | 102 |

### 최근 작업 (2026-01-27)
- ✅ **번역 커버리지 확장 작업** (41% → 62.6%)
- ✅ 패턴 어휘 대규모 확장 (nouns, modifiers, materials)
- ✅ 생물 어휘 확장 (golems, cherubs, NPCs)
- ✅ FoodTranslator 개선 (congealed, concentrated 패턴)
- ✅ 고유명사 추가 (Agolgot, Bethsaida, Qon 등)
- ✅ **템플릿 변수 문서화** (`Docs/07_TEMPLATE_VARIABLES.md`)
  - 34개 변수 유형, 8개 카테고리 분류
  - creature_region만 번역 가능 (향후 패치 필요)

### 이전 작업 (2026-01-26)
- ✅ XML vs JSON 번역 비교 스크립트 작성 (`tools/compare_translations.py`)
- ✅ 미번역 항목 리포트 생성 (`Docs/Issues/untranslated_report.md`)
- ✅ 용어 표준화 적용 (97+항목 수정)
- ✅ 용어집 문서 작성 (`Docs/terminology_standard.md`)
- ✅ 불일치 수정: willpower→의지, floating nearby→부유 아이템, warden→경비관

### 최근 이슈 (해결됨)
| 이슈 | 상태 | 해결 |
|------|------|------|
| 용어 불일치 88개 | ✅ CLEAR | 용어 표준화 스크립트 적용 |
| willpower 번역 불일치 | ✅ CLEAR | 의지력 → 의지 통일 |
| warden 번역 불일치 | ✅ CLEAR | 경비원 → 경비관 통일 |

---

## 핵심 규칙 (반드시 준수)

### 1. 코드 변경 시
```bash
# 1. 수정
# 2. 배포 + 테스트
./deploy.sh
# 게임 실행 → 로그 확인
grep -i "error\|exception" "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | tail -20

# 3. 즉시 커밋 (나중에 하지 않기!)
git add <files> && git commit -m "type: 설명"
```

### 2. Dictionary 수정 시
```bash
# 중복 키 확인 필수!
grep -n "키이름" ObjectTranslator.cs
```

### 3. 번역 태그 보존
```
{{tag}}  - 게임 변수, 번역 금지
%var%    - 동적 값, 번역 금지
```

### 4. 위험 필드 (절대 번역 금지)
| 클래스 | 필드 | 이유 |
|--------|------|------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` 사용 |
| `ChoiceWithColorIcon` | `Id` | 선택 로직에 사용 |

---

## 주요 명령어

```bash
# 배포
./deploy.sh

# 로그 확인
tail -f "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | grep -i "qud-kr"

# 검증
python3 tools/project_tool.py

# 번역 비교 (XML vs JSON)
python3 tools/compare_translations.py

# ObjectTranslator 번역 테스트 (100개 케이스)
python3 tools/test_object_translator.py

# 게임 내 디버그 (Ctrl+W → Wish)
kr:reload       # JSON 리로드
kr:stats        # 번역 통계
kr:check <id>   # 특정 블루프린트 확인
```

---

## 핵심 파일 위치

| 용도 | 파일 |
|------|------|
| 모드 진입점 | `Scripts/00_Core/00_00_00_ModEntry.cs` |
| 번역 엔진 | `Scripts/00_Core/00_00_01_TranslationEngine.cs` |
| 오브젝트 번역 (V2) | `Scripts/02_Patches/20_Objects/V2/ObjectTranslatorV2.cs` |
| 공통 어휘 | `LOCALIZATION/OBJECTS/items/_common.json` |
| 용어 표준 | `Docs/terminology_standard.md` |
| 번역 비교 스크립트 | `tools/compare_translations.py` |
| 미번역 리포트 | `Docs/Issues/untranslated_report.md` |

---

## 문서 구조

```
Docs/
├── 00_CONTEXT.md           ← 이 파일 (세션 시작 시 필독)
├── 01_ARCHITECTURE.md       # 시스템 구조
├── 01_CORE_PROJECT_INDEX.md # 프로젝트 파일 인덱스
├── 02_CORE_QUICK_REFERENCE.md # 빠른 참조 가이드
├── 02_DEVELOPMENT.md        # 개발 가이드
├── 03_DATA.md               # JSON 구조
├── 04_TODO.md               # 작업 목록
├── 05_CHANGELOG.md          # 변경 이력
├── 06_ERRORS.md             # 에러 기록
├── 07_TEMPLATE_VARIABLES.md # 템플릿 변수 문서 (동적 생성)
├── terminology_standard.md  # 용어 표준
├── Issues/                  # 이슈 리포트
│   ├── untranslated_report.md
│   ├── translation_priority.md
│   └── ...
└── plans/                   # 구현 계획
```

---

## 용어 기준 (표준)

| 영문 | 한글 |
|------|------|
| Toughness | 건강 |
| Strength | 힘 |
| Agility | 민첩 |
| Intelligence | 지능 |
| Willpower | 의지 |
| Ego | 자아 |
| armor | 갑옷 |
| boots | 장화 |
| gloves | 장갑 |
| cape | 망토 |
| warden | 경비관 |

> 전체 용어집: `Docs/terminology_standard.md`
