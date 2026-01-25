# 번역 전수 조사 보고서

> **작성일**: 2026-01-25
> **작성자**: Claude Code

---

## 조사 범위

1. `Mods.xml` - 60개 TinkerDisplayName
2. `Items.xml` - DisplayName 패턴 분석
3. `_common.json` - 현재 등록된 수식어
4. `_nouns.json` - 현재 등록된 명사

---

## 1. 누락된 수식어 (Mods.xml → _common.json)

### 높은 우선순위 (게임에서 자주 등장)
| 영어 | 한글 제안 | 비고 |
|------|----------|------|
| willowy | 버들같은 | 무게 감소 |
| gigantic | 거대한 | 크기 증가 |
| visored | 바이저 달린 | 투구 |
| flexiweaved | 유연 직조 | 방어구 |
| high-capacity | 고용량 | 에너지 셀 |
| nulling | 무효화 | 특수 효과 |
| recycling | 재활용 | 방어구 |
| disguise | 위장 | 방어구 |

### 중간 우선순위
| 영어 | 한글 제안 | 비고 |
|------|----------|------|
| fitted with suspensors | 서스펜서 장착 | 구문 |
| jewel-encrusted | 보석 박힌 | 장식 |
| drum-loaded | 드럼 장전 | 총기 |
| fitted with a beamsplitter | 빔스플리터 장착 | 광선 무기 |
| fitted with filters | 필터 장착 | 투구 |
| two-faced | 양면 | 투구 |
| six-fingered | 여섯 손가락 | 장갑 |
| snail-encrusted | 달팽이 박힌 | 장식 |
| fitted with cleats | 클리트 장착 | 부츠 |
| nav | 내비 | 고글 |
| metered | 계량 | 에너지 셀 |
| airfoil | 에어포일 | 수류탄 |

### 낮은 우선순위 (희귀)
| 영어 | 한글 제안 | 비고 |
|------|----------|------|
| of terrifying visage | 공포의 형상 | 접미사 형식 |
| of serene visage | 평온의 형상 | 접미사 형식 |
| co-processor | 코프로세서 | 고급 |
| extradimensional | 차원간 | 희귀 |
| phase-harmonic | 위상 조화 | 고급 |
| electromagnetically-shielded | 전자기 차폐 | 고급 |

---

## 2. 누락된 명사 (Items.xml → _nouns.json)

### 무기류
| 영어 | 한글 제안 | 카테고리 |
|------|----------|----------|
| kukri | 쿠크리 | weapons |
| kris | 크리스 | weapons |
| flyssa | 플리싸 | weapons |
| maul | 대형 망치 | weapons |
| warhammer | 워해머 | compound_weapons |
| baton | 경찰봉 | weapons |
| rod | 로드 | weapons |
| vinereaper | 덩굴낫 | weapons |
| shillelagh | 실렐라 | weapons |
| chisel | 끌 | tools |
| cleaver | 식칼 | weapons |

### 방어구류
| 영어 | 한글 제안 | 카테고리 |
|------|----------|----------|
| bracelet | 팔찌 | misc (있음) |
| bracer | 완갑 | armor |
| apron | 앞치마 | clothing |
| tunic | 튜닉 | clothing (있음) |
| mantle | 망토 | clothing |
| jerkin | 조끼 | clothing |
| doublet | 더블릿 | clothing |
| coif | 코이프 | armor |
| gorget | 목보호대 | armor |
| pauldrons | 어깨보호대 | armor |
| vambraces | 팔보호대 | armor |

### 기타
| 영어 | 한글 제안 | 카테고리 |
|------|----------|----------|
| figurine | 조각상 | misc |
| recoiler | 귀환 장치 | mechanical |
| toolkit | 공구함 | tools |
| manacles | 수갑 | misc |
| wings | 날개 | mechanical |
| netting | 그물 | misc |

---

## 3. 이미 존재하는 항목 (추가 불필요)

### _common.json에 있음
- painted ✓
- reinforced ✓
- scoped ✓
- padded ✓
- lanterned ✓
- feathered ✓
- scaled ✓
- wooly ✓
- spring-loaded ✓
- polarized ✓
- radio-powered ✓
- illuminated ✓
- phase-conjugate ✓
- jacked ✓
- overloaded ✓
- engraved ✓
- sharp ✓
- serrated ✓
- electrified ✓
- freezing ✓
- flaming ✓
- counterweighted ✓
- spiked ✓
- refractive ✓
- gesticulating ✓
- slender ✓
- displacer ✓
- morphogenetic ✓
- nanon ✓
- lacquered ✓
- sturdy ✓
- masterwork ✓
- liquid-cooled ✓

### _nouns.json에 있음
- nugget ✓ (덩어리)
- hat ✓ (모자) - 방금 추가
- stinger ✓ (독침) - 방금 추가
- plate mail ✓ (판금 갑옷) - 방금 추가

### creatures/_common.json에 있음
- ape ✓ (유인원)

---

## 4. 권장 조치

### 즉시 추가 (높은 우선순위)
1. _common.json에 누락된 Mod 수식어 26개 추가
2. _nouns.json에 누락된 무기명 11개 추가
3. _nouns.json에 누락된 방어구명 11개 추가

### 추후 검토
- "of X visage" 패턴은 접미사로 처리 필요
- "fitted with X" 패턴은 구문 번역 로직 검토 필요

---

## 5. 추가 작업 예상 규모

| 파일 | 추가 항목 수 |
|------|-------------|
| _common.json modifiers | ~26개 |
| _nouns.json weapons | ~11개 |
| _nouns.json armor | ~11개 |
| _nouns.json misc | ~6개 |
| **총계** | **~54개** |

