#!/usr/bin/env python3
"""
convert_mutation_json.py
Converts mutation JSON files from current structure to new description + leveltext structure
"""

import json
import sys
from pathlib import Path

def convert_mutation_file(json_path):
    """Convert a single mutation JSON file"""
    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    if 'names' not in data or 'descriptions' not in data:
        print(f"⚠️  Skipping {json_path.name}: missing names or descriptions")
        return False
    
    # Extract description items
    desc_items = list(data['descriptions'].items())
    if len(desc_items) == 0:
        print(f"⚠️  Skipping {json_path.name}: empty descriptions")
        return False
    
    # First item = description
    # Rest = leveltext (array)
    new_data = {
        'names': data['names'],
        'description': desc_items[0][0],  # English text
        'leveltext': [item[0] for item in desc_items[1:]]  # English text array
    }
    
    # Write back
    with open(json_path, 'w', encoding='utf-8') as f:
        json.dump(new_data, f, ensure_ascii=False, indent=2)
    
    return True

def main():
    """Convert all mutation JSON files"""
    mutations_dir = Path("/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS")
    
    if not mutations_dir.exists():
        print(f"❌ Directory not found: {mutations_dir}")
        sys.exit(1)
    
    print("🔄 Converting mutation JSON files to new structure...\n")
    
    total = 0
    converted = 0
    skipped = 0
    
    for json_file in mutations_dir.rglob("*.json"):
        total += 1
        if convert_mutation_file(json_file):
            converted += 1
            print(f"✅ {json_file.relative_to(mutations_dir)}")
        else:
            skipped += 1
    
    print(f"\n{'='*60}")
    print(f"총 파일: {total}개")
    print(f"변환 완료: {converted}개")
    print(f"건너뜀: {skipped}개")
    
    if converted > 0:
        print(f"\n✅ 변환 완료! 게임에서 테스트하세요.")
    else:
        print(f"\n⚠️  변환된 파일이 없습니다.")

if __name__ == "__main__":
    main()
