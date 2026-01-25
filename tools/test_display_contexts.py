#!/usr/bin/env python3
"""
ObjectTranslator V1 vs V2 Context Test Script

게임 내 다양한 표시 컨텍스트에서 한글화가 올바르게 동작하는지 검증하고
V1(기존)과 V2(리팩토링) 번역 결과 동등성을 비교합니다.

사용법:
    python3 tools/test_display_contexts.py              # 기본 실행
    python3 tools/test_display_contexts.py --verbose    # 상세 출력
    python3 tools/test_display_contexts.py --context inventory  # 특정 컨텍스트만
    python3 tools/test_display_contexts.py --v1-only    # V1만 테스트
    python3 tools/test_display_contexts.py --v2-only    # V2만 테스트
    python3 tools/test_display_contexts.py --failures-only  # 실패만 출력
"""

import json
import re
import os
import sys
import argparse
from pathlib import Path
from typing import Dict, List, Tuple, Optional, Any
from dataclasses import dataclass
from abc import ABC, abstractmethod
from datetime import datetime
from enum import Enum, auto


# ============================================================
# Colors for Terminal Output
# ============================================================
class Colors:
    GREEN = '\033[92m'
    RED = '\033[91m'
    YELLOW = '\033[93m'
    CYAN = '\033[96m'
    MAGENTA = '\033[95m'
    BOLD = '\033[1m'
    DIM = '\033[2m'
    RESET = '\033[0m'


# ============================================================
# Display Context Types
# ============================================================
class DisplayContext(Enum):
    INVENTORY = auto()  # Inventory list
    TOOLTIP = auto()    # Hover tooltip
    SHOP = auto()       # Shop display
    LOOK = auto()       # Look screen (examine)


# ============================================================
# Global Dictionaries (loaded from JSON)
# ============================================================
materials: Dict[str, str] = {}
qualities: Dict[str, str] = {}
processing: Dict[str, str] = {}
modifiers: Dict[str, str] = {}
tonics: Dict[str, str] = {}
grenades: Dict[str, str] = {}
marks: Dict[str, str] = {}
colors: Dict[str, str] = {}

species: Dict[str, str] = {}
base_nouns: Dict[str, str] = {}

states: Dict[str, str] = {}
liquids: Dict[str, str] = {}
of_patterns: Dict[str, str] = {}
body_parts: Dict[str, str] = {}
part_suffixes: Dict[str, str] = {}

# Sorted prefix lists (longest first)
all_prefixes_sorted: List[Tuple[str, str]] = []
color_tag_vocab_sorted: List[Tuple[str, str]] = []
base_nouns_sorted: List[Tuple[str, str]] = []


# ============================================================
# JSON Loading Functions
# ============================================================
def get_project_root() -> Path:
    """Return project root directory"""
    script_path = Path(__file__).resolve()
    return script_path.parent.parent


def load_json_section(data: dict, key: str, target: Dict[str, str]):
    """Load JSON section into dictionary (skip comments)"""
    if key not in data:
        return
    section = data[key]
    for k, v in section.items():
        if not k.startswith("_"):
            target[k.lower()] = v


def load_dictionaries():
    """Load all JSON dictionary files"""
    global materials, qualities, processing, modifiers, tonics, grenades, marks, colors
    global species, base_nouns
    global states, liquids, of_patterns, body_parts, part_suffixes
    global all_prefixes_sorted, color_tag_vocab_sorted, base_nouns_sorted

    root = get_project_root()
    objects_path = root / "LOCALIZATION" / "OBJECTS"

    # 1. items/_common.json
    common_path = objects_path / "items" / "_common.json"
    if common_path.exists():
        with open(common_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        load_json_section(data, "materials", materials)
        load_json_section(data, "qualities", qualities)
        load_json_section(data, "processing", processing)
        load_json_section(data, "modifiers", modifiers)
        load_json_section(data, "tonics", tonics)
        load_json_section(data, "grenades", grenades)
        load_json_section(data, "marks", marks)
        load_json_section(data, "colors", colors)

    # 2. items/_nouns.json
    nouns_path = objects_path / "items" / "_nouns.json"
    if nouns_path.exists():
        with open(nouns_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        for section_key in data:
            if not section_key.startswith("_"):
                load_json_section(data, section_key, base_nouns)

    # 3. creatures/_common.json
    creatures_common_path = objects_path / "creatures" / "_common.json"
    if creatures_common_path.exists():
        with open(creatures_common_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        load_json_section(data, "species", species)
        load_json_section(data, "common_terms", species)

    # 4. _suffixes.json
    suffixes_path = objects_path / "_suffixes.json"
    if suffixes_path.exists():
        with open(suffixes_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        load_json_section(data, "states", states)
        load_json_section(data, "liquids", liquids)
        load_json_section(data, "of_patterns", of_patterns)
        load_json_section(data, "body_parts", body_parts)
        load_json_section(data, "part_suffixes", part_suffixes)

    # 5. Build sorted prefix lists
    all_prefixes = {}
    all_prefixes.update(materials)
    all_prefixes.update(qualities)
    all_prefixes.update(processing)
    all_prefixes.update(modifiers)
    all_prefixes.update(tonics)
    all_prefixes.update(grenades)
    all_prefixes.update(colors)
    all_prefixes_sorted.clear()
    all_prefixes_sorted.extend(
        sorted(all_prefixes.items(), key=lambda x: -len(x[0]))
    )

    # 6. Build color tag vocabulary list
    color_vocab = {}
    color_vocab.update(materials)
    color_vocab.update(qualities)
    color_vocab.update(tonics)
    color_vocab.update(grenades)
    color_vocab.update(modifiers)
    color_vocab.update(species)
    color_vocab.update(liquids)
    color_tag_vocab_sorted.clear()
    color_tag_vocab_sorted.extend(
        sorted(color_vocab.items(), key=lambda x: -len(x[0]))
    )

    # 7. Build base nouns list
    base_nouns_sorted.clear()
    base_nouns_sorted.extend(
        sorted(base_nouns.items(), key=lambda x: -len(x[0]))
    )


# ============================================================
# V1 Translator (Original Logic)
# ============================================================
class TranslatorV1:
    """Original ObjectTranslator logic ported to Python"""

    @staticmethod
    def strip_color_tags(text: str) -> str:
        """Remove color tags: {{X|content}} -> content"""
        if not text:
            return text

        result = text
        result = re.sub(r'&[\^]?[a-zA-Z]', '', result)

        limit = 10
        while limit > 0 and '{{' in result:
            new_result = re.sub(r'\{\{[^{}|]+\|([^{}]*)\}\}', r'\1', result)
            if new_result == result:
                break
            result = new_result
            limit -= 1

        return result

    @staticmethod
    def translate_materials_in_color_tags(text: str) -> str:
        """Translate materials inside color tags"""
        if not text or '{{' not in text:
            return text

        def replace_in_tag(match):
            tag = match.group(1)
            content = match.group(2)
            content_lower = content.lower()

            for eng, ko in color_tag_vocab_sorted:
                if content_lower == eng:
                    return f'{{{{{tag}|{ko}}}}}'

            for eng, ko in base_nouns_sorted:
                if content_lower == eng:
                    return f'{{{{{tag}|{ko}}}}}'

            words = content.split()
            if len(words) > 1:
                translated_words = []
                for word in words:
                    word_lower = word.lower()
                    translated = None
                    for eng, ko in color_tag_vocab_sorted:
                        if word_lower == eng:
                            translated = ko
                            break
                    if not translated:
                        for eng, ko in base_nouns_sorted:
                            if word_lower == eng:
                                translated = ko
                                break
                    translated_words.append(translated if translated else word)
                return f'{{{{{tag}|{" ".join(translated_words)}}}}}'

            return match.group(0)

        result = re.sub(r'\{\{([^|]+)\|([^{}]+)\}\}', replace_in_tag, text)
        return result

    @staticmethod
    def extract_and_translate_prefixes(name: str) -> Tuple[Optional[str], str]:
        """Extract and translate prefixes"""
        translated_prefixes = []
        current = name

        found_any = True
        while found_any:
            found_any = False
            for eng, ko in all_prefixes_sorted:
                if current.lower().startswith(eng.lower() + " "):
                    translated_prefixes.append(ko)
                    current = current[len(eng) + 1:]
                    found_any = True
                    break

        if translated_prefixes:
            return " ".join(translated_prefixes), current
        return None, name

    @staticmethod
    def extract_all_suffixes(name: str) -> Tuple[str, str]:
        """Extract all suffixes"""
        if not name:
            return name, ""

        result = name
        extracted_suffixes = []

        # Parentheses suffix
        paren_match = re.search(r'(\s*\([^)]+\))$', result)
        if paren_match:
            extracted_suffixes.insert(0, paren_match.group(1))
            result = result[:paren_match.start()]

        # Quantity suffix
        quantity_match = re.search(r'(\s*x\d+)$', result)
        if quantity_match:
            extracted_suffixes.insert(0, quantity_match.group(1))
            result = result[:quantity_match.start()]

        # Bracket suffix
        bracket_match = re.search(r'(\s*\[[^\]]+\])$', result)
        if bracket_match:
            extracted_suffixes.insert(0, bracket_match.group(1))
            result = result[:bracket_match.start()]

        # Additional quantity suffix
        quantity_match2 = re.search(r'(\s*x\d+)$', result)
        if quantity_match2:
            extracted_suffixes.insert(0, quantity_match2.group(1))
            result = result[:quantity_match2.start()]

        # +X suffix
        plus_match = re.search(r'(\s*[+-]\d+)$', result)
        if plus_match:
            extracted_suffixes.insert(0, plus_match.group(1))
            result = result[:plus_match.start()]

        # "of X" suffix
        of_match = re.search(r"(\s+of\s+[\w\s\-']+)$", result, re.IGNORECASE)
        if of_match:
            extracted_suffixes.insert(0, of_match.group(1))
            result = result[:of_match.start()]

        return result.strip(), "".join(extracted_suffixes)

    @staticmethod
    def translate_all_suffixes(suffixes: str) -> str:
        """Translate suffixes"""
        if not suffixes:
            return ""

        result = suffixes

        for eng, ko in states.items():
            if eng.lower() in result.lower():
                result = re.sub(re.escape(eng), ko, result, flags=re.IGNORECASE)

        def drams_replace(m):
            amount = m.group(1)
            liquid = m.group(2).strip()
            liquid_stripped = TranslatorV1.strip_color_tags(liquid).lower()
            liquid_ko = liquids.get(liquid_stripped, liquid)
            return f"[{liquid_ko} {amount}드램]"

        result = re.sub(r'\[(\d+) drams? of ([^\]]+)\]', drams_replace, result, flags=re.IGNORECASE)

        def of_replace(m):
            element = m.group(1).strip()
            full_pattern = f"of {element}".lower()
            if full_pattern in of_patterns:
                return of_patterns[full_pattern]
            element_ko = of_patterns.get(element.lower(), element)
            if element_ko != element:
                return f"의 {element_ko}"
            return m.group(0)

        result = re.sub(r"\s+of\s+([\w\s\-']+)$", of_replace, result, flags=re.IGNORECASE)

        return result

    @staticmethod
    def try_get_creature_translation(creature_name: str) -> Optional[str]:
        return species.get(creature_name.lower())

    @staticmethod
    def try_get_item_translation(item_name: str) -> Optional[str]:
        return base_nouns.get(item_name.lower())

    @staticmethod
    def translate_nouns_in_text(text: str) -> str:
        result = text
        for eng, ko in base_nouns_sorted:
            pattern = r'\b' + re.escape(eng) + r'\b'
            result = re.sub(pattern, ko, result, flags=re.IGNORECASE)
        return result

    @staticmethod
    def translate_prefixes_in_text(text: str) -> str:
        result = text
        for eng, ko in all_prefixes_sorted:
            pattern = r'\b' + re.escape(eng) + r'\b'
            result = re.sub(pattern, ko, result, flags=re.IGNORECASE)
        return result

    @staticmethod
    def try_translate_corpse(original_name: str) -> Optional[str]:
        stripped = TranslatorV1.strip_color_tags(original_name)
        if not stripped.lower().endswith(" corpse"):
            return None

        creature_part = stripped[:-len(" corpse")]
        creature_ko = TranslatorV1.try_get_creature_translation(creature_part)
        if creature_ko:
            return f"{creature_ko} 시체"
        return None

    @staticmethod
    def try_translate_dynamic_food(original_name: str) -> Optional[str]:
        stripped = TranslatorV1.strip_color_tags(original_name)

        if stripped.lower().endswith(" jerky"):
            creature_part = stripped[:-len(" jerky")]
            creature_ko = TranslatorV1.try_get_creature_translation(creature_part)
            if creature_ko:
                return f"{creature_ko} 육포"

        if stripped.lower().endswith(" meat"):
            creature_part = stripped[:-len(" meat")]
            if creature_part.lower() == "cooked":
                return "조리된 고기"
            creature_ko = TranslatorV1.try_get_creature_translation(creature_part)
            if creature_ko:
                return f"{creature_ko} 고기"

        if stripped.lower().endswith(" haunch"):
            creature_part = stripped[:-len(" haunch")]
            creature_ko = TranslatorV1.try_get_creature_translation(creature_part)
            if creature_ko:
                return f"{creature_ko} 넓적다리"

        if stripped.lower().startswith("preserved "):
            ingredient_part = stripped[len("preserved "):]
            creature_ko = TranslatorV1.try_get_creature_translation(ingredient_part)
            if creature_ko:
                return f"절임 {creature_ko}"

        return None

    @staticmethod
    def try_translate_dynamic_parts(original_name: str) -> Optional[str]:
        stripped = TranslatorV1.strip_color_tags(original_name)

        for eng_suffix, ko_suffix in sorted(part_suffixes.items(), key=lambda x: -len(x[0])):
            if stripped.lower().endswith(eng_suffix.lower()):
                creature_part = stripped[:len(stripped) - len(eng_suffix)]

                if creature_part.lower().startswith("elder "):
                    actual_creature = creature_part[len("elder "):]
                    creature_ko = TranslatorV1.try_get_creature_translation(actual_creature)
                    if creature_ko:
                        return f"장로 {creature_ko} {ko_suffix.strip()}"

                if creature_part.lower().startswith("raw "):
                    actual_creature = creature_part[len("raw "):]
                    creature_ko = TranslatorV1.try_get_creature_translation(actual_creature)
                    if creature_ko:
                        return f"생 {creature_ko} {ko_suffix.strip()}"

                creature_ko = TranslatorV1.try_get_creature_translation(creature_part)
                if creature_ko:
                    return f"{creature_ko} {ko_suffix.strip()}"

        return None

    @staticmethod
    def try_translate_possessive(original_name: str) -> Optional[str]:
        stripped = TranslatorV1.strip_color_tags(original_name)

        match = re.match(r"^(.+)'s\s+(.+)$", stripped, re.IGNORECASE)
        if not match:
            return None

        creature = match.group(1).strip()
        part = match.group(2).strip()

        creature_ko = TranslatorV1.try_get_creature_translation(creature)
        if not creature_ko:
            return None

        part_ko = body_parts.get(part.lower()) or base_nouns.get(part.lower())
        if part_ko:
            return f"{creature_ko}의 {part_ko}"

        return None

    @staticmethod
    def try_translate_of_pattern(original_name: str) -> Optional[str]:
        stripped = TranslatorV1.strip_color_tags(original_name)

        match = re.match(r"^(.+?)\s+of\s+(?:the\s+)?(.+)$", stripped, re.IGNORECASE)
        if not match:
            return None

        item_part = match.group(1).strip()
        of_part = match.group(2).strip()

        of_ko = None
        full_pattern_with_the = f"of the {of_part}".lower()
        full_pattern = f"of {of_part}".lower()

        if full_pattern_with_the in of_patterns:
            of_ko = of_patterns[full_pattern_with_the]
        elif full_pattern in of_patterns:
            of_ko = of_patterns[full_pattern]
        elif of_part.lower() in of_patterns:
            of_ko = f"{of_patterns[of_part.lower()]}의"
        else:
            of_part_translated = TranslatorV1.translate_nouns_in_text(of_part)
            of_part_translated = TranslatorV1.translate_prefixes_in_text(of_part_translated)
            if of_part_translated != of_part:
                of_ko = f"{of_part_translated}의"
            else:
                return None

        prefix_ko, remainder = TranslatorV1.extract_and_translate_prefixes(item_part)

        if prefix_ko:
            material_ko, base_only = TranslatorV1.extract_and_translate_prefixes(remainder)
            if material_ko:
                base_ko = TranslatorV1.try_get_item_translation(base_only)
                if base_ko:
                    return f"{prefix_ko} {material_ko} {of_ko} {base_ko}".strip()

            base_ko = TranslatorV1.try_get_item_translation(remainder)
            if base_ko:
                return f"{prefix_ko} {of_ko} {base_ko}".strip()
            else:
                base_ko = TranslatorV1.translate_nouns_in_text(remainder)
                if base_ko != remainder:
                    return f"{prefix_ko} {of_ko} {base_ko}".strip()

        item_ko = TranslatorV1.try_get_item_translation(item_part)
        if not item_ko:
            item_ko = TranslatorV1.translate_nouns_in_text(item_part)
            item_ko = TranslatorV1.translate_prefixes_in_text(item_ko)

        return f"{of_ko} {item_ko}".strip()

    @staticmethod
    def restore_color_tags(original: str, translated: str) -> str:
        if '{{' not in original:
            return translated

        full_tag_match = re.match(r'^\{\{([^|]+)\|(.+)\}\}$', original)
        if full_tag_match:
            tag = full_tag_match.group(1)
            return f'{{{{{tag}|{translated}}}}}'

        tag_match = re.search(r'\{\{([^|]+)\|([^{}]+)\}\}', original)
        if tag_match:
            tag = tag_match.group(1)
            content = tag_match.group(2)

            content_translated = None
            for eng, ko in color_tag_vocab_sorted:
                if content.lower() == eng:
                    content_translated = ko
                    break
            if not content_translated:
                for eng, ko in base_nouns_sorted:
                    if content.lower() == eng:
                        content_translated = ko
                        break

            if content_translated and content_translated in translated:
                tagged_content = f'{{{{{tag}|{content_translated}}}}}'
                result = translated.replace(content_translated, tagged_content, 1)
                return result

        return translated

    @staticmethod
    def translate(original_name: str) -> Tuple[bool, str]:
        """Main translation function (V1)"""
        if not original_name:
            return False, original_name

        with_translated_materials = TranslatorV1.translate_materials_in_color_tags(original_name)
        has_color_tags = '{{' in original_name

        stripped = TranslatorV1.strip_color_tags(original_name)
        has_of_pattern = ' of ' in stripped.lower()
        is_drams_pattern = 'drams of' in stripped.lower()
        is_in_bracket = re.search(r'\[[^\]]*of[^\]]*\]', stripped.lower())

        if has_of_pattern and not is_drams_pattern and not is_in_bracket:
            of_result = TranslatorV1.try_translate_of_pattern(original_name)
            if of_result:
                return True, of_result

        base_name, all_suffixes = TranslatorV1.extract_all_suffixes(stripped)
        prefix_ko, remainder = TranslatorV1.extract_and_translate_prefixes(base_name)

        if prefix_ko:
            base_ko = TranslatorV1.try_get_item_translation(remainder) or TranslatorV1.try_get_creature_translation(remainder)
            if base_ko:
                suffix_ko = TranslatorV1.translate_all_suffixes(all_suffixes)
                result = f"{prefix_ko} {base_ko}{suffix_ko}"
                if has_color_tags:
                    result = TranslatorV1.restore_color_tags(original_name, result)
                return True, result

            material_ko, base_only = TranslatorV1.extract_and_translate_prefixes(remainder)
            if material_ko:
                base_ko = TranslatorV1.try_get_item_translation(base_only) or TranslatorV1.try_get_creature_translation(base_only)
                if base_ko:
                    suffix_ko = TranslatorV1.translate_all_suffixes(all_suffixes)
                    result = f"{prefix_ko} {material_ko} {base_ko}{suffix_ko}"
                    if has_color_tags:
                        result = TranslatorV1.restore_color_tags(original_name, result)
                    return True, result

        base_ko = TranslatorV1.try_get_item_translation(base_name) or TranslatorV1.try_get_creature_translation(base_name)
        if base_ko:
            suffix_ko = TranslatorV1.translate_all_suffixes(all_suffixes)
            result = f"{base_ko}{suffix_ko}" if suffix_ko else base_ko
            if has_color_tags:
                result = TranslatorV1.restore_color_tags(original_name, result)
            return True, result

        corpse_result = TranslatorV1.try_translate_corpse(original_name)
        if corpse_result:
            return True, corpse_result

        food_result = TranslatorV1.try_translate_dynamic_food(original_name)
        if food_result:
            return True, food_result

        parts_stripped = TranslatorV1.strip_color_tags(original_name)
        parts_base, parts_suffix = TranslatorV1.extract_all_suffixes(parts_stripped)
        parts_result = TranslatorV1.try_translate_dynamic_parts(parts_base)
        if parts_result:
            suffix_ko = TranslatorV1.translate_all_suffixes(parts_suffix)
            return True, f"{parts_result}{suffix_ko}"

        of_result = TranslatorV1.try_translate_of_pattern(original_name)
        if of_result:
            return True, of_result

        possessive_result = TranslatorV1.try_translate_possessive(original_name)
        if possessive_result:
            return True, possessive_result

        if with_translated_materials != original_name:
            result = with_translated_materials

            def translate_outside_tags(text):
                parts = re.split(r'(\{\{[^}]+\}\})', text)
                translated_parts = []
                for part in parts:
                    if part.startswith('{{'):
                        translated_parts.append(part)
                    else:
                        translated = TranslatorV1.translate_nouns_in_text(part)
                        translated = TranslatorV1.translate_prefixes_in_text(translated)
                        translated_parts.append(translated)
                return ''.join(translated_parts)

            result = translate_outside_tags(result)
            if result != with_translated_materials:
                return True, result
            if result != original_name:
                return True, result

        result = TranslatorV1.translate_nouns_in_text(original_name)
        if result != original_name:
            return True, result

        return False, original_name


# ============================================================
# V2 Translator (Pipeline/Strategy Pattern)
# ============================================================
@dataclass
class TranslationContext:
    """Context passed through the translation pipeline"""
    original_name: str
    stripped_name: str = ""
    base_name: str = ""
    suffixes: str = ""
    prefix_ko: Optional[str] = None
    remainder: str = ""
    has_color_tags: bool = False
    result: Optional[str] = None
    success: bool = False


class TranslationHandler(ABC):
    """Abstract base class for translation handlers"""

    @abstractmethod
    def can_handle(self, ctx: TranslationContext) -> bool:
        pass

    @abstractmethod
    def handle(self, ctx: TranslationContext) -> TranslationContext:
        pass


class ColorTagHandler(TranslationHandler):
    """Handle color tag translations"""

    def can_handle(self, ctx: TranslationContext) -> bool:
        return '{{' in ctx.original_name

    def handle(self, ctx: TranslationContext) -> TranslationContext:
        ctx.has_color_tags = True
        return ctx


class OfPatternHandler(TranslationHandler):
    """Handle 'of X' patterns"""

    def can_handle(self, ctx: TranslationContext) -> bool:
        stripped = ctx.stripped_name
        has_of = ' of ' in stripped.lower()
        is_drams = 'drams of' in stripped.lower()
        is_in_bracket = re.search(r'\[[^\]]*of[^\]]*\]', stripped.lower())
        return has_of and not is_drams and not is_in_bracket

    def handle(self, ctx: TranslationContext) -> TranslationContext:
        result = TranslatorV1.try_translate_of_pattern(ctx.original_name)
        if result:
            ctx.result = result
            ctx.success = True
        return ctx


class PrefixSuffixHandler(TranslationHandler):
    """Handle prefix + base + suffix patterns"""

    def can_handle(self, ctx: TranslationContext) -> bool:
        return True  # Always try

    def handle(self, ctx: TranslationContext) -> TranslationContext:
        if ctx.success:
            return ctx

        base_name, all_suffixes = TranslatorV1.extract_all_suffixes(ctx.stripped_name)
        ctx.base_name = base_name
        ctx.suffixes = all_suffixes

        prefix_ko, remainder = TranslatorV1.extract_and_translate_prefixes(base_name)
        ctx.prefix_ko = prefix_ko
        ctx.remainder = remainder

        if prefix_ko:
            base_ko = TranslatorV1.try_get_item_translation(remainder) or TranslatorV1.try_get_creature_translation(remainder)
            if base_ko:
                suffix_ko = TranslatorV1.translate_all_suffixes(all_suffixes)
                result = f"{prefix_ko} {base_ko}{suffix_ko}"
                if ctx.has_color_tags:
                    result = TranslatorV1.restore_color_tags(ctx.original_name, result)
                ctx.result = result
                ctx.success = True
                return ctx

            material_ko, base_only = TranslatorV1.extract_and_translate_prefixes(remainder)
            if material_ko:
                base_ko = TranslatorV1.try_get_item_translation(base_only) or TranslatorV1.try_get_creature_translation(base_only)
                if base_ko:
                    suffix_ko = TranslatorV1.translate_all_suffixes(all_suffixes)
                    result = f"{prefix_ko} {material_ko} {base_ko}{suffix_ko}"
                    if ctx.has_color_tags:
                        result = TranslatorV1.restore_color_tags(ctx.original_name, result)
                    ctx.result = result
                    ctx.success = True
                    return ctx

        return ctx


class DirectMatchHandler(TranslationHandler):
    """Handle direct dictionary matches"""

    def can_handle(self, ctx: TranslationContext) -> bool:
        return True

    def handle(self, ctx: TranslationContext) -> TranslationContext:
        if ctx.success:
            return ctx

        base_ko = TranslatorV1.try_get_item_translation(ctx.base_name) or TranslatorV1.try_get_creature_translation(ctx.base_name)
        if base_ko:
            suffix_ko = TranslatorV1.translate_all_suffixes(ctx.suffixes)
            result = f"{base_ko}{suffix_ko}" if suffix_ko else base_ko
            if ctx.has_color_tags:
                result = TranslatorV1.restore_color_tags(ctx.original_name, result)
            ctx.result = result
            ctx.success = True

        return ctx


class DynamicPatternHandler(TranslationHandler):
    """Handle dynamic patterns (corpse, food, parts, possessive)"""

    def can_handle(self, ctx: TranslationContext) -> bool:
        return True

    def handle(self, ctx: TranslationContext) -> TranslationContext:
        if ctx.success:
            return ctx

        # Corpse
        corpse_result = TranslatorV1.try_translate_corpse(ctx.original_name)
        if corpse_result:
            ctx.result = corpse_result
            ctx.success = True
            return ctx

        # Food
        food_result = TranslatorV1.try_translate_dynamic_food(ctx.original_name)
        if food_result:
            ctx.result = food_result
            ctx.success = True
            return ctx

        # Parts
        parts_base, parts_suffix = TranslatorV1.extract_all_suffixes(ctx.stripped_name)
        parts_result = TranslatorV1.try_translate_dynamic_parts(parts_base)
        if parts_result:
            suffix_ko = TranslatorV1.translate_all_suffixes(parts_suffix)
            ctx.result = f"{parts_result}{suffix_ko}"
            ctx.success = True
            return ctx

        # Of pattern (retry)
        of_result = TranslatorV1.try_translate_of_pattern(ctx.original_name)
        if of_result:
            ctx.result = of_result
            ctx.success = True
            return ctx

        # Possessive
        possessive_result = TranslatorV1.try_translate_possessive(ctx.original_name)
        if possessive_result:
            ctx.result = possessive_result
            ctx.success = True
            return ctx

        return ctx


class ColorTagContentHandler(TranslationHandler):
    """Handle color tag content translation"""

    def can_handle(self, ctx: TranslationContext) -> bool:
        return ctx.has_color_tags

    def handle(self, ctx: TranslationContext) -> TranslationContext:
        if ctx.success:
            return ctx

        with_translated = TranslatorV1.translate_materials_in_color_tags(ctx.original_name)
        if with_translated != ctx.original_name:
            def translate_outside_tags(text):
                parts = re.split(r'(\{\{[^}]+\}\})', text)
                translated_parts = []
                for part in parts:
                    if part.startswith('{{'):
                        translated_parts.append(part)
                    else:
                        translated = TranslatorV1.translate_nouns_in_text(part)
                        translated = TranslatorV1.translate_prefixes_in_text(translated)
                        translated_parts.append(translated)
                return ''.join(translated_parts)

            result = translate_outside_tags(with_translated)
            if result != ctx.original_name:
                ctx.result = result
                ctx.success = True

        return ctx


class FallbackHandler(TranslationHandler):
    """Fallback: translate nouns in text"""

    def can_handle(self, ctx: TranslationContext) -> bool:
        return True

    def handle(self, ctx: TranslationContext) -> TranslationContext:
        if ctx.success:
            return ctx

        result = TranslatorV1.translate_nouns_in_text(ctx.original_name)
        if result != ctx.original_name:
            ctx.result = result
            ctx.success = True

        return ctx


class TranslatorV2:
    """V2 Translator using Pipeline/Strategy pattern"""

    def __init__(self):
        self.handlers: List[TranslationHandler] = [
            ColorTagHandler(),
            OfPatternHandler(),
            PrefixSuffixHandler(),
            DirectMatchHandler(),
            DynamicPatternHandler(),
            ColorTagContentHandler(),
            FallbackHandler(),
        ]

    def translate(self, original_name: str) -> Tuple[bool, str]:
        """Main translation function (V2)"""
        if not original_name:
            return False, original_name

        ctx = TranslationContext(
            original_name=original_name,
            stripped_name=TranslatorV1.strip_color_tags(original_name),
        )

        for handler in self.handlers:
            if handler.can_handle(ctx):
                ctx = handler.handle(ctx)
                if ctx.success:
                    return True, ctx.result

        return False, original_name


# ============================================================
# Context Simulators
# ============================================================
class ContextSimulator:
    """Simulate different display contexts"""

    @staticmethod
    def inventory_format(item: str, quantity: int = 1) -> str:
        """Format for inventory display: with color tags and quantity suffix"""
        if quantity > 1:
            return f"{item} x{quantity}"
        return item

    @staticmethod
    def tooltip_format(item: str, state: Optional[str] = None) -> str:
        """Format for tooltip display: with state suffix"""
        if state:
            return f"{item} {state}"
        return item

    @staticmethod
    def shop_format(item: str) -> str:
        """Format for shop display: color tags, no price"""
        return item

    @staticmethod
    def look_format(item: str, stats: Optional[str] = None) -> str:
        """Format for look screen: with stat suffixes"""
        if stats:
            return f"{item} {stats}"
        return item


# ============================================================
# Test Cases
# ============================================================
@dataclass
class TestCase:
    """Test case definition"""
    num: int
    context: DisplayContext
    input_name: str
    expected: str  # Can contain "또는" for multiple valid answers
    description: str = ""


# Test cases organized by context
TEST_CASES: List[TestCase] = [
    # === INVENTORY CONTEXT (25 cases) ===
    # Basic items with color tags
    TestCase(1, DisplayContext.INVENTORY, "{{w|bronze}} dagger", "{{w|청동}} 단검", "Basic color tag material"),
    TestCase(2, DisplayContext.INVENTORY, "{{B|carbide}} sword", "{{B|카바이드}} 검", "Carbide with color tag"),
    TestCase(3, DisplayContext.INVENTORY, "{{c|crysteel}} blade", "{{c|크리스틸}} 블레이드", "Crysteel blade"),
    TestCase(4, DisplayContext.INVENTORY, "{{r|flaming}} torch", "{{r|불타는}} 횃불", "Flaming modifier"),
    TestCase(5, DisplayContext.INVENTORY, "{{G|hulk}} honey injector", "{{G|헐크}} 꿀 주사기", "Tonic with space"),

    # Quantity suffix
    TestCase(6, DisplayContext.INVENTORY, "arrow x15", "화살 x15", "Quantity suffix"),
    TestCase(7, DisplayContext.INVENTORY, "grenade x3", "수류탄 x3", "Grenade quantity"),
    TestCase(8, DisplayContext.INVENTORY, "torch x14 (unburnt)", "횃불 x14 (미사용)", "Quantity + state"),
    TestCase(9, DisplayContext.INVENTORY, "{{w|bronze}} dagger x5", "{{w|청동}} 단검 x5", "Color tag + quantity"),
    TestCase(10, DisplayContext.INVENTORY, "wooden arrow x100", "나무 화살 x100", "Prefix + quantity"),

    # Prefixes
    TestCase(11, DisplayContext.INVENTORY, "bronze mace", "청동 메이스", "Single prefix"),
    TestCase(12, DisplayContext.INVENTORY, "iron dagger", "철 단검", "Iron prefix"),
    TestCase(13, DisplayContext.INVENTORY, "crysteel sword", "크리스틸 검", "Crysteel prefix"),
    TestCase(14, DisplayContext.INVENTORY, "leather boots", "가죽 부츠", "Leather prefix"),
    TestCase(15, DisplayContext.INVENTORY, "wooden staff", "나무 지팡이", "Wooden prefix"),

    # Compound prefixes
    TestCase(16, DisplayContext.INVENTORY, "engraved bronze mace", "새겨진 청동 메이스", "Double prefix"),
    TestCase(17, DisplayContext.INVENTORY, "flawless crysteel sword", "완벽한 크리스틸 검", "Quality + material"),
    TestCase(18, DisplayContext.INVENTORY, "folded carbide dagger", "접힌 카바이드 단검", "Folded carbide"),
    TestCase(19, DisplayContext.INVENTORY, "studded leather armor", "징박힌 가죽 갑옷", "Studded leather"),
    TestCase(20, DisplayContext.INVENTORY, "reinforced steel shield", "강화된 강철 방패", "Reinforced steel"),

    # +X suffix
    TestCase(21, DisplayContext.INVENTORY, "dagger +3", "단검 +3", "Plus modifier"),
    TestCase(22, DisplayContext.INVENTORY, "sword +2", "검 +2", "Sword plus"),
    TestCase(23, DisplayContext.INVENTORY, "engraved bronze mace +3", "새겨진 청동 메이스 +3", "Compound + plus"),
    TestCase(24, DisplayContext.INVENTORY, "{{c|vibro blade}} +2", "{{c|바이브로 블레이드}} +2", "Color tag + plus"),
    TestCase(25, DisplayContext.INVENTORY, "flawless crysteel sword +1", "완벽한 크리스틸 검 +1", "Quality material + plus"),

    # === TOOLTIP CONTEXT (25 cases) ===
    # State suffixes
    TestCase(26, DisplayContext.TOOLTIP, "torch (lit)", "횃불 (점화됨)", "Lit state"),
    TestCase(27, DisplayContext.TOOLTIP, "torch (unburnt)", "횃불 (미사용)", "Unburnt state"),
    TestCase(28, DisplayContext.TOOLTIP, "waterskin [empty]", "물주머니 [비어있음]", "Empty bracket"),
    TestCase(29, DisplayContext.TOOLTIP, "canteen [full]", "수통 [가득 참]", "Full bracket"),
    TestCase(30, DisplayContext.TOOLTIP, "musket [loaded]", "머스킷 [장전됨]", "Loaded bracket"),

    # Drams pattern
    TestCase(31, DisplayContext.TOOLTIP, "canteen [32 drams of water]", "수통 [물 32드램]", "Drams of water"),
    TestCase(32, DisplayContext.TOOLTIP, "waterskin [16 drams of fresh water]", "물주머니 [신선한 물 16드램]", "Fresh water drams"),
    TestCase(33, DisplayContext.TOOLTIP, "canteen [8 drams of acid]", "수통 [산 8드램]", "Acid drams"),
    TestCase(34, DisplayContext.TOOLTIP, "waterskin [64 drams of honey]", "물주머니 [꿀 64드램]", "Honey drams"),
    TestCase(35, DisplayContext.TOOLTIP, "canteen [1 dram of blood]", "수통 [피 1드램]", "Single dram"),

    # Description items (color tag multi-word)
    TestCase(36, DisplayContext.TOOLTIP, "{{c|vibro blade}}", "{{c|바이브로 블레이드}}", "Multi-word in tag"),
    TestCase(37, DisplayContext.TOOLTIP, "{{c|stun whip}}", "{{c|기절 채찍}}", "Stun whip tag"),
    TestCase(38, DisplayContext.TOOLTIP, "{{G|fresh water}} injector", "{{G|신선한 물}} 주사기", "Fresh water tag"),
    TestCase(39, DisplayContext.TOOLTIP, "{{g|seed-spitting vine}}", "{{g|씨앗발사 덩굴}}", "Hyphen in tag"),
    TestCase(40, DisplayContext.TOOLTIP, "{{r|thermal}} grenade", "{{r|열}} 수류탄", "Thermal grenade"),

    # Complex tooltip items
    TestCase(41, DisplayContext.TOOLTIP, "bronze dagger (lit)", "청동 단검 (점화됨)", "Prefix + state"),
    TestCase(42, DisplayContext.TOOLTIP, "crysteel sword [empty]", "크리스틸 검 [비어있음]", "Prefix + bracket"),
    TestCase(43, DisplayContext.TOOLTIP, "leather boots +2", "가죽 부츠 +2", "Prefix + plus"),
    TestCase(44, DisplayContext.TOOLTIP, "waterskin [32 drams of water] x2", "물주머니 [물 32드램] x2", "Drams + quantity"),
    TestCase(45, DisplayContext.TOOLTIP, "canteen [empty] x3", "수통 [비어있음] x3", "Bracket + quantity"),

    # Items with multiple suffixes
    TestCase(46, DisplayContext.TOOLTIP, "torch x5 (unburnt)", "횃불 x5 (미사용)", "Quantity + paren"),
    TestCase(47, DisplayContext.TOOLTIP, "arrow x100 [empty]", "화살 x100 [비어있음]", "Quantity + bracket"),
    TestCase(48, DisplayContext.TOOLTIP, "dagger +2 (lit)", "단검 +2 (점화됨)", "Plus + paren"),
    TestCase(49, DisplayContext.TOOLTIP, "mace", "메이스", "Simple item"),
    TestCase(50, DisplayContext.TOOLTIP, "helmet", "투구", "Helmet"),

    # === SHOP CONTEXT (25 cases) ===
    # Nested color tags (challenging)
    TestCase(51, DisplayContext.SHOP, "{{c|crysteel}} sword", "{{c|크리스틸}} 검", "Simple crysteel tag"),
    TestCase(52, DisplayContext.SHOP, "{{w|bronze}} helm", "{{w|청동}} 투구", "Bronze helm tag"),
    TestCase(53, DisplayContext.SHOP, "{{B|carbide}} axe", "{{B|카바이드}} 도끼", "Carbide axe"),
    TestCase(54, DisplayContext.SHOP, "{{r|flaming}} sword", "{{r|불타는}} 검", "Flaming sword"),
    TestCase(55, DisplayContext.SHOP, "{{G|hulk}} honey", "{{G|헐크}} 꿀", "Hulk honey"),

    # Material items
    TestCase(56, DisplayContext.SHOP, "agate tube", "마노 튜브", "Agate tube"),
    TestCase(57, DisplayContext.SHOP, "amethyst tube", "자수정 튜브", "Amethyst tube"),
    TestCase(58, DisplayContext.SHOP, "jasper tube", "벽옥 튜브", "Jasper tube"),
    TestCase(59, DisplayContext.SHOP, "topaz tube", "토파즈 튜브", "Topaz tube"),
    TestCase(60, DisplayContext.SHOP, "peridot tube", "페리도트 튜브", "Peridot tube"),

    # Weapon types
    TestCase(61, DisplayContext.SHOP, "vibro dagger", "바이브로 단검", "Vibro dagger"),
    TestCase(62, DisplayContext.SHOP, "electric whip", "전기 채찍", "Electric whip"),
    TestCase(63, DisplayContext.SHOP, "plasma rifle", "플라즈마 라이플", "Plasma rifle"),
    TestCase(64, DisplayContext.SHOP, "eigen pistol", "아이겐 권총", "Eigen pistol"),
    TestCase(65, DisplayContext.SHOP, "nullray cannon", "널레이 대포", "Nullray cannon"),

    # Complex shop items
    TestCase(66, DisplayContext.SHOP, "high explosive grenade", "고폭 수류탄", "HE grenade"),
    TestCase(67, DisplayContext.SHOP, "stun grenade", "기절 수류탄", "Stun grenade"),
    TestCase(68, DisplayContext.SHOP, "thermal grenade", "열 수류탄", "Thermal grenade"),
    TestCase(69, DisplayContext.SHOP, "phase grenade", "위상 수류탄", "Phase grenade"),
    TestCase(70, DisplayContext.SHOP, "freeze grenade", "냉동 수류탄", "Freeze grenade"),

    # Armor items
    TestCase(71, DisplayContext.SHOP, "leather armor", "가죽 갑옷", "Leather armor"),
    TestCase(72, DisplayContext.SHOP, "iron helm", "철 투구", "Iron helm"),
    TestCase(73, DisplayContext.SHOP, "steel boots", "강철 부츠", "Steel boots"),
    TestCase(74, DisplayContext.SHOP, "copper gauntlets", "구리 건틀릿", "Copper gauntlets"),
    TestCase(75, DisplayContext.SHOP, "fullerite cloak", "풀러라이트 망토", "Fullerite cloak"),

    # === LOOK CONTEXT (25 cases) ===
    # Corpse patterns
    TestCase(76, DisplayContext.LOOK, "bear corpse", "곰 시체", "Bear corpse"),
    TestCase(77, DisplayContext.LOOK, "snapjaw corpse", "스냅조 시체", "Snapjaw corpse"),
    TestCase(78, DisplayContext.LOOK, "basilisk corpse", "바실리스크 시체", "Basilisk corpse"),
    TestCase(79, DisplayContext.LOOK, "mopango corpse", "모팡고 시체", "Mopango corpse"),
    TestCase(80, DisplayContext.LOOK, "wolf corpse", "늑대 시체", "Wolf corpse"),

    # Dynamic food patterns
    TestCase(81, DisplayContext.LOOK, "bear jerky", "곰 육포", "Bear jerky"),
    TestCase(82, DisplayContext.LOOK, "pig meat", "돼지 고기", "Pig meat"),
    TestCase(83, DisplayContext.LOOK, "boar haunch", "멧돼지 넓적다리", "Boar haunch"),
    TestCase(84, DisplayContext.LOOK, "preserved bear", "절임 곰", "Preserved bear"),
    TestCase(85, DisplayContext.LOOK, "cooked meat", "조리된 고기", "Cooked meat"),

    # Dynamic parts patterns
    TestCase(86, DisplayContext.LOOK, "bear egg", "곰 알", "Bear egg"),
    TestCase(87, DisplayContext.LOOK, "wolf hide", "늑대 가죽", "Wolf hide"),
    TestCase(88, DisplayContext.LOOK, "bear bone", "곰 뼈", "Bear bone"),
    TestCase(89, DisplayContext.LOOK, "snapjaw skull", "스냅조 두개골", "Snapjaw skull"),
    TestCase(90, DisplayContext.LOOK, "scorpion egg", "전갈 알", "Scorpion egg"),

    # Parts with prefix
    TestCase(91, DisplayContext.LOOK, "raw bear hide", "생 곰 가죽", "Raw bear hide"),
    TestCase(92, DisplayContext.LOOK, "elder bear skull", "장로 곰 두개골", "Elder bear skull"),
    TestCase(93, DisplayContext.LOOK, "elder bear skull x3", "장로 곰 두개골 x3", "Elder skull with quantity"),
    TestCase(94, DisplayContext.LOOK, "centipede scale", "지네 비늘", "Centipede scale"),
    TestCase(95, DisplayContext.LOOK, "cherub egg", "케루브 알", "Cherub egg"),

    # Possessive patterns
    TestCase(96, DisplayContext.LOOK, "panther's claw", "표범의 발톱", "Panther claw"),
    TestCase(97, DisplayContext.LOOK, "bear's fang", "곰의 송곳니", "Bear fang"),
    TestCase(98, DisplayContext.LOOK, "wolf's hide", "늑대의 가죽", "Wolf hide possessive"),
    TestCase(99, DisplayContext.LOOK, "snapjaw's tooth", "스냅조의 이빨", "Snapjaw tooth"),
    TestCase(100, DisplayContext.LOOK, "basilisk's eye", "바실리스크의 눈", "Basilisk eye"),
]


# ============================================================
# Verification
# ============================================================
@dataclass
class TestResult:
    """Result of a single test case"""
    test_case: TestCase
    v1_result: str
    v2_result: str
    v1_success: bool
    v2_success: bool
    v1_matches_expected: bool
    v2_matches_expected: bool
    v1_v2_equal: bool


class ComparisonVerifier:
    """Verify and compare V1 vs V2 results"""

    def __init__(self):
        self.v1 = TranslatorV1()
        self.v2 = TranslatorV2()

    def check_expected(self, result: str, expected: str) -> bool:
        """Check if result matches expected (supports '또는' separator)"""
        expected_options = [e.strip() for e in expected.split("또는")]
        return result in expected_options

    def run_test(self, test_case: TestCase) -> TestResult:
        """Run a single test case"""
        v1_success, v1_result = TranslatorV1.translate(test_case.input_name)
        v2_success, v2_result = self.v2.translate(test_case.input_name)

        return TestResult(
            test_case=test_case,
            v1_result=v1_result,
            v2_result=v2_result,
            v1_success=v1_success,
            v2_success=v2_success,
            v1_matches_expected=self.check_expected(v1_result, test_case.expected),
            v2_matches_expected=self.check_expected(v2_result, test_case.expected),
            v1_v2_equal=(v1_result == v2_result),
        )


# ============================================================
# Report Generation
# ============================================================
class ReportGenerator:
    """Generate test report"""

    CONTEXT_ICONS = {
        DisplayContext.INVENTORY: "📦",
        DisplayContext.TOOLTIP: "💬",
        DisplayContext.SHOP: "🏪",
        DisplayContext.LOOK: "👁",
    }

    CONTEXT_NAMES = {
        DisplayContext.INVENTORY: "INVENTORY",
        DisplayContext.TOOLTIP: "TOOLTIP",
        DisplayContext.SHOP: "SHOP",
        DisplayContext.LOOK: "LOOK",
    }

    def __init__(self, results: List[TestResult], verbose: bool = False, failures_only: bool = False):
        self.results = results
        self.verbose = verbose
        self.failures_only = failures_only

    def print_header(self):
        """Print report header"""
        print(f"\n{Colors.BOLD}╔══════════════════════════════════════════════════════════════╗")
        print(f"║  ObjectTranslator V1 vs V2 Context Test                      ║")
        print(f"║  Date: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}                                   ║")
        print(f"╚══════════════════════════════════════════════════════════════╝{Colors.RESET}\n")

    def print_v1_v2_summary(self):
        """Print V1 vs V2 equality summary"""
        equal_count = sum(1 for r in self.results if r.v1_v2_equal)
        total = len(self.results)

        print(f"{Colors.BOLD}━━━ V1 vs V2 동등성 검사 ━━━{Colors.RESET}")
        if equal_count == total:
            print(f"  {Colors.GREEN}✓ {equal_count}/{total} 케이스 동등{Colors.RESET}")
        else:
            print(f"  {Colors.YELLOW}⚠ {equal_count}/{total} 케이스 동등{Colors.RESET}")
            print(f"  {Colors.RED}✗ {total - equal_count}개 케이스 불일치{Colors.RESET}")
        print()

    def print_context_results(self, context: DisplayContext):
        """Print results for a specific context"""
        icon = self.CONTEXT_ICONS[context]
        name = self.CONTEXT_NAMES[context]
        context_results = [r for r in self.results if r.test_case.context == context]

        if not context_results:
            return

        print(f"{Colors.BOLD}━━━ {icon} {name} CONTEXT ━━━{Colors.RESET}")

        for result in context_results:
            tc = result.test_case
            passed = result.v1_matches_expected and result.v2_matches_expected and result.v1_v2_equal

            if self.failures_only and passed:
                continue

            if passed:
                status = f"{Colors.GREEN}✓{Colors.RESET}"
            else:
                status = f"{Colors.RED}✗{Colors.RESET}"

            print(f"  {status} #{tc.num}: \"{tc.input_name}\"")

            if self.verbose or not passed:
                print(f"    V1: \"{result.v1_result}\"")
                print(f"    V2: \"{result.v2_result}\"")

                if not result.v1_v2_equal:
                    print(f"    {Colors.RED}[V1 ≠ V2]{Colors.RESET}")

                if not result.v1_matches_expected:
                    print(f"    {Colors.YELLOW}Expected: \"{tc.expected}\"{Colors.RESET}")

        print()

    def print_summary(self):
        """Print final summary"""
        total = len(self.results)
        v1_v2_equal = sum(1 for r in self.results if r.v1_v2_equal)
        v1_pass = sum(1 for r in self.results if r.v1_matches_expected)
        v2_pass = sum(1 for r in self.results if r.v2_matches_expected)
        both_pass = sum(1 for r in self.results if r.v1_matches_expected and r.v2_matches_expected)

        print(f"{Colors.BOLD}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
        print(f"SUMMARY")
        print(f"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━{Colors.RESET}")
        print(f"  V1 vs V2 동등: {v1_v2_equal}/{total} ({100*v1_v2_equal/total:.1f}%)")
        print(f"  V1 번역 성공:  {v1_pass}/{total} ({100*v1_pass/total:.1f}%)")
        print(f"  V2 번역 성공:  {v2_pass}/{total} ({100*v2_pass/total:.1f}%)")
        print(f"  전체 통과:     {both_pass}/{total} ({100*both_pass/total:.1f}%)")

        # Print failed cases
        failed = [r for r in self.results if not (r.v1_matches_expected and r.v2_matches_expected and r.v1_v2_equal)]
        if failed:
            print(f"\n{Colors.BOLD}{Colors.RED}FAILED CASES:{Colors.RESET}")
            for r in failed:
                tc = r.test_case
                ctx_name = self.CONTEXT_NAMES[tc.context]
                print(f"  {tc.num}. [{ctx_name}] \"{tc.input_name}\"")
                print(f"     Expected: \"{tc.expected}\"")
                print(f"     V1 Got:   \"{r.v1_result}\"")
                print(f"     V2 Got:   \"{r.v2_result}\"")
                if not r.v1_v2_equal:
                    print(f"     {Colors.RED}Issue: V1 ≠ V2{Colors.RESET}")
                elif not r.v1_matches_expected:
                    print(f"     {Colors.YELLOW}Issue: Does not match expected{Colors.RESET}")

    def generate(self):
        """Generate full report"""
        self.print_header()
        self.print_v1_v2_summary()

        for context in DisplayContext:
            self.print_context_results(context)

        self.print_summary()


# ============================================================
# Main
# ============================================================
def parse_args():
    """Parse command line arguments"""
    parser = argparse.ArgumentParser(description="ObjectTranslator V1 vs V2 Context Test")
    parser.add_argument('--verbose', '-v', action='store_true', help='Show detailed output for all cases')
    parser.add_argument('--failures-only', '-f', action='store_true', help='Show only failed cases')
    parser.add_argument('--context', '-c', choices=['inventory', 'tooltip', 'shop', 'look'],
                        help='Test only specific context')
    parser.add_argument('--v1-only', action='store_true', help='Test V1 only')
    parser.add_argument('--v2-only', action='store_true', help='Test V2 only')
    return parser.parse_args()


def filter_tests_by_context(tests: List[TestCase], context_name: Optional[str]) -> List[TestCase]:
    """Filter tests by context name"""
    if not context_name:
        return tests

    context_map = {
        'inventory': DisplayContext.INVENTORY,
        'tooltip': DisplayContext.TOOLTIP,
        'shop': DisplayContext.SHOP,
        'look': DisplayContext.LOOK,
    }
    target_context = context_map[context_name]
    return [t for t in tests if t.context == target_context]


def print_loaded_stats():
    """Print loaded dictionary statistics"""
    print(f"{Colors.DIM}=== 로드된 사전 통계 ==={Colors.RESET}")
    print(f"  Materials:    {len(materials)} items")
    print(f"  Qualities:    {len(qualities)} items")
    print(f"  Modifiers:    {len(modifiers)} items")
    print(f"  Species:      {len(species)} items")
    print(f"  Base Nouns:   {len(base_nouns)} items")
    print(f"  States:       {len(states)} items")
    print(f"  Liquids:      {len(liquids)} items")
    print(f"  Of Patterns:  {len(of_patterns)} items")
    print(f"  All Prefixes: {len(all_prefixes_sorted)} items")
    print()


def main():
    """Main function"""
    args = parse_args()

    print(f"{Colors.BOLD}ObjectTranslator V1 vs V2 Context Test{Colors.RESET}")
    print("Loading dictionaries...")

    load_dictionaries()
    print_loaded_stats()

    # Filter tests
    tests = filter_tests_by_context(TEST_CASES, args.context)

    # Run tests
    verifier = ComparisonVerifier()
    results = [verifier.run_test(tc) for tc in tests]

    # Generate report
    report = ReportGenerator(results, verbose=args.verbose, failures_only=args.failures_only)
    report.generate()

    # Return exit code
    all_pass = all(r.v1_matches_expected and r.v2_matches_expected and r.v1_v2_equal for r in results)
    return 0 if all_pass else 1


if __name__ == "__main__":
    sys.exit(main())
