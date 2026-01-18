#!/bin/bash
# List issues by status
# Usage: bash tools/list-issues.sh [status]

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

ISSUES_DIR="Docs/Issues"
FILTER_STATUS="${1:-all}"

# Normalize filter
FILTER_STATUS=$(echo "$FILTER_STATUS" | tr '[:upper:]' '[:lower:]')

echo -e "${CYAN}=== Issue Status Summary ===${NC}"
echo ""

# Count by status
ACTIVE_COUNT=$(ls -1 "${ISSUES_DIR}"/ISSUE_*.md 2>/dev/null | grep -v "^${ISSUES_DIR}/\(WIP\|CLEAR\|BLOCKED\|DEPRECATED\)_" | wc -l | tr -d ' ')
WIP_COUNT=$(ls -1 "${ISSUES_DIR}"/WIP_ISSUE_*.md 2>/dev/null | wc -l | tr -d ' ')
CLEAR_COUNT=$(ls -1 "${ISSUES_DIR}"/CLEAR_ISSUE_*.md 2>/dev/null | wc -l | tr -d ' ')
BLOCKED_COUNT=$(ls -1 "${ISSUES_DIR}"/BLOCKED_ISSUE_*.md 2>/dev/null | wc -l | tr -d ' ')
DEPRECATED_COUNT=$(ls -1 "${ISSUES_DIR}"/DEPRECATED_ISSUE_*.md 2>/dev/null | wc -l | tr -d ' ')
TOTAL_COUNT=$((ACTIVE_COUNT + WIP_COUNT + CLEAR_COUNT + BLOCKED_COUNT + DEPRECATED_COUNT))

echo -e "${YELLOW}Total Issues: ${TOTAL_COUNT}${NC}"
echo -e "  🟡 Active: ${ACTIVE_COUNT}"
echo -e "  🟡 WIP: ${WIP_COUNT}"
echo -e "  🟢 Clear: ${CLEAR_COUNT}"
echo -e "  🔴 Blocked: ${BLOCKED_COUNT}"
echo -e "  ⚫ Deprecated: ${DEPRECATED_COUNT}"
echo ""

# Function to display issue info
display_issue() {
    local file="$1"
    local status_emoji="$2"
    local basename=$(basename "$file")
    
    # Extract date and description
    local date_part=$(echo "$basename" | sed -E 's/.*ISSUE_([0-9]{8}).*/\1/')
    local desc_part=$(echo "$basename" | sed -E 's/.*ISSUE_[0-9]{8}_(.*)\.md/\1/' | tr '_' ' ')
    
    # Extract priority and category from file
    local priority=$(grep "^\*\*Priority\*\*:" "$file" 2>/dev/null | sed 's/\*\*Priority\*\*: //' | tr -d ' ')
    local category=$(grep "^\*\*Category\*\*:" "$file" 2>/dev/null | sed 's/\*\*Category\*\*: //' | tr -d ' ')
    
    # Format date
    local year=${date_part:0:4}
    local month=${date_part:4:2}
    local day=${date_part:6:2}
    local formatted_date="${year}-${month}-${day}"
    
    echo -e "${status_emoji} ${BLUE}${formatted_date}${NC} | ${priority} | ${category} | ${desc_part}"
    echo -e "   ${CYAN}${basename}${NC}"
}

# Display issues based on filter
case "$FILTER_STATUS" in
    active)
        echo -e "${YELLOW}=== Active Issues ===${NC}"
        for file in "${ISSUES_DIR}"/ISSUE_*.md; do
            if [[ "$file" == */ISSUE_*.md ]] && [[ ! "$file" =~ (WIP|CLEAR|BLOCKED|DEPRECATED)_ISSUE ]]; then
                [ -f "$file" ] && display_issue "$file" "🟡"
            fi
        done
        ;;
    wip)
        echo -e "${YELLOW}=== Work In Progress ===${NC}"
        for file in "${ISSUES_DIR}"/WIP_ISSUE_*.md; do
            [ -f "$file" ] && display_issue "$file" "🟡"
        done
        ;;
    clear|resolved)
        echo -e "${GREEN}=== Resolved Issues ===${NC}"
        for file in "${ISSUES_DIR}"/CLEAR_ISSUE_*.md; do
            [ -f "$file" ] && display_issue "$file" "🟢"
        done
        ;;
    blocked)
        echo -e "${RED}=== Blocked Issues ===${NC}"
        for file in "${ISSUES_DIR}"/BLOCKED_ISSUE_*.md; do
            [ -f "$file" ] && display_issue "$file" "🔴"
        done
        ;;
    deprecated)
        echo -e "${CYAN}=== Deprecated Issues ===${NC}"
        for file in "${ISSUES_DIR}"/DEPRECATED_ISSUE_*.md; do
            [ -f "$file" ] && display_issue "$file" "⚫"
        done
        ;;
    all|*)
        echo -e "${YELLOW}=== Active Issues ===${NC}"
        for file in "${ISSUES_DIR}"/ISSUE_*.md; do
            if [[ "$file" == */ISSUE_*.md ]] && [[ ! "$file" =~ (WIP|CLEAR|BLOCKED|DEPRECATED)_ISSUE ]]; then
                [ -f "$file" ] && display_issue "$file" "🟡"
            fi
        done
        
        echo ""
        echo -e "${YELLOW}=== Work In Progress ===${NC}"
        for file in "${ISSUES_DIR}"/WIP_ISSUE_*.md; do
            [ -f "$file" ] && display_issue "$file" "🟡"
        done
        
        echo ""
        echo -e "${GREEN}=== Resolved Issues ===${NC}"
        for file in "${ISSUES_DIR}"/CLEAR_ISSUE_*.md; do
            [ -f "$file" ] && display_issue "$file" "🟢"
        done
        
        if [ "$BLOCKED_COUNT" -gt 0 ]; then
            echo ""
            echo -e "${RED}=== Blocked Issues ===${NC}"
            for file in "${ISSUES_DIR}"/BLOCKED_ISSUE_*.md; do
                [ -f "$file" ] && display_issue "$file" "🔴"
            done
        fi
        
        if [ "$DEPRECATED_COUNT" -gt 0 ]; then
            echo ""
            echo -e "${CYAN}=== Deprecated Issues ===${NC}"
            for file in "${ISSUES_DIR}"/DEPRECATED_ISSUE_*.md; do
                [ -f "$file" ] && display_issue "$file" "⚫"
            done
        fi
        ;;
esac

echo ""
