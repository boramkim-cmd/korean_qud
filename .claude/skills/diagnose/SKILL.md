# 번역 진단 워크플로우

## 언제 사용
- 특정 아이템이 번역되지 않는 이유 파악
- 번역 실패 원인 분류
- 어휘 추가 위치 결정

## 도구
```bash
# 단일 진단
python3 tools/diagnose_translation.py "bronze mace"

# JSON 출력
python3 tools/diagnose_translation.py "unknown item" --json

# 일괄 진단
python3 tools/diagnose_translation.py --batch "item1" "item2" "item3"

# 조용한 모드 (스크립트용)
python3 tools/diagnose_translation.py "item" --quiet
```

## 분류 체계

| 분류 | 의미 | 해결 방법 |
|------|------|----------|
| `TRANSLATION_SUCCESS` | 정상 번역됨 | 없음 |
| `ALREADY_KOREAN` | 이미 한글 | 없음 |
| `VOCABULARY_MISSING` | 어휘 사전에 없음 | 도구가 추가 위치 안내 |
| `PATTERN_UNSUPPORTED` | 패턴 미지원 | 새 패턴 핸들러 필요 |
| `RANDOM_GENERATED` | 랜덤/고유명사 | species 또는 base_nouns에 추가 |
| `LOGIC_BUG` | 로직 버그 | 코드 수정 필요 |

## 워크플로우
1. `diagnose_translation.py`로 원인 분류
2. `VOCABULARY_MISSING` → 해당 JSON 파일에 추가
3. `PATTERN_UNSUPPORTED` → C# 패턴 번역기 확인 → `.claude/code-context/` 참조
4. 수정 후 `run_regression.py`로 회귀 확인
