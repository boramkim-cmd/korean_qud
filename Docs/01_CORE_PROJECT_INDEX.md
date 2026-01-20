# 📚 프로젝트 완전 인덱스 (자동 생성)

**생성**: 2026-01-20 18:43:09

이 문서는 프로젝트의 모든 파일과 메서드 시그니처를 포함합니다. **새로운 기능을 만들기 전, 반드시 여기서 기존 메서드를 검색하십시오.**

================================================================================

## 📂 [Core]

### `Scripts/00_Core/00_00_00_ModEntry.cs`
- **역할**: 모드 로드 시 LocalizationManager를 초기화하고 모든 Harmony 패치를 어셈블리에서 찾아 실행합니다.
- **Namespace**: `QudKRTranslation`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void Main()
  ```

### `Scripts/00_Core/00_00_01_TranslationEngine.cs`
- **역할**: 색상 태그, 체크박스, 대소문자를 무시하고 번역을 찾아주는 핵심 로직
- **Namespace**: `QudKRTranslation`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool TryTranslate(string text, out string translated)
  bool TryTranslate(string text, out string translated, Dictionary<string, string>[] scopes)
  ```

### `Scripts/00_Core/00_00_02_ScopeManager.cs`
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

### `Scripts/00_Core/00_00_03_LocalizationManager.cs`
- **역할**: JSON 번역 파일을 로드하고 카테고리별로 관리하며, 세분화된 카테고리 병합 기능을 제공합니다.
- **Namespace**: `QudKRTranslation.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void Initialize()
  void Reload()
  void LoadGlossary()
  string GetModDirectory()
  string GetTerm(string category, string key, string fallback = "")
  bool TryGetAnyTerm(string key, out string result, params string[] categories)
  bool HasTerm(string category, string key)
  ```

### `Scripts/00_Core/00_00_05_GlossaryExtensions.cs`
- **역할**: 문자열 보간으로 간단하게 용어 사용
- **Namespace**: `QudKRTranslation.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string G(this string placeholder)
  ```

### `Scripts/00_Core/00_00_06_G.cs`
- **역할**: 초간단 glossary 접근을 위한 헬퍼
- **Namespace**: `QudKRTranslation.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string _(string placeholder)
  ```

### `Scripts/00_Core/00_00_99_QudKREngine.cs`
- **역할**: 한국어 폰트 강제 적용, 조사(Josa) 처리 로직 등 엔진 레벨의 기능을 제공합니다.
- **Namespace**: `QudKRTranslation.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void ApplyKoreanFont()
  void ApplyFallbackToTMPComponent(TMPro.TMP_Text txt)
  void ApplyFallbackToAllTMPComponents()
  TMP_FontAsset GetKoreanTMPFont()
  void TranslateMainMenuOptions()
  bool HasJongsung(char c)
  string ResolveJosa(string text)
  ```

## 📂 [Core Patch]

### `Scripts/02_Patches/00_Core/02_00_01_SteamGalaxy.cs`
- **역할**: 스팀 환경에서 GOG Galaxy 초기화 중 오류가 발생하는 것을 방지하기 위해 Galaxy 초기화를 건너뛰고 Steam만 초기화합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/00_Core/02_00_02_ScreenBuffer.cs`
- **역할**: ScreenBuffer.Write 메서드를 패치하여 모든 화면의 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

## 📂 [UI Patch]

### `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs`
- **역할**: 메인 메뉴, 팝업 메시지, 네비게이션 바, 공용 버튼 등 전반적인 UI 번역을 담당합니다.
- **Namespace**: `QudKRTranslation.Patches`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void TranslateMenuData()
  ```

### `Scripts/02_Patches/10_UI/02_10_01_Options.cs`
- **역할**: 데이터 로딩(LoadOptionNode) 및 UI 표시(OptionsScreen) 시점을 모두 패치하여 완벽한 번역을 제공합니다.
- **Namespace**: `QudKRTranslation.Patches`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void TranslateOption(GameOption opt)
  ```

### `Scripts/02_Patches/10_UI/02_10_02_Tooltip.cs`
- **역할**: ModelShark Tooltip 시스템의 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/10_UI/02_10_03_UITextSkin.cs`
- **역할**: UITextSkin.Apply 메서드를 패치하여 TMPro 기반 UI 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/10_UI/02_10_04_ListScroller.cs`
- **역할**: FrameworkScroller가 프리팹(각 줄의 UI)을 설정할 때 즉시 번역을 적용합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/10_UI/02_10_07_Inventory.cs`
- **역할**: 인벤토리 화면의 메뉴, 카테고리, 도움말 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches.UI`

### `Scripts/02_Patches/10_UI/02_10_08_Status.cs`
- **역할**: 상태창(인벤토리, 장비, 캐릭터 시트 등 포함)이 열릴 때
- **Namespace**: `QudKRTranslation.Patches.UI`

### `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`
- **역할**: 캐릭터 생성의 모든 단계(모드, 종족, 직업, 스탯, 변이 등)의 UI와 설명을 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/10_UI/02_10_15_EmbarkOverlay.cs`
- **역할**: 캐릭터 생성 화면 하단의 'Back', 'Next' 공통 버튼 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

## 📂 [Util]

### `Scripts/99_Utils/99_00_03_StructureTranslator.cs`
- **역할**: MUTATIONS, GENOTYPES, SUBTYPES 등의 폴더에 있는 구조화된 JSON(이름, 설명, 레벨텍스트)을 로드하고 번역을 제공합니다.
- **Namespace**: `QudKRTranslation.Utils`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string GetCombinedLongDescription(string fallbackOriginal = null)
  void InitializeDirectory(string directoryPath)
  bool TryGetData(string englishName, out TranslationData data)
  string TranslateName(string englishName)
  string GetLongDescription(string englishName, string fallbackOriginal = null)
  List<string> TranslateLevelText(string englishName)
  ```

## 📂 [Utils]

### `Scripts/99_Utils/99_00_01_TranslationUtils.cs`
- **역할**: UI 태그(<...>, {{...}})를 보존하고, 숫구나 제어값을 번역에서 제외합니다.
- **Namespace**: `QudKRTranslation.Utils`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool TryTranslatePreservingTags(string input, out string output, Dictionary<string, string> scope)
  bool TryTranslatePreservingTags(string input, out string output, Dictionary<string, string>[] scopes)
  bool IsControlValue(string s)
  ```

### `Scripts/99_Utils/99_00_02_ChargenTranslationUtils.cs`
- **역할**: 캐릭터 생성 화면의 다중 라인 설명 등을 TranslationEngine을 사용해 번역합니다.
- **Namespace**: `QudKRTranslation.Utils`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string TranslateLongDescription(string original, params string[] categories)
  IEnumerable<MenuOption> TranslateMenuOptions(IEnumerable<MenuOption> options)
  void TranslateBreadcrumb(UIBreadcrumb breadcrumb)
  ```