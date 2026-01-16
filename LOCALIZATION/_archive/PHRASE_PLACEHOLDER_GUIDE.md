# 문장 전체를 플레이스홀더로 만들기

## 🎯 목표
"happyday is good" 전체를 JSON에 저장하고 플레이스홀더로 사용

---

## ✅ 방법

### 1단계: glossary.json에 문장 추가

```json
{
  "phrases": {
    "happydayIsGood": "행복한 날이 좋아",
    "welcomeMessage": "환영합니다, 여행자님",
    "thanksForVisiting": "방문해 주셔서 감사합니다"
  }
}
```

### 2단계: XML에서 플레이스홀더 사용

```xml
<!-- 원문 -->
<text>happyday is good</text>

<!-- 플레이스홀더로 변경 -->
<text>{{PHRASE_HAPPYDAY_IS_GOOD}}</text>
```

### 3단계: 런타임 치환

자동으로 "행복한 날이 좋아"로 변환됩니다!

---

## 📋 실전 예시

### glossary.json
```json
{
  "phrases": {
    "greeting": "안녕하세요",
    "farewell": "안녕히 가세요",
    "thankYou": "감사합니다",
    "waterRitual": "당신의 갈증은 나의 것, 나의 물은 당신의 것",
    "liveAndDrink": "살아서 마시라"
  },
  "factions": {
    "crystalism": "크리스탈리즘"
  }
}
```

### XML 사용
```xml
<conversation>
  <text>{{PHRASE_GREETING}}</text>
  <text>{{PHRASE_WATER_RITUAL}}</text>
  <text>{{FACTION_CRYSTALISM}}에 오신 것을 환영합니다</text>
  <choice>
    <text>{{PHRASE_THANK_YOU}}</text>
  </choice>
</conversation>
```

### 결과
```
안녕하세요
당신의 갈증은 나의 것, 나의 물은 당신의 것
크리스탈리즘에 오신 것을 환영합니다
[선택지] 감사합니다
```

---

## 🔧 플레이스홀더 명명 규칙

### 단어
```
{{FACTION_CRYSTALISM}}     → "크리스탈리즘"
{{WEAPON_SHORTBOW}}        → "짧은 활"
```

### 문장
```
{{PHRASE_GREETING}}        → "안녕하세요"
{{PHRASE_WATER_RITUAL}}    → "당신의 갈증은..."
{{PHRASE_HAPPYDAY_IS_GOOD}} → "행복한 날이 좋아"
```

### 규칙
- `PHRASE_` 접두사 사용
- 단어는 언더스코어(`_`)로 구분
- 대문자 사용
- JSON에서는 camelCase 사용

---

## 💡 용도별 카테고리

```json
{
  "phrases": {
    "greeting": "안녕하세요",
    "farewell": "안녕히 가세요"
  },
  "factions": {
    "crystalism": "크리스탈리즘"
  },
  "weapons": {
    "shortbow": "짧은 활"
  },
  "common": {
    "yes": "예",
    "no": "아니오"
  }
}
```

---

## 🎯 사용 예시

### 예시 1: 인사말
```json
{"phrases": {"happydayIsGood": "행복한 날이 좋아"}}
```
```xml
<text>{{PHRASE_HAPPYDAY_IS_GOOD}}</text>
```
→ "행복한 날이 좋아"

### 예시 2: 긴 문장
```json
{"phrases": {"waterRitual": "당신의 갈증은 나의 것, 나의 물은 당신의 것입니다"}}
```
```xml
<text>{{PHRASE_WATER_RITUAL}}</text>
```
→ "당신의 갈증은 나의 것, 나의 물은 당신의 것입니다"

### 예시 3: 혼합 사용
```json
{
  "phrases": {"welcome": "환영합니다"},
  "factions": {"crystalism": "크리스탈리즘"}
}
```
```xml
<text>{{PHRASE_WELCOME}}, {{FACTION_CRYSTALISM}} 신자여</text>
```
→ "환영합니다, 크리스탈리즘 신자여"

---

## ✅ 정리

**질문:** "happyday is good" 문장을 플레이스홀더로?

**답변:**
1. JSON: `{"phrases": {"happydayIsGood": "행복한 날이 좋아"}}`
2. XML: `{{PHRASE_HAPPYDAY_IS_GOOD}}`
3. 결과: "행복한 날이 좋아"

**용어 변경:**
- JSON만 수정: `"happydayIsGood": "즐거운 하루가 좋아"`
- 게임 재시작 → 모든 곳에 자동 적용!

🎉 완료!
