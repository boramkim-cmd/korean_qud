#!/usr/bin/env python3
"""
🔍 미번역 항목 탐색 도구 (Missing Translation Checker)
- XML 파일의 텍스트가 glossary_*.json에 포함되었는지 확인
- C# 코드 내의 문자열 리터럴이 glossary_*.json에 포함되었는지 확인
"""

import os
import json
import re
import xml.etree.ElementTree as ET
from pathlib import Path

# 설정부
PROJECT_ROOT = Path(__file__).parent.parent.resolve()
LOCALIZATION_DIR = PROJECT_ROOT / "LOCALIZATION"

def load_glossary_keys(filename, category):
    """지정한 glossary 파일의 특정 카테고리 키들을 집합으로 반환"""
    path = LOCALIZATION_DIR / filename
    if not path.exists(): return set()
    try:
        with open(path, 'r', encoding='utf-8') as f:
            data = json.load(f)
            return {k.lower().strip() for k in data.get(category, {}).keys()}
    except: return set()

def check_xml(xml_path, glossary_file, category):
    """XML 파일 내 텍스트 추출 및 매칭 검사"""
    xml_path = PROJECT_ROOT / xml_path
    if not xml_path.exists(): return
    
    print(f"\n--- XML 검사: {xml_path.name} ---")
    keys = load_glossary_keys(glossary_file, category)
    
    try:
        tree = ET.parse(xml_path)
        root = tree.getroot()
        missing = set()
        
        # 속성 및 태그 탐색
        for elem in root.iter():
            # common attributes
            for attr in ['Description', 'DisplayName', 'ChargenDescription', 'Title']:
                val = elem.get(attr)
                if val and val.lower().strip() not in keys and not val.startswith('*'):
                    missing.add(val)
            
            # description tag
            if elem.tag == 'description' and elem.text:
                text = elem.text.strip()
                if text and text.lower() not in keys: missing.add(text)
        
        if missing:
            for m in sorted(list(missing))[:10]: print(f"  [M] {m[:70]}")
            if len(missing) > 10: print(f"  ... 외 {len(missing)-10}개")
        else: print("  ✅ 모든 항목이 용어집에 포함되어 있습니다.")
    except Exception as e: print(f"  ❌ XML 파싱 오류: {e}")

def check_csharp(cs_dir, glossary_file, category):
    """C# 코드 내 리터럴 추출 및 매칭 검사"""
    cs_dir = PROJECT_ROOT / cs_dir
    if not cs_dir.exists(): return
    
    print(f"\n--- C# 검사: {cs_dir.name} ---")
    keys = load_glossary_keys(glossary_file, category)
    missing = set()
    
    for cs_file in cs_dir.rglob("*.cs"):
        with open(cs_file, 'r', encoding='utf-8') as f:
            content = f.read()
            # " " 로 감싸인 10자 이상의 문자열 추출 (휴리스틱)
            matches = re.findall(r'"([^"]{10,})"', content)
            for m in matches:
                if m.lower().strip() not in keys: missing.add(m)
                
    if missing:
        for m in sorted(list(missing))[:10]: print(f"  [M] {m[:70]}")
        if len(missing) > 10: print(f"  ... 외 {len(missing)-10}개")
    else: print("  ✅ 코드 내 의심 문자열이 모두 용어집에 있거나 짧습니다.")

def main():
    print("="*80)
    print("🔍 미번역 항목 정밀 스캔")
    print("="*80)
    
    # 1. 스킬/돌연변이 XML 검사
    check_xml('Assets/StreamingAssets/Base/Skills.xml', 'glossary_skills.json', 'skill_desc')
    check_xml('Assets/StreamingAssets/Base/Mutations.xml', 'glossary_mutations.json', 'mutation_desc')
    
    # 2. 캐릭터 생성 화면 XML 검사
    check_xml('Assets/StreamingAssets/Base/EmbarkModules.xml', 'glossary_chargen.json', 'chargen')
    
    # 3. C# 코드 내 텍스트 검사 (돌연변이 설명 등)
    # 실제 소스 코드가 있을 경우 수행하도록 경로 설정 가능
    # check_csharp('Assets/core_source/XRL.World.Parts.Mutation/', 'glossary_mutations.json', 'mutation_desc')

    print("\n" + "="*80)

if __name__ == "__main__":
    main()
