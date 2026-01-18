#!/usr/bin/env python3
"""
JSON 정렬 및 포맷팅 도구
- 카테고리를 알파벳 순으로 정렬
- 각 카테고리 내부의 키를 알파벳 순으로 정렬
- 들여쓰기 2칸, 한글 깨짐 방지
"""

from __future__ import annotations
import json
import sys
from pathlib import Path
from typing import Any


def sort_json(file_path: str | Path) -> bool:
    """JSON 파일을 정렬하여 저장. 성공 시 True 반환."""
    path = Path(file_path)
    
    if not path.exists():
        print(f"❌ 파일 없음: {file_path}")
        return False

    try:
        data: dict[str, Any] = json.loads(path.read_text(encoding='utf-8'))
    except json.JSONDecodeError as e:
        print(f"❌ JSON 파싱 오류 ({path.name}): {e}")
        return False
    except IOError as e:
        print(f"❌ 파일 읽기 오류 ({path.name}): {e}")
        return False

    # Python 3.7+ dict는 삽입 순서 보장 → OrderedDict 불필요
    sorted_data: dict[str, Any] = {}
    
    for cat in sorted(data.keys()):
        items = data[cat]
        if isinstance(items, dict):
            # 카테고리 내 키 정렬
            sorted_data[cat] = {k: items[k] for k in sorted(items.keys())}
        else:
            sorted_data[cat] = items

    try:
        path.write_text(
            json.dumps(sorted_data, ensure_ascii=False, indent=2),
            encoding='utf-8'
        )
        print(f"✅ 정렬 완료: {path.name}")
        return True
    except IOError as e:
        print(f"❌ 파일 저장 오류 ({path.name}): {e}")
        return False


def main() -> None:
    if len(sys.argv) > 1:
        for arg in sys.argv[1:]:
            sort_json(arg)
    else:
        # 기본: LOCALIZATION 디렉토리의 모든 glossary 정리
        loc_dir = Path(__file__).parent.parent / "LOCALIZATION"
        glossary_files = sorted(loc_dir.glob("glossary_*.json"))
        
        if not glossary_files:
            print("⚠️  glossary_*.json 파일이 없습니다.")
            return
            
        for f in glossary_files:
            sort_json(f)


if __name__ == "__main__":
    main()
