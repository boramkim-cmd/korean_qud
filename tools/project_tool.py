#!/usr/bin/env python3
"""
🚀 통합 프로젝트 도구 (Unified Project Tool)
- 코드/JSON 검증
- 메타데이터 및 문서 생성
- 미번역 항목 탐색
"""

import os
import re
import json
from pathlib import Path
from datetime import datetime
from collections import OrderedDict

# ============================================================================
# 설정 및 경로
# ============================================================================

# 스크립트 위치 기준으로 프로젝트 루트 설정
PROJECT_ROOT = Path(__file__).parent.parent.resolve()
SCRIPTS_DIR = PROJECT_ROOT / "Scripts"
LOCALIZATION_DIR = PROJECT_ROOT / "LOCALIZATION"
DOCS_DIR = PROJECT_ROOT / "Docs"
TOOLS_DIR = PROJECT_ROOT / "tools"
ASSETS_DIR = PROJECT_ROOT / "Assets"

# ============================================================================
# 1. 코드 및 구문 검증
# ============================================================================

def verify_code():
    """C# 코드 중복 및 구문 오류 검증"""
    print("\n" + "=" * 80)
    print("🔍 코드 검증 (Code Validation)")
    print("=" * 80)
    
    functions = {}
    classes = {}
    errors = []
    
    missing_headers = []
    
    for cs_file in SCRIPTS_DIR.rglob("*.cs"):
        if "_Legacy" in str(cs_file): continue
            
        with open(cs_file, 'r', encoding='utf-8') as f:
            content = f.read()
        
        rel_path = str(cs_file.relative_to(PROJECT_ROOT))
        
        # 0. 표준 헤더 체크
        if "분류:" not in content or "역할:" not in content:
            missing_headers.append(rel_path)

        # 1. 함수/클래스 찾기
        func_pattern = r'(?:public|private|protected|internal)\s+(?:static\s+)?(?:\w+\s+)?(\w+)\s*\('
        for func_name in re.findall(func_pattern, content):
            if func_name not in functions: functions[func_name] = []
            functions[func_name].append(rel_path)
        
        class_pattern = r'(?:public|internal|private)\s+(?:static\s+)?(?:partial\s+)?class\s+(\w+)'
        for class_name in re.findall(class_pattern, content):
            if class_name not in classes: classes[class_name] = []
            classes[class_name].append(rel_path)
        
        # 구문 오류 체크 (주석/문자열 제외)
        clean = re.sub(r'//.*', '', content)
        clean = re.sub(r'/\*.*?\*/', '', clean, flags=re.DOTALL)
        clean = re.sub(r'"[^"\\]*(?:\\.[^"\\]*)*"', '', clean)
        clean = re.sub(r"'[^'\\]*(?:\\.[^'\\]*)*'", '', clean)
        
        if clean.count('{') != clean.count('}'):
            errors.append(f"{rel_path}: 중괄호 불일치 ({clean.count('{')} vs {clean.count('}')})")
    
    # 보고
    dupes = {n: f for n, f in functions.items() if len(f) > 1 and n not in ["Postfix", "Prefix", "TargetMethod"]}
    if dupes:
        print(f"⚠️  중복 함수 탐지: {len(dupes)}개")
        for n, f in list(dupes.items())[:5]:
            print(f"   - {n}: {len(f)}개 파일")
    
    if errors:
        print(f"❌ 구문 오류: {len(errors)}개")
        for e in errors: print(f"   - {e}")
    
    if missing_headers:
        print(f"⚠️  표준 헤더 누락: {len(missing_headers)}개 파일")
        for h in missing_headers[:5]:
            print(f"   - {h}")
    
    if not dupes and not errors and not missing_headers: print("✅ 코드 검증 통과")
    return not errors and not missing_headers

# ============================================================================
# 2. 번역 데이터 검증
# ============================================================================

def verify_localization():
    """JSON 번역 데이터 무결성 검증"""
    print("\n" + "=" * 80)
    print("📚 번역 데이터 검증 (Localization Validation)")
    print("=" * 80)
    
    total_entries = 0
    empty_count = 0
    dupe_count = 0
    
    for json_file in LOCALIZATION_DIR.glob("*.json"):
        try:
            with open(json_file, 'r', encoding='utf-8') as f:
                content = f.read()
                data = json.loads(content)
            
            # 1. 빈 값 체크
            for cat, entries in data.items():
                if isinstance(entries, dict):
                    for k, v in entries.items():
                        total_entries += 1
                        if not v or not v.strip(): empty_count += 1
            
            # 2. 중복 키 체크
            cat_blocks = re.findall(r'"([^"]+)":\s*\{([^\}]*)\}', content, re.DOTALL)
            for cat_name, cat_content in cat_blocks:
                keys = re.findall(r'"([^"]+)"\s*:', cat_content)
                dupes = [k for k in set(keys) if keys.count(k) > 1]
                if dupes:
                    print(f"⚠️  [{json_file.name}] '{cat_name}' 카테고리 내 중복 키: {', '.join(dupes)}")
                    dupe_count += len(dupes)
                    
        except Exception as e:
            print(f"❌ [{json_file.name}] JSON 파싱 오류: {e}")
            return False
            
    print(f"총 번역 항목: {total_entries}개")
    if empty_count: print(f"⚠️  빈 번역 항목: {empty_count}개")
    if dupe_count: print(f"⚠️  중복 키 발견: {dupe_count}개")
    
    if not empty_count and not dupe_count: print("✅ 번역 데이터 무결성 확인")
    return dupe_count == 0

# ============================================================================
# 3. 메타데이터 및 인덱스 생성
# ============================================================================

def build_project_references():
    """프로젝트 메타데이터(json) 및 인덱스(md) 생성"""
    print("\n" + "=" * 80)
    print("🔧 프로젝트 레퍼런스 생성 (Metadata & Index)")
    print("=" * 80)
    
    db = {
        "generated": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
        "scripts": {}, "tools": {}, "docs": {}, "localization": {}
    }
    
    # 1. Scripts 스캔
    for cs_file in SCRIPTS_DIR.rglob("*.cs"):
        if "_Legacy" in str(cs_file): continue
        with open(cs_file, 'r', encoding='utf-8') as f: content = f.read()
        
        rel_path = str(cs_file.relative_to(PROJECT_ROOT))
        
        # 파일 헤더 정보 추출
        classification = "N/A"
        role = "N/A"
        class_match = re.search(r'\* 분류:\s*\[([^\]]+)\]', content)
        role_match = re.search(r'\* 역할:\s*([^\n\*]+)', content)
        if class_match: classification = class_match.group(1).strip()
        if role_match: role = role_match.group(1).strip()

        ns_match = re.search(r'namespace\s+([\w\.]+)', content)
        
        db["scripts"][rel_path] = {
            "classification": classification,
            "role": role,
            "namespace": ns_match.group(1) if ns_match else None,
            "classes": re.findall(r'(?:public|internal)\s+(?:static\s+)?class\s+(\w+)', content),
            "methods": [
                {"name": m[1], "return": m[0], "params": m[2].strip() or ""}
                for m in re.findall(r'public\s+(?:static\s+)?([\w\[\]<>]+)\s+(\w+)\s*\(([^)]*)\)', content)
            ]
        }

    # 2. 도구/문서/로컬라이제이션 스캔
    for path in TOOLS_DIR.iterdir():
        if path.is_file() and path.suffix == ".py":
            db["tools"][path.name] = {"type": "script", "size": path.stat().st_size}
        elif path.is_dir() and not path.name.startswith(('.', '_')):
            db["tools"][path.name] = {"type": "tool_folder"}
    for md_file in DOCS_DIR.glob("*.md"):
        db["docs"][md_file.name] = {"modified": datetime.fromtimestamp(md_file.stat().st_mtime).strftime("%Y-%m-%d")}
    for json_file in LOCALIZATION_DIR.glob("*.json"):
        try:
            with open(json_file, 'r', encoding='utf-8') as f: data = json.load(f)
            db["localization"][json_file.name] = {
                "categories": list(data.keys()),
                "entries": sum(len(v) if isinstance(v, dict) else 0 for v in data.values())
            }
        except: pass

    # 파일 저장
    with open(TOOLS_DIR / "project_metadata.json", 'w', encoding='utf-8') as f:
        json.dump(db, f, indent=2, ensure_ascii=False)
    
    # 3. 01_CORE_PROJECT_INDEX.md 생성
    lines = ["# 📚 프로젝트 완전 인덱스 (자동 생성)", f"\n**생성**: {db['generated']}\n", "이 문서는 프로젝트의 모든 파일과 메서드 시그니처를 포함합니다. **새로운 기능을 만들기 전, 반드시 여기서 기존 메서드를 검색하십시오.**\n", "=" * 80]
    
    # 분류별로 그룹화하여 출력하면 더 체계적임
    by_class = {}
    for key, meta in db["scripts"].items():
        cls = meta["classification"]
        if cls not in by_class: by_class[cls] = []
        by_class[cls].append((key, meta))
        
    for cls in sorted(by_class.keys()):
        lines.append(f"\n## 📂 [{cls}]")
        for key, meta in sorted(by_class[cls]):
            lines.append(f"\n### `{key}`")
            lines.append(f"- **역할**: {meta['role']}")
            if meta["namespace"]: lines.append(f"- **Namespace**: `{meta['namespace']}`")
            if meta["methods"]:
                lines.append("- **공개 메서드 (Public Methods)**:")
                lines.append("  ```csharp")
                for m in meta["methods"][:15]: # 메서드 수 15개로 상향
                    lines.append(f"  {m['return']} {m['name']}({m['params']})")
                if len(meta["methods"]) > 15:
                    lines.append(f"  ... 외 {len(meta['methods'])-15}개")
                lines.append("  ```")
    
    with open(DOCS_DIR / "01_CORE_PROJECT_INDEX.md", 'w', encoding='utf-8') as f:
        f.write("\n".join(lines))
        
    # 4. 02_CORE_QUICK_REFERENCE.md 생성
    q_lines = ["# 🚀 프로젝트 빠른 참조 (자동 생성)", f"\n**생성**: {db['generated']}\n", "## ⭐ 핵심 경로"]
    q_lines.append("```\nScripts/00_Core/00_00_01_TranslationEngine.cs  → 핵심 엔진\nScripts/00_Core/00_00_03_LocalizationManager.cs → 데이터 관리\nLOCALIZATION/glossary_*.json              → 용어집 데이터\n```")
    
    q_lines.append("\n## 📚 용어집 현황")
    for k, v in sorted(db["localization"].items()):
        q_lines.append(f"- `{k}`: {v['entries']}개 항목")

    q_lines.append("\n## ⛔ 절대 금지 (DO NOT)")
    q_lines.append("```")
    q_lines.append("❌ _Legacy/ 폴더의 코드 사용")
    q_lines.append("❌ TranslationEngine 로직 중복 구현")
    q_lines.append("❌ 색상 태그/프리픽스 수동 처리")
    q_lines.append("❌ project_tool.py 검증 없이 배포")
    q_lines.append("```")

    q_lines.append("\n## ✅ 작업 체크리스트")
    q_lines.append("```")
    q_lines.append("1. 01_CORE_PROJECT_INDEX.md에서 기존 함수 확인")
    q_lines.append("2. Scripts/ 내부 로직 수정")
    q_lines.append("3. python3 tools/project_tool.py 로 검증")
    q_lines.append("4. ./tools/deploy-mods.sh 로 게임 적용")
    q_lines.append("```")
        
    with open(DOCS_DIR / "02_CORE_QUICK_REFERENCE.md", 'w', encoding='utf-8') as f:
        f.write("\n".join(q_lines))

    print(f"✅ 레퍼런스 가이드 갱신: Docs/02_CORE_QUICK_REFERENCE.md")

    print(f"✅ 메타데이터 및 인덱스 생성 완료 (Docs 01, 02 갱신)")

# ============================================================================
# 메인 실행부
# ============================================================================

def main():
    print("\n" + "🚀" * 40)
    print("  Qud 한글화 프로젝트 통합 도구 환경 검증 시작")
    print("🚀" * 40)
    
    results = [
        verify_code(),
        verify_localization()
    ]
    
    build_project_references()
    
    print("\n" + "=" * 80)
    if all(results):
        print("✨ 모든 검증 및 생성 작업이 성공적으로 완료되었습니다.")
    else:
        print("⚠️  일부 검증 단계에서 주의사항이 발견되었습니다. 위 리포트를 확인하세요.")
    print("=" * 80 + "\n")

if __name__ == "__main__":
    main()
