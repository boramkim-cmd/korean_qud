#!/usr/bin/env python3
"""
캐릭터 생성 화면에서 실제로 사용되는 텍스트와 glossary 매칭 확인
"""

import json
import os
import re

def check_xml_vs_glossary():
    """XML 파일의 텍스트가 glossary에 있는지 확인"""
    
    xml_path = "/Users/ben/Desktop/qud_korean/Assets/StreamingAssets/Base/EmbarkModules.xml"
    glossary_path = "/Users/ben/Desktop/qud_korean/LOCALIZATION/glossary_chargen.json"
    
    print("=" * 80)
    print("XML vs Glossary 매칭 검사")
    print("=" * 80)
    
    # Glossary 로드
    with open(glossary_path, 'r', encoding='utf-8') as f:
        glossary = json.load(f)
    
    # 모든 번역 키를 소문자로 변환하여 저장
    all_keys = set()
    for category, entries in glossary.items():
        if isinstance(entries, dict):
            for key in entries.keys():
                all_keys.add(key.lower())
    
    print(f"\n📚 Glossary 총 키 개수: {len(all_keys)}")
    
    # XML 파일 읽기
    with open(xml_path, 'r', encoding='utf-8') as f:
        xml_content = f.read()
    
    # 게임 모드 관련 텍스트 추출
    mode_titles = re.findall(r'<mode ID="[^"]*" Title="([^"]*)"', xml_content)
    mode_descriptions = re.findall(r'<description>(.*?)</description>', xml_content, re.DOTALL)
    
    # 캐릭터 타입 관련 텍스트 추출
    type_titles = re.findall(r'<type ID="[^"]*" Title="([^"]*)"', xml_content)
    type_descriptions = re.findall(r'<description>(.*?)</description>', xml_content, re.DOTALL)
    
    print(f"\n📄 XML에서 추출한 항목:")
    print(f"   - 모드 타이틀: {len(mode_titles)}개")
    print(f"   - 모드 설명: {len(mode_descriptions)}개")
    print(f"   - 타입 타이틀: {len(type_titles)}개")
    
    # 매칭되지 않는 항목 찾기
    missing = []
    
    # 타이틀 검사
    for title in mode_titles + type_titles:
        if title.lower() not in all_keys:
            missing.append(("Title", title))
    
    # 설명 검사 (각 라인별로)
    for desc in mode_descriptions:
        # 줄바꿈으로 분리
        lines = [line.strip() for line in desc.split('\n') if line.strip()]
        for line in lines:
            # 색상 태그 등 정리
            clean_line = re.sub(r'{{[^}]+\|([^}]+)}}', r'\1', line)
            clean_line = clean_line.strip()
            if clean_line and clean_line.lower() not in all_keys:
                # 동적 값이 있는 경우 (예: {day_of_year})
                if '{' not in clean_line:
                    missing.append(("Description", clean_line))
    
    if missing:
        print(f"\n⚠️  Glossary에 없는 텍스트 ({len(missing)}개):\n")
        for item_type, text in missing[:20]:  # 최대 20개만 표시
            print(f"   [{item_type}] {text[:70]}")
        if len(missing) > 20:
            print(f"   ... 외 {len(missing) - 20}개")
    else:
        print(f"\n✅ 모든 XML 텍스트가 Glossary에 존재합니다!")
    
    print("\n" + "=" * 80)

if __name__ == "__main__":
    check_xml_vs_glossary()
