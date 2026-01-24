# Caves of Qud 한글 모드 (qud_korean) - 현재 상태

## 프로젝트 개요
- **저장소**: https://github.com/boramkim-cmd/korean_qud
- **작업 폴더**: `/Users/ben/Desktop/qud_korean`
- **모드 설치 위치**: `/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/qud_korean`

---

## 완료된 기능 ✅

### 1. 폰트 시스템
- `d2coding.bundle`: 한글 폰트 번들 (모드 루트에 위치)
- `TMPFallbackFontBundle.cs`: ControlManager.Update() 패치로 매 프레임 폰트 확인
- `TooltipFallback.cs`: 툴팁 전용 폰트 fallback 적용
- `FontManager`: 새 시스템으로 위임 (레거시 호환 유지)

### 2. 배포 자동화
- `deploy.sh`: 작업 폴더 → 모드 폴더 자동 복사
- 사용법: `./deploy.sh`

### 3. JSON 기반 로컬라이제이션
- XML 대신 JSON 형식 사용
- `LocalizationManager.cs`: JSON 파일 로드 및 관리
- `ObjectTranslator.cs`: 오브젝트(아이템/생물) 번역

---

## 모드 구조

```
qud_korean/
├── d2coding.bundle              # 한글 폰트
├── manifest.json                # 모드 메타데이터
├── deploy.sh                    # 배포 스크립트
│
├── Scripts/
│   ├── 00_Core/                 # 핵심 시스템
│   │   ├── 00_00_00_ModEntry.cs           # 모드 진입점
│   │   ├── 00_00_01_TranslationEngine.cs  # 번역 엔진
│   │   ├── 00_00_02_ScopeManager.cs       # 스코프 관리
│   │   ├── 00_00_03_LocalizationManager.cs # JSON 로더
│   │   ├── 00_00_04_TMPFallbackFontBundle.cs # 폰트 시스템 ⭐
│   │   ├── 00_00_05_GlossaryExtensions.cs # 용어집 확장
│   │   ├── 00_00_06_G.cs                  # 전역 헬퍼
│   │   └── 00_00_99_QudKREngine.cs        # 엔진 유틸리티
│   │
│   ├── 02_Patches/
│   │   ├── 00_Core/             # 코어 패치
│   │   ├── 10_UI/               # UI 패치
│   │   │   ├── 02_10_00_GlobalUI.cs
│   │   │   ├── 02_10_01_Options.cs
│   │   │   ├── 02_10_02_Tooltip.cs
│   │   │   ├── 02_10_10_CharacterCreation.cs
│   │   │   ├── 02_10_17_TooltipFallback.cs  # 툴팁 폰트 ⭐
│   │   │   └── ...
│   │   └── 20_Objects/          # 오브젝트 패치
│   │       ├── 02_20_00_ObjectTranslator.cs
│   │       ├── 02_20_01_DisplayNamePatch.cs # 이름 번역 ⭐
│   │       └── 02_20_02_DescriptionPatch.cs
│   │
│   └── 99_Utils/                # 유틸리티
│       ├── 99_00_01_TranslationUtils.cs
│       ├── 99_00_02_ChargenTranslationUtils.cs
│       ├── 99_00_03_StructureTranslator.cs
│       └── 99_99_TorchTest.cs   # 테스트용 패치
│
├── LOCALIZATION/                # JSON 번역 데이터
│   ├── CHARGEN/                 # 캐릭터 생성
│   │   ├── modes.json
│   │   ├── stats.json
│   │   ├── ui.json
│   │   ├── locations.json
│   │   ├── factions.json
│   │   ├── GENOTYPES/
│   │   ├── SUBTYPES/
│   │   └── PRESETS/
│   ├── GAMEPLAY/                # 게임플레이
│   │   ├── skills.json
│   │   └── cybernetics.json
│   ├── OBJECTS/                 # 오브젝트 번역
│   │   ├── items/
│   │   │   └── test.json        # Torch → 횃불 ⭐
│   │   └── creatures/
│   └── UI/                      # UI 텍스트
│       ├── common.json
│       ├── options.json
│       ├── terms.json
│       └── display.json
│
└── StreamingAssets/             # 게임 에셋 오버라이드
```

---

## 번역 데이터 형식

### OBJECTS (아이템/생물)
```json
{
  "Torch": {
    "names": {
      "torch": "횃불"
    },
    "description": "A length of brinestalk...",
    "description_ko": "브라인스톡 막대기가..."
  }
}
```

### UI/CHARGEN
```json
{
  "New Game": "새로 시작",
  "Continue": "이어하기"
}
```

---

## 작업 흐름

### 코드 수정 후 테스트
```bash
cd /Users/ben/Desktop/qud_korean
# 코드 수정...
./deploy.sh          # 모드 폴더에 배포
# 게임 실행하여 테스트
```

### Git 동기화
```bash
git add .
git commit -m "변경 내용"
git push origin main
```

---

## 남은 작업 (TODO)

### 높은 우선순위
- [ ] 더 많은 아이템 번역 추가 (LOCALIZATION/OBJECTS/items/)
- [ ] 생물 번역 추가 (LOCALIZATION/OBJECTS/creatures/)
- [ ] 캐릭터 생성 UI 번역 완성

### 중간 우선순위
- [ ] 스킬/능력 번역
- [ ] 대화 번역
- [ ] 퀘스트 번역

### 낮은 우선순위
- [ ] 책 번역
- [ ] 히스토리/로어 번역

---

## 참고 사항

### 테스트된 기능
- ✅ 폰트 표시 (한글 깨짐 없음)
- ✅ 인벤토리 아이템 이름 번역
- ✅ 툴팁 한글 표시
- ✅ 메인 메뉴 번역

### korean-test 모드
- 위치: `Mods/_korean-test-disabled/` (비활성화됨)
- 참고용으로 보관 중 (폰트 시스템 원본)
