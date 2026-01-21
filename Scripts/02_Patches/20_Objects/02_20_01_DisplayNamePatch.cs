// Object Localization System - DisplayName Patch
// Patches GetDisplayNameEvent.GetFor() to translate creature/item names
// ISOLATED from existing translation infrastructure
// Version: 2.0 | Created: 2026-01-22

using System;
using System.Collections.Generic;
using HarmonyLib;
using XRL.World;

namespace QudKorean.Objects
{
    /// <summary>
    /// Harmony patch for GetDisplayNameEvent.GetFor() method.
    /// Translates creature and item display names to Korean.
    /// </summary>
    [HarmonyPatch(typeof(GetDisplayNameEvent))]
    public static class Patch_ObjectDisplayName
    {
        private const string LOG_PREFIX = "[QudKR-Objects]";
        
        /// <summary>
        /// Postfix patch for GetDisplayNameEvent.GetFor().
        /// CRITICAL: Must check ForSort and ColorOnly parameters to avoid breaking sorting/color-only operations.
        /// </summary>
        [HarmonyPatch(nameof(GetDisplayNameEvent.GetFor))]
        [HarmonyPostfix]
        static void GetFor_Postfix(
            ref string __result,
            GameObject Object,
            string Base,
            bool ColorOnly,    // If true, only returns color code - DO NOT translate
            bool ForSort)      // If true, used for sorting - DO NOT translate
        {
            try
            {
                // CRITICAL: Skip translation for special modes
                if (ForSort || ColorOnly) return;
                
                // Basic validation
                if (Object == null || string.IsNullOrEmpty(__result)) return;
                
                string blueprint = Object.Blueprint;
                if (string.IsNullOrEmpty(blueprint)) return;
                
                // Attempt translation
                if (ObjectTranslator.TryGetDisplayName(blueprint, __result, out string translated))
                {
                    __result = translated;
                }
            }
            catch (Exception ex)
            {
                // Never crash the game - log and continue
                UnityEngine.Debug.LogError($"{LOG_PREFIX} GetFor_Postfix error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Clears the translation cache. Called on game load.
        /// </summary>
        public static void ClearCache()
        {
            ObjectTranslator.ClearCache();
        }
        
        /// <summary>
        /// Reloads JSON files and clears cache. Used by kr:reload wish command.
        /// </summary>
        public static void ReloadAndClear()
        {
            ObjectTranslator.ReloadJson();
        }
    }
    
    /// <summary>
    /// Invalidates cache on game load to ensure fresh translations.
    /// </summary>
    [HarmonyPatch(typeof(XRLCore))]
    public static class Patch_CacheInvalidation
    {
        [HarmonyPatch("LoadGame")]
        [HarmonyPostfix]
        static void LoadGame_Postfix()
        {
            Patch_ObjectDisplayName.ClearCache();
        }
    }
}
