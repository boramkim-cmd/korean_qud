# 게임 명령어 vs 플레이스홀더 충돌 해결

## ⚠️ 문제: 중괄호 충돌

Caves of Qud는 이미 `{{}}` 를 게임 명령어로 사용합니다!

### 게임 명령어 예시
```xml
<text>{{emote|licks lips}}</text>           <!-- 감정 표현 -->
<text>{{color|red|위험!}}</text>            <!-- 색상 -->
<text>{{player.name}}</text>                <!-- 플레이어 이름 -->
<text>{{subject.waterRitualLiquid}}</text>  <!-- 동적 값 -->
```

**문제:** 우리 플레이스홀더 `{{PHRASE_HAPPY}}`와 충돌!

---

## ✅ 해결책: 다른 구분자 사용

### 방법 1: `[[]]` 사용 (권장 ⭐)

**glossary.json**
```json
{
  "phrase": {
    "happy": "행복한 날이 좋아"
  }
}
```

**XML**
```xml
<!-- 게임 명령어 (그대로) -->
<text>{{emote|smile}}</text>

<!-- 우리 플레이스홀더 (대괄호) -->
<text>[[PHRASE_HAPPY]]</text>

<!-- 혼합 사용 -->
<text>{{color|blue|[[PHRASE_HAPPY]]}}</text>
```

**처리 코드**
```csharp
public static string ProcessGlossary(string text)
{
    // [[CATEGORY_KEY]] 패턴만 치환
    var pattern = @"\[\[([A-Z]+)_([A-Z_]+)\]\]";
    
    return Regex.Replace(text, pattern, match =>
    {
        string categoryKey = match.Groups[1].Value;
        string termKey = match.Groups[2].Value.ToLower();
        
        if (_categoryMap.ContainsKey(categoryKey))
        {
            string category = _categoryMap[categoryKey];
            return GlossaryLoader.GetTerm(category, termKey, match.Value);
        }
        
        return match.Value;
    });
}
```

---

### 방법 2: `@@` 사용

**XML**
```xml
<!-- 게임 명령어 -->
<text>{{emote|smile}}</text>

<!-- 우리 플레이스홀더 -->
<text>@@PHRASE_HAPPY@@</text>
```

**처리 코드**
```csharp
var pattern = @"@@([A-Z]+)_([A-Z_]+)@@";
```

---

### 방법 3: `$$` 사용

**XML**
```xml
<text>$$PHRASE_HAPPY$$</text>
```

**처리 코드**
```csharp
var pattern = @"\$\$([A-Z]+)_([A-Z_]+)\$\$";
```

---

## 📋 실전 예시

### 게임 명령어 + 플레이스홀더 혼합

**glossary.json**
```json
{
  "faction": {
    "crystalism": "크리스탈리즘"
  },
  "phrase": {
    "welcome": "환영합니다"
  }
}
```

**XML (대괄호 사용)**
```xml
<conversation>
  <!-- 게임 명령어 -->
  <text>{{emote|bow}}</text>
  
  <!-- 플레이스홀더 -->
  <text>[[PHRASE_WELCOME]], 여행자님</text>
  
  <!-- 혼합 -->
  <text>{{color|cyan|[[FACTION_CRYSTALISM]]}}에 오신 것을 환영합니다</text>
  
  <!-- 복잡한 혼합 -->
  <text>{{player.name}}, [[PHRASE_WELCOME]]! {{emote|smile}}</text>
</conversation>
```

**결과**
```
[인사 동작]
환영합니다, 여행자님
[청록색]크리스탈리즘[/색상]에 오신 것을 환영합니다
플레이어이름, 환영합니다! [미소]
```

---

## 🔧 완전한 구현

### XMLGlossaryProcessor.cs 업데이트

```csharp
using System.Text.RegularExpressions;
using QudKRTranslation.Core;

namespace QudKRTranslation.XML
{
    public static class XMLGlossaryProcessor
    {
        private static Dictionary<string, string> _categoryMap = new Dictionary<string, string>()
        {
            { "PHRASE", "phrase" },
            { "FACTION", "faction" },
            { "WEAPON", "weapon" },
            { "ITEM", "item" }
        };
        
        /// <summary>
        /// [[CATEGORY_KEY]] 형식의 플레이스홀더만 치환
        /// (게임의 {{}} 명령어와 충돌 방지)
        /// </summary>
        public static string Process(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            try
            {
                GlossaryLoader.LoadGlossary();
                
                // [[CATEGORY_KEY]] 패턴 (대괄호 2개)
                var pattern = @"\[\[([A-Z]+)_([A-Z_]+)\]\]";
                
                return Regex.Replace(text, pattern, match =>
                {
                    string categoryKey = match.Groups[1].Value;
                    string termKey = match.Groups[2].Value.ToLower();
                    
                    if (_categoryMap.ContainsKey(categoryKey))
                    {
                        string category = _categoryMap[categoryKey];
                        string term = GlossaryLoader.GetTerm(category, termKey, null);
                        
                        if (!string.IsNullOrEmpty(term))
                        {
                            return term;
                        }
                    }
                    
                    // 못 찾으면 원본 유지
                    return match.Value;
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[XMLGlossary] 처리 오류: {ex.Message}");
                return text;
            }
        }
    }
}
```

---

## 📊 구분자 비교

| 구분자 | 예시 | 장점 | 단점 |
|--------|------|------|------|
| `[[]]` | `[[PHRASE_HAPPY]]` | 명확, 게임과 구분 | 약간 김 |
| `@@` | `@@PHRASE_HAPPY@@` | 짧음 | 이메일과 혼동 가능 |
| `$$` | `$$PHRASE_HAPPY$$` | 짧음 | 달러 기호와 혼동 |
| `##` | `##PHRASE_HAPPY##` | 짧음 | 주석과 혼동 가능 |

**권장:** `[[]]` (대괄호 2개) ⭐

---

## ✅ 최종 가이드

### 1. glossary.json 작성
```json
{
  "phrase": {
    "happy": "행복한 날이 좋아"
  },
  "faction": {
    "crystalism": "크리스탈리즘"
  }
}
```

### 2. XML에서 사용
```xml
<!-- 게임 명령어 (그대로) -->
<text>{{emote|smile}}</text>
<text>{{color|red|위험!}}</text>
<text>{{player.name}}</text>

<!-- 우리 플레이스홀더 (대괄호) -->
<text>[[PHRASE_HAPPY]]</text>
<text>[[FACTION_CRYSTALISM]]에 오신 것을 환영합니다</text>

<!-- 혼합 -->
<text>{{color|cyan|[[FACTION_CRYSTALISM]]}}</text>
```

### 3. 처리 코드
- `XMLGlossaryProcessor.cs`에서 `[[]]` 패턴만 치환
- 게임의 `{{}}` 명령어는 그대로 유지

### 4. 결과
- 게임 명령어: 정상 작동
- 플레이스홀더: JSON 값으로 치환
- 충돌 없음!

---

## 🎯 정리

**문제:** 게임이 이미 `{{}}` 사용  
**해결:** `[[]]` 사용으로 구분

**예시:**
```xml
<!-- 게임 명령어 -->
{{emote|smile}}

<!-- 우리 플레이스홀더 -->
[[PHRASE_HAPPY]]

<!-- 혼합 -->
{{color|blue|[[PHRASE_HAPPY]]}}
```

**결과:** 충돌 없이 모두 정상 작동! 🎉
