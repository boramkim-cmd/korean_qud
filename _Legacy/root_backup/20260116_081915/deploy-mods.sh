#!/bin/bash

# 모드 자동 배포 스크립트
# 작업 폴더 → 게임 Mods 폴더 자동 복사

set -e

# 색상 정의
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  모드 자동 배포${NC}"
echo -e "${BLUE}========================================${NC}"

# 프로젝트 디렉토리
PROJECT_DIR="/Users/ben/Desktop/qud_korean"
MODS_DIR="/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods"

# 1. Core_QudKREngine 복사
echo -e "\n${YELLOW}[1/2] Core_QudKREngine 배포 중...${NC}"
if [ -d "$PROJECT_DIR/Core_QudKREngine" ]; then
    cp -r "$PROJECT_DIR/Core_QudKREngine" "$MODS_DIR/"
    echo -e "${GREEN}✓ Core_QudKREngine 배포 완료${NC}"
else
    echo -e "${YELLOW}⚠ Core_QudKREngine 폴더를 찾을 수 없습니다${NC}"
fi

# 2. Data_QudKRContent 복사
echo -e "\n${YELLOW}[2/2] Data_QudKRContent 배포 중...${NC}"
if [ -d "$PROJECT_DIR/Data_QudKRContent" ]; then
    # 기존 폴더 삭제 후 복사 (깔끔한 배포)
    rm -rf "$MODS_DIR/QudKR_Translation"
    cp -r "$PROJECT_DIR/Data_QudKRContent" "$MODS_DIR/QudKR_Translation"
    echo -e "${GREEN}✓ Data_QudKRContent → QudKR_Translation 배포 완료${NC}"
else
    echo -e "${YELLOW}⚠ Data_QudKRContent 폴더를 찾을 수 없습니다${NC}"
fi

# 완료
echo -e "\n${BLUE}========================================${NC}"
echo -e "${GREEN}✓ 모드 배포 완료!${NC}"
echo -e "${BLUE}========================================${NC}"

echo -e "\n${YELLOW}배포 위치:${NC}"
echo -e "$MODS_DIR/Core_QudKREngine"
echo -e "$MODS_DIR/QudKR_Translation"

echo -e "\n${YELLOW}다음 단계:${NC}"
echo -e "1. Caves of Qud 실행"
echo -e "2. Mods 메뉴에서 모드 확인"
echo -e "3. 게임 재시작 (변경사항 적용)"
