# QudKR_Translation 설치 완료

## ✅ 설치 상태

프로젝트가 게임 Mods 폴더에 성공적으로 복사되었습니다!

**위치:** `/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/QudKR_Translation`

**파일 개수:**
- C# 스크립트: 7개
- 문서: 4개
- 총 파일: 11개

---

## 🎮 게임에서 활성화하기

### 1단계: 게임 실행
Caves of Qud를 실행하세요.

### 2단계: Mods 메뉴 진입
메인 메뉴에서 **Mods** 선택

### 3단계: 모드 활성화
1. `[Data] Qud-KR Translation` 찾기
2. 체크박스 선택하여 활성화
3. **Apply** 또는 **확인** 클릭

### 4단계: 게임 재시작
모드가 적용되려면 게임을 재시작해야 합니다.

---

## 🔍 작동 확인 방법

### 로그 확인 (권장)

터미널에서 실시간 로그 확인:
```bash
tail -f "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log"
```

**확인할 메시지:**
```
=================================================
[Qud-KR Translation] 모드 초기화 시작...
[Qud-KR Translation] Version: 0.2.0
=================================================
[Qud-KR Translation] 패치 대상 검증 중...
[Qud-KR Translation]   ✓ XRL.UI.ScreenBuffer 발견
[Qud-KR Translation] Harmony PatchAll 실행 중...
[Qud-KR Translation] ✓ 패치됨: ScreenBuffer.Write
[Qud-KR Translation] ✓ 패치됨: ...
=================================================
[Qud-KR Translation] 총 X개 메서드 패치 완료
[Qud-KR Translation] 모드 로드 완료!
=================================================
```

### 게임 내 확인

1. **메인 메뉴**: 일부 텍스트가 한글로 표시되어야 함
   - "New Game" → "새 게임"
   - "Options" → "옵션"
   - "Quit" → "종료"

2. **로그 메시지**: 메인 메뉴 진입 시
   ```
   [MainMenu_Patch] Scope activated
   ```

---

## ⚠️ 문제 해결

### 모드가 목록에 없는 경우

1. **파일 확인:**
   ```bash
   ls "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/QudKR_Translation/mod_info.json"
   ```
   
2. **mod_info.json 확인:**
   - 파일이 존재하는지
   - JSON 형식이 올바른지

### 모드가 활성화되지 않는 경우

1. **의존성 확인:**
   - `[Core] Qud-KR Engine` 모드가 먼저 설치되어 있어야 함
   - 없으면 이 모드가 작동하지 않음

2. **로그 확인:**
   ```bash
   grep -i "error\|exception" "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log"
   ```

### 컴파일 에러가 발생하는 경우

**증상:** 로그에 C# 컴파일 에러 메시지

**원인:** 
- 게임 버전과 코드 불일치
- 네임스페이스 오류

**해결:**
1. 로그에서 정확한 에러 메시지 확인
2. 해당 파일 수정
3. 게임 재시작

---

## 📊 현재 번역 범위

### ✅ 구현됨
- 핵심 번역 엔진
- 범위 관리 시스템
- 메인 메뉴 (부분)

### 🚧 진행 중
- 메인 메뉴 완성
- 옵션 화면
- 인벤토리/거래
- 캐릭터 상태창

### 📝 계획됨
- 팝업 메시지
- 대화 시스템
- 퀘스트 텍스트

---

## 🔧 개발 모드

### 번역 누락 텍스트 찾기

게임 플레이 중 번역되지 않은 텍스트를 발견하면:

1. **로그 확인:**
   ```bash
   grep "발견된 텍스트" "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log"
   ```

2. **해당 텍스트를 적절한 데이터 파일에 추가:**
   - 메인 메뉴: `Scripts/01_Data/MainMenu.cs`
   - 공통: `Scripts/01_Data/Common.cs`

3. **게임 재시작**

### 새로운 화면 번역 추가

자세한 방법은 [Development.md](file:///Users/ben/Desktop/QudKR_Translation/Docs/Development.md) 참조

---

## 📞 지원

### 문서
- [개발 가이드](file:///Users/ben/Desktop/QudKR_Translation/Docs/Development.md)
- [버그 분석](file:///Users/ben/.gemini/antigravity/brain/45cec7a6-188c-4981-95fb-a8a7f17bb8f0/bug_analysis.md)
- [구현 계획](file:///Users/ben/.gemini/antigravity/brain/45cec7a6-188c-4981-95fb-a8a7f17bb8f0/implementation_plan.md)

### 버그 보고
문제 발견 시 `Docs/BugReports/` 폴더에 보고서 작성

---

## 🎉 다음 단계

1. **게임 실행 및 테스트**
2. **로그 확인** (에러 없는지)
3. **번역 확인** (메인 메뉴)
4. **피드백 제공**

문제가 없으면 Phase 2 (메인 메뉴 완성)로 진행합니다!
