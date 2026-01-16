# 네임스페이스 참조 가이드

## 🎯 목적
Harmony 패치 작성 시 올바른 네임스페이스를 사용하기 위한 참조 가이드입니다.

---

## 📚 주요 클래스 네임스페이스 매핑

### UI 관련 (Qud.UI)
```csharp
using Qud.UI;

// 사용 가능한 클래스:
- MainMenuScreen
- MainMenu (Qud.UI)           // 메인 메뉴 창
- TradeScreen
- CharacterStatusScreen
- StatusScreensScreen         // 모던 통합 상태창
- SkillsAndPowersStatusScreen  // 스킬 및 권능 탭
- QuestsStatusScreen          // 퀘스트 탭
- FactionsStatusScreen        // 평판 탭
- JournalStatusScreen         // 일지 탭
- TinkeringStatusScreen        // 팅커링 탭
- MessageLogStatusScreen      // 메시지 로그 탭
- OptionsScreen               // 옵션 화면
- PlayerStatusBar             // 상단 HUD 상태바
- HighScoresScreen            // 점수판
- PickGameObjectScreen        // 객체 선택 화면
- CyberneticsTerminalScreen   // 사이버네틱스 터미널
- BookScreen                  // 책 읽기 화면
- HelpScreen                  // 도움말 화면
- WorldGenerationScreen       // 세계 생성 화면
- KeybindsScreen              // 키보드 설정 화면
- AbilityManagerScreen        // 능력 관리 화면
- InventoryAndEquipmentStatusScreen
- AskNumberScreen             // 숫자 입력 팝업
- GameSummaryScreen           // 게임 요약 화면
```

### XRL.UI (기존/공통 UI)
```csharp
using XRL.UI;

// 사용 가능한 클래스:
- Popup                       // 알림/선택 팝업
- InventoryScreen             // 인벤토리 화면
- StatusScreen                // 클래식 상태창
- CyberneticsScreen           // 모던 사이버네틱스 화면
- TinkeringScreen             // 모던 팅커링 화면
- EquipmentScreen             // 모던 장비 화면
- UITextSkin                  // 텍스트 렌더링 스킨
- Look                        // 살펴보기(Look) UI
- ConversationUI              // 대화 UI
```

### 콘솔 관련 (ConsoleLib.Console)
```csharp
using ConsoleLib.Console;

// 사용 가능한 클래스:
- ScreenBuffer                // ⚠️ 전역 텍스트 패치용
- TextConsole                 // 콘솔 제어
```

### 다이얼로그 (RedShadow.CommonDialogs)
```csharp
using RedShadow.CommonDialogs;

// 사용 가능한 클래스:
- InputDialog
- FileDialog
- ProgressDialog
- MessageDialog
- LoginDialog
```

### XRL.World 및 기타
```csharp
using XRL.World;
using XRL.World.Parts;

// 사용 가능한 클래스:
- GameObject                  // 모든 게임 객체
- Cell                        // 맵 타일
- Zone                        // 맵 영역
- Leveler                     // 레벨 처리 (XRL.World.Parts)
```

---

## ⚠️ 자주 발생하는 실수

### 1. ScreenBuffer 네임스페이스 오류

**❌ 잘못된 예:**
```csharp
using XRL.UI;  // 틀림!

[HarmonyPatch(typeof(ScreenBuffer))]
public static class ScreenBuffer_Patch { }
```

**✅ 올바른 예:**
```csharp
using ConsoleLib.Console;  // 맞음!

[HarmonyPatch(typeof(ScreenBuffer))]
public static class ScreenBuffer_Patch { }
```

### 2. MainMenu 클래스 찾기

게임 버전에 따라 다를 수 있으므로 동적 검색 사용:

```csharp
static System.Reflection.MethodBase TargetMethod()
{
    // 시도 1: XRL.UI.MainMenu
    var type1 = AccessTools.TypeByName("XRL.UI.MainMenu");
    if (type1 != null) return AccessTools.Method(type1, "Show");
    
    // 시도 2: Qud.UI.MainMenuScreen
    var type2 = AccessTools.TypeByName("Qud.UI.MainMenuScreen");
    if (type2 != null) return AccessTools.Method(type2, "Show");
    
    return null;
}
```

---

## 🔍 클래스 찾는 방법

### 방법 1: 게임 소스 검색
```bash
grep -r "class ClassName" Assets/core_source/
```

**예시:**
```bash
$ grep -r "class ScreenBuffer" Assets/core_source/
Assets/core_source/ConsoleLib.Console/ScreenBuffer.cs:public class ScreenBuffer
```

→ `ScreenBuffer`는 `ConsoleLib.Console` 네임스페이스에 있음

### 방법 2: 네임스페이스와 클래스 함께 찾기
```bash
grep -B 5 "class ClassName" /path/to/source.cs
```

출력 예시:
```csharp
namespace ConsoleLib.Console
{
    public class ScreenBuffer
```

---

---

## 📖 참고: 전체 네임스페이스 목록

### 게임 코어
- `XRL.Core`, `XRL.World`, `XRL.UI`, `XRL.Rules`, `XRL.Messages`

### UI 시스템
- `Qud.UI`, `ConsoleLib.Console`

### 유틸리티
- `Qud.API`, `HistoryKit`, `Genkit`

---

## 🚨 문제 해결 요약

- **Undefined target method**: 메서드 시그니처 불일치 → `05_CORE_DEVELOPMENT_PROCESS.md` 확인
- **Type or namespace not found**: `using` 문 누락 → 상단 매핑 표 확인
- **불확실할 때**: 항상 `Assets/core_source/` 직접 검색
