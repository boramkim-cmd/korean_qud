# 미번역 항목 완성 구현 플랜

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 남은 미번역 항목을 모두 처리하여 번역 커버리지를 99%+ 로 끌어올린다.

**Architecture:** 3단계 접근 — (1) 진단으로 정확한 갭 파악, (2) 누락 어휘 추가로 복합어 번역기 커버리지 확장, (3) 패턴 번역기가 처리 못하는 카테고리에 JSON 직접 번역 추가. 동적 변수 패턴(`=variable=`)은 C# 코드 수정 필요 여부를 판단 후 결정.

**Tech Stack:** Python (진단 도구), JSON (번역 데이터), C# + Harmony (필요 시 패치)

---

## Task 1: 진단 — 현재 정확한 미번역 현황 파악

**Files:**
- Run: `tools/find_missing_vocab.py`
- Run: `tools/build_asset_index.py --stats`
- Read: `tools/asset_index.json`

**Step 1: 누락 어휘 분석 실행**

Run: `cd /Users/ben/Desktop/qud_korean && python3 tools/find_missing_vocab.py`
Expected: 누락 단어 목록 + 해결 가능한 복합어 수 출력

**Step 2: 에셋 인덱스 통계 확인**

Run: `cd /Users/ben/Desktop/qud_korean && python3 tools/build_asset_index.py --stats`
Expected: 카테고리별 번역/미번역 수 출력

**Step 3: 결과를 기록하고 Task 2 이후 작업 범위 확정**

untranslated_report.md의 숫자와 실제 진단 결과를 대조한다. 리포트 상단 요약("85개 미번역")과 하단 상세 목록(~2,000개) 사이의 불일치를 해소한다.

**Step 4: 커밋**

```bash
git add -A && git commit -m "chore: run diagnostics for untranslated gap analysis"
```

---

## Task 2: 누락 어휘 12개 추가 → 복합어 65개 해결

**Files:**
- Modify: `LOCALIZATION/OBJECTS/items/_nouns.json`
- Modify: `LOCALIZATION/OBJECTS/_vocabulary/modifiers.json`

**Step 1: _nouns.json에 누락 명사 추가**

Task 1 진단 결과에서 나온 누락 명사를 추가한다. 예상 목록:

```json
"baron": "남작",
"cactus": "선인장",
"kudu": "쿠두",
"moa": "모아",
"zebra": "얼룩말",
"consortium": "컨소시엄"
```

기존 JSON 구조의 적절한 섹션(animals, misc 등)에 배치한다.

**Step 2: modifiers.json에 누락 수식어 추가**

```json
"blooming": "꽃피는",
"shading": "그늘진",
"spiny": "가시 돋친"
```

기존 카테고리(appearance 등)에 배치한다.

**Step 3: 복합어 번역 검증**

Run: `cd /Users/ben/Desktop/qud_korean && python3 tools/find_missing_vocab.py`
Expected: 누락 단어 0개, 미번역 복합어 0개

**Step 4: 커밋**

```bash
git add LOCALIZATION/OBJECTS/items/_nouns.json LOCALIZATION/OBJECTS/_vocabulary/modifiers.json
git commit -m "feat: add 12 missing vocabulary words, resolve 65 compound translations"
```

---

## Task 3: 비-오브젝트 카테고리 번역 추가 — Factions

**Files:**
- Modify: `LOCALIZATION/OBJECTS/creatures/_common.json` (또는 별도 factions.json)
- Reference: `untranslated_report.md` Factions 섹션 (37 + 30 = 67개)

**Step 1: 기존 팩션 번역 파일 확인**

프로젝트 내 팩션 번역이 어디에 있는지 확인한다. `LOCALIZATION/` 하위에 factions 관련 JSON이 있는지 검색한다.

**Step 2: Factions.xml 37개 번역 추가**

고유명사는 음역, 일반명사는 의역. 예:
```json
"antelopes": "영양",
"apes": "유인원",
"arachnids": "거미류",
"baboons": "개코원숭이",
"baetyls": "배틸",
"birds": "새",
"cannibals": "식인종",
"cats": "고양이",
"chavvah, the tree of life": "차바, 생명의 나무",
"crabs": "게",
"dogs": "개",
"dromad merchants": "드로마드 상인",
"equines": "말류",
"frogs": "개구리",
"fungi": "균류",
"grazing hedonists": "방목 쾌락주의자",
"gyre wights": "선회 망령",
"hermits": "은둔자",
"highly entropic beings": "고엔트로피 존재",
"hindren of bey lah": "베이 라의 힌드렌",
"naphtaali tribe": "납탈리 부족",
"oozes": "점액",
"pariahs": "추방자",
"roots": "뿌리",
"snapjaws": "스냅조",
"succulents": "다육식물",
"tortoises": "거북",
"trees": "나무",
"trolls": "트롤",
"unshelled reptiles": "껍질 없는 파충류",
"urchins": "성게",
"villagers of ezra": "에즈라 마을 주민",
"villagers of kyakukya": "키아쿠키아 마을 주민",
"vines": "덩굴",
"winged mammals": "날개 달린 포유류",
"worms": "벌레",
"denizens of the yd freehold": "이드 자유보유지 주민"
```

**Step 3: ChiliadFactions.xml 30개 번역 추가**

```json
"algae": "조류",
"bacilli": "간균",
"batfolk": "박쥐족",
"cocci": "구균",
"elephantines": "코끼리족",
"foxen": "여우족",
"gelatinous polyhedra": "젤라틴 다면체",
"grandchildren of mamon": "마몬의 후예",
"halophiles": "호염균",
"jellies": "젤리",
"knights liminal": "경계의 기사단",
"methanogens": "메탄생성균",
"mildews": "흰곰팡이",
"molds": "곰팡이",
"mushrooms": "버섯",
"quetzal council": "케찰 의회",
"rusts": "녹균",
"slimes": "슬라임",
"sludges": "슬러지",
"smuts": "깜부기균",
"snailfolk": "달팽이족",
"soups": "수프",
"spirilla": "나선균",
"spirochaetes": "스피로헤타",
"splayed crystal-things that glint": "반짝이는 벌어진 수정체",
"thermoacidophiles": "고온산성균",
"vibrios": "비브리오균",
"walking ibes": "걸어다니는 이베",
"yanshufim": "얀슈핌",
"yeasts": "효모"
```

**Step 4: C# 코드에서 팩션 번역이 로드되는지 확인**

팩션 이름이 실제로 번역 엔진을 통해 치환되는 경로를 확인한다. 만약 Harmony 패치가 없다면 추가 필요.

**Step 5: 커밋**

```bash
git add LOCALIZATION/
git commit -m "feat: add Korean translations for 67 faction names"
```

---

## Task 4: 비-오브젝트 카테고리 번역 — PhysicalPhenomena, Data, Widgets, Worlds

**Files:**
- Modify/Create: 해당 카테고리 JSON 파일들

**Step 1: PhysicalPhenomena 19개 번역**

```json
"confusion gas": "혼란 가스",
"corrosive gas": "부식성 가스",
"cryogenic mist": "극저온 안개",
"empty space": "빈 공간",
"freezing mist": "동결 안개",
"fungal spores": "균류 포자",
"glitter dust": "반짝이 먼지",
"kindled flame": "피워진 불꽃",
"miasma": "미아즈마",
"normality field": "정상성 장",
"normality gas": "정상성 가스",
"osseous ash": "뼈 재",
"pool": "웅덩이",
"scalding steam": "뜨거운 증기",
"shimmering heat": "아지랑이",
"shimmering sphere": "빛나는 구체",
"sleep gas": "수면 가스",
"steam": "증기",
"stun gas": "기절 가스"
```

**Step 2: Data.xml 19개 번역**

```json
"bizarre contraption": "기이한 장치",
"cobalt tube": "코발트 튜브",
"gold-flecked tube": "금박 튜브",
"jewelry": "장신구",
"milky tube": "유백색 튜브",
"mossy tube": "이끼 낀 튜브",
"muddy tube": "흙투성이 튜브",
"odd trinket": "이상한 장식품",
"platinum tube": "백금 튜브",
"rosey tube": "장밋빛 튜브",
"smokey tube": "연기빛 튜브",
"strange furniture": "이상한 가구",
"strange piece of meat": "이상한 고기 조각",
"strange plant": "이상한 식물",
"strange tubes": "이상한 튜브들",
"tool": "도구",
"turquoise tube": "청록색 튜브",
"violet tube": "보라색 튜브",
"weird artifact": "기이한 유물"
```

**Step 3: Widgets.xml 12개 번역**

```json
"arsplice hyphae": "아스플라이스 균사",
"coral polyp": "산호 폴립",
"hut": "오두막",
"known modifications": "알려진 개조",
"machine press": "기계 프레스",
"medical bay": "의료실",
"palladium strut": "팔라듐 버팀대",
"palladium strut with coral growth": "산호가 자란 팔라듐 버팀대",
"tent": "천막",
"village mill": "마을 방앗간",
"watervine patch": "수초 밭",
"workshop": "작업장"
```

**Step 4: Worlds.xml 3개 번역**

```json
"inside": "내부",
"the north sheva": "북부 셰바",
"thin world": "얇은 세계"
```

**Step 5: 커밋**

```bash
git add LOCALIZATION/
git commit -m "feat: add translations for phenomena, data, widgets, worlds (53 items)"
```

---

## Task 5: 비-오브젝트 카테고리 번역 — WishCommands, Mutations 카테고리명

**Files:**
- Modify/Create: 해당 JSON 파일

**Step 1: WishCommands 13개 번역**

```json
"calm creatures": "생물 진정시키기",
"cure fungal infection": "균류 감염 치료",
"fast-forward to tomb of the eaters": "먹는 자들의 무덤으로 빠르게 이동",
"gain 25,000 xp": "경험치 25,000 획득",
"spawn a can of spray-a-brain": "스프레이-어-브레인 캔 생성",
"spawn a dreamcrungle": "드림크런글 생성",
"spawn a floating glowsphere": "떠다니는 발광구 생성",
"spawn an ubernostrum tonic (regrows limbs)": "우버노스트럼 토닉 생성 (사지 재생)",
"spawn rare liquids": "희귀 액체 생성",
"swap bodies (choose direction after wish)": "신체 교환 (소원 후 방향 선택)",
"switch to roleplay mode (checkpointing at settlements)": "롤플레이 모드 전환 (정착지 체크포인트)",
"toggle godmode": "갓모드 전환",
"wish for something specific": "특정 소원 빌기"
```

**Step 2: Mutations 카테고리명 5개 번역**

```json
"mental defects": "정신 결함",
"mental mutations": "정신 돌연변이",
"morphotypes": "형태형",
"physical defects": "신체 결함",
"physical mutations": "신체 돌연변이"
```

**Step 3: 커밋**

```bash
git add LOCALIZATION/
git commit -m "feat: add translations for wish commands (13) and mutation categories (5)"
```

---

## Task 6: Furniture, Walls, ZoneTerrain, WorldTerrain 직접 번역 추가

**Files:**
- Modify: `LOCALIZATION/OBJECTS/furniture/` 하위 JSON
- Modify: `LOCALIZATION/OBJECTS/terrain/` 하위 JSON

이 카테고리는 항목 수가 많다 (Furniture 209, Walls 95, ZoneTerrain 76, WorldTerrain 22 = **402개**).

**Step 1: 패턴 번역기가 이미 처리하는 항목 필터링**

CompoundTranslator, OfPatternTranslator 등이 런타임에 처리하는 항목을 제외한다. Task 1 진단 결과에서 실제로 게임에서 영어로 표시되는 항목만 대상으로 한다.

**Step 2: Furniture 미번역 항목 번역**

진단 결과에서 나온 실제 미번역 목록을 기준으로 JSON 번역 추가. 기존 `LOCALIZATION/OBJECTS/furniture/` 파일에 추가한다. 고유명사(barathrum clock 등)는 음역+의역 혼합.

**Step 3: Walls 미번역 항목 번역**

벽/구조물 이름 번역. `*sultanName*` 패턴 항목은 동적이므로 Task 8에서 별도 처리.

**Step 4: ZoneTerrain + WorldTerrain 번역**

지형/월드맵 이름 번역. 고유지명(bethesda susa, golgotha 등)은 음역.

**Step 5: 검증**

Run: `cd /Users/ben/Desktop/qud_korean && python3 tools/build_asset_index.py --stats`
Expected: 해당 카테고리 커버리지 상승 확인

**Step 6: 커밋**

```bash
git add LOCALIZATION/
git commit -m "feat: add translations for furniture, walls, terrain (N items)"
```

---

## Task 7: Creatures + Items 잔여 미번역 확인 및 보완

**Files:**
- Modify: `LOCALIZATION/OBJECTS/creatures/` 하위 JSON
- Modify: `LOCALIZATION/OBJECTS/items/` 하위 JSON
- Modify: `LOCALIZATION/OBJECTS/_manual_translations.json`

**Step 1: 진단으로 패턴 번역기 실패 항목 추출**

Creatures 770, Items 463 중 CompoundTranslator가 처리 못하는 항목을 찾는다. 이들은 고유명사이거나 복잡한 구조여서 패턴 매칭이 안 되는 경우.

**Step 2: _manual_translations.json에 직접 번역 추가**

패턴 번역기가 처리 못하는 항목을 수동 번역으로 추가한다. 고유명사는 기존 `terminology_standard.md` 규칙 준수.

**Step 3: 검증**

Run: `cd /Users/ben/Desktop/qud_korean && python3 tools/build_asset_index.py --stats`
Expected: Creatures, Items 커버리지 99%+

**Step 4: 커밋**

```bash
git add LOCALIZATION/
git commit -m "feat: add manual translations for remaining creatures/items"
```

---

## Task 8: 동적 변수 패턴 처리 (=variable=, *template*)

**Files:**
- Modify: `Scripts/02_Patches/20_Objects/V2/Patterns/` (새 패턴 번역기 또는 기존 수정)
- Reference: `Docs/07_TEMPLATE_VARIABLES.md`

**Step 1: 동적 패턴 유형 분석**

68개 항목의 패턴 유형:
- `=creatureRegionAdjective= {creature}` (34개) — 지역형용사 + 생물
- `{creature} =creatureRegionNoun=` (21개) — 생물 + 지역명사
- `{material} *creature* figurine` (10개) — 재료 + 동적생물 + 조각상
- `*sultanName*` 관련 (3개) — 술탄 이름 동적 생성

**Step 2: C# 패치 구현 여부 판단**

- 게임에서 `=variable=`이 치환된 후 번역 엔진을 통과하는지 확인
- 만약 치환 후 CompoundTranslator가 처리 가능하다면 추가 코드 불필요
- 만약 치환 전에 DisplayName이 캐시되면 C# 패치 필요

**Step 3: 필요 시 CreatureRegionTranslator 구현**

```csharp
// Scripts/02_Patches/20_Objects/V2/Patterns/CreatureRegionTranslator.cs
// =creatureRegionAdjective= 치환 후 생물명 번역 처리
```

**Step 4: 테스트**

Run: `cd /Users/ben/Desktop/qud_korean && python3 tools/test_object_translator.py`
Expected: 새 패턴 포함 테스트 통과

**Step 5: 커밋**

```bash
git add Scripts/ LOCALIZATION/
git commit -m "feat: handle dynamic variable patterns for creature region names"
```

---

## Task 9: Foods 잔여 22개 번역

**Files:**
- Modify: `LOCALIZATION/OBJECTS/items/consumables/food.json`

**Step 1: 미번역 음식 항목 번역 추가**

```json
"bone meal": "뼈 가루",
"canned have-it-all": "올인원 통조림",
"concentrated dreambeard gland paste": "농축 꿈수염 분비샘 반죽",
"concentrated flamebeard gland paste": "농축 불꽃수염 분비샘 반죽",
"concentrated gallbeard gland paste": "농축 쓸개수염 분비샘 반죽",
"concentrated mazebeard gland paste": "농축 미로수염 분비샘 반죽",
"concentrated nullbeard gland paste": "농축 허무수염 분비샘 반죽",
"concentrated sleetbeard gland paste": "농축 진눈깨비수염 분비샘 반죽",
"concentrated stillbeard gland paste": "농축 고요수염 분비샘 반죽",
"concentrated tartbeard gland paste": "농축 신맛수염 분비샘 반죽",
"congealed blaze": "응고된 불꽃",
"congealed hulk honey": "응고된 헐크 꿀",
"congealed love": "응고된 사랑",
"congealed rubbergum": "응고된 고무껌",
"congealed salve": "응고된 연고",
"congealed shade oil": "응고된 그늘기름",
"congealed skulk": "응고된 잠행",
"croc jerky": "악어 육포",
"crusty loaf": "딱딱한 빵",
"dream smoke": "꿈 연기",
"drop of nectar": "넥타르 한 방울",
"soul curd": "영혼 커드"
```

**Step 2: 검증**

Run: `cd /Users/ben/Desktop/qud_korean && python3 tools/build_asset_index.py --stats`
Expected: Foods 100% 커버리지

**Step 3: 커밋**

```bash
git add LOCALIZATION/OBJECTS/items/consumables/food.json
git commit -m "feat: add translations for 22 remaining food items"
```

---

## Task 10: 최종 검증 및 리포트 업데이트

**Files:**
- Run: `tools/build_asset_index.py --stats`
- Run: `tools/find_missing_vocab.py`
- Run: `tools/project_tool.py`
- Modify: `Docs/plans/untranslated_report.md`
- Modify: `Docs/04_TODO.md`
- Modify: `Docs/05_CHANGELOG.md`

**Step 1: 전체 검증 실행**

Run: `cd /Users/ben/Desktop/qud_korean && python3 tools/project_tool.py`
Expected: 검증 통과

**Step 2: 에셋 인덱스 재생성 및 통계 확인**

Run: `cd /Users/ben/Desktop/qud_korean && python3 tools/build_asset_index.py --stats`
Expected: 커버리지 99%+

**Step 3: 리포트 업데이트**

untranslated_report.md의 숫자를 최신 결과로 업데이트한다.

**Step 4: TODO, CHANGELOG 업데이트**

완료된 작업을 반영한다.

**Step 5: 빌드 및 배포**

Run: `cd /Users/ben/Desktop/qud_korean && ./deploy.sh`
Expected: 빌드 성공, 번들 생성

**Step 6: 커밋**

```bash
git add -A
git commit -m "docs: update reports and changelog after completing untranslated items"
```

---

## 작업 순서 요약

| Task | 내용 | 예상 항목 수 |
|------|------|-------------|
| 1 | 진단 — 정확한 갭 파악 | — |
| 2 | 누락 어휘 12개 추가 | 65개 해결 |
| 3 | Factions 번역 | 67개 |
| 4 | Phenomena, Data, Widgets, Worlds | 53개 |
| 5 | WishCommands, Mutations 카테고리 | 18개 |
| 6 | Furniture, Walls, Terrain | ~402개 (필터링 후 감소 예상) |
| 7 | Creatures + Items 잔여 | 진단 후 확정 |
| 8 | 동적 변수 패턴 (C# 코드) | 68개 |
| 9 | Foods 잔여 | 22개 |
| 10 | 최종 검증 및 문서 업데이트 | — |

**의존성:** Task 1 완료 후 나머지 작업의 정확한 범위가 확정된다. Task 2-9는 독립적으로 병렬 실행 가능하나, Task 8(C# 코드)은 가장 복잡하므로 마지막에 한다. Task 10은 모든 작업 완료 후.
