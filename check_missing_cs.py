import json
import os
import re

def check_missing_cs(cs_dir, glossary_path, category):
    try:
        with open(glossary_path, 'r', encoding='utf-8') as f:
            glossary = json.load(f)
            dict_data = glossary.get(category, {})
    except Exception as e:
        print(f"Error loading glossary {glossary_path}: {e}")
        return

    missing = []
    
    # Simple regex to find return "..." or strings in GetDescription / GetLevelText
    # This is heuristic but better than nothing
    for root_dir, dirs, files in os.walk(cs_dir):
        for file in files:
            if file.endswith('.cs'):
                path = os.path.join(root_dir, file)
                with open(path, 'r', encoding='utf-8') as f:
                    content = f.read()
                    # Find return "..." where ... is more than a whitespace
                    matches = re.findall(r'return\s+"([^"]{10,})"', content)
                    for m in matches:
                        if m.lower() not in dict_data:
                            missing.append(m)
                    
                    # Also find string text = "..."
                    matches = re.findall(r'text\s+\+?=\s+"([^"]{10,})"', content)
                    for m in matches:
                        if m.lower() not in dict_data:
                            missing.append(m)
                            
    return sorted(list(set(missing)))

if __name__ == "__main__":
    cs_missing = check_missing_cs('Assets/core_source/XRL.World.Parts.Mutation/', 'LOCALIZATION/glossary_mutations.json', 'mutation_desc')
    print("Missing Mutation Strings from C#:")
    for m in cs_missing:
        print(f"- {m}")
