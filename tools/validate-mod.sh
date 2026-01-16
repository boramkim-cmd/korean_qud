#!/bin/bash

# 고급 모드 검증 스크립트 (Normalized)
# 프로젝트 루트 및 게임 소스 경로를 환경에 맞게 자동 감지합니다.

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# 경로 감지
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_DIR="$( dirname "$SCRIPT_DIR" )"
# 구조 변경 반영: _GameSource, _HotSource, _MiscRoot 등을 포함한 전체 재귀 탐색
GAME_SOURCE="$PROJECT_DIR/Assets/core_source"
ERRORS_FOUND=0
WARNINGS_FOUND=0

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  고급 모드 검증 시작 (Normalized)${NC}"
echo -e "  경로: $PROJECT_DIR"
echo -e "${BLUE}========================================${NC}"

# 검증 결과 저장
VALIDATION_LOG="$PROJECT_DIR/validation_report.txt"
echo "모드 검증 보고서 - $(date)" > "$VALIDATION_LOG"
echo "========================================" >> "$VALIDATION_LOG"

# 1. 메서드 및 클래스 확인
echo -e "\n${YELLOW}[1/3] Harmony 패치 메서드 확인...${NC}"
PATCH_FILES=$(find "$PROJECT_DIR/Scripts" -name "*.cs" 2>/dev/null | grep -v "_Legacy")

for patch_file in $PATCH_FILES; do
    rel_file=$(echo "$patch_file" | sed "s|$PROJECT_DIR/||")
    
    # HarmonyPatch 어트리뷰트에서 메서드명 추출
    METHODS=$(grep -o 'HarmonyPatch("[^"]*")' "$patch_file" 2>/dev/null | sed 's/HarmonyPatch("//;s/")//' || true)
    CLASSES=$(grep -o 'typeof([^)]*)' "$patch_file" 2>/dev/null | sed 's/typeof(//;s/)//' || true)
    
    if grep -q "TargetMethod()" "$patch_file"; then
        continue # TargetMethod 사용 시 수동 검증 생략
    fi

    for class in $CLASSES; do
        for method in $METHODS; do
            if [ -n "$class" ] && [ -n "$method" ]; then
                # 소스 코드에서 해당 클래스 파일 찾기
                CLASS_FILE=$(find "$GAME_SOURCE" -name "$class.cs" 2>/dev/null | head -1)
                if [ -z "$CLASS_FILE" ]; then
                    CLASS_FILE=$(grep -l "class $class" "$GAME_SOURCE" 2>/dev/null | head -1 || true)
                fi

                if [ -n "$CLASS_FILE" ]; then
                    if ! grep -q "$method\s*(" "$CLASS_FILE" 2>/dev/null; then
                        echo -e "${RED}✗ $class.$method 를 찾을 수 없음 (${rel_file})${NC}"
                        ERRORS_FOUND=$((ERRORS_FOUND + 1))
                    fi
                fi
            fi
        done
    done
done

# 2. 통합 도구 실행 (Python 기반 검증)
echo -e "\n${YELLOW}[2/3] 통합 프로젝트 도구 실행...${NC}"
if python3 "$SCRIPT_DIR/project_tool.py" > /dev/null; then
    echo -e "${GREEN}✓ 통합 도구 검증 통과${NC}"
else
    echo -e "${RED}✗ 통합 도구 검증 실패 (중괄호 또는 JSON 오류)${NC}"
    ERRORS_FOUND=$((ERRORS_FOUND + 1))
fi

# 3. 네임스페이스 및 기타 규칙
echo -e "\n${YELLOW}[3/3] 코딩 규칙 확인...${NC}"
for cs_file in $PATCH_FILES; do
    rel_file=$(echo "$cs_file" | sed "s|$PROJECT_DIR/||")
    # 네임스페이스 확인
    if ! grep -q "namespace QudKRTranslation" "$cs_file"; then
        echo -e "${YELLOW}⚠ $rel_file: 권장 네임스페이스(QudKRTranslation) 사용 안 함${NC}"
        WARNINGS_FOUND=$((WARNINGS_FOUND + 1))
    fi
done

# 결과 요약
echo -e "\n${BLUE}========================================${NC}"
if [ $ERRORS_FOUND -eq 0 ]; then
    echo -e "${GREEN}✓ 검증 성공! (에러 $ERRORS_FOUND, 경고 $WARNINGS_FOUND)${NC}"
    exit 0
else
    echo -e "${RED}✗ 검증 실패! (에러 $ERRORS_FOUND, 경고 $WARNINGS_FOUND)${NC}"
    exit 1
fi
