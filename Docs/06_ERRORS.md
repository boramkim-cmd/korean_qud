# 에러 로그

> **Version**: 4.0 | **Last Updated**: 2026-01-25

---

## Active Issues

**None** - All issues resolved

---

## Resolved Issues

| ID | Issue | Severity | Date | Key Fix |
|----|-------|----------|------|---------|
| ERR-019 | Dictionary duplicate key bug | Critical | 01-25 | 중복 키 3개 삭제 (worn, polished, weird) |
| ERR-018 | Tutorial text not translated | High | 01-21 | Smart quote normalization + Korean skip |
| ERR-017 | Attribute screen multi-issues | Medium | 01-19 | StructureTranslator + regex fix |
| ERR-016 | Attribute tooltip not localized | Medium | 01-19 | Scope + BonusSource parsing |
| ERR-015 | Chargen overlay title | Medium | 01-19 | EmbarkBuilder scope management |
| ERR-014 | Toughness inconsistency | Medium | 01-19 | 건강 (not 지구력) |
| ERR-013 | Caste stat modifiers | High | 01-19 | CamelCase normalization |
| ERR-009 | Double dot / missing bullet | High | 01-19 | Trailing period removal |
| ERR-008 | Attribute crash | Critical | 01-19 | Postfix patch (not data) |

---

## ERR-019: Dictionary Duplicate Key Bug (2026-01-25)

### 증상
- 모든 오브젝트 번역 실패
- `TypeInitializationException` 발생

### 원인
커밋 `a3651bf`에서 `_descriptivePrefixes` 딕셔너리에 중복 키 3개 추가됨:
- `"worn"` (147줄과 223줄)
- `"polished"` (214줄과 229줄)
- `"weird"` (149줄과 235줄)

### 해결
```csharp
// 중복 항목 삭제
- { "worn", "낡은" },      // 223줄 삭제
- { "polished", "광택나는" }, // 229줄 삭제
- { "weird", "이상한" },    // 235줄 삭제
```

### 교훈
1. Dictionary 항목 추가 전 `grep -n "키" 파일.cs`로 중복 확인
2. `deploy.sh` 성공 ≠ 코드 정상 (런타임 테스트 필수)
3. `TypeInitializationException` = static 필드 점검

---

## Critical Rules

### NEVER Translate These Fields
| Class | Field | Reason |
|-------|-------|--------|
| `AttributeDataElement` | `Attribute` | Substring(0,3) crashes |
| `ChoiceWithColorIcon` | `Id` | Selection logic breaks |

### Dictionary 수정 시
```bash
# 반드시 중복 확인!
grep -n "키이름" ObjectTranslator.cs
```

### Key Patterns

**Smart Quotes**: Game uses U+2019, JSON uses U+0027 - normalize!
```csharp
text.Replace('\u2019', '\'').Replace('\u2018', '\'')
```

**Korean Detection**: Skip already-translated text
```csharp
if (c >= 0xAC00 && c <= 0xD7A3) return true; // Hangul
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
