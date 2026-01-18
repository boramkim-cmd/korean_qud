#!/usr/bin/env python3
"""
add_korean_translations.py
Adds Korean translations to the new mutation JSON structure
"""

import json
from pathlib import Path

# Translation map for common mutation description/leveltext patterns
TRANSLATIONS = {
    # Description patterns
    "You regulate your body's release of adrenaline.": "몸의 아드레날린 분비를 조절합니다.",
    "You can increase your body's adrenaline flow for 20 rounds.": "20턴 동안 몸의 아드레날린 흐름을 증가시킬 수 있습니다.",
    "While it's flowing, you gain +{{C|10}} quickness and other physical mutations gain +{{C|1}} rank.": "흐르는 동안 +{{C|10}} 신속성을 얻고 다른 물리적 변이가 +{{C|1}} 등급을 얻습니다.",
    "Cooldown: 200 rounds": "재사용 대기시간: 200턴",
}

def add_korean_to_file(json_path):
    """Add Korean translations to a mutation JSON file"""
    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    # Add Korean translations for names (keep existing)
    if 'names' in data:
        # Already has Korean, keep it
        pass
    
    # Add Korean description
    if 'description' in data and data['description'] in TRANSLATIONS:
        data['description_ko'] = TRANSLATIONS[data['description']]
    
    # Add Korean leveltext
    if 'leveltext' in data:
        leveltext_ko = []
        for line in data['leveltext']:
            if line in TRANSLATIONS:
                leveltext_ko.append(TRANSLATIONS[line])
            else:
                leveltext_ko.append(line)  # Keep English if no translation
        
        if leveltext_ko:
            data['leveltext_ko'] = leveltext_ko
    
    # Write back
    with open(json_path, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

def main():
    mutations_dir = Path("/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS")
    
    print("🔄 Adding Korean translations to mutation JSON files...")
    
    for json_file in mutations_dir.rglob("*.json"):
        add_korean_to_file(json_file)
        print(f"✅ {json_file.relative_to(mutations_dir)}")
    
    print("\n✅ 완료!")

if __name__ == "__main__":
    main()
