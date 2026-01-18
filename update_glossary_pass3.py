import json
import os

GLOSSARY_PATH = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_mutations.json"
EXTRACTION_PATH = "/Users/ben/Desktop/qud_korean/extra_extraction.json"

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

    # Descriptions / Fragments
    "20% chance on melee attack to gore your opponent\\n": "근접 공격 시 20% 확률로 상대를 들이받음\\n",
    "Active mutations cost nothing to use but give you no attribute bonuses.": "활성 변이는 사용 시 비용이 들지 않지만 능력치 보너스를 제공하지 않습니다.",
    "Bilge sphincter acts as a melee weapon.\\n": "빌지 괄약근이 근접 무기로 작동합니다.\\n",
    "Horns jut out of your head.": "머리에 뿔이 솟아나 있습니다.",
    "Several horns jut out of your head.": "머리에 여러 개의 뿔이 솟아나 있습니다.",
    "Hundreds of needle-pointed quills cover your body.": "수백 개의 바늘 같은 가시가 온몸을 덮고 있습니다.",
    "Through sheer force of will, you perform uncanny physical feats.": "순수한 의지의 힘으로 초인적인 신체 능력을 발휘합니다.",
    "You are a crystalline being.": "당신은 결정체 존재입니다.",
    "You are gifted with tremendous speed.": "엄청난 속도를 타고났습니다.",
    "You are possessed of exceptionally acute smell.": "예외적으로 예리한 후각을 가지고 있습니다.",
    "You are possessed of hulking strength.": "엄청난 괴력을 지니고 있습니다.",
    "You are possessed of unnaturally acute hearing.": "비정상적으로 예리한 청각을 가지고 있습니다.",
    "You are protected by a durable carapace.": "튼튼한 갑각으로 보호받습니다.",
    "You are unusually large.": "비정상적으로 거대합니다.",
    "You assume the form of any creature you touch.": "접촉한 생물의 형태로 변신합니다.",
    "You bear a sphincter-choked bilge hose that you use to slurp up nearby liquids and spew them at enemies, occasionally knocking them down.": "빌지 호스를 사용하여 주변의 액체를 빨아들인 뒤 적에게 뿜어낼 수 있으며, 때때로 적을 쓰러뜨립니다.",
    "You bear spade-like claws that can burrow through the earth.": "땅을 팔 수 있는 삽 모양의 발톱을 가지고 있습니다.",
    "You bear two spinnerets with which you spin a sticky silk.\\n": "두 개의 거미줄 생성기로 끈적한 거미줄을 뽑아냅니다.\\n",
    "You beguile a nearby creature into serving you loyally.": "주변 생물을 유혹하여 충직한 하인으로 만듭니다.",
    "You belch urchins in a nearby area.\\n": "주변 지역에 성게를 게웁니다.\\n",
    "You bond with a nearby organic creature and leech its life force.": "주변의 유기체 생물과 연결하여 생명력을 흡수합니다.",
    "You breathe confusion gas.": "혼란 가스를 내뿜습니다.",
    "You breathe corrosive gas.": "부식성 가스를 내뿜습니다.",
    "You breathe fire.": "화염을 내뿜습니다.",
    "You breathe ice.": "얼음을 내뿜습니다.",
    "You breathe normality gas.": "정상성 가스를 내뿜습니다.",
    "You breathe poison gas.": "독 가스를 내뿜습니다.",
    "You breathe shame gas.": "수치심 가스를 내뿜습니다.",
    "You breathe sleep gas.": "수면 가스를 내뿜습니다.",
    "You breathe stun gas.": "기절 가스를 내뿜습니다.",
    "You capture prey with your sticky tongue.": "끈적한 혀로 먹잇감을 낚아챕니다.",
    "You cause plants to spontaneously grow in a nearby area, hindering your enemies.": "주변 지역에 식물을 급속도로 자라게 하여 적을 방해합니다.",
    "You chill a nearby area with your mind.": "정신력으로 주변 지역을 냉각시킵니다.",
    "You confuse nearby enemies.": "주변 적들을 혼란에 빠뜨립니다.",
    "You dash along a waveform.": "파형을 따라 질주합니다.",
    "You disintegrate nearby matter.": "주변의 물질을 분해합니다.",
    "You distort time around your person in order to slow down your enemies.": "자신 주변의 시간을 왜곡하여 적들을 느리게 만듭니다.",
    "You emit jets of frost from your mouth.": "입에서 냉기를 뿜어냅니다.",
    "You emit powerful magnetic pulses.": "강력한 자기 펄스를 방출합니다.",
    "You extract carbon from living material.": "생물학적 물질에서 탄소를 추출합니다.",
    "You fly.": "당신은 날 수 있습니다.",
    "You garrote an adjacent creature's mind and control its actions while your own body lies dormant.": "인접한 생물의 정신을 옥죄어 자신의 몸이 휴면 상태인 동안 그 행동을 조종합니다.",
    "You generate a forcefield around yourself.": "자신 주변에 포스 필드를 생성합니다.",
    "You generate a wall of force that protects you from your enemies.": "적을 막아주는 포스 월을 생성합니다.",
    "You generate an electromagnetic pulse that disables nearby artifacts and machines.": "전자기 펄스를 발생시켜 주변의 아티팩트와 기계를 무력화합니다.",
    "You have an extra set of arms.": "팔이 한 쌍 더 있습니다.",
    "You have an extra set of legs.": "다리가 한 쌍 더 있습니다.",
    "You have two heads.": "머리가 두 개입니다.",
    "You have two hearts.": "심장이 두 개입니다.",
    "You heat a nearby area with your mind.": "정신력으로 주변 지역을 가열합니다.",
    "You invoke a concussive force in a nearby area, throwing enemies back and stunning them.": "주변 지역에 충격파를 일으켜 적들을 밀쳐내고 기절시킵니다.",
    "You invoke a repelling force in the surrounding area, throwing enemies back.": "주변 지역에 척력을 일으켜 적들을 밀쳐냅니다.",
    "You live in an alternate plane of reality.": "차원이 다른 현실 평면에서 살아갑니다.",
    "You manipulate light to your advantage.": "빛을 조작하여 유리하게 사용합니다.",
    "You peer into your near future.": "가까운 미래를 엿봅니다.",
    "You possess a towering vision of self that you project onto the minds of nearby creatures.": "거대한 자아의 형상을 주변 생물들의 정신에 투영합니다.",
    "You possess extraordinary analytical prowess but you find difficulty in relating to others.": "비범한 분석 능력을 갖추었으나 타인과의 관계에는 어려움을 겪습니다.",
    "You provoke waking dreams with your gaze.": "시선으로 백일몽을 유발합니다.",
    "You quickly pass back and forth through time creating multiple copies of yourself.": "시간을 빠르게 왕복하며 자신의 복제본을 여러 개 생성합니다.",
    "You read the history of artifacts by touching them, learning what they do and how they were made.": "아티팩트를 만져 그 역사를 읽어내고, 용도와 제작 방법을 알아냅니다.",
    "You reflect mental attacks back at your attackers.": "정신 공격을 공격자에게 반사합니다.",
    "You reflect the shameful countenance of nearby creatures.": "주변 생물들의 수치스러운 면모를 반사합니다.",
    "You regenerate by absorbing cold.": "냉기를 흡수하여 재생합니다.",
    "You regenerate by absorbing heat.": "열을 흡수하여 재생합니다.",
    "You regulate your body's release of adrenaline.": "신체의 아드레날린 분비를 조절합니다.",
    "You replenish yourself by absorbing sunlight through your hearty green skin.": "건강한 녹색 피부를 통해 햇빛을 흡수하여 기력을 회복합니다.",
    "You scare creatures around you.": "주변의 생물들을 겁나게 합니다.",
    "You see in the dark.": "어둠 속에서도 볼 수 있습니다.",
    "You sunder spacetime, sending things nearby careening through a tear in the cosmic fabric.": "시공간을 찢어 주변의 물체를 우주의 틈새로 날려버립니다.",
    "You sunder the mind of an enemy, leaving them reeling in pain.": "적의 정신을 찢어발겨 고통 속에 몸부림치게 만듭니다.",
    "You tap into the aggregate mind and steal power from other espers.": "집단 지성에 접속하여 다른 에스퍼들의 힘을 훔칩니다.",
    "You teleport an adjacent creature to a random nearby location.": "인접한 생물을 무작위 주변 위치로 순간이동시킵니다.",
    "You teleport and bring creatures along with you.": "순간이동 시 다른 생물도 함께 데려갑니다.",
    "You turn things to stone with your gaze.": "시선으로 대상을 석화시킵니다.",
    "Your bones are brittle.": "뼈가 잘 부러집니다.",
    "Your joints stretch much further than usual.": "관절이 평소보다 훨씬 더 유연하게 움직입니다.",
    "Your wounds heal very quickly.": "상처가 매우 빠르게 치유됩니다.",

    # New fragments
    "natural weapon": "자연 무기",
    "short-blade class": "단검 계열",
    "is a short-blade class natural weapon.\n": "은(는) 단검 계열의 자연 무기입니다.\n",
    "are a short-blade class natural weapon.\n": "은(는) 단검 계열의 자연 무기들입니다.\n",
    "Bonus duration: {{rules|": "추가 지속 시간: {{rules|",
    "Damage increment: {{rules|": "피해 증가량: {{rules|",
    "To-hit bonus: {{rules|": "명중 보너스: {{rules|",
    "Goring attacks may cause bleeding\\n": "들이받기 공격은 출혈을 일으킬 수 있음\\n",
    "{{rules|Increased bleeding save difficulty}}\\n": "{{rules|출혈 저항 난이도 증가}}\\n",
    "{{rules|Increased bleeding save difficulty and intensity}}\\n": "{{rules|출혈 저항 난이도 및 강도 증가}}\\n",
    "Cannot wear helmets\\n": "헬멧 착용 불가\\n",
    "reputation with": "평판",
    "Stinger is a long blade and can only penetrate once.\\nAlways sting on charge or lunge.\\nStinger applies venom on damage (only 20% chance if Stinger is your primary weapon).\\nMay use Sting activated ability to strike with your stinger and automatically hit and penetrate.\\nSting cooldown: ": "독침은 롱 블레이드이며 한 번만 관통할 수 있습니다.\\n돌진이나 런지 시 항상 독침을 쏩니다.\\n독침은 피해를 입힐 때 독을 주입합니다(독침이 주무기인 경우 확률 20%).\\n독침 활성 능력을 사용하여 독침으로 공격하고 자동으로 명중 및 관통할 수 있습니다.\\n독침 재사용 대기시간: ",
    "You can travel underground by burrowing.": "땅을 파서 지하로 이동할 수 있습니다.",
    "\\nUseful in many tinkering Sifrah games.": "\\n많은 제작 시프라 게임에서 유용합니다.",
    "\\nUseful in many social and psionic Sifrah games.": "\\n많은 사교 및 사이오닉 시프라 게임에서 유용합니다.",
}

# Add more common small fragments
SMALL_FRAGS = {
    "Range: 12\\n": "사거리: 12\\n",
    "Range: 8\\n": "사거리: 8\\n",
    "Cooldown: {{rules|": "재사용 대기시간: {{rules|",
    "Duration: {{rules|": "지속 시간: {{rules|",
    "Area: {{rules|": "범위: {{rules|",
    "Area: 3x3\\n": "범위: 3x3\\n",
    "Area: 7x7": "범위: 7x7",
    "penetration vs. walls: {{rules|": "벽 관통력: {{rules|",
    "Damage: {{rules|": "피해량: {{rules|",
}
TRANSLATIONS.update(SMALL_FRAGS)

def main():
    with open(GLOSSARY_PATH, 'r', encoding='utf-8') as f:
        glossary = json.load(f)
    
    # Update mutation_factions (new category possibly?)
    # Let's put factions in "factions" or "common" or "mutation_reputation"
    if "factions" not in glossary:
        glossary["factions"] = {}
    
    for f_eng, f_kor in TRANSLATIONS.items():
        # Factions go to "factions"
        if f_eng in ["antelopes", "apes", "arachnids", "birds", "frogs", "highly entropic beings", "roots", "the Consortium of Phyta", "the Seekers of the Sightless Way", "unshelled reptiles"]:
            glossary["factions"][f_eng] = f_kor
        # Descriptions go to relevant categories or a new catch-all for Pass 3
        else:
            # Check existing categories first to avoid duplicates or update them
            found = False
            for cat in ["mutation_desc_physical_body", "mutation_desc_physical_breath", "mutation_desc_physical_passive", "mutation_desc_mental", "mutation_desc_defect", "mutation_frag_stats", "mutation_frag_text"]:
                if cat in glossary and f_eng in glossary[cat]:
                    glossary[cat][f_eng] = f_kor
                    found = True
                    break
            
            if not found:
                # Put in "mutation_pass3_extra"
                if "mutation_pass3_extra" not in glossary:
                    glossary["mutation_pass3_extra"] = {}
                glossary["mutation_pass3_extra"][f_eng] = f_kor

    # Save sorted
    with open(GLOSSARY_PATH, 'w', encoding='utf-8') as f:
        json.dump(glossary, f, ensure_ascii=False, indent=2)

    print("Glossary updated with Pass 3 translations.")

if __name__ == "__main__":
    main()
