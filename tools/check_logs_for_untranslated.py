#!/usr/bin/env python3
"""
실제 게임 로그에서 번역되지 않은 영문 텍스트를 찾는 스크립트
"""

import re
import os

def find_untranslated_in_logs():
    """게임 로그에서 번역되지 않은 영문 패턴 찾기"""
    
    log_path = "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log"
    
    if not os.path.exists(log_path):
        print(f"❌ 로그 파일을 찾을 수 없습니다: {log_path}")
        return
    
    print("=" * 80)
    print("게임 로그에서 번역되지 않은 영문 텍스트 검색")
    print("=" * 80)
    
    # 캐릭터 생성 관련 영문 패턴
    chargen_patterns = [
        r"Choose.*:",
        r"Select.*:",
        r"Pick.*:",
        r"Permadeath",
        r"Checkpointing",
        r"Most creatures",
        r"No XP",
        r"More XP",
        r"One chance",
        r"Currently in day",
        r"Learn the basics",
        r"Start a new game",
        r"Create a new character",
        r"Roll a random",
        r"Replay the last",
        r"from your build library",
        r"preset characters",
        r"You have unspent",
        r"You have spent too many",
        r"Invalid choice",
        r"No game mode selected"
    ]
    
    found_issues = {}
    
    try:
        with open(log_path, 'r', encoding='utf-8', errors='ignore') as f:
            log_content = f.read()
            
        for pattern in chargen_patterns:
            matches = re.findall(f".*{pattern}.*", log_content, re.IGNORECASE)
            if matches:
                # 중복 제거
                unique_matches = list(set(matches))
                if unique_matches:
                    found_issues[pattern] = unique_matches[:3]  # 최대 3개만
    
        if found_issues:
            print("\n⚠️  발견된 번역되지 않은 텍스트:\n")
            for pattern, matches in found_issues.items():
                print(f"패턴: {pattern}")
                for match in matches:
                    clean_match = match.strip()[:100]
                    print(f"  - {clean_match}")
                print()
        else:
            print("\n✅ 로그에서 번역되지 않은 캐릭터 생성 관련 텍스트를 찾지 못했습니다.")
            print("   (게임을 실행하고 캐릭터 생성 화면을 확인한 후 다시 검사하세요)")
    
    except Exception as e:
        print(f"❌ 로그 분석 중 오류: {e}")
    
    print("\n" + "=" * 80)

if __name__ == "__main__":
    find_untranslated_in_logs()
