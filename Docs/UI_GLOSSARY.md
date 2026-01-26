# UI 번역 용어 사전

> Caves of Qud 한글화 프로젝트 - UI 번역 참조 문서

---

## 목차

1. [6대 속성 (Attributes)](#1-6대-속성-attributes)
2. [보조 속성 (Secondary Stats)](#2-보조-속성-secondary-stats)
3. [저항력 (Resistances)](#3-저항력-resistances)
4. [장비 슬롯 (Equipment Slots)](#4-장비-슬롯-equipment-slots)
5. [인벤토리 액션 (Inventory Actions)](#5-인벤토리-액션-inventory-actions)
6. [아이템 카테고리 (Item Categories)](#6-아이템-카테고리-item-categories)
7. [품질 등급 (Quality Tiers)](#7-품질-등급-quality-tiers)
8. [아이템 상태 (Item Condition)](#8-아이템-상태-item-condition)
9. [스테이터스 화면 탭 (Status Screen Tabs)](#9-스테이터스-화면-탭-status-screen-tabs)
10. [메인 메뉴 (Main Menu)](#10-메인-메뉴-main-menu)
11. [공통 버튼/액션 (Common Buttons)](#11-공통-버튼액션-common-buttons)
12. [캐릭터 생성 (Character Creation)](#12-캐릭터-생성-character-creation)
13. [툴팁 (Tooltips)](#13-툴팁-tooltips)

---

## 1. 6대 속성 (Attributes)

| English | Korean | 약어 |
|---------|--------|------|
| Strength | 힘 | STR |
| Agility | 민첩 | AGI |
| Toughness | 건강 | TOUGH |
| Intelligence | 지능 | INT |
| Willpower | 의지 | WILL |
| Ego | 자아 | EGO |

**파일:** `LOCALIZATION/_SHARED/attributes.json` (SSOT)

---

## 2. 보조 속성 (Secondary Stats)

| English | Korean |
|---------|--------|
| Hit Points | 체력 |
| Move Speed | 이동 속도 |
| Quickness | 속도 |
| Attribute Points | 속성 포인트 |
| Mutation Points | 변이 포인트 |
| Skill Points | 스킬 포인트 |
| Experience Points | 경험치 |
| Compute Power (CP) | 연산 능력 (CP) |
| Level | 레벨 |
| Health | 체력 |

**파일:** `LOCALIZATION/UI/common.json` → `common` 섹션

---

## 3. 저항력 (Resistances)

| English | Korean |
|---------|--------|
| Heat Resistance | 열 저항 |
| Cold Resistance | 냉기 저항 |
| Acid Resistance | 산 저항 |
| Electric Resistance | 전기 저항 |
| Resistances | 저항력 |

**파일:** `LOCALIZATION/UI/common.json` → `common` 섹션

---

## 4. 장비 슬롯 (Equipment Slots)

| English | Korean |
|---------|--------|
| Head | 머리 |
| Body | 몸통 |
| Back | 등 |
| Face | 얼굴 |
| Arm | 팔 |
| Hands | 손 |
| Feet | 발 |
| Floating Nearby | 부유 아이템 |
| Left Hand | 왼손 |
| Right Hand | 오른손 |
| Right Arm | 오른팔 |
| Right Face | 오른쪽 얼굴 |
| Missile Weapon | 원거리 무기 |
| Thrown Weapon | 투척 무기 |

**파일:** `LOCALIZATION/_SHARED/body_parts.json` (SSOT), `LOCALIZATION/UI/common.json` → `inventory` 섹션

---

## 5. 인벤토리 액션 (Inventory Actions)

| English | Korean |
|---------|--------|
| Equip | 장착 |
| Equipped | 장착됨 |
| Unequip | 해제 |
| Unequipped | 장착 해제됨 |
| Drop | 버리기 |
| Dropped | 버림 |
| Use | 사용 |
| Examine | 조사 |
| Take All | 모두 가져가기 |
| Picked Up | 주움 |
| Quick Apply | 빠른 적용 |
| Quick Drink | 빠른 마시기 |
| Quick Eat | 빠른 먹기 |
| Quick Drop | 빠른 버리기 |

**파일:** `LOCALIZATION/UI/common.json` → `inventory` 섹션

---

## 6. 아이템 카테고리 (Item Categories)

| English | Korean |
|---------|--------|
| Water Containers | 물 용기 |
| Trade Goods | 교역품 |
| Tonics | 토닉 |
| Artifacts | 유물 |
| Weapons | 무기 |
| Armor | 방어구 |
| Food | 음식 |
| Books | 책 |
| Ammo | 탄약 |
| Grenades | 수류탄 |
| Tools | 도구 |
| Miscellaneous | 기타 |
| Junk | 잡동사니 |
| Medicine | 약품 |

**파일:** `LOCALIZATION/UI/common.json` → `categories` 섹션

---

## 7. 품질 등급 (Quality Tiers)

| English | Korean |
|---------|--------|
| Legendary | 전설적인 |
| Epic | 서사적인 |
| Masterwork | 명품 |
| Flawless | 완벽한 |
| Pristine | 완전무결한 |
| Excellent | 뛰어난 |
| Superior | 우수한 |
| Fine | 고급 |
| Perfect | 완벽한 |
| Rare | 희귀한 |
| Uncommon | 비범한 |
| Common | 일반 |
| Basic | 기본 |
| Crude | 조잡한 |
| Poor | 조악한 |

**파일:** `LOCALIZATION/_SHARED/qualities.json` (SSOT)

---

## 8. 아이템 상태 (Item Condition)

| English | Korean |
|---------|--------|
| Perfect | 완벽 |
| Broken | 고장 |
| Worn | 낡음 |
| Rusted | 녹슴 |
| Cracked | 금간 |
| Damaged | 손상 |
| Tattered | 해진 |
| Bent | 휜 |
| Flawless | 완벽한 |
| Masterwork | 명품 |
| Pristine | 완전무결 |

**파일:** `LOCALIZATION/UI/common.json` → `quality` 섹션

---

## 9. 스테이터스 화면 탭 (Status Screen Tabs)

| English | Korean |
|---------|--------|
| Stats | 능력치 |
| Skills | 스킬 |
| Powers | 권능 |
| Resistances | 저항력 |
| Cybernetics | 사이버네틱스 |
| Physical Mutation | 신체적 변이 |
| Equipment | 장비 |
| Inventory | 인벤토리 |
| Journal | 일지 |
| Quest | 퀘스트 |
| Message Log | 메시지 기록 |
| Recipes | 요리법 |
| Factions | 세력 |
| Reputation | 평판 |
| Experience | 경험치 |
| Level | 레벨 |
| Health | 체력 |
| Chronology | 연대기 |
| Locations | 장소 |
| General Notes | 일반 노트 |
| Gossip and Lore | 소문과 전승 |
| Sultan Histories | 술탄의 역사 |
| Village Histories | 마을의 역사 |

**파일:** `LOCALIZATION/UI/common.json` → `status` 섹션

---

## 10. 메인 메뉴 (Main Menu)

| English | Korean |
|---------|--------|
| New Game | 새로운 게임 |
| Continue | 이어하기 |
| Load Game | 불러오기 |
| Save Game | 저장하기 |
| Options | 설정 |
| Quit | 종료 |
| Credits | 제작진 |
| Help | 도움말 |
| Daily Challenge | 일일 도전 |
| Weekly Challenge | 주간 도전 |
| Quit to Desktop | 게임 종료 |
| Quit to Main Menu | 메인 메뉴로 |
| Mods | 모드 |
| Modding Toolkit | 모딩 도구 |
| Library | 라이브러리 |
| Records | 기록실 |
| Redeem Code | 코드 입력 |

**파일:** `LOCALIZATION/UI/common.json` → `ui` 섹션

---

## 11. 공통 버튼/액션 (Common Buttons)

| English | Korean |
|---------|--------|
| Accept | 수락 |
| Cancel | 취소 |
| Close | 닫기 |
| Continue | 계속 |
| Yes | 예 |
| No | 아니오 |
| OK | 확인 |
| Submit | 확인 |
| Skip | 건너뛰기 |
| Delete | 삭제 |
| Copy | 복사 |
| Keep | 유지 |
| Ignore | 무시 |
| Decline | 거절 |
| Hold to Accept | 길게 눌러 수락 |
| Press Any Key to Continue | 아무 키나 눌러 계속하기 |

**파일:** `LOCALIZATION/UI/common.json` → `common` 섹션

---

## 12. 캐릭터 생성 (Character Creation)

| English | Korean |
|---------|--------|
| Genotype | 유전형 |
| Calling | 소명 |
| Caste | 계급 |
| Mutant | 돌연변이 |
| True Kin | 순수인 |
| Attribute | 능력치 |
| Mutation | 변이 |
| Skill | 스킬 |
| Build Library | 빌드 라이브러리 |
| Character Creation | 캐릭터 생성 |
| Main Attributes | 주요 속성 |
| Secondary Attributes | 보조 속성 |

**파일:** `LOCALIZATION/UI/terms.json`, `LOCALIZATION/CHARGEN/ui.json`

---

## 13. 툴팁 (Tooltips)

| English | Korean |
|---------|--------|
| This Item | 현재 아이템 |
| Equipped Item | 장착 아이템 |

**파일:** `LOCALIZATION/UI/common.json` → `tooltips` 섹션

---

## 번역 커버리지 현황

| 카테고리 | 상태 | 완성도 |
|----------|------|--------|
| 메인 메뉴 | 완료 | 95% |
| 옵션/설정 | 완료 | 95% (450+ 항목) |
| 캐릭터 생성 | 완료 | 90% |
| 스테이터스 화면 | 완료 | 90% |
| 인벤토리 화면 | 완료 | 90% |
| 툴팁 | 완료 | 85% |
| 스킬/파워 | 부분 | 80% |
| 사이버네틱스 | 부분 | 80% |
| 변이 | 부분 | 75% |

---

## 주요 파일 위치

| 목적 | 파일 경로 |
|------|-----------|
| 핵심 UI 용어 | `LOCALIZATION/UI/common.json` |
| 게임 용어 사전 | `LOCALIZATION/UI/terms.json` |
| 옵션 설정 | `LOCALIZATION/UI/options.json` |
| 6대 속성 (SSOT) | `LOCALIZATION/_SHARED/attributes.json` |
| 신체 부위 (SSOT) | `LOCALIZATION/_SHARED/body_parts.json` |
| 품질 등급 (SSOT) | `LOCALIZATION/_SHARED/qualities.json` |
| 캐릭터 생성 UI | `LOCALIZATION/CHARGEN/ui.json` |
| 전역 UI 패치 | `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs` |

---

## SSOT (Single Source of Truth) 패턴

공유 용어는 `LOCALIZATION/_SHARED/` 폴더에 정의하고, 다른 파일에서 `_imports`로 참조:

```json
{
  "_imports": ["_SHARED/attributes", "_SHARED/body_parts"],
  "common": {
    // ...
  }
}
```

### SSOT 파일 목록

- `_SHARED/attributes.json` - 6대 속성
- `_SHARED/body_parts.json` - 신체 부위
- `_SHARED/qualities.json` - 품질 등급
- `_SHARED/factions.json` - 세력
- `_SHARED/materials.json` - 재질

---

## 검증 방법

1. `./deploy.sh` 실행하여 모드 배포
2. 게임 실행
3. 다음 화면들 확인:
   - 메인 메뉴
   - 캐릭터 생성 전체 플로우
   - 인벤토리 화면 (I 키)
   - 스테이터스 화면 (@ 키)
   - 옵션 화면 (ESC → Options)
   - 스킬/파워 화면
4. `kr:reload` 명령으로 실시간 JSON 리로드 테스트
