#!/bin/bash

# 완전 자동화 스크립트
# Git 동기화 + 모드 배포를 한 번에!
# 사용법: ./sync-and-deploy.sh "커밋 메시지"

set -e

# 색상 정의
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  완전 자동화: 검증 + Git + 모드 배포${NC}"
echo -e "${BLUE}========================================${NC}"

# 프로젝트 디렉토리로 이동
cd "$(dirname "$0")"

# 0단계: 모드 검증
echo -e "\n${BLUE}[단계 0/3] 모드 검증${NC}"
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

# 1단계: Git 동기화
echo -e "\n${BLUE}[단계 1/3] Git 동기화${NC}"
echo -e "${BLUE}========================================${NC}"

if [ -z "$1" ]; then
    # 커밋 메시지가 없으면 quick-save 사용
    ./quick-save.sh
else
    # 커밋 메시지가 있으면 sync 사용
    ./sync.sh "$1"
fi

# 2단계: 모드 배포
echo -e "\n${BLUE}[단계 2/3] 모드 배포${NC}"
echo -e "${BLUE}========================================${NC}"

./deploy-mods.sh

# 완료
echo -e "\n${BLUE}========================================${NC}"
echo -e "${GREEN}✓ 모든 작업 완료!${NC}"
echo -e "${BLUE}========================================${NC}"

echo -e "\n${YELLOW}완료된 작업:${NC}"
echo -e "1. ✓ Git 커밋 및 Push"
echo -e "2. ✓ 게임 Mods 폴더 업데이트"

echo -e "\n${YELLOW}다음 단계:${NC}"
echo -e "게임을 재시작하면 변경사항이 적용됩니다."
