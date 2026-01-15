# 02. Naming.xml 절차적 생성 전략

**절차적 생성 관여도:** ⭐⭐⭐⭐⭐ (최고)  
**파일 크기:** 169 KB  
**예상 작업 기간:** 1-2일  
**역할:** 모든 이름의 동적 생성

---

## 🎯 절차적 생성에서의 역할

### 핵심 기능
1. **술탄 이름 생성** - 매 게임마다 다른 술탄
2. **NPC 이름 생성** - 수천 개의 고유 이름
3. **지역 이름 생성** - 동적 지역명
4. **파벌 이름 생성** - 절차적 파벌

### 생성 방식

**음절 조합:**
```
prefix + infix + postfix
= "fa" + "ra" + "mut"
= "Faramut"
```

**다양한 문화:**
- Qudish (쿼드식)
- Eater (먹는 자)
- Ekuemekiyyen (에쿠에메키옌)
- Ibulian (이불리안)
- Yawningmoon (하품하는 달)

---

## 📊 절차적 생성 구조

### 기본 구조

```xml
<namestyle Name="Qudish" Format="TitleCase">
  <prefixes Amount="1">
    <prefix Name="fa" />
    <prefix Name="ka" />
  </prefixes>
  <infixes Amount="0-2">
    <infix Name="ra" />
    <infix Name="ga" />
  </infixes>
  <postfixes Amount="1">
    <postfix Name="mut" />
    <postfix Name="bas" />
  </postfixes>
</namestyle>
```

### 생성 예시

**조합 1:**
```
fa + ra + mut = Faramut
```

**조합 2:**
```
ka + ga + bas = Kagabas
```

**조합 3:**
```
mu + (없음) + shur = Mushur
```

---

## 🔧 번역 전략 (음차 권장)

### 방법 1: 음차 (⭐ 권장)

**장점:**
- 구현 간단
- 원작 느낌 유지
- 발음 가능

**예시:**
```
Faramut → 파라무트
Kagabas → 카가바스
Mushur → 무슈르
```

**작업:**
- **번역 불필요!**
- 게임이 자동으로 생성한 이름을 그대로 사용
- 한글 폰트만 지원되면 OK

### 방법 2: 한글 음절 재구축 (비권장)

**단점:**
- 작업량 막대함
- 원작과 다른 느낌
- 세계관 위화감

**예시:**
```xml
<namestyle Name="Korean">
  <prefixes Amount="1">
    <prefix Name="김" />
    <prefix Name="이" />
  </prefixes>
  <postfixes Amount="1">
    <postfix Name="준" />
    <postfix Name="우" />
  </postfixes>
</namestyle>
```

**생성:**
```
김 + 준 = 김준
이 + 우 = 이우
```

**문제:**
- Caves of Qud 세계관과 맞지 않음
- 한국식 이름은 어색함

---

## ⚠️ 절차적 생성 특화 주의사항

### 1. 음절 조합의 발음 가능성

**좋은 조합:**
```
fa + ra + mut = Faramut (파라무트) ✅
```

**나쁜 조합:**
```
kr + xt + zq = Krxtzq (크르스츠크?) ❌
```

**확인:**
- 대부분의 조합이 발음 가능한지 확인
- 한글로 표기 가능한지 테스트

### 2. 문화별 이름 스타일

**Qudish (쿼드식):**
```
Faramut, Kagabas, Mushur
→ 파라무트, 카가바스, 무슈르
```

**Eater (먹는 자):**
```
Orad, Yrad, Xerxes
→ 오라드, 이라드, 크세르크세스
```

**Ekuemekiyyen (아프리카풍):**
```
Abafemi, Chukwu, Nkechi
→ 아바페미, 추쿠, 은케치
```

---

## 📝 실전 예시

### 예시 1: Qudish 이름

**생성 과정:**
```xml
<prefixes>
  <prefix Name="fa" />
  <prefix Name="ka" />
  <prefix Name="mu" />
</prefixes>
<infixes>
  <infix Name="ra" />
  <infix Name="ga" />
</infixes>
<postfixes>
  <postfix Name="mut" />
  <postfix Name="bas" />
  <postfix Name="shur" />
</postfixes>
```

**가능한 조합:**
```
fa + ra + mut = Faramut (파라무트)
fa + ra + bas = Farabas (파라바스)
fa + ga + mut = Fagamut (파가무트)
ka + ra + mut = Karamut (카라무트)
mu + (없음) + shur = Mushur (무슈르)
```

### 예시 2: 지역 이름

**원문:**
```xml
<namestyle Name="Qudish Site">
  <templates>
    <template Name="Ruins *Name*" />
  </templates>
</namestyle>
```

**생성:**
```
Ruins Faramut → 파라무트 폐허
Ruins Kagabas → 카가바스 폐허
```

**번역:**
```xml
<templates>
  <template Name="*Name* 폐허" />
</templates>
```

---

## 🧪 테스트 시나리오

### 테스트 1: 술탄 이름
```
1. 새 게임 시작 (5회)
2. 역사 화면 확인
3. 술탄 이름 확인
```

**확인 사항:**
- ✅ 매번 다른 이름
- ✅ 발음 가능
- ✅ 한글 표기 가능

### 테스트 2: NPC 이름
```
1. 게임 진행
2. 여러 NPC 만남
3. 이름 확인
```

**확인 사항:**
- ✅ 고유한 이름
- ✅ 문화별 스타일 유지

---

## 📊 진행 상황 추적

### 체크리스트

```markdown
## Naming.xml 절차적 생성 전략

### 방법 결정 (1시간)
- [ ] 음차 vs 재구축 결정
- [ ] 음차 선택 (권장)

### 음차 적용 (1일)
- [ ] 게임 테스트
- [ ] 이름 생성 확인
- [ ] 발음 가능성 확인

### 선택적: 지역명 템플릿 번역 (1일)
- [ ] "Ruins *Name*" → "*Name* 폐허"
- [ ] "Desert *Name*" → "*Name* 사막"
```

---

## 🎓 절차적 생성 이해하기

### 생성 흐름

```
1. 게임 시작
   ↓
2. Naming.xml 로드
   ↓
3. 문화 선택 (예: Qudish)
   ↓
4. 랜덤 음절 선택
   - prefix: "fa"
   - infix: "ra"
   - postfix: "mut"
   ↓
5. 조합
   - "Faramut"
   ↓
6. 이름 완성
```

---

## 🔗 관련 문서

- **문서 01:** HistorySpice.json (술탄 이름 사용)
- **문서 04:** Conversations.xml (NPC 이름 표시)

---

**작성일:** 2026-01-13  
**절차적 생성 관여도:** ⭐⭐⭐⭐⭐  
**권장 전략:** 음차 (번역 불필요)  
**예상 완료:** 1-2일
