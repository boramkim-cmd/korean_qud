# 미번역 시스템 전반 분석

> 작성: 2026-01-31 | 목적: 개별 케이스가 아닌 구조적 공백 식별

---

## 1. 현재 번역 아키텍처 개요

```
게임 영어 텍스트
    │
    ├─ [Harmony Patch 19개] ─ 게임 메서드 후킹
    │      │
    │      ├─ DisplayName (아이템/생물 이름)
    │      ├─ Description (설명)
    │      ├─ UI 텍스트 (메뉴, 옵션, 팝업)
    │      ├─ 캐릭터 생성
    │      ├─ 메시지 로그
    │      └─ 인벤토리 카테고리 헤더
    │
    └─ [번역 파이프라인]
           │
           ├─ 1순위: display_lookup (2,764개 XML DisplayName → 한글)
           │     └─ StripSuffixFast로 스탯 제거 후 매칭
           │
           ├─ 2순위: fastCache (blueprint별 Names 딕셔너리)
           │     └─ objects.json에서 로드 (color tag 포함 키)
           │
           └─ 3순위: Pipeline (DirectMatch → PrefixSuffix → Pattern → Fallback)
                 └─ Names 딕셔너리 기반 다단계 매칭
```

---

## 2. 미번역 유형 분류 (6가지 구조적 원인)

### 유형 A: Harmony 패치 부재 — 후킹 자체가 없음

**증상:** 게임 텍스트가 번역 파이프라인에 아예 도달하지 않음

| 미번역 텍스트 | 카테고리 | 원인 |
|---|---|---|
| `Left Arm`, `Right Hand` | 장비 슬롯 이름 | 인게임 장비 화면 미패치 (캐릭생성만 존재) |
| `Worn on Hands`, `Worn on Back` | 장비 위치 | 동일 |
| `Left Missile Weapon`, `Right Missile Weapon` | 장비 슬롯 | 동일 |
| `show cybernetics` | 액션 UI | 하단 바 액션 문자열 미패치 |
| `Set Primary Limb` | 액션 UI | 동일 |
| `Show Tooltip` | 액션 UI | 동일 |
| `navigation` | 하단 바 UI | 동일 |
| `Perfect` | 아이템 품질 | 품질 수식어 미패치 |

**분석:**
- 현재 19개 패치 파일이 존재하지만, 인게임 장비 화면의 body part slot 렌더링과 하단 액션 바는 후킹되지 않음
- 캐릭터 생성의 `SlotTranslations`에 7개 슬롯만 번역 (Face, Head, Body, Back, Arm, Hands, Feet)
- 인게임에서는 이 딕셔너리가 사용되지 않음

**수정 범위:** 신규 Harmony 패치 개발 필요 (중간~대규모)

---

### 유형 B: Color Tag 키 불일치 — 데이터 경로에서 체계적 매칭 실패

**증상:** display_lookup 경로에서는 번역되지만, objects.json/Pipeline 경로에서 실패

**근본 원인:**
```
빌드 시:
  auto_generated.json 키: "congealed &Ysalve"  (color tag 포함)
  → objects.json에 그대로 복사

  display_lookup.json: normalize_for_lookup() → "congealed salve"  (strip됨)

런타임:
  게임 입력: "congealed salve"  (color tag 없음)

  display_lookup 경로: "congealed salve" → 매칭 ✓ (키가 strip됨)
  fastCache 경로:      "congealed salve" vs "congealed &Ysalve" → 매칭 ✗
  Pipeline 경로:       DirectMatchHandler도 키를 strip 안 함 → 매칭 ✗
```

**영향 범위:** auto_generated.json에서 `&` 포함 키는 현재 1개로 확인되나, 다른 JSON 파일에서도 `{{color|text}}` 형식 키가 존재할 수 있음. 체계적 점검 필요.

**코드 위치:**
- `JsonRepository.cs` lines 570-577: Names 로딩 시 키 strip 안 함
- `DirectMatchHandler.cs` lines 57-68: strip된 입력으로 조회하지만, 딕셔너리 키는 원본

**수정 범위:** JsonRepository 로딩 시 Names 키에서 color tag strip 추가 (소규모)

---

### 유형 C: StripSuffixFast 미처리 — 스탯 패턴 제거 실패

**증상:** 스탯이 붙은 아이템 이름에서 base name 추출 실패 → display_lookup 매칭 실패

**이번 세션 수정 사항:**
- step 3 (유니코드 마커)과 step 4 (평문 숫자)를 통합 루프로 병합
- `♦2 \t-3` 같은 혼합 패턴 처리 가능

**확인 필요:**
- 게임 재시작 후 `iron buckler ♦2 ○-3` 등이 해결되는지
- display_lookup.json에 `"iron buckler": "철 버클러"` 존재 확인됨
- 이론적으로 StripSuffixFast가 `♦2 ○-3`을 정상 제거하면 매칭되어야 함

**수정 범위:** 이미 구현 완료, 게임 테스트로 검증

---

### 유형 D: 절차적 생성 콘텐츠 — 번역 데이터 자체 불가

**증상:** 마르코프 체인 등으로 런타임 생성된 텍스트

| 텍스트 | 생성 방식 |
|---|---|
| `Have Gazed` | 마르코프 책 제목 |
| `a Gesture-Poem to Say의 Sorrow Once: 완본` | BookTitleTranslator 부분 번역 |

**이번 세션 수정 사항:**
- `IsMixedScript()` 가드 추가 → 영한 혼합 결과 시 원문 유지

**한계:** 절차적 생성 텍스트는 근본적으로 사전 번역 불가. 현실적 목표는 부분 번역(혼합) 방지뿐.

---

### 유형 E: 접미사 번역 경로 미도달 — base name 실패의 연쇄 효과

**증상:** `(unburnt)`, `[1 serving]` 등 접미사 데이터는 존재하나 번역 안 됨

**메커니즘:**
```
"응고된 salve [1 serving]"
  → base name 매칭 실패 (유형 B: color tag 키 불일치)
  → PrefixSuffixHandler 폴스루
  → prefix "congealed" → "응고된" 매칭
  → noun "salve" 매칭 실패 (별도 noun 등록 안 됨)
  → suffix "[1 serving]" 처리 경로 미도달
```

**핵심:** 접미사 번역 시스템 자체는 동작하나, base name 매칭이 선행 실패하면 접미사까지 연쇄 실패.

`_suffixes.json` 에 정의된 번역:
- 상태 38개: `[empty]`, `(unburnt)→(미사용)`, `[sealed]` 등
- 액체 19종: `fresh water→신선한 물` 등
- of 패턴 58개, 신체부위 71개

**수정 범위:** 유형 B 해결 시 자동 해결

---

### 유형 F: 데이터 누락 — 번역 자체가 없음

**증상:** LOCALIZATION JSON에 해당 아이템 항목 없음

**예시:**
- `Iron Buckler`, `Wooden Buckler` → auto_generated.json에 미등재 (Steel Buckler만 있음)
- `Borderlands Revolver` (chrome revolver) → auto_generated.json에 미등재

**하지만:** display_lookup.json에는 존재함 (`"iron buckler": "철 버클러"`).
이는 빌드 스크립트가 XML에서 DisplayName을 추출하여 별도 매핑하기 때문.

**결론:** display_lookup 경로가 정상 작동하면 이 아이템들은 번역됨. objects.json에 없어도 display_lookup에서 커버.

---

## 3. 경로별 커버리지 매트릭스

```
                    display_lookup  fastCache/objects.json  Pipeline
                    (2,764개)       (4,457 blueprint)       (패턴 기반)
iron buckler        ✓ 키 존재       ✗ 미등재                ✗ 미도달
wooden buckler      ✓ 키 존재       ✗ 미등재                ✗ 미도달
chrome revolver     ✓ 키 존재       ✗ 미등재                ✗ 미도달
congealed salve     ✓ 키 존재       ✓ 등재 (키 불일치)      ✓ (키 불일치로 실패)
leather moccasins   ? 확인 필요     ? 확인 필요             ?
머스킷(musket)      ✓ 번역됨       -                       -
```

**핵심 인사이트:** display_lookup이 1순위로 실행되고 데이터도 존재하므로, StripSuffixFast만 정상 작동하면 대부분 해결. 문제는 StripSuffixFast 수정이 **이번 배포 후 게임 미재시작** 상태일 가능성.

---

## 4. 추가 조사 필요 항목

### 4-1. 게임 재시작 후 재검증
- [ ] buckler, revolver, rifle → StripSuffixFast 수정 효과 확인
- [ ] congealed salve → display_lookup 경로로 번역되는지
- [ ] 마르코프 책 → IsMixedScript 가드 효과 확인
- [ ] `(unburnt)` → suffix 번역 경로 도달하는지

### 4-2. Color Tag 키 전수 조사
- [ ] 모든 LOCALIZATION JSON에서 `&` 또는 `{{` 포함 키 목록 추출
- [ ] 각 키가 display_lookup에서 정상 커버되는지 교차 확인
- [ ] JsonRepository 로딩 시 키 normalize 방안 설계

### 4-3. 패치 공백 영역 우선순위 결정
- [ ] 장비 슬롯 이름 — 어떤 게임 클래스/메서드를 후킹해야 하는지 조사
- [ ] 하단 액션 바 — 어떤 게임 클래스/메서드를 후킹해야 하는지 조사
- [ ] `Perfect` 등 품질 수식어 — 어디서 생성되는지 조사

### 4-4. display_lookup vs objects.json 역할 명확화
- [ ] display_lookup만으로 충분한 아이템 vs objects.json이 필요한 아이템 구분
- [ ] 두 경로의 우선순위와 fallback 관계 정리
- [ ] 불필요한 중복 제거 또는 보완 전략

### 4-5. PrefixSuffixHandler 부분 번역 방지
- [ ] prefix만 번역되고 noun이 누락되는 케이스 전수 조사
- [ ] 부분 번역 결과가 혼합(영한)이면 원문 유지하는 가드 검토 (BookTitleTranslator와 동일 패턴)

---

## 5. 수정 우선순위 제안

| 순위 | 유형 | 작업 | 효과 |
|------|------|------|------|
| 1 | 검증 | 게임 재시작 후 StripSuffixFast 효과 확인 | buckler/revolver/rifle 해결 여부 |
| 2 | B | JsonRepository 키 color tag strip | salve류 + 연쇄 suffix 해결 |
| 3 | D | IsMixedScript 효과 확인 | 마르코프 책 혼합 방지 |
| 4 | A | 장비 슬롯 패치 개발 | Left Arm 등 해결 |
| 5 | A | 액션 UI 패치 개발 | show cybernetics 등 해결 |
| 6 | E | PrefixSuffix 혼합 방지 가드 | 부분 번역 전반 개선 |
