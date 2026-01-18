
import os
import json
import xml.etree.ElementTree as ET

# Paths
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SUBTYPES_XML = os.path.join(BASE_DIR, 'Assets/StreamingAssets/Base/Subtypes.xml')
GLOSSARY_JSON = os.path.join(BASE_DIR, 'LOCALIZATION/glossary_proto.json')
OUTPUT_DIR = os.path.join(BASE_DIR, 'LOCALIZATION/SUBTYPES')

def load_glossary():
    with open(GLOSSARY_JSON, 'r', encoding='utf-8') as f:
        data = json.load(f)
        # Flatten chargen_proto or just look in known categories
        # glossary_proto has structure { "chargen_proto": { ... } }
        return data.get('chargen_proto', {})

def normalize_key(key):
    return key.lower().strip()

def main():
    if not os.path.exists(SUBTYPES_XML):
        print(f"Error: {SUBTYPES_XML} not found")
        return

    glossary = load_glossary()
    
    tree = ET.parse(SUBTYPES_XML)
    root = tree.getroot()

    # Create directories
    for subdir in ['Callings', 'Castes']:
        path = os.path.join(OUTPUT_DIR, subdir)
        os.makedirs(path, exist_ok=True)

    for cls in root.findall('class'):
        class_id = cls.get('ID') # Castes or Callings
        
        # Subtypes can be direct children or inside category
        subtypes = []
        subtypes.extend(cls.findall('subtype'))
        for cat in cls.findall('category'):
            subtypes.extend(cat.findall('subtype'))

        for subtype in subtypes:
            name = subtype.get('Name')
            if not name: continue
            
            # Find translation
            # Try exact match or lowercase
            name_ko = glossary.get(name.lower(), name)
            
            # Extract extrainfo
            extrainfo = []
            extrainfo_ko = []
            
            for info in subtype.findall('extrainfo'):
                text = info.text
                if text:
                    extrainfo.append(text)
                    # Try translation
                    # text might contain tags like {{B|recycling suit}}
                    # glossary keys are usually lowercased
                    text_lower = text.lower()
                    if text_lower in glossary:
                        extrainfo_ko.append(glossary[text_lower])
                    else:
                        # Try stripping tags?
                        # For now, just append original or empty? 
                        # Assuming glossary covers most lines found in extrainfo.
                        # If not found, use English (fallback) or try to find partial match?
                        # Let's check glossary for specific lines.
                        # "starts with a {{b|recycling suit}}" -> key is lowercased strict.
                        extrainfo_ko.append(glossary.get(text_lower, text))

            # Construct JSON content
            content = {
                "names": {
                    name: name_ko
                },
                "description": "",
                "description_ko": ""
            }
            
            if extrainfo:
                content["leveltext"] = extrainfo
                content["leveltext_ko"] = extrainfo_ko

            # Write file
            safe_name = name.replace(' ', '_')
            filename = os.path.join(OUTPUT_DIR, class_id, f"{safe_name}.json")
            
            with open(filename, 'w', encoding='utf-8') as f:
                json.dump(content, f, indent=2, ensure_ascii=False)
            
            print(f"Generated {filename}")

if __name__ == "__main__":
    main()
