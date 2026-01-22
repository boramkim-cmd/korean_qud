#!/usr/bin/env python3
"""
Remove empty description and description_ko fields from Caste and Calling JSON files.
These fields are not present in the game's original XML data.
"""

import json
from pathlib import Path

def main():
    # Folders containing Caste and Calling files
    folders = [
        'LOCALIZATION/CHARGEN/SUBTYPES/Castes',
        'LOCALIZATION/CHARGEN/SUBTYPES/Callings'
    ]
    
    updated_files = []
    
    for folder in folders:
        folder_path = Path(folder)
        if not folder_path.exists():
            print(f"Warning: Folder not found: {folder}")
            continue
            
        for json_file in folder_path.glob('*.json'):
            try:
                with open(json_file, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                
                modified = False
                
                # Remove empty description and description_ko
                if 'description' in data and data['description'] == '':
                    del data['description']
                    modified = True
                if 'description_ko' in data and data['description_ko'] == '':
                    del data['description_ko']
                    modified = True
                
                if modified:
                    with open(json_file, 'w', encoding='utf-8') as f:
                        json.dump(data, f, ensure_ascii=False, indent=2)
                    updated_files.append(str(json_file))
                    
            except Exception as e:
                print(f"Error processing {json_file}: {e}")
    
    print(f"Updated {len(updated_files)} files:")
    for f in updated_files:
        print(f"  - {f}")

if __name__ == "__main__":
    main()
