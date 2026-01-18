# Caves of Qud 한글화 프로젝트 - AI 지시사항
# Version: 2.1 | Updated: 2026-01-19
# 이 파일은 GitHub Copilot이 매 세션마다 자동으로 읽습니다.
# SSOT(Single Source of Truth) - AI_SESSION_START.md는 deprecated

################################################################################
# LAYER 0: LANGUAGE RULE (최우선 원칙)
################################################################################

## THINK IN ENGLISH, REPORT IN KOREAN
# - All reasoning, code, comments, documentation: ENGLISH
# - Only user-facing reports/responses: KOREAN
# - User may ask in Korean, but you MUST think and work in English
# - This prevents cognitive overhead and maintains code consistency

################################################################################
# LAYER 1: CRITICAL RULES (절대 위반 금지)
################################################################################

## 위험 필드 직접 번역 금지
# 게임 원본이 Substring(), Split() 등으로 가공하는 필드는 직접 번역하면 크래시!

| 클래스 | 필드 | 가공 방식 | 안전한 패치 지점 |
|--------|------|----------|-----------------|
| AttributeDataElement | Attribute | Substring(0,3) | AttributeSelectionControl.Updated() Postfix |
| ChoiceWithColorIcon | Id | 선택 로직 비교 | Title만 번역, Id 절대 변경 금지 |

## 번역문에 색상 태그 포함 금지
# TranslationEngine이 자동 복원하므로 이중 표시됨
# BAD:  {"{{c|u}} text": "{{c|u}} 번역문"}
# GOOD: {"{{c|u}} text": "번역문"}

## 검증 없이 배포 금지
# 반드시 python3 tools/project_tool.py 먼저 실행

################################################################################
# LAYER 2: 7대 대원칙
################################################################################

1. 문서 우선: 문서에 없으면 존재하지 않는 것
2. 추측 금지: 반드시 실제 코드에서 확인 (grep -r "키워드" Assets/core_source/)
3. 재사용 우선: 새 코드 전에 기존 코드 검색
4. 검증 필수: project_tool.py 없이 배포 금지
5. 에러 기록: 모든 이슈 -> Docs/05_ERROR_LOG.md
6. 완료 기록: 변경사항 -> Docs/04_CHANGELOG.md
7. XRL.UI + Qud.UI 양쪽 확인: 대부분의 화면이 이중 구현됨

################################################################################
# LAYER 3: 과거 Critical 에러 요약 (반복 금지!)
################################################################################

| ID | 증상 | 원인 | 해결 |
|----|------|------|------|
| ERR-008 | 캐릭터 생성에서 계급/직업 선택 후 다음 단계... | 게임 원본 코드 `AttributeSelect... | `Patch_QudAttributesModul... |
| ERR-009 | 직업/계급 선택 시 설명 앞에 불렛(`{{c|... | `CHARGEN/SUBTYPES/` JSON ... | `CombineWithLevelText()` ... |
| ERR-011 | 게임 원본 `SubtypeEntry.GetCh... | 게임 원본 `SubtypeEntry.GetCh... | `ChargenTranslationUtils.... |
| ERR-006 | `Stinger`: 변이 세부 타입(Venom... | `02_10_10_CharacterCr | `description` (핵심 한 줄) + ... |
| ERR-001 | 인벤토리 "*All" 필터 미번역 | Unknown | `InventoryAndEquipmentSta... |

# 상세 내용: Docs/05_ERROR_LOG.md 참조

################################################################################
# LAYER 4: 핵심 파일 경로
################################################################################

## Core Layer (번역 엔진)
# Scripts/00_Core/00_00_00_ModEntry.cs          - 모드 진입점
# Scripts/00_Core/00_00_01_TranslationEngine.cs - 태그 보존/복원
# Scripts/00_Core/00_00_02_ScopeManager.cs      - 화면별 스코프 관리
# Scripts/00_Core/00_00_03_LocalizationManager.cs - JSON 로드/검색

## Patch Layer (UI 패치)
# Scripts/02_Patches/10_UI/02_10_10_CharacterCreation.cs - 캐릭터 생성 12모듈

## Utils Layer
# Scripts/99_Utils/99_00_03_StructureTranslator.cs - MUTATIONS/GENOTYPES/SUBTYPES

## Data Layer (번역 JSON)
# LOCALIZATION/CHARGEN/   - 캐릭터 생성
# LOCALIZATION/GAMEPLAY/  - 게임플레이 (MUTATIONS/ 포함)
# LOCALIZATION/UI/        - 공통 UI

## 문서
# Docs/00_PRINCIPLES.md   - 상세 원칙 (필독)
# Docs/05_ERROR_LOG.md    - 에러 이력
# Docs/04_CHANGELOG.md    - 변경 이력
# Docs/06_ARCHITECTURE.md - 시스템 구조

################################################################################
# LAYER 5: 필수 명령어
################################################################################

## 작업 전 확인
# grep -r "ClassName" Assets/core_source/     # 게임 원본 확인
# grep -r "키워드" Scripts/                   # 기존 패치 확인

## 디버깅
# macOS: tail -f ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log
# Windows: %APPDATA%\..\LocalLow\Freehold Games\CavesOfQud\Player.log

## 배포
# python3 tools/project_tool.py   # 검증 (필수!)
# bash tools/sync-and-deploy.sh   # 배포

################################################################################
# LAYER 6: Harmony 패치 패턴 (필수 준수)
################################################################################

## 스코프 관리 필수 패턴:
# [HarmonyPatch(typeof(TargetClass))]
# public static class Patch_ScreenName {
#     private static bool _scopePushed = false;
#     [HarmonyPrefix] void Show_Prefix() {
#         if (!_scopePushed) {
#             ScopeManager.PushScope(LocalizationManager.GetCategory("category"));
#             _scopePushed = true;
#         }
#     }
#     [HarmonyPostfix] void Hide_Postfix() {
#         if (_scopePushed) { ScopeManager.PopScope(); _scopePushed = false; }
#     }
# }

## UI 텍스트만 변경하는 Postfix 패턴 (데이터 필드 보호):
# [HarmonyPostfix]
# static void Updated_Postfix(SomeControl __instance) {
#     // BAD:  __instance.data.Field = "한글";  // 데이터 필드 변경 금지!
#     // GOOD: __instance.textElement.text = "한글";  // UI만 변경
# }

################################################################################
# LAYER 7: 번역 API 사용법
################################################################################

## 1. 단순 번역 (현재 스코프)
# if (TranslationEngine.TryTranslate(text, out string translated))
#     element.text = translated;

## 2. 카테고리 지정 번역
# if (LocalizationManager.TryGetAnyTerm(key, out string value, "chargen_ui", "ui"))
#     element.text = value;

## 3. 구조화 데이터 (MUTATIONS, GENOTYPES, SUBTYPES)
# var data = StructureTranslator.GetTranslationData("Clairvoyance", "MUTATIONS");
# if (data != null) element.text = data.GetCombinedLongDescription();

################################################################################
# 작업 완료 체크리스트
################################################################################

# [ ] python3 tools/project_tool.py 검증 통과
# [ ] Docs/05_ERROR_LOG.md에 에러 기록 (있으면)
# [ ] Docs/04_CHANGELOG.md에 변경사항 기록
# [ ] bash tools/sync-and-deploy.sh 배포
