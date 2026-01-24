/*
 * 파일명: 02_10_12_Skills.cs
 * 분류: [UI Patch] 스킬 및 파워 번역
 * 역할: SkillFactory에서 로드된 스킬/파워의 이름과 설명을 번역합니다.
 */

using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using XRL;
using XRL.World.Skills;

namespace QudKRTranslation.Patches.UI
{
    /// <summary>
    /// 스킬 로컬라이제이션 데이터 관리자
    /// LOCALIZATION/GAMEPLAY/SKILLS/*.json 파일들을 로드하여 스킬/파워 번역을 제공합니다.
    /// </summary>
    public static class SkillLocalizationManager
    {
        // 스킬 이름 번역: "Axe" -> "도끼"
        private static Dictionary<string, string> _skillNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 스킬 설명 번역: "You are skilled with axes." -> "당신은 도끼에 숙달되어 있습니다."
        private static Dictionary<string, string> _skillDescs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 파워 이름 번역: "axe proficiency" -> "도끼 숙련"
        private static Dictionary<string, string> _powerNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 파워 설명 번역: 영문 설명 -> 한글 설명
        private static Dictionary<string, string> _powerDescs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static bool _isLoaded = false;

        public static void EnsureLoaded()
        {
            if (_isLoaded) return;
            LoadAllSkillFiles();
            _isLoaded = true;
        }

        public static void Reload()
        {
            _skillNames.Clear();
            _skillDescs.Clear();
            _powerNames.Clear();
            _powerDescs.Clear();
            _isLoaded = false;
            EnsureLoaded();
        }

        private static void LoadAllSkillFiles()
        {
            string modDir = GetModDirectory();
            if (string.IsNullOrEmpty(modDir))
            {
                Debug.LogError("[SkillLocalizationManager] Mod directory not found.");
                return;
            }

            string skillsDir = Path.Combine(modDir, "LOCALIZATION/GAMEPLAY/SKILLS");
            if (!Directory.Exists(skillsDir))
            {
                Debug.LogWarning($"[SkillLocalizationManager] Skills directory not found: {skillsDir}");
                return;
            }

            foreach (var file in Directory.GetFiles(skillsDir, "*.json"))
            {
                try
                {
                    LoadSkillFile(file);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SkillLocalizationManager] Failed to load {file}: {e.Message}");
                }
            }

            Debug.Log($"[SkillLocalizationManager] Loaded {_skillNames.Count} skills, {_powerNames.Count} powers");
        }

        private static void LoadSkillFile(string path)
        {
            string json = File.ReadAllText(path);

            // Parse names section
            string namesBlock = ExtractBlock(json, "\"names\"");
            if (!string.IsNullOrEmpty(namesBlock))
            {
                var names = ParseSimpleObject(namesBlock);
                foreach (var kv in names)
                {
                    _skillNames[kv.Key] = kv.Value;
                }
            }

            // Parse description_ko
            string descKo = ExtractStringValue(json, "\"description_ko\"");
            string descEn = ExtractStringValue(json, "\"description\"");
            if (!string.IsNullOrEmpty(descEn) && !string.IsNullOrEmpty(descKo))
            {
                _skillDescs[descEn] = descKo;
            }

            // Parse powers section
            string powersBlock = ExtractBlock(json, "\"powers\"");
            if (!string.IsNullOrEmpty(powersBlock))
            {
                ParsePowers(powersBlock);
            }
        }

        private static void ParsePowers(string powersJson)
        {
            int index = 0;
            SkipWhitespace(powersJson, ref index);
            if (index >= powersJson.Length || powersJson[index] != '{') return;
            index++; // skip {

            while (index < powersJson.Length)
            {
                SkipWhitespace(powersJson, ref index);
                if (index >= powersJson.Length || powersJson[index] == '}') break;

                // Power key (e.g., "axe proficiency")
                string powerKey = ParseString(powersJson, ref index);
                SkipWhitespace(powersJson, ref index);
                if (index < powersJson.Length && powersJson[index] == ':') index++;
                SkipWhitespace(powersJson, ref index);

                // Power value object
                if (index < powersJson.Length && powersJson[index] == '{')
                {
                    string powerBlock = ExtractObjectBlock(powersJson, ref index);

                    string name = ExtractStringValue(powerBlock, "\"name\"");
                    string desc = ExtractStringValue(powerBlock, "\"desc\"");

                    if (!string.IsNullOrEmpty(powerKey))
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            _powerNames[powerKey] = name;
                        }
                        if (!string.IsNullOrEmpty(desc))
                        {
                            _powerDescs[powerKey] = desc;
                        }
                    }
                }

                SkipWhitespace(powersJson, ref index);
                if (index < powersJson.Length && powersJson[index] == ',') index++;
            }
        }

        private static string ExtractObjectBlock(string json, ref int index)
        {
            if (index >= json.Length || json[index] != '{') return "";

            int start = index;
            int depth = 1;
            index++; // skip opening {

            while (index < json.Length && depth > 0)
            {
                if (json[index] == '{') depth++;
                else if (json[index] == '}') depth--;
                else if (json[index] == '"')
                {
                    index++;
                    while (index < json.Length && json[index] != '"')
                    {
                        if (json[index] == '\\') index++;
                        index++;
                    }
                }
                index++;
            }

            return json.Substring(start, index - start);
        }

        private static string ExtractBlock(string json, string key)
        {
            int keyIndex = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0) return null;

            int colonIndex = json.IndexOf(':', keyIndex + key.Length);
            if (colonIndex < 0) return null;

            int braceStart = json.IndexOf('{', colonIndex);
            if (braceStart < 0) return null;

            int depth = 1;
            int i = braceStart + 1;
            while (i < json.Length && depth > 0)
            {
                if (json[i] == '{') depth++;
                else if (json[i] == '}') depth--;
                else if (json[i] == '"')
                {
                    i++;
                    while (i < json.Length && json[i] != '"')
                    {
                        if (json[i] == '\\') i++;
                        i++;
                    }
                }
                i++;
            }

            return json.Substring(braceStart, i - braceStart);
        }

        private static string ExtractStringValue(string json, string key)
        {
            int keyIndex = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0) return null;

            int colonIndex = json.IndexOf(':', keyIndex + key.Length);
            if (colonIndex < 0) return null;

            int quoteStart = json.IndexOf('"', colonIndex + 1);
            if (quoteStart < 0) return null;

            int quoteEnd = quoteStart + 1;
            while (quoteEnd < json.Length)
            {
                if (json[quoteEnd] == '"') break;
                if (json[quoteEnd] == '\\') quoteEnd++;
                quoteEnd++;
            }

            string raw = json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
            return UnescapeString(raw);
        }

        private static Dictionary<string, string> ParseSimpleObject(string json)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int index = 0;
            SkipWhitespace(json, ref index);
            if (index >= json.Length || json[index] != '{') return result;
            index++;

            while (index < json.Length)
            {
                SkipWhitespace(json, ref index);
                if (index >= json.Length || json[index] == '}') break;

                string key = ParseString(json, ref index);
                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == ':') index++;
                SkipWhitespace(json, ref index);
                string value = ParseString(json, ref index);

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    result[key] = value;
                }

                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == ',') index++;
            }

            return result;
        }

        private static string ParseString(string json, ref int index)
        {
            SkipWhitespace(json, ref index);
            if (index >= json.Length || json[index] != '"') return "";
            index++; // skip opening quote

            var sb = new System.Text.StringBuilder();
            while (index < json.Length)
            {
                char c = json[index++];
                if (c == '"') break;
                if (c == '\\' && index < json.Length)
                {
                    char next = json[index++];
                    switch (next)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case 'u':
                            if (index + 4 <= json.Length &&
                                int.TryParse(json.Substring(index, 4), System.Globalization.NumberStyles.HexNumber, null, out int code))
                            {
                                sb.Append((char)code);
                                index += 4;
                            }
                            break;
                        default: sb.Append(next); break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static string UnescapeString(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    char next = s[++i];
                    switch (next)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case 'u':
                            if (i + 4 < s.Length &&
                                int.TryParse(s.Substring(i + 1, 4), System.Globalization.NumberStyles.HexNumber, null, out int code))
                            {
                                sb.Append((char)code);
                                i += 4;
                            }
                            break;
                        default: sb.Append(next); break;
                    }
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }

        private static void SkipWhitespace(string s, ref int index)
        {
            while (index < s.Length && char.IsWhiteSpace(s[index])) index++;
        }

        private static string GetModDirectory()
        {
            try
            {
                var mod = ModManager.GetMod("KoreanLocalization");
                if (mod != null && !string.IsNullOrEmpty(mod.Path))
                {
                    return mod.Path;
                }
            }
            catch { }

            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string modsRoot = Path.Combine(homeDir, "Library/Application Support/com.FreeholdGames.CavesOfQud/Mods");
                string target = Path.Combine(modsRoot, "KoreanLocalization");
                if (Directory.Exists(target)) return target;
            }

            return "";
        }

        // ========== Public API ==========

        public static string GetSkillName(string englishName)
        {
            EnsureLoaded();
            return _skillNames.TryGetValue(englishName, out string korean) ? korean : englishName;
        }

        public static string GetSkillDescription(string englishDesc)
        {
            EnsureLoaded();
            return _skillDescs.TryGetValue(englishDesc, out string korean) ? korean : englishDesc;
        }

        public static string GetPowerName(string englishName)
        {
            EnsureLoaded();
            return _powerNames.TryGetValue(englishName, out string korean) ? korean : englishName;
        }

        public static string GetPowerDescription(string powerKey)
        {
            EnsureLoaded();
            return _powerDescs.TryGetValue(powerKey, out string korean) ? korean : null;
        }

        public static bool TryGetPowerName(string englishName, out string korean)
        {
            EnsureLoaded();
            return _powerNames.TryGetValue(englishName, out korean);
        }

        public static bool TryGetPowerDescription(string powerKey, out string korean)
        {
            EnsureLoaded();
            return _powerDescs.TryGetValue(powerKey, out korean);
        }
    }

    /// <summary>
    /// SkillFactory 패치: 스킬 로드 후 번역 적용
    /// </summary>
    [HarmonyPatch(typeof(SkillFactory))]
    public static class Patch_SkillFactory
    {
        // Factory getter가 호출될 때 (스킬 로딩 후) 번역 적용
        [HarmonyPatch("get_Factory")]
        [HarmonyPostfix]
        static void Factory_Postfix(SkillFactory __result)
        {
            if (__result == null) return;

            // 이미 번역된 경우 스킵 (첫 번째 스킬 이름으로 체크)
            foreach (var skill in __result.SkillList.Values)
            {
                // 한글이 포함되어 있으면 이미 번역됨
                if (ContainsKorean(skill.Name)) return;
                break;
            }

            ApplyTranslations(__result);
        }

        private static bool ContainsKorean(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                if (c >= 0xAC00 && c <= 0xD7A3) return true; // 한글 음절
                if (c >= 0x3131 && c <= 0x318E) return true; // 한글 자모
            }
            return false;
        }

        private static void ApplyTranslations(SkillFactory factory)
        {
            SkillLocalizationManager.EnsureLoaded();

            foreach (var skillPair in factory.SkillList)
            {
                var skill = skillPair.Value;

                // 스킬 이름 번역
                string translatedName = SkillLocalizationManager.GetSkillName(skill.Name);
                if (translatedName != skill.Name)
                {
                    skill.Name = translatedName;
                }

                // 스킬 설명 번역
                if (!string.IsNullOrEmpty(skill.Description))
                {
                    string translatedDesc = SkillLocalizationManager.GetSkillDescription(skill.Description);
                    if (translatedDesc != skill.Description)
                    {
                        skill.Description = translatedDesc;
                    }
                }

                // 파워 번역
                foreach (var power in skill.PowerList)
                {
                    // 파워 이름 번역 (key로 검색)
                    string originalPowerName = power.Name;
                    if (SkillLocalizationManager.TryGetPowerName(originalPowerName, out string translatedPowerName))
                    {
                        power.Name = translatedPowerName;
                    }

                    // 파워 설명 번역 (key로 검색)
                    if (SkillLocalizationManager.TryGetPowerDescription(originalPowerName, out string translatedPowerDesc))
                    {
                        power.Description = translatedPowerDesc;
                    }
                }
            }

            Debug.Log("[QudKR] Skills and Powers translated successfully");
        }
    }
}
