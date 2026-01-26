# 번역 구조 분석 및 누락 항목 정리

> 기존 LOCALIZATION 폴더 구조 분석 후 미번역 항목을 해당 구조에 맞게 분류

## 요약

| 항목 | 개수 |
|------|------|
| 기존 JSON 파일 | 294 |
| 기존 번역 | 3214 |
| 미번역 (총) | 2193 |
| - 일반 항목 | 1477 |
| - 컬러 코드 | 606 |
| - 템플릿 | 58 |

---

## JSON 구조 패턴

### 패턴 A: OBJECTS/ 폴더 (생물, 아이템, 가구)
```json
{
  "_meta": { "category": "animals/mammals", "version": "2.0" },
  "Bear": {
    "tier": 2, "category": "animal", "tags": ["mammal"],
    "names": { "bear": "곰" },
    "description": "...", "description_ko": "..."
  }
}
```

### 패턴 B: CHARGEN/ 폴더 (팩션, 속성)
```json
{
  "factions": { "the Fellowship of Wardens": "워든 연합" }
}
```

### 패턴 C: GAMEPLAY/ 폴더 (능력, 스킬)
```json
{
  "_meta": { "skill": "Tactics" },
  "names": { "Charge": "돌진" },
  "descriptions": { "Charge": "..." }
}
```

---

## 폴더별 누락 항목

### `OBJECTS/creatures/_misc` (462개)

**🆕 새 파일 생성 필요**

- `0lam`
- `1-FF`
- `Agate Severance Star`
- `Dagasha's crucifix`
- `Eschelstadt II`
- `Girsh godling`
- `Hindriarch Keh`
- `Issachari raider`
- `Issachari rifler`
- `Jy egregore`
- `Kah's conveyor`
- `Lake Avalon`
- `Many Eyes`
- `Nacham's loom`
- `Naphtaali jeer`
- `Naphtaali nimrod`
- `Naphtaali sap`
- `Naphtaali tinker`
- `North Forum`
- `Northern Ark`
- `Pax Klanq`
- `Phinae Hoshaiah`
- `Q Girl`
- `Qorish sorceress gown`
- `Qorish sorceress veil`
- `Rodanis Y`
- `Saad Amus`
- `Star Orchid Temple`
- `Starfarer's Quay`
- `Triangulum Consulate`
- *... 외 432개*

### `OBJECTS/furniture/_misc` (156개)

**🆕 새 파일 생성 필요**

- `Barathrum clock`
- `air well`
- `alchemist's table`
- `armchair`
- `armillary sphere`
- `armor rack`
- `becoming nook`
- `bedger`
- `bone oven`
- `book table`
- `brain sculpture`
- `brass foaminator`
- `broadcast power station`
- `cage`
- `campfire remains`
- `candelabra`
- `canvas folding chair`
- `carcass kneader`
- `carved diptych`
- `catch basin`
- `ceramic amphora`
- `chairbear`
- `chalkboard`
- `chrome bust of K4K5`
- `chrome bust of Mehmet I`
- `chrome sculpture`
- `chromeling signal relay`
- `clay jug`
- `clay oven`
- `clay pitcher`
- *... 외 126개*

### `OBJECTS/items/_misc` (125개)

**🆕 새 파일 생성 필요**

- `3D cobblers`
- `BaseFloat`
- `Fix-It spray foam`
- `Ruin of House Isner`
- `Templar phylactery`
- `antlers`
- `arc winder`
- `ashes`
- `bill`
- `blood-gradient hand vacuum`
- `blunt scalpel`
- `boulder`
- `bouquet of flowers`
- `bubble level`
- `camel bladder`
- `casque`
- `cast net`
- `charred corpse`
- `circle of light`
- `clay pot`
- `cloth overalls`
- `crude toga`
- `crystal flowers`
- `crystalline point`
- `dazzle cheek`
- `desalination pellet`
- `detuned antimatter microreactor`
- `detuned cortical regenerator`
- `dysfunctional AI master unit`
- `electric snail shell`
- *... 외 95개*

### `OBJECTS/terrain/walls` (91개)

*기존 파일에 추가 필요*

- `arrowslit`
- `black shale`
- `brick`
- `brinestalk fence`
- `brinestalk stakes`
- `brinestalk wall`
- `burnished azzurum`
- `canvas`
- `carved stone from the sultanate of *Sultan1Name*`
- `carved stone from the sultanate of *Sultan2Name*`
- `carved stone from the sultanate of *Sultan3Name*`
- `carved stone from the sultanate of *Sultan4Name*`
- `carved stone from the sultanate of *Sultan5Name*`
- `carved stone from the sultanate of Resheph`
- `charred wood`
- `chrome plaque`
- `compacted bone`
- `concrete wall`
- `coral rag`
- `crystalline taproot`
- `crystalline trunk`
- `crysteel plate shear wall`
- `ebon fulcrete`
- `electrofired clay, metal plating, and tissue`
- `fluted marble column`
- `foamcrete`
- `forcefield`
- `frosted pillar`
- `fulcrete with square wave`
- `fuming vents`
- *... 외 61개*

### `OBJECTS/terrain/zone` (84개)

*기존 파일에 추가 필요*

- `arsplice hyphae`
- `ashy stalagmite`
- `banana tree`
- `black marble walkway`
- `bop sponge`
- `brick walkway`
- `bridge`
- `brightshroom`
- `brinestalk`
- `chiming rock`
- `coral path`
- `coral pit`
- `coral polyp`
- `crystalline branch`
- `crystalline root`
- `dandy cap`
- `deep shaft`
- `dense crystal leaves`
- `diacalyptus tree`
- `dirt path`
- `dogthorn tree`
- `elevator shaft`
- `finger coral`
- `foamcrete floor`
- `fractus`
- `frosty web`
- `glitchwood tree`
- `glowing soft`
- `gnawed watervine`
- `grave moss`
- *... 외 54개*

### `CHARGEN/factions` (72개)

*기존 파일에 추가 필요*

- `Chavvah, the Tree of Life`
- `Grandchildren of Mamon`
- `Gyre wights`
- `Knights Liminal`
- `Naphtaali tribe`
- `Quetzal Council`
- `algae`
- `amoeba`
- `antelopes`
- `apes`
- `arachnids`
- `baboons`
- `bacilli`
- `baetyls`
- `batfolk`
- `birds`
- `cannibals`
- `cats`
- `cocci`
- `crabs`
- `cragmensch`
- `denizens of the Yd Freehold`
- `dogs`
- `dromad merchants`
- `elephantines`
- `equines`
- `flowers`
- `foxen`
- `frogs`
- `fungi`
- *... 외 42개*

### `OBJECTS/creatures/npcs` (62개)

*기존 파일에 추가 필요*

- `Agolgot`
- `Agyra`
- `Aloysius`
- `Angohind`
- `Aoyg-No-Longer`
- `Asphodel`
- `Bep`
- `Bethsaida`
- `Dardi`
- `Doyoba`
- `Dyvvrach`
- `Erah`
- `Eskhind`
- `Esther`
- `Euclid`
- `Fjorn-Kosef`
- `Geeub`
- `Goek`
- `Gyamyo`
- `Haddas`
- `Haggabah`
- `Herododicus`
- `Hortensa`
- `Indrix`
- `Isahind`
- `Iseppa`
- `Jacobo`
- `Jotun`
- `Keh-hind`
- `Kesehind`
- *... 외 32개*

### `OBJECTS/creatures/animals/mammals` (54개)

*기존 파일에 추가 필요*

- `Barathrum`
- `Dadogom`
- `Mechanimist catechist`
- `baboon cherub`
- `baboon golem`
- `base bear`
- `bear cherub`
- `bear golem`
- `breathbeard`
- `cat cherub`
- `cat golem`
- `cat herder`
- `charred goatfolk corpse`
- `chitinous puma corpse`
- `communication ribbons`
- `decorative moss pod`
- `dog cherub`
- `dog golem`
- `dreambeard corpse`
- `feral dog`
- `flamebeard corpse`
- `flayed goatfolk corpse`
- `freight crate`
- `fulcrete catapult`
- `gallbeard corpse`
- `goat golem`
- `goat herder`
- `goatfolk golem`
- `goatfolk hornblower`
- `goatfolk qlippoth`
- *... 외 24개*

### `GAMEPLAY/abilities` (49개)

**🆕 새 파일 생성 필요**

- `Additional direct hit damage`
- `Agility bonus`
- `Ambient light recharge rate`
- `Batch size`
- `Beam distance`
- `Bleed damage`
- `Bleed save`
- `Bonus to natural healing rate`
- `Chance to knock opponents down`
- `Charge distance`
- `Charge use per round`
- `Claw penetration vs. walls`
- `Cone angle`
- `Cone length`
- `Confusion rank`
- `Cooldown`
- `Current charge`
- `Current location`
- `Damage increment`
- `Damage to non-structural objects`
- `Damage to structural objects`
- `Daze save`
- `Disabled duration`
- `Duration between use and reversion`
- `Hitpoints per batch`
- `Hobble duration`
- `Hobble effect`
- `Knockdown save`
- `Laser damage increment`
- `Laser penetration`
- *... 외 19개*

### `OBJECTS/creatures/humanoids` (48개)

*기존 파일에 추가 필요*

- `Banner-Knight Templar`
- `Gunner-Knight Templar`
- `Knight Commander of the Holy Temple`
- `Knight Templar`
- `Mechanimist pilgrim`
- `Templar squire`
- `Urshiib Merchant`
- `Wraith-Knight Templar of the Binary Honorum`
- `alchemist`
- `amoeba farmer`
- `beetle farmer`
- `crab farmer`
- `dromad trader`
- `elder breathbeard`
- `elder dreambeard`
- `elder dreambeard corpse`
- `elder flamebeard`
- `elder flamebeard corpse`
- `elder gallbeard`
- `elder gallbeard corpse`
- `elder lagroot`
- `elder mazebeard`
- `elder mazebeard corpse`
- `elder nullbeard`
- `elder nullbeard corpse`
- `elder sleetbeard`
- `elder sleetbeard corpse`
- `elder stillbeard`
- `elder stillbeard corpse`
- `elder tartbeard`
- *... 외 18개*

### `OBJECTS/creatures/robots` (47개)

*기존 파일에 추가 필요*

- `Issachari raider and Mechanimist convert`
- `Mechanimist houndmaster`
- `Mechanimist paladin`
- `Mechanimist preacher`
- `Mechanimist priest`
- `Mechanimist rummager`
- `Mechanimist zealot`
- `bazaar of Samech`
- `bipedal robot golem`
- `blueshifted chrome`
- `cannibal and Mechanimist convert`
- `chaingun turret tinker`
- `chrome chibur`
- `dynamic turret tinker`
- `eigenturret tinker`
- `hexapodal robot golem`
- `humanoid robot golem`
- `laser turret tinker`
- `low-light laser turret tinker`
- `mecha power core`
- `mechanical ape cherub`
- `mechanical baetyl cherub`
- `mechanical cannibal cherub`
- `mechanical equine cherub`
- `mechanical fractus cherub`
- `mechanical frog cherub`
- `mechanical grazing cherub`
- `mechanical insect cherub`
- `mechanical mollusk cherub`
- `mechanical pottery cherub`
- *... 외 17개*

### `OBJECTS/creatures/insects` (31개)

*기존 파일에 추가 필요*

- `Santalalotze`
- `antelope cherub`
- `antelope golem`
- `beekeeper`
- `beetlebum`
- `bone worm corpse`
- `earthworm corpse`
- `gamma moth`
- `gelatinous antiprism`
- `giant amoeba`
- `giant beetle nest`
- `giant centipede`
- `giant centipede corpse`
- `giant centipede nest`
- `girshworm`
- `mechanical antelope cherub`
- `mechanical spider cherub`
- `mechanical worm cherub`
- `plated knollworm`
- `segmented mirthworm corpse`
- `slynth cantor`
- `spider cherub`
- `spider golem`
- `strip fly`
- `vantabloom`
- `vantabud`
- `waveform worm`
- `whipping antenna`
- `worker ant`
- `worm cherub`
- *... 외 1개*

### `OBJECTS/creatures/animals/fish` (30개)

*기존 파일에 추가 필요*

- `Hamilcrab`
- `Neelahind`
- `broken snail egg`
- `chute crab`
- `clam golem`
- `crab cherub`
- `crab claw`
- `crab golem`
- `crab-crushing beak`
- `crysteel bay plates`
- `crysteel braid`
- `eel grass`
- `electric snail`
- `electric snail corpse`
- `engine crabs`
- `enigma snail`
- `enigma snail corpse`
- `fish cherub`
- `fish golem`
- `giant clam`
- `glowfish corpse`
- `mechanical crab cherub`
- `mechanical fish cherub`
- `sewage eel`
- `snail egg`
- `snail golem`
- `snailmother`
- `snailmother corpse`
- `wheel of Qv`
- `wheeled robot golem`

### `OBJECTS/creatures/plants` (28개)

*기존 파일에 추가 필요*

- `Triangulum tree sculpture`
- `aloe fugues`
- `aloe porta`
- `aloe pyra`
- `aloe volta`
- `bush golem`
- `flower cherub`
- `flower golem`
- `fungus golem`
- `lagroot`
- `mechanical flower cherub`
- `mechanical mushroom cherub`
- `mechanical root cherub`
- `mechanical tree cherub`
- `mechanical vine cherub`
- `mushroom cherub`
- `root cherub`
- `root golem`
- `seed`
- `seed spitter`
- `seed vault`
- `seedsprout worm corpse`
- `sower's seed`
- `sprouting orb`
- `tree cherub`
- `tree golem`
- `vine cherub`
- `vine golem`

### `OBJECTS/terrain/world` (23개)

*기존 파일에 추가 필요*

- `Bethesda Susa`
- `Brightsheol`
- `Eyn Rogel`
- `Eyn Roj`
- `Golgotha`
- `Grit Gate`
- `Kyakukya`
- `Lake Hinnom`
- `Omonporch`
- `Opal's Duskwaters`
- `Palladium Reef`
- `Red Rock`
- `River Opal`
- `River Svy`
- `River Yonth`
- `Six Day Stilt`
- `Spindle`
- `Tzimtzlum`
- `Yd Freehold`
- `banana grove`
- `mountain stream`
- `rust wells`
- `rusted archway`

### `OBJECTS/furniture/decoration` (23개)

*기존 파일에 추가 필요*

- `marble statue`
- `nephilim shrine`
- `painting`
- `ruined shrine`
- `shrine`
- `shrine to Girsh Agolgot`
- `shrine to Girsh Bethsaida`
- `shrine to Girsh Qas`
- `shrine to Girsh Qon`
- `shrine to Girsh Rermadon`
- `shrine to Resheph`
- `shrine to Shug'ruith the Burrower`
- `statue of Bel`
- `statue of Carthax`
- `statue of Dagon`
- `statue of Eater`
- `statue of Nisroch`
- `statue of Oboroqoru`
- `statue of Resheph`
- `statue of Shekhinah`
- `statue of a deer`
- `statue of implanted Eater`
- `stone statue`

### `OBJECTS/items/armor` (15개)

*기존 파일에 추가 필요*

- `bark armor`
- `black robes`
- `boar-skin gloves`
- `bounding boots`
- `chain gauntlets`
- `cloth robe`
- `flexivest`
- `greased steel boots`
- `ironweave cloak`
- `leather cloak`
- `magnetized boots`
- `pocketed vest`
- `psychodyne helmet`
- `ring mail`
- `scrap cape`

### `OBJECTS/items/junk` (11개)

**🆕 새 파일 생성 필요**

- `bent metal sheet`
- `bent surgical stent`
- `broken microcontroller array`
- `burnt capacitor`
- `corroded circuit board`
- `cracked lens`
- `cracked robotics housing`
- `depleted stem-generator`
- `destroyed cybernetics controller`
- `failed energy relay`
- `faulty cellular detelomerator`

### `OBJECTS/furniture/doors` (11개)

*기존 파일에 추가 필요*

- `Death Gate`
- `Gate to Brightsheol`
- `Life Gate`
- `brinestalk gate`
- `exit hatch`
- `fused security door`
- `gate`
- `iron gate`
- `ornately engraved marble door with plaque`
- `ruined gate`
- `slumping metal door`

### `OBJECTS/creatures/oozes` (9개)

*기존 파일에 추가 필요*

- `gelatinous cupola`
- `gelatinous frustum`
- `gelatinous prism`
- `gelatinous wedge`
- `jelly golem`
- `mechanical ooze cherub`
- `ooze cherub`
- `ooze golem`
- `plasma jelly`

### `OBJECTS/items/artifacts` (8개)

*기존 파일에 추가 필요*

- `Joppa recoiler`
- `force bracelet`
- `moldering corpse`
- `neck-ring`
- `programmable recoiler`
- `random-point recoiler`
- `reprogrammable recoiler`
- `slip ring`

### `OBJECTS/items/weapons/ranged` (6개)

*기존 파일에 추가 필요*

- `chrome revolver`
- `grappling gun`
- `grenade launcher`
- `longreach grappling gun`
- `missile launcher`
- `semi-automatic pistol`

### `OBJECTS/creatures/animals/birds` (5개)

*기존 파일에 추가 필요*

- `Crowsong`
- `bird cherub`
- `bird golem`
- `glowcrow`
- `mechanical bird cherub`

### `OBJECTS/furniture/lighting` (4개)

*기존 파일에 추가 필요*

- `arc sconce`
- `bronze brazier`
- `full-spectrum bright sconce`
- `light sculpture`

### `OBJECTS/furniture/containers` (4개)

*기존 파일에 추가 필요*

- `credit lockbox`
- `medical locker`
- `scrapable deposit box`
- `sliding drawer cabinet`

### `OBJECTS/creatures/animals/reptiles` (4개)

*기존 파일에 추가 필요*

- `irritable tortoise`
- `mechanical tortoise cherub`
- `tortoise cherub`
- `tortoise golem`

### `OBJECTS/creatures/animals/bats` (3개)

*기존 파일에 추가 필요*

- `bat cherub`
- `bat golem`
- `mechanical bat cherub`

### `OBJECTS/items/tools` (3개)

*기존 파일에 추가 필요*

- `bulging waterskin`
- `leatherworking tools`
- `night-vision goggles`

### `OBJECTS/items/weapons/melee` (3개)

*기존 파일에 추가 필요*

- `leatherworking hammer`
- `nanopneumatic jackhammer`
- `throwing axe`

### `OBJECTS/items/ammo` (2개)

*기존 파일에 추가 필요*

- `HE Missile`
- `blast of shot`

### `OBJECTS/furniture/tech` (2개)

*기존 파일에 추가 필요*

- `electric generator`
- `pilot console`

### `OBJECTS/items/consumables/foods` (1개)

**🆕 새 파일 생성 필요**

- `crusty loaf`

### `OBJECTS/items/consumables` (1개)

*기존 파일에 추가 필요*

- `used injector`

---

## 컬러 코드 항목 (606개)

*태그 구조 유지하며 내부 텍스트만 번역 필요*

예시:
- `&amp;Cps&amp;Yion&amp;Cic amp&amp;Ylif&amp;Cier he&amp;Ylm&amp;Cet`
- `&amp;Kinhi&amp;rb&amp;Kitor cuff`
- `&amp;Kps&amp;Cion&amp;Kic amp&amp;Clif&amp;Kier ba&amp;Cckpa&amp;Kck`
- `Barathrum clock with {{M\|Q Girl}} {{Y\|pendulum}}`
- `Schrodinger page from the {{K\|Annals of Qud}}`
- `Spray{{r\|-}}a{{r\|-}}Brain`
- `banner of the {{r\|Holy Rhombus}}`
- `bladed {{metachrome\|metachrome}} bands`
- `bladed {{metachrome\|metachrome}} tail`
- `burnished {{K\|fullerite}} shield`
- `circle of light in the chord of {{agolgot\|Agolgot}}`
- `circle of light in the chord of {{bethsaida\|Bethsaida}}`
- `circle of light in the chord of {{qas\|Qas}}`
- `circle of light in the chord of {{qon\|Qon}}`
- `circle of light in the chord of {{rermadon\|Rermadon}}`
- `circle of light in the chord of {{shugruith\|Shugruith}}`
- `congealed &amp;Ysalve`
- `congealed {{G\|hulk}} {{w\|honey}}`
- `congealed {{blaze\|blaze}}`
- `congealed {{love\|love}}`
- *... 외 586개*

---

## 작업 우선순위

| 순위 | 폴더/파일 | 항목 수 | 상태 |
|------|-----------|---------|------|
| 1 | `OBJECTS/creatures/_misc` | 462 | 🆕 생성 |
| 2 | `OBJECTS/furniture/_misc` | 156 | 🆕 생성 |
| 3 | `OBJECTS/items/_misc` | 125 | 🆕 생성 |
| 4 | `OBJECTS/terrain/walls` | 91 | 추가 |
| 5 | `OBJECTS/terrain/zone` | 84 | 추가 |
| 6 | `CHARGEN/factions` | 72 | 추가 |
| 7 | `OBJECTS/creatures/npcs` | 62 | 추가 |
| 8 | `OBJECTS/creatures/animals/mammals` | 54 | 추가 |
| 9 | `GAMEPLAY/abilities` | 49 | 🆕 생성 |
| 10 | `OBJECTS/creatures/humanoids` | 48 | 추가 |
| 11 | `OBJECTS/creatures/robots` | 47 | 추가 |
| 12 | `OBJECTS/creatures/insects` | 31 | 추가 |
| 13 | `OBJECTS/creatures/animals/fish` | 30 | 추가 |
| 14 | `OBJECTS/creatures/plants` | 28 | 추가 |
| 15 | `OBJECTS/terrain/world` | 23 | 추가 |
