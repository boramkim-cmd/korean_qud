# Issue Resolution Report: Dictionary Duplicate Key Bug
**Date**: 2026-01-25
**Session Duration**: ~30 minutes (발견부터 해결까지)
**Issue Count**: 1
**Resolution Status**: CLEAR
**Severity**: Critical (전체 번역 시스템 작동 불가)

---

## 1. 이슈 요약

### 증상
- 게임 내 모든 번역이 작동하지 않음 (아이템명, UI 등)
- 오브젝트 이름이 영어로만 표시됨

### 에러 로그
```
[QudKR-Objects] GetFor_Postfix error: The type initializer for 'QudKorean.Objects.ObjectTranslator' threw an exception.
```

### 영향 범위
- `ObjectTranslator` 클래스 전체 로드 실패
- 모든 오브젝트 번역 기능 비활성화
- 연쇄적으로 다른 번역 시스템에도 영향

---

## 2. 근본 원인 분석

### 문제의 커밋
- **Commit**: `a3651bf`
- **날짜**: 2026-01-25 11:24
- **메시지**: "fix: ObjectTranslator 정규식 버그 수정 및 접두사/명사 확장"

### 직접 원인
`ObjectTranslator.cs`의 `_descriptivePrefixes` 딕셔너리에 **중복 키 3개** 추가됨:

| 위치 | 중복 키 | 원본 위치 |
|------|---------|-----------|
| 223번 줄 | `"worn"` | 147번 줄과 중복 |
| 229번 줄 | `"polished"` | 214번 줄과 중복 |
| 235번 줄 | `"weird"` | 149번 줄과 중복 |

### C# 동작 원리
```csharp
// C# Dictionary 초기화 시 중복 키가 있으면 ArgumentException 발생
private static readonly Dictionary<string, string> _dict = new()
{
    { "worn", "낡은" },      // 147번 줄 (원본)
    // ... 76줄 후 ...
    { "worn", "낡은" },      // 223번 줄 (중복!) → ArgumentException
};
```

### 연쇄 실패 과정
```
1. 모드 로드 시 ObjectTranslator 클래스 초기화 시도
   ↓
2. static readonly 필드 _descriptivePrefixes 초기화
   ↓
3. Dictionary에 중복 키 발견 → ArgumentException
   ↓
4. static 초기화 실패 → TypeInitializationException
   ↓
5. ObjectTranslator 클래스 전체 사용 불가
   ↓
6. DisplayNamePatch에서 ObjectTranslator 호출 시 예외
   ↓
7. 모든 오브젝트 번역 실패
```

---

## 3. 왜 검증이 늦었는가?

### 1) 배포 스크립트 성공 = 괜찮다는 착각
```bash
./deploy.sh
# 출력: "=== 배포 완료 ==="
# 하지만 이건 파일 복사 성공일 뿐, 코드 검증 아님
```

**문제점**: `deploy.sh`는 파일 복사만 수행하며, C# 문법 검증이나 런타임 검증을 하지 않음

### 2) 정적 분석 도구 부재
- 현재 검증 스크립트(`project_tool.py`)는 JSON 문법만 체크
- **C# Dictionary 중복 키 검사 없음**
- 컴파일 시점에도 잡히지 않음 (런타임에만 발견)

### 3) 대량 수정 후 미흡한 리뷰
- 커밋 `a3651bf`에서 접두사 약 50개 일괄 추가
- 기존에 있던 항목과 중복 여부 확인 없이 추가
- 200줄 이상의 Dictionary에서 눈으로 중복 찾기 어려움

### 4) 인게임 테스트 미실시
- `deploy.sh` 성공 후 게임 실행 테스트 생략
- 로그 확인 없이 다음 작업 진행

### 타임라인
```
11:24  커밋 a3651bf (버그 도입)
  ↓    deploy.sh 실행 → 성공 메시지
  ↓    게임 테스트 없이 다른 작업 진행
  ...
13:10  유저 리포트: "번역이 안 됨"
  ↓
13:12  로그 확인 → TypeInitializationException 발견
  ↓
13:15  원인 파악 (Dictionary 중복 키)
  ↓
13:18  수정 및 재배포
```

---

## 4. 해결 방법

### 수정 내용
중복된 3개 항목 삭제:

```csharp
// 삭제된 항목들 (223, 229, 235번 줄)
- { "worn", "낡은" },      // 147번 줄과 중복
- { "polished", "광택나는" }, // 214번 줄과 중복
- { "weird", "이상한" },    // 149번 줄과 중복
```

### 검증 명령어
```bash
# 배포
./deploy.sh

# 게임 재시작 후 로그 확인
grep -i "error\|exception\|Qud-KR" "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | tail -30
```

### 검증 결과
- TypeInitializationException 없음
- 오브젝트 번역 정상 작동

---

## 5. 재발 방지 대책

### 즉시 적용 (이번 세션에서 완료)

#### 1) skills.md에 주의사항 문서화
```markdown
### C# Dictionary 중복 키 금지

**증상**: TypeInitializationException으로 클래스 전체 로드 실패
**해결**: 추가 전 `grep "키이름" 파일.cs`로 중복 확인
```

#### 2) 접두사 추가 시 체크리스트
```bash
# 1. 중복 확인
grep -n "추가할키" ObjectTranslator.cs

# 2. 배포
./deploy.sh

# 3. 게임 실행 + 로그 확인 (필수!)
tail -30 "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log"
```

### 단기 개선 (다음 세션)

#### 1) 배포 스크립트에 기본 검증 추가
```bash
# deploy.sh에 추가
echo ">> C# Dictionary 중복 키 검사..."
for file in Scripts/**/*.cs; do
    # Dictionary 초기화 블록에서 중복 키 찾기
    grep -oP '\{ "\K[^"]+' "$file" | sort | uniq -d
done
```

#### 2) 자동 테스트 스크립트
```bash
# test-after-deploy.sh
#!/bin/bash
# 1. 게임 시작 (헤드리스 모드가 있다면)
# 2. 로그에서 에러 확인
# 3. 결과 리포트
```

### 장기 개선

#### 1) CI/CD 파이프라인
- GitHub Actions에서 빌드 검증
- Dictionary 중복 키 정적 분석

#### 2) 단위 테스트
```csharp
[Test]
public void ObjectTranslator_ShouldInitializeWithoutException()
{
    // static 초기화 강제
    var _ = ObjectTranslator.GetStats();
    // 예외 없으면 통과
}
```

---

## 6. 핵심 교훈

### 교훈 1: 배포 성공 ≠ 코드 정상
> `deploy.sh` 성공 메시지는 파일 복사 완료일 뿐. 런타임 검증은 별도로 필요.

### 교훈 2: 대량 수정 시 중복 체크 필수
> 100줄 이상의 Dictionary에 항목 추가할 때는 `grep`으로 기존 키 확인.

### 교훈 3: 인게임 테스트 생략 금지
> 코드 변경 후 반드시 게임 실행 + 로그 확인. "나중에 테스트" = 버그 발생.

### 교훈 4: TypeInitializationException = static 필드 점검
> 이 예외가 보이면 해당 클래스의 `static` 필드 초기화 부분부터 확인.

---

## 7. 관련 파일

| 파일 | 변경 내용 |
|------|-----------|
| `Scripts/02_Patches/20_Objects/02_20_00_ObjectTranslator.cs` | 중복 키 3개 삭제 |
| `skills.md` | 주의사항 문서 추가 |
| `Docs/Issues/03_ISSUE_20260125_DICTIONARY_DUPLICATE_KEY.md` | 이 문서 |

---

## 8. 체크리스트 (향후 유사 이슈 방지)

접두사/명사 Dictionary 수정 시:

- [ ] `grep -n "키이름" ObjectTranslator.cs` 로 중복 확인
- [ ] 적절한 카테고리(material/quality/processing/descriptive)에 추가
- [ ] 복합 접두사는 단일 접두사보다 먼저 배치
- [ ] `./deploy.sh` 실행
- [ ] 게임 재시작
- [ ] 로그에서 `TypeInitializationException` 없는지 확인
- [ ] 인게임에서 해당 아이템 표시 테스트
