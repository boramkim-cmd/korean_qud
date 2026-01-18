#!/usr/bin/env python3
"""
📊 Glossary 교차 검증 및 분석 도구 (Glossary Cross-Validation Tool)
- 중복 키 탐지 (파일 간 / 파일 내)
- 대소문자 중복 탐지
- CS 코드 의존성 분석
- 카테고리 구조 분석
"""

from __future__ import annotations
import json
import re
from collections import defaultdict
from pathlib import Path
from typing import Any

# ============================================================================
# 설정
# ============================================================================

PROJECT_ROOT = Path(__file__).parent.parent.resolve()
LOCALIZATION_DIR = PROJECT_ROOT / "LOCALIZATION"
SCRIPTS_DIR = PROJECT_ROOT / "Scripts"
REPORT_FILE = LOCALIZATION_DIR / "integrity_report.md"

# CS에서 참조하는 카테고리 (코드 분석 결과)
CS_REFERENCED_CATEGORIES = {
    "common", "ui", "status", "inventory", "options", "display",
    "chargen_ui", "attribute", "skill", "powers", "skill_desc"
}


def load_all_glossaries() -> dict[str, dict[str, Any]]:
    """모든 glossary JSON 파일 로드"""
    glossaries = {}
    for json_file in sorted(LOCALIZATION_DIR.glob("glossary_*.json")):
        try:
            data = json.loads(json_file.read_text(encoding='utf-8'))
            glossaries[json_file.name] = data
        except json.JSONDecodeError as e:
            print(f"⚠️  JSON 오류 ({json_file.name}): {e}")
    return glossaries


def analyze_duplicates(glossaries: dict[str, dict[str, Any]]) -> dict[str, list]:
    """중복 키 분석"""
    # 1. 전역 키-위치 맵핑
    global_keys: dict[str, list[str]] = defaultdict(list)  # key -> [(file, category), ...]
    
    # 2. 대소문자 중복 추적
    case_insensitive: dict[str, list[tuple[str, str, str]]] = defaultdict(list)  # lower_key -> [(file, cat, original_key), ...]
    
    for filename, data in glossaries.items():
        for category, entries in data.items():
            if not isinstance(entries, dict):
                continue
            for key in entries.keys():
                location = f"{filename}:{category}"
                global_keys[key].append(location)
                case_insensitive[key.lower()].append((filename, category, key))
    
    # 정확한 중복 찾기
    exact_duplicates = {k: v for k, v in global_keys.items() if len(v) > 1}
    
    # 대소문자만 다른 중복 찾기 (정확한 중복 제외)
    case_duplicates = {}
    for lower_key, locations in case_insensitive.items():
        if len(locations) > 1:
            unique_originals = set(loc[2] for loc in locations)
            if len(unique_originals) > 1:  # 대소문자가 실제로 다른 경우
                case_duplicates[lower_key] = locations
    
    return {
        "exact": exact_duplicates,
        "case": case_duplicates
    }


def analyze_structure(glossaries: dict[str, dict[str, Any]]) -> dict[str, Any]:
    """구조 분석"""
    structure = {}
    for filename, data in glossaries.items():
        file_info = {"categories": {}, "total_entries": 0}
        for category, entries in data.items():
            if category.startswith("_"):  # 메타데이터 스킵
                continue
            if isinstance(entries, dict):
                count = len(entries)
                file_info["categories"][category] = count
                file_info["total_entries"] += count
        structure[filename] = file_info
    return structure


def analyze_cs_dependencies() -> dict[str, list[str]]:
    """CS 파일에서 참조하는 카테고리 분석"""
    dependencies: dict[str, list[str]] = defaultdict(list)
    pattern = re.compile(r'LocalizationManager\.GetCategory\("([^"]+)"\)')
    
    for cs_file in SCRIPTS_DIR.rglob("*.cs"):
        try:
            content = cs_file.read_text(encoding='utf-8')
            for match in pattern.finditer(content):
                category = match.group(1)
                rel_path = str(cs_file.relative_to(PROJECT_ROOT))
                if rel_path not in dependencies[category]:
                    dependencies[category].append(rel_path)
        except IOError:
            continue
    
    return dict(dependencies)


def find_missing_categories(glossaries: dict, cs_deps: dict) -> tuple[set, set]:
    """CS에서 참조하지만 glossary에 없는 카테고리 찾기"""
    existing_categories = set()
    for data in glossaries.values():
        existing_categories.update(data.keys())
    
    referenced = set(cs_deps.keys())
    missing = referenced - existing_categories
    unused = existing_categories - referenced - {"_meta"}
    
    return missing, unused


def generate_report(
    glossaries: dict,
    duplicates: dict,
    structure: dict,
    cs_deps: dict,
    missing: set,
    unused: set
) -> str:
    """마크다운 리포트 생성"""
    lines = [
        "# 📊 Glossary 무결성 리포트",
        "",
        f"**생성 시각**: {__import__('datetime').datetime.now().strftime('%Y-%m-%d %H:%M:%S')}",
        "",
        "---",
        "",
        "## 📁 파일별 구조",
        "",
        "| 파일 | 카테고리 수 | 항목 수 |",
        "|------|----------:|------:|"
    ]
    
    total_entries = 0
    for filename, info in sorted(structure.items()):
        cat_count = len(info["categories"])
        entry_count = info["total_entries"]
        total_entries += entry_count
        lines.append(f"| `{filename}` | {cat_count} | {entry_count} |")
    
    lines.append(f"| **총계** | | **{total_entries}** |")
    lines.append("")
    
    # 카테고리 상세
    lines.extend([
        "### 카테고리 상세",
        ""
    ])
    
    for filename, info in sorted(structure.items()):
        if info["categories"]:
            lines.append(f"**{filename}**:")
            for cat, count in sorted(info["categories"].items()):
                lines.append(f"  - `{cat}`: {count}개")
            lines.append("")
    
    # 중복 분석
    lines.extend([
        "---",
        "",
        "## ⚠️ 중복 키 분석",
        ""
    ])
    
    if duplicates["exact"]:
        lines.append(f"### 정확한 중복: {len(duplicates['exact'])}개")
        lines.append("")
        for key, locations in sorted(duplicates["exact"].items())[:20]:
            lines.append(f"- `{key[:50]}{'...' if len(key) > 50 else ''}`")
            for loc in locations:
                lines.append(f"  - {loc}")
        if len(duplicates["exact"]) > 20:
            lines.append(f"- ... 외 {len(duplicates['exact']) - 20}개")
        lines.append("")
    else:
        lines.append("✅ 정확한 중복 키 없음")
        lines.append("")
    
    if duplicates["case"]:
        lines.append(f"### 대소문자만 다른 중복: {len(duplicates['case'])}개")
        lines.append("")
        lines.append("> 이 항목들은 Options 화면 등에서 대소문자 모두 필요할 수 있습니다.")
        lines.append("")
        for lower_key, locations in sorted(duplicates["case"].items())[:15]:
            variants = list(set(loc[2] for loc in locations))
            lines.append(f"- `{lower_key[:40]}`: {len(variants)}개 변형")
            for v in variants[:3]:
                lines.append(f"  - `{v}`")
        if len(duplicates["case"]) > 15:
            lines.append(f"- ... 외 {len(duplicates['case']) - 15}개")
        lines.append("")
    
    # CS 의존성
    lines.extend([
        "---",
        "",
        "## 🔗 CS 코드 의존성",
        "",
        "코드에서 `LocalizationManager.GetCategory()`로 참조하는 카테고리:",
        ""
    ])
    
    for category, files in sorted(cs_deps.items()):
        lines.append(f"### `{category}`")
        for f in files[:5]:
            lines.append(f"- {f}")
        if len(files) > 5:
            lines.append(f"- ... 외 {len(files) - 5}개 파일")
        lines.append("")
    
    # 문제점
    lines.extend([
        "---",
        "",
        "## 🔴 발견된 문제점",
        ""
    ])
    
    if missing:
        lines.append("### 누락된 카테고리 (CS에서 참조하지만 glossary에 없음)")
        for cat in sorted(missing):
            lines.append(f"- ❌ `{cat}`")
        lines.append("")
    
    if unused:
        lines.append("### 미사용 카테고리 (glossary에 있지만 CS에서 참조 안 함)")
        for cat in sorted(unused):
            lines.append(f"- ⚪ `{cat}`")
        lines.append("")
    
    # 권장 사항
    lines.extend([
        "---",
        "",
        "## 💡 권장 구조 개편",
        "",
        "현재 파일들을 화면 기준으로 재구성하면 다음과 같습니다:",
        "",
        "```",
        "LOCALIZATION/",
        "├── screens/              # 화면별 번역",
        "│   ├── mainmenu.json     # 메인 메뉴",
        "│   ├── options.json      # 설정 화면",
        "│   ├── chargen.json      # 캐릭터 생성",
        "│   ├── gameplay.json     # 게임플레이 UI",
        "│   └── inventory.json    # 인벤토리/장비",
        "├── data/                 # 게임 데이터",
        "│   ├── skills.json       # 스킬",
        "│   ├── mutations.json    # 변이",
        "│   ├── cybernetics.json  # 사이버네틱스",
        "│   └── factions.json     # 세력",
        "├── shared/               # 공용 용어",
        "│   ├── common.json       # 공용 UI",
        "│   └── terms.json        # 게임 용어",
        "└── SUBTYPES/             # 기존 하위유형 (유지)",
        "    └── ...               ",
        "```",
        ""
    ])
    
    return "\n".join(lines)


def main() -> None:
    print("=" * 60)
    print("📊 Glossary 교차 검증 시작")
    print("=" * 60)
    
    # 1. 데이터 로드
    print("\n1️⃣ Glossary 파일 로드 중...")
    glossaries = load_all_glossaries()
    print(f"   → {len(glossaries)}개 파일 로드됨")
    
    # 2. 구조 분석
    print("\n2️⃣ 구조 분석 중...")
    structure = analyze_structure(glossaries)
    total = sum(info["total_entries"] for info in structure.values())
    print(f"   → 총 {total}개 번역 항목")
    
    # 3. 중복 분석
    print("\n3️⃣ 중복 키 분석 중...")
    duplicates = analyze_duplicates(glossaries)
    print(f"   → 정확한 중복: {len(duplicates['exact'])}개")
    print(f"   → 대소문자 중복: {len(duplicates['case'])}개")
    
    # 4. CS 의존성 분석
    print("\n4️⃣ CS 코드 의존성 분석 중...")
    cs_deps = analyze_cs_dependencies()
    print(f"   → {len(cs_deps)}개 카테고리가 코드에서 참조됨")
    
    # 5. 누락/미사용 카테고리
    print("\n5️⃣ 카테고리 매핑 검증 중...")
    missing, unused = find_missing_categories(glossaries, cs_deps)
    if missing:
        print(f"   ⚠️  누락 카테고리: {', '.join(missing)}")
    if unused:
        print(f"   ℹ️  미사용 카테고리: {len(unused)}개")
    
    # 6. 리포트 생성
    print("\n6️⃣ 리포트 생성 중...")
    report = generate_report(glossaries, duplicates, structure, cs_deps, missing, unused)
    REPORT_FILE.write_text(report, encoding='utf-8')
    print(f"   → 저장됨: {REPORT_FILE.relative_to(PROJECT_ROOT)}")
    
    print("\n" + "=" * 60)
    print("✅ 교차 검증 완료!")
    print("=" * 60)


if __name__ == "__main__":
    main()
