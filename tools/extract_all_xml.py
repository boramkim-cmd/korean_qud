#!/usr/bin/env python3
"""
XML 전수 조사 스크립트
모든 XML 파일에서 번역 가능한 텍스트를 추출하여 정리
"""

import os
import re
import json
import xml.etree.ElementTree as ET
from pathlib import Path
from collections import defaultdict
from datetime import datetime

BASE_DIR = Path("/Users/ben/Desktop/qud_korean")
XML_DIR = BASE_DIR / "Assets/StreamingAssets/Base"
OUTPUT_DIR = BASE_DIR / "Docs/Issues"

# 번역 대상 속성들
TRANSLATABLE_ATTRS = [
    'DisplayName', 'Name', 'Tile', 'Description',
    'Title', 'Text', 'Label', 'Short',
    'TinkerDisplayName', 'Adjective'
]

def extract_from_xml(xml_path):
    """단일 XML 파일에서 모든 번역 가능한 텍스트 추출"""
    results = []

    try:
        tree = ET.parse(xml_path)
        root = tree.getroot()
    except ET.ParseError as e:
        print(f"  [ERROR] XML 파싱 실패: {xml_path.name} - {e}")
        return results

    def process_element(elem, path=""):
        """재귀적으로 요소 처리"""
        current_path = f"{path}/{elem.tag}" if path else elem.tag

        # 요소의 속성 검사
        for attr in TRANSLATABLE_ATTRS:
            if attr in elem.attrib:
                value = elem.attrib[attr]
                if value and not value.startswith('=') and len(value) > 0:
                    # Name 속성도 함께 기록 (ID용)
                    obj_name = elem.attrib.get('Name', elem.attrib.get('ID', ''))
                    results.append({
                        'file': xml_path.name,
                        'element': elem.tag,
                        'object_name': obj_name,
                        'attribute': attr,
                        'value': value,
                        'path': current_path
                    })

        # 자식 요소 처리
        for child in elem:
            process_element(child, current_path)

    process_element(root)
    return results

def categorize_items(all_items):
    """아이템들을 카테고리별로 분류"""
    categories = defaultdict(list)

    for item in all_items:
        file_name = item['file']

        # 파일명으로 대분류
        if file_name == 'Items.xml':
            categories['items'].append(item)
        elif file_name == 'Creatures.xml':
            categories['creatures'].append(item)
        elif file_name == 'Foods.xml':
            categories['foods'].append(item)
        elif file_name == 'Furniture.xml':
            categories['furniture'].append(item)
        elif file_name == 'Skills.xml':
            categories['skills'].append(item)
        elif file_name == 'Mutations.xml':
            categories['mutations'].append(item)
        elif file_name == 'Mods.xml':
            categories['mods'].append(item)
        elif file_name == 'Bodies.xml':
            categories['bodies'].append(item)
        elif file_name == 'Factions.xml':
            categories['factions'].append(item)
        elif file_name == 'Quests.xml':
            categories['quests'].append(item)
        elif 'Conversation' in file_name:
            categories['conversations'].append(item)
        elif file_name == 'Books.xml':
            categories['books'].append(item)
        elif 'Walls' in file_name or 'Terrain' in file_name:
            categories['terrain'].append(item)
        else:
            categories['other'].append(item)

    return categories

def has_korean(text):
    """한글 포함 여부 확인"""
    return bool(re.search(r'[\uac00-\ud7af]', text))

def has_color_tag(text):
    """컬러 태그 포함 여부"""
    return '{{' in text and '}}' in text

def generate_markdown_report(categories, all_items):
    """마크다운 리포트 생성"""
    lines = []
    lines.append("# XML 전수 조사 리포트")
    lines.append(f"\n생성일: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")

    # 요약
    lines.append("## 요약\n")
    lines.append(f"- 총 추출 항목: {len(all_items)}개")

    display_names = [i for i in all_items if i['attribute'] == 'DisplayName']
    lines.append(f"- DisplayName 항목: {len(display_names)}개")

    color_tagged = [i for i in display_names if has_color_tag(i['value'])]
    lines.append(f"- 컬러 태그 포함: {len(color_tagged)}개")

    lines.append("\n### 카테고리별 항목 수\n")
    lines.append("| 카테고리 | 항목 수 |")
    lines.append("|----------|---------|")
    for cat, items in sorted(categories.items(), key=lambda x: -len(x[1])):
        lines.append(f"| {cat} | {len(items)} |")

    # 카테고리별 상세
    for cat, items in sorted(categories.items()):
        lines.append(f"\n---\n\n## {cat.upper()}\n")

        # DisplayName만 필터
        display_items = [i for i in items if i['attribute'] == 'DisplayName']

        if not display_items:
            lines.append("(DisplayName 항목 없음)\n")
            continue

        # 정렬 (알파벳순)
        display_items.sort(key=lambda x: x['value'].lower())

        lines.append(f"총 {len(display_items)}개 DisplayName\n")
        lines.append("| Object Name | DisplayName | Color Tag |")
        lines.append("|-------------|-------------|-----------|")

        for item in display_items:
            obj = item['object_name'].replace('|', '\\|')
            val = item['value'].replace('|', '\\|')
            has_tag = "O" if has_color_tag(item['value']) else ""
            lines.append(f"| {obj} | `{val}` | {has_tag} |")

    return '\n'.join(lines)

def generate_json_data(all_items):
    """JSON 데이터 생성 (프로그래밍용)"""
    # DisplayName만 추출하여 정리
    display_names = {}

    for item in all_items:
        if item['attribute'] == 'DisplayName':
            key = item['value'].lower()
            if key not in display_names:
                display_names[key] = {
                    'original': item['value'],
                    'object_name': item['object_name'],
                    'file': item['file'],
                    'has_color_tag': has_color_tag(item['value']),
                    'occurrences': 1
                }
            else:
                display_names[key]['occurrences'] += 1

    return display_names

def main():
    print("=" * 60)
    print("XML 전수 조사 시작")
    print("=" * 60)

    all_items = []

    # 모든 XML 파일 처리
    xml_files = list(XML_DIR.rglob("*.xml"))
    print(f"\n총 {len(xml_files)}개 XML 파일 발견\n")

    for xml_file in sorted(xml_files):
        print(f"처리 중: {xml_file.name}...", end=" ")
        items = extract_from_xml(xml_file)
        print(f"{len(items)}개 항목")
        all_items.extend(items)

    print(f"\n총 {len(all_items)}개 항목 추출 완료")

    # 카테고리화
    categories = categorize_items(all_items)

    # 마크다운 리포트 생성
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    report = generate_markdown_report(categories, all_items)
    report_path = OUTPUT_DIR / "xml_full_audit.md"
    with open(report_path, 'w', encoding='utf-8') as f:
        f.write(report)
    print(f"\n마크다운 리포트 저장: {report_path}")

    # JSON 데이터 생성
    json_data = generate_json_data(all_items)
    json_path = OUTPUT_DIR / "xml_display_names.json"
    with open(json_path, 'w', encoding='utf-8') as f:
        json.dump(json_data, f, ensure_ascii=False, indent=2)
    print(f"JSON 데이터 저장: {json_path}")

    # 통계 출력
    print("\n" + "=" * 60)
    print("통계")
    print("=" * 60)

    display_names = [i for i in all_items if i['attribute'] == 'DisplayName']
    color_tagged = [i for i in display_names if has_color_tag(i['value'])]

    print(f"총 DisplayName: {len(display_names)}개")
    print(f"컬러 태그 포함: {len(color_tagged)}개")
    print(f"고유 DisplayName: {len(json_data)}개")

    print("\n카테고리별:")
    for cat, items in sorted(categories.items(), key=lambda x: -len(x[1])):
        dn_count = len([i for i in items if i['attribute'] == 'DisplayName'])
        print(f"  {cat}: {dn_count}개 DisplayName")

if __name__ == "__main__":
    main()
