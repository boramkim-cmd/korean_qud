# 🔄 반복 실수 방지 패턴

> 과거에 발생했던 버그와 동일한 실수를 방지하기 위한 체크리스트

---

## 패턴 1: Dictionary 중복 키 버그 (ERR-019)

**발생일**: 2026-01-25
**증상**: 모든 오브젝트 번역 실패
**원인**: `_descriptivePrefixes`에 중복 키 3개

### 예방 체크리스트

```bash
# ObjectTranslator.cs에 항목 추가 전 필수!
grep -n "추가할키" Scripts/02_Patches/20_Objects/02_20_00_ObjectTranslator.cs

# 결과가 있으면 → 이미 존재, 추가 금지
# 결과가 없으면 → 추가 가능
```

### 자동 검증 (deploy.sh에 추가 권장)

```bash
# 중복 키 자동 감지
grep -oP '\{ "\K[^"]+(?=",)' Scripts/**/*.cs 2>/dev/null | sort | uniq -d
```

---

## 패턴 2: Static 초기화 실패

**증상**: `TypeInitializationException`
**원인**: `static readonly` 필드 초기화 중 예외

### 디버깅 순서

1. 에러 메시지에서 클래스명 확인
2. 해당 클래스의 `static` 필드 모두 검토
3. Dictionary, List 등 컬렉션 초기화 부분 집중 확인
4. 특히 중복 키, null 참조 확인

---

## 패턴 3: Python 테스트 통과 but 게임 실패

**원인**: Python 시뮬레이션 ≠ C# 실제 동작

### 검증 원칙

```
Python 테스트 = 로직 검증 (필요조건)
게임 테스트 = 실제 동작 검증 (충분조건)

Python 통과 → 게임 테스트 필수
```

### 차이점

| 항목 | Python | C# 게임 |
|------|--------|---------|
| Dictionary 초기화 | 런타임 중복 허용 | 예외 발생 |
| 정규식 | Python re | .NET Regex |
| 캐싱 | 매번 새로 | 게임 내 캐시 |
| 이벤트 순서 | 순차적 | 게임 루프 |

---

## 패턴 4: 대량 수정 후 리뷰 누락

**위험**: 50개+ 항목 일괄 추가 시 중복/오타 발생 확률 높음

### 안전한 대량 수정 절차

```
1. 작은 배치로 분할 (10-15개씩)
2. 각 배치마다:
   - grep으로 중복 확인
   - deploy.sh
   - 게임 테스트
   - 커밋
3. 다음 배치로
```

---

## 패턴 5: 세션 상태 미업데이트

**문제**: 다음 세션에서 컨텍스트 손실, 같은 작업 반복

### 세션 종료 체크리스트

```bash
# 1. session-state.md 업데이트
# - "다음 액션" 명확히 기술
# - "완료" 항목 기록
# - "주의사항" 있으면 추가

# 2. 커밋
git add .
git commit -m "chore: session state update"

# 3. (선택) 체크포인트 생성
# 대규모 작업 완료 시
cp .claude/session-state.md .claude/checkpoints/$(date +%Y-%m-%d-%H-%M).md
```

---

## 패턴 6: 색상 태그 처리 오류

**증상**: `{{w|청동}} {{w|메이스}}` (태그 중복)

### 올바른 처리

```csharp
// 1. 태그 제거 (번역 전)
var stripped = StripColorTags(text);

// 2. 번역
var translated = Translate(stripped);

// 3. 태그 복원 (번역 후)
var result = RestoreColorTags(translated, originalTags);
```

### JSON 규칙

```json
// ❌ 태그 포함
{"{{c|text}}": "{{c|번역}}"}

// ✅ 태그 제외
{"{{c|text}}": "번역"}
```

---

## 패턴 7: 소유격/of 패턴 어순 오류

**영어**: X of Y, X's Y
**한국어**: Y의 X

### 예시

| 영어 | 잘못된 번역 | 올바른 번역 |
|------|------------|------------|
| sword of fire | 검 불의 | 불의 검 |
| bear's claw | 곰's 발톱 | 곰의 발톱 |

### 구현

```csharp
// BookTitleTranslator, OfPatternHandler에서 처리
// 직접 JSON에 넣지 말고 패턴 핸들러에 위임
```

---

## 자가 진단 질문

작업 전:
- [ ] 이 작업이 danger-zones.md의 규칙을 위반하지 않는가?
- [ ] 비슷한 버그가 과거에 있었는가? (06_ERRORS.md 확인)

작업 후:
- [ ] deploy.sh 성공했는가?
- [ ] 게임에서 직접 테스트했는가?
- [ ] 로그에 에러가 없는가?
- [ ] session-state.md를 업데이트했는가?
- [ ] 커밋했는가?
