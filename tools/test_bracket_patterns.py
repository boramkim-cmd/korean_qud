#!/usr/bin/env python3
"""
Test bracket pattern fixes for ColorTagProcessor.
Tests that nouns/prefixes inside brackets like [fresh water] are now translated.
"""

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

# Load vocabularies
prefixes = {}
base_nouns = {}

common = load_json(BASE_PATH / "items" / "_common.json")
if common:
    load_section(common.get("materials"), prefixes)
    load_section(common.get("qualities"), prefixes)
    load_section(common.get("modifiers"), prefixes)
    load_section(common.get("processing"), prefixes)
    load_section(common.get("tonics"), prefixes)

vocab_mod = load_json(BASE_PATH / "_vocabulary" / "modifiers.json")
if vocab_mod:
    load_nested(vocab_mod, prefixes)

vocab_proc = load_json(BASE_PATH / "_vocabulary" / "processing.json")
if vocab_proc:
    load_nested(vocab_proc, prefixes)

nouns = load_json(BASE_PATH / "items" / "_nouns.json")
if nouns:
    for section in nouns.values():
        if isinstance(section, dict):
            load_section(section, base_nouns)

suffixes = load_json(BASE_PATH / "_suffixes.json")
if suffixes:
    load_section(suffixes.get("liquids"), base_nouns)

# Sort by length (longest first)
prefixes_sorted = sorted(prefixes.items(), key=lambda x: len(x[0]), reverse=True)
base_nouns_sorted = sorted(base_nouns.items(), key=lambda x: len(x[0]), reverse=True)

def translate_nouns_in_text_old(text):
    """OLD pattern - doesn't handle [ bracket"""
    result = text
    for key, value in base_nouns_sorted:
        pattern = rf"(^|\s)({re.escape(key)})($|\s|[,.\[\]()])"
        result = re.sub(pattern, lambda m: m.group(1) + value + m.group(3), result, flags=re.IGNORECASE)
    return result

def translate_nouns_in_text_new(text):
    """NEW pattern - handles [ bracket"""
    result = text
    for key, value in base_nouns_sorted:
        pattern = rf"(^|\s|\[)({re.escape(key)})($|\s|[,.\[\]()])"
        result = re.sub(pattern, lambda m: m.group(1) + value + m.group(3), result, flags=re.IGNORECASE)
    return result

def translate_prefixes_in_text_old(text):
    """OLD pattern - doesn't handle [ ] brackets"""
    result = text
    for key, value in prefixes_sorted:
        pattern = rf"(^|\s)({re.escape(key)})(\s|$)"
        result = re.sub(pattern, lambda m: m.group(1) + value + m.group(3), result, flags=re.IGNORECASE)
    return result

def translate_prefixes_in_text_new(text):
    """NEW pattern - handles [ ] brackets"""
    result = text
    for key, value in prefixes_sorted:
        pattern = rf"(^|\s|\[)({re.escape(key)})(\s|$|\])"
        result = re.sub(pattern, lambda m: m.group(1) + value + m.group(3), result, flags=re.IGNORECASE)
    return result

def full_translate_old(text):
    """Simulate old translation flow"""
    with_nouns = translate_nouns_in_text_old(text)
    return translate_prefixes_in_text_old(with_nouns)

def full_translate_new(text):
    """Simulate new translation flow"""
    with_nouns = translate_nouns_in_text_new(text)
    return translate_prefixes_in_text_new(with_nouns)

# Test cases
test_cases = [
    # Format: (input, expected_new_output)
    ("[fresh water]", "[신선한 물]"),
    ("[water]", "[물]"),
    ("[dried meat]", "[말린 고기]"),
    ("[brackish water]", "[소금기 있는 물]"),
    ("fresh water", "신선한 물"),
    ("water", "물"),
    ("dried la petal", "말린 la petal"),  # la petal not in nouns
    ("[canned sundries]", "[통조림 잡화]"),
]

print("=" * 70)
print("Bracket Pattern Fix Test")
print("=" * 70)

all_pass = True
for test_input, expected in test_cases:
    old_result = full_translate_old(test_input)
    new_result = full_translate_new(test_input)

    passed = new_result == expected
    if not passed:
        all_pass = False

    status = "✓" if passed else "✗"
    print(f"\n{status} Input: '{test_input}'")
    print(f"   OLD: '{old_result}'")
    print(f"   NEW: '{new_result}'")
    print(f"   Expected: '{expected}'")
    if old_result != new_result:
        print(f"   → Pattern fix made a difference!")

print("\n" + "=" * 70)
if all_pass:
    print("All tests PASSED!")
else:
    print("Some tests FAILED - check expected values")
print("=" * 70)

# Additional verification: check that specific words are in dictionaries
print("\n[Dictionary Verification]")
check_words = ["fresh", "water", "dried", "brackish", "canned", "sundries", "meat"]
for word in check_words:
    in_prefixes = word.lower() in prefixes
    in_nouns = word.lower() in base_nouns
    prefix_val = prefixes.get(word.lower(), "-")
    noun_val = base_nouns.get(word.lower(), "-")
    print(f"  {word}: prefix={prefix_val}, noun={noun_val}")
