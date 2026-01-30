# 세션 종료 체크리스트

## 세션 종료 시 반드시 수행

### 1. 테스트 확인
```bash
python3 tools/run_regression.py --quiet
```
실패 시 수정 완료 후 진행.

### 2. session-state.md 업데이트
```
# 다음 항목 포함:
- 이번 세션에서 완료한 작업
- 다음 액션 (구체적으로)
- 미해결 이슈
```

### 3. 커밋
```bash
git add .
git commit -m "type: 설명"
```
커밋 타입: feat, fix, refactor, docs, test, chore

### 4. code-context 업데이트 (해당 시)
C# 핵심 파일을 수정한 경우 `.claude/code-context/` YAML에 history 추가.

## 절대 금지
- 커밋 없이 세션 종료
- session-state.md 업데이트 없이 종료
- 테스트 실패 상태로 종료
