import json
import xml.etree.ElementTree as ET
import sys

def check_missing(xml_path, glossary_path, category):
    try:
        tree = ET.parse(xml_path)
        root = tree.getroot()
    except Exception as e:
        print(f"Error parsing XML {xml_path}: {e}")
        return

    try:
        with open(glossary_path, 'r', encoding='utf-8') as f:
            glossary = json.load(f)
            dict_data = glossary.get(category, {})
    except Exception as e:
        print(f"Error loading glossary {glossary_path}: {e}")
        return

    missing = []
    
    # Check all attributes with "Description" or "DisplayName" or tags like <description>
    for elem in root.iter():
        # Handle attributes
        for attr in ['Description', 'DisplayName', 'ChargenDescription']:
            val = elem.get(attr)
            if val and val.lower() not in dict_data:
                missing.append(val)
        
        # Handle tags
        if elem.tag == 'description':
            text = elem.text
            if text and text.strip().lower() not in dict_data:
                missing.append(text.strip())
                
    return sorted(list(set(missing)))

if __name__ == "__main__":
    skills_missing = check_missing('Assets/StreamingAssets/Base/Skills.xml', 'LOCALIZATION/glossary_skills.json', 'skill_desc')
    print("Missing Skill Descriptions:")
    for m in skills_missing:
        print(f"- {m}")
    
    mut_missing = check_missing('Assets/StreamingAssets/Base/Mutations.xml', 'LOCALIZATION/glossary_mutations.json', 'mutation_desc')
    print("\nMissing Mutation Descriptions:")
    for m in mut_missing:
        print(f"- {m}")
