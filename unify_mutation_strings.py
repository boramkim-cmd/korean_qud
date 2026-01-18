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
GLOSSARY_DIR = "/Users/ben/Desktop/qud_korean/LOCALIZATION"
OUTPUT_DIR = "/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS"

def clean_filename(name):
    if not name: return "Unknown"
    return re.sub(r'[^\w\s-]', '', name).strip().replace(' ', '_')

def main():
    # 1. Gather ALL global data from all glossaries
    master_all_data = {}
    other_glossaries = {}
    
    for filename in os.listdir(GLOSSARY_DIR):
        if filename.endswith(".json"):
            path = os.path.join(GLOSSARY_DIR, filename)
            with open(path, 'r', encoding='utf-8') as f:
                try:
                    content = json.load(f)
                    other_glossaries[path] = content
                    for cat, items in content.items():
                        for k, v in items.items():
                            master_all_data[k] = v
                except: pass

    # Also load whatever is ALREADY in the mutation files and then delete them
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

    # 2. Map XML: Name <-> Class
    class_to_names = {}
    xml_data = {} # Name -> {bearer, desc}
    
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
                if name and cls:
                    class_to_names.setdefault(cls, []).append(name)
                    xml_data[name] = {"bearer": bearer, "desc": desc}

    # 3. C#-Driven Scanning - Exhaustive
    final_output = {} # Fname -> Data
    moved_keys = set()
    
    for filename in os.listdir(CS_DIR):
        if filename.endswith(".cs"):
            cls_name = filename.replace(".cs", "")
            
            # Determine Filename
            if cls_name in class_to_names:
                primary_name = class_to_names[cls_name][0]
                fname = clean_filename(primary_name) + ".json"
            else:
                primary_name = cls_name
                fname = clean_filename(cls_name) + ".json"
                
            data = final_output.setdefault(fname, {
                "names": {},
                "bearer_descriptions": {},
                "descriptions": {},
                "mutation_extra": {}
            })
            
            # A) Add XML placeholders/data
            if cls_name in class_to_names:
                for name in class_to_names[cls_name]:
                    data["names"][name] = master_all_data.get(name, name)
                    if name in master_all_data: moved_keys.add(name)
                    
                    x = xml_data.get(name, {})
                    b = x.get("bearer")
                    if b:
                        data["bearer_descriptions"][b] = master_all_data.get(b, b)
                        if b in master_all_data: moved_keys.add(b)
                    
                    d = x.get("desc")
                    if d:
                        jk = d.replace('\n', '\\n')
                        if jk in master_all_data:
                            data["descriptions"][jk] = master_all_data[jk]
                            moved_keys.add(jk)
                        elif d in master_all_data:
                            data["descriptions"][d] = master_all_data[d]
                            moved_keys.add(d)
                        else:
                            data["descriptions"][jk] = jk
            elif not data["names"]:
                # If no XML data, use the display name from C# or class name itself as name placeholder
                data["names"][cls_name] = master_all_data.get(cls_name, cls_name)

            # B) Scan C# strings
            with open(os.path.join(CS_DIR, filename), 'r', encoding='utf-8') as f:
                content = f.read()
                matches = re.findall(r'"(.*?)"', content, re.DOTALL)
                for m in matches:
                    text = m.replace('""', '"').replace('\\n', '\n')
                    if len(text) < 4: continue
                    
                    jk = text.replace('\n', '\\n')
                    if jk in master_all_data:
                        cat = "mutation_extra"
                        if "reputation" in text.lower(): cat = "mutation_reputation"
                        elif any(x in text for x in ["Cooldown", "Duration", "Range", "Area", "chance"]): cat = "mutation_stats"
                        data.setdefault(cat, {})[jk] = master_all_data[jk]
                        moved_keys.add(jk)
                    elif text in master_all_data:
                        cat = "mutation_extra"
                        if "reputation" in text.lower(): cat = "mutation_reputation"
                        elif any(x in text for x in ["Cooldown", "Duration", "Range", "Area", "chance"]): cat = "mutation_stats"
                        data.setdefault(cat, {})[text] = master_all_data[text]
                        moved_keys.add(text)

    # 4. Global Heuristic for orphans
    # Use even more aggressive pattern matching
    sorted_groups = sorted(final_output.keys(), key=lambda x: len(x), reverse=True)
    for eng, kor in master_all_data.items():
        if eng in moved_keys: continue
        
        target_fname = None
        eng_lower = eng.lower()
        
        # 1. Direct group name check
        for fname in sorted_groups:
            gn = fname.replace("_", " ").replace(".json", "").lower()
            if gn in eng_lower:
                target_fname = fname
                break
        
        # 2. Key word heuristics
        if not target_fname:
            if "clot" in eng_lower or "bleed" in eng_lower: target_fname = "Hemophilia.json"
            elif "two hearts" in eng_lower or "extra heart" in eng_lower: target_fname = "Two-hearted.json"
            elif "two heads" in eng_lower: target_fname = "Two-headed.json"
            elif "nearsighted" in eng_lower or "myopia" in eng_lower: target_fname = "Myopic.json"
            elif "skittish" in eng_lower: target_fname = "Skittish.json"
            elif "weak heart" in eng_lower: target_fname = "Weak_Heart.json"
        
        if target_fname and target_fname in final_output:
            category = "mutation_extra"
            if "reputation" in eng.lower(): category = "mutation_reputation"
            elif any(x in eng for x in ["Cooldown", "Duration", "Range", "Area", "chance"]): category = "mutation_stats"
            final_output[target_fname].setdefault(category, {})[eng] = kor
            moved_keys.add(eng)

    # 5. Save ALL files
    count = 0
    for fname, content in final_output.items():
        # Clean up empty categories (but keep names/descriptions if you want placeholders)
        save_data = {k: v for k, v in content.items() if v}
        if save_data:
            with open(os.path.join(OUTPUT_DIR, fname), 'w', encoding='utf-8') as f:
                json.dump(save_data, f, ensure_ascii=False, indent=2)
            count += 1

    # 6. Global Cleanup
    for path, content in other_glossaries.items():
        if "MUTATIONS" in path: continue
        new_content = {}
        for cat, items in content.items():
            rem = {k: v for k, v in items.items() if k not in moved_keys}
            if rem: new_content[cat] = rem
        if new_content != content:
            with open(path, 'w', encoding='utf-8') as f:
                json.dump(new_content, f, ensure_ascii=False, indent=2)

    print(f"C#-DRIVEN 100% EXHAUSTIVE UNIFICATION DONE. {count} files created.")
    print(f"Moved {len(moved_keys)} keys total.")

if __name__ == "__main__":
    main()
