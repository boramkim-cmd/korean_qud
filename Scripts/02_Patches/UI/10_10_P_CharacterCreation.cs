/*
 * 파일명: 10_10_P_CharacterCreation.cs
 * 분류: [UI Patch] 캐릭터 생성 화면 통합 패치
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;
using XRL.CharacterBuilds.Qud.UI;
using QudKRTranslation.Core;
using QudKRTranslation.Utils;

namespace QudKRTranslation.Patches
{
    // ========================================================================
    // 공통 유틸리티
    // ========================================================================
    public static class ChargenTranslationUtils
    {
        public static string TranslateLongDescription(string original, params string[] categories)
        {
            if (string.IsNullOrEmpty(original)) return original;
            var lines = original.Split('\n');
            bool changed = false;
            for (int i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                if (LocalizationManager.TryGetAnyTerm(trimmed.ToLowerInvariant(), out string tLine, categories))
                {
                    lines[i] = lines[i].Replace(trimmed, tLine);
                    changed = true;
                }
            }
            return changed ? string.Join("\n", lines) : original;
        }

        public static IEnumerable<MenuOption> TranslateMenuOptions(IEnumerable<MenuOption> options)
        {
            foreach (var opt in options)
            {
                if (opt != null)
                {
                    // Description ambiguity fix using Traverse
                    var tr = Traverse.Create(opt);
                    string desc = tr.Field<string>("Description").Value;
                    if (!string.IsNullOrEmpty(desc))
                    {
                        if (LocalizationManager.TryGetAnyTerm(desc.ToLowerInvariant(), out string translated, "chargen_ui", "mutation_desc", "ui", "common"))
                        {
                            tr.Field<string>("Description").Value = translated;
                        }
                    }
                }
                yield return opt;
            }
        }

        // 브레드크럼 공통 번역 로직
        public static void TranslateBreadcrumb(UIBreadcrumb breadcrumb)
        {
            if (breadcrumb == null || string.IsNullOrEmpty(breadcrumb.Title)) return;
            if (LocalizationManager.TryGetAnyTerm(breadcrumb.Title.ToLowerInvariant(), out string translated, "chargen_ui", "chargen_proto", "mutation", "skill", "cybernetics", "ui", "common"))
            {
                breadcrumb.Title = translated;
            }
        }
    }

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
                    tr.Field<string>("Description").Value = ChargenTranslationUtils.TranslateLongDescription(desc, "chargen_mode", "chargen_ui");
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
                        genotype.ExtraInfo[i] = ChargenTranslationUtils.TranslateLongDescription(genotype.ExtraInfo[i], "chargen_proto", "chargen_ui", "mutation", "mutation_desc", "skill", "skill_desc");
                    }
                }
            }
        }
    }

    // ========================================================================
    // [4] 하위 유형 및 직업/계급 선택 (QudSubtypeModuleWindow)
    // ========================================================================
    [HarmonyPatch(typeof(QudSubtypeModuleWindow))]
    public static class Patch_QudSubtypeModuleWindow
    {
        [HarmonyPatch(nameof(QudSubtypeModuleWindow.BeforeShow))]
        [HarmonyPrefix]
        static void BeforeShow_Prefix(QudSubtypeModuleWindow __instance)
        {
            if (__instance.module?.subtypes == null) return;
            foreach (var subtype in __instance.module.subtypes)
            {
                if (subtype == null) continue;
                if (LocalizationManager.TryGetAnyTerm(subtype.DisplayName?.ToLowerInvariant(), out string tName, "chargen_proto", "mutation", "skill"))
                    subtype.DisplayName = tName;
                
                if (subtype.ExtraInfo != null)
                {
                    for (int i = 0; i < subtype.ExtraInfo.Count; i++)
                    {
                        subtype.ExtraInfo[i] = ChargenTranslationUtils.TranslateLongDescription(subtype.ExtraInfo[i], "chargen_proto", "chargen_ui", "mutation", "mutation_desc", "skill", "skill_desc");
                    }
                }
            }
        }

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
