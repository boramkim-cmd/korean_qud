# Claude Code 프로젝트 규칙

> **qud_korean** - Caves of Qud 한글화 프로젝트

---

## 세션 시작 시 필수 작업

### 1. 컨텍스트 파일 읽기 (반드시!)
```
Docs/00_CONTEXT.md
```
이 파일에 현재 상태, 최근 이슈, 핵심 규칙이 있습니다.

### 2. 최근 커밋 확인
```bash
git log --oneline -5
```

### 3. 필요시 추가 문서
- `Docs/04_TODO.md` - 작업 목록
- `Docs/01_ARCHITECTURE.md` - 시스템 구조
- `skills.md` - 작업 가이드 (상세)

---

## 핵심 규칙

### 코드 변경 시
1. 수정 후 **반드시 `./deploy.sh` + 게임 테스트**
2. 테스트 후 **즉시 커밋** (나중에 하지 않기!)
3. Dictionary 수정 시 `grep -n "키" 파일.cs`로 중복 확인

### 커밋 규칙
- **매 작업 단위 완료 시** 즉시 커밋
- 대화 세션 종료 전 모든 변경사항 커밋
- "나중에 한꺼번에 커밋" 금지

### 문서 업데이트
- 코드 변경 후 `Docs/00_CONTEXT.md` 업데이트 (현재 상태, 최근 이슈)
- 버그 발생 시 `Docs/Issues/`에 리포트 작성
- 중요 변경 시 `Docs/05_CHANGELOG.md` 업데이트

---

## 위험 필드 (절대 번역 금지)

| 클래스 | 필드 | 이유 |
|--------|------|------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` 사용 |
| `ChoiceWithColorIcon` | `Id` | 선택 로직에 사용 |

---

## 주요 명령어

```bash
# 배포
./deploy.sh

# 검증
python3 tools/project_tool.py

# 로그 확인
grep -i "error\|exception" "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log" | tail -20

# 게임 내 디버그 (Ctrl+W)
kr:reload       # JSON 리로드
kr:stats        # 번역 통계
```

---

## 문서 구조

```
Docs/
├── 00_CONTEXT.md      ← 세션 시작 시 필독!
├── 01_ARCHITECTURE.md  # 시스템 구조
├── 02_DEVELOPMENT.md   # 개발 가이드
├── 03_DATA.md          # JSON 구조, 용어
├── 04_TODO.md          # 작업 목록
├── 05_CHANGELOG.md     # 변경 이력
├── 06_ERRORS.md        # 에러 기록
└── Issues/             # 이슈 리포트
```
