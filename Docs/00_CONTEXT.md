# QUD_KOREAN 프로젝트 컨텍스트

> **이 파일은 Claude Code가 세션 시작 시 반드시 읽어야 하는 핵심 문서입니다.**
> 최종 업데이트: 2026-01-27 09:40

---

## 프로젝트 개요

| 항목 | 값 |
|------|-----|
| 프로젝트 | Caves of Qud 한글화 모드 |
| 저장소 | https://github.com/boramkim-cmd/korean_qud |
| 작업 폴더 | `/Users/ben/Desktop/qud_korean` |
| 모드 위치 | `~/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/qud_korean` |
| 진행률 | 복합어 1,634개 중 1,617개 번역 가능 (**99.0%**) |

---

## 현재 상태

### 동작 중인 기능
- 폰트 시스템 (d2coding.bundle)
- JSON 기반 로컬라이제이션
- 캐릭터 생성 UI
- 옵션 화면
- 튜토리얼 팝업
- **ObjectTranslator V2** (Pipeline 아키텍처, 27개 모듈)
- 메시지 로그 패치
- **ObjectTranslator 테스트 스크립트** (197개 테스트 케이스, 100% 통과)
- **비자기참조 색상태그 번역** (`{{glittering|glitter}}` → `{{glittering|글리터}}`)
- **CompoundTranslator** - 복합어 번역 (99% 커버리지)
- **BookTitleTranslator** - 책 제목 한글 어순 변환 (18개 전치사 패턴)
- **빌드 시스템 Phase 1** - JSON 번들링 + 소스맵 (607KB 번들)

### 번역 현황 (2026-01-27)
| 항목 | 개수 |
|------|------|
| XML DisplayName (총) | 2,261 |
| 복합어 후보 | 1,634 |
| **복합어 번역 성공** | **1,617 (99.0%)** |
| 용어 불일치 | **0개** (모두 해결됨) |

### 카테고리별 커버리지
| 카테고리 | 커버리지 |
|----------|----------|
| 벽 | 100% (67/67) |
| 가구 | 100% (174/174) |
| 생물 | 98.8% (726/735) |
| 아이템 | 98.8% (650/658) |

### 어휘 현황 (2026-01-27)
| 항목 | 개수 |
|------|------|
| **총 어휘** | **4,320** |
| modifiers.json | 600+ |
| Materials | 67 |
| Base Nouns | 340+ |
| Species | 260+ |
| Body Parts | 102 |

### 최근 작업 (2026-01-27 오전 - 빌드 최적화)
- ✅ **Phase 1 빌드 시스템 구현** - JSON 번들링 + 소스맵
  - `tools/build_optimized.py`: 302개 JSON → 5개 번들 (607KB)
  - `Scripts/.../SourceMap.cs`: 에러 추적용 소스맵 로더
  - `JsonRepository.cs`: 번들/소스 자동 선택 로딩
  - `ObjectTranslatorV2.cs`: 소스맵 기반 에러 로깅
  - `deploy.sh`: 빌드 → 배포 통합
- 📋 **다음 단계**: 게임 내 테스트 (`kr:stats`로 번들 모드 확인)

### 이전 작업 (2026-01-27 새벽)
- ✅ **BookTitleTranslator 추가** - 책 제목 한글 어순 변환
  - 18개 전치사 패턴: of, with, without, for, from, by, in, to, against, through, under, beyond, among
  - 특수 패턴: "A Guide to X" → "X 안내서", "Introduction to X" → "X 입문"
  - 소유격: "Murmurs' Prayer" → "속삭임의 기도"
  - 복합: "Blood and Fear: On the Life Cycle of La" → "피와 공포: 라의 생명 주기에 대하여"
- ✅ **ColorTagProcessor 패턴 확장**
  - bracket `[]`, colon `:`, quote `"`, `!`, `?` 경계 문자 지원
  - `[fresh water]` → `[신선한 물]` 정상 번역
  - `Fear:` → `공포:` 정상 번역
- ✅ **FallbackHandler 패턴 수정** - 동일한 경계 문자 지원

### 이전 작업 (2026-01-27 오전)
- ✅ **CompoundTranslator 99% 커버리지 달성**
  - ShouldKeepAsIs 메서드 추가 (숫자, 로마숫자, 고유명사, 단일문자 보존)
  - modifiers.json 대규모 확장 (+600개 어휘)
- ✅ 테스트 스크립트 확장 (197개 테스트 케이스)
- ✅ 컬러태그 + 복합어 + 접미사 조합 완벽 지원
- ✅ 소유격 패턴 지원 (`merchant's sword` → `상인의 검`)

### 이전 작업 (2026-01-27 오전)
- ✅ **번역 커버리지 확장 작업** (41% → 62.6%)
- ✅ 패턴 어휘 대규모 확장 (nouns, modifiers, materials)
- ✅ 생물 어휘 확장 (golems, cherubs, NPCs)
- ✅ FoodTranslator 개선 (congealed, concentrated 패턴)
- ✅ 고유명사 추가 (Agolgot, Bethsaida, Qon 등)

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
| **복합어 번역** | `Scripts/02_Patches/20_Objects/V2/Patterns/CompoundTranslator.cs` |
| **책 제목 번역** | `Scripts/02_Patches/20_Objects/V2/Patterns/BookTitleTranslator.cs` |
| **컬러태그 처리** | `Scripts/02_Patches/20_Objects/V2/Processing/ColorTagProcessor.cs` |
| **수식어 어휘** | `LOCALIZATION/OBJECTS/_vocabulary/modifiers.json` |
| 공통 어휘 | `LOCALIZATION/OBJECTS/items/_common.json` |
| 용어 표준 | `Docs/terminology_standard.md` |
| 번역 비교 스크립트 | `tools/compare_translations.py` |
| **복합어 테스트** | `tools/test_compound_translator.py` |
| **패턴 테스트** | `tools/test_all_patterns.py` |
| **빌드 스크립트** | `tools/build_optimized.py` |
| **소스맵 클래스** | `Scripts/.../V2/Data/SourceMap.cs` |
| **빌드 계획** | `Docs/plans/2026-01-27-build-optimization-plan.md` |

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
