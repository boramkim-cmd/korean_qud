import os
import json
import re
import xml.etree.ElementTree as ET
import shutil

XML_PATHS = [
    "/Users/ben/Desktop/qud_korean/Assets/StreamingAssets/Base/Mutations.xml",
    "/Users/ben/Desktop/qud_korean/Assets/StreamingAssets/Base/HiddenMutations.xml"
]
CS_DIR = "/Users/ben/Desktop/qud_korean/Assets/core_source/_GameSource/XRL.World.Parts.Mutation"
COMMON_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations_common.json"
OUTPUT_DIR = "/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS"

def clean_filename(name):
    return re.sub(r'[^\w\s-]', '', name).strip().replace(' ', '_')

def main():
    # 1. Collect ALL data from all sources (common + existing mutation files)
    master_all_data = {} # eng -> kor
    
    with open(COMMON_PATH, 'r', encoding='utf-8') as f:
        common = json.load(f)
        for cat, items in common.items():
            for k, v in items.items():
                master_all_data[k] = v

    if os.path.exists(OUTPUT_DIR):
        for filename in os.listdir(OUTPUT_DIR):
            if filename.endswith(".json"):
                with open(os.path.join(OUTPUT_DIR, filename), 'r', encoding='utf-8') as f:
                    try:
                        data = json.load(f)
                        for cat, items in data.items():
                            for k, v in items.items():
                                master_all_data[k] = v
                    except: pass
        shutil.rmtree(OUTPUT_DIR)
    os.makedirs(OUTPUT_DIR)

    # 2. Extract Mutation Definitions
    mutation_definitions = {} # name -> {class, bearer, descriptions: set()}
    class_to_names = {}
    
    for xml_path in XML_PATHS:
        if not os.path.exists(xml_path): continue
        tree = ET.parse(xml_path)
        root = tree.getroot()
        for cat_elem in root.findall('category'):
            for mut_elem in cat_elem.findall('mutation'):
                name = mut_elem.get('Name')
                cls = mut_elem.get('Class')
                bearer = mut_elem.get('BearerDescription')
                
                desc = mut_elem.findtext('description')
                if not desc and mut_elem.find('description') is not None:
                    p = mut_elem.find('description').find('p')
                    if p is not None: desc = p.text

                if name not in mutation_definitions:
                    mutation_definitions[name] = {"class": cls, "bearer": bearer, "descs": set()}
                if desc: mutation_definitions[name]["descs"].add(desc)
                if cls:
                    class_to_names.setdefault(cls, set()).add(name)

    # 3. Add C# descriptions
    for filename in os.listdir(CS_DIR):
        if filename.endswith(".cs"):
            cls_name = filename.replace(".cs", "")
            with open(os.path.join(CS_DIR, filename), 'r', encoding='utf-8') as f:
                content = f.read()
                matches = re.findall(r'return\s+"(.*?)"\s*;', content, re.DOTALL)
                for m in matches:
                    text = m.replace('""', '"').replace('\\n', '\n')
                    if cls_name in class_to_names:
                        for name in class_to_names[cls_name]:
                            mutation_definitions[name]["descs"].add(text)

    # 4. Map everything to individual mutation files using a master output map
    mutation_files_content = {} # filename -> {category -> {eng: kor}}
    moved_keys = set()

    # Pass 1: Direct matches (Name, Bearer, Descriptions)
    for name, defs in mutation_definitions.items():
        fname = clean_filename(name) + ".json"
        content = mutation_files_content.setdefault(fname, {})
        
        # Name match
        if name in master_all_data:
            content.setdefault("names", {})[name] = master_all_data[name]
            moved_keys.add(name)
        
        # Bearer match
        bearer = defs["bearer"]
        if bearer and bearer in master_all_data:
            content.setdefault("bearer_descriptions", {})[bearer] = master_all_data[bearer]
            moved_keys.add(bearer)
            
        # Descriptions match
        for d in defs["descs"]:
            jk = d.replace('\n', '\\n')
            if jk in master_all_data:
                content.setdefault("descriptions", {})[jk] = master_all_data[jk]
                moved_keys.add(jk)
            elif d in master_all_data:
                content.setdefault("descriptions", {})[d] = master_all_data[d]
                moved_keys.add(d)

    # Pass 2: Substring heuristic for mutation_extra
    sorted_names = sorted(mutation_definitions.keys(), key=len, reverse=True)
    for eng, kor in master_all_data.items():
        if eng in moved_keys: continue
        for name in sorted_names:
            if name.lower() in eng.lower():
                fname = clean_filename(name) + ".json"
                cat = "mutation_extra"
                if "reputation" in eng.lower(): cat = "mutation_reputation"
                elif "Cooldown" in eng or "Duration" in eng: cat = "mutation_stats"
                mutation_files_content.setdefault(fname, {}).setdefault(cat, {})[eng] = kor
                moved_keys.add(eng)
                break

    # 5. Save Files
    for fname, content in mutation_files_content.items():
        if content:
            with open(os.path.join(OUTPUT_DIR, fname), 'w', encoding='utf-8') as f:
                json.dump(content, f, ensure_ascii=False, indent=2)

    # 6. Final Common Cleanup
    final_common = {}
    # Use the original common as structure source
    for cat, items in common.items():
        rem = {k: v for k, v in items.items() if k not in moved_keys}
        if rem: final_common[cat] = rem
    with open(COMMON_PATH, 'w', encoding='utf-8') as f:
        json.dump(final_common, f, ensure_ascii=False, indent=2)

    print(f"Ultra Robust Refactor complete. Processed {len(mutation_files_content)} files.")
    print(f"Moved {len(moved_keys)} keys total.")

if __name__ == "__main__":
    main()
