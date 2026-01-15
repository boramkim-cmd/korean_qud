```markdown
# Qud-KR: Agent Instructions for Applying Fixes

Purpose
- Provide a clear, runnable set of instructions for another AI agent (or automation) to apply fixes and diagnostics to the Qud-KR mod.
- Main problems to address:
  1. Galaxy native DLL missing -> DllNotFoundException during PlatformManager.Awake
  2. Harmony patch failures due to mixed annotation styles (class-level TargetMethod / TargetMethods / PatchAll combined with member-level [HarmonyPatch("...")])

Summary (high level)
1. Prevent Galaxy initialization from crashing Steam-run instances by installing a small Harmony prefix that skips Galaxy.Awake when appropriate.
2. Make Harmony patch application resilient so a single broken patch class doesn't stop all patches.
3. Detect patch classes that mix class-level dynamic targets (TargetMethod / TargetMethods / [HarmonyPatch]) with member-level [HarmonyPatch("Name")] and propose fixes.

Quick checks (run from repo root)
- Find HarmonyPatch annotations:
  grep -R --line-number '\[HarmonyPatch' Data_QudKRContent/Scripts/02_Patches || true

- Find TargetMethod usage:
  grep -R --line-number 'TargetMethod(' Data_QudKRContent/Scripts/02_Patches || true

- Find class-level HarmonyPatch lines:
  grep -R --line-number '^\s*\[HarmonyPatch' Data_QudKRContent/Scripts/02_Patches || true

Recommended safe procedure (for an agent to execute)
1. Always backup before changing files:
   cp -r Data_QudKRContent Data_QudKRContent.backup.$(date +%s)

2. Apply Galaxy-skip patch
   - Add the provided SteamGalaxyPatch.cs into:
     Data_QudKRContent/Scripts/02_Patches/SteamGalaxyPatch.cs
   - Purpose: Intercept PlatformManager.Awake and call only Steam.Awake (skip Galaxy.Awake) to avoid DllNotFoundException on systems without Galaxy.

3. Make Harmony patch application robust (two options)
   A) Recommended quick fix:
      - Replace PatchAll() usage with a "safe patcher" approach: enumerate patch classes and call CreateClassProcessor(type).Patch() inside try/catch for each type.
      - This prevents one failing class from stopping all patching.
      - File supplied: Data_QudKRContent/Scripts/00_Core/00_ModEntry.cs (safe-patcher variant).
   B) Root fix:
      - Detect and fix patch classes that mix class-level TargetMethod and member-level [HarmonyPatch("...")] annotations and unify them to a single consistent style.

4. Diagnose and optionally auto-correct mixed-style patch classes
   - Use the provided detect_and_fix.sh script to list candidate files and optionally comment out member-level [HarmonyPatch("...")] occurrences (creates backups before changes).
   - Note: automatic fixes are not 100% safe — manual review is required.

5. Run and validate
   - Start the game and inspect logs for:
     - No Galaxy DllNotFound errors
     - Safe-patcher logs showing per-class successes/failures
     - Translation patches (e.g., MainMenu, UITextSkin) functioning in relevant UI flows

Commands for a remote agent to run
- Diagnostics:
  grep -R --line-number '\[HarmonyPatch' Data_QudKRContent/Scripts/02_Patches || true
  grep -R --line-number 'TargetMethod(' Data_QudKRContent/Scripts/02_Patches || true

- Run detection script (dry-run):
  bash Data_QudKRContent/Scripts/02_Patches/detect_and_fix.sh

- Run detection + auto-fix (agent SHOULD require explicit approval):
  bash Data_QudKRContent/Scripts/02_Patches/detect_and_fix.sh --fix

Commit / PR guidance (if agent will create commits)
- Branch name: fix/harmony-galaxy-compat-YYYYMMDD
- Example commit messages:
  - fix: avoid Galaxy DllNotFound on Steam (SteamGalaxyPatch)
  - chore: apply safe Harmony patcher to avoid PatchAll stopping on error
- PR description should include:
  - Why these changes are needed (logs showing the original error)
  - What was changed and where
  - How the change was tested (log snippets, smoke tests)
  - Rollback instructions (backup location and how to restore)

Test checklist (pass criteria)
- No DllNotFoundException for Galaxy during startup.
- ModEntry logs show per-patch-class results (safe-patcher) or "total patches applied" message.
- Several UI flows demonstrate expected translations (e.g., main menu, popups).
- No compile errors in modified .cs files.

Logs and information to collect on failure
- Full startup console/log output up to the failure point.
- The name(s) of the failing patch class(es).
- Grep outputs for [HarmonyPatch] and TargetMethod around the failing files.
- Backed-up original files for review.

Caveats
- Do not rely solely on automated edits for mixed-style patch classes — always request human review after auto changes.
- Harmony versions differ; if other Harmony behavior is in use, adjust patterns and fixes accordingly.

If you want, I can produce a ready-to-commit patch (diff) including these files. Please indicate whether to produce the patch or just provide these files for manual application.
```