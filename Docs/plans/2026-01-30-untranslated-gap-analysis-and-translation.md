# 누락 번역 파악 및 번역 작업 계획

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** asset_index.json 기준 미번역 2,056개 항목을 우선순위별로 번역하여 LOCALIZATION/ JSON 파일에 추가한다 (Conversation.xml 제외).

**Architecture:** 우선순위(P1→P8) 순서로 진행. 각 단계에서 미번역 항목을 asset_index에서 추출 → 한국어 번역 생성 → 기존 JSON 파일에 추가 또는 새 JSON 생성 → `build_asset_index.py --stats`로 커버리지 검증 → 커밋.

**Tech Stack:** Python (tools/), JSON, XML (참조용), `build_asset_index.py`

---

## 현황

- 전체 고유 항목: 2,989
- 번역 완료: 933 (31.2%)
- **미번역: 2,056 (68.8%)**
- 교차 검증: 거짓 양성/음성 0건 확인 완료

---

## Task 1: P1 — ActivatedAbilities (48개) + Mutations (10개) = 58개

**Files:**
- Read: `tools/asset_index.json` (미번역 항목 추출)
- Create: `LOCALIZATION/GAMEPLAY/activated_abilities.json`
- Update: `LOCALIZATION/GAMEPLAY/MUTATIONS/` 하위 JSON (필요시)

**Step 1:** asset_index.json에서 ActivatedAbilities.xml 미번역 48개 추출
```bash
python3 -c "
import json
with open('tools/asset_index.json') as f:
    data = json.load(f)['items']
items = [(k,v) for k,v in data.items() if not v.get('translated') and 'ActivatedAbilities.xml' in v.get('files',[])]
for k,v in sorted(items):
    print(f'{v[\"original\"]}')"
```

**Step 2:** 번역 JSON 파일 생성

ActivatedAbilities는 능력치/스킬 관련 수치 라벨이므로 대부분 게임 메카닉 용어:
```json
{
  "_meta": {
    "description": "Activated ability stat labels and parameters",
    "version": "1.0",
    "created": "2026-01-30"
  },
  "ability_labels": {
    "Additional direct hit damage": "추가 직격 피해",
    "Agility bonus": "민첩 보너스",
    "Ambient light recharge rate": "주변광 충전 속도",
    "Batch size": "배치 크기",
    ...
  }
}
```

각 항목의 `"names"` 딕셔너리에 `영어 원문: 한국어 번역` 형식으로 추가.

**Step 3:** Mutations.xml 미번역 10개 추출 및 번역
- `{{R|Mental Defects}}` → `{{R|정신 결함}}`
- `{{G|Mental Mutations}}` → `{{G|정신 돌연변이}}`
- `{{W|Morphotypes}}` → `{{W|변형체}}`
- `{{R|Physical Defects}}` → `{{R|신체 결함}}`
- `{{G|Physical Mutations}}` → `{{G|신체 돌연변이}}`
- `Round 1 damage` → `1라운드 피해`
- `Round 2 damage` → `2라운드 피해`
- `Round 3 damage` → `3라운드 피해`
- 나머지 2개는 추출 후 확인

기존 LOCALIZATION/GAMEPLAY/MUTATIONS/ 하위 JSON에 추가하거나 새 파일 생성.

**Step 4:** 검증
```bash
python3 tools/build_asset_index.py --stats
```
Expected: ActivatedAbilities 커버리지 100%, Mutations 커버리지 100%

**Step 5:** 커밋
```bash
git add LOCALIZATION/GAMEPLAY/ && git commit -m "feat: translate P1 - ActivatedAbilities (48) + Mutations (10)"
```

---

## Task 2: P2 — Creatures (770개)

**Files:**
- Read: `tools/asset_index.json`
- Update: `LOCALIZATION/OBJECTS/creatures/` 하위 JSON 파일들
- Create: 필요시 새 카테고리 JSON 파일

**Step 1:** 미번역 크리처 770개를 카테고리별로 분류

크리처 이름 패턴 분석:
- **고유명사** (음역): `0lam`, `1-FF`, `Agolgut` 등 → 한글 음역
- **일반 크리처** (의역): `acid weep`, `albino ape` 등 → 한글 번역
- **신체 부위/공격** (의역): `acidic mandibles`, `achromous bite` 등
- **컬러 태그 포함**: `{{G|acid}} weep` → 태그 보존하고 내용만 번역
- **=variable= 패턴**: `=creatureregionadjective= frog` 등 → 패턴 보존

**Step 2:** 기존 creatures/ 하위 JSON 파일 구조 파악
```bash
ls -la LOCALIZATION/OBJECTS/creatures/
```
기존 파일에 항목 추가하거나, 없는 카테고리는 새 파일 생성.

**Step 3:** 배치 번역 — 기존 JSON에 `"names"` 항목 추가

예시 (humanoids/):
```json
"acid weep": "산성 눈물",
"{{G|acid}} weep": "{{G|산성}} 눈물"
```

770개를 한 번에 하기엔 많으므로 하위 카테고리별로 나눠서 진행:
1. humanoids/ (기존 파일 확장)
2. animals/ (기존 파일 확장)
3. insects/
4. plants/
5. robots/
6. oozes/
7. npcs/
8. 나머지 (새 파일)

**Step 4:** 각 하위 카테고리 완료 후 중간 검증
```bash
python3 tools/build_asset_index.py --stats
```

**Step 5:** 커밋 (카테고리별 또는 일괄)
```bash
git add LOCALIZATION/OBJECTS/creatures/ && git commit -m "feat: translate P2 - Creatures batch N"
```

---

## Task 3: P3 — Items (463개)

**Files:**
- Read: `tools/asset_index.json`
- Update: `LOCALIZATION/OBJECTS/items/` 하위 JSON 파일들

**Step 1:** 미번역 아이템 463개를 카테고리별로 분류

아이템 패턴:
- **무기**: weapons/melee/, weapons/ranged/
- **방어구**: armor/
- **소모품**: consumables/
- **도구**: tools/
- **유물**: artifacts/
- **탄약**: ammo/
- **책**: books/ (고유명사 제목 포함)
- **기타**: 그 외

**Step 2:** 기존 items/ 하위 JSON 파일에 번역 추가

예시 (artifacts/):
```json
"{{W|*advertisement*}}": "{{W|*광고*}}",
"3D cobblers": "3D 구두 제조기"
```

**Step 3:** 책 제목 등 고유명사는 음역+의역 혼용
- `Across Moghra'yi, Vol. I: The Sunderlies` → `모그라이 횡단기, 제1권: 선덜리즈`

**Step 4:** 검증
```bash
python3 tools/build_asset_index.py --stats
```

**Step 5:** 커밋
```bash
git add LOCALIZATION/OBJECTS/items/ && git commit -m "feat: translate P3 - Items (463)"
```

---

## Task 4: P4 — Furniture (209개) + Walls (95개) = 304개

**Files:**
- Update: `LOCALIZATION/OBJECTS/furniture/` 하위 JSON
- Update: `LOCALIZATION/OBJECTS/terrain/` (walls 포함)

**Step 1:** 미번역 항목 추출 및 분류

**Step 2:** 번역 추가
- Furniture: 가구/설비 이름 (`bookshelf` → `책장`, `forge` → `용광로`)
- Walls: 벽/장벽 이름 (`brick wall` → `벽돌 벽`)

**Step 3:** 검증 및 커밋
```bash
python3 tools/build_asset_index.py --stats
git add LOCALIZATION/OBJECTS/ && git commit -m "feat: translate P4 - Furniture (209) + Walls (95)"
```

---

## Task 5: P5 — ZoneTerrain (76개) + WorldTerrain (22개) = 98개

**Files:**
- Update: `LOCALIZATION/OBJECTS/terrain/`

**Step 1:** 미번역 지형 항목 추출

**Step 2:** 번역 추가
- `salt marsh` → `소금 습지`
- `desert canyon` → `사막 협곡`

**Step 3:** 검증 및 커밋
```bash
python3 tools/build_asset_index.py --stats
git add LOCALIZATION/OBJECTS/terrain/ && git commit -m "feat: translate P5 - Terrain (98)"
```

---

## Task 6: P6 — Factions (37개) + ChiliadFactions (30개) = 67개

**Files:**
- Update: `LOCALIZATION/_SHARED/factions.json` 또는 새 파일
- Update: `LOCALIZATION/CHARGEN/factions.json`

**Step 1:** 팩션 이름 추출

**Step 2:** 번역 — 고유명사는 음역, 일반명은 의역
- `Mechanimists` → `메카니미스트` (음역)
- `Farmers' Guild` → `농부 길드` (의역)

**Step 3:** 검증 및 커밋
```bash
python3 tools/build_asset_index.py --stats
git add LOCALIZATION/ && git commit -m "feat: translate P6 - Factions (67)"
```

---

## Task 7: P7 — HiddenObjects (213개)

**Files:**
- Update: `LOCALIZATION/OBJECTS/hidden.json`

**Step 1:** 미번역 항목 추출 — `=variable=` 패턴 분석

`=variable=` 패턴은 동적 치환이므로 번역 불가한 것과 가능한 것 분류:
- 번역 가능: 고정 텍스트 부분 (`=creatureregionadjective= frog`에서 `frog`)
- 번역 불가: 순수 변수 (`=name=`)

**Step 2:** 번역 가능 항목만 JSON에 추가

**Step 3:** 검증 및 커밋
```bash
python3 tools/build_asset_index.py --stats
git add LOCALIZATION/OBJECTS/ && git commit -m "feat: translate P7 - HiddenObjects (partial)"
```

---

## Task 8: P8 — 나머지 (Foods 22, Data 19, PhysicalPhenomena 19, WishCommands 13, Widgets 12, Worlds 3) = ~88개

**Files:**
- Update: 각 카테고리별 기존 JSON 파일

**Step 1:** 각 소스별 미번역 항목 추출

**Step 2:** 번역 추가
- Foods: 나머지 음식 이름
- Data: 데이터 라벨
- PhysicalPhenomena: 물리 현상 이름
- WishCommands: 디버그 명령어 (번역 필요성 낮음, 선택)
- Widgets: UI 위젯 이름
- Worlds: 세계 이름

**Step 3:** 검증 및 커밋
```bash
python3 tools/build_asset_index.py --stats
git add LOCALIZATION/ && git commit -m "feat: translate P8 - remaining items (88)"
```

---

## Task 9: 최종 검증 및 리포트 업데이트

**Files:**
- Run: `tools/build_asset_index.py --stats`
- Update: `Docs/plans/untranslated_report.md`
- Update: `Docs/04_TODO.md`

**Step 1:** asset_index 재생성
```bash
python3 tools/build_asset_index.py --stats
```
Expected: 커버리지 90%+ (HiddenObjects =variable= 패턴 제외)

**Step 2:** 미번역 리포트 업데이트 — 남은 항목만 표시

**Step 3:** TODO.md P1-P8 상태를 Done으로 업데이트

**Step 4:** 최종 커밋
```bash
git add Docs/ tools/asset_index.json && git commit -m "docs: update translation coverage report - target 90%+"
```

---

## Verification

각 Task 완료 시:
1. `python3 tools/build_asset_index.py --stats` — 해당 XML 소스 커버리지 확인
2. JSON 파일 문법 검증 (`python3 -c "import json; json.load(open('파일'))"`)
3. 기존 번역 깨지지 않았는지 확인 (전체 번역 수 감소 없어야 함)

최종:
- 전체 커버리지 90%+ 달성
- 거짓 양성/음성 0건 유지
- 모든 JSON 파일 valid
