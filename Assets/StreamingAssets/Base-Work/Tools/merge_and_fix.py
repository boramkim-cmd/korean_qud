import os
import xml.etree.ElementTree as ET
import json
import re

# 경로 설정
BASE_PATH = "/Users/ben/Desktop/무제 폴더/StreamingAssets/Base"
MOD_PATH = "/Users/ben/Desktop/무제 폴더/StreamingAssets/Base-Work/Mod/KoreanLocalization"

def fix_naming():
    print("Fixing Naming.xml...")
    base_file = os.path.join(BASE_PATH, "Naming.xml")
    target_file = os.path.join(MOD_PATH, "Naming.xml")
    
    with open(base_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Qudish Namestyle 교체 (XML 파싱 대신 문자열 단순 교체로 구조 보존)
    # 원본 Qudish 내용을 찾아서 교체
    
    # 1. Qudish (NPC)
    korean_qudish = '''<namestyle Name="Qudish" Format="TitleCase">
      <scopes>
        <scope Name="General" Priority="0" Combine="true" />
        <scope Name="Genotype" Genotype="Mutated Human" Priority="50" Combine="true" />
        <scope Name="Culture" Culture="Qudish" Priority="100" Combine="true" />
      </scopes>
      <prefixes Amount="1">
        <prefix Name="파" /> <!-- fa -->
        <prefix Name="하" /> <!-- ha -->
        <prefix Name="이" Weight="2" /> <!-- i -->
        <prefix Name="카" /> <!-- ka -->
        <prefix Name="키" /> <!-- ki -->
        <prefix Name="쿠" /> <!-- ku -->
        <prefix Name="마" /> <!-- ma -->
        <prefix Name="메" /> <!-- meh -->
        <prefix Name="모" /> <!-- mo -->
        <prefix Name="무" /> <!-- mu -->
        <prefix Name="나" /> <!-- na -->
        <prefix Name="니" /> <!-- ni -->
        <prefix Name="누" /> <!-- nu -->
        <prefix Name="니" /> <!-- ny -->
        <prefix Name="오" /> <!-- o -->
        <prefix Name="큐" /> <!-- q -->
        <prefix Name="슈" /> <!-- shwu -->
        <prefix Name="슈" /> <!-- shwy -->
        <prefix Name="시" /> <!-- si -->
        <prefix Name="시" /> <!-- sy -->
        <prefix Name="타" /> <!-- ta -->
        <prefix Name="티" /> <!-- ti -->
        <prefix Name="티" /> <!-- ty -->
        <prefix Name="유" /> <!-- u -->
        <prefix Name="우" /> <!-- uu -->
        <prefix Name="이" /> <!-- y -->
        <prefix Name="야" /> <!-- ya -->
        <prefix Name="이" /> <!-- yi -->
        <prefix Name="이" /> <!-- yy -->
      </prefixes>
      <infixes Amount="0-2">
        <infix Name="가" /> <!-- ga -->
        <infix Name="메" /> <!-- me -->
        <infix Name="모" /> <!-- mo -->
        <infix Name="무" /> <!-- moo -->
        <infix Name="무" /> <!-- mu -->
        <infix Name="마" /> <!-- muu -->
        <infix Name="라" /> <!-- ra -->
        <infix Name="로" /> <!-- ro -->
        <infix Name="루" /> <!-- roo -->
        <infix Name="루" /> <!-- ru -->
        <infix Name="루" /> <!-- ruu -->
        <infix Name="시" /> <!-- shi -->
        <infix Name="슈라" /> <!-- shra -->
        <infix Name="슈리" /> <!-- shri -->
        <infix Name="슈로" /> <!-- shro -->
        <infix Name="슈루" /> <!-- shru -->
        <infix Name="슈와" /> <!-- shwa -->
        <infix Name="슈오" /> <!-- shwo -->
        <infix Name="슈" /> <!-- shwu -->
        <infix Name="유" Weight="2" /> <!-- yu -->
      </infixes>
      <postfixes Amount="1">
        <postfix Name="바스" /> <!-- bas -->
        <postfix Name="드" /> <!-- d -->
        <postfix Name="르" /> <!-- jr -->
        <postfix Name="카스" /> <!-- kas -->
        <postfix Name="캇" /> <!-- kat -->
        <postfix Name="라" /> <!-- la -->
        <postfix Name="음" /> <!-- m -->
        <postfix Name="메트" /> <!-- met -->
        <postfix Name="미르" /> <!-- mir -->
        <postfix Name="무트" /> <!-- mut -->
        <postfix Name="큐" /> <!-- q -->
        <postfix Name="캇" /> <!-- qat -->
        <postfix Name="라크" /> <!-- raq -->
        <postfix Name="렘" /> <!-- rem -->
        <postfix Name="로크" /> <!-- roq -->
        <postfix Name="슘" /> <!-- shum -->
        <postfix Name="셔" /> <!-- shur -->
        <postfix Name="텝" /> <!-- tep -->
        <postfix Name="운" /> <!-- un -->
        <postfix Name="우르" /> <!-- ur -->
        <postfix Name="운" /> <!-- uun -->
        <postfix Name="와르" /> <!-- war -->
        <postfix Name="웨어" /> <!-- wer -->
        <postfix Name="워" /> <!-- wur -->
      </postfixes>    
    </namestyle>'''

    # 정규표현식으로 Qudish Namestyle 찾아서 교체
    # 주의: XML 태그 구조가 복잡하므로 단순 문자열 치환 시도
    # 원본 파일에서 <namestyle Name="Qudish" ... </namestyle> 블록을 찾기 위한 패턴
    pattern_qudish = r'<namestyle Name="Qudish" Format="TitleCase">.*?</namestyle>'
    content = re.sub(pattern_qudish, korean_qudish, content, flags=re.DOTALL)

    # 2. Qudish Site (Locations)
    korean_site = '''<namestyle Name="Qudish Site" Format="TitleCase">
      <scopes>
        <scope Name="Main" Type="Site" Priority="100" Combine="false" />
      </scopes>
      <prefixes Amount="1">
        <prefix Name="아" />
        <prefix Name="알라" />
        <prefix Name="비" />
        <prefix Name="다" />
        <prefix Name="두" />
        <prefix Name="에카" />
        <prefix Name="하" />
        <prefix Name="이" />
        <prefix Name="카" />
        <prefix Name="키" />
        <prefix Name="마" />
        <prefix Name="미" />
        <prefix Name="나" />
        <prefix Name="카" />
        <prefix Name="사" />
        <prefix Name="샤" />
        <prefix Name="쉐" />
        <prefix Name="슈" />
        <prefix Name="수" />
        <prefix Name="타" />
        <prefix Name="테" />
        <prefix Name="투" />
        <prefix Name="우" />
      </prefixes>
      <infixes Amount="0-1">
        <infix Name="아라" />
        <infix Name="아조" />
        <infix Name="바" />
        <infix Name="바이" />
        <infix Name="가" />
        <infix Name="가" />
        <infix Name="일리" />
        <infix Name="므리" />
        <infix Name="네" />
        <infix Name="라" />
        <infix Name="르체" />
        <infix Name="르카" />
        <infix Name="르쿠" />
        <infix Name="르시" />
        <infix Name="르바" />
      </infixes>
      <postfixes Amount="1">
        <postfix Name="발" />
        <postfix Name="드" />
        <postfix Name="케쉬" />
        <postfix Name="크" />
        <postfix Name="키쉬" />
        <postfix Name="렙" />
        <postfix Name="릴" />
        <postfix Name="마스" />
        <postfix Name="미쉬" />
        <postfix Name="무" />
        <postfix Name="모르" />
        <postfix Name="므로드" />
        <postfix Name="무르" />
        <postfix Name="닙" />
        <postfix Name="나" />
        <postfix Name="패드" />
        <postfix Name="파" />
        <postfix Name="파르" />
        <postfix Name="피르" />
        <postfix Name="푸르" />
        <postfix Name="르" />
        <postfix Name="루크" />
        <postfix Name="쉬" />
        <postfix Name="샨" />
        <postfix Name="셔" />
        <postfix Name="타라" />
        <postfix Name="툼" />
        <postfix Name="바" />
        <postfix Name="부" />
        <postfix Name="완" />
        <postfix Name="와르" />
        <postfix Name="조르" />
      </postfixes>
    </namestyle>'''
    pattern_site = r'<namestyle Name="Qudish Site" Format="TitleCase">.*?</namestyle>'
    content = re.sub(pattern_site, korean_site, content, flags=re.DOTALL)

    # 3. 템플릿 교체 (Banana Grove 등)
    templates = {
        'Banana Grove *Name*': '바나나 숲 *Name*',
        'Baroque Ruins *Name*': '바록 *Name* 폐허',
        'Deep Jungle *Name*': '깊은 정글 *Name*',
        'Desert Canyon *Name*': '사막 협곡 *Name*',
        'Flower Fields *Name*': '꽃밭 *Name*',
        'Fungal *Name*': '곰팡이 숲 *Name*',
        'Hills *Name*': '언덕 *Name*',
        'Jungle *Name*': '정글 *Name*',
        'Water *Name*': '호수 *Name*',
        'Moon Stair *Name*': '달의 계단 *Name*',
        'Mountains *Name*': '산맥 *Name*',
        'Palladium Reef *Name*': '팔라듐 산호초 *Name*',
        'Ruins *Name*': '폐허 *Name*',
        'Salt Dunes *Name*': '소금 사구 *Name*',
        'Salt Marsh *Name*': '소금 늪 *Name*',
        'Spindle *Name*': '스핀들 *Name*'
    }

    for eng, kor in templates.items():
        # 정규표현식에서 *는 특수문자이므로 escape 필요
        escaped_eng = re.escape(eng).replace(r'\*', '*') 
        # XML에서는 *Name*이 그대로 쓰임
        content = content.replace(f'Name="{eng}"', f'Name="{kor}"')

    with open(target_file, 'w', encoding='utf-8') as f:
        f.write(content)

def fix_history_spice():
    print("Fixing HistorySpice.json...")
    base_file = os.path.join(BASE_PATH, "HistorySpice.json")
    target_file = os.path.join(MOD_PATH, "HistorySpice.json")

    with open(base_file, 'r', encoding='utf-8') as f:
        data = json.load(f)

    # 공통 문구 번역 (일부 important 한 것만 예시로)
    phrases = data['spice']['commonPhrases']
    
    # LeadIns
    phrases['leadIns'] = [
        "=year=년 =month=월, =name=(이)가 크롬 아치 아래를 걸었다",
        "=year=년, =name=(은)는 거슬링 무리(을)를 물리쳤다",
        "=month=월의 어느 날, =name=(이)가 시들어가는 작물(을)를 치유했다",
        "=year=년, =name=(이)가 =place=(으)로 여행했다",
        "=month=월, =name=(은)는 전설적인 =artifact=(을)를 발견했다"
    ]

    # oneStarryNight (스크린샷 문제 해결)
    if 'oneStarryNight' in phrases:
        phrases['oneStarryNight'] = [
            "어느 별이 빛나는 밤",
            "보름달이 뜬 밤에",
            "<spice.myth.mythicDays.!random>의 새벽에",
            "딱정벌레 달 아래 어느 밤",
            "어느 상서로운 날",
            "위대한 전투의 기념일에" # "on the anniversary of a great battle"
        ]
    
    # scion (자손)
    if 'scion' in phrases:
        phrases['scion'] = ["아이", "어린 양", "후계자", "자손", "씨앗", "친족", "아기", "상속녀", "자식", "새싹"]
    
    # found (발견됨)
    if 'found' in phrases:
        phrases['found'] = ["발견되었다", "찾아졌다"]

    # wrapped (싸인)
    if 'wrapped' in phrases:
        phrases['wrapped'] = ["감싸인 채로", "덮인 채로", "싸인 채로"]

    # roost (둥지/거처) -> NameGenFail83 Roost 문제 해결
    # Roost가 Naming.xml이 아니라 여기서 쓰이는 접미사일 수 있음. 확인 결과 HistorySpice에 'hearth' 등이 있음.
    # 하지만 "NameGenFail83 Roost"는 아마도 "NameGenFail83"이 지명이고 "Roost"가 뒤에 붙는 텍스트일 것.
    # "Roost" 텍스트를 찾아서 번역
    if 'hearth' in phrases:
        phrases['hearth'] = ["난로", "집", "거처", "소굴", "본거지", "둥지"]
    
    # 저장
    with open(target_file, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)

def fix_quests():
    print("Fixing Quests.xml...")
    base_file = os.path.join(BASE_PATH, "Quests.xml")
    target_file = os.path.join(MOD_PATH, "Quests.xml")
    
    with open(base_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # 주요 퀘스트 번역 맵
    translations = {
        'Name="A Canticle for Barathrum"': 'Name="A Canticle for Barathrum" DisplayName="바라스럼을 위한 찬가"',
        'Name="What\'s Eating the Watervine?"': 'Name="What\'s Eating the Watervine?" DisplayName="워터바인에 무슨 일이?"',
        'Name="Fetch Argyve a Knickknack"': 'Name="Fetch Argyve a Knickknack" DisplayName="아르기브에게 고대고철 가져다주기"',
        'Name="Fetch Argyve Another Knickknack"': 'Name="Fetch Argyve Another Knickknack" DisplayName="아르기브에게 또 다른 고대고철 가져다주기"',
        'Name="Weirdwire Conduit... Eureka!"': 'Name="Weirdwire Conduit... Eureka!" DisplayName="위어드와이어 도관... 유레카!"',
        'Name="Looking for Work"': 'Name="Looking for Work" DisplayName="일거리 찾기"',
        '>Travel to Red Rock<': '>붉은 바위로 여행하라<',
        '>Find the Vermin<': '>해충 찾기<',
        '>Journey two parasangs north of Joppa to Red Rock.<': '>조파에서 북쪽으로 2 파라상 떨어진 붉은 바위로 여행하라.<',
        '>Find the creatures that are eating Joppa\'s watervine.<': '>조파의 워터바인을 먹고 있는 생물을 찾아라.<',
        '>Collect a corpse from one of the creatures.<': '>그 생물 중 하나의 시체를 수집하라.<',
        '>Return to Joppa with the corpse of the watervine vermin.<': '>워터바인 해충의 시체를 가지고 조파로 돌아가라.<',
        '>Search the regions surrounding Joppa for an artifact.<': '>조파 주변 지역에서 아티팩트(고대고철)를 찾아라.<',
        '>Bring the artifact to Argyve.<': '>아르기브에게 아티팩트를 가져가라.<',
        '>Travel to the ruined, subterranean gate northeast of Joppa.<': '>조파 북동쪽의 폐허가 된 지하 관문으로 여행하라.<',
        '>Locate the Barathrumite enclave within Grit Gate<': '>그릿 게이트 내부의 바라스럼 추종자 거주지를 찾아라<',
        '>Ask the Barathrumites about Argyve\'s strange signal.<': '>바라스럼 추종자들에게 아르기브의 이상한 신호에 대해 물어보라.<'
    }
    
    for eng, kor in translations.items():
        content = content.replace(eng, kor)
        
    with open(target_file, 'w', encoding='utf-8') as f:
        f.write(content)

if __name__ == "__main__":
    fix_naming()
    fix_history_spice()
    fix_quests()
