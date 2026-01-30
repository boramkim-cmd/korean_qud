#!/usr/bin/env python3
"""
번역 진단 도구 (Translation Diagnosis Tool)

"왜 번역이 안 됐는지" 자동 진단합니다.
파이프라인 각 단계를 시뮬레이션하고 실패 원인을 분류합니다.

사용법:
    python3 tools/diagnose_translation.py "bronze mace"
    python3 tools/diagnose_translation.py "bronze mace" --json
    python3 tools/diagnose_translation.py --batch "bronze mace" "sword of fire" "unknown thing"
"""

import sys
import json
import re
import argparse
from pathlib import Path
from typing import Dict, List, Tuple, Optional

# ============================================================
# 분류 체계
# ============================================================
class Diagnosis:
    ALREADY_KOREAN = "ALREADY_KOREAN"        # 이미 한글
    VOCABULARY_MISSING = "VOCABULARY_MISSING"  # 어휘 사전에 없음
    PATTERN_UNSUPPORTED = "PATTERN_UNSUPPORTED"  # 패턴 미지원
    LOGIC_BUG = "LOGIC_BUG"                  # 로직 버그 가능성
    RANDOM_GENERATED = "RANDOM_GENERATED"    # 랜덤 생성 이름
    TRANSLATION_SUCCESS = "TRANSLATION_SUCCESS"  # 번역 성공


# ============================================================
# 사전 로드 (test_object_translator.py 로직 재사용)
# ============================================================
def get_project_root() -> Path:
    return Path(__file__).resolve().parent.parent


def load_json_section(data: dict, key: str, target: Dict[str, str]):
    if key not in data:
        return
    section = data[key]
    for k, v in section.items():
        if not k.startswith("_") and isinstance(v, str):
            target[k.lower()] = v


class TranslationContext:
    """번역에 필요한 모든 사전 데이터"""

    def __init__(self):
        self.materials: Dict[str, str] = {}
        self.qualities: Dict[str, str] = {}
        self.processing: Dict[str, str] = {}
        self.modifiers: Dict[str, str] = {}
        self.tonics: Dict[str, str] = {}
        self.grenades: Dict[str, str] = {}
        self.marks: Dict[str, str] = {}
        self.colors: Dict[str, str] = {}
        self.shaders: Dict[str, str] = {}
        self.species: Dict[str, str] = {}
        self.base_nouns: Dict[str, str] = {}
        self.states: Dict[str, str] = {}
        self.liquids: Dict[str, str] = {}
        self.of_patterns: Dict[str, str] = {}
        self.body_parts: Dict[str, str] = {}
        self.part_suffixes: Dict[str, str] = {}
        self.all_prefixes_sorted: List[Tuple[str, str]] = []
        self.base_nouns_sorted: List[Tuple[str, str]] = []

    def load(self):
        root = get_project_root()
        objects_path = root / "LOCALIZATION" / "OBJECTS"

        # items/_common.json
        common_path = objects_path / "items" / "_common.json"
        if common_path.exists():
            with open(common_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
            load_json_section(data, "materials", self.materials)
            load_json_section(data, "qualities", self.qualities)
            load_json_section(data, "processing", self.processing)
            load_json_section(data, "modifiers", self.modifiers)
            load_json_section(data, "tonics", self.tonics)
            load_json_section(data, "grenades", self.grenades)
            load_json_section(data, "marks", self.marks)
            load_json_section(data, "colors", self.colors)
            load_json_section(data, "shaders", self.shaders)

        # items/_nouns.json
        nouns_path = objects_path / "items" / "_nouns.json"
        if nouns_path.exists():
            with open(nouns_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
            for section_key in data:
                if not section_key.startswith("_"):
                    load_json_section(data, section_key, self.base_nouns)

        # creatures/_common.json
        creatures_common_path = objects_path / "creatures" / "_common.json"
        if creatures_common_path.exists():
            with open(creatures_common_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
            load_json_section(data, "species", self.species)
            load_json_section(data, "common_terms", self.species)

        # _suffixes.json
        suffixes_path = objects_path / "_suffixes.json"
        if suffixes_path.exists():
            with open(suffixes_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
            load_json_section(data, "states", self.states)
            load_json_section(data, "liquids", self.liquids)
            load_json_section(data, "of_patterns", self.of_patterns)
            load_json_section(data, "body_parts", self.body_parts)
            load_json_section(data, "part_suffixes", self.part_suffixes)

        # 정렬된 리스트 생성
        all_prefixes = {}
        all_prefixes.update(self.materials)
        all_prefixes.update(self.qualities)
        all_prefixes.update(self.processing)
        all_prefixes.update(self.modifiers)
        all_prefixes.update(self.tonics)
        all_prefixes.update(self.grenades)
        all_prefixes.update(self.colors)
        all_prefixes.update(self.shaders)
        self.all_prefixes_sorted = sorted(all_prefixes.items(), key=lambda x: -len(x[0]))
        self.base_nouns_sorted = sorted(self.base_nouns.items(), key=lambda x: -len(x[0]))


# ============================================================
# 진단 엔진
# ============================================================
def strip_color_tags(text: str) -> str:
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


def diagnose(name: str, ctx: TranslationContext) -> dict:
    """
    번역 파이프라인을 시뮬레이션하고 각 단계의 결과를 기록합니다.

    Returns:
        {
            "input": str,
            "diagnosis": str,  # Diagnosis 상수
            "result": str | None,
            "steps": [{"step": str, "status": "pass"|"fail"|"skip", "detail": str}],
            "suggestion": str
        }
    """
    steps = []
    result = {"input": name, "steps": steps}

    # Step 0: 이미 한글인지 확인
    if re.search(r'[\uac00-\ud7af]', name) and not re.search(r'[a-zA-Z]', name):
        steps.append({"step": "한글 확인", "status": "pass", "detail": "이미 완전한 한글"})
        result["diagnosis"] = Diagnosis.ALREADY_KOREAN
        result["result"] = name
        result["suggestion"] = "번역 불필요"
        return result

    stripped = strip_color_tags(name)
    has_color_tags = '{{' in name

    if has_color_tags:
        steps.append({"step": "컬러태그 감지", "status": "pass", "detail": f"태그 제거 후: '{stripped}'"})
    else:
        steps.append({"step": "컬러태그 감지", "status": "skip", "detail": "태그 없음"})

    # Step 1: 접미사 추출
    base_name = stripped
    suffixes = ""
    # 괄호
    paren_match = re.search(r'(\s*\([^)]+\))$', base_name)
    if paren_match:
        suffixes = paren_match.group(1) + suffixes
        base_name = base_name[:paren_match.start()]
    # 수량
    qty_match = re.search(r'(\s*x\d+)$', base_name)
    if qty_match:
        suffixes = qty_match.group(1) + suffixes
        base_name = base_name[:qty_match.start()]
    # 대괄호
    bracket_match = re.search(r'(\s*\[[^\]]+\])$', base_name)
    if bracket_match:
        suffixes = bracket_match.group(1) + suffixes
        base_name = base_name[:bracket_match.start()]
    # +N
    plus_match = re.search(r'(\s*[+-]\d+)$', base_name)
    if plus_match:
        suffixes = plus_match.group(1) + suffixes
        base_name = base_name[:plus_match.start()]
    # of X
    of_match = re.search(r"(\s+of\s+[\w\s\-']+)$", base_name, re.IGNORECASE)
    if of_match:
        suffixes = of_match.group(1) + suffixes
        base_name = base_name[:of_match.start()]

    base_name = base_name.strip()
    if suffixes:
        steps.append({"step": "접미사 추출", "status": "pass", "detail": f"base='{base_name}', suffixes='{suffixes.strip()}'"})
    else:
        steps.append({"step": "접미사 추출", "status": "skip", "detail": "접미사 없음"})

    # Step 2: 직접 번역 (base_nouns / species)
    direct_ko = ctx.base_nouns.get(base_name.lower()) or ctx.species.get(base_name.lower())
    if direct_ko:
        steps.append({"step": "직접 번역", "status": "pass", "detail": f"'{base_name}' → '{direct_ko}'"})
        result["diagnosis"] = Diagnosis.TRANSLATION_SUCCESS
        result["result"] = direct_ko + (f" {suffixes.strip()}" if suffixes.strip() else "")
        result["suggestion"] = "정상 번역됨"
        return result
    steps.append({"step": "직접 번역", "status": "fail", "detail": f"'{base_name}' 사전에 없음"})

    # Step 3: 접두사 추출
    prefix_parts = []
    current = base_name
    found = True
    while found:
        found = False
        for eng, ko in ctx.all_prefixes_sorted:
            if current.lower().startswith(eng.lower() + " "):
                prefix_parts.append((eng, ko))
                current = current[len(eng) + 1:]
                found = True
                break

    if prefix_parts:
        prefix_str = " + ".join([f"'{e}'→'{k}'" for e, k in prefix_parts])
        steps.append({"step": "접두사 추출", "status": "pass", "detail": f"{prefix_str}, remainder='{current}'"})

        # Step 4: 나머지 베이스 번역
        remainder_ko = ctx.base_nouns.get(current.lower()) or ctx.species.get(current.lower())
        if remainder_ko:
            steps.append({"step": "베이스 번역", "status": "pass", "detail": f"'{current}' → '{remainder_ko}'"})
            prefix_ko = " ".join([k for _, k in prefix_parts])
            result["diagnosis"] = Diagnosis.TRANSLATION_SUCCESS
            result["result"] = f"{prefix_ko} {remainder_ko}"
            result["suggestion"] = "정상 번역됨"
            return result
        else:
            steps.append({"step": "베이스 번역", "status": "fail", "detail": f"'{current}' 사전에 없음"})
    else:
        steps.append({"step": "접두사 추출", "status": "fail", "detail": "알려진 접두사 없음"})

    # Step 5: 패턴 매칭
    # 5a: corpse
    if stripped.lower().endswith(" corpse"):
        creature = stripped[:-len(" corpse")]
        creature_ko = ctx.species.get(creature.lower())
        if creature_ko:
            steps.append({"step": "시체 패턴", "status": "pass", "detail": f"'{creature}' → '{creature_ko} 시체'"})
            result["diagnosis"] = Diagnosis.TRANSLATION_SUCCESS
            result["result"] = f"{creature_ko} 시체"
            result["suggestion"] = "정상 번역됨"
            return result
        steps.append({"step": "시체 패턴", "status": "fail", "detail": f"생물 '{creature}' 미등록"})
    else:
        steps.append({"step": "시체 패턴", "status": "skip", "detail": "해당 없음"})

    # 5b: food patterns (jerky, meat, haunch)
    food_matched = False
    for suffix, ko_suffix in [("jerky", "육포"), ("meat", "고기"), ("haunch", "넓적다리")]:
        if stripped.lower().endswith(f" {suffix}"):
            creature = stripped[:-len(f" {suffix}")]
            creature_ko = ctx.species.get(creature.lower())
            if creature_ko:
                steps.append({"step": f"식품 패턴 ({suffix})", "status": "pass", "detail": f"'{creature}' → '{creature_ko} {ko_suffix}'"})
                result["diagnosis"] = Diagnosis.TRANSLATION_SUCCESS
                result["result"] = f"{creature_ko} {ko_suffix}"
                result["suggestion"] = "정상 번역됨"
                return result
            steps.append({"step": f"식품 패턴 ({suffix})", "status": "fail", "detail": f"생물 '{creature}' 미등록"})
            food_matched = True
            break
    if not food_matched:
        steps.append({"step": "식품 패턴", "status": "skip", "detail": "해당 없음"})

    # 5c: part_suffixes
    part_matched = False
    for eng_suffix, ko_suffix in sorted(ctx.part_suffixes.items(), key=lambda x: -len(x[0])):
        if stripped.lower().endswith(eng_suffix.lower()):
            creature = stripped[:len(stripped) - len(eng_suffix)]

            # Handle "elder" prefix
            if creature.lower().startswith("elder "):
                actual_creature = creature[len("elder "):]
                creature_ko = ctx.species.get(actual_creature.lower())
                if creature_ko:
                    steps.append({"step": "부위 패턴", "status": "pass", "detail": f"'elder {actual_creature}' + '{eng_suffix.strip()}' → '장로 {creature_ko} {ko_suffix.strip()}'"})
                    result["diagnosis"] = Diagnosis.TRANSLATION_SUCCESS
                    result["result"] = f"장로 {creature_ko} {ko_suffix.strip()}"
                    result["suggestion"] = "정상 번역됨"
                    return result

            # Handle "raw" prefix
            if creature.lower().startswith("raw "):
                actual_creature = creature[len("raw "):]
                creature_ko = ctx.species.get(actual_creature.lower())
                if creature_ko:
                    steps.append({"step": "부위 패턴", "status": "pass", "detail": f"'raw {actual_creature}' + '{eng_suffix.strip()}' → '생 {creature_ko} {ko_suffix.strip()}'"})
                    result["diagnosis"] = Diagnosis.TRANSLATION_SUCCESS
                    result["result"] = f"생 {creature_ko} {ko_suffix.strip()}"
                    result["suggestion"] = "정상 번역됨"
                    return result

            creature_ko = ctx.species.get(creature.lower())
            if creature_ko:
                steps.append({"step": "부위 패턴", "status": "pass", "detail": f"'{creature}' + '{eng_suffix.strip()}' → '{creature_ko} {ko_suffix.strip()}'"})
                result["diagnosis"] = Diagnosis.TRANSLATION_SUCCESS
                result["result"] = f"{creature_ko} {ko_suffix.strip()}"
                result["suggestion"] = "정상 번역됨"
                return result
            steps.append({"step": "부위 패턴", "status": "fail", "detail": f"생물 '{creature}' 미등록"})
            part_matched = True
            break
    if not part_matched:
        steps.append({"step": "부위 패턴", "status": "skip", "detail": "해당 없음"})

    # 5d: possessive
    poss_match = re.match(r"^(.+)'s\s+(.+)$", stripped, re.IGNORECASE)
    if poss_match:
        creature = poss_match.group(1).strip()
        part = poss_match.group(2).strip()
        creature_ko = ctx.species.get(creature.lower())
        part_ko = ctx.body_parts.get(part.lower()) or ctx.base_nouns.get(part.lower())
        if creature_ko and part_ko:
            steps.append({"step": "소유격 패턴", "status": "pass", "detail": f"'{creature}의 {part}' → '{creature_ko}의 {part_ko}'"})
            result["diagnosis"] = Diagnosis.TRANSLATION_SUCCESS
            result["result"] = f"{creature_ko}의 {part_ko}"
            result["suggestion"] = "정상 번역됨"
            return result
        missing = []
        if not creature_ko:
            missing.append(f"생물 '{creature}'")
        if not part_ko:
            missing.append(f"부위 '{part}'")
        steps.append({"step": "소유격 패턴", "status": "fail", "detail": f"미등록: {', '.join(missing)}"})
    else:
        steps.append({"step": "소유격 패턴", "status": "skip", "detail": "해당 없음"})

    # 5e: of pattern
    of_match = re.match(r"^(.+?)\s+of\s+(?:the\s+)?(.+)$", stripped, re.IGNORECASE)
    if of_match:
        item_part = of_match.group(1).strip()
        of_part = of_match.group(2).strip()
        of_key = f"of {of_part}".lower()
        of_ko = ctx.of_patterns.get(of_key) or ctx.of_patterns.get(of_part.lower())
        item_ko = ctx.base_nouns.get(item_part.lower())
        if of_ko and item_ko:
            steps.append({"step": "of 패턴", "status": "pass", "detail": f"'{of_part}'의 '{item_part}'"})
            result["diagnosis"] = Diagnosis.TRANSLATION_SUCCESS
            result["result"] = f"{of_ko} {item_ko}" if of_ko else f"{of_part}의 {item_ko}"
            result["suggestion"] = "정상 번역됨"
            return result
        missing = []
        if not of_ko:
            missing.append(f"of 패턴 '{of_part}'")
        if not item_ko:
            missing.append(f"명사 '{item_part}'")
        steps.append({"step": "of 패턴", "status": "fail", "detail": f"미등록: {', '.join(missing)}"})
    else:
        steps.append({"step": "of 패턴", "status": "skip", "detail": "해당 없음"})

    # Step 6: 최종 진단
    # 단어 분석으로 원인 추정
    words = base_name.split() if prefix_parts else stripped.split()
    remaining_word = current if prefix_parts else base_name

    # 랜덤 생성 이름 감지 (대문자 시작 + 사전에 없는 단어)
    if remaining_word and remaining_word[0].isupper() and remaining_word.lower() not in ctx.base_nouns and remaining_word.lower() not in ctx.species:
        # 고유명사일 가능성
        if len(remaining_word) > 3 and not remaining_word.lower().endswith(("corpse", "jerky", "meat")):
            steps.append({"step": "최종 분석", "status": "fail", "detail": f"'{remaining_word}' - 고유명사/랜덤 생성 가능성"})
            result["diagnosis"] = Diagnosis.RANDOM_GENERATED
            result["result"] = None
            result["suggestion"] = f"'{remaining_word}'이(가) 고유명사라면 species 또는 base_nouns에 추가 필요"
            return result

    # 어휘 누락 분석
    missing_words = []
    for word in words:
        w = word.lower().rstrip("'s")
        if (w not in ctx.base_nouns and w not in ctx.species and
            w not in ctx.materials and w not in ctx.qualities and
            w not in ctx.modifiers and w not in ctx.processing and
            w not in ctx.of_patterns and w not in ctx.body_parts and
            w not in ctx.liquids and w not in ctx.states and
            w not in ("of", "the", "a", "an")):
            missing_words.append(word)

    if missing_words:
        result["diagnosis"] = Diagnosis.VOCABULARY_MISSING
        result["result"] = None
        # 구체적 해결 경로 제시
        suggestions = []
        for w in missing_words:
            # 어디에 추가해야 하는지 추정
            if any(stripped.lower().endswith(f" {s}") for s in ("corpse", "jerky", "meat", "haunch", "egg", "hide", "bone", "skull")):
                suggestions.append(f"creatures/_common.json의 species에 '{w}' 추가")
            elif prefix_parts:
                suggestions.append(f"items/_nouns.json에 '{w}' 추가")
            else:
                suggestions.append(f"items/_nouns.json 또는 items/_common.json에 '{w}' 추가")
        result["suggestion"] = " → ".join(suggestions)
    else:
        result["diagnosis"] = Diagnosis.PATTERN_UNSUPPORTED
        result["result"] = None
        result["suggestion"] = f"패턴 '{stripped}'이(가) 현재 파이프라인에서 지원되지 않음. 새 패턴 핸들러 필요할 수 있음."

    return result


# ============================================================
# 출력 포맷
# ============================================================
def print_diagnosis(diag: dict, verbose: bool = True):
    """진단 결과를 사람이 읽기 쉽게 출력"""
    COLORS = {
        Diagnosis.TRANSLATION_SUCCESS: '\033[92m',  # green
        Diagnosis.ALREADY_KOREAN: '\033[96m',       # cyan
        Diagnosis.VOCABULARY_MISSING: '\033[93m',   # yellow
        Diagnosis.PATTERN_UNSUPPORTED: '\033[91m',  # red
        Diagnosis.LOGIC_BUG: '\033[91m',            # red
        Diagnosis.RANDOM_GENERATED: '\033[95m',     # magenta
    }
    RESET = '\033[0m'
    BOLD = '\033[1m'

    color = COLORS.get(diag["diagnosis"], '')
    print(f"\n{BOLD}{'=' * 60}")
    print(f"  Input: '{diag['input']}'")
    print(f"  Diagnosis: {color}{diag['diagnosis']}{RESET}")
    if diag.get("result"):
        print(f"  Result: {diag['result']}")
    print(f"{'=' * 60}{RESET}")

    if verbose:
        for step in diag["steps"]:
            status_icon = {"pass": "\033[92m✓\033[0m", "fail": "\033[91m✗\033[0m", "skip": "\033[90m-\033[0m"}
            icon = status_icon.get(step["status"], "?")
            print(f"  {icon} [{step['step']}] {step['detail']}")

    print(f"\n  → {BOLD}{diag['suggestion']}{RESET}")


# ============================================================
# CLI
# ============================================================
def main():
    parser = argparse.ArgumentParser(description="번역 진단 도구")
    parser.add_argument("name", nargs="?", help="진단할 영어 이름")
    parser.add_argument("--json", action="store_true", help="JSON 출력")
    parser.add_argument("--batch", nargs="+", help="여러 이름 일괄 진단")
    parser.add_argument("--quiet", action="store_true", help="결과만 출력")
    args = parser.parse_args()

    names = []
    if args.batch:
        names = args.batch
    elif args.name:
        names = [args.name]
    else:
        parser.print_help()
        return 1

    # 사전 로드
    ctx = TranslationContext()
    ctx.load()

    if not args.json and not args.quiet:
        print(f"사전 로드 완료: base_nouns={len(ctx.base_nouns)}, species={len(ctx.species)}, prefixes={len(ctx.all_prefixes_sorted)}")

    results = []
    for name in names:
        diag = diagnose(name, ctx)
        results.append(diag)
        if args.json:
            continue
        if args.quiet:
            status = "OK" if diag["diagnosis"] == Diagnosis.TRANSLATION_SUCCESS else diag["diagnosis"]
            print(f"{status}\t{name}\t{diag.get('result', '-')}")
        else:
            print_diagnosis(diag)

    if args.json:
        print(json.dumps(results, ensure_ascii=False, indent=2))

    # 종료 코드: 모두 성공이면 0, 아니면 실패 개수
    failures = sum(1 for r in results if r["diagnosis"] != Diagnosis.TRANSLATION_SUCCESS and r["diagnosis"] != Diagnosis.ALREADY_KOREAN)
    return failures


if __name__ == "__main__":
    sys.exit(main())
