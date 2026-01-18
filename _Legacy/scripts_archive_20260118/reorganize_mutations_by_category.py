#!/usr/bin/env python3
"""
Extract mutation information from C# sources and XML, and organize by category
"""
import os
import json
import re
import xml.etree.ElementTree as ET

# Paths
CS_DIR = "/Users/ben/Desktop/qud_korean/Assets/core_source/_GameSource/XRL.World.Parts.Mutation"
XML_PATH = "/Users/ben/Desktop/qud_korean/Assets/StreamingAssets/Base/Mutations.xml"
OUTPUT_BASE = "/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS"

# Categories based on character creation screen
# Simplified to 3 folders as requested
CATEGORIES = {
    "Morphotypes": [],        # Cost = 1
    "Physical_Mutations": [], # Cost > 1 (all mutations)
    "Mental_Defects": []      # Cost < 0 (all defects)
}

def extract_from_cs(filepath):
    """Extract mutation info from C# file"""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    info = {
        "class_name": os.path.basename(filepath).replace('.cs', ''),
        "type": None,
        "cost": None,
        "description": None,
        "level_text": []
    }
    
    # Extract Type
    type_match = re.search(r'Type\s*=\s*"(Physical|Mental)"', content)
    if type_match:
        info["type"] = type_match.group(1)
    
    # Extract Cost
    cost_match = re.search(r'Cost\s*=\s*(\d+)', content)
    if cost_match:
        info["cost"] = int(cost_match.group(1))
    
    # Extract GetDescription
    desc_match = re.search(r'GetDescription\(\)[^{]*{[^"]*"([^"]+)"', content, re.DOTALL)
    if desc_match:
        info["description"] = desc_match.group(1)
    
    # Extract GetLevelText strings
    level_text_match = re.search(r'GetLevelText\(int Level\)[^{]*{(.*?)^[\t ]*}', content, re.MULTILINE | re.DOTALL)
    if level_text_match:
        level_text_content = level_text_match.group(1)
        # Find all string literals
        strings = re.findall(r'"([^"]*)"', level_text_content)
        info["level_text"] = [s for s in strings if len(s) > 3 and '\\t' not in s and 'public' not in s]
    
    return info

def load_xml_data():
    """Load mutation data from XML"""
    tree = ET.parse(XML_PATH)
    root = tree.getroot()
    
    xml_data = {}
    for category in root.findall('category'):
        cat_name = category.get('Name', '')
        for mutation in category.findall('mutation'):
            name = mutation.get('Name')
            class_name = mutation.get('Class')
            cost = mutation.get('Cost', '0')
            
            if class_name:
                xml_data[class_name] = {
                    "name": name,
                    "cost": int(cost) if cost.lstrip('-').isdigit() else 0,
                    "category": cat_name,
                    "bearer": mutation.get('BearerDescription', ''),
                    "variant": mutation.get('Variant', '')
                }
    
    return xml_data

def categorize_mutation(cs_info, xml_info):
    """Categorize based on EXACT mutations visible in character creation screen images"""
    class_name = cs_info.get('class_name', '')
    
    # === MORPHOTYPES (이미지 2 - 정확히 3개) ===
    MORPHOTYPES = {
        'Chimera', 'Esper', 'UnstableGenome'
    }
    
    # === MENTAL_DEFECTS (이미지 1 전체) ===
    # Mental Mutations (21개)
    MENTAL_MUTATIONS = {
        'Domination', 'EgoProjection', 'ForceBubble', 'ForceWall', 'Kindle',
        'LightManipulation', 'MassMind', 'MentalMirror', 'Precognition',
        'Psychometry', 'Pyrokinesis', 'SensePsychic', 'SpacetimeVortex',
        'StunningForce', 'SunderMind', 'LifeDrain',  # Syphon Vim
        'Telepathy', 'TeleportOther', 'Teleportation', 'TemporalFugue',
        'TimeDilation'
    }
    # Mental Defects (8개)
    MENTAL_DEFECTS = {
        'Amnesia', 'BlinkingTic', 'Dystechnia', 'EvilTwin',
        'Narcolepsy', 'PsionicMigraines', 'QuantumJitters', 'SociallyRepugnant'
    }
    
    # === PHYSICAL_MUTATIONS (이미지 2 - Physical + Defects, Morphotypes 제외) ===
    # Physical Mutations (31개 visible in image)
    PHYSICAL_MUTATIONS = {
        'AdrenalControl2',  # Adrenal Control
        'Beak', 'BurrowingClaws', 'Carapace', 'CorrosiveGasGeneration',
        'DoubleMuscled', 'ElectricalGeneration', 'ElectromagneticPulse',
        'FlamingRay', 'FreezingRay', 'HeightenedHearing', 'HeightenedSpeed',  # Heightened Quickness
        'Horns', 'MultipleArms', 'MultipleLegs', 'NightVision', 'Phasing',
        'PhotosyntheticSkin', 'Quills', 'Regeneration', 'SleepGasGeneration',
        'SlimeGlands', 'Spinnerets', 'Stinger',  # 3 variants
        'ThickFur', 'TripleJointed', 'TwoHeaded', 'TwoHearted', 'Wings'
    }
    # Physical Defects (1개)
    PHYSICAL_DEFECTS = {
        'Albino'
    }
    
    ALL_MENTAL = MENTAL_MUTATIONS | MENTAL_DEFECTS
    ALL_PHYSICAL = PHYSICAL_MUTATIONS | PHYSICAL_DEFECTS
    
    if class_name in MORPHOTYPES:
        return "Morphotypes"
    elif class_name in ALL_MENTAL:
        return "Mental_Defects"
    elif class_name in ALL_PHYSICAL:
        return "Physical_Mutations"
    else:
        # 이미지에 없는 뮤테이션은 제외
        return None

def create_json_file(category, class_name, cs_info, xml_info):
    """Create JSON file for mutation"""
    data = {}
    
    # Names
    if xml_info and xml_info.get('name'):
        data["names"] = {xml_info['name']: xml_info['name']}  # English only for now
    
    # Bearer descriptions  
    if xml_info and xml_info.get('bearer'):
        data["bearer_descriptions"] = {xml_info['bearer']: xml_info['bearer']}
    
    # Description
    if cs_info.get('description'):
        data["descriptions"] = {cs_info['description']: cs_info['description']}
    
    # Level text fragments
    if cs_info.get('level_text'):
        data["mutation_extra"] = {}
        for text in cs_info['level_text']:
            data["mutation_extra"][text] = text
    
    # Save to file
    filename = f"{class_name}.json"
    filepath = os.path.join(OUTPUT_BASE, category, filename)
    
    with open(filepath, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    
    return filepath

def main():
    print("Loading XML data...")
    xml_data = load_xml_data()
    
    print("Processing C# files...")
    created_files = {cat: [] for cat in CATEGORIES}
    
    for filename in sorted(os.listdir(CS_DIR)):
        if not filename.endswith('.cs'):
            continue
        
        class_name = filename.replace('.cs', '')
        filepath = os.path.join(CS_DIR, filename)
        
        print(f"  Processing {class_name}...")
        
        cs_info = extract_from_cs(filepath)
        xml_info = xml_data.get(class_name, {})
        
        category = categorize_mutation(cs_info, xml_info)
        
        # Skip if not in image
        if category is None:
            print(f"    ⊗ Skipped (not in character creation screen)")
            continue
        
        try:
            json_path = create_json_file(category, class_name, cs_info, xml_info)
            created_files[category].append(class_name)
            print(f"    → {category}/{class_name}.json")
        except Exception as e:
            print(f"    ✗ Error: {e}")
    
    # Print summary
    print("\n" + "="*60)
    print("SUMMARY")
    print("="*60)
    for cat, files in created_files.items():
        print(f"{cat}: {len(files)} files")
    print(f"Total: {sum(len(f) for f in created_files.values())} files")

if __name__ == "__main__":
    main()
