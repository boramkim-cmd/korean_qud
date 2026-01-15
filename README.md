# Qud Korean Translation - 통합 프로젝트

Caves of Qud 한글화 프로젝트의 모든 리소스를 통합 관리하는 저장소입니다.

## 📁 프로젝트 구조

```
qud_korean/
├── Core_QudKREngine/          # 핵심 엔진 모드
│   └── Scripts/               # Harmony 패치 및 Josa 처리
│
├── Data_QudKRContent/         # 번역 데이터 모드 (v0.2.0)
│   ├── Scripts/
│   │   ├── 00_Core/          # 핵심 시스템
│   │   ├── 01_Data/          # 번역 데이터
│   │   └── 02_Patches/       # UI 패치
│   └── Docs/                 # 문서
│
├── Assets/                    # 게임 원본 분석 자료
│   ├── core_source/          # 디컴파일된 게임 소스 (C#, ~520 파일)
│   ├── core_ui_src/          # UI 관련 소스
│   ├── StreamingAssets/      # 게임 데이터 파일
│   └── core_source_index.md  # 소스 코드 인덱스
│
├── Legacy/                    # 구 버전 (참고용)
│   └── Data_QudKRContent_old/ # 구 번역 모드
│
└── Docs/                      # 프로젝트 문서
    ├── Development.md         # 개발 가이드
    ├── BugReports/           # 버그 보고서
    └── Solutions/            # 해결책 문서
```

## 🎯 모드 설명

### Core_QudKREngine
**역할:** 한글화의 핵심 엔진
- Harmony 패치 시스템
- 한글 Josa(조사) 처리
- 메시지 큐 번역
- 대화 시스템 번역

### Data_QudKRContent
**역할:** UI 및 게임 텍스트 번역
- 메인 메뉴, 옵션, 인벤토리 등 UI 번역
- 모듈화된 구조 (v0.2.0)
- 범위 관리 시스템

## 📚 Assets 폴더

### core_source/
디컴파일된 게임 소스 코드 (C#)
- **용도:** 게임 내부 구조 이해, 패치 대상 메서드 확인
- **파일 수:** 약 520개

### StreamingAssets/
게임 데이터 파일
- XML 파일 (ObjectBlueprints, Conversations 등)
- 원본 영문 텍스트

## 🚀 시작하기

### 1. 저장소 클론
```bash
git clone https://github.com/codekkj/qud_korean.git
cd qud_korean
```

### 2. 모드 설치
```bash
# macOS
cp -r Core_QudKREngine "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/"
cp -r Data_QudKRContent "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/QudKR_Translation"
```

### 3. 게임에서 활성화
1. Caves of Qud 실행
2. Mods 메뉴
3. 두 모드 모두 활성화 (Core → Data 순서)
4. 게임 재시작

## 🔧 개발

### 빠른 동기화
```bash
cd Data_QudKRContent
./quick-save.sh
```

### 커밋 메시지 지정
```bash
./sync.sh "feat: 새로운 기능 추가"
```

자세한 내용:
- [Translation Data 개발 가이드](Data_QudKRContent/Docs/Development.md)
- [Git 자동 동기화](Data_QudKRContent/AUTO_SYNC.md)

## 📖 문서

- [버그 분석 및 해결책](Data_QudKRContent/Docs/BugAnalysis.md)
- [구현 계획](Data_QudKRContent/Docs/ImplementationPlan.md)
- [설치 가이드](Data_QudKRContent/INSTALLATION.md)

## 🤝 기여

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'feat: Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 버전 히스토리

### v0.2.0 (2026-01-15)
- 모듈화된 구조로 완전히 재작성
- Stack 기반 범위 관리 시스템
- 버그 방지 메커니즘
- 완전한 문서화

### v0.1.0 (2026-01-14)
- 초기 번역 시스템
- 기본 UI 번역

## 📝 라이선스

MIT License

## 👥 제작자

- **Boram** - 초기 작업 및 유지보수

## 🙏 감사의 말

- Caves of Qud 개발팀
- Harmony 라이브러리
- 한글화 커뮤니티
