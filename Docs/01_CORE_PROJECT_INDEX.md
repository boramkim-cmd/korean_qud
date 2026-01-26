# 📚 프로젝트 완전 인덱스 (자동 생성)

**생성**: 2026-01-26 23:53:47

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

### `Scripts/00_Core/00_00_04_TMPFallbackFontBundle.cs`
- **역할**: 한글 TMP 폰트 번들을 로드하고, 매 프레임 fallback 적용을 확인하여 동적 UI에도 한글이 표시되도록 함
- **Namespace**: `QudKRTranslation.Core`

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
  TMP_FontAsset GetKoreanFont()
  TMP_FontAsset GetKoreanTMPFont()
  void ApplyKoreanFont()
  void ApplyFallbackToTMPComponent(TMPro.TMP_Text txt, bool forceLog = false)
  void ApplyFallbackToAllTMPComponents()
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

## 📂 [N/A]

### `Scripts/02_Patches/10_UI/02_10_16_MessageLog.cs`
- **역할**: N/A
- **Namespace**: `QudKoreanMod.Patches`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void AddPlayerMessage_Prefix(ref string Message)
  ```

### `Scripts/02_Patches/20_Objects/V2/Core/ITranslationContext.cs`
- **역할**: 번역 컨텍스트 인터페이스 정의
- **Namespace**: `QudKorean.Objects.V2.Core`

### `Scripts/02_Patches/20_Objects/V2/Core/TranslationContext.cs`
- **역할**: 번역 컨텍스트 구현
- **Namespace**: `QudKorean.Objects.V2.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool TryGetCached(string key, out string value)
  void SetCached(string key, string value)
  void ClearCache()
  ```

### `Scripts/02_Patches/20_Objects/V2/Core/TranslationResult.cs`
- **역할**: 번역 결과 객체
- **Namespace**: `QudKorean.Objects.V2.Core`
- **공개 메서드 (Public Methods)**:
  ```csharp
  TranslationResult Hit(string translated, string handler)
  TranslationResult Miss()
  TranslationResult Partial(string partialResult, string handler)
  ```

### `Scripts/02_Patches/20_Objects/V2/Data/DictionaryCache.cs`
- **역할**: 정렬된 사전 캐시 관리
- **Namespace**: `QudKorean.Objects.V2.Data`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void MergeInto(Dictionary<string, string> target, Dictionary<string, string> source)
  ```

### `Scripts/02_Patches/20_Objects/V2/Data/ITranslationRepository.cs`
- **역할**: Repository 패턴 인터페이스 정의
- **Namespace**: `QudKorean.Objects.V2.Data`

### `Scripts/02_Patches/20_Objects/V2/Data/JsonRepository.cs`
- **역할**: JSON 파일 기반 Repository 구현
- **Namespace**: `QudKorean.Objects.V2.Data`
- **공개 메서드 (Public Methods)**:
  ```csharp
  ObjectData GetCreature(string id)
  ObjectData GetItem(string id)
  void Reload()
  string GetStats()
  void EnsureInitialized()
  ```

### `Scripts/02_Patches/20_Objects/V2/Data/ObjectData.cs`
- **역할**: 오브젝트 데이터 모델
- **Namespace**: `QudKorean.Objects.V2.Data`

### `Scripts/02_Patches/20_Objects/V2/ObjectTranslatorV2.cs`
- **역할**: ObjectTranslator V2 Public API
- **Namespace**: `QudKorean.Objects.V2`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void EnsureInitialized()
  void ReloadJson()
  void ClearCache()
  bool TryGetDisplayName(string blueprint, string originalName, out string translated)
  bool TryGetDescription(string blueprint, out string translated)
  bool TryTranslateDescriptionExact(string blueprint, string currentText, out string translated)
  bool HasTranslation(string blueprint)
  string GetStats()
  ```

### `Scripts/02_Patches/20_Objects/V2/Patterns/CorpseTranslator.cs`
- **역할**: 시체 패턴 번역기
- **Namespace**: `QudKorean.Objects.V2.Patterns`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool CanHandle(string name)
  TranslationResult Translate(string name, ITranslationContext context)
  ```

### `Scripts/02_Patches/20_Objects/V2/Patterns/FoodTranslator.cs`
- **역할**: 음식 패턴 번역기
- **Namespace**: `QudKorean.Objects.V2.Patterns`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool CanHandle(string name)
  TranslationResult Translate(string name, ITranslationContext context)
  ```

### `Scripts/02_Patches/20_Objects/V2/Patterns/IPatternTranslator.cs`
- **역할**: Strategy 패턴 인터페이스 정의
- **Namespace**: `QudKorean.Objects.V2.Patterns`

### `Scripts/02_Patches/20_Objects/V2/Patterns/OfPatternTranslator.cs`
- **역할**: "of X" 패턴 번역기
- **Namespace**: `QudKorean.Objects.V2.Patterns`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool CanHandle(string name)
  TranslationResult Translate(string name, ITranslationContext context)
  ```

### `Scripts/02_Patches/20_Objects/V2/Patterns/PartsTranslator.cs`
- **역할**: 부위 패턴 번역기
- **Namespace**: `QudKorean.Objects.V2.Patterns`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool CanHandle(string name)
  TranslationResult Translate(string name, ITranslationContext context)
  ```

### `Scripts/02_Patches/20_Objects/V2/Patterns/PatternTranslatorRegistry.cs`
- **역할**: 패턴 번역기 등록 및 관리
- **Namespace**: `QudKorean.Objects.V2.Patterns`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void Register(IPatternTranslator translator)
  TranslationResult TryTranslate(string name, ITranslationContext context)
  PatternTranslatorRegistry CreateDefault()
  ```

### `Scripts/02_Patches/20_Objects/V2/Patterns/PossessiveTranslator.cs`
- **역할**: 소유격 패턴 번역기
- **Namespace**: `QudKorean.Objects.V2.Patterns`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool CanHandle(string name)
  TranslationResult Translate(string name, ITranslationContext context)
  ```

### `Scripts/02_Patches/20_Objects/V2/Pipeline/Handlers/CacheHandler.cs`
- **역할**: 캐시 조회 핸들러
- **Namespace**: `QudKorean.Objects.V2.Pipeline.Handlers`
- **공개 메서드 (Public Methods)**:
  ```csharp
  TranslationResult Handle(ITranslationContext context)
  ```

### `Scripts/02_Patches/20_Objects/V2/Pipeline/Handlers/DirectMatchHandler.cs`
- **역할**: 직접 매칭 핸들러
- **Namespace**: `QudKorean.Objects.V2.Pipeline.Handlers`
- **공개 메서드 (Public Methods)**:
  ```csharp
  TranslationResult Handle(ITranslationContext context)
  ```

### `Scripts/02_Patches/20_Objects/V2/Pipeline/Handlers/FallbackHandler.cs`
- **역할**: 최종 폴백 핸들러
- **Namespace**: `QudKorean.Objects.V2.Pipeline.Handlers`
- **공개 메서드 (Public Methods)**:
  ```csharp
  TranslationResult Handle(ITranslationContext context)
  ```

### `Scripts/02_Patches/20_Objects/V2/Pipeline/Handlers/PatternHandler.cs`
- **역할**: 패턴 번역기 위임 핸들러
- **Namespace**: `QudKorean.Objects.V2.Pipeline.Handlers`
- **공개 메서드 (Public Methods)**:
  ```csharp
  TranslationResult Handle(ITranslationContext context)
  ```

### `Scripts/02_Patches/20_Objects/V2/Pipeline/Handlers/PrefixSuffixHandler.cs`
- **역할**: 접두사/접미사 처리 핸들러
- **Namespace**: `QudKorean.Objects.V2.Pipeline.Handlers`
- **공개 메서드 (Public Methods)**:
  ```csharp
  TranslationResult Handle(ITranslationContext context)
  ```

### `Scripts/02_Patches/20_Objects/V2/Pipeline/ITranslationHandler.cs`
- **역할**: Chain of Responsibility 핸들러 인터페이스
- **Namespace**: `QudKorean.Objects.V2.Pipeline`

### `Scripts/02_Patches/20_Objects/V2/Pipeline/TranslationPipeline.cs`
- **역할**: 번역 파이프라인 관리자
- **Namespace**: `QudKorean.Objects.V2.Pipeline`
- **공개 메서드 (Public Methods)**:
  ```csharp
  TranslationPipeline AddHandler(ITranslationHandler handler)
  TranslationResult Execute(ITranslationContext context)
  TranslationPipeline CreateDefault(ITranslationRepository repo)
  ```

### `Scripts/02_Patches/20_Objects/V2/Processing/ColorTagProcessor.cs`
- **역할**: 컬러 태그 처리 유틸리티
- **Namespace**: `QudKorean.Objects.V2.Processing`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string Strip(string text)
  string TranslatePossessivesInTags(string text, ITranslationRepository repo)
  string TranslateMaterials(string text, ITranslationRepository repo)
  string TranslateNounsOutsideTags(string text, ITranslationRepository repo)
  string RestoreFormatting(string original, string coreName, string translatedCore, string suffix, string translatedSuffix)
  ```

### `Scripts/02_Patches/20_Objects/V2/Processing/PrefixExtractor.cs`
- **역할**: 접두사 추출 및 번역 유틸리티
- **Namespace**: `QudKorean.Objects.V2.Processing`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool TryExtract(string name, ITranslationRepository repo, out string prefixKo, out string remainder)
  string TranslateInText(string text, ITranslationRepository repo)
  ```

### `Scripts/02_Patches/20_Objects/V2/Processing/SuffixExtractor.cs`
- **역할**: 접미사 추출 및 번역 유틸리티
- **Namespace**: `QudKorean.Objects.V2.Processing`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string ExtractAll(string name, out string suffixes)
  string TranslateAll(string suffixes, ITranslationRepository repo)
  string StripState(string name)
  string TranslateState(string suffix, ITranslationRepository repo)
  ```

### `Scripts/02_Patches/20_Objects/V2/Processing/TextNormalizer.cs`
- **역할**: 텍스트 정규화 유틸리티
- **Namespace**: `QudKorean.Objects.V2.Processing`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string NormalizeBlueprintId(string id)
  string NormalizeCacheKey(string originalName)
  ```

## 📂 [Patch]

### `Scripts/02_Patches/20_Objects/02_20_01_DisplayNamePatch.cs`
- **역할**: GetDisplayNameEvent.GetFor() 패치로 생물/아이템 이름 한글화
- **Namespace**: `QudKorean.Objects`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void ClearCache()
  void ReloadAndClear()
  void OnGameLoaded()
  ```

### `Scripts/02_Patches/20_Objects/02_20_02_DescriptionPatch.cs`
- **역할**: Description.GetShortDescription() 패치로 설명 한글화
- **Namespace**: `QudKorean.Objects`

## 📂 [Patches/UI]

### `Scripts/02_Patches/10_UI/02_10_17_TooltipFallback.cs`
- **역할**: 툴팁(ModelShark.Tooltip)이 표시될 때 한글 fallback 폰트를 적용
- **Namespace**: `QudKRTranslation.Patches.UI`

## 📂 [UI Patch]

### `Scripts/02_Patches/10_UI/02_10_00_GlobalUI.cs`
- **역할**: 메인 메뉴, 팝업 메시지, 네비게이션 바, 공용 버튼 등 전반적인 UI 번역을 담당합니다.
- **Namespace**: `QudKRTranslation.Patches`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void TranslateMenuData()
  bool TryGetHardcodedTranslation(string text, out string translated)
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

### `Scripts/02_Patches/10_UI/02_10_08_Status.cs`
- **역할**: 상태창(인벤토리, 장비, 캐릭터 시트 등 포함)이 열릴 때
- **Namespace**: `QudKRTranslation.Patches.UI`

### `Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs`
- **역할**: 캐릭터 생성의 모든 단계(모드, 종족, 직업, 스탯, 변이 등)의 UI와 설명을 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void ClearTranslatedCanvasCache()
  ```

### `Scripts/02_Patches/10_UI/02_10_11_WorldCreation.cs`
- **역할**: "Creating World" 화면의 진행 메시지를 번역하고 한글 폰트를 적용합니다.
- **Namespace**: `QudKRTranslation.Patches`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void DestroyOverlay()
  ```

### `Scripts/02_Patches/10_UI/02_10_12_Skills.cs`
- **역할**: SkillFactory에서 로드된 스킬/파워의 이름과 설명을 번역합니다.
- **Namespace**: `QudKRTranslation.Patches.UI`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void EnsureLoaded()
  void Reload()
  string GetSkillName(string englishName)
  string GetSkillDescription(string englishDesc)
  string GetPowerName(string englishName)
  string GetPowerDescription(string powerKey)
  bool TryGetPowerName(string englishName, out string korean)
  bool TryGetPowerDescription(string powerKey, out string korean)
  ```

### `Scripts/02_Patches/10_UI/02_10_15_EmbarkOverlay.cs`
- **역할**: 캐릭터 생성 화면 하단의 'Back', 'Next' 공통 버튼 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`

### `Scripts/02_Patches/10_UI/02_10_15_Tutorial.cs`
- **역할**: TutorialManager의 텍스트를 번역합니다.
- **Namespace**: `QudKRTranslation.Patches`
- **공개 메서드 (Public Methods)**:
  ```csharp
  bool TryTranslateTutorial(string originalText, out string translated)
  ```

## 📂 [Util]

### `Scripts/99_Utils/99_00_03_StructureTranslator.cs`
- **역할**: MUTATIONS, GENOTYPES, SUBTYPES 등의 폴더에 있는 구조화된 JSON(이름, 설명, 레벨텍스트)을 로드하고 번역을 제공합니다.
- **Namespace**: `QudKRTranslation.Utils`
- **공개 메서드 (Public Methods)**:
  ```csharp
  string GetCombinedLongDescription(string fallbackOriginal = null)
  string GetCombinedCyberneticDescription()
  void InitializeDirectory(string directoryPath)
  bool TryGetData(string englishName, out TranslationData data)
  string TranslateName(string englishName)
  string GetLongDescription(string englishName, string fallbackOriginal = null)
  List<string> TranslateLevelText(string englishName)
  ```

## 📂 [Utility]

### `Scripts/02_Patches/20_Objects/02_20_99_DebugWishes.cs`
- **역할**: kr:reload, kr:check, kr:untranslated 등 디버그 명령 제공
- **Namespace**: `QudKorean.Objects`
- **공개 메서드 (Public Methods)**:
  ```csharp
  void ReloadTranslations()
  void CheckTranslation(string blueprint)
  void ListUntranslated()
  void ShowStats()
  void ClearCache()
  void InvestigateFont()
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