# 03. HistorySpice.json 번역 전략

**우선순위:** 🔴 최우선 (Critical)  
**파일 크기:** 183 KB  
**예상 작업 기간:** 5-7일  
**난이도:** ⭐⭐⭐⭐⭐

---

## 📋 파일 개요

### 파일 정보
- **경로:** `/StreamingAssets/Base/HistorySpice.json`
- **내용:** 절차적 역사 생성 템플릿
- **중요도:** 게임 세계관의 핵심

### 왜 최우선인가?
1. 게임의 역사를 동적으로 생성
2. 술탄 계보, 파벌 관계 생성
3. NPC 대화에서 참조됨
4. 게임 리플레이 가치의 핵심

---

## 🔍 구조 분석

### JSON 기본 구조

```json
{
  "spice": {
    "elements": { ... },
    "professions": { ... },
    "commonPhrases": { ... },
    "gossip": { ... }
  }
}
```

### 주요 섹션

#### 1. Elements (요소)
```json
"glass": {
  "professions": ["glassblower", "window maker"],
  "materials": ["glass", "sand"],
  "adjectives": ["glazed", "stained"],
  "nouns": ["prism", "glass", "mirror"],
  "murdermethods": ["by trapping <pronouns> in a prism"]
}
```

#### 2. Professions (직업)
```json
"glassblower": {
  "singular": "glassblower",
  "plural": "glassblowers",
  "actions": ["built a glass gazebo"],
  "guildhall": "workshop"
}
```

#### 3. Common Phrases (공통 문구)
```json
"commonPhrases": {
  "astral": ["astral", "shadow", "spectral"],
  "blessed": ["blessed", "exalted", "sacred"]
}
```

---

## 🎯 번역 전략

### 1단계: 섹션별 분석 (1일)

**우선순위:**
1. **commonPhrases** (가장 자주 사용)
2. **elements** (역사 생성 핵심)
3. **professions** (직업 관련)
4. **gossip** (NPC 대화)

### 2단계: Elements 번역 (2-3일)

**11개 Element:**
- glass (유리)
- jewels (보석)
- stars (별)
- time (시간)
- salt (소금)
- ice (얼음)
- scholarship (학문)
- might (힘)
- chance (운)
- circuitry (회로)
- travel (여행)

**각 Element당 10-15개 속성 번역**

### 3단계: Common Phrases 번역 (1-2일)

**200개 이상의 문구:**
```json
"astral": ["천상의", "그림자", "유령 같은"],
"blessed": ["축복받은", "고귀한", "신성한"]
```

### 4단계: Professions 번역 (1일)

**30개 이상의 직업:**
```json
"glassblower": {
  "singular": "유리공",
  "plural": "유리공들",
  "actions": ["유리 정자를 지었다"],
  "guildhall": "작업장"
}
```

---

## ⚠️ 주의사항

### 1. 변수 문법 절대 유지

**❌ 잘못:**
```json
"<spice.pronouns.object.!random>" → "<spice.대명사.목적격.!랜덤>"
```

**✅ 올바름:**
```json
"<spice.pronouns.object.!random>" → 그대로 유지
```

### 2. JSON 구조 변경 금지

**❌ 잘못:**
```json
"adjectives": ["red", "blue"]
→ "형용사": {"빨간": "red", "파란": "blue"}
```

**✅ 올바름:**
```json
"adjectives": ["빨간", "파란"]
```

### 3. 조사 처리 문제

**문제:**
```json
"*f1*이/가 *f2*에게서 유물을 훔쳤다"
```

**해결:**
```json
// 방법 1: 조사 회피
"*f1*, 그들은 *f2*에게서 유물을 훔쳤다"

// 방법 2: 명사형
"*f1*의 *f2* 유물 절도"
```

---

## 📝 실전 예시

### 예시 1: Elements - Glass

**원문:**
```json
"glass": {
  "professions": ["glassblower", "window maker"],
  "materials": ["glass", "sand"],
  "adjectives": ["glazed", "stained", "clear", "prismatic"],
  "nouns": ["prism", "glass", "mirror"],
  "murdermethods": [
    "by trapping <spice.pronouns.object.!random> in a prism",
    "with a dagger made of <^.materials.!random>"
  ]
}
```

**번역:**
```json
"glass": {
  "professions": ["유리공", "창문 제작자"],
  "materials": ["유리", "모래"],
  "adjectives": ["유약을 바른", "착색된", "투명한", "프리즘의"],
  "nouns": ["프리즘", "유리", "거울"],
  "murdermethods": [
    "<spice.pronouns.object.!random>을/를 프리즘에 가둬서",
    "<^.materials.!random>로 만든 단검으로"
  ]
}
```

### 예시 2: Common Phrases

**원문:**
```json
"commonPhrases": {
  "astral": ["astral", "shadow", "spectral", "illusory"],
  "blessed": ["blessed", "exalted", "sacred"],
  "killed": ["killed", "murdered", "drawn and quartered", "exiled"]
}
```

**번역:**
```json
"commonPhrases": {
  "astral": ["천상의", "그림자", "유령 같은", "환영의"],
  "blessed": ["축복받은", "고귀한", "신성한"],
  "killed": ["죽였다", "살해했다", "사지를 찢었다", "추방했다"]
}
```

### 예시 3: Gossip

**원문:**
```json
"gossip": {
  "leadIns": [
    "Did you hear?",
    "Rumor is that",
    "Someone told me that"
  ],
  "twoFaction": [
    "*f1* stole @item.a@item.name from *f2*.",
    "*f1* sold @item.a@item.name to *f2* for too much money."
  ]
}
```

**번역:**
```json
"gossip": {
  "leadIns": [
    "들었어?",
    "소문에 의하면",
    "누군가 말하길"
  ],
  "twoFaction": [
    "*f1*이/가 *f2*에게서 @item.name을/를 훔쳤대.",
    "*f1*이/가 *f2*에게 @item.name을/를 너무 비싸게 팔았대."
  ]
}
```

---

## 🎓 용어 가이드

### Elements 용어

| English | 한글 | 비고 |
|---------|------|------|
| glass | 유리 | |
| jewels | 보석 | |
| stars | 별 | |
| time | 시간 | |
| salt | 소금 | |
| ice | 얼음 | |
| scholarship | 학문 | |
| might | 힘 | |
| chance | 운 | |
| circuitry | 회로 | |
| travel | 여행 | |

### 역사 용어

| English | 한글 |
|---------|------|
| Sultan | 술탄 |
| Chronicle | 연대기 |
| Legendary | 전설적인 |
| Mythical | 신화적인 |
| Relic | 유물 |
| Artifact | 고대고철 |

---

## 🔧 기술적 고려사항

### 변수 참조 패턴

```json
// 절대 경로
"<spice.elements.glass.nouns.!random>"

// 상대 경로 (현재 element 내)
"<^.materials.!random>"

// 랜덤 선택
".!random"
```

### 중첩 변수

```json
"a famous <^.professions.!random> completed their work on a legendary *var*"
→ "유명한 <^.professions.!random>이/가 전설적인 *var*에 대한 작업을 완료했다"
```

---

## 📊 진행 상황 추적

### 체크리스트

```markdown
## HistorySpice.json 번역 진행

### Elements (2-3일)
- [ ] glass (유리)
- [ ] jewels (보석)
- [ ] stars (별)
- [ ] time (시간)
- [ ] salt (소금)
- [ ] ice (얼음)
- [ ] scholarship (학문)
- [ ] might (힘)
- [ ] chance (운)
- [ ] circuitry (회로)
- [ ] travel (여행)

### Common Phrases (1-2일)
- [ ] adjectives (형용사)
- [ ] nouns (명사)
- [ ] verbs (동사)
- [ ] 기타 문구

### Professions (1일)
- [ ] 30개 직업 번역

### Gossip (1일)
- [ ] leadIns
- [ ] twoFaction
```

---

## 🚨 흔한 실수

### 1. 변수 경로 수정
**❌:** `<spice.elements.glass>` → `<spice.요소.유리>`

### 2. JSON 문법 오류
**❌:** 마지막 항목에 쉼표 추가
```json
"items": [
  "item1",
  "item2",  // 마지막 쉼표 제거!
]
```

### 3. 따옴표 이스케이프
**✅:** 문자열 내 따옴표는 `\"`로 이스케이프

---

## 🔗 관련 문서

- **문서 01:** Conversations.xml (동적 대화 사용)
- **문서 02:** Quests.xml (동적 퀘스트 사용)
- **문서 10:** Naming.xml (이름 생성)

---

**작성일:** 2026-01-13  
**우선순위:** 🔴 최우선  
**예상 완료:** 5-7일
