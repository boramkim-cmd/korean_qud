#!/usr/bin/env python3
import sys
import xml.etree.ElementTree as ET
import re

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
            # Special case for color tags often used in headers
            if "color" not in dt and not dt.startswith("<"):
                 keys.add(f"<color=#77BFCFFF>{dt.upper()}</color>")

        help_el = opt.find('helptext')
        if help_el is not None and help_el.text:
            keys.add(help_el.text.strip())

        for attr in ['DisplayValues', 'Values']:
            val = opt.get(attr)
            if val and not val.startswith('*'): # Skip dynamic values like *Resolution
                parts = re.split(r'[,\|]', val)
                for p in parts:
                    if p.strip():
                        keys.add(p.strip())
    return keys

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: script.py <xml_file>")
        sys.exit(1)
    
    extracted_keys = get_keys(sys.argv[1])
    # Print simply the keys, one per line, for internal processing
    for k in sorted(list(extracted_keys)):
        print(k)
