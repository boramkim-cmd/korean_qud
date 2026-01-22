#!/bin/bash

# 검증 + 모드 배포 스크립트
# 사용법: ./sync-and-deploy.sh

set -e

# 색상 정의
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  검증 + 모드 배포${NC}"
echo -e "${BLUE}========================================${NC}"

# 프로젝트 루트로 이동
cd "$(dirname "$0")/.."

# 1단계: 모드 검증
echo -e "\n${BLUE}[단계 1/3] 모드 검증${NC}"
echo -e "${BLUE}========================================${NC}"

if [ -f "./validate-mod.sh" ]; then
    if ./validate-mod.sh; then
        echo -e "${GREEN}✓ 검증 통과${NC}"
    else
        echo -e "${RED}✗ 검증 실패 - 배포 중단${NC}"
        exit 1
    fi
else
    echo -e "${YELLOW}⚠ 검증 스크립트 없음 - 건너뜀${NC}"
fi

# 2단계: Copilot Instructions 동기화
echo -e "\n${BLUE}[단계 2/3] Copilot Instructions 동기화${NC}"
echo -e "${BLUE}========================================${NC}"
if [ -f "./tools/sync_copilot_instructions.py" ]; then
    python3 ./tools/sync_copilot_instructions.py
    echo -e "${GREEN}✓ Copilot Instructions 동기화 완료${NC}"
else
    echo -e "${YELLOW}⚠ 동기화 스크립트 없음 - 건너뜀${NC}"
fi

# 3단계: 모드 배포
echo -e "\n${BLUE}[단계 3/3] 모드 배포${NC}"
echo -e "${BLUE}========================================${NC}"

./tools/deploy-mods.sh

# 완료
echo -e "\n${BLUE}========================================${NC}"
echo -e "${GREEN}✓ 모든 작업 완료!${NC}"
echo -e "${BLUE}========================================${NC}"

echo -e "\n${YELLOW}완료된 작업:${NC}"
echo -e "1. ✓ 모드 검증"
echo -e "2. ✓ Copilot Instructions 동기화"
echo -e "3. ✓ 게임 Mods 폴더 업데이트"

echo -e "\n${YELLOW}다음 단계:${NC}"
echo -e "게임을 재시작하면 변경사항이 적용됩니다."
