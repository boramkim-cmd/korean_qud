#!/usr/bin/env python3
"""
add_ko_fields.py
Adds description_ko and leveltext_ko fields to mutation JSON files
by moving existing Korean translations from names to new fields
"""

import json
import sys
from pathlib import Path

# Map of English to Korean for common patterns
KOREAN_MAP = {
    # Already have from old translations - need to preserve these
}

def process_mutation_file(json_path):
    """Add _ko fields to a mutation JSON file"""
    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    # Already has names with Korean
    korean_name = list(data.get('names', {}).values())[0] if data.get('names') else None
    
    # For now, add placeholder Korean translations
    # (Will be filled with actual translations in next step)
    if 'description' in data:
        # Placeholder: copy English for now
        data['description_ko'] = data['description']
    
    if 'leveltext' in data and len(data['leveltext']) > 0:
        # Placeholder: copy English for now
        data['leveltext_ko'] = data['leveltext'].copy()
    
    # Write back with new structure
    with open(json_path, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    
    return True

def main():
    """Add _ko fields to all mutation JSON files"""
    mutations_dir = Path("/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS")
    
    if not mutations_dir.exists():
        print(f"❌ Directory not found: {mutations_dir}")
        sys.exit(1)
    
    print("🔄 Adding _ko fields to mutation JSON files...\n")
    
    total = 0
    processed = 0
    
    for json_file in mutations_dir.rglob("*.json"):
        total += 1
        if process_mutation_file(json_file):
            processed += 1
            print(f"✅ {json_file.relative_to(mutations_dir)}")
    
    print(f"\n{'='*60}")
    print(f"총 파일: {total}개")
    print(f"처리 완료: {processed}개")
    print(f"\n⚠️  현재 _ko 필드는 영문 복사본입니다.")
    print(f"   다음 단계에서 실제 한글 번역으로 교체하세요.")

if __name__ == "__main__":
    main()
