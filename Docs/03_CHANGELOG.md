# Caves of Qud 한글화 프로젝트 - 변경 이력 (Changelog)

> **버전**: 2.1 | **최종 업데이트**: 2026-01-16

> [!NOTE]
> **AI 에이전트**: 이 문서는 완료 기록용입니다. 먼저 `00_PRINCIPLES.md`를 읽으세요!

완료된 모든 작업을 기록하는 **공식 변경 이력**입니다.
`02_TODO.md`에서 완료된 항목은 이 문서로 이동됩니다.

---

## 📋 문서 시스템 연동

### 연동 구조
```
01_DEVELOPMENT_GUIDE.md (불변 참조)
          ↓
02_TODO.md (동적 추적)
          ↓
03_CHANGELOG.md (이 문서 - 완료 기록)
          ↓
04_ERROR_LOG.md (에러/이슈 추적)
```

### 버전 규칙
- **Major (X.0.0)**: 대규모 기능 추가 또는 시스템 변경
- **Minor (0.X.0)**: 새로운 화면/기능 번역 완료
- **Patch (0.0.X)**: 버그 수정, 용어 수정

---

# 2026년 1분기 (Q1)

---

## [2026-01-16] v2.0.0 - 문서 시스템 대규모 개편

### 🔄 문서 재구성
기존 14개의 분산된 문서를 4개의 핵심 문서로 통합:

| 이전 | 이후 | 변경 |
|------|------|------|
| 00_CORE_START_HERE.md | → | 01_DEVELOPMENT_GUIDE.md Part A |
| 01_CORE_PROJECT_INDEX.md | → | project_tool.py 자동 생성 (참조) |
| 02_CORE_QUICK_REFERENCE.md | → | 01_DEVELOPMENT_GUIDE.md Part A |
| 03_CORE_API_REFERENCE.md | → | 01_DEVELOPMENT_GUIDE.md Part C |
| 04_CORE_NAMESPACE_GUIDE.md | → | 01_DEVELOPMENT_GUIDE.md Part C |
| 05_CORE_DEVELOPMENT_PROCESS.md | → | 01_DEVELOPMENT_GUIDE.md Part D |
| 06_CORE_TOOLS_GUIDE.md | → | 01_DEVELOPMENT_GUIDE.md Part E |
| 10_DEVELOPMENT_GUIDE.md | → | 01_DEVELOPMENT_GUIDE.md (통합) |
| 10_LOC_WORKFLOW.md | → | 01_DEVELOPMENT_GUIDE.md Part F |
| 11_LOC_GLOSSARY_GUIDE.md | → | 01_DEVELOPMENT_GUIDE.md Part G |
| 11_TODO.md | → | 02_TODO.md (확장) |
| 12_CHANGELOG.md | → | 03_CHANGELOG.md (이 문서, 확장) |
| 13_LOC_STYLE_GUIDE.md | → | 01_DEVELOPMENT_GUIDE.md Part H |
| 14_LOC_QA_CHECKLIST.md | → | 01_DEVELOPMENT_GUIDE.md Part I |

### 📁 백업
- 기존 문서 백업 위치: `Docs/_Legacy_20260116/`

### ✨ 신규 문서
- `04_ERROR_LOG.md`: 에러/이슈 추적 시스템 신규 도입

### 📊 통계
- 이전: 14개 파일, ~92KB
- 이후: 4개 파일 (통합 및 정리)

---

## [2026-01-16] v1.0.0 - 초기 문서 시스템 생성

### 생성된 문서
- `10_DEVELOPMENT_GUIDE.md`: 개발 가이드 (v2.1)
- `11_TODO.md`: TODO 리스트 (v1.0)
- `12_CHANGELOG.md`: 변경 이력

### 문서 연동 구조 확립
```
10_DEVELOPMENT_GUIDE.md (불변 참조)
         ↓
11_TODO.md (동적 추적)
         ↓
12_CHANGELOG.md (완료 기록)
```

---

# Phase 1 완료 항목

> **기간**: 2026-01-?? ~ 2026-??-??
> **목표**: 안정화 - 현재 기능 완성도 100%

---

## [완료 예정]

*아직 Phase 1에서 완료된 항목이 없습니다.*

완료 시 아래 형식으로 기록:

```markdown
### P1-01: 인벤토리 "*All" 필터 번역 ✅
- **완료일**: 2026-??-??
- **담당**: AI 에이전트 / 개발자
- **변경 파일**:
  - `Scripts/02_Patches/UI/02_10_07_Inventory.cs`
- **상세**:
  - FilterBar 컴포넌트 접근 방법 구현
  - "*All" → "전체" 변환 패치
  - 다른 카테고리 (Weapons→무기, Armor→방어구 등) 번역
- **검증**: `project_tool.py` 통과
```

---

# Phase 2 완료 항목

> **기간**: 2026-??-?? ~ 2026-??-??
> **목표**: 게임플레이 - 실제 게임 플레이 요소 번역

---

*아직 Phase 2에서 완료된 항목이 없습니다.*

---

# Phase 3 완료 항목

> **기간**: 2026-??-?? ~ 2026-??-??
> **목표**: 최적화 - 성능 및 유지보수성 개선

---

*아직 Phase 3에서 완료된 항목이 없습니다.*

---

# Phase 4 완료 항목

> **기간**: 2026-??-?? ~ 2026-??-??
> **목표**: 커뮤니티 - 커뮤니티 기여 환경 구축

---

*아직 Phase 4에서 완료된 항목이 없습니다.*

---

# 기록 이전 주요 성과 (Pre-Documentation)

> 2026-01-16 문서 시스템 도입 이전에 완료된 주요 작업들

---

## 핵심 시스템 구축

### TranslationEngine 구현
- **시기**: 2026-01-15 이전
- **상세**: 색상 태그, 체크박스, 대소문자를 무시하고 번역을 찾아주는 핵심 로직 구현
- **파일**: `Scripts/00_Core/00_01_TranslationEngine.cs`

### LocalizationManager 구현
- **시기**: 2026-01-15 이전
- **상세**: JSON 번역 파일 로드 및 카테고리별 관리 시스템 구현
- **파일**: `Scripts/00_Core/00_03_LocalizationManager.cs`

### ScopeManager 구현
- **시기**: 2026-01-15 이전
- **상세**: Stack 기반 현재 활성 번역 범위 관리 시스템 구현
- **파일**: `Scripts/00_Core/00_02_ScopeManager.cs`

### QudKREngine 구현
- **시기**: 2026-01-15 이전
- **상세**: 한국어 폰트 강제 적용, 조사(Josa) 처리 로직 구현
- **파일**: `Scripts/00_Core/00_99_QudKREngine.cs`

---

## UI 패치 구현

### 메인 메뉴 번역 ✅
- **커버리지**: 95%+
- **파일**: `Scripts/02_Patches/UI/02_10_00_GlobalUI.cs`

### 캐릭터 생성 번역 ✅
- **커버리지**: 90%+
- **상세**: 12개 모듈 전체 패치
  - 게임 모드 선택
  - 종족/직업 선택
  - 속성 분배
  - 변이/사이버네틱스 선택
  - 시작 위치 선택
  - 빌드 요약
- **파일**: `Scripts/02_Patches/UI/02_10_10_CharacterCreation.cs`

### 설정 화면 번역 ✅
- **커버리지**: 85%+
- **상세**: 데이터 레이어 + UI 레이어 이중 패치
- **파일**: `Scripts/02_Patches/UI/02_10_01_Options.cs`

### 인벤토리 번역 (부분) 🔄
- **커버리지**: 60%+
- **상세**: 기본 UI 번역 완료, 필터 바 미완성
- **파일**: `Scripts/02_Patches/UI/02_10_07_Inventory.cs`

---

## 용어집 구축

### 완성된 용어집
| 파일 | 항목 수 | 완성도 |
|------|---------|--------|
| `glossary_ui.json` | ~170 | 100% |
| `glossary_chargen.json` | ~130 | 100% |
| `glossary_skills.json` | ~280 | 100% |
| `glossary_cybernetics.json` | ~190 | 100% |
| `glossary_pregen.json` | ~50 | 100% |
| `glossary_proto.json` | ~60 | 100% |
| `glossary_location.json` | ~50 | 100% |
| `glossary_terms.json` | ~35 | 100% |

### 진행 중인 용어집
| 파일 | 항목 수 | 완성도 |
|------|---------|--------|
| `glossary_mutations.json` | ~150 | 96.5% |
| `glossary_options.json` | ~800 | 94% |

---

## 도구 개발

### project_tool.py
- **용도**: 통합 검증 및 메타데이터 생성
- **기능**: 코드 검증, JSON 검증, 문서 자동 생성

### check_missing_translations.py
- **용도**: XML/C# 내 미번역 전수 조사

### check_logs_for_untranslated.py
- **용도**: 게임 로그에서 미번역 텍스트 추출

### deploy-mods.sh
- **용도**: 게임 모드 폴더로 자동 배포

---

# 통계

## 전체 진행률

| 영역 | 완료 | 진행 중 | 미착수 | 완료율 |
|------|------|---------|--------|--------|
| Core 시스템 | 4 | 0 | 0 | 100% |
| UI 패치 | 3 | 1 | 2 | 50% |
| 용어집 | 8 | 2 | 0 | 80% |
| 도구 | 4 | 0 | 3 | 57% |
| **전체** | **19** | **3** | **5** | **70%** |

## 월별 활동

| 월 | 주요 작업 | 완료 항목 수 |
|----|----------|--------------|
| 2026-01 | 문서 시스템 개편, 기초 시스템 구축 | - |

---

*CHANGELOG 버전 2.0 | 2026-01-16 | 문서 통합 완료*
