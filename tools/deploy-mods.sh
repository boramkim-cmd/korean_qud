#!/bin/bash

# 모드 배포 스크립트 (단방향: Desktop → Mods)
# Desktop/qud_korean이 유일한 소스
# Mods 폴더는 게임 실행용 복사본

set -e

# 색상 정의
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}========================================"
echo -e "  모드 배포 (Desktop → Mods)"
echo -e "========================================${NC}"

# 경로 설정

# 경로 설정
SOURCE_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )/.." && pwd )"
GAME_MOD=$(python3 "$SOURCE_DIR/tools/get_deploy_path.py")

echo -e "${BLUE}설정:${NC} ${GAME_MOD}"

# 게임 모드 폴더 존재 확인
if [ ! -d "$GAME_MOD" ]; then
    echo -e "${RED}✗ 게임 모드 폴더 없음: $GAME_MOD${NC}"
    echo -e "${YELLOW}💡 tools/config.json을 생성하여 경로를 지정할 수 있습니다. (tools/config.json.example 참조)${NC}"
    exit 1
fi

echo -e "\n${BLUE}소스:${NC} $SOURCE_DIR"
echo -e "${BLUE}대상:${NC} $GAME_MOD"

# ========================================
# Step 1: 개발 파일 정리 (Mods에서 제거)
# ========================================
echo -e "\n${YELLOW}[1/4] 개발 파일 정리...${NC}"

# 삭제할 개발 파일/폴더 (전체 트리에서 검색)
patterns_to_remove=(
    "_Legacy" "_archive" "_backup" "_Docs_Archive" "Docs" "Assets" "tools" 
    "Fonts" ".git" ".gitignore" ".gitattributes" "*.md" "*.sh" "*.code-workspace"
    "project_metadata.json" "README*" "Scripts_backup" "config.json" "config.json.example"
)

for pattern in "${patterns_to_remove[@]}"; do
    find "$GAME_MOD" -name "$pattern" -exec rm -rf {} + 2>/dev/null || true
done

echo -e "${GREEN}✓ 개발 파일 및 레거시 폴더 제거 완료${NC}"

# ========================================
# Step 2: Scripts 배포 (_Legacy 제외)
# ========================================
echo -e "\n${YELLOW}[2/4] Scripts 배포...${NC}"
if [ -d "$SOURCE_DIR/Scripts" ]; then
    rsync -a --delete --exclude='_Legacy' "$SOURCE_DIR/Scripts/" "$GAME_MOD/Scripts/"
    script_count=$(find "$SOURCE_DIR/Scripts" -name "*.cs" -not -path "*/_Legacy/*" | wc -l | tr -d ' ')
    echo -e "${GREEN}✓ ${script_count}개 .cs 파일${NC}"
else
    echo -e "${RED}✗ Scripts 폴더 없음${NC}"
    exit 1
fi

# ========================================
# Step 3: LOCALIZATION 배포 (_archive 제외)
# ========================================
echo -e "\n${YELLOW}[3/4] LOCALIZATION 배포...${NC}"
if [ -d "$SOURCE_DIR/LOCALIZATION" ]; then
    mkdir -p "$GAME_MOD/LOCALIZATION"
    rsync -a --delete --exclude='_archive' "$SOURCE_DIR/LOCALIZATION/" "$GAME_MOD/LOCALIZATION/"
    json_count=$(find "$SOURCE_DIR/LOCALIZATION" -name "*.json" -not -path "*/_archive/*" | wc -l | tr -d ' ')
    echo -e "${GREEN}✓ ${json_count}개 .json 파일${NC}"
else
    echo -e "${RED}✗ LOCALIZATION 폴더 없음${NC}"
    exit 1
fi

# ========================================
# Step 3.5: StreamingAssets 배포 (폰트 번들)
# ========================================
echo -e "\n${YELLOW}[3.5/5] StreamingAssets 배포...${NC}"
if [ -d "$SOURCE_DIR/StreamingAssets" ]; then
    rsync -a --delete "$SOURCE_DIR/StreamingAssets/" "$GAME_MOD/StreamingAssets/"
    echo -e "${GREEN}✓ StreamingAssets (폰트 번들 포함)${NC}"
else
    echo -e "${YELLOW}⚠ StreamingAssets 폴더 없음 (건너뜀)${NC}"
fi

# ========================================
# Step 4: 메타 파일 복사
# ========================================
echo -e "\n${YELLOW}[4/5] 메타 파일 복사...${NC}"

meta_files=("manifest.json" "preview.png" "workshop.json")
for file in "${meta_files[@]}"; do
    if [ -f "$SOURCE_DIR/$file" ]; then
        cp -f "$SOURCE_DIR/$file" "$GAME_MOD/"
        echo -e "  ${BLUE}✓${NC} $file"
    fi
done

# 완료
echo -e "\n${BLUE}========================================"
echo -e "${GREEN}✓ 배포 완료!${NC}"
echo -e "${BLUE}========================================${NC}"

echo -e "\n${YELLOW}배포된 항목:${NC}"
echo -e "  📁 Scripts/ (코드)"
echo -e "  📁 LOCALIZATION/ (번역)"
echo -e "  📄 메타 파일"

# ========================================
# Step 5: Git 자동 커밋 (사용자 요청)
# ========================================
echo -e "\n${BLUE}========================================"
echo -e "  Git 자동 동기화"
echo -e "========================================${NC}"

# 현재 변경사항이 있는지 확인
if [[ -n $(git status --porcelain) ]]; then
    # 커밋 메시지가 인자로 전달되었는지 확인
    if [ ! -z "$1" ]; then
        COMMIT_MSG="$1"
    else
        # 인자 없으면 입력 받기
        echo -e "${YELLOW}변경사항이 감지되었습니다.${NC}"
        read -p "커밋 메시지를 입력하세요 (엔터 시 'Auto deploy update' 사용): " USER_MSG
        if [ -z "$USER_MSG" ]; then
            COMMIT_MSG="Auto deploy update"
        else
            COMMIT_MSG="$USER_MSG"
        fi
    fi
    
    echo -e "\n${YELLOW}[Git] 동기화 진행 중...${NC}"
    # sync.sh 실행 (같은 디렉토리에 있다고 가정)
    SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
    if [ -f "$SCRIPT_DIR/sync.sh" ]; then
        "$SCRIPT_DIR/sync.sh" "$COMMIT_MSG"
    else
        # sync.sh가 없으면 직접 git 명령 실행
        git add .
        git commit -m "$COMMIT_MSG"
        git push origin main 2>/dev/null || echo -e "${YELLOW}⚠ Push 실패 (로컬 커밋만 완료)${NC}"
    fi
    echo -e "${GREEN}✓ Git 동기화 완료${NC}"
else
    echo -e "${GREEN}✓ 변경사항 없음 (Git 동기화 건너뜀)${NC}"
fi

echo -e "\n${YELLOW}다음:${NC} 게임 재시작"

echo -e "\n${BLUE}💡 Desktop/qud_korean에서만 작업하세요!${NC}"
