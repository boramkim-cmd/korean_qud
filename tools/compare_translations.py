#!/usr/bin/env python3
"""
XML DisplayName vs JSON 번역 항목 비교 스크립트
미번역 항목을 식별하고 리포트 생성
"""

import json
import re
from pathlib import Path
from collections import defaultdict

BASE_DIR = Path("/Users/ben/Desktop/qud_korean")
XML_DATA = BASE_DIR / "Docs/Issues/all_display_names.json"
JSON_DIR = BASE_DIR / "LOCALIZATION"
OUTPUT_FILE = BASE_DIR / "Docs/Issues/untranslated_report.md"
PRIORITY_FILE = BASE_DIR / "Docs/Issues/translation_priority.md"


def load_xml_display_names():
    """XML에서 추출한 DisplayName 로드"""
    with open(XML_DATA, 'r', encoding='utf-8') as f:
        data = json.load(f)
    return data['items'], data['summary']


def normalize_name(name):
    """이름 정규화 - 컬러태그 제거, 소문자화"""
    # {{shader|text}} -> text
    normalized = re.sub(r'\{\{[^|]*\|([^}]*)\}\}', r'\1', name)
    # &amp; -> &
    normalized = normalized.replace('&amp;', '&')
    # &Xtext -> text (remove color codes like &C, &Y, &K, etc.)
    normalized = re.sub(r'&[A-Za-z]', '', normalized)
    return normalized.lower().strip()


def load_json_translations():
    """모든 JSON 파일에서 번역 항목 로드"""
    translations = {}  # {normalized_name: {'korean': str, 'file': str, 'original': str}}

    for json_file in JSON_DIR.rglob("*.json"):
        try:
            with open(json_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
        except (json.JSONDecodeError, UnicodeDecodeError):
            continue

        rel_path = json_file.relative_to(JSON_DIR)
        _extract_translations(data, translations, str(rel_path))

    return translations


def _extract_translations(data, translations, file_path, prefix=""):
    """재귀적으로 JSON에서 번역 항목 추출"""
    if not isinstance(data, dict):
        return

    for key, value in data.items():
        if key.startswith('_'):
            continue

        if isinstance(value, str):
            # 키가 영어이고 값이 한글인 경우
            norm = normalize_name(key)
            if norm and not re.search(r'[\uac00-\ud7af]', key):
                translations[norm] = {
                    'korean': value,
                    'file': file_path,
                    'original': key
                }
        elif isinstance(value, dict):
            # "names" 섹션 특별 처리
            if key == 'names':
                for eng, kor in value.items():
                    if isinstance(kor, str):
                        norm = normalize_name(eng)
                        translations[norm] = {
                            'korean': kor,
                            'file': file_path,
                            'original': eng
                        }
            else:
                # 재귀적으로 탐색
                _extract_translations(value, translations, file_path, f"{prefix}{key}.")


def compare_and_categorize(xml_items, translations):
    """XML 항목과 번역 비교, 카테고리별 분류"""
    results = {
        'translated': [],      # 번역됨
        'untranslated': [],    # 미번역
        'partial': [],         # 부분 번역 (이미 한글 포함)
        'template': []         # 템플릿 ([Item] 등)
    }

    for name_key, info in xml_items.items():
        original = info['original']
        normalized = normalize_name(original)

        # 템플릿 제외
        if original.startswith('[') and original.endswith(']'):
            results['template'].append(original)
            continue

        # 이미 한글 포함 (부분 번역)
        if re.search(r'[\uac00-\ud7af]', original):
            results['partial'].append({
                'original': original,
                'files': info['files'],
                'has_color_tag': info['has_color_tag']
            })
            continue

        # 번역 존재 여부 확인
        if normalized in translations:
            results['translated'].append({
                'original': original,
                'korean': translations[normalized]['korean'],
                'json_file': translations[normalized]['file']
            })
        else:
            results['untranslated'].append({
                'original': original,
                'normalized': normalized,
                'files': info['files'],
                'has_color_tag': info['has_color_tag'],
                'count': info['count']
            })

    return results


def categorize_priority(untranslated):
    """미번역 항목 우선순위 분류"""
    priority = {
        'high': [],    # Items.xml, Foods.xml (플레이어가 자주 보는)
        'medium': [],  # Creatures.xml, Furniture.xml
        'low': []      # 기타
    }

    high_files = {'Items.xml', 'Foods.xml'}
    medium_files = {'Creatures.xml', 'Furniture.xml', 'Walls.xml'}

    for item in untranslated:
        files = set(item['files'])
        if files & high_files:
            priority['high'].append(item)
        elif files & medium_files:
            priority['medium'].append(item)
        else:
            priority['low'].append(item)

    return priority


def generate_report(results, summary, trans_count):
    """마크다운 리포트 생성"""
    lines = []
    lines.append("# XML vs JSON 번역 비교 리포트")
    lines.append("")
    lines.append("## 요약")
    lines.append("")
    lines.append("| 항목 | 개수 |")
    lines.append("|------|------|")
    lines.append(f"| XML DisplayName (총) | {summary['unique_names']} |")
    lines.append(f"| JSON 번역 항목 | {trans_count} |")
    lines.append(f"| 번역됨 | {len(results['translated'])} |")
    lines.append(f"| **미번역** | **{len(results['untranslated'])}** |")
    lines.append(f"| 부분 번역 | {len(results['partial'])} |")
    lines.append(f"| 템플릿 (제외) | {len(results['template'])} |")
    lines.append("")

    # 미번역 항목 - 파일별 그룹
    lines.append("---")
    lines.append("")
    lines.append("## 미번역 항목 (파일별)")
    lines.append("")

    by_file = defaultdict(list)
    for item in results['untranslated']:
        for f in item['files']:
            by_file[f].append(item)

    for file_name in sorted(by_file.keys()):
        items = by_file[file_name]
        lines.append(f"### {file_name} ({len(items)}개)")
        lines.append("")
        lines.append("| DisplayName | 컬러태그 |")
        lines.append("|-------------|----------|")
        for item in sorted(items, key=lambda x: x['original'].lower()):
            tag = "O" if item['has_color_tag'] else ""
            orig = item['original'].replace('|', '\\|')
            lines.append(f"| `{orig}` | {tag} |")
        lines.append("")

    # 부분 번역 항목
    lines.append("---")
    lines.append("")
    lines.append("## 부분 번역 항목 (한영 혼합)")
    lines.append("")
    lines.append("| DisplayName | 출처 |")
    lines.append("|-------------|------|")
    for item in sorted(results['partial'], key=lambda x: x['original'].lower()):
        orig = item['original'].replace('|', '\\|')
        files = ', '.join(item['files'])
        lines.append(f"| `{orig}` | {files} |")
    lines.append("")

    return '\n'.join(lines)


def generate_priority_report(priority, total_untranslated):
    """우선순위별 리포트 생성"""
    lines = []
    lines.append("# 번역 우선순위 리포트")
    lines.append("")
    lines.append("미번역 항목을 플레이어 경험 빈도 기준으로 분류했습니다.")
    lines.append("")
    lines.append("## 요약")
    lines.append("")
    lines.append("| 우선순위 | 개수 | 비율 |")
    lines.append("|----------|------|------|")
    for level, items in [('HIGH', priority['high']), ('MEDIUM', priority['medium']), ('LOW', priority['low'])]:
        pct = len(items) / total_untranslated * 100 if total_untranslated > 0 else 0
        lines.append(f"| {level} | {len(items)} | {pct:.1f}% |")
    lines.append(f"| **총계** | **{total_untranslated}** | 100% |")
    lines.append("")

    # HIGH 우선순위
    lines.append("---")
    lines.append("")
    lines.append("## HIGH 우선순위 (Items.xml, Foods.xml)")
    lines.append("")
    lines.append("플레이어 인벤토리와 상호작용에서 자주 보이는 항목")
    lines.append("")
    if priority['high']:
        lines.append("| DisplayName | 출처 |")
        lines.append("|-------------|------|")
        for item in sorted(priority['high'], key=lambda x: x['original'].lower())[:100]:
            orig = item['original'].replace('|', '\\|')
            files = ', '.join(item['files'])
            lines.append(f"| `{orig}` | {files} |")
        if len(priority['high']) > 100:
            lines.append(f"| ... | ({len(priority['high']) - 100}개 더) |")
    else:
        lines.append("*없음*")
    lines.append("")

    # MEDIUM 우선순위
    lines.append("---")
    lines.append("")
    lines.append("## MEDIUM 우선순위 (Creatures.xml, Furniture.xml, Walls.xml)")
    lines.append("")
    lines.append("게임 환경에서 자주 보이는 항목")
    lines.append("")
    if priority['medium']:
        lines.append("| DisplayName | 출처 |")
        lines.append("|-------------|------|")
        for item in sorted(priority['medium'], key=lambda x: x['original'].lower())[:100]:
            orig = item['original'].replace('|', '\\|')
            files = ', '.join(item['files'])
            lines.append(f"| `{orig}` | {files} |")
        if len(priority['medium']) > 100:
            lines.append(f"| ... | ({len(priority['medium']) - 100}개 더) |")
    else:
        lines.append("*없음*")
    lines.append("")

    # LOW 우선순위
    lines.append("---")
    lines.append("")
    lines.append("## LOW 우선순위 (기타)")
    lines.append("")
    lines.append("기타 파일의 항목")
    lines.append("")
    if priority['low']:
        # 파일별 개수만 표시
        by_file = defaultdict(int)
        for item in priority['low']:
            for f in item['files']:
                by_file[f] += 1
        lines.append("| 파일 | 미번역 개수 |")
        lines.append("|------|-------------|")
        for f in sorted(by_file.keys()):
            lines.append(f"| {f} | {by_file[f]} |")
    else:
        lines.append("*없음*")
    lines.append("")

    return '\n'.join(lines)


def main():
    print("XML DisplayName 로드 중...")
    xml_items, summary = load_xml_display_names()
    print(f"  {len(xml_items)}개 항목")

    print("JSON 번역 로드 중...")
    translations = load_json_translations()
    print(f"  {len(translations)}개 번역")

    print("비교 중...")
    results = compare_and_categorize(xml_items, translations)

    print(f"  번역됨: {len(results['translated'])}")
    print(f"  미번역: {len(results['untranslated'])}")
    print(f"  부분 번역: {len(results['partial'])}")
    print(f"  템플릿: {len(results['template'])}")

    print("리포트 생성 중...")
    report = generate_report(results, summary, len(translations))

    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    with open(OUTPUT_FILE, 'w', encoding='utf-8') as f:
        f.write(report)
    print(f"저장됨: {OUTPUT_FILE}")

    # 우선순위 리포트 생성
    print("우선순위 리포트 생성 중...")
    priority = categorize_priority(results['untranslated'])
    priority_report = generate_priority_report(priority, len(results['untranslated']))

    with open(PRIORITY_FILE, 'w', encoding='utf-8') as f:
        f.write(priority_report)
    print(f"저장됨: {PRIORITY_FILE}")


if __name__ == "__main__":
    main()
