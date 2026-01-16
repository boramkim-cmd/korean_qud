#!/usr/bin/env python3
"""
프로젝트 메타데이터 데이터베이스 생성기
- 모든 파일의 시그니처 추출
- JSON 데이터베이스로 저장
- 파일 열지 않고 메서드/클래스 확인 가능
"""

import os
import re
import json
from pathlib import Path
from datetime import datetime

PROJECT_ROOT = Path("/Users/ben/Desktop/qud_korean")

def extract_cs_metadata(file_path):
    """C# 파일에서 메타데이터 추출"""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    metadata = {
        "type": "csharp",
        "path": str(file_path.relative_to(PROJECT_ROOT)),
        "size": len(content),
        "lines": content.count('\n'),
        "modified": datetime.fromtimestamp(file_path.stat().st_mtime).strftime("%Y-%m-%d %H:%M"),
        "namespace": None,
        "classes": [],
        "methods": [],
        "using": []
    }
    
    # Namespace 추출
    ns_match = re.search(r'namespace\s+([\w\.]+)', content)
    if ns_match:
        metadata["namespace"] = ns_match.group(1)
    
    # Using 문 추출
    metadata["using"] = re.findall(r'using\s+([\w\.]+);', content)
    
    # 클래스 추출 (public/internal만)
    class_pattern = r'(?:public|internal)\s+(?:static\s+)?(?:partial\s+)?class\s+(\w+)'
    metadata["classes"] = re.findall(class_pattern, content)
    
    # 메서드 추출 (public만, 시그니처 포함)
    method_pattern = r'public\s+(?:static\s+)?(?:async\s+)?(\w+(?:<[^>]+>)?)\s+(\w+)\s*\(([^)]*)\)'
    methods = re.findall(method_pattern, content)
    metadata["methods"] = [
        {
            "return_type": m[0],
            "name": m[1],
            "params": m[2].strip() if m[2].strip() else "void"
        }
        for m in methods
    ]
    
    return metadata

def extract_py_metadata(file_path):
    """Python 파일에서 메타데이터 추출"""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    metadata = {
        "type": "python",
        "path": str(file_path.relative_to(PROJECT_ROOT)),
        "size": len(content),
        "lines": content.count('\n'),
        "modified": datetime.fromtimestamp(file_path.stat().st_mtime).strftime("%Y-%m-%d %H:%M"),
        "functions": [],
        "description": None
    }
    
    # Docstring 추출
    doc_match = re.search(r'"""([^"]+)"""', content)
    if doc_match:
        metadata["description"] = doc_match.group(1).strip().split('\n')[0]
    
    # 함수 추출
    func_pattern = r'def\s+(\w+)\s*\(([^)]*)\)'
    functions = re.findall(func_pattern, content)
    metadata["functions"] = [
        {
            "name": f[0],
            "params": f[1].strip() if f[1].strip() else "void"
        }
        for f in functions
    ]
    
    return metadata

def extract_md_metadata(file_path):
    """Markdown 파일에서 메타데이터 추출"""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    metadata = {
        "type": "markdown",
        "path": str(file_path.relative_to(PROJECT_ROOT)),
        "size": len(content),
        "lines": content.count('\n'),
        "modified": datetime.fromtimestamp(file_path.stat().st_mtime).strftime("%Y-%m-%d %H:%M"),
        "title": None,
        "headers": []
    }
    
    # 제목 추출 (첫 번째 # 헤더)
    title_match = re.search(r'^#\s+(.+)$', content, re.MULTILINE)
    if title_match:
        metadata["title"] = title_match.group(1).strip()
    
    # 모든 헤더 추출
    metadata["headers"] = re.findall(r'^#{1,3}\s+(.+)$', content, re.MULTILINE)
    
    return metadata

def extract_json_metadata(file_path):
    """JSON 파일에서 메타데이터 추출"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        metadata = {
            "type": "json",
            "path": str(file_path.relative_to(PROJECT_ROOT)),
            "size": file_path.stat().st_size,
            "modified": datetime.fromtimestamp(file_path.stat().st_mtime).strftime("%Y-%m-%d %H:%M"),
            "categories": list(data.keys()) if isinstance(data, dict) else [],
            "total_entries": sum(len(v) if isinstance(v, dict) else 0 for v in data.values()) if isinstance(data, dict) else 0
        }
        return metadata
    except:
        return None

def build_database():
    """전체 프로젝트 데이터베이스 구축"""
    db = {
        "generated": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
        "scripts": {},
        "tools": {},
        "docs": {},
        "localization": {}
    }
    
    print("🔍 프로젝트 스캔 중...")
    
    # Scripts 폴더
    scripts_dir = PROJECT_ROOT / "Scripts"
    if scripts_dir.exists():
        for cs_file in scripts_dir.rglob("*.cs"):
            if "_Legacy" in str(cs_file):
                continue  # 레거시 제외
            key = str(cs_file.relative_to(PROJECT_ROOT))
            db["scripts"][key] = extract_cs_metadata(cs_file)
    
    # Python 도구
    for py_file in PROJECT_ROOT.glob("*.py"):
        key = py_file.name
        db["tools"][key] = extract_py_metadata(py_file)
    
    # 문서
    for md_file in PROJECT_ROOT.glob("*.md"):
        if "_Docs_Archive" in str(md_file):
            continue  # 아카이브 제외
        key = md_file.name
        db["docs"][key] = extract_md_metadata(md_file)
    
    # Localization
    loc_dir = PROJECT_ROOT / "LOCALIZATION"
    if loc_dir.exists():
        for json_file in loc_dir.glob("*.json"):
            key = json_file.name
            meta = extract_json_metadata(json_file)
            if meta:
                db["localization"][key] = meta
    
    return db

def generate_human_readable_index(db):
    """사람이 읽기 쉬운 인덱스 생성"""
    lines = []
    lines.append("# 📚 프로젝트 완전 인덱스 (자동 생성)")
    lines.append(f"\n**생성**: {db['generated']}")
    lines.append("\n" + "=" * 80)
    
    # 핵심 Scripts
    lines.append("\n## 🔧 Scripts (핵심 코드)")
    lines.append("\n### TranslationEngine 및 Core")
    
    core_files = [k for k in db["scripts"].keys() if "00_Core" in k]
    for key in sorted(core_files):
        meta = db["scripts"][key]
        lines.append(f"\n#### `{key}`")
        if meta["classes"]:
            lines.append(f"- **클래스**: {', '.join(meta['classes'])}")
        if meta["methods"]:
            lines.append(f"- **주요 메서드**:")
            for m in meta["methods"][:5]:  # 최대 5개
                lines.append(f"  - `{m['return_type']} {m['name']}({m['params']})`")
    
    lines.append("\n### Utils")
    utils_files = [k for k in db["scripts"].keys() if "99_Utils" in k]
    for key in sorted(utils_files):
        meta = db["scripts"][key]
        lines.append(f"\n#### `{key}`")
        if meta["classes"]:
            lines.append(f"- **클래스**: {', '.join(meta['classes'])}")
        if meta["methods"]:
            lines.append(f"- **메서드**: {', '.join([m['name'] for m in meta['methods'][:5]])}")
    
    # Python 도구
    lines.append("\n## 🐍 Python 도구")
    for key in sorted(db["tools"].keys()):
        meta = db["tools"][key]
        lines.append(f"\n### `{key}`")
        if meta["description"]:
            lines.append(f"- {meta['description']}")
        if meta["functions"]:
            lines.append(f"- **함수**: {', '.join([f['name'] for f in meta['functions'][:5]])}")
    
    # 문서
    lines.append("\n## 📖 문서")
    priority_docs = ["AI_START_HERE.md", "QUICK_REFERENCE.md", "CODEBASE_MAP.md", "WORKFLOW.md"]
    for doc in priority_docs:
        if doc in db["docs"]:
            meta = db["docs"][doc]
            lines.append(f"\n### ⭐ `{doc}`")
            if meta["title"]:
                lines.append(f"- **제목**: {meta['title']}")
            lines.append(f"- **수정**: {meta['modified']}")
    
    # Glossary
    lines.append("\n## 📚 Glossary 파일")
    for key in sorted(db["localization"].keys()):
        meta = db["localization"][key]
        lines.append(f"\n### `{key}`")
        lines.append(f"- **항목 수**: {meta['total_entries']}")
        lines.append(f"- **카테고리**: {', '.join(meta['categories'])}")
    
    lines.append("\n" + "=" * 80)
    lines.append("\n**이 파일은 자동 생성됩니다.**")
    lines.append("\n재생성: `python3 build_project_db.py`")
    
    return "\n".join(lines)

def main():
    print("=" * 80)
    print("🚀 프로젝트 메타데이터 데이터베이스 생성")
    print("=" * 80 + "\n")
    
    # 데이터베이스 구축
    db = build_database()
    
    # JSON 저장
    db_file = PROJECT_ROOT / "project_metadata.json"
    with open(db_file, 'w', encoding='utf-8') as f:
        json.dump(db, f, indent=2, ensure_ascii=False)
    
    print(f"✅ JSON 데이터베이스 저장: {db_file}")
    print(f"   - Scripts: {len(db['scripts'])}개")
    print(f"   - Tools: {len(db['tools'])}개")
    print(f"   - Docs: {len(db['docs'])}개")
    print(f"   - Localization: {len(db['localization'])}개")
    
    # 사람이 읽기 쉬운 인덱스 생성
    index_content = generate_human_readable_index(db)
    index_file = PROJECT_ROOT / "PROJECT_INDEX.md"
    with open(index_file, 'w', encoding='utf-8') as f:
        f.write(index_content)
    
    print(f"\n✅ 인덱스 파일 생성: {index_file}")
    print(f"   크기: {len(index_content)} bytes")
    
    print("\n" + "=" * 80)
    print("✅ 완료!")
    print("=" * 80)
    print("\n사용법:")
    print("  1. cat PROJECT_INDEX.md  # 사람이 읽기")
    print("  2. cat project_metadata.json  # 프로그램이 읽기")

if __name__ == "__main__":
    main()
