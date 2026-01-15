# 01. Conversations.xml 번역 전략

**우선순위:** 🔴 최우선 (Critical)  
**파일 크기:** 647 KB  
**예상 작업 기간:** 30-40일  
**난이도:** ⭐⭐⭐⭐⭐

---

## 📋 파일 개요

### 파일 정보
- **경로:** `/StreamingAssets/Base/Conversations.xml`
- **내용:** 게임 내 모든 NPC 대화, 퀘스트 대화, 동적 대화
- **중요도:** 게임 플레이의 핵심 (스토리, 퀘스트, 세계관)

### 왜 최우선인가?
1. 플레이어가 가장 많이 접하는 텍스트
2. 게임 스토리와 세계관 전달
3. 퀘스트 진행에 필수
4. 번역 품질이 게임 경험에 직접 영향

---

## 🔍 구조 분석

### XML 기본 구조

```xml
<conversations>
  <conversation ID="ConversationID">
    <node ID="NodeID">
      <text>대화 텍스트</text>
      <choice>
        <text>선택지 텍스트</text>
        <target>TargetNodeID</target>
      </choice>
    </node>
  </conversation>
</conversations>
```

### 주요 요소

#### 1. Conversation (대화 그룹)
```xml
<conversation ID="Argyve" SpecialRequirement="None">
  <!-- 여러 node 포함 -->
</conversation>
```

#### 2. Node (대화 노드)
```xml
<node ID="Start">
  <text>안녕하세요, 여행자님.</text>
  <choice>...</choice>
</node>
```

#### 3. Choice (선택지)
```xml
<choice>
  <text>당신은 누구신가요?</text>
  <target>WhoAreYou</target>
  <if>...</if>
</choice>
```

#### 4. 조건문
```xml
<if>{{player.reputation|Barathrumites}} &gt;= 100</if>
<unless>{{player.hasquest|Kith and Kin}}</unless>
```

### 동적 텍스트 변수

```xml
<text>안녕하세요, =player.formalAddressTerm=.</text>
<text>당신은 <spice.gossip.leadIns.!random> 들었나요?</text>
<text>{{player.name}}님, 환영합니다.</text>
```

---

## 🎯 번역 전략

### 1단계: 구조 파악 (1-2일)

**작업:**
- 전체 파일 스캔
- conversation 개수 확인
- 주요 NPC 목록 작성
- 변수 사용 패턴 파악

**도구:**
```bash
# conversation 개수 확인
grep -c '<conversation' Conversations.xml

# 고유 ID 목록
grep 'ID="' Conversations.xml | sort | uniq
```

### 2단계: 우선순위 분류 (1일)

**최우선 대화:**
- 메인 퀘스트 관련 NPC
- 튜토리얼 NPC (Mehmet, Argyve)
- 주요 마을 NPC (Joppa, Six Day Stilt)

**중간 우선순위:**
- 서브 퀘스트 NPC
- 상인 NPC
- 파벌 대표 NPC

**낮은 우선순위:**
- 랜덤 NPC
- 숨겨진 대화
- 이벤트성 대화

### 3단계: 용어집 구축 (2-3일)

**필수 용어:**
```
Qud → 쿼드
Sultan → 술탄
Barathrumites → 바라스러미트
Mechanimists → 메카니미스트
Putus Templar → 푸투스템플러
Six Day Stilt → 식스데이 스틸트
Grit Gate → 그릿 게이트
```

**변수 용어:**
```
=player.name= → 플레이어 이름
=player.formalAddressTerm= → 존칭
{{player.reputation|faction}} → 평판
```

### 4단계: 번역 실행 (25-35일)

**일일 목표:**
- 하루 20-30개 conversation
- 품질 > 속도
- 매일 용어집 업데이트

**번역 순서:**
1. Mehmet (튜토리얼)
2. Argyve (메인 퀘스트)
3. Joppa 마을 NPC
4. 메인 퀘스트 라인
5. 서브 퀘스트
6. 기타 NPC

---

## ⚠️ 주의사항

### 1. 변수 문법 절대 변경 금지

**❌ 잘못된 예:**
```xml
<!-- 원본 -->
<text>Hello, =player.name=.</text>

<!-- 잘못됨 -->
<text>안녕하세요, =플레이어.이름=.</text>
```

**✅ 올바른 예:**
```xml
<text>안녕하세요, =player.name=.</text>
```

### 2. XML 태그 및 속성 유지

**❌ 잘못된 예:**
```xml
<!-- 원본 -->
<choice ID="Choice1" GotoID="Node2">
  <text>Yes</text>
</choice>

<!-- 잘못됨 -->
<선택지 ID="Choice1" GotoID="Node2">
  <텍스트>예</텍스트>
</선택지>
```

**✅ 올바른 예:**
```xml
<choice ID="Choice1" GotoID="Node2">
  <text>예</text>
</choice>
```

### 3. 조건문 로직 보존

**원본:**
```xml
<if>{{player.reputation|Barathrumites}} &gt;= 100</if>
```

**번역 시:**
- `&gt;`, `&lt;`, `&amp;` 등 HTML 엔티티 유지
- 숫자 값 변경 금지
- 파벌명 등 식별자 유지

### 4. 캐릭터 톤 일관성

**예시:**

**Mehmet (친근한 노인):**
```xml
<text>자네, 처음 보는 얼굴이군. 조파에 온 걸 환영하네.</text>
```

**Argyve (학자, 격식):**
```xml
<text>당신을 만나게 되어 영광입니다. 저는 아르기브라고 합니다.</text>
```

**Warden Ualraig (군인, 직설적):**
```xml
<text>무슨 일이지? 빨리 말해.</text>
```

---

## 🔧 기술적 고려사항

### 한글 조사 처리

**문제:**
```xml
<text>You found <item.name>.</text>
→ "당신은 <item.name>을/를 발견했다"
```

**해결 방안 1: 조사 회피**
```xml
<text><item.name> 발견</text>
<text><item.name>의 발견</text>
<text><item.name>, 그것을 발견했다</text>
```

**해결 방안 2: 엔진 지원 확인**
```xml
<!-- 엔진이 지원한다면 -->
<text>당신은 <item.name.josa_eul_reul> 발견했다</text>
```

**해결 방안 3: 후처리 스크립트**
```python
# 번역 후 Python으로 조사 자동 추가
def add_josa(text):
    # 받침 확인 후 조사 추가
    pass
```

### 동적 대화 처리

**HistorySpice.json 연계:**
```xml
<text><spice.gossip.leadIns.!random> <spice.gossip.twoFaction.!random></text>
```

**전략:**
1. HistorySpice.json 먼저 번역 (문서 03 참조)
2. 동적 대화는 나중에 테스트
3. 게임 실행하여 확인

---

## 📝 실전 예시

### 예시 1: 기본 대화

**원문:**
```xml
<conversation ID="Mehmet">
  <node ID="Start">
    <text>Greetings, wayfarer. Welcome to Joppa.</text>
    <choice>
      <text>Who are you?</text>
      <target>WhoAreYou</target>
    </choice>
    <choice>
      <text>Goodbye.</text>
      <target>End</target>
    </choice>
  </node>
  
  <node ID="WhoAreYou">
    <text>I am Mehmet, elder of this village.</text>
    <choice>
      <text>Tell me about Joppa.</text>
      <target>AboutJoppa</target>
    </choice>
  </node>
</conversation>
```

**번역:**
```xml
<conversation ID="Mehmet">
  <node ID="Start">
    <text>안녕하시오, 여행자. 조파에 온 걸 환영하오.</text>
    <choice>
      <text>당신은 누구신가요?</text>
      <target>WhoAreYou</target>
    </choice>
    <choice>
      <text>안녕히 계세요.</text>
      <target>End</target>
    </choice>
  </node>
  
  <node ID="WhoAreYou">
    <text>나는 메흐메트, 이 마을의 장로라오.</text>
    <choice>
      <text>조파에 대해 말씀해 주세요.</text>
      <target>AboutJoppa</target>
    </choice>
  </node>
</conversation>
```

### 예시 2: 변수 포함 대화

**원문:**
```xml
<text>Ah, =player.name=! Good to see you again.</text>
<text>Your reputation with the {{faction.name}} is {{player.reputation|faction}}.</text>
```

**번역:**
```xml
<text>아, =player.name=! 다시 만나서 반갑소.</text>
<text>{{faction.name}}에 대한 당신의 평판은 {{player.reputation|faction}}입니다.</text>
```

### 예시 3: 조건부 대화

**원문:**
```xml
<choice>
  <text>I have the relic you requested.</text>
  <target>GiveRelic</target>
  <if>{{player.hasitem|Ancient Relic}}</if>
</choice>
```

**번역:**
```xml
<choice>
  <text>요청하신 유물을 가져왔습니다.</text>
  <target>GiveRelic</target>
  <if>{{player.hasitem|Ancient Relic}}</if>
</choice>
```

---

## 🛠️ 권장 도구

### 1. 텍스트 에디터
- **Visual Studio Code**
  - XML 문법 강조
  - 자동 완성
  - 검색/치환 강력

### 2. XML 검증 도구
```bash
# XML 문법 검사
xmllint --noout Conversations.xml
```

### 3. 용어 일관성 검사
```bash
# 특정 용어 검색
grep -n "Barathrumites" Conversations.xml
```

### 4. 백업 스크립트
```bash
# 작업 전 자동 백업
cp Conversations.xml Conversations.xml.backup.$(date +%Y%m%d_%H%M%S)
```

---

## 📊 진행 상황 추적

### 체크리스트 템플릿

```markdown
## Conversations.xml 번역 진행

### Phase 1: 튜토리얼 & 메인 (10일)
- [ ] Mehmet (조파 장로)
- [ ] Argyve (그릿 게이트)
- [ ] Barathrum (바라스럼)
- [ ] Q Girl (큐 걸)

### Phase 2: 주요 마을 (10일)
- [ ] Joppa 마을 NPC (10명)
- [ ] Six Day Stilt NPC (15명)
- [ ] Grit Gate NPC (8명)

### Phase 3: 퀘스트 (10일)
- [ ] 메인 퀘스트 대화
- [ ] 서브 퀘스트 대화

### Phase 4: 기타 (5-10일)
- [ ] 상인 NPC
- [ ] 파벌 대표
- [ ] 랜덤 NPC
```

---

## 🎓 학습 곡선

### 1주차: 적응기
- XML 구조 파악
- 변수 문법 학습
- 첫 10개 conversation 번역
- **속도:** 느림 (하루 5-10개)

### 2-3주차: 숙련기
- 패턴 파악 완료
- 용어집 안정화
- **속도:** 보통 (하루 15-20개)

### 4주차 이후: 전문가
- 빠른 번역
- 높은 품질
- **속도:** 빠름 (하루 25-30개)

---

## 🚨 흔한 실수

### 1. 변수 삭제
**❌ 잘못:**
```xml
<text>Hello, =player.name=.</text>
→ <text>안녕하세요.</text>  <!-- 변수 삭제됨! -->
```

### 2. 태그 닫기 누락
**❌ 잘못:**
```xml
<text>안녕하세요
<choice>...</choice>
```

**✅ 올바름:**
```xml
<text>안녕하세요</text>
<choice>...</choice>
```

### 3. 특수 문자 처리
**❌ 잘못:**
```xml
<if>reputation >= 100</if>  <!-- & 기호 잘못 -->
```

**✅ 올바름:**
```xml
<if>reputation &gt;= 100</if>
```

---

## 📈 품질 관리

### 자체 검수 체크리스트
- [ ] XML 문법 오류 없음
- [ ] 모든 변수 유지됨
- [ ] 캐릭터 톤 일관성
- [ ] 오타 없음
- [ ] 용어 일관성
- [ ] 자연스러운 한글

### 테스트 방법
1. 게임 실행
2. 번역한 NPC와 대화
3. 모든 선택지 확인
4. 변수 정상 작동 확인

---

## 📅 예상 일정

| 주차 | 작업 내용 | 목표 |
|-----|----------|------|
| 1주 | 구조 파악 + 튜토리얼 | 50개 |
| 2주 | 메인 퀘스트 NPC | 100개 |
| 3주 | 주요 마을 NPC | 150개 |
| 4주 | 서브 퀘스트 | 100개 |
| 5-6주 | 기타 NPC | 200개 |

**총 예상:** 600개 conversation, 30-40일

---

## 🔗 관련 문서

- **문서 02:** Quests.xml (퀘스트 텍스트)
- **문서 03:** HistorySpice.json (동적 대화 소스)
- **문서 04:** Factions.xml (파벌 정보)

---

**작성일:** 2026-01-13 09:35  
**우선순위:** 🔴 최우선  
**다음 문서:** 02_Quests.xml_전략.md
