# 시스템 아키텍처

> 한글화 모드의 기술적 구조 및 핵심 API

---

## 시스템 구조도

```
┌─────────────────────────────────────────────────────────────┐
│                     Caves of Qud (게임)                      │
├─────────────────────────────────────────────────────────────┤
│                     Harmony Library                          │
├─────────────────────────────────────────────────────────────┤
│  Core Layer                                                  │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐         │
│  │ ModEntry     │ │ Translation  │ │ Localization │         │
│  │ (진입점)     │ │ Engine       │ │ Manager      │         │
│  └──────────────┘ └──────────────┘ └──────────────┘         │
│  ┌──────────────┐ ┌──────────────┐                          │
│  │ ScopeManager │ │ Object       │                          │
│  │ (스코프)     │ │ Translator   │                          │
│  └──────────────┘ └──────────────┘                          │
├─────────────────────────────────────────────────────────────┤
│  Patch Layer                                                 │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            │
│  │ GlobalUI    │ │ CharCreate  │ │ Options     │            │
│  │ (전역 UI)   │ │ (캐릭터)    │ │ (설정)      │            │
│  └─────────────┘ └─────────────┘ └─────────────┘            │
│  ┌─────────────┐ ┌─────────────┐                            │
│  │ DisplayName │ │ Description │                            │
│  │ (이름 패치) │ │ (설명 패치) │                            │
│  └─────────────┘ └─────────────┘                            │
├─────────────────────────────────────────────────────────────┤
│  Data Layer (LOCALIZATION/)                                  │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐            │
│  │ CHARGEN │ │ GAMEPLAY│ │ UI      │ │ OBJECTS │            │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘            │
└─────────────────────────────────────────────────────────────┘
```

---

## 핵심 컴포넌트

### Core Layer

| 파일 | 역할 | 주요 API |
|------|------|----------|
| `00_00_00_ModEntry.cs` | 모드 진입점 | `Main()` |
| `00_00_01_TranslationEngine.cs` | 번역 엔진 | `TryTranslate(text, out translated)` |
| `00_00_02_ScopeManager.cs` | 스코프 관리 | `PushScope()`, `PopScope()`, `GetDepth()` |
| `00_00_03_LocalizationManager.cs` | JSON 로더 | `GetTerm()`, `TryGetAnyTerm()`, `GetCategory()` |
| `00_00_99_QudKREngine.cs` | 조사 처리 | `HasJongsung()`, `ResolveJosa()` |

### Object Layer

| 파일 | 역할 | 주요 API |
|------|------|----------|
| `02_20_00_ObjectTranslator.cs` | 오브젝트 번역 | `TryGetDisplayName()`, `TryGetDescription()` |
| `02_20_01_DisplayNamePatch.cs` | 이름 패치 | Harmony Postfix |
| `02_20_02_DescriptionPatch.cs` | 설명 패치 | Harmony Postfix |

---

## 번역 파이프라인

### UI 번역 (TranslationEngine)

```
원본: "{{C|20}} bonus skill points"
  ↓
1. 전처리: 공백 제거, 태그 정규화
  ↓
2. 접두사 추출: 체크박스 [■], 핫키 [A] 등 → 보관
  ↓
3. 색상 태그 제거: "20 bonus skill points"
  ↓
4. 사전 검색: 원본 → UPPER → Title → lower
  ↓
5. 태그 복원 + 접두사 복원
  ↓
결과: "{{C|20}} 레벨당 보너스 기술 포인트"
```

### 오브젝트 번역 (ObjectTranslator)

```
원본: "wooden arrow x15 (lit)"
  ↓
1. 캐시 확인 (빠른 경로)
  ↓
2. 접미사 분리: "x15 (lit)" → 보관
  ↓
3. 접두사 분리: "wooden" → "나무"
  ↓
4. 기본명 검색: "arrow" → "화살"
  ↓
5. 접미사 번역: "(lit)" → "(점화됨)"
  ↓
결과: "나무 화살 x15 (점화됨)"
```

---

## 스코프 스택

```
┌─────────────────────────────────┐
│ Stack[2]: 팝업 (UI/common)      │ ← 최우선 검색
├─────────────────────────────────┤
│ Stack[1]: 캐릭터 생성 (CHARGEN) │
├─────────────────────────────────┤
│ Stack[0]: 전역 (UI/common)      │ ← 베이스
└─────────────────────────────────┘

// 사용법
ScopeManager.PushScope(LocalizationManager.GetCategory("chargen"));
// ... UI 렌더링 ...
ScopeManager.PopScope();  // 반드시 Pop!
```

---

## 위험 필드 (번역 금지)

| 클래스 | 필드 | 이유 | 안전한 지점 |
|--------|------|------|-------------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` 사용 | UI Postfix만 |
| `ChoiceWithColorIcon` | `Id` | 선택 로직에 사용 | `Title`만 번역 |

---

## 폴더 구조

```
Scripts/
├── 00_Core/                    # 핵심 시스템 (7개)
│   ├── 00_00_00_ModEntry.cs    # 진입점
│   ├── 00_00_01_TranslationEngine.cs
│   ├── 00_00_02_ScopeManager.cs
│   ├── 00_00_03_LocalizationManager.cs
│   ├── 00_00_04_TMPFallbackFontBundle.cs
│   ├── 00_00_05_GlossaryExtensions.cs
│   ├── 00_00_06_G.cs
│   └── 00_00_99_QudKREngine.cs
│
├── 02_Patches/
│   ├── 00_Core/                # 플랫폼 패치
│   ├── 10_UI/                  # UI 패치 (23개)
│   │   ├── 02_10_00_GlobalUI.cs
│   │   ├── 02_10_01_Options.cs
│   │   ├── 02_10_10_CharacterCreation.cs
│   │   ├── 02_10_19_AbilityBar.cs        # NEW
│   │   ├── 02_10_20_StatHelpText.cs      # NEW
│   │   ├── 02_10_21_ActivatedAbilities.cs # NEW
│   │   ├── 02_10_22_EquipmentSlots.cs    # NEW
│   │   ├── 02_10_23_StatusFormat.cs      # NEW
│   │   ├── 02_10_24_StatAbbreviations.cs # NEW
│   │   ├── 02_10_25_SkillsScreen.cs     # NEW
│   │   ├── 02_10_26_PlayerStatusBar.cs   # NEW
│   │   ├── 02_10_27_WeightUnit.cs        # NEW
│   │   └── ...
│   └── 20_Objects/             # 오브젝트 패치 (4개)
│       ├── 02_20_00_ObjectTranslator.cs
│       ├── 02_20_01_DisplayNamePatch.cs
│       ├── 02_20_02_DescriptionPatch.cs
│       └── 02_20_99_DebugWishes.cs
│
└── 99_Utils/                   # 유틸리티 (4개)
    ├── 99_00_01_TranslationUtils.cs
    ├── 99_00_02_ChargenTranslationUtils.cs
    ├── 99_00_03_StructureTranslator.cs
    └── 99_00_04_PerfCounters.cs

LOCALIZATION/
├── CHARGEN/      # 캐릭터 생성
├── GAMEPLAY/     # 게임플레이
├── OBJECTS/      # 오브젝트 (아이템/생물)
└── UI/           # UI 텍스트
```
