# Caves of Qud 한글화 프로젝트

## 프로젝트 구조

```
qud_korean/
├── Docs/                    문서
│   ├── MASTER.md            프로젝트 개요
│   ├── guides/              가이드
│   │   ├── 01_PRINCIPLES.md     개발 대원칙
│   │   ├── 02_ARCHITECTURE.md   아키텍처
│   │   ├── 03_TOOLS_AND_BUILD.md 도구/빌드
│   │   └── 04_DEVELOPMENT_GUIDE.md 개발 가이드
│   ├── reference/           참조
│   │   ├── 01_TODO.md           작업 추적
│   │   ├── 02_CHANGELOG.md      변경 이력
│   │   └── 03_ERROR_LOG.md      에러 로그
│   └── Issues/              이슈 추적
│
├── Scripts/                 C# 코드
│   ├── 00_Core/             핵심 시스템
│   ├── 02_Patches/          Harmony 패치
│   └── 99_Utils/            유틸리티
│
├── LOCALIZATION/            번역 데이터 (JSON)
│   ├── CHARGEN/             캐릭터 생성
│   ├── GAMEPLAY/            게임플레이
│   ├── OBJECTS/             오브젝트
│   └── UI/                  UI
│
├── tools/                   도구
│   ├── project_tool.py      통합 검증 도구
│   └── *.sh                 Shell 스크립트
│
└── Assets/                  게임 에셋 (참조용)
```

## 빠른 시작

```bash
# 1. 검증
python3 tools/project_tool.py

# 2. 배포
./deploy.sh
```

## 핵심 문서

1. **Docs/MASTER.md** - 프로젝트 현황
2. **Docs/guides/01_PRINCIPLES.md** - 개발 원칙
3. **Docs/reference/01_TODO.md** - 작업 목록

## 핵심 규칙

- 기존 함수 재사용 (TranslationEngine, LocalizationManager)
- project_tool.py로 검증 후 배포
- 검증 없이 배포 금지

---

**시작점**: `cat Docs/MASTER.md`
