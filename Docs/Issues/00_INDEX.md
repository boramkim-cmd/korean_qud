# Issue Resolution Index
> **Purpose**: Standardized issue tracking and documentation  
> **Location**: `Docs/Issues/`  
> **Naming Convention**: `[STATUS_]ISSUE_YYYYMMDD_SHORT_DESCRIPTION.md`  
> **Management System**: See [05_ISSUE_RULES.md](../reference/05_ISSUE_RULES.md)

---

## Status Legend

| Emoji | Status | Description |
|-------|--------|-------------|
| 🟡 | Active/WIP | Currently being worked on |
| 🟢 | CLEAR | Completely resolved |
| 🔴 | BLOCKED | Waiting on external dependency |
| ⚫ | DEPRECATED | No longer relevant |

---

## Active Issues

| Date | Description | Priority | Category | File |
|------|-------------|----------|----------|------|
| - | - | - | - | - |

## Work In Progress

| Date | Description | Priority | Category | File |
|------|-------------|----------|----------|------|
| - | - | - | - | - |

## Resolved Issues

| Date | Description | Priority | Category | File |
|------|-------------|----------|----------|------|
| 2026-01-25 | Dictionary Duplicate Key Bug | Critical | Bug | [03_ISSUE_20260125_DICTIONARY_DUPLICATE_KEY.md](03_ISSUE_20260125_DICTIONARY_DUPLICATE_KEY.md) |
| 2026-01-19 | Options Panel Translation (Root Fix) | High | UI | [01_ISSUE_20260119_OPTIONS_PANEL.md](02_ISSUE_20260119_OPTIONS_PANEL.md) |
| 2026-01-19 | Code Analysis 16 Fixes | High | Bug | [01_ISSUE_20260119_CODE_ANALYSIS.md](01_ISSUE_20260119_CODE_ANALYSIS.md) |

## Blocked Issues

| Date | Description | Priority | Category | File |
|------|-------------|----------|----------|------|
| - | - | - | - | - |

---

## Tools & Commands

### Create New Issue
```bash
bash tools/create-issue.sh "Short Description" [priority] [category]
```

### Update Issue Status
```bash
bash tools/update-issue-status.sh ISSUE_FILE.md wip
bash tools/update-issue-status.sh ISSUE_FILE.md clear
bash tools/update-issue-status.sh ISSUE_FILE.md blocked
bash tools/update-issue-status.sh ISSUE_FILE.md deprecated
```

### List Issues
```bash
bash tools/list-issues.sh           # All issues
bash tools/list-issues.sh wip       # WIP only
bash tools/list-issues.sh clear     # Resolved only
bash tools/list-issues.sh blocked   # Blocked only
```

---

## Statistics

| Metric | Value |
|--------|-------|
| 🟡 Active | 0 |
| 🟡 WIP | 0 |
| 🟢 Resolved | 3 |
| 🔴 Blocked | 0 |
| ⚫ Deprecated | 0 |
| **Total** | **3** |

---

## Issue Document Template

When creating new issue reports, follow this structure:

```markdown
# Issue Resolution Report: [Short Title]
**Date**: YYYY-MM-DD
**Session Duration**: ~X minutes
**Issue Count**: N
**Resolution Status**: ✅/❌

## 1. Issue Identification Method
- How issues were discovered
- Source documents or symptoms

## 2. Issues Summary
- Table of all issues with severity

## 3. Resolution Approach
- Strategy used
- Tools and patterns applied

## 4. Failed Attempts & Lessons Learned
- What didn't work and why
- Key insights gained

## 5. Successful Resolutions
- What worked and why
- Code before/after examples

## 6. Build Verification Results
- Compilation status
- Test results

## 7. Session Review & Conclusion
- What went well
- What could improve
- Key takeaways

## 8. Next Steps
- Immediate actions
- Short-term tasks
- Long-term improvements

## Appendix: Files Modified
- List of all changed files
```

---

## Categories

### By Severity Distribution
- **Critical Issues**: Reports with 🔴 critical fixes
- **Refactoring**: Code improvement without bug fixes
- **Performance**: Optimization-focused sessions
- **Translation**: Localization data issues

### By Component
- **Core**: `Scripts/00_Core/` changes
- **Patches**: `Scripts/02_Patches/` changes
- **Utils**: `Scripts/99_Utils/` changes
- **Localization**: `LOCALIZATION/` data changes

---

## Statistics

| Metric | Value |
|--------|-------|
| Total Reports | 1 |
| Total Issues Resolved | 16 |
| Success Rate | 100% |
| Most Common Failure | API assumption errors |

---

## Related Documents
- [03_ERROR_LOG.md](../reference/03_ERROR_LOG.md) - Critical error history (ERR-XXX)
- [02_CHANGELOG.md](../reference/02_CHANGELOG.md) - Version change history
- [02_CODE_ANALYSIS_REPORT_20260119.md](../reports/02_CODE_ANALYSIS_REPORT_20260119.md) - Source analysis
