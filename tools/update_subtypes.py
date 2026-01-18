import os
import json
import xml.etree.ElementTree as ET

# Paths
BASE_DIR = '/Users/ben/Desktop/qud_korean'
SUBTYPES_XML = os.path.join(BASE_DIR, 'Assets/StreamingAssets/Base/Subtypes.xml')
GLOSSARY_JSON = os.path.join(BASE_DIR, 'LOCALIZATION/glossary_chargen.json')
SUBTYPES_DIR = os.path.join(BASE_DIR, 'LOCALIZATION/SUBTYPES')

def load_json(path):
    if not os.path.exists(path):
        return {}
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f)

def save_json(path, data):
    with open(path, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def main():
    # Load Glossary
    glossary = load_json(GLOSSARY_JSON)
    glossary_map = {}
    if 'chargen_ui' in glossary:
        for k, v in glossary['chargen_ui'].items():
            glossary_map[k] = v
    
    # Load XML
    tree = ET.parse(SUBTYPES_XML)
    root = tree.getroot()

    for cls in root.findall('class'):
        class_id = cls.get('ID')
        
        # Determine target directory
        if class_id == 'Callings':
            target_dir = os.path.join(SUBTYPES_DIR, 'Callings')
            subtypes = cls.findall('subtype')
        elif class_id == 'Castes':
            target_dir = os.path.join(SUBTYPES_DIR, 'Castes')
            # Castes are nested in categories
            subtypes = []
            for cat in cls.findall('category'):
                subtypes.extend(cat.findall('subtype'))
        else:
            continue
            
        if not os.path.exists(target_dir):
            os.makedirs(target_dir)

        for subtype in subtypes:
            name = subtype.get('Name')
            json_path = os.path.join(target_dir, f"{name.replace(' ', '_')}.json")
            
            # Get Extra Info (leveltext)
            extrainfos = subtype.findall('extrainfo')
            leveltext = [info.text for info in extrainfos if info.text]
            
            # Prepare Data
            existing_data = load_json(json_path)
            
            # Set Name
            if 'names' not in existing_data:
                existing_data['names'] = {}
            
            translated_name = glossary_map.get(name, name)
            existing_data['names'][name] = translated_name
            
            # Set Level Text
            if leveltext:
                existing_data['leveltext'] = leveltext
                
                # Translate Level Text
                leveltext_ko = []
                for text in leveltext:
                    # Try exact match first
                    trans = glossary_map.get(text)
                    if not trans:
                        # Try case-insensitive or stripped
                        # But for now, just check dictionary
                        trans = text # Default to English if not found
                        # Check keys roughly
                        for gk, gv in glossary_map.items():
                            if gk.lower() == text.lower() or gk.replace('{{', '').replace('}}', '') == text:
                                trans = gv
                                break
                    leveltext_ko.append(trans)
                
                existing_data['leveltext_ko'] = leveltext_ko
            
            # Ensure description fields exist
            if 'description' not in existing_data:
                existing_data['description'] = ""
            if 'description_ko' not in existing_data:
                existing_data['description_ko'] = ""

            print(f"Updating {json_path}")
            save_json(json_path, existing_data)

if __name__ == "__main__":
    main()
