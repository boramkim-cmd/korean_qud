#!/bin/bash
# 빠른 테스트 가이드
# 게임을 실행한 후 이 스크립트로 로그를 실시간 모니터링

echo "=========================================="
echo "Caves of Qud 로그 모니터링 시작"
echo "=========================================="
echo ""
echo "게임을 실행하세요..."
echo "Ctrl+C로 종료"
echo ""

LOG_PATH=~/Library/Application\ Support/com.FreeholdGames.CavesOfQud/Player.log

if [ ! -f "$LOG_PATH" ]; then
    echo "⚠️  로그 파일이 아직 생성되지 않았습니다."
    echo "게임을 한 번 실행하면 생성됩니다."
    exit 1
fi

# 로그 실시간 모니터링
tail -f "$LOG_PATH" | grep --line-buffered -E "Korean|Josa|Error|Exception|Mod"
