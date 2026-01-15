/*
 * File: 00_ModEntry.cs
 * Category: [Core] Mod entrypoint (safe Harmony patch application)
 * Purpose: Initialize Harmony and apply patches safely so one bad patch class doesn't stop all patches.
 * Created: 2026-01-15 (english version)
 */

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation
{
    public class ModEntry
    {
        public static void Main()
        {
            try
            {
                Debug.Log("=================================================");
                Debug.Log("[Qud-KR Translation] Mod initialization starting...");
                Debug.Log("[Qud-KR Translation] Version: 0.2.0 (safe-patcher)");
                Debug.Log("=================================================");

                var harmony = new Harmony("com.boram.qud.translation");

                // Optional: pre-check targets
                VerifyPatchTargets();

                // Apply patches by class type with per-type try/catch
                var assembly = Assembly.GetExecutingAssembly();
                var patchTypes = assembly.GetTypes()
                    .Where(t => t.IsDefined(typeof(HarmonyAttribute), inherit: true))
                    .ToArray();

                Debug.Log($"[Qud-KR Translation] Found {patchTypes.Length} patch classes. Applying per-class...");

                int success = 0;
                foreach (var type in patchTypes)
                {
                    try
                    {
                        harmony.CreateClassProcessor(type).Patch();
                        success++;
                        Debug.Log($"[Qud-KR Translation] ✓ Patched: {type.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Qud-KR Translation] ✗ Patch failed: {type.FullName} => {ex.GetType().Name}: {ex.Message}");
                        Debug.LogException(ex);
                    }
                }

                Debug.Log("=================================================");
                Debug.Log($"[Qud-KR Translation] Safe patching complete: {success}/{patchTypes.Length} classes patched");
                Debug.Log("[Qud-KR Translation] Mod loaded!");
                Debug.Log("=================================================");
            }
            catch (Exception e)
            {
                Debug.LogError("=================================================");
                Debug.LogError("[Qud-KR Translation] ❌ Load failed!");
                Debug.LogError($"[Qud-KR Translation] Error: {e}");
                Debug.LogError("=================================================");
            }
        }

        private static void VerifyPatchTargets()
        {
            // Optional: implement target validation to warn about missing types/methods
            return;
        }
    }
}