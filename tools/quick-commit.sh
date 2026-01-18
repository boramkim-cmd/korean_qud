#!/bin/bash
# Quick commit script with prompt
# Usage: bash tools/quick-commit.sh

set -e

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

REPO_DIR="/Users/ben/Desktop/qud_korean"
cd "$REPO_DIR"

# Check if there are changes
if git diff-index --quiet HEAD --; then
    echo -e "${YELLOW}No changes to commit${NC}"
    exit 0
fi

echo -e "${BLUE}=== Current Changes ===${NC}"
git status --short

echo -e "\n${BLUE}=== Changed Files Summary ===${NC}"
git diff --stat

echo ""
echo -e "${GREEN}Enter commit message (or press Ctrl+C to cancel):${NC}"
read -r COMMIT_MSG

if [ -z "$COMMIT_MSG" ]; then
    echo -e "${RED}Commit message cannot be empty${NC}"
    exit 1
fi

# Detect commit type based on files changed
if git diff --name-only | grep -q "^Scripts/"; then
    PREFIX="feat"
    if git diff --name-only | grep -q "02_Patches"; then
        PREFIX="fix"
    fi
elif git diff --name-only | grep -q "^LOCALIZATION/"; then
    PREFIX="i18n"
elif git diff --name-only | grep -q "^Docs/"; then
    PREFIX="docs"
elif git diff --name-only | grep -q "^tools/"; then
    PREFIX="chore"
else
    PREFIX="chore"
fi

FULL_MSG="${PREFIX}: ${COMMIT_MSG}"

echo -e "\n${GREEN}=== Running Validation ===${NC}"
if ! python3 tools/project_tool.py; then
    echo -e "${RED}Validation failed!${NC}"
    echo -e "${YELLOW}Do you want to commit anyway? (y/N)${NC}"
    read -r FORCE
    if [ "$FORCE" != "y" ] && [ "$FORCE" != "Y" ]; then
        echo -e "${RED}Commit cancelled${NC}"
        exit 1
    fi
fi

echo -e "\n${GREEN}=== Committing ===${NC}"
echo -e "Message: ${BLUE}${FULL_MSG}${NC}"
git add -A
git commit -m "$FULL_MSG"

echo -e "\n${GREEN}=== Pushing to Remote ===${NC}"
git push origin main

echo -e "\n${GREEN}✓ Successfully committed and pushed!${NC}"
echo -e "Commit: ${BLUE}${FULL_MSG}${NC}"
