#!/usr/bin/env python3
"""
Screenshot Items Translation Test
Tests all untranslated items from the user's screenshots.
"""

import json
from pathlib import Path

BASE_PATH = Path(__file__).parent.parent / "LOCALIZATION" / "OBJECTS"
SHARED_PATH = Path(__file__).parent.parent / "LOCALIZATION" / "_SHARED"

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
    count = 0
    for k, v in obj.items():
        if k.startswith("_"):
            continue
        if isinstance(v, dict):
            if "ko" in v:
                target[k.lower()] = v["ko"]
                count += 1
            else:
                count += load_nested(v, target)
        elif isinstance(v, str):
            target[k.lower()] = v
            count += 1
    return count

class V2Simulator:
    def __init__(self):
        self.modifiers = {}
        self.processing = {}
        self.materials = {}
        self.qualities = {}
        self.base_nouns = {}
        self.species = {}
        self.tonics = {}
        self.liquids = {}
        self.body_parts = {}
        self.colors = {}
        self.shaders = {}

    def load_all(self):
        # Load items/_common.json
        common = load_json(BASE_PATH / "items" / "_common.json")
        if common:
            load_section(common.get("materials"), self.materials)
            load_section(common.get("qualities"), self.qualities)
            load_section(common.get("modifiers"), self.modifiers)
            load_section(common.get("processing"), self.processing)
            load_section(common.get("tonics"), self.tonics)
            load_section(common.get("colors"), self.colors)
            load_section(common.get("shaders"), self.shaders)

        # Load creatures/_common.json
        creature_common = load_json(BASE_PATH / "creatures" / "_common.json")
        if creature_common:
            load_section(creature_common.get("species"), self.species)

        # Load items/_nouns.json
        nouns = load_json(BASE_PATH / "items" / "_nouns.json")
        if nouns:
            for section_name, section in nouns.items():
                if section_name.startswith("_"):
                    continue
                if isinstance(section, dict):
                    load_section(section, self.base_nouns)

        # Load _suffixes.json
        suffixes = load_json(BASE_PATH / "_suffixes.json")
        if suffixes:
            load_section(suffixes.get("liquids"), self.liquids)
            load_section(suffixes.get("body_parts"), self.body_parts)

        # Load _vocabulary/modifiers.json
        vocab = load_json(BASE_PATH / "_vocabulary" / "modifiers.json")
        if vocab:
            load_nested(vocab, self.modifiers)

        # Load _vocabulary/processing.json
        proc = load_json(BASE_PATH / "_vocabulary" / "processing.json")
        if proc:
            load_nested(proc, self.processing)

        # Load _SHARED/materials.json
        mat = load_json(SHARED_PATH / "materials.json")
        if mat and "materials" in mat:
            load_nested(mat["materials"], self.materials)

    def find(self, word):
        """Find translation in any dictionary"""
        w = word.lower()
        dicts = [
            ("modifiers", self.modifiers),
            ("processing", self.processing),
            ("materials", self.materials),
            ("qualities", self.qualities),
            ("base_nouns", self.base_nouns),
            ("species", self.species),
            ("tonics", self.tonics),
            ("liquids", self.liquids),
            ("colors", self.colors),
            ("shaders", self.shaders),
        ]
        for name, d in dicts:
            if w in d:
                return (name, d[w])
        return (None, None)

def main():
    print("=" * 70)
    print("Screenshot Items Translation Test")
    print("=" * 70)

    sim = V2Simulator()
    sim.load_all()

    # Items from screenshots that need translation
    # Format: (word, expected_translation or None to just check existence)
    test_items = [
        # Screenshot 1: 기이한 furniture
        ("furniture", "가구"),
        ("기이한", None),  # Already Korean - should be in vocab

        # Screenshot 2: filthy 토가
        ("filthy", "더러운"),
        ("toga", "토가"),

        # Screenshot 3: Books
        # Book titles are proper nouns - usually not translated

        # Screenshot 4: 피켓 전지 (fidget cell)
        ("fidget", None),

        # Screenshot 5: bubble 수준기, brackish 물
        ("bubble", "거품"),
        ("brackish", "기수"),
        ("수준기", None),  # Already Korean

        # Screenshot 6: 말린 라 꽃잎 (dried la petal)
        ("dried", "말린"),
        ("la", "라"),
        ("petal", "꽃잎"),

        # Screenshot 7: canned 만물상
        ("canned", "통조림"),
        ("만물상", None),  # sundries - check if exists

        # Screenshot 8: frill
        ("frill", "프릴"),

        # Screenshot 9: 신선한 water]의 재활용 슈트
        ("water", "물"),
        ("fresh", "신선한"),
        ("recycling", None),
        ("suit", "슈트"),

        # Screenshot 10: 신선한 water]의 bubble 수준기
        # Same as above

        # Screenshot 11: Book titles (proper nouns)

        # Screenshot 12: 피 and Fear: On the Life Cycle of 라
        ("blood", "피"),
        ("fear", None),

        # Screenshot 13: 강철 utility 칼
        ("steel", "강철"),
        ("utility", "다용도"),
        ("knife", "칼"),
    ]

    print("\n=== Individual Word Tests ===")
    passed = 0
    failed = 0
    missing = []

    for word, expected in test_items:
        dict_name, actual = sim.find(word)

        if expected is None:
            # Just check existence
            if actual:
                print(f"  {word}: '{actual}' in {dict_name} [EXISTS]")
                passed += 1
            else:
                print(f"  {word}: NOT FOUND [MISSING]")
                missing.append(word)
        elif actual == expected:
            print(f"  {word}: '{actual}' [PASS]")
            passed += 1
        elif actual:
            print(f"  {word}: '{actual}' (expected '{expected}') [MISMATCH]")
            failed += 1
        else:
            print(f"  {word}: NOT FOUND (expected '{expected}') [MISSING]")
            missing.append(word)
            failed += 1

    # Test compound translations
    print("\n=== Compound Translation Tests ===")
    compounds = [
        ("filthy toga", ["filthy", "toga"], "더러운 토가"),
        ("utility knife", ["utility", "knife"], "다용도 칼"),
        ("steel utility knife", ["steel", "utility", "knife"], "강철 다용도 칼"),
        ("dried la petal", ["dried", "la", "petal"], "말린 라 꽃잎"),
        ("canned sundries", ["canned"], None),  # partial
        ("bubble level", ["bubble"], None),  # partial - level needs translation
        ("brackish water", ["brackish", "water"], "기수 물"),
    ]

    for name, words, expected in compounds:
        translations = []
        all_found = True
        for w in words:
            _, tr = sim.find(w)
            if tr:
                translations.append(tr)
            else:
                translations.append(f"[{w}]")
                all_found = False

        result = " ".join(translations)
        if expected and result == expected:
            print(f"  '{name}' -> '{result}' [PASS]")
        elif all_found:
            print(f"  '{name}' -> '{result}' [OK]")
        else:
            print(f"  '{name}' -> '{result}' [PARTIAL]")

    # Check specific issues from screenshots
    print("\n=== Specific Screenshot Issues ===")

    issues = [
        ("furniture", "기이한 furniture -> 기이한 가구"),
        ("level", "bubble 수준기 - need 'level' translation"),
        ("sundries", "canned 만물상 - need 'sundries' check"),
    ]

    for word, desc in issues:
        _, tr = sim.find(word)
        if tr:
            print(f"  {word}: '{tr}' - {desc} [FOUND]")
        else:
            print(f"  {word}: NOT FOUND - {desc} [MISSING]")
            missing.append(word)

    print("\n" + "=" * 70)
    print(f"RESULTS: {passed} passed, {failed} failed")
    if missing:
        print(f"MISSING: {', '.join(set(missing))}")
    print("=" * 70)

    return 0 if failed == 0 else 1

if __name__ == "__main__":
    exit(main())
