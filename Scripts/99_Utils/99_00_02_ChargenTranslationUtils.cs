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
                
                // TranslationEngine 사용 - 모든 프리픽스/태그 처리가 자동으로 됨
                if (TranslationEngine.TryTranslate(trimmed, out string translated, scopes))
                {
                    lines[i] = lines[i].Replace(trimmed, translated);
                    changed = true;
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
