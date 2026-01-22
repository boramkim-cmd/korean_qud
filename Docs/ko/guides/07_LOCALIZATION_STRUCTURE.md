# LOCALIZATION 구조 가이드

> 이 문서는 번역 데이터의 구조와 규칙을 설명합니다.

---

## 📁 디렉토리 구조

```
LOCALIZATION/
├── README.md                    # 이 파일
├── integrity_report.md          # 자동 생성 보고서
│
├── CHARGEN/                     # 캐릭터 생성 컨텍스트
│   ├── README.md                # 하위 폴더 가이드
│   ├── modes.json               # 게임 모드 (Classic, Roleplay, Wander 등)
│   ├── stats.json               # 스탯 설명 (Strength, Agility 등)
│   ├── ui.json                  # 캐릭터 생성 UI 텍스트
│   ├── presets.json             # 프리셋 캐릭터 설명
│   ├── locations.json           # 시작 위치 이름
│   ├── factions.json            # 팩션 이름
│   ├── GENOTYPES/               # 종족 (Layer 2)
│   │   ├── Mutated_Human.json
│   │   └── True_Kin.json
│   └── SUBTYPES/                # 직업/계급 (Layer 2)
│       ├── Callings/            # 변이 인간 직업 (12개)
│       └── Castes/              # 순수 인간 계급 (12개)
│
├── GAMEPLAY/                    # 게임플레이 기능
│   ├── README.md
│   ├── skills.json              # 스킬 및 권능
│   ├── cybernetics.json         # 사이버네틱스 이식물
│   └── MUTATIONS/               # 변이 (Layer 2)
│       ├── Physical_Mutations/  # 육체 변이 (31개)
│       ├── Mental_Mutations/    # 정신 변이 (27개)
│       ├── Physical_Defects/    # 육체 결함 (12개)
│       ├── Mental_Defects/      # 정신 결함 (8개)
│       └── Morphotypes/         # 형태형 (3개)
│
├── UI/                          # 사용자 인터페이스
│   ├── README.md
│   ├── common.json              # 공통 UI 요소 (버튼, 메뉴 등)
│   ├── options.json             # 설정 화면 (362개 항목)
│   ├── display.json             # 디스플레이 설정 (16개 항목)
│   └── terms.json               # 일반 게임 용어
│
└── _DEPRECATED/                 # 구 버전 파일 보관
    └── glossary_proto.json      # 더 이상 사용 안 함
```

---

## 📊 Layer 1 vs Layer 2 선택 기준

### Layer 1: 단일 파일 용어집 (*.json)

**사용 시점**:
- 단순한 key:value 쌍
- 한 카테고리의 모든 항목이 ~500줄 이하
- 이름만 번역하면 되는 경우

**담당 컴포넌트**: `LocalizationManager`

**JSON 스키마**:
```json
{
  "category_name": {
    "english_key": "한글 값",
    "another key with spaces": "또 다른 값",
    "UPPERCASE_KEY": "대소문자 무관"
  }
}
```

**예시** (`UI/common.json`):
```json
{
  "ui": {
    "New Game": "새 게임",
    "Continue": "계속하기",
    "Options": "옵션"
  }
}
```

### Layer 2: 디렉토리 구조 (구조화된 데이터)

**사용 시점**:
- 이름 외에 설명(description), 추가 정보(leveltext) 필요
- 개별 항목이 복잡함 (10줄+ JSON)
- C# 소스에서 동적으로 생성되는 텍스트

**담당 컴포넌트**: `StructureTranslator`

**JSON 스키마**:
```json
{
  "names": {
    "English Name": "한글 이름"
  },
  "description": "영문 설명 (한 줄, C# GetDescription()에서 추출)",
  "description_ko": "한글 설명",
  "leveltext": [
    "영문 추가 정보 1 (C# GetLevelText()에서 추출)",
    "영문 추가 정보 2"
  ],
  "leveltext_ko": [
    "한글 추가 정보 1",
    "한글 추가 정보 2"
  ]
}
```

**예시** (`GAMEPLAY/MUTATIONS/Physical_Mutations/Stinger_(Poisoning_Venom).json`):
```json
{
  "names": {
    "Stinger (Poisoning Venom)": "독침 (독성 맹독)"
  },
  "description": "You bear a tail with a stinger that delivers poisonous venom.",
  "description_ko": "적에게 독성 맹독을 전달하는 침이 달린 꼬리를 가지고 있습니다.",
  "leveltext": [
    "20% chance on melee attack to sting your opponent",
    "Stinger is a long blade and can only penetrate once."
  ],
  "leveltext_ko": [
    "근접 공격 시 20% 확률로 상대를 찌릅니다",
    "독침은 긴 칼날이며 한 번만 관통할 수 있습니다."
  ]
}
```

---

## ⚠️ 중요한 규칙

### 1. 태그 금지 (번역문에 색상 태그 포함 금지)

```json
// ❌ 잘못됨 - 태그 포함
{
  "{{c|ù}} Most creatures...": "{{c|ù}} 대부분의 생물이..."
}

// ✅ 올바름 - 태그 없이
{
  "{{c|ù}} Most creatures...": "대부분의 생물이..."
}
```

**이유**: `TranslationEngine.RestoreColorTags()`가 자동으로 원본 태그를 복원합니다.

### 2. 키 정규화

- **소문자**: 검색 시 자동으로 소문자 변환됨
- **공백 유지**: `"New Game"` 그대로 사용
- **특수문자 허용**: `"{{c|20}} bonus points"` 형태 가능

### 3. 새 카테고리 추가 시

1. **먼저 Layer 1 검토**: 단순한 구조면 `glossary_{category}.json` 생성
2. **복잡하면 Layer 2**: 디렉토리 생성 후 개별 JSON 파일
3. **LocalizationManager vs StructureTranslator**: 담당 컴포넌트 확인

### 4. C# 소스 확인 필수 (Layer 2)

```bash
# 1. C# 파일 찾기
find Assets/core_source -name "Stinger.cs"

# 2. GetDescription() 확인
grep -A 5 "GetDescription" Assets/core_source/.../Stinger.cs

# 3. GetLevelText() 확인 (있는 경우)
grep -A 10 "GetLevelText" Assets/core_source/.../Stinger.cs
```

**주의**: Variant 변이(Stinger의 Poisoning/Paralyzing 등)는 `*Properties.cs` 파일 별도 확인!

---

## 🔧 컴포넌트 역할

| 컴포넌트 | 담당 | API |
|----------|------|-----|
| `LocalizationManager` | Layer 1 (glossary_*.json) | `GetTerm(category, key)` |
| `StructureTranslator` | Layer 2 (디렉토리) | `TranslateName(englishName)` |
| `TranslationEngine` | 전체 번역 (태그 처리 포함) | `TryTranslate(text, out translated)` |

---

## 📋 체크리스트

새 번역 데이터 추가 시:

- [ ] Layer 1 vs Layer 2 결정
- [ ] 올바른 파일/폴더 위치 선택
- [ ] JSON 스키마 준수
- [ ] 태그 미포함 확인
- [ ] `python3 tools/project_tool.py` 검증
- [ ] 게임 내 테스트

---

## 📂 파일 현황

| 파일/폴더 | 항목 수 | 완성도 | 비고 |
|-----------|---------|--------|------|
| `glossary_ui.json` | 148 | 100% | UI 공통 |
| `glossary_skills.json` | 218 | 100% | 스킬 |
| `glossary_options.json` | 362 | 94% | 설정 |
| `glossary_chargen_*.json` | 150+ | 100% | 캐릭터 생성 |
| `MUTATIONS/` | 81 | 60% | 변이 |
| `GENOTYPES/` | 2 | 100% | 종족 |
| `SUBTYPES/` | 24 | 50% | 직업/계급 |

---

## 🔄 변경 이력

### 2026-01-18: 폴더 구조 재정리
- 컨텍스트 기반 구조로 재편성 (CHARGEN, GAMEPLAY, UI)
- Layer 1 파일을 서브폴더로 이동 및 이름 변경
  - `glossary_ui.json` → `UI/common.json`
  - `glossary_chargen_modes.json` → `CHARGEN/modes.json` 등
- Layer 2 폴더를 컨텍스트별로 재배치
  - `MUTATIONS/` → `GAMEPLAY/MUTATIONS/`
  - `GENOTYPES/` → `CHARGEN/GENOTYPES/`
  - `SUBTYPES/` → `CHARGEN/SUBTYPES/`
- `glossary_proto.json`을 GENOTYPES/SUBTYPES에 통합 후 deprecated 처리
- LocalizationManager 및 StructureTranslator 경로 업데이트

---

> **다음 단계**: 번역 규칙과 스타일은 `Docs/08_STYLE_GUIDE.md` 참조
