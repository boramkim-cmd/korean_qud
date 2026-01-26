#!/usr/bin/env python3
"""
Final Analysis: Why Items Are Not Translated
"""

analysis = """
========================================================================
최종 분석: 왜 아이템들이 번역되지 않는가?
========================================================================

[1] JSON 데이터 상태: ✅ 정상
    - dried: '말린' ✓
    - canned: '통조림' ✓
    - filthy: '더러운' ✓
    - bubble: '거품' ✓
    - utility: '다용도' ✓
    - frill: '프릴' ✓
    → 번역 데이터는 모두 존재함

[2] 스크린샷 분석 결과: ⚠️ 복합적 문제

    [Screenshot 6 증거]
    - 리스트: "dried 라 꽃잎" (dried 미번역)
    - 툴팁: "말린 라 꽃잎" (모두 번역됨!)

    → 같은 아이템인데 표시 위치에 따라 번역 결과가 다름
    → 인벤토리 리스트 렌더링은 다른 코드 경로 사용

[3] 가능한 원인들:

    A. 배포 시점 문제
       - 스크린샷이 JSON 수정 전에 촬영됨
       - 해결: ./deploy.sh 후 게임 재시작 또는 kr:reload

    B. 코드 경로 차이 (★ 핵심 문제)
       - GetDisplayNameEvent.GetFor() 패치는 적용됨
       - 하지만 인벤토리 리스트는 다른 메서드 사용 가능
       - 특히:
         * ShortDisplayName
         * BaseDisplayName
         * 직접 DisplayName 필드 접근
       - 해결: 추가 패치 필요

    C. 컬러 태그 내부 처리
       - "신선한 water]의" 같은 패턴
       - water가 컬러 태그 안에 있어서 파싱 실패
       - 해결: ColorTagProcessor 로직 개선 필요

[4] 책 제목 (별도 시스템)

    - "The Murmurs' Prayer"
    - "피 and Fear: On the Life Cycle of 라"

    책 제목은 V2 아이템 번역기 범위 밖:
    - RandomBooks 시스템으로 생성됨
    - 마르코프 체인 또는 랜덤 단어 조합
    - 별도 패치 필요 (XRL.World.Parts.RandomBook 등)

[5] 즉시 해결 가능한 것들:

    ✅ 어휘 추가: furniture, frill, sundries, fear 등 완료
    ✅ utility 중복 제거 완료
    ⚠️ ./deploy.sh 실행 필요
    ⚠️ 게임에서 kr:reload 실행 필요

[6] 코드 수정이 필요한 것들:

    ❌ 인벤토리 리스트 렌더링 패치
    ❌ 컬러 태그 내부 liquid 이름 처리
    ❌ 책 제목 시스템 패치

========================================================================
결론: 스크린샷의 미번역은 JSON 데이터 문제가 아님
      코드 경로 문제이거나 배포 후 테스트 필요
========================================================================
"""

print(analysis)

# Verification
import json
from pathlib import Path

BASE_PATH = Path(__file__).parent.parent / "LOCALIZATION" / "OBJECTS"

def load_json(path):
    try:
        with open(path, 'r', encoding='utf-8') as f:
            return json.load(f)
    except:
        return None

def load_section(section, target):
    if section:
        for k, v in section.items():
            if not k.startswith("_") and isinstance(v, str):
                target[k.lower()] = v

def load_nested(obj, target):
    for k, v in obj.items():
        if k.startswith("_"):
            continue
        if isinstance(v, dict):
            if "ko" in v:
                target[k.lower()] = v["ko"]
            else:
                load_nested(v, target)
        elif isinstance(v, str):
            target[k.lower()] = v

# Build complete vocab
vocab = {}

common = load_json(BASE_PATH / "items" / "_common.json")
if common:
    load_section(common.get("modifiers"), vocab)
    load_section(common.get("processing"), vocab)
    load_section(common.get("materials"), vocab)

nouns = load_json(BASE_PATH / "items" / "_nouns.json")
if nouns:
    for section in nouns.values():
        if isinstance(section, dict):
            load_section(section, vocab)

vocab_mod = load_json(BASE_PATH / "_vocabulary" / "modifiers.json")
if vocab_mod:
    load_nested(vocab_mod, vocab)

vocab_proc = load_json(BASE_PATH / "_vocabulary" / "processing.json")
if vocab_proc:
    load_nested(vocab_proc, vocab)

suffixes = load_json(BASE_PATH / "_suffixes.json")
if suffixes:
    load_section(suffixes.get("liquids"), vocab)

print("\n[현재 JSON 검증]")
test_words = ["dried", "canned", "filthy", "bubble", "utility", "frill",
              "furniture", "fear", "sundries", "water", "level"]

all_found = True
for word in test_words:
    tr = vocab.get(word)
    if tr:
        print(f"  ✓ {word}: '{tr}'")
    else:
        print(f"  ✗ {word}: 미번역")
        all_found = False

if all_found:
    print("\n모든 어휘가 JSON에 존재합니다.")
    print("./deploy.sh 실행 후 게임 테스트가 필요합니다.")
