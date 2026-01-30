#!/usr/bin/env python3
"""
회귀 테스트 실행기

regression_registry.json의 핵심 케이스를 실행하여 회귀를 감지합니다.

사용법:
    python3 tools/run_regression.py                     # 전체 실행
    python3 tools/run_regression.py --pattern of_pattern  # 특정 패턴만
    python3 tools/run_regression.py --add "입력" "출력" --pattern direct  # 케이스 추가
    python3 tools/run_regression.py --list-patterns       # 패턴 목록
"""

import sys
import json
import argparse
from pathlib import Path

# test_object_translator의 번역 로직 재사용
sys.path.insert(0, str(Path(__file__).parent))
from test_object_translator import load_dictionaries, try_translate


REGISTRY_PATH = Path(__file__).parent / "regression_registry.json"


def load_registry() -> dict:
    with open(REGISTRY_PATH, 'r', encoding='utf-8') as f:
        return json.load(f)


def save_registry(data: dict):
    with open(REGISTRY_PATH, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
    # Ensure trailing newline
    with open(REGISTRY_PATH, 'a') as f:
        f.write('\n')


def run_tests(pattern_filter: str = None, verbose: bool = True) -> tuple:
    """
    레지스트리 케이스 실행.
    Returns: (passed, failed, failures_list)
    """
    registry = load_registry()
    cases = registry["cases"]

    if pattern_filter:
        cases = [c for c in cases if c["pattern"] == pattern_filter]
        if not cases:
            print(f"패턴 '{pattern_filter}'에 해당하는 케이스가 없습니다.")
            return 0, 0, []

    passed = 0
    failed = 0
    failures = []

    GREEN = '\033[92m'
    RED = '\033[91m'
    RESET = '\033[0m'
    BOLD = '\033[1m'

    if verbose:
        print(f"\n{BOLD}회귀 테스트 실행{RESET}")
        if pattern_filter:
            print(f"  패턴 필터: {pattern_filter}")
        print(f"  케이스 수: {len(cases)}")
        print(f"{'=' * 60}\n")

    for case in cases:
        input_text = case["input"]
        expected = case["expected"]
        pattern = case["pattern"]

        success, result = try_translate(input_text)

        # 다중 허용값 (또는)
        expected_options = [e.strip() for e in expected.split("또는")]
        is_pass = result in expected_options

        if is_pass:
            passed += 1
            if verbose:
                print(f"  {GREEN}✓{RESET} [{pattern}] {input_text} → {result}")
        else:
            failed += 1
            failures.append(case | {"got": result})
            if verbose:
                print(f"  {RED}✗{RESET} [{pattern}] {input_text}")
                print(f"    expected: {expected}")
                print(f"    got:      {result}")

    if verbose:
        print(f"\n{'=' * 60}")
        print(f"  Pass: {GREEN}{passed}{RESET} / Fail: {RED}{failed}{RESET} / Total: {passed + failed}")
        if failures:
            print(f"\n  {RED}{BOLD}회귀 감지!{RESET} 다음 케이스가 깨졌습니다:")
            for f in failures:
                print(f"    - [{f['pattern']}] {f['input']} (source: {f.get('source', '?')})")

    return passed, failed, failures


def add_case(input_text: str, expected: str, pattern: str, source: str = "manual"):
    """레지스트리에 케이스 추가"""
    registry = load_registry()

    # 중복 확인
    for case in registry["cases"]:
        if case["input"] == input_text:
            print(f"이미 존재하는 입력: '{input_text}'")
            return False

    registry["cases"].append({
        "pattern": pattern,
        "input": input_text,
        "expected": expected,
        "source": source
    })

    save_registry(registry)
    print(f"추가됨: [{pattern}] '{input_text}' → '{expected}'")
    return True


def list_patterns():
    """패턴 목록과 케이스 수 출력"""
    registry = load_registry()
    patterns = {}
    for case in registry["cases"]:
        p = case["pattern"]
        patterns[p] = patterns.get(p, 0) + 1

    print("\n패턴별 케이스 수:")
    for p, count in sorted(patterns.items()):
        print(f"  {p}: {count}개")
    print(f"\n총 {len(registry['cases'])}개 케이스")


def main():
    parser = argparse.ArgumentParser(description="회귀 테스트 실행기")
    parser.add_argument("--pattern", help="특정 패턴만 실행")
    parser.add_argument("--add", nargs=2, metavar=("INPUT", "EXPECTED"), help="케이스 추가")
    parser.add_argument("--list-patterns", action="store_true", help="패턴 목록")
    parser.add_argument("--json", action="store_true", help="JSON 출력")
    parser.add_argument("--quiet", action="store_true", help="요약만 출력")
    args = parser.parse_args()

    if args.list_patterns:
        list_patterns()
        return 0

    if args.add:
        if not args.pattern:
            print("--add 사용 시 --pattern 필수")
            return 1
        load_dictionaries()
        add_case(args.add[0], args.add[1], args.pattern)
        return 0

    # 테스트 실행
    load_dictionaries()
    passed, failed, failures = run_tests(
        pattern_filter=args.pattern,
        verbose=not args.json and not args.quiet
    )

    if args.json:
        print(json.dumps({
            "passed": passed,
            "failed": failed,
            "failures": failures
        }, ensure_ascii=False, indent=2))
    elif args.quiet:
        status = "PASS" if failed == 0 else "FAIL"
        print(f"{status} ({passed}/{passed + failed})")

    return 1 if failed > 0 else 0


if __name__ == "__main__":
    sys.exit(main())
