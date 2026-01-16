# ⚡ 프로젝트 통합 시작 가이드 (Quick Start)

이 문서는 AI 에이전트와 인간 기여자가 프로젝트를 빠르고 정확하게 파악하기 위한 **단 하나의 진입점**입니다.

## 🎯 작업 시작 전 필수 실행 (1분)

```bash
# 1. 문서 인덱스 및 시그니처 확인
cat Docs/01_CORE_PROJECT_INDEX.md

# 2. 통합 검증 및 상태 확인
python3 tools/project_tool.py
```

## 📚 핵심 파일 (우선순위 순)

### 1. **01_CORE_PROJECT_INDEX.md** ⭐⭐⭐
- **모든 파일의 메서드 시그니처 포함**
- 파일 열지 않고 메서드 확인 가능
- 자동 생성: `python3 tools/project_tool.py`

### 2. **02_CORE_QUICK_REFERENCE.md** ⭐⭐⭐
- 프로젝트 구조 요약
- 핵심 함수 위치
- 자동 생성: `python3 tools/project_tool.py`

### 3. **project_metadata.json** ⭐⭐
- 프로그래밍 방식으로 읽을 수 있는 메타데이터
- 모든 시그니처 JSON 형식

## ⚡ 핵심 규칙 (암기!)

### ✅ 항상 해야 할 것
```
1. 01_CORE_PROJECT_INDEX.md 먼저 확인
2. 기존 메서드 재사용
3. project_tool.py 실행 후 배포
```

### ❌ 절대 금지
```
1. _Legacy/ 폴더 코드 사용
2. TranslationEngine 로직 중복
3. 검증 없이 배포
4. 파일 열기 전에 01_CORE_PROJECT_INDEX.md 확인 안 함
```

## 🔧 핵심 함수 (암기!)

```
TranslationEngine.TryTranslate()
  ├─ ExtractPrefix()      → 체크박스 자동 추출
  ├─ StripColorTags()     → {{w|text}} 제거
  └─ RestoreColorTags()   → 태그 복원

LocalizationManager
  ├─ GetCategory()        → 카테고리 딕셔너리
  └─ TryGetAnyTerm()      → 여러 카테고리 검색

ChargenTranslationUtils
  ├─ TranslateLongDescription()
  ├─ TranslateMenuOptions()
  └─ TranslateBreadcrumb()
```

## 🚀 워크플로우

```
1. cat 01_CORE_PROJECT_INDEX.md     # 메서드 확인
   ↓
2. 기존 함수 재사용 확인
   ↓
3. 코드 작성
   ↓
4. python3 tools/project_tool.py   # 검증
   ↓
5. 배포
```

```bash
# 프로젝트 변경 후 실행
python3 tools/project_tool.py
```

## 📖 로컬라이제이션 (번역 지침) ⭐⭐⭐
- **10_LOC_WORKFLOW.md**: 번역 작업 절차 및 PR 방법
- **11_LOC_GLOSSARY_GUIDE.md**: 용어 시스템(G 클래스 등) 사용 가이드
- **13_LOC_STYLE_GUIDE.md**: 한글화 스타일 및 조사 처리 규칙
- **14_LOC_QA_CHECKLIST.md**: 품질 검증 및 자가 체크리스트

## 📦 기타 기술 문서
- **03_CORE_API_REFERENCE.md**: API 상세 참조
- **04_CORE_NAMESPACE_GUIDE.md**: 네임스페이스 및 파일 컨벤션
- **05_CORE_DEVELOPMENT_PROCESS.md**: 개발 프로세스 가이드
- **06_CORE_TOOLS_GUIDE.md**: 프로젝트 도구 가이드 (스크립트 용도)

## 💡 빠른 검색

```bash
# 함수 찾기
grep -r "함수명" Scripts/ --include="*.cs"

# 메서드 시그니처 확인
cat 01_CORE_PROJECT_INDEX.md | grep -A 5 "메서드명"

# JSON 메타데이터 검색
cat project_metadata.json | jq '.scripts[] | select(.methods[].name == "TryTranslate")'
```

---

**이 파일만 읽으면 모든 작업 가능!**

**컨텍스트 상실 시**: 이 파일 + 01_CORE_PROJECT_INDEX.md 읽기
