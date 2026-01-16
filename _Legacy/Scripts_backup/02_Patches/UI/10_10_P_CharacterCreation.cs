/*
 * 파일명: 10_10_P_CharacterCreation.cs
 * 분류: [UI Patch] 캐릭터 생성 화면 통합 패치
 */

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;
using XRL.CharacterBuilds.Qud.UI;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;
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
                if (LocalizationManager.TryGetAnyTerm(choice.Title?.ToLowerInvariant(), out string tTitle, "chargen_mode", "chargen_ui"))
                    choice.Title = tTitle;
                
                var tr = Traverse.Create(choice);
                string desc = tr.Field<string>("Description").Value;
                if (!string.IsNullOrEmpty(desc))
                {
                    // [DEBUG] 원본 Description 로깅
                    UnityEngine.Debug.Log($"[Qud-KR DEBUG] GameMode '{choice.Id}' Description 원본:\n{desc}");
                    
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
                    
                    string translated = ChargenTranslationUtils.TranslateLongDescription(desc, "chargen_mode", "chargen_ui");
                    
                    // [DEBUG] 번역 결과 로깅
                    UnityEngine.Debug.Log($"[Qud-KR DEBUG] GameMode '{choice.Id}' Description 번역 후:\n{translated}");
                    
                    tr.Field<string>("Description").Value = translated;
                }
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
                if (LocalizationManager.TryGetAnyTerm(choice.Title?.ToLowerInvariant(), out string tTitle, "chargen_ui", "ui"))
                    choice.Title = tTitle;

                var tr = Traverse.Create(choice);
                string desc = tr.Field<string>("Description").Value;
                if (!string.IsNullOrEmpty(desc))
                {
                    tr.Field<string>("Description").Value = ChargenTranslationUtils.TranslateLongDescription(desc, "chargen_ui", "ui");
                }
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
        [HarmonyPatch(nameof(QudGenotypeModuleWindow.BeforeShow))]
        [HarmonyPrefix]
        static void BeforeShow_Prefix(QudGenotypeModuleWindow __instance)
        {
            if (__instance.module?.genotypes == null) return;
            foreach (var genotype in __instance.module.genotypes)
            {
                if (genotype == null) continue;
                if (LocalizationManager.TryGetAnyTerm(genotype.DisplayName?.ToLowerInvariant(), out string tName, "chargen_proto", "mutation"))
                    genotype.DisplayName = tName;
                
                if (genotype.ExtraInfo != null)
                {
                    for (int i = 0; i < genotype.ExtraInfo.Count; i++)
                    {
                        // [DEBUG] Log original ExtraInfo
                        UnityEngine.Debug.Log($"[Qud-KR DEBUG] Genotype '{genotype.DisplayName}' ExtraInfo[{i}] 원본:\n{genotype.ExtraInfo[i]}");
                        
                        genotype.ExtraInfo[i] = ChargenTranslationUtils.TranslateLongDescription(genotype.ExtraInfo[i], "chargen_proto", "chargen_ui", "mutation", "mutation_desc", "skill", "skill_desc");
                        
                        // [DEBUG] Log translated ExtraInfo
                        UnityEngine.Debug.Log($"[Qud-KR DEBUG] Genotype '{genotype.DisplayName}' ExtraInfo[{i}] 번역 후:\n{genotype.ExtraInfo[i]}");
                    }
                }
            }
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
            var list = __result.ToList();
            
            foreach (var choice in list)
            {
                // Title 번역
                if (LocalizationManager.TryGetAnyTerm(choice.Title?.ToLowerInvariant(), out string tTitle, "chargen_proto", "mutation", "skill"))
                {
                    UnityEngine.Debug.Log($"[Subtype] Title 번역: {choice.Title} → {tTitle}");
                    choice.Title = tTitle;
                }
                
                // Description 번역 (GetFlatChargenInfo()의 결과)
                var tr = Traverse.Create(choice);
                string desc = tr.Field<string>("Description").Value;
                if (!string.IsNullOrEmpty(desc))
                {
                    UnityEngine.Debug.Log($"[Subtype] Description 원본 [{choice.Title}]: {desc}");
                    
                    string translated = ChargenTranslationUtils.TranslateLongDescription(desc, "chargen_proto", "chargen_ui", "mutation", "mutation_desc", "skill", "skill_desc");
                    
                    if (translated != desc)
                    {
                        tr.Field<string>("Description").Value = translated;
                        UnityEngine.Debug.Log($"[Subtype] Description 번역됨: {translated}");
                    }
                }
            }
            
            __result = list;
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
    [HarmonyPatch(typeof(QudAttributesModuleWindow))]
    public static class Patch_QudAttributesModuleWindow
    {
        [HarmonyPatch(nameof(QudAttributesModuleWindow.UpdateControls))]
        [HarmonyPrefix]
        static void UpdateControls_Prefix(QudAttributesModuleWindow __instance)
        {
            if (__instance.attributes == null) return;
            foreach (var attr in __instance.attributes)
            {
                if (LocalizationManager.TryGetAnyTerm(attr.Attribute?.ToLowerInvariant(), out string tName, "chargen_ui", "chargen_stats", "attributes", "ui"))
                    attr.Attribute = tName;
                
                if (LocalizationManager.TryGetAnyTerm(attr.ShortAttributeName?.ToLowerInvariant(), out string tShort, "chargen_ui", "chargen_stats", "attributes", "ui"))
                    attr.ShortAttributeName = tShort;
            }
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
                            var tr = Traverse.Create(opt);
                            string desc = tr.Field<string>("Description").Value;
                            if (LocalizationManager.TryGetAnyTerm(desc?.ToLowerInvariant(), out string tDesc, "mutation", "mutation_desc", "skill", "skill_desc"))
                            {
                                tr.Field<string>("Description").Value = tDesc;
                            }

                            if (!string.IsNullOrEmpty(opt.LongDescription))
                            {
                                opt.LongDescription = ChargenTranslationUtils.TranslateLongDescription(opt.LongDescription, "mutation_desc", "skill_desc", "ui");
                            }
                        }
                    }
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
                if (LocalizationManager.TryGetAnyTerm(choice.Title?.ToLowerInvariant(), out string tTitle, "chargen_pregen", "chargen_proto"))
                    choice.Title = tTitle;
                
                var tr = Traverse.Create(choice);
                string desc = tr.Field<string>("Description").Value;
                if (!string.IsNullOrEmpty(desc))
                {
                    tr.Field<string>("Description").Value = ChargenTranslationUtils.TranslateLongDescription(desc, "chargen_pregen", "chargen_proto");
                }
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
        [HarmonyPatch(nameof(QudChooseStartingLocationModuleWindow.BeforeShow))]
        [HarmonyPrefix]
        static void BeforeShow_Prefix(QudChooseStartingLocationModuleWindow __instance)
        {
            if (__instance.module?.startingLocations == null) return;
            foreach (var loc in __instance.module.startingLocations.Values)
            {
                if (loc == null) continue;
                if (LocalizationManager.TryGetAnyTerm(loc.Name?.ToLowerInvariant(), out string tName, "chargen_location"))
                    loc.Name = tName;
                
                var tr = Traverse.Create(loc);
                string desc = tr.Field<string>("Description").Value;
                if (!string.IsNullOrEmpty(desc))
                {
                    tr.Field<string>("Description").Value = ChargenTranslationUtils.TranslateLongDescription(desc, "chargen_location");
                }
            }
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
                    tr.Field<string>("Description").Value = ChargenTranslationUtils.TranslateLongDescription(desc, "mutation", "mutation_desc", "skill", "skill_desc", "cybernetics", "cybernetics_desc", "chargen_proto", "chargen_location", "ui", "common");
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
}
