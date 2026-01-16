#!/bin/bash

# 파일 감시 및 자동 커밋 스크립트
# 파일 변경 감지 시 자동으로 Git 커밋 + 모드 배포
# 사용법: ./watch-and-sync.sh

# 색상 정의
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  파일 감시 모드 시작${NC}"
echo -e "${BLUE}========================================${NC}"

PROJECT_DIR="/Users/ben/Desktop/qud_korean"
cd "$PROJECT_DIR"

echo -e "\n${YELLOW}감시 중인 폴더:${NC}"
echo -e "- Core_QudKREngine/Scripts/"
echo -e "- Data_QudKRContent/Scripts/"
echo -e "- Data_QudKRContent/Docs/"

echo -e "\n${GREEN}파일 변경 시 자동으로:${NC}"
echo -e "1. Git 커밋 (자동 메시지)"
echo -e "2. GitHub Push"
echo -e "3. 게임 Mods 폴더 업데이트"

echo -e "\n${YELLOW}종료하려면 Ctrl+C를 누르세요${NC}"
echo -e "${BLUE}========================================${NC}\n"

# 마지막 커밋 시간
LAST_COMMIT_TIME=0
COOLDOWN=10  # 10초 쿨다운 (너무 자주 커밋 방지)

# fswatch check
if ! command -v fswatch &> /dev/null; then
    echo -e "${YELLOW}⚠ fswatch가 설치되어 있지 않습니다.${NC}"
    echo -e "${YELLOW}🔄 폴링 모드(5초 주기)로 대체하여 실행합니다...${NC}"
    
    while true; do
        CURRENT_TIME=$(date +%s)
        TIME_DIFF=$((CURRENT_TIME - LAST_COMMIT_TIME))
        
        if [ $TIME_DIFF -ge $COOLDOWN ]; then
            if [ -n "$(git status --porcelain)" ]; then
                echo -e "\n${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
                echo -e "${GREEN}📝 파일 변경 감지! (폴링)${NC}"
                echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
                
                echo -e "\n${YELLOW}변경된 파일:${NC}"
                git status --short
                
                echo -e "\n${BLUE}자동 동기화 시작...${NC}"
                ./sync-and-deploy.sh
                
                LAST_COMMIT_TIME=$(date +%s)
                
                echo -e "\n${GREEN}✓ 자동 동기화 완료!${NC}"
                echo -e "${YELLOW}다음 변경사항을 기다리는 중...${NC}\n"
            fi
        fi
        sleep 5
    done
    exit 0
fi

# 파일 감시 시작
fswatch -0 -r \
    --exclude='.git' \
    --exclude='.DS_Store' \
    --exclude='*.log' \
    --exclude='.vscode' \
    Core_QudKREngine/Scripts \
    Data_QudKRContent/Scripts \
    Data_QudKRContent/Docs \
    | while read -d "" event; do
    
    # 현재 시간
    CURRENT_TIME=$(date +%s)
    TIME_DIFF=$((CURRENT_TIME - LAST_COMMIT_TIME))
    
    # 쿨다운 체크
    if [ $TIME_DIFF -lt $COOLDOWN ]; then
        echo -e "${YELLOW}⏳ 쿨다운 중... (${TIME_DIFF}/${COOLDOWN}초)${NC}"
        continue
    fi
    
    # 변경사항 확인
    if [ -n "$(git status --porcelain)" ]; then
        echo -e "\n${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
        echo -e "${GREEN}📝 파일 변경 감지!${NC}"
        echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
        
        # 변경된 파일 표시
        echo -e "\n${YELLOW}변경된 파일:${NC}"
        git status --short
        
        # 자동 커밋 및 배포
        echo -e "\n${BLUE}자동 동기화 시작...${NC}"
        ./sync-and-deploy.sh
        
        # 마지막 커밋 시간 업데이트
        LAST_COMMIT_TIME=$(date +%s)
        
        echo -e "\n${GREEN}✓ 자동 동기화 완료!${NC}"
        echo -e "${YELLOW}다음 변경사항을 기다리는 중...${NC}\n"
    fi
done
