#!/usr/bin/env python3
"""Test book title translation patterns."""

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

prefixes_sorted = sorted(prefixes.items(), key=lambda x: len(x[0]), reverse=True)
base_nouns_sorted = sorted(base_nouns.items(), key=lambda x: len(x[0]), reverse=True)

def translate_nouns_in_text(text):
    result = text
    for key, value in base_nouns_sorted:
        pattern = rf'(^|\s|\[|")({re.escape(key)})($|\s|[,.\[\]():\'\"!?])'
        result = re.sub(pattern, lambda m: m.group(1) + value + m.group(3), result, flags=re.IGNORECASE)
    return result

def translate_prefixes_in_text(text):
    result = text
    for key, value in prefixes_sorted:
        pattern = rf'(^|\s|\[|")({re.escape(key)})(\s|$|\]|[:\'\"!?])'
        result = re.sub(pattern, lambda m: m.group(1) + value + m.group(3), result, flags=re.IGNORECASE)
    return result

def full_translate(text):
    with_nouns = translate_nouns_in_text(text)
    return translate_prefixes_in_text(with_nouns)

test_cases = [
    ('Fear: On the Life Cycle of La', "Colon after word"),
    ('Blood and Fear', "Word followed by and"),
    ("The Murmurs' Prayer", "Possessive pattern"),
    ('Life Cycle', "Compound words"),
    ('On Sight', "Simple phrase"),
    ('Fear!', "Word with exclamation"),
    ('"Fear"', "Word in quotes"),
    ('[fresh water]', "Bracket pattern"),
]

print("=" * 70)
print("Book Title Translation Test (Updated Patterns)")
print("=" * 70)

for test_input, desc in test_cases:
    result = full_translate(test_input)
    changed = result != test_input
    status = "✓" if changed else "~"
    print(f"\n{status} [{desc}]")
    print(f"   Input:  '{test_input}'")
    print(f"   Output: '{result}'")

print("\n" + "=" * 70)
print("Full Book Title Simulation")
print("=" * 70)
actual_title = "Blood and Fear: On the Life Cycle of La"
result = full_translate(actual_title)
print(f"Input:  '{actual_title}'")
print(f"Output: '{result}'")
