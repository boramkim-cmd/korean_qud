# 프로젝트 문서 구조

## 🎯 필수 문서 (루트 디렉토리)

### 1. **AI_START_HERE.md** ⭐⭐⭐
- **목적**: AI 에이전트가 가장 먼저 읽어야 할 파일
- **내용**: 필수 규칙, 워크플로우, 자주 사용하는 명령
- **사용 시점**: 모든 작업 시작 전

### 2. **QUICK_REFERENCE.md** ⭐⭐⭐
- **목적**: 프로젝트 전체 상태 요약 (자동 생성)
- **내용**: Scripts 구조, 핵심 함수 위치, Glossary 목록
- **갱신**: `python3 generate_quick_reference.py`
- **사용 시점**: 코드 작성 전 참조

### 3. **CODEBASE_MAP.md** ⭐⭐
- **목적**: 상세한 코드베이스 설명
- **내용**: 각 파일의 역할, 코딩 패턴, 사용 예제
- **사용 시점**: 새 기능 추가 시

### 4. **WORKFLOW.md** ⭐⭐
- **목적**: 표준 작업 절차
- **내용**: 6단계 프로세스, 체크리스트
- **사용 시점**: 코드 작성/배포 시

### 5. **README.md** / **README_KO.md**
- **목적**: 프로젝트 소개 (사용자용)
- **내용**: 설치 방법, 사용법
- **대상**: 최종 사용자

### 6. **PRD_Korean_Qud_Translation_KO.md**
- **목적**: 프로젝트 요구사항 문서
- **내용**: 기능 명세, 목표
- **대상**: 프로젝트 기획

## 🔧 도구 스크립트

### 자동화 도구
- `generate_quick_reference.py`: QUICK_REFERENCE.md 생성
- `verify_code.py`: 코드 검증 (중복, 오류 탐지)

### 번역 도구
- `check_missing.py`: XML에서 미번역 텍스트 찾기
- `check_missing_cs.py`: C# 코드에서 미번역 텍스트 찾기
- `check_translation_coverage.py`: 번역 커버리지 확인
- `clean_json.py`: JSON 중복 키 제거

### 검증 도구
- `check_json_dupes.py`: JSON 중복 키 탐지
- `check_xml_glossary_match.py`: XML vs Glossary 매칭 확인
- `check_logs_for_untranslated.py`: 게임 로그에서 미번역 찾기

## 📁 아카이브 (_Docs_Archive/)

레거시 문서 보관소:
- `AI_GUIDE_FILE_STRUCTURE.md`
- `AUTO_COMMIT_GUIDE.md`
- `CODE_REVIEW_REPORT.md`
- `POTENTIAL_ERRORS.md`
- `SYNC_STATUS.md`

**⚠️ 참고용으로만 사용, 최신 정보는 루트의 필수 문서 참조**

## 🚀 AI 에이전트 사용 가이드

### 작업 시작 시
```bash
cat AI_START_HERE.md
cat QUICK_REFERENCE.md
```

### 코드 작성 전
```bash
cat CODEBASE_MAP.md  # 상세 참조
cat WORKFLOW.md      # 절차 확인
```

### 검증
```bash
python3 verify_code.py
```

### 프로젝트 상태 갱신
```bash
python3 generate_quick_reference.py
```

## 📝 문서 우선순위

1. **AI_START_HERE.md** - 항상 먼저
2. **QUICK_REFERENCE.md** - 빠른 참조
3. **CODEBASE_MAP.md** - 상세 정보
4. **WORKFLOW.md** - 절차
5. 나머지 - 필요 시

---

**핵심**: AI_START_HERE.md와 QUICK_REFERENCE.md만 읽어도 대부분의 작업 가능!
