# 템플릿 변수 (Template Variables) 문서

> **최종 업데이트:** 2026-01-27

---

## 개요

Caves of Qud는 XML의 DisplayName과 Description에서 `=variableName=` 형식의 **템플릿 변수**를 사용합니다. 이 변수들은 **런타임에 게임 엔진이 동적으로 대체**하므로, 대부분 직접 번역할 수 없습니다.

### 예시

```xml
<!-- XML 원본 -->
<part Name="Render" DisplayName="=creatureRegionAdjective= kudu" />

<!-- 런타임 결과 (Saltmarsh 지역) -->
DisplayName = "marsh kudu"

<!-- 런타임 결과 (Hills 지역) -->
DisplayName = "hill kudu"
```

---

## 통계 요약

| 항목 | 값 |
|------|-----|
| 총 변수 유형 | 34개 |
| 총 사용 횟수 | 1,198회 |
| 번역 가능 카테고리 | 1개 (creature_region) |
| DisplayName에서 사용 | 58개 항목 |

---

## 카테고리별 상세

### 1. 대명사 (Pronouns) - 990회

캐릭터/오브젝트의 성별과 유형에 따라 동적으로 결정됩니다.

| 변수 | 설명 | 예시 값 |
|------|------|---------|
| `=pronouns.possessive=` | 소유격 (소문자) | his, her, its, their |
| `=pronouns.Possessive=` | 소유격 (대문자) | His, Her, Its, Their |
| `=pronouns.subjective=` | 주격 (소문자) | he, she, it, they |
| `=pronouns.Subjective=` | 주격 (대문자) | He, She, It, They |
| `=pronouns.objective=` | 목적격 | him, her, it, them |
| `=pronouns.reflexive=` | 재귀대명사 | himself, herself, itself |
| `=pronouns.personTerm=` | 인칭 호칭 | man, woman, person |
| `=pronouns.IndicativeProximal=` | 지시대명사 | this, that |

**소스:** 게임 엔진의 Gender/Pronouns 시스템
**번역 가능:** X (영어 문법 구조에 종속)

---

### 2. 주체/객체 참조 (Subject/Object References) - 68회

문장에서 주어와 목적어를 동적으로 참조합니다.

| 변수 | 설명 | 예시 |
|------|------|------|
| `=subject.T=` | 주어 (대문자, 관사 포함) | The snapjaw |
| `=subject.t=` | 주어 (소문자) | the snapjaw |
| `=subject.possessive=` | 주어의 소유격 | the snapjaw's |
| `=subject.directionIfAny=` | 주어의 방향 | to the north |
| `=object.t=` | 목적어 | the door |
| `=object.name=` | 목적어 이름 | iron door |

**소스:** 게임 엔진 (문맥에서 결정)
**번역 가능:** X (참조된 오브젝트의 DisplayName이 번역되어 있으면 자동 적용)

---

### 3. 플레이어 참조 (Player References) - 12회

플레이어 캐릭터 정보를 참조합니다.

| 변수 | 설명 |
|------|------|
| `=player.possessive=` | 플레이어 소유격 |
| `=player.subjective=` | 플레이어 주격 (소문자) |
| `=player.Subjective=` | 플레이어 주격 (대문자) |
| `=player.reflexive=` | 플레이어 재귀대명사 |
| `=player.formalAddressTerm=` | 공식 호칭 |

**소스:** 플레이어 캐릭터 설정
**번역 가능:** X

---

### 4. 지역별 생물 수식어 (Creature Region) - 58회 ⭐

**유일하게 번역 가능한 카테고리입니다.**

| 변수 | 설명 |
|------|------|
| `=creatureRegionAdjective=` | 지역 형용사 |
| `=creatureRegionNoun=` | 지역 명사 |

**소스:** `Assets/StreamingAssets/Base/HistorySpice.json`
**핸들러:** `CreatureRegionSpice.cs`

#### 지역별 값

##### Saltmarsh (염습지)
```
creatureRegionAdjective: marsh, brine, lily, glade, mud, saltwater, sedge
creatureRegionNoun: marshdweller, brinestalker, lilyeater, marsheater, mudeater
```

##### DesertCanyon (사막 협곡)
```
creatureRegionAdjective: canyon, trench, valley, cliff, gorge
creatureRegionNoun: gorgedweller, shale-eater, cliffling, sloper, erosionist
```

##### Hills (언덕)
```
creatureRegionAdjective: hill, mound, sun, stone, rock, knoll
creatureRegionNoun: lime-eater, hilldweller, sunbaker, stone-eater, hillchild
```

#### 사용 예시 (HiddenObjects.xml)

| XML 패턴 | 결과 예시 |
|----------|----------|
| `=creatureRegionAdjective= kudu` | marsh kudu, canyon kudu, hill kudu |
| `=creatureRegionAdjective= bear` | brine bear, cliff bear, stone bear |
| `baboon =creatureRegionNoun=` | baboon marshdweller, baboon cliffling |
| `=creatureRegionNoun= of the Gyre` | hilldweller of the Gyre |

#### 번역 방법

**옵션 1:** HistorySpice.json 수정 (권장하지 않음 - 게임 업데이트 시 덮어씌워짐)

**옵션 2:** Harmony 패치로 CreatureRegionSpice 이후 번역 적용 (권장)

---

### 5. 오브젝트 자기 참조 (Object Self References) - 41회

현재 오브젝트 자신을 참조합니다.

| 변수 | 설명 | 예시 |
|------|------|------|
| `=name=` | 오브젝트 이름 | Argyve |
| `=this.an=` | 관사 + 이름 | a waterskin |
| `=this.refname=` | 참조 이름 | the waterskin |

**소스:** 현재 오브젝트의 DisplayName
**번역 가능:** X (DisplayName이 번역되어 있으면 자동 적용)

---

### 6. 시간/장소 (Time/Location) - 10회

게임 월드의 시간과 장소 정보입니다.

| 변수 | 설명 | 예시 값 |
|------|------|---------|
| `=day=` | 요일 | Tishrei |
| `=month=` | 달 | Ut yara Ux |
| `=year=` | 연도 | 1200 |
| `=landmark.nearest=` | 가장 가까운 랜드마크 | Joppa |
| `=terrain.t=` | 지형 | the salt marsh |
| `=dirward=` | 방향 | northward |

**소스:** 게임 월드 상태
**번역 가능:** X (별도 시스템에서 처리 필요)

---

### 7. 팩션 참조 (Faction References) - 2회

플레이어와 팩션 관계를 참조합니다.

| 변수 | 설명 |
|------|------|
| `=mostHatedFaction.t=` | 가장 적대적인 팩션 |
| `=secondMostHatedFaction.t=` | 두 번째로 적대적인 팩션 |

**소스:** 플레이어 팩션 관계
**번역 가능:** X (팩션 이름이 번역되어 있으면 자동 적용)

---

### 8. 골렘 퀘스트 (Golem Quest) - 15회

골렘 제작 퀘스트에서 플레이어가 선택한 재료 정보입니다.

| 변수 | 설명 |
|------|------|
| `=body.features=` | 선택한 신체 특징 |
| `=catalyst=` | 선택한 촉매 |
| `=hamsa.an=` | 선택한 함사 (관사 포함) |
| `=atzmus.creature=` | 선택한 아츠무스 생물 |
| `=atzmus.creature.an=` | 선택한 아츠무스 생물 (관사 포함) |
| `=armament.skill.snippet=` | 선택한 무장 스킬 |

**소스:** GolemQuest 시스템 (`GolemAtzmusSelection.cs`, `GolemCatalystSelection.cs` 등)
**번역 가능:** X (선택된 오브젝트의 DisplayName이 번역되어 있으면 자동 적용)

---

### 9. 기타 (Misc) - 2회

| 변수 | 설명 |
|------|------|
| `=gas=` | 가스 종류 |
| `=verb:bat=` | 동사 변환 (bat → bats/batted) |

---

## 번역 커버리지 계산 시 주의사항

템플릿 변수를 포함한 DisplayName은 번역 커버리지 계산에서 **제외**해야 합니다.

```python
# 예시: 템플릿 변수 포함 여부 확인
def has_template_variable(text):
    return '=' in text and text.count('=') >= 2

# 제외 대상 (58개)
excluded = [name for name in display_names if has_template_variable(name)]
```

---

## 기술적 세부사항

### CreatureRegionSpice.cs 동작 원리

```csharp
public override void Apply(GameObject Object, string Context)
{
    // 1. 현재 지역 확인
    string region = ZoneManager.ZoneGenerationContext?.GetTerrainRegion();

    // 2. HistorySpice.json에서 해당 지역 데이터 로드
    JToken regionData = HistoricSpice.root["history"]["regions"]["terrain"][region];
    JArray adjectives = (JArray)regionData["creatureRegionAdjective"];
    JArray nouns = (JArray)regionData["creatureRegionNoun"];

    // 3. 랜덤 선택
    string adjective = adjectives[random.Next(adjectives.Count)];
    string noun = nouns[random.Next(nouns.Count)];

    // 4. DisplayName의 변수 대체
    render.DisplayName = render.DisplayName
        .AddReplacer("creatureRegionAdjective", adjective)  // 영어!
        .AddReplacer("creatureRegionNoun", noun);           // 영어!
}
```

**문제점:** 변수 대체가 우리 번역 패치 이후에 실행되므로, 결과가 영어로 나옴.

**해결책:** CreatureRegionSpice.Apply() 이후에 실행되는 Postfix 패치 필요.

---

## 관련 파일

| 파일 | 용도 |
|------|------|
| `Assets/StreamingAssets/Base/HistorySpice.json` | 지역 변수 값 정의 |
| `Assets/core_source/.../CreatureRegionSpice.cs` | 지역 변수 처리 |
| `Assets/core_source/.../GolemQuest/*.cs` | 골렘 퀘스트 변수 처리 |
| `LOCALIZATION/OBJECTS/_template_variables.json` | 변수 목록 JSON |

---

## 향후 작업

1. **creature_region 번역 패치 구현**
   - CreatureRegionSpice.Apply() Postfix 패치
   - 어휘 사전에 지역 형용사/명사 추가

2. **시간/장소 시스템 조사**
   - day, month 등의 값이 어디서 정의되는지 확인
   - 필요시 번역 적용

---

## 참고

- 이 문서의 JSON 버전: `LOCALIZATION/OBJECTS/_template_variables.json`
- 관련 이슈: 템플릿 변수 포함 항목은 번역 커버리지에서 제외됨 (58개)
