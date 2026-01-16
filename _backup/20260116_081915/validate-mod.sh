#!/bin/bash

# 고급 모드 검증 스크립트
# 게임 소스 코드를 참조하여 메서드, 네임스페이스, 문법을 검증합니다

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  고급 모드 검증 시작${NC}"
echo -e "${BLUE}========================================${NC}"

PROJECT_DIR="/Users/ben/Desktop/qud_korean"
GAME_SOURCE="/Users/ben/Desktop/qud_korean/Assets/core_source"
ERRORS_FOUND=0
WARNINGS_FOUND=0

# 검증 결과 저장
VALIDATION_LOG="$PROJECT_DIR/validation_report.txt"
echo "모드 검증 보고서 - $(date)" > "$VALIDATION_LOG"
echo "========================================" >> "$VALIDATION_LOG"

# 1. 메서드 존재 확인
echo -e "\n${YELLOW}[1/5] 메서드 존재 확인...${NC}"
echo -e "\n[1/5] 메서드 존재 확인" >> "$VALIDATION_LOG"

# Harmony 패치에서 사용하는 메서드 추출
PATCH_FILES=$(find "$PROJECT_DIR/Data_QudKRContent/Scripts/02_Patches" -name "*.cs" 2>/dev/null)

for patch_file in $PATCH_FILES; do
    # HarmonyPatch 어트리뷰트에서 메서드명 추출
    METHODS=$(grep -o 'HarmonyPatch("[^"]*")' "$patch_file" 2>/dev/null | sed 's/HarmonyPatch("//;s/")//' || true)
    
    # typeof에서 클래스명 추출
    CLASSES=$(grep -o 'typeof([^)]*)' "$patch_file" 2>/dev/null | sed 's/typeof(//;s/)//' || true)
    
    # TargetMethod 존재 여부 확인
    HAS_TARGET_METHOD=$(grep -q "TargetMethod()" "$patch_file" && echo "yes" || echo "no")
    
    if [ "$HAS_TARGET_METHOD" == "yes" ]; then
        echo -e "${BLUE}ℹ $patch_file: TargetMethod() 사용됨 (동적 매칭)${NC}"
        echo "ℹ $patch_file: TargetMethod() 사용됨" >> "$VALIDATION_LOG"
        continue
    fi

    for class in $CLASSES; do
        for method in $METHODS; do
            if [ -n "$class" ] && [ -n "$method" ]; then
                # 게임 소스에서 해당 클래스 파일 찾기
                CLASS_FILE=$(find "$GAME_SOURCE" -name "$class.cs" 2>/dev/null | head -1)
                
                if [ -z "$CLASS_FILE" ]; then
                    CLASS_FILE=$(grep -r "class $class[ \t{]" "$GAME_SOURCE" 2>/dev/null | head -1 | cut -d: -f1 || true)
                fi

                if [ -n "$CLASS_FILE" ]; then
                    # 1. 메서드 이름 존재 확인
                    if ! grep -q "$method\s*(" "$CLASS_FILE" 2>/dev/null; then
                        echo -e "${RED}✗ $class에서 '$method' 메서드를 찾을 수 없음${NC}"
                        echo -e "  파일: $patch_file"
                        
                        echo -e "  ${YELLOW}유사한 메서드:${NC}"
                        grep -i "$method" "$CLASS_FILE" | grep "void\|string\|bool\|int\|ScreenBuffer" | sed 's/^[ \t]*//' | sed 's/{.*//' | head -5 || true
                        
                        ERRORS_FOUND=$((ERRORS_FOUND + 1))
                        continue
                    fi

                    # 2. 시그니처(파라미터) 검증 (어트리뷰트에 리스트가 있는 경우)
                    PARAM_TYPES=$(grep -A 1 "HarmonyPatch(\"$method\"" "$patch_file" | grep "new System.Type\[\]" | grep -o "typeof([^)]*)" | sed 's/typeof(//;s/)//' | tr '\n' ',' | sed 's/,$//' || true)
                    
                    if [ -n "$PARAM_TYPES" ]; then
                        echo -e "${BLUE}ℹ $class.$method 시그니처 검증 중: ($PARAM_TYPES)${NC}"
                        
                        # 게임 소스에서 해당 파라미터 구성을 가진 메서드가 있는지 확인 (매우 단순화된 체크)
                        # 실제로는 쉼표 개수나 타입 순서 등을 더 엄밀히 봐야 함
                        PARAM_COUNT=$(echo "$PARAM_TYPES" | tr -cd ',' | wc -c)
                        PARAM_COUNT=$((PARAM_COUNT + 1))
                        
                        # 소스 코드에서 해당 메서드 라인의 쉼표 개수로 파라미터 수 짐작 (오탐 가능성 있음)
                        SOURCE_METHOD_LINE=$(grep "$method\s*(" "$CLASS_FILE" | grep -v ";" | head -1 || true)
                        SOURCE_PARAM_COUNT=$(echo "$SOURCE_METHOD_LINE" | tr -cd ',' | wc -c)
                        # 파라미터가 n개면 쉼표는 n-1개 (단, 파라미터가 아예 없으면 0개)
                        if [ -n "$SOURCE_METHOD_LINE" ]; then
                            if [[ "$SOURCE_METHOD_LINE" == *"()"* ]]; then
                                EXPECTED_SOURCE_COUNT=0
                            else
                                EXPECTED_SOURCE_COUNT=$((SOURCE_PARAM_COUNT + 1))
                            fi
                            
                            if [ "$PARAM_COUNT" -ne "$EXPECTED_SOURCE_COUNT" ]; then
                                echo -e "${RED}✗ 시그니처 불일치 가능성: $class.$method${NC}"
                                echo -e "  패치 정의: $PARAM_COUNT 개 파라미터 ($PARAM_TYPES)"
                                echo -e "  소스 코드: 약 $EXPECTED_SOURCE_COUNT 개 파라미터"
                                echo -e "  소스 라인: $(echo "$SOURCE_METHOD_LINE" | sed 's/^[ \t]*//')"
                                ERRORS_FOUND=$((ERRORS_FOUND + 1))
                            else
                                echo -e "${GREEN}✓ $class.$method 시그니처 일치 확인${NC}"
                            fi
                        fi
                    else
                        echo -e "${GREEN}✓ $class.$method 존재 확인 (이름 매칭)${NC}"
                    fi
                else
                    echo -e "${YELLOW}⚠ $class 클래스 파일을 찾을 수 없음${NC}"
                fi
            fi
        done
    done
done

# 2. 네임스페이스 검증 (비활성화됨)
echo -e "\n${YELLOW}[2/5] 네임스페이스 검증... (건너뜀)${NC}"
echo -e "\n[2/5] 네임스페이스 검증 (비활성화됨)" >> "$VALIDATION_LOG"

# 알려진 클래스와 올바른 네임스페이스 매핑 (파일 기반)
# MAPPING_FILE="/tmp/class_namespace_map.txt"
# > "$MAPPING_FILE"  # 파일 초기화
# ... (중략) ...
# REQUIRED_NS=$(grep "^$class:" "$MAPPING_FILE" 2>/dev/null | cut -d: -f2 || true)
# ... (중략) ...

# 3. using 지시문 검증
echo -e "\n${YELLOW}[3/5] using 지시문 검증...${NC}"
echo -e "\n[3/5] using 지시문 검증" >> "$VALIDATION_LOG"

ALL_CS_FILES=$(find "$PROJECT_DIR/Data_QudKRContent/Scripts" -name "*.cs" 2>/dev/null)

for cs_file in $ALL_CS_FILES; do
    # 중복 using 확인
    DUPLICATE_USINGS=$(grep "^using " "$cs_file" 2>/dev/null | sort | uniq -d)
    if [ -n "$DUPLICATE_USINGS" ]; then
        echo -e "${YELLOW}⚠ $cs_file에 중복 using 지시문:${NC}"
        echo "$DUPLICATE_USINGS"
        echo "⚠ $cs_file에 중복 using 지시문" >> "$VALIDATION_LOG"
        WARNINGS_FOUND=$((WARNINGS_FOUND + 1))
    fi
    
    # 사용하지 않는 using 확인 (간단한 휴리스틱)
    USINGS=$(grep "^using " "$cs_file" 2>/dev/null | sed 's/using //;s/;$//' || true)
    for using in $USINGS; do
        # System, HarmonyLib, UnityEngine 등은 항상 필요하므로 제외
        if [[ "$using" != "System"* ]] && [[ "$using" != "HarmonyLib"* ]] && [[ "$using" != "UnityEngine"* ]]; then
            # 파일 내용에서 해당 네임스페이스의 클래스 사용 여부 확인
            NS_PREFIX=$(echo "$using" | sed 's/\./\\./g')
            if ! grep -q "$NS_PREFIX\." "$cs_file" 2>/dev/null; then
                echo -e "${YELLOW}⚠ $cs_file에서 $using 사용하지 않을 가능성${NC}"
                WARNINGS_FOUND=$((WARNINGS_FOUND + 1))
            fi
        fi
    done
done

# 4. C# 문법 검증
echo -e "\n${YELLOW}[4/5] C# 문법 검증...${NC}"
echo -e "\n[4/5] C# 문법 검증" >> "$VALIDATION_LOG"

for cs_file in $ALL_CS_FILES; do
    # 네임스페이스 일관성
    if grep -q "namespace QudKRContent" "$cs_file"; then
        echo -e "${RED}✗ $cs_file: 잘못된 네임스페이스 'QudKRContent'${NC}"
        echo -e "  'QudKRTranslation'으로 변경 필요"
        echo "✗ $cs_file: 잘못된 네임스페이스 'QudKRContent'" >> "$VALIDATION_LOG"
        ERRORS_FOUND=$((ERRORS_FOUND + 1))
    fi
    
    # 중괄호 짝 확인 (단순 문자열/주석 무시 시도)
    CLEAN_CONTENT=$(sed -e 's/\/\/.*//' -e 's/"[^"]*"//g' -e "s/'[^']*'//g" "$cs_file" 2>/dev/null)
    OPEN_BRACES=$(echo "$CLEAN_CONTENT" | grep -o "{" | wc -l)
    CLOSE_BRACES=$(echo "$CLEAN_CONTENT" | grep -o "}" | wc -l)
    if [ "$OPEN_BRACES" -ne "$CLOSE_BRACES" ]; then
        echo -e "${RED}✗ $cs_file: 중괄호 불일치 (열림: $OPEN_BRACES, 닫힘: $CLOSE_BRACES)${NC}"
        echo "✗ $cs_file: 중괄호 불일치" >> "$VALIDATION_LOG"
        ERRORS_FOUND=$((ERRORS_FOUND + 1))
    fi
    
    # 세미콜론 누락 확인 (간단한 패턴)
    MISSING_SEMICOLON=$(grep -n "^\s*[a-zA-Z].*[^;{}\"]$" "$cs_file" 2>/dev/null | \
        grep -v "//\|/\*\|\*/\|namespace\|class\|public\|private\|protected\|if\|else\|for\|while\|switch" || true)
    if [ -n "$MISSING_SEMICOLON" ]; then
        echo -e "${YELLOW}⚠ $cs_file: 세미콜론 누락 가능성${NC}"
        WARNINGS_FOUND=$((WARNINGS_FOUND + 1))
    fi
done

# 5. 다른 파일에서 정의 찾기
echo -e "\n${YELLOW}[5/5] 교차 참조 검증...${NC}"
echo -e "\n[5/5] 교차 참조 검증" >> "$VALIDATION_LOG"

# 데이터 클래스 참조 확인
DATA_CLASSES=$(grep -r "public static class.*Data" "$PROJECT_DIR/Data_QudKRContent/Scripts/01_Data" 2>/dev/null | \
    sed 's/.*public static class //;s/ .*//' || true)

for data_class in $DATA_CLASSES; do
    # 패치 파일에서 사용되는지 확인
    USAGE=$(grep -r "$data_class" "$PROJECT_DIR/Data_QudKRContent/Scripts/02_Patches" 2>/dev/null || true)
    
    if [ -z "$USAGE" ]; then
        echo -e "${YELLOW}⚠ $data_class가 패치 파일에서 사용되지 않음${NC}"
        echo "⚠ $data_class가 패치 파일에서 사용되지 않음" >> "$VALIDATION_LOG"
        WARNINGS_FOUND=$((WARNINGS_FOUND + 1))
    else
        echo -e "${GREEN}✓ $data_class 사용 확인${NC}"
    fi
done

# ScopeManager, TranslationEngine, TranslationUtils 참조 확인
CORE_CLASSES=("ScopeManager" "TranslationEngine" "TranslationUtils")
for core_class in "${CORE_CLASSES[@]}"; do
    if ! grep -r "$core_class" "$PROJECT_DIR/Data_QudKRContent/Scripts/02_Patches" >/dev/null 2>&1; then
        echo -e "${RED}✗ $core_class가 패치에서 사용되지 않음${NC}"
        echo "✗ $core_class가 패치에서 사용되지 않음" >> "$VALIDATION_LOG"
        ERRORS_FOUND=$((ERRORS_FOUND + 1))
    fi
done

# 최종 결과
echo -e "\n${BLUE}========================================${NC}"
echo -e "\n========================================" >> "$VALIDATION_LOG"
echo -e "검증 완료" >> "$VALIDATION_LOG"
echo -e "에러: $ERRORS_FOUND개" >> "$VALIDATION_LOG"
echo -e "경고: $WARNINGS_FOUND개" >> "$VALIDATION_LOG"

if [ $ERRORS_FOUND -eq 0 ]; then
    echo -e "${GREEN}✓ 모든 검증 통과!${NC}"
    if [ $WARNINGS_FOUND -gt 0 ]; then
        echo -e "${YELLOW}⚠ $WARNINGS_FOUND개의 경고 (무시 가능)${NC}"
    fi
    echo -e "${BLUE}========================================${NC}"
    echo -e "\n${GREEN}검증 보고서: $VALIDATION_LOG${NC}"
    exit 0
else
    echo -e "${RED}✗ $ERRORS_FOUND개의 에러 발견${NC}"
    if [ $WARNINGS_FOUND -gt 0 ]; then
        echo -e "${YELLOW}⚠ $WARNINGS_FOUND개의 경고${NC}"
    fi
    echo -e "${BLUE}========================================${NC}"
    echo -e "\n${YELLOW}문제를 수정한 후 다시 시도하세요.${NC}"
    echo -e "${GREEN}상세 보고서: $VALIDATION_LOG${NC}"
    exit 1
fi
