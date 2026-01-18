#!/usr/bin/env python3
"""
manual_translation_template.py
Creates a translation template from current English text for manual translation
"""

import json
from pathlib import Path

def create_translation_template():
    """Create a template file for manual Korean translation"""
    mutations_dir = Path("/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS")
    output_file = Path("/Users/ben/Desktop/qud_korean/TRANSLATION_TEMPLATE.txt")
    
    translations_needed = []
    
    for json_file in sorted(mutations_dir.rglob("*.json")):
        with open(json_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        mutation_name = list(data.get('names', {}).keys())[0] if data.get('names') else "Unknown"
        korean_name = list(data.get('names', {}).values())[0] if data.get('names') else ""
        
        entry = f"\n## {mutation_name} ({korean_name})\n"
        entry += f"File: {json_file.name}\n\n"
        
        if 'description' in data:
            entry += f"EN: {data['description']}\n"
            entry += f"KO: \n\n"
        
        if 'leveltext' in data:
            for i, line in enumerate(data['leveltext'], 1):
                entry += f"EN{i}: {line}\n"
                entry += f"KO{i}: \n"
        
        translations_needed.append(entry)
    
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write("# Mutation Translation Template\n")
        f.write("# Fill in the KO: lines with Korean translations\n")
        f.write("="*60 + "\n")
        f.writelines(translations_needed)
    
    print(f"✅ Translation template created: {output_file}")
    print(f"📝 Total mutations: {len(translations_needed)}")
    print(f"\n⚠️  이 파일을 열어서 KO: 줄에 한글 번역을 입력하세요.")
    print(f"   완료 후 다른 스크립트로 JSON에 자동 적용할 수 있습니다.")

if __name__ == "__main__":
    create_translation_template()
