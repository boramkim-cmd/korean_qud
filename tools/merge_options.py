#!/usr/bin/env python3
import sys
import re
import os
import xml.etree.ElementTree as ET

# 1. Core translations provided by the user
core_translations = {
    "Sound": "사운드",
    "SOUND": "사운드",
    "Display": "디스플레이",
    "DISPLAY": "디스플레이",
    "Controls": "조작",
    "CONTROLS": "조작",
    "Accessibility": "접근성",
    "ACCESSIBILITY": "접근성",
    "UI": "UI",
    "Legacy UI": "레거시 UI" ,
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
    "Main volume": "주 볼륨",
    "MAIN VOLUME": "주 볼륨",
    "Sound effects": "효과음",
    "SOUND EFFECTS": "효과음",
    "Sound effects volume": "효과음 음량",
    "Music": "음악",
    "Music volume": "음악 음량",
    "Ambient sounds": "환경음",
    "Ambient volume": "환경음 음량",
    "Interface sounds": "인터페이스 소리",
    "Interface volume": "인터페이스 음량",
    "Combat sounds": "전투 소리",
    "Combat volume": "전투 음량",
    "Fire crackling sounds": "불 타닥거리는 소리",
    "Brightness": "밝기",
    "Contrast": "대비",
    "Fullscreen": "전체 화면",
    "Fullscreen resolution": "전체 화면 해상도",
    "Frame rate": "프레임 레이트",
    "Display vignette effect": "비네트 효과 표시",
    "Display scanlines": "스캔라인 표시",
    "Control Mapping": "키 설정",
    "Key repeat delay": "키 반복 지연",
    "Key repeat rate": "키 반복 속도",
    "Show advanced options": "고급 설정 보기",
    "Apply": "적용",
    "Back": "뒤로",
    "Default": "기본값",
    "Cancel": "취소",
    "Reset to Defaults": "초기화",
    "Modern": "모던",
    "Classic": "클래식",
    "Full": "전체",
    "Compact": "컴팩트",
    "Fit": "맞춤(Fit)",
    "Cover": "커버(Cover)",
    "Pixel Perfect": "픽셀 퍼펙트",
    "Unset": "미설정",
    "Unlimited": "무제한",
    "VSync": "수직동기화(VSync)",
    "Auto": "자동",
    "Left": "왼쪽",
    "Right": "오른쪽",
    "System": "시스템",
    "Alternate": "대체",
    "KBM": "키보드+마우스",
    "XBox": "Xbox",
    "PS": "PS",
    "PS Filled": "PS (채움)",
    "XBox Filled": "Xbox (채움)",
    "Show minimap": "미니맵 표시",
    "Show nearby objects list": "근처 객체 목록 표시",
    "Enable mods": "모드 사용",
    "Send anonymous gameplay statistics": "익명 게임 통계 전송",
    "No Warning": "경고 없음",
    "Controls how the play area will scale in the area not used by the UI.\n\n        {{W|Fit}}: Fits the whole play area on screen. May necessitate letterboxing.\n        {{W|Cover}}: Ensures the play area covers the screen. Minimizes letterboxing.\n        {{W|Pixel Perfect}}: Sizes the play area to an integer multiple of the pixel art. Maximizes sharpness.": "재생 영역의 스케일 방식을 설정합니다.\n\n{{W|Fit}}: 전체 재생 영역이 화면에 맞도록 축소합니다(레터박스가 생길 수 있음).\n{{W|Cover}}: 재생 영역이 화면을 완전히 덮도록 조정합니다(레터박스 최소화).\n{{W|Pixel Perfect}}: 픽셀 아트의 정수 배수로 크기를 조정해 선명도를 최적화합니다.",
    "When you load a save from the main menu rolling backups of loaded saves will be maintained in your data folder (<...>/CavesOfQud/Local/Session) up to this number.": "메인 메뉴에서 저장을 불러올 때, 이 숫자만큼 최근 세션의 롤링 백업을 로컬 폴더(<...>/CavesOfQud/Local/Session)에 보관합니다."
}

def strip_tags(s):
    return re.sub(r'(<[^>]+>|\{\{[^}]+\}\})', '', s).strip()

def get_keys(path):
    tree = ET.parse(path)
    root = tree.getroot()
    keys = set()
    for opt in root.findall('.//option'):
        dt = opt.get('DisplayText')
        if dt:
            keys.add(dt)
            keys.add(dt.upper())
            keys.add(strip_tags(dt))
            # Note: We don't add color tags here anymore because C# will handle it
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
    # Skip keys that already have color tags (we'll generate them in C#)
    if k.startswith("<color"):
        continue
        
    # Use core translation if available
    if k in core_translations:
        final_dict[k] = core_translations[k]
    else:
        # Fallback to smart matching
        lower_k = k.lower()
        found = ""
        for ck, cv in core_translations.items():
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
                if (!string.IsNullOrEmpty(kvp.Value))
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
