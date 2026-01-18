# ⚠️ DEPRECATED - This file is no longer used

> **Merged into `.github/copilot-instructions.md`**
> 
> GitHub Copilot automatically reads `.github/copilot-instructions.md`.
> This file is kept for reference only. (Scheduled removal: 2026-02-01)

---

# 🚨 AI Agent Session Start - Required Reading (LEGACY)

> **Do not start work without reading this file!**
> 
> Read this file first when starting a new session.

---

## 📋 Session Start Checklist

```bash
# Step 1: Read this file (done)

# Step 2: Check error log (required)
# Review recent errors and resolved issues
cat Docs/05_ERROR_LOG.md | head -200

# Step 3: Check recent changes
cat Docs/04_CHANGELOG.md | head -100

# Step 4: Check current TODO
cat Docs/03_TODO.md | head -100
```

---

## 🔴 Past Critical Issues (Never Repeat!)

### ERR-008: Substring Crash (2026-01-19)
- **Cause**: Translated `AttributeDataElement.Attribute` to Korean → game calls `Substring(0,3)` → crash on strings < 3 chars
- **Lesson**: **Check if game source processes data fields (Substring, Split, etc.)!**
- **Resolution**: Never translate data fields directly, use Postfix patch at UI display

### Dangerous Fields List (NEVER translate directly)
| Class | Field | Processing |
|-------|-------|------------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` |
| `ChoiceWithColorIcon` | `Id` | Selection logic comparison |

---

## ⚠️ Pre-Work Required Checks

1. **Before new patch**: Check game source (`Assets/core_source/`) for how field is used
2. **Translation not working**: Check Player.log (`~/Library/Logs/Freehold Games/CavesOfQud/Player.log`)
3. **Character creation work**: Review ERR-008 ~ ERR-011

---

## 📝 Session End Required Tasks

1. Log errors in `05_ERROR_LOG.md`
2. Log completed work in `04_CHANGELOG.md`
3. Add lessons learned to this file or `00_PRINCIPLES.md`

---

## 🔗 Key Document Links

- [00_PRINCIPLES.md](Docs/00_PRINCIPLES.md) - Seven Core Principles
- [05_ERROR_LOG.md](Docs/05_ERROR_LOG.md) - Error History
- [04_CHANGELOG.md](Docs/04_CHANGELOG.md) - Change History
- [06_ARCHITECTURE.md](Docs/06_ARCHITECTURE.md) - System Architecture

---

**Last Updated**: 2026-01-19
**Last Session Summary**: Converted all documentation to English, enhanced Copilot instructions
