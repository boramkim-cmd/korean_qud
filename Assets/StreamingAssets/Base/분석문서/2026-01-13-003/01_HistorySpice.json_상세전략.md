# 01. HistorySpice.json 절차적 생성 전략

**절차적 생성 관여도:** ⭐⭐⭐⭐⭐ (최고)  
**파일 크기:** 183 KB  
**예상 작업 기간:** 5-7일  
**역할:** 게임 역사의 핵심 생성 엔진

---

## 🎯 절차적 생성에서의 역할

### 핵심 기능
1. **술탄 계보 생성** - 매 게임마다 다른 술탄 이름과 역사
2. **역사적 사건 생성** - 전쟁, 결혼, 발명 등
3. **파벌 관계 생성** - 동적 파벌 간 갈등
4. **NPC 소문 생성** - 동적 대화 콘텐츠
5. **지역 역사 생성** - 각 지역의 과거

### 생성 예시

**게임 1:**
```
술탄 Resheph the Radiant ruled during the Age of Glass.
He was known for trapping his enemies in prisms.
```

**게임 2:**
```
술탄 Klanq the Mighty ruled during the Age of Iron.
He was known for conquering the salt dunes.
```

---

## 📊 절차적 생성 구조

### 1. Elements (요소) - 역사의 테마

```json
"glass": {
  "professions": ["glassblower"],
  "materials": ["glass", "sand"],
  "adjectives": ["glazed", "prismatic"],
  "murdermethods": ["by trapping <pronouns> in a prism"]
}
```

**생성 방식:**
- 랜덤으로 element 선택 (glass, jewels, stars 등)
- 해당 element의 속성 조합
- 술탄의 특성, 시대 분위기 결정

### 2. Professions (직업) - 역사적 인물

```json
"glassblower": {
  "singular": "glassblower",
  "actions": ["built a glass gazebo"],
  "guildhall": "workshop"
}
```

**생성 방식:**
- 술탄의 직업 결정
- 역사적 업적 생성
- 건물/길드 이름 생성

### 3. Common Phrases (문구) - 서술 템플릿

```json
"killed": ["killed", "murdered", "exiled"],
"blessed": ["blessed", "exalted", "sacred"]
```

**생성 방식:**
- 역사 서술 시 랜덤 선택
- 다양한 표현으로 반복 방지

### 4. Gossip (소문) - 동적 대화

```json
"gossip": {
  "leadIns": ["Did you hear?"],
  "twoFaction": ["*f1* stole a relic from *f2*"]
}
```

**생성 방식:**
- NPC 대화에서 실시간 조합
- 파벌 변수 치환
- 매번 다른 소문 생성

---

## 🔧 번역 전략 (절차적 생성 중심)

### Phase 1: 템플릿 이해 (1일)

**목표:** 생성 메커니즘 파악

```bash
# Elements 개수 확인
grep -c '"professions"' HistorySpice.json

# 변수 패턴 확인
grep -o '<[^>]*>' HistorySpice.json | sort | uniq
```

### Phase 2: 핵심 템플릿 번역 (2-3일)

**우선순위:**
1. **commonPhrases** (가장 자주 조합됨)
2. **elements** (역사 테마)
3. **gossip** (NPC 대화)

### Phase 3: 조합 테스트 (1일)

**테스트 방법:**
1. 번역 적용
2. 새 게임 시작 (3-5회)
3. 생성된 역사 확인
4. 자연스러움 검증

### Phase 4: 조정 및 수정 (1-2일)

**확인 사항:**
- 조사 처리 문제
- 어색한 조합
- 용어 일관성

---

## ⚠️ 절차적 생성 특화 주의사항

### 1. 변수 조합의 자연스러움

**문제:**
```json
"adjectives": ["빛나는", "어두운"],
"nouns": ["프리즘", "거울"]
```

**생성 결과:**
- ✅ "빛나는 프리즘" (자연스러움)
- ✅ "어두운 거울" (자연스러움)
- ❌ "어두운 프리즘" (어색할 수 있음)

**해결:**
- 모든 조합이 자연스러운지 확인
- 필요시 형용사 제한

### 2. 조사 처리 (최대 난제)

**문제:**
```json
"*f1*이/가 *f2*에게서 유물을 훔쳤다"
```

**생성 시:**
- "푸투스 템플러" + "이/가" → ?
- "메카니카 신자" + "이/가" → ?

**해결 방안:**

**방법 A: 조사 회피**
```json
"*f1*, 그들은 *f2*에게서 유물을 훔쳤다"
```

**방법 B: 명사형**
```json
"*f1*의 *f2* 유물 절도"
```

**방법 C: 엔진 지원 (이상적)**
```json
"*f1*<josa_i_ga> *f2*에게서 유물을 훔쳤다"
```

### 3. 문맥 의존성

**문제:**
```json
"murdermethods": [
  "by trapping <pronouns> in a prism"
]
```

**생성 시:**
- `<pronouns>` → "him" 또는 "her"
- 한글: "그를" 또는 "그녀를"

**주의:**
- 대명사가 문맥에 맞는지 확인
- 성별 중립 표현 고려

---

## 📝 실전 번역 예시

### 예시 1: Elements - Glass

**원문:**
```json
"glass": {
  "professions": ["glassblower", "window maker"],
  "materials": ["glass", "sand"],
  "adjectives": ["glazed", "stained", "clear", "prismatic"],
  "nouns": ["prism", "glass", "mirror"],
  "murdermethods": [
    "by trapping <spice.pronouns.object.!random> in a prism"
  ],
  "mythicalEvent": [
    "a *var* shattered in every home"
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
    "<spice.pronouns.object.!random>을/를 프리즘에 가둬서"
  ],
  "mythicalEvent": [
    "모든 가정에서 *var*이/가 산산조각 났다"
  ]
}
```

**생성 결과 예상:**
```
술탄 시대: 유리의 시대
술탄 특성: 프리즘의 술탄
역사적 사건: 모든 가정에서 거울이 산산조각 났다
```

### 예시 2: Gossip (동적 대화)

**원문:**
```json
"gossip": {
  "leadIns": [
    "Did you hear?",
    "Rumor is that"
  ],
  "twoFaction": [
    "*f1* stole @item.a@item.name from *f2*.",
    "*f1* sold @item.a@item.name to *f2* for too much money."
  ]
}
```

**번역 (조사 회피):**
```json
"gossip": {
  "leadIns": [
    "들었어?",
    "소문에 의하면"
  ],
  "twoFaction": [
    "*f1*, 그들은 *f2*에게서 @item.name을/를 훔쳤대.",
    "*f1*, 그들은 *f2*에게 @item.name을/를 너무 비싸게 팔았대."
  ]
}
```

**생성 결과:**
```
NPC: "들었어? 푸투스 템플러, 그들은 메카니카 신자에게서 고대 유물을 훔쳤대."
```

---

## 🧪 테스트 시나리오

### 테스트 1: 역사 생성
```
1. HistorySpice.json 번역 적용
2. 새 게임 시작
3. 역사 화면 확인 (H 키)
4. 술탄 이름 확인
5. 역사적 사건 확인
```

**확인 사항:**
- ✅ 술탄 이름이 한글로 표시
- ✅ 역사 텍스트가 자연스러움
- ✅ 조사가 올바름 (또는 회피됨)

### 테스트 2: NPC 소문
```
1. 게임 진행
2. 여러 NPC와 대화
3. 소문 확인
```

**확인 사항:**
- ✅ 소문이 한글로 표시
- ✅ 파벌명이 올바름
- ✅ 매번 다른 소문 생성

### 테스트 3: 반복 생성
```
1. 게임 종료
2. 새 게임 시작 (5회 반복)
3. 매번 다른 역사 확인
```

**확인 사항:**
- ✅ 매번 다른 술탄
- ✅ 매번 다른 사건
- ✅ 모든 조합이 자연스러움

---

## 📊 진행 상황 추적

### 체크리스트

```markdown
## HistorySpice.json 절차적 생성 번역

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

### Common Phrases (1일)
- [ ] adjectives
- [ ] nouns
- [ ] killed, blessed 등

### Gossip (1일)
- [ ] leadIns
- [ ] twoFaction

### 테스트 (1일)
- [ ] 역사 생성 테스트 (5회)
- [ ] NPC 소문 테스트
- [ ] 조합 자연스러움 확인
```

---

## 🎓 절차적 생성 이해하기

### 생성 흐름

```
1. 게임 시작
   ↓
2. HistorySpice.json 로드
   ↓
3. 랜덤 시드 생성
   ↓
4. Element 선택 (예: glass)
   ↓
5. 술탄 이름 생성 (Naming.xml 사용)
   ↓
6. 술탄 특성 조합
   - professions.!random → "glassblower"
   - adjectives.!random → "prismatic"
   - quality.!random → "mirrored eyes"
   ↓
7. 역사적 사건 생성
   - mythicalEvent.!random
   - 변수 치환
   ↓
8. 파벌 관계 생성
   ↓
9. 역사 완성
```

### 변수 치환 예시

**템플릿:**
```
"a famous <professions.!random> completed their work on a legendary *var*"
```

**치환 과정:**
```
1. <professions.!random> → "glassblower"
2. *var* → "prism" (문맥에서 결정)
3. 최종: "a famous glassblower completed their work on a legendary prism"
```

**번역:**
```
"유명한 <professions.!random>이/가 전설적인 *var*에 대한 작업을 완료했다"
```

**생성 결과:**
```
"유명한 유리공이 전설적인 프리즘에 대한 작업을 완료했다"
```

---

## 🔗 관련 문서

- **문서 02:** Naming.xml (이름 생성)
- **문서 04:** Conversations.xml (동적 대화 사용)
- **문서 05:** Quests.xml (동적 퀘스트 사용)

---

**작성일:** 2026-01-13  
**절차적 생성 관여도:** ⭐⭐⭐⭐⭐  
**예상 완료:** 5-7일
