#!/bin/bash

# 모드 검증 스크립트 (v2)
# JSON 문법, C# 기본 검사 수행

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_DIR="$( dirname "$SCRIPT_DIR" )"
ERRORS_FOUND=0
WARNINGS_FOUND=0

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  모드 검증 시작${NC}"
echo -e "  경로: $PROJECT_DIR"
echo -e "${BLUE}========================================${NC}"

# 1. JSON 문법 검증 (필수)
echo -e "\n${YELLOW}[1/3] JSON 문법 검증...${NC}"
JSON_ERRORS=$(python3 -c "
import json
import glob
import sys

errors = []
for f in glob.glob('$PROJECT_DIR/LOCALIZATION/**/*.json', recursive=True):
    try:
        with open(f, 'r', encoding='utf-8') as fp:
            json.load(fp)
    except json.JSONDecodeError as e:
        errors.append(f'{f}: {e.msg} (line {e.lineno})')
    except Exception as e:
        errors.append(f'{f}: {str(e)}')

if errors:
    for e in errors:
        print(e)
    sys.exit(1)
else:
    print('OK')
    sys.exit(0)
" 2>&1)

if [ "$JSON_ERRORS" = "OK" ]; then
    JSON_COUNT=$(find "$PROJECT_DIR/LOCALIZATION" -name "*.json" | wc -l | tr -d ' ')
    echo -e "${GREEN}✓ JSON 검증 통과 ($JSON_COUNT 파일)${NC}"
else
    echo -e "${RED}✗ JSON 문법 오류 발견:${NC}"
    echo "$JSON_ERRORS"
    ERRORS_FOUND=$((ERRORS_FOUND + 1))
fi

# 2. C# 파일 존재 확인
echo -e "\n${YELLOW}[2/3] C# 파일 확인...${NC}"
CS_COUNT=$(find "$PROJECT_DIR/Scripts" -name "*.cs" 2>/dev/null | wc -l | tr -d ' ')
if [ "$CS_COUNT" -gt 0 ]; then
    echo -e "${GREEN}✓ C# 파일 확인 완료 ($CS_COUNT 파일)${NC}"
else
    echo -e "${RED}✗ C# 파일을 찾을 수 없음${NC}"
    ERRORS_FOUND=$((ERRORS_FOUND + 1))
fi

# 3. 필수 파일 확인
echo -e "\n${YELLOW}[3/3] 필수 파일 확인...${NC}"
REQUIRED_FILES=("manifest.json" "Scripts/00_Core/00_00_00_ModEntry.cs")
for req_file in "${REQUIRED_FILES[@]}"; do
    if [ -f "$PROJECT_DIR/$req_file" ]; then
        echo -e "${GREEN}✓ $req_file${NC}"
    else
        echo -e "${RED}✗ $req_file 없음${NC}"
        ERRORS_FOUND=$((ERRORS_FOUND + 1))
    fi
done

# 결과 요약
echo -e "\n${BLUE}========================================${NC}"
if [ $ERRORS_FOUND -eq 0 ]; then
    echo -e "${GREEN}✓ 검증 성공!${NC}"
    exit 0
else
    echo -e "${RED}✗ 검증 실패! (에러 $ERRORS_FOUND개)${NC}"
    exit 1
fi
