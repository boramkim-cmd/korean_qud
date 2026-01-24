#!/bin/bash
# qud_korean 모드 배포 스크립트
# 사용법: ./deploy.sh

SRC="/Users/ben/Desktop/qud_korean"
DST="/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/qud_korean"

echo "=== qud_korean 모드 배포 ==="

# 대상 폴더 생성
mkdir -p "$DST"

# 기존 파일 정리
rm -rf "$DST/Scripts"
rm -rf "$DST/LOCALIZATION"
rm -rf "$DST/StreamingAssets"

# 필수 파일 복사
echo ">> manifest.json 복사"
cp "$SRC/manifest.json" "$DST/"

echo ">> d2coding.bundle 복사"
cp "$SRC/d2coding.bundle" "$DST/"

echo ">> Scripts 복사"
cp -R "$SRC/Scripts" "$DST/"

echo ">> LOCALIZATION 복사"
cp -R "$SRC/LOCALIZATION" "$DST/"

echo ">> StreamingAssets 복사"
[ -d "$SRC/StreamingAssets" ] && cp -R "$SRC/StreamingAssets" "$DST/"

# .DS_Store 제거
find "$DST" -name ".DS_Store" -delete 2>/dev/null

echo ""
echo "=== 배포 완료 ==="
echo "위치: $DST"
echo ""
ls -la "$DST/"
