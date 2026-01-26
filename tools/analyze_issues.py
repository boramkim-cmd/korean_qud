#!/usr/bin/env python3
"""
Screenshot Issue Analysis
Categorizes and analyzes untranslated items from screenshots.
"""

# Analysis of each screenshot

issues = """
=== SCREENSHOT ISSUE ANALYSIS ===

[Screenshot 1: 기이한 furniture]
- 문제: "furniture" 미번역
- 유형: BaseNouns 누락
- 원인: _nouns.json furniture 섹션에 "furniture" 단어 자체가 없음
- 수정: furniture -> 가구 추가 완료

[Screenshot 2: filthy 토가]
- 문제: "filthy" 미번역
- 유형: Modifiers 로딩 문제
- 원인: _common.json modifiers에 있지만 V2가 로드하지 않았음
- 수정: 이미 수정됨 (utility 중복 제거 등)

[Screenshot 3: Book Titles]
- "The Murmurs' Prayer"
- "Poor Insulation Baked Under the Shell"
- 문제: 책 제목 미번역
- 유형: ★ 다른 시스템 (Markov 생성 / 고유명사)
- 원인: 책 제목은 절차적 생성됨 - V2 아이템 번역기가 아닌 별도 시스템 필요
- 수정 방향: Books 전용 번역 패치 또는 직접 번역 JSON 필요

[Screenshot 4: 피켓 전지]
- 상태: 정상 번역됨 (fidget cell -> 피켓 전지)

[Screenshot 5: bubble 수준기, brackish 물]
- 문제: "bubble", "brackish" 미번역
- 유형: 컬러 태그 내부 번역 문제
- 원인: 스크린샷 시점에 V2 수정 미배포
- 확인 필요: 배포 후 테스트 필요

[Screenshot 6: dried 라 꽃잎]
- 리스트: "dried 라 꽃잎" (미번역)
- 툴팁: "말린 라 꽃잎" (번역됨!)
- 문제: 같은 아이템인데 표시 위치에 따라 다름
- 유형: ★ 코드 경로 차이
- 원인: 인벤토리 리스트 vs 툴팁이 다른 번역 로직 사용
- 수정 방향: 인벤토리 리스트 렌더링 코드 확인 필요

[Screenshot 7: canned 만물상]
- 문제: "canned" 미번역
- 유형: Processing 적용 문제
- 원인: canned는 processing 카테고리인데 아이템명에 적용 안됨
- 수정 방향: Processing prefix 적용 로직 확인

[Screenshot 8: frill]
- 문제: "frill" 미번역 (단독 아이템명)
- 유형: 단일 단어 아이템
- 원인: CompoundTranslator는 2+ 단어만 처리
- 수정: BaseNouns에 추가 완료

[Screenshot 9-10: 신선한 water]의]
- 문제: "water" 가 컬러태그 안에서 미번역
- 예: "신선한 water]의 재활용 슈트"
- 유형: ★ 컬러 태그 파싱 버그
- 원인: "]의" 패턴에서 liquid 이름 추출 실패
- 수정 방향: ColorTagProcessor 로직 확인

[Screenshot 11-12: Book Titles]
- "Sight Older Than We: Unabridged"
- "피 and Fear: On the Life Cycle of 라"
- 문제: 책 제목 부분 번역
- 유형: Markov 생성 책 제목
- 원인: 책 제목은 단어 조합으로 생성됨 - 일부만 번역
- 참고: "피" (blood)는 번역됨, "Fear"는 미번역
- 수정 방향: 책 제목 생성 시스템 별도 패치 필요

[Screenshot 13: 강철 utility 칼]
- 문제: "utility" 미번역
- 유형: Modifiers 중복 문제
- 원인: _vocabulary/modifiers.json이 "유틸리티"로 덮어씀
- 수정: 중복 제거 완료 -> "다용도"

=== 이슈 분류 요약 ===

1. [수정완료] 어휘 누락/중복:
   - furniture, frill: BaseNouns 추가
   - utility 중복: 제거 완료

2. [배포 필요] JSON 수정 후 게임 테스트:
   - filthy, bubble, brackish, dried, canned 등

3. [코드 수정 필요] 시스템적 문제:
   - 인벤토리 리스트 vs 툴팁 번역 불일치
   - 컬러 태그 내 liquid 이름 파싱 버그
   - Processing prefix (dried, canned) 적용 로직

4. [별도 시스템] 책 제목:
   - Markov 생성 책 제목은 V2 시스템 범위 밖
   - RandomBooks 관련 패치 별도 필요
"""

print(issues)

# Check which items would be translated NOW
print("\n=== 현재 JSON 상태에서 번역 가능 여부 ===")

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
    if not section or not isinstance(section, dict):
        return
    for k, v in section.items():
        if k.startswith("_"):
            continue
        if isinstance(v, str):
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

# Build vocab
vocab = {}

common = load_json(BASE_PATH / "items" / "_common.json")
if common:
    load_section(common.get("modifiers"), vocab)
    load_section(common.get("processing"), vocab)
    load_section(common.get("materials"), vocab)

nouns = load_json(BASE_PATH / "items" / "_nouns.json")
if nouns:
    for section_name, section in nouns.items():
        if not section_name.startswith("_") and isinstance(section, dict):
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

# Test screenshot items
test_words = [
    "furniture", "filthy", "bubble", "brackish", "dried", "canned",
    "frill", "water", "utility", "fear", "sundries", "level"
]

for word in test_words:
    tr = vocab.get(word.lower())
    status = f"'{tr}'" if tr else "미번역"
    print(f"  {word}: {status}")

print("\n=== 결론 ===")
print("1. JSON에 번역 데이터는 대부분 존재함")
print("2. 문제는 V2 시스템이 데이터를 로드/적용하지 못하는 것")
print("3. ./deploy.sh 실행 후 게임에서 kr:reload 필요")
print("4. 책 제목은 별도 시스템이므로 V2로 해결 불가")
