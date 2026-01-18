import json
import os
import glob
from collections import defaultdict

PROTO_PATH = "LOCALIZATION/_legacy_glossary_proto.json"
TARGET_DIRS = ["LOCALIZATION/GENOTYPES", "LOCALIZATION/SUBTYPES/Callings", "LOCALIZATION/SUBTYPES/Castes"]

# Manual mappings for added entries based on plan
# keys are filename (no ext), values are matching proto keys to ADD to extrainfo
ADDITIONAL_ENTRIES = {
    "Apostle": ["may rebuke robots"]
}

def load_json(path):
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f)

def save_json(path, data):
    with open(path, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def main():
    if not os.path.exists(PROTO_PATH):
        print(f"Error: {PROTO_PATH} not found.")
        return

    proto_data = load_json(PROTO_PATH)
    # proto might be wrapped in "chargen_proto" or just be the dict. check structure.
    # From file view: {"chargen_proto": { ... }}
    glossary = proto_data.get("chargen_proto", proto_data)
    
    # normalize glossary keys for caseless matching
    glossary_lower = {k.lower(): v for k, v in glossary.items()}

    files_processed = 0
    updates_made = 0

    for directory in TARGET_DIRS:
        if not os.path.exists(directory):
            print(f"Skipping {directory} (not found)")
            continue

        for filepath in glob.glob(os.path.join(directory, "*.json")):
            filename = os.path.basename(filepath)
            basename = os.path.splitext(filename)[0]
            
            data = load_json(filepath)
            modified = False

            # 1. Update existing leveltext translations
            if "leveltext" in data and "leveltext_ko" in data:
                # Ensure lists are same length
                while len(data["leveltext_ko"]) < len(data["leveltext"]):
                    data["leveltext_ko"].append("")
                
                for i, text in enumerate(data["leveltext"]):
                    key = text.lower().strip()
                    # Try exact match first
                    if key in glossary_lower:
                        original_ko = data["leveltext_ko"][i]
                        new_ko = glossary_lower[key]
                        if original_ko != new_ko:
                            print(f"[{filename}] Updating leveltext: '{text}' -> '{new_ko}' (was '{original_ko}')")
                            data["leveltext_ko"][i] = new_ko
                            modified = True
            
            # 2. Add specific missing items (Extra Info)
            if basename in ADDITIONAL_ENTRIES:
                for proto_key in ADDITIONAL_ENTRIES[basename]:
                    # Find the translation
                    if proto_key.lower() in glossary_lower:
                        trans_ko = glossary_lower[proto_key.lower()]
                        
                        # Init fields if missing
                        if "extrainfo" not in data:
                            data["extrainfo"] = []
                        if "extrainfo_ko" not in data:
                            data["extrainfo_ko"] = []
                        
                        # Check if already present
                        # We compare against existing extrainfo content to avoid duplication
                        existing_texts = [t.lower() for t in data["extrainfo"]]
                        if proto_key.lower() not in existing_texts:
                            print(f"[{filename}] Adding extrainfo: '{proto_key}'")
                            data["extrainfo"].append(proto_key)
                            data["extrainfo_ko"].append(trans_ko)
                            modified = True
            
            if modified:
                save_json(filepath, data)
                updates_made += 1
                files_processed += 1

    print(f"Done. Processed {files_processed} files with updates using legacy glossary data.")

if __name__ == "__main__":
    main()
