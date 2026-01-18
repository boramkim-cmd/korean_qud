# 변이 편집 가이드 (Mutation Editing)

> 이 문서는 Layer 2 변이 데이터(MUTATIONS/**/*.json) 편집 시 참조합니다.
> ⚠️ **수동 작업 필수** - 자동화 스크립트 사용 금지!

## 파일 위치

```
LOCALIZATION/MUTATIONS/
├── Physical_Mutations/     ← 육체적 변이
│   ├── Stinger_*.json
│   ├── Multiple_Arms.json
│   └── ...
├── Mental_Mutations/       ← 정신적 변이
│   ├── Telepathy.json
│   ├── Domination.json
│   └── ...
├── Physical_Defects/       ← 육체적 결함
├── Mental_Defects/         ← 정신적 결함
└── Morphotypes/            ← 형태
```

## JSON 구조

```json
{
  "names": {
    "English Name": "한글 이름"
  },
  "description": "영문 설명 (GetDescription()에서 추출)",
  "description_ko": "한글 설명",
  "leveltext": [
    "영문 추가 정보 1 (GetLevelText()에서 추출)",
    "영문 추가 정보 2"
  ],
  "leveltext_ko": [
    "한글 추가 정보 1",
    "한글 추가 정보 2"
  ]
}
```

## 편집 절차

### 1. C# 소스 찾기

```bash
find Assets/core_source -name "MutationName.cs"
```

### 2. GetDescription() 확인

```bash
grep -A 10 "GetDescription" Assets/core_source/.../MutationName.cs
```

### 3. GetLevelText() 확인 (있는 경우)

```bash
grep -A 15 "GetLevelText" Assets/core_source/.../MutationName.cs
```

### 4. Variant 확인 (Properties 클래스)

일부 변이는 여러 변형이 있습니다:
- `Stinger` → `StingerPoisonProperties`, `StingerParalyzingProperties`

```bash
grep -r "Properties" Assets/core_source/ | grep MutationName
```

### 5. JSON 작성

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

### 6. 게임 내 확인

```bash
./tools/deploy-mods.sh
# 게임 실행 → 캐릭터 생성 → 변이 선택 → 설명 확인
```

## ⚠️ 주의사항

### Variant 변이

| 변이 | Variant | 파일명 |
|------|---------|--------|
| Stinger | Poisoning Venom | `Stinger_(Poisoning_Venom).json` |
| Stinger | Paralyzing Venom | `Stinger_(Paralyzing_Venom).json` |
| Horns | 없음 | `Horns.json` |

### leveltext 배열

- `\n\n`로 구분된 문자열을 배열로 분리
- 각 요소는 한 줄에 해당
- 빈 문자열 허용

### 태그 처리

변이 설명에는 색상 태그가 없는 것이 일반적입니다.
만약 있다면 제외하세요.

## 담당 컴포넌트

- **StructureTranslator**: MUTATIONS 디렉토리 로드, 구조 파싱
- **ChargenTranslationUtils**: 캐릭터 생성 화면에서 변이 번역

## 관련 에러

- `ERR-006`: 변이 설명 불일치 및 줄바꿈 처리 오류
- `ERR-007`: 변이 JSON 파일 중복 및 경로 오류

→ 상세: `Docs/04_ERROR_LOG.md`
