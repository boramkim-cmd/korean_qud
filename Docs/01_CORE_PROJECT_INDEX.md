# 📚 프로젝트 완전 인덱스 (자동 생성)

**생성**: 2026-01-16 09:19:54

================================================================================

## 🔧 Scripts (핵심 코드)

### TranslationEngine 및 Core

#### `Scripts/00_Core/00_03_LocalizationManager.cs`
- **클래스**: LocalizationManager, SimpleJsonParser
- **주요 메서드**:
  - `void Initialize(void)`
  - `void Reload(void)`
  - `Dictionary<string, string> GetCategory(string category)`
  - `Dictionary<string, string> GetCategoryGroup(string prefix)`
  - `string GetTerm(string category, string key, string fallback = "")`

#### `Scripts/00_Core/00_04_GlossaryLoader.cs`
- **클래스**: GlossaryLoader
- **주요 메서드**:
  - `void LoadGlossary(void)`
  - `string GetTerm(string category, string key, string fallback = "")`
  - `bool HasTerm(string category, string key)`
  - `void ReloadGlossary(void)`

#### `Scripts/00_Core/00_05_GlossaryExtensions.cs`
- **클래스**: GlossaryExtensions
- **주요 메서드**:
  - `string G(this string placeholder)`

#### `Scripts/00_Core/00_06_G.cs`
- **클래스**: G
- **주요 메서드**:
  - `string _(string placeholder)`

#### `Scripts/00_Core/00_99_QudKREngine.cs`
- **클래스**: FontManager, UILoadPatch, MessageLogPatch, ArticleKillerPatch, PluralizeKillerPatch, NameOrderPatch, DescriptionPatch, KoreanTextHelper
- **주요 메서드**:
  - `void ApplyKoreanFont(void)`
  - `bool HasJongsung(char c)`
  - `string ResolveJosa(string text)`

#### `Scripts/00_Core/00_ModEntry.cs`
- **클래스**: ModEntry
- **주요 메서드**:
  - `void Main(void)`

#### `Scripts/00_Core/01_TranslationEngine.cs`
- **클래스**: TranslationEngine
- **주요 메서드**:
  - `bool TryTranslate(string text, out string translated)`
  - `bool TryTranslate(string text, out string translated, Dictionary<string, string>[] scopes)`

#### `Scripts/00_Core/02_ScopeManager.cs`
- **클래스**: ScopeManager
- **주요 메서드**:
  - `void PushScope(params Dictionary<string, string>[] scopes)`
  - `void PopScope(void)`
  - `int GetDepth(void)`
  - `void ClearAll(void)`
  - `bool IsScopeActive(Dictionary<string, string> targetDict)`

### Utils

#### `Scripts/99_Utils/ChargenTranslationUtils.cs`
- **클래스**: ChargenTranslationUtils
- **메서드**: TranslateLongDescription, TranslateMenuOptions, TranslateBreadcrumb

#### `Scripts/99_Utils/TranslationUtils.cs`
- **클래스**: TranslationUtils
- **메서드**: TryTranslatePreservingTags, TryTranslatePreservingTags, IsControlValue

## 🐍 Python 도구

### `analyze_json_conflicts.py`
- **함수**: analyze_json_conflicts

### `build_project_db.py`
- 프로젝트 메타데이터 데이터베이스 생성기
- **함수**: extract_cs_metadata, extract_py_metadata, extract_md_metadata, extract_json_metadata, build_database

### `check_json_dupes.py`
- **함수**: find_duplicates, check_file, __init__, dict_with_check

### `check_logs_for_untranslated.py`
- 실제 게임 로그에서 번역되지 않은 영문 텍스트를 찾는 스크립트
- **함수**: find_untranslated_in_logs

### `check_missing.py`
- **함수**: check_missing

### `check_missing_cs.py`
- **함수**: check_missing_cs

### `check_translation_coverage.py`
- 캐릭터 생성 화면에서 번역되지 않은 텍스트를 찾는 스크립트
- **함수**: check_glossary_coverage

### `check_xml_glossary_match.py`
- 캐릭터 생성 화면에서 실제로 사용되는 텍스트와 glossary 매칭 확인
- **함수**: check_xml_vs_glossary

### `clean_json.py`
- **함수**: clean_json, dict_with_order

### `extract_keys.py`
- **함수**: strip_tags, get_keys

### `fix_json_duplicates.py`
- JSON 중복 키 제거 도구 (개선 버전)
- **함수**: remove_duplicates, clean_all_glossaries

### `generate_quick_reference.py`
- 프로젝트 상태 자동 요약 생성기
- **함수**: scan_project_structure, generate_quick_reference, main

### `merge_options.py`
- /*
- **함수**: strip_tags, get_keys

### `project_tool.py`
- 통합 프로젝트 도구
- **함수**: verify_code, check_translation_coverage, check_json_duplicates, build_metadata, generate_quick_reference

### `sort_json.py`
- JSON 정렬 및 포맷팅 도구
- **함수**: sort_json

### `sync_glossary.py`
- 번역 파일 찾기
- **함수**: find_translation_files, replace_in_file, replacer, main

### `verify_code.py`
- 코드 검증 시스템
- **함수**: find_duplicate_functions, find_duplicate_classes, check_common_functions, verify_compilation, main

## 📖 문서

### ⭐ `00_CORE_START_HERE.md`
- **제목**: ⚡ 프로젝트 통합 시작 가이드 (Quick Start)
- **수정**: 2026-01-16 09:19

### ⭐ `01_CORE_PROJECT_INDEX.md`
- **제목**: 📚 프로젝트 완전 인덱스 (자동 생성)
- **수정**: 2026-01-16 09:17

### ⭐ `02_CORE_QUICK_REFERENCE.md`
- **제목**: 🚀 프로젝트 빠른 참조 (자동 생성)
- **수정**: 2026-01-16 09:19

### ⭐ `10_LOC_WORKFLOW.md`
- **제목**: 번역 작업 워크플로우
- **수정**: 2026-01-16 09:19

## 📚 Glossary 파일

### `glossary_chargen.json`
- **항목 수**: 121
- **카테고리**: chargen_mode, chargen_stats, chargen_ui

### `glossary_cybernetics.json`
- **항목 수**: 63
- **카테고리**: cybernetics, cybernetics_desc

### `glossary_location.json`
- **항목 수**: 23
- **카테고리**: chargen_location

### `glossary_mutations.json`
- **항목 수**: 130
- **카테고리**: mutation_base, mutation_defect, mutation_desc_base, mutation_desc_physical, mutation_mental, mutation_physical

### `glossary_options.json`
- **항목 수**: 403
- **카테고리**: options

### `glossary_pregen.json`
- **항목 수**: 34
- **카테고리**: chargen_pregen

### `glossary_proto.json`
- **항목 수**: 40
- **카테고리**: chargen_proto

### `glossary_skills.json`
- **항목 수**: 158
- **카테고리**: power_axe, power_axe_desc, power_general, power_general_desc, power_pistol, power_pistol_desc, power_tinkering, power_tinkering_desc, power_wayfaring, power_wayfaring_desc, skill, skill_desc

### `glossary_terms.json`
- **항목 수**: 39
- **카테고리**: attribute, character, faction, genotype, item, phrase, weapon, world

### `glossary_ui.json`
- **항목 수**: 159
- **카테고리**: common, inventory, status, ui

================================================================================

**이 파일은 자동 생성됩니다.**

재생성: `python3 build_project_db.py`