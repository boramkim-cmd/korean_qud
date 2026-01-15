#!/usr/bin/env python3
import sys
import re
import os
import xml.etree.ElementTree as ET

# 1. Comprehensive core translations extracted from user's provided list
raw_core = {
    "DISABLE BLOOD SPLATTERS": "혈흔 비활성화",
    "DISABLE BRAIN HIJACKING ON PLAYER-CONTROLLED CHARACTERS": "플레이어 조종 캐릭터의 뇌 탈취 비활성화",
    "DISABLE CONFLICT CHECKING WHEN REBINDING CONTROLS": "조작 재설정 시 충돌 확인 비활성화",
    "DISABLE FLOOR TEXTURES": "바닥 텍스처 비활성화",
    "DISABLE FULLSCREEN SCREEN-WARPING EFFECTS": "전체 화면 화면 왜곡 효과 비활성화",
    "DISABLE INPUT WARNING": "입력 경고 비활성화",
    "DISABLE LIMIT ON ZONE BUILD TRIES": "구역 빌드 시도 제한 비활성화",
    "DISABLE MODERN, TILE-BASED GRAPHICAL EFFECTS": "최신 타일 기반 그래픽 효과 비활성화",
    "DISABLE MOST TILE-BASED FLASHING EFFECTS": "대부분의 타일 기반 깜빡임 효과 비활성화",
    "DISABLE PRELOADING OF SOUNDS": "사운드 사전 로딩 비활성화",
    "DISABLE SMOKE EFFECTS": "연기 효과 비활성화",
    "DISABLE TILE-BASED SCREEN-WARPING EFFECTS": "타일 기반 화면 왜곡 효과 비활성화",
    "DISABLE ZONE CACHING": "구역 캐싱 비활성화",
    "DISPLAY ABILITY ICONS ON THE SIDEBAR": "사이드바에 능력 아이콘 표시",
    "DISPLAY ASCII VISUAL EFFECTS": "ASCII 시각 효과 표시",
    "DISPLAY COMBAT ANIMATIONS": "전투 애니메이션 표시",
    "DISPLAY CONTENTS OF CURRENT CELL IN A POPUP": "현재 칸의 내용을 팝업으로 표시",
    "DISPLAY DETAILED WEAPON PENETRATION AND DAMAGE IN WEAPON NAMES": "무기 이름에 상세한 관통력과 피해량 표시",
    "DISPLAY FLOATING DAMAGE NUMBERS": "부동 피해량 숫자 표시",
    "DISPLAY LOCATION INSTEAD OF NAME ON THE SIDEBAR": "사이드바에 이름 대신 위치 표시",
    "DISPLAY MODERN VISUAL EFFECTS": "최신 시각 효과 표시",
    "DISPLAY MOUSE-CLICKABLE ZONE TRANSITION ARROWS": "마우스 클릭 가능한 구역 전환 화살표 표시",
    "DISPLAY OVERLAY KEYBOARD FOR INPUT ON SUPPORTED DEVICES": "지원되는 기기에서 입력용 오버레이 키보드 표시",
    "DISPLAY POPUPS WHEN NOTING INFORMATION IN YOUR JOURNAL": "저널에 정보를 기록할 때 팝업 표시",
    "DISPLAY SCANLINES": "스캔라인 표시",
    "DISPLAY VERBOSE LEVEL UP MESSAGES FOR COMPANIONS": "동료의 상세한 레벨업 메시지 표시",
    "DISPLAY VIGNETTE EFFECT": "비네트 효과 표시",
    "DO NOT GENERATE FLOOR TEXTURE OBJECTS": "바닥 텍스처 객체 생성 안 함",
    "DOCK BACKGROUND OPACITY": "도크 배경 투명도",
    "DOCK MESSAGE LOG && MINIMAP": "메시지 로그 및 미니맵 도킹",
    "DON'T DELETE CLASSIC MODE SAVED GAMES ON DEATH": "클래식 모드 저장 게임을 죽음 시 삭제 안 함",
    "DRAW AUDIBILITY FLOOD FILL": "청음 플러드 필 그리기",
    "DRAW CELLULAR AUTOMATA WHEN USED IN MAP GENERATION": "지도 생성에 사용될 때 셀룰러 오토마타 그리기",
    "DRAW CREATURE PATHFINDING": "생물 경로 찾기 그리기",
    "DRAW ELECTRICAL ARC HISTORY FOR LAST FEW ROUNDS": "최근 몇 라운드의 전기 호 역사 그리기",
    "DRAW NAVIGATION WEIGHT MAPS ON EACH STEP": "각 단계에서 네비게이션 가중치 맵 그리기",
    "DRAW OLFACTION FLOOD FILL": "후각 플러드 필 그리기",
    "DRAW POPULATION PLACEMENT HINT MAPS WHEN BUILDING ZONES": "구역 건설 시 인구 배치 힌트 맵 그리기",
    "DRAW POPULATION PLACEMENT REGIONS WHEN BUILDING ZONES": "구역 건설 시 인구 배치 영역 그리기",
    "DRAW REACHABILITY MAPS ON ZONE GENERATION": "구역 생성 시 도달성 맵 그리기",
    "DRAW SEMANTIC TAG INFO WHEN BUILDING ZONES": "구역 건설 시 의미론적 태그 정보 그리기",
    "DRAW VISIBILITY FLOOD FILL": "시야 플러드 필 그리기",
    "ENABLE A 'DROP ALL' INTERACTION ON ITEMS IN YOUR INVENTORY": "인벤토리의 아이템에서 '모두 떨어뜨리기' 상호작용 활성화",
    "ENABLE HARMONY DEBUG OUTPUT": "Harmony 디버그 출력 활성화",
    "ENABLE MODERN UI CHARACTER SHEET": "최신 UI 캐릭터 시트 활성화",
    "ENABLE MODERN UI": "최신 UI 활성화",
    "ENABLE MODS": "모드 활성화",
    "ENABLE SAVE AND LOAD": "저장 및 불러오기 활성화",
    "ENABLE TILE GRAPHICS": "타일 그래픽 활성화",
    "ENABLE UNFINISHED AND UNSUPPORTED CONTENT": "미완성 및 지원되지 않는 콘텐츠 활성화",
    "ENABLE WORKAROUND FOR STEAM DECK SHIFT KEY BUG": "Steam Deck Shift 키 버그 해결방법 활성화",
    "EQUIP/UNEQUIP HIGHLIGHTED ITEM WHEN PRESSING RIGHT OR LEFT, RESPECTIVELY, ON THE EQUIPMENT SCREEN": "장비 화면에서 오른쪽 또는 왼쪽 버튼을 누르면 강조된 아이템 장착/해제",
    "EXPAND THE EQUIPMENT OR INVENTORY PANE WHEN FOCUSED": "포커스될 때 장비 또는 인벤토리 창 확장",
    "FIRE CRACKLING SOUNDS": "불 타닥거리는 소리",
    "FLUSH ZONES TO CACHE EARLY (FOR LOW MEMORY ENVIRONMENTS)": "구역을 캐시에 조기 플러시 (저메모리 환경용)",
    "FRAME RATE": "프레임 레이트",
    "FULLSCREEN RESOLUTION": "전체 화면 해상도",
    "FULLSCREEN": "전체 화면",
    "GAMEPAD BUTTON APPEARANCE": "게임패드 버튼 외형",
    "GARBAGE COLLECT AFTER EACH ZONE FLUSH (FOR LOW MEMORY ENVIRONMENTS)": "각 구역 플러시 후 가비지 수집 (저메모리 환경용)",
    "GET PROMPTED TO CONFIRM DEATHS": "죽음 확인 메시지 수신",
    "INDENT BODY PARTS BY ATTACHMENT ON THE EQUIPMENT SCREEN": "장비 화면에서 신체 부위를 부착 상태에 따라 들여쓰기",
    "INDICATE BIOMES ON THE WORLDMAP": "월드맵에 생물군계 표시",
    "INDICATE SPECIAL ENCOUNTERS ON THE WORLDMAP": "월드맵에 특수 만남 표시",
    "INTERFACE SOUNDS": "인터페이스 소리",
    "INTERFACE VOLUME": "인터페이스 음량",
    "INTERRUPT HELD MOVEMENT WHEN ENTERING AN UNEXPLORED ZONE": "미탐색 구역 진입 시 지속 이동 중단",
    "INTERRUPT HELD MOVEMENT WHEN HOSTILES ARE NEARBY": "적이 근처에 있을 때 지속 이동 중단",
    "KEEP CAVES OF QUD ACTIVE IN BACKGROUND": "Caves of Qud를 백그라운드에서 활성 상태로 유지",
    "KEY REPEAT DELAY": "키 반복 지연",
    "KEY REPEAT RATE": "키 반복 속도",
    "LIMIT THE INPUT BUFFER TO TWO COMMANDS": "입력 버퍼를 두 명령으로 제한",
    "MAIN MENU BACKGROUND ART": "메인 메뉴 배경 미술",
    "MAIN VOLUME": "주 음량",
    "MAP SHIFT+DIRECTIONS TO MENU PAGINATION BINDS (DOESN'T WORK FOR NUMPAD BINDINGS)": "Shift+방향을 메뉴 페이지 네이션 바인드로 매핑 (숫자패드 바인드에는 작동 안 함)",
    "MAXIMUM AUTOMOVE CELLS/SEC AND AUTOATTACK ACTIONS/SEC": "최대 자동 이동 칸/초 및 자동 공격 동작/초",
    "MESSAGE LOG FONT SIZE (PT)": "메시지 로그 글꼴 크기 (포인트)",
    "MOUSE CURSOR APPEARANCE": "마우스 커서 외형",
    "MOVE TUTORIAL TO THE END OF THE GAME MODE LIST": "튜토리얼을 게임 모드 목록 끝으로 이동",
    "MUSIC VOLUME": "음악 음량",
    "MUSIC": "음악",
    "NAVIGATE INVENTORY AND EQUIPMENT PANES WITH LEFT/RIGHT PAGINATION INSTEAD OF MOVEMENT BINDS": "이동 바인드 대신 왼쪽/오른쪽 페이지 네이션으로 인벤토리 및 장비 창 탐색",
    "NEARBY OBJECTS LIST:   SHOW LIQUID POOLS": "근처 객체 목록: 액체 웅덩이 표시",
    "NEARBY OBJECTS LIST:  SHOW ONLY CURRENT CELL'S CONTENTS": "근처 객체 목록: 현재 칸의 내용만 표시",
    "NEARBY OBJECTS LIST: SHOW ONLY SHOW TAKEABLE OBJECTS": "근처 객체 목록:  획득 가능한 객체만 표시",
    "NEARBY OBJECTS LIST: SHOW PLANTS": "근처 객체 목록: 식물 표시",
    "NUMBER OF ROLLING BACKUP SAVES": "롤링 백업 저장의 수",
    "PERFORM INVENTORY CONSISTENCY CHECKS": "인벤토리 일관성 검사 수행",
    "PIXEL PERFECT TILE GRAPHIC SCALE": "픽셀 퍼펙트 타일 그래픽 스케일",
    "PLAY AREA SCALE": "재생 영역 스케일",
    "PREGENERATE ZONE NAMES FOR WISH SUPPORT": "소원 지원을 위해 구역 이름 사전 생성",
    "PRESSING SHIFT HIDES THE SIDEBAR": "Shift를 누르면 사이드바 숨김",
    "PROMPT BEFORE AUTOWALKING TO STAIRS": "계단으로 자동 이동하기 전에 확인",
    "PROMPT BEFORE DRINKING AND MOVING INTO CERTAIN DANGEROUS LIQUIDS": "특정 위험한 액체에 진입하기 전에 마시기 전에 확인",
    "PROMPT BEFORE MOVING TO THE WORLD MAP": "월드맵으로 이동하기 전에 확인",
    "PROMPT BEFORE SWIMMING": "수영하기 전에 확인",
    "PROMPT LIST OF ITEMS WHEN GETTING FROM THE GROUND, EVEN IF THERE'S ONLY ONE": "하나만 있어도 땅에서 획득할 때 아이템 목록 확인",
    "RANGE THRESHOLD FOR IGNORING HOSTILE CREATURES": "적대적 생물을 무시할 범위 기준",
    "SEARCH CONTAINERS WHILE AUTOEXPLORING": "자동 탐색 중 용기 검색",
    "SEND ANONYMOUS GAMEPLAY STATISTICS": "익명 게임플레이 통계 전송",
    "SHOW ABILITY-ON-COOLDOWN PROMPTS AS MESSAGE LOG ENTRIES INSTEAD OF POPUPS": "쿨다운 중 능력 확인을 팝업 대신 메시지 로그 항목으로 표시",
    "SHOW ADVANCED OPTIONS": "고급 설정 표시",
    "SHOW ATTITUDE INFO ON OBJECTS": "객체에 태도 정보 표시",
    "SHOW COLLECTOR'S SEALS ON THE MAIN MENU": "메인 메뉴에 수집가의 인장 표시",
    "SHOW DAMAGE PENETRATION DEBUG TEXT": "피해 관통력 디버그 텍스트 표시",
    "SHOW DEBUG INFO ON OBJECTS": "객체에 디버그 정보 표시",
    "SHOW DEBUG TEXT FOR STAT SHIFTS": "스탯 변화에 대한 디버그 텍스트 표시",
    "SHOW ENCOUNTER CHANCE DEBUG CHANCE": "만남 확률 디버그 표시",
    "SHOW ERROR POPUPS": "오류 팝업 표시",
    "SHOW LOST CHANCE DEBUG TEXT": "손실 확률 디버그 텍스트 표시",
    "SHOW MINIMAP": "미니맵 표시",
    "SHOW NEARBY OBJECTS LIST": "근처 객체 목록 표시",
    "SHOW NUMBER OF ITEMS IN EACH INVENTORY CATEGORY": "각 인벤토리 카테고리의 아이템 수 표시",
    "SHOW POPUPS WHEN YOU DISMEMBER OR DECAPITATE SOMEONE": "누군가를 절단하거나 목을 베었을 때 팝업 표시",
    "SHOW QUICKSTART OPTION DURING CHARACTER CREATION": "캐릭터 생성 중 빠른 시작 옵션 표시",
    "SHOW REPUTATION WITH A CREATURE'S FACTIONS WHEN LOOKING AT THEM": "생물을 볼 때 그의 진영과의 평판 표시",
    "SHOW SAVING THROW DEBUG TEXT": "내성 판정 디버그 텍스트 표시",
    "SHOW SCAVENGED ITEMS AS MESSAGE LOG ENTRIES INSTEAD OF POPUPS": "수집한 아이템을 팝업 대신 메시지 로그 항목으로 표시",
    "SHOW THE FULL ZONE ID DURING ZONE CREATION": "구역 생성 중 전체 구역 ID 표시",
    "SHOW TRAVEL SPEED DEBUG TEXT": "이동 속도 디버그 텍스트 표시",
    "SHOW XML CONVERSATION AND NODE NAMES DURING CONVERSATION": "대화 중 XML 대화 및 노드 이름 표시",
    "SOUND EFFECTS VOLUME": "효과음 음량",
    "SOUND EFFECTS": "효과음",
    "TAKE CORPSES WHEN USING THE 'TAKE ALL' COMMAND": "시신 가져오기 ('모두 가져오기' 명령 사용 시)",
    "THIRST THRESHOLD FOR AUTOMATIC DRINKING": "자동 음용의 갈증 기준",
    "THRESHOLD FOR LOW HITPOINT WARNING": "낮은 체력 경고 기준",
    "THROTTLE ANIMATIONS (FOR LOW CPU ENVIRONMENTS)": "애니메이션 제한 (저CPU 환경용)",
    "TOOLTIP DELAY (MS)": "도구 설명 지연 (ms)",
    "UI SCALE": "UI 스케일",
    "USE TEXT AUTOACT INTERRUPT INDICATOR INSTEAD OF FLASHING RED BOX": "빨간 상자 깜빡임 대신 텍스트 자동 행동 중단 표시기 사용",
    "WAIT FOR KEYPRESS WHEN DRAWING PATHFINDING": "경로 그리기 시 키 입력 대기",
    "WHEN ZOOMED OUT TO MAX ZOOM, REMOVE THE UI FRAME FROM THE MAIN GAMEPLAY SCREEN": "최대 축소 시 메인 게임플레이 화면에서 UI 프레임 제거",
    "WRITE COMPILED MOD ASSEMBLIES TO DISK": "컴파일된 모드 어셈블리를 디스크에 작성",
    "WRITE PLAYED SOUND FILE NAMES TO MESSAGE LOG": "재생된 사운드 파일 이름을 메시지 로그에 작성",
    "ZOOM SENSITIVITY": "줌 감도",
    "Ability bar mode": "능력 바 모드",
    "Add a line separator at the end of each round during combat": "전투의 각 라운드 끝에 줄 구분자 추가",
    "Allow creatures to have multiple defects": "생물이 다중 결함을 가질 수 있도록 허용",
    "Allow mouse input": "마우스 입력 허용",
    "Allow mouse movement": "마우스 이동 허용",
    "Allow mutations to recolor your character's glyph": "돌연변이가 캐릭터 글리프를 다시 칠하도록 허용",
    "Allow scripting mods (scripting mods may contain malicious code! )": "스크립팅 모드 허용 (악성 코드 포함 가능)",
    "Allow scroll wheel to zoom": "스크롤 휠로 확대/축소 허용",
    "Always highlight stairs": "항상 계단 강조",
    "Always map directions to numpad": "항상 방향을 숫자패드로 매핑",
    "Always pass A-Z hotkeys through to the legacy UI": "A-Z 단축키를 레거시 UI로 항상 전달",
    "Ambient sounds": "환경음",
    "Ambient volume": "환경음 음량",
    "Attack ignored hostiles that you move adjacent to during autoexplore": "자동 탐색 중 인접으로 이동한 무시된 적 공격",
    "Autoget ammo": "탄약 자동 획득",
    "Autoget artifacts": "유물 자동 획득",
    "Autoget books": "서적 자동 획득",
    "Autoget copper, silver, and gold nuggets": "구리·은·금 덩어리 자동 획득",
    "Autoget food": "음식 자동 획득",
    "Autoget fresh water": "신선한 물 자동 획득",
    "Autoget from adjacent cells": "인접 칸에서 자동으로 획득",
    "Autoget if hostiles are nearby": "주변에 적이 있으면 자동으로 획득",
    "Autoget liquids from containers you've dropped": "내가 떨어뜨린 용기에서 액체 자동 획득",
    "Autoget primitive ammo": "원시 탄약 자동 획득",
    "Autoget scrap": "스크랩 자동 획득",
    "Autoget special items": "특수 아이템 자동 획득",
    "Autoget trade goods": "무역품 자동 획득",
    "Autoget weightless items": "무게 없는 아이템 자동 획득",
    "Automatically dig through walls by moving into them when wielding a digging implement": "굴착 도구를 들고 벽으로 이동하면 자동으로 벽을 파고 통과",
    "Automatically disassemble scrap": "스크랩 자동 분해",
    "Automatically douse and light torches": "횃불 자동 소등/점화",
    "Automatically drink fresh water when thirsty": "목마를 때 신선한 물 자동 음용",
    "Automatically hide mouse cursor when inactive": "비활성 시 마우스 커서 자동 숨김",
    "Automatically save when moving to a different zone": "다른 구역으로 이동할 때 자동 저장",
    "Average": "평균",
    "Character sheet scale percentage": "캐릭터 시트 스케일(%)",
    "Character sheet size": "캐릭터 시트 크기",
    "Check memory usage and show a warning when it's high": "메모리 사용량을 확인하고 높을 때 경고 표시",
    "Classic": "클래식",
    "Color hit hearts per the target's remaining HP": "대상의 남은 HP에 따라 히트 하트 색상 지정",
    "Color your character's glyph based on health status": "체력 상태에 따라 캐릭터 글리프 색상 지정",
    "Combat sounds": "전투 소리",
    "Combat volume": "전투 음량",
    "Compact": "컴팩트",
    "Contrast": "대비",
    "Control Mapping": "키 설정",
    "Cover": "커버(Cover)",
    "Default the selection on death prompts in Roleplay mode to 'View final messages' instead of 'Reload checkpoint'": "롤플레이 모드에서 죽음 확인창의 기본 선택을 '체크포인트 다시 불러오기' 대신 '최종 메시지 보기'로 설정",
    "Dehydrated": "탈수됨",
    "Difficulty threshold for ignoring hostile creatures": "적대적 생물을 무시할 난이도 기준",
    "Disable achievements": "업적 비활성화",
    "Disable blood splatters": "혈흔 비활성화",
    "Disable brain hijacking on player-controlled characters": "플레이어 조종 캐릭터의 뇌 탈취 비활성화",
    "Disable conflict checking when rebinding controls": "조작 재설정 시 충돌 확인 비활성화",
    "Disable floor textures": "바닥 텍스처 비활성화",
    "Disable fullscreen color effects (for color blindness)": "전체 화면 색상 효과 비활성화 (색맹용)",
    "Disable fullscreen screen-warping effects": "전체 화면 화면 왜곡 효과 비활성화",
    "Disable input warning": "입력 경고 비활성화",
    "Disable limit on zone build tries": "구역 빌드 시도 제한 비활성화",
    "Disable modern, tile-based graphical effects": "최신 타일 기반 그래픽 효과 비활성화",
    "Disable most tile-based flashing effects": "대부분의 타일 기반 깜빡임 효과 비활성화",
    "Disable preloading of sounds": "사운드 사전 로딩 비활성화",
    "Disable smoke effects": "연기 효과 비활성화",
    "Disable tile-based screen-warping effects": "타일 기반 화면 왜곡 효과 비활성화",
    "Disable zone caching": "구역 캐싱 비활성화",
    "Display ASCII visual effects": "ASCII 시각 효과 표시",
    "Display ability icons on the sidebar": "사이드바에 능력 아이콘 표시",
    "Display bits with alphanumerics instead of dots (for color blindness)": "점 대신 문자와 숫자로 비트 표시 (색맹용)",
    "Display combat animations": "전투 애니메이션 표시",
    "Display contents of current cell in a popup": "현재 칸의 내용을 팝업으로 표시",
    "Display detailed weapon penetration and damage in weapon names": "무기 이름에 상세한 관통력과 피해량 표시",
    "Display floating damage numbers": "부동 피해량 숫자 표시",
    "Display location instead of name on the sidebar": "사이드바에 이름 대신 위치 표시",
    "Display modern visual effects": "최신 시각 효과 표시",
    "Display mouse-clickable zone transition arrows": "마우스 클릭 가능한 구역 전환 화살표 표시",
    "Display overlay keyboard for input on supported devices": "지원되는 기기에서 입력용 오버레이 키보드 표시",
    "Display popups when noting information in your journal": "저널에 정보를 기록할 때 팝업 표시",
    "Display scanlines": "스캔라인 표시",
    "Display verbose level up messages for companions": "동료의 상세한 레벨업 메시지 표시",
    "Display vignette effect": "비네트 효과 표시",
    "Do not generate floor texture objects": "바닥 텍스처 객체 생성 안 함",
    "Dock background opacity": "도크 배경 투명도",
    "Dock message log && minimap": "메시지 로그 및 미니맵 도킹",
    "Don't delete Classic mode saved games on death": "클래식 모드 저장 게임을 죽음 시 삭제 안 함",
    "Draw audibility flood fill": "청음 플러드 필 그리기",
    "Draw cellular automata when used in map generation": "지도 생성에 사용될 때 셀룰러 오토마타 그리기",
    "Draw creature pathfinding": "생물 경로 찾기 그리기",
    "Draw electrical arc history for last few rounds": "최근 몇 라운드의 전기 호 역사 그리기",
    "Draw navigation weight maps on each step": "각 단계에서 네비게이션 가중치 맵 그리기",
    "Draw olfaction flood fill": "후각 플러드 필 그리기",
    "Draw population placement hint maps when building zones": "구역 건설 시 인구 배치 힌트 맵 그리기",
    "Draw population placement regions when building zones": "구역 건설 시 인구 배치 영역 그리기",
    "Draw reachability maps on zone generation": "구역 생성 시 도달성 맵 그리기",
    "Draw semantic tag info when building zones": "구역 건설 시 의미론적 태그 정보 그리기",
    "Draw visibility flood fill": "시야 플러드 필 그리기",
    "Easy": "쉬움",
    "Enable Harmony debug output": "Harmony 디버그 출력 활성화",
    "Enable a 'drop all' interaction on items in your inventory": "인벤토리의 아이템에서 '모두 떨어뜨리기' 상호작용 활성화",
    "Enable color menu for text input": "텍스트 입력을 위해 색상 메뉴 활성화",
    "Enable modern UI": "최신 UI 활성화",
    "Enable modern UI character sheet": "최신 UI 캐릭터 시트 활성화",
    "Enable mods": "모드 활성화",
    "Enable save and load": "저장 및 불러오기 활성화",
    "Enable tile graphics": "타일 그래픽 활성화",
    "Enable unfinished and unsupported content": "미완성 및 지원되지 않는 콘텐츠 활성화",
    "Enable workaround for Steam Deck shift key bug": "Steam Deck Shift 키 버그 해결방법 활성화",
    "Equip/unequip highlighted item when pressing right or left, respectively, on the Equipment screen": "장비 화면에서 오른쪽 또는 왼쪽 버튼을 누르면 강조된 아이템 장착/해제",
    "Expand the equipment or inventory pane when focused": "포커스될 때 장비 또는 인벤토리 창 확장",
    "Fire crackling sounds": "불 타닥거리는 소리",
    "Fit": "맞춤(Fit)",
    "Flip": "반전",
    "Flush zones to cache early (for low memory environments)": "구역을 캐시에 조기 플러시 (저메모리 환경용)",
    "Frame rate": "프레임 레이트",
    "Full": "전체",
    "Full Height": "전체 높이",
    "Fullscreen": "전체 화면",
    "Fullscreen resolution": "전체 화면 해상도",
    "Gamepad button appearance": "게임패드 버튼 외형",
    "Garbage collect after each zone flush (for low memory environments)": "각 구역 플러시 후 가비지 수집 (저메모리 환경용)",
    "Get prompted to confirm deaths": "죽음 확인 메시지 수신",
    "Ignore all effects that recolor your character's glyph other than health status": "체력 상태 외의 캐릭터 글리프를 다시 칠하는 모든 효과 무시",
    "Impossible": "불가능",
    "Indent body parts by attachment on the Equipment screen": "장비 화면에서 신체 부위를 부착 상태에 따라 들여쓰기",
    "Indicate biomes on the worldmap": "월드맵에 생물군계 표시",
    "Indicate special encounters on the worldmap": "월드맵에 특수 만남 표시",
    "Interface sounds": "인터페이스 소리",
    "Interface volume": "인터페이스 음량",
    "Interrupt held movement when entering an unexplored zone": "미탐색 구역 진입 시 지속 이동 중단",
    "Interrupt held movement when hostiles are nearby": "적이 근처에 있을 때 지속 이동 중단",
    "KBM": "키보드+마우스",
    "Keep Caves of Qud active in background": "Caves of Qud를 백그라운드에서 활성 상태로 유지",
    "Key repeat delay": "키 반복 지연",
    "Key repeat rate": "키 반복 속도",
    "Limit the input buffer to two commands": "입력 버퍼를 두 명령으로 제한",
    "Left": "왼쪽",
    "Main menu background art": "메인 메뉴 배경 미술",
    "Main volume": "주 음량",
    "Map Shift+directions to menu pagination binds (doesn't work for numpad bindings)": "Shift+방향을 메뉴 페이지 네이션 바인드로 매핑 (숫자패드 바인드에는 작동 안 함)",
    "Maximum automove cells/sec and autoattack actions/sec": "최대 자동 이동 칸/초 및 자동 공격 동작/초",
    "Message log font size (pt)": "메시지 로그 글꼴 크기 (포인트)",
    "Modern": "모던",
    "Mouse cursor appearance": "마우스 커서 외형",
    "Move tutorial to the end of the game mode list": "튜토리얼을 게임 모드 목록 끝으로 이동",
    "Music": "음악",
    "Music volume": "음악 음량",
    "Navigate inventory and equipment panes with left/right pagination instead of movement binds": "이동 바인드 대신 왼쪽/오른쪽 페이지 네이션으로 인벤토리 및 장비 창 탐색",
    "Nearby objects list: show only current cell's contents": "근처 객체 목록: 현재 칸의 내용만 표시",
    "Nearby objects list: show only show takeable objects": "근처 객체 목록:  획득 가능한 객체만 표시",
    "Nearby objects list: show plants": "근처 객체 목록: 식물 표시",
    "No Warning": "경고 없음",
    "None": "없음",
    "Number of rolling backup saves": "롤링 백업 저장의 수",
    "Perform inventory consistency checks": "인벤토리 일관성 검사 수행",
    "Pixel Perfect": "픽셀 퍼펙트",
    "Pixel perfect tile graphic scale": "픽셀 퍼펙트 타일 그래픽 스케일",
    "Play area scale": "재생 영역 스케일",
    "Pregenerate zone names for wish support": "소원 지원을 위해 구역 이름 사전 생성",
    "Pressing shift hides the sidebar": "Shift를 누르면 사이드바 숨김",
    "Prompt before autowalking to stairs": "계단으로 자동 이동하기 전에 확인",
    "Prompt before drinking and moving into certain dangerous liquids": "특정 위험한 액체에 진입하기 전에 마시기 전에 확인",
    "Prompt before moving to the world map": "월드맵으로 이동하기 전에 확인",
    "Prompt before swimming": "수영하기 전에 확인",
    "Prompt list of items when getting from the ground, even if there's only one": "하나만 있어도 땅에서 획득할 때 아이템 목록 확인",
    "PS": "PS",
    "PS Filled": "PS (채움)",
    "Parched": "건조함",
    "Quenched": "충분히 마심",
    "Range threshold for ignoring hostile creatures": "적대적 생물을 무시할 범위 기준",
    "Right": "오른쪽",
    "Search containers while autoexploring": "자동 탐색 중 용기 검색",
    "Show XML conversation and node names during conversation": "대화 중 XML 대화 및 노드 이름 표시",
    "Show ability-on-cooldown prompts as message log entries instead of popups": "쿨다운 중 능력 확인을 팝업 대신 메시지 로그 항목으로 표시",
    "Show advanced options": "고급 설정 표시",
    "Show attitude info on objects": "객체에 태도 정보 표시",
    "Show collector's seals on the main menu": "메인 메뉴에 수집가의 인장 표시",
    "Show damage penetration debug text": "피해 관통력 디버그 텍스트 표시",
    "Show debug info on objects": "객체에 디버그 정보 표시",
    "Show debug text for stat shifts": "스탯 변화에 대한 디버그 텍스트 표시",
    "Show encounter chance debug chance": "만남 확률 디버그 표시",
    "Show error popups": "오류 팝업 표시",
    "Show lost chance debug text": "손실 확률 디버그 텍스트 표시",
    "Show minimap": "미니맵 표시",
    "Show nearby objects list": "근처 객체 목록 표시",
    "Show number of items in each inventory category": "각 인벤토리 카테고리의 아이템 수 표시",
    "Show popups when you dismember or decapitate someone": "누군가를 절단하거나 목을 베었을 때 팝업 표시",
    "Show quickstart option during character creation": "캐릭터 생성 중 빠른 시작 옵션 표시",
    "Show reputation with a creature's factions when looking at them": "생물을 볼 때 그의 진영과의 평판 표시",
    "Show saving throw debug text": "내성 판정 디버그 텍스트 표시",
    "Show scavenged items as message log entries instead of popups": "수집한 아이템을 팝업 대신 메시지 로그 항목으로 표시",
    "Show the full zone ID during zone creation": "구역 생성 중 전체 구역 ID 표시",
    "Show travel speed debug text": "이동 속도 디버그 텍스트 표시",
    "Sound effects": "효과음",
    "Sound effects volume": "효과음 음량",
    "Standard": "표준",
    "Strip color formatting from UI text": "UI 텍스트에서 색상 형식 제거",
    "System": "시스템",
    "Take corpses when using the 'take all' command": "시신 가져오기 ('모두 가져오기' 명령 사용 시)",
    "Thirst threshold for automatic drinking": "자동 음용의 갈증 기준",
    "Thirsty": "목마름",
    "Threshold for low hitpoint warning": "낮은 체력 경고 기준",
    "Throttle animations (for low CPU environments)": "애니메이션 제한 (저CPU 환경용)",
    "Tooltip delay (ms)": "도구 설명 지연 (ms)",
    "Tough": "터프",
    "Tumescent": "배부름",
    "UI scale": "UI 스케일",
    "Unlimited": "무제한",
    "Unset": "미설정",
    "Use text autoact interrupt indicator instead of flashing red box": "빨간 상자 깜빡임 대신 텍스트 자동 행동 중단 표시기 사용",
    "VSync": "수직동기화(VSync)",
    "Very Tough": "매우 터프",
    "Wait for keypress when drawing pathfinding": "경로 그리기 시 키 입력 대기",
    "When zoomed out to max zoom, remove the UI frame from the main gameplay screen": "최대 축소 시 메인 게임플레이 화면에서 UI 프레임 제거",
    "Write compiled mod assemblies to disk": "컴파일된 모드 어셈블리를 디스크에 작성",
    "Write played sound file names to message log": "재생된 사운드 파일 이름을 메시지 로그에 작성",
    "XBox": "Xbox",
    "XBox Filled": "Xbox (채움)",
    "Zoom sensitivity": "줌 감도",
    "auto": "자동",
    "auto x1.25": "자동 x1.25",
    "auto x1.5": "자동 x1.5",
    "Sound": "사운드",
    "SOUND": "사운드",
    "Display": "디스플레이",
    "DISPLAY": "디스플레이",
    "Controls": "조작",
    "CONTROLS": "조작",
    "Accessibility": "접근성",
    "ACCESSIBILITY": "접근성",
    "UI": "UI",
    "Legacy UI": "레거시 UI",
    "Automation": "자동화",
    "AUTOMATION": "자동화",
    "Autoget": "자동 습득",
    "Prompts": "알림",
    "Mods": "모드",
    "MODS": "모드",
    "Performance": "성능",
    "App Settings": "앱 설정",
    "Debug": "디버그",
    "DEBUG": "디버그",
    "Control Mapping": "키 설정",
    "Show advanced options": "고급 설정 보기",
    "Apply": "적용",
    "Back": "뒤로",
    "Default": "기본값",
    "Cancel": "취소",
    "Reset to Defaults": "초기화",
    "Controls how the play area will scale in the area not used by the UI.\n\n        {{W|Fit}}: Fits the whole play area on screen. May necessitate letterboxing.\n        {{W|Cover}}: Ensures the play area covers the screen. Minimizes letterboxing.\n        {{W|Pixel Perfect}}: Sizes the play area to an integer multiple of the pixel art. Maximizes sharpness.": "재생 영역의 스케일 방식을 설정합니다.\\n\\n{{W|Fit}}: 전체 재생 영역이 화면에 맞도록 축소합니다(레터박스가 생길 수 있음).\\n{{W|Cover}}: 재생 영역이 화면을 완전히 덮도록 조정합니다(레터박스 최소화).\\n{{W|Pixel Perfect}}: 픽셀 아트의 정수 배수로 크기를 조정해 선명도를 최적화합니다.",
    "When you load a save from the main menu rolling backups of loaded saves will be maintained in your data folder (<...>/CavesOfQud/Local/Session) up to this number.": "메인 메뉴에서 저장을 불러올 때, 이 숫자만큼 최근 세션의 롤링 백업을 로컬 폴더(<...>/CavesOfQud/Local/Session)에 보관합니다."
}

def strip_tags(s):
    # Expanded tag stripping
    s = re.sub(r'(<[^>]+>|\{\{[^}]+\}\})', '', s)
    return s.strip()

def get_keys(path):
    tree = ET.parse(path)
    root = tree.getroot()
    keys = set()
    for opt in root.findall('.//option'):
        dt = opt.get('DisplayText')
        if dt:
            keys.add(dt)
            keys.add(dt.upper())
            stripped = strip_tags(dt)
            keys.add(stripped)
            keys.add(stripped.upper())
            
        help_el = opt.find('helptext')
        if help_el is not None and help_el.text:
            keys.add(help_el.text.strip())
            
        for attr in ['DisplayValues', 'Values']:
            val = opt.get(attr)
            if val and not val.startswith('*'): 
                parts = re.split(r'[,\|]', val)
                for p in parts:
                    if p.strip():
                        keys.add(p.strip())
    return keys

xml_path = "Assets/StreamingAssets/Base/Options.xml"
all_keys = get_keys(xml_path)

final_dict = {}
for k in all_keys:
    # Use core translation if available
    if k in raw_core:
        final_dict[k] = raw_core[k]
    else:
        # Fallback to smart matching (case-insensitive)
        lower_k = k.lower()
        found = ""
        for ck, cv in raw_core.items():
            if ck.lower() == lower_k and cv:
                found = cv
                break
        final_dict[k] = found

header = """/*
 * 파일명: OptionsData.cs
 * 분류: [Data] 설정 화면 텍스트
 * 역할: 게임 설정(Options) 화면의 카테고리 및 설정 항목 텍스트를 정의합니다.
 */

using System.Collections.Generic;

namespace QudKRTranslation.Data
{
    public static class OptionsData
    {
        // 실제 번역 데이터 (컬러 코드 제거)
        private static Dictionary<string, string> baseTranslations = new Dictionary<string, string>()
        {
"""

footer = """        };

        // 동적으로 생성된 공개 Dictionary (기존 코드와 호환)
        public static Dictionary<string, string> Translations { get; private set; }

        static OptionsData()
        {
            Translations = new Dictionary<string, string>();

            // 원본 데이터와 컬러 코드 버전 모두 등록
            foreach (var kvp in baseTranslations)
            {
                // 원본 추가
                Translations[kvp.Key] = kvp.Value;

                // 컬러 코드 버전 추가 (헤더 등에서 사용됨)
                if (!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
                {
                    string coloredKey = $"<color=#77BFCFFF>{kvp.Key}</color>";
                    string coloredValue = $"<color=#77BFCFFF>{kvp.Value}</color>";
                    Translations[coloredKey] = coloredValue;
                }
            }
        }
    }
}
"""

print(header, end='')
sorted_keys = sorted(final_dict.keys())
for i, k in enumerate(sorted_keys):
    val = final_dict[k]
    k_esc = k.replace('"', '\\"').replace('\n', '\\n')
    v_esc = val.replace('"', '\\"').replace('\n', '\\n')
    comma = "," if i < len(sorted_keys) - 1 else ""
    print(f'            {{ "{k_esc}", "{v_esc}" }}{comma}')
print(footer, end='')
