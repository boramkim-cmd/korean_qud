# Issue Management System - Quick Reference

## Overview

Automated issue tracking with status-based lifecycle management for the Caves of Qud Korean Localization project.

---

## File Naming Rules

```
[STATUS_]ISSUE_YYYYMMDD_SHORT_DESCRIPTION.md
```

### Status Prefixes

| Status | Prefix | When to Use |
|--------|--------|-------------|
| 🟡 Active | None | New issue, single session work |
| 🟡 WIP | `WIP_` | Multi-session issue, incomplete |
| 🟢 Resolved | `CLEAR_` | Completely fixed and verified |
| 🔴 Blocked | `BLOCKED_` | Waiting for external dependency |
| ⚫ Deprecated | `DEPRECATED_` | No longer relevant |

---

## Quick Commands

### Create Issue
```bash
bash tools/create-issue.sh "Short Description" [priority] [category]

# Examples:
bash tools/create-issue.sh "Stat translation duplication"
bash tools/create-issue.sh "Mutation format" high bug
bash tools/create-issue.sh "Add tooltip support" medium feature
```

### Update Status
```bash
bash tools/update-issue-status.sh ISSUE_FILE.md STATUS

# Examples:
bash tools/update-issue-status.sh ISSUE_20260119_STAT_TRANS.md wip
bash tools/update-issue-status.sh WIP_ISSUE_20260119_STAT_TRANS.md clear
bash tools/update-issue-status.sh ISSUE_20260119_API.md blocked
```

### List Issues
```bash
bash tools/list-issues.sh              # All issues
bash tools/list-issues.sh wip          # Work in progress
bash tools/list-issues.sh clear        # Resolved
bash tools/list-issues.sh blocked      # Blocked
bash tools/list-issues.sh active       # Active only
```

---

## VS Code Integration

### Run Tasks
`Cmd+Shift+P` → Type "Run Task" → Select:
- **List All Issues** - Show all issues with colored status
- **Create New Issue** - Interactive issue creation

### Recommended Workflow
1. `Cmd+Shift+P` → "Run Task" → "List All Issues"
2. Work on issue
3. Update status: `bash tools/update-issue-status.sh ISSUE_FILE.md clear`
4. Commit: `bash tools/quick-commit.sh`

---

## Automation Rules

### On Status Change

| From | To | Actions |
|------|----|----|
| ISSUE_* | WIP_ISSUE_* | 1. Rename file<br>2. Update status in document<br>3. Update timestamp |
| WIP_ISSUE_* | CLEAR_ISSUE_* | 1. Rename file<br>2. Update status in document<br>3. Update timestamp<br>4. Update related issues<br>5. Prompt to update ERROR_LOG/CHANGELOG |
| Any | BLOCKED_* | 1. Rename file<br>2. Add blocking reason note |

### Related Issues
When you update an issue status:
- Script automatically finds files that reference the issue
- Updates all references with new filename
- Lists affected files in output

---

## Workflow Examples

### Simple Bug Fix (Single Session)
```bash
# 1. Create issue
bash tools/create-issue.sh "Color tag duplication" high bug

# 2. Work on fix...

# 3. Mark as resolved
bash tools/update-issue-status.sh ISSUE_20260119_COLOR_TAG.md clear

# 4. Commit
bash tools/quick-commit.sh
```

### Complex Feature (Multi-Session)
```bash
# Day 1: Start work
bash tools/create-issue.sh "Implement tooltip system" high feature

# Day 1: End of session (not complete)
bash tools/update-issue-status.sh ISSUE_20260119_TOOLTIP.md wip

# Day 2: Continue work...

# Day 2: Complete
bash tools/update-issue-status.sh WIP_ISSUE_20260119_TOOLTIP.md clear
```

### Blocked Issue
```bash
# Create issue
bash tools/create-issue.sh "Update to new API" high enhancement

# Discover it's blocked
bash tools/update-issue-status.sh ISSUE_20260119_NEW_API.md blocked

# Later, when unblocked
bash tools/update-issue-status.sh BLOCKED_ISSUE_20260119_NEW_API.md active

# Then resolve
bash tools/update-issue-status.sh ISSUE_20260119_NEW_API.md clear
```

---

## Integration Points

### With ERROR_LOG
When issue is CLEAR:
```markdown
## ERR-XXX: [Error Title]
...
**Resolution**: See [CLEAR_ISSUE_20260119_NAME.md](../Issues/CLEAR_ISSUE_20260119_NAME.md)
```

### With CHANGELOG
When issue is CLEAR:
```markdown
### [Date] - Bug Fixes
- Fixed stat translation duplication ([CLEAR_ISSUE_20260119_STAT_TRANS.md](../Issues/CLEAR_ISSUE_20260119_STAT_TRANS.md))
```

### In Git Commits
```
fix: Resolve stat translation duplication

Resolves ISSUE_20260119_STAT_TRANSLATION
See Docs/Issues/CLEAR_ISSUE_20260119_STAT_TRANSLATION.md
```

---

## Best Practices

### ✅ DO
- Create issues for bugs requiring investigation
- Update status immediately after changes
- Add clear reproduction steps
- Link related issues
- Keep descriptions specific
- Update ERROR_LOG when marking CLEAR
- Commit after status changes

### ❌ DON'T
- Create issues for simple typos
- Leave issues in limbo (update to WIP if continuing)
- Forget to update related issue references
- Mark as CLEAR without verification
- Use vague descriptions

---

## File Locations

- **Issue files**: `Docs/Issues/`
- **Index**: `Docs/Issues/00_INDEX.md`
- **Rules (English)**: `Docs/en/reference/12_ISSUE_MANAGEMENT_RULES.md`
- **Rules (Korean)**: `Docs/ko/reference/12_ISSUE_MANAGEMENT_RULES.md`
- **Tools**: `tools/create-issue.sh`, `tools/update-issue-status.sh`, `tools/list-issues.sh`

---

## Statistics

View current issue statistics:
```bash
bash tools/list-issues.sh
```

Output shows:
- Total issues by status
- List of all issues with dates
- Priority and category breakdown
