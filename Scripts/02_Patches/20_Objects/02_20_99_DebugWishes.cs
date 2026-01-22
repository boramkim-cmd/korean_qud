/*
 * 파일명: 02_20_99_DebugWishes.cs
 * 분류: [Utility] 디버그 명령어
 * 역할: kr:reload, kr:check, kr:untranslated 등 디버그 명령 제공
 * 작성일: 2026-01-22
 * 비고: Ctrl+W로 게임 내에서 사용 가능
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using XRL;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using UnityEngine;

namespace QudKorean.Objects
{
    /// <summary>
    /// Debug wish commands for testing Object translations.
    /// Access via Ctrl+W in game, then type command.
    /// </summary>
    [HasWishCommand]
    public static class ObjectDebugWishes
    {
        private const string LOG_PREFIX = "[QudKR-Objects]";
        
        /// <summary>
        /// Reloads all Object translation JSON files without restarting the game.
        /// Usage: Ctrl+W → "kr:reload" → Enter
        /// </summary>
        [WishCommand(Command = "kr:reload")]
        public static void ReloadTranslations()
        {
            try
            {
                ObjectTranslator.ReloadJson();
                string stats = ObjectTranslator.GetStats();
                Popup.Show($"Object translations reloaded!\n{stats}");
                UnityEngine.Debug.Log($"{LOG_PREFIX} Translations reloaded: {stats}");
            }
            catch (Exception ex)
            {
                Popup.Show($"Reload failed: {ex.Message}");
                UnityEngine.Debug.LogError($"{LOG_PREFIX} Reload failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Checks translation status for a specific blueprint.
        /// Usage: Ctrl+W → "kr:check Bear" → Enter
        /// </summary>
        [WishCommand(Command = "kr:check")]
        public static void CheckTranslation(string blueprint)
        {
            try
            {
                if (string.IsNullOrEmpty(blueprint))
                {
                    Popup.Show("Usage: kr:check <blueprint>\nExample: kr:check Bear");
                    return;
                }
                
                blueprint = blueprint.Trim();
                
                if (ObjectTranslator.TryGetDisplayName(blueprint, "", out string displayName))
                {
                    string description = "";
                    ObjectTranslator.TryGetDescription(blueprint, out description);
                    
                    Popup.Show($"Blueprint: {blueprint}\n\nDisplayName: {displayName}\n\nDescription: {description ?? "(none)"}");
                }
                else
                {
                    Popup.Show($"No translation found for: {blueprint}");
                }
            }
            catch (Exception ex)
            {
                Popup.Show($"Check failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Lists untranslated objects in the current zone.
        /// Usage: Ctrl+W → "kr:untranslated" → Enter
        /// </summary>
        [WishCommand(Command = "kr:untranslated")]
        public static void ListUntranslated()
        {
            try
            {
                var player = The.Game?.Player?.Body;
                if (player == null)
                {
                    Popup.Show("No player found");
                    return;
                }
                
                var zone = player.CurrentZone;
                if (zone == null)
                {
                    Popup.Show("No zone found");
                    return;
                }
                
                var untranslated = new HashSet<string>();
                var translated = new HashSet<string>();
                
                foreach (var obj in zone.GetObjects())
                {
                    if (obj == player) continue;
                    
                    string blueprint = obj.Blueprint;
                    if (string.IsNullOrEmpty(blueprint)) continue;
                    
                    if (ObjectTranslator.HasTranslation(blueprint))
                        translated.Add(blueprint);
                    else
                        untranslated.Add(blueprint);
                }
                
                string message = $"Zone: {zone.DisplayName}\n\n";
                message += $"✅ Translated ({translated.Count}):\n{string.Join(", ", translated.Take(10))}";
                if (translated.Count > 10) message += $"\n... and {translated.Count - 10} more";
                
                message += $"\n\n❌ Untranslated ({untranslated.Count}):\n{string.Join(", ", untranslated.Take(15))}";
                if (untranslated.Count > 15) message += $"\n... and {untranslated.Count - 15} more";
                
                Popup.Show(message);
            }
            catch (Exception ex)
            {
                Popup.Show($"List failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Shows Object translation system stats.
        /// Usage: Ctrl+W → "kr:stats" → Enter
        /// </summary>
        [WishCommand(Command = "kr:stats")]
        public static void ShowStats()
        {
            try
            {
                string stats = ObjectTranslator.GetStats();
                Popup.Show($"Object Translation Stats:\n\n{stats}");
            }
            catch (Exception ex)
            {
                Popup.Show($"Stats failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Clears the display name cache.
        /// Usage: Ctrl+W → "kr:clearcache" → Enter
        /// </summary>
        [WishCommand(Command = "kr:clearcache")]
        public static void ClearCache()
        {
            try
            {
                ObjectTranslator.ClearCache();
                Popup.Show("Display name cache cleared!");
            }
            catch (Exception ex)
            {
                Popup.Show($"Clear cache failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Investigates TextConsole font system for Korean support.
        /// Usage: Ctrl+W → "kr:fontinfo" → Enter
        /// </summary>
        [WishCommand(Command = "kr:fontinfo")]
        public static void InvestigateFont()
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("=== TextConsole Font Investigation ===\n");

                // 1. ex 타입 조사
                var exType = AccessTools.TypeByName("ConsoleLib.Console.ex");
                if (exType != null)
                {
                    sb.AppendLine($"[ex] Type: {exType.FullName}");

                    // 폰트 관련 필드
                    var fields = exType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    foreach (var f in fields)
                    {
                        string typeName = f.FieldType.Name.ToLower();
                        string fieldName = f.Name.ToLower();
                        if (typeName.Contains("font") || typeName.Contains("sprite") || typeName.Contains("texture") ||
                            fieldName.Contains("font") || fieldName.Contains("char"))
                        {
                            object val = null;
                            try { if (f.IsStatic) val = f.GetValue(null); } catch { }
                            sb.AppendLine($"  {f.Name}: {f.FieldType.Name} = {val?.ToString() ?? "(instance)"}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("[ex] Type NOT FOUND");
                }

                sb.AppendLine();

                // 2. SpriteManager 조사
                var smType = AccessTools.TypeByName("ConsoleLib.Console.SpriteManager");
                if (smType != null)
                {
                    sb.AppendLine($"[SpriteManager] Type: {smType.FullName}");
                    var fields = smType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    foreach (var f in fields)
                    {
                        string typeName = f.FieldType.Name.ToLower();
                        if (typeName.Contains("font") || typeName.Contains("texture") || typeName.Contains("dict"))
                        {
                            sb.AppendLine($"  {f.Name}: {f.FieldType.Name}");
                        }
                    }
                }

                sb.AppendLine();

                // 3. TextConsole 조사
                var tcType = AccessTools.TypeByName("ConsoleLib.Console.TextConsole");
                if (tcType != null)
                {
                    sb.AppendLine($"[TextConsole] Type: {tcType.FullName}");
                    var fields = tcType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    foreach (var f in fields)
                    {
                        string typeName = f.FieldType.Name.ToLower();
                        if (typeName.Contains("font") || typeName.Contains("tmp") || typeName.Contains("text"))
                        {
                            sb.AppendLine($"  {f.Name}: {f.FieldType.Name}");
                        }
                    }
                }

                // 로그에도 출력
                Debug.Log(sb.ToString());
                Popup.Show(sb.ToString());
            }
            catch (Exception ex)
            {
                Popup.Show($"Font investigation failed: {ex.Message}\n\n{ex.StackTrace}");
                Debug.LogError($"{LOG_PREFIX} Font investigation failed: {ex}");
            }
        }
    }
}
