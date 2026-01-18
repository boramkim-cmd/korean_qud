/*
 * 파일명: ChargenTranslationUtils.cs
 * 분류: [Utils] 캐릭터 생성 번역 유틸리티
 * 역할: 캐릭터 생성 화면의 다중 라인 설명 등을 TranslationEngine을 사용해 번역합니다.
 */

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using XRL.CharacterBuilds;
using XRL.UI;
using XRL.UI.Framework;
using QudKRTranslation.Core;

namespace QudKRTranslation.Utils
{
    public static class ChargenTranslationUtils
    {
        /// <summary>
        /// 여러 줄의 설명 텍스트를 번역합니다. TranslationEngine을 사용하여 각 라인을 처리합니다.
        /// </summary>
        public static string TranslateLongDescription(string original, params string[] categories)
        {
            if (string.IsNullOrEmpty(original)) return original;
            
            // 카테고리를 Dictionary 배열로 변환
            var scopes = categories
                .Select(cat => LocalizationManager.GetCategory(cat))
                .Where(d => d != null)
                .ToArray();
            
            if (scopes.Length == 0) return original;
            
            var lines = original.Split('\n');
            bool changed = false;
            
            for (int i = 0; i < lines.Length; i++)
            {
                var trimmed = lines[i].Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // 0. UI Artifact Preservation (Prefixes/Suffixes like "[ ] [1] " or " [V]")
                string uiPrefix = "";
                string uiSuffix = "";
                
                // Handle "[ ] [1] " prefix
                var uiMatch = Regex.Match(contentToTranslate, @"^(\[[ X*]\]\s+\[[+-]?\d+\]\s+)(.+)$");
                if (uiMatch.Success)
                {
                    uiPrefix = uiMatch.Groups[1].Value;
                    contentToTranslate = uiMatch.Groups[2].Value;
                }
                
                // Handle " [V]" or " (Selected)" suffix
                if (contentToTranslate.EndsWith(" [V]"))
                {
                    uiSuffix = " [V]";
                    contentToTranslate = contentToTranslate.Substring(0, contentToTranslate.Length - 4);
                }
                else if (contentToTranslate.EndsWith(" (Selected)"))
                {
                    uiSuffix = " (Selected)";
                    contentToTranslate = contentToTranslate.Substring(0, contentToTranslate.Length - 11);
                }

                // 1. 불렛 포인트 보존 로직
                // Qud uses {{c|ù}} or just ù for bullets
                string bulletPrefix = "";
                
                // Check for common Qud bullets
                if (contentToTranslate.StartsWith("{{c|ù}}")) 
                {
                    bulletPrefix = "{{c|ù}} ";
                    contentToTranslate = contentToTranslate.Replace("{{c|ù}}", "").Trim();
                }
                else if (contentToTranslate.StartsWith("ù"))
                {
                    bulletPrefix = "ù ";
                    contentToTranslate = contentToTranslate.TrimStart('ù', ' ').Trim();
                }

                // 2. 기본 번역 시도
                if (TranslationEngine.TryTranslate(contentToTranslate, out string translated, scopes))
                {
                    lines[i] = prefix + uiPrefix + bulletPrefix + translated + uiSuffix;
                    changed = true;
                    continue;
                }
                
                // 3. Reputation Regex ("+200 reputation with ...")
                // Pattern: "+<Number> reputation with <Faction>"
                 var repMatch = Regex.Match(contentToTranslate, @"^([+-]?\d+)\s+reputation with\s+(.+)$", RegexOptions.IgnoreCase);
                 if (repMatch.Success)
                 {
                     string amount = repMatch.Groups[1].Value;
                     string faction = repMatch.Groups[2].Value;
                     
                     if (TranslationEngine.TryTranslate(faction, out string tFaction, scopes) || 
                         LocalizationManager.TryGetAnyTerm(faction, out tFaction, "factions", "ui", "common"))
                     {
                         lines[i] = prefix + uiPrefix + bulletPrefix + $"{tFaction} 평판 {amount}" + uiSuffix;
                         changed = true;
                         continue;
                     }
                 }

                // 4. Regex 번역 (스탯 등: "+2 Agility", "+200 reputation...")
                // Pattern: "+<Number> <Text>"
                var match = Regex.Match(contentToTranslate, @"^([+-]?\d+)\s+(.+)$");
                if (match.Success)
                {
                    string numberPart = match.Groups[1].Value;
                    string textPart = match.Groups[2].Value;
                    
                    if (TranslationEngine.TryTranslate(textPart, out string translatedText, scopes))
                    {
                        lines[i] = prefix + uiPrefix + bulletPrefix + numberPart + " " + translatedText + uiSuffix;
                        changed = true;
                        continue;
                    }
                }
            }
            
            return changed ? string.Join("\n", lines) : original;
        }

        public static IEnumerable<MenuOption> TranslateMenuOptions(IEnumerable<MenuOption> options)
        {
            var scopes = new[] { "chargen_ui", "mutation", "mutation_names", "mutation_desc", "powers", "power", "skill", "skill_desc", "ui", "common" }
                .Select(cat => LocalizationManager.GetCategory(cat))
                .Where(d => d != null)
                .ToArray();

            foreach (var opt in options)
            {
                if (opt != null)
                {
                    var tr = Traverse.Create(opt);
                    string desc = tr.Field<string>("Description").Value;
                    if (!string.IsNullOrEmpty(desc))
                    {
                        // Wrap inside TranslateLongDescription logic or similar
                        string translated = TranslateLongDescription(desc, "mutation", "mutation_names", "mutation_desc", "skill", "ui", "common");
                        if (translated != desc)
                        {
                            tr.Field<string>("Description").Value = translated;
                        }
                    }
                }
                yield return opt;
            }
        }

        /// <summary>
        /// UIBreadcrumb의 Title을 번역합니다.
        /// </summary>
        public static void TranslateBreadcrumb(UIBreadcrumb breadcrumb)
        {
            if (breadcrumb == null || string.IsNullOrEmpty(breadcrumb.Title)) return;
            
            var scopes = new[] { "chargen_ui", "chargen_proto", "mutation", "skill", "cybernetics", "ui", "common" }
                .Select(cat => LocalizationManager.GetCategory(cat))
                .Where(d => d != null)
                .ToArray();

            if (TranslationEngine.TryTranslate(breadcrumb.Title, out string translated, scopes))
            {
                breadcrumb.Title = translated;
            }
        }
    }
}
