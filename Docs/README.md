# Documentation Structure / 문서 구조

This folder contains all project documentation, organized by language and purpose.

---

## Directory Layout

```
Docs/
├── README.md              ← This file
├── en/                    # English (AI/Developer - Primary)
│   ├── guides/            # Principles, architecture, development guides
│   ├── reference/         # Project index, TODO, changelog, error log
│   └── reports/           # Bug reports, analysis reports, session notes
├── ko/                    # Korean (User-facing)
│   ├── guides/            # 원칙, 아키텍처, 워크플로우, 스타일 가이드
│   └── reference/         # TODO, 변경로그, 에러로그
└── Issues/                # Issue tracking
```

---

## English Documents (en/)

### guides/
| File | Description |
|------|-------------|
| `00_PRINCIPLES.md` | Core development principles (REQUIRED READ) |
| `06_ARCHITECTURE.md` | System architecture overview |
| `09_TOOLS_AND_BUILD.md` | Build tools and commands |
| `10_DEVELOPMENT_GUIDE.md` | Detailed development reference |

### reference/
| File | Description |
|------|-------------|
| `01_CORE_PROJECT_INDEX.md` | Project file/method index |
| `01_PROJECT_INDEX.md` | Legacy project index |
| `02_CORE_QUICK_REFERENCE.md` | Quick reference card |
| `02_QUICK_REFERENCE.md` | Legacy quick reference |
| `03_TODO.md` | Task tracking |
| `04_CHANGELOG.md` | Change history |
| `05_ERROR_LOG.md` | Error tracking & solutions |
| `12_ISSUE_MANAGEMENT_RULES.md` | Issue tracking system rules |

### reports/
| File | Description |
|------|-------------|
| `BUGFIX_REPORT_*.md` | Bug fix documentation |
| `CODE_ANALYSIS_REPORT_*.md` | Code analysis reports |
| `NEXT_SESSION_*.md` | Session continuation notes |

---

## 한국어 문서 (ko/)

### guides/
| 파일 | 설명 |
|------|------|
| `00_PRINCIPLES_KO.md` | 핵심 개발 원칙 |
| `06_ARCHITECTURE_KO.md` | 시스템 아키텍처 |
| `07_WORKFLOW.md` | 작업 흐름 가이드 |
| `08_STYLE_GUIDE.md` | 번역 스타일 가이드 |

### reference/
| 파일 | 설명 |
|------|------|
| `03_TODO_KO.md` | 할일 목록 |
| `04_CHANGELOG_KO.md` | 변경 기록 |
| `05_ERROR_LOG_KO.md` | 에러 기록 |
| `12_ISSUE_MANAGEMENT_RULES.md` | 이슈 추적 시스템 규칙 |

---

## Reading Order

### For AI Agents
```
1. en/guides/00_PRINCIPLES.md     → Required first read
2. en/reference/03_TODO.md        → Current tasks
3. en/reference/05_ERROR_LOG.md   → Known issues
4. en/guides/10_DEVELOPMENT_GUIDE.md → Reference as needed
```

### For Developers
```
1. en/guides/00_PRINCIPLES.md     → Core principles
2. en/guides/06_ARCHITECTURE.md   → System overview
3. en/guides/09_TOOLS_AND_BUILD.md → Build setup
```

### 번역자용
```
1. ko/guides/08_STYLE_GUIDE.md    → 번역 스타일
2. ko/guides/07_WORKFLOW.md       → 작업 절차
3. ko/reference/03_TODO_KO.md     → 현재 작업
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

- [en/reference/12_ISSUE_MANAGEMENT_RULES.md](en/reference/12_ISSUE_MANAGEMENT_RULES.md) - Complete rules
- [ko/reference/12_ISSUE_MANAGEMENT_RULES.md](ko/reference/12_ISSUE_MANAGEMENT_RULES.md) - 규칙 (한국어)
- [Issues/00_INDEX.md](Issues/00_INDEX.md) - Issue index
