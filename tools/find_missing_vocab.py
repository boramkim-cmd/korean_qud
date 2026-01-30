#!/usr/bin/env python3
"""
누락 어휘 분석 스크립트

CompoundTranslator가 실패하는 미번역 복합어를 분석하여
사전에 없는 단어를 빈도순으로 출력합니다.

사용법:
    python3 tools/find_missing_vocab.py              # 전체 출력
    python3 tools/find_missing_vocab.py --top 100    # 상위 100개만
    python3 tools/find_missing_vocab.py --csv        # CSV 형식
"""

import json
import re
import sys
import argparse
from pathlib import Path
from collections import defaultdict

BASE_DIR = Path(__file__).resolve().parent.parent
INDEX_PATH = Path(__file__).parent / "asset_index.json"
LOCALIZATION_DIR = BASE_DIR / "LOCALIZATION"


def load_all_vocab():
    """Load all vocabulary from the 8 dictionaries CompoundTranslator uses."""
    vocab = set()

    # Helper: walk JSON and collect English keys that have Korean string values
    def collect_keys(data, depth=0):
        if not isinstance(data, dict):
            return
        for key, value in data.items():
            if key.startswith('_'):
                continue
            if isinstance(value, str) and re.search(r'[\uac00-\ud7af]', value):
                # English key -> Korean value
                vocab.add(key.lower())
            elif isinstance(value, dict):
                if 'ko' in value:
                    vocab.add(key.lower())
                elif key == 'names':
                    for eng in value:
                        if isinstance(value[eng], str):
                            vocab.add(eng.lower())
                else:
                    collect_keys(value, depth + 1)

    for json_file in LOCALIZATION_DIR.rglob("*.json"):
        try:
            with open(json_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
            collect_keys(data)
        except (json.JSONDecodeError, UnicodeDecodeError):
            continue

    return vocab


def normalize_name(name):
    """Remove color tags and normalize."""
    normalized = re.sub(r'\{\{[^|]*\|([^}]*)\}\}', r'\1', name)
    normalized = normalized.replace('&amp;', '&')
    normalized = re.sub(r'&[A-Za-z]', '', normalized)
    return normalized.lower().strip()


def should_keep_as_is(word):
    """Mirror CompoundTranslator.ShouldKeepAsIs logic."""
    if not word:
        return False
    # Numbers
    if re.match(r'^-?\d+$', word):
        return True
    # Roman numerals
    if re.match(r'^[ivxlcdm]+$', word, re.IGNORECASE) and len(word) <= 8:
        return True
    # MK abbreviations
    if re.match(r'^mk\.?$', word, re.IGNORECASE):
        return True
    # Single letters
    if len(word) == 1 and word.isalpha():
        return True
    # We skip the proper noun check here since we normalize to lowercase
    # All-caps abbreviations checked on original
    return False


def is_compound(name):
    """Check if name would be handled by CompoundTranslator."""
    stripped = normalize_name(name)
    if ' ' not in stripped:
        return False
    if re.search(r'\bof\b', stripped, re.IGNORECASE):
        return False
    if re.search(r'\bthe\b', stripped, re.IGNORECASE):
        return False
    parts = stripped.split()
    return 2 <= len(parts) <= 4


def analyze_missing(top_n=None):
    """Find missing vocabulary words from untranslated compound names."""
    if not INDEX_PATH.exists():
        print("asset_index.json not found. Run build_asset_index.py first.")
        return []

    with open(INDEX_PATH, 'r', encoding='utf-8') as f:
        index = json.load(f)

    vocab = load_all_vocab()
    items = index['items']

    # Collect untranslated compounds
    missing = defaultdict(int)
    examples = defaultdict(list)
    total_compounds = 0
    solvable_by_word = defaultdict(int)  # how many items would be unblocked

    for norm, info in items.items():
        if info.get('translated'):
            continue
        original = info['original']
        if not is_compound(original):
            continue

        total_compounds += 1
        stripped = normalize_name(original)
        words = stripped.split()

        missing_words_for_this = []
        for word in words:
            if word in vocab:
                continue
            if should_keep_as_is(word):
                continue
            missing_words_for_this.append(word)

        for word in missing_words_for_this:
            missing[word] += 1
            if len(examples[word]) < 5:
                examples[word].append(original)

    # Sort by frequency
    sorted_missing = sorted(missing.items(), key=lambda x: -x[1])

    if top_n:
        sorted_missing = sorted_missing[:top_n]

    # Guess which dictionary each word belongs to
    def guess_dict(word):
        # Simple heuristics
        creature_hints = {'cherub', 'golem', 'weep', 'ooze', 'crone', 'sultan',
                          'troll', 'dervish', 'knight', 'templar', 'pilgrim',
                          'acolyte', 'merchant', 'snapjaw', 'madpole', 'eyeless',
                          'slog', 'slug', 'lurker', 'seedsprout', 'sawgrass',
                          'thirst', 'girshling', 'dragonfly', 'rhinox', 'knollworm'}
        material_hints = {'chrome', 'metachrome', 'marble', 'bronze', 'copper',
                          'iron', 'steel', 'crystal', 'bone', 'wood', 'stone',
                          'obsidian', 'glass', 'clay', 'fullerite', 'carbide',
                          'flint', 'zetachrome', 'lead', 'gold', 'silver',
                          'umber', 'ice'}
        modifier_hints = {'mechanical', 'crystalline', 'chitinous', 'serrated',
                          'magnetic', 'folded', 'spiked', 'tattered', 'ancient',
                          'broken', 'rusted', 'blessed', 'cursed', 'worn',
                          'cracked', 'jeweled', 'ornate', 'masterwork',
                          'electrified', 'flaming', 'freezing', 'poisoned',
                          'bloody', 'corroded', 'pristine', 'extradimensional',
                          'psychal', 'spiritual', 'psionic'}

        if word in creature_hints:
            return 'creatures/_common.json (species)'
        if word in material_hints:
            return '_SHARED/materials.json'
        if word in modifier_hints:
            return '_vocabulary/modifiers.json'
        return 'items/_nouns.json (noun)'

    return sorted_missing, examples, total_compounds, vocab, guess_dict


def main():
    parser = argparse.ArgumentParser(description="누락 어휘 분석")
    parser.add_argument("--top", type=int, help="상위 N개만 출력")
    parser.add_argument("--csv", action="store_true", help="CSV 형식 출력")
    parser.add_argument("--summary", action="store_true", help="요약만 출력")
    args = parser.parse_args()

    result = analyze_missing(args.top)
    if not result:
        return 1

    sorted_missing, examples, total_compounds, vocab, guess_dict = result

    if args.summary:
        print(f"미번역 복합어: {total_compounds}개")
        print(f"누락 단어 (고유): {len(sorted_missing)}개")
        print(f"기존 사전 단어: {len(vocab)}개")
        if sorted_missing:
            # Estimate coverage impact of top N
            for n in [50, 100, 150, 200]:
                top_words = set(w for w, _ in sorted_missing[:n])
                # Count items where ALL missing words would be resolved
                with open(INDEX_PATH, 'r', encoding='utf-8') as f:
                    index = json.load(f)
                solvable = 0
                for norm, info in index['items'].items():
                    if info.get('translated') or not is_compound(info['original']):
                        continue
                    stripped = normalize_name(info['original'])
                    words = stripped.split()
                    all_resolved = all(
                        w in vocab or should_keep_as_is(w) or w in top_words
                        for w in words
                    )
                    if all_resolved:
                        solvable += 1
                print(f"상위 {n}개 단어 추가 시 해결 가능: ~{solvable}개 항목")
        return 0

    if args.csv:
        print("word,frequency,suggested_dict,examples")
        for word, freq in sorted_missing:
            exs = "; ".join(examples[word][:3])
            print(f"{word},{freq},{guess_dict(word)},\"{exs}\"")
    else:
        print(f"\n{'='*70}")
        print(f"누락 어휘 분석 결과")
        print(f"{'='*70}")
        print(f"미번역 복합어: {total_compounds}개")
        print(f"누락 단어 (고유): {len(sorted_missing)}개")
        print(f"기존 사전 단어: {len(vocab)}개")
        print(f"{'='*70}\n")
        print(f"{'순위':>4} {'단어':<25} {'빈도':>5} {'추천 사전':<35} {'예시'}")
        print(f"{'-'*4} {'-'*25} {'-'*5} {'-'*35} {'-'*30}")

        for i, (word, freq) in enumerate(sorted_missing, 1):
            exs = ", ".join(examples[word][:2])
            d = guess_dict(word)
            print(f"{i:>4} {word:<25} {freq:>5} {d:<35} {exs}")

    return 0


if __name__ == "__main__":
    sys.exit(main())
