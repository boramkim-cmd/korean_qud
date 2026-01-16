# 프로젝트 도구 가이드 (Tools Guide)

이 문서는 `tools/` 폴더에 포함된 각종 자동화 스크립트의 용도와 사용법을 정의합니다. 새로운 도구를 만들기 전, 이미 유사한 기능을 수행하는 도구가 있는지 이 문서를 먼저 확인하십시오.

---

## 🚀 핵심 도구 (Core Tools)

### 1. **project_tool.py** (통합 관리자) ⭐⭐⭐
- **용도**: 프로젝트의 품질을 한 번에 검증하고 필수 문서를 갱신합니다.
- **주요 기능**:
  - 코드 구문 오류 및 중복 함수 검사
  - 번역 커버리지 및 JSON 중복 키 검사
  - `project_metadata.json` 생성
  - `02_CORE_QUICK_REFERENCE.md` 갱신
- **사용법**: `python3 tools/project_tool.py`

### 2. **build_project_db.py** (AI 컨텍스트 빌더) ⭐⭐⭐
- **용도**: 프로젝트 전체 파일의 시그니처를 추출하여 AI 에이전트가 이해하기 쉬운 인덱스를 생성합니다.
- **결과물**: 
  - `project_metadata.json`: 기계 판독용 데이터베이스
  - `Docs/01_CORE_PROJECT_INDEX.md`: 인간 및 AI용 전체 인덱스
- **사용법**: `python3 tools/build_project_db.py`

---

## 🧪 검증 및 QA 도구 (Validation & QA)

### 3. **validate-mod.sh** (리눅스/맥 전용 검증)
- **용도**: 배포 전 쉘 환경에서 코드를 빠르게 검증합니다.
- **사용법**: `./tools/validate-mod.sh`

### 4. **check_logs_for_untranslated.py** (미번역 탐색)
- **용도**: 게임 실행 로그(`Player.log`)를 분석하여 번역되지 않은 영어 원문을 찾아냅니다.
- **사용법**: `python3 tools/check_logs_for_untranslated.py`

### 5. **check_xml_glossary_match.py** (용어 일치 확인)
- **용도**: XML 파일 내의 텍스트가 용어집(`glossary_*.json`)에 존재하는지 대조합니다.
- **사용법**: `python3 tools/check_xml_glossary_match.py`

---

## 🛠️ 유지보수 및 유틸리티 (Maintenance)

### 6. **fix_json_duplicates.py** / **sort_json.py**
- **용도**: JSON 파일 내의 중복 키를 자동으로 제거하거나, 키를 알파벳 순으로 정렬합니다.

### 7. **merge_options.py**
- **용도**: 설정(Options) 화면의 대역표를 기반으로 `OptionsData.cs` 소스 코드를 자동 생성합니다.

---

## 📝 도구 관리 원칙

1.  **중복 금지**: 새로운 기능을 추가할 때는 가능한 `project_tool.py`에 통합하거나 기존 유틸리티를 확장하십시오.
2.  **경로 일반화**: 스크립트 내부에 특정 사용자의 절대 경로(예: `/Users/ben/...`)를 넣지 마십시오. 항상 상대 경로를 사용하십시오.
3.  **문서화**: 새로운 유용한 도구를 추가했다면 반드시 이 가이드(`06_CORE_TOOLS_GUIDE.md`)를 갱신하십시오.
