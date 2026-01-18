#!/usr/bin/env python3
"""
🔍 미번역 항목 탐색 도구 (Missing Translation Checker)
- XML 파일의 텍스트가 glossary_*.json에 포함되었는지 확인
- C# 코드 내의 문자열 리터럴이 glossary_*.json에 포함되었는지 확인
"""

from __future__ import annotations
import json
import re
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Set, Optional

# 설정부
PROJECT_ROOT = Path(__file__).parent.parent.resolve()
LOCALIZATION_DIR = PROJECT_ROOT / "LOCALIZATION"

# 검색할 XML 속성 목록
XML_TEXT_ATTRS = ('Description', 'DisplayName', 'ChargenDescription', 'Title')


def load_glossary_keys(filename: str, category: str) -> Set[str]:
    """지정한 glossary 파일의 특정 카테고리 키들을 집합으로 반환"""
    path = LOCALIZATION_DIR / filename
    if not path.exists():
        return set()
    try:
        with path.open('r', encoding='utf-8') as f:
            data = json.load(f)
            return {k.lower().strip() for k in data.get(category, {})}
    except (json.JSONDecodeError, IOError) as e:
        print(f"⚠️  용어집 로드 오류 ({filename}): {e}")
        return set()


def check_xml(xml_path: str, glossary_file: str, category: str) -> Optional[Set[str]]:
    """XML 파일 내 텍스트 추출 및 매칭 검사. 미발견 항목 반환."""
    full_path = PROJECT_ROOT / xml_path
    if not full_path.exists():
        return None

    print(f"\n--- XML 검사: {full_path.name} ---")
    keys = load_glossary_keys(glossary_file, category)
    if not keys:
        print(f"  ⚠️  용어집이 비어있거나 로드 실패: {glossary_file}")
        return None

    try:
        tree = ET.parse(full_path)
    except ET.ParseError as e:
        print(f"  ❌ XML 파싱 오류: {e}")
        return None

    missing: Set[str] = set()

    for elem in tree.iter():
        # 속성 검사
        for attr in XML_TEXT_ATTRS:
            val = elem.get(attr)
            if val and not val.startswith('*') and val.lower().strip() not in keys:
                missing.add(val)

        # description 태그 검사
        if elem.tag == 'description' and elem.text:
            text = elem.text.strip()
            if text and text.lower() not in keys:
                missing.add(text)

    _print_missing(missing)
    return missing


def check_csharp(cs_dir: str, glossary_file: str, category: str) -> Optional[Set[str]]:
    """C# 코드 내 리터럴 추출 및 매칭 검사. 미발견 항목 반환."""
    full_path = PROJECT_ROOT / cs_dir
    if not full_path.exists():
        return None

    print(f"\n--- C# 검사: {full_path.name} ---")
    keys = load_glossary_keys(glossary_file, category)
    missing: Set[str] = set()

    # 10자 이상의 문자열 리터럴 추출 (휴리스틱)
    string_pattern = re.compile(r'"([^"]{10,})"')

    for cs_file in full_path.rglob("*.cs"):
        try:
            content = cs_file.read_text(encoding='utf-8')
            for match in string_pattern.findall(content):
                if match.lower().strip() not in keys:
                    missing.add(match)
        except IOError as e:
            print(f"  ⚠️  파일 읽기 오류 ({cs_file.name}): {e}")

    _print_missing(missing, "코드 내 의심 문자열이 모두 용어집에 있거나 짧습니다.")
    return missing


def _print_missing(missing: Set[str], success_msg: str = "모든 항목이 용어집에 포함되어 있습니다.") -> None:
    """미발견 항목 출력 헬퍼"""
    if missing:
        for m in sorted(missing)[:10]:
            print(f"  [M] {m[:70]}")
        if len(missing) > 10:
            print(f"  ... 외 {len(missing) - 10}개")
    else:
        print(f"  ✅ {success_msg}")


def main() -> None:
    print("=" * 80)
    print("🔍 미번역 항목 정밀 스캔")
    print("=" * 80)

    # 1. 스킬/돌연변이 XML 검사
    check_xml('Assets/StreamingAssets/Base/Skills.xml', 'glossary_skills.json', 'skill_desc')
    check_xml('Assets/StreamingAssets/Base/Mutations.xml', 'glossary_mutations.json', 'mutation_desc')

    # 2. 캐릭터 생성 화면 XML 검사
    check_xml('Assets/StreamingAssets/Base/EmbarkModules.xml', 'glossary_chargen.json', 'chargen')

    # 3. C# 코드 내 텍스트 검사 (필요시 활성화)
    # check_csharp('Assets/core_source/XRL.World.Parts.Mutation/', 'glossary_mutations.json', 'mutation_desc')

    print("\n" + "=" * 80)


if __name__ == "__main__":
    main()
