# 데이터 구조 및 현황

> JSON 구조, 번역 데이터 현황, 용어 기준

---

## 번역 데이터 현황

| 카테고리 | 파일 수 | 항목 수 | 비고 |
|----------|---------|---------|------|
| CHARGEN | 15+ | 200+ | 캐릭터 생성 |
| GAMEPLAY | 20+ | 500+ | 스킬, 변이 등 |
| OBJECTS | 67 | 6,956+ | 아이템, 생물 |
| UI | 10+ | 700+ | 메뉴, 옵션 등 |
| **합계** | **112+** | **8,356+** | |

---

## JSON 구조

### Layer 1: 단순 키-값 (UI, 메뉴 등)
```json
// LOCALIZATION/UI/common.json
{
  "common": {
    "Back": "뒤로",
    "Next": "다음",
    "Cancel": "취소"
  }
}
```

### Layer 2: 구조화된 데이터 (오브젝트)
```json
// LOCALIZATION/OBJECTS/items/weapons.json
{
  "Dagger": {
    "names": {
      "dagger": "단검",
      "Dagger": "단검"
    },
    "description": "A small blade.",
    "description_ko": "작은 칼날."
  }
}
```

### Layer 2: 스킬/파워
```json
// LOCALIZATION/GAMEPLAY/SKILLS/Axe.json
{
  "names": { "Axe": "도끼" },
  "description": "You are skilled with axes.",
  "description_ko": "당신은 도끼에 숙달되어 있습니다.",
  "powers": {
    "axe proficiency": {
      "name": "도끼 숙련",
      "desc": "도끼로 공격할 때 명중에 +2 보너스."
    }
  }
}
```

---

## 폴더 구조

```
LOCALIZATION/
├── CHARGEN/           # 캐릭터 생성
│   ├── modes.json     # 게임 모드
│   ├── stats.json     # 스탯
│   ├── ui.json        # UI 텍스트
│   ├── presets.json   # 프리셋
│   ├── locations.json # 시작 위치
│   └── factions.json  # 팩션
│
├── GAMEPLAY/
│   ├── cybernetics.json
│   ├── messages.json   # 메시지 로그
│   ├── MUTATIONS/      # 변이 (81개 파일)
│   └── SKILLS/         # 스킬 (20개 파일)
│
├── OBJECTS/
│   ├── creatures/      # 생물
│   │   ├── animals/
│   │   ├── humanoids/
│   │   └── insects/
│   └── items/          # 아이템
│       ├── armor/
│       ├── weapons/
│       ├── consumables/
│       └── artifacts/
│
└── UI/
    ├── common.json     # 공통 UI
    ├── options.json    # 옵션 화면
    ├── terms.json      # 게임 용어
    └── display.json    # 표시 텍스트
```

---

## 용어 기준

### 스탯 (Attributes)
| 영문 | 한글 | 비고 |
|------|------|------|
| Strength | 힘 | |
| Agility | 민첩 | |
| Toughness | 건강 | NOT 지구력 |
| Intelligence | 지능 | |
| Willpower | 의지 | |
| Ego | 자아 | |

### 스킬 (Skills)
| 영문 | 한글 |
|------|------|
| Endurance | 지구력 |
| Axe | 도끼 |
| Cudgel | 곤봉 |
| Long Blade | 롱 블레이드 |
| Short Blade | 숏 블레이드 |
| Heavy Weapon | 중화기 |
| Tinkering | 팅커링 |
| Cooking | 요리 |

### 재료 (Materials)
| 영문 | 한글 |
|------|------|
| wooden | 나무 |
| iron | 철 |
| bronze | 청동 |
| steel | 강철 |
| carbide | 카바이드 |
| crysteel | 크리스틸 |
| fullerite | 풀러라이트 |
| zetachrome | 제타크롬 |

### 상태 접미사
| 영문 | 한글 |
|------|------|
| [empty] | [비어있음] |
| [full] | [가득 참] |
| (lit) | (점화됨) |
| (unlit) | (꺼짐) |
| (unburnt) | (미사용) |

---

## 동적 패턴

### 음식 패턴
```
{creature} jerky → {생물명} 육포
{creature} meat → {생물명} 고기
{creature} haunch → {생물명} 넓적다리
preserved {X} → 절임 {X}
cooked {X} → 조리된 {X}
```

### 부위 패턴
```
{creature} corpse → {생물명} 시체
{creature} hide → {생물명} 가죽
{creature} egg → {생물명} 알
{creature} skull → {생물명} 두개골
```

### 접두사 우선순위
```
1. 복합 재료: "folded carbide", "flawless crysteel"
2. 품질: "flawless", "masterwork"
3. 가공: "freeze-dried", "preserved"
4. 단순 재료: "wooden", "iron", "bronze"
5. 설명: "broken", "rusted", "luminous"
```

---

## 번역 규칙

### 절대 번역 금지
```
{{tag}}  - 게임 내부 변수
%var%    - 동적 값
=text=   - 색상/서식 태그
```

### 색상 태그 보존
```
원본: "{{c|cyan}} text"
번역: "{{c|시안}} 텍스트"  ← 태그 구조 유지
```

### 대소문자 변형 시도
```
검색 순서: 원본 → UPPER → Title → lower
예: "STRENGTH" → "Strength" → "strength"
```
