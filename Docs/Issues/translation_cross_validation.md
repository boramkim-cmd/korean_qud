# 번역 교차 검증 리포트

> XML DisplayName과 JSON 번역 항목 간 중복/누락 검증

## 요약

| 항목 | 개수 |
|------|------|
| XML DisplayName | 3006 |
| JSON 번역 항목 | 6613 |
| **매칭 성공** | **832** |
| **미번역** | **2174** |
| 중복 키 (여러 파일) | 128 |
| XML에 없는 JSON | 4698 |

---

## 중복 키 (같은 항목이 여러 파일에 존재)

*동일한 영어 키가 서로 다른 JSON 파일에 번역됨*

### `agolfly`

- `OBJECTS/creatures/_common.json`: `아골파리`
- `OBJECTS/creatures/_common.json`: `아골파리`
- `OBJECTS/creatures/_common.json`: `아골파리`
- `OBJECTS/creatures/_common.json`: `아골파리`
- `OBJECTS/creatures/insects/moths.json`: `아골파리`
- `OBJECTS/creatures/insects/moths.json`: `아골파리`
- `OBJECTS/creatures/insects/moths.json`: `아골파리`
- `OBJECTS/creatures/insects/moths.json`: `아골파리`
- `OBJECTS/creatures/insects/moths.json`: `아골파리`
- `OBJECTS/creatures/insects/moths.json`: `아골파리`
- `OBJECTS/creatures/insects/moths.json`: `아골파리`
- `OBJECTS/creatures/insects/moths.json`: `아골파리`

### `armor`

- `UI/common.json`: `방어구`
- `UI/common.json`: `방어구`
- `UI/common.json`: `방어구`
- `UI/common.json`: `방어구`
- `UI/common.json`: `방어구`
- `UI/common.json`: `방어구`
- `UI/common.json`: `방어구`
- `UI/common.json`: `방어구`
- `OBJECTS/items/_nouns.json`: `갑옷`
- `OBJECTS/items/_nouns.json`: `갑옷`
- `OBJECTS/items/_nouns.json`: `갑옷`
- `OBJECTS/items/_nouns.json`: `갑옷`
- `OBJECTS/items/base_items.json`: `갑옷`
- `OBJECTS/items/base_items.json`: `갑옷`
- `OBJECTS/items/base_items.json`: `갑옷`
- `OBJECTS/items/base_items.json`: `갑옷`
- `OBJECTS/items/base_items.json`: `갑옷`
- `OBJECTS/items/base_items.json`: `갑옷`
- `OBJECTS/items/base_items.json`: `갑옷`
- `OBJECTS/items/base_items.json`: `갑옷`

### `arrow`

- `OBJECTS/items/_nouns.json`: `화살`
- `OBJECTS/items/_nouns.json`: `화살`
- `OBJECTS/items/_nouns.json`: `화살`
- `OBJECTS/items/_nouns.json`: `화살`
- `OBJECTS/items/base_items.json`: `화살`
- `OBJECTS/items/base_items.json`: `화살`
- `OBJECTS/items/base_items.json`: `화살`
- `OBJECTS/items/base_items.json`: `화살`
- `OBJECTS/items/base_items.json`: `화살`
- `OBJECTS/items/base_items.json`: `화살`
- `OBJECTS/items/base_items.json`: `화살`
- `OBJECTS/items/base_items.json`: `화살`
- `OBJECTS/items/ammo/ammo.json`: `화살`
- `OBJECTS/items/ammo/ammo.json`: `화살`
- `OBJECTS/items/ammo/ammo.json`: `화살`
- `OBJECTS/items/ammo/ammo.json`: `화살`
- `OBJECTS/items/ammo/ammo.json`: `화살`
- `OBJECTS/items/ammo/ammo.json`: `화살`
- `OBJECTS/items/ammo/ammo.json`: `화살`
- `OBJECTS/items/ammo/ammo.json`: `화살`
- `OBJECTS/items/weapons/ranged/bows.json`: `화살`
- `OBJECTS/items/weapons/ranged/bows.json`: `화살`
- `OBJECTS/items/weapons/ranged/bows.json`: `화살`
- `OBJECTS/items/weapons/ranged/bows.json`: `화살`
- `OBJECTS/items/weapons/ranged/bows.json`: `화살`
- `OBJECTS/items/weapons/ranged/bows.json`: `화살`
- `OBJECTS/items/weapons/ranged/bows.json`: `화살`
- `OBJECTS/items/weapons/ranged/bows.json`: `화살`

### `axe`

- `UI/terms.json`: `도끼`
- `UI/terms.json`: `도끼`
- `UI/terms.json`: `도끼`
- `UI/terms.json`: `도끼`
- `GAMEPLAY/SKILLS/Axe.json`: `도끼`
- `GAMEPLAY/SKILLS/Axe.json`: `도끼`
- `GAMEPLAY/SKILLS/Axe.json`: `도끼`
- `GAMEPLAY/SKILLS/Axe.json`: `도끼`
- `OBJECTS/items/_nouns.json`: `도끼`
- `OBJECTS/items/_nouns.json`: `도끼`
- `OBJECTS/items/_nouns.json`: `도끼`
- `OBJECTS/items/_nouns.json`: `도끼`
- `OBJECTS/items/base_items.json`: `도끼`
- `OBJECTS/items/base_items.json`: `도끼`
- `OBJECTS/items/base_items.json`: `도끼`
- `OBJECTS/items/base_items.json`: `도끼`
- `OBJECTS/items/base_items.json`: `도끼`
- `OBJECTS/items/base_items.json`: `도끼`
- `OBJECTS/items/base_items.json`: `도끼`
- `OBJECTS/items/base_items.json`: `도끼`

### `baboon`

- `OBJECTS/creatures/_common.json`: `비비`
- `OBJECTS/creatures/_common.json`: `비비`
- `OBJECTS/creatures/_common.json`: `비비`
- `OBJECTS/creatures/_common.json`: `비비`
- `OBJECTS/creatures/animals/mammals.json`: `비비`
- `OBJECTS/creatures/animals/mammals.json`: `비비`
- `OBJECTS/creatures/animals/mammals.json`: `비비`
- `OBJECTS/creatures/animals/mammals.json`: `비비`
- `OBJECTS/creatures/animals/mammals.json`: `비비`
- `OBJECTS/creatures/animals/mammals.json`: `비비`
- `OBJECTS/creatures/animals/mammals.json`: `비비`
- `OBJECTS/creatures/animals/mammals.json`: `비비`

### `backpack`

- `OBJECTS/items/_nouns.json`: `배낭`
- `OBJECTS/items/_nouns.json`: `배낭`
- `OBJECTS/items/_nouns.json`: `배낭`
- `OBJECTS/items/_nouns.json`: `배낭`
- `OBJECTS/items/base_items.json`: `배낭`
- `OBJECTS/items/base_items.json`: `배낭`
- `OBJECTS/items/base_items.json`: `배낭`
- `OBJECTS/items/base_items.json`: `배낭`
- `OBJECTS/items/base_items.json`: `배낭`
- `OBJECTS/items/base_items.json`: `배낭`
- `OBJECTS/items/base_items.json`: `배낭`
- `OBJECTS/items/base_items.json`: `배낭`
- `OBJECTS/items/armor/back.json`: `배낭`
- `OBJECTS/items/armor/back.json`: `배낭`
- `OBJECTS/items/armor/back.json`: `배낭`
- `OBJECTS/items/armor/back.json`: `배낭`
- `OBJECTS/items/armor/back.json`: `배낭`
- `OBJECTS/items/armor/back.json`: `배낭`
- `OBJECTS/items/armor/back.json`: `배낭`
- `OBJECTS/items/armor/back.json`: `배낭`

### `bandage`

- `OBJECTS/items/_nouns.json`: `붕대`
- `OBJECTS/items/_nouns.json`: `붕대`
- `OBJECTS/items/_nouns.json`: `붕대`
- `OBJECTS/items/_nouns.json`: `붕대`
- `OBJECTS/items/base_items.json`: `붕대`
- `OBJECTS/items/base_items.json`: `붕대`
- `OBJECTS/items/base_items.json`: `붕대`
- `OBJECTS/items/base_items.json`: `붕대`
- `OBJECTS/items/base_items.json`: `붕대`
- `OBJECTS/items/base_items.json`: `붕대`
- `OBJECTS/items/base_items.json`: `붕대`
- `OBJECTS/items/base_items.json`: `붕대`

### `Barathrumites`

- `UI/terms.json`: `바라스럼추종자`
- `UI/terms.json`: `바라스럼추종자`
- `UI/terms.json`: `바라스럼추종자`
- `UI/terms.json`: `바라스럼추종자`
- `CHARGEN/factions.json`: `바라트룸의 후예들`
- `CHARGEN/factions.json`: `바라트룸의 후예들`
- `CHARGEN/factions.json`: `바라트룸의 후예들`
- `CHARGEN/factions.json`: `바라트룸의 후예들`

### `bat`

- `OBJECTS/creatures/_common.json`: `박쥐`
- `OBJECTS/creatures/_common.json`: `박쥐`
- `OBJECTS/creatures/_common.json`: `박쥐`
- `OBJECTS/creatures/_common.json`: `박쥐`
- `OBJECTS/creatures/animals/bats.json`: `박쥐`
- `OBJECTS/creatures/animals/bats.json`: `박쥐`
- `OBJECTS/creatures/animals/bats.json`: `박쥐`
- `OBJECTS/creatures/animals/bats.json`: `박쥐`
- `OBJECTS/creatures/animals/bats.json`: `박쥐`
- `OBJECTS/creatures/animals/bats.json`: `박쥐`
- `OBJECTS/creatures/animals/bats.json`: `박쥐`
- `OBJECTS/creatures/animals/bats.json`: `박쥐`

### `beak`

- `OBJECTS/_suffixes.json`: `부리`
- `OBJECTS/_suffixes.json`: `부리`
- `OBJECTS/_suffixes.json`: `부리`
- `OBJECTS/_suffixes.json`: `부리`
- `OBJECTS/_suffixes.json`: ` 부리`
- `OBJECTS/_suffixes.json`: ` 부리`
- `OBJECTS/_suffixes.json`: ` 부리`
- `OBJECTS/_suffixes.json`: ` 부리`
- `GAMEPLAY/MUTATIONS/Physical_Mutations/Beak.json`: `부리`
- `GAMEPLAY/MUTATIONS/Physical_Mutations/Beak.json`: `부리`
- `GAMEPLAY/MUTATIONS/Physical_Mutations/Beak.json`: `부리`
- `GAMEPLAY/MUTATIONS/Physical_Mutations/Beak.json`: `부리`

### `bear`

- `OBJECTS/creatures/_common.json`: `곰`
- `OBJECTS/creatures/_common.json`: `곰`
- `OBJECTS/creatures/_common.json`: `곰`
- `OBJECTS/creatures/_common.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/tutorial.json`: `곰`
- `OBJECTS/creatures/animals/mammals.json`: `곰`
- `OBJECTS/creatures/animals/mammals.json`: `곰`
- `OBJECTS/creatures/animals/mammals.json`: `곰`
- `OBJECTS/creatures/animals/mammals.json`: `곰`
- `OBJECTS/creatures/animals/mammals.json`: `곰`
- `OBJECTS/creatures/animals/mammals.json`: `곰`
- `OBJECTS/creatures/animals/mammals.json`: `곰`
- `OBJECTS/creatures/animals/mammals.json`: `곰`

### `bed`

- `OBJECTS/furniture/misc.json`: `침대`
- `OBJECTS/furniture/misc.json`: `침대`
- `OBJECTS/furniture/misc.json`: `침대`
- `OBJECTS/furniture/misc.json`: `침대`
- `OBJECTS/furniture/misc.json`: `침대`
- `OBJECTS/furniture/misc.json`: `침대`
- `OBJECTS/furniture/misc.json`: `침대`
- `OBJECTS/furniture/misc.json`: `침대`
- `OBJECTS/items/_nouns.json`: `침대`
- `OBJECTS/items/_nouns.json`: `침대`
- `OBJECTS/items/_nouns.json`: `침대`
- `OBJECTS/items/_nouns.json`: `침대`

### `bench`

- `OBJECTS/furniture/misc.json`: `벤치`
- `OBJECTS/furniture/misc.json`: `벤치`
- `OBJECTS/furniture/misc.json`: `벤치`
- `OBJECTS/furniture/misc.json`: `벤치`
- `OBJECTS/furniture/misc.json`: `벤치`
- `OBJECTS/furniture/misc.json`: `벤치`
- `OBJECTS/furniture/misc.json`: `벤치`
- `OBJECTS/furniture/misc.json`: `벤치`
- `OBJECTS/items/_nouns.json`: `벤치`
- `OBJECTS/items/_nouns.json`: `벤치`
- `OBJECTS/items/_nouns.json`: `벤치`
- `OBJECTS/items/_nouns.json`: `벤치`

### `boar`

- `OBJECTS/creatures/_common.json`: `멧돼지`
- `OBJECTS/creatures/_common.json`: `멧돼지`
- `OBJECTS/creatures/_common.json`: `멧돼지`
- `OBJECTS/creatures/_common.json`: `멧돼지`
- `OBJECTS/creatures/animals/mammals.json`: `멧돼지`
- `OBJECTS/creatures/animals/mammals.json`: `멧돼지`
- `OBJECTS/creatures/animals/mammals.json`: `멧돼지`
- `OBJECTS/creatures/animals/mammals.json`: `멧돼지`
- `OBJECTS/creatures/animals/mammals.json`: `멧돼지`
- `OBJECTS/creatures/animals/mammals.json`: `멧돼지`
- `OBJECTS/creatures/animals/mammals.json`: `멧돼지`
- `OBJECTS/creatures/animals/mammals.json`: `멧돼지`

### `boots`

- `OBJECTS/items/_nouns.json`: `부츠`
- `OBJECTS/items/_nouns.json`: `부츠`
- `OBJECTS/items/_nouns.json`: `부츠`
- `OBJECTS/items/_nouns.json`: `부츠`
- `OBJECTS/items/base_items.json`: `장화`
- `OBJECTS/items/base_items.json`: `장화`
- `OBJECTS/items/base_items.json`: `장화`
- `OBJECTS/items/base_items.json`: `장화`
- `OBJECTS/items/base_items.json`: `장화`
- `OBJECTS/items/base_items.json`: `장화`
- `OBJECTS/items/base_items.json`: `장화`
- `OBJECTS/items/base_items.json`: `장화`

### `bow`

- `OBJECTS/items/_nouns.json`: `활`
- `OBJECTS/items/_nouns.json`: `활`
- `OBJECTS/items/_nouns.json`: `활`
- `OBJECTS/items/_nouns.json`: `활`
- `OBJECTS/items/base_items.json`: `활`
- `OBJECTS/items/base_items.json`: `활`
- `OBJECTS/items/base_items.json`: `활`
- `OBJECTS/items/base_items.json`: `활`
- `OBJECTS/items/base_items.json`: `활`
- `OBJECTS/items/base_items.json`: `활`
- `OBJECTS/items/base_items.json`: `활`
- `OBJECTS/items/base_items.json`: `활`

### `canteen`

- `OBJECTS/items/_nouns.json`: `수통`
- `OBJECTS/items/_nouns.json`: `수통`
- `OBJECTS/items/_nouns.json`: `수통`
- `OBJECTS/items/_nouns.json`: `수통`
- `OBJECTS/items/base_items.json`: `수통`
- `OBJECTS/items/base_items.json`: `수통`
- `OBJECTS/items/base_items.json`: `수통`
- `OBJECTS/items/base_items.json`: `수통`
- `OBJECTS/items/base_items.json`: `수통`
- `OBJECTS/items/base_items.json`: `수통`
- `OBJECTS/items/base_items.json`: `수통`
- `OBJECTS/items/base_items.json`: `수통`
- `OBJECTS/items/artifacts/misc.json`: `수통`
- `OBJECTS/items/artifacts/misc.json`: `수통`
- `OBJECTS/items/artifacts/misc.json`: `수통`
- `OBJECTS/items/artifacts/misc.json`: `수통`
- `OBJECTS/items/artifacts/misc.json`: `수통`
- `OBJECTS/items/artifacts/misc.json`: `수통`
- `OBJECTS/items/artifacts/misc.json`: `수통`
- `OBJECTS/items/artifacts/misc.json`: `수통`

### `carapace`

- `OBJECTS/_suffixes.json`: ` 갑각`
- `OBJECTS/_suffixes.json`: ` 갑각`
- `OBJECTS/_suffixes.json`: ` 갑각`
- `OBJECTS/_suffixes.json`: ` 갑각`
- `GAMEPLAY/MUTATIONS/Physical_Mutations/Carapace.json`: `갑피`
- `GAMEPLAY/MUTATIONS/Physical_Mutations/Carapace.json`: `갑피`
- `GAMEPLAY/MUTATIONS/Physical_Mutations/Carapace.json`: `갑피`
- `GAMEPLAY/MUTATIONS/Physical_Mutations/Carapace.json`: `갑피`

### `carbine`

- `OBJECTS/items/_nouns.json`: `카빈총`
- `OBJECTS/items/_nouns.json`: `카빈총`
- `OBJECTS/items/_nouns.json`: `카빈총`
- `OBJECTS/items/_nouns.json`: `카빈총`
- `OBJECTS/items/weapons/ranged/guns.json`: `카빈`
- `OBJECTS/items/weapons/ranged/guns.json`: `카빈`
- `OBJECTS/items/weapons/ranged/guns.json`: `카빈`
- `OBJECTS/items/weapons/ranged/guns.json`: `카빈`
- `OBJECTS/items/weapons/ranged/guns.json`: `카빈`
- `OBJECTS/items/weapons/ranged/guns.json`: `카빈`
- `OBJECTS/items/weapons/ranged/guns.json`: `카빈`
- `OBJECTS/items/weapons/ranged/guns.json`: `카빈`

### `cave spider`

- `OBJECTS/creatures/_common.json`: `동굴 거미`
- `OBJECTS/creatures/_common.json`: `동굴 거미`
- `OBJECTS/creatures/_common.json`: `동굴 거미`
- `OBJECTS/creatures/_common.json`: `동굴 거미`
- `OBJECTS/creatures/insects/spiders.json`: `동굴 거미`
- `OBJECTS/creatures/insects/spiders.json`: `동굴 거미`
- `OBJECTS/creatures/insects/spiders.json`: `동굴 거미`
- `OBJECTS/creatures/insects/spiders.json`: `동굴 거미`
- `OBJECTS/creatures/insects/spiders.json`: `동굴 거미`
- `OBJECTS/creatures/insects/spiders.json`: `동굴 거미`
- `OBJECTS/creatures/insects/spiders.json`: `동굴 거미`
- `OBJECTS/creatures/insects/spiders.json`: `동굴 거미`

### `chair`

- `OBJECTS/furniture/misc.json`: `의자`
- `OBJECTS/furniture/misc.json`: `의자`
- `OBJECTS/furniture/misc.json`: `의자`
- `OBJECTS/furniture/misc.json`: `의자`
- `OBJECTS/furniture/misc.json`: `의자`
- `OBJECTS/furniture/misc.json`: `의자`
- `OBJECTS/furniture/misc.json`: `의자`
- `OBJECTS/furniture/misc.json`: `의자`
- `OBJECTS/items/_nouns.json`: `의자`
- `OBJECTS/items/_nouns.json`: `의자`
- `OBJECTS/items/_nouns.json`: `의자`
- `OBJECTS/items/_nouns.json`: `의자`

### `chest`

- `OBJECTS/_suffixes.json`: `가슴`
- `OBJECTS/_suffixes.json`: `가슴`
- `OBJECTS/_suffixes.json`: `가슴`
- `OBJECTS/_suffixes.json`: `가슴`
- `OBJECTS/furniture/containers.json`: `상자`
- `OBJECTS/furniture/containers.json`: `상자`
- `OBJECTS/furniture/containers.json`: `상자`
- `OBJECTS/furniture/containers.json`: `상자`
- `OBJECTS/furniture/containers.json`: `상자`
- `OBJECTS/furniture/containers.json`: `상자`
- `OBJECTS/furniture/containers.json`: `상자`
- `OBJECTS/furniture/containers.json`: `상자`
- `OBJECTS/items/_nouns.json`: `상자`
- `OBJECTS/items/_nouns.json`: `상자`
- `OBJECTS/items/_nouns.json`: `상자`
- `OBJECTS/items/_nouns.json`: `상자`

### `cloak`

- `OBJECTS/items/_nouns.json`: `망토`
- `OBJECTS/items/_nouns.json`: `망토`
- `OBJECTS/items/_nouns.json`: `망토`
- `OBJECTS/items/_nouns.json`: `망토`
- `OBJECTS/items/base_items.json`: `망토`
- `OBJECTS/items/base_items.json`: `망토`
- `OBJECTS/items/base_items.json`: `망토`
- `OBJECTS/items/base_items.json`: `망토`
- `OBJECTS/items/base_items.json`: `망토`
- `OBJECTS/items/base_items.json`: `망토`
- `OBJECTS/items/base_items.json`: `망토`
- `OBJECTS/items/base_items.json`: `망토`
- `OBJECTS/items/armor/back.json`: `망토`
- `OBJECTS/items/armor/back.json`: `망토`
- `OBJECTS/items/armor/back.json`: `망토`
- `OBJECTS/items/armor/back.json`: `망토`
- `OBJECTS/items/armor/back.json`: `망토`
- `OBJECTS/items/armor/back.json`: `망토`
- `OBJECTS/items/armor/back.json`: `망토`
- `OBJECTS/items/armor/back.json`: `망토`

### `clockwork beetle`

- `OBJECTS/creatures/tutorial.json`: `클락워크 비틀`
- `OBJECTS/creatures/tutorial.json`: `클락워크 비틀`
- `OBJECTS/creatures/tutorial.json`: `클락워크 비틀`
- `OBJECTS/creatures/tutorial.json`: `클락워크 비틀`
- `OBJECTS/creatures/tutorial.json`: `클락워크 비틀`
- `OBJECTS/creatures/tutorial.json`: `클락워크 비틀`
- `OBJECTS/creatures/tutorial.json`: `클락워크 비틀`
- `OBJECTS/creatures/tutorial.json`: `클락워크 비틀`
- `OBJECTS/creatures/insects/beetles.json`: `클락워크 비틀`
- `OBJECTS/creatures/insects/beetles.json`: `클락워크 비틀`
- `OBJECTS/creatures/insects/beetles.json`: `클락워크 비틀`
- `OBJECTS/creatures/insects/beetles.json`: `클락워크 비틀`
- `OBJECTS/creatures/insects/beetles.json`: `클락워크 비틀`
- `OBJECTS/creatures/insects/beetles.json`: `클락워크 비틀`
- `OBJECTS/creatures/insects/beetles.json`: `클락워크 비틀`
- `OBJECTS/creatures/insects/beetles.json`: `클락워크 비틀`

### `club`

- `OBJECTS/items/_nouns.json`: `곤봉`
- `OBJECTS/items/_nouns.json`: `곤봉`
- `OBJECTS/items/_nouns.json`: `곤봉`
- `OBJECTS/items/_nouns.json`: `곤봉`
- `OBJECTS/items/weapons/melee/cudgels.json`: `곤봉`
- `OBJECTS/items/weapons/melee/cudgels.json`: `곤봉`
- `OBJECTS/items/weapons/melee/cudgels.json`: `곤봉`
- `OBJECTS/items/weapons/melee/cudgels.json`: `곤봉`
- `OBJECTS/items/weapons/melee/cudgels.json`: `곤봉`
- `OBJECTS/items/weapons/melee/cudgels.json`: `곤봉`
- `OBJECTS/items/weapons/melee/cudgels.json`: `곤봉`
- `OBJECTS/items/weapons/melee/cudgels.json`: `곤봉`

### `dawnglider`

- `OBJECTS/creatures/_common.json`: `새벽활강꾼`
- `OBJECTS/creatures/_common.json`: `새벽활강꾼`
- `OBJECTS/creatures/_common.json`: `새벽활강꾼`
- `OBJECTS/creatures/_common.json`: `새벽활강꾼`
- `OBJECTS/creatures/animals/mammals.json`: `새벽활공수`
- `OBJECTS/creatures/animals/mammals.json`: `새벽활공수`
- `OBJECTS/creatures/animals/mammals.json`: `새벽활공수`
- `OBJECTS/creatures/animals/mammals.json`: `새벽활공수`
- `OBJECTS/creatures/animals/mammals.json`: `새벽활공수`
- `OBJECTS/creatures/animals/mammals.json`: `새벽활공수`
- `OBJECTS/creatures/animals/mammals.json`: `새벽활공수`
- `OBJECTS/creatures/animals/mammals.json`: `새벽활공수`

### `dromad`

- `OBJECTS/creatures/_common.json`: `드로마드`
- `OBJECTS/creatures/_common.json`: `드로마드`
- `OBJECTS/creatures/_common.json`: `드로마드`
- `OBJECTS/creatures/_common.json`: `드로마드`
- `OBJECTS/creatures/humanoids/dromad.json`: `드로마드`
- `OBJECTS/creatures/humanoids/dromad.json`: `드로마드`
- `OBJECTS/creatures/humanoids/dromad.json`: `드로마드`
- `OBJECTS/creatures/humanoids/dromad.json`: `드로마드`
- `OBJECTS/creatures/humanoids/dromad.json`: `드로마드`
- `OBJECTS/creatures/humanoids/dromad.json`: `드로마드`
- `OBJECTS/creatures/humanoids/dromad.json`: `드로마드`
- `OBJECTS/creatures/humanoids/dromad.json`: `드로마드`

### `electrofuge`

- `OBJECTS/creatures/_common.json`: `전기거미`
- `OBJECTS/creatures/_common.json`: `전기거미`
- `OBJECTS/creatures/_common.json`: `전기거미`
- `OBJECTS/creatures/_common.json`: `전기거미`
- `OBJECTS/creatures/insects/spiders.json`: `전기거미`
- `OBJECTS/creatures/insects/spiders.json`: `전기거미`
- `OBJECTS/creatures/insects/spiders.json`: `전기거미`
- `OBJECTS/creatures/insects/spiders.json`: `전기거미`
- `OBJECTS/creatures/insects/spiders.json`: `전기거미`
- `OBJECTS/creatures/insects/spiders.json`: `전기거미`
- `OBJECTS/creatures/insects/spiders.json`: `전기거미`
- `OBJECTS/creatures/insects/spiders.json`: `전기거미`

### `eyeless crab`

- `OBJECTS/creatures/_common.json`: `눈먼 게`
- `OBJECTS/creatures/_common.json`: `눈먼 게`
- `OBJECTS/creatures/_common.json`: `눈먼 게`
- `OBJECTS/creatures/_common.json`: `눈먼 게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 게`

### `eyeless king crab`

- `OBJECTS/creatures/_common.json`: `눈먼 왕게`
- `OBJECTS/creatures/_common.json`: `눈먼 왕게`
- `OBJECTS/creatures/_common.json`: `눈먼 왕게`
- `OBJECTS/creatures/_common.json`: `눈먼 왕게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 왕게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 왕게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 왕게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 왕게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 왕게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 왕게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 왕게`
- `OBJECTS/creatures/insects/crabs.json`: `눈먼 왕게`

*... 외 98개*

---

## XML에 없는 JSON 항목

*JSON에 번역이 있지만 XML DisplayName에 없는 항목*
*(UI 텍스트, 설명문, 또는 이미 삭제된 항목일 수 있음)*

### `CHARGEN/PRESETS/Mutated_Human.json` (11개)

- `marsh taur` → `늪지 타우르`
- `Marsh Taur` → `늪지 타우르`
- `dream tortoise` → `꿈의 거북`
- `Dream Tortoise` → `꿈의 거북`
- `gunwing` → `건윙`
- `Gunwing` → `건윙`
- `star-eye esper` → `별눈 에스퍼`
- `Star-Eye Esper` → `별눈 에스퍼`
- `firefrond` → `불의 잎새`
- `Firefrond` → `불의 잎새`
- *... 외 1개*

### `CHARGEN/PRESETS/True_Kin.json` (6개)

- `praetorian prime` → `근위병 프라임`
- `Praetorian Prime` → `근위병 프라임`
- `first gardener` → `원예가`
- `First Gardener` → `원예가`
- `first child of the hearth` → `화로의자녀`
- `First Child of the Hearth` → `화로의자녀`

### `CHARGEN/PRESETS/descriptions.json` (25개)

- `ability to freeze enemies` → `적 동결 능력`
- `ability to set enemies on fire` → `적을 불태우는 능력`
- `burrower and trap-setter` → `굴파기 및 함정 설치자`
- `causes harvestable plants to grow nearby` → `근처에 채집 가능한 식물을 자라게 함`
- `charge-based melee fighter` → `돌격 기반 근접 전투원`
- `effective crowd control and map vision` → `효과적인 군중 제어 및 지도 시야`
- `emits sleep gas to disable enemies` → `수면 가스 방출로 적 무력화`
- `flies` → `비행 가능`
- `highly maneuverable` → `뛰어난 기동성`
- `highly-armored` → `중장갑`
- *... 외 15개*

### `CHARGEN/SUBTYPES/Callings/Apostle.json` (1개)

- `Apostle` → `사도`

### `CHARGEN/SUBTYPES/Callings/Greybeard.json` (1개)

- `Greybeard` → `백발노인`

### `CHARGEN/SUBTYPES/Callings/Gunslinger.json` (1개)

- `Gunslinger` → `총잡이`

### `CHARGEN/SUBTYPES/Callings/Marauder.json` (1개)

- `Marauder` → `약탈자`

### `CHARGEN/SUBTYPES/Callings/Nomad.json` (1개)

- `Nomad` → `유목민`

### `CHARGEN/SUBTYPES/Callings/Pilgrim.json` (1개)

- `Pilgrim` → `순례자`

### `CHARGEN/SUBTYPES/Callings/Scholar.json` (1개)

- `Scholar` → `학자`

### `CHARGEN/SUBTYPES/Callings/Warden.json` (1개)

- `Warden` → `워든`

### `CHARGEN/SUBTYPES/Callings/Water_Merchant.json` (1개)

- `Water Merchant` → `생수 상인`

### `CHARGEN/SUBTYPES/Castes/Artifex.json` (1개)

- `Artifex` → `기술자`

### `CHARGEN/SUBTYPES/Castes/Child_of_the_Deep.json` (1개)

- `Child of the Deep` → `심연의자녀`

### `CHARGEN/SUBTYPES/Castes/Child_of_the_Hearth.json` (1개)

- `Child of the Hearth` → `화로의자녀`

### `CHARGEN/SUBTYPES/Castes/Child_of_the_Wheel.json` (1개)

- `Child of the Wheel` → `수레의자녀`

### `CHARGEN/SUBTYPES/Castes/Consul.json` (1개)

- `Consul` → `영사`

### `CHARGEN/SUBTYPES/Castes/Eunuch.json` (1개)

- `Eunuch` → `환관`

### `CHARGEN/SUBTYPES/Castes/Fuming_God-Child.json` (1개)

- `Fuming God-Child` → `연신의자녀`

### `CHARGEN/SUBTYPES/Castes/Horticulturist.json` (1개)

- `Horticulturist` → `원예가`

---

## 매칭 유형별 통계

| 정규화 방식 | 매칭 수 | 설명 |
|-------------|---------|------|
| basic | 573 | 단순 소문자 변환 |
| no_color | 259 | 컬러 태그 제거 후 매칭 |
| no_special | 0 | 특수문자 제거 후 매칭 |
| spaces | 0 | 공백 정규화 후 매칭 |

---

## 미번역 샘플 (처음 50개)

- `&amp;Cps&amp;Yion&amp;Cic amp&amp;Ylif&amp;Cier he&amp;Ylm&amp;Cet` (Items.xml)
- `&amp;Kinhi&amp;rb&amp;Kitor cuff` (Items.xml)
- `&amp;Kps&amp;Cion&amp;Kic amp&amp;Clif&amp;Kier ba&amp;Cckpa&amp;Kck` (Items.xml)
- `0lam` (Creatures.xml)
- `1-FF` (Creatures.xml)
- `3D cobblers` (Items.xml)
- `=creatureRegionAdjective= ape` (HiddenObjects.xml)
- `=creatureRegionAdjective= baetyl` (HiddenObjects.xml)
- `=creatureRegionAdjective= baron` (HiddenObjects.xml)
- `=creatureRegionAdjective= bat` (HiddenObjects.xml)
- `=creatureRegionAdjective= bear` (HiddenObjects.xml)
- `=creatureRegionAdjective= cactus` (HiddenObjects.xml)
- `=creatureRegionAdjective= cannibal` (HiddenObjects.xml)
- `=creatureRegionAdjective= cat` (HiddenObjects.xml)
- `=creatureRegionAdjective= chime` (HiddenObjects.xml)
- `=creatureRegionAdjective= clam` (HiddenObjects.xml)
- `=creatureRegionAdjective= crab` (HiddenObjects.xml)
- `=creatureRegionAdjective= daughter` (HiddenObjects.xml)
- `=creatureRegionAdjective= dog` (HiddenObjects.xml)
- `=creatureRegionAdjective= farmer` (HiddenObjects.xml)
- `=creatureRegionAdjective= fish` (HiddenObjects.xml)
- `=creatureRegionAdjective= fly` (HiddenObjects.xml)
- `=creatureRegionAdjective= frog` (HiddenObjects.xml)
- `=creatureRegionAdjective= fungus` (HiddenObjects.xml)
- `=creatureRegionAdjective= Girshling` (HiddenObjects.xml)
- `=creatureRegionAdjective= goat` (HiddenObjects.xml)
- `=creatureRegionAdjective= hermit` (HiddenObjects.xml)
- `=creatureRegionAdjective= kudu` (HiddenObjects.xml)
- `=creatureRegionAdjective= lizard` (HiddenObjects.xml)
- `=creatureRegionAdjective= moa` (HiddenObjects.xml)
- `=creatureRegionAdjective= ooze` (HiddenObjects.xml)
- `=creatureRegionAdjective= pig` (HiddenObjects.xml)
- `=creatureRegionAdjective= root` (HiddenObjects.xml)
- `=creatureRegionAdjective= scorpion` (HiddenObjects.xml)
- `=creatureRegionAdjective= tortoise` (HiddenObjects.xml)
- `=creatureRegionAdjective= trader` (HiddenObjects.xml)
- `=creatureRegionAdjective= vine` (HiddenObjects.xml)
- `=creatureRegionAdjective= warden` (HiddenObjects.xml)
- `=creatureRegionAdjective= worm` (HiddenObjects.xml)
- `=creatureRegionAdjective= zebra` (HiddenObjects.xml)
- `=creatureRegionNoun= and pariah to =subject.possessive= people` (HiddenObjects.xml)
- `=creatureRegionNoun= of the Gyre` (HiddenObjects.xml)
- `=creatureRegionNoun= of the Sightless Way` (HiddenObjects.xml)
- `_` (Creatures.xml)
- `achromous bite` (Creatures.xml)
- `Additional direct hit damage` (ActivatedAbilities.xml)
- `addling urchin` (Creatures.xml)
- `adiyy` (Creatures.xml)
- `Agate Severance Star` (Creatures.xml)
- `Agility bonus` (ActivatedAbilities.xml)
