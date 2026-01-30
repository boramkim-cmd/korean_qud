# Caves of Qud Korean Localization - TODO

> **Version**: 3.9 | **Last Updated**: 2026-01-30

---

## Progress Summary

| Phase | Done | WIP | Todo | Progress |
|-------|------|-----|------|----------|
| Phase 1: Stabilization | 7 | 0 | 0 | 100% |
| Phase 2: Gameplay | 5 | 0 | 1 | 90% |
| Phase 3: Optimization | 6 | 0 | 1 | 86% |
| Phase 4: Community | 0 | 0 | 3 | 0% |
| **Total** | **18** | **0** | **5** | **78%** |

### Translation Coverage (2026-01-30)
| Metric | Value |
|--------|-------|
| **총 고유 항목** | 2,989 |
| **번역됨** | 2,904 |
| **미번역** | 85 |
| **커버리지** | **97.2%** |
| **미번역 사유** | 모두 `=variable=` 또는 `*template*` 패턴 (런타임 생성) |

이전: 933/2,989 (31.2%) → 현재: 2,904/2,989 (97.2%)

### Test Coverage
| 스크립트 | 케이스 | 통과율 | 목적 |
|----------|--------|--------|------|
| `test_object_translator.py` | 100 | 100% | JSON 사전 기반 번역 로직 검증 |
| `test_display_contexts.py` | 100 | 100% | V1 vs V2 동등성 + 컨텍스트별 검증 |

---

## Next Session Required

### CRITICAL: Python 테스트 vs C# 실제 동작 검증
> **문제**: Python 테스트 스크립트(test_object_translator.py, test_display_contexts.py)는
> JSON 사전 기반으로 번역 로직을 **시뮬레이션**할 뿐, 실제 C# 코드를 검증하지 않음.

| 위험 요소 | 설명 | 대응 |
|-----------|------|------|
| **로직 동기화 불일치** | Python 로직 수정 시 C# 미반영 가능 | C# 코드 직접 확인 필요 |
| **런타임 차이** | 게임 내 이벤트 순서, 캐싱 등 Python에서 재현 불가 | 게임 테스트 필수 |
| **V2 미구현** | V2(Pipeline 패턴)는 Python에만 존재, C#은 여전히 V1 | V2 C# 구현 필요 여부 판단 |

**권장 작업**:
1. `./deploy.sh` 후 실제 게임에서 아래 항목 직접 확인
2. Python 테스트 100% 통과가 게임 동작 보장 아님을 인지

### Game Test: PRD v2 버그 수정 검증 - HIGH PRIORITY
| Item | Expected | Python 테스트 | 게임 테스트 |
|------|----------|---------------|-------------|
| `{{c|basic toolkit}}` | 기본 공구함 | ✅ Pass | [ ] |
| `{{w|copper nugget}}` | 구리 덩어리 | ✅ Pass | [ ] |
| `{{m|violet}} tube` | 보라색 튜브 | ✅ Pass | [ ] |
| `ape fur cloak` | 유인원 모피 망토 | ✅ Pass | [ ] |
| `sandals of the river-wives` | 강 아내들의 샌들 | ✅ Pass | [ ] |
| `fried processing core` | 튀긴 처리 코어 | ✅ Pass | [ ] |

**Test Path**: `kr:stats` 확인 후 wish 명령어로 개별 아이템 테스트

### Game Test: Item Tooltip (P2-02)
| Item | Expected | Status |
|------|----------|--------|
| Tooltip header (comparison) | "현재 아이템" / "장착 아이템" | [ ] |
| Static item name | "횃불", "청동 단검" | [ ] |
| Item with state suffix | "물주머니 [비어있음]" | [ ] |
| Dynamic food item | "곰 육포", "곰 고기" | [ ] |
| Font display | Korean characters visible | [ ] |
| World map item tooltip | Same translation as inventory | [ ] |

**Test Path**: New game - Pick up items - Hover for tooltip - Compare with equipped item

### Game Test: Message Log (P2-01)
| Item | Expected | Status |
|------|----------|--------|
| Flight messages | "당신은 날아오릅니다" | [ ] |
| Movement messages | "당신은 위로 올라갑니다" | [ ] |
| Item messages | "당신은 ~을(를) 집었습니다" | [ ] |

**Test Path**: New game - Start moving around - Check message log

### Game Test: Attribute Screen (ERR-017 fix)
| Item | Expected | Status |
|------|----------|--------|
| Attribute names | 힘, 민첩, 건강, 지능, 의지, 자아 | [ ] |
| Point cost | [1점], [2점] | [ ] |
| Remaining points | 남은 포인트: 38 | [ ] |
| Caste bonus tooltip | 달의사제 계급 +2 | [ ] |
| Breadcrumb | Korean caste name | [ ] |

**Test Path**: New game - True Kin - Select caste - Attribute screen

### Game Test: Skills & Powers (NEW - 01-25)
| Item | Expected | Status |
|------|----------|--------|
| Skill names | 도끼, 곤봉, 롱 블레이드 등 | [ ] |
| Power names | 도끼 숙련, 쪼개기, 돌격 타격 등 | [ ] |
| Skill descriptions | 한글 설명 표시 | [ ] |
| Power descriptions | 한글 상세 설명 표시 | [ ] |

**Test Path**: New game - Character screen (C) - Skills tab

---

## 번역 커버리지 확장 (2,056개 미번역)

> 상세 리포트: `Docs/plans/untranslated_report.md`

| 우선순위 | 대상 | 미번역 | 상태 |
|----------|------|--------|------|
| P1 | ActivatedAbilities (48) + Mutations (10) | 58 | Todo |
| P2 | Creatures | 770 | Todo |
| P3 | Items | 463 | Todo |
| P4 | Furniture (209) + Walls (99) | 308 | Todo |
| P5 | ZoneTerrain (81) + WorldTerrain (22) | 103 | Todo |
| P6 | Factions (37) + ChiliadFactions (30) | 67 | Todo |
| P7 | HiddenObjects (동적 패턴) | 221 | Todo |
| P8 | 나머지 (Foods, Data, etc.) | 89 | Todo |

---

## Backlog

### Phase 2: Gameplay
| ID | Task | Hours | Priority | Status |
|----|------|-------|----------|--------|
| P2-01 | Message Log Patch | 8h | Medium | ✅ Done |
| P2-04 | NPC Dialogue (Conversations.xml 647KB) | 16h | Medium | Todo |

### Phase 3: Optimization
| ID | Task | Hours | Priority | Status |
|----|------|-------|----------|--------|
| P3-01 | Translation Caching (LRU) | 4h | Low | Todo |
| P3-02 | Missing Term Auto-Collector | 3h | Low | Todo |
| P3-03 | TMP_Text Conditional Skip | 2h | Low | ✅ Done |
| P3-04 | Performance Profiling (kr:perf) | 4h | Low | ✅ Done |
| P3-05 | Glossary Conflict Resolution | 12h | High | Todo |
| P3-06 | Tool Scripts Consolidation | 4h | High | ✅ Done |
| P3-07 | V1 vs V2 테스트 스크립트 | 2h | Medium | ✅ Done |
| P3-08 | V2 ObjectTranslator Pipeline 이전 | 4h | High | ✅ Done |
| **P3-09** | **빌드 최적화 Phase 1 (JSON 번들링)** | 4h | High | ✅ Done |
| P3-10 | 빌드 최적화 Phase 2 (바이너리+Trie) | 6h | Medium | Todo (필요시) |

### Phase 4: Community
| ID | Task | Priority |
|----|------|----------|
| P4-01 | Web Translation Editor | Optional |
| P4-02 | Steam Workshop Auto-Deploy | Optional |
| P4-03 | Version Compatibility CI | Optional |

---

## Completed

### Phase 1 (7/7)
| Task | Date |
|------|------|
| Inventory "*All" Filter | 01-16 |
| Options 50 Empty Values | 01-16 |
| Josa Color Tag Support | 01-16 |
| Missing Mutation Desc | 01-16 |
| Mutation JSON Restructure (81 files) | 01-22 |
| Tutorial Translation (ERR-018) | 01-21 |
| Skills JSON Restructure (20 files) + Patch | 01-25 |

### Phase 2 (5/6)
| Task | Date |
|------|------|
| Object Blueprint System | 01-22 |
| Object/Creature Translation (57 files, 321+ entries) | 01-22 |
| Message Log Patch (P2-01) | 01-22 |
| Objects Translation Major Expansion (67 files, 6,169 entries) | 01-24 |
| Items Translation Extension (weapons, armor, grenades) (+787 entries) | 01-25 |

### Testing & Tooling
| Task | Date |
|------|------|
| ObjectTranslator 종합 테스트 (100개 케이스) | 01-26 |
| V1 vs V2 컨텍스트별 검증 테스트 (100개 케이스, Pipeline 패턴) | 01-26 |

---

## Critical Rules

### Dangerous Fields - NEVER Translate
| Class | Field | Why | Safe Point |
|-------|-------|-----|------------|
| AttributeDataElement | Attribute | Substring(0,3) | UI Postfix |
| ChoiceWithColorIcon | Id | Selection logic | Title only |

### Pre-Deploy Checklist
```bash
python3 tools/project_tool.py   # Required!
bash tools/sync-and-deploy.sh   # Deploy
```

---

## Critical Assessment: 다음 세션 우선순위

### 반드시 해야 할 것 (MUST)
| 순위 | 작업 | 이유 |
|------|------|------|
| 1 | **게임 테스트 (번들 모드 + 성능)** | 빌드 최적화 + 성능 최적화 구현 완료. `kr:stats` + `kr:perf` 확인 |
| 2 | **성능 카운터 확인** | `kr:perf`로 스킵율 50%+ 확인, 폰트 캐시 히트 확인 |
| 3 | **에러 추적 테스트** | 소스맵 기반 에러 로깅 동작 확인 |

### 하면 좋은 것 (SHOULD)
| 작업 | 이유 |
|------|------|
| V1 파일 완전 삭제 | 테스트 기간 후 `.cs.disabled` 파일 삭제 |
| Phase 2 필요성 판단 | 성능 측정 후 바이너리+Trie 인덱스 필요 여부 결정 |
| 성능 프로파일링 | 번들 로딩 시간 측정 |

### 하지 않아도 되는 것 (NICE TO HAVE)
| 작업 | 이유 |
|------|------|
| Phase 2 바이너리 포맷 | Phase 1으로 충분할 가능성 높음 |
| 추가 테스트 케이스 | 기존 197개로 충분한 커버리지 |

### 경고: 잠재적 문제
1. **Python-C# 괴리**: Python 테스트 통과 ≠ 게임 동작 보장 (게임 테스트 필수)
2. **번들 로딩**: Phase 1 완료, 게임 내 번들 로딩 동작 확인 필요
   - 번들 경로: `data/objects.json` 등 5개 파일
   - 소스맵: `sourcemap.json` (에러 추적용)
   - 폴백: LOCALIZATION/ 있으면 소스 모드로 동작
3. **캐시 문제**: 게임 내 캐시와 Python 테스트의 캐시 로직이 다를 수 있음

---

*Full archive: _archive/01_TODO_full_20260122.md*
