#!/usr/bin/env python3
"""
XML에서 모든 DisplayName 추출하여 JSON으로 저장
"""

import os
import re
import json
from pathlib import Path
from collections import defaultdict

BASE_DIR = Path("/Users/ben/Desktop/qud_korean")
XML_DIR = BASE_DIR / "Assets/StreamingAssets/Base"
OUTPUT_FILE = BASE_DIR / "Docs/Issues/all_display_names.json"

def extract_displaynames(xml_path):
    """XML 파일에서 DisplayName 추출 (정규식 사용)"""
    results = []

    try:
        with open(xml_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception as e:
        print(f"  [ERROR] {xml_path.name}: {e}")
        return results

    # DisplayName="..." 패턴 매칭
    pattern = r'DisplayName="([^"]*)"'
    matches = re.findall(pattern, content)

    for match in matches:
        if match and not match.startswith('['):  # 템플릿 제외
            results.append({
                'file': xml_path.name,
                'display_name': match,
                'has_color_tag': '{{' in match,
                'is_compound': ' ' in match and '{{' not in match.replace('}}', '').replace('{{', '')
            })

    return results

def main():
    print("=" * 60)
    print("DisplayName 전수 추출")
    print("=" * 60)

    all_items = []
    file_counts = {}

    # 모든 XML 파일 처리
    for xml_file in sorted(XML_DIR.rglob("*.xml")):
        items = extract_displaynames(xml_file)
        if items:
            print(f"{xml_file.name}: {len(items)}개")
            file_counts[xml_file.name] = len(items)
            all_items.extend(items)

    print(f"\n총 {len(all_items)}개 항목")

    # 중복 제거한 고유 목록 생성
    unique_names = {}
    for item in all_items:
        name = item['display_name'].lower()
        if name not in unique_names:
            unique_names[name] = {
                'original': item['display_name'],
                'files': [item['file']],
                'has_color_tag': item['has_color_tag'],
                'count': 1
            }
        else:
            if item['file'] not in unique_names[name]['files']:
                unique_names[name]['files'].append(item['file'])
            unique_names[name]['count'] += 1

    output = {
        'summary': {
            'total_entries': len(all_items),
            'unique_names': len(unique_names),
            'file_counts': file_counts
        },
        'items': dict(sorted(unique_names.items()))
    }

    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        json.dump(output, f, ensure_ascii=False, indent=2)

    print(f"\n저장: {OUTPUT_FILE}")
    print(f"고유 항목: {len(unique_names)}개")

if __name__ == "__main__":
    main()
