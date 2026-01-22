import os
import re

def parse_json_simulated(content):
    data = {}
    lines = content.split('\n')
    current_section = None
    
    english_name = None
    korean_name = None
    
    for line in lines:
        trimmed = line.strip()
        
        if '"names":' in trimmed:
            current_section = "names"
        elif '"description_ko":' in trimmed:
            current_section = "description_ko"
        elif '"leveltext_ko":' in trimmed:
            current_section = "leveltext_ko"
        elif current_section == "names" and '":' in trimmed:
            # "English": "Korean"
            try:
                q1 = trimmed.find('"')
                q2 = trimmed.find('"', q1 + 1)
                q3 = trimmed.find('"', q2 + 1)
                q4 = trimmed.rfind('"')
                
                if q1 >= 0 and q2 > q1 and q3 > q2 and q4 > q3:
                    english_name = trimmed[q1+1 : q2]
                    korean_name = trimmed[q3+1 : q4]
                    # Simulate Unescape
                    korean_name = korean_name.replace('\\"', '"')
            except:
                pass
                
    return english_name, korean_name

def check_structure_data(base_dir):
    target_dirs = [
        "LOCALIZATION/GAMEPLAY/MUTATIONS",
        "LOCALIZATION/CHARGEN/GENOTYPES",
        "LOCALIZATION/CHARGEN/SUBTYPES"
    ]
    
    found_arconaut = False
    
    for relative_dir in target_dirs:
        full_path = os.path.join(base_dir, relative_dir)
        if not os.path.exists(full_path):
            print(f"Directory not found: {full_path}")
            continue
            
        print(f"Scanning {relative_dir}...")
        for root, dirs, files in os.walk(full_path):
            for file in files:
                if file.endswith(".json"):
                    path = os.path.join(root, file)
                    with open(path, 'r', encoding='utf-8') as f:
                        content = f.read()
                        
                    en, ko = parse_json_simulated(content)
                    
                    if not en:
                        print(f"[FAIL] No name found in {file}. Content sample: {content[:50]}...")
                    else:
                        print(f"[PARSED] {file} -> {en} : {ko}")
                        if en == "Arconaut":
                            found_arconaut = True
                            print(f"[PASS] Found Arconaut -> {ko}")
                        # print(f"[OK] {file} -> {en}: {ko}")

    if found_arconaut:
        print("Verification SUCCESS: Arconaut found and parsed.")
    else:
        print("Verification FAILURE: Arconaut NOT found.")

if __name__ == "__main__":
    check_structure_data(os.getcwd())
