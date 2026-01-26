#!/usr/bin/env python3
"""
ObjectTranslator 종합 테스트 스크립트

JSON 사전 파일들을 읽어서 번역 로직을 시뮬레이션하고
100개 테스트 케이스를 실행합니다.

사용법:
    cd /Users/ben/Desktop/qud_korean
    python3 tools/test_object_translator.py
"""

import json
import re
import os
from pathlib import Path
from typing import Dict, List, Tuple, Optional

# ============================================================
# 색상 코드 정의 (터미널 출력용)
# ============================================================
class Colors:
    GREEN = '\033[92m'
    RED = '\033[91m'
    YELLOW = '\033[93m'
    CYAN = '\033[96m'
    BOLD = '\033[1m'
    RESET = '\033[0m'


# ============================================================
# 전역 사전 (JSON에서 로드)
# ============================================================
materials: Dict[str, str] = {}
qualities: Dict[str, str] = {}
processing: Dict[str, str] = {}
modifiers: Dict[str, str] = {}
tonics: Dict[str, str] = {}
grenades: Dict[str, str] = {}
marks: Dict[str, str] = {}
colors: Dict[str, str] = {}
shaders: Dict[str, str] = {}

species: Dict[str, str] = {}
base_nouns: Dict[str, str] = {}

states: Dict[str, str] = {}
liquids: Dict[str, str] = {}
of_patterns: Dict[str, str] = {}
body_parts: Dict[str, str] = {}
part_suffixes: Dict[str, str] = {}

# 통합 접두사 리스트 (긴 것 우선)
all_prefixes_sorted: List[Tuple[str, str]] = []
# 컬러 태그 내 어휘 리스트
color_tag_vocab_sorted: List[Tuple[str, str]] = []
# 베이스 명사 리스트
base_nouns_sorted: List[Tuple[str, str]] = []


# ============================================================
# JSON 로드 함수들
# ============================================================
def get_project_root() -> Path:
    """프로젝트 루트 디렉토리 반환"""
    # 이 스크립트는 tools/ 폴더에 있음
    script_path = Path(__file__).resolve()
    return script_path.parent.parent


def load_json_section(data: dict, key: str, target: Dict[str, str]):
    """JSON 섹션을 사전에 로드 (주석 제외)"""
    if key not in data:
        return
    section = data[key]
    for k, v in section.items():
        if not k.startswith("_"):  # Skip comments
            target[k.lower()] = v


def load_dictionaries():
    """모든 JSON 사전 파일들을 로드"""
    global materials, qualities, processing, modifiers, tonics, grenades, marks, colors, shaders
    global species, base_nouns
    global states, liquids, of_patterns, body_parts, part_suffixes
    global all_prefixes_sorted, color_tag_vocab_sorted, base_nouns_sorted

    root = get_project_root()
    objects_path = root / "LOCALIZATION" / "OBJECTS"

    # 1. items/_common.json 로드
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
        load_json_section(data, "shaders", shaders)

    # 2. items/_nouns.json 로드
    nouns_path = objects_path / "items" / "_nouns.json"
    if nouns_path.exists():
        with open(nouns_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        for section_key in data:
            if not section_key.startswith("_"):
                load_json_section(data, section_key, base_nouns)

    # 3. creatures/_common.json 로드
    creatures_common_path = objects_path / "creatures" / "_common.json"
    if creatures_common_path.exists():
        with open(creatures_common_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        load_json_section(data, "species", species)
        # common_terms도 species에 추가 (corpse 등)
        load_json_section(data, "common_terms", species)

    # 4. _suffixes.json 로드
    suffixes_path = objects_path / "_suffixes.json"
    if suffixes_path.exists():
        with open(suffixes_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        load_json_section(data, "states", states)
        load_json_section(data, "liquids", liquids)
        load_json_section(data, "of_patterns", of_patterns)
        load_json_section(data, "body_parts", body_parts)
        load_json_section(data, "part_suffixes", part_suffixes)

    # 5. 통합 접두사 리스트 생성 (긴 것 우선)
    all_prefixes = {}
    all_prefixes.update(materials)
    all_prefixes.update(qualities)
    all_prefixes.update(processing)
    all_prefixes.update(modifiers)
    all_prefixes.update(tonics)
    all_prefixes.update(grenades)
    all_prefixes.update(colors)
    all_prefixes.update(shaders)
    all_prefixes_sorted.clear()
    all_prefixes_sorted.extend(
        sorted(all_prefixes.items(), key=lambda x: -len(x[0]))
    )

    # 6. 컬러 태그 내 어휘 리스트 생성
    color_vocab = {}
    color_vocab.update(materials)
    color_vocab.update(qualities)
    color_vocab.update(tonics)
    color_vocab.update(grenades)
    color_vocab.update(modifiers)
    color_vocab.update(shaders)
    color_vocab.update(species)
    color_vocab.update(liquids)  # "fresh water" 등의 액체 추가
    color_tag_vocab_sorted.clear()
    color_tag_vocab_sorted.extend(
        sorted(color_vocab.items(), key=lambda x: -len(x[0]))
    )

    # 7. 베이스 명사 리스트 생성 (긴 것 우선)
    base_nouns_sorted.clear()
    base_nouns_sorted.extend(
        sorted(base_nouns.items(), key=lambda x: -len(x[0]))
    )


# ============================================================
# 헬퍼 함수들
# ============================================================
def strip_color_tags(text: str) -> str:
    """색상 태그 제거: {{X|content}} -> content"""
    if not text:
        return text

    result = text
    # Remove simple color codes like &r, &W
    result = re.sub(r'&[\^]?[a-zA-Z]', '', result)

    # Remove {{...}} tags iteratively (innermost first)
    limit = 10
    while limit > 0 and '{{' in result:
        # Match innermost tags: {{tag|content}} where content has no {{ or }}
        new_result = re.sub(r'\{\{[^{}|]+\|([^{}]*)\}\}', r'\1', result)
        if new_result == result:
            break
        result = new_result
        limit -= 1

    return result


def translate_materials_in_color_tags(text: str) -> str:
    """색상 태그 내 재료 번역: {{w|bronze}} -> {{w|청동}}"""
    if not text or '{{' not in text:
        return text

    result = text

    # Step 0: Handle self-referential color tags {{word|word}}
    # These are mod adjectives like {{feathered|feathered}}
    # IMPORTANT: Preserve shader name (first part), only translate display text (second part)
    for eng, ko in all_prefixes_sorted:
        pattern = r'\{\{' + re.escape(eng) + r'\|' + re.escape(eng) + r'\}\}'
        if re.search(pattern, result, re.IGNORECASE):
            result = re.sub(pattern, '{{' + eng + '|' + ko + '}}', result, flags=re.IGNORECASE)

    # Step 0.5: Handle {{shader|shader full text}} pattern
    # e.g., {{feathered|feathered leather armor}} → {{feathered|깃털 달린 leather armor}}
    for eng, ko in all_prefixes_sorted:
        pattern = r'\{\{' + re.escape(eng) + r'\|' + re.escape(eng) + r'\s+([^{}]+)\}\}'
        def make_replacement(eng_key, ko_val):
            def replacer(m):
                return '{{' + eng_key + '|' + ko_val + ' ' + m.group(1) + '}}'
            return replacer
        result = re.sub(pattern, make_replacement(eng, ko), result, flags=re.IGNORECASE)

    # Step 1: Handle non-self-referential color tags {{shaderName|prefix}}
    # e.g., {{glittering|glitter}} → {{glittering|글리터}}
    # IMPORTANT: Shader name (first part) is NEVER translated, only display text (second part)
    for eng, ko in all_prefixes_sorted:
        pattern = r'\{\{([^|{}]+)\|' + re.escape(eng) + r'\}\}'
        def make_nonself_replacement(ko_val):
            def replacer(m):
                return '{{' + m.group(1) + '|' + ko_val + '}}'
            return replacer
        result = re.sub(pattern, make_nonself_replacement(ko), result, flags=re.IGNORECASE)

    def replace_in_tag(match):
        tag = match.group(1)
        content = match.group(2)
        # 태그 내 콘텐츠 번역 (여러 단어도 처리)
        content_lower = content.lower()

        # 정확히 일치하는 경우
        for eng, ko in color_tag_vocab_sorted:
            if content_lower == eng:
                return f'{{{{{tag}|{ko}}}}}'

        # 태그 내에 여러 단어가 있는 경우 (예: "vibro blade", "stun whip")
        # 복합 명사 먼저 확인
        for eng, ko in base_nouns_sorted:
            if content_lower == eng:
                return f'{{{{{tag}|{ko}}}}}'

        # 개별 단어 번역 시도
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

    # {{X|content}} 패턴 매칭
    result = re.sub(r'\{\{([^|]+)\|([^{}]+)\}\}', replace_in_tag, result)
    return result


def extract_and_translate_prefixes(name: str) -> Tuple[Optional[str], str]:
    """
    접두사 추출 및 번역
    "wooden arrow" → ("나무", "arrow")
    "flawless crysteel dagger" → ("완벽한 크리스틸", "dagger")
    """
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


def extract_all_suffixes(name: str) -> Tuple[str, str]:
    """
    모든 접미사 추출
    "torch x14 (unburnt)" → ("torch", " x14 (unburnt)")
    "sword of fire" → ("sword", " of fire")
    "canteen [32 drams of water] x2" → ("canteen", " [32 drams of water] x2")
    """
    if not name:
        return name, ""

    result = name
    extracted_suffixes = []

    # 1. 괄호 접미사: (lit), (unlit), (unburnt)
    paren_match = re.search(r'(\s*\([^)]+\))$', result)
    if paren_match:
        extracted_suffixes.insert(0, paren_match.group(1))
        result = result[:paren_match.start()]

    # 2. 수량 접미사 (맨 끝에서 먼저 확인): x3, x14
    # Note: 대괄호보다 먼저 확인해야 "canteen [...] x2" 처리 가능
    quantity_match = re.search(r'(\s*x\d+)$', result)
    if quantity_match:
        extracted_suffixes.insert(0, quantity_match.group(1))
        result = result[:quantity_match.start()]

    # 3. 대괄호 접미사: [empty], [full], [32 drams of water]
    bracket_match = re.search(r'(\s*\[[^\]]+\])$', result)
    if bracket_match:
        extracted_suffixes.insert(0, bracket_match.group(1))
        result = result[:bracket_match.start()]

    # 4. 추가 수량 접미사 (대괄호 앞에 있을 수도 있음)
    quantity_match2 = re.search(r'(\s*x\d+)$', result)
    if quantity_match2:
        extracted_suffixes.insert(0, quantity_match2.group(1))
        result = result[:quantity_match2.start()]

    # 5. "+X" 접미사: +1, +2, -1
    plus_match = re.search(r'(\s*[+-]\d+)$', result)
    if plus_match:
        extracted_suffixes.insert(0, plus_match.group(1))
        result = result[:plus_match.start()]

    # 6. "of X" 접미사 (대괄호 안의 "of"는 이미 추출됨)
    of_match = re.search(r"(\s+of\s+[\w\s\-']+)$", result, re.IGNORECASE)
    if of_match:
        extracted_suffixes.insert(0, of_match.group(1))
        result = result[:of_match.start()]

    return result.strip(), "".join(extracted_suffixes)


def translate_all_suffixes(suffixes: str) -> str:
    """접미사 번역"""
    if not suffixes:
        return ""

    result = suffixes

    # 상태 번역
    for eng, ko in states.items():
        if eng.lower() in result.lower():
            result = re.sub(re.escape(eng), ko, result, flags=re.IGNORECASE)

    # [X drams of Y] 패턴
    def drams_replace(m):
        amount = m.group(1)
        liquid = m.group(2).strip()
        liquid_stripped = strip_color_tags(liquid).lower()
        liquid_ko = liquids.get(liquid_stripped, liquid)
        return f"[{liquid_ko} {amount}드램]"

    result = re.sub(r'\[(\d+) drams? of ([^\]]+)\]', drams_replace, result, flags=re.IGNORECASE)

    # [X servings] pattern -> [X인분]
    result = re.sub(r'\[(\d+) servings?\]', r'[\1인분]', result, flags=re.IGNORECASE)

    # "of X" 패턴
    def of_replace(m):
        element = m.group(1).strip()
        # 전체 패턴 먼저 확인
        full_pattern = f"of {element}".lower()
        if full_pattern in of_patterns:
            return of_patterns[full_pattern]
        # 원소만 확인
        element_ko = of_patterns.get(element.lower(), element)
        if element_ko != element:
            return f"의 {element_ko}"
        return m.group(0)

    result = re.sub(r"\s+of\s+([\w\s\-']+)$", of_replace, result, flags=re.IGNORECASE)

    return result


def try_get_creature_translation(creature_name: str) -> Optional[str]:
    """생물 번역 시도"""
    return species.get(creature_name.lower())


def try_get_item_translation(item_name: str) -> Optional[str]:
    """아이템 번역 시도"""
    return base_nouns.get(item_name.lower())


def translate_nouns_in_text(text: str) -> str:
    """텍스트 내 명사 번역"""
    result = text
    for eng, ko in base_nouns_sorted:
        pattern = r'\b' + re.escape(eng) + r'\b'
        result = re.sub(pattern, ko, result, flags=re.IGNORECASE)
    return result


def translate_prefixes_in_text(text: str) -> str:
    """텍스트 내 접두사 번역"""
    result = text
    for eng, ko in all_prefixes_sorted:
        pattern = r'\b' + re.escape(eng) + r'\b'
        result = re.sub(pattern, ko, result, flags=re.IGNORECASE)
    return result


# ============================================================
# 패턴별 번역 함수들
# ============================================================
def try_translate_corpse(original_name: str) -> Optional[str]:
    """시체 패턴 번역: "{creature} corpse" -> "{creature_ko} 시체" """
    stripped = strip_color_tags(original_name)
    if not stripped.lower().endswith(" corpse"):
        return None

    creature_part = stripped[:-len(" corpse")]
    creature_ko = try_get_creature_translation(creature_part)
    if creature_ko:
        return f"{creature_ko} 시체"
    return None


def try_translate_dynamic_food(original_name: str) -> Optional[str]:
    """동적 식품 패턴 번역"""
    stripped = strip_color_tags(original_name)

    # jerky 패턴
    if stripped.lower().endswith(" jerky"):
        creature_part = stripped[:-len(" jerky")]
        creature_ko = try_get_creature_translation(creature_part)
        if creature_ko:
            return f"{creature_ko} 육포"

    # meat 패턴
    if stripped.lower().endswith(" meat"):
        creature_part = stripped[:-len(" meat")]
        # "cooked meat" 특수 처리
        if creature_part.lower() == "cooked":
            return "조리된 고기"
        creature_ko = try_get_creature_translation(creature_part)
        if creature_ko:
            return f"{creature_ko} 고기"

    # haunch 패턴
    if stripped.lower().endswith(" haunch"):
        creature_part = stripped[:-len(" haunch")]
        creature_ko = try_get_creature_translation(creature_part)
        if creature_ko:
            return f"{creature_ko} 넓적다리"

    # "preserved X" 패턴
    if stripped.lower().startswith("preserved "):
        ingredient_part = stripped[len("preserved "):]
        creature_ko = try_get_creature_translation(ingredient_part)
        if creature_ko:
            return f"절임 {creature_ko}"

    return None


def try_translate_dynamic_parts(original_name: str) -> Optional[str]:
    """동적 부위 패턴 번역: "{creature} {part}" -> "{creature_ko} {part_ko}" """
    stripped = strip_color_tags(original_name)

    # part_suffixes에서 패턴 검색
    for eng_suffix, ko_suffix in sorted(part_suffixes.items(), key=lambda x: -len(x[0])):
        if stripped.lower().endswith(eng_suffix.lower()):
            creature_part = stripped[:len(stripped) - len(eng_suffix)]

            # "elder" 접두사 처리
            if creature_part.lower().startswith("elder "):
                actual_creature = creature_part[len("elder "):]
                creature_ko = try_get_creature_translation(actual_creature)
                if creature_ko:
                    # 공백 추가하여 "장로 곰 두개골" 형태로
                    return f"장로 {creature_ko} {ko_suffix.strip()}"

            # "raw" 접두사 처리
            if creature_part.lower().startswith("raw "):
                actual_creature = creature_part[len("raw "):]
                creature_ko = try_get_creature_translation(actual_creature)
                if creature_ko:
                    return f"생 {creature_ko} {ko_suffix.strip()}"

            creature_ko = try_get_creature_translation(creature_part)
            if creature_ko:
                # 공백 추가하여 "곰 알" 형태로
                return f"{creature_ko} {ko_suffix.strip()}"

    return None


def try_translate_possessive(original_name: str) -> Optional[str]:
    """소유격 패턴 번역: "{creature}'s {part}" -> "{creature_ko}의 {part_ko}" """
    stripped = strip_color_tags(original_name)

    match = re.match(r"^(.+)'s\s+(.+)$", stripped, re.IGNORECASE)
    if not match:
        return None

    creature = match.group(1).strip()
    part = match.group(2).strip()

    creature_ko = try_get_creature_translation(creature)
    if not creature_ko:
        return None

    # 부위 번역 시도
    part_ko = body_parts.get(part.lower()) or base_nouns.get(part.lower())
    if part_ko:
        return f"{creature_ko}의 {part_ko}"

    return None


def try_translate_of_pattern(original_name: str) -> Optional[str]:
    """of 패턴 번역: "X of Y" -> 한국어 어순 변환

    규칙:
    - 단순: "sword of fire" → "불의 검"
    - 접두사 포함: "flawless crysteel sword of fire" → "완벽한 크리스틸 불의 검"
    """
    stripped = strip_color_tags(original_name)

    match = re.match(r"^(.+?)\s+of\s+(?:the\s+)?(.+)$", stripped, re.IGNORECASE)
    if not match:
        return None

    item_part = match.group(1).strip()  # "sword" 또는 "flawless crysteel sword"
    of_part = match.group(2).strip()    # "fire" 또는 "river-wives"

    # of_patterns 사전에서 전체 패턴 확인
    of_ko = None
    full_pattern_with_the = f"of the {of_part}".lower()
    full_pattern = f"of {of_part}".lower()

    if full_pattern_with_the in of_patterns:
        # 이미 "~의" 형태로 저장됨 (예: "강 아내들의")
        of_ko = of_patterns[full_pattern_with_the]
    elif full_pattern in of_patterns:
        of_ko = of_patterns[full_pattern]
    elif of_part.lower() in of_patterns:
        # 원소만 있는 경우 (예: "fire" -> "불")
        of_ko = f"{of_patterns[of_part.lower()]}의"
    else:
        # 사전에 없으면 기본 번역 시도
        of_part_translated = translate_nouns_in_text(of_part)
        of_part_translated = translate_prefixes_in_text(of_part_translated)
        if of_part_translated != of_part:
            of_ko = f"{of_part_translated}의"
        else:
            return None

    # 아이템 부분 번역 - 접두사와 베이스 분리
    prefix_ko, remainder = extract_and_translate_prefixes(item_part)

    if prefix_ko:
        # 추가 접두사(재료) 추출 시도
        material_ko, base_only = extract_and_translate_prefixes(remainder)
        if material_ko:
            base_ko = try_get_item_translation(base_only)
            if base_ko:
                # "완벽한 크리스틸 불의 검" 형태
                return f"{prefix_ko} {material_ko} {of_ko} {base_ko}".strip()

        base_ko = try_get_item_translation(remainder)
        if base_ko:
            # "완벽한 불의 검" 형태 (접두사 + of_ko + base)
            return f"{prefix_ko} {of_ko} {base_ko}".strip()
        else:
            base_ko = translate_nouns_in_text(remainder)
            if base_ko != remainder:
                return f"{prefix_ko} {of_ko} {base_ko}".strip()

    # 접두사 없는 경우: "불의 검"
    item_ko = try_get_item_translation(item_part)
    if not item_ko:
        item_ko = translate_nouns_in_text(item_part)
        item_ko = translate_prefixes_in_text(item_ko)

    return f"{of_ko} {item_ko}".strip()


# ============================================================
# 메인 번역 함수
# ============================================================
def restore_color_tags(original: str, translated: str) -> str:
    """원본의 색상 태그 구조를 번역된 텍스트에 복원

    original: 번역된 태그를 포함한 문자열 (e.g., "{{깃털 달린|깃털 달린}} leather armor")
    translated: 태그 없이 번역된 문자열 (e.g., "깃털 달린 가죽 갑옷")
    """
    # 원본에 태그가 없으면 그대로 반환
    if '{{' not in original:
        return translated

    # 원본이 전체가 태그로 감싸진 경우: {{tag|content}}
    full_tag_match = re.match(r'^\{\{([^|]+)\|(.+)\}\}$', original)
    if full_tag_match:
        tag = full_tag_match.group(1)
        return f'{{{{{tag}|{translated}}}}}'

    # 원본의 첫 번째 태그를 찾아서 구조 파악
    tag_match = re.search(r'\{\{([^|]+)\|([^{}]+)\}\}', original)
    if tag_match:
        tag = tag_match.group(1)
        content = tag_match.group(2)
        rest_of_original = original[tag_match.end():].strip()

        # 태그 내용이 번역 결과에 있는지 확인 (이미 한글일 수 있음)
        if content in translated:
            # 태그 내용을 태그로 감싸기
            tagged_content = f'{{{{{tag}|{content}}}}}'
            result = translated.replace(content, tagged_content, 1)
            return result

        # 태그 내용이 영어인 경우: 번역 찾기
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
            # 번역된 내용을 태그로 감싸기
            tagged_content = f'{{{{{tag}|{content_translated}}}}}'
            result = translated.replace(content_translated, tagged_content, 1)
            return result

    return translated


def try_translate(original_name: str) -> Tuple[bool, str]:
    """
    ObjectTranslator.TryGetDisplayName 로직 시뮬레이션

    Returns:
        (success: bool, translated: str)
    """
    if not original_name:
        return False, original_name

    # 1. 색상 태그 내 재료 번역
    with_translated_tags = translate_materials_in_color_tags(original_name)

    # 색상 태그 정보 보존을 위해 저장
    has_color_tags = '{{' in original_name

    # EARLY CHECK 1: 색상 태그 내 복합어가 번역된 경우, 태그 외부만 번역하여 반환
    # 예: {{G|fresh water}} injector → {{G|신선한 물}} 주사기
    # 이 경우 prefix extraction이 "fresh water"를 분리하므로 먼저 처리
    # 단, 대괄호 안에 색상 태그가 있는 경우 (예: [32 drams of {{G|fresh water}}])는 제외
    # 이 경우 suffix 번역이 별도로 처리해야 함
    has_color_tag_in_bracket = re.search(r'\[[^\]]*\{\{[^}]+\}\}[^\]]*\]', original_name)
    if has_color_tags and with_translated_tags != original_name and not has_color_tag_in_bracket:
        # 태그 외부 부분만 번역
        def translate_outside_tags_early(text):
            parts = re.split(r'(\{\{[^}]+\}\})', text)
            translated_parts = []
            for part in parts:
                if part.startswith('{{'):
                    translated_parts.append(part)
                else:
                    translated = translate_nouns_in_text(part)
                    translated = translate_prefixes_in_text(translated)
                    translated_parts.append(translated)
            return ''.join(translated_parts)

        early_result = translate_outside_tags_early(with_translated_tags)
        if early_result != original_name:
            return True, early_result

    # EARLY CHECK 2: "of" 패턴 - 접미사 추출 전에 먼저 확인
    # "of" 패턴은 특수한 어순 변환이 필요하므로 일반 접미사로 처리하면 안됨
    # 단, 다음 패턴은 제외:
    # - "[... drams of ...]" 패턴 (이건 drams 패턴으로 처리)
    # - "of"가 대괄호 안에 있는 경우 (bracket suffix 내부)
    # Strip ORIGINAL (not translated) for prefix matching - need English keys!
    stripped = strip_color_tags(original_name)
    has_of_pattern = ' of ' in stripped.lower()
    is_drams_pattern = 'drams of' in stripped.lower()
    is_in_bracket = re.search(r'\[[^\]]*of[^\]]*\]', stripped.lower())

    if has_of_pattern and not is_drams_pattern and not is_in_bracket:
        of_result = try_translate_of_pattern(original_name)
        if of_result:
            return True, of_result

    # 2. 접미사 추출 (색상 태그 제거 후)
    base_name, all_suffixes = extract_all_suffixes(stripped)

    # 3. 접두사 추출 및 번역
    prefix_ko, remainder = extract_and_translate_prefixes(base_name)

    if prefix_ko:
        # 베이스 아이템 번역 시도
        base_ko = try_get_item_translation(remainder) or try_get_creature_translation(remainder)
        if base_ko:
            suffix_ko = translate_all_suffixes(all_suffixes)
            result = f"{prefix_ko} {base_ko}{suffix_ko}"
            # 색상 태그 복원
            if has_color_tags:
                result = restore_color_tags(with_translated_tags, result)
            return True, result

        # 추가 접두사(재료) 추출 시도
        material_ko, base_only = extract_and_translate_prefixes(remainder)
        if material_ko:
            base_ko = try_get_item_translation(base_only) or try_get_creature_translation(base_only)
            if base_ko:
                suffix_ko = translate_all_suffixes(all_suffixes)
                result = f"{prefix_ko} {material_ko} {base_ko}{suffix_ko}"
                if has_color_tags:
                    result = restore_color_tags(with_translated_tags, result)
                return True, result

        # Try extracting base noun from end of remainder
        # Pattern: "<creature_or_modifier> <base_noun>" e.g., "진눈깨비수염 gland paste"
        for eng, ko in base_nouns_sorted:
            if remainder.lower().endswith(' ' + eng.lower()):
                modifier_part = remainder[:-len(eng)-1].strip()  # "진눈깨비수염" or "bear"
                # Try translating modifier as creature
                modifier_ko = try_get_creature_translation(modifier_part)
                if modifier_ko is None:
                    modifier_ko = modifier_part  # Keep as-is if not a known creature
                suffix_ko = translate_all_suffixes(all_suffixes)
                result = f"{prefix_ko} {modifier_ko} {ko}{suffix_ko}"
                if has_color_tags:
                    result = restore_color_tags(with_translated_tags, result)
                return True, result

    # 4. 베이스 아이템 직접 번역 시도
    base_ko = try_get_item_translation(base_name) or try_get_creature_translation(base_name)
    if base_ko:
        suffix_ko = translate_all_suffixes(all_suffixes)
        result = f"{base_ko}{suffix_ko}" if suffix_ko else base_ko
        if has_color_tags:
            result = restore_color_tags(with_translated_tags, result)
        return True, result

    # 5. 시체 패턴
    corpse_result = try_translate_corpse(original_name)
    if corpse_result:
        return True, corpse_result

    # 6. 동적 식품 패턴
    food_result = try_translate_dynamic_food(original_name)
    if food_result:
        return True, food_result

    # 7. 동적 부위 패턴 (with suffix support)
    # 부위 패턴에서 접미사 처리
    parts_stripped = strip_color_tags(original_name)
    parts_base, parts_suffix = extract_all_suffixes(parts_stripped)
    parts_result = try_translate_dynamic_parts(parts_base)
    if parts_result:
        suffix_ko = translate_all_suffixes(parts_suffix)
        return True, f"{parts_result}{suffix_ko}"

    # 8. of 패턴
    of_result = try_translate_of_pattern(original_name)
    if of_result:
        return True, of_result

    # 9. 소유격 패턴
    possessive_result = try_translate_possessive(original_name)
    if possessive_result:
        return True, possessive_result

    # 10. 색상 태그 내 번역 + 외부 명사 번역 (태그 보존)
    if with_translated_tags != original_name:
        # 태그 외부 부분만 번역
        result = with_translated_tags
        # 태그 외부의 명사 번역
        def translate_outside_tags(text):
            # 태그 부분과 비태그 부분 분리
            parts = re.split(r'(\{\{[^}]+\}\})', text)
            translated_parts = []
            for part in parts:
                if part.startswith('{{'):
                    translated_parts.append(part)
                else:
                    translated = translate_nouns_in_text(part)
                    translated = translate_prefixes_in_text(translated)
                    translated_parts.append(translated)
            return ''.join(translated_parts)

        result = translate_outside_tags(result)
        if result != with_translated_tags:
            return True, result
        # 태그 내부만 번역된 경우도 성공
        if result != original_name:
            return True, result

    # 11. 베이스 명사만 번역
    result = translate_nouns_in_text(original_name)
    if result != original_name:
        return True, result

    return False, original_name


# ============================================================
# 테스트 케이스 정의
# ============================================================
TEST_CASES = [
    # === 1. 단순 직접 번역 (10개) ===
    (1, "mace", "메이스"),
    (2, "dagger", "단검"),
    (3, "sword", "검"),
    (4, "torch", "횃불"),
    (5, "canteen", "수통"),
    (6, "helmet", "투구"),
    (7, "boots", "부츠"),
    (8, "cloak", "망토"),
    (9, "injector", "주사기"),
    (10, "grenade", "수류탄"),

    # === 2. 단일 접두사 (10개) ===
    (11, "bronze mace", "청동 메이스"),
    (12, "iron dagger", "철 단검"),
    (13, "crysteel sword", "크리스틸 검"),
    (14, "leather boots", "가죽 부츠"),
    (15, "wooden staff", "나무 지팡이"),
    (16, "flawless helm", "완벽한 투구"),
    (17, "masterwork armor", "명품 갑옷"),
    (18, "rusted blade", "녹슨 블레이드"),
    (19, "frozen axe", "얼어붙은 도끼"),
    (20, "vibro dagger", "바이브로 단검"),

    # === 3. 복합 접두사 (10개) ===
    (21, "engraved bronze mace", "새겨진 청동 메이스"),
    (22, "folded carbide dagger", "접힌 카바이드 단검"),
    (23, "flawless crysteel sword", "완벽한 크리스틸 검"),
    (24, "two-handed iron hammer", "양손 철 해머"),
    (25, "studded leather armor", "징박힌 가죽 갑옷"),
    (26, "reinforced steel shield", "강화된 강철 방패"),
    (27, "polished obsidian blade", "광택나는 흑요석 블레이드"),
    (28, "gilded bronze helm", "금박입힌 청동 투구"),
    (29, "high explosive grenade", "고폭 수류탄"),
    (30, "electric vibro whip", "전기 바이브로 채찍"),

    # === 4. 상태 접미사 (10개) ===
    (31, "torch (lit)", "횃불 (점화됨)"),
    (32, "torch (unburnt)", "횃불 (미사용)"),
    (33, "waterskin [empty]", "물주머니 [비어있음]"),
    (34, "canteen [full]", "수통 [가득 참]"),
    (35, "arrow x15", "화살 x15"),
    (36, "grenade x3", "수류탄 x3"),
    (37, "musket [loaded]", "머스킷 [장전됨]"),
    (38, "torch x14 (unburnt)", "횃불 x14 (미사용)"),
    (39, "arrow x100 [empty]", "화살 x100 [비어있음]"),
    (40, "dagger +3", "단검 +3"),

    # === 5. drams 패턴 (5개) ===
    (41, "canteen [32 drams of water]", "수통 [물 32드램]"),
    (42, "waterskin [16 drams of fresh water]", "물주머니 [신선한 물 16드램]"),
    (43, "canteen [8 drams of acid]", "수통 [산 8드램]"),
    (44, "waterskin [64 drams of honey]", "물주머니 [꿀 64드램]"),
    # Note: Color tags inside drams may be preserved or stripped depending on context
    (45, "canteen [32 drams of {{G|fresh water}}]", "수통 [신선한 물 32드램] 또는 수통 [{{G|신선한 물}} 32드램]"),

    # === 6. 컬러 태그 (10개) ===
    (46, "{{w|bronze}} mace", "{{w|청동}} 메이스"),
    (47, "{{B|carbide}} dagger", "{{B|카바이드}} 단검"),
    (48, "{{c|crysteel}} sword", "{{c|크리스틸}} 검"),
    (49, "{{G|hulk}} honey injector", "{{G|헐크}} 꿀 주사기"),
    (50, "{{r|flaming}} torch", "{{r|불타는}} 횃불"),
    # Note: Nested color tags are complex edge cases
    (51, "{{K|{{crysteel|crysteel}}}} blade", "{{K|{{crysteel|크리스틸}}}} 블레이드 또는 {{crysteel|크리스틸}} 블레이드 또는 {{K|{{크리스틸|크리스틸}}}} 블레이드 또는 {{크리스틸|크리스틸}} 블레이드"),
    (52, "{{c|vibro blade}}", "{{c|바이브로 블레이드}}"),
    (53, "{{c|stun whip}}", "{{c|기절 채찍}}"),
    (54, "{{G|fresh water}} injector", "{{G|신선한 물}} 주사기"),
    (55, "{{g|seed-spitting vine}}", "{{g|씨앗발사 덩굴}}"),

    # === 7. 동적 식품 패턴 (8개) ===
    (56, "bear jerky", "곰 육포"),
    (57, "pig meat", "돼지 고기"),
    (58, "boar haunch", "멧돼지 넓적다리"),
    (59, "preserved bear", "절임 곰"),
    (60, "cooked meat", "조리된 고기"),
    (61, "snapjaw jerky", "스냅조 육포"),
    (62, "saltback meat", "소금등 고기"),
    (63, "basilisk jerky", "바실리스크 육포"),

    # === 8. 동적 부위 패턴 (8개) ===
    (64, "bear egg", "곰 알"),
    (65, "wolf hide", "늑대 가죽"),
    (66, "bear bone", "곰 뼈"),
    (67, "snapjaw skull", "스냅조 두개골"),
    (68, "raw bear hide", "생 곰 가죽"),
    (69, "elder bear skull", "장로 곰 두개골"),
    (70, "scorpion egg", "전갈 알"),
    (71, "centipede scale", "지네 비늘"),

    # === 9. 소유격 패턴 (5개) ===
    (72, "panther's claw", "표범의 발톱"),
    (73, "bear's fang", "곰의 송곳니"),
    (74, "wolf's hide", "늑대의 가죽"),
    (75, "snapjaw's tooth", "스냅조의 이빨"),
    (76, "basilisk's eye", "바실리스크의 눈"),

    # === 10. of 패턴 (5개) ===
    (77, "sword of fire", "불의 검"),
    (78, "sandals of the river-wives", "강 아내들의 샌들"),
    (79, "banner of the Holy Rhombus", "성스러운 마름모의 깃발"),
    (80, "boots of ice", "얼음의 부츠"),
    (81, "dagger of lightning", "번개의 단검"),

    # === 11. 시체 패턴 (3개) ===
    (82, "bear corpse", "곰 시체"),
    (83, "snapjaw corpse", "스냅조 시체"),
    (84, "basilisk corpse", "바실리스크 시체"),

    # === 12. 신규 추가 항목 (10개) ===
    (85, "agate tube", "마노 튜브"),
    (86, "amethyst tube", "자수정 튜브"),
    (87, "bloated saltback", "부풀어오른 소금등"),
    (88, "chitinous puma", "키틴질의 퓨마"),  # 또는 "키틴질 퓨마"
    (89, "mopango corpse", "모팡고 시체"),
    (90, "cherub egg", "케루브 알"),
    (91, "barathrumite jerky", "바라스룸인 육포"),
    (92, "plasma rifle", "플라즈마 라이플"),
    (93, "eigen pistol", "아이겐 권총"),
    (94, "nullray cannon", "널레이 대포"),

    # === 13. 복합 케이스 (6개) ===
    (95, "engraved bronze mace +3", "새겨진 청동 메이스 +3"),
    (96, "{{w|bronze}} dagger x15", "{{w|청동}} 단검 x15"),
    (97, "flawless crysteel sword of fire", "완벽한 크리스틸 불의 검"),
    # Note: Complex pattern with color tags in drams + quantity
    (98, "canteen [32 drams of {{G|fresh water}}] x2", "수통 [신선한 물 32드램] x2 또는 수통 [{{G|신선한 물}} 32드램] x2"),
    (99, "elder bear skull x3", "장로 곰 두개골 x3"),
    (100, "{{c|vibro blade}} +2", "{{c|바이브로 블레이드}} +2"),

    # === 14. 자기참조 색상태그 (모드 접두사) ===
    # IMPORTANT: Shader name preserved, only display text translated
    (101, "{{feathered|feathered}} leather armor", "{{feathered|깃털 달린}} 가죽 갑옷"),
    (102, "{{feathered|feathered}} boots", "{{feathered|깃털 달린}} 부츠"),
    (103, "{{spiked|spiked}} leather armor", "{{spiked|가시 달린}} 가죽 갑옷"),
    (104, "{{lanterned|lanterned}} helmet", "{{lanterned|랜턴 달린}} 투구"),

    # === 15. 생물 이름 포함 패턴 (이미 한글화된 생물명 + 베이스 명사) ===
    (105, "concentrated 진눈깨비수염 gland paste [1 serving]", "농축된 진눈깨비수염 분비샘 페이스트 [1인분]"),

    # === 16. 신규 셰이더 테스트 (전수조사 추가) ===
    # 셰이더 이름 보존, 표시 텍스트 + 명사 번역
    (106, "{{gaslight|gaslight}} kris", "{{gaslight|가스라이트}} 크리스"),
    (107, "{{metachrome|metachrome}} sword", "{{metachrome|메타크롬}} 검"),
    (108, "{{lava|lava}} weep", "{{lava|용암}} weep"),
    (109, "{{syphon|syphon}} baton", "{{syphon|사이펀}} 경찰봉"),

    # === 17. 비자기참조 색상태그 ===
    # {{shaderName|displayText}} 패턴: 셰이더 이름 보존, 표시 텍스트만 번역
    (110, "{{glittering|glitter}} grenade mk I", "{{glittering|글리터}} 수류탄 mk I"),
    (111, "{{shimmering|crysteel}} sword", "{{shimmering|크리스틸}} 검"),
]


# ============================================================
# 테스트 실행
# ============================================================
def run_tests():
    """모든 테스트 케이스 실행"""
    print(f"\n{Colors.BOLD}{'=' * 60}")
    print("=== ObjectTranslator 테스트 결과 ===")
    print(f"{'=' * 60}{Colors.RESET}\n")

    passed = 0
    failed = 0
    failed_cases = []

    for num, input_text, expected in TEST_CASES:
        success, result = try_translate(input_text)

        # 다중 허용값 처리 (또는으로 구분된 경우)
        expected_options = [e.strip() for e in expected.split("또는")]
        is_pass = result in expected_options

        if is_pass:
            passed += 1
            print(f"{Colors.GREEN}[PASS]{Colors.RESET} #{num}: {input_text} → {result}")
        else:
            failed += 1
            failed_cases.append((num, input_text, expected, result))
            print(f"{Colors.RED}[FAIL]{Colors.RESET} #{num}: {input_text}")
            print(f"       Expected: {Colors.CYAN}{expected}{Colors.RESET}")
            print(f"       Got:      {Colors.YELLOW}{result}{Colors.RESET}")

    # 요약
    total = len(TEST_CASES)
    success_rate = (passed / total) * 100 if total > 0 else 0

    print(f"\n{Colors.BOLD}{'=' * 60}")
    print("=== 요약 ===")
    print(f"{'=' * 60}{Colors.RESET}")
    print(f"Total:        {total}")
    print(f"Pass:         {Colors.GREEN}{passed}{Colors.RESET}")
    print(f"Fail:         {Colors.RED}{failed}{Colors.RESET}")
    print(f"Success Rate: {success_rate:.1f}%")

    if failed_cases:
        print(f"\n{Colors.BOLD}=== 실패 케이스 목록 ==={Colors.RESET}")
        for num, input_text, expected, got in failed_cases:
            print(f"  #{num}: {input_text}")
            print(f"        Expected: {expected}")
            print(f"        Got:      {got}")

    return passed, failed


def print_loaded_stats():
    """로드된 사전 통계 출력"""
    print(f"\n{Colors.BOLD}=== 로드된 사전 통계 ==={Colors.RESET}")
    print(f"  Materials:    {len(materials)} items")
    print(f"  Qualities:    {len(qualities)} items")
    print(f"  Processing:   {len(processing)} items")
    print(f"  Modifiers:    {len(modifiers)} items")
    print(f"  Colors:       {len(colors)} items")
    print(f"  Shaders:      {len(shaders)} items")
    print(f"  Species:      {len(species)} items")
    print(f"  Base Nouns:   {len(base_nouns)} items")
    print(f"  States:       {len(states)} items")
    print(f"  Liquids:      {len(liquids)} items")
    print(f"  Of Patterns:  {len(of_patterns)} items")
    print(f"  Body Parts:   {len(body_parts)} items")
    print(f"  Part Suffixes:{len(part_suffixes)} items")
    print(f"  All Prefixes: {len(all_prefixes_sorted)} items (sorted)")
    print(f"  Color Vocab:  {len(color_tag_vocab_sorted)} items (sorted)")


def main():
    """메인 함수"""
    print(f"{Colors.BOLD}ObjectTranslator 종합 테스트{Colors.RESET}")
    print("JSON 사전 파일들을 로드하고 번역 로직을 테스트합니다.\n")

    # 사전 로드
    print("JSON 사전 로드 중...")
    load_dictionaries()
    print_loaded_stats()

    # 테스트 실행
    passed, failed = run_tests()

    # 종료 코드
    return 0 if failed == 0 else 1


if __name__ == "__main__":
    exit(main())
