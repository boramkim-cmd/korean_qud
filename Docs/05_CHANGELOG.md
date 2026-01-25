# Caves of Qud Korean Localization - Changelog

> **Version**: 6.8 | **Last Updated**: 2026-01-26

---

## Recent Changes

### [2026-01-26] V1 vs V2 컨텍스트별 검증 테스트 스크립트 생성
- **새 파일**: `tools/test_display_contexts.py` (~590줄)
- **목적**: V1(기존)과 V2(리팩토링) 번역 결과 동등성 비교 + 게임 컨텍스트별 검증
- **V2 아키텍처**: Pipeline/Strategy 패턴
  - 7개 핸들러: ColorTag, OfPattern, PrefixSuffix, DirectMatch, DynamicPattern, ColorTagContent, Fallback
  - 각 핸들러가 `can_handle()` + `handle()` 메서드로 독립 처리
- **테스트 케이스**: 100개 (4개 컨텍스트별 25개씩)
  | 컨텍스트 | 테스트 항목 |
  |----------|-------------|
  | INVENTORY | 색상태그, 수량, 접두사, +X 수식어 |
  | TOOLTIP | 상태(lit/unburnt), drams 패턴, 멀티워드 태그 |
  | SHOP | 재료 접두사, 무기 타입, 수류탄 |
  | LOOK | 시체, 음식, 부위, 소유격 |
- **결과**: V1 vs V2 동등: 100/100 (100.0%)
- **사용법**:
  ```bash
  python3 tools/test_display_contexts.py              # 기본 실행
  python3 tools/test_display_contexts.py --verbose    # 상세 출력
  python3 tools/test_display_contexts.py --context inventory  # 특정 컨텍스트
  python3 tools/test_display_contexts.py --failures-only      # 실패만
  ```

### [2026-01-26] ObjectTranslator 종합 테스트 스크립트 생성
- **새 파일**: `tools/test_object_translator.py`
- **목적**: JSON 사전 파일들을 읽어 번역 로직 시뮬레이션 및 검증
- **테스트 케이스**: 100개 (단순→복잡 점진적 테스트)
- **성공률**: 100% (100/100 통과)
- **테스트 카테고리**:
  | 카테고리 | 개수 | 예시 |
  |----------|------|------|
  | 단순 명사 | 10 | `mace` → `메이스` |
  | 단일 접두사 | 10 | `bronze mace` → `청동 메이스` |
  | 복합 접두사 | 10 | `engraved bronze mace` → `새겨진 청동 메이스` |
  | 상태 접미사 | 10 | `torch (lit)` → `횃불 (점화됨)` |
  | drams 패턴 | 5 | `canteen [32 drams of water]` → `수통 [물 32드램]` |
  | 컬러 태그 | 10 | `{{w|bronze}} mace` → `{{w|청동}} 메이스` |
  | 동적 식품 | 8 | `bear jerky` → `곰 육포` |
  | 동적 부위 | 8 | `wolf hide` → `늑대 가죽` |
  | 소유격 | 5 | `panther's claw` → `표범의 발톱` |
  | of 패턴 | 5 | `sword of fire` → `불의 검` |
  | 시체 | 3 | `bear corpse` → `곰 시체` |
  | 신규 어휘 | 10 | `plasma rifle` → `플라즈마 라이플` |
  | 복합 케이스 | 6 | `flawless crysteel sword of fire` → `완벽한 크리스틸 불의 검` |
- **사용법**: `python3 tools/test_object_translator.py`
- **기능**:
  - JSON 사전 자동 로드 (items/_common.json, _nouns.json, creatures/_common.json, _suffixes.json)
  - 접두사/접미사 추출 및 번역
  - 컬러 태그 보존
  - 한국어 어순 변환 (of 패턴)
  - 동적 패턴 처리 (food, parts, corpse, possessive)
  - 컬러 출력 (PASS/FAIL 시각화)

### [2026-01-25] 번역 불일치 및 누락 근본 수정 (PRD v2)
- **BUG #1 수정: 색상 태그 내 명사 미번역**
  - `TranslateMaterialsInColorTags()`에서 명사도 번역하도록 수정
  - `{{c|basic toolkit}}` → `{{c|기본 공구함}}` 정상 번역
  - `{{w|copper nugget}}` → `{{w|구리 덩어리}}` 정상 번역
- **BUG #2 수정: 색상 형용사 누락**
  - `items/_common.json`에 colors 섹션 추가
  - violet, milky, smokey, rosey, turquoise, cobalt, mossy, muddy, platinum 등
  - `{{m|violet}} tube` → `보라색 튜브` 정상 번역
- **Step 4: species를 접두사 사전에 병합**
  - `LoadCreatureCommon()`에서 species를 colorTagVocab과 allPrefixes 모두에 병합
  - `ape fur cloak` → `유인원 모피 망토` 정상 번역
- **Step 5: "of X" 패턴 어순 처리**
  - `TryTranslateOfPattern()` 메서드 신규 추가
  - 영어 "X of Y" → 한국어 "Y의 X" 어순 변환
  - `_suffixes.json`에 of_patterns 확장 (river-wives, Holy Rhombus 등)
  - `sandals of the river-wives` → `강 아내들의 샌들`
- **Step 6: 누락 어휘 추가**
  - `_common.json` modifiers: salt-encrusted, dilute, fried, sun, vanta
  - `_nouns.json` misc: banner, petals, quills, veil
  - `creatures/_common.json` species: lah, Issachari, witchwood 등
- **검증**: 총 5개 파일 수정, 210줄 추가

### [2026-01-25] 미번역 아이템 버그 수정
- **ObjectTranslator 개선**:
  - `TranslatePrefixesInText()` 메서드 추가: fallback 경로에서 접두사 번역 처리
  - 디버그 로그 추가: 번역 실패 원인 추적 용이
- **JSON 데이터 확장**:
  - `items/_common.json`: 56개 modifier 추가
    - 하이픈 접두사 24개 (e.g., ice-, fire-, lightning-)
    - 동적 Mod 형용사 32개 (e.g., fitted, reinforced, serrated)
  - `items/_nouns.json`: 16개 항목 추가
    - furniture 카테고리 14개 (chair, table, bed 등)
    - compound_weapons 2개 (spear-thrower, atlatl)
  - `_suffixes.json`: 상태 접미사 2개 추가 (`(Full)`, `(Empty)`)
- **수정된 아이템 예시**: leather moccasins, ice-slicked boots, fitted leather armor

### [2026-01-25] Skills JSON Restructure & Patch System
- **구조 재설계**: skills.json → 개별 스킬 파일 분리 (뮤테이션 패턴 적용)
  - 기존: `LOCALIZATION/GAMEPLAY/skills.json` (하나의 거대한 파일)
  - 신규: `LOCALIZATION/GAMEPLAY/SKILLS/*.json` (20개 개별 파일)
- **새 JSON 구조**: powers와 power_desc를 한 세트로 묶음
  ```json
  {
    "names": { "Axe": "도끼" },
    "description": "You are skilled with axes.",
    "description_ko": "당신은 도끼에 숙달되어 있습니다.",
    "powers": {
      "axe proficiency": {
        "name": "도끼 숙련",
        "desc": "도끼로 공격할 때 명중에 +2 보너스를 받습니다."
      }
    }
  }
  ```
- **생성된 스킬 파일** (20개):
  - Acrobatics, Axe, Bow_and_Rifle, Cooking_and_Gathering, Cudgel
  - Customs_and_Folklore, Endurance, Heavy_Weapon, Long_Blade, Multiweapon_Fighting
  - Persuasion, Physic, Pistol, Self_Discipline, Shield
  - Short_Blade, Single_Weapon_Fighting, Tactics, Tinkering, Wayfaring
- **패치 코드 생성**: `Scripts/02_Patches/10_UI/02_10_12_Skills.cs`
  - `SkillLocalizationManager`: SKILLS/*.json 파일 로드 및 파싱
  - `Patch_SkillFactory`: SkillFactory 로드 후 번역 자동 적용
- **LocalizationManager 업데이트**: skills.json 참조 제거
- **검증**: project_tool.py 통과, 총 7,037개 번역 항목

### [2026-01-25] Items Translation Extension
- **Weapons 확장**:
  - `weapons/melee/cudgels.json`: 20 → 32 항목 (+12)
  - `weapons/melee/blades.json`: 9 → 20 항목 (+11)
  - `weapons/melee/axes.json`: 16 → 24 항목 (+8)
  - `weapons/melee/long_blades.json`: 11 → 26 항목 (+15)
  - `weapons/ranged/guns.json`: 23 → 26 항목 (+3)
  - `weapons/ranged/bows.json`: 2 → 12 항목 (+10)
- **Armor 확장**:
  - `armor/body.json`: 7 → 22 항목 (+15)
  - `armor/face.json`: 7 → 16 항목 (+9)
  - `armor/back.json`: 7 → 16 항목 (+9)
- **Grenades 확장**:
  - `artifacts/grenades.json`: 10 → 56 항목 (+46, mk I/II/III 시리즈)
- **검증**: project_tool.py 통과, 총 6,956개 번역 항목 (+787)

### [2026-01-24] Objects Translation Major Expansion
- **Creatures 확장** (21.8% → ~45%):
  - `insects/ants.json`: 1 → 16 항목
  - `insects/beetles.json`: 1 → 18 항목
  - `insects/crabs.json`: 1 → 16 항목
  - `insects/hoppers.json`: 1 → 14 항목
  - `insects/moths.json`: 3 → 22 항목
  - `insects/spiders.json`: 3 → 24 항목
  - `insects/worms.json`: 5 → 21 항목
  - `humanoids/goatfolk.json`: 4 → 18 항목
  - `humanoids/others.json`: 4 → 32 항목 (Baetyl, 골렘, 트롤 등)
  - `animals/mammals.json`: 13 → 38 항목
- **Items 확장** (30.8% → ~48%):
  - `armor/head.json`: 6 → 20 항목 (전 티어 투구)
  - `armor/hands.json`: 5 → 18 항목 (전 티어 장갑)
  - `armor/feet.json`: 6 → 19 항목 (전 티어 장화)
  - `weapons/melee/axes.json`: 5 → 18 항목
  - `weapons/melee/cudgels.json`: 9 → 22 항목
  - `weapons/ranged/guns.json`: 7 → 24 항목
- **Widgets 확장**: 15 → 44 항목
- **Terrain 확장**: zone.json 26 → 52 항목
- **검증**: project_tool.py 통과, 총 6,169개 번역 항목

### [2026-01-22] Item Tooltip Localization Complete
- **Patches Created/Modified**:
  - `02_10_02_Tooltip.cs` - ShowManually unified patch for all tooltip paths
  - `02_20_00_ObjectTranslator.cs` - Dynamic food patterns + state suffix handling
- **Features Added**:
  - Tooltip header translation ("This Item" → "현재 아이템", "Equipped Item" → "장착 아이템")
  - Dynamic food patterns: `{creature} jerky/meat/haunch` → Korean
  - State suffix translation: `[empty]` → `[비어있음]`, `(lit)` → `(점화됨)`
- **Bugs Fixed**:
  - TooltipTrigger vs Tooltip.GameObject confusion (critical)
  - State suffix processing order (waterskin [empty] now properly translated)
  - Look.QueueLookerTooltip path not covered (world map tooltips)
- **JSON Added**:
  - `common.json`: tooltips section
  - `food.json`: bear jerky, haunch, preserved meat entries
- See: [10_ITEM_TOOLTIP_ANALYSIS.md](10_ITEM_TOOLTIP_ANALYSIS.md)

### [2026-01-22] P3-06 Tool Scripts Consolidation
- Upgraded `project_tool.py` to v2.0 with CLI subcommands
  - `validate`, `build`, `glossary`, `stats`, `help`
- Moved 3 legacy scripts to `tools/_legacy/`:
  - `check_missing_translations.py` (XML-based, obsolete)
  - `verify_structure_data.py` (integrated into project_tool)
  - `fix_empty_descriptions.py` (one-time fix, complete)
- Created `tools/README.md` documentation
- Total legacy scripts: 13 (safely archived)

### [2026-01-22] Object Translation Expansion
- Added new creature categories: birds, reptiles, farmers, seedsprout
- Added new item categories: ammo, books
- Final stats: 57 JSON files, 321+ translation entries
- Git synced: commit a6d9cf2

### [2026-01-22] P2-01 Message Log Patch Complete
- Created `02_10_16_MessageLog.cs` - Harmony patch for MessageQueue.AddPlayerMessage
- Created `LOCALIZATION/GAMEPLAY/messages.json` - 50+ message patterns
- Features: verb translation dictionary, Korean josa handling, pattern matching
- Categories: flight, movement, items, combat, status, interaction, system

### [2026-01-22] Mutation & Object Systems Complete
- Verified all 81 mutation files translated (Physical/Mental/Defects/Morphotypes)
- Object JSON reorganized: 51 files, 300+ entries (type-based structure)
- Validation fixes: 0 empty translations, 0 duplicate keys

### [2026-01-21] Tutorial Translation (ERR-018)
- Fixed smart quote mismatch (U+2019 vs U+0027)
- Added Korean text skip logic
- Added missing plain text variations

### [2026-01-20] Font System
- Korean font bundle loading from StreamingAssets
- TMP_FontAsset fallback registration
- Tooltip Korean glyph support

### [2026-01-19] Character Creation Fixes
- ERR-017: Attribute screen multi-fix (breadcrumb, tooltip, descriptions)
- ERR-016: Attribute tooltip/description localization
- ERR-015: Chargen overlay scope fix
- ERR-014: Toughness terminology (건강 not 지구력)
- ERR-013: Caste stat/save modifiers
- ERR-008: Attribute crash (Substring fix)
- ERR-009: Bullet dot issues

---

## Summary by Phase

### Phase 1: Stabilization (100%)
| Date | Work |
|------|------|
| 01-16 | Inventory filter, Options values, Josa support, Mutation desc |
| 01-17~22 | Mutation JSON restructure (81 files) |
| 01-21 | Tutorial translation system |

### Phase 2: Gameplay (90%)
| Date | Work |
|------|----- |
| 01-22 | Object localization system (Phases 0-4) |
| 01-22 | Message Log Patch (P2-01) |
| 01-24 | Objects Translation Major Expansion (67 files, 6,169 entries) |
| 01-25 | Items Translation Extension (weapons, armor, grenades) (+787 entries) |

---

## Statistics
- Total translation entries: 7,037
- Mutation files: 81
- Skill files: 20
- Object files: 67
- Message patterns: 50+
- **Test coverage**: 100 cases (100% pass)
- Build status: Success

---

*Full history: _archive/02_CHANGELOG_full_20260122.md*
