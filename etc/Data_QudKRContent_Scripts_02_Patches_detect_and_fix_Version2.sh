#!/bin/bash
#
# detect_and_fix.sh
# Purpose: Detect possible Harmony patch-class mixed-style issues in Data_QudKRContent/Scripts/02_Patches
# Warning: --fix mode performs automatic edits and creates backups; manual review required.
#
ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
PATCH_DIR="$ROOT_DIR/02_Patches"

RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m'

echo -e "${GREEN}>> Running Harmony patch diagnostic...${NC}"
echo "Patch dir: $PATCH_DIR"
echo

# 1) Files containing TargetMethod()
echo "1) Files containing TargetMethod():"
TARGET_FILES=$(grep -R --line-number "TargetMethod(" "$PATCH_DIR" 2>/dev/null || true)
if [ -z "$TARGET_FILES" ]; then
  echo "  (none)"
else
  echo "$TARGET_FILES"
fi
echo

# 2) Files containing [HarmonyPatch]
echo "2) Files containing [HarmonyPatch]:"
HP_FILES=$(grep -R --line-number "\[HarmonyPatch" "$PATCH_DIR" 2>/dev/null || true)
if [ -z "$HP_FILES" ]; then
  echo "  (none)"
else
  echo "$HP_FILES"
fi
echo

# 3) Suspected mixed-pattern files (TargetMethod + member-level HarmonyPatch("..."))
echo "3) Suspected mixed-pattern files (TargetMethod + member-level HarmonyPatch):"
CANDIDATES=()
for f in $(find "$PATCH_DIR" -name "*.cs"); do
  has_target=$(grep -q "TargetMethod(" "$f" 2>/dev/null && echo "yes" || echo "no")
  has_member_patch=$(grep -q "^\s*\[HarmonyPatch\s*(\s*\"[^\"]+\"\s*\))" "$f" 2>/dev/null && echo "yes" || echo "no")
  if [ "$has_target" = "yes" ] && [ "$has_member_patch" = "yes" ]; then
    CANDIDATES+=("$f")
  fi
done

if [ ${#CANDIDATES[@]} -eq 0 ]; then
  echo "  (none)"
else
  for f in "${CANDIDATES[@]}"; do
    echo "  - $f"
  done
fi
echo

# 4) Optional automatic remediation
echo "4) Optional automatic remediation:"
echo "   This script will BACKUP each candidate file and comment out member-level [HarmonyPatch(\"...\")] lines if run with --fix"
echo "   To perform remediation, run: $0 --fix"
echo

if [ "$1" = "--fix" ]; then
  if [ ${#CANDIDATES[@]} -eq 0 ]; then
    echo "No candidate files to fix."
    exit 0
  fi
  for f in "${CANDIDATES[@]}"; do
    bak="${f}.bak.$(date +%s)"
    echo "Backing up $f -> $bak"
    cp "$f" "$bak"
    echo "Commenting member-level HarmonyPatch(...) lines in $f"
    # Note: simple perl replacement. Manual review required.
    perl -0777 -pe 's/^\s*\[HarmonyPatch\(\"[^\"]+\"\)\]\s*\n/\/\/ [AUTO-REMOVED] $&/mg' -i "$f"
    echo "Patched $f (backup at $bak)"
  done
  echo -e "${GREEN}Auto-fix completed. Please review changes and recompile.${NC}"
fi

echo -e "${GREEN}>> Diagnostic complete.${NC}"
exit 0