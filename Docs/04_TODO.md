# Caves of Qud Korean Localization - TODO

> **Version**: 3.6 | **Last Updated**: 2026-01-26

---

## Progress Summary

| Phase | Done | WIP | Todo | Progress |
|-------|------|-----|------|----------|
| Phase 1: Stabilization | 7 | 0 | 0 | 100% |
| Phase 2: Gameplay | 5 | 0 | 1 | 90% |
| Phase 3: Optimization | 1 | 0 | 5 | 17% |
| Phase 4: Community | 0 | 0 | 3 | 0% |
| **Total** | **13** | **0** | **9** | **59%** |

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
| P3-03 | TMP_Text Conditional Skip | 2h | Low | Todo |
| P3-04 | Performance Profiling | 4h | Low | Todo |
| P3-05 | Glossary Conflict Resolution | 12h | High | Todo |
| P3-06 | Tool Scripts Consolidation | 4h | High | ✅ Done |

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

---

## Critical Assessment: 다음 세션 우선순위

### 반드시 해야 할 것 (MUST)
| 순위 | 작업 | 이유 |
|------|------|------|
| 1 | **게임 테스트** | Python 테스트는 시뮬레이션일 뿐, 실제 C# 동작 확인 필수 |
| 2 | **C# 코드 리뷰** | `ObjectTranslator.cs`가 Python 로직과 동기화되었는지 확인 |
| 3 | **V2 C# 구현 판단** | Pipeline 패턴이 실제로 필요한지, 현재 V1 C#로 충분한지 결정 |

### 하면 좋은 것 (SHOULD)
| 작업 | 이유 |
|------|------|
| `Scripts/02_Patches/20_Objects/V2/` 폴더 정리 | 미완성 V2 C# 코드가 있으면 삭제 또는 완성 |
| 엣지 케이스 추가 | 현재 테스트에 없는 패턴 (예: 3단 중첩 색상태그) |
| 성능 프로파일링 | 번역 캐시 효율성 확인 |

### 하지 않아도 되는 것 (NICE TO HAVE)
| 작업 | 이유 |
|------|------|
| V2 Python 더 확장 | Python은 검증용, 이미 100% 통과 |
| 추가 테스트 케이스 100개 더 | 기존 100개로 충분한 커버리지 |

### 경고: 잠재적 문제
1. **Python-C# 괴리**: Python 테스트 통과 ≠ 게임 동작 보장
2. **V2 C# 코드 존재**: `Scripts/02_Patches/20_Objects/V2/` 폴더에 미완성 C# V2 구현 있음
   ```
   V2/
   ├── Core/           # 코어 인터페이스?
   ├── Data/           # 데이터 구조?
   ├── ObjectTranslatorV2.cs  # 6.8KB 메인 파일
   ├── Patterns/       # 패턴 핸들러?
   ├── Pipeline/       # 파이프라인 구조?
   └── Processing/     # 처리 로직?
   ```
   **결정 필요**: 이 V2 C#을 완성할 것인가, 삭제할 것인가?
3. **캐시 문제**: 게임 내 캐시와 Python 테스트의 캐시 로직이 다를 수 있음
4. **이중 관리 부담**: V1 C# + V2 C# + V1 Python + V2 Python = 4개 버전 관리

---

*Full archive: _archive/01_TODO_full_20260122.md*
