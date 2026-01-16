---
description: 한글화 프로젝트 작업 시작 전 필수 문서 읽기 및 상태 확인
---

# 한글화 작업 시작하기

이 워크플로우는 모든 한글화 작업 전에 실행해야 합니다.

## 1. 필수 문서 읽기 (순서대로)

// turbo
```bash
cat Docs/00_PRINCIPLES.md
```
> 대원칙 문서 - 전체 읽기 필수

// turbo
```bash
cat Docs/02_TODO.md
```
> 현재 진행 중인 작업 확인

// turbo
```bash
cat Docs/04_ERROR_LOG.md
```
> 알려진 이슈 및 해결 방법 확인

## 2. 프로젝트 상태 검증

// turbo
```bash
python3 tools/project_tool.py
```
> 코드 및 JSON 무결성 확인

## 3. 작업 선택

`02_TODO.md`에서 진행할 작업 선택 후:
- 해당 항목 상태를 `[ ]` → `[/]`로 변경
- 시작일 기록

## 4. 상세 가이드 참조 (필요 시)

작업 유형에 따라 `01_DEVELOPMENT_GUIDE.md`의 해당 Part 참조:
- 새 화면 번역: Part L (AI 에이전트 가이드)
- API 참조: Part C
- 스타일 가이드: Part H
- QA: Part I

## 5. 작업 완료 후

1. `project_tool.py` 검증
2. 게임 내 테스트
3. `02_TODO.md` 상태 업데이트 (`[/]` → `[x]`)
4. 에러 발생 시 `04_ERROR_LOG.md` 기록
