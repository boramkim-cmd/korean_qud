# 🚨 AI 에이전트 필수 문서 - 개발 대원칙

> **이 문서는 모든 작업의 시작점입니다. 반드시 전체를 읽으세요.**
>
> 📍 **문서 크기**: ~4KB | **예상 읽기 시간**: 2분 | **필수 숙지**: ⭐⭐⭐

---

## 📚 문서 시스템 구조

```
Docs/
├── 00_PRINCIPLES.md        ← 지금 읽는 문서 (필수, 먼저 읽기)
├── 01_DEVELOPMENT_GUIDE.md ← 상세 개발 가이드 (필요 시 참조)
├── 02_TODO.md              ← 작업 추적 (작업 전 확인)
├── 03_CHANGELOG.md         ← 완료 기록 (Phase 완료 시 업데이트)
└── 04_ERROR_LOG.md         ← 에러 추적 (에러 발생 시 기록/확인)
```

### 문서 읽기 순서
```
1. 00_PRINCIPLES.md (이 문서) → 필수, 전체 읽기
2. 02_TODO.md → 현재 진행 중인 작업 확인
3. 04_ERROR_LOG.md → 알려진 이슈 확인
4. 01_DEVELOPMENT_GUIDE.md → 필요한 Part만 참조
```

---

## 🎯 7대 대원칙 (반드시 암기)

```
┌─────────────────────────────────────────────────────────────┐
│ 1. 문서 우선: 문서에 없으면 존재하지 않는 것                    │
│ 2. 추측 금지: 반드시 실제 코드에서 확인 (grep 명령 사용)        │
│ 3. 재사용 우선: 새 코드 전에 기존 코드 검색                    │
│ 4. 검증 필수: project_tool.py 없이 배포 금지                  │
│ 5. 에러 기록: 모든 이슈는 04_ERROR_LOG.md에 기록              │
│ 6. 완전성 추구: 중간 상태로 두지 않고 완료까지                  │
│ 7. 상세 기록: AI도 이해할 수 있게 구체적으로 작성               │
└─────────────────────────────────────────────────────────────┘
```

---

## ⚠️ 절대 금지 사항 (NEVER DO)

| 금지 | 이유 | 올바른 방법 |
|------|------|------------|
| `_Legacy/` 폴더 사용 | 더 이상 유효하지 않음 | `Scripts/` 폴더만 사용 |
| 메서드 시그니처 추측 | Harmony 패치 실패 | `grep`으로 실제 확인 |
| 검증 없이 배포 | 런타임 에러 | `project_tool.py` 먼저 |
| 특수 태그 번역 | 게임 파손 | `%var%`, `{{tag}}` 유지 |
| XRL.UI만 확인 | Qud.UI 사용 가능 | 양쪽 네임스페이스 확인 |

---

## ✅ 작업 시작 전 체크리스트

```bash
# 1. 문서 확인 (필수)
cat Docs/00_PRINCIPLES.md     # 이 문서
cat Docs/02_TODO.md           # 진행 중 작업
cat Docs/04_ERROR_LOG.md      # 알려진 이슈

# 2. 프로젝트 상태 검증
python3 tools/project_tool.py

# 3. 작업 대상 조사 (새 기능 시)
grep -r "class ClassName" Assets/core_source/
grep -r "기능명" Scripts/02_Patches/
grep -r "키워드" LOCALIZATION/*.json
```

---

## 📝 문서 업데이트 규칙

| 상황 | 행동 |
|------|------|
| 작업 시작 시 | `02_TODO.md`에서 `[ ]` → `[/]` |
| 작업 완료 시 | `02_TODO.md`에서 `[/]` → `[x]` |
| 에러 발생 시 | `04_ERROR_LOG.md`에 즉시 기록 |
| 에러 해결 시 | `04_ERROR_LOG.md`에 해결 방법 기록 |
| Phase 완료 시 | `03_CHANGELOG.md`에 정리 |

---

## 🔧 핵심 명령어 (복사해서 사용)

```bash
# 클래스 찾기
grep -r "class ClassName" Assets/core_source/

# 메서드 시그니처 확인
grep -A 5 "void MethodName" Assets/core_source/_GameSource/*/File.cs

# 텍스트 출처 확인
grep -ri "텍스트" Assets/core_source/ Assets/StreamingAssets/Base/

# 프로젝트 검증
python3 tools/project_tool.py

# 모드 배포
./tools/deploy-mods.sh
```

---

## 📂 핵심 파일 경로

| 용도 | 경로 |
|------|------|
| 번역 엔진 | `Scripts/00_Core/00_01_TranslationEngine.cs` |
| 데이터 관리 | `Scripts/00_Core/00_03_LocalizationManager.cs` |
| 전역 UI 패치 | `Scripts/02_Patches/UI/02_10_00_GlobalUI.cs` |
| 용어집 | `LOCALIZATION/glossary_*.json` |

---

## 🎯 작업 완료 기준

작업이 "완료"로 간주되려면:

```markdown
☐ project_tool.py 검증 통과
☐ 게임 내 테스트 완료
☐ 02_TODO.md 상태 업데이트
☐ 에러 발생 시 04_ERROR_LOG.md 기록
```

---

> **다음 단계**: 구체적인 작업 내용은 `01_DEVELOPMENT_GUIDE.md`의 해당 Part 참조
