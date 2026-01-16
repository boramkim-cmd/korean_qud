# 프로젝트 동기화 완료 ✅

## 📊 동기화 상태

### GitHub 저장소
- **URL**: https://github.com/codekkj/qud_korean
- **최신 커밋**: `1c81126` - chore: quick save - 2026-01-15 14:59:26
- **이전 커밋**: `8368ac7` - Initial commit: Qud Korean Translation - 통합 프로젝트

### 로컬 프로젝트
- **위치**: `/Users/ben/Desktop/qud_korean`
- **상태**: ✅ 최신 (GitHub와 동기화됨)

### 게임 Mods 폴더
- **Core_QudKREngine**: ✅ 동기화 완료
- **QudKR_Translation**: ✅ 동기화 완료
- **위치**: `/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/`

---

## 📁 전체 구조

```
qud_korean/ (GitHub + 로컬)
├── Core_QudKREngine/          ✅ → Mods/Core_QudKREngine
├── Data_QudKRContent/         ✅ → Mods/QudKR_Translation
├── Assets/                    (85MB, 디컴파일된 소스)
├── Legacy/                    (구 버전 참고용)
├── sync.sh                    (자동 동기화 스크립트)
├── quick-save.sh              (빠른 저장)
└── README.md
```

---

## 🔄 향후 워크플로우

### 1. 코드 수정
```bash
# VS Code 또는 에디터에서 파일 수정
# 예: Data_QudKRContent/Scripts/01_Data/MainMenu.cs
```

### 2. 자동 동기화
```bash
cd /Users/ben/Desktop/qud_korean
./quick-save.sh
```

이 명령어 하나로:
- ✅ Git add
- ✅ Git commit (자동 메시지)
- ✅ Git pull (원격 변경사항 가져오기)
- ✅ Git push (GitHub에 업로드)

### 3. 게임 Mods 폴더 업데이트
```bash
# 자동 스크립트 (선택사항)
cp -r /Users/ben/Desktop/qud_korean/Core_QudKREngine "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/"
cp -r /Users/ben/Desktop/qud_korean/Data_QudKRContent "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/QudKR_Translation"
```

또는 수동으로 복사

---

## 🎯 주요 작업 위치

### 번역 데이터 추가
```
/Users/ben/Desktop/qud_korean/Data_QudKRContent/Scripts/01_Data/
├── Common.cs          # 공통 UI 텍스트
├── MainMenu.cs        # 메인 메뉴
├── Options/           # 옵션 화면 (추가 예정)
└── Gameplay/          # 게임플레이 화면 (추가 예정)
```

### Harmony 패치 추가
```
/Users/ben/Desktop/qud_korean/Data_QudKRContent/Scripts/02_Patches/UI/
├── MainMenu_Patch.cs
├── Options_Patch.cs   # (추가 예정)
└── ...
```

### 원본 소스 참고
```
/Users/ben/Desktop/qud_korean/Assets/core_source/
└── (약 520개 C# 파일)
```

---

## 📝 커밋 메시지 규칙

### 자동 (quick-save.sh)
```
chore: quick save - 2026-01-15 14:59:26
```

### 수동 (sync.sh)
```bash
./sync.sh "feat: 옵션 화면 번역 추가"
./sync.sh "fix: 색상 태그 처리 버그 수정"
./sync.sh "docs: 개발 가이드 업데이트"
```

---

## 🔧 문제 해결

### "변경사항이 없습니다"
→ 정상입니다. 저장할 내용이 없습니다.

### 동기화 실패
```bash
cd /Users/ben/Desktop/qud_korean
git status
git pull --rebase origin main
```

### Mods 폴더 수동 업데이트
```bash
cp -r /Users/ben/Desktop/qud_korean/Data_QudKRContent "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/QudKR_Translation"
```

---

## 📚 참고 문서

- [프로젝트 README](file:///Users/ben/Desktop/qud_korean/README.md)
- [자동 동기화 가이드](file:///Users/ben/Desktop/qud_korean/Data_QudKRContent/AUTO_SYNC.md)
- [개발 가이드](file:///Users/ben/Desktop/qud_korean/Data_QudKRContent/Docs/Development.md)
- [설치 가이드](file:///Users/ben/Desktop/qud_korean/Data_QudKRContent/INSTALLATION.md)

---

## ✨ 완료!

모든 프로젝트가 동기화되었습니다:
- ✅ GitHub 저장소
- ✅ 로컬 프로젝트
- ✅ 게임 Mods 폴더

이제 `/Users/ben/Desktop/qud_korean`에서 작업하고 `./quick-save.sh`로 저장하면 됩니다!
