#!/usr/bin/env python3
import sys
import re
import os

# 1. Read existing Options.cs to preserve translations
options_cs_path = "Data_QudKRContent/Scripts/01_Data/Options.cs"
existing_translations = {}

if os.path.exists(options_cs_path):
    with open(options_cs_path, 'r', encoding='utf-8') as f:
        content = f.read()
        # Regex to find { "Key", "Value" }
        matches = re.findall(r'\{\s*"(.*?)",\s*"(.*?)"\s*\}', content)
        for k, v in matches:
            # Unescape C# string format if necessary (basic handling)
            k = k.replace('\\"', '"')
            v = v.replace('\\"', '"')
            existing_translations[k] = v

# 2. Get keys from extraction script (we'll just run the extraction logic here again for simplicity)
import xml.etree.ElementTree as ET

def strip_tags(s):
    return re.sub(r'(<[^>]+>|\{\{[^}]+\}\})', '', s).strip()

def get_keys(path):
    tree = ET.parse(path)
    root = tree.getroot()
    keys = set()
    
    for opt in root.findall('.//option'):
        dt = opt.get('DisplayText')
        if dt:
            keys.add(dt)
            keys.add(dt.upper())
            keys.add(strip_tags(dt))
            if "color" not in dt and not dt.startswith("<"):
                 keys.add(f"<color=#77BFCFFF>{dt.upper()}</color>")

        help_el = opt.find('helptext')
        if help_el is not None and help_el.text:
            keys.add(help_el.text.strip())

        for attr in ['DisplayValues', 'Values']:
            val = opt.get(attr)
            if val and not val.startswith('*'): 
                parts = re.split(r'[,\|]', val)
                for p in parts:
                    if p.strip():
                        keys.add(p.strip())
    return keys

xml_path = "Assets/StreamingAssets/Base/Options.xml"
new_keys = get_keys(xml_path)

# 3. Merge
# Add new keys to existing dictionary if they don't exist
final_dict = existing_translations.copy()
for k in new_keys:
    if k not in final_dict:
        # Try to be smart: if we have "sound" translated as "사운드", maybe "SOUND" should be "사운드"
        # Simple case insensitive lookup fallback
        lower_k = k.lower()
        # Find if we have a translation for the lowercase version
        found_translation = None
        for ek, ev in existing_translations.items():
            if ek.lower() == lower_k and ev:
                found_translation = ev
                break
        
        if found_translation:
            final_dict[k] = found_translation
        else:
            final_dict[k] = "" # Leave blank for now

# 4. Generate C# content
header = """/*
 * 파일명: OptionsData.cs
 * 분류: [Data] 설정 화면 텍스트
 * 역할: 게임 설정(Options) 화면의 카테고리 및 설정 항목 텍스트를 정의합니다.
 */

using System.Collections.Generic;

namespace QudKRTranslation.Data
{
    public static class OptionsData
    {
        public static Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
"""

footer = """
        };
    }
}
"""

print(header, end='')
keys = sorted(final_dict.keys())
for i, k in enumerate(keys):
    val = final_dict[k]
    # Escape for C# string
    k_esc = k.replace('"', '\\"').replace('\n', '\\n')
    v_esc = val.replace('"', '\\"').replace('\n', '\\n')
    comma = "," if i < len(keys) - 1 else ""
    print(f'            {{ "{k_esc}", "{v_esc}" }}{comma}')
print(footer)
