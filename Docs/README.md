# Documentation Structure / 문서 구조

> **Last Updated**: 2026-01-22 | Documentation consolidation completed

This folder contains all project documentation, organized by language and purpose.

---

## Directory Layout

```
Docs/
├── README.md              ← This file (index)
├── en/                    # English (Primary - AI/Developer)
│   ├── guides/            # Principles, architecture, development guides
│   ├── reference/         # Project index, TODO, changelog, error log, FAQ
│   └── reports/           # Bug reports, analysis reports, session notes
├── ko/                    # Korean (Deprecated - for historical reference only)
│   ├── guides/            # 원칙, 아키텍처, 워크플로우, 스타일 가이드
│   └── reference/         # ⛔ DEPRECATED - Use en/reference instead
└── Issues/                # Issue tracking
```

> ⚠️ **Note**: Korean reference documents (`ko/reference/`) are deprecated as of 2026-01-22.
> Always use English versions in `en/reference/` as the authoritative source.

---

## English Documents (en/)

### guides/
| File | Description |
|------|-------------|
| `01_PRINCIPLES.md` | Core development principles (REQUIRED READ) |
| `02_ARCHITECTURE.md` | System architecture overview |
| `03_TOOLS_AND_BUILD.md` | Build tools and commands |
| `04_DEVELOPMENT_GUIDE.md` | Detailed development reference |
| `05_GIT_WORKFLOW.md` | Git commit workflow |
| `06_DEBUG_TOOLS.md` | In-game debug/wish commands |
| `07_OBJECT_LOCALIZATION.md` | Object/creature translation plan |
| `08_CHARGEN_DATA.md` | Character creation data structure |
| `09_GAMEPLAY_DATA.md` | Gameplay features data structure |
| `10_UI_DATA.md` | UI localization data structure |
| `11_OBJECTS_DATA.md` | Objects (creatures/items) data structure |
| `12_CYBERNETICS_DATA.md` | Cybernetics data structure |

### reference/
| File | Description |
|------|-------------|
| `01_PROJECT_INDEX.md` | Project file/method index (auto-generated) |
| `02_QUICK_REFERENCE.md` | Quick reference card (auto-generated) |
| `03_TODO.md` | Task tracking (authoritative) |
| `04_CHANGELOG.md` | Change history |
| `05_ERROR_LOG.md` | Error tracking, solutions & FAQ |
| `06_FONT_TRACKER.md` | Font system tracking |
| `07_ISSUE_RULES.md` | Issue tracking system rules |
| `08_ISSUE_QUICK_REF.md` | Issue quick reference |

### reports/
| File | Description |
|------|-------------|
| `01_BUGFIX_REPORT_*.md` | Bug fix documentation |
| `02_CODE_ANALYSIS_REPORT_*.md` | Code analysis reports |
| `03_INTEGRITY_REPORT.md` | LOCALIZATION data integrity report |
| `04_NEXT_SESSION_*.md` | Session continuation notes |

---

## 한국어 문서 (ko/)

> ⛔ **reference/ 폴더는 폐기되었습니다**
> 
> 2026-01-22부터 한국어 참조 문서는 유지되지 않습니다.
> 최신 정보는 영어 문서를 참조하세요: `en/reference/`

### guides/ (유지)
| 파일 | 설명 |
|------|------|
| `01_PRINCIPLES.md` | 핵심 개발 원칙 |
| `02_ARCHITECTURE.md` | 시스템 아키텍처 |
| `03_WORKFLOW.md` | 작업 흐름 가이드 |
| `04_STYLE_GUIDE.md` | 번역 스타일 가이드 |
| `05_GIT_WORKFLOW.md` | Git 커밋 워크플로우 |
| `06_TUTORIAL_GUIDE.md` | 튜토리얼 로컬라이제이션 |
| `07_LOCALIZATION_STRUCTURE.md` | LOCALIZATION 폴더 구조 가이드 |

### reference/ (⛔ 폐기됨)
| 파일 | 상태 |
|------|------|
| `03_TODO.md` | ⛔ 폐기됨 → `en/reference/03_TODO.md` 참조 |
| `04_CHANGELOG.md` | ⛔ 폐기됨 → `en/reference/04_CHANGELOG.md` 참조 |
| `05_ERROR_LOG.md` | ⛔ 폐기됨 → `en/reference/05_ERROR_LOG.md` 참조 |
| `07_ISSUE_RULES.md` | ⛔ 폐기됨 → `en/reference/07_ISSUE_RULES.md` 참조 |

---

## Reading Order

### For AI Agents
```
1. .github/copilot-instructions.md → Session instructions
2. en/reference/03_TODO.md         → Current tasks
3. en/reference/05_ERROR_LOG.md    → Known issues + FAQ
4. en/guides/01_PRINCIPLES.md      → Core principles
```

### For Developers
```
1. en/guides/01_PRINCIPLES.md      → Core principles
2. en/guides/02_ARCHITECTURE.md    → System overview
3. en/guides/03_TOOLS_AND_BUILD.md → Build setup
4. en/reference/05_ERROR_LOG.md    → Errors + FAQ
```

### 번역자용
```
1. ko/guides/04_STYLE_GUIDE.md     → 번역 스타일
2. ko/guides/03_WORKFLOW.md        → 작업 절차
3. en/reference/03_TODO.md         → 현재 작업 (영어)
```

---

## Issues/

Standardized issue tracking system with automated status management.

### Issue Status

| Prefix | Status | Description |
|--------|--------|-------------|
| None | Active | Currently working on |
| `WIP_` | In Progress | Multi-session issue |
| `CLEAR_` | Resolved | Completely fixed |
| `BLOCKED_` | Blocked | Waiting on dependency |
| `DEPRECATED_` | Deprecated | No longer relevant |

### Quick Commands

```bash
# Create new issue
bash tools/create-issue.sh "Description" [priority] [category]

# Update status
bash tools/update-issue-status.sh ISSUE_FILE.md clear
bash tools/update-issue-status.sh ISSUE_FILE.md wip

# List issues
bash tools/list-issues.sh           # All
bash tools/list-issues.sh wip       # WIP only
bash tools/list-issues.sh clear     # Resolved
```

### VS Code Tasks

`Cmd+Shift+P` → "Run Task" → Select:
- "List All Issues" - Show all issues with status
- "Create New Issue" - Interactive issue creation

### Documentation

- [en/reference/07_ISSUE_RULES.md](en/reference/07_ISSUE_RULES.md) - Complete rules
- [Issues/00_INDEX.md](Issues/00_INDEX.md) - Issue index
