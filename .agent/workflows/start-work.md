---
description: 한글화 프로젝트 작업 시작 전 필수 문서 읽기 및 상태 확인
---

# 한글화 작업 시작하기

이 워크플로우는 모든 한글화 작업 전에 실행해야 합니다.

## 1. 필수 문서 읽기

```bash
cat Docs/MASTER.md
```
> 프로젝트 현황 확인

```bash
cat Docs/guides/01_PRINCIPLES.md
```
> 대원칙 문서

```bash
cat Docs/reference/01_TODO.md
```
> 현재 진행 중인 작업 확인

## 2. 프로젝트 상태 검증

```bash
python3 tools/project_tool.py
```
> 코드 및 JSON 무결성 확인

## 3. 작업 완료 후

```bash
python3 tools/project_tool.py
./deploy.sh
```
> 검증 후 배포

1. 게임 재시작하여 테스트
2. `01_TODO.md` 상태 업데이트 (`[/]` → `[x]`)
3. 에러 발생 시 `03_ERROR_LOG.md` 기록
