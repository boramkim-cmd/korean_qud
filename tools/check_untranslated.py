import os
import json
import re

for root, dirs, files in os.walk("LOCALIZATION/SUBTYPES"):
    for file in files:
        if file.endswith(".json"):
            path = os.path.join(root, file)
            with open(path, 'r', encoding='utf-8') as f:
                data = json.load(f)
                if "leveltext_ko" in data:
                    for line in data["leveltext_ko"]:
                        # Check for Latin characters
                        if re.search(r"[A-Za-z]", line):
                            # Allow "Stat +N" format if Stat is korean? No, Stat is usually "Strength +1" in English if untranslated.
                            # But I mapped stats to Korean: "힘 +1".
                            # So "Strength +1" IS untranslated.
                            # Allow "Name (English)" format? e.g. "비방(Nostrums)".
                            if re.search(r"\(.*\)", line):
                                continue # Likely localized with English suffix
                                
                            # Check known patterns to ignore?
                            # If it contains Hangul, it's partially translated.
                            has_hangul = re.search(r"[가-힣]", line)
                            
                            if not has_hangul:
                                print(f"{file}: {line}")
