#!/bin/bash

# Git 자동 동기화 스크립트
# 사용법: ./sync.sh "커밋 메시지"

set -e  # 에러 발생 시 중단

# 색상 정의
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  Git 자동 동기화 시작${NC}"
echo -e "${BLUE}========================================${NC}"

# 프로젝트 디렉토리로 이동
cd "$(dirname "$0")"

# 1. 현재 상태 확인
echo -e "\n${YELLOW}[1/5] 현재 상태 확인...${NC}"
git status --short

# 변경사항이 없으면 종료
if [ -z "$(git status --porcelain)" ]; then
    echo -e "${GREEN}✓ 변경사항이 없습니다.${NC}"
    exit 0
fi

# 2. 모든 변경사항 스테이징
echo -e "\n${YELLOW}[2/5] 변경사항 스테이징...${NC}"
git add .
echo -e "${GREEN}✓ 스테이징 완료${NC}"

# 3. 커밋
echo -e "\n${YELLOW}[3/5] 커밋 생성...${NC}"
if [ -z "$1" ]; then
    # 커밋 메시지가 없으면 자동 생성
    TIMESTAMP=$(date +"%Y-%m-%d %H:%M:%S")
    COMMIT_MSG="chore: auto sync - $TIMESTAMP"
else
    COMMIT_MSG="$1"
fi

git commit -m "$COMMIT_MSG"
echo -e "${GREEN}✓ 커밋 완료: $COMMIT_MSG${NC}"

# 4. 원격 저장소에서 최신 변경사항 가져오기
echo -e "\n${YELLOW}[4/5] 원격 저장소 동기화...${NC}"
if git pull --rebase origin main; then
    echo -e "${GREEN}✓ Pull 완료${NC}"
else
    echo -e "${RED}✗ Pull 실패 - 충돌 해결 필요${NC}"
    exit 1
fi

# 5. Push
echo -e "\n${YELLOW}[5/5] 원격 저장소에 Push...${NC}"
if git push origin main; then
    echo -e "${GREEN}✓ Push 완료${NC}"
else
    echo -e "${RED}✗ Push 실패${NC}"
    exit 1
fi

# 완료
echo -e "\n${BLUE}========================================${NC}"
echo -e "${GREEN}✓ 동기화 완료!${NC}"
echo -e "${BLUE}========================================${NC}"

# 최종 상태 표시
echo -e "\n${YELLOW}최근 커밋:${NC}"
git log --oneline -3

echo -e "\n${YELLOW}원격 저장소:${NC}"
echo -e "https://github.com/codekkj/qud_korean"
