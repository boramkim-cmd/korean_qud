import os
import json
import re
import xml.etree.ElementTree as ET
import shutil

XML_PATHS = [
    "/Users/ben/Desktop/qud_korean/Assets/StreamingAssets/Base/Mutations.xml",
    "/Users/ben/Desktop/qud_korean/Assets/StreamingAssets/Base/HiddenMutations.xml"
]
CS_DIR = "/Users/ben/Desktop/qud_korean/Assets/core_source/_GameSource/XRL.World.Parts.Mutation"
COMMON_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations_common.json"
OUTPUT_DIR = "/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS"

# ABSOLUTE MASTER LIST OF CRITICAL TRANSLATIONS (From initial passes)
RESCUE_DATA = {
    "Nerve Poppy": "신경 양귀비 (통점 상실)",
    "Sticky Tongue": "끈적한 혀",
    "You capture prey with your sticky tongue.": "끈적한 혀로 먹잇감을 낚아챕니다.",
    "You replenish yourself by absorbing sunlight through your hearty green skin.": "건강한 녹색 피부를 통해 햇빛을 흡수하여 에너지를 얻습니다.",
    "Your joints stretch much further than usual.": "관절이 평소보다 훨씬 더 유연하게 늘어납니다.",
    "You produce a viscous slime that you can spit at things.\\n\\n": "끈적한 점액을 생성하여 뱉을 수 있습니다.\\n\\n",
    "You emit a ray of frost.": "냉기 광선을 발사합니다.",
    "You generate a forcefield around yourself.": "자신 주변에 포스 필드를 생성합니다.",
    "You generate a wall of force that protects you from your enemies.": "적을 막아주는 포스 월을 생성합니다.",
    "You see in the dark.": "어둠 속에서도 볼 수 있습니다.",
    "You fly.": "당신은 비행합니다.",
    "You regenerate by absorbing heat.": "열을 흡수하여 재생합니다.",
    "You regenerate by absorbing cold.": "냉기를 흡수하여 재생합니다.",
    "You emit jets of frost from your mouth.": "입에서 냉기를 내뿜습니다.",
    "You emit a ray of flame.": "화염 광선을 발사합니다.",
    "You spontaneously erupt into flames.": "갑자기 화염에 휩싸입니다.",
    "You accrue electrical charge that you can use and discharge to deal damage.": "전하를 축축하여 피해를 입히는 데 사용할 수 있습니다.",
    "You regulate your body's release of adrenaline.": "신체의 아드레날린 분비를 조절합니다.",
    "You teleport about uncontrollably.": "통제할 수 없이 순간이동합니다.",
    "You distort time around your person in order to slow down your enemies.": "자신 주변의 시간을 왜곡하여 적들을 느리게 만듭니다.",
    "You peer into your near future.": "가까운 미래를 엿봅니다.",
    "You teleport and bring creatures along with you.": "순간이동 시 다른 생물들도 함께 데려갑니다.",
    "You disintegrate nearby matter.": "주변의 물질을 분해합니다.",
    "Horns are a short-blade class natural weapon.\n": "뿔은 단검 계열의 자연 무기입니다.\n",
    "Stinger is a long blade and can only penetrate once.": "독침은 롱 블레이드이며 한 번만 관통할 수 있습니다.",
}

def clean_filename(name):
    return re.sub(r'[^\w\s-]', '', name).strip().replace(' ', '_')

def main():
    # 1. Gather all data
    master_all_data = {}
    master_all_data.update(RESCUE_DATA)
    
    with open(COMMON_PATH, 'r', encoding='utf-8') as f:
        common = json.load(f)
        for cat, items in common.items():
            for k, v in items.items():
                master_all_data[k] = v

    if os.path.exists(OUTPUT_DIR):
        for filename in os.listdir(OUTPUT_DIR):
            if filename.endswith(".json"):
                with open(os.path.join(OUTPUT_DIR, filename), 'r', encoding='utf-8') as f:
                    try:
                        data = json.load(f)
                        for cat, items in data.items():
                            for k, v in items.items():
                                master_all_data[k] = v
                    except: pass
        shutil.rmtree(OUTPUT_DIR)
    os.makedirs(OUTPUT_DIR)

    # 2. Extract Definitions
    mutation_definitions = {}
    class_to_names = {}
    
    for xml_path in XML_PATHS:
        if not os.path.exists(xml_path): continue
        tree = ET.parse(xml_path)
        root = tree.getroot()
        for cat_elem in root.findall('category'):
            for mut_elem in cat_elem.findall('mutation'):
                name = mut_elem.get('Name')
                cls = mut_elem.get('Class')
                bearer = mut_elem.get('BearerDescription')
                desc = mut_elem.findtext('description')
                if not desc and mut_elem.find('description') is not None:
                    p = mut_elem.find('description').find('p')
                    if p is not None: desc = p.text

                if name not in mutation_definitions:
                    mutation_definitions[name] = {"class": cls, "bearer": bearer, "descs": set()}
                if desc: mutation_definitions[name]["descs"].add(desc)
                if cls: class_to_names.setdefault(cls, set()).add(name)

    # 3. C# descriptions
    for filename in os.listdir(CS_DIR):
        if filename.endswith(".cs"):
            cls_name = filename.replace(".cs", "")
            with open(os.path.join(CS_DIR, filename), 'r', encoding='utf-8') as f:
                content = f.read()
                matches = re.findall(r'return\s+"(.*?)"\s*;', content, re.DOTALL)
                for m in matches:
                    text = m.replace('""', '"').replace('\\n', '\n')
                    if cls_name in class_to_names:
                        for name in class_to_names[cls_name]:
                            mutation_definitions[name]["descs"].add(text)

    # 4. Map everything
    final_output = {}
    moved_keys = set()

    for name, defs in mutation_definitions.items():
        fname = clean_filename(name) + ".json"
        data = final_output.setdefault(fname, {})
        
        if name in master_all_data:
            data.setdefault("names", {})[name] = master_all_data[name]
            moved_keys.add(name)
        
        if defs["bearer"] and defs["bearer"] in master_all_data:
            data.setdefault("bearer_descriptions", {})[defs["bearer"]] = master_all_data[defs["bearer"]]
            moved_keys.add(defs["bearer"])
            
        for d in defs["descs"]:
            jk = d.replace('\n', '\\n')
            if jk in master_all_data:
                data.setdefault("descriptions", {})[jk] = master_all_data[jk]
                moved_keys.add(jk)
            elif d in master_all_data:
                data.setdefault("descriptions", {})[d] = master_all_data[d]
                moved_keys.add(d)

    # Secondary heuristic for fragments
    all_names = sorted(mutation_definitions.keys(), key=len, reverse=True)
    for eng, kor in master_all_data.items():
        if eng in moved_keys: continue
        for name in all_names:
            if name.lower() in eng.lower():
                fname = clean_filename(name) + ".json"
                cat = "mutation_extra"
                if "reputation" in eng.lower(): cat = "mutation_reputation"
                elif "Cooldown" in eng or "Duration" in eng: cat = "mutation_stats"
                final_output.setdefault(fname, {}).setdefault(cat, {})[eng] = kor
                moved_keys.add(eng)
                break

    # 5. Save
    for fname, content in final_output.items():
        if content:
            with open(os.path.join(OUTPUT_DIR, fname), 'w', encoding='utf-8') as f:
                json.dump(content, f, ensure_ascii=False, indent=2)

    # 6. Clean Common
    new_common = {}
    for cat, items in common.items():
        rem = {k: v for k, v in items.items() if k not in moved_keys}
        if rem: new_common[cat] = rem
    with open(COMMON_PATH, 'w', encoding='utf-8') as f:
        json.dump(new_common, f, ensure_ascii=False, indent=2)

    print(f"ULTIMATE RESCUE DONE. {len(final_output)} files. {len(moved_keys)} keys moved.")

if __name__ == "__main__":
    main()
