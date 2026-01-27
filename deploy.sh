#!/bin/bash
# qud_korean 모드 빌드 & 배포 스크립트
# 사용법: ./deploy.sh

set -e  # 에러 시 중단

SRC="/Users/ben/Desktop/qud_korean"
DST="/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/qud_korean"

echo "=============================================="
echo "  qud_korean 모드 빌드 & 배포"
echo "=============================================="
echo ""

# 1. 빌드 실행
echo "[1/6] 빌드 중..."
if ! python3 "$SRC/tools/build_optimized.py"; then
    echo ""
    echo "빌드 실패!"
    exit 1
fi
echo ""

# 2. 대상 폴더 준비
echo "[2/6] 대상 폴더 준비..."
mkdir -p "$DST"
rm -rf "$DST/Scripts"
rm -rf "$DST/LOCALIZATION"
rm -rf "$DST/data"
rm -rf "$DST/StreamingAssets"
rm -f "$DST/sourcemap.json"

# 3. 빌드 결과물 복사
echo "[3/6] 빌드 결과물 복사..."
cp -R "$SRC/dist/data" "$DST/"
cp "$SRC/dist/sourcemap.json" "$DST/"

# 4. 필수 파일 복사
echo "[4/6] 필수 파일 복사..."
cp "$SRC/manifest.json" "$DST/"
cp "$SRC/d2coding.bundle" "$DST/"
cp -R "$SRC/Scripts" "$DST/"

# 5. LOCALIZATION 복사 (개발 모드 폴백용)
echo "[5/6] LOCALIZATION 복사 (폴백용)..."
cp -R "$SRC/LOCALIZATION" "$DST/"

# StreamingAssets 복사 (존재하는 경우)
[ -d "$SRC/StreamingAssets" ] && cp -R "$SRC/StreamingAssets" "$DST/"

# 6. 정리
echo "[6/6] 정리 중..."
find "$DST" -name ".DS_Store" -delete 2>/dev/null || true
find "$DST" -name "*.meta" -delete 2>/dev/null || true

echo ""
echo "=============================================="
echo "  빌드 & 배포 완료!"
echo "=============================================="
echo ""
echo "배포 위치: $DST"
echo ""
echo "배포된 파일:"
ls -lh "$DST/"
echo ""
echo "데이터 번들:"
ls -lh "$DST/data/" 2>/dev/null || echo "(없음)"
echo ""
echo "다음 단계:"
echo "  1. 게임 실행"
echo "  2. Ctrl+W -> Wish: kr:stats 로 확인"
echo ""
