#!/usr/bin/env python3
"""Test all preposition patterns for Korean word order transformation."""

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
        return f"{translate_words(match.group(1).strip())}와 {translate_words(match.group(2).strip())}"
    return text

def translate_phrase(phrase):
    result = phrase

    # Possessive
    m = re.match(r"^(?:The\s+)?(.+?)'s?\s+(.+)$", result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(1).strip())}의 {translate_words(m.group(2).strip())}"

    # Guide to
    m = re.match(r'^(?:A\s+)?Guide\s+to\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(1).strip())} 안내서"

    # Introduction to
    m = re.match(r'^(?:An?\s+)?Introduction\s+to\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(1).strip())} 입문"

    # On X of Y
    m = re.match(r'^On\s+(?:the\s+)?(.+?)\s+of\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())}의 {translate_words(m.group(1).strip())}에 대하여"

    # On X
    m = re.match(r'^On\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(1).strip())}에 대하여"

    # X with Y
    m = re.match(r'^(.+?)\s+with\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())}가 있는 {translate_words(m.group(1).strip())}"

    # X without Y
    m = re.match(r'^(.+?)\s+without\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())} 없는 {translate_words(m.group(1).strip())}"

    # X for Y
    m = re.match(r'^(.+?)\s+for\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())}를 위한 {translate_words(m.group(1).strip())}"

    # X from Y
    m = re.match(r'^(.+?)\s+from\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())}로부터의 {translate_words(m.group(1).strip())}"

    # X by Y
    m = re.match(r'^(.+?)\s+by\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())}의 {translate_words(m.group(1).strip())}"

    # X in Y
    m = re.match(r'^(.+?)\s+in\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())}의 {translate_words(m.group(1).strip())}"

    # X to Y
    m = re.match(r'^(.+?)\s+to\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())}로의 {translate_words(m.group(1).strip())}"

    # X against Y
    m = re.match(r'^(.+?)\s+against\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())}에 대항하는 {translate_words(m.group(1).strip())}"

    # X through Y
    m = re.match(r'^(.+?)\s+through\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())}를 통한 {translate_words(m.group(1).strip())}"

    # X under Y
    m = re.match(r'^(.+?)\s+under\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())} 아래의 {translate_words(m.group(1).strip())}"

    # X beyond Y
    m = re.match(r'^(.+?)\s+beyond\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())} 너머의 {translate_words(m.group(1).strip())}"

    # X among Y
    m = re.match(r'^(.+?)\s+among\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())} 사이의 {translate_words(m.group(1).strip())}"

    # X of Y
    m = re.match(r'^(.+?)\s+of\s+(?:the\s+)?(.+)$', result, re.IGNORECASE)
    if m:
        return f"{translate_words(m.group(2).strip())}의 {translate_words(m.group(1).strip())}"

    # X and Y
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

# Test all patterns
test_cases = [
    # (input, expected_pattern)
    ("Blood and Fear: On the Life Cycle of La", "피와 공포: 라의 생명 주기에 대하여"),
    ("The Murmurs' Prayer", "속삭임의 기도"),
    ("A Guide to Water", "물 안내서"),
    ("Introduction to Fear", "공포 입문"),
    ("On Sight", "시야에 대하여"),
    ("Sword with Blood", "피가 있는 칼"),
    ("Life without Fear", "공포 없는 생명"),
    ("Armor for War", "전쟁를 위한 갑옷"),
    ("Letter from La", "라로부터의 편지"),
    ("Tales by Blood", "피의 이야기"),
    ("Life in Water", "물의 생명"),
    ("Path to Fear", "공포로의 길"),
    ("War against Fear", "공포에 대항하는 전쟁"),
    ("Journey through Water", "물를 통한 여행"),
    ("Life under Water", "물 아래의 생명"),
    ("Land beyond Fear", "공포 너머의 땅"),
    ("Peace among Fear", "공포 사이의 평화"),
    ("Secrets of La", "라의 비밀"),
    ("Blood and Fear", "피와 공포"),
]

print("=" * 80)
print("All Preposition Patterns Test")
print("=" * 80)

passed = 0
for test_input, expected in test_cases:
    result = translate_book_title(test_input)
    match = result == expected
    if match:
        passed += 1
    status = "✓" if match else "~"
    print(f"\n{status} Input:  '{test_input}'")
    print(f"   Output: '{result}'")
    if not match:
        print(f"   Expect: '{expected}'")

print(f"\n{'=' * 80}")
print(f"Passed: {passed}/{len(test_cases)}")
print("=" * 80)
