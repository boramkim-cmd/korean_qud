# LOCALIZATION 구조 가이드

> 이 문서는 번역 데이터의 구조와 규칙을 설명합니다.

---

## 📁 디렉토리 구조

```
LOCALIZATION/
├── CHARGEN/                  # 캐릭터 생성 (Character Generation)
│   ├── modes.json            → 게임 모드
│   ├── stats.json            → 스탯 (Strength, Agility 등)
│   ├── ui.json               → 캐릭터 생성 UI
│   ├── presets.json          → 프리셋 캐릭터
│   ├── locations.json        → 시작 위치
│   ├── factions.json         → 팩션
│   ├── GENOTYPES/            # [Layer 2] 종족 데이터 (Mutated Human, True Kin)
│   └── SUBTYPES/             # [Layer 2] 하위 타입 (Callings, Castes)
│
├── GAMEPLAY/                 # 인게임 플레이 (Gameplay)
│   ├── skills.json           → 스킬 이름/설명
│   ├── cybernetics.json      → 사이버네틱스
│   └── MUTATIONS/            # [Layer 2] 변이 및 결함
│       ├── Physical_Mutations/
│       ├── Mental_Mutations/
│       ├── Physical_Defects/
│       ├── Mental_Defects/
│       └── Morphotypes/
│
├── UI/                       # 사용자 인터페이스 (User Interface)
│   ├── common.json           → UI 공통 용어 (버튼, 메뉴 등)
│   ├── options.json          → 설정 화면 텍스트
│   └── terms.json            → 일반 용어
│
├── _DEPRECATED/              # 보관소 (Archived)
│   └── glossary_proto.json   → (구) 레거시 파일
│
└── integrity_report.md       # 번역 무결성 리포트
```

---

## 📊 Layer 1 vs Layer 2 선택 기준

### Layer 1: glossary_*.json (단일 파일 용어집)

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

**파일 위치**:
- `CHARGEN/GENOTYPES/`
- `CHARGEN/SUBTYPES/`
- `GAMEPLAY/MUTATIONS/`

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

1. **먼저 Layer 1 검토**: 단순한 구조면 적절한 폴더(GAMEPLAY/UI/CHARGEN) 내에 `filename.json` 생성
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
| `LocalizationManager` | Layer 1 (단일 JSON) | `GetTerm(category, key)` |
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
| `UI/common.json` | 148 | 100% | UI 공통 |
| `GAMEPLAY/skills.json` | 218 | 100% | 스킬 |
| `UI/options.json` | 362 | 94% | 설정 |
| `CHARGEN/*.json` | 150+ | 100% | 캐릭터 생성 |
| `GAMEPLAY/MUTATIONS/` | 81 | 60% | 변이 |
| `CHARGEN/GENOTYPES/` | 2 | 100% | 종족 |
| `CHARGEN/SUBTYPES/` | 24 | 100% | 직업/계급 |

---

> **다음 단계**: 번역 규칙과 스타일은 `Docs/STYLE_GUIDE.md` 참조
