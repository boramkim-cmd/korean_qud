import os
import json
import xml.etree.ElementTree as ET

# Paths
BASE_DIR = os.getcwd()
SKILLS_XML = os.path.join(BASE_DIR, "Assets/StreamingAssets/Base/Skills.xml")
SUBTYPES_XML = os.path.join(BASE_DIR, "Assets/StreamingAssets/Base/Subtypes.xml")
GLOSSARY_SKILLS = os.path.join(BASE_DIR, "LOCALIZATION/glossary_skills.json")
GLOSSARY_CHARGEN = os.path.join(BASE_DIR, "LOCALIZATION/glossary_chargen.json")
TARGET_DIR = os.path.join(BASE_DIR, "LOCALIZATION/SUBTYPES")

def load_json(path):
    if not os.path.exists(path):
        return {}
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f)

def save_json(path, data):
    with open(path, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def parse_skills_xml():
    tree = ET.parse(SKILLS_XML)
    root = tree.getroot()
    mapping = {}
    
    # Map both 'Name' and 'Class' to 'Name' (Display Name)
    for skill in root.findall(".//skill"):
        name = skill.get("Name")
        cls = skill.get("Class")
        if name:
            if cls: mapping[cls] = name
            mapping[name] = name # Fallback
            
        for power in skill.findall("power"):
            p_name = power.get("Name")
            p_cls = power.get("Class")
            if p_name:
                if p_cls: mapping[p_cls] = p_name
                mapping[p_name] = p_name

    return mapping

def parse_subtypes_xml(skill_mapping):
    tree = ET.parse(SUBTYPES_XML)
    root = tree.getroot()
    subtypes = {}
    
    for subtype in root.findall(".//subtype"):
        name = subtype.get("Name")
        if not name: continue
        
        info_lines = []
        
        # Stats
        for stat in subtype.findall("stat"):
            stat_name = stat.get("Name")
            bonus = stat.get("Bonus")
            if stat_name and bonus:
                # Format: "Stat +Bonus"
                sign = "+" if int(bonus) >= 0 else ""
                info_lines.append(f"{stat_name} {sign}{bonus}")
        
        # Skills
        skills = subtype.find("skills")
        if skills is not None:
            for skill in skills.findall("skill"):
                s_name = skill.get("Name")
                # Lookup display name
                display_name = skill_mapping.get(s_name, s_name)
                info_lines.append(display_name)
                
        # Extrainfo
        # <extrainfo> text </extrainfo>
        # Note: XML might have tags like {{B|...}}. We keep them for now?
        # The game usually strips them for display using GetFlatChargenInfo?
        # Actually Extrainfo in XML is usually plain text or with tags. 
        # But for 'leveltext' which is used for filtering, we should match what SubtypeEntry.GetChargenInfo returns.
        # SubtypeEntry DOES NOT parse Extrainfo other than extracting text.
        # But wait, looking at extraction script output from previous turn, it had tags?
        # Let's strip simple tags just in case, or keep them.
        # Arconaut: "Starts with random junk and artifacts" (Plain)
        # Pilgrim: "Starts with the {{B|pilgrim}} reputation"
        # We need the TEXT content. ET .text handles it.
        # Inner tags might be an issue.
        # Let's use string representation of inner content if needed.
        for extra in subtype.findall("extrainfo"):
            # Simple text extraction
            text = "".join(extra.itertext())
            info_lines.append(text)
            
        # Hardcoded Scholar Logic (as requested to appear)
        if name == "Scholar":
             info_lines.append("Wilderness Lore: Random")

        subtypes[name] = info_lines
        
    return subtypes

def load_glossary_map():
    # Merge glossaries: key (lowercase) -> value (Korean)
    g_map = {}
    
    # Skills
    skills_data = load_json(GLOSSARY_SKILLS)
    
    # Pass 1: Load descriptions first (so they can be overwritten by names)
    for category_name, category_data in skills_data.items():
        if isinstance(category_data, dict) and category_name.endswith("_desc"):
            for k, v in category_data.items():
                g_map[k.lower()] = v

    # Pass 2: Load names (skills, powers)
    for category_name, category_data in skills_data.items():
        if isinstance(category_data, dict) and not category_name.endswith("_desc"):
             for k, v in category_data.items():
                g_map[k.lower()] = v

    # Chargen (Highest priority?)
    chargen_data = load_json(GLOSSARY_CHARGEN)
    if "chargen_ui" in chargen_data:
        for k, v in chargen_data["chargen_ui"].items():
            g_map[k.lower()] = v
            
    # Add Stat mappings (Hardcoded fallback if not in glossary)
    g_map["strength"] = "힘"
    g_map["agility"] = "민첩성"
    g_map["toughness"] = "지구력"
    g_map["intelligence"] = "지능"
    g_map["willpower"] = "의지력"
    g_map["ego"] = "자아"
    
    # Resistances
    g_map["heatresistance"] = "열 저항"
    g_map["coldresistance"] = "냉기 저항"
    g_map["acidresistance"] = "산성 저항"
    g_map["electricresistance"] = "전기 저항"
    
    return g_map

def translate_line(line, glossary_map):
    # 1. Try exact match
    if line.lower() in glossary_map:
        return glossary_map[line.lower()]
    
    # 2. Try separating "Name +Bonus"
    import re
    match = re.match(r"([A-Za-z]+)\s+([+\-]\d+)", line)
    if match:
        stat = match.group(1).lower()
        bonus = match.group(2)
        if stat in glossary_map:
            return f"{glossary_map[stat]} {bonus}"
            
    # 3. Fallback: Return original
    return line

def update_jsons():
    skill_mapping = parse_skills_xml()
    subtype_data = parse_subtypes_xml(skill_mapping)
    glossary_map = load_glossary_map()
    
    # Walk through target directory
    for root, dirs, files in os.walk(TARGET_DIR):
        for file in files:
            if file.endswith(".json"):
                path = os.path.join(root, file)
                subtype_name = os.path.splitext(file)[0]
                # Handle spaces/underscores? Subtypes.xml usually matches filename.
                # E.g. "Mutated_Human" -> "Mutated Human".
                # Subtypes usually don't have underscores in XML Name attribute?
                # Check mapping.
                # Actually filenames have underscores, XML Names have spaces.
                xml_name = subtype_name.replace("_", " ")
                
                # Special handling for "Priest of All Suns" etc.
                if xml_name not in subtype_data:
                    # Try finding case insensitive match or underscore variant
                    for k in subtype_data.keys():
                        if k.replace(" ", "_").lower() == subtype_name.lower():
                            xml_name = k
                            break
                            
                if xml_name in subtype_data:
                    print(f"Updating {subtype_name}...")
                    data = load_json(path)
                    
                    # Construct Lists
                    leveltext = subtype_data[xml_name]
                    leveltext_ko = [translate_line(line, glossary_map) for line in leveltext]
                    
                    # Update fields
                    data["leveltext"] = leveltext
                    data["leveltext_ko"] = leveltext_ko
                    
                    save_json(path, data)
                else:
                    print(f"Skipping {subtype_name} (Not found in XML)")

if __name__ == "__main__":
    update_jsons()
