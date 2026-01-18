# Localization Integrity Report

Generated to identify duplicate keys and potential translation conflicts across all glossary files.

## 1. Value Conflicts (Same Key, Different Translation)
| Normalized Key | File 1 | Translation 1 | File 2 | Translation 2 |
| :--- | :--- | :--- | :--- | :--- |
| +1 ego | glossary_chargen.json | 자아 +1 | glossary_mutations.json | +1 자아 |
| -600 reputation with the putus templar | glossary_proto.json | {{c|푸투스 템플러}} 평판 -600 | glossary_chargen.json | {{c|푸투스 템플러(the Putus Templar)}} 평판 -600 |
| 20 bonus skill points each level | glossary_chargen.json | 레벨당 {{c|20}}의 보너스 기술 포인트 | glossary_proto.json | 레벨업 시 {{c|20}} 보너스 기술 포인트 |
| acrobatics | glossary_skills.json | 회피와 신속한 이동 능력이 향상됩니다. | glossary_skills.json | 곡예 |
| akimbo | glossary_skills.json | 아킴보 | glossary_skills.json | 권총 두 자루를 동시에 사용하는 기술이 숙달됩니다. |
| allowed entrance to many settlements for purposes of trade | glossary_proto.json | 교역 목적으로 많은 정착지에 출입이 허용됩니다 | glossary_chargen.json | 무역 목적으로 많은 정착지 출입이 허용됨 |
| artifex | glossary_chargen.json | 아티펙스 | glossary_proto.json | 아르티펙스(Artifex) |
| axe | glossary_terms.json | 도끼 | glossary_skills.json | 도끼와 날붙이 무기 숙련도가 향상됩니다. |
| back | glossary_ui.json | 등 | glossary_chargen.json | 뒤로 |
| begins with a number of random artifacts and scrap | glossary_chargen.json | 여러 개의 무작위 유물과 고철을 가지고 시작 | glossary_proto.json | 무작위 유물과 고철을 소지하고 시작합니다 |
| berserk! | glossary_skills.json | 활성화; 재사용 대기시간 100. 잠시 동안 광폭화 상태가 되어 모든 저항력이 높아지지만 방어력이 낮아집니다. | glossary_skills.json | 광폭화! |
| biodynamic power plant | glossary_cybernetics.json | 신진대사를 에너지로 변환하여 사이버네틱스에 전력을 공급합니다. | glossary_cybernetics.json | 생체역학 발전소 |
| bonus resistances based on arcology of origin | glossary_proto.json | 출신 아콜로지에 따른 보너스 저항 | glossary_chargen.json | 출신지(Arcology)에 따른 보너스 저항 |
| bow and rifle | glossary_skills.json | 활과 소총을 다루는 기술이 뛰어납니다. | glossary_skills.json | 활과 소총 |
| calling | glossary_terms.json | 소명 | glossary_chargen.json | 직업 |
| carbide hand bones | glossary_cybernetics.json | 손뼈를 카바이드 합금으로 교체하여 근접 공격력을 강화합니다. | glossary_cybernetics.json | 카바이드 손뼈 |
| charge | glossary_ui.json | 돌진 | glossary_skills.json | 멀리서 적에게 빠른 속도로 돌격합니다. |
| charging strike | glossary_skills.json | 적에게 돌진하며 강력한 도끼 타격을 날립니다. | glossary_skills.json | 돌격 타격 |
| child of the hearth | glossary_chargen.json | 화로의 아이 | glossary_proto.json | 벽난로의 아이 |
| cleave | glossary_skills.json | 쪼개기 | glossary_skills.json | 도끼 공격 시 적의 방어력(AV)을 일시적으로 낮춥니다. |
| conatus | glossary_skills.json | 한계를 넘어서 더 오래 달릴 수 있습니다. | glossary_skills.json | 코나투스 |
| consul | glossary_chargen.json | 집정관 | glossary_proto.json | 영사(Consul) |
| continue | glossary_ui.json | 이어하기 | glossary_ui.json | 계속 |
| cooking and gathering | glossary_skills.json | 요리 및 채집 | glossary_skills.json | 요리 재료를 채집하고 음식을 만드는 기술이 뛰어납니다. |
| cudgel | glossary_skills.json | 곤봉 | glossary_skills.json | 곤봉과 둔기류 무기 숙련도가 향상됩니다. |
| customs and folklore | glossary_skills.json | 다양한 문화와 민속 지식에 해박합니다. | glossary_skills.json | 풍습 및 민속 |
| dead shot | glossary_skills.json | 정밀한 사격으로 적을 즉사시키거나 치명상을 입힙니다. | glossary_skills.json | 데드 샷 |
| decapitate | glossary_skills.json | 참수 | glossary_skills.json | 적의 머리를 즉시 참수할 확률이 생깁니다. |
| deploy turret | glossary_skills.json | 원거리 무기를 고정형 포탑으로 설치합니다. | glossary_skills.json | 포탑 배치 |
| dermal plating | glossary_cybernetics.json | 피하에 삽입된 금속 판으로 방어력을 높입니다. | glossary_cybernetics.json | 피부 장갑 |
| disarming shot | glossary_skills.json | 사격으로 적의 손에서 무기를 떨어뜨립니다. | glossary_skills.json | 무장 해제 사격 |
| disassemble | glossary_skills.json | 고철이나 유물을 분해하여 부품을 얻습니다. | glossary_skills.json | 분해 |
| dismember | glossary_skills.json | 신체 절단 | glossary_skills.json | 도끼 공격 시 적의 신체 부위를 절단할 확률이 생깁니다. |
| empty the clips | glossary_skills.json | 짧은 시간에 모든 탄환을 쏟아붓습니다. | glossary_skills.json | 탄창 비우기 |
| endurance | glossary_skills.json | 지구력 | glossary_skills.json | 체력과 지구력이 향상됩니다. |
| eunuch | glossary_proto.json | 환관(Eunuch) | glossary_chargen.json | 환관 |
| fastest gun in the rust | glossary_skills.json | 총잡이 | glossary_skills.json | 누구보다 빠른 속도로 권총을 뽑아 사격합니다. |
| fasting way | glossary_skills.json | 배고픔과 갈증을 덜 느끼게 됩니다. | glossary_skills.json | 단식의 길 |
| fuming god-child | glossary_proto.json | 연기를 내뿜는 신의 아이 | glossary_chargen.json | 분노하는 신의 아이 |
| gadget inspector | glossary_skills.json | 유물의 기능을 파악하고 설비를 다루는 능력이 향상됩니다. | glossary_skills.json | 가젯 검사관 |
| heavy weapon | glossary_skills.json | 중화기 | glossary_skills.json | 강력한 중화기를 다루는 기술이 향상됩니다. |
| high starting attributes | glossary_chargen.json | 높은 시작 속성 | glossary_proto.json | 높은 수준의 능력치로 시작 |
| hook and drag | glossary_skills.json | 걸어서 끌기 | glossary_skills.json | 도끼로 적을 걸어 채서 원하는 방향으로 끌고 갑니다. |
| iron mind | glossary_skills.json | 강철 정신 | glossary_skills.json | 강철 같은 의지로 정신 공격에 저항합니다. |
| juke | glossary_skills.json | 순식간에 위치를 바꾸어 적의 공격을 회피합니다. | glossary_skills.json | 쥬크(Juke) |
| jump | glossary_ui.json | 점프 | glossary_skills.json | 활성화; 재사용 대기시간 15. 인접한 빈 공간으로 점프합니다. |
| lay mine / set bomb | glossary_skills.json | 지뢰 매설 / 폭탄 설치 | glossary_skills.json | 폭탄을 지뢰처럼 매설하거나 시간차 폭발을 설정합니다. |
| lionheart | glossary_skills.json | 사자 심장 | glossary_skills.json | 공포에 굴하지 않는 용기를 가집니다. |
| long blade | glossary_skills.json | 롱 블레이드 계열 무기 숙련도가 향상됩니다. | glossary_skills.json | 롱 블레이드 |
| make camp | glossary_skills.json | 야영지를 구축하여 요리와 휴식을 취합니다. | glossary_skills.json | 캠프 설치 |
| may rebuke robots | glossary_chargen.json | 로봇을 복종(Rebuke)시킬 수 있음 | glossary_proto.json | 로봇을 꾸짖을 수 있음(Rebuke) |
| meditate | glossary_skills.json | 명상을 통해 생명력 회복 속도를 비약적으로 높입니다. | glossary_skills.json | 명상 |
| mind over body | glossary_skills.json | 육체를 초월한 정신 | glossary_skills.json | 정신력으로 신체적인 결핍(배고픔 등)을 극복합니다. |
| mind's compass | glossary_skills.json | 마음의 나침반 | glossary_skills.json | 길을 잃을 확률이 줄어들고 월드 맵 이동이 빨라집니다. |
| moderate starting attributes | glossary_chargen.json | 중간 정도의 시작 속성 | glossary_proto.json | 보통 수준의 능력치로 시작 |
| motorized treads | glossary_cybernetics.json | 동력 무한궤도 | glossary_cybernetics.json | 다리를 무한궤도로 교체하여 이동 속도와 운반 능력을 높입니다. |
| multiweapon fighting | glossary_skills.json | 다중 무기를 동시에 다루는 전투 능력이 향상됩니다. | glossary_skills.json | 다중 무기 전투 |
| mutant | glossary_terms.json | 돌연변이 | glossary_proto.json | 변이체 |
| mutation | glossary_terms.json | 돌연변이 | glossary_ui.json | 변이 |
| night vision eyes | glossary_cybernetics.json | 야간 시야 의안 | glossary_cybernetics.json | 어둠 속에서도 선명하게 볼 수 있는 강화 의안입니다. |
| on-board recoiler | glossary_cybernetics.json | 내장형 리코일러 | glossary_cybernetics.json | 사전에 설정된 위치로 즉시 순간이동할 수 있는 내장 장치입니다. |
| optical bioscanner | glossary_cybernetics.json | 유기 생명체의 상세 정보를 분석하는 미세 가공 어레이입니다. | glossary_cybernetics.json | 광학 바이오스캐너 |
| persuasion | glossary_skills.json | 설득 | glossary_skills.json | 타인을 설득하고 매력을 발휘하는 능력이 향상됩니다. |
| physic | glossary_skills.json | 의술 | glossary_skills.json | 부상을 치료하고 의약품을 다루는 능력이 향상됩니다. |
| pistol | glossary_skills.json | 권총 숙련도가 향상됩니다. | glossary_skills.json | 권총 |
| poison tolerance | glossary_skills.json | 독성 내성 | glossary_skills.json | 독에 대한 저항력이 높아집니다. |
| praetorian | glossary_chargen.json | 근위대 | glossary_proto.json | 프라이토리아(Praetorian) |
| re-randomize selections | glossary_chargen.json | 선택 다시 무작위화 | glossary_ui.json | 다시 무작위 선택 |
| repair | glossary_skills.json | 부서진 아이템을 수리합니다. | glossary_skills.json | 수리 |
| reverse engineer | glossary_skills.json | 역설계 | glossary_skills.json | 분해한 유물의 설계도를 얻을 확률이 생깁니다. |
| scavenger | glossary_skills.json | 쓰레기 더미 등에서 유용한 부품을 더 잘 찾아냅니다. | glossary_skills.json | 스캐빈저 |
| self-discipline | glossary_skills.json | 자기 수양 | glossary_skills.json | 정신적인 안정과 자기 제어 능력이 향상됩니다. |
| shake it off | glossary_skills.json | 떨쳐내기 | glossary_skills.json | 해로운 상태 효과로부터 더 빠르게 회복합니다. |
| shield | glossary_skills.json | 방패를 이용한 방어 능력이 향상됩니다. | glossary_skills.json | 방패 |
| short blade | glossary_skills.json | 숏 블레이드 계열 무기 숙련도가 향상됩니다. | glossary_skills.json | 숏 블레이드 |
| show advanced options | glossary_options.json | 고급 설정 표시 | glossary_options.json | 고급 설정 보기 |
| single weapon fighting | glossary_skills.json | 단일 무기를 다루는 전투 능력이 향상됩니다. | glossary_skills.json | 단일 무기 전투 |
| sling and run | glossary_skills.json | 쏘고 달리기 | glossary_skills.json | 사격 후 즉시 이동하는 전술을 구사합니다. |
| spry | glossary_skills.json | 활기찬 움직임 | glossary_skills.json | 활발함 |
| starts with a recycling suit | glossary_chargen.json | {{B|재활용 슈트}}를 입고 시작 | glossary_proto.json | {{b|재활용 슈트}}를 착용한 상태로 시작합니다 |
| starts with random cooking ingredients | glossary_chargen.json | 무작위 요리 재료를 가지고 시작 | glossary_proto.json | 무작위 요리 재료를 소지하고 시작합니다 |
| starts with random junk and artifacts | glossary_chargen.json | 무작위 잡동사니와 유물을 가지고 시작 | glossary_proto.json | 무작위 잡동사니와 유물을 소지하고 시작합니다 |
| starts with trade goods | glossary_chargen.json | 무역 등급 상품을 가지고 시작 | glossary_proto.json | 교역품을 소지하고 시작합니다 |
| steady hand | glossary_skills.json | 안정된 자세 | glossary_skills.json | 안정된 자세로 사격 정확도를 높입니다. |
| stowing arm | glossary_cybernetics.json | 추가적인 아이템을 장착할 수 있는 기계 팔입니다. | glossary_cybernetics.json | 수납용 팔 |
| swift reflexes | glossary_skills.json | 빠른 반사신경 | glossary_skills.json | 회피(DV)가 +1 증가합니다. |
| swimming | glossary_skills.json | 물 속에서의 이동 속도 페널티가 감소합니다. | glossary_skills.json | 수영 |
| syzygyrior | glossary_chargen.json | 시지지리어 | glossary_proto.json | 시지지리어(Syzygyrior) |
| tactics | glossary_skills.json | 전술 | glossary_skills.json | 전략적인 전투 운용 능력이 향상됩니다. |
| tinker i | glossary_skills.json | 팅커 I | glossary_skills.json | 기초적인 수준의 유물을 제작하거나 배터리를 충전합니다. |
| tinker ii | glossary_skills.json | 팅커 II | glossary_skills.json | 중급 수준의 복잡한 유물을 제작합니다. |
| tinker iii | glossary_skills.json | 팅커 III | glossary_skills.json | 고등 문명의 정교한 유물을 제작합니다. |
| tinkering | glossary_skills.json | 유물을 조사, 제작, 수리 및 개조하는 기술이 뛰어납니다. | glossary_skills.json | 팅커링 |
| tumble | glossary_skills.json | 공중제비 | glossary_skills.json | 달리기 중에도 민첩하게 회피할 수 있습니다. |
| wayfaring | glossary_skills.json | 여행술 | glossary_skills.json | 야생에서의 생존 기술이 뛰어납니다. |
| weak spotter | glossary_skills.json | 적의 약점을 포착하여 치명타 확률을 높입니다. | glossary_skills.json | 약점 포착 |
| weathered | glossary_skills.json | 단련된 피부 | glossary_skills.json | 거친 환경에 단련되어 원소 저항력이 생깁니다. |
| wilderness lore: flower fields | glossary_skills.json | 야생 지식: 꽃밭 | glossary_skills.json | 꽃밭 지형에서의 생존 지식이 풍부해집니다. |
| wilderness lore: hills and mountains | glossary_skills.json | 언덕 및 산악 지형에서의 생존 지식이 풍부해집니다. | glossary_skills.json | 야생 지식: 언덕과 산 |
| wilderness lore: marshes | glossary_skills.json | 늪지 지형에서의 생존 지식이 풍부해집니다. | glossary_skills.json | 야생 지식: 늪지 |
| wilderness lore: ruins | glossary_skills.json | 유적지에서의 생존 지식이 풍부해집니다. | glossary_skills.json | 야생 지식: 유적 |
| you have spent too many mutation points. | glossary_mutations.json | 너무 많은 변이 포인트를 사용했습니다. | glossary_chargen.json | 변이 포인트를 너무 많이 사용했습니다. |

**Total Conflicts:** 102

## 2. High Frequency Shadowing (Keys in 3+ Categories)
| Key | Occurrences |
| :--- | :--- |
| charge | 4 |
| jump | 4 |
| none | 3 |
| allowed entrance to many settlements for purposes of trade | 3 |
| begins with a number of random artifacts and scrap | 3 |
| starts with a recycling suit | 3 |
| starts with random cooking ingredients | 3 |
| starts with random junk and artifacts | 3 |
| starts with trade goods | 3 |
| axe | 3 |
| cudgel | 3 |
| conatus | 3 |
| fasting way | 3 |
| iron mind | 3 |
| juke | 3 |
| lionheart | 3 |
| meditate | 3 |
| mind over body | 3 |
| poison tolerance | 3 |
| shake it off | 3 |
| spry | 3 |
| swift reflexes | 3 |
| swimming | 3 |
| tumble | 3 |
| weathered | 3 |
| akimbo | 3 |
| dead shot | 3 |
| empty the clips | 3 |
| fastest gun in the rust | 3 |
| sling and run | 3 |
| weak spotter | 3 |
| deploy turret | 3 |
| disassemble | 3 |
| lay mine / set bomb | 3 |
| repair | 3 |
| scavenger | 3 |
| tinker i | 3 |
| tinker ii | 3 |
| tinker iii | 3 |
| make camp | 3 |
| mind's compass | 3 |
| wilderness lore: flower fields | 3 |
| wilderness lore: hills and mountains | 3 |
| wilderness lore: marshes | 3 |
| wilderness lore: ruins | 3 |

**Total Shadowed Keys:** 45
