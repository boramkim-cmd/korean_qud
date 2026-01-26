#!/usr/bin/env python3
"""
Comprehensive V2 Translation System Test
Simulates the full V2 loading and tests all screenshot items.
"""

import json
import os
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
    """Load nested vocabulary (supports flat and { "ko": "...", "aliases": [...] } formats)"""
    count = 0
    for k, v in obj.items():
        if k.startswith("_"):
            continue
        if isinstance(v, dict):
            if "ko" in v:
                target[k.lower()] = v["ko"]
                count += 1
                if "aliases" in v and isinstance(v["aliases"], list):
                    for alias in v["aliases"]:
                        target[alias.lower()] = v["ko"]
                        count += 1
            else:
                count += load_nested(v, target)
        elif isinstance(v, str):
            target[k.lower()] = v
            count += 1
    return count

class V2RepositorySimulator:
    """Simulates JsonRepository loading logic."""

    def __init__(self):
        self.materials = {}
        self.qualities = {}
        self.modifiers = {}
        self.processing = {}
        self.tonics = {}
        self.grenades = {}
        self.marks = {}
        self.colors = {}
        self.shaders = {}
        self.species = {}
        self.base_nouns = {}
        self.states = {}
        self.liquids = {}
        self.of_patterns = {}
        self.body_parts = {}

    def load_item_common(self):
        """Load items/_common.json"""
        common = load_json(BASE_PATH / "items" / "_common.json")
        if common:
            load_section(common.get("materials"), self.materials)
            load_section(common.get("qualities"), self.qualities)
            load_section(common.get("modifiers"), self.modifiers)
            load_section(common.get("processing"), self.processing)
            load_section(common.get("tonics"), self.tonics)
            load_section(common.get("grenades"), self.grenades)
            load_section(common.get("marks"), self.marks)
            load_section(common.get("colors"), self.colors)
            load_section(common.get("shaders"), self.shaders)

    def load_creature_common(self):
        """Load creatures/_common.json"""
        common = load_json(BASE_PATH / "creatures" / "_common.json")
        if common:
            load_section(common.get("species"), self.species)

    def load_item_nouns(self):
        """Load items/_nouns.json"""
        nouns = load_json(BASE_PATH / "items" / "_nouns.json")
        if nouns:
            for section_name, section in nouns.items():
                if section_name.startswith("_"):
                    continue
                if isinstance(section, dict):
                    load_section(section, self.base_nouns)

    def load_suffixes(self):
        """Load _suffixes.json"""
        suffixes = load_json(BASE_PATH / "_suffixes.json")
        if suffixes:
            load_section(suffixes.get("states"), self.states)
            load_section(suffixes.get("liquids"), self.liquids)
            load_section(suffixes.get("of_patterns"), self.of_patterns)
            load_section(suffixes.get("body_parts"), self.body_parts)

    def load_vocabulary(self):
        """Load _vocabulary/*.json"""
        vocab_path = BASE_PATH / "_vocabulary"
        if vocab_path.exists():
            # modifiers.json
            vocab = load_json(vocab_path / "modifiers.json")
            if vocab:
                load_nested(vocab, self.modifiers)

            # processing.json
            proc = load_json(vocab_path / "processing.json")
            if proc:
                load_nested(proc, self.processing)

    def load_shared(self):
        """Load _SHARED/*.json"""
        if SHARED_PATH.exists():
            # materials.json
            mat = load_json(SHARED_PATH / "materials.json")
            if mat and "materials" in mat:
                load_nested(mat["materials"], self.materials)

            # qualities.json
            qual = load_json(SHARED_PATH / "qualities.json")
            if qual and "qualities" in qual:
                load_nested(qual["qualities"], self.qualities)

            # body_parts.json
            bp = load_json(SHARED_PATH / "body_parts.json")
            if bp and "body_parts" in bp:
                load_nested(bp["body_parts"], self.body_parts)

    def load_all(self):
        """Execute full load sequence (same order as JsonRepository)"""
        self.load_item_common()
        self.load_creature_common()
        self.load_item_nouns()
        self.load_suffixes()
        self.load_vocabulary()
        self.load_shared()

    def get_stats(self):
        return {
            "materials": len(self.materials),
            "qualities": len(self.qualities),
            "modifiers": len(self.modifiers),
            "processing": len(self.processing),
            "tonics": len(self.tonics),
            "species": len(self.species),
            "base_nouns": len(self.base_nouns),
            "liquids": len(self.liquids),
            "body_parts": len(self.body_parts),
        }

    def find_translation(self, word):
        """Find translation in any dictionary, return (dict_name, translation)"""
        word_lower = word.lower()

        checks = [
            ("modifiers", self.modifiers),
            ("processing", self.processing),
            ("materials", self.materials),
            ("qualities", self.qualities),
            ("base_nouns", self.base_nouns),
            ("species", self.species),
            ("tonics", self.tonics),
            ("liquids", self.liquids),
            ("body_parts", self.body_parts),
        ]

        for name, dict_ in checks:
            if word_lower in dict_:
                return (name, dict_[word_lower])

        return (None, None)


def main():
    print("=" * 70)
    print("Comprehensive V2 Translation System Test")
    print("=" * 70)

    repo = V2RepositorySimulator()
    repo.load_all()

    print("\n=== Dictionary Statistics ===")
    stats = repo.get_stats()
    for name, count in stats.items():
        print(f"  {name}: {count}")

    # Test all screenshot items from the original bug report
    print("\n=== Screenshot Items Test ===")
    screenshot_items = [
        ("filthy", "더러운"),          # filthy 토가
        ("utility", "다용도"),         # utility 칼
        ("bubble", "거품"),            # bubble 수준기
        ("frill", "프릴"),             # frill (standalone)
        ("brackish", "기수"),          # brackish 물
        ("dried", "말린"),             # dried 라 꽃잎
        ("canned", "통조림"),          # canned 만물상
        ("la", "라"),                  # la (flower name part)
        ("knife", "칼"),               # utility knife
        ("toga", "토가"),              # filthy toga
        ("water", "물"),               # brackish water
    ]

    passed = 0
    failed = 0

    for word, expected in screenshot_items:
        dict_name, actual = repo.find_translation(word)
        if actual == expected:
            print(f"  {word}: '{actual}' in {dict_name} [PASS]")
            passed += 1
        elif actual:
            print(f"  {word}: '{actual}' (expected '{expected}') in {dict_name} [MISMATCH]")
            failed += 1
        else:
            print(f"  {word}: NOT FOUND (expected '{expected}') [MISSING]")
            failed += 1

    # Test compound translation simulation
    print("\n=== Compound Translation Simulation ===")

    # Simulate "utility knife" -> "다용도 칼"
    utility_tr = repo.modifiers.get("utility")
    knife_tr = repo.base_nouns.get("knife")
    if utility_tr and knife_tr:
        compound = f"{utility_tr} {knife_tr}"
        print(f"  'utility knife' -> '{compound}' [{'PASS' if compound == '다용도 칼' else 'FAIL'}]")
    else:
        print(f"  'utility knife' -> PARTIAL (utility={utility_tr}, knife={knife_tr})")

    # Simulate "filthy toga"
    filthy_tr = repo.modifiers.get("filthy")
    toga_tr = repo.base_nouns.get("toga")
    if filthy_tr:
        print(f"  'filthy toga' -> filthy={filthy_tr}, toga={toga_tr or 'MISSING'}")
    else:
        print(f"  'filthy toga' -> MISSING (filthy not found)")

    # Simulate "dried la petal" (processing + noun)
    dried_tr = repo.processing.get("dried")
    la_tr = repo.modifiers.get("la")
    petal_tr = repo.base_nouns.get("petal")
    print(f"  'dried la petal' -> dried={dried_tr}, la={la_tr}, petal={petal_tr}")

    print("\n" + "=" * 70)
    print(f"RESULTS: {passed}/{passed+failed} items found with correct translations")
    print("=" * 70)

    # Missing items that need to be added
    missing = []
    for word, expected in screenshot_items:
        _, actual = repo.find_translation(word)
        if actual is None:
            missing.append(word)

    if missing:
        print(f"\nMissing translations: {', '.join(missing)}")
        print("These need to be added to the appropriate JSON files.")

    return 0 if failed == 0 else 1

if __name__ == "__main__":
    exit(main())
