#!/usr/bin/env python3
"""
빠른 번역 검증 스크립트
SKILL.md에서 참조하는 간단한 검증 도우미
"""

import json
import sys
from pathlib import Path

def validate_json(file_path: str) -> tuple[bool, str]:
    """JSON 파일 구문 검증"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            json.load(f)
        return True, "OK"
    except json.JSONDecodeError as e:
        return False, f"JSON 오류: {e}"
    except Exception as e:
        return False, f"파일 오류: {e}"

def check_tags_in_value(value: str) -> list[str]:
    """번역문에 포함된 태그 검출 (경고용)"""
    import re
    tags = re.findall(r'\{\{[a-zA-Z]\|', value)
    return tags

def validate_glossary(file_path: str) -> list[str]:
    """용어집 검증"""
    issues = []
    
    valid, msg = validate_json(file_path)
    if not valid:
        issues.append(msg)
        return issues
    
    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    for category, terms in data.items():
        if not isinstance(terms, dict):
            continue
        for key, value in terms.items():
            if not isinstance(value, str):
                continue
            tags = check_tags_in_value(value)
            if tags:
                issues.append(f"⚠️  태그 포함: [{category}] '{key[:30]}...' → {tags}")
    
    return issues

def validate_mutation(file_path: str) -> list[str]:
    """변이 JSON 검증"""
    issues = []
    
    valid, msg = validate_json(file_path)
    if not valid:
        issues.append(msg)
        return issues
    
    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    # 필수 필드 확인
    if 'names' not in data:
        issues.append("❌ 'names' 필드 없음")
    
    # leveltext 배열 검증
    if 'leveltext' in data and not isinstance(data['leveltext'], list):
        issues.append("❌ 'leveltext'는 배열이어야 함")
    
    if 'leveltext_ko' in data:
        if not isinstance(data['leveltext_ko'], list):
            issues.append("❌ 'leveltext_ko'는 배열이어야 함")
        elif 'leveltext' in data:
            if len(data['leveltext']) != len(data['leveltext_ko']):
                issues.append(f"⚠️  leveltext 길이 불일치: {len(data['leveltext'])} vs {len(data['leveltext_ko'])}")
    
    return issues

def main():
    if len(sys.argv) < 2:
        print("사용법: python quick_validate.py <파일경로>")
        print("예시: python quick_validate.py LOCALIZATION/glossary_ui.json")
        sys.exit(1)
    
    file_path = sys.argv[1]
    path = Path(file_path)
    
    if not path.exists():
        print(f"❌ 파일 없음: {file_path}")
        sys.exit(1)
    
    print(f"검증 중: {file_path}")
    
    if 'MUTATIONS' in file_path or 'GENOTYPES' in file_path or 'SUBTYPES' in file_path:
        issues = validate_mutation(file_path)
    else:
        issues = validate_glossary(file_path)
    
    if issues:
        print(f"\n발견된 이슈 ({len(issues)}개):")
        for issue in issues:
            print(f"  {issue}")
        sys.exit(1)
    else:
        print("✅ 검증 통과!")
        sys.exit(0)

if __name__ == "__main__":
    main()
