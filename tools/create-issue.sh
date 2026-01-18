#!/bin/bash
# Create new issue document with template
# Usage: bash tools/create-issue.sh "Short Description" [priority] [category]

set -e

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Check arguments
if [ -z "$1" ]; then
    echo -e "${RED}Error: Issue description required${NC}"
    echo "Usage: bash tools/create-issue.sh \"Short Description\" [priority] [category]"
    echo "Priority: critical|high|medium|low (default: medium)"
    echo "Category: bug|feature|enhancement|documentation (default: bug)"
    exit 1
fi

DESCRIPTION="$1"
PRIORITY="${2:-medium}"
CATEGORY="${3:-bug}"
DATE=$(date +%Y%m%d)
PRETTY_DATE=$(date +%Y-%m-%d)

# Convert description to filename format
FILENAME=$(echo "$DESCRIPTION" | tr '[:upper:]' '[:lower:]' | tr ' ' '_' | tr -cd '[:alnum:]_')
FILEPATH="Docs/Issues/ISSUE_${DATE}_${FILENAME}.md"

# Check if file already exists
if [ -f "$FILEPATH" ]; then
    echo -e "${YELLOW}Issue already exists: ${FILEPATH}${NC}"
    exit 1
fi

# Capitalize first letter for display
TITLE=$(echo "$DESCRIPTION" | awk '{for(i=1;i<=NF;i++)sub(/./,toupper(substr($i,1,1)),$i)}1')

# Create issue document
cat > "$FILEPATH" << EOF
# Issue: ${TITLE}

**Status**: 🟡 Active  
**Created**: ${PRETTY_DATE}  
**Updated**: ${PRETTY_DATE}  
**Priority**: ${PRIORITY^}  
**Category**: ${CATEGORY^}  
**Related Issues**: None

---

## Problem Description

<!-- Clear description of the issue -->
- **What is not working**: 
- **Expected behavior**: 
- **Actual behavior**: 
- **Impact/Severity**: 

---

## Steps to Reproduce (if bug)

1. 
2. 
3. 

**Environment**:
- Game Version: 
- Mod Version: 
- Platform: macOS/Windows

---

## Root Cause Analysis

<!-- Technical explanation of why the issue occurs -->

**Investigation Notes**:
- 


**Relevant Code**:
\`\`\`csharp
// Code snippet
\`\`\`

---

## Solution Approach

### Attempted Solutions

| Attempt | Description | Result | Reason |
|---------|-------------|--------|--------|
| 1 |  | ⏳ Pending |  |

### Final Solution

<!-- Detailed explanation of the working solution -->

---

## Implementation

### Files Modified

- \`path/to/file.cs\` - Description

### Code Changes

\`\`\`csharp
// Before


// After

\`\`\`

---

## Verification Checklist

- [ ] Code compiles without errors
- [ ] Game launches successfully
- [ ] Issue is resolved in-game
- [ ] No regressions introduced
- [ ] Related tests pass
- [ ] Documentation updated
- [ ] ERROR_LOG.md updated (if applicable)
- [ ] CHANGELOG.md updated (if applicable)

---

## Related Issues

<!-- List related issues with their status -->
- 

---

## Lessons Learned

<!-- Key insights and patterns discovered -->

---

## Next Steps

- [ ] 
- [ ] 
- [ ] 

---

## Notes

<!-- Additional context, references, or links -->

EOF

echo -e "${GREEN}✓ Created issue: ${FILEPATH}${NC}"
echo -e "${BLUE}Priority: ${PRIORITY^} | Category: ${CATEGORY^}${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Edit the issue file with details"
echo "2. Update Docs/Issues/00_INDEX.md"
echo "3. Link to ERROR_LOG.md if error-related"

# Open in VS Code if available
if command -v code &> /dev/null; then
    code "$FILEPATH"
    echo -e "${GREEN}Opened in VS Code${NC}"
fi
