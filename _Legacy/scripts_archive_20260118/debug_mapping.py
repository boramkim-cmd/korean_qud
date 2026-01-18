import os
import json
import xml.etree.ElementTree as ET

MUTATIONS_XML = "/Users/ben/Desktop/qud_korean/Assets/StreamingAssets/Base/Mutations.xml"
COMMON_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations_common.json"

def main():
    tree = ET.parse(MUTATIONS_XML)
    root = tree.getroot()
    
    xml_names = []
    for category in root.findall('category'):
        for mut_elem in category.findall('mutation'):
            xml_names.append(mut_elem.get('Name'))

    with open(COMMON_PATH, 'r', encoding='utf-8') as f:
        common = json.load(f)
    
    all_keys = []
    for items in common.values():
        all_keys.extend(items.keys())

    print(f"Total mutations in XML: {len(xml_names)}")
    
    missing_in_common = []
    for name in xml_names:
        if name not in all_keys:
            missing_in_common.append(name)
            
    print(f"Missing in Common: {len(missing_in_common)}")
    for m in missing_in_common:
        print(f" - {m}")

    print("\nFirst 10 keys in Common:")
    for k in sorted(all_keys)[:10]:
        print(f" - {k}")

if __name__ == "__main__":
    main()
