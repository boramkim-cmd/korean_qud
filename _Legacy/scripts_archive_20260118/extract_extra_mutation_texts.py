import os
import re
import json

MUTATION_DIR = "/Users/ben/Desktop/qud_korean/Assets/core_source/_GameSource/XRL.World.Parts.Mutation"
GLOSSARY_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations.json"

def extract_strings_from_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # 1. Look for methods GetDescription and GetLevelText
    # This is a simplified regex to find string literals within these methods
    # It might grab some extra things but we can filter later
    
    methods = re.findall(r'(GetDescription|GetLevelText).*?\{(.*?)\}', content, re.DOTALL)
    
    extracted = []
    for method_name, body in methods:
        # Find string literals: "..."
        # Avoid things that look like code but are in quotes
        strings = re.findall(r'"([^"\n\\]*(?:\\.[^"\n\\]*)*)"', body)
        for s in strings:
            # Clean up and filter
            s = s.strip()
            if len(s) > 5 and ' ' in s: # Likely a sentence
                extracted.append(s)
            elif "reputation with" in s:
                extracted.append(s)
            elif "Natural weapon" in s.lower():
                extracted.append(s)

    # 2. Look for reputation factions specifically
    rep_factions = re.findall(r'reputation with \{\{w\|([^}]+)\}\}', content)
    
    return extracted, rep_factions

def main():
    all_extracted = set()
    all_factions = set()
    
    for filename in os.listdir(MUTATION_DIR):
        if filename.endswith(".cs"):
            ext, fac = extract_strings_from_file(os.path.join(MUTATION_DIR, filename))
            all_extracted.update(ext)
            all_factions.update(fac)
    
    # Also handle the "and" combinations
    refined_factions = set()
    for f in all_factions:
        # Split by "}} and {{w|"
        parts = re.split(r'\}\} and \{\{w\|', f)
        for p in parts:
            refined_factions.add(p.strip())

    print(f"Extracted {len(all_extracted)} potential description lines.")
    print(f"Extracted {len(refined_factions)} faction names.")
    
    # Output to a temporary JSON for review
    memo = {
        "new_descriptions": sorted(list(all_extracted)),
        "new_factions": sorted(list(refined_factions))
    }
    
    output_path = "/Users/ben/Desktop/qud_korean/extra_extraction.json"
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(memo, f, ensure_ascii=False, indent=2)
    
    print(f"Results saved to {output_path}")

if __name__ == "__main__":
    main()
