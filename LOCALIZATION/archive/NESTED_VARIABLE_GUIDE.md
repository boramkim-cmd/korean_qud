# 변수 안에 변수 사용하기 (중첩 변수)

## 🎯 목표
"비싼우유"를 정의하고, "비싼우유가 맛있어"에서 재사용

---

## ✅ 방법 1: JSON에서 중첩 (간단)

### glossary.json
```json
{
  "item": {
    "expensiveMilk": "비싼 우유"
  },
  "phrase": {
    "milkTasty": "{{ITEM_EXPENSIVE_MILK}}{이/가} 맛있어"
  }
}
```

### 처리 코드 (2단계 치환)
```csharp
public static string ProcessNested(string text)
{
    // 1단계: 일반 변수 치환
    text = XMLGlossaryProcessor.Process(text);
    // "{{ITEM_EXPENSIVE_MILK}}{이/가} 맛있어"
    
    // 2단계: 중첩 변수 다시 치환
    text = XMLGlossaryProcessor.Process(text);
    // "비싼 우유{이/가} 맛있어"
    
    return text;
}
```

### XML 사용
```xml
<text>{{PHRASE_MILK_TASTY}}</text>
```

### 결과
→ "비싼 우유가 맛있어"

---

## ✅ 방법 2: 코드에서 조합 (권장 ⭐)

### glossary.json (단순하게)
```json
{
  "item": {
    "expensiveMilk": "비싼 우유"
  }
}
```

### 코드에서 조합
```csharp
// GlossaryLoader에 헬퍼 함수 추가
public static string GetPhrase(string phraseKey, params object[] args)
{
    string template = GetTerm("phrase", phraseKey, "");
    
    // {0}, {1} 같은 플레이스홀더 치환
    return string.Format(template, args);
}
```

### 사용
```csharp
string milk = GlossaryLoader.GetTerm("item", "expensiveMilk", "비싼 우유");
string phrase = milk + "{이/가} 맛있어";
// → "비싼 우유{이/가} 맛있어"
```

---

## 🎯 실전 예시

### 예시 1: 아이템 + 문장

**glossary.json**
```json
{
  "item": {
    "expensiveMilk": "비싼 우유",
    "cheapBread": "싼 빵"
  },
  "phrase": {
    "itemTasty": "{0}{이/가} 맛있어",
    "itemFound": "{0}{을/를} 발견했습니다"
  }
}
```

**코드**
```csharp
string milk = GlossaryLoader.GetTerm("item", "expensiveMilk", "비싼 우유");
string template = GlossaryLoader.GetTerm("phrase", "itemTasty", "{0}{이/가} 맛있어");

string result = template.Replace("{0}", milk);
// → "비싼 우유{이/가} 맛있어"
```

### 예시 2: 세력 + 인사말

**glossary.json**
```json
{
  "faction": {
    "crystalism": "크리스탈리즘"
  },
  "phrase": {
    "welcome": "{0}에 오신 것을 환영합니다"
  }
}
```

**XML**
```xml
<!-- 옵션 A: 직접 조합 -->
<text>{{FACTION_CRYSTALISM}}에 오신 것을 환영합니다</text>

<!-- 옵션 B: 코드에서 처리 -->
<text>{{PHRASE_WELCOME_CRYSTALISM}}</text>
```

**코드 (옵션 B)**
```csharp
string faction = GlossaryLoader.GetTerm("faction", "crystalism", "크리스탈리즘");
string template = GlossaryLoader.GetTerm("phrase", "welcome", "{0}에 오신 것을 환영합니다");

AddTranslation("PHRASE_WELCOME_CRYSTALISM", template.Replace("{0}", faction));
// → "크리스탈리즘에 오신 것을 환영합니다"
```

---

## 📋 고급: 재귀 치환 구현

### GlossaryLoader에 추가

```csharp
/// <summary>
/// 중첩 변수를 재귀적으로 치환
/// </summary>
public static string ProcessNestedVariables(string text, int maxDepth = 3)
{
    int depth = 0;
    string previous = "";
    
    while (text != previous && depth < maxDepth)
    {
        previous = text;
        
        // {{CATEGORY_KEY}} 패턴 찾아서 치환
        var pattern = @"\{\{([A-Z]+)_([A-Z_]+)\}\}";
        text = Regex.Replace(text, pattern, match =>
        {
            string categoryKey = match.Groups[1].Value;
            string termKey = match.Groups[2].Value.ToLower();
            
            if (_categoryMap.ContainsKey(categoryKey))
            {
                string category = _categoryMap[categoryKey];
                return GetTerm(category, termKey, match.Value);
            }
            
            return match.Value;
        });
        
        depth++;
    }
    
    return text;
}
```

### 사용

**glossary.json**
```json
{
  "item": {
    "milk": "우유"
  },
  "adj": {
    "expensive": "비싼"
  },
  "phrase": {
    "expensiveMilk": "{{ADJ_EXPENSIVE}} {{ITEM_MILK}}",
    "milkTasty": "{{PHRASE_EXPENSIVE_MILK}}{이/가} 맛있어"
  }
}
```

**코드**
```csharp
string text = GlossaryLoader.GetTerm("phrase", "milkTasty", "");
text = GlossaryLoader.ProcessNestedVariables(text);
// 1단계: "{{PHRASE_EXPENSIVE_MILK}}{이/가} 맛있어"
// 2단계: "{{ADJ_EXPENSIVE}} {{ITEM_MILK}}{이/가} 맛있어"
// 3단계: "비싼 우유{이/가} 맛있어"
```

---

## 🎯 권장 방법

### 간단한 경우 (권장)
```json
{
  "item": {
    "milk": "비싼 우유"
  }
}
```
```xml
<text>{{ITEM_MILK}}{이/가} 맛있어</text>
```

### 재사용이 많은 경우
```json
{
  "item": {
    "milk": "우유"
  },
  "adj": {
    "expensive": "비싼"
  }
}
```
```xml
<text>{{ADJ_EXPENSIVE}} {{ITEM_MILK}}{이/가} 맛있어</text>
```

### 복잡한 문장
```json
{
  "item": {
    "expensiveMilk": "비싼 우유"
  },
  "phrase": {
    "milkTasty": "{{ITEM_EXPENSIVE_MILK}}{이/가} 맛있어"
  }
}
```
```xml
<text>{{PHRASE_MILK_TASTY}}</text>
```

---

## ✅ 정리

**질문:** "비싼우유"를 정의하고 "비싼우유가 맛있어"에 재사용?

**답변 1: 간단 (권장)**
```json
{"item": {"expensiveMilk": "비싼 우유"}}
```
```xml
<text>{{ITEM_EXPENSIVE_MILK}}{이/가} 맛있어</text>
```

**답변 2: 중첩 변수**
```json
{
  "item": {"expensiveMilk": "비싼 우유"},
  "phrase": {"milkTasty": "{{ITEM_EXPENSIVE_MILK}}{이/가} 맛있어"}
}
```
```xml
<text>{{PHRASE_MILK_TASTY}}</text>
```
(재귀 치환 코드 필요)

**답변 3: 조합**
```json
{
  "item": {"milk": "우유"},
  "adj": {"expensive": "비싼"}
}
```
```xml
<text>{{ADJ_EXPENSIVE}} {{ITEM_MILK}}{이/가} 맛있어</text>
```

---

**개인 사용 권장:** 답변 1 (간단) 또는 답변 3 (조합)
- 중첩 변수는 복잡도가 높음
- 대부분의 경우 직접 조합이 더 명확함

🎉 완료!
