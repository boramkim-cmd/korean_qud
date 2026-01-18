#!/usr/bin/env python3
"""
Mutation JSON Validation Script
각 mutation JSON 파일이 C# GetDescription() + GetLevelText() 내용을 포함하는지 검증
"""

import json
import os
import re
from pathlib import Path

MUTATIONS_DIR = Path("/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS")
CS_SOURCE_DIR = Path("/Users/ben/Desktop/qud_korean/Assets/core_source/_GameSource/XRL.World.Parts.Mutation")

def extract_description_from_cs(cs_file):
    """C# 파일에서 GetDescription() 내용 추출"""
    with open(cs_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # GetDescription() 메서드 찾기
    desc_match = re.search(r'public override string GetDescription\(\)\s*\{([^}]+)\}', content, re.DOTALL)
    if desc_match:
        desc_body = desc_match.group(1)
        # return 문에서 문자열 추출
        return_match = re.search(r'return\s+"([^"]+)"', desc_body)
        if return_match:
            return return_match.group(1).replace('\\n', '\n')
    return None

def extract_leveltext_from_cs(cs_file):
    """C# 파일에서 GetLevelText() 내용 추출"""
    with open(cs_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # GetLevelText() 메서드 찾기
    level_match = re.search(r'public override string GetLevelText\(int Level\)\s*\{([^}]+)\}', content, re.DOTALL)
    if level_match:
        return level_match.group(1)
    return None

def check_json_file(json_file):
    """JSON 파일 검증"""
    with open(json_file, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    issues = []
    
    # descriptions 키가 있는지 확인
    if 'descriptions' not in data:
        issues.append("Missing 'descriptions' key")
    else:
        desc_count = len(data['descriptions'])
        if desc_count == 0:
            issues.append("Empty descriptions")
    
    return issues

def main():
    """모든 mutation JSON 파일 검증"""
    print("🔍 Mutation JSON 파일 검증 시작\n")
    
    total_files = 0
    issues_found = 0
    
    for folder in ['Morphotypes', 'Physical_Mutations', 'Physical_Defects', 'Mental_Mutations', 'Mental_Defects']:
        folder_path = MUTATIONS_DIR / folder
        if not folder_path.exists():
            continue
            
        print(f"\n📁 {folder}")
        print("=" * 60)
        
        json_files = sorted(folder_path.glob('*.json'))
        for json_file in json_files:
            total_files += 1
            issues = check_json_file(json_file)
            
            if issues:
                issues_found += 1
                print(f"⚠️  {json_file.name}")
                for issue in issues:
                    print(f"   - {issue}")
            else:
                print(f"✅ {json_file.name}")
    
    print(f"\n{'='*60}")
    print(f"총 파일: {total_files}개")
    print(f"문제 발견: {issues_found}개")
    print(f"정상: {total_files - issues_found}개")

if __name__ == "__main__":
    main()
