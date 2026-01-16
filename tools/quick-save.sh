#!/bin/bash

# 빠른 저장 스크립트 (커밋 메시지 자동 생성)
# 사용법: ./quick-save.sh

cd "$(dirname "$0")"

# 변경사항이 없으면 종료
if [ -z "$(git status --porcelain)" ]; then
    echo "✓ 변경사항이 없습니다."
    exit 0
fi

# 자동 커밋 메시지 생성
TIMESTAMP=$(date +"%Y-%m-%d %H:%M:%S")
COMMIT_MSG="chore: quick save - $TIMESTAMP"

# 동기화 스크립트 실행
./sync.sh "$COMMIT_MSG"
