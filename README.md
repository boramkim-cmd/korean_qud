# Caves of Qud 한글화 프로젝트

## 📁 프로젝트 구조 (최종 정리)

```
qud_korean/
├── Docs/                    📖 모든 가이드 문서 (11개)
│   ├── 00_CORE_START_HERE.md       ⭐ AI 에이전트 필독
│   ├── 01_CORE_PROJECT_INDEX.md    📚 전체 메서드 시그니처
│   ├── 02_CORE_QUICK_REFERENCE.md  🔍 빠른 참조
│   ├── ... (03~05 기타 기술/API 문서)
│   ├── 06_CORE_TOOLS_GUIDE.md      🛠️ 프로젝트 도구 가이드
│   │
│   ├── 10_LOC_WORKFLOW.md          📝 번역 작업 프로세스
│   ├── 11_LOC_GLOSSARY_GUIDE.md    📖 용어 시스템 가이드
│   ├── 13_LOC_STYLE_GUIDE.md       🎨 한글화 스타일 가이드
│   └── 14_LOC_QA_CHECKLIST.md      ✅ 품질 검증 체크리스트
│
├── tools/                   🔧 도구 모음
│   ├── *.py (14개)         🐍 Python 도구 (project_tool.py 등)
│   └── *.sh (6개)          🔨 Shell 스크립트 (deploy-mods.sh 등)
│
├── Scripts/                 💻 C# 코드 (21개)
│   ├── 00_Core/            핵심 로컬라이제이션 시스템
│   ├── 99_Utils/           유틸리티 및 헬퍼
│   └── 02_Patches/         Harmony 엔진 패치
│
├── LOCALIZATION/            📚 번역 데이터 (JSON)
│   └── glossary_*.json     # 카테고리별 용어 데이터 (10개)
│
├── Assets/                  🎮 게임 연동 에셋 (XML 등)
├── _backup/                 💾 자동 백업 폴더
└── _Docs_Archive/           📦 레거시/참고용 문서
```

## 🚀 빠른 시작 (AI 에이전트)

### 1. 작업 시작 전 (30초)
```bash
cat Docs/00_CORE_START_HERE.md
```

### 2. 메서드 확인 (10초)
```bash
cat Docs/01_CORE_PROJECT_INDEX.md | grep -A 5 "메서드명"
```

### 3. 통합 검증 (1분)
```bash
python3 tools/project_tool.py
```

## 🔧 주요 도구

### Python 도구 (tools/*.py)

#### `project_tool.py` - 통합 도구 ⭐
```bash
python3 tools/project_tool.py
```
- 코드 검증 (중복, 구문 오류)
- 번역 커버리지 확인
- 메타데이터 자동 생성
- 빠른 참조 업데이트

#### 개별 도구
```bash
python3 tools/verify_code.py              # 코드 검증
python3 tools/build_project_db.py         # 메타데이터 생성
python3 tools/check_translation_coverage.py  # 번역 확인
python3 tools/clean_json.py               # JSON 정리
```

### Shell 스크립트 (tools/*.sh)

```bash
./tools/sync.sh                # Git 동기화
./tools/deploy-mods.sh         # 모드 배포
./tools/sync-and-deploy.sh     # 동기화 + 배포
./tools/validate-mod.sh        # 모드 검증
./tools/watch-and-sync.sh      # 파일 감시
./tools/quick-save.sh          # 빠른 저장
```

## 📚 핵심 문서

1. **Docs/00_CORE_START_HERE.md** - 단 하나의 진입점
2. **Docs/01_CORE_PROJECT_INDEX.md** - 모든 메서드 시그니처
3. **Docs/02_CORE_QUICK_REFERENCE.md** - 프로젝트 구조

## ⚡ 워크플로우

```
1. cat Docs/00_CORE_START_HERE.md
   ↓
2. 메서드 확인 (01_CORE_PROJECT_INDEX.md)
   ↓
3. 코드 작성
   ↓
4. python3 tools/project_tool.py
   ↓
5. ./tools/deploy-mods.sh
```

## 🎯 핵심 규칙

### ✅ 해야 할 것
- 00_CORE_START_HERE.md 먼저 읽기
- 기존 함수 재사용 (TranslationEngine, LocalizationManager)
- project_tool.py로 검증 후 배포

### ❌ 하지 말아야 할 것
- _Legacy/ 폴더 코드 사용
- TranslationEngine 로직 중복 구현
- 검증 없이 배포

## 📊 통계

- **Scripts**: 21개 C# 파일
- **Python Tools**: 14개
- **Shell Scripts**: 6개
- **Localization**: 10개 JSON (1,492개 항목)
- **Docs**: 6개 핵심 문서

## 🔄 업데이트

프로젝트 변경 후:
```bash
python3 tools/project_tool.py
```

모든 메타데이터와 참조 문서가 자동 갱신됩니다.

---

**시작점**: `cat Docs/00_CORE_START_HERE.md`
