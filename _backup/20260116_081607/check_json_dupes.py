import json

def find_duplicates(obj, path=""):
    seen = {}
    if isinstance(obj, dict):
        for k, v in obj.items():
            new_path = f"{path}.{k}" if path else k
            # json.load already handles this, but let's use a custom decoder
            pass

def check_file(path):
    with open(path, 'r') as f:
        content = f.read()
    
    # Simple check for duplicate keys in same level
    import re
    # This is hard because of nesting.
    # Better: use a custom decoder
    
    class DuplicateCheckDecoder(json.JSONDecoder):
        def __init__(self, *args, **kwargs):
            json.JSONDecoder.__init__(self, object_pairs_hook=self.dict_with_check, *args, **kwargs)
            
        def dict_with_check(self, pairs):
            d = {}
            for k, v in pairs:
                if k in d:
                    print(f"Duplicate key found: {k}")
                d[k] = v
            return d

    try:
        json.loads(content, cls=DuplicateCheckDecoder)
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    print("Checking glossary_skills.json:")
    check_file('/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_skills.json')
    print("\nChecking glossary_mutations.json:")
    check_file('/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations.json')
    print("\nChecking glossary_chargen.json:")
    check_file('/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_chargen.json')
