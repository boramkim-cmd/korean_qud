# 잔여 영어 UI 번역 설계 문서 (v2)

> 2026-01-31 | 게임 플레이 화면에 남아있는 영어 텍스트 한글화
> v2: 비판적 리뷰 + 코드 검증 결과 반영

---

## 0. 설계 원칙

### 0-1. 쿼드 세계관 용어 규칙

| 규칙 | 예시 |
|------|------|
| 고유명사(인명/지명)는 음차 | Joppa → 조파, Artifex → 기술자(기존), Shwut Ux → 슈우트 욱스 |
| 게임 고유 단위는 원문 유지 또는 음차 | dram → 드램, # → # (파운드 약어) |
| 기술/RPG 용어는 의역 | True Kin → 순수 인간(기존), Mutated Human → 변이된 인간(기존) |
| 상태 텍스트는 간결한 한글 | Sated → 포만, Quenched → 해갈 |

### 0-2. UI 깨짐 방지 원칙

1. **고정폭 영역 글자 수 검증**: 상단 바, 능력 바 등은 픽셀 폭 제한 있음. 한글 2바이트 문자가 영어보다 넓으므로 **번역 후 실제 렌더링 폭 확인** 필수
2. **색상 태그 완전 보존**: `{{W|...}}`, `{{C|...}}`, `{{K|...}}`, `<color=...>` 모두 유지
3. **포맷 문자열 보존**: `{0}`, `{1}` 등 플레이스홀더 위치/개수 변경 금지
4. **약어 유지**: STR, AGI, AV, DV, MA, QN, MS, AR, ER, CR, HR — 박스 내 표시되는 약어는 영어 유지 (이미 적용됨)
5. **StringBuilder 주입 시 문자열 길이**: Replace 방식은 원본보다 한글이 짧으면 안전. 길면 레이아웃 확인
6. **null/empty 체크**: 번역 실패 시 원문 반환, 빈 문자열 반환 금지

### 0-3. 무게 단위 시스템

게임 내 무게 표시 형태 (모두 **파운드 기반**, 톤 변환 없음):

| 형태 | 소스 | 예시 |
|------|------|------|
| `X lbs.` | Description.cs:156, InventoryLine.cs:243 | `[12 lbs.]` |
| `X/Y lbs.` | InventoryAndEquipmentStatusScreen.cs:575 | `121/300 lbs.` |
| `#X/Y` | Sidebar.cs:568 | `#121/300` |
| `X/Y#` | PlayerStatusBar.cs:326 | `121/300#` |

**번역 전략**:
- `lbs.` → 한국 표기 고려. 선택지:
  - (A) `lbs.` 유지 — 게임 고유 느낌 보존, 기존 사용자 혼란 없음
  - (B) `파운드` — 한글화 일관성, 하지만 길이 증가
  - (C) `#` 기호 통일 — 게임 내 이미 `#`을 파운드 약어로 사용 중
- **권장: (A) `lbs.` 유지** — 게임의 고유 세계관 단위처럼 취급. 무게 숫자가 중요하고 단위는 맥락으로 이해 가능. 길이 문제도 없음.
- `Weight:` 라벨만 `무게:`로 번역

---

## 1. 미번역 항목 목록 및 소스 위치

### A. 속성 설명 텍스트 (16개) — 새 패치 필요

**소스**: `Statistic.cs` → `GetHelpText()` 메서드 (line 368-435)

**중요 발견**: `stats.json`에 6대 속성 설명이 이미 번역되어 있지만, 이는 **캐릭터 생성 화면용** 텍스트(Genotypes.xml)이며 `GetHelpText()` 반환값과 **내용이 다르다**.

| Name 키 | stats.json 키 (캐릭생성) | GetHelpText 반환값 (인게임) |
|---------|------------------------|--------------------------|
| Strength | "your {{w\|strength}} **score** determines..." | "Your {{W\|Strength}} determines how much **melee damage**..." |

→ 동일 속성이지만 **다른 텍스트**이므로 새 번역 필요. 단, 기존 stats.json 번역의 **어조와 용어**를 최대한 일치시킨다.

**전체 목록** (16개 — Temperature 포함):

| Name 키 | 영문 요약 |
|---------|----------|
| Strength | melee damage, armor penetration, forced movement, carry capacity |
| Agility | accuracy, dodge attacks |
| Toughness | hit points, regeneration, poison/disease resist |
| Intelligence | skill points, examine artifacts |
| Willpower | cooldowns, mental attack resist, HP regen |
| Ego | mental mutations, haggle, dominate |
| AV | protection against physical attacks |
| DV | chance to be hit, Agility modifier |
| MA | mental attack protection, Willpower modifier |
| Speed | action speed, base 100 |
| MoveSpeed | walk/fly speed, base 100 |
| T | temperature, cold reduces Quickness, hot causes fire |
| AcidResistance | acid damage ablation, base 0, immune at 100 |
| ColdResistance | cold damage, temperature reduction insulation |
| ElectricResistance | electrical damage, electric current resistance |
| HeatResistance | heat damage, temperature increase insulation |

**번역 전략**:
- 새 패치: `02_10_20_StatHelpText.cs`
- `[HarmonyPostfix]` on `Statistic.GetHelpText()`
- **`Name` 값을 키로** JSON 조회 (전체 문장을 키로 쓰면 원문 변경 시 깨짐)
- JSON: `LOCALIZATION/UI/stat_help.json`

```csharp
[HarmonyPatch(typeof(XRL.World.Statistic), nameof(XRL.World.Statistic.GetHelpText))]
public static class Patch_Statistic_GetHelpText
{
    private static Dictionary<string, string> _helpTexts;

    [HarmonyPostfix]
    static void Postfix(Statistic __instance, ref string __result)
    {
        if (string.IsNullOrEmpty(__result)) return;
        if (_helpTexts == null)
            _helpTexts = LocalizationManager.GetCategory("stat_help");
        if (_helpTexts != null && _helpTexts.TryGetValue(__instance.Name, out var ko))
            __result = ko;
    }
}
```

**JSON 예시** (`LOCALIZATION/UI/stat_help.json`):
```json
{
  "stat_help": {
    "Strength": "{{W|힘}}은 근접 피해량(방어 관통력 향상), 강제 이동 저항력, 운반 용량을 결정합니다.",
    "Agility": "{{W|민첩}}은 근접 및 원거리 무기의 명중률과 공격 회피 확률을 결정합니다.",
    "Toughness": "{{W|건강}}은 체력, 체력 재생 속도, 독과 질병에 대한 저항력을 결정합니다.",
    "Intelligence": "{{W|지능}}은 기술 포인트 수와 유물 감정 능력을 결정합니다.",
    "Willpower": "{{W|의지}}는 활성 능력의 재사용 대기시간, 정신 공격 저항력, 체력 재생 속도를 조절합니다.",
    "Ego": "{{W|자아}}는 정신 변이의 위력, 상인과의 흥정 능력, 다른 생명체의 의지를 지배하는 능력을 결정합니다.",
    "AV": "{{W|방어력(AV)}}은 물리 공격에 대한 방어 수준을 나타냅니다. 수치가 높을수록 적의 공격이 방어구를 관통하여 피해를 입히는 횟수가 줄어듭니다. 기본 AV는 0입니다.",
    "DV": "{{W|회피력(DV)}}은 물리 공격에 맞을 확률을 나타냅니다. 수치가 높을수록 적의 공격이 빗나갈 확률이 높아집니다. DV는 민첩 보정치의 영향을 받습니다. 기본 DV는 6입니다.",
    "MA": "{{W|정신 방어력(MA)}}은 정신 공격에 대한 방어 수준을 나타냅니다. 수치가 높을수록 적의 정신 공격이 방어를 뚫고 피해를 입힐 확률이 낮아집니다. MA는 의지 보정치의 영향을 받습니다. 기본 MA는 4입니다.",
    "Speed": "{{W|속도}}는 모든 행동을 수행하는 빠르기를 나타냅니다. 기본 속도는 100입니다.",
    "MoveSpeed": "{{W|이동 속도}}는 걷거나 나는 빠르기를 나타냅니다. 기본 이동 속도는 100입니다.",
    "T": "{{W|온도}}는 현재 체온 상태를 나타냅니다. 너무 차가우면 속도가 감소하고 물리 행동이 제한될 수 있습니다. 너무 뜨거우면 불이 붙어 매 턴 피해를 받습니다.",
    "AcidResistance": "{{W|산 저항}}은 산성 피해 경감량을 결정합니다. 기본 산 저항 수치는 0입니다. 100이면 산성 피해에 면역입니다.",
    "ColdResistance": "{{W|냉기 저항}}은 냉기 피해 경감량과 체온 감소 효과에 대한 절연력을 결정합니다. 기본 냉기 저항 수치는 0입니다. 100이면 냉기 피해에 면역이며 체온이 감소하지 않습니다.",
    "ElectricResistance": "{{W|전기 저항}}은 전기 피해 경감량과 전류에 대한 저항력을 결정합니다. 기본 전기 저항 수치는 0입니다. 100이면 전기 피해에 면역이며 전기를 전도하지 않습니다.",
    "HeatResistance": "{{W|열 저항}}은 열 피해 경감량과 체온 상승 효과에 대한 절연력을 결정합니다. 기본 열 저항 수치는 0입니다. 100이면 열 피해에 면역이며 체온이 상승하지 않습니다."
  }
}
```

**번역 시 기존 stats.json과 어조 통일점**:
- 속성명: `_SHARED/attributes.json` 기준 (힘, 민첩, 건강, 지능, 의지, 자아)
- 문체: "~을 결정합니다", "~을 나타냅니다" (stats.json의 "~을 결정합니다" 패턴 유지)
- 게임 용어: hit points → 체력, skill points → 기술 포인트, armor penetration → 방어 관통 (기존 번역 일관)

---

### B. HUD 하단 바 (AbilityBar) — 새 패치 필요

**소스**: `AbilityBar.cs`

| 메서드 | 접근 | 텍스트 | 번역 |
|--------|------|--------|------|
| `InternalUpdateActiveEffects` (private, void) | line 260 | `ACTIVE EFFECTS:` | `활성 효과:` |
| `AfterRender` (private, void) | line 380 | `TARGET: {DisplayName}` | `대상: {DisplayName}` |
| `AfterRender` (private, void) | line 412 | `TARGET: [none]` | `대상: [없음]` |
| `UpdateAbilitiesText` (public, void) | line 723,730 | `ABILITIES` / `page X of Y` | `능력` / `X/Y 페이지` |
| (능력 상태) | line 569 | `[disabled]` | `[비활성]` |
| (토글 상태) | line 579 | `[on]` | `[켜짐]` |
| (토글 상태) | line 583 | `[off]` | `[꺼짐]` |

**번역 전략** (이전 리뷰에서 수정):
- 모든 메서드가 **void 반환** → `ref string __result` 불가
- **인스턴스 필드 직접 수정** 방식 사용

```csharp
// ACTIVE EFFECTS — InternalUpdateActiveEffects Postfix
[HarmonyPatch(typeof(AbilityBar), "InternalUpdateActiveEffects")]
static void Postfix(AbilityBar __instance)
{
    // Reflection으로 effectText 필드 접근 후 Replace
    // 또는 Traverse.Create(__instance).Field("effectText")
    var field = AccessTools.Field(typeof(AbilityBar), "effectText");
    if (field != null)
    {
        string val = (string)field.GetValue(__instance);
        if (val != null && val.Contains("ACTIVE EFFECTS:"))
            field.SetValue(__instance, val.Replace("ACTIVE EFFECTS:", "활성 효과:"));
    }
}

// ABILITIES — UpdateAbilitiesText Postfix
// UITextSkin.text에 직접 설정하므로 기존 TMP_Text 전역 패치가 잡을 수 있음
// → 먼저 common.json에 "ABILITIES" 추가 테스트, 실패 시 직접 패치
```

**UI 깨짐 위험 항목**:
- `ACTIVE EFFECTS:` (15자) → `활성 효과:` (5자) — 짧아져서 안전
- `TARGET:` (7자) → `대상:` (3자) — 안전
- `ABILITIES` (9자) → `능력` (2자) — 안전, 단 `page X of Y` → `X/Y 페이지` 길이 확인
- `[disabled]` (10자) → `[비활성]` (5자) — 안전

---

### C. 능력 이름 (5개+) — 새 패치 + 용어 통일 필요

**검증 결과**: `activated_abilities.json`에 능력명 번역 **없음**. `ActivatedAbilityEntry.DisplayName` 패치도 **없음**.

**기존 용어 충돌 발견**:

| 능력 | 기존 번역 1 | 기존 번역 2 | 위치 |
|------|------------|------------|------|
| Sprint | 질주 | 달리기 | common.json / tutorial |
| Make Camp | 캠프 설치 | — | Wayfaring.json |
| Deploy Turret | 포탑 배치 | — | Tinkering.json |
| Rebuke Robot | (없음, 설명만) | — | — |
| Recharge | (없음, 복합어만) | — | — |

**용어 결정 (권장)**:

| 영문 | 한글 | 근거 |
|------|------|------|
| Sprint | **질주** | common.json 기존 + 짧고 직관적 |
| Make Camp | **캠프 설치** | Wayfaring.json 기존 |
| Rebuke Robot | **로봇 복종** | chargen ui "로봇을 복종시킬 수 있음" 기반 |
| Deploy Turret | **포탑 배치** | Tinkering.json 기존 |
| Recharge | **충전** | "주변광 충전 속도" 기존 패턴 |

**번역 전략**:
- `AddMyActivatedAbility`의 Postfix로 `DisplayName` 교체, 또는
- `ActivatedAbilityEntry` 생성/렌더링 시점에서 교체
- JSON: `LOCALIZATION/GAMEPLAY/ability_names.json`
- 기존 `tutorial`, `common.json`의 충돌 항목도 통일 (Sprint → 질주로 통일)

---

### D. 장비 슬롯명 — 조합 메서드 특정 필요

**소스**: `Bodies.xml`의 `DescriptionPrefix` + `Type` + `Laterality` 런타임 조합

**기존 JSON 상태**:
- `_SHARED/body_parts.json`: Arm → 팔, Hands → 손, Back → 등 (있음)
- "Worn on" 프리픽스 번역: **없음**
- "Left"/"Right" Laterality 번역: **없음**

**번역 제안**:

| 영문 | 한글 | 비고 |
|------|------|------|
| Worn on Hands | 손 착용 | |
| Worn on Back | 등 착용 | |
| Left Arm | 왼쪽 팔 | |
| Right Arm | 오른쪽 팔 | |
| Left Missile Weapon | 왼쪽 투사 무기 | |
| Right Missile Weapon | 오른쪽 투사 무기 | |
| Left Hand | 왼손 | `*Left Hand` 아니고 `왼쪽 손`보다 자연스러움 |
| Right Hand | 오른손 | |
| Floating Nearby | 부유 아이템 | body_parts.json 기존 |

**조합 로직 특정 (미해결)**:
`DescriptionPrefix + " " + Type` 조합이 어느 C# 메서드에서 실행되는지 아직 미확인. 구현 전 반드시:
1. `Assets/core_source/`에서 `DescriptionPrefix`를 참조하는 코드 검색
2. 해당 메서드에 Postfix 적용 또는 결과 문자열을 전역 패치로 잡기

---

### E. 상태 화면 UI — KeyMenuOption 패치 경로 확인됨

**검증 결과**: `KeyMenuOption.setDataMenuOption` Postfix가 **이미 존재** (`02_10_01_Options.cs:345-389`). `"options"`과 `"common"` 카테고리를 검색.

→ `common.json`에 추가하면 `Show Effects`, `Buy Mutation` 등이 **잡힌다**.

| 텍스트 | 처리 방법 | 필요 조치 |
|--------|-----------|-----------|
| `Show Effects` | KeyMenuOption 패치가 잡음 | `common.json`에 추가 |
| `Buy Mutation` | KeyMenuOption 패치가 잡음 | `common.json`에 추가 |
| `navigation` | KeyMenuOption 패치가 잡음 | `common.json`에 추가 |
| `Set Primary Limb` | KeyMenuOption 패치가 잡음 | `common.json`에 추가 |
| `Show Tooltip` | KeyMenuOption 패치가 잡음 | `common.json`에 추가 |
| `Attribute Points: {N}` | `string.Format()` 결과가 TMP_Text에 설정 | 포맷 문자열 패치 필요 (별도) |
| `Level: 1 ¯ HP: 14/14 ¯ XP: 0/220 ¯ Weight: 5T2#` | `string.Format()` 결과 | 포맷 문자열 패치 필요 (별도) |
| `True Kin Artifex` | `GetGenotype() + " " + GetSubtype()` | Genotype/Subtype 반환값 패치 또는 결합 시점 패치 |

**JSON 추가 내용** (`UI/common.json`):
```json
"Show Effects": "효과 보기",
"Buy Mutation": "변이 구매",
"navigation": "이동",
"Set Primary Limb": "주 사용 부위 설정",
"Show Tooltip": "도움말 보기"
```

---

### F. show cybernetics — JSON만으로 불가, 별도 패치 필요

**근본 원인 확인됨**: `InventoryAndEquipmentStatusScreen.cs:590`에서 `{{hotkey|[~Toggle]}} show cybernetics` 형태로 설정. TMP_Text 전역 패치가 태그를 벗긴 후 `"[~Toggle] show cybernetics"`가 되어 정확한 키 매칭 실패.

**해결 방법**:
- `InventoryAndEquipmentStatusScreen`에 새 Postfix 패치
- 또는 TMP_Text 패치의 전처리에서 hotkey 태그 뒤 부분을 분리하여 번역

---

### G. 상단 바 상태 텍스트 — 세계관 용어

스크린샷 상단 바: `Sated Quenched`, `Harvest Dawn 2nd of Shwut Ux`, `Joppa`

| 텍스트 | 현재 상태 | 번역 |
|--------|-----------|------|
| Sated | 미번역 | **포만** |
| Quenched | `options.json`에 "충분히 마심" | **해갈** (짧게) |
| Harvest Dawn | 미번역 | **수확의 새벽** (쿼드 달력 시간대) |
| 2nd of Shwut Ux | 미번역 | **슈우트 욱스 2일** (쿼드 달력 월) |
| Joppa | 기존 번역 "조파" | **조파** |

**주의**: 달력/시간 시스템은 별도 대규모 작업. 이 문서에서는 범위 외로 표기하되 목록에 기록.

---

### H. 기타

| 텍스트 | 소스 | 처리 |
|--------|------|------|
| `Message log` | MessageLogStatusScreen.cs:99 | `common.json`에 이미 "메시지 기록" 존재 (line 135). 적용 안 되면 별도 패치 |
| `You have no missile weapons equipped.` | MissileWeaponArea.cs:219 | TMP_Text 패치 경로 확인 후 JSON 추가 또는 별도 패치 |
| `lbs.` (인벤토리 무게) | 4곳 | 유지 (0-3 무게 단위 정책 참조) |

---

## 2. 구현 우선순위

### Phase 1: JSON 추가 (패치 불필요, 즉시 적용)
- `common.json`에 추가: `Show Effects`, `Buy Mutation`, `navigation`, `Set Primary Limb`, `Show Tooltip`
- 기존 `KeyMenuOption.setDataMenuOption` Postfix가 잡음
- `Message log` → 이미 있으므로 적용 여부 확인만

### Phase 2: 속성 설명 (가장 눈에 띄는 미번역, 16개)
- 새 파일: `02_10_20_StatHelpText.cs`
- 새 JSON: `LOCALIZATION/UI/stat_help.json`
- `Name` 키 기반 조회 (전체 문장 키 아님)
- 기존 `stats.json` 어조/용어와 통일

### Phase 3: HUD AbilityBar
- 새 파일: `02_10_19_AbilityBar.cs`
- 인스턴스 필드 직접 수정 방식 (void 메서드이므로)
- `ACTIVE EFFECTS:`, `TARGET:`, `[on]`/`[off]`/`[disabled]`, `page X of Y`
- `ABILITIES` → 먼저 JSON 추가로 TMP_Text 패치 테스트, 실패 시 직접 패치

### Phase 4: 능력 이름
- 새 JSON: `LOCALIZATION/GAMEPLAY/ability_names.json`
- `ActivatedAbilityEntry.DisplayName` 패치 또는 `AddMyActivatedAbility` Postfix
- 기존 용어 충돌 통일 (Sprint → 질주)

### Phase 5: 장비 슬롯명
- 조합 메서드 특정 후 패치
- `Worn on` + Laterality 번역
- `_SHARED/body_parts.json` 확장

### Phase 6: 상태 화면 포맷 문자열 + show cybernetics
- `CharacterStatusScreen` 포맷 문자열 패치
- `True Kin Artifex` → `순수 인간 기술자`
- `InventoryAndEquipmentStatusScreen` show cybernetics 패치

### 범위 외 (별도 작업):
- 쿼드 달력 시스템 (Harvest Dawn, Shwut Ux 등)
- 상태 텍스트 (Sated, Quenched 등) — Sidebar/PlayerStatusBar 패치 필요

---

## 3. 검증된 용어표

`_SHARED/attributes.json` 및 기존 JSON에서 **실제 확인된** 값:

### 주요 속성

| 영어 | 한글 (SSOT) | 출처 |
|------|------------|------|
| Strength | 힘 | _SHARED/attributes.json |
| Agility | 민첩 | _SHARED/attributes.json |
| Toughness | **건강** | _SHARED/attributes.json (~~강인함~~ 아님) |
| Intelligence | 지능 | _SHARED/attributes.json |
| Willpower | 의지 | _SHARED/attributes.json |
| Ego | 자아 | _SHARED/attributes.json |

### 보조 속성

| 영어 | 한글 | 출처 |
|------|------|------|
| Quickness | **속도** | common.json (~~신속~~ 아님) |
| Move Speed | 이동 속도 | common.json |
| Armor Value (AV) | 방어력 | 확인 필요 |
| Dodge Value (DV) | 회피력 | 확인 필요 |
| Mental Armor (MA) | 정신 방어력 | 확인 필요 |

### 저항력

| 영어 | 한글 | 출처 |
|------|------|------|
| Heat Resist | 열 저항 | common.json |
| Cold Resist | 냉기 저항 | common.json |
| Acid Resist | 산 저항 | common.json |
| Electrical Resist | 전기 저항 | common.json |

### 신체 부위 (장비 슬롯 관련)

| 영어 | 한글 | 출처 |
|------|------|------|
| Head | 머리 | _SHARED/body_parts.json |
| Body | 몸통 | _SHARED/body_parts.json |
| Feet | 발 | _SHARED/body_parts.json |
| Face | 얼굴 | _SHARED/body_parts.json |
| Back | 등 | _SHARED/body_parts.json |
| Hands | 손 | _SHARED/body_parts.json |
| Arm | 팔 | _SHARED/body_parts.json |
| Floating Nearby | 부유 아이템 | _SHARED/body_parts.json |

### 세계관 고유명사

| 영어 | 한글 | 출처 |
|------|------|------|
| True Kin | 순수 인간 | GENOTYPES/True_Kin.json |
| Mutated Human | 변이된 인간 | GENOTYPES/Mutated_Human.json |
| Artifex | 기술자 | SUBTYPES/Castes/Artifex.json |
| Joppa | 조파 | locations.json |
| dram | 드램 (음차) | _suffixes.json 주석 |

### 능력 이름 (통일안)

| 영어 | 한글 | 근거 |
|------|------|------|
| Sprint | 질주 | common.json 기존 |
| Make Camp | 캠프 설치 | Wayfaring.json 기존 |
| Rebuke Robot | 로봇 복종 | chargen ui 기반 |
| Deploy Turret | 포탑 배치 | Tinkering.json 기존 |
| Recharge | 충전 | 기존 패턴 |

---

## 4. 검증 체크리스트

### Phase 1 (JSON 추가)
- [ ] `common.json`에 5개 항목 추가
- [ ] `python3 tools/project_tool.py` 통과
- [ ] 게임 실행 → 속성 화면 하단 `효과 보기`, `변이 구매` 확인
- [ ] 인벤토리 하단 `주 사용 부위 설정`, `도움말 보기` 확인

### Phase 2 (속성 설명)
- [ ] `stat_help.json` 16개 항목 작성
- [ ] `02_10_20_StatHelpText.cs` 컴파일 오류 없음
- [ ] 게임 실행 → 속성 화면에서 각 속성 선택 시 한글 설명 표시
- [ ] 색상 태그 `{{W|...}}` 정상 렌더링
- [ ] 용어가 stats.json (캐릭생성)과 일관성 유지

### Phase 3 (AbilityBar)
- [ ] `02_10_19_AbilityBar.cs` 컴파일 오류 없음
- [ ] `활성 효과:` 표시, 레이아웃 깨짐 없음
- [ ] `대상: [없음]` 표시
- [ ] 능력 토글 `[켜짐]`/`[꺼짐]`/`[비활성]` 표시
- [ ] `능력` 헤더, 페이지 표시

### Phase 4 (능력 이름)
- [ ] 능력 바에서 `질주`, `캠프 설치`, `충전` 등 한글 표시
- [ ] 기존 tutorial "달리기" → "질주"로 통일 여부 결정 및 적용

### Phase 5 (장비 슬롯)
- [ ] 인벤토리 장비 슬롯 `손 착용`, `등 착용`, `왼쪽 팔` 등 표시
- [ ] 장비 슬롯 길이로 인한 레이아웃 깨짐 없음

### Phase 6 (포맷 문자열)
- [ ] `순수 인간 기술자` 표시
- [ ] `사이버네틱스 보기` 표시
- [ ] `레벨: 1 ¯ 체력: 14/14 ¯ 경험치: 0/220 ¯ 무게: 121#` 표시
