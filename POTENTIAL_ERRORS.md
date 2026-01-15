# 잠재적 오류 분석

## 🔴 발견된 문제들

### 1. **glossary.json - Invalid JSON** (치명적!)
```json
"bread": "빵",  // ← 마지막 항목인데 쉼표가 있음!
}
```
**line 48**: `"bread": "빵",` 다음에 바로 `}` - JSON 오류!

**해결:** 마지막 쉼표 제거 필요

---

### 2. **JSON 파서 - 단순화 문제**
현재 파서:
```csharp
var parts = trimmed.Split(new[] { ':' }, 2);
string value = parts[1].Trim().Trim('"', ' ');
```

**문제:**
- 값에 콜론(`:`)이 있으면? 예: `"time": "오후 3:00"`
- 값에 쉼표가 있으면? 예: `"list": "A, B, C"`
- 한글 문자열의 이스케이프 처리 안 됨

**해결:** 더 견고한 파싱 필요

---

### 3. **ModManager 타이밍 이슈**
```csharp
var mods = XRL.ModManager.ModManager.Mods;
```

**문제:**
- `Mods`가 게임 초기화 전에 null일 수 있음
- `MainMenu.cs`의 `Translations` getter가 너무 일찍 호출되면?

**해결:** null 체크 강화 필요

---

### 4. **GetTerm fallback 문제**
```csharp
public static string GetTerm(string category, string key, string fallback = "")
{
    // ...
    return string.IsNullOrEmpty(fallback) ? key : fallback;
}
```

**MainMenu.cs**:
```csharp
{ "New Game", _("ui.newGame") }
```

`_()` 함수에서:
```csharp
return GlossaryLoader.GetTerm(category, key, placeholder);
```
→ fallback이 `"ui.newGame"`로 전달됨
→ 파싱 실패 시 `"ui.newGame"`이 그대로 표시됨! (현재 상황)

**해결:** fallback을 빈 문자열로 하거나, 파싱 로직 수정

---

### 5. **Dictionary 타입 캐스팅**
```csharp
var categoryDict = _glossary[category] as Dictionary<string, object>;
```

**문제:** `ParseGlossaryJson`이 `Dictionary<string, object>`를 넣지만, 값이 `string`이어야 함
→ 타입 불일치 가능

---

## ✅ 우선순위 수정사항

1. **즉시 수정:** `glossary.json` line 48 쉼표 제거
2. **중요:** ModManager null 체크 추가
3. **권장:** JSON 파서 개선 또는 단순화

---

**가장 큰 문제: glossary.json이 invalid JSON입니다!**
