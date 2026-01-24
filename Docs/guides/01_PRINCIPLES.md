# 🚨 AI Agent Required Document - Development Principles

> **This document is the starting point for all work. Read it completely.**
>
> 📍 **Document Size**: ~4KB | **Estimated Reading Time**: 2 min | **Required**: ⭐⭐⭐

---

## 📚 Documentation System Structure

```
Docs/
├── README.md                      ← Documentation index
├── MASTER.md                      ← Project overview
├── 01_CORE_PROJECT_INDEX.md       ← Auto-generated index
├── 02_CORE_QUICK_REFERENCE.md     ← Auto-generated quick reference
│
├── guides/                        # Guides
│   ├── 01_PRINCIPLES.md           ← This document (required, read first)
│   ├── 02_ARCHITECTURE.md         ← System architecture
│   ├── 03_TOOLS_AND_BUILD.md      ← Build tools guide
│   └── 04_DEVELOPMENT_GUIDE.md    ← Detailed development guide
│
├── reference/                     # Reference docs
│   ├── 01_TODO.md                 ← Task tracking
│   ├── 02_CHANGELOG.md            ← Completion log
│   └── 03_ERROR_LOG.md            ← Error tracking
│
├── reports/                       ← Bug/analysis reports
└── Issues/                        ← Issue tracking
```

### Document Reading Order
```
1. guides/01_PRINCIPLES.md (this document) → Required, read fully
2. reference/01_TODO.md → Check current work in progress
3. reference/03_ERROR_LOG.md → Check known issues
4. guides/04_DEVELOPMENT_GUIDE.md → Reference specific parts as needed
```

---

## 🎯 Seven Core Principles (Must Memorize)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ 1. Documentation First: If not documented, it doesn't exist            │
│ 2. No Guessing: Always verify in actual code (use grep commands)       │
│ 3. Reuse First: Search existing code before writing new                │
│ 4. Validation Required: Never deploy without project_tool.py           │
│ 5. Log Errors: All issues go to 03_ERROR_LOG.md                        │
│ 6. Pursue Completeness: Don't leave in intermediate state              │
│ 7. Detailed Records: Write specifically so AI can understand           │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## ⚠️ Forbidden Actions (NEVER DO)

| Forbidden | Reason | Correct Approach |
|-----------|--------|------------------|
| Use `_Legacy/` folder | No longer valid | Use only `Scripts/` folder |
| Guess method signatures | Harmony patch fails | Verify with `grep` |
| Deploy without validation | Runtime errors | Run `project_tool.py` first |
| Translate special tags | Game breaks | Keep `%var%`, `{{tag}}` as-is |
| Check only XRL.UI | Qud.UI may be used | Check both namespaces |

---

## ✅ Pre-Work Checklist

```bash
# 1. Check documentation (required)
cat Docs/guides/01_PRINCIPLES.md    # This document
cat Docs/reference/01_TODO.md       # Work in progress
cat Docs/reference/03_ERROR_LOG.md  # Known issues

# 2. Validate project state
python3 tools/project_tool.py

# 3. Research target (for new features)
grep -r "class ClassName" Assets/core_source/
grep -r "feature_name" Scripts/02_Patches/
grep -r "keyword" LOCALIZATION/*.json
```

---

## 📝 Documentation Update Rules

| Situation | Action |
|-----------|--------|
| Starting work | `01_TODO.md`: `[ ]` → `[/]` |
| Completing work | `01_TODO.md`: `[/]` → `[x]` |
| Error occurs | Log immediately in `03_ERROR_LOG.md` |
| Error resolved | Log solution in `03_ERROR_LOG.md` |
| Phase complete | Summarize in `02_CHANGELOG.md` |

---

## 🔧 Essential Commands (Copy and Use)

```bash
# Find class
grep -r "class ClassName" Assets/core_source/

# Check method signature
grep -A 5 "void MethodName" Assets/core_source/_GameSource/*/File.cs

# Find text source
grep -ri "text" Assets/core_source/ Assets/StreamingAssets/Base/

# Validate project
python3 tools/project_tool.py

# Deploy mod
./tools/deploy-mods.sh
```

---

## 📂 Key File Paths

| Purpose | Path |
|---------|------|
| Translation Engine | `Scripts/00_Core/00_00_01_TranslationEngine.cs` |
| Data Management | `Scripts/00_Core/00_00_03_LocalizationManager.cs` |
| Global UI Patch | `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs` |
| Glossaries | `LOCALIZATION/*.json` |

---

## 🎯 Work Completion Criteria

For work to be considered "complete":

```markdown
☐ project_tool.py validation passed
☐ In-game testing completed
☐ 01_TODO.md status updated
☐ Errors logged in 03_ERROR_LOG.md if any occurred
```

---

## 💡 Lessons Learned

> **"Game engines are masters of text fragmentation."**

| Lesson | Details | Solution |
|--------|---------|----------|
| **Tag Fragmentation** | Tags can appear mid-word like `{{C|2}}0` | Always assume Replace-based restoration may fail; implement fallback logic |
| **Tags in Glossary** | Auto-restoration is not perfect | Include important color/format tags directly in glossary translations |
| **Engine API First** | `LocalizationManager` is for pure data access | Always use `TranslationEngine.TryTranslate` which includes tag handling |
| **🔴 Data Processing Check** | Game source may use `Substring()`, `Split()` on fields | **NEVER translate processed fields directly!** Use Postfix patch at UI display. (See ERR-008) |
| **🔴 Dynamic Text** | Reputation etc. are runtime-generated, cannot define in JSON | Use Regex pattern matching for dynamic translation. (See ERR-011) |

---

## 🔴 Dangerous Fields List (NEVER Translate Directly)

> **Added 2026-01-19**: These fields are processed by game source - direct translation causes crash

| Class | Field | Processing | Safe Patch Point |
|-------|-------|------------|------------------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` | `AttributeSelectionControl.Updated()` |
| `ChoiceWithColorIcon` | `Id` | Selection logic comparison | Translate `Title` only |

Add new dangerous fields to this list when discovered.

---

> **Next Step**: See relevant Part in `10_DEVELOPMENT_GUIDE.md` for specific work details
