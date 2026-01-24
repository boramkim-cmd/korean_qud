#!/bin/bash
# Update issue status by renaming file
# Usage: bash tools/update-issue-status.sh ISSUE_FILE.md STATUS

set -e

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Check arguments
if [ -z "$1" ] || [ -z "$2" ]; then
    echo -e "${RED}Error: Issue file and status required${NC}"
    echo "Usage: bash tools/update-issue-status.sh ISSUE_FILE.md STATUS"
    echo "Status: wip|clear|blocked|deprecated|active"
    exit 1
fi

ISSUE_FILE="$1"
NEW_STATUS="$2"
ISSUES_DIR="Docs/Issues"

# Normalize status
NEW_STATUS=$(echo "$NEW_STATUS" | tr '[:upper:]' '[:lower:]')

# Validate status
case "$NEW_STATUS" in
    wip|clear|blocked|deprecated|active)
        ;;
    *)
        echo -e "${RED}Error: Invalid status '$NEW_STATUS'${NC}"
        echo "Valid statuses: wip, clear, blocked, deprecated, active"
        exit 1
        ;;
esac

# Get full path
if [[ "$ISSUE_FILE" == Docs/Issues/* ]]; then
    FULL_PATH="$ISSUE_FILE"
else
    FULL_PATH="${ISSUES_DIR}/${ISSUE_FILE}"
fi

# Check if file exists
if [ ! -f "$FULL_PATH" ]; then
    echo -e "${RED}Error: Issue file not found: ${FULL_PATH}${NC}"
    exit 1
fi

# Get basename and directory
DIR=$(dirname "$FULL_PATH")
BASENAME=$(basename "$FULL_PATH")

# Remove existing status prefix
CLEAN_NAME=$(echo "$BASENAME" | sed -E 's/^(WIP_|CLEAR_|BLOCKED_|DEPRECATED_)//g')

# Apply new status prefix
case "$NEW_STATUS" in
    wip)
        NEW_NAME="WIP_${CLEAN_NAME}"
        STATUS_EMOJI="🟡"
        STATUS_TEXT="WIP"
        ;;
    clear)
        NEW_NAME="CLEAR_${CLEAN_NAME}"
        STATUS_EMOJI="🟢"
        STATUS_TEXT="CLEAR"
        ;;
    blocked)
        NEW_NAME="BLOCKED_${CLEAN_NAME}"
        STATUS_EMOJI="🔴"
        STATUS_TEXT="BLOCKED"
        ;;
    deprecated)
        NEW_NAME="DEPRECATED_${CLEAN_NAME}"
        STATUS_EMOJI="⚫"
        STATUS_TEXT="DEPRECATED"
        ;;
    active)
        NEW_NAME="${CLEAN_NAME}"
        STATUS_EMOJI="🟡"
        STATUS_TEXT="Active"
        ;;
esac

NEW_PATH="${DIR}/${NEW_NAME}"

# Check if already has this status
if [ "$FULL_PATH" == "$NEW_PATH" ]; then
    echo -e "${YELLOW}Issue already has status: ${STATUS_TEXT}${NC}"
    exit 0
fi

# Rename file
mv "$FULL_PATH" "$NEW_PATH"
echo -e "${GREEN}✓ Renamed: ${BASENAME}${NC}"
echo -e "${GREEN}      To: ${NEW_NAME}${NC}"

# Update status in file content
UPDATED_DATE=$(date +%Y-%m-%d)
sed -i.bak "s/^\*\*Status\*\*:.*/\*\*Status\*\*: ${STATUS_EMOJI} ${STATUS_TEXT}/" "$NEW_PATH"
sed -i.bak "s/^\*\*Updated\*\*:.*/\*\*Updated\*\*: ${UPDATED_DATE}/" "$NEW_PATH"
rm "${NEW_PATH}.bak"

echo -e "${BLUE}Updated status in file: ${STATUS_EMOJI} ${STATUS_TEXT}${NC}"
echo -e "${BLUE}Updated date: ${UPDATED_DATE}${NC}"

# Update related issues
echo ""
echo -e "${YELLOW}Checking for related issues...${NC}"
ISSUE_ID=$(echo "$CLEAN_NAME" | sed 's/\.md$//')

# Find files that reference this issue
RELATED_FILES=$(grep -l "$ISSUE_ID" "${ISSUES_DIR}"/*.md 2>/dev/null || true)

if [ -n "$RELATED_FILES" ]; then
    echo -e "${GREEN}Found related issues:${NC}"
    for file in $RELATED_FILES; do
        if [ "$file" != "$NEW_PATH" ]; then
            echo "  - $(basename "$file")"
            # Update reference with new status
            sed -i.bak "s/${BASENAME}/${NEW_NAME}/g" "$file"
            rm "${file}.bak"
        fi
    done
else
    echo -e "${YELLOW}No related issues found${NC}"
fi

# Remind to update index
echo ""
echo -e "${YELLOW}Don't forget to:${NC}"
echo "1. Update Docs/Issues/00_INDEX.md"
if [ "$NEW_STATUS" == "clear" ]; then
    echo "2. Update Docs/reference/03_ERROR_LOG.md (if applicable)"
    echo "3. Update Docs/reference/02_CHANGELOG.md"
    echo "4. Commit changes: bash tools/quick-commit.sh"
fi

# Open in VS Code if available
if command -v code &> /dev/null; then
    code "$NEW_PATH"
    echo -e "${GREEN}Opened updated issue in VS Code${NC}"
fi
