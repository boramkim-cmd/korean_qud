```markdown
# Harmony Patch Failure: Diagnosis & Remediation Plan

Observed issues
- Startup logs contain:
  - DllNotFoundException: GalaxyCSharpGlue (thrown during Galaxy initialization)
  - MODERROR: Harmony patch application failure with message like:
    "You cannot combine TargetMethod, TargetMethods or [HarmonyPatchAll] with individual annotations [Hide]"

Likely root causes
1. Galaxy DllNotFoundException
   - PlatformManager.Awake unconditionally calls Galaxy.Awake().
   - On Steam-only installs (or machines without Galaxy libs), Galaxy native DLLs are missing -> DllNotFoundException -> initialization abort/partial startup.

2. Harmony annotation mixing
   - Some patch classes combine class-level dynamic target selection (TargetMethod/TargetMethods/class-level [HarmonyPatch]) with member-level [HarmonyPatch("MethodName")] annotations in the same class.
   - Certain Harmony versions disallow combining those styles in the same class, causing PatchClassProcessor to throw and potentially abort PatchAll().

Detection steps
- List all files with [HarmonyPatch]:
  grep -R --line-number '\[HarmonyPatch' Data_QudKRContent/Scripts/02_Patches || true

- Identify files with TargetMethod():
  grep -R --line-number 'TargetMethod(' Data_QudKRContent/Scripts/02_Patches || true

- Candidate heuristic: files that contain both TargetMethod and member-level HarmonyPatch("...") attributes are likely problematic.

Remediation options (priority)
1. Emergency / Quick (recommended)
   - Add a small Harmony prefix to intercept PlatformManager.Awake and skip Galaxy.Awake, preventing DllNotFoundException from aborting startup.
   - Add a "safe patcher" ModEntry that patches classes individually in try/catch so one broken class won't stop all patches.

2. Medium-term
   - Run detection script to list candidate files that mix styles; manually fix each file to use a single consistent annotation style.

3. Long-term / proper fix
   - Refactor patch classes so each class follows either:
     A) Class-level target style:
        - Use [HarmonyPatch] plus TargetMethod()/TargetMethods() (or class-level [HarmonyPatch(type, "name")]).
        - Member methods should only have [HarmonyPrefix]/[HarmonyPostfix]/[HarmonyTranspiler], and must NOT have member-level [HarmonyPatch("...")].
     B) Method-level style:
        - Remove TargetMethod() and use separate classes (or method-level [HarmonyPatch(typeof(TargetType), "MethodName")]) for each patched method.

Example problematic pattern
```csharp
[HarmonyPatch]
public static class MixedPatch {
    static MethodBase TargetMethod() { return AccessTools.Method(typeof(SomeType), "Show"); }

    [HarmonyPatch("Hide")] // <-- mixing styles — problematic
    [HarmonyPrefix]
    static void Prefix_Hide() { ... }
}
```

Correct patterns
- If using TargetMethod() at class level:
```csharp
[HarmonyPatch]
public static class CleanClassLevelPatch {
    static MethodBase TargetMethod() { return AccessTools.Method(typeof(SomeType), "Show"); }

    [HarmonyPrefix]
    static void Prefix_Show() { ... } // No [HarmonyPatch("...")] on the method
}
```

- Or use method-level classes:
```csharp
[HarmonyPatch(typeof(SomeType), "Show")]
public static class ShowPatch {
    [HarmonyPrefix]
    static void Prefix_Show() { ... }
}

[HarmonyPatch(typeof(SomeType), "Hide")]
public static class HidePatch {
    [HarmonyPrefix]
    static void Prefix_Hide() { ... }
}
```

Automated detection & optional remediation
- The provided detect_and_fix.sh lists candidates and can optionally comment out member-level [HarmonyPatch("...")] attributes (it makes backups first).
- Note: automatic edits can introduce subtle errors. Always review backups and test.

Recommended workflow
1. Add emergency Galaxy-skip patch (SteamGalaxyPatch.cs).
2. Deploy safe ModEntry (type-by-type patch application).
3. Run the detection script, inspect candidate files, and fix them manually (or accept an automated suggestion then review).
4. Run the game, collect logs, verify translations and absence of startup exceptions.

Logging to capture when issues recur
- Full console logs from startup.
- The failing patch class name and stack trace.
- Candidate grep outputs for the failing files.

Conclusion
- Short-term: prevent Galaxy DllNotFoundException from halting startup and make patch application resilient.
- Mid-term: find and fix mixed-style Harmony patch classes to be consistent with the Harmony version in use.
- Long-term: add a CI check or a pre-commit script to detect mixed annotation styles to avoid regression.
```