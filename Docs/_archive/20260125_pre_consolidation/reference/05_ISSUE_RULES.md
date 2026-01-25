# Issue Management System - Rules and Guidelines

> **Version**: 1.0  
> **Last Updated**: 2026-01-19  
> **Purpose**: Standardized issue tracking and documentation system

---

## Overview

This system provides automated issue documentation and lifecycle management for the Caves of Qud Korean Localization project.

---

## File Naming Convention

### Status Prefixes

| Status | Prefix | Meaning | Example |
|--------|--------|---------|---------|
| New/Active | None | Issue is being worked on | `ISSUE_20260119_STAT_TRANSLATION.md` |
| In Progress | `WIP_` | Multi-session issue, incomplete | `WIP_ISSUE_20260119_STAT_TRANSLATION.md` |
| Resolved | `CLEAR_` | Completely resolved | `CLEAR_ISSUE_20260119_STAT_TRANSLATION.md` |
| Blocked | `BLOCKED_` | Waiting for external dependency | `BLOCKED_ISSUE_20260119_API_CHANGE.md` |
| Deprecated | `DEPRECATED_` | No longer relevant | `DEPRECATED_ISSUE_20260119_OLD_APPROACH.md` |

### Format Structure

```
[STATUS_]ISSUE_YYYYMMDD_SHORT_DESCRIPTION.md

Components:
- [STATUS_]: Optional prefix (WIP/CLEAR/BLOCKED/DEPRECATED)
- ISSUE: Fixed prefix for all issue files
- YYYYMMDD: Date of issue creation
- SHORT_DESCRIPTION: Underscore-separated description (3-5 words)
```

**Examples:**
```
ISSUE_20260119_CHARACTER_CREATION_CRASH.md
WIP_ISSUE_20260119_MUTATION_DESCRIPTION_FORMAT.md
CLEAR_ISSUE_20260118_COLOR_TAG_DUPLICATION.md
BLOCKED_ISSUE_20260119_GAME_API_UPDATE.md
```

---

## Issue Lifecycle

```
┌─────────────┐
│   Created   │ ISSUE_*.md
└──────┬──────┘
       │
       ├──→ Work Started
       │
┌──────▼──────┐
│ In Progress │ WIP_ISSUE_*.md
└──────┬──────┘
       │
       ├──→ Resolved ──────→ ┌──────────┐
       │                     │ Resolved │ CLEAR_ISSUE_*.md
       │                     └──────────┘
       │
       ├──→ Blocked ───────→ ┌──────────┐
       │                     │ Blocked  │ BLOCKED_ISSUE_*.md
       │                     └──────────┘
       │
       └──→ Deprecated ────→ ┌────────────┐
                             │ Deprecated │ DEPRECATED_ISSUE_*.md
                             └────────────┘
```

---

## Document Structure Template

```markdown
# Issue: [Title]

**Status**: 🟡 WIP / 🟢 CLEAR / 🔴 BLOCKED / ⚫ DEPRECATED  
**Created**: YYYY-MM-DD  
**Updated**: YYYY-MM-DD  
**Priority**: Critical/High/Medium/Low  
**Category**: Bug/Feature/Enhancement/Documentation  
**Related Issues**: #[issue_id], #[issue_id]

---

## Problem Description

Clear description of the issue, including:
- What is not working
- Expected behavior
- Actual behavior
- Impact/severity

## Steps to Reproduce (if bug)

1. Step one
2. Step two
3. ...

## Root Cause Analysis

Technical explanation of why the issue occurs.

## Solution Approach

### Attempted Solutions

| Attempt | Description | Result | Reason |
|---------|-------------|--------|--------|
| 1 | ... | ❌ Failed | ... |
| 2 | ... | ✅ Success | ... |

### Final Solution

Detailed explanation of the working solution.

## Implementation

### Files Modified

- `path/to/file1.cs` - Description
- `path/to/file2.json` - Description

### Code Changes

```csharp
// Before
[problematic code]

// After
[fixed code]
```

## Verification

- [ ] Code compiles without errors
- [ ] Game launches successfully
- [ ] Issue is resolved in-game
- [ ] No regressions introduced
- [ ] Documentation updated

## Related Issues

List of related issues with status:
- ✅ CLEAR_ISSUE_20260118_RELATED_ISSUE_1
- 🟡 WIP_ISSUE_20260119_RELATED_ISSUE_2

## Lessons Learned

Key insights and patterns discovered.

## Next Steps (if WIP)

- [ ] Task 1
- [ ] Task 2
- [ ] Task 3
```

---

## Automation Rules

### Auto-create Issue

Trigger conditions:
1. Error/bug discovered during development
2. New feature request identified
3. Complex problem requiring multi-step solution
4. Issue requiring cross-session work

### Auto-update Status

| Event | Action |
|-------|--------|
| Issue becomes multi-session | Rename: `ISSUE_*` → `WIP_ISSUE_*` |
| Issue fully resolved | Rename: `[WIP_]ISSUE_*` → `CLEAR_ISSUE_*` |
| Related issue resolved | Update "Related Issues" section |
| Issue blocked | Rename: `[WIP_]ISSUE_*` → `BLOCKED_ISSUE_*` |
| Issue no longer relevant | Rename: `[WIP_]ISSUE_*` → `DEPRECATED_ISSUE_*` |

### Auto-update Index

After any status change:
1. Update `Docs/Issues/00_INDEX.md`
2. Sort by date (newest first)
3. Update status indicators
4. Generate summary statistics

---

## Integration with Other Systems

### Link to Error Log

When issue is resolved:
```
ERROR_LOG.md → Reference to CLEAR_ISSUE_*.md
```

### Link to Changelog

When issue is resolved:
```
CHANGELOG.md → Add entry with link to CLEAR_ISSUE_*.md
```

### Git Commits

Reference issues in commit messages:
```
fix: Resolve stat translation duplication

Resolves ISSUE_20260119_STAT_TRANSLATION
See Docs/Issues/CLEAR_ISSUE_20260119_STAT_TRANSLATION.md
```

---

## Tools

### Create New Issue
```bash
bash tools/create-issue.sh "Short Description"
```

### Update Issue Status
```bash
bash tools/update-issue-status.sh ISSUE_20260119_NAME.md clear
bash tools/update-issue-status.sh ISSUE_20260119_NAME.md wip
bash tools/update-issue-status.sh ISSUE_20260119_NAME.md blocked
```

### List Issues by Status
```bash
bash tools/list-issues.sh          # All
bash tools/list-issues.sh wip      # Only WIP
bash tools/list-issues.sh clear    # Only resolved
bash tools/list-issues.sh blocked  # Only blocked
```

---

## Best Practices

### When to Create Issue

✅ **DO Create**:
- Bug requires investigation
- Feature needs planning
- Problem spans multiple files/sessions
- Complex debugging needed
- Requires coordination with multiple systems

❌ **DON'T Create**:
- Simple typo fix
- Single-line change
- Obvious solution
- Already documented in ERROR_LOG

### Issue Descriptions

✅ **Good**:
- Specific and actionable
- Includes reproduction steps
- Links to relevant code/docs
- Clear success criteria

❌ **Bad**:
- Vague ("fix stuff")
- No context
- No clear resolution criteria

### Status Updates

- Update status **immediately** after state change
- Keep "Related Issues" section current
- Update timestamps
- Add resolution details before marking CLEAR

---

## Statistics & Reporting

Generate statistics:
```bash
python3 tools/issue-stats.py
```

Output includes:
- Total issues by status
- Average resolution time
- Most common issue types
- Weekly issue creation rate

---

## Migration from Old System

Old reports in `Docs/en/reports/` are preserved for reference.  
New issues follow this standardized system in `Docs/Issues/`.
