#!/usr/bin/env python3
"""
qud_korean 빌드 최적화 스크립트

기능:
1. 302개 JSON 파일 → 5개 번들로 병합
2. sourcemap.json 생성 (에러 추적용)
3. 빌드 로그 생성

사용법:
    cd /Users/ben/Desktop/qud_korean
    python3 tools/build_optimized.py
"""

import json
import os
import re
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Any, Tuple, Optional

# ============================================================
# 설정
# ============================================================

PROJECT_ROOT = Path(__file__).parent.parent
LOCALIZATION_DIR = PROJECT_ROOT / "LOCALIZATION"
DIST_DIR = PROJECT_ROOT / "dist"
DATA_DIR = DIST_DIR / "data"
XML_DIR = PROJECT_ROOT / "Assets" / "StreamingAssets" / "Base"

# 번들 매핑
BUNDLE_CONFIG = {
    "objects": {
        "sources": [
            "OBJECTS/creatures",
            "OBJECTS/items",
            "OBJECTS/furniture",
            "OBJECTS/terrain",
            "OBJECTS/hidden.json",
            "OBJECTS/widgets.json",
            "OBJECTS/misc",
        ],
        "vocabulary": [
            "OBJECTS/_vocabulary",
            "OBJECTS/items/_common.json",
            "OBJECTS/items/_nouns.json",
            "OBJECTS/creatures/_common.json",
            "OBJECTS/_suffixes.json",
            "OBJECTS/furniture/_common.json",
        ],
    },
    "shared": {
        "sources": ["_SHARED"],
    },
    "chargen": {
        "sources": ["CHARGEN"],
    },
    "gameplay": {
        "sources": ["GAMEPLAY"],
    },
    "ui": {
        "sources": ["UI"],
    },
}

# ============================================================
# 소스맵 생성
# ============================================================

class SourceMapper:
    """소스맵 생성 및 관리 클래스"""

    def __init__(self):
        self.blueprints: Dict[str, Dict] = {}
        self.vocabulary: Dict[str, Dict] = {}
        self.warnings: List[Dict] = []

    def add_blueprint(self, blueprint_id: str, file_path: str, line: int,
                      category: str, names: Optional[List[str]] = None,
                      analysis: Optional[Dict] = None):
        """블루프린트 소스 정보 추가"""
        self.blueprints[blueprint_id] = {
            "file": str(file_path),
            "line": line,
            "category": category,
        }
        if names:
            self.blueprints[blueprint_id]["names"] = names
        if analysis:
            self.blueprints[blueprint_id]["analysis"] = analysis

    def add_vocabulary(self, term: str, file_path: str, line: int, korean: str):
        """어휘 소스 정보 추가"""
        self.vocabulary[term] = {
            "file": str(file_path),
            "line": line,
            "korean": korean,
        }

    def add_warning(self, warning_type: str, message: str, **kwargs):
        """빌드 경고 추가"""
        warning = {"type": warning_type, "message": message}
        warning.update(kwargs)
        self.warnings.append(warning)

    def to_dict(self) -> Dict:
        """소스맵 딕셔너리 반환"""
        return {
            "_meta": {
                "buildTime": datetime.now().isoformat(),
                "sourceDir": str(PROJECT_ROOT),
                "phase": 1,
                "totalBlueprints": len(self.blueprints),
                "totalVocabulary": len(self.vocabulary),
            },
            "blueprints": self.blueprints,
            "vocabulary": self.vocabulary,
            "warnings": self.warnings,
        }

# ============================================================
# JSON 파싱 (라인 번호 추적)
# ============================================================

def parse_json_with_lines(file_path: Path) -> Tuple[Dict, Dict[str, int]]:
    """JSON 파일을 파싱하면서 각 키의 라인 번호 추적"""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
        lines = content.split('\n')

    data = json.loads(content)
    line_map = {}

    # 각 최상위 키의 라인 번호 찾기
    for key in data.keys():
        # 키를 이스케이프하고 패턴 생성
        escaped_key = re.escape(key)
        pattern = rf'^\s*"{escaped_key}"\s*:'
        for i, line in enumerate(lines, 1):
            if re.match(pattern, line):
                line_map[key] = i
                break

    return data, line_map

# ============================================================
# 번들 생성
# ============================================================

def build_bundle(bundle_name: str, config: Dict, source_mapper: SourceMapper) -> Dict:
    """단일 번들 생성"""
    bundle_data = {}

    for source_path in config.get("sources", []):
        full_path = LOCALIZATION_DIR / source_path

        if full_path.is_file() and full_path.suffix == ".json":
            # 단일 파일
            if not full_path.name.startswith("_"):
                process_json_file(full_path, bundle_data, source_mapper, bundle_name)
        elif full_path.is_dir():
            # 디렉토리 - 재귀적으로 처리
            for json_file in sorted(full_path.rglob("*.json")):
                if json_file.name.startswith("_"):
                    continue  # _로 시작하는 파일은 vocabulary로 처리
                process_json_file(json_file, bundle_data, source_mapper, bundle_name)

    # vocabulary 처리 (objects 번들에서만)
    for vocab_path in config.get("vocabulary", []):
        full_path = LOCALIZATION_DIR / vocab_path
        if full_path.is_file() and full_path.suffix == ".json":
            process_vocabulary_file(full_path, bundle_data, source_mapper)
        elif full_path.is_dir():
            for json_file in sorted(full_path.rglob("*.json")):
                process_vocabulary_file(json_file, bundle_data, source_mapper)

    return bundle_data

def get_category_from_path(file_path: Path) -> str:
    """파일 경로에서 카테고리 추출"""
    rel_path = file_path.relative_to(LOCALIZATION_DIR)
    parts = rel_path.parts

    if len(parts) >= 2:
        if parts[0] == "OBJECTS":
            category = parts[1]
            # 파일 확장자 제거 (hidden.json -> hidden)
            if category.endswith(".json"):
                category = category[:-5]
            return category  # creatures, items, furniture, terrain, hidden, widgets
        return parts[0]  # CHARGEN, GAMEPLAY, UI, _SHARED
    return "unknown"

def process_json_file(file_path: Path, bundle_data: Dict,
                      source_mapper: SourceMapper, bundle_name: str):
    """JSON 파일 처리 및 소스맵 기록"""
    try:
        data, line_map = parse_json_with_lines(file_path)
        rel_path = file_path.relative_to(PROJECT_ROOT)
        category = get_category_from_path(file_path)

        for key, value in data.items():
            if key.startswith("_"):
                continue

            # 번들에 카테고리별로 구분하여 추가
            if category not in bundle_data:
                bundle_data[category] = {}
            bundle_data[category][key] = value

            # 소스맵에 기록
            names = []
            if isinstance(value, dict) and "names" in value:
                names = list(value.get("names", {}).keys())

            source_mapper.add_blueprint(
                blueprint_id=key,
                file_path=str(rel_path),
                line=line_map.get(key, 0),
                category=category,
                names=names if names else None,
            )
    except Exception as e:
        source_mapper.add_warning(
            "parse_error",
            f"Failed to parse {file_path}: {e}",
            file=str(file_path),
        )

def process_vocabulary_file(file_path: Path, bundle_data: Dict, source_mapper: SourceMapper):
    """어휘 파일 처리"""
    try:
        data, line_map = parse_json_with_lines(file_path)
        rel_path = file_path.relative_to(PROJECT_ROOT)

        if "vocabulary" not in bundle_data:
            bundle_data["vocabulary"] = {}

        def extract_terms(obj: Any, parent_key: str = "", depth: int = 0) -> Dict[str, str]:
            """중첩 구조에서 어휘 추출"""
            terms = {}
            if isinstance(obj, dict):
                for key, value in obj.items():
                    if key.startswith("_"):
                        continue

                    if isinstance(value, str):
                        terms[key] = value
                        source_mapper.add_vocabulary(
                            key, str(rel_path),
                            line_map.get(parent_key, 0) if parent_key else line_map.get(key, 0),
                            value
                        )
                    elif isinstance(value, dict):
                        # SHARED 포맷: {"ko": "한글", "aliases": [...]}
                        if "ko" in value:
                            ko = value["ko"]
                            terms[key] = ko
                            source_mapper.add_vocabulary(
                                key, str(rel_path),
                                line_map.get(key, 0),
                                ko
                            )
                            for alias in value.get("aliases", []):
                                terms[alias] = ko
                                source_mapper.add_vocabulary(
                                    alias, str(rel_path),
                                    line_map.get(key, 0),
                                    ko
                                )
                        else:
                            # 재귀적으로 중첩 객체 처리
                            terms.update(extract_terms(value, key, depth + 1))
            return terms

        extracted = extract_terms(data)
        bundle_data["vocabulary"].update(extracted)

    except Exception as e:
        source_mapper.add_warning(
            "vocabulary_error",
            f"Failed to process vocabulary {file_path}: {e}",
            file=str(file_path),
        )

# ============================================================
# Display Lookup 생성 (XML DisplayName → 한글 1:1 매핑)
# ============================================================

def normalize_for_lookup(name: str) -> str:
    """컬러태그 제거, 소문자화하여 번역 사전 조회용 키 생성"""
    normalized = re.sub(r'\{\{[^|]*\|([^}]*)\}\}', r'\1', name)
    normalized = normalized.replace('&amp;', '&')
    normalized = re.sub(r'&[A-Za-z]', '', normalized)
    return normalized.lower().strip()


def load_all_translations() -> Dict[str, str]:
    """LOCALIZATION 디렉토리에서 모든 번역 사전 로드 (normalized_key → korean)"""
    translations = {}

    def walk_json(data: dict, file_path: str):
        for key, value in data.items():
            if key.startswith('_'):
                continue
            if isinstance(value, str) and not re.search(r'[\uac00-\ud7af]', key):
                norm = normalize_for_lookup(key)
                if norm:
                    translations[norm] = value
            elif isinstance(value, dict):
                if "ko" in value:
                    translations[normalize_for_lookup(key)] = value["ko"]
                elif key == "names":
                    for eng, kor in value.items():
                        if isinstance(kor, str):
                            translations[normalize_for_lookup(eng)] = kor
                else:
                    walk_json(value, file_path)

    for json_file in LOCALIZATION_DIR.rglob("*.json"):
        try:
            with open(json_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
            if isinstance(data, dict):
                walk_json(data, str(json_file.relative_to(PROJECT_ROOT)))
        except (json.JSONDecodeError, UnicodeDecodeError):
            continue

    return translations


def build_display_lookup() -> Tuple[Dict[str, str], int, int]:
    """
    XML ObjectBlueprints에서 DisplayName을 추출하고 번역과 1:1 매핑.
    상속(Inherits) 관계를 반영하여 부모 블루프린트의 번역도 활용.

    Returns: (lookup_dict, total_extracted, matched_count)
    """
    if not XML_DIR.exists():
        print("  [WARN] XML 디렉토리 없음, display_lookup 스킵")
        return {}, 0, 0

    # 1. XML에서 모든 DisplayName 추출
    # {blueprint_name: display_name}, {blueprint_name: inherits_from}
    blueprint_display: Dict[str, str] = {}
    blueprint_inherits: Dict[str, str] = {}

    # 정규식 기반 추출 (게임 XML에 invalid character references가 있어 ET 사용 불가)
    obj_pattern = re.compile(
        r'<object\s+[^>]*?Name="([^"]*)"[^>]*?Inherits="([^"]*)"',
    )
    dn_pattern = re.compile(
        r'<object\s+[^>]*?Name="([^"]*)"[^>]*?>.*?'
        r'<part\s+[^>]*?Name="Render"[^>]*?DisplayName="([^"]*)"',
        re.DOTALL,
    )

    for xml_file in sorted(XML_DIR.rglob("*.xml")):
        try:
            with open(xml_file, 'r', encoding='utf-8') as f:
                content = f.read()
        except Exception:
            continue

        # 상속 관계 추출
        for match in obj_pattern.finditer(content):
            bp_name, inherits = match.group(1), match.group(2)
            blueprint_inherits[bp_name] = inherits

        # DisplayName 추출 (object → Render part)
        # 더 정확한 매칭: object 블록 단위로 처리
        for match in re.finditer(
            r'<object\s+[^>]*?Name="([^"]*)"[^>]*?>(.*?)</object>',
            content, re.DOTALL
        ):
            bp_name = match.group(1)
            body = match.group(2)
            dn_match = re.search(
                r'<part\s+[^>]*?Name="Render"[^>]*?DisplayName="([^"]*)"',
                body
            )
            if dn_match:
                display = dn_match.group(1)
                if display and not display.startswith('['):
                    blueprint_display[bp_name] = display

    # 2. 번역 사전 로드
    translations = load_all_translations()

    # 3. DisplayName → 한글 매핑 (원문 그대로 key)
    lookup: Dict[str, str] = {}
    matched = 0

    for bp_name, display_name in blueprint_display.items():
        norm = normalize_for_lookup(display_name)

        # 직접 매칭
        if norm in translations:
            lookup[display_name] = translations[norm]
            matched += 1
            continue

        # 상속 체인을 따라 부모의 DisplayName으로 매칭 시도
        parent = blueprint_inherits.get(bp_name)
        found = False
        visited = set()
        while parent and parent not in visited:
            visited.add(parent)
            parent_display = blueprint_display.get(parent)
            if parent_display:
                parent_norm = normalize_for_lookup(parent_display)
                if parent_norm in translations:
                    lookup[display_name] = translations[parent_norm]
                    matched += 1
                    found = True
                    break
            parent = blueprint_inherits.get(parent)

    return lookup, len(blueprint_display), matched


# ============================================================
# 메인 빌드 함수
# ============================================================

def build():
    """메인 빌드 실행"""
    print("=" * 60)
    print("qud_korean 빌드 시작")
    print(f"소스: {PROJECT_ROOT}")
    print("=" * 60)

    # dist 디렉토리 생성 (기존 내용 삭제)
    if DIST_DIR.exists():
        import shutil
        shutil.rmtree(DIST_DIR)
    DATA_DIR.mkdir(parents=True, exist_ok=True)

    source_mapper = SourceMapper()
    build_log = []

    # 각 번들 생성
    for bundle_name, config in BUNDLE_CONFIG.items():
        print(f"\n>> {bundle_name} 번들 생성 중...")
        bundle_data = build_bundle(bundle_name, config, source_mapper)

        # 번들 저장
        output_path = DATA_DIR / f"{bundle_name}.json"
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(bundle_data, f, ensure_ascii=False, indent=2)

        size_kb = output_path.stat().st_size / 1024
        entry_count = sum(
            len(v) if isinstance(v, dict) else 1
            for v in bundle_data.values()
        )
        print(f"   저장: {output_path.name} ({size_kb:.1f}KB, {entry_count}개 항목)")
        build_log.append(f"{bundle_name}: {size_kb:.1f}KB, {entry_count} entries")

    # Display Lookup 생성
    print(f"\n>> display_lookup 생성 중...")
    display_lookup, total_dn, matched_dn = build_display_lookup()
    if display_lookup:
        lookup_path = DATA_DIR / "display_lookup.json"
        with open(lookup_path, 'w', encoding='utf-8') as f:
            json.dump(display_lookup, f, ensure_ascii=False, indent=2)
        size_kb = lookup_path.stat().st_size / 1024
        print(f"   저장: display_lookup.json ({size_kb:.1f}KB, {matched_dn}/{total_dn} 매칭)")
        build_log.append(f"display_lookup: {size_kb:.1f}KB, {matched_dn}/{total_dn} matched")
    else:
        print(f"   display_lookup 생성 실패 (XML 없음)")

    # 소스맵 저장
    sourcemap_path = DIST_DIR / "sourcemap.json"
    with open(sourcemap_path, 'w', encoding='utf-8') as f:
        json.dump(source_mapper.to_dict(), f, ensure_ascii=False, indent=2)

    sm_size_kb = sourcemap_path.stat().st_size / 1024
    print(f"\n>> 소스맵 저장: {sourcemap_path.name} ({sm_size_kb:.1f}KB)")
    print(f"   블루프린트: {len(source_mapper.blueprints)}개")
    print(f"   어휘: {len(source_mapper.vocabulary)}개")

    if source_mapper.warnings:
        print(f"   경고: {len(source_mapper.warnings)}개")

    # 빌드 로그 저장
    log_path = DIST_DIR / "build.log"
    with open(log_path, 'w', encoding='utf-8') as f:
        f.write(f"Build Time: {datetime.now().isoformat()}\n")
        f.write(f"Source: {PROJECT_ROOT}\n")
        f.write(f"Phase: 1 (JSON Bundling)\n")
        f.write("\nBundles:\n")
        for entry in build_log:
            f.write(f"  {entry}\n")
        f.write(f"\nSourceMap:\n")
        f.write(f"  Blueprints: {len(source_mapper.blueprints)}\n")
        f.write(f"  Vocabulary: {len(source_mapper.vocabulary)}\n")
        if source_mapper.warnings:
            f.write("\nWarnings:\n")
            for w in source_mapper.warnings:
                f.write(f"  [{w['type']}] {w['message']}\n")

    print("\n" + "=" * 60)
    print("빌드 완료!")
    print(f"출력 디렉토리: {DIST_DIR}")
    print("=" * 60)

    # 결과 요약
    total_size = sum(f.stat().st_size for f in DATA_DIR.glob("*.json")) / 1024
    print(f"\n총 번들 크기: {total_size:.1f}KB")
    print(f"소스맵 크기: {sm_size_kb:.1f}KB")

if __name__ == "__main__":
    build()
