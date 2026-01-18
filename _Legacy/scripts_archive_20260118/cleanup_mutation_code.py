#!/usr/bin/env python3
"""
Clean up mutation JSON files by removing C# code fragments
that were incorrectly extracted as translation strings
"""
import os
import json
import re

MUTATIONS_DIR = "/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS"

def is_code_fragment(text):
    """Check if text appears to be C# code rather than game text"""
    code_indicators = [
        'public override',
        'private void',
        'return obj',
        '\\t{\\n',
        'base.DeepCopy',
        'GameObject Parent',
        'Func<',
        ') as ',
        'obj.',
        ' = new ',
        'Level / 3);',
        'string baseDamage',
        'int aV = ',
    ]
    
    for indicator in code_indicators:
        if indicator in text:
            return True
    
    # Check if it's very long (>500 chars) with lots of code-like syntax
    if len(text) > 500 and ('{' in text or ';' in text):
        return True
        
    return False

def clean_mutation_file(filepath):
    """Remove code fragments from a mutation JSON file"""
    with open(filepath, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    changed = False
    
    for category, items in data.items():
        if not isinstance(items, dict):
            continue
            
        keys_to_remove = []
        
        for key, value in items.items():
            if is_code_fragment(key) or is_code_fragment(value):
                keys_to_remove.append(key)
                changed = True
        
        for key in keys_to_remove:
            del items[key]
            print(f"  Removed code fragment from {category}: {key[:80]}...")
    
    # Remove empty categories
    empty_cats = [cat for cat, items in data.items() if isinstance(items, dict) and not items]
    for cat in empty_cats:
        del data[cat]
        if empty_cats:
            changed = True
    
    if changed:
        with open(filepath, 'w', encoding='utf-8') as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
        return True
    return False

def main():
    print("Cleaning mutation JSON files...")
    cleaned_count = 0
    
    for filename in sorted(os.listdir(MUTATIONS_DIR)):
        if not filename.endswith('.json'):
            continue
            
        filepath = os.path.join(MUTATIONS_DIR, filename)
        print(f"\nChecking {filename}...")
        
        try:
            if clean_mutation_file(filepath):
                cleaned_count += 1
                print(f"  ✓ Cleaned {filename}")
        except Exception as e:
            print(f"  ✗ Error processing {filename}: {e}")
    
    print(f"\n✅ Cleanup complete. Modified {cleaned_count} files.")

if __name__ == "__main__":
    main()
