# Caves of Qud Korean Localization - Error Log

> **Version**: 3.0 | **Last Updated**: 2026-01-22

---

## Active Issues

**None** - All issues resolved

---

## Resolved Issues (Quick Reference)

| ID | Issue | Severity | Date | Key Fix |
|----|-------|----------|------|---------|
| ERR-018 | Tutorial text not translated | High | 01-21 | Smart quote normalization + Korean skip |
| ERR-017 | Attribute screen multi-issues | Medium | 01-19 | StructureTranslator + regex fix |
| ERR-016 | Attribute tooltip not localized | Medium | 01-19 | Scope + BonusSource parsing |
| ERR-015 | Chargen overlay title | Medium | 01-19 | EmbarkBuilder scope management |
| ERR-014 | Toughness inconsistency | Medium | 01-19 | 건강 (not 지구력) |
| ERR-013 | Caste stat modifiers | High | 01-19 | CamelCase normalization |
| ERR-009 | Double dot / missing bullet | High | 01-19 | Trailing period removal |
| ERR-008 | Attribute crash | Critical | 01-19 | Postfix patch (not data) |

---

## Critical Rules Learned

### NEVER Translate These Fields
| Class | Field | Reason |
|-------|-------|--------|
| AttributeDataElement | Attribute | Substring(0,3) crashes |
| ChoiceWithColorIcon | Id | Selection logic breaks |

### Key Patterns

**Smart Quotes**: Game uses U+2019, JSON uses U+0027 - normalize!
```csharp
text.Replace('\u2019', '\'').Replace('\u2018', '\'')
```

**Korean Detection**: Skip already-translated text
```csharp
if (c >= 0xAC00 && c <= 0xD7A3) return true; // Hangul
```

**BonusSource Regex**: Use greedy for color tags
```csharp
@"(.+)\s+(caste|calling|genotype|subtype)\s*([+-]?\d+)"  // greedy .+
```

**Scope Management**: Always push/pop for UI screens
```csharp
BeforeShow: ScopeManager.PushScope(...)
Hide: ScopeManager.PopScope()
```

---

## Terminology Standards

| English | Korean | Note |
|---------|--------|------|
| Toughness (Attr) | 건강 | NOT 지구력 |
| Endurance (Skill) | 지구력 | Different from Toughness |
| Strength | 힘 | |
| Agility | 민첩 | |
| Intelligence | 지능 | |
| Willpower | 의지 | |
| Ego | 자아 | |

---

*Full details: _archive/05_ERROR_LOG_full_20260122.md*
