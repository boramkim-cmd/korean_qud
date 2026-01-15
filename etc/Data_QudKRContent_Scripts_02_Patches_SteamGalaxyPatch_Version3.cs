/*
 * File: SteamGalaxyPatch.cs
 * Category: [Patch] Skip Galaxy initialization when running on Steam-only environments
 * Purpose: Prefix PlatformManager.Awake to call Steam.Awake only and skip Galaxy.Awake to avoid DllNotFoundException.
 * Created: 2026-01-15 (english version)
 */

using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch]
    public static class SteamGalaxyPatch
    {
        // Target PlatformManager.Awake()
        static MethodBase TargetMethod()
        {
            var t = AccessTools.TypeByName("PlatformManager") 
                    ?? AccessTools.TypeByName("XRL.PlatformManager") 
                    ?? AccessTools.TypeByName("Game.PlatformManager");
            return t != null ? AccessTools.Method(t, "Awake") : null;
        }

        // Prefix: call Steam.Awake() if available and skip original (to avoid Galaxy.Awake)
        [HarmonyPrefix]
        static bool Prefix()
        {
            try
            {
                var pmType = AccessTools.TypeByName("PlatformManager");
                if (pmType == null)
                {
                    Debug.Log("[Qud-KR] SteamGalaxyPatch: Could not find PlatformManager type.");
                    return true; // allow original to run, safe fallback
                }

                var steamField = AccessTools.Field(pmType, "Steam");
                if (steamField == null)
                {
                    Debug.Log("[Qud-KR] SteamGalaxyPatch: PlatformManager.Steam field not found.");
                    return true;
                }

                var steamObj = steamField.GetValue(null);
                if (steamObj == null)
                {
                    Debug.Log("[Qud-KR] SteamGalaxyPatch: Steam instance is null.");
                    return true;
                }

                var steamAwake = AccessTools.Method(steamObj.GetType(), "Awake");
                steamAwake?.Invoke(steamObj, null);

                Debug.Log("[Qud-KR] SteamGalaxyPatch: Performed Steam initialization only; skipped Galaxy initialization.");
                return false; // skip original to prevent Galaxy.Awake()
            }
            catch (Exception ex)
            {
                Debug.LogError("[Qud-KR] SteamGalaxyPatch exception: " + ex);
                return true; // on exception, allow original to run
            }
        }
    }
}