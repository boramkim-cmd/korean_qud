#!/usr/bin/env python3
"""
JSON 정렬 및 포맷팅 도구
- 카테고리를 알파벳 순으로 정렬
- 각 카테고리 내부의 키를 알파벳 순으로 정렬
- 들여쓰기 2칸, 한글 깨짐 방지
"""

import json
import sys
from pathlib import Path
from collections import OrderedDict

def sort_json(file_path):
    path = Path(file_path)
    if not path.exists():
        print(f"❌ 파일 없음: {file_path}")
        return

    try:
        with open(path, 'r', encoding='utf-8') as f:
            data = json.load(f, object_pairs_hook=OrderedDict)

        # 1. 카테고리 정렬
        sorted_data = OrderedDict()
        for cat in sorted(data.keys()):
            items = data[cat]
            if isinstance(items, dict):
                # 2. 키 정렬
                sorted_items = OrderedDict()
                for key in sorted(items.keys()):
                    sorted_items[key] = items[key]
                sorted_data[cat] = sorted_items
            else:
                sorted_data[cat] = items

        # 저장
        with open(path, 'w', encoding='utf-8') as f:
            json.dump(sorted_data, f, ensure_ascii=False, indent=2)
        
        print(f"✅ 정렬 완료: {path.name}")
    except Exception as e:
        print(f"❌ 오류 발생 ({path.name}): {e}")

if __name__ == "__main__":
    if len(sys.argv) > 1:
        for arg in sys.argv[1:]:
            sort_json(arg)
    else:
        # 기본 디렉토리의 모든 glossary 정리
        loc_dir = Path(__file__).parent.parent / "LOCALIZATION"
        for f in loc_dir.glob("glossary_*.json"):
            sort_json(f)
