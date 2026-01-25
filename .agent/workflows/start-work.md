---
description: 한글화 프로젝트 작업 시작 전 필수 문서 읽기 및 상태 확인
---

# 한글화 작업 시작하기

이 워크플로우는 **모든 한글화 작업 전에** 실행해야 합니다.

## 1. 필수 문서 읽기 (반드시!)

```bash
cat Docs/00_CONTEXT.md
```
> **핵심 컨텍스트** - 현재 상태, 최근 이슈, 규칙

```bash
cat Docs/04_TODO.md
```
> 현재 진행 중인 작업 확인

```bash
git log --oneline -5
```
> 최근 커밋 확인

## 2. 필요시 참조 문서

| 문서 | 용도 |
|------|------|
| `Docs/01_ARCHITECTURE.md` | 시스템 구조, API |
| `Docs/02_DEVELOPMENT.md` | 개발 규칙, 명령어 |
| `Docs/03_DATA.md` | JSON 구조, 용어 |
| `Docs/06_ERRORS.md` | 과거 에러 및 해결책 |

## 3. 작업 흐름

```
코드/JSON 수정
    ↓
./deploy.sh (배포)
    ↓
게임 실행 + 테스트
    ↓
로그 확인 (에러 없는지)
    ↓
git commit (즉시!)
    ↓
문서 업데이트 (00_CONTEXT.md 등)
```

## 4. 작업 완료 후

### 검증 및 배포
```bash
python3 tools/project_tool.py  # 검증
./deploy.sh                     # 배포
```

### 커밋 (즉시!)
```bash
git add <files>
git commit -m "type: 설명"
```

### 문서 업데이트
- `00_CONTEXT.md`: 현재 상태, 최근 이슈 업데이트
- `04_TODO.md`: 완료 항목 체크
- 버그 발생 시: `Issues/`에 리포트 작성
