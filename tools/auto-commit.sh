#!/bin/bash
# Auto-commit script for significant code changes
# Usage: bash tools/auto-commit.sh "commit message"

set -e

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if commit message provided
if [ -z "$1" ]; then
    echo -e "${RED}Error: Commit message required${NC}"
    echo "Usage: bash tools/auto-commit.sh \"your commit message\""
    exit 1
fi

COMMIT_MSG="$1"
REPO_DIR="/Users/ben/Desktop/qud_korean"

cd "$REPO_DIR"

# Check if there are changes
if git diff-index --quiet HEAD --; then
    echo -e "${YELLOW}No changes to commit${NC}"
    exit 0
fi

echo -e "${GREEN}=== Git Status ===${NC}"
git status --short

echo -e "\n${GREEN}=== Running Validation ===${NC}"
python3 tools/project_tool.py

if [ $? -ne 0 ]; then
    echo -e "${RED}Validation failed! Fix errors before committing.${NC}"
    exit 1
fi

echo -e "\n${GREEN}=== Staging Changes ===${NC}"
git add -A

echo -e "\n${GREEN}=== Committing ===${NC}"
git commit -m "$COMMIT_MSG"

echo -e "\n${GREEN}=== Pushing to Remote ===${NC}"
git push origin main

echo -e "\n${GREEN}✓ Successfully committed and pushed!${NC}"
