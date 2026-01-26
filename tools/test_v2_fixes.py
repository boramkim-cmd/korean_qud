#!/usr/bin/env python3
"""
V2 Translation System Fix Validation
Tests the fixes for:
1. utility duplicate ("다용도" vs "유틸리티")
2. frill as standalone noun
"""

import json
import os
from pathlib import Path

BASE_PATH = Path(__file__).parent.parent / "LOCALIZATION" / "OBJECTS"

def load_json(path):
    """Load JSON file with error handling."""
    try:
        with open(path, 'r', encoding='utf-8') as f:
            return json.load(f)
    except Exception as e:
        print(f"  Error loading {path}: {e}")
        return None

def test_utility_duplicate():
    """Verify 'utility' only has one translation: 다용도"""
    print("\n=== Test 1: utility duplicate check ===")

    # Check items/_common.json
    common_path = BASE_PATH / "items" / "_common.json"
    common = load_json(common_path)

    common_utility = None
    if common and "modifiers" in common:
        common_utility = common["modifiers"].get("utility")
        print(f"  items/_common.json: utility = '{common_utility}'")

    # Check _vocabulary/modifiers.json
    vocab_path = BASE_PATH / "_vocabulary" / "modifiers.json"
    vocab = load_json(vocab_path)

    vocab_utility = vocab.get("utility") if vocab else None
    print(f"  _vocabulary/modifiers.json: utility = '{vocab_utility}'")

    # Verify fix
    if vocab_utility is None and common_utility == "다용도":
        print("  [PASS] utility duplicate removed. Final value: 다용도")
        return True
    else:
        print(f"  [FAIL] utility still has conflict!")
        return False

def test_frill_in_nouns():
    """Verify 'frill' is in BaseNouns."""
    print("\n=== Test 2: frill in BaseNouns check ===")

    nouns_path = BASE_PATH / "items" / "_nouns.json"
    nouns = load_json(nouns_path)

    if nouns is None:
        print("  [FAIL] Could not load _nouns.json")
        return False

    # Search all sections for frill
    found = False
    for section_name, section in nouns.items():
        if section_name.startswith("_"):
            continue
        if isinstance(section, dict) and "frill" in section:
            print(f"  Found 'frill' in section '{section_name}': {section['frill']}")
            found = True
            break

    if found:
        print("  [PASS] frill is in BaseNouns")
        return True
    else:
        print("  [FAIL] frill NOT found in any section of _nouns.json")
        return False

def test_load_order_simulation():
    """Simulate V2 load order to verify final values."""
    print("\n=== Test 3: Load order simulation ===")

    modifiers = {}  # Case-insensitive simulation

    # Step 1: Load items/_common.json modifiers section
    common_path = BASE_PATH / "items" / "_common.json"
    common = load_json(common_path)
    if common and "modifiers" in common:
        for k, v in common["modifiers"].items():
            if not k.startswith("_"):
                modifiers[k.lower()] = v
        print(f"  After _common.json: {len(modifiers)} modifiers")

    # Step 2: Load _vocabulary/modifiers.json (nested structure)
    vocab_path = BASE_PATH / "_vocabulary" / "modifiers.json"
    vocab = load_json(vocab_path)

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

    if vocab:
        count = load_nested(vocab, modifiers)
        print(f"  After _vocabulary/modifiers.json: +{count} entries, total {len(modifiers)} modifiers")

    # Check final value for 'utility'
    final_utility = modifiers.get("utility")
    print(f"  Final 'utility' value: {final_utility}")

    if final_utility == "다용도":
        print("  [PASS] Load order produces correct 'utility' = 다용도")
        return True
    else:
        print(f"  [FAIL] Load order produces wrong value: {final_utility}")
        return False

def test_screenshot_items():
    """Test all items from the original screenshots."""
    print("\n=== Test 4: Screenshot items check ===")

    # Build merged vocabulary (simulating JsonRepository)
    modifiers = {}
    base_nouns = {}

    # Load _common.json modifiers
    common = load_json(BASE_PATH / "items" / "_common.json")
    if common and "modifiers" in common:
        for k, v in common["modifiers"].items():
            if not k.startswith("_"):
                modifiers[k.lower()] = v

    # Load _vocabulary/modifiers.json
    vocab = load_json(BASE_PATH / "_vocabulary" / "modifiers.json")
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

    if vocab:
        load_nested(vocab, modifiers)

    # Load _nouns.json
    nouns = load_json(BASE_PATH / "items" / "_nouns.json")
    if nouns:
        for section_name, section in nouns.items():
            if section_name.startswith("_"):
                continue
            if isinstance(section, dict):
                for k, v in section.items():
                    if not k.startswith("_"):
                        base_nouns[k.lower()] = v

    # Test items from screenshots
    test_items = [
        ("filthy", "modifiers", "더러운"),
        ("utility", "modifiers", "다용도"),
        ("bubble", "modifiers", "거품"),
        ("frill", "base_nouns", "프릴"),
        ("brackish", "modifiers", "기수"),
        ("dried", "modifiers", None),  # Should be in processing
        ("canned", "modifiers", None),  # Should be in processing
    ]

    passed = 0
    failed = 0

    for word, expected_dict, expected_value in test_items:
        if expected_dict == "modifiers":
            actual = modifiers.get(word.lower())
        else:
            actual = base_nouns.get(word.lower())

        if expected_value is None:
            # Just check existence
            if actual:
                print(f"  {word}: '{actual}' [OK - found]")
                passed += 1
            else:
                print(f"  {word}: NOT FOUND [INFO - may be in other dict]")
        elif actual == expected_value:
            print(f"  {word}: '{actual}' [PASS]")
            passed += 1
        else:
            print(f"  {word}: '{actual}' (expected '{expected_value}') [FAIL]")
            failed += 1

    print(f"\n  Results: {passed} passed, {failed} failed")
    return failed == 0

def main():
    print("=" * 60)
    print("V2 Translation System Fix Validation")
    print("=" * 60)

    results = []
    results.append(("utility duplicate", test_utility_duplicate()))
    results.append(("frill in nouns", test_frill_in_nouns()))
    results.append(("load order", test_load_order_simulation()))
    results.append(("screenshot items", test_screenshot_items()))

    print("\n" + "=" * 60)
    print("SUMMARY")
    print("=" * 60)

    all_passed = True
    for name, passed in results:
        status = "PASS" if passed else "FAIL"
        print(f"  {name}: {status}")
        if not passed:
            all_passed = False

    print("\n" + ("All tests PASSED!" if all_passed else "Some tests FAILED!"))
    return 0 if all_passed else 1

if __name__ == "__main__":
    exit(main())
