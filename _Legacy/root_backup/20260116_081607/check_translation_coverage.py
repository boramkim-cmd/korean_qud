#!/usr/bin/env python3
"""
캐릭터 생성 화면에서 번역되지 않은 텍스트를 찾는 스크립트
"""

import json
import os
import re

def check_glossary_coverage():
    """각 glossary 파일의 번역 커버리지를 확인"""
    
    localization_dir = "/Users/ben/Desktop/qud_korean/LOCALIZATION"
    
    # 각 glossary 파일 검사
    glossaries = [
        "glossary_chargen.json",
        "glossary_proto.json", 
        "glossary_skills.json",
        "glossary_mutations.json",
        "glossary_cybernetics.json",
        "glossary_location.json",
        "glossary_pregen.json",
        "glossary_ui.json",
        "glossary_terms.json"
    ]
    
    print("=" * 80)
    print("번역 파일 커버리지 분석")
    print("=" * 80)
    
    for glossary_file in glossaries:
        filepath = os.path.join(localization_dir, glossary_file)
        if not os.path.exists(filepath):
            print(f"\n⚠️  {glossary_file}: 파일 없음")
            continue
            
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                data = json.load(f)
            
            total_entries = 0
            empty_translations = []
            
            # 모든 카테고리 순회
            for category, entries in data.items():
                if isinstance(entries, dict):
                    for key, value in entries.items():
                        total_entries += 1
                        if not value or value.strip() == "":
                            empty_translations.append((category, key))
            
            print(f"\n📄 {glossary_file}")
            print(f"   총 항목: {total_entries}개")
            
            if empty_translations:
                print(f"   ⚠️  빈 번역: {len(empty_translations)}개")
                for cat, key in empty_translations[:5]:  # 처음 5개만 표시
                    print(f"      - [{cat}] {key[:60]}...")
                if len(empty_translations) > 5:
                    print(f"      ... 외 {len(empty_translations) - 5}개")
            else:
                print(f"   ✅ 모든 항목 번역 완료")
                
        except json.JSONDecodeError as e:
            print(f"\n❌ {glossary_file}: JSON 파싱 오류 - {e}")
        except Exception as e:
            print(f"\n❌ {glossary_file}: 오류 - {e}")
    
    print("\n" + "=" * 80)

if __name__ == "__main__":
    check_glossary_coverage()
