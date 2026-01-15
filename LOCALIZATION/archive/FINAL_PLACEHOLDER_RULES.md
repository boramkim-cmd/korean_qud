# 최종 플레이스홀더 규칙 (점 표기법)

## ✅ 확정된 규칙

### 구분자: `.` (점)
```
[[category.key]]
```

### 대소문자: 소문자
```
[[phrase.happy]]      ✅
[[PHRASE.HAPPY]]      ❌
```

---

## 📋 사용 예시

### glossary.json
```json
{
  "phrase": {
    "happy": "행복한 날이 좋아",
    "welcome": "환영합니다",
    "waterRitual": "당신의 갈증은 나의 것"
  },
  "faction": {
    "crystalism": "크리스탈리즘",
    "mechanimists": "메카니카신자"
  },
  "item": {
    "expensiveMilk": "비싼 우유",
    "shortbow": "짧은 활"
  }
}
```

### XML 사용
```xml
<!-- 단순 사용 -->
<text>[[phrase.happy]]</text>
<text>[[phrase.welcome]], 여행자님</text>

<!-- 게임 명령어와 혼합 -->
<text>{{color|cyan|[[faction.crystalism]]}}에 오신 것을 환영합니다</text>
<text>[[item.expensiveMilk]]{을/를} 발견했습니다</text>

<!-- 게임 변수와 혼합 -->
<text>[[phrase.welcome]], =player.name=</text>
<text>{{emote|smile}} [[phrase.waterRitual]]</text>
```

---

## 🔧 처리 코드

### XMLGlossaryProcessor.cs
```csharp
using System.Text.RegularExpressions;
using QudKRTranslation.Core;

namespace QudKRTranslation.XML
{
    public static class XMLGlossaryProcessor
    {
        /// <summary>
        /// [[category.key]] 형식의 플레이스홀더 치환
        /// </summary>
        public static string Process(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            try
            {
                GlossaryLoader.LoadGlossary();
                
                // [[category.key]] 패턴 (점 구분자, 소문자)
                var pattern = @"\[\[([a-z]+)\.([a-zA-Z]+)\]\]";
                
                return Regex.Replace(text, pattern, match =>
                {
                    string category = match.Groups[1].Value;  // phrase
                    string key = match.Groups[2].Value;       // happy
                    
                    string term = GlossaryLoader.GetTerm(category, key, null);
                    
                    if (!string.IsNullOrEmpty(term))
                    {
                        return term;
                    }
                    
                    // 못 찾으면 원본 유지
                    Debug.LogWarning($"[XMLGlossary] 용어를 찾을 수 없음: {match.Value}");
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

## 📊 완전한 예시

### 1. glossary.json 작성
```json
{
  "phrase": {
    "greeting": "안녕하세요",
    "farewell": "안녕히 가세요",
    "waterRitual": "당신의 갈증은 나의 것, 나의 물은 당신의 것"
  },
  "faction": {
    "crystalism": "크리스탈리즘",
    "mechanimists": "메카니카신자",
    "barathrumites": "바라스럼추종자"
  },
  "item": {
    "water": "물",
    "bread": "빵",
    "shortbow": "짧은 활"
  }
}
```

### 2. XML 번역
```xml
<conversation ID="Merchant">
  <node ID="Start">
    <!-- 간단한 인사 -->
    <text>[[phrase.greeting]]!</text>
    
    <!-- 세력 소개 -->
    <text>[[faction.crystalism]] 상점에 오신 것을 환영합니다.</text>
    
    <!-- 아이템 제안 -->
    <text>[[item.shortbow]]{을/를} 찾으시나요?</text>
    
    <!-- 게임 명령어와 혼합 -->
    <text>{{color|cyan|[[item.water]]}}{이/가} 필요하신가요?</text>
    
    <!-- 게임 변수와 혼합 -->
    <text>[[phrase.greeting]], =player.name=!</text>
    
    <!-- 복잡한 혼합 -->
    <text>{{emote|bow}} [[phrase.waterRitual]], =pronouns.siblingTerm=</text>
    
    <choice GotoID="Trade">
      <text>[[item.bread]]{을/를} 사겠습니다</text>
    </choice>
    
    <choice GotoID="End">
      <text>[[phrase.farewell]]</text>
    </choice>
  </node>
</conversation>
```

### 3. 결과
```
안녕하세요!
크리스탈리즘 상점에 오신 것을 환영합니다.
짧은 활을 찾으시나요?
[청록색]물[/색상]이 필요하신가요?
안녕하세요, 플레이어이름!
[인사] 당신의 갈증은 나의 것, 나의 물은 당신의 것, 형제여
[선택지] 빵을 사겠습니다
[선택지] 안녕히 가세요
```

---

## 🎯 빠른 참조

### 플레이스홀더 형식
```
[[category.key]]
```

### 카테고리 예시
- `phrase` - 문장/구문
- `faction` - 세력
- `item` - 아이템
- `weapon` - 무기
- `ui` - UI 텍스트
- `common` - 공통 용어

### 사용 예시
```xml
[[phrase.happy]]
[[faction.crystalism]]
[[item.water]]
[[weapon.shortbow]]
[[ui.continue]]
[[common.yes]]
```

---

## ✅ 정리

**확정된 규칙:**
- 구분자: `.` (점)
- 대소문자: 소문자
- 형식: `[[category.key]]`

**예시:**
```json
{"phrase": {"happy": "행복한 날이 좋아"}}
```
```xml
<text>[[phrase.happy]]</text>
```

**결과:** "행복한 날이 좋아"

🎉 완료!
