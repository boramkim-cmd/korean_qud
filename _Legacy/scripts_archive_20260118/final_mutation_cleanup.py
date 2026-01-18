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
    # 1. Gather ALL existing data
    all_data = {} # key -> kor
    
    # Load common
    with open(COMMON_PATH, 'r', encoding='utf-8') as f:
        common_full = json.load(f)
        for cat, items in common_full.items():
            for k, v in items.items():
                all_data[k] = v

    # Load existing mutation files
    if os.path.exists(OUTPUT_DIR):
        for filename in os.listdir(OUTPUT_DIR):
            if filename.endswith(".json"):
                with open(os.path.join(OUTPUT_DIR, filename), 'r', encoding='utf-8') as f:
                    try:
                        data = json.load(f)
                        for cat, items in data.items():
                            for k, v in items.items():
                                all_data[k] = v
                    except: pass
        shutil.rmtree(OUTPUT_DIR)
    
    os.makedirs(OUTPUT_DIR)

    # 2. Extract mappings from ALL XMLs
    mutations = []
    names_to_mutations = {}
    class_to_names = {}
    
    for xml_path in XML_PATHS:
        if not os.path.exists(xml_path): continue
        tree = ET.parse(xml_path)
        root = tree.getroot()
        for category in root.findall('category'):
            for mut_elem in category.findall('mutation'):
                name = mut_elem.get('Name')
                cls = mut_elem.get('Class')
                bearer = mut_elem.get('BearerDescription')
                
                desc = mut_elem.findtext('description')
                if not desc and mut_elem.find('description') is not None:
                    p = mut_elem.find('description').find('p')
                    if p is not None: desc = p.text

                m_dict = {
                    "name": name,
                    "class": cls,
                    "bearer": bearer,
                    "desc": desc,
                    "strings": {} # category -> {eng: kor}
                }
                mutations.append(m_dict)
                names_to_mutations[name] = m_dict
                if cls:
                    class_to_names.setdefault(cls, []).append(name)

    # 3. Extract descriptions from C#
    class_to_desc = {}
    for filename in os.listdir(CS_DIR):
        if filename.endswith(".cs"):
            cls_name = filename.replace(".cs", "")
            with open(os.path.join(CS_DIR, filename), 'r', encoding='utf-8') as f:
                content = f.read()
                matches = re.findall(r'return\s+"(.*?)"\s*;', content, re.DOTALL)
                for m in matches:
                    text = m.replace('""', '"').replace('\\n', '\n')
                    class_to_desc.setdefault(cls_name, set()).add(text)

    # 4. Assignment Logic
    moved_keys = set()
    
    # Priority 1: Exact matches
    for m in mutations:
        name = m['name']
        if name in all_data:
            m['strings'].setdefault("names", {})[name] = all_data[name]
            moved_keys.add(name)
        
        bearer = m['bearer']
        if bearer and bearer in all_data:
            m['strings'].setdefault("bearer_descriptions", {})[bearer] = all_data[bearer]
            moved_keys.add(bearer)
            
        desc = m['desc']
        if desc:
            jk = desc.replace('\n', '\\n')
            if jk in all_data:
                m['strings'].setdefault("descriptions", {})[jk] = all_data[jk]
                moved_keys.add(jk)
            elif desc in all_data:
                m['strings'].setdefault("descriptions", {})[desc] = all_data[desc]
                moved_keys.add(desc)

    # Priority 2: C# Descriptions
    for cls, names in class_to_names.items():
        if cls in class_to_desc:
            for d in class_to_desc[cls]:
                jk = d.replace('\n', '\\n')
                if jk in all_data:
                    for n in names:
                        names_to_mutations[n]['strings'].setdefault("descriptions", {})[jk] = all_data[jk]
                    moved_keys.add(jk)
                elif d in all_data:
                    for n in names:
                        names_to_mutations[n]['strings'].setdefault("descriptions", {})[d] = all_data[d]
                    moved_keys.add(d)

    # Priority 3: Substring
    sorted_names = sorted(names_to_mutations.keys(), key=len, reverse=True)
    for eng, kor in all_data.items():
        if eng in moved_keys: continue
        for name in sorted_names:
            if name.lower() in eng.lower():
                cat = "mutation_extra"
                if "reputation" in eng.lower(): cat = "mutation_reputation"
                elif "Cooldown" in eng: cat = "mutation_stats"
                names_to_mutations[name]['strings'].setdefault(cat, {})[eng] = kor
                moved_keys.add(eng)
                break

    # 5. Save Files
    for m in mutations:
        if m['strings']:
            filename = clean_filename(m['name']) + ".json"
            filepath = os.path.join(OUTPUT_DIR, filename)
            existing = {}
            if os.path.exists(filepath):
                with open(filepath, 'r', encoding='utf-8') as f: existing = json.load(f)
            for cat, items in m['strings'].items(): existing.setdefault(cat, {}).update(items)
            with open(filepath, 'w', encoding='utf-8') as f:
                json.dump(existing, f, ensure_ascii=False, indent=2)

    # 6. Final Common Cleanup
    final_common = {}
    for cat, items in common_full.items():
        rem = {k: v for k, v in items.items() if k not in moved_keys}
        if rem: final_common[cat] = rem
    with open(COMMON_PATH, 'w', encoding='utf-8') as f: json.dump(final_common, f, ensure_ascii=False, indent=2)

    print(f"Final Final Cleanup done. Produced {len(os.listdir(OUTPUT_DIR))} mutation files.")
    print(f"Remaining keys in common: {sum(len(v) for v in final_common.values())}")

if __name__ == "__main__":
    main()
