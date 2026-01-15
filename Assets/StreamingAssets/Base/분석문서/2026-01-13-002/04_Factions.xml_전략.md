# 04. Factions.xml 번역 전략

**우선순위:** 🔴 최우선  
**파일 크기:** 66 KB  
**예상 작업 기간:** 2-3일  
**난이도:** ⭐⭐⭐

---

## 파일 개요
- **경로:** `/StreamingAssets/Base/Factions.xml`
- **내용:** 모든 파벌의 이름, 설명, 관계
- **중요도:** 게임 세계관 및 평판 시스템

---

## 구조 분석

```xml
<factions>
  <faction Name="FactionID" DisplayName="파벌 이름">
    <description>파벌 설명</description>
    <reputation>
      <faction Name="OtherFaction" Value="100"/>
    </reputation>
  </faction>
</factions>
```

---

## 주요 파벌

### 메인 파벌
- **Barathrumites** → 바라스럼추종자
- **Mechanimists** → 메카니카신자
- **Putus Templar** → 푸투스템플러
- **Consortium of Phyta** → 식물 연합
- **Seekers of the Sightless Way** → 맹목의 길 추구자

### 지역 파벌
- **Joppa** → 조파
- **Grit Gate** → 그릿 게이트
- **Six Day Stilt** → 여섯날의 스틸트

---

## 번역 전략

### 1단계: 파벌 목록 작성 (2시간)
- 모든 파벌 ID 추출
- DisplayName 확인

### 2단계: 우선순위 번역 (1일)
- 메인 파벌 먼저
- 지역 파벌
- 적대 파벌

### 3단계: 관계 설명 번역 (1일)
- 각 파벌 간 관계 설명
- 평판 시스템 텍스트

---

## 실전 예시

**원문:**
```xml
<faction Name="Barathrumites" DisplayName="Barathrumites">
  <description>Followers of Barathrum the Old, who seek to understand the ancient technology of Qud.</description>
</faction>
```

**번역:**
```xml
<faction Name="Barathrumites" DisplayName="바라스럼추종자">
  <description>고대 바라스럼의 추종자들로, 쿼드의 고대 기술을 이해하고자 한다.</description>
</faction>
```

---

## 관련 문서
- 문서 01: Conversations.xml
- 문서 03: HistorySpice.json

---

**예상 완료:** 2-3일
