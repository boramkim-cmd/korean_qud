import json
import os
import re

MUTATIONS_DIR = "/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS"
COMMON_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations_common.json"

TRANSLATIONS = {
    # Factions
    "antelopes": "영양",
    "apes": "유인원",
    "arachnids": "거미류",
    "birds": "새",
    "frogs": "개구리",
    "highly entropic beings": "고엔트로피 존재",
    "roots": "뿌리",
    "the Consortium of Phyta": "피타 컨소시엄",
    "the Seekers of the Sightless Way": "보이지 않는 길의 탐구자",
    "unshelled reptiles": "껍질 없는 파충류",

    # Stinger specifically (for the screenshot fix)
    "You bear a tail with a stinger that delivers paralyzing venom to your enemies.": "당신은 적에게 마비 독을 주입하는 침이 달린 꼬리를 가지고 있습니다.",
    "20% chance on melee attack to sting your opponent": "근접 공격 시 20% 확률로 상대를 찌름",
    "Stinger is a long blade and can only penetrate once.": "독침은 롱 블레이드이며 한 번만 관통할 수 있습니다.",
    "Always sting on charge or lunge.": "돌진이나 런지 시 항상 독침을 쏩니다.",
    "Stinger applies venom on damage (only 20% chance if Stinger is your primary weapon).": "독침은 피해를 입힐 때 독을 주입합니다(독침이 주무기인 경우 확률 20%).",
    "May use Sting activated ability to strike with your stinger and automatically hit and penetrate.": "독침 활성 능력을 사용하여 독침으로 공격하고 자동으로 명중 및 관통할 수 있습니다.",
    "Sting cooldown: ": "독침 재사용 대기시간: ",
    "Venom paralyzes opponents for ": "독이 상대를 다음 시간 동안 마비시킵니다: ",
    " rounds": "턴",
    "+200 reputation with arachnids": "거미류 평판 +200",

    # Horns specifically
    "Horns are a short-blade class natural weapon.\n": "뿔은 단검 계열의 자연 무기입니다.\n",
    "Horns jut out of your head.": "머리에 뿔이 솟아나 있습니다.",
    "Damage increment: {{rules|": "피해 증가량: {{rules|",
    "To-hit bonus: {{rules|": "명중 보너스: {{rules|",
    "Goring attacks may cause bleeding\\n": "들이받기 공격은 출혈을 일으킬 수 있음\\n",
    "{{rules|Increased bleeding save difficulty}}\\n": "{{rules|출혈 저항 난이도 증가}}\\n",
    "{{rules|Increased bleeding save difficulty and intensity}}\\n": "{{rules|출혈 저항 난이도 및 강도 증가}}\\n",
    "Cannot wear helmets\\n": "헬멧 착용 불가\\n",
    "+100 reputation with {{w|antelopes}} and {{w|goatfolk}}": "{{w|영양}} 및 {{w|염소인간}} 평판 +100",

    # Fragments
    "Range: 8\\n": "사거리: 8\\n",
    "Range: 12\\n": "사거리: 12\\n",
    "Cooldown: 10 rounds\\n": "재사용 대기시간: 10턴\\n",
    "Cooldown: {{rules|": "재사용 대기시간: {{rules|",
    "Duration: {{rules|": "지속 시간: {{rules|",
    "Area: {{rules|": "범위: {{rules|",
}

def distribute_translation(eng, kor):
    # Try to find a file where 'eng' matches or is likely to belong
    found = False
    
    # Heuristic: search all files for the key 'eng' (lowercase) or the mutation name in the string
    for filename in os.listdir(MUTATIONS_DIR):
        filepath = os.path.join(MUTATIONS_DIR, filename)
        with open(filepath, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        # Check if the mutation name (filename) is in the string
        m_name = filename.replace('.json', '').replace('_', ' ')
        if m_name.lower() in eng.lower():
            # Add to this file
            if "mutation_pass3_extra" not in data:
                data["mutation_pass3_extra"] = {}
            data["mutation_pass3_extra"][eng] = kor
            found = True
            with open(filepath, 'w', encoding='utf-8') as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
            break

    if not found:
        # Add to common
        with open(COMMON_PATH, 'r', encoding='utf-8') as f:
            common = json.load(f)
        if "mutation_pass3_extra" not in common:
            common["mutation_pass3_extra"] = {}
        common["mutation_pass3_extra"][eng] = kor
        with open(COMMON_PATH, 'w', encoding='utf-8') as f:
            json.dump(common, f, ensure_ascii=False, indent=2)

def main():
    for eng, kor in TRANSLATIONS.items():
        distribute_translation(eng, kor)
    print("Pass 3 translations distributed.")

if __name__ == "__main__":
    main()
