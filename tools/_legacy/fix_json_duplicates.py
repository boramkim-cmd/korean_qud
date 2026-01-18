#!/usr/bin/env python3
"""
JSON 중복 키 제거 도구 (개선 버전)
- 중복 키 탐지 및 제거
- 마지막 값 유지
- 백업 자동 생성
"""

import json
import sys
import shutil
from pathlib import Path
from datetime import datetime
from collections import OrderedDict

def remove_duplicates(json_path):
    """JSON 파일에서 중복 키 제거"""
    json_path = Path(json_path)
    
    if not json_path.exists():
        print(f"❌ 파일 없음: {json_path}")
        return False
    
    # 백업
    backup_path = json_path.with_suffix('.json.bak')
    shutil.copy(json_path, backup_path)
    print(f"💾 백업: {backup_path}")
    
    # 로드
    with open(json_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # 중복 키 찾기
    duplicates_found = {}
    
    try:
        # JSON 파싱 (중복 키는 마지막 값으로 자동 덮어씀)
        data = json.loads(content, object_pairs_hook=OrderedDict)
        
        # 각 카테고리별로 중복 확인
        for category, entries in data.items():
            if isinstance(entries, dict):
                # 원본 content에서 이 카테고리의 키들을 찾아 중복 확인
                import re
                pattern = f'"{category}"\\s*:\\s*{{([^}}]*)}}'
                match = re.search(pattern, content, re.DOTALL)
                if match:
                    category_content = match.group(1)
                    keys = re.findall(r'"([^"]+)"\\s*:', category_content)
                    
                    # 중복 찾기
                    seen = {}
                    for key in keys:
                        if key in seen:
                            if category not in duplicates_found:
                                duplicates_found[category] = []
                            if key not in duplicates_found[category]:
                                duplicates_found[category].append(key)
                        seen[key] = True
        
        if duplicates_found:
            print(f"\n⚠️  중복 키 발견:")
            for cat, keys in duplicates_found.items():
                print(f"  [{cat}]: {len(keys)}개")
                for key in keys[:3]:  # 처음 3개만 표시
                    print(f"    - {key}")
                if len(keys) > 3:
                    print(f"    ... 외 {len(keys) - 3}개")
        
        # 저장 (중복은 자동으로 마지막 값으로 덮어씀)
        with open(json_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
        
        print(f"✅ 정리 완료: {json_path.name}")
        return True
        
    except json.JSONDecodeError as e:
        print(f"❌ JSON 파싱 오류: {e}")
        # 백업 복원
        shutil.copy(backup_path, json_path)
        return False

def clean_all_glossaries():
    """모든 glossary 파일 정리"""
    # 스크립트 위치 기준으로 LOCALIZATION 폴더 찾기
    loc_dir = Path(__file__).parent.parent / "LOCALIZATION"
    
    print("=" * 80)
    print("🧹 JSON 중복 키 제거")
    print("=" * 80 + "\n")
    
    success_count = 0
    fail_count = 0
    
    for json_file in sorted(loc_dir.glob("glossary_*.json")):
        print(f"\n📄 {json_file.name}")
        if remove_duplicates(json_file):
            success_count += 1
        else:
            fail_count += 1
    
    print("\n" + "=" * 80)
    print(f"✅ 성공: {success_count}개")
    if fail_count > 0:
        print(f"❌ 실패: {fail_count}개")
    print("=" * 80)

if __name__ == "__main__":
    if len(sys.argv) > 1:
        # 특정 파일만 처리
        remove_duplicates(sys.argv[1])
    else:
        # 모든 glossary 파일 처리
        clean_all_glossaries()
