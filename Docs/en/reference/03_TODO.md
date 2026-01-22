# Caves of Qud Korean Localization - TODO

> **Version**: 3.0 | **Last Updated**: 2026-01-22

---

## Progress Summary

| Phase | Done | WIP | Todo | Progress |
|-------|------|-----|------|----------|
| Phase 1: Stabilization | 6 | 0 | 0 | 100% |
| Phase 2: Gameplay | 1 | 1 | 2 | 25% |
| Phase 3: Optimization | 0 | 0 | 6 | 0% |
| Phase 4: Community | 0 | 0 | 3 | 0% |
| **Total** | **7** | **1** | **11** | **37%** |

---

## Next Session Required

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

## In Progress

### P2-03: Object/Creature Translation
| Item | Value |
|------|-------|
| Files | LOCALIZATION/OBJECTS/**/*.json |
| Status | 51 JSON files, 300+ entries |
| Progress | Partial (names done, descriptions WIP) |

---

## Backlog

### Phase 2: Gameplay
| ID | Task | Hours | Priority |
|----|------|-------|----------|
| P2-01 | Message Log Patch (XRL.Messages.MessageQueue) | 8h | Medium |
| P2-04 | NPC Dialogue (Conversations.xml 647KB) | 16h | Medium |

### Phase 3: Optimization  
| ID | Task | Hours | Priority |
|----|------|-------|----------|
| P3-01 | Translation Caching (LRU) | 4h | Low |
| P3-02 | Missing Term Auto-Collector | 3h | Low |
| P3-03 | TMP_Text Conditional Skip | 2h | Low |
| P3-04 | Performance Profiling | 4h | Low |
| P3-05 | Glossary Conflict Resolution | 12h | High |
| P3-06 | Tool Scripts Consolidation | 4h | High |

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

### Phase 2 (1/4)
| Task | Date |
|------|------|
| Object Blueprint System | 01-22 |

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
