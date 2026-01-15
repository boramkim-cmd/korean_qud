#!/bin/bash
# Caves of Qud 한글화 모드 검증 스크립트
# 작성일: 2026-01-13

echo "=========================================="
echo "Caves of Qud 한글화 모드 검증"
echo "=========================================="
echo ""

# 색상 정의
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 1. 모드 설치 확인
echo "1. 모드 설치 확인..."
MOD_PATH=~/Library/Application\ Support/com.FreeholdGames.CavesOfQud/Mods/KoreanLocalization

if [ -d "$MOD_PATH" ]; then
    echo -e "${GREEN}✓${NC} 모드 폴더 존재: $MOD_PATH"
else
    echo -e "${RED}✗${NC} 모드 폴더 없음: $MOD_PATH"
    exit 1
fi

# 2. 필수 파일 확인
echo ""
echo "2. 필수 파일 확인..."

FILES=(
    "manifest.json"
    "Scripts/JosaHandler.cs"
    "Quests.xml"
    "Conversations.xml"
    "HistorySpice.json"
)

for file in "${FILES[@]}"; do
    if [ -f "$MOD_PATH/$file" ]; then
        echo -e "${GREEN}✓${NC} $file"
    else
        echo -e "${RED}✗${NC} $file (없음)"
    fi
done

# 3. 파일 크기 확인
echo ""
echo "3. 파일 크기 확인..."
echo "Quests.xml: $(wc -c < "$MOD_PATH/Quests.xml" 2>/dev/null || echo "0") bytes"
echo "Conversations.xml: $(wc -c < "$MOD_PATH/Conversations.xml" 2>/dev/null || echo "0") bytes"
echo "HistorySpice.json: $(wc -c < "$MOD_PATH/HistorySpice.json" 2>/dev/null || echo "0") bytes"

# 4. 게임 로그 확인
echo ""
echo "4. 게임 로그 확인..."
LOG_PATH=~/Library/Application\ Support/com.FreeholdGames.CavesOfQud/Player.log

if [ -f "$LOG_PATH" ]; then
    echo -e "${GREEN}✓${NC} 로그 파일 존재"
    
    # 모드 로드 메시지 확인
    if grep -q "Korean Josa" "$LOG_PATH" 2>/dev/null; then
        echo -e "${GREEN}✓${NC} 모드 로드 확인됨:"
        grep "Korean Josa" "$LOG_PATH" | tail -3
    else
        echo -e "${YELLOW}⚠${NC} 모드 로드 메시지 없음 (게임을 실행하지 않았거나 모드가 비활성화됨)"
    fi
else
    echo -e "${YELLOW}⚠${NC} 로그 파일 없음 (게임을 한 번도 실행하지 않음)"
fi

# 5. 한글 텍스트 확인
echo ""
echo "5. 한글 텍스트 확인..."

# Quests.xml에서 한글 확인
if grep -q "워터바인" "$MOD_PATH/Quests.xml" 2>/dev/null; then
    echo -e "${GREEN}✓${NC} Quests.xml에 한글 존재"
else
    echo -e "${RED}✗${NC} Quests.xml에 한글 없음"
fi

# Conversations.xml에서 한글 확인
if grep -q "조파" "$MOD_PATH/Conversations.xml" 2>/dev/null; then
    echo -e "${GREEN}✓${NC} Conversations.xml에 한글 존재"
else
    echo -e "${RED}✗${NC} Conversations.xml에 한글 없음"
fi

# HistorySpice.json에서 한글 확인
if grep -q "년" "$MOD_PATH/HistorySpice.json" 2>/dev/null; then
    echo -e "${GREEN}✓${NC} HistorySpice.json에 한글 존재"
else
    echo -e "${YELLOW}⚠${NC} HistorySpice.json에 한글 없음 (아직 번역 안 됨)"
fi

# 6. 조사 처리 패턴 확인
echo ""
echo "6. 조사 처리 패턴 확인..."

# (조사) 패턴 확인
JOSA_COUNT=$(grep -o "(이)\|(을)\|(으)로\|(와)과" "$MOD_PATH/Conversations.xml" 2>/dev/null | wc -l)
echo "Conversations.xml에서 조사 패턴 발견: $JOSA_COUNT 개"

if [ "$JOSA_COUNT" -gt 0 ]; then
    echo -e "${GREEN}✓${NC} 조사 처리 시스템 사용 중"
else
    echo -e "${YELLOW}⚠${NC} 조사 패턴 없음"
fi

# 7. 요약
echo ""
echo "=========================================="
echo "검증 완료"
echo "=========================================="
echo ""
echo "다음 단계:"
echo "1. 게임 실행"
echo "2. Main Menu → Mods → Korean Localization 체크"
echo "3. 게임 재시작"
echo "4. 새 게임 시작 → 퀘스트 로그 (J키) 확인"
echo ""
