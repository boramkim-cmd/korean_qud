# 캐릭터 생성 화면 한글화 이슈 수정 보고서

**날짜**: 2026년 1월 19일

---

## 1. 문제 현상

스크린샷에서 발견된 문제:
- 캐릭터 생성 화면에서 일부 텍스트가 영어로 표시됨
- 영어와 한글이 중복으로 표시되는 현상 발생
- 스탯, 평판, 게임 모드 설명 등이 번역되지 않음
- 화면 제목(breadcrumb)과 하단 메뉴가 번역되지 않음

### 구체적 예시
| 화면 | 영어 원문 | 예상 한글 |
|------|----------|----------|
| 게임 모드 | `character creation` | 캐릭터 생성 |
| 게임 모드 | `:choose game mode:` | 게임 모드 선택 |
| 게임 모드 | `Permadeath: lose your character when you die.` | 번역 누락 |
| 게임 모드 | `[¬] Randomize Selection`, `[Delete] Reset Selection` | 무작위 선택, 선택 초기화 |
| 유목민(Nomad) | `+2 Toughness` | `지구력 +2` (이미 존재하나 표시 안됨) |
| 유목민 | `+200 reputation with the Issachari tribe` | 이사카리 부족 평판 +200 |
| 총잡이(Gunslinger) | `+2 Agility` | `민첩성 +2` |
| 백발노인(Greybeard) | `-1 Strength`, `+3 Willpower` | `힘 -1`, `의지력 +3` |

---

## 2. 원인 분석

### 2.1 스탯 형식 불일치
- **게임 원본**: `"+2 Toughness"` (숫자가 앞)
- **JSON 데이터**: `"Toughness +2"` (텍스트가 앞)
- 정규식 패턴이 한 방향만 지원하여 매칭 실패

### 2.2 StructureTranslator 중복 필터링 실패
- `GetCombinedLongDescription()` 메서드에서 중복 라인 제거 로직이 형식 차이로 인해 작동하지 않음
- 결과: 영어 원문 + 한글 번역이 둘 다 표시됨

### 2.3 평판/게임 모드 번역 키 누락
- `modes.json`에 개별 문장 키 누락
- `ui.json`의 Reputations 섹션에 일부 평판 항목 누락

---

## 3. 수정 내용

### 3.1 modes.json 번역 추가
**파일**: [LOCALIZATION/CHARGEN/modes.json](../LOCALIZATION/CHARGEN/modes.json)

```json
"permadeath: lose your character when you die.": "영구적 죽음: 캐릭터 사망 시 해당 캐릭터를 잃게 됩니다.",
"permadeath: you lose your character when you die.": "영구적 죽음: 캐릭터 사망 시 해당 캐릭터로 더 이상 플레이할 수 없습니다."
```

### 3.2 ChargenTranslationUtils.cs 수정
**파일**: [Scripts/99_Utils/99_00_02_ChargenTranslationUtils.cs](../Scripts/99_Utils/99_00_02_ChargenTranslationUtils.cs)

변경 사항:
1. **직접 평판 조회 추가** (2.5단계)
   - `chargen_ui.Reputations`에서 정확한 키 매칭 시도
2. **양방향 스탯 형식 지원** (4.5단계)
   - 기존: `"+2 Toughness"` 패턴만 지원
   - 추가: `"Toughness +2"` 패턴도 지원
3. **출력 형식 통일**
   - 한글 출력: `"지구력 +2"` (텍스트 + 숫자)
4. **Breadcrumb Subtitle 번역 추가**
   - Title과 Subtitle 모두 번역 처리
   - `:choose game mode:` 등의 부제목 번역 지원

### 3.3 StructureTranslator.cs 수정
**파일**: [Scripts/99_Utils/99_00_03_StructureTranslator.cs](../Scripts/99_Utils/99_00_03_StructureTranslator.cs)

변경 사항:
1. **`NormalizeLine()` 메서드 추가**
   - 색상 태그 제거
   - 불렛 포인트 제거
   - 스탯 형식 정규화: `"+2 Toughness"` ↔ `"Toughness +2"` → `"toughness 2"`
2. **중복 필터링 개선**
   - 정규화된 텍스트로 비교하여 형식 차이 무시
   - `HashSet` 기반 빠른 조회

### 3.4 stats.json 스탯 이름 번역 추가
**파일**: [LOCALIZATION/CHARGEN/stats.json](../LOCALIZATION/CHARGEN/stats.json)

```json
"Attribute Names": {
  "Strength": "힘",
  "Agility": "민첩성",
  "Toughness": "지구력",
  "Intelligence": "지능",
  "Willpower": "의지력",
  "Ego": "자아"
}
```

### 3.5 평판 및 팩션 번역 확장
**파일**: 
- [LOCALIZATION/CHARGEN/ui.json](../LOCALIZATION/CHARGEN/ui.json) - Reputations 섹션 확장 + Bottom Bar 확장
- [LOCALIZATION/CHARGEN/factions.json](../LOCALIZATION/CHARGEN/factions.json) - 팩션 이름 추가

추가된 평판 번역:
- `"+200 reputation with mysterious strangers"` → `"의문의 이방인(Mysterious Strangers) 평판 +200"`
- `"+100 reputation with bears"` → `"곰 세력 평판 +100"`
- 기타 다수의 팩션 평판 추가

추가된 하단 메뉴 번역:
- `"Randomize Selection"` → `"무작위 선택"`
- `"Reset Selection"` → `"선택 초기화"`
- `"Delete"` → `"삭제"`
- `"Quickstart"` → `"빠른 시작"`

---

## 4. 수정된 파일 목록

| 파일 | 변경 유형 |
|------|----------|
| `LOCALIZATION/CHARGEN/modes.json` | 번역 키 추가 |
| `LOCALIZATION/CHARGEN/stats.json` | 스탯 이름 번역 추가 |
| `LOCALIZATION/CHARGEN/ui.json` | 평판 번역 확장 |
| `LOCALIZATION/CHARGEN/factions.json` | 팩션 이름 추가 |
| `Scripts/99_Utils/99_00_02_ChargenTranslationUtils.cs` | 양방향 스탯 매칭 + 직접 평판 조회 |
| `Scripts/99_Utils/99_00_03_StructureTranslator.cs` | 중복 필터링 개선 + NormalizeLine 추가 |

---

## 5. 예상 결과

수정 후 예상되는 화면 표시:

### 게임 모드 선택 화면
- 제목: `character creation` → `캐릭터 생성`
- 부제목: `:choose game mode:` → `:게임 모드 선택:`
- 설명: `Permadeath: lose your character when you die.` → `영구적 죽음: 캐릭터 사망 시 해당 캐릭터를 잃게 됩니다.`
- 하단 메뉴: `[¬] 무작위 선택 [Delete] 선택 초기화 . [디버그] 빠른 시작`

### 유목민(Nomad) 직업
```
지구력 +2
여행술
야생 지식: 소금 사구
수확
마음의 나침반
단련된 피부
재활용 슈트를 착용한 상태로 시작합니다
```

### 총잡이(Gunslinger) 직업
```
민첩성 +2
권총
아킴보
```

### 백발노인(Greybeard) 직업
```
의지력 +3
힘 -1
곤봉
질책
피부 각질화
```

---

## 6. 테스트 권장 사항

1. 게임 빌드 후 캐릭터 생성 화면 진입
2. 각 게임 모드 선택 시 설명 확인
3. 모든 직업(Calling) 선택 시 스탯/스킬/평판 표시 확인
4. 영어 원문이 중복 표시되지 않는지 확인

---

## 7. 향후 개선 사항

1. **자동 테스트 도입**: 번역 키 존재 여부 자동 검증
2. **형식 일관성**: JSON의 `leveltext` 형식을 게임 원본과 동일하게 유지
3. **색상 태그 통일**: `{{B|...}}` vs `{{b|...}}` 대소문자 일관성 확보
