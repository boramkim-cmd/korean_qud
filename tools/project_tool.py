#!/usr/bin/env python3
"""
🚀 통합 프로젝트 도구 (Unified Project Tool)
- 코드/JSON 검증
- 메타데이터 및 문서 생성
- 미번역 항목 탐색
"""

from __future__ import annotations
import json
import re
import subprocess
from datetime import datetime
from pathlib import Path
from typing import Any

# ============================================================================
# 설정 및 경로
# ============================================================================

PROJECT_ROOT = Path(__file__).parent.parent.resolve()
SCRIPTS_DIR = PROJECT_ROOT / "Scripts"
LOCALIZATION_DIR = PROJECT_ROOT / "LOCALIZATION"
DOCS_DIR = PROJECT_ROOT / "Docs"
TOOLS_DIR = PROJECT_ROOT / "tools"
ASSETS_DIR = PROJECT_ROOT / "Assets"

# 정규표현식 패턴 (한 번만 컴파일)
FUNC_PATTERN = re.compile(r'(?:public|private|protected|internal)\s+(?:static\s+)?(?:\w+\s+)?(\w+)\s*\(')
CLASS_PATTERN = re.compile(r'(?:public|internal|private)\s+(?:static\s+)?(?:partial\s+)?class\s+(\w+)')
NAMESPACE_PATTERN = re.compile(r'namespace\s+([\w\.]+)')
METHOD_PATTERN = re.compile(r'public\s+(?:static\s+)?([\w\[\]<>]+)\s+(\w+)\s*\(([^)]*)\)')
CLASSIFICATION_PATTERN = re.compile(r'\* 분류:\s*\[([^\]]+)\]')
ROLE_PATTERN = re.compile(r'\* 역할:\s*([^\n\*]+)')


# Harmony 함수명 (중복 허용)
HARMONY_FUNCS = frozenset({"Postfix", "Prefix", "TargetMethod"})


def _read_file(path: Path) -> str | None:
    """파일을 읽고 내용 반환. 실패 시 None. utf-8-sig로 BOM 처리."""
    try:
        return path.read_text(encoding='utf-8-sig')
    except IOError:
        return None

# Master Code Noise Regex
# 1. Line Comment: //...
# 2. Block Comment: /*...*/
# 3. Verbatim String: @"..." (allows "")
# 4. Normal String: "..." (allows \")
# 5. Char Literal: '.' (allows \')
NOISE_PATTERN = re.compile(
    r'//[^\n]*|/\*.*?\*/|@"(?:[^"]|"")*"|"(?:[^"\\]|\\.)*"|\'(?:[^\'\\]|\\.)*\'',
    re.DOTALL
)

def _strip_code_noise(content: str) -> str:
    """주석 및 문자열 제거하여 구조만 추출 (Master Regex 사용)"""
    # 매칭된 모든 노이즈(주석, 문자열)를 공백으로 치환하여 길이/라인 보존
    # 다만 단순히 제거('')하면 인덱스가 밀릴 수 있으나, 
    # 중괄호 개수만 세는 용도라면 제거해도 무방.
    return NOISE_PATTERN.sub('', content)


# ============================================================================
# 1. 코드 및 구문 검증
# ============================================================================

def verify_code() -> bool:
    """C# 코드 중복 및 구문 오류 검증"""
    print("\n" + "=" * 80)
    print("🔍 코드 검증 (Code Validation)")
    print("=" * 80)

    functions: dict[str, list[str]] = {}
    classes: dict[str, list[str]] = {}
    errors: list[str] = []
    missing_headers: list[str] = []

    for cs_file in SCRIPTS_DIR.rglob("*.cs"):
        if "_Legacy" in str(cs_file):
            continue

        content = _read_file(cs_file)
        if content is None:
            continue

        rel_path = str(cs_file.relative_to(PROJECT_ROOT))

        # 표준 헤더 체크
        if "분류:" not in content or "역할:" not in content:
            missing_headers.append(rel_path)

        # 함수/클래스 찾기
        for func_name in FUNC_PATTERN.findall(content):
            functions.setdefault(func_name, []).append(rel_path)

        for class_name in CLASS_PATTERN.findall(content):
            classes.setdefault(class_name, []).append(rel_path)

        # 구문 오류 체크
        clean = _strip_code_noise(content)
        open_count, close_count = clean.count('{'), clean.count('}')
        if open_count != close_count:
            errors.append(f"{rel_path}: 중괄호 불일치 ({open_count} vs {close_count})")

    # 결과 보고
    dupes = {}
    for name, paths in functions.items():
        if name in HARMONY_FUNCS:
            continue
        
        # 유니크한 파일 경로만 카운트 (같은 파일 내 오버로딩 허용)
        unique_files = set(paths)
        if len(unique_files) > 1:
            dupes[name] = unique_files

    if dupes:
        print(f"⚠️  중복 함수 탐지: {len(dupes)}개 (서로 다른 파일에 존재)")
        for name, files in list(dupes.items())[:5]:
            print(f"   - {name}: {len(files)}개 파일")

    if errors:
        print(f"❌ 구문 오류: {len(errors)}개")
        for e in errors:
            print(f"   - {e}")

    if missing_headers:
        print(f"⚠️  표준 헤더 누락: {len(missing_headers)}개 파일")
        for h in missing_headers[:5]:
            print(f"   - {h}")

    passed = not errors and not missing_headers
    if not dupes and passed:
        print("✅ 코드 검증 통과")
    
    return passed


# ============================================================================
# 2. 번역 데이터 검증
# ============================================================================

def verify_localization() -> bool:
    """JSON 번역 데이터 무결성 검증"""
    print("\n" + "=" * 80)
    print("📚 번역 데이터 검증 (Localization Validation)")
    print("=" * 80)

    total_entries = 0
    empty_count = 0
    dupe_count = 0

    def duplicate_key_checker(pairs):
        nonlocal dupe_count
        result = {}
        seen_keys = set()
        for key, value in pairs:
            if key in seen_keys:
                print(f"⚠️  중복 키 발견: {key}")
                dupe_count += 1
            else:
                seen_keys.add(key)
                result[key] = value
        return result

    for json_file in LOCALIZATION_DIR.rglob("*.json"):
        content = _read_file(json_file)
        if content is None:
            print(f"❌ [{json_file.name}] 파일 읽기 오류")
            return False

        try:
            # Custom loader to catch duplicates
            data = json.loads(content, object_pairs_hook=duplicate_key_checker)
            
            # 1. 빈 값 체크
            if isinstance(data, dict):
                 stack = [data]
                 while stack:
                     current = stack.pop()
                     if isinstance(current, dict):
                         # items() returns key, value
                         for k, v in current.items():
                             if isinstance(v, (dict, list)):
                                 stack.append(v)
                             elif isinstance(v, str):
                                 total_entries += 1
                                 if not v.strip():
                                     # print(f"⚠️  [{json_file.name}] 빈 값: {k}")
                                     empty_count += 1
                     elif isinstance(current, list):
                         for item in current:
                             if isinstance(item, (dict, list)):
                                 stack.append(item)
                             elif isinstance(item, str):
                                 # List strings usually used for leveltext, not key-value
                                 pass

        except Exception as e:
            print(f"❌ [{json_file.name}] 데이터 검증 오류: {e}")
            return False

    print(f"총 번역 항목: {total_entries}개")
    if empty_count:
        print(f"⚠️  빈 번역 항목: {empty_count}개")
    if dupe_count:
        print(f"⚠️  중복 키 발견: {dupe_count}개")

    if not empty_count and not dupe_count:
        print("✅ 번역 데이터 무결성 확인")

    return dupe_count == 0

# ============================================================================
# 2.5 빌드 검증 (Build Validation)
# ============================================================================

def verify_build() -> bool:
    """dotnet build 실행 및 결과 검증"""
    print("\n" + "=" * 80)
    print("🔨 빌드 검증 (Build Validation)")
    print("=" * 80)

    try:
        # Run dotnet build
        result = subprocess.run(
            ["dotnet", "build", "-v", "q", "/property:WarningLevel=0"], # Quiet output, suppress warnings for cleaner output checks
            cwd=PROJECT_ROOT,
            capture_output=True,
            text=True
        )

        if result.returncode == 0:
            print("✅ 빌드 성공")
            return True
        else:
            print("❌ 빌드 실패")
            print(result.stdout)
            print(result.stderr)
            return False

    except FileNotFoundError:
        print("❌ dotnet 명령어를 찾을 수 없습니다. .NET SDK가 설치되어 있는지 확인하세요.")
        return False
    except Exception as e:
        print(f"❌ 빌드 실행 중 오류 발생: {e}")
        return False
# ============================================================================
# 3. 메타데이터 및 인덱스 생성
# ============================================================================

def _scan_scripts() -> dict[str, dict[str, Any]]:
    """Scripts 디렉토리 스캔"""
    scripts: dict[str, dict[str, Any]] = {}
    
    for cs_file in SCRIPTS_DIR.rglob("*.cs"):
        if "_Legacy" in str(cs_file):
            continue
            
        content = _read_file(cs_file)
        if content is None:
            continue

        rel_path = str(cs_file.relative_to(PROJECT_ROOT))

        # 헤더 정보 추출
        cls_match = CLASSIFICATION_PATTERN.search(content)
        role_match = ROLE_PATTERN.search(content)
        ns_match = NAMESPACE_PATTERN.search(content)

        scripts[rel_path] = {
            "classification": cls_match.group(1).strip() if cls_match else "N/A",
            "role": role_match.group(1).strip() if role_match else "N/A",
            "namespace": ns_match.group(1) if ns_match else None,
            "classes": CLASS_PATTERN.findall(content),
            "methods": [
                {"name": m[1], "return": m[0], "params": m[2].strip() or ""}
                for m in METHOD_PATTERN.findall(content)
            ]
        }
    
    return scripts


def build_project_references() -> None:
    """프로젝트 메타데이터(json) 및 인덱스(md) 생성"""
    print("\n" + "=" * 80)
    print("🔧 프로젝트 레퍼런스 생성 (Metadata & Index)")
    print("=" * 80)

    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    db: dict[str, Any] = {
        "generated": timestamp,
        "scripts": _scan_scripts(),
        "tools": {},
        "docs": {},
        "localization": {}
    }

    # 도구 스캔
    for path in TOOLS_DIR.iterdir():
        if path.is_file() and path.suffix == ".py":
            db["tools"][path.name] = {"type": "script", "size": path.stat().st_size}
        elif path.is_dir() and not path.name.startswith(('.', '_')):
            db["tools"][path.name] = {"type": "tool_folder"}

    # 문서 스캔
    for md_file in DOCS_DIR.glob("*.md"):
        mtime = datetime.fromtimestamp(md_file.stat().st_mtime)
        db["docs"][md_file.name] = {"modified": mtime.strftime("%Y-%m-%d")}

    # 로컬라이제이션 스캔
    for json_file in LOCALIZATION_DIR.rglob("*.json"):
        content = _read_file(json_file)
        if content is None:
            continue
        try:
            data = json.loads(content)
            db["localization"][json_file.name] = {
                "categories": list(data.keys()),
                "entries": sum(len(v) if isinstance(v, dict) else 0 for v in data.values())
            }
        except json.JSONDecodeError:
            pass

    # 메타데이터 저장
    (TOOLS_DIR / "project_metadata.json").write_text(
        json.dumps(db, indent=2, ensure_ascii=False),
        encoding='utf-8'
    )

    # 인덱스 문서 생성
    _generate_project_index(db)
    _generate_quick_reference(db)

    print("✅ 메타데이터 및 인덱스 생성 완료 (Docs 01, 02 갱신)")


def _generate_project_index(db: dict[str, Any]) -> None:
    """01_CORE_PROJECT_INDEX.md 생성"""
    lines = [
        "# 📚 프로젝트 완전 인덱스 (자동 생성)",
        f"\n**생성**: {db['generated']}\n",
        "이 문서는 프로젝트의 모든 파일과 메서드 시그니처를 포함합니다. "
        "**새로운 기능을 만들기 전, 반드시 여기서 기존 메서드를 검색하십시오.**\n",
        "=" * 80
    ]

    # 분류별 그룹화
    by_class: dict[str, list[tuple[str, dict]]] = {}
    for key, meta in db["scripts"].items():
        cls = meta["classification"]
        by_class.setdefault(cls, []).append((key, meta))

    for cls in sorted(by_class.keys()):
        lines.append(f"\n## 📂 [{cls}]")
        for key, meta in sorted(by_class[cls]):
            lines.append(f"\n### `{key}`")
            lines.append(f"- **역할**: {meta['role']}")
            if meta["namespace"]:
                lines.append(f"- **Namespace**: `{meta['namespace']}`")
            if meta["methods"]:
                lines.append("- **공개 메서드 (Public Methods)**:")
                lines.append("  ```csharp")
                for m in meta["methods"][:15]:
                    lines.append(f"  {m['return']} {m['name']}({m['params']})")
                if len(meta["methods"]) > 15:
                    lines.append(f"  ... 외 {len(meta['methods']) - 15}개")
                lines.append("  ```")

    (DOCS_DIR / "01_CORE_PROJECT_INDEX.md").write_text("\n".join(lines), encoding='utf-8')


def _generate_quick_reference(db: dict[str, Any]) -> None:
    """02_CORE_QUICK_REFERENCE.md 생성"""
    lines = [
        "# 🚀 프로젝트 빠른 참조 (자동 생성)",
        f"\n**생성**: {db['generated']}\n",
        "## ⭐ 핵심 경로",
        "```",
        "Scripts/00_Core/00_00_01_TranslationEngine.cs  → 핵심 엔진",
        "Scripts/00_Core/00_00_03_LocalizationManager.cs → 데이터 관리",
        "LOCALIZATION/**/*.json              → 용어집 데이터",
        "```",
        "\n## 📚 용어집 현황"
    ]

    for k, v in sorted(db["localization"].items()):
        lines.append(f"- `{k}`: {v['entries']}개 항목")

    lines.extend([
        "\n## ⛔ 절대 금지 (DO NOT)",
        "```",
        "❌ _Legacy/ 폴더의 코드 사용",
        "❌ TranslationEngine 로직 중복 구현",
        "❌ 색상 태그/프리픽스 수동 처리",
        "❌ project_tool.py 검증 없이 배포",
        "```",
        "\n## ✅ 작업 체크리스트",
        "```",
        "1. 01_CORE_PROJECT_INDEX.md에서 기존 함수 확인",
        "2. Scripts/ 내부 로직 수정",
        "3. python3 tools/project_tool.py 로 검증",
        "4. ./tools/deploy-mods.sh 로 게임 적용",
        "```"
    ])

    (DOCS_DIR / "02_CORE_QUICK_REFERENCE.md").write_text("\n".join(lines), encoding='utf-8')
    print("✅ 레퍼런스 가이드 갱신: Docs/02_CORE_QUICK_REFERENCE.md")


# ============================================================================
# 메인 실행부
# ============================================================================

def main() -> None:
    print("\n" + "🚀" * 40)
    print("  Qud 한글화 프로젝트 통합 도구 환경 검증 시작")
    print("🚀" * 40)

    results = [
        verify_code(),
        verify_localization(),
        verify_build()
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
