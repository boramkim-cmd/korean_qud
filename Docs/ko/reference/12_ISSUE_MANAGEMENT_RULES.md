# 이슈 관리 시스템 - 규칙 및 가이드라인

> **버전**: 1.0  
> **최종 업데이트**: 2026-01-19  
> **목적**: 표준화된 이슈 추적 및 문서화 시스템

---

## 개요

Caves of Qud 한국어 로컬라이제이션 프로젝트를 위한 자동화된 이슈 문서화 및 수명 주기 관리 시스템입니다.

---

## 파일 명명 규칙

### 상태 접두사

| 상태 | 접두사 | 의미 | 예시 |
|------|--------|------|------|
| 신규/활성 | 없음 | 작업 중인 이슈 | `ISSUE_20260119_STAT_TRANSLATION.md` |
| 진행중 | `WIP_` | 다중 세션 이슈, 미완료 | `WIP_ISSUE_20260119_STAT_TRANSLATION.md` |
| 해결됨 | `CLEAR_` | 완전히 해결됨 | `CLEAR_ISSUE_20260119_STAT_TRANSLATION.md` |
| 차단됨 | `BLOCKED_` | 외부 종속성 대기 | `BLOCKED_ISSUE_20260119_API_CHANGE.md` |
| 폐기됨 | `DEPRECATED_` | 더 이상 관련 없음 | `DEPRECATED_ISSUE_20260119_OLD_APPROACH.md` |

### 형식 구조

```
[STATUS_]ISSUE_YYYYMMDD_SHORT_DESCRIPTION.md

구성요소:
- [STATUS_]: 선택적 접두사 (WIP/CLEAR/BLOCKED/DEPRECATED)
- ISSUE: 모든 이슈 파일의 고정 접두사
- YYYYMMDD: 이슈 생성 날짜
- SHORT_DESCRIPTION: 밑줄로 구분된 설명 (3-5단어)
```

**예시:**
```
ISSUE_20260119_CHARACTER_CREATION_CRASH.md
WIP_ISSUE_20260119_MUTATION_DESCRIPTION_FORMAT.md
CLEAR_ISSUE_20260118_COLOR_TAG_DUPLICATION.md
BLOCKED_ISSUE_20260119_GAME_API_UPDATE.md
```

---

## 이슈 수명 주기

```
┌─────────────┐
│    생성     │ ISSUE_*.md
└──────┬──────┘
       │
       ├──→ 작업 시작
       │
┌──────▼──────┐
│   진행중    │ WIP_ISSUE_*.md
└──────┬──────┘
       │
       ├──→ 해결됨 ──────→ ┌──────────┐
       │                  │  해결됨  │ CLEAR_ISSUE_*.md
       │                  └──────────┘
       │
       ├──→ 차단됨 ───────→ ┌──────────┐
       │                   │  차단됨  │ BLOCKED_ISSUE_*.md
       │                   └──────────┘
       │
       └──→ 폐기됨 ────→ ┌────────────┐
                         │   폐기됨   │ DEPRECATED_ISSUE_*.md
                         └────────────┘
```

---

## 문서 구조 템플릿

```markdown
# Issue: [제목]

**상태**: 🟡 WIP / 🟢 CLEAR / 🔴 BLOCKED / ⚫ DEPRECATED  
**생성일**: YYYY-MM-DD  
**업데이트**: YYYY-MM-DD  
**우선순위**: Critical/High/Medium/Low  
**카테고리**: Bug/Feature/Enhancement/Documentation  
**관련 이슈**: #[issue_id], #[issue_id]

---

## 문제 설명

명확한 이슈 설명:
- 무엇이 작동하지 않는가
- 예상 동작
- 실제 동작
- 영향/심각도

## 재현 단계 (버그인 경우)

1. 첫 번째 단계
2. 두 번째 단계
3. ...

## 근본 원인 분석

이슈가 발생한 기술적 이유 설명.

## 해결 방법

### 시도한 해결책

| 시도 | 설명 | 결과 | 이유 |
|------|------|------|------|
| 1 | ... | ❌ 실패 | ... |
| 2 | ... | ✅ 성공 | ... |

### 최종 해결책

작동하는 솔루션의 상세 설명.

## 구현

### 수정된 파일

- `path/to/file1.cs` - 설명
- `path/to/file2.json` - 설명

### 코드 변경

```csharp
// 이전
[문제가 있던 코드]

// 이후
[수정된 코드]
```

## 검증

- [ ] 코드가 오류 없이 컴파일됨
- [ ] 게임이 성공적으로 실행됨
- [ ] 게임 내에서 이슈가 해결됨
- [ ] 회귀 버그가 발생하지 않음
- [ ] 문서가 업데이트됨

## 관련 이슈

상태와 함께 관련 이슈 목록:
- ✅ CLEAR_ISSUE_20260118_RELATED_ISSUE_1
- 🟡 WIP_ISSUE_20260119_RELATED_ISSUE_2

## 배운 점

발견한 주요 인사이트와 패턴.

## 다음 단계 (WIP인 경우)

- [ ] 작업 1
- [ ] 작업 2
- [ ] 작업 3
```

---

## 자동화 규칙

### 이슈 자동 생성

트리거 조건:
1. 개발 중 오류/버그 발견
2. 새 기능 요청 식별
3. 다단계 솔루션이 필요한 복잡한 문제
4. 다중 세션 작업이 필요한 이슈

### 상태 자동 업데이트

| 이벤트 | 액션 |
|--------|------|
| 이슈가 다중 세션으로 변경됨 | 이름 변경: `ISSUE_*` → `WIP_ISSUE_*` |
| 이슈가 완전히 해결됨 | 이름 변경: `[WIP_]ISSUE_*` → `CLEAR_ISSUE_*` |
| 관련 이슈가 해결됨 | "관련 이슈" 섹션 업데이트 |
| 이슈가 차단됨 | 이름 변경: `[WIP_]ISSUE_*` → `BLOCKED_ISSUE_*` |
| 이슈가 더 이상 관련 없음 | 이름 변경: `[WIP_]ISSUE_*` → `DEPRECATED_ISSUE_*` |

### 인덱스 자동 업데이트

상태 변경 후:
1. `Docs/Issues/00_INDEX.md` 업데이트
2. 날짜별 정렬 (최신 순)
3. 상태 표시기 업데이트
4. 요약 통계 생성

---

## 다른 시스템과의 통합

### 에러 로그 연결

이슈가 해결되면:
```
ERROR_LOG.md → CLEAR_ISSUE_*.md 참조
```

### 변경로그 연결

이슈가 해결되면:
```
CHANGELOG.md → CLEAR_ISSUE_*.md 링크와 함께 항목 추가
```

### Git 커밋

커밋 메시지에서 이슈 참조:
```
fix: 스탯 번역 중복 해결

Resolves ISSUE_20260119_STAT_TRANSLATION
See Docs/Issues/CLEAR_ISSUE_20260119_STAT_TRANSLATION.md
```

---

## 도구

### 새 이슈 생성
```bash
bash tools/create-issue.sh "Short Description"
```

### 이슈 상태 업데이트
```bash
bash tools/update-issue-status.sh ISSUE_20260119_NAME.md clear
bash tools/update-issue-status.sh ISSUE_20260119_NAME.md wip
bash tools/update-issue-status.sh ISSUE_20260119_NAME.md blocked
```

### 상태별 이슈 목록
```bash
bash tools/list-issues.sh          # 전체
bash tools/list-issues.sh wip      # WIP만
bash tools/list-issues.sh clear    # 해결된 것만
bash tools/list-issues.sh blocked  # 차단된 것만
```

---

## 모범 사례

### 이슈 생성 시기

✅ **생성해야 할 때**:
- 버그가 조사를 필요로 함
- 기능이 계획을 필요로 함
- 문제가 여러 파일/세션에 걸쳐 있음
- 복잡한 디버깅이 필요함
- 여러 시스템과의 조정이 필요함

❌ **생성하지 말아야 할 때**:
- 단순 오타 수정
- 한 줄 변경
- 명확한 해결책
- 이미 ERROR_LOG에 문서화됨

### 이슈 설명

✅ **좋음**:
- 구체적이고 실행 가능함
- 재현 단계 포함
- 관련 코드/문서 링크
- 명확한 성공 기준

❌ **나쁨**:
- 모호함 ("수정")
- 컨텍스트 없음
- 명확한 해결 기준 없음

### 상태 업데이트

- 상태 변경 **즉시** 업데이트
- "관련 이슈" 섹션 최신 유지
- 타임스탬프 업데이트
- CLEAR로 표시하기 전에 해결 세부사항 추가

---

## 통계 및 보고

통계 생성:
```bash
python3 tools/issue-stats.py
```

출력 포함:
- 상태별 총 이슈
- 평균 해결 시간
- 가장 일반적인 이슈 유형
- 주간 이슈 생성률

---

## 이전 시스템에서 마이그레이션

`Docs/en/reports/`의 이전 보고서는 참조용으로 보존됩니다.  
새 이슈는 `Docs/Issues/`의 표준화된 시스템을 따릅니다.
