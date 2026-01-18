import os
import json
import re
import xml.etree.ElementTree as ET

MUTATIONS_XML = "/Users/ben/Desktop/qud_korean/Assets/StreamingAssets/Base/Mutations.xml"
CS_DIR = "/Users/ben/Desktop/qud_korean/Assets/core_source/_GameSource/XRL.World.Parts.Mutation"
COMMON_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations_common.json"
OUTPUT_DIR = "/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS"

def clean_filename(name):
    return re.sub(r'[^\w\s-]', '', name).strip().replace(' ', '_')

def main():
    if not os.path.exists(OUTPUT_DIR):
        os.makedirs(OUTPUT_DIR)

    # 1. Map Class to Name from XML
    tree = ET.parse(MUTATIONS_XML)
    root = tree.getroot()
    
    class_to_name = {}
    name_to_bearer = {}
    name_to_xml_desc = {}
    
    for category in root.findall('category'):
        for mutation in category.findall('mutation'):
            name = mutation.get('Name')
            cls = mutation.get('Class')
            if name and cls:
                class_to_name[cls] = name
            
            bearer = mutation.get('BearerDescription')
            if name and bearer:
                name_to_bearer[name] = bearer
            
            desc = mutation.findtext('description')
            if not desc and mutation.find('description') is not None:
                p = mutation.find('description').find('p')
                if p is not None:
                    desc = p.text
            if name and desc:
                name_to_xml_desc[name] = desc

    # 2. Map Class to Description from C#
    class_to_desc = {}
    for filename in os.listdir(CS_DIR):
        if filename.endswith(".cs"):
            cls_name = filename.replace(".cs", "")
            filepath = os.path.join(CS_DIR, filename)
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
                # Simple regex for GetDescription() { return "..."; }
                # Handles multi-line strings with \n
                match = re.search(r'public override string GetDescription\(\)\s*\{(?:[^{}]*)\breturn\s+"(.*?)"\s*;', content, re.DOTALL)
                if match:
                    desc = match.group(1).replace('""', '"').replace('\\n', '\n') # Basic unescape
                    # Note: We need the EXACT string from common, which has literal \n as \n characters usually
                    # But in JSON it's \\n. 
                    class_to_desc[cls_name] = desc

    # 3. Load Common Glossary
    with open(COMMON_PATH, 'r', encoding='utf-8') as f:
        common = json.load(f)

    # Flatten common into a single lookup for matching
    global_lookup = {}
    for cat_name, items in common.items():
        global_lookup.update(items)

    # 4. Process each mutation
    moved_keys = set()
    output_data = {}

    all_names = set(class_to_name.values())
    
    for cls, name in class_to_name.items():
        filename = clean_filename(name) + ".json"
        data = {
            "names": {},
            "descriptions": {},
            "bearer_descriptions": {},
            "mutation_pass3_extra": {}
        }
        
        # Name
        if name in global_lookup:
            data["names"][name] = global_lookup[name]
            moved_keys.add(name)
        
        # XML Description
        if name in name_to_xml_desc:
            desc = name_to_xml_desc[name]
            # Replace literal newlines if needed
            json_key = desc.replace('\n', '\\n')
            if json_key in global_lookup:
                data["descriptions"][json_key] = global_lookup[json_key]
                moved_keys.add(json_key)
            elif desc in global_lookup:
                 data["descriptions"][desc] = global_lookup[desc]
                 moved_keys.add(desc)

        # C# Description
        if cls in class_to_desc:
            desc = class_to_desc[cls]
            json_key = desc.replace('\n', '\\n')
            if json_key in global_lookup:
                data["descriptions"][json_key] = global_lookup[json_key]
                moved_keys.add(json_key)
            elif desc in global_lookup:
                 data["descriptions"][desc] = global_lookup[desc]
                 moved_keys.add(desc)
        
        # Bearer
        if name in name_to_bearer:
            bearer = name_to_bearer[name]
            if bearer in global_lookup:
                data["bearer_descriptions"][bearer] = global_lookup[bearer]
                moved_keys.add(bearer)

        output_data[filename] = data

    # 5. Secondary heuristic: Catch-all for remaining mutation tags in common
    # If a key in common still contains a mutation name, assign it.
    for cat_name, items in common.items():
        if cat_name.startswith("mutation_"):
            for eng, kor in items.items():
                if eng in moved_keys: continue
                # Match longest name first to avoid partial matches
                best_match = None
                for m_name in sorted(all_names, key=len, reverse=True):
                    if m_name.lower() in eng.lower():
                        best_match = m_name
                        break
                if best_match:
                    fname = clean_filename(best_match) + ".json"
                    output_data.setdefault(fname, {}).setdefault(cat_name, {})[eng] = kor
                    moved_keys.add(eng)

    # 6. Save Files
    for filename, data in output_data.items():
        clean_data = {k: v for k, v in data.items() if v}
        if clean_data:
            filepath = os.path.join(OUTPUT_DIR, filename)
            # Merge with existing file if it has Pass 3 stuff
            if os.path.exists(filepath):
                 with open(filepath, 'r', encoding='utf-8') as f:
                     try:
                         existing = json.load(f)
                         for k, v in existing.get("mutation_pass3_extra", {}).items():
                             clean_data.setdefault("mutation_pass3_extra", {})[k] = v
                             moved_keys.add(k)
                     except: pass
            
            with open(filepath, 'w', encoding='utf-8') as f:
                json.dump(clean_data, f, ensure_ascii=False, indent=2)

    # 7. Final Cleanup of Common
    new_common = {}
    for cat_name, items in common.items():
        remaining = {k: v for k, v in items.items() if k not in moved_keys}
        if remaining:
            new_common[cat_name] = remaining

    with open(COMMON_PATH, 'w', encoding='utf-8') as f:
        json.dump(new_common, f, ensure_ascii=False, indent=2)

    print(f"Grand Refactor complete. Moved {len(moved_keys)} keys.")

if __name__ == "__main__":
    main()
