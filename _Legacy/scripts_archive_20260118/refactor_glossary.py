import json
import os
import re

GLOSSARY_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations.json"
OUTPUT_DIR = "/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS"
COMMON_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations_common.json"

def main():
    if not os.path.exists(OUTPUT_DIR):
        os.makedirs(OUTPUT_DIR)

    with open(GLOSSARY_PATH, 'r', encoding='utf-8') as f:
        glossary = json.load(f)

    # 1. Identify all mutations
    mutation_names = {}
    for cat in ["mutation_names_physical_body", "mutation_names_physical_breath", "mutation_names_physical_passive", "mutation_names_mental", "mutation_names_defect"]:
        if cat in glossary:
            mutation_names.update(glossary[cat])

    # 2. Map mutations to their files
    mutations_to_data = {name: {"names": {name: kor}} for name, kor in mutation_names.items()}
    
    # 3. Handle descriptions - Try to match mutation names in descriptions or use heuristic
    # This is hard because multiple mutations might share similar sounding descriptions or one mutation has multiple lines.
    # We'll just distribute lines from mutation_desc_* to the mutation that matches the start of the description if possible.
    
    # Actually, simpler: just use the categories we already have but split them?
    # No, user wants "one file per mutation".
    
    # Let's use the mutation names we have to find related strings.
    for cat_name, items in glossary.items():
        if cat_name.startswith("mutation_names_"):
            for eng, kor in items.items():
                # Already handled in step 2 init
                pass
        elif cat_name.startswith("mutation_desc_"):
            for eng, kor in items.items():
                # Heuristic: Find matching mutation name
                assigned = False
                for m_name in mutation_names:
                    # Look for mutation name in the English description
                    if m_name.lower() in eng.lower():
                        mutations_to_data[m_name].setdefault(cat_name, {})[eng] = kor
                        assigned = True
                        break
                if not assigned:
                    # Collect in common for now
                    pass
        elif cat_name == "mutation_commands":
             for eng, kor in items.items():
                assigned = False
                for m_name in mutation_names:
                    if m_name.lower() in eng.lower() or kor.lower() in eng.lower():
                        mutations_to_data[m_name].setdefault(cat_name, {})[eng] = kor
                        assigned = True
                        break
    
    # 4. Save individual files
    for m_name, data in mutations_to_data.items():
        # Clean filename
        safe_name = re.sub(r'[^\w\s-]', '', m_name).strip().replace(' ', '_')
        filepath = os.path.join(OUTPUT_DIR, f"{safe_name}.json")
        
        # Structure it by category for manager compatibility
        with open(filepath, 'w', encoding='utf-8') as f:
            json.dump(data, f, ensure_ascii=False, indent=2)

    # 5. Create common for remaining stuff
    common_data = {}
    for cat_name, items in glossary.items():
        # Find items that were NOT assigned
        remaining = {}
        for eng, kor in items.items():
            found_somewhere = False
            for m_name, d in mutations_to_data.items():
                if cat_name in d and eng in d[cat_name]:
                    found_somewhere = True
                    break
            if not found_somewhere:
                remaining[eng] = kor
        if remaining:
            common_data[cat_name] = remaining

    with open(COMMON_PATH, 'w', encoding='utf-8') as f:
        json.dump(common_data, f, ensure_ascii=False, indent=2)

    print(f"Refactor complete. {len(mutations_to_data)} mutation files created.")
    print(f"Common strings saved to {COMMON_PATH}")

if __name__ == "__main__":
    main()
