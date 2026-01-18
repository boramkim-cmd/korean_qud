#!/usr/bin/env python3
"""
populate_korean_translations.py
Extracts Korean translations from git history and populates _ko fields
"""

import json
import subprocess
from pathlib import Path
from collections import defaultdict

def get_old_translations_from_git():
    """Extract Korean translations from git history before conversion"""
    translations = {}
    
    # Get list of mutation files
    mutations_dir = Path("/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS")
    
    for json_file in mutations_dir.rglob("*.json"):
        relative_path = str(json_file.relative_to(Path("/Users/ben/Desktop/qud_korean")))
        
        try:
            # Get file from before conversion (HEAD~5 should be before our changes)
            result = subprocess.run(
                ["git", "show", f"HEAD~5:{relative_path}"],
                cwd="/Users/ben/Desktop/qud_korean",
                capture_output=True,
                text=True
            )
            
            if result.returncode == 0:
                old_data = json.loads(result.stdout)
                
                # Extract Korean from old "descriptions" structure
                if 'descriptions' in old_data:
                    mutation_name = list(old_data.get('names', {}).keys())[0] if old_data.get('names') else None
                    
                    if mutation_name:
                        ko_translations = list(old_data['descriptions'].values())
                        translations[mutation_name] = ko_translations
        except Exception as e:
            pass
    
    return translations

def populate_translations():
    """Populate _ko fields with actual Korean translations"""
    mutations_dir = Path("/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS")
    
    print("🔍 Extracting Korean translations from git history...\n")
    old_translations = get_old_translations_from_git()
    print(f"✅ Found translations for {len(old_translations)} mutations\n")
    
    print("📝 Populating _ko fields...\n")
    
    updated = 0
    for json_file in mutations_dir.rglob("*.json"):
        with open(json_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        mutation_name = list(data.get('names', {}).keys())[0] if data.get('names') else None
        
        if mutation_name and mutation_name in old_translations:
            ko_list = old_translations[mutation_name]
            
            # First item = description_ko
            if len(ko_list) > 0:
                data['description_ko'] = ko_list[0]
            
            # Rest = leveltext_ko
            if len(ko_list) > 1:
                data['leveltext_ko'] = ko_list[1:]
            
            with open(json_file, 'w', encoding='utf-8') as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
            
            updated += 1
            print(f"✅ {json_file.name}")
    
    print(f"\n{'='*60}")
    print(f"업데이트 완료: {updated}개")
    print(f"누락: {len(list(mutations_dir.rglob('*.json'))) - updated}개")

if __name__ == "__main__":
    populate_translations()
