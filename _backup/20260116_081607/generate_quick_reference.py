#!/usr/bin/env python3
"""
프로젝트 상태 자동 요약 생성기
- 모든 중요 파일/폴더 스캔
- 핵심 정보를 하나의 파일로 통합
- AI가 매번 읽어야 할 필수 정보만 추출
"""

import os
import json
from pathlib import Path
from datetime import datetime

PROJECT_ROOT = Path("/Users/ben/Desktop/qud_korean")

def scan_project_structure():
    """프로젝트 전체 구조 스캔"""
    structure = {
        "scripts": {},
        "localization": {},
        "docs": {},
        "tools": {}
    }
    
    # Scripts 폴더
    scripts_dir = PROJECT_ROOT / "Scripts"
    if scripts_dir.exists():
        for cs_file in scripts_dir.rglob("*.cs"):
            rel_path = cs_file.relative_to(scripts_dir)
            category = str(rel_path.parts[0]) if len(rel_path.parts) > 1 else "root"
            
            if category not in structure["scripts"]:
                structure["scripts"][category] = []
            
            # 파일에서 핵심 정보 추출
            with open(cs_file, 'r', encoding='utf-8') as f:
                content = f.read()
                
            # 클래스명 추출
            import re
            classes = re.findall(r'(?:public|internal)\s+(?:static\s+)?class\s+(\w+)', content)
            # 주요 메서드 추출
            methods = re.findall(r'public\s+(?:static\s+)?(?:\w+\s+)?(\w+)\s*\(', content)
            
            structure["scripts"][category].append({
                "file": str(rel_path),
                "classes": classes[:3],  # 최대 3개
                "key_methods": list(set(methods))[:5]  # 최대 5개
            })
    
    # LOCALIZATION 폴더
    loc_dir = PROJECT_ROOT / "LOCALIZATION"
    if loc_dir.exists():
        for json_file in loc_dir.glob("*.json"):
            try:
                with open(json_file, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                structure["localization"][json_file.name] = {
                    "categories": list(data.keys()),
                    "total_entries": sum(len(v) if isinstance(v, dict) else 0 for v in data.values())
                }
            except:
                pass
    
    # 문서 파일
    for md_file in PROJECT_ROOT.glob("*.md"):
        structure["docs"][md_file.name] = {
            "size": md_file.stat().st_size,
            "modified": datetime.fromtimestamp(md_file.stat().st_mtime).strftime("%Y-%m-%d %H:%M")
        }
    
    # Python 도구
    for py_file in PROJECT_ROOT.glob("*.py"):
        structure["tools"][py_file.name] = {
            "size": py_file.stat().st_size
        }
    
    return structure

def generate_quick_reference():
    """빠른 참조 가이드 생성"""
    
    print("🔍 프로젝트 스캔 중...")
    structure = scan_project_structure()
    
    output = []
    output.append("# 🚀 프로젝트 빠른 참조 (자동 생성)")
    output.append(f"\n**생성 시각**: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    output.append("\n" + "=" * 80)
    
    # 1. 핵심 Scripts
    output.append("\n## 📁 Scripts 구조\n")
    
    core_files = {
        "00_Core": "핵심 시스템 (번역 엔진, LocalizationManager 등)",
        "99_Utils": "유틸리티 함수 (재사용 가능한 헬퍼)",
        "02_Patches": "Harmony 패치 (UI, Core 등)",
        "_Legacy": "⚠️ 레거시 코드 (사용 금지!)"
    }
    
    for category, desc in core_files.items():
        if category in structure["scripts"]:
            output.append(f"### {category}/ - {desc}")
            for file_info in structure["scripts"][category][:5]:  # 최대 5개 파일만
                output.append(f"- `{file_info['file']}`")
                if file_info['classes']:
                    output.append(f"  - 클래스: {', '.join(file_info['classes'])}")
                if file_info['key_methods']:
                    output.append(f"  - 주요 메서드: {', '.join(file_info['key_methods'][:3])}")
            output.append("")
    
    # 2. 핵심 함수 위치 (하드코딩 - 가장 중요!)
    output.append("\n## ⭐ 핵심 함수 위치 (필수 암기!)\n")
    output.append("```")
    output.append("TranslationEngine.TryTranslate()     → 01_TranslationEngine.cs")
    output.append("  ├─ ExtractPrefix()                 → 체크박스/접두사 자동 추출")
    output.append("  ├─ StripColorTags()                → {{w|text}} 색상 태그 제거")
    output.append("  └─ RestoreColorTags()              → 번역 후 태그 복원")
    output.append("")
    output.append("LocalizationManager.GetCategory()    → 00_03_LocalizationManager.cs")
    output.append("LocalizationManager.TryGetAnyTerm()  → 여러 카테고리 검색")
    output.append("")
    output.append("ChargenTranslationUtils              → 99_Utils/ChargenTranslationUtils.cs")
    output.append("  ├─ TranslateLongDescription()      → 다중 라인 번역")
    output.append("  ├─ TranslateMenuOptions()          → MenuOption 번역")
    output.append("  └─ TranslateBreadcrumb()           → Breadcrumb 번역")
    output.append("```")
    
    # 3. Glossary 파일
    output.append("\n## 📚 Glossary 파일\n")
    for filename, info in sorted(structure["localization"].items()):
        output.append(f"- `{filename}`: {info['total_entries']}개 항목")
        output.append(f"  - 카테고리: {', '.join(info['categories'])}")
    
    # 4. 도구
    output.append("\n## 🔧 사용 가능한 도구\n")
    for tool_name in sorted(structure["tools"].keys()):
        if tool_name.endswith('.py'):
            output.append(f"- `{tool_name}`")
    
    # 5. 문서
    output.append("\n## 📖 문서 파일\n")
    for doc_name, info in sorted(structure["docs"].items()):
        output.append(f"- `{doc_name}` (수정: {info['modified']})")
    
    # 6. 금지 사항
    output.append("\n## ⛔ 절대 금지!\n")
    output.append("```")
    output.append("❌ _Legacy/ 폴더의 코드 사용")
    output.append("❌ TranslationEngine 로직 중복 구현")
    output.append("❌ 색상 태그/프리픽스 수동 처리")
    output.append("❌ verify_code.py 실행 없이 배포")
    output.append("```")
    
    # 7. 필수 워크플로우
    output.append("\n## ✅ 코드 작성 전 필수 체크\n")
    output.append("```bash")
    output.append("# 1. 기존 함수 검색")
    output.append('grep -r "함수명" Scripts/ --include="*.cs"')
    output.append("")
    output.append("# 2. 검증 실행")
    output.append("python3 verify_code.py")
    output.append("")
    output.append("# 3. 이 파일 확인!")
    output.append("cat QUICK_REFERENCE.md")
    output.append("```")
    
    output.append("\n" + "=" * 80)
    output.append("\n**⚠️ 이 파일은 자동 생성됩니다. 수동 편집 금지!**")
    output.append("\n재생성: `python3 generate_quick_reference.py`")
    
    return "\n".join(output)

def main():
    content = generate_quick_reference()
    
    output_file = PROJECT_ROOT / "QUICK_REFERENCE.md"
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(content)
    
    print(f"✅ 빠른 참조 가이드 생성 완료: {output_file}")
    print(f"📄 파일 크기: {len(content)} bytes")
    print("\n" + "=" * 80)
    print("다음 명령으로 확인:")
    print(f"  cat {output_file}")
    print("=" * 80)

if __name__ == "__main__":
    main()
