#!/bin/bash

# ================================================
# Caves of Qud 한글화 모드 설치 스크립트 (Mac)
# ================================================

echo "🎮 Caves of Qud 한글화 모드 설치 시작..."

# 변수 설정
WORK_DIR="/Users/ben/Desktop/무제 폴더/StreamingAssets/Base-Work"
MOD_NAME="KoreanLocalization"
GAME_MODS_DIR="$HOME/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods"

# 1. Mods 폴더 확인/생성
echo ""
echo "📁 게임 Mods 폴더 확인 중..."
if [ ! -d "$GAME_MODS_DIR" ]; then
    echo "   Mods 폴더가 없습니다. 생성 중..."
    mkdir -p "$GAME_MODS_DIR"
    echo "   ✅ Mods 폴더 생성 완료"
else
    echo "   ✅ Mods 폴더 존재"
fi

# 2. 기존 모드 백업
if [ -d "$GAME_MODS_DIR/$MOD_NAME" ]; then
    echo ""
    echo "📦 기존 모드 백업 중..."
    BACKUP_NAME="${MOD_NAME}_backup_$(date +%Y%m%d_%H%M%S)"
    mv "$GAME_MODS_DIR/$MOD_NAME" "$GAME_MODS_DIR/$BACKUP_NAME"
    echo "   ✅ 백업 완료: $BACKUP_NAME"
fi

# 3. 모드 복사
echo ""
echo "📋 모드 복사 중..."
cp -r "$WORK_DIR/Mod/$MOD_NAME" "$GAME_MODS_DIR/"

if [ $? -eq 0 ]; then
    echo "   ✅ 모드 복사 완료"
else
    echo "   ❌ 모드 복사 실패"
    exit 1
fi

# 4. 파일 확인
echo ""
echo "🔍 설치된 파일 확인..."
echo ""
ls -lh "$GAME_MODS_DIR/$MOD_NAME"

# 5. 완료 메시지
echo ""
echo "✅ 설치 완료!"
echo ""
echo "다음 단계:"
echo "1. Caves of Qud 실행"
echo "2. Main Menu → Mods"
echo "3. 'Korean Localization' 활성화"
echo "4. 게임 재시작"
echo ""
echo "로그 확인:"
echo "tail -f ~/Library/Application\ Support/com.FreeholdGames.CavesOfQud/Player.log"
echo ""
