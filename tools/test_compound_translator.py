#!/usr/bin/env python3
"""
CompoundTranslator 검증 스크립트
- 150개 이상 테스트 케이스
- 기본 단어, 복합어, 접두사/접미사, 컬러태그 다양한 케이스
- 중요: 컬러태그 파이프(|) 왼쪽은 번역 금지!

테스트 카테고리:
1. 기본 단어 (단일)
2. 복합어 (2-4단어)
3. 접두사 패턴
4. 접미사 패턴
5. 컬러태그 단일
6. 컬러태그 중첩
7. 컬러태그 내부 복합어
8. 셰이더 태그
9. 소유격 패턴
10. of 패턴
11. 시체 패턴
12. 음식 패턴
13. 에지 케이스
"""

import json
import re
import os
from pathlib import Path
from typing import Dict, List, Tuple, Optional
from dataclasses import dataclass
from enum import Enum

BASE_DIR = Path("/Users/ben/Desktop/qud_korean")
LOCALIZATION_DIR = BASE_DIR / "LOCALIZATION"


class TestResult(Enum):
    PASS = "✅"
    FAIL = "❌"
    SKIP = "⏭️"


@dataclass
class TestCase:
    """테스트 케이스 정의"""
    id: int
    category: str
    input: str
    expected: str
    description: str
    check_color_tag_preserved: bool = False  # 컬러태그 왼쪽 보존 확인


# ============================================================
# 어휘 로드
# ============================================================

def load_all_vocabulary() -> Dict[str, str]:
    """모든 JSON 파일에서 어휘 로드"""
    vocab = {}

    for json_file in LOCALIZATION_DIR.rglob("*.json"):
        try:
            with open(json_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
            _extract_vocab(data, vocab)
        except (json.JSONDecodeError, UnicodeDecodeError):
            continue

    return vocab


def _extract_vocab(data, vocab: Dict[str, str], prefix=""):
    """재귀적으로 어휘 추출"""
    if not isinstance(data, dict):
        return

    for key, value in data.items():
        if key.startswith('_'):
            continue

        if isinstance(value, str):
            # 영어 키 -> 한글 값
            if not re.search(r'[\uac00-\ud7af]', key):
                vocab[key.lower()] = value
        elif isinstance(value, dict):
            if 'ko' in value:
                # _SHARED 형식: {"ko": "번역", "aliases": [...]}
                vocab[key.lower()] = value['ko']
                # aliases도 등록
                for alias in value.get('aliases', []):
                    vocab[alias.lower()] = value['ko']
            elif 'names' in value:
                for eng, kor in value.get('names', {}).items():
                    if isinstance(kor, str):
                        vocab[eng.lower()] = kor
            else:
                _extract_vocab(value, vocab, f"{prefix}{key}.")


# ============================================================
# 번역 시뮬레이터 (C# 로직 모방)
# ============================================================

class TranslationSimulator:
    """C# TranslationEngine + CompoundTranslator 시뮬레이터"""

    # 컬러태그 패턴: {{X|content}} 또는 {{shader|content}}
    COLOR_TAG_PATTERN = re.compile(r'\{\{([^|]+)\|([^}]+)\}\}')

    def __init__(self, vocab: Dict[str, str]):
        self.vocab = vocab

    def strip_color_tags(self, text: str) -> str:
        """컬러태그 제거 (내용만 추출)"""
        return self.COLOR_TAG_PATTERN.sub(r'\2', text)

    def translate_word(self, word: str) -> Optional[str]:
        """단일 단어 번역"""
        lower = word.lower()
        if lower in self.vocab:
            return self.vocab[lower]
        if word in self.vocab:
            return self.vocab[word]
        return None

    def translate_compound(self, text: str) -> Optional[str]:
        """복합어 번역 (CompoundTranslator 로직)"""
        stripped = self.strip_color_tags(text)
        parts = stripped.split()

        if len(parts) < 2 or len(parts) > 4:
            return None

        translated_parts = []
        for part in parts:
            trans = self.translate_word(part)
            if trans is None:
                return None
            translated_parts.append(trans)

        return ' '.join(translated_parts)

    def translate_with_color_tags(self, text: str) -> str:
        """컬러태그 보존하면서 번역 (태그 외부 텍스트도 번역)"""
        # 1. 태그와 텍스트 영역 분리
        parts = []
        last_end = 0

        for match in self.COLOR_TAG_PATTERN.finditer(text):
            # 태그 앞의 일반 텍스트
            if match.start() > last_end:
                plain_text = text[last_end:match.start()]
                parts.append(('text', plain_text))

            # 태그 자체
            tag_name = match.group(1)  # 파이프 왼쪽 (번역 금지!)
            content = match.group(2)   # 파이프 오른쪽 (번역 대상)
            parts.append(('tag', tag_name, content))
            last_end = match.end()

        # 마지막 태그 뒤의 일반 텍스트
        if last_end < len(text):
            plain_text = text[last_end:]
            parts.append(('text', plain_text))

        # 2. 각 부분 번역
        result_parts = []
        for part in parts:
            if part[0] == 'text':
                plain_text = part[1].strip()
                if plain_text:
                    # 일반 텍스트 번역 시도
                    trans = self.translate_word(plain_text)
                    if trans is None:
                        trans = self.translate_compound(plain_text)
                    if trans is None:
                        trans = plain_text
                    # 원본 공백 구조 유지
                    leading = len(part[1]) - len(part[1].lstrip())
                    trailing = len(part[1]) - len(part[1].rstrip())
                    result_parts.append(part[1][:leading] + trans + part[1][len(part[1])-trailing:] if trailing else part[1][:leading] + trans)
                else:
                    result_parts.append(part[1])  # 공백만 있으면 그대로
            else:
                tag_name, content = part[1], part[2]
                # 태그 내용 번역 시도
                trans = self.translate_word(content)
                if trans is None:
                    trans = self.translate_compound(content)
                if trans is None:
                    trans = content
                result_parts.append(f"{{{{{tag_name}|{trans}}}}}")

        return ''.join(result_parts)

    def translate(self, text: str) -> str:
        """전체 번역 프로세스"""
        if not text:
            return text

        # 1. 컬러태그가 있으면 태그 보존 번역
        if '{{' in text and '|' in text:
            return self.translate_with_color_tags(text)

        # 2. 직접 번역 시도
        trans = self.translate_word(text)
        if trans:
            return trans

        # 3. 복합어 번역 시도
        compound = self.translate_compound(text)
        if compound:
            return compound

        return text


# ============================================================
# 테스트 케이스 정의 (150개 이상)
# ============================================================

def create_test_cases() -> List[TestCase]:
    """150개 이상의 테스트 케이스 생성"""
    cases = []
    id_counter = [0]

    def add(category: str, input: str, expected: str, desc: str, check_color=False):
        id_counter[0] += 1
        cases.append(TestCase(id_counter[0], category, input, expected, desc, check_color))

    # ========== 1. 기본 단어 (20개) ==========
    add("기본단어", "bear", "곰", "단일 동물")
    add("기본단어", "golem", "골렘", "단일 명사")
    add("기본단어", "cherub", "케루브", "단일 명사")
    add("기본단어", "sword", "검", "단일 무기")
    add("기본단어", "armor", "갑옷", "단일 방어구")
    add("기본단어", "iron", "철", "단일 재료")
    add("기본단어", "steel", "강철", "단일 재료")
    add("기본단어", "broken", "부서진", "단일 수식어")
    add("기본단어", "rusted", "녹슨", "단일 수식어")
    add("기본단어", "corpse", "시체", "단일 명사")
    add("기본단어", "bat", "박쥐", "단일 동물")
    add("기본단어", "cat", "고양이", "단일 동물")
    add("기본단어", "dog", "개", "단일 동물")
    add("기본단어", "antelope", "영양", "단일 동물")
    add("기본단어", "head", "머리", "신체 부위")
    add("기본단어", "body", "몸통", "신체 부위")
    add("기본단어", "feet", "발", "신체 부위")
    add("기본단어", "hand", "손", "신체 부위")
    add("기본단어", "water", "물", "액체")
    add("기본단어", "fire", "불", "원소")

    # ========== 2. 복합어 2단어 (30개) ==========
    add("복합어2", "bear golem", "곰 골렘", "동물+골렘")
    add("복합어2", "cat golem", "고양이 골렘", "동물+골렘")
    add("복합어2", "bat golem", "박쥐 골렘", "동물+골렘")
    add("복합어2", "dog golem", "개 골렘", "동물+골렘")
    add("복합어2", "antelope cherub", "영양 케루브", "동물+체루브")
    add("복합어2", "bear cherub", "곰 케루브", "동물+체루브")
    add("복합어2", "cat cherub", "고양이 케루브", "동물+체루브")
    add("복합어2", "iron sword", "철 검", "재료+무기")
    add("복합어2", "steel sword", "강철 검", "재료+무기")
    add("복합어2", "iron armor", "철 갑옷", "재료+방어구")
    add("복합어2", "steel armor", "강철 갑옷", "재료+방어구")
    add("복합어2", "broken sword", "부서진 검", "수식어+무기")
    add("복합어2", "rusted armor", "녹슨 갑옷", "수식어+방어구")
    add("복합어2", "bear corpse", "곰 시체", "동물+시체")
    add("복합어2", "cat corpse", "고양이 시체", "동물+시체")
    add("복합어2", "iron helm", "철 투구", "재료+투구")
    add("복합어2", "leather armor", "가죽 갑옷", "재료+방어구")
    add("복합어2", "wooden shield", "나무 방패", "재료+방패")
    add("복합어2", "crystal dagger", "수정 단검", "재료+무기")
    add("복합어2", "frozen water", "얼어붙은 물", "상태+액체")
    add("복합어2", "cooked meat", "조리된 고기", "가공+음식")
    add("복합어2", "dried meat", "말린 고기", "가공+음식")
    add("복합어2", "raw meat", "생고기", "가공+음식 (공백없음)")  # 자연스러운 한글 표현
    add("복합어2", "fresh corpse", "신선한 시체", "상태+시체")
    add("복합어2", "giant bear", "거대한 곰", "크기+동물")
    add("복합어2", "small cat", "작은 고양이", "크기+동물")
    add("복합어2", "young bear", "어린 곰", "나이+동물")
    add("복합어2", "aged dog", "늙은 개", "나이+동물")
    add("복합어2", "wild cat", "야생 고양이", "상태+동물")
    add("복합어2", "tame dog", "길들인 개", "상태+동물")  # common_terms에서 수정됨

    # ========== 3. 복합어 3단어 (20개) ==========
    add("복합어3", "giant bear golem", "거대한 곰 골렘", "크기+동물+골렘")
    add("복합어3", "small cat cherub", "작은 고양이 케루브", "크기+동물+체루브")
    add("복합어3", "broken iron sword", "부서진 철 검", "상태+재료+무기")
    add("복합어3", "rusted steel armor", "녹슨 강철 갑옷", "상태+재료+방어구")
    add("복합어3", "frozen bear corpse", "얼어붙은 곰 시체", "상태+동물+시체")
    add("복합어3", "cooked bear meat", "조리된 곰 고기", "가공+동물+음식")
    add("복합어3", "dried cat meat", "말린 고양이 고기", "가공+동물+음식")
    add("복합어3", "fresh dog corpse", "신선한 개 시체", "상태+동물+시체")
    add("복합어3", "giant iron golem", "거대한 철 골렘", "크기+재료+골렘")
    add("복합어3", "small wooden shield", "작은 나무 방패", "크기+재료+방패")
    add("복합어3", "old leather armor", "낡은 가죽 갑옷", "상태+재료+방어구")
    add("복합어3", "broken crystal dagger", "부서진 수정 단검", "상태+재료+무기")
    add("복합어3", "wild bear cub", "야생 곰 새끼", "상태+동물+유아")
    add("복합어3", "young cat golem", "어린 고양이 골렘", "나이+동물+골렘")
    add("복합어3", "frozen iron armor", "얼어붙은 철 갑옷", "상태+재료+방어구")
    add("복합어3", "burnt wooden shield", "타버린 나무 방패", "상태+재료+방패")
    add("복합어3", "cracked crystal helm", "금간 수정 투구", "상태+재료+투구")
    add("복합어3", "giant wild bear", "거대한 야생 곰", "크기+상태+동물")
    add("복합어3", "small tame cat", "작은 길들인 고양이", "크기+상태+동물")
    add("복합어3", "aged wild dog", "늙은 야생 개", "나이+상태+동물")

    # ========== 4. 복합어 4단어 (10개) ==========
    add("복합어4", "giant frozen bear golem", "거대한 얼어붙은 곰 골렘", "크기+상태+동물+골렘")
    add("복합어4", "small broken iron sword", "작은 부서진 철 검", "크기+상태+재료+무기")
    add("복합어4", "old rusted steel armor", "낡은 녹슨 강철 갑옷", "나이+상태+재료+방어구")
    add("복합어4", "fresh cooked bear meat", "신선한 조리된 곰 고기", "상태+가공+동물+음식")
    add("복합어4", "giant wild bear corpse", "거대한 야생 곰 시체", "크기+상태+동물+시체")
    add("복합어4", "small young cat golem", "작은 어린 고양이 골렘", "크기+나이+동물+골렘")
    add("복합어4", "old broken wooden shield", "낡은 부서진 나무 방패", "나이+상태+재료+방패")
    add("복합어4", "frozen dried bear meat", "얼어붙은 말린 곰 고기", "상태+가공+동물+음식")
    add("복합어4", "giant cracked crystal helm", "거대한 금간 수정 투구", "크기+상태+재료+투구")
    add("복합어4", "small burnt leather armor", "작은 타버린 가죽 갑옷", "크기+상태+재료+방어구")

    # ========== 5. 컬러태그 단일 (20개) ==========
    # 중요: 파이프 왼쪽은 번역 금지!
    add("컬러태그1", "{{c|bear}}", "{{c|곰}}", "단일 컬러태그", True)
    add("컬러태그1", "{{r|golem}}", "{{r|골렘}}", "단일 컬러태그", True)
    add("컬러태그1", "{{g|sword}}", "{{g|검}}", "단일 컬러태그", True)
    add("컬러태그1", "{{y|iron}}", "{{y|철}}", "단일 컬러태그", True)
    add("컬러태그1", "{{w|water}}", "{{w|물}}", "단일 컬러태그", True)
    add("컬러태그1", "{{C|bear}}", "{{C|곰}}", "대문자 컬러코드", True)
    add("컬러태그1", "{{R|golem}}", "{{R|골렘}}", "대문자 컬러코드", True)
    add("컬러태그1", "{{G|sword}}", "{{G|검}}", "대문자 컬러코드", True)
    add("컬러태그1", "{{Y|iron}}", "{{Y|철}}", "대문자 컬러코드", True)
    add("컬러태그1", "{{W|water}}", "{{W|물}}", "대문자 컬러코드", True)
    add("컬러태그1", "{{c|broken}}", "{{c|부서진}}", "수식어 컬러태그", True)
    add("컬러태그1", "{{r|rusted}}", "{{r|녹슨}}", "수식어 컬러태그", True)
    add("컬러태그1", "{{g|frozen}}", "{{g|얼어붙은}}", "상태 컬러태그", True)
    add("컬러태그1", "{{y|cooked}}", "{{y|조리된}}", "가공 컬러태그", True)
    add("컬러태그1", "{{w|dried}}", "{{w|말린}}", "가공 컬러태그", True)
    add("컬러태그1", "{{c|corpse}}", "{{c|시체}}", "명사 컬러태그", True)
    add("컬러태그1", "{{r|armor}}", "{{r|갑옷}}", "명사 컬러태그", True)
    add("컬러태그1", "{{g|helm}}", "{{g|투구}}", "명사 컬러태그", True)
    add("컬러태그1", "{{y|shield}}", "{{y|방패}}", "명사 컬러태그", True)
    add("컬러태그1", "{{w|dagger}}", "{{w|단검}}", "명사 컬러태그", True)

    # ========== 6. 컬러태그 내부 복합어 (20개) ==========
    add("컬러태그복합", "{{c|bear golem}}", "{{c|곰 골렘}}", "복합어 in 태그", True)
    add("컬러태그복합", "{{r|iron sword}}", "{{r|철 검}}", "복합어 in 태그", True)
    add("컬러태그복합", "{{g|broken armor}}", "{{g|부서진 갑옷}}", "복합어 in 태그", True)
    add("컬러태그복합", "{{y|frozen water}}", "{{y|얼어붙은 물}}", "복합어 in 태그", True)
    add("컬러태그복합", "{{w|bear corpse}}", "{{w|곰 시체}}", "복합어 in 태그", True)
    add("컬러태그복합", "{{c|giant bear golem}}", "{{c|거대한 곰 골렘}}", "3단어 in 태그", True)
    add("컬러태그복합", "{{r|broken iron sword}}", "{{r|부서진 철 검}}", "3단어 in 태그", True)
    add("컬러태그복합", "{{g|frozen bear corpse}}", "{{g|얼어붙은 곰 시체}}", "3단어 in 태그", True)
    add("컬러태그복합", "{{y|cooked bear meat}}", "{{y|조리된 곰 고기}}", "3단어 in 태그", True)
    add("컬러태그복합", "{{w|rusted steel armor}}", "{{w|녹슨 강철 갑옷}}", "3단어 in 태그", True)
    add("컬러태그복합", "{{C|antelope cherub}}", "{{C|영양 케루브}}", "대문자 복합어", True)
    add("컬러태그복합", "{{R|cat golem}}", "{{R|고양이 골렘}}", "대문자 복합어", True)
    add("컬러태그복합", "{{G|dog corpse}}", "{{G|개 시체}}", "대문자 복합어", True)
    add("컬러태그복합", "{{Y|bat cherub}}", "{{Y|박쥐 케루브}}", "대문자 복합어", True)
    add("컬러태그복합", "{{W|steel sword}}", "{{W|강철 검}}", "대문자 복합어", True)
    add("컬러태그복합", "{{c|leather armor}}", "{{c|가죽 갑옷}}", "재료+방어구 in 태그", True)
    add("컬러태그복합", "{{r|wooden shield}}", "{{r|나무 방패}}", "재료+방패 in 태그", True)
    add("컬러태그복합", "{{g|crystal dagger}}", "{{g|수정 단검}}", "재료+무기 in 태그", True)
    add("컬러태그복합", "{{y|iron helm}}", "{{y|철 투구}}", "재료+투구 in 태그", True)
    add("컬러태그복합", "{{w|fresh corpse}}", "{{w|신선한 시체}}", "상태+명사 in 태그", True)

    # ========== 7. 셰이더 태그 (15개) ==========
    # 셰이더 이름(파이프 왼쪽)은 절대 번역 금지!
    add("셰이더", "{{fiery|sword}}", "{{fiery|검}}", "셰이더 태그", True)
    add("셰이더", "{{icy|armor}}", "{{icy|갑옷}}", "셰이더 태그", True)
    add("셰이더", "{{chrome|helm}}", "{{chrome|투구}}", "셰이더 태그", True)
    add("셰이더", "{{crystalline|dagger}}", "{{crystalline|단검}}", "셰이더 태그", True)
    add("셰이더", "{{bloody|corpse}}", "{{bloody|시체}}", "셰이더 태그", True)
    add("셰이더", "{{holographic|shield}}", "{{holographic|방패}}", "셰이더 태그", True)
    add("셰이더", "{{prismatic|sword}}", "{{prismatic|검}}", "셰이더 태그", True)
    add("셰이더", "{{nectar|water}}", "{{nectar|물}}", "셰이더 태그", True)
    add("셰이더", "{{love|bear}}", "{{love|곰}}", "셰이더 태그", True)
    add("셰이더", "{{shade|cat}}", "{{shade|고양이}}", "셰이더 태그", True)
    add("셰이더", "{{fiery|bear golem}}", "{{fiery|곰 골렘}}", "셰이더+복합어", True)
    add("셰이더", "{{icy|iron sword}}", "{{icy|철 검}}", "셰이더+복합어", True)
    add("셰이더", "{{chrome|steel armor}}", "{{chrome|강철 갑옷}}", "셰이더+복합어", True)
    add("셰이더", "{{bloody|bear corpse}}", "{{bloody|곰 시체}}", "셰이더+복합어", True)
    add("셰이더", "{{prismatic|crystal dagger}}", "{{prismatic|수정 단검}}", "셰이더+복합어", True)

    # ========== 8. 태그+텍스트 혼합 패턴 (중요!) ==========
    # 실제 게임에서 많이 사용되는 패턴: {{tag|content}} + plain text
    add("태그텍스트혼합", "{{c|bear}} golem", "{{c|곰}} 골렘", "태그+텍스트 혼합", True)
    add("태그텍스트혼합", "{{r|iron}} sword", "{{r|철}} 검", "태그+텍스트 혼합", True)
    add("태그텍스트혼합", "broken {{g|armor}}", "부서진 {{g|갑옷}}", "텍스트+태그 혼합", True)
    add("태그텍스트혼합", "{{c|bear}} {{r|corpse}}", "{{c|곰}} {{r|시체}}", "다중 태그", True)
    add("태그텍스트혼합", "{{y|iron}} {{w|sword}}", "{{y|철}} {{w|검}}", "다중 태그", True)
    add("태그텍스트혼합", "giant {{c|bear}} golem", "거대한 {{c|곰}} 골렘", "텍스트+태그+텍스트", True)
    add("태그텍스트혼합", "{{r|broken}} iron {{g|sword}}", "{{r|부서진}} 철 {{g|검}}", "복잡한 혼합", True)
    add("태그텍스트혼합", "{{w|bronze}} mace", "{{w|청동}} 철퇴", "실제 게임: Items.xml", True)
    add("태그텍스트혼합", "{{Y|steel}} dagger", "{{Y|강철}} 단검", "실제 게임: Items.xml", True)
    add("태그텍스트혼합", "{{b|carbide}} hammer", "{{b|카바이드}} 해머", "실제 게임: Items.xml", True)
    add("태그텍스트혼합", "two-handed {{Y|steel}} sword", "양손 {{Y|강철}} 검", "실제 게임 패턴", True)

    # ========== 9. 2중 중첩 태그 (제한 사항) ==========
    # 2중 중첩은 복잡한 파싱 필요 - 현재 미지원
    add("제한사항_중첩", "{{c|{{r|bear}}}}", "{{c|{{r|bear}}}}", "2중 중첩 미지원", True)
    add("제한사항_중첩", "{{g|{{y|sword}}}}", "{{g|{{y|sword}}}}", "2중 중첩 미지원", True)
    add("제한사항_중첩", "{{w|{{c|iron}}}}", "{{w|{{c|iron}}}}", "2중 중첩 미지원", True)
    add("제한사항_중첩", "{{fiery|{{c|bear}}}}", "{{fiery|{{c|bear}}}}", "셰이더+컬러 중첩 미지원", True)

    # ========== 9. 파이프 왼쪽 보존 검증 (20개) ==========
    # 이 테스트들은 파이프 왼쪽이 절대 번역되지 않았는지 확인
    add("파이프보존", "{{iron|sword}}", "{{iron|검}}", "iron은 셰이더로 보존", True)
    add("파이프보존", "{{steel|armor}}", "{{steel|갑옷}}", "steel은 셰이더로 보존", True)
    add("파이프보존", "{{bear|golem}}", "{{bear|골렘}}", "bear은 셰이더로 보존", True)
    add("파이프보존", "{{fire|sword}}", "{{fire|검}}", "fire은 셰이더로 보존", True)
    add("파이프보존", "{{water|armor}}", "{{water|갑옷}}", "water은 셰이더로 보존", True)
    add("파이프보존", "{{broken|sword}}", "{{broken|검}}", "broken은 셰이더로 보존", True)
    add("파이프보존", "{{frozen|armor}}", "{{frozen|갑옷}}", "frozen은 셰이더로 보존", True)
    add("파이프보존", "{{corpse|bear}}", "{{corpse|곰}}", "corpse은 셰이더로 보존", True)
    add("파이프보존", "{{golem|bear}}", "{{golem|곰}}", "golem은 셰이더로 보존", True)
    add("파이프보존", "{{sword|iron}}", "{{sword|철}}", "sword은 셰이더로 보존", True)
    add("파이프보존", "{{armor|steel}}", "{{armor|강철}}", "armor은 셰이더로 보존", True)
    add("파이프보존", "{{helm|iron}}", "{{helm|철}}", "helm은 셰이더로 보존", True)
    add("파이프보존", "{{shield|wooden}}", "{{shield|나무}}", "shield은 셰이더로 보존", True)
    add("파이프보존", "{{dagger|crystal}}", "{{dagger|수정}}", "dagger은 셰이더로 보존", True)
    add("파이프보존", "{{meat|bear}}", "{{meat|곰}}", "meat은 셰이더로 보존", True)
    add("파이프보존", "{{cat|golem}}", "{{cat|골렘}}", "cat은 셰이더로 보존", True)
    add("파이프보존", "{{dog|cherub}}", "{{dog|케루브}}", "dog은 셰이더로 보존", True)
    add("파이프보존", "{{bat|corpse}}", "{{bat|시체}}", "bat은 셰이더로 보존", True)
    add("파이프보존", "{{antelope|golem}}", "{{antelope|골렘}}", "antelope은 셰이더로 보존", True)
    add("파이프보존", "{{giant|bear}}", "{{giant|곰}}", "giant은 셰이더로 보존", True)

    # ========== 10. 실제 게임 데이터 기반 테스트 (20개) ==========
    # Creatures.xml에서 추출한 실제 게임 이름
    add("실제게임_골렘", "bear golem", "곰 골렘", "실제 게임: Creatures.xml")
    add("실제게임_골렘", "cat golem", "고양이 골렘", "실제 게임: Creatures.xml")
    add("실제게임_골렘", "dog golem", "개 골렘", "실제 게임: Creatures.xml")
    add("실제게임_골렘", "bat golem", "박쥐 골렘", "실제 게임: Creatures.xml")
    add("실제게임_골렘", "bird golem", "조류 골렘", "실제 게임: Creatures.xml (bird=조류)")
    add("실제게임_골렘", "fish golem", "물고기 골렘", "실제 게임: Creatures.xml")
    add("실제게임_골렘", "crab golem", "게 골렘", "실제 게임: Creatures.xml")
    add("실제게임_골렘", "spider golem", "거미 골렘", "실제 게임: Creatures.xml")
    add("실제게임_체루브", "antelope cherub", "영양 케루브", "실제 게임: Creatures.xml")
    add("실제게임_체루브", "bear cherub", "곰 케루브", "실제 게임: Creatures.xml")
    add("실제게임_체루브", "cat cherub", "고양이 케루브", "실제 게임: Creatures.xml")
    add("실제게임_체루브", "dog cherub", "개 케루브", "실제 게임: Creatures.xml")
    add("실제게임_체루브", "bat cherub", "박쥐 케루브", "실제 게임: Creatures.xml")
    add("실제게임_체루브", "spider cherub", "거미 케루브", "실제 게임: Creatures.xml")
    # Items.xml 컬러태그 패턴은 태그텍스트혼합 카테고리로 이동됨
    # 실제 게임의 mechanical 패턴
    add("실제게임_기계", "mechanical bear cherub", "기계 곰 케루브", "실제 게임: Creatures.xml")
    add("실제게임_기계", "mechanical cat golem", "기계 고양이 골렘", "실제 게임 추정")
    add("실제게임_기계", "mechanical dog cherub", "기계 개 케루브", "실제 게임: Creatures.xml")

    # ========== 11. 에지 케이스 (10개) ==========
    add("에지케이스", "", "", "빈 문자열")
    add("에지케이스", "   ", "   ", "공백만")
    add("에지케이스", "unknownword", "unknownword", "미등록 단어")
    add("에지케이스", "a b c d e", "a b c d e", "5단어 (범위 초과)")
    add("에지케이스", "{{|bear}}", "{{|bear}}", "빈 태그명")
    add("에지케이스", "{{c|}}", "{{c|}}", "빈 내용")
    add("에지케이스", "bear  golem", "곰 골렘", "이중 공백 정규화 후 번역")
    add("에지케이스", "Bear", "곰", "대문자 시작")
    add("에지케이스", "BEAR", "곰", "전체 대문자")
    add("에지케이스", "BeAr GoLeM", "곰 골렘", "혼합 대소문자")

    return cases


# ============================================================
# 테스트 실행
# ============================================================

def verify_color_tag_preserved(input_str: str, output_str: str) -> bool:
    """컬러태그 파이프 왼쪽이 보존되었는지 확인"""
    input_tags = re.findall(r'\{\{([^|]+)\|', input_str)
    output_tags = re.findall(r'\{\{([^|]+)\|', output_str)

    if len(input_tags) != len(output_tags):
        return False

    for i, o in zip(input_tags, output_tags):
        if i != o:
            return False

    return True


def run_tests():
    """테스트 실행"""
    print("=" * 80)
    print("CompoundTranslator 검증 테스트")
    print("=" * 80)
    print()

    # 어휘 로드
    print("어휘 로드 중...")
    vocab = load_all_vocabulary()
    print(f"  로드된 어휘: {len(vocab)}개")
    print()

    # 시뮬레이터 생성
    sim = TranslationSimulator(vocab)

    # 테스트 케이스 생성
    cases = create_test_cases()
    print(f"테스트 케이스: {len(cases)}개")
    print()

    # 결과 집계
    results = {
        TestResult.PASS: 0,
        TestResult.FAIL: 0,
        TestResult.SKIP: 0
    }
    failures = []

    # 카테고리별 결과
    category_results = {}

    for case in cases:
        if case.category not in category_results:
            category_results[case.category] = {"pass": 0, "fail": 0, "skip": 0}

        try:
            actual = sim.translate(case.input)

            # 컬러태그 보존 확인
            if case.check_color_tag_preserved and '{{' in case.input:
                if not verify_color_tag_preserved(case.input, actual):
                    result = TestResult.FAIL
                    failures.append((case, actual, "컬러태그 파이프 왼쪽이 변경됨!"))
                    results[TestResult.FAIL] += 1
                    category_results[case.category]["fail"] += 1
                    continue

            # 결과 비교
            if actual == case.expected:
                result = TestResult.PASS
                results[TestResult.PASS] += 1
                category_results[case.category]["pass"] += 1
            else:
                result = TestResult.FAIL
                failures.append((case, actual, None))
                results[TestResult.FAIL] += 1
                category_results[case.category]["fail"] += 1

        except Exception as e:
            result = TestResult.FAIL
            failures.append((case, str(e), "예외 발생"))
            results[TestResult.FAIL] += 1
            category_results[case.category]["fail"] += 1

    # 결과 출력
    print("=" * 80)
    print("카테고리별 결과")
    print("=" * 80)
    for cat, res in sorted(category_results.items()):
        total = res["pass"] + res["fail"] + res["skip"]
        pct = res["pass"] / total * 100 if total > 0 else 0
        status = "✅" if res["fail"] == 0 else "❌"
        print(f"  {status} {cat}: {res['pass']}/{total} ({pct:.1f}%)")
    print()

    # 전체 결과
    total = sum(results.values())
    print("=" * 80)
    print("전체 결과")
    print("=" * 80)
    print(f"  ✅ 통과: {results[TestResult.PASS]}/{total} ({results[TestResult.PASS]/total*100:.1f}%)")
    print(f"  ❌ 실패: {results[TestResult.FAIL]}/{total}")
    print(f"  ⏭️  스킵: {results[TestResult.SKIP]}/{total}")
    print()

    # 실패 상세
    if failures:
        print("=" * 80)
        print("실패 상세 (처음 30개)")
        print("=" * 80)
        for i, (case, actual, note) in enumerate(failures[:30]):
            print(f"\n[{case.id}] {case.category}: {case.description}")
            print(f"  입력:   '{case.input}'")
            print(f"  기대:   '{case.expected}'")
            print(f"  실제:   '{actual}'")
            if note:
                print(f"  ⚠️  {note}")

    # 요약
    print()
    print("=" * 80)
    if results[TestResult.FAIL] == 0:
        print("🎉 모든 테스트 통과!")
    else:
        print(f"⚠️  {results[TestResult.FAIL]}개 테스트 실패")
    print("=" * 80)

    return results[TestResult.FAIL] == 0


if __name__ == "__main__":
    success = run_tests()
    exit(0 if success else 1)
