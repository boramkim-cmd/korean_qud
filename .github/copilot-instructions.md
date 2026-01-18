# Caves of Qud 한글화 프로젝트 - AI 필수 지시사항

> **이 파일은 GitHub Copilot이 매 세션마다 자동으로 읽습니다.**

---

## 🔴 절대 금지 사항 (CRITICAL)

### 1. 위험 필드 직접 번역 금지
게임 원본이 `Substring()`, `Split()` 등으로 가공하는 필드는 **절대 직접 번역하면 안됨**!

| 클래스 | 필드 | 가공 방식 | 안전한 방법 |
|--------|------|----------|------------|
| `AttributeDataElement` | `Attribute` | `Substring(0,3)` | `AttributeSelectionControl.Updated()` Postfix |
| `ChoiceWithColorIcon` | `Id` | 선택 로직 비교 | `Title`만 번역 |

**ERR-008 사건**: `attr.Attribute = "힘"` 했다가 게임이 `Substring(0,3)` 호출해서 크래시 발생!

### 2. 작업 전 필수 확인
- **새 패치 작성 전**: `Assets/core_source/`에서 게임 원본이 해당 필드를 어떻게 사용하는지 확인
- **번역 안되면**: Player.log 확인 (`~/Library/Logs/Freehold Games/CavesOfQud/Player.log`)

---

## 📚 핵심 규칙

1. **문서 우선**: 문서에 없으면 존재하지 않는 것
2. **추측 금지**: 반드시 실제 코드에서 확인 (grep 사용)
3. **재사용 우선**: 새 코드 전에 기존 코드 검색
4. **에러 기록**: 모든 이슈는 `Docs/05_ERROR_LOG.md`에 기록
5. **세션 종료 시**: 변경사항 `Docs/04_CHANGELOG.md`에 기록

---

## 📂 핵심 경로

| 용도 | 경로 |
|------|------|
| 번역 엔진 | `Scripts/00_Core/00_00_01_TranslationEngine.cs` |
| 데이터 관리 | `Scripts/00_Core/00_00_03_LocalizationManager.cs` |
| 구조화 번역 | `Scripts/99_Utils/99_00_03_StructureTranslator.cs` |
| 캐릭터 생성 패치 | `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs` |
| 에러 로그 | `Docs/05_ERROR_LOG.md` |
| 변경 이력 | `Docs/04_CHANGELOG.md` |

---

## ⚠️ 과거 Critical 이슈 (반복 금지)

- **ERR-008**: AttributeDataElement.Attribute 직접 번역 → Substring 크래시
- **ERR-009**: leveltext_ko에 불렛 누락 → 닷 표시 안됨
- **ERR-010**: Castes JSON에 영문 괄호 포함 → UI 폭 초과
- **ERR-011**: 평판 동적 생성 텍스트 → Regex 패턴 매칭 필요

---

## 🔧 작업 완료 시 필수

```bash
# 1. 모드 배포
bash tools/sync-and-deploy.sh

# 2. 에러 발생 시 기록
# Docs/05_ERROR_LOG.md에 추가

# 3. 변경사항 기록
# Docs/04_CHANGELOG.md에 추가
```

---

**마지막 업데이트**: 2026-01-19
