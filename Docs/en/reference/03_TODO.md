# Caves of Qud Korean Localization - TODO

> **Version**: 3.2 | **Last Updated**: 2026-01-22

---

## Progress Summary

| Phase | Done | WIP | Todo | Progress |
|-------|------|-----|------|----------|
| Phase 1: Stabilization | 6 | 0 | 0 | 100% |
| Phase 2: Gameplay | 3 | 0 | 1 | 75% |
| Phase 3: Optimization | 1 | 0 | 5 | 17% |
| Phase 4: Community | 0 | 0 | 3 | 0% |
| **Total** | **10** | **0** | **9** | **53%** |

---

## Next Session Required

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

### Phase 1 (6/6)
| Task | Date |
|------|------|
| Inventory "*All" Filter | 01-16 |
| Options 50 Empty Values | 01-16 |
| Josa Color Tag Support | 01-16 |
| Missing Mutation Desc | 01-16 |
| Mutation JSON Restructure (81 files) | 01-22 |
| Tutorial Translation (ERR-018) | 01-21 |

### Phase 2 (3/4)
| Task | Date |
|------|------|
| Object Blueprint System | 01-22 |
| Object/Creature Translation (57 files, 321+ entries) | 01-22 |
| Message Log Patch (P2-01) | 01-22 |

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

*Full archive: _archive/03_TODO_full_20260122.md*
