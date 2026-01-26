#!/usr/bin/env python3
"""Test BookTitleTranslator logic - Korean word order."""

import re
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

prefixes = {}
base_nouns = {}

common = load_json(BASE_PATH / "items" / "_common.json")
if common:
    for sec in ["materials", "qualities", "modifiers", "processing", "tonics"]:
        load_section(common.get(sec), prefixes)

vocab_mod = load_json(BASE_PATH / "_vocabulary" / "modifiers.json")
if vocab_mod:
    load_nested(vocab_mod, prefixes)

nouns = load_json(BASE_PATH / "items" / "_nouns.json")
if nouns:
    for section in nouns.values():
        if isinstance(section, dict):
            load_section(section, base_nouns)

suffixes = load_json(BASE_PATH / "_suffixes.json")
if suffixes:
    load_section(suffixes.get("liquids"), base_nouns)

prefixes_sorted = sorted(prefixes.items(), key=lambda x: len(x[0]), reverse=True)
base_nouns_sorted = sorted(base_nouns.items(), key=lambda x: len(x[0]), reverse=True)

def translate_words(text):
    result = text
    result = re.sub(r'\bthe\s+', '', result, flags=re.IGNORECASE)
    for key, value in base_nouns_sorted:
        pattern = rf'(^|\s|\[|")({re.escape(key)})($|\s|[,.\[\]():\'\"!?])'
        result = re.sub(pattern, lambda m: m.group(1) + value + m.group(3), result, flags=re.IGNORECASE)
    for key, value in prefixes_sorted:
        pattern = rf'(^|\s|\[|")({re.escape(key)})(\s|$|\]|[:\'\"!?])'
        result = re.sub(pattern, lambda m: m.group(1) + value + m.group(3), result, flags=re.IGNORECASE)
    return result.strip()

def translate_and_pattern(text):
    match = re.match(r'^(.+?)\s+and\s+(.+)$', text, re.IGNORECASE)
    if match:
        first_ko = translate_words(match.group(1).strip())
        second_ko = translate_words(match.group(2).strip())
        return f"{first_ko}와 {second_ko}"
    return text

def translate_phrase(phrase):
    result = phrase

    # 0. Possessive "X's Y" or "X' Y" -> "X의 Y"
    poss_match = re.match(r"^(?:The\s+)?(.+?)'s?\s+(.+)$", result, re.IGNORECASE)
    if poss_match:
        owner_ko = translate_words(poss_match.group(1).strip())
        owned_ko = translate_words(poss_match.group(2).strip())
        return f"{owner_ko}의 {owned_ko}"

    # 1. "On the X of Y" -> "Y의 X에 대하여"
    on_of_match = re.match(r'^On\s+(?:the\s+)?(.+?)\s+of\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if on_of_match:
        subject_ko = translate_words(on_of_match.group(1).strip())
        of_part_ko = translate_words(on_of_match.group(2).strip())
        return f"{of_part_ko}의 {subject_ko}에 대하여"

    # 2. "On the X" -> "X에 대하여"
    on_match = re.match(r'^On\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if on_match:
        subject_ko = translate_words(on_match.group(1).strip())
        return f"{subject_ko}에 대하여"

    # 3. "X of Y" -> "Y의 X"
    of_match = re.match(r'^(.+?)\s+of\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if of_match:
        item_ko = translate_words(of_match.group(1).strip())
        of_part_ko = translate_words(of_match.group(2).strip())
        return f"{of_part_ko}의 {item_ko}"

    # 4. "X and Y" -> "X와 Y"
    result = translate_and_pattern(result)
    result = translate_words(result)
    return result

def translate_book_title(name):
    if ':' in name:
        colon_idx = name.index(':')
        title = name[:colon_idx].strip()
        subtitle = name[colon_idx + 1:].strip()
        return f"{translate_phrase(title)}: {translate_phrase(subtitle)}"
    return translate_phrase(name)

test_cases = [
    ("Blood and Fear: On the Life Cycle of La", "피와 공포: 라의 생명 주기에 대하여"),
    ("The Murmurs' Prayer", "속삭임의 기도"),
    ("Asphodel's Testament", "아스포델의 유언"),
    ("On the Life Cycle of La", "라의 생명 주기에 대하여"),
    ("On Sight", "시야에 대하여"),
    ("Sandals of the River-wives", "River-wives의 샌들"),
    ("Blood and Fear", "피와 공포"),
    ("Fear and Life", "공포와 생명"),
]

print("=" * 80)
print("BookTitleTranslator Test - Korean Word Order")
print("=" * 80)

passed = 0
for test_input, expected in test_cases:
    result = translate_book_title(test_input)
    status = "✓" if result == expected else "~"
    if result == expected:
        passed += 1
    print(f"\n{status} Input:    '{test_input}'")
    print(f"   Output:   '{result}'")
    if result != expected:
        print(f"   Expected: '{expected}'")

print(f"\n{'=' * 80}")
print(f"Passed: {passed}/{len(test_cases)}")
print("=" * 80)
