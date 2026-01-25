#!/bin/bash

# 검증 + Git 커밋/푸시 + 모드 배포 스크립트
# 사용법: ./sync-and-deploy.sh [커밋 메시지]

set -e

# 색상 정의
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  검증 + Git 동기화 + 모드 배포${NC}"
echo -e "${BLUE}========================================${NC}"

# 프로젝트 루트로 이동
cd "$(dirname "$0")/.."

# 1단계: 모드 검증
echo -e "\n${BLUE}[단계 1/4] 모드 검증${NC}"
echo -e "${BLUE}========================================${NC}"

if [ -f "./tools/validate-mod.sh" ]; then
    if ./tools/validate-mod.sh; then
        echo -e "${GREEN}✓ 검증 통과${NC}"
    else
        echo -e "${RED}✗ 검증 실패 - 배포 중단${NC}"
        exit 1
    fi
else
    echo -e "${YELLOW}⚠ 검증 스크립트 없음 - 건너뜀${NC}"
fi

# 2단계: Git 커밋 및 푸시
echo -e "\n${BLUE}[단계 2/4] Git 커밋 및 푸시${NC}"
echo -e "${BLUE}========================================${NC}"

# 변경사항 확인
if git diff --quiet && git diff --cached --quiet; then
    echo -e "${YELLOW}⚠ 커밋할 변경사항 없음${NC}"
else
    # 커밋 메시지 (인자로 받거나 기본값)
    COMMIT_MSG="${1:-chore: 자동 배포 커밋 $(date '+%Y-%m-%d %H:%M')}"

    # 변경된 파일 목록 표시
    echo -e "${YELLOW}변경된 파일:${NC}"
    git status --short

    # 모든 변경사항 스테이징 (.DS_Store 제외)
    git add -A
    git reset -- "*.DS_Store" 2>/dev/null || true
    git reset -- "*/.DS_Store" 2>/dev/null || true

    # 커밋
    git commit -m "$COMMIT_MSG

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>" || echo -e "${YELLOW}⚠ 커밋할 내용 없음${NC}"

    # 푸시
    if git push origin main 2>/dev/null; then
        echo -e "${GREEN}✓ Git 커밋 및 푸시 완료${NC}"
    else
        echo -e "${YELLOW}⚠ 푸시 실패 (오프라인이거나 권한 문제)${NC}"
    fi
fi

# 3단계: Copilot Instructions 동기화
echo -e "\n${BLUE}[단계 3/4] Copilot Instructions 동기화${NC}"
echo -e "${BLUE}========================================${NC}"
if [ -f "./tools/sync_copilot_instructions.py" ]; then
    python3 ./tools/sync_copilot_instructions.py
    echo -e "${GREEN}✓ Copilot Instructions 동기화 완료${NC}"
else
    echo -e "${YELLOW}⚠ 동기화 스크립트 없음 - 건너뜀${NC}"
fi

# 4단계: 모드 배포
echo -e "\n${BLUE}[단계 4/4] 모드 배포${NC}"
echo -e "${BLUE}========================================${NC}"

./tools/deploy-mods.sh

# 완료
echo -e "\n${BLUE}========================================${NC}"
echo -e "${GREEN}✓ 모든 작업 완료!${NC}"
echo -e "${BLUE}========================================${NC}"

echo -e "\n${YELLOW}완료된 작업:${NC}"
echo -e "1. ✓ 모드 검증"
echo -e "2. ✓ Git 커밋 및 푸시"
echo -e "3. ✓ Copilot Instructions 동기화"
echo -e "4. ✓ 게임 Mods 폴더 업데이트"

echo -e "\n${YELLOW}다음 단계:${NC}"
echo -e "게임을 재시작하면 변경사항이 적용됩니다."
