/*
 * 파일명: 00_01_P_SteamGalaxy.cs
 * 분류: [Core Patch] 플랫폼 초기화 수정
 * 역할: 스팀 환경에서 GOG Galaxy 초기화 중 오류가 발생하는 것을 방지하기 위해 Galaxy 초기화를 건너뛰고 Steam만 초기화합니다.
 * 
 * [FIX Issue 15] NOTE: This patch is NOT related to localization.
 * It exists to prevent Steam users from encountering GOG Galaxy initialization errors.
 * This is a platform compatibility fix that was bundled with this mod for convenience.
 * Consider moving to a separate utility mod if maintaining separation of concerns.
 * 
 * To disable this patch: Remove the [HarmonyPatch] attribute or delete this file.
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
