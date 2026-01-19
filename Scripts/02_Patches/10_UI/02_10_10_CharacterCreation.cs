/*
 * 파일명: 10_10_P_CharacterCreation.cs
 * 분류: [UI Patch] 캐릭터 생성 화면 통합 패치
 * 역할: 캐릭터 생성의 모든 단계(모드, 종족, 직업, 스탯, 변이 등)의 UI와 설명을 번역합니다.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ModelShark;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;
using XRL.CharacterBuilds.Qud.UI;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;
using Qud.UI;
using QudKRTranslation.Core;
using QudKRTranslation.Utils;

namespace QudKRTranslation.Patches
{
    // ========================================================================
    // 캐릭터 생성 화면 번역 패치
    // ========================================================================

    // ========================================================================
    // [0] 베이스 클래스 패치 (모든 화면 공통 하단 메뉴 및 브레드크럼)
    // ========================================================================
    [HarmonyPatch(typeof(AbstractBuilderModuleWindowBase))]
    public static class Patch_AbstractBuilderModuleWindowBase
    {
        [HarmonyPatch(nameof(AbstractBuilderModuleWindowBase.GetKeyLegend))]
        [HarmonyPostfix]
        static void GetKeyLegend_Postfix(ref IEnumerable<MenuOption> __result)
        {
            if (__result == null) return;
            __result = ChargenTranslationUtils.TranslateMenuOptions(__result);
        }

        [HarmonyPatch(nameof(AbstractBuilderModuleWindowBase.GetKeyMenuBar))]
        [HarmonyPostfix]
        static void GetKeyMenuBar_Postfix(ref IEnumerable<MenuOption> __result)
        {
            if (__result == null) return;
            __result = ChargenTranslationUtils.TranslateMenuOptions(__result);
        }
    }

    // [REMOVED] Patch_AbstractBuilderModuleWindowBase_Title (Invalid property target)

    // ========================================================================
    // [1] 게임 모드 선택 (QudGamemodeModuleWindow)
    // ========================================================================
    [HarmonyPatch(typeof(QudGamemodeModuleWindow))]
    public static class Patch_QudGamemodeModuleWindow
    {
        [HarmonyPatch(nameof(QudGamemodeModuleWindow.GetSelections))]
        [HarmonyPostfix]
        static void GetSelections_Postfix(ref IEnumerable<ChoiceWithColorIcon> __result)
        {
            if (__result == null) return;
            var list = __result.ToList();
            foreach (var choice in list)
            {
                if (choice == null) continue;
                
                // [FIX Issue 4] CRITICAL: NEVER modify choice.Id - used for selection logic
                string originalId = choice.Id;
                
                // [FIX Issue 8] Null check before ToLowerInvariant()
                if (!string.IsNullOrEmpty(choice.Title) && 
                    LocalizationManager.TryGetAnyTerm(choice.Title.ToLowerInvariant(), out string tTitle, "chargen_mode", "chargen_ui"))
                    choice.Title = tTitle;
                
                // [FIX Issue 5] Traverse field access
                var tr = Traverse.Create(choice);
                var descField = tr.Field<string>("Description");
                string desc = descField.Value;
                if (string.IsNullOrEmpty(desc)) continue;
                
                // Daily 모드의 동적 날짜 처리
                if (choice.Id == "Daily" && desc.Contains("Currently in day"))
                    {
                        // 숫자 부분 추출 (예: "Currently in day {{W|16}} of {{W|2026}}.")
                        var match = System.Text.RegularExpressions.Regex.Match(desc, @"day {{W\|(\d+)}} of {{W\|(\d+)}}");
                        if (match.Success)
                        {
                            string dayOfYear = match.Groups[1].Value;
                            string year = match.Groups[2].Value;
                            
                            // 번역된 템플릿 가져오기
                            if (LocalizationManager.TryGetAnyTerm("{{c|ù}} currently in day {{w|{day_of_year}}} of {{w|{year}}}.", out string template, "chargen_mode"))
                            {
                                // 플레이스홀더를 실제 값으로 치환
                                string translatedLine = template
                                    .Replace("{day_of_year}", dayOfYear)
                                    .Replace("{year}", year);
                                
                                // Description의 해당 라인만 교체
                                desc = System.Text.RegularExpressions.Regex.Replace(
                                    desc,
                                    @"{{c\|ù}} Currently in day {{W\|\d+}} of {{W\|\d+}}\.",
                                    translatedLine,
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                                );
                            }
                        }
                    }
                    
                descField.Value = ChargenTranslationUtils.TranslateLongDescription(desc, "chargen_mode", "chargen_ui");
                
                // [FIX Issue 4] Verify Id field was not modified
                Debug.Assert(choice.Id == originalId, $"[Qud-KR] CRITICAL: Id field was modified from '{originalId}'!");
            }
            __result = list;
        }
    }

    // ========================================================================
    // [2] 캐릭터 유형 선택 (QudChartypeModuleWindow)
    // ========================================================================
    [HarmonyPatch(typeof(QudChartypeModuleWindow))]
    public static class Patch_QudChartypeModuleWindow
    {
        [HarmonyPatch(nameof(QudChartypeModuleWindow.GetSelections))]
        [HarmonyPostfix]
        static void GetSelections_Postfix(ref IEnumerable<ChoiceWithColorIcon> __result)
        {
            if (__result == null) return;
            var list = __result.ToList();
            foreach (var choice in list)
            {
                if (choice == null) continue;
                
                // [FIX Issue 4] CRITICAL: NEVER modify choice.Id
                string originalId = choice.Id;
                
                // [FIX Issue 8] Null check before ToLowerInvariant()
                if (!string.IsNullOrEmpty(choice.Title) && 
                    LocalizationManager.TryGetAnyTerm(choice.Title.ToLowerInvariant(), out string tTitle, "chargen_ui", "ui"))
                    choice.Title = tTitle;

                // [FIX Issue 5] Traverse field existence check - use non-generic for FieldExists
                var tr = Traverse.Create(choice);
                var descTraverse = tr.Field("Description");
                if (descTraverse.FieldExists())
                {
                    string descVal = descTraverse.GetValue<string>();
                    if (!string.IsNullOrEmpty(descVal))
                    {
                        descTraverse.SetValue(ChargenTranslationUtils.TranslateLongDescription(descVal, "chargen_ui", "ui"));
                    }
                }
                
                // [FIX Issue 4] Verify Id field was not modified
                Debug.Assert(choice.Id == originalId, $"[Qud-KR] CRITICAL: Id field was modified from '{originalId}'!");
            }
            __result = list;
        }
    }

    // ========================================================================
    // [3] 종족 선택 (QudGenotypeModuleWindow)
    // ========================================================================
    [HarmonyPatch(typeof(QudGenotypeModuleWindow))]
    public static class Patch_QudGenotypeModuleWindow
    {
        // [FIX Issue 3] BeforeShow_Prefix -> GetSelections_Postfix로 변경
        // genotype.DisplayName/ExtraInfo (데이터 필드)를 직접 수정하는 대신
        // ChoiceWithColorIcon (UI 객체)만 수정하여 "UI-Only Postfix Pattern" 원칙 준수
        
        [HarmonyPatch(nameof(QudGenotypeModuleWindow.GetSelections))]
        [HarmonyPostfix]
        static void GetSelections_Postfix(QudGenotypeModuleWindow __instance, ref IEnumerable<ChoiceWithColorIcon> __result)
        {
            if (__result == null) return;
            var list = __result.ToList();
            
            foreach (var choice in list)
            {
                if (choice == null) continue;
                
                // CRITICAL: NEVER modify choice.Id - used for selection logic (see ERR-008)
                string originalId = choice.Id;
                
                // choice.Id (= genotype.Name)를 사용하여 번역 데이터 조회
                if (StructureTranslator.TryGetData(originalId, out var data))
                {
                    // UI 필드만 수정 (Title = DisplayName에 해당)
                    if (!string.IsNullOrEmpty(data.KoreanName))
                        choice.Title = data.KoreanName;
                    
                    // Description (GetFlatChargenInfo 결과)도 UI 객체 필드
                    var tr = Traverse.Create(choice);
                    string originalDesc = tr.Field<string>("Description").Value;
                    
                    // LevelText를 Description에 병합
                    if (data.LevelTextKo != null && data.LevelTextKo.Count > 0)
                    {
                        string newDesc = data.GetCombinedLongDescription(originalDesc);
                        if (!string.IsNullOrEmpty(newDesc))
                        {
                            tr.Field<string>("Description").Value = newDesc;
                        }
                    }
                }
                else
                {
                    // Fallback: 기존 로직 (UI 필드만 수정)
                    if (LocalizationManager.TryGetAnyTerm(choice.Title?.ToLowerInvariant(), out string tName, "chargen_proto", "mutation"))
                        choice.Title = tName;
                    
                    var tr = Traverse.Create(choice);
                    string desc = tr.Field<string>("Description").Value;
                    if (!string.IsNullOrEmpty(desc))
                    {
                        tr.Field<string>("Description").Value = ChargenTranslationUtils.TranslateLongDescription(
                            desc, "chargen_proto", "chargen_ui", "mutation", "mutation_desc", "powers", "power", "skill", "skill_desc");
                    }
                }
                
                // Verify Id field was not modified
                Debug.Assert(choice.Id == originalId, $"[Qud-KR] CRITICAL: Id field was modified from '{originalId}' - this will cause crashes!");
            }
            __result = list;
        }
    }

    // ========================================================================
    // [4] 하위 유형 및 직업/계급 선택 (QudSubtypeModule & QudSubtypeModuleWindow)
    // ========================================================================
    
    // QudSubtypeModule.GetSelections 패치 - Description 번역
    [HarmonyPatch(typeof(QudSubtypeModule))]
    public static class Patch_QudSubtypeModule
    {
        [HarmonyPatch(nameof(QudSubtypeModule.GetSelections))]
        [HarmonyPostfix]
        static void GetSelections_Postfix(ref IEnumerable<ChoiceWithColorIcon> __result)
        {
            if (__result == null) return;
            
            // 원본 백업 (예외 발생 시 복원용)
            var originalList = __result.ToList();
            
            try 
            {
                var list = new List<ChoiceWithColorIcon>(originalList);
                
                foreach (var choice in list)
                {
                    if (choice == null) continue;
                    
                    bool handledByStructure = false;

                    // Try StructureTranslator first (Subtypes)
                    if (!string.IsNullOrEmpty(choice.Title) && StructureTranslator.TryGetData(choice.Title, out var data))
                    {
                        choice.Title = data.KoreanName;
                        
                        var tr = Traverse.Create(choice);
                        string originalDesc = tr.Field<string>("Description").Value;
                        
                        string newDesc = data.GetCombinedLongDescription(originalDesc);
                        
                        if (!string.IsNullOrEmpty(newDesc))
                        {
                            tr.Field<string>("Description").Value = newDesc;
                        }
                        handledByStructure = true;
                    }

                    if (!handledByStructure)
                    {
                        // Title 번역 - 더 안전하게
                        string originalTitle = choice.Title;
                        if (!string.IsNullOrEmpty(originalTitle) && LocalizationManager.TryGetAnyTerm(originalTitle.ToLowerInvariant(), out string tTitle, "chargen_ui", "chargen_proto", "mutation", "skill"))
                        {
                            choice.Title = tTitle;
                        }
                        
                        // Description 번역 (GetFlatChargenInfo()의 결과) - 더 안전하게
                        var tr = Traverse.Create(choice);
                        string desc = tr.Field<string>("Description").Value;
                        if (!string.IsNullOrEmpty(desc))
                        {
                            string translated = ChargenTranslationUtils.TranslateLongDescription(desc, "chargen_proto", "chargen_ui", "factions", "mutation", "mutation_desc", "powers", "power", "skill", "skill_desc", "common");
                            
                            if (!string.IsNullOrEmpty(translated) && translated != desc)
                            {
                                tr.Field<string>("Description").Value = translated;
                            }
                        }
                    }
                }
                
                __result = list;
            }
            catch (Exception ex)
            {
                // 패치 실패 시 원본 복원
                UnityEngine.Debug.LogError($"[KoreanLocalization] GetSelections translation failed: {ex.Message}\n{ex.StackTrace}");
                __result = originalList;
            }
        }

        [HarmonyPatch(nameof(QudSubtypeModule.GetSelectionCategories))]
        [HarmonyPostfix]
        static void GetSelectionCategories_Postfix(ref IEnumerable<CategoryIcons> __result)
        {
            if (__result == null) return;
            
            // 원본 백업 (예외 발생 시 복원용)
            var originalList = __result.ToList();
            
            try
            {
                var list = new List<CategoryIcons>(originalList);
            
                foreach (var cat in list)
                {

                // Category Title Translation (e.g. "The Toxic Arboreta...")
                // Normalize key (remove color tags) for lookup
                string rawTitle = cat.Title;
                if (!string.IsNullOrEmpty(rawTitle))
                {
                    // Basic Strip of {{X|...}} but keep content? No, lookup keys usually need clean text?
                    // LocalizationManager.NormalizeKey handles stripping tags. 
                    // But TryGetAnyTerm does NOT auto-normalize inputs unless we check against normalized DB keys loop?
                    // Actually LocalizationManager.LoadJsonFile stores normalized keys.
                    // TryGetAnyTerm iterates over DB and matches keys.
                    // Wait, Look at LocalizationManager.cs:
                    // It stores [category][key] = value AND [category][normalizedKey] = value.
                    // So if we pass "The Toxic Arboreta...", NormalizeKey("The Toxic Arboreta...") -> lowercase etc.
                    // But if input has "{{G|The ...}}", NormalizeKey strips tags.
                    // HOWEVER, TryGetAnyTerm expects exact key match OR... it doesn't call NormalizeKey on input `key`.
                    // So if input is "{{G|Text}}", and DB has normalized key "text", we miss it.
                    // We must normalize input here manually if we want to match normalized keys in DB.
                    // But we don't have access to private NormalizeKey.
                    // Let's strip tags manually.
                    
                    string cleanTitle = System.Text.RegularExpressions.Regex.Replace(rawTitle, @"\{\{[a-zA-Z]\|([^}]+)\}\}", "$1");
                    cleanTitle = cleanTitle.Trim();

                    if (LocalizationManager.TryGetAnyTerm(cleanTitle, out string tTitle, "chargen_ui", "chargen_proto", "mutation", "ui"))
                        cat.Title = tTitle;
                    // Also try raw just in case
                    else if (LocalizationManager.TryGetAnyTerm(rawTitle, out tTitle, "chargen_ui", "chargen_proto", "mutation", "ui"))
                        cat.Title = tTitle;
                }

                if (cat.Choices != null)
                {
                     foreach (var choice in cat.Choices)
                    {
                         try {
                             // Use StructureTranslator if available
                             if (StructureTranslator.TryGetData(choice.Title, out var data))
                             {
                                 choice.Title = data.KoreanName;
                                 
                                 var trStruct = Traverse.Create(choice);
                                 string originalDesc = trStruct.Field<string>("Description").Value;
                                 
                                 string newDesc = data.GetCombinedLongDescription(originalDesc);
                                 if (!string.IsNullOrEmpty(newDesc))
                                 {
                                     trStruct.Field<string>("Description").Value = newDesc;
                                 }
                                 continue;
                             }

                             // Fallback - 더 안전한 번역 방식
                              // Translate Choice Title (e.g. "Horticulturist")
                            string originalTitle = choice.Title;
                            if (!string.IsNullOrEmpty(originalTitle) && LocalizationManager.TryGetAnyTerm(originalTitle.ToLowerInvariant(), out string tChoiceTitle, "chargen_ui", "chargen_proto", "mutation", "skill"))
                                 choice.Title = tChoiceTitle;

                             // Translate Description - 더 안전한 방식
                            var trFallback = Traverse.Create(choice);
                            string desc = trFallback.Field<string>("Description").Value;
                            if (!string.IsNullOrEmpty(desc))
                            {
                                string translated = ChargenTranslationUtils.TranslateLongDescription(desc, "chargen_proto", "chargen_ui", "mutation", "mutation_desc", "powers", "power", "skill", "skill_desc");
                                 if (!string.IsNullOrEmpty(translated) && translated != desc)
                                {
                                    trFallback.Field<string>("Description").Value = translated;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // 패치 실패 시 원본 유지 (선택 로직 손상 방지)
                            UnityEngine.Debug.LogError($"[KoreanLocalization] GetSelectionCategories choice translation failed: {ex.Message}");
                        }
                    }
                }
            }
            __result = list;
            }
            catch (Exception ex)
            {
                // 패치 실패 시 원본 복원
                UnityEngine.Debug.LogError($"[KoreanLocalization] GetSelectionCategories translation failed: {ex.Message}\n{ex.StackTrace}");
                __result = originalList;
            }
        }
    }
    
    // QudSubtypeModuleWindow 패치 - UI 텍스트 번역
    [HarmonyPatch(typeof(QudSubtypeModuleWindow))]
    public static class Patch_QudSubtypeModuleWindow
    {

        [HarmonyPatch(nameof(QudSubtypeModuleWindow.getSubtypeTitle))]
        [HarmonyPostfix]
        static void getSubtypeTitle_Postfix(ref string __result)
        {
            if (LocalizationManager.TryGetAnyTerm(__result?.ToLowerInvariant(), out string translated, "chargen_ui", "ui"))
                __result = translated;
        }

        [HarmonyPatch(nameof(QudSubtypeModuleWindow.getSubtypeSingularTitle))]
        [HarmonyPostfix]
        static void getSubtypeSingularTitle_Postfix(ref string __result)
        {
            if (LocalizationManager.TryGetAnyTerm(__result?.ToLowerInvariant(), out string translated, "chargen_ui", "ui"))
                __result = translated;
        }
    }

    // ========================================================================
    // [5] 속성 분배 (QudAttributesModuleWindow)
    // ========================================================================
    // 주의: attr.Attribute를 직접 번역하면 안 됨!
    // 게임 원본 AttributeSelectionControl.Updated()에서 Substring(0,3)을 사용하므로
    // 3글자 미만의 한글(예: "힘")로 번역 시 ArgumentOutOfRangeException 발생.
    // 대신 AttributeSelectionControl.Updated()를 Postfix 패치하여 UI 텍스트만 번역.
    
    [HarmonyPatch(typeof(AttributeSelectionControl))]
    public static class Patch_AttributeSelectionControl
    {
        // 계급명 영문 -> 한글 매핑 (6자 이내, 띄어쓰기 없음)
        private static readonly Dictionary<string, string> CasteShortNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Priest of All Moons", "달의사제" },
            { "Priest of All Suns", "태양사제" },
            { "Horticulturist", "원예가" },
            { "Child of the Deep", "심연의자녀" },
            { "Child of the Hearth", "화로의자녀" },
            { "Child of the Wheel", "수레의자녀" },
            { "Fuming God-Child", "연신의자녀" },
            { "Artifex", "기술자" },
            { "Consul", "영사" },
            { "Eunuch", "환관" },
            { "Praetorian", "근위병" },
            { "Syzygyrior", "합위전사" }
        };
        
        // 속성명 영문 -> 한글 매핑 (1~2글자)
        private static readonly Dictionary<string, string> AttributeShortNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Strength", "힘" },
            { "Agility", "민첩" },
            { "Toughness", "건강" },
            { "Intelligence", "지능" },
            { "Willpower", "의지" },
            { "Ego", "자아" }
        };

        private static Font _koreanFont;
        private static TMP_FontAsset _koreanTMPFont;

        private static Font GetKoreanFont()
        {
            if (_koreanFont != null) return _koreanFont;

            var candidates = new List<string> { "AppleGothic" };
            candidates.AddRange(FontManager.TargetFontNames);
            candidates.AddRange(new[]
            {
                "Apple SD Gothic Neo",
                "AppleSDGothicNeo-Regular",
                "Noto Sans CJK KR",
                "Noto Sans KR",
                "NanumGothic",
                "Nanum Gothic"
            });

            foreach (string fontName in candidates)
            {
                try
                {
                    Font tempFont = Font.CreateDynamicFontFromOSFont(fontName, 32);
                    if (tempFont == null || tempFont.fontNames == null || tempFont.fontNames.Length == 0)
                    {
                        continue;
                    }

                    // Ensure the font can render Korean glyphs.
                    if (!tempFont.HasCharacter('가'))
                    {
                        continue;
                    }

                    _koreanFont = tempFont;
                    break;
                }
                catch
                {
                    // Ignore and try next font.
                }
            }

            return _koreanFont;
        }

        private static TMP_FontAsset GetKoreanTMPFont()
        {
            if (_koreanTMPFont != null) return _koreanTMPFont;

            Font font = GetKoreanFont();
            if (font == null) 
            {
                UnityEngine.Debug.Log("[KR-Font] GetKoreanFont returned null, cannot create TMP font");
                return null;
            }

            try
            {
                UnityEngine.Debug.Log($"[KR-Font] Attempting TMP_FontAsset.CreateFontAsset with font: {font.name}");
                _koreanTMPFont = TMP_FontAsset.CreateFontAsset(font);
                if (_koreanTMPFont == null) 
                {
                    UnityEngine.Debug.Log("[KR-Font] TMP_FontAsset.CreateFontAsset returned null");
                    return null;
                }
                _koreanTMPFont.name = "QudKR_Tooltip_Fallback";
                UnityEngine.Debug.Log($"[KR-Font] TMP font created successfully: {_koreanTMPFont.name}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"[KR-Font] TMP_FontAsset.CreateFontAsset failed: {ex.Message}");
                _koreanTMPFont = null;
            }

            return _koreanTMPFont;
        }

        private static void ApplyTooltipFont(TooltipTrigger tooltip)
        {
            if (tooltip == null) return;

            Font font = GetKoreanFont();
            UnityEngine.Debug.Log($"[KR-Font] GetKoreanFont returned: {(font != null ? font.name : "null")}");
            if (font != null)
            {
                foreach (var text in tooltip.GetComponentsInChildren<UnityEngine.UI.Text>(true))
                {
                    if (text != null && text.font != font)
                    {
                        text.font = font;
                    }
                }
            }

            TMP_FontAsset tmpFont = GetKoreanTMPFont();
            UnityEngine.Debug.Log($"[KR-Font] GetKoreanTMPFont returned: {(tmpFont != null ? tmpFont.name : "null")}");
            if (tmpFont != null)
            {
                foreach (var tmp in tooltip.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (tmp == null) continue;
                    UnityEngine.Debug.Log($"[KR-Font] Setting TMP font on: {tmp.name}, text: '{tmp.text}'");

                    // Force tooltip TMP to use Korean-capable font
                    tmp.font = tmpFont;

                    if (tmp.font.fallbackFontAssetTable == null)
                        tmp.font.fallbackFontAssetTable = new List<TMP_FontAsset>();

                    if (!tmp.font.fallbackFontAssetTable.Contains(tmpFont))
                    {
                        tmp.font.fallbackFontAssetTable.Add(tmpFont);
                    }
                }
            }
        }

        // 툴팁 표시 시간 추적 - Show→Hide 즉시 발생 방지
        private static readonly Dictionary<AttributeSelectionControl, float> _tooltipShowTimes = 
            new Dictionary<AttributeSelectionControl, float>();
        private const float MIN_TOOLTIP_DURATION = 2.0f; // 최소 2초 유지

        /// <summary>
        /// Update 메서드 Postfix - 툴팁이 너무 빨리 숨겨지는 것을 방지
        /// 원본 Update()가 HidePopup()을 호출해도, 최소 표시 시간이 지나지 않았으면 다시 표시
        /// </summary>
        [HarmonyPatch(nameof(AttributeSelectionControl.Update))]
        [HarmonyPostfix]
        static void Update_Postfix(AttributeSelectionControl __instance)
        {
            if (__instance?.data == null || __instance.tooltip == null) return;
            
            bool hasBonus = !string.IsNullOrEmpty(__instance.data.BonusSource);
            if (!hasBonus) return;
            
            bool isDisplayed = __instance.tooltip.IsDisplayed();
            bool isActive = __instance.navContext.IsActive();
            
            // 툴팁이 표시되기 시작하면 시간 기록
            if (isDisplayed && !_tooltipShowTimes.ContainsKey(__instance))
            {
                _tooltipShowTimes[__instance] = Time.unscaledTime;
            }
            
            // 툴팁이 숨겨졌지만 최소 시간이 지나지 않았고 여전히 활성 상태이면 다시 표시
            if (!isDisplayed && isActive && _tooltipShowTimes.TryGetValue(__instance, out float showTime))
            {
                if (Time.unscaledTime - showTime < MIN_TOOLTIP_DURATION)
                {
                    __instance.tooltip.ShowManually(true);
                }
                else
                {
                    _tooltipShowTimes.Remove(__instance);
                }
            }
            
            // 비활성화되면 기록 제거
            if (!isActive && _tooltipShowTimes.ContainsKey(__instance))
            {
                _tooltipShowTimes.Remove(__instance);
            }
        }

        [HarmonyPatch(nameof(AttributeSelectionControl.Updated))]
        [HarmonyPostfix]
        static void Updated_Postfix(AttributeSelectionControl __instance)
        {
            if (__instance.data == null) return;
            
            string originalAttr = __instance.data.Attribute;
            if (!string.IsNullOrEmpty(originalAttr))
            {
                // 1. 속성명 번역 (1~2글자)
                if (AttributeShortNames.TryGetValue(originalAttr, out string shortName))
                {
                    __instance.attribute.text = shortName;
                }
                
                // 2. 포인트 비용 번역: "[1pts]" -> "[1점]"
                var titledButton = __instance.GetComponent<TitledIconButton>();
                if (titledButton != null)
                {
                    int apToRaise = __instance.data.APToRaise;
                    titledButton.SetTitle($"[{apToRaise}점]");
                }
            }
            
            // 3. 툴팁 BonusSource 번역: "+2 from Priest of All Moons caste" -> "달의사제 계급 +2"
            string bonusSource = __instance.data.BonusSource;
            if (!string.IsNullOrEmpty(bonusSource) && __instance.tooltip != null)
            {
                ApplyTooltipFont(__instance.tooltip);
                string translated = TranslateBonusSource(bonusSource);
                UnityEngine.Debug.Log($"[KR-Bonus] Original: '{bonusSource}' -> Translated: '{translated}'");
                __instance.tooltip.SetText("BodyText", Sidebar.FormatToRTF(translated));
            }
        }
        
        /// <summary>
        /// BonusSource 문자열 번역
        /// 원본 형식: "+2 from Priest of All Moons caste\n+1 from calling\n"
        /// 번역 형식: "달의사제 계급 +2\n소명 +1\n"
        /// </summary>
        private static string TranslateBonusSource(string bonusSource)
        {
            if (string.IsNullOrEmpty(bonusSource)) return bonusSource;
            
            var lines = bonusSource.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var translatedLines = new List<string>();
            
            foreach (var line in lines)
            {
                string translatedLine = TranslateBonusLine(line.Trim());
                if (!string.IsNullOrEmpty(translatedLine))
                {
                    translatedLines.Add(translatedLine);
                }
            }
            
            return string.Join("\n", translatedLines);
        }
        
        /// <summary>
        /// 단일 보너스 라인 번역
        /// 예: "+2 from Priest of All Moons caste" -> "달의사제 계급 +2"
        /// 예: "+1 from calling" -> "소명 +1"
        /// </summary>
        private static string TranslateBonusLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return line;
            
            UnityEngine.Debug.Log($"[KR-BonusLine] Input: '{line}'");
            
            // BonusSource format: "+2 from {{important|Priest of All Moons}} caste"
            // Pattern: "{+/-N} from {source} [caste/calling/genotype/subtype]"
            // Note: source may include Qud color tags
            var typedMatch = System.Text.RegularExpressions.Regex.Match(
                line,
                @"^([+-]?\d+)\s+from\s+(.+)\s+(caste|calling|genotype|subtype)\s*$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            string bonus;
            string rawSource;
            string sourceType = null;

            if (typedMatch.Success)
            {
                bonus = typedMatch.Groups[1].Value;
                rawSource = typedMatch.Groups[2].Value.Trim();
                sourceType = typedMatch.Groups[3].Value?.Trim();
                UnityEngine.Debug.Log($"[KR-BonusLine] TypedMatch: bonus='{bonus}', rawSource='{rawSource}', sourceType='{sourceType}'");
            }
            else
            {
                var basicMatch = System.Text.RegularExpressions.Regex.Match(
                    line,
                    @"^([+-]?\d+)\s+from\s+(.+)\s*$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (basicMatch.Success)
                {
                    bonus = basicMatch.Groups[1].Value;
                    rawSource = basicMatch.Groups[2].Value.Trim();
                    UnityEngine.Debug.Log($"[KR-BonusLine] BasicMatch: bonus='{bonus}', rawSource='{rawSource}'");
                }
                else
                {
                    var trailingBonusMatch = System.Text.RegularExpressions.Regex.Match(
                        line,
                        @"^(.+?)\s+([+-]?\d+)\s*$",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    if (!trailingBonusMatch.Success)
                    {
                        UnityEngine.Debug.Log($"[KR-BonusLine] No match, returning original line");
                        return line;
                    }

                    rawSource = trailingBonusMatch.Groups[1].Value.Trim();
                    bonus = trailingBonusMatch.Groups[2].Value.Trim();
                    UnityEngine.Debug.Log($"[KR-BonusLine] TrailingMatch: rawSource='{rawSource}', bonus='{bonus}'");
                }
            }

            // 색상 태그 제거: {{important|Priest of All Moons}} -> Priest of All Moons
            string source = StripQudTags(rawSource);
            UnityEngine.Debug.Log($"[KR-BonusLine] StripQudTags: '{rawSource}' -> '{source}'");

            if (string.IsNullOrEmpty(source))
            {
                UnityEngine.Debug.Log($"[KR-BonusLine] Source is empty, returning original line");
                return line;
            }

            // 계급명/직업명 번역 시도
            string translatedSource = source;

            // 1. 하드코딩된 CasteShortNames 딕셔너리에서 찾기
            if (CasteShortNames.TryGetValue(source, out string casteName))
            {
                translatedSource = casteName;
                UnityEngine.Debug.Log($"[KR-BonusLine] CasteShortNames found: '{source}' -> '{casteName}'");
            }
            // 2. StructureTranslator에서 찾기 (Calling 포함)
            else if (StructureTranslator.TryGetData(source, out var data) && !string.IsNullOrEmpty(data.KoreanName))
            {
                translatedSource = data.KoreanName;
                UnityEngine.Debug.Log($"[KR-BonusLine] StructureTranslator found: '{source}' -> '{data.KoreanName}'");
            }
            // 3. LocalizationManager에서 찾기
            else if (LocalizationManager.TryGetAnyTerm(source, out string tSource, "chargen_attributes", "chargen_ui", "ui", "common") ||
                     LocalizationManager.TryGetAnyTerm(source.ToLowerInvariant(), out tSource, "chargen_attributes", "chargen_ui", "ui", "common"))
            {
                translatedSource = tSource;
                UnityEngine.Debug.Log($"[KR-BonusLine] LocalizationManager found: '{source}' -> '{tSource}'");
            }
            else
            {
                UnityEngine.Debug.Log($"[KR-BonusLine] No translation found for '{source}'");
            }

            string translatedType = TranslateBonusSourceType(sourceType, source);
            UnityEngine.Debug.Log($"[KR-BonusLine] TranslatedType: '{sourceType}' -> '{translatedType}'");
            
            string result;
            if (!string.IsNullOrEmpty(translatedType))
            {
                result = $"{translatedSource} {translatedType} {bonus}";
            }
            else
            {
                result = $"{translatedSource} {bonus}";
            }
            
            UnityEngine.Debug.Log($"[KR-BonusLine] Result: '{result}'");
            return result;
        }

        private static string StripQudTags(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return System.Text.RegularExpressions.Regex.Replace(input, @"\{\{[^|}]+\|([^}]+)\}\}", "$1").Trim();
        }

        private static string TranslateBonusSourceType(string sourceType, string source)
        {
            if (string.IsNullOrEmpty(sourceType))
            {
                if (source.Equals("calling", StringComparison.OrdinalIgnoreCase))
                {
                    return "직업";
                }
                return null;
            }
            
            string key = sourceType.ToLowerInvariant();
            if (LocalizationManager.TryGetAnyTerm(key, out string translated, "chargen_ui", "ui", "common", "chargen_attributes"))
            {
                return translated;
            }
            
            return key switch
            {
                "caste" => "계급",
                "calling" => "직업",
                "genotype" => "유전자형",
                "subtype" => "하위 유형",
                _ => sourceType
            };
        }
    }
    
    // QudAttributesModuleWindow 패치 - 하단 메뉴 번역
    [HarmonyPatch(typeof(QudAttributesModuleWindow))]
    public static class Patch_QudAttributesModuleWindow
    {
        [HarmonyPatch(nameof(QudAttributesModuleWindow.UpdateControls))]
        [HarmonyPostfix]
        static void UpdateControls_Postfix(QudAttributesModuleWindow __instance)
        {
            if (__instance?.attributes == null) return;
            
            foreach (var attr in __instance.attributes)
            {
                if (attr == null || string.IsNullOrEmpty(attr.Description)) continue;
                
                string translated = ChargenTranslationUtils.TranslateLongDescription(
                    attr.Description,
                    "chargen_attributes",
                    "chargen_ui",
                    "ui",
                    "common");
                
                if (!string.IsNullOrEmpty(translated))
                {
                    attr.Description = translated;
                }
            }
        }

        [HarmonyPatch(nameof(QudAttributesModuleWindow.GetKeyMenuBar))]
        [HarmonyPostfix]
        static void GetKeyMenuBar_Postfix(QudAttributesModuleWindow __instance, ref IEnumerable<MenuOption> __result)
        {
            if (__result == null) return;
            
            var list = __result.ToList();
            foreach (var opt in list)
            {
                if (opt.Description != null && opt.Description.Contains("Points Remaining"))
                {
                    // "Points Remaining: X" -> "남은 포인트: X"
                    var match = System.Text.RegularExpressions.Regex.Match(
                        opt.Description, 
                        @"Points Remaining:\s*(-?\d+)");
                    
                    if (match.Success)
                    {
                        string pts = match.Groups[1].Value;
                        opt.Description = opt.Description.Replace(
                            $"Points Remaining: {pts}",
                            $"남은 포인트: {pts}");
                    }
                }
            }
            __result = list;
        }
    }

    // ========================================================================
    // [6] 변이 선택 (QudMutationsModuleWindow)
    // ========================================================================
    [HarmonyPatch(typeof(QudMutationsModuleWindow))]
    public static class Patch_QudMutationsModuleWindow
    {
        [HarmonyPatch(nameof(QudMutationsModuleWindow.UpdateControls))]
        [HarmonyPostfix]
        static void UpdateControls_Postfix(QudMutationsModuleWindow __instance)
        {
            // categoryMenus is private, use Traverse
            var categoryMenus = Traverse.Create(__instance).Field("categoryMenus").GetValue<List<CategoryMenuData>>();
            if (categoryMenus != null)
            {
                foreach (var cat in categoryMenus)
                {
                    if (LocalizationManager.TryGetAnyTerm(cat.Title?.ToLowerInvariant(), out string tTitle, "mutation_desc", "chargen_ui"))
                        cat.Title = tTitle;

                    if (cat.menuOptions != null)
                    {
                        foreach (var opt in cat.menuOptions)
                        {
                            // Translate mutation name using StructureTranslator
                            string translatedName = StructureTranslator.TranslateName(opt.Id);
                            if (!string.IsNullOrEmpty(translatedName))
                            {
                                var tr = Traverse.Create(opt);
                                string desc = tr.Field<string>("Description").Value;
                                
                                // Update description (keep UI suffix like " [V]")
                                if (!string.IsNullOrEmpty(desc))
                                {
                                    string suffix = "";
                                    if (desc.EndsWith(" [{{W|V}}]"))
                                    {
                                        suffix = " [{{W|V}}]";
                                        desc = desc.Substring(0, desc.Length - suffix.Length);
                                    }
                                    
                                    tr.Field<string>("Description").Value = translatedName + suffix;
                                }
                            }

                            // Translate LongDescription using StructureTranslator
                            if (!string.IsNullOrEmpty(opt.LongDescription) && !string.IsNullOrEmpty(opt.Id))
                            {
                                string translatedLong = StructureTranslator.GetLongDescription(opt.Id);
                                if (!string.IsNullOrEmpty(translatedLong))
                                {
                                    opt.LongDescription = translatedLong;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // ========================================================================
    // [6.5] 변이 선택 왼쪽 리스트 UI 렌더링 패치 (KeyMenuOption)
    // 문제: UpdateControls_Postfix는 BeforeShow() 호출 이후에 실행되어
    //       데이터를 수정해도 이미 UI에 전달된 상태임.
    // 해결: KeyMenuOption.setDataPrefixMenuOption()을 패치하여 
    //       UI 렌더링 시점에 번역을 적용.
    // ========================================================================
    [HarmonyPatch(typeof(KeyMenuOption))]
    public static class Patch_KeyMenuOption_MutationNames
    {
        [HarmonyPatch(nameof(KeyMenuOption.setDataPrefixMenuOption))]
        [HarmonyPrefix]
        static void setDataPrefixMenuOption_Prefix(PrefixMenuOption data)
        {
            if (data == null || string.IsNullOrEmpty(data.Id)) return;
            
            // Check if this is a mutation (has translation in StructureTranslator)
            string translatedName = StructureTranslator.TranslateName(data.Id);
            if (!string.IsNullOrEmpty(translatedName) && translatedName != data.Id)
            {
                // Get current description and preserve ALL suffixes
                // Examples: "Albino (D)", "Flaming Ray [V]", "Horns [{{W|V}}]"
                string desc = data.Description;
                if (!string.IsNullOrEmpty(desc))
                {
                    // Find suffix by locating where the mutation name ends
                    // data.Id is the English mutation name, find it in desc and extract suffix
                    string suffix = "";
                    int idIndex = desc.IndexOf(data.Id);
                    if (idIndex >= 0)
                    {
                        // Everything after the mutation name is the suffix
                        suffix = desc.Substring(idIndex + data.Id.Length);
                    }
                    data.Description = translatedName + suffix;
                }
            }
        }
    }

    // ========================================================================
    // [7] 사이버네틱스 선택 (QudCyberneticsModuleWindow)
    // ========================================================================
    [HarmonyPatch(typeof(QudCyberneticsModuleWindow))]
    public static class Patch_QudCyberneticsModuleWindow
    {
        [HarmonyPatch(nameof(QudCyberneticsModuleWindow.UpdateControls))]
        [HarmonyPostfix]
        static void UpdateControls_Postfix(QudCyberneticsModuleWindow __instance)
        {
            // categoryMenus is private, use Traverse
            var categoryMenus = Traverse.Create(__instance).Field("categoryMenus").GetValue<List<CategoryMenuData>>();
            if (categoryMenus != null)
            {
                foreach (var cat in categoryMenus)
                {
                    if (LocalizationManager.TryGetAnyTerm(cat.Title?.ToLowerInvariant(), out string tTitle, "chargen_ui", "ui"))
                        cat.Title = tTitle;

                    if (cat.menuOptions != null)
                    {
                        foreach (var opt in cat.menuOptions)
                        {
                            var tr = Traverse.Create(opt);
                            string desc = tr.Field<string>("Description").Value;
                            if (LocalizationManager.TryGetAnyTerm(desc?.ToLowerInvariant(), out string tDesc, "cybernetics", "ui"))
                            {
                                tr.Field<string>("Description").Value = tDesc;
                            }

                            if (!string.IsNullOrEmpty(opt.LongDescription))
                            {
                                opt.LongDescription = ChargenTranslationUtils.TranslateLongDescription(opt.LongDescription, "cybernetics_desc", "ui");
                            }
                        }
                    }
                }
            }
        }
    }

    // ========================================================================
    // [8] 프리젠 선택 (QudPregenModuleWindow)
    // ========================================================================
    [HarmonyPatch(typeof(QudPregenModuleWindow))]
    public static class Patch_QudPregenModuleWindow
    {
        [HarmonyPatch(nameof(QudPregenModuleWindow.GetSelections))]
        [HarmonyPostfix]
        static void GetSelections_Postfix(ref IEnumerable<ChoiceWithColorIcon> __result)
        {
            if (__result == null) return;
            var list = __result.ToList();
            foreach (var choice in list)
            {
                if (choice == null) continue;
                
                // [FIX Issue 4] CRITICAL: NEVER modify choice.Id
                string originalId = choice.Id;
                
                // [FIX Issue 8] Null check before ToLowerInvariant()
                if (!string.IsNullOrEmpty(choice.Title) && 
                    LocalizationManager.TryGetAnyTerm(choice.Title.ToLowerInvariant(), out string tTitle, "chargen_pregen", "chargen_proto"))
                    choice.Title = tTitle;
                
                // [FIX Issue 5] Traverse field existence check - use non-generic for FieldExists
                var tr = Traverse.Create(choice);
                var descTraverse = tr.Field("Description");
                if (descTraverse.FieldExists())
                {
                    string descVal = descTraverse.GetValue<string>();
                    if (!string.IsNullOrEmpty(descVal))
                    {
                        descTraverse.SetValue(ChargenTranslationUtils.TranslateLongDescription(descVal, "chargen_pregen", "chargen_proto"));
                    }
                }
                
                // [FIX Issue 4] Verify Id field was not modified
                Debug.Assert(choice.Id == originalId, $"[Qud-KR] CRITICAL: Id field was modified from '{originalId}'!");
            }
            __result = list;
        }
    }

    // ========================================================================
    // [9] 시작 지점 선택 (QudChooseStartingLocationModuleWindow)
    // ========================================================================
    [HarmonyPatch(typeof(QudChooseStartingLocationModuleWindow))]
    public static class Patch_QudChooseStartingLocationModuleWindow
    {
        // [FIX Issue 7] BeforeShow_Prefix -> GetSelections_Postfix로 변경
        // loc.Name (데이터 필드)를 직접 수정하는 대신 StartingLocationData의 UI 필드만 수정
        
        [HarmonyPatch(nameof(QudChooseStartingLocationModuleWindow.GetSelections))]
        [HarmonyPostfix]
        static void GetSelections_Postfix(ref IEnumerable<StartingLocationData> __result)
        {
            if (__result == null) return;
            var list = __result.ToList();
            
            foreach (var loc in list)
            {
                if (loc == null) continue;
                
                // [FIX Issue 4] Id 필드 보호 - 원본 캐시
                string originalId = loc.Id;
                
                // [FIX Issue 5] Traverse 필드 존재 확인 후 접근 - use non-generic for FieldExists
                var tr = Traverse.Create(loc);
                
                // Name 필드 번역 (UI용 표시 이름)
                var nameTraverse = tr.Field("Name");
                if (nameTraverse.FieldExists())
                {
                    string name = nameTraverse.GetValue<string>();
                    if (!string.IsNullOrEmpty(name) && LocalizationManager.TryGetAnyTerm(name.ToLowerInvariant(), out string tName, "chargen_location"))
                    {
                        nameTraverse.SetValue(tName);
                    }
                }
                
                // Description 필드 번역
                var descTraverse = tr.Field("Description");
                if (descTraverse.FieldExists())
                {
                    string descVal = descTraverse.GetValue<string>();
                    if (!string.IsNullOrEmpty(descVal))
                    {
                        descTraverse.SetValue(ChargenTranslationUtils.TranslateLongDescription(descVal, "chargen_location"));
                    }
                }
                
                // Verify Id field was not modified
                Debug.Assert(loc.Id == originalId, $"[Qud-KR] CRITICAL: StartingLocation Id was modified from '{originalId}'!");
            }
            __result = list;
        }
    }

    // ========================================================================
    // [10] 커스터마징 (QudCustomizeCharacterModuleWindow)
    // ========================================================================
    [HarmonyPatch(typeof(QudCustomizeCharacterModuleWindow))]
    public static class Patch_QudCustomizeCharacterModuleWindow
    {
        [HarmonyPatch(nameof(QudCustomizeCharacterModuleWindow.GetSelections))]
        [HarmonyPostfix]
        static void GetSelections_Postfix(ref IEnumerable<PrefixMenuOption> __result)
        {
            if (__result == null) return;
            var list = __result.ToList();
            foreach (var opt in list)
            {
                if (LocalizationManager.TryGetAnyTerm(opt.Prefix?.ToLowerInvariant(), out string tPrefix, "chargen_ui", "ui"))
                    opt.Prefix = tPrefix;

                var tr = Traverse.Create(opt);
                string desc = tr.Field<string>("Description").Value;
                if (LocalizationManager.TryGetAnyTerm(desc?.ToLowerInvariant(), out string tDesc, "chargen_ui", "ui"))
                {
                    tr.Field<string>("Description").Value = tDesc;
                }
            }
            __result = list;
        }

        [HarmonyPatch(nameof(QudCustomizeCharacterModuleWindow.GetPets))]
        [HarmonyPostfix]
        static void GetPets_Postfix(ref IEnumerable<QudCustomizeCharacterModuleWindow.PetData> __result)
        {
            if (__result == null) return;
            var list = __result.ToList();
            foreach (var pet in list)
            {
                var tr = Traverse.Create(pet);
                string desc = tr.Field<string>("Description").Value;
                if (LocalizationManager.TryGetAnyTerm(desc?.ToLowerInvariant(), out string tDesc, "chargen_ui", "ui"))
                {
                    tr.Field<string>("Description").Value = tDesc;
                }
            }
            __result = list;
        }
    }

    // ========================================================================
    // [11] 빌드 요약 (QudBuildSummaryModuleWindow)
    // ========================================================================
    [HarmonyPatch(typeof(QudBuildSummaryModuleWindow))]
    public static class Patch_QudBuildSummaryModuleWindow
    {
        [HarmonyPatch(nameof(QudBuildSummaryModuleWindow.GetSelections))]
        [HarmonyPostfix]
        static void GetSelections_Postfix(ref IEnumerable<SummaryBlockData> __result)
        {
            if (__result == null) return;
            var list = __result.ToList();
            foreach (var block in list)
            {
                if (LocalizationManager.TryGetAnyTerm(block.Title?.ToLowerInvariant(), out string tTitle, "chargen_ui", "ui"))
                    block.Title = tTitle;

                var tr = Traverse.Create(block);
                string desc = tr.Field<string>("Description").Value;
                if (!string.IsNullOrEmpty(desc))
                {
                    tr.Field<string>("Description").Value = ChargenTranslationUtils.TranslateLongDescription(desc, "mutation", "mutation_desc", "powers", "power", "skill", "skill_desc", "cybernetics", "cybernetics_desc", "chargen_proto", "chargen_location", "ui", "common");
                }
            }
            __result = list;
        }
    }

    // ========================================================================
    // [12] 브레드크럼 통합 패치
    // ========================================================================
    [HarmonyPatch]
    public static class Patch_GenericBreadcrumbs
    {
        [HarmonyTargetMethods]
        static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(QudGamemodeModuleWindow), nameof(QudGamemodeModuleWindow.GetBreadcrumb));
            yield return AccessTools.Method(typeof(QudChartypeModuleWindow), nameof(QudChartypeModuleWindow.GetBreadcrumb));
            yield return AccessTools.Method(typeof(QudGenotypeModuleWindow), nameof(QudGenotypeModuleWindow.GetBreadcrumb));
            yield return AccessTools.Method(typeof(QudSubtypeModuleWindow), nameof(QudSubtypeModuleWindow.GetBreadcrumb));
            yield return AccessTools.Method(typeof(QudAttributesModuleWindow), nameof(QudAttributesModuleWindow.GetBreadcrumb));
            yield return AccessTools.Method(typeof(QudMutationsModuleWindow), nameof(QudMutationsModuleWindow.GetBreadcrumb));
            yield return AccessTools.Method(typeof(QudPregenModuleWindow), nameof(QudPregenModuleWindow.GetBreadcrumb));
            yield return AccessTools.Method(typeof(QudCyberneticsModuleWindow), nameof(QudCyberneticsModuleWindow.GetBreadcrumb));
            yield return AccessTools.Method(typeof(QudBuildSummaryModuleWindow), nameof(QudBuildSummaryModuleWindow.GetBreadcrumb));
            yield return AccessTools.Method(typeof(QudCustomizeCharacterModuleWindow), nameof(QudCustomizeCharacterModuleWindow.GetBreadcrumb));
            yield return AccessTools.Method(typeof(QudChooseStartingLocationModuleWindow), nameof(QudChooseStartingLocationModuleWindow.GetBreadcrumb));
        }

        [HarmonyPostfix]
        static void Postfix(ref UIBreadcrumb __result)
        {
            ChargenTranslationUtils.TranslateBreadcrumb(__result);
        }
    }

    // ========================================================================
    // [8] Chargen Overlay Scope Management (UITextSkin translation)
    // ========================================================================
    [HarmonyPatch(typeof(EmbarkBuilderOverlayWindow))]
    public static class Patch_EmbarkBuilderOverlayWindow_Scope
    {
        internal static bool _scopePushed;

        [HarmonyPatch(nameof(EmbarkBuilderOverlayWindow.BeforeShowWithWindow))]
        [HarmonyPrefix]
        static void BeforeShowWithWindow_Prefix()
        {
            if (!_scopePushed)
            {
                var scopes = new List<Dictionary<string, string>>
                {
                    LocalizationManager.GetCategory("chargen_ui"),
                    LocalizationManager.GetCategory("chargen_proto"),
                    LocalizationManager.GetCategory("mutation"),
                    LocalizationManager.GetCategory("skill"),
                    LocalizationManager.GetCategory("ui"),
                    LocalizationManager.GetCategory("common")
                };

                scopes = scopes.Where(s => s != null).ToList();
                if (scopes.Count > 0)
                {
                    ScopeManager.PushScope(scopes.ToArray());
                    _scopePushed = true;
                }
            }

            // Translate static menu options
            if (LocalizationManager.TryGetAnyTerm("back", out string backText, "chargen_ui", "common", "ui"))
            {
                EmbarkBuilderOverlayWindow.BackMenuOption.Description = backText;
            }

            if (LocalizationManager.TryGetAnyTerm("next", out string nextText, "chargen_ui", "common", "ui"))
            {
                EmbarkBuilderOverlayWindow.NextMenuOption.Description = nextText;
            }
        }
    }

    [HarmonyPatch(typeof(WindowBase), nameof(WindowBase.Hide))]
    public static class Patch_EmbarkBuilderOverlayWindow_Hide
    {
        [HarmonyPostfix]
        static void Hide_Postfix(WindowBase __instance)
        {
            if (__instance is EmbarkBuilderOverlayWindow && Patch_EmbarkBuilderOverlayWindow_Scope._scopePushed)
            {
                ScopeManager.PopScope();
                Patch_EmbarkBuilderOverlayWindow_Scope._scopePushed = false;
            }
        }
    }
}
