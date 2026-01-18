import json
import os

GLOSSARY_DIR = "/Users/ben/Desktop/qud_korean/LOCALIZATION/MUTATIONS"
COMMON_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations_common.json"

BULK_TRANSLATIONS = {
    "You replenish yourself by absorbing sunlight through your hearty green skin.": "건강한 녹색 피부를 통해 햇빛을 흡수하여 에너지를 얻습니다.",
    "Your joints stretch much further than usual.": "관절이 평소보다 훨씬 더 유연하게 늘어납니다.",
    "You capture prey with your sticky tongue.": "끈적한 혀로 먹잇감을 낚아챕니다.",
    "You have an extra set of legs.": "다리가 한 쌍 더 달려 있습니다.",
    "You produce a viscous slime that you can spit at things.\\n\\n": "끈적한 점액을 생성하여 뱉을 수 있습니다.\\n\\n",
    "You emit a ray of frost.": "냉기 광선을 발사합니다.",
    "You generate a forcefield around yourself.": "자신 주변에 포스 필드를 생성합니다.",
    "You generate a wall of force that protects you from your enemies.": "적을 막아주는 포스 월을 생성합니다.",
    "You see in the dark.": "어둠 속에서도 볼 수 있습니다.",
    "You fly.": "당신은 비행합니다.",
    "You regenerate hit points at one-fifth the usual rate in the daylight.": "낮에는 체력 재생 속도가 평소의 1/5로 감소합니다.",
    "You regenerate by absorbing heat.": "열을 흡수하여 재생합니다.",
    "You regenerate by absorbing cold.": "냉기를 흡수하여 재생합니다.",
    "You emit jets of frost from your mouth.": "입에서 냉기를 내뿜습니다.",
    "You emit a ray of flame.": "화염 광선을 발사합니다.",
    "You spontaneously erupt into flames.": "갑자기 화염에 휩싸입니다.",
    "You accrue electrical charge that you can use and discharge to deal damage.": "전하를 축축하여 피해를 입히는 데 사용할 수 있습니다.",
    "You regulate your body's release of adrenaline.": "신체의 아드레날린 분비를 조절합니다.",
    "You detect the presence of psychic enemies within a radius of": "반경 내의 사이킥 적을 감지합니다: ",
    "You teleport about uncontrollably.": "통제할 수 없이 순간이동합니다.",
    "You distort time around your person in order to slow down your enemies.": "자신 주변의 시간을 왜곡하여 적들을 느리게 만듭니다.",
    "You peer into your near future.": "가까운 미래를 엿봅니다.",
    "You teleport and bring creatures along with you.": "순간이동 시 다른 생물들도 함께 데려갑니다.",
    "You disintegrate nearby matter.": "주변의 물질을 분해합니다.",
    "\\nAdds a bonus turn and improves performance in psionic Sifrah games.": "\\n사이오닉 시프라 게임에서 보너스 턴을 추가하고 성과를 높여줍니다.",
    "Area: 3x3\\n": "범위: 3x3\\n",
    "Cooldown: 10 rounds\\n": "재사용 대기시간: 10턴\\n",
    "Range: 8\\n": "사거리: 8\\n",
    "You detect the presence of creatures within a radius of {{rules|": "반경 {{rules| 내의 생물을 감지합니다.",
    "penetration vs. walls: {{rules|": "벽 관통력: {{rules|",
    "Damage increment: {{rules|": "피해 증가량: {{rules|",
    "To-hit bonus: {{rules|": "명중 보너스: {{rules|",
    "Horns are a short-blade class natural weapon.\n": "뿔은 단검 계열의 자연 무기입니다.\n",
    "Stinger is a long blade and can only penetrate once.": "독침은 롱 블레이드이며 한 번만 관통할 수 있습니다.",
}

def main():
    # Load common as a fallback distribution
    with open(COMMON_PATH, 'r', encoding='utf-8') as f:
        common = json.load(f)

    for eng, kor in BULK_TRANSLATIONS.items():
        distributed = False
        # Try to find matching file
        for filename in os.listdir(GLOSSARY_DIR):
            m_name = filename.replace('.json', '').replace('_', ' ')
            if m_name.lower() in eng.lower():
                filepath = os.path.join(GLOSSARY_DIR, filename)
                with open(filepath, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                data.setdefault("mutation_pass3_extra", {})[eng] = kor
                with open(filepath, 'w', encoding='utf-8') as f:
                    json.dump(data, f, ensure_ascii=False, indent=2)
                distributed = True
                break
        
        if not distributed:
             common.setdefault("mutation_pass3_extra", {})[eng] = kor

    with open(COMMON_PATH, 'w', encoding='utf-8') as f:
        json.dump(common, f, ensure_ascii=False, indent=2)

    print("Bulk translation integrated.")

if __name__ == "__main__":
    main()
