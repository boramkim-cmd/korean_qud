#!/usr/bin/env python3
"""
Sync options translations from tools/options_translations.json 
into LOCALIZATION/UI/options.json

This is the ROOT FIX for the options translation issue.
"""

import json
import re
import subprocess
import os
from pathlib import Path

def main():
    base_dir = Path(__file__).parent.parent
    os.chdir(base_dir)
    
    print("=" * 60)
    print("Options Translation Sync Tool")
    print("=" * 60)
    
    # 1. Extract all DisplayText from game Options.xml
    result = subprocess.run(
        ['grep', 'DisplayText=', 'Assets/StreamingAssets/Base/Options.xml'],
        capture_output=True, text=True
    )
    
    game_options = set()
    for line in result.stdout.strip().split('\n'):
        match = re.search(r'DisplayText="([^"]+)"', line)
        if match:
            text = match.group(1)
            # Decode HTML entities
            text = text.replace('&amp;', '&').replace('&lt;', '<').replace('&gt;', '>')
            game_options.add(text)
    
    print(f'[1] 게임 Options.xml에서 추출: {len(game_options)}개')
    
    # 2. Load tools translations
    tools_path = base_dir / 'tools' / 'options_translations.json'
    with open(tools_path, 'r', encoding='utf-8') as f:
        tools_data = json.load(f)
    tools_options = tools_data.get('options', {})
    print(f'[2] tools 번역 데이터: {len(tools_options)}개')
    
    # 3. Load current LOCALIZATION
    loc_path = base_dir / 'LOCALIZATION' / 'UI' / 'options.json'
    with open(loc_path, 'r', encoding='utf-8') as f:
        loc_data = json.load(f)
    loc_options = loc_data.get('options', {})
    original_count = len(loc_options)
    print(f'[3] 현재 LOCALIZATION 데이터: {original_count}개')
    
    # 4. Merge: add from tools what's missing in loc
    added = []
    for key, value in tools_options.items():
        if key not in loc_options:
            loc_options[key] = value
            added.append(key)
    
    print(f'\n[4] tools에서 추가된 항목: {len(added)}개')
    for key in added:
        print(f'    + {key}')
    
    # 5. Check what's still missing - also try stripped version
    still_missing = []
    for opt in game_options:
        opt_stripped = opt.strip()
        # Check exact, uppercase, and stripped versions
        if opt in loc_options:
            continue
        if opt.upper() in loc_options:
            continue
        if opt_stripped in loc_options:
            # Add trailing space version with same translation
            loc_options[opt] = loc_options[opt_stripped]
            added.append(f'{opt} (from stripped)')
            continue
        if opt_stripped.upper() in loc_options:
            loc_options[opt] = loc_options[opt_stripped.upper()]
            added.append(f'{opt} (from stripped upper)')
            continue
        still_missing.append(opt)
    
    # 6. Manually add known missing translations
    manual_additions = {
        'Nearby objects list: show liquid pools': '근처 객체 목록: 액체 웅덩이 표시',
        'NEARBY OBJECTS LIST: SHOW LIQUID POOLS': '근처 객체 목록: 액체 웅덩이 표시',
        'Nearby objects list:   show liquid pools': '근처 객체 목록: 액체 웅덩이 표시',
        'NEARBY OBJECTS LIST:   SHOW LIQUID POOLS': '근처 객체 목록: 액체 웅덩이 표시',
    }
    
    for key, value in manual_additions.items():
        if key not in loc_options:
            loc_options[key] = value
            added.append(f'{key} (manual)')
    
    # Re-check still missing
    still_missing = []
    for opt in game_options:
        if opt not in loc_options and opt.upper() not in loc_options and opt.strip() not in loc_options:
            still_missing.append(opt)
    
    if still_missing:
        print(f'\n[5] ⚠️ 번역이 없는 게임 옵션: {len(still_missing)}개')
        for m in still_missing:
            print(f'    - "{m}"')
    else:
        print(f'\n[5] ✅ 모든 게임 옵션에 번역 있음!')
    
    # 7. Save
    loc_data['options'] = loc_options
    with open(loc_path, 'w', encoding='utf-8') as f:
        json.dump(loc_data, f, ensure_ascii=False, indent=2)
    
    print(f'\n[6] ✅ 저장 완료: {loc_path}')
    print(f'    {original_count}개 → {len(loc_options)}개')
    print("=" * 60)

if __name__ == '__main__':
    main()
