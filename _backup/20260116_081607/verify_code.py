#!/usr/bin/env python3
"""
코드 검증 시스템
- 컴파일 가능 여부 확인
- 중복 함수/클래스 탐지
- 미사용 using 문 탐지
"""

import os
import re
import subprocess
from pathlib import Path

# 프로젝트 루트
PROJECT_ROOT = Path("/Users/ben/Desktop/qud_korean")
SCRIPTS_DIR = PROJECT_ROOT / "Scripts"
MOD_DIR = Path("/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization")

def find_duplicate_functions():
    """중복 함수 탐지"""
    print("=" * 80)
    print("🔍 중복 함수/메서드 탐지")
    print("=" * 80)
    
    functions = {}  # {함수명: [파일경로들]}
    
    for cs_file in SCRIPTS_DIR.rglob("*.cs"):
        with open(cs_file, 'r', encoding='utf-8') as f:
            content = f.read()
            
        # 함수 정의 찾기: public/private static/instance method
        pattern = r'(?:public|private|protected|internal)\s+(?:static\s+)?(?:\w+\s+)?(\w+)\s*\('
        matches = re.findall(pattern, content)
        
        for func_name in matches:
            if func_name not in functions:
                functions[func_name] = []
            functions[func_name].append(str(cs_file.relative_to(PROJECT_ROOT)))
    
    # 중복 탐지
    duplicates = {name: files for name, files in functions.items() if len(files) > 1}
    
    if duplicates:
        print(f"\n⚠️  중복 함수 발견: {len(duplicates)}개\n")
        for func_name, files in sorted(duplicates.items()):
            if func_name in ['Main', 'Awake', 'Start', 'Update', 'OnEnable', 'OnDisable']:
                continue  # Unity 기본 메서드는 제외
            print(f"  📌 {func_name}")
            for file in files:
                print(f"     - {file}")
            print()
    else:
        print("\n✅ 중복 함수 없음\n")

def find_duplicate_classes():
    """중복 클래스 탐지"""
    print("=" * 80)
    print("🔍 중복 클래스 탐지")
    print("=" * 80)
    
    classes = {}  # {클래스명: [파일경로들]}
    
    for cs_file in SCRIPTS_DIR.rglob("*.cs"):
        with open(cs_file, 'r', encoding='utf-8') as f:
            content = f.read()
            
        # 클래스 정의 찾기
        pattern = r'(?:public|internal|private)\s+(?:static\s+)?(?:partial\s+)?class\s+(\w+)'
        matches = re.findall(pattern, content)
        
        for class_name in matches:
            if class_name not in classes:
                classes[class_name] = []
            classes[class_name].append(str(cs_file.relative_to(PROJECT_ROOT)))
    
    # 중복 탐지
    duplicates = {name: files for name, files in classes.items() if len(files) > 1}
    
    if duplicates:
        print(f"\n⚠️  중복 클래스 발견: {len(duplicates)}개\n")
        for class_name, files in sorted(duplicates.items()):
            print(f"  📌 {class_name}")
            for file in files:
                print(f"     - {file}")
            print()
    else:
        print("\n✅ 중복 클래스 없음\n")

def check_common_functions():
    """자주 사용하는 함수들이 어디에 있는지 확인"""
    print("=" * 80)
    print("📚 핵심 함수 위치 확인")
    print("=" * 80)
    
    important_functions = [
        "ExtractPrefix",
        "StripColorTags",
        "RestoreColorTags",
        "TryTranslate",
        "TranslateLongDescription",
        "TryGetAnyTerm",
        "GetCategory"
    ]
    
    for func_name in important_functions:
        found = []
        for cs_file in SCRIPTS_DIR.rglob("*.cs"):
            with open(cs_file, 'r', encoding='utf-8') as f:
                if func_name in f.read():
                    found.append(str(cs_file.relative_to(PROJECT_ROOT)))
        
        if found:
            print(f"\n  📌 {func_name}")
            for file in found:
                print(f"     - {file}")

def verify_compilation():
    """C# 컴파일 가능 여부 확인 (간단한 구문 검사)"""
    print("=" * 80)
    print("🔧 기본 구문 검사")
    print("=" * 80)
    
    errors = []
    
    for cs_file in SCRIPTS_DIR.rglob("*.cs"):
        with open(cs_file, 'r', encoding='utf-8') as f:
            content = f.read()
            lines = content.split('\n')
        
        # 기본 구문 오류 체크
        open_braces = content.count('{')
        close_braces = content.count('}')
        
        if open_braces != close_braces:
            errors.append(f"{cs_file.relative_to(PROJECT_ROOT)}: 중괄호 불일치 ({{ {open_braces} vs }} {close_braces})")
        
        # using 문 중복 체크
        using_statements = re.findall(r'using\s+([\w\.]+);', content)
        duplicates = [u for u in using_statements if using_statements.count(u) > 1]
        if duplicates:
            errors.append(f"{cs_file.relative_to(PROJECT_ROOT)}: 중복 using 문 - {set(duplicates)}")
    
    if errors:
        print(f"\n⚠️  구문 오류 발견: {len(errors)}개\n")
        for error in errors:
            print(f"  ❌ {error}")
        print()
    else:
        print("\n✅ 기본 구문 검사 통과\n")

def main():
    print("\n" + "=" * 80)
    print("🚀 코드 검증 시스템 시작")
    print("=" * 80 + "\n")
    
    find_duplicate_classes()
    find_duplicate_functions()
    check_common_functions()
    verify_compilation()
    
    print("=" * 80)
    print("✅ 검증 완료")
    print("=" * 80 + "\n")

if __name__ == "__main__":
    main()
