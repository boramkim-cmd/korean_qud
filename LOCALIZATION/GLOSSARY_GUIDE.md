# 용어집 완전 가이드

Caves of Qud 한글화 프로젝트의 용어 관리 및 사용 방법

---

## 📋 목차

1. [용어집 개요](#용어집-개요)
2. [JSON 용어집 시스템](#json-용어집-시스템)
3. [플레이스홀더 사용법](#플레이스홀더-사용법)
4. [용어 변경 방법](#용어-변경-방법)
5. [주요 용어 목록](#주요-용어-목록)

---

## 용어집 개요

### 역할
- **참고 문서**: 번역 일관성 유지
- **중앙 관리**: JSON 파일로 용어 통합 관리
- **자동 적용**: 플레이스홀더로 코드/XML에서 사용

### 파일 구조
```
LOCALIZATION/
├── glossary.json           # 용어 데이터 (JSON)
├── GLOSSARY_GUIDE.md       # 이 문서
├── GLOSSARY_Korean.md      # 참고용 용어 목록 (선택)
└── STYLE_GUIDE_Korean.md   # 번역 스타일 가이드
```

---

## JSON 용어집 시스템

### glossary.json 구조

```json
{
  "phrase": {
    "greeting": "안녕하세요",
    "farewell": "안녕히 가세요",
    "waterRitual": "당신의 갈증은 나의 것, 나의 물은 당신의 것"
  },
  "faction": {
    "crystalism": "크리스탈리즘",
    "mechanimists": "메카니카신자",
    "barathrumites": "바라스럼추종자"
  },
  "item": {
    "water": "물",
    "bread": "빵",
    "expensiveMilk": "비싼 우유"
  },
  "weapon": {
    "shortbow": "짧은 활",
    "longblade": "장검"
  },
  "attribute": {
    "strength": "힘",
    "agility": "민첩",
    "intelligence": "지능"
  }
}
```

### 카테고리 설명
- `phrase` - 자주 쓰는 문장/구문
- `faction` - 세력 이름
- `item` - 아이템 이름
- `weapon` - 무기 이름
- `attribute` - 능력치 이름
- `ui` - UI 텍스트
- `common` - 공통 용어

---

## 플레이스홀더 사용법

### 형식
```
[[category.key]]
```

### XML에서 사용

```xml
<!-- 단순 사용 -->
<text>[[phrase.greeting]]</text>

<!-- 조사와 함께 -->
<text>[[item.water]]{을/를} 마셨습니다</text>

<!-- 게임 명령어와 혼합 -->
<text>{{color|cyan|[[faction.crystalism]]}}</text>

<!-- 게임 변수와 혼합 -->
<text>[[phrase.greeting]], =player.name=</text>
```

### C# 코드에서 사용

```csharp
using QudKRTranslation.Core;

// 용어 가져오기
string term = GlossaryLoader.GetTerm("faction", "crystalism", "크리스탈리즘");

// Dictionary에 추가
public static Dictionary<string, string> Translations
{
    get
    {
        GlossaryLoader.LoadGlossary();
        return new Dictionary<string, string>()
        {
            { "Crystalism", GlossaryLoader.GetTerm("faction", "crystalism", "크리스탈리즘") }
        };
    }
}
```

---

## 용어 변경 방법

### 1. glossary.json 수정 (권장 ⭐)

**변경 전:**
```json
{"item": {"milk": "우유"}}
```

**변경 후:**
```json
{"item": {"milk": "생수"}}
```

→ 게임 재시작 → 모든 곳에 자동 적용!

### 2. 자동화 스크립트 (선택)

```bash
# 미리보기
python tools/sync_glossary.py --old "우유" --new "생수" --dry-run

# 실제 적용
python tools/sync_glossary.py --old "우유" --new "생수"
```

---

## 주요 용어 목록

모든 용어는 `glossary.json` 파일에 정의되어 있습니다.

### 카테고리별 용어

**세계관** (`world`)
- qud, joppa, redRock, sixDayStilt, gritGate, bethesdaSusa

**세력** (`faction`)
- crystalism, mechanimists, barathrumites, putusTemplar, consortiumOfPhyta

**캐릭터** (`character`, `genotype`)
- genotype, calling, attribute, skill, mutation, trueKin, mutant

**능력치** (`attribute`)
- strength, agility, toughness, intelligence, willpower, ego

**아이템** (`item`)
- artifact, relic, schematic, blueprint, trinket, water, bread, expensiveMilk

**무기** (`weapon`)
- shortbow, longblade, shortblade, axe, cudgel

**UI** (`ui`)
- newGame, continue, loadGame, saveGame, options, mods, quit
- inventory, equipment, weight, value, equip, unequip, drop, use

**공통** (`common`)
- yes, no, ok, cancel

### 사용 예시
```xml
<text>[[world.qud]]에 오신 것을 환영합니다</text>
<text>[[faction.crystalism]] 신자입니다</text>
<text>[[attribute.strength]]{이/가} 증가했습니다</text>
<text>[[item.water]]{을/를} 마셨습니다</text>
```

전체 용어 목록은 `LOCALIZATION/glossary.json` 파일을 참조하세요.

---

## 빠른 참조

### 용어 추가
```json
// glossary.json
{
  "item": {
    "newItem": "새 아이템"  // ← 추가
  }
}
```

### XML에서 사용
```xml
<text>[[item.newItem]]{을/를} 발견했습니다</text>
```

### 결과
→ "새 아이템을 발견했습니다"

---

## 주의사항

1. **JSON 구문**: 큰따옴표(`"`) 사용, 마지막 항목 뒤 쉼표 제거
2. **키명**: 영문 소문자, camelCase 사용
3. **플레이스홀더**: `[[category.key]]` 형식 (점 구분자)
4. **게임 명령어**: `{{}}`, `=...=` 는 건드리지 마세요

---

## 문제 해결

### 용어가 적용 안 됨
1. glossary.json 구문 오류 확인
2. 플레이스홀더 형식 확인: `[[category.key]]`
3. 게임 재시작

### 플레이스홀더가 그대로 표시됨
1. XMLGlossaryProcessor.cs 패치 확인
2. 카테고리/키명 일치 확인
3. 로그 확인

---

**요약:**
- ✅ `glossary.json`에 용어 정의
- ✅ `[[category.key]]` 형식으로 사용
- ✅ JSON만 수정하면 모든 곳에 적용
- ✅ 게임 재시작으로 반영

🎉 완료!
