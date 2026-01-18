# 🚨 AI 에이전트 세션 시작 필수 읽기

> **이 파일을 읽지 않고 작업을 시작하지 마세요!**
> 
> 새 세션 시작 시 반드시 이 파일을 먼저 읽으세요.

---

## 📋 세션 시작 체크리스트

```bash
# 1단계: 이 파일 읽기 (완료)

# 2단계: 에러 로그 확인 (필수)
# 최근 에러와 해결된 이슈 확인
cat Docs/05_ERROR_LOG.md | head -200

# 3단계: 최근 변경사항 확인
cat Docs/04_CHANGELOG.md | head -100

# 4단계: 현재 TODO 확인
cat Docs/03_TODO.md | head -100
```

---

## 🔴 과거에 발생한 Critical 이슈 (반복 금지!)

### ERR-008: Substring 크래시 (2026-01-19)
- **원인**: `AttributeDataElement.Attribute`를 한글로 번역 → 게임이 `Substring(0,3)` 호출 → 3글자 미만이라 크래시
- **교훈**: **게임 원본이 데이터 필드를 가공(Substring, Split 등)하는지 확인 필수!**
- **해결**: 데이터 필드 직접 번역 금지, UI 표시 시점에 Postfix 패치

### 위험 필드 목록 (절대 직접 번역 금지)
| 클래스 | 필드 | 가공 방식 |
|--------|------|----------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` |
| `ChoiceWithColorIcon` | `Id` | 선택 로직 비교 |

---

## ⚠️ 작업 전 필수 확인사항

1. **새 패치 작성 시**: 게임 원본 소스(`Assets/core_source/`)에서 해당 필드가 어떻게 사용되는지 확인
2. **번역 안되는 이슈 시**: Player.log에서 에러 확인 (`~/Library/Logs/Freehold Games/CavesOfQud/Player.log`)
3. **캐릭터 생성 관련 작업 시**: ERR-008 ~ ERR-011 반드시 확인

---

## 📝 세션 종료 시 필수 작업

1. 발생한 에러 → `05_ERROR_LOG.md`에 기록
2. 완료한 작업 → `04_CHANGELOG.md`에 기록
3. 배운 교훈 → 이 파일 또는 `00_PRINCIPLES.md`에 추가

---

## 🔗 핵심 문서 링크

- [00_PRINCIPLES.md](Docs/00_PRINCIPLES.md) - 7대 대원칙
- [05_ERROR_LOG.md](Docs/05_ERROR_LOG.md) - 에러 이력
- [04_CHANGELOG.md](Docs/04_CHANGELOG.md) - 변경 이력
- [06_ARCHITECTURE.md](Docs/06_ARCHITECTURE.md) - 시스템 구조

---

**마지막 업데이트**: 2026-01-19
**마지막 세션 요약**: 캐릭터 생성 Critical 버그 5개 수정 (ERR-008~011)
