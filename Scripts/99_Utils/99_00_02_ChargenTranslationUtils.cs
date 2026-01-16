/*
 * 파일명: ChargenTranslationUtils.cs
 * 분류: [Utils] 캐릭터 생성 번역 유틸리티
 * 역할: 캐릭터 생성 화면의 다중 라인 설명 등을 TranslationEngine을 사용해 번역합니다.
 */

using System;
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

                // 1. 불렛 포인트 보존 로직
                // Qud uses {{c|ù}} or just ù for bullets
                string prefix = "";
                string contentToTranslate = trimmed;
                
                // Check for common Qud bullets
                if (trimmed.StartsWith("{{c|ù}}")) 
                {
                    prefix = "{{c|ù}} ";
                    contentToTranslate = trimmed.Replace("{{c|ù}}", "").Trim();
                }
                else if (trimmed.StartsWith("ù"))
                {
                    prefix = "ù ";
                    contentToTranslate = trimmed.TrimStart('ù', ' ').Trim();
                }

                // 2. 기본 번역 시도
                if (TranslationEngine.TryTranslate(contentToTranslate, out string translated, scopes))
                {
                    lines[i] = prefix + translated;
                    changed = true;
                    continue;
                }
                
                // 3. Regex 번역 (스탯 등: "+2 Agility", "+200 reputation...")
                // Pattern: "+<Number> <Text>"
                var match = Regex.Match(contentToTranslate, @"^([+-]?\d+)\s+(.+)$");
                if (match.Success)
                {
                    string numberPart = match.Groups[1].Value;
                    string textPart = match.Groups[2].Value;
                    
                    if (TranslationEngine.TryTranslate(textPart, out string translatedText, scopes))
                    {
                        lines[i] = prefix + numberPart + " " + translatedText;
                        changed = true;
                        continue;
                    }
                }
                
                // 4. Reputation Regex ("+200 reputation with ...")
                // Pattern: "+<Number> reputation with <Faction>"
                 var repMatch = Regex.Match(contentToTranslate, @"^([+-]?\d+)\s+reputation with\s+(.+)$", RegexOptions.IgnoreCase);
                 if (repMatch.Success)
                 {
                     string amount = repMatch.Groups[1].Value;
                     string faction = repMatch.Groups[2].Value;
                     
                     // Try translate faction (might need specific scope?)
                     // Factions usually are not in 'skill' scope, maybe 'factions' scope is needed but let's try available ones
                     // Or simplify to "파벌 <Faction> 평판 <Amount>"
                     
                     if (TranslationEngine.TryTranslate(faction, out string tFaction, scopes) || 
                         LocalizationManager.TryGetAnyTerm(faction, out tFaction, "factions", "ui", "common"))
                     {
                         lines[i] = prefix + $"{tFaction} 평판 {amount}";
                         changed = true;
                     }
                 }
            }
            
            return changed ? string.Join("\n", lines) : original;
        }

        /// <summary>
        /// MenuOption 리스트를 번역합니다.
        /// </summary>
        /// <summary>
        /// MenuOption 리스트를 번역합니다.
        /// </summary>
        public static IEnumerable<MenuOption> TranslateMenuOptions(IEnumerable<MenuOption> options)
        {
            var scopes = new[] { "chargen_ui", "mutation_desc", "powers", "power", "skill", "skill_desc", "ui", "common" }
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
                        if (TranslationEngine.TryTranslate(desc, out string translated, scopes))
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
