import os
import json
import re
import xml.etree.ElementTree as ET

MUTATIONS_XML = "/Users/ben/Desktop/qud_korean/Assets/StreamingAssets/Base/Mutations.xml"
COMMON_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations_common.json"
OUTPUT_DIR = "/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS"

def clean_filename(name):
    return re.sub(r'[^\w\s-]', '', name).strip().replace(' ', '_')

def main():
    if not os.path.exists(OUTPUT_DIR):
        os.makedirs(OUTPUT_DIR)

    # 1. Parse Mutations.xml
    tree = ET.parse(MUTATIONS_XML)
    root = tree.getroot()
    
    xml_mutations = []
    for category in root.findall('category'):
        for mutation in category.findall('mutation'):
            m_data = {
                "name": mutation.get('Name'),
                "description": mutation.findtext('description') if mutation.find('description') is not None else None,
                "bearer": mutation.get('BearerDescription'),
                "class": mutation.get('Class')
            }
            # Handle <p> tags in description
            desc_elem = mutation.find('description')
            if desc_elem is not None:
                p_text = desc_elem.findtext('p')
                if p_text:
                    m_data["description"] = p_text
            
            xml_mutations.append(m_data)

    # 2. Load Common Glossary
    with open(COMMON_PATH, 'r', encoding='utf-8') as f:
        common = json.load(f)

    # Collect all translated terms into a global lookup
    global_lookup = {}
    for cat_name, items in common.items():
        global_lookup.update(items)

    # 3. Load existing (imperfect) mutation files to keep Pass 3 additions
    old_files_data = {}
    for filename in os.listdir(OUTPUT_DIR):
        if filename.endswith(".json"):
            filepath = os.path.join(OUTPUT_DIR, filename)
            with open(filepath, 'r', encoding='utf-8') as f:
                try:
                    old_files_data[filename] = json.load(f)
                except:
                    pass

    # 4. Map XML to JSON
    new_mutations_data = {}
    moved_keys = set()
    
    for m in xml_mutations:
        m_name = m['name']
        filename = clean_filename(m_name) + ".json"
        
        data = {
            "names": {},
            "descriptions": {},
            "bearer_descriptions": {},
            "mutation_pass3_extra": {}
        }
        
        # Name
        if m_name in global_lookup:
            data["names"][m_name] = global_lookup[m_name]
            moved_keys.add(m_name)
        
        # Description
        m_desc = m['description']
        if m_desc and m_desc in global_lookup:
            data["descriptions"][m_desc] = global_lookup[m_desc]
            moved_keys.add(m_desc)
        
        # Bearer
        m_bearer = m['bearer']
        if m_bearer and m_bearer in global_lookup:
            data["bearer_descriptions"][m_bearer] = global_lookup[m_bearer]
            moved_keys.add(m_bearer)
            
        # Merge from old file if it exists
        if filename in old_files_data:
            old = old_files_data[filename]
            for cat in ["mutation_pass3_extra", "mutation_commands", "mutation_desc_physical_body", "mutation_desc_physical_breath", "mutation_desc_physical_passive", "mutation_desc_mental", "mutation_desc_defect"]:
                if cat in old:
                    for k, v in old[cat].items():
                        if cat == "mutation_pass3_extra":
                            data["mutation_pass3_extra"][k] = v
                        else:
                            # Put into descriptions or fragments? 
                            # Let's keep existing categories for safety
                            data.setdefault(cat, {})[k] = v
                        moved_keys.add(k)

        # Heuristic: Find any strings in common that contain the mutation name
        # (This was my previous strategy, let's keep it as secondary)
        for cat_name, items in common.items():
            if cat_name.startswith("mutation_"):
                for eng, kor in items.items():
                    if eng not in moved_keys and m_name.lower() in eng.lower():
                        data.setdefault(cat_name, {})[eng] = kor
                        moved_keys.add(eng)

        new_mutations_data[filename] = data

    # 5. Save new files
    for filename, data in new_mutations_data.items():
        # Remove empty categories
        clean_data = {k: v for k, v in data.items() if v}
        if clean_data:
            filepath = os.path.join(OUTPUT_DIR, filename)
            with open(filepath, 'w', encoding='utf-8') as f:
                json.dump(clean_data, f, ensure_ascii=False, indent=2)

    # 6. Cleanup Common
    new_common = {}
    for cat_name, items in common.items():
        remaining = {k: v for k, v in items.items() if k not in moved_keys}
        if remaining:
            new_common[cat_name] = remaining

    with open(COMMON_PATH, 'w', encoding='utf-8') as f:
        json.dump(new_common, f, ensure_ascii=False, indent=2)

    print(f"Perfect refactor complete. {len(new_mutations_data)} files processed.")
    print(f"Moved {len(moved_keys)} keys from common to individual files.")

if __name__ == "__main__":
    main()
