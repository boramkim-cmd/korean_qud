# Caves of Qud 한글화 API 참조 가이드

이 문서는 한글화 패치 작성 시 참조할 수 있는 주요 클래스, 네임스페이스 및 메서드 목록을 정리합니다.

---

## 1. UI 핵심 시스템

### [ConsoleLib.Console] ScreenBuffer
**용도:** 전역 텍스트 렌더링 시스템
*   `Write` (가장 중요)
    *   **정확한 시그니처:** `void Write(string s, bool processMarkup, bool HFlip, bool VFlip, List<string> imposters, int maxCharsWritten)`
    *   **패치 어트리뷰트:** `[HarmonyPatch("Write", new System.Type[] { typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(System.Collections.Generic.List<string>), typeof(int) })]`
*   `WriteBlockWithNewlines`
    *   **패치 어트리뷰트:** `[HarmonyPatch("WriteBlockWithNewlines", new[] { typeof(string), typeof(int), typeof(int), typeof(bool) })]`

### [XRL.UI] Popup
**용도:** 모든 알림 및 선택 팝업
*   `Show` (기본 메시지)
    *   **시그니처:** `static void Show(string Text, bool CopyToHistory, bool bQuiet)`
*   `ShowOptionPicker` (선택지)
    *   **시그니처:** `static int ShowOptionPicker(string Title, string[] Options, ...)`

### [XRL.UI] UITextSkin
**용도:** 모던 UI 텍스트 관리
*   `Apply`
    *   **시그니처:** `void Apply()` (파라미터 없음)
    *   **번역 방법:** `__instance.text` 값을 직접 수정

---

## 2. 주요 게임 화면 (Qud.UI)

### MainMenuScreen (또는 XRL.UI.MainMenu)
*   **Show**: 화면 진입 시점 (`void Show()`)
*   **UpdateMenuBars**: 하단 버튼 바 갱신 (`void UpdateMenuBars()`)

### InventoryAndEquipmentStatusScreen
*   **UpdateViewFromData**: 화면 데이터 갱신 (`void UpdateViewFromData()`)

### CharacterStatusScreen
*   **GetTabString**: 탭 이름 반환 (`string GetTabString()`)
*   **UpdateViewFromData**: 상세 정보 갱신 (`void UpdateViewFromData()`)
### TradeScreen
**용도:** 상점 거래 화면
*   `void BeforeShow()` : 화면 표시 전 준비.
*   `void UpdateTitleBars()` : 상단 텍스트(상인 이름 등) 업데이트.
*   `void UpdateTotals()` : 총 가치 및 무게 합계 업데이트.

---

## 3. 정보 제공 화면 (Qud.UI)

### JournalStatusScreen / QuestsStatusScreen
**용도:** 일지 및 퀘스트 추적
*   `string GetTabString()` : "Journal", "Quests" 등의 탭 이름.
*   `void UpdateViewFromData()` : 목록 갱신.

### FactionsStatusScreen / ReputationStatusScreen
**용도:** 세력 관계 및 평판 확인
*   `string GetTabString()` : "Reputation" 탭 이름.

### TinkeringStatusScreen / TinkeringScreen
**용도:** 아이템 제작 및 개조(팅커링)
*   `string GetTabString()` : "Tinkering" 탭 이름.

---

## 4. 기타 유틸리티

### [XRL.World] GameObject
**용도:** 플레이어, NPC, 아이템 등 모든 게임 객체
*   `string DisplayName` : 객체의 표시 이름.
*   `int Stat(string Name)` : 특정 능력치 값 확인.
*   `int GetStat(string Name).Value` : 속성 상세 값 확인.

### [Qud.UI] PlayerStatusBar
**용도:** 게임 화면 상단 HUD (HP, LVL, EXP 등)
*   `void Update()` : 매 프레임 상태바 정보 갱신.
*   `void UpdateString(StringDataType type, ...)` : 특정 데이터(HPBar, XPBar 등) 텍스트 갱신.

---

## 🛠️ 패치 작성 팁

1.  **Scope 설정 (Prefix/Postfix):**
    대부분의 Screen(`Show()`, `SetPage()`) 클래스에서 `Prefix`로 `ScopeManager.PushScope()`를 호출하고, `Postfix`로 `ScopeManager.PopScope()`를 호출하여 번역 범위를 제한하세요.

2.  **직접 텍스트 수정 (Postfix):**
    `UpdateViewFromData()`와 같이 UI 요소를 직접 건드리는 메서드에서는 `Postfix`에서 `instance.textElement.SetText()` 등을 사용하여 한글로 덮어쓰세요.

3.  **동적 대상 찾기 (TargetMethod):**
    `MainMenu`와 같이 클래스명이 변경될 가능성이 있는 경우, `AccessTools.TypeByName()`을 사용하는 `TargetMethod`를 정의하여 안전하게 패치하세요.

---

## 🔍 실제 메서드 탐색 방법

특정 클래스의 모든 메서드를 확인하려면 터미널에서 다음 명령을 사용하세요:

```bash
# 특정 클래스의 public 메서드 목록 보기
grep "public.*void" Assets/core_source/Path/To/Class.cs
```
