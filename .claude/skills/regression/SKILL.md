# 회귀 테스트 워크플로우

## 언제 사용
- 패턴 번역 코드 수정 후
- JSON 사전 변경 후
- 새 번역 케이스 추가 시

## 도구
```bash
# 전체 회귀 테스트
python3 tools/run_regression.py

# 특정 패턴만
python3 tools/run_regression.py --pattern of_pattern

# 케이스 추가
python3 tools/run_regression.py --add "입력" "예상출력" --pattern direct

# 패턴 목록
python3 tools/run_regression.py --list-patterns
```

## 패턴 분류
- `direct`: 단순 직접 번역 (mace → 메이스)
- `single_prefix`: 단일 접두사 (bronze mace → 청동 메이스)
- `multi_prefix`: 복합 접두사 (engraved bronze mace)
- `suffix_state/quantity/plus`: 접미사 패턴
- `drams`: 드램 패턴 [32 drams of water]
- `color_tag`: 컬러태그 {{w|bronze}}
- `self_ref_tag`: 자기참조 태그 {{feathered|feathered}}
- `food/parts/corpse`: 식품/부위/시체 패턴
- `possessive`: 소유격 (bear's fang)
- `of_pattern`: of 패턴 (sword of fire)
- `compound`: 복합 케이스

## 필수 규칙
1. **패턴 수정 전**: `run_regression.py` 실행하여 현재 상태 확인
2. **패턴 수정 후**: `run_regression.py` 다시 실행하여 회귀 없음 확인
3. **새 번역 추가 시**: 핵심 케이스를 `--add`로 레지스트리에 등록
4. **배포 전**: `deploy.sh`가 자동으로 pytest 게이트 실행
