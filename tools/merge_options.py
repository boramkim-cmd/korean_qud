#!/usr/bin/env python3
"""
Options.xml 번역 데이터를 C# OptionsData.cs로 변환하는 도구.

사용법:
    python merge_options.py > Scripts/Data/OptionsData.cs
"""

from __future__ import annotations
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Set

# 경로 설정
SCRIPT_DIR = Path(__file__).parent
PROJECT_ROOT = SCRIPT_DIR.parent
TRANSLATIONS_FILE = SCRIPT_DIR / "options_translations.json"
OPTIONS_XML = PROJECT_ROOT / "Assets/StreamingAssets/Base/Options.xml"

# 정규표현식 (한 번만 컴파일)
TAG_PATTERN = re.compile(r'(<[^>]+>|\{\{[^}]+\}\})')
SPLIT_PATTERN = re.compile(r'[,|]')


def strip_tags(text: str) -> str:
    """XML/Unity 태그 제거"""
    return TAG_PATTERN.sub('', text).strip()


def load_translations() -> dict[str, str]:
    """번역 데이터를 JSON에서 로드"""
    if not TRANSLATIONS_FILE.exists():
        print(f"⚠️  번역 파일 없음: {TRANSLATIONS_FILE}", file=sys.stderr)
        return {}
    
    try:
        data = json.loads(TRANSLATIONS_FILE.read_text(encoding='utf-8'))
        # options와 helptext를 병합
        translations = {}
        for section in ('options', 'helptext'):
            if section in data:
                translations.update(data[section])
        return translations
    except (json.JSONDecodeError, IOError) as e:
        print(f"⚠️  번역 파일 로드 오류: {e}", file=sys.stderr)
        return {}


def extract_xml_keys(xml_path: Path) -> Set[str]:
    """Options.xml에서 번역 대상 키 추출"""
    if not xml_path.exists():
        print(f"⚠️  XML 파일 없음: {xml_path}", file=sys.stderr)
        return set()
    
    try:
        tree = ET.parse(xml_path)
    except ET.ParseError as e:
        print(f"⚠️  XML 파싱 오류: {e}", file=sys.stderr)
        return set()
    
    keys: Set[str] = set()
    
    for opt in tree.findall('.//option'):
        # DisplayText 추출
        display_text = opt.get('DisplayText')
        if display_text:
            keys.add(display_text)
            keys.add(display_text.upper())
            stripped = strip_tags(display_text)
            keys.add(stripped)
            keys.add(stripped.upper())
        
        # helptext 추출
        help_elem = opt.find('helptext')
        if help_elem is not None and help_elem.text:
            keys.add(help_elem.text.strip())
        
        # DisplayValues/Values 추출
        for attr in ('DisplayValues', 'Values'):
            val = opt.get(attr)
            if val and not val.startswith('*'):
                for part in SPLIT_PATTERN.split(val):
                    part = part.strip()
                    if part:
                        keys.add(part)
    
    return keys


def build_translation_dict(keys: Set[str], translations: dict[str, str]) -> dict[str, str]:
    """키와 번역을 매칭하여 최종 사전 생성"""
    result: dict[str, str] = {}
    
    # 대소문자 무시 검색용 인덱스
    lower_index = {k.lower(): v for k, v in translations.items()}
    
    for key in keys:
        # 정확한 매칭 우선
        if key in translations:
            result[key] = translations[key]
        # 대소문자 무시 매칭
        elif key.lower() in lower_index:
            result[key] = lower_index[key.lower()]
        else:
            result[key] = ""  # 미번역
    
    return result


def escape_csharp(text: str) -> str:
    """C# 문자열 이스케이프"""
    return text.replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n')


def generate_csharp(translations: dict[str, str]) -> str:
    """C# 소스 코드 생성"""
    lines = [
        "/*",
        " * 파일명: OptionsData.cs",
        " * 분류: [Data] 설정 화면 텍스트",
        " * 역할: 게임 설정(Options) 화면의 카테고리 및 설정 항목 텍스트를 정의합니다.",
        " * 생성: merge_options.py에 의해 자동 생성됨",
        " */",
        "",
        "using System.Collections.Generic;",
        "",
        "namespace QudKRTranslation.Data",
        "{",
        "    public static class OptionsData",
        "    {",
        "        // 기본 번역 데이터",
        "        private static readonly Dictionary<string, string> baseTranslations = new Dictionary<string, string>()",
        "        {"
    ]
    
    sorted_items = sorted(translations.items())
    for i, (key, value) in enumerate(sorted_items):
        key_escaped = escape_csharp(key)
        val_escaped = escape_csharp(value)
        comma = "," if i < len(sorted_items) - 1 else ""
        lines.append(f'            {{ "{key_escaped}", "{val_escaped}" }}{comma}')
    
    lines.extend([
        "        };",
        "",
        "        // 동적으로 생성된 공개 Dictionary (기존 코드와 호환)",
        "        public static Dictionary<string, string> Translations { get; }",
        "",
        "        static OptionsData()",
        "        {",
        "            Translations = new Dictionary<string, string>();",
        "",
        "            foreach (var kvp in baseTranslations)",
        "            {",
        "                // 원본 추가",
        "                Translations[kvp.Key] = kvp.Value;",
        "",
        "                // 컬러 코드 버전 추가 (헤더 등에서 사용됨)",
        "                if (!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))",
        "                {",
        '                    string coloredKey = $"<color=#77BFCFFF>{kvp.Key}</color>";',
        '                    string coloredValue = $"<color=#77BFCFFF>{kvp.Value}</color>";',
        "                    Translations[coloredKey] = coloredValue;",
        "                }",
        "            }",
        "        }",
        "    }",
        "}"
    ])
    
    return "\n".join(lines)


def main() -> None:
    # 1. 번역 데이터 로드
    translations = load_translations()
    if not translations:
        print("❌ 번역 데이터가 없습니다.", file=sys.stderr)
        sys.exit(1)
    
    # 2. XML에서 키 추출
    xml_keys = extract_xml_keys(OPTIONS_XML)
    if not xml_keys:
        print("⚠️  XML에서 추출된 키가 없습니다.", file=sys.stderr)
    
    # 3. 번역 매칭
    final_dict = build_translation_dict(xml_keys, translations)
    
    # 4. C# 코드 생성 및 출력
    print(generate_csharp(final_dict))
    
    # 통계 출력 (stderr로)
    translated = sum(1 for v in final_dict.values() if v)
    print(f"\n// 통계: {translated}/{len(final_dict)} 항목 번역됨", file=sys.stderr)


if __name__ == "__main__":
    main()
