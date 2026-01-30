#!/usr/bin/env python3
"""
generate_missing_translations.py

XML ObjectBlueprints에서 모든 구체 블루프린트의 DisplayName을 추출하고,
기존 LOCALIZATION JSON과 비교하여 누락된 번역을 자동 생성합니다.

Phase 1: XML → LOCALIZATION 자동 생성
Phase 3: Base* → 구체 블루프린트 상속 매핑

Usage:
    python3 tools/generate_missing_translations.py           # 자동 생성 실행
    python3 tools/generate_missing_translations.py --stats   # 통계만 출력
    python3 tools/generate_missing_translations.py --dry-run # 파일 쓰지 않고 미리보기
"""

import json
import os
import re
import sys
from collections import defaultdict
from pathlib import Path

PROJECT_ROOT = Path(__file__).parent.parent
XML_DIR = PROJECT_ROOT / "Assets" / "StreamingAssets" / "Base" / "ObjectBlueprints"
LOCALIZATION_DIR = PROJECT_ROOT / "LOCALIZATION"
OBJECTS_DIR = LOCALIZATION_DIR / "OBJECTS"

# Output files
ITEMS_AUTO = OBJECTS_DIR / "items" / "auto_generated.json"
CREATURES_AUTO = OBJECTS_DIR / "creatures" / "auto_generated.json"


# ============================================================
# XML Parsing (handles invalid character references)
# ============================================================

def parse_xml_file(filepath):
    """Parse XML file, handling invalid character references."""
    import xml.etree.ElementTree as ET

    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Replace invalid XML character references
    content = re.sub(r'&#x[0-9a-fA-F]+;', '?', content)
    content = re.sub(r'&#\d+;', '?', content)

    try:
        return ET.fromstring(content)
    except ET.ParseError as e:
        print(f"  WARNING: Failed to parse {filepath.name}: {e}")
        return None


def extract_blueprints_from_xml():
    """Extract all concrete blueprints with their DisplayName and inheritance."""
    blueprints = {}  # Name -> {display_name, inherits, file}

    for xml_file in sorted(XML_DIR.glob("*.xml")):
        root = parse_xml_file(xml_file)
        if root is None:
            continue

        for obj in root.iter("object"):
            name = obj.get("Name")
            if not name:
                continue

            inherits = obj.get("Inherits", "")
            display_name = ""

            # Look for DisplayName in <part> elements
            for part in obj.findall("part"):
                dn = part.get("DisplayName")
                if dn:
                    display_name = dn
                    break

            # Also check <property> elements
            if not display_name:
                for prop in obj.findall("property"):
                    if prop.get("Name") == "DisplayName":
                        display_name = prop.get("Value", "")
                        break

            blueprints[name] = {
                "display_name": display_name,
                "inherits": inherits,
                "file": xml_file.name,
            }

    return blueprints


# ============================================================
# Load Existing Translations
# ============================================================

def load_existing_translations():
    """Load all existing translation data from LOCALIZATION/OBJECTS."""
    existing = {}  # blueprint_id (normalized) -> {names: {en: ko}}

    # Load creature files
    creatures_dir = OBJECTS_DIR / "creatures"
    if creatures_dir.exists():
        for json_file in creatures_dir.rglob("*.json"):
            if json_file.name.startswith("_"):
                continue
            _load_json_blueprints(json_file, existing)

    # Load item files
    items_dir = OBJECTS_DIR / "items"
    if items_dir.exists():
        for json_file in items_dir.rglob("*.json"):
            if json_file.name.startswith("_"):
                continue
            _load_json_blueprints(json_file, existing)

    # Load furniture files
    furniture_dir = OBJECTS_DIR / "furniture"
    if furniture_dir.exists():
        for json_file in furniture_dir.rglob("*.json"):
            if json_file.name.startswith("_"):
                continue
            _load_json_blueprints(json_file, existing)

    # Load terrain files
    terrain_dir = OBJECTS_DIR / "terrain"
    if terrain_dir.exists():
        for json_file in terrain_dir.rglob("*.json"):
            if json_file.name.startswith("_"):
                continue
            _load_json_blueprints(json_file, existing)

    # Load standalone files
    for standalone in ["hidden.json", "widgets.json"]:
        path = OBJECTS_DIR / standalone
        if path.exists():
            _load_json_blueprints(path, existing)

    # Load _compound_translations.json and _manual_translations.json
    compound_path = OBJECTS_DIR / "_compound_translations.json"
    if compound_path.exists():
        _load_flat_translations(compound_path, existing)

    manual_path = OBJECTS_DIR / "_manual_translations.json"
    if manual_path.exists():
        _load_manual_translations(manual_path, existing)

    return existing


def _load_json_blueprints(filepath, target):
    """Load blueprint entries from a JSON file."""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            data = json.load(f)

        for key, value in data.items():
            if key.startswith("_"):
                continue
            if isinstance(value, dict) and "names" in value:
                names = value["names"]
                if isinstance(names, dict) and any(v for v in names.values() if v):
                    target[key.lower()] = names
    except (json.JSONDecodeError, Exception) as e:
        print(f"  WARNING: Failed to load {filepath}: {e}")


def _load_flat_translations(filepath, target):
    """Load flat section-based translations (compound_translations format)."""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            data = json.load(f)

        for section_key, section_val in data.items():
            if section_key.startswith("_"):
                continue
            if isinstance(section_val, dict):
                for en_name, ko_name in section_val.items():
                    if en_name.startswith("_"):
                        continue
                    if isinstance(ko_name, str) and ko_name:
                        # Store as a name mapping
                        # Use the english name as a pseudo-blueprint key
                        target[en_name.lower()] = {en_name: ko_name}
    except (json.JSONDecodeError, Exception) as e:
        print(f"  WARNING: Failed to load {filepath}: {e}")


def _load_manual_translations(filepath, target):
    """Load manual translations."""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            data = json.load(f)

        translations = data.get("translations", {})
        for en_name, ko_name in translations.items():
            if en_name.startswith("_"):
                continue
            if isinstance(ko_name, str) and ko_name:
                target[en_name.lower()] = {en_name: ko_name}
    except (json.JSONDecodeError, Exception) as e:
        print(f"  WARNING: Failed to load {filepath}: {e}")


# ============================================================
# Vocabulary Loading
# ============================================================

def load_vocabulary():
    """Load all vocabulary dictionaries for compound translation."""
    vocab = {}

    # Materials from _common.json
    common_path = OBJECTS_DIR / "items" / "_common.json"
    if common_path.exists():
        data = json.load(open(common_path, 'r', encoding='utf-8'))
        for section in ["materials", "qualities", "tonics", "grenades",
                        "colors", "shaders", "marks"]:
            if section in data and isinstance(data[section], dict):
                for k, v in data[section].items():
                    if not k.startswith("_") and isinstance(v, str):
                        vocab[k.lower()] = v

    # Nouns
    nouns_path = OBJECTS_DIR / "items" / "_nouns.json"
    if nouns_path.exists():
        data = json.load(open(nouns_path, 'r', encoding='utf-8'))
        for section_key, section_val in data.items():
            if section_key.startswith("_"):
                continue
            if isinstance(section_val, dict):
                for k, v in section_val.items():
                    if not k.startswith("_") and isinstance(v, str):
                        vocab[k.lower()] = v

    # Modifiers
    modifiers_path = OBJECTS_DIR / "_vocabulary" / "modifiers.json"
    if modifiers_path.exists():
        data = json.load(open(modifiers_path, 'r', encoding='utf-8'))
        _load_nested_vocab(data, vocab)

    # Processing
    processing_path = OBJECTS_DIR / "_vocabulary" / "processing.json"
    if processing_path.exists():
        data = json.load(open(processing_path, 'r', encoding='utf-8'))
        _load_nested_vocab(data, vocab)

    # Species
    creatures_common = OBJECTS_DIR / "creatures" / "_common.json"
    if creatures_common.exists():
        data = json.load(open(creatures_common, 'r', encoding='utf-8'))
        for section_key, section_val in data.items():
            if section_key.startswith("_"):
                continue
            if isinstance(section_val, dict):
                for k, v in section_val.items():
                    if not k.startswith("_") and isinstance(v, str):
                        vocab[k.lower()] = v

    # Shared materials
    shared_dir = LOCALIZATION_DIR / "_SHARED"
    if shared_dir.exists():
        for json_file in shared_dir.glob("*.json"):
            data = json.load(open(json_file, 'r', encoding='utf-8'))
            _load_nested_vocab(data, vocab)

    # Body parts from suffixes
    suffixes_path = OBJECTS_DIR / "_suffixes.json"
    if suffixes_path.exists():
        data = json.load(open(suffixes_path, 'r', encoding='utf-8'))
        for section in ["body_parts", "liquids"]:
            if section in data and isinstance(data[section], dict):
                for k, v in data[section].items():
                    if not k.startswith("_") and isinstance(v, str):
                        vocab[k.lower()] = v

    return vocab


def _load_nested_vocab(data, vocab):
    """Recursively load nested vocabulary."""
    if isinstance(data, dict):
        for k, v in data.items():
            if k.startswith("_"):
                continue
            if isinstance(v, str):
                vocab[k.lower()] = v
            elif isinstance(v, dict):
                _load_nested_vocab(v, vocab)


# ============================================================
# Compound Translation Engine
# ============================================================

def strip_color_tags(text):
    """Remove {{X|...}} color tags, returning inner text."""
    return re.sub(r'\{\{[A-Za-z]\|([^}]*)\}\}', r'\1', text)


def try_compound_translate(display_name, vocab):
    """
    Try to translate a compound name using vocabulary.
    "iron dagger" -> "철 단검"
    "chrome revolver" -> "크롬 리볼버"
    Returns Korean string or None if can't translate.
    """
    if not display_name:
        return None

    # Strip color tags for matching
    clean = strip_color_tags(display_name).strip()
    if not clean:
        return None

    # Don't try to translate names that look like proper nouns (capitalized multi-word)
    # But DO translate if all words are in vocab
    words = clean.lower().split()
    if not words:
        return None

    # Try to match words from vocab
    # Strategy: try longest prefix match, then remaining
    translated_parts = []
    i = 0
    while i < len(words):
        matched = False
        # Try longest match first (up to 4 words)
        for length in range(min(4, len(words) - i), 0, -1):
            phrase = " ".join(words[i:i+length])
            if phrase in vocab:
                translated_parts.append(vocab[phrase])
                i += length
                matched = True
                break

        if not matched:
            # Can't translate this word - give up on compound translation
            return None

    if translated_parts:
        return " ".join(translated_parts)

    return None


# ============================================================
# Inheritance Resolution
# ============================================================

def resolve_inheritance(blueprints, existing_translations, vocab):
    """
    Resolve translations through inheritance chain.
    If BaseTorch has a translation and Torch inherits from it,
    try to use the same translation.
    """
    inherited = {}

    for bp_name, bp_data in blueprints.items():
        bp_key = bp_name.lower()
        if bp_key in existing_translations:
            continue  # Already has translation

        # Walk inheritance chain
        current = bp_name
        chain = []
        visited = set()
        while current and current not in visited:
            visited.add(current)
            chain.append(current)
            parent_name = blueprints.get(current, {}).get("inherits", "")
            current = parent_name if parent_name else None

        # Check if any parent has a translation
        for parent in chain[1:]:  # Skip self
            parent_key = parent.lower()
            if parent_key in existing_translations:
                parent_names = existing_translations[parent_key]
                # Use parent's DisplayName translation for this blueprint
                display_name = bp_data.get("display_name", "")
                if display_name:
                    clean_dn = strip_color_tags(display_name).strip().lower()
                    # Check if parent has this exact display name translated
                    for en, ko in parent_names.items():
                        if en.lower() == clean_dn and ko:
                            inherited[bp_name] = {display_name: ko}
                            break
                break

    return inherited


# ============================================================
# Main Generation Logic
# ============================================================

def categorize_blueprint(bp_name, bp_data, all_blueprints):
    """Determine if a blueprint is a creature or item based on inheritance chain."""
    current = bp_name
    visited = set()
    while current and current not in visited:
        visited.add(current)
        if current in ("Creature", "BaseMerchant", "BaseHumanoid"):
            return "creature"
        if current in ("Item", "MeleeWeapon", "MissileWeapon", "Armor", "Shield",
                       "Food", "Tonic", "Grenade", "Tool", "Furniture",
                       "Wall", "LightSource", "Container", "Door",
                       "Widget", "PhysicalObject", "InertObject",
                       "BaseBow", "BasePistol", "BaseRifle"):
            return "item"
        parent = all_blueprints.get(current, {}).get("inherits", "")
        current = parent if parent else None

    # Default to item if can't determine
    return "item"


def generate_translations(dry_run=False, stats_only=False):
    """Main entry point: generate missing translations."""
    print("=" * 60)
    print("번역 커버리지 확대 도구")
    print("=" * 60)

    # Step 1: Extract all blueprints from XML
    print("\n[1/5] XML 블루프린트 추출 중...")
    blueprints = extract_blueprints_from_xml()
    print(f"  총 블루프린트: {len(blueprints)}개")

    # Filter to concrete blueprints (ones with DisplayName)
    concrete = {k: v for k, v in blueprints.items() if v["display_name"]}
    abstract = {k: v for k, v in blueprints.items() if not v["display_name"]}
    print(f"  구체 블루프린트 (DisplayName 있음): {len(concrete)}개")
    print(f"  추상 블루프린트 (DisplayName 없음): {len(abstract)}개")

    # Step 2: Load existing translations
    print("\n[2/5] 기존 번역 데이터 로드 중...")
    existing = load_existing_translations()
    print(f"  기존 번역: {len(existing)}개")

    # Step 3: Find missing
    print("\n[3/5] 누락 분석 중...")
    missing = {}
    for bp_name, bp_data in concrete.items():
        bp_key = bp_name.lower()
        if bp_key not in existing:
            # Also check if display_name is translated (name-based lookup)
            dn = strip_color_tags(bp_data["display_name"]).strip().lower()
            if dn not in existing:
                missing[bp_name] = bp_data

    print(f"  누락 블루프린트: {len(missing)}개")
    print(f"  번역 커버리지: {len(concrete) - len(missing)}/{len(concrete)} "
          f"({(len(concrete) - len(missing)) / max(len(concrete), 1) * 100:.1f}%)")

    if stats_only:
        _print_stats(missing, concrete, blueprints)
        return

    # Step 4: Load vocabulary and attempt auto-translation
    print("\n[4/5] 어휘 사전으로 자동 번역 시도 중...")
    vocab = load_vocabulary()
    print(f"  어휘 사전 크기: {len(vocab)}개")

    auto_translated = {}
    untranslatable = {}

    for bp_name, bp_data in missing.items():
        display_name = bp_data["display_name"]
        clean_dn = strip_color_tags(display_name).strip()

        ko = try_compound_translate(clean_dn, vocab)
        if ko:
            auto_translated[bp_name] = {
                "display_name": display_name,
                "ko": ko,
            }
        else:
            untranslatable[bp_name] = bp_data

    # Also try inheritance resolution
    inherited = resolve_inheritance(blueprints, existing, vocab)
    for bp_name, names in inherited.items():
        if bp_name in untranslatable:
            for en, ko in names.items():
                auto_translated[bp_name] = {
                    "display_name": en,
                    "ko": ko,
                }
                del untranslatable[bp_name]
                break

    print(f"  자동 번역 성공: {len(auto_translated)}개")
    print(f"  자동 번역 불가: {len(untranslatable)}개")

    # Step 5: Generate output files
    print("\n[5/5] 번역 파일 생성 중...")

    # Categorize auto-translated into creatures vs items
    creature_entries = {}
    item_entries = {}

    for bp_name, data in auto_translated.items():
        display_name = data["display_name"]
        ko = data["ko"]
        category = categorize_blueprint(bp_name, blueprints.get(bp_name, {}), blueprints)

        entry = {
            "names": {
                strip_color_tags(display_name).strip(): ko
            }
        }

        if category == "creature":
            creature_entries[bp_name] = entry
        else:
            item_entries[bp_name] = entry

    # Also add untranslatable with empty translations for manual filling
    for bp_name, bp_data in untranslatable.items():
        display_name = bp_data["display_name"]
        clean_dn = strip_color_tags(display_name).strip()
        category = categorize_blueprint(bp_name, blueprints.get(bp_name, {}), blueprints)

        entry = {
            "names": {
                clean_dn: ""
            }
        }

        if category == "creature":
            creature_entries[bp_name] = entry
        else:
            item_entries[bp_name] = entry

    if dry_run:
        print(f"\n  [DRY RUN] 아이템: {len(item_entries)}개, 크리처: {len(creature_entries)}개")
        print(f"  자동 번역 샘플 (처음 20개):")
        for i, (bp, data) in enumerate(auto_translated.items()):
            if i >= 20:
                break
            print(f"    {data['display_name']} → {data['ko']}")
        print(f"\n  미번역 샘플 (처음 20개):")
        for i, (bp, data) in enumerate(untranslatable.items()):
            if i >= 20:
                break
            print(f"    {bp}: {data['display_name']}")
        return

    # Write items auto-generated
    if item_entries:
        items_output = {
            "_meta": {
                "description": "Auto-generated item translations from XML blueprints",
                "description_ko": "XML 블루프린트에서 자동 생성된 아이템 번역",
                "version": "1.0",
                "created": "2026-01-30",
                "source": "tools/generate_missing_translations.py",
                "note": "Empty values need manual translation"
            }
        }
        # Sort by blueprint name
        for bp_name in sorted(item_entries.keys()):
            items_output[bp_name] = item_entries[bp_name]

        ITEMS_AUTO.parent.mkdir(parents=True, exist_ok=True)
        with open(ITEMS_AUTO, 'w', encoding='utf-8') as f:
            json.dump(items_output, f, ensure_ascii=False, indent=2)
        print(f"  아이템 번역 파일: {ITEMS_AUTO.relative_to(PROJECT_ROOT)} ({len(item_entries)}개)")

    # Write creatures auto-generated
    if creature_entries:
        creatures_output = {
            "_meta": {
                "description": "Auto-generated creature translations from XML blueprints",
                "description_ko": "XML 블루프린트에서 자동 생성된 크리처 번역",
                "version": "1.0",
                "created": "2026-01-30",
                "source": "tools/generate_missing_translations.py",
                "note": "Empty values need manual translation"
            }
        }
        for bp_name in sorted(creature_entries.keys()):
            creatures_output[bp_name] = creature_entries[bp_name]

        CREATURES_AUTO.parent.mkdir(parents=True, exist_ok=True)
        with open(CREATURES_AUTO, 'w', encoding='utf-8') as f:
            json.dump(creatures_output, f, ensure_ascii=False, indent=2)
        print(f"  크리처 번역 파일: {CREATURES_AUTO.relative_to(PROJECT_ROOT)} ({len(creature_entries)}개)")

    # Summary
    total_auto = len(auto_translated)
    total_empty = len(untranslatable)
    total_existing = len(concrete) - len(missing)
    total_concrete = len(concrete)

    print(f"\n{'=' * 60}")
    print(f"결과 요약")
    print(f"{'=' * 60}")
    print(f"  총 구체 블루프린트:   {total_concrete}")
    print(f"  기존 번역:           {total_existing} ({total_existing/max(total_concrete,1)*100:.1f}%)")
    print(f"  자동 번역 (신규):    {total_auto} ({total_auto/max(total_concrete,1)*100:.1f}%)")
    print(f"  수동 번역 필요:      {total_empty} ({total_empty/max(total_concrete,1)*100:.1f}%)")
    print(f"  새 커버리지:         {total_existing + total_auto}/{total_concrete} "
          f"({(total_existing + total_auto)/max(total_concrete,1)*100:.1f}%)")


def _print_stats(missing, concrete, all_blueprints):
    """Print detailed stats about missing translations."""
    print(f"\n{'=' * 60}")
    print(f"누락 블루프린트 분석")
    print(f"{'=' * 60}")

    # Group by XML file
    by_file = defaultdict(list)
    for bp_name, bp_data in missing.items():
        by_file[bp_data["file"]].append((bp_name, bp_data["display_name"]))

    for xml_file, items in sorted(by_file.items()):
        print(f"\n  {xml_file}: {len(items)}개 누락")
        for bp_name, dn in items[:5]:
            print(f"    - {bp_name}: {dn}")
        if len(items) > 5:
            print(f"    ... 외 {len(items) - 5}개")


# ============================================================
# Entry Point
# ============================================================

if __name__ == "__main__":
    args = sys.argv[1:]

    if "--stats" in args:
        generate_translations(stats_only=True)
    elif "--dry-run" in args:
        generate_translations(dry_run=True)
    else:
        generate_translations()
