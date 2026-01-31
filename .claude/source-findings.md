# Source Code Findings (Key References)

> 소스 코드 조사에서 발견한 핵심 정보. 패치 작성/유지보수 시 참조.

## BodyPart & Equipment Slots

- **`BodyPart.GetCardinalDescription()`** (`BodyPart.cs:5729`): `DescriptionPrefix + " " + Description + " (N)"` 형식으로 슬롯명 조합
- **`BodyPart.GetOrdinalDescription()`** (`BodyPart.cs:5757`): `Ordinal(N) + " " + DescriptionPrefix + " " + Description` 형식
- **`BodyPart.ChangeLaterality()`** (`BodyPart.cs:822`): `Laterality.WithLateralityAdjective()`로 Description/Name에 "Left"/"Right" 등 합침
- **`Laterality.BuildLateralityAdjective()`** (`Laterality.cs:70`): int → "Left"/"Right"/"Upper"/"Lower"/"Fore"/"Mid"/"Hind"/"Inside"/"Outside"/"Inner"/"Outer" 변환
- **`DescriptionPrefix`**: XML body part 데이터에서 로드 (예: "Missile Weapon")
- **장비 화면 호출**: `EquipmentLine.cs:304` → `GetCardinalDescription()`, `EquipmentScreen.cs:228` → `GetCardinalDescription()`

## AbilityBar

- **`effectText`** (private string, `AbilityBar.cs:134`): "ACTIVE EFFECTS:" 텍스트
- **`targetText`** (private string, `AbilityBar.cs:144`): "TARGET:" 텍스트
- **`AbilityCommandText`** (public UITextSkin, `AbilityBar.cs:118`): "ABILITIES" + 페이지 정보
- **`PageText`** (TextMeshProUGUI, `AbilityBar.cs:114`): 페이지 번호
- 실제 문자열 패턴:
  - Line 260: `"{{Y|<color=#508d75>ACTIVE EFFECTS:</color>}} "`
  - Line 380: `"{{C|<color=#3e83a5>TARGET:</color> "`
  - Line 412: `"{{K|TARGET: [none]}}"`
  - Line 723: `$"ABILITIES\npage {X} of {Y}"`
- 메서드: `InternalUpdateActiveEffects` (line 235), `AfterRender` (line 283), `UpdateAbilitiesText` (line 711) - 모두 존재 확인

## ActivatedAbilities

- **`AddAbility`** signature (`ActivatedAbilities.cs:463`): 23개 파라미터, 반환형 `Guid`
- **`AbilityByGuid`** (`ActivatedAbilities.cs:181`): `Dictionary<Guid, ActivatedAbilityEntry>`, public, [NonSerialized]
- **`ActivatedAbilityEntry.DisplayName`** (`ActivatedAbilityEntry.cs:40`): public string field

## CharacterStatusScreen

- **`UpdateViewFromData()`** (`CharacterStatusScreen.cs:184`): override
- Line 226: `classText.SetText(GO.GetGenotype() + " " + GO.GetSubtype())`
- Line 227: `levelText.SetText(string.Format("Level: {0} ¯ HP: {1}/{2} ¯ XP: {3}/{4} ¯ Weight: {5}#", ...))`
- Line 228: `attributePointsText.SetText(string.Format("Attribute Points: {0}{1}}}}}", ...))`
- 모든 텍스트 필드: `UITextSkin` 타입 (nameText, classText, levelText, attributePointsText)

## InventoryAndEquipmentStatusScreen

- **`UpdateViewFromData()`** (`InventoryAndEquipmentStatusScreen.cs:468`): override
- Line 575: `weightText.SetText($"...{GO.GetCarriedWeight()}.../{GO.GetMaxCarriedWeight()}}} lbs. }}}}")`
- Line 590: `cyberneticsHotkeySkin.text = (showCybernetics ? "... show equipment" : "... show cybernetics")`
- Line 597: `cyberneticsHotkeySkinForList.text` — 동일 패턴

## Statistic

- **`GetHelpText()`** (`Statistic.cs`): 반환 string, `Name` 프로퍼티로 키 매칭

## LocalizationManager

- `Initialize()` → `LoadAllJsonFiles()` → 재귀적 JSON 로드 (모든 LOCALIZATION/** 하위)
- `_SHARED/*.json` → `LoadSharedJsonFile()` → `SharedJsonParser` (ko/aliases 구조)
- 일반 JSON → `LoadJsonFile()` → flat key-value
- `GetCategory("body_parts")` → `Dictionary<string, string>` (canonical → ko)
