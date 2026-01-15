# 02. Quests.xml 번역 전략

**우선순위:** 🔴 최우선 (Critical)  
**파일 크기:** 27 KB  
**예상 작업 기간:** 1-2일  
**난이도:** ⭐⭐⭐

---

## 📋 파일 개요

### 파일 정보
- **경로:** `/StreamingAssets/Base/Quests.xml`
- **내용:** 모든 퀘스트의 이름, 설명, 목표
- **중요도:** 게임 진행의 핵심

### 왜 최우선인가?
1. 작은 파일 크기 (빠른 완성 가능)
2. 게임 진행에 필수적
3. Conversations.xml과 연계
4. 첫 번째 프로젝트로 적합

---

## 🔍 구조 분석

### XML 기본 구조

```xml
<quests>
  <quest Name="QuestID" DisplayName="퀘스트 이름">
    <text>퀘스트 설명</text>
    <step Name="StepID" XP="100">
      <text>목표 설명</text>
    </step>
  </quest>
</quests>
```

### 주요 요소

#### 1. Quest (퀘스트)
```xml
<quest Name="Kith and Kin" DisplayName="Kith and Kin">
  <text>Find out who killed Keh.</text>
</quest>
```

#### 2. Step (단계)
```xml
<step Name="FindClues" XP="50">
  <text>Search for clues in Bey Lah.</text>
</step>
```

#### 3. 동적 텍스트
```xml
<text>Find the <spice.adjectives.!random> <spice.nouns.!random></text>
```

---

## 🎯 번역 전략

### 1단계: 전체 퀘스트 목록 작성 (1시간)

```bash
# 퀘스트 개수 확인
grep -c '<quest' Quests.xml

# 퀘스트 이름 목록
grep 'Name="' Quests.xml | sed 's/.*Name="\([^"]*\)".*/\1/'
```

### 2단계: 우선순위 분류 (1시간)

**메인 퀘스트:**
- What's Eating the Watervine?
- Kith and Kin
- Tomb of the Eaters
- More Than a Willing Spirit

**서브 퀘스트:**
- Decoding the Signal
- Raising Indrix
- The Earl of Omonporch

**절차적 퀘스트:**
- 동적으로 생성되는 퀘스트

### 3단계: 번역 실행 (6-8시간)

**번역 순서:**
1. 메인 퀘스트 (4-5시간)
2. 서브 퀘스트 (2-3시간)
3. 절차적 퀘스트 템플릿 (1시간)

---

## ⚠️ 주의사항

### 1. DisplayName vs Name

**Name (ID):** 변경 금지
```xml
<quest Name="KithAndKin" DisplayName="...">
```

**DisplayName (표시명):** 번역 대상
```xml
<quest Name="KithAndKin" DisplayName="혈족과 친족">
```

### 2. 변수 유지

**원문:**
```xml
<text>Find <entity.name> in <region.name></text>
```

**번역:**
```xml
<text><region.name>에서 <entity.name>을/를 찾아라</text>
```

### 3. XP 값 유지

```xml
<step Name="Complete" XP="100">
  <!-- XP 값 변경 금지 -->
</step>
```

---

## 📝 실전 예시

### 예시 1: 메인 퀘스트

**원문:**
```xml
<quest Name="WatersEdge" DisplayName="What's Eating the Watervine?">
  <text>Investigate the dying watervine in Joppa.</text>
  <step Name="TalkToMehmet" XP="50">
    <text>Talk to Mehmet about the watervine.</text>
  </step>
  <step Name="FindCause" XP="100">
    <text>Find out what's killing the watervine.</text>
  </step>
</quest>
```

**번역:**
```xml
<quest Name="WatersEdge" DisplayName="워터바인에 무슨 일이?">
  <text>조파의 죽어가는 워터바인을 조사하라.</text>
  <step Name="TalkToMehmet" XP="50">
    <text>메흐메트에게 워터바인에 대해 물어보라.</text>
  </step>
  <step Name="FindCause" XP="100">
    <text>워터바인을 죽이는 원인을 찾아라.</text>
  </step>
</quest>
```

### 예시 2: 절차적 퀘스트

**원문:**
```xml
<quest Name="FindArtifact" DisplayName="Find the <spice.adjectives.!random> <spice.nouns.!random>">
  <text>Seek the legendary artifact in the <region.name>.</text>
</quest>
```

**번역:**
```xml
<quest Name="FindArtifact" DisplayName="<spice.adjectives.!random> <spice.nouns.!random> 찾기">
  <text><region.name>에서 전설적인 유물을 찾아라.</text>
</quest>
```

---

## 🎓 용어 가이드

### 퀘스트 관련 용어

| English | 한글 | 비고 |
|---------|------|------|
| Quest | 퀘스트 | |
| Objective | 목표 | |
| Step | 단계 | |
| Reward | 보상 | |
| XP | 경험치 | |
| Complete | 완료 | |
| Failed | 실패 | |

### 주요 지명

| English | 한글 |
|---------|------|
| Joppa | 조파 |
| Grit Gate | 그릿 게이트 |
| Six Day Stilt | 여섯날의 스틸트 |
| Bey Lah | 베이 라 |
| Tomb of the Eaters | 먹는 자들의 무덤 |

---

## 📊 진행 상황 추적

### 체크리스트

```markdown
## Quests.xml 번역 진행

### 메인 퀘스트 (4-5시간)
- [ ] What's Eating the Watervine?
- [ ] Kith and Kin
- [ ] Tomb of the Eaters
- [ ] More Than a Willing Spirit
- [ ] The Earl of Omonporch

### 서브 퀘스트 (2-3시간)
- [ ] Decoding the Signal
- [ ] Raising Indrix
- [ ] Spread the Word
- [ ] Pax Klanq

### 절차적 퀘스트 (1시간)
- [ ] 동적 퀘스트 템플릿
```

---

## 🔗 관련 문서

- **문서 01:** Conversations.xml (퀘스트 대화)
- **문서 03:** HistorySpice.json (동적 퀘스트 소스)

---

**작성일:** 2026-01-13  
**우선순위:** 🔴 최우선  
**예상 완료:** 1-2일
