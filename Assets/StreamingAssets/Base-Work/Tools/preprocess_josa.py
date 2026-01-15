#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Caves of Qud 한글 조사 전처리 스크립트
기반: csjosa 알고리즘
용도: XML 파일의 조사 태그를 미리 처리
"""

import re
import sys

# 한글 유니코드 범위
HANGUL_START = 0xAC00
HANGUL_END = 0xD7A3
JONGSEONG_COUNT = 28
RIEUL_JONGSEONG = 8

def has_jongseong(word):
    """받침 확인"""
    if not word:
        return False
    
    # 마지막 한글 글자 찾기
    last_char = None
    for c in reversed(word):
        if HANGUL_START <= ord(c) <= HANGUL_END:
            last_char = c
            break
    
    if not last_char:
        return False
    
    code = ord(last_char)
    return (code - HANGUL_START) % JONGSEONG_COUNT > 0

def has_rieul_jongseong(word):
    """ㄹ 받침 확인"""
    if not word:
        return False
    
    last_char = None
    for c in reversed(word):
        if HANGUL_START <= ord(c) <= HANGUL_END:
            last_char = c
            break
    
    if not last_char:
        return False
    
    code = ord(last_char)
    return (code - HANGUL_START) % JONGSEONG_COUNT == RIEUL_JONGSEONG

def select_josa(word, josa1, josa2):
    """조사 선택"""
    has_jong = has_jongseong(word)
    
    # ㄹ 받침 특수 처리 (으로/로)
    if josa1 == "으" and josa2 == "로":
        if has_rieul_jongseong(word):
            return josa2  # "로"
        else:
            return (josa1 + josa2) if has_jong else josa2  # "으로" or "로"
    
    return josa1 if has_jong else josa2

def replace_josa(text):
    """텍스트 내 모든 조사 괄호 처리"""
    # 패턴: "단어(조사1)조사2"
    pattern = r'(\S+)\(([^)]+)\)([^(\s]*)'
    
    def replacer(match):
        word = match.group(1)
        josa1 = match.group(2)
        josa2 = match.group(3)
        
        selected = select_josa(word, josa1, josa2)
        return word + selected
    
    return re.sub(pattern, replacer, text)

def process_file(input_path, output_path):
    """XML 파일 처리"""
    print(f"처리 중: {input_path}")
    
    with open(input_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    processed = replace_josa(content)
    
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(processed)
    
    print(f"완료: {output_path}")

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("사용법: python preprocess_josa.py <입력파일> <출력파일>")
        print("예시: python preprocess_josa.py Translation/Quests.xml Mod/KoreanLocalization/Quests.xml")
        sys.exit(1)
    
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    
    process_file(input_file, output_file)
    
    # 테스트
    print("\n테스트:")
    print(f"검(이)가 → {replace_josa('검(이)가')}")
    print(f"사과(을)를 → {replace_josa('사과(을)를')}")
    print(f"서울(으)로 → {replace_josa('서울(으)로')}")
    print(f"물(을)를 → {replace_josa('물(을)를')}")
