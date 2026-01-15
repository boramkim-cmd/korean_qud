import json
from collections import OrderedDict

def clean_json(path):
    with open(path, 'r', encoding='utf-8') as f:
        # Load as a list of pairs to preserve order and catch duplicates
        def dict_with_order(pairs):
            d = OrderedDict()
            for k, v in pairs:
                d[k] = v
            return d
        
        data = json.load(f, object_pairs_hook=dict_with_order)
        
    with open(path, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

if __name__ == "__main__":
    clean_json('/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_skills.json')
    clean_json('/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations.json')
    clean_json('/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_chargen.json')
    print("Cleaned up duplicates.")
