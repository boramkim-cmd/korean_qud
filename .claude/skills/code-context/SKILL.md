# 코드 맥락 조회/기록 프로토콜

## 언제 사용
- C# 파일 수정 전 반드시 해당 code-context 확인
- 코드 간소화/리팩토링 시도 시
- "이거 왜 이렇게 복잡하지?" 의문 시

## 조회
```bash
# 특정 파일의 맥락 확인
cat .claude/code-context/OfPatternTranslator.yaml
cat .claude/code-context/CompoundTranslator.yaml
cat .claude/code-context/ColorTagProcessor.yaml
```

## YAML 구조
```yaml
file: 파일 경로
lines: 줄 수

why: 이 파일이 존재하는 이유

do_not_simplify:
  키_이름: |
    어떤 코드가 왜 단순화하면 안 되는지 설명.
    라인 번호 포함.

depends_on: 의존하는 파일/인터페이스
depended_by: 이 파일에 의존하는 파일
history: 변경 이력
```

## 규칙
1. **수정 전**: 반드시 해당 code-context YAML 읽기
2. **do_not_simplify 항목 절대 무시 금지**
3. **수정 후**: 변경 사항을 history에 추가
4. **새 핵심 파일 추가 시**: code-context YAML 작성
