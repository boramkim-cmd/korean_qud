# 프로젝트 도구 가이드 (Tools Guide)

이 문서는 `tools/` 폴더에 포함된 각종 자동화 스크립트의 용도와 사용법을 정의합니다. 새로운 도구를 만들기 전, 이미 유사한 기능을 수행하는 도구가 있는지 이 문서를 먼저 확인하십시오.

---

## 🚀 핵심 도구 (Core Tools)

### 1. **project_tool.py** (통합 관리자) ⭐⭐⭐
- **용도**: 프로젝트의 품질을 검증하고, AI 인덱서 및 레퍼런스 문서를 즉시 최신화합니다.
- **주요 기능**:
  - **검증**: 코드 구문 오류, 중복 함수, JSON 무결성(중복 키/빈 값) 체크
  - **문서 생성**: `01_CORE_PROJECT_INDEX.md` (전체 시그니처), `02_CORE_QUICK_REFERENCE.md` (빠른 참조) 자동 생성
  - **데이터베이스**: `project_metadata.json` 고도화
- **사용법**: `python3 tools/project_tool.py`

---

## 🧪 검증 및 QA 도구 (Validation & QA)

### 2. **check_missing_translations.py** (정밀 미번역 탐색) ⭐⭐
- **용도**: XML(Skills, Mutations, Embark 등) 및 C# 코드 내 리터럴이 용어집에 누락되었는지 전수 조사합니다.
- **사용법**: `python3 tools/check_missing_translations.py`

### 3. **check_logs_for_untranslated.py** (런타임 미번역 탐색)
- **용도**: 실제 게임 플레이 로그(`Player.log`)를 분석하여 번역되지 않은 텍스트를 찾아냅니다.
- **사용법**: `python3 tools/check_logs_for_untranslated.py`

### 4. **validate-mod.sh** (쉘 환경 검증)
- **용도**: 배포 전 터미널에서 코드를 빠르게 검증합니다.
- **사용법**: `./tools/validate-mod.sh`

### 5. **HarmonyAnalyzer** (C# 어셈블리 분석기) ⭐
- **위치**: `tools/HarmonyAnalyzer/`
- **용도**: `0Harmony.dll` 또는 기타 게임 DLL에서 공개 API 시그니처를 C# 코드 형태로 추출하여 분석합니다.
- **사용법**: `dotnet run --project tools/HarmonyAnalyzer/HarmonyAnalyzer.csproj`

---

## 🛠️ 유지보수 및 유틸리티 (Maintenance)

### 5. **fix_json_duplicates.py** / **sort_json.py**
- **용도**: JSON 파일 내의 중복 키를 제거하거나 알파벳 순으로 정렬하여 가독성과 무결성을 높입니다.

### 6. **merge_options.py**
- **용도**: `Options.xml` 텍스트를 기반으로 `OptionsData.cs` 소스 코드를 동적 생성합니다.

---

## 📝 도구 관리 원칙

1.  **중복 금지**: 새로운 기능을 추가할 때는 가능한 `project_tool.py`에 통합하거나 기존 유틸리티를 확장하십시오.
2.  **경로 일반화**: 스크립트 내부에 특정 사용자의 절대 경로를 넣지 마십시오. `Path(__file__)`를 활용해 상대 경로로 작성하십시오.
3.  **결과물 동기화**: `project_tool.py`를 실행하면 항상 `Docs/` 내의 핵심 인덱스 문서들이 최신 상태로 유지되어야 합니다.
