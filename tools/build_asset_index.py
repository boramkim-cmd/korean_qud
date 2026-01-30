#!/usr/bin/env python3
"""
XML DisplayName 정적 인덱스 빌더

XML 파일에서 모든 DisplayName을 추출하여 asset_index.json을 생성합니다.
반복적인 XML 검색을 제거하고 빠른 조회를 제공합니다.

사용법:
    python3 tools/build_asset_index.py              # 인덱스 빌드
    python3 tools/build_asset_index.py --stats       # 통계만 출력
    python3 tools/build_asset_index.py --lookup "mace"  # 인덱스에서 조회
"""

import json
import re
import sys
import argparse
from pathlib import Path
from collections import defaultdict

BASE_DIR = Path(__file__).resolve().parent.parent
XML_DIR = BASE_DIR / "Assets" / "StreamingAssets" / "Base"
INDEX_PATH = Path(__file__).parent / "asset_index.json"
LOCALIZATION_DIR = BASE_DIR / "LOCALIZATION"


def extract_displaynames_from_xml(xml_path: Path) -> list:
    """XML 파일에서 DisplayName 추출"""
    results = []
    try:
        with open(xml_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception:
        return results

    # Blueprint ID 추출 시도
    blueprints = re.findall(
        r'<object\s+[^>]*?Name="([^"]*)"[^>]*?DisplayName="([^"]*)"',
        content
    )
    for bp_id, display_name in blueprints:
        if display_name and not display_name.startswith('['):
            results.append({
                'blueprint': bp_id,
                'display_name': display_name,
                'file': xml_path.name
            })

    # Blueprint 없이 DisplayName만 있는 경우도 수집
    standalone = re.findall(r'DisplayName="([^"]*)"', content)
    seen = {dn for _, dn in blueprints}
    for dn in standalone:
        if dn and not dn.startswith('[') and dn not in seen:
            results.append({
                'blueprint': None,
                'display_name': dn,
                'file': xml_path.name
            })
            seen.add(dn)

    return results


def normalize_name(name: str) -> str:
    """이름 정규화 - 컬러태그 제거, 소문자화"""
    normalized = re.sub(r'\{\{[^|]*\|([^}]*)\}\}', r'\1', name)
    normalized = normalized.replace('&amp;', '&')
    normalized = re.sub(r'&[A-Za-z]', '', normalized)
    return normalized.lower().strip()


def load_json_translations() -> dict:
    """JSON 번역 사전 로드 (간소화 버전)"""
    translations = {}
    for json_file in LOCALIZATION_DIR.rglob("*.json"):
        try:
            with open(json_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
        except (json.JSONDecodeError, UnicodeDecodeError):
            continue

        if not isinstance(data, dict):
            continue

        rel_path = str(json_file.relative_to(LOCALIZATION_DIR))
        _walk_json(data, translations, rel_path)

    return translations


def _walk_json(data: dict, translations: dict, file_path: str):
    for key, value in data.items():
        if key.startswith('_'):
            continue
        if isinstance(value, str) and not re.search(r'[\uac00-\ud7af]', key):
            norm = normalize_name(key)
            if norm:
                translations[norm] = {"korean": value, "file": file_path}
        elif isinstance(value, dict):
            if "ko" in value:
                translations[normalize_name(key)] = {"korean": value["ko"], "file": file_path}
            elif key == "names":
                for eng, kor in value.items():
                    if isinstance(kor, str):
                        translations[normalize_name(eng)] = {"korean": kor, "file": file_path}
            else:
                _walk_json(value, translations, file_path)


def build_index() -> dict:
    """인덱스 빌드"""
    if not XML_DIR.exists():
        print(f"XML 디렉토리 없음: {XML_DIR}")
        print("게임 에셋이 필요합니다.")
        return None

    all_items = []
    file_counts = {}

    for xml_file in sorted(XML_DIR.rglob("*.xml")):
        items = extract_displaynames_from_xml(xml_file)
        if items:
            file_counts[xml_file.name] = len(items)
            all_items.extend(items)

    # 중복 제거 + 정규화
    unique = {}
    for item in all_items:
        norm = normalize_name(item['display_name'])
        if norm not in unique:
            unique[norm] = {
                'original': item['display_name'],
                'normalized': norm,
                'blueprints': [item['blueprint']] if item['blueprint'] else [],
                'files': [item['file']],
                'has_color_tag': '{{' in item['display_name'],
                'is_compound': ' ' in norm
            }
        else:
            if item['blueprint'] and item['blueprint'] not in unique[norm]['blueprints']:
                unique[norm]['blueprints'].append(item['blueprint'])
            if item['file'] not in unique[norm]['files']:
                unique[norm]['files'].append(item['file'])

    # 번역 상태 매칭
    translations = load_json_translations()
    translated_count = 0
    for norm, info in unique.items():
        if norm in translations:
            info['translated'] = True
            info['korean'] = translations[norm]['korean']
            info['json_file'] = translations[norm]['file']
            translated_count += 1
        else:
            info['translated'] = False

    index = {
        '_meta': {
            'total_entries': len(all_items),
            'unique_names': len(unique),
            'translated': translated_count,
            'untranslated': len(unique) - translated_count,
            'coverage': f"{translated_count / len(unique) * 100:.1f}%" if unique else "0%",
            'xml_files': file_counts
        },
        'items': dict(sorted(unique.items()))
    }

    return index


def save_index(index: dict):
    with open(INDEX_PATH, 'w', encoding='utf-8') as f:
        json.dump(index, f, ensure_ascii=False, indent=2)
    print(f"저장됨: {INDEX_PATH}")
    print(f"  항목: {index['_meta']['unique_names']}개")
    print(f"  번역됨: {index['_meta']['translated']}개")
    print(f"  미번역: {index['_meta']['untranslated']}개")
    print(f"  커버리지: {index['_meta']['coverage']}")


def lookup(query: str):
    """인덱스에서 조회"""
    if not INDEX_PATH.exists():
        print("인덱스 없음. 먼저 빌드하세요: python3 tools/build_asset_index.py")
        return

    with open(INDEX_PATH, 'r', encoding='utf-8') as f:
        index = json.load(f)

    query_lower = query.lower()
    matches = []
    for norm, info in index['items'].items():
        if query_lower in norm or query_lower in info['original'].lower():
            matches.append(info)

    if not matches:
        print(f"'{query}'에 해당하는 항목 없음")
        return

    print(f"\n'{query}' 검색 결과 ({len(matches)}개):\n")
    for m in matches[:30]:
        status = "✓" if m['translated'] else "✗"
        korean = f" → {m.get('korean', '')}" if m.get('korean') else ""
        print(f"  {status} {m['original']}{korean}")
        if m.get('blueprints'):
            print(f"    blueprints: {', '.join(m['blueprints'][:3])}")

    if len(matches) > 30:
        print(f"\n  ... 외 {len(matches) - 30}개")


def print_stats():
    """인덱스 통계 출력"""
    if not INDEX_PATH.exists():
        print("인덱스 없음")
        return

    with open(INDEX_PATH, 'r', encoding='utf-8') as f:
        index = json.load(f)

    meta = index['_meta']
    print(f"\nAsset Index 통계:")
    print(f"  총 항목: {meta['total_entries']}")
    print(f"  고유 항목: {meta['unique_names']}")
    print(f"  번역됨: {meta['translated']}")
    print(f"  미번역: {meta['untranslated']}")
    print(f"  커버리지: {meta['coverage']}")

    # 파일별 통계
    print(f"\n  XML 파일별:")
    for f, count in sorted(meta.get('xml_files', {}).items(), key=lambda x: -x[1])[:10]:
        print(f"    {f}: {count}개")


def main():
    parser = argparse.ArgumentParser(description="XML DisplayName 인덱스 빌더")
    parser.add_argument("--stats", action="store_true", help="통계만 출력")
    parser.add_argument("--lookup", help="인덱스에서 조회")
    args = parser.parse_args()

    if args.stats:
        print_stats()
        return 0

    if args.lookup:
        lookup(args.lookup)
        return 0

    # 빌드
    print("Asset Index 빌드 중...")
    index = build_index()
    if index:
        save_index(index)
    else:
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
