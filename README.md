# Caves of Qud 한글화 프로젝트

## 📁 프로젝트 구조 (최종 정리)

```
qud_korean/
├── Docs/                    📖 모든 가이드 문서
│   ├── 00_PRINCIPLES.md            ⭐ AI 에이전트 필독 - 개발 대원칙
│   ├── 01_PROJECT_INDEX.md         📚 전체 메서드 시그니처 (자동생성)
│   ├── 02_QUICK_REFERENCE.md       🔍 빠른 참조 (자동생성)
│   ├── 03_TODO.md                  📝 작업 추적
│   ├── 04_CHANGELOG.md             📋 변경 이력
│   ├── 05_ERROR_LOG.md             ⚠️ 에러/이슈 추적
│   ├── 06_ARCHITECTURE.md          🏗️ 아키텍처 문서
│   ├── 07_WORKFLOW.md              🔄 워크플로우
│   ├── 08_STYLE_GUIDE.md           🎨 스타일 가이드
│   ├── 09_TOOLS_AND_BUILD.md       🛠️ 도구 및 빌드 가이드
│   └── 10_DEVELOPMENT_GUIDE.md     📘 상세 개발 가이드
│
├── tools/                   🔧 도구 모음
│   ├── HarmonyAnalyzer/    🔧 게임 DLL API 추출 도구 (C#)
│   ├── *.py                🐍 Python 도구 (project_tool.py 등)
│   └── *.sh                🔨 Shell 스크립트 (deploy-mods.sh 등)
│
├── Scripts/                 💻 C# 코드
│   ├── 00_Core/            핵심 로컬라이제이션 시스템
│   ├── 02_Patches/         Harmony 엔진 패치
│   └── 99_Utils/           유틸리티 및 헬퍼
│
├── LOCALIZATION/            📚 번역 데이터 (JSON)
│   ├── CHARGEN/            캐릭터 생성 관련
│   ├── GAMEPLAY/           게임플레이 관련
│   ├── UI/                 UI 관련
│   └── _DEPRECATED/        더 이상 사용하지 않는 파일
│
├── Assets/                  🎮 게임 연동 에셋 (XML 등)
├── Fonts/                   🔤 폰트 파일
└── _Legacy/                 📦 레거시/참고용 코드
```

## 🚀 빠른 시작 (AI 에이전트)

### 1. 작업 시작 전 (30초)
```bash
cat Docs/00_PRINCIPLES.md
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

#### 정밀 검증 도구
```bash
python3 tools/check_missing_translations.py  # 미번역 정밀 탐색
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

1. **Docs/00_PRINCIPLES.md** - AI 에이전트 필독 대원칙
2. **Docs/01_PROJECT_INDEX.md** - 모든 메서드 시그니처
3. **Docs/02_QUICK_REFERENCE.md** - 프로젝트 구조

## ⚡ 워크플로우

```
1. cat Docs/00_PRINCIPLES.md
   ↓
2. 메서드 확인 (01_PROJECT_INDEX.md)
   ↓
3. 코드 작성
   ↓
4. python3 tools/project_tool.py
   ↓
5. ./tools/deploy-mods.sh
```

## 🎯 핵심 규칙

### ✅ 해야 할 것
- 00_PRINCIPLES.md 먼저 읽기
- 기존 함수 재사용 (TranslationEngine, LocalizationManager)
- project_tool.py로 검증 후 배포

### ❌ 하지 말아야 할 것
- _Legacy/ 폴더 코드 사용
- TranslationEngine 로직 중복 구현
- 검증 없이 배포

## 📊 통계

- **Scripts**: 21개 C# 파일
- **Localization**: Context-based 구조 (CHARGEN/, GAMEPLAY/, UI/)
- **Docs**: 11개 문서

## 🔄 업데이트

프로젝트 변경 후:
```bash
python3 tools/project_tool.py
```

모든 메타데이터와 참조 문서가 자동 갱신됩니다.

---

**시작점**: `cat Docs/00_PRINCIPLES.md`
