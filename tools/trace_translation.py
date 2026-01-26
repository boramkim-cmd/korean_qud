#!/usr/bin/env python3
"""
Translation Flow Trace
Simulates the V2 translation pipeline for specific items.
"""

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

# Load all vocabularies
prefixes = {}  # materials + qualities + modifiers + processing + tonics + etc
base_nouns = {}
processing = {}

common = load_json(BASE_PATH / "items" / "_common.json")
if common:
    load_section(common.get("materials"), prefixes)
    load_section(common.get("qualities"), prefixes)
    load_section(common.get("modifiers"), prefixes)
    load_section(common.get("processing"), prefixes)
    load_section(common.get("processing"), processing)
    load_section(common.get("tonics"), prefixes)

vocab_mod = load_json(BASE_PATH / "_vocabulary" / "modifiers.json")
if vocab_mod:
    load_nested(vocab_mod, prefixes)

vocab_proc = load_json(BASE_PATH / "_vocabulary" / "processing.json")
if vocab_proc:
    load_nested(vocab_proc, prefixes)
    load_nested(vocab_proc, processing)

nouns = load_json(BASE_PATH / "items" / "_nouns.json")
if nouns:
    for section in nouns.values():
        if isinstance(section, dict):
            load_section(section, base_nouns)

# Sort by length (longest first) for greedy matching
prefixes_sorted = sorted(prefixes.items(), key=lambda x: len(x[0]), reverse=True)
base_nouns_sorted = sorted(base_nouns.items(), key=lambda x: len(x[0]), reverse=True)

def extract_prefix(name):
    """Extract first prefix from name."""
    name_lower = name.lower()
    for key, value in prefixes_sorted:
        if name_lower.startswith(key + " "):
            return (value, name[len(key)+1:])
    return (None, name)

def find_base_noun(name):
    """Find if name is a known base noun."""
    name_lower = name.lower()
    if name_lower in base_nouns:
        return base_nouns[name_lower]
    return None

def trace_translation(name):
    """Trace through the translation pipeline."""
    print(f"\n{'='*60}")
    print(f"Input: '{name}'")
    print(f"{'='*60}")

    current = name
    translated_parts = []

    # Step 1: Extract prefixes iteratively
    print("\n[Step 1: Prefix Extraction]")
    while True:
        prefix_ko, remainder = extract_prefix(current)
        if prefix_ko:
            print(f"  Found prefix: '{current.split()[0]}' -> '{prefix_ko}'")
            print(f"  Remainder: '{remainder}'")
            translated_parts.append(prefix_ko)
            current = remainder
        else:
            print(f"  No more prefixes in: '{current}'")
            break

    # Step 2: Try to find base noun
    print("\n[Step 2: Base Noun Lookup]")
    base_ko = find_base_noun(current)
    if base_ko:
        print(f"  Found base noun: '{current}' -> '{base_ko}'")
        translated_parts.append(base_ko)
    else:
        # Try compound: check if last word is base noun
        words = current.split()
        if len(words) > 1:
            print(f"  Not a single base noun, trying compound...")
            last_word = words[-1]
            last_ko = find_base_noun(last_word)
            if last_ko:
                print(f"  Found trailing base noun: '{last_word}' -> '{last_ko}'")
                # Remaining words might be creature names or modifiers
                remaining = " ".join(words[:-1])
                print(f"  Remaining: '{remaining}'")
                # Try prefix on remaining
                prefix_ko2, rem2 = extract_prefix(remaining)
                if prefix_ko2:
                    print(f"  Found additional prefix: '{remaining.split()[0]}' -> '{prefix_ko2}'")
                    translated_parts.append(prefix_ko2)
                    if rem2:
                        # rem2 might be a creature name or just kept as-is
                        translated_parts.append(rem2)
                else:
                    translated_parts.append(remaining)
                translated_parts.append(last_ko)
            else:
                print(f"  Could not find base noun for: '{current}'")
                translated_parts.append(current)
        else:
            print(f"  Could not find base noun for: '{current}'")
            translated_parts.append(current)

    # Step 3: Build result
    print("\n[Step 3: Build Result]")
    result = " ".join(translated_parts)
    print(f"  Result: '{result}'")

    return result

# Test problematic items
test_items = [
    "dried la petal",
    "canned sundries",
    "filthy toga",
    "utility knife",
    "bubble level",
    "brackish water",
    "steel utility knife",
    "기이한 furniture",
    "frill",
]

print("=" * 70)
print("Translation Pipeline Trace")
print("=" * 70)

for item in test_items:
    trace_translation(item)

print("\n" + "=" * 70)
print("Processing Dictionary Contents:")
print("=" * 70)
for k, v in sorted(processing.items()):
    print(f"  {k}: {v}")
