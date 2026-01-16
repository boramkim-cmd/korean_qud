# 📚 프로젝트 완전 인덱스 (자동 생성)

**생성**: 2026-01-16 09:50:54

이 문서는 프로젝트의 모든 파일과 메서드 시그니처를 포함합니다. **새로운 기능을 만들기 전, 반드시 여기서 기존 메서드를 검색하십시오.**

================================================================================

## 📂 [Core]

### `Scripts/00_Core/00_03_LocalizationManager.cs`
- **역할**: JSON 번역 파일을 로드하고 카테고리별로 관리하며, 세분화된 카테고리 병합 기능을 제공합니다.
- **Namespace**: `QudKRTranslation.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void Initialize()
  void Reload()
  string GetTerm(string category, string key, string fallback = "")
  bool TryGetAnyTerm(string key, out string result, params string[] categories)
  bool HasTerm(string category, string key)
  ```

### `Scripts/00_Core/00_04_GlossaryLoader.cs`
- **역할**: 기존 코드가 LocalizationManager를 사용할 수 있도록 연결해줍니다.
- **Namespace**: `QudKRTranslation.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void LoadGlossary()
  string GetTerm(string category, string key, string fallback = "")
  bool HasTerm(string category, string key)
  void ReloadGlossary()
  ```

### `Scripts/00_Core/00_05_GlossaryExtensions.cs`
- **역할**: 문자열 보간으로 간단하게 용어 사용
- **Namespace**: `QudKRTranslation.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string G(this string placeholder)
  ```

### `Scripts/00_Core/00_06_G.cs`
- **역할**: 초간단 glossary 접근을 위한 헬퍼
- **Namespace**: `QudKRTranslation.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string _(string placeholder)
  ```

### `Scripts/00_Core/00_99_QudKREngine.cs`
- **역할**: 한국어 폰트 강제 적용, 조사(Josa) 처리 로직 등 엔진 레벨의 기능을 제공합니다.
- **Namespace**: `QudKRTranslation.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void ApplyKoreanFont()
  bool HasJongsung(char c)
  string ResolveJosa(string text)
  ```

### `Scripts/00_Core/00_ModEntry.cs`
- **역할**: 모드 로드 시 LocalizationManager를 초기화하고 모든 Harmony 패치를 어셈블리에서 찾아 실행합니다.
- **Namespace**: `QudKRTranslation`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void Main()
  ```

### `Scripts/00_Core/01_TranslationEngine.cs`
- **역할**: 색상 태그, 체크박스, 대소문자를 무시하고 번역을 찾아주는 핵심 로직
- **Namespace**: `QudKRTranslation`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool TryTranslate(string text, out string translated)
  bool TryTranslate(string text, out string translated, Dictionary<string, string>[] scopes)
  ```

### `Scripts/00_Core/02_ScopeManager.cs`
- **역할**: Stack 기반으로 현재 활성 번역 범위를 관리합니다.
- **Namespace**: `QudKRTranslation`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void PushScope(params Dictionary<string, string>[] scopes)
  void PopScope()
  int GetDepth()
  void ClearAll()
  bool IsScopeActive(Dictionary<string, string> targetDict)
  ```

## 📂 [Core Patch]

### `Scripts/02_Patches/Core/00_01_P_SteamGalaxy.cs`
- **역할**: 스팀 환경에서 GOG Galaxy 초기화 중 오류가 발생하는 것을 방지하기 위해 Galaxy 초기화를 건너뛰고 Steam만 초기화합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/Core/00_02_P_ScreenBuffer.cs`
- **역할**: ScreenBuffer.Write 메서드를 패치하여 모든 화면의 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

## 📂 [UI Patch]

### `Scripts/02_Patches/UI/10_00_P_GlobalUI.cs`
- **역할**: 메인 메뉴, 팝업 메시지, 네비게이션 바, 공용 버튼 등 전반적인 UI 번역을 담당합니다.
- **Namespace**: `QudKRTranslation.Patches`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void TranslateMenuData()
  ```

### `Scripts/02_Patches/UI/10_01_P_Options.cs`
- **역할**: 데이터 로딩(LoadOptionNode) 및 UI 표시(OptionsScreen) 시점을 모두 패치하여 완벽한 번역을 제공합니다.
- **Namespace**: `QudKRTranslation.Patches`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void TranslateOption(GameOption opt)
  ```

### `Scripts/02_Patches/UI/10_02_P_Tooltip.cs`
- **역할**: ModelShark Tooltip 시스템의 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/UI/10_03_P_UITextSkin.cs`
- **역할**: UITextSkin.Apply 메서드를 패치하여 TMPro 기반 UI 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/UI/10_04_P_ListScroller.cs`
- **역할**: FrameworkScroller가 프리팹(각 줄의 UI)을 설정할 때 즉시 번역을 적용합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/UI/10_07_P_Inventory.cs`
- **역할**: 인벤토리 화면의 메뉴, 카테고리, 도움말 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches.UI`

### `Scripts/02_Patches/UI/10_08_P_Status.cs`
- **역할**: 상태창(인벤토리, 장비, 캐릭터 시트 등 포함)이 열릴 때
- **Namespace**: `QudKRTranslation.Patches.UI`

### `Scripts/02_Patches/UI/10_10_P_CharacterCreation.cs`
- **역할**: 캐릭터 생성의 모든 단계(모드, 종족, 직업, 스탯, 변이 등)의 UI와 설명을 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/UI/10_15_P_EmbarkOverlay.cs`
- **역할**: 캐릭터 생성 화면 하단의 'Back', 'Next' 공통 버튼 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

## 📂 [Utils]

### `Scripts/99_Utils/ChargenTranslationUtils.cs`
- **역할**: 캐릭터 생성 화면의 다중 라인 설명 등을 TranslationEngine을 사용해 번역합니다.
- **Namespace**: `QudKRTranslation.Utils`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string TranslateLongDescription(string original, params string[] categories)
  IEnumerable<MenuOption> TranslateMenuOptions(IEnumerable<MenuOption> options)
  void TranslateBreadcrumb(UIBreadcrumb breadcrumb)
  ```

### `Scripts/99_Utils/TranslationUtils.cs`
- **역할**: UI 태그(<...>, {{...}})를 보존하고, 숫구나 제어값을 번역에서 제외합니다.
- **Namespace**: `QudKRTranslation.Utils`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool TryTranslatePreservingTags(string input, out string output, Dictionary<string, string> scope)
  bool TryTranslatePreservingTags(string input, out string output, Dictionary<string, string>[] scopes)
  bool IsControlValue(string s)
  ```