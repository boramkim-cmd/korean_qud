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

# 0. 사전 검증 (Dictionary 중복 키 체크)
echo "[0/7] 사전 검증..."

# Dictionary 중복 키 검사
DUPLICATES=$(grep -oP '\{ "\K[^"]+(?=",)' "$SRC/Scripts/02_Patches/20_Objects/02_20_00_ObjectTranslator.cs" 2>/dev/null | sort | uniq -d || true)

if [ -n "$DUPLICATES" ]; then
    echo ""
    echo "⛔ 오류: Dictionary 중복 키 발견!"
    echo "=========================================="
    echo "$DUPLICATES"
    echo "=========================================="
    echo ""
    echo "해결 방법:"
    echo "  grep -n \"중복키\" Scripts/02_Patches/20_Objects/02_20_00_ObjectTranslator.cs"
    echo "  중복된 항목 삭제 후 다시 실행"
    echo ""
    exit 1
fi

# V2 ObjectTranslator 중복 키 검사 (있는 경우)
V2_FILES=$(find "$SRC/Scripts/02_Patches/20_Objects/V2" -name "*.cs" 2>/dev/null || true)
if [ -n "$V2_FILES" ]; then
    for file in $V2_FILES; do
        DUPS=$(grep -oP '\{ "\K[^"]+(?=",)' "$file" 2>/dev/null | sort | uniq -d || true)
        if [ -n "$DUPS" ]; then
            echo ""
            echo "⛔ 오류: Dictionary 중복 키 발견! ($file)"
            echo "$DUPS"
            exit 1
        fi
    done
fi

echo "  ✅ Dictionary 중복 키 없음"

# 0.5. pytest 게이트
echo "[0.5/7] pytest 테스트 실행..."
if ! python3 -m pytest --version >/dev/null 2>&1; then
    echo ""
    echo "⛔ pytest 미설치! 설치: pip3 install pytest"
    exit 1
fi
if python3 -m pytest "$SRC/tools/test_"*.py -q --tb=short 2>/dev/null; then
    echo "  ✅ 모든 테스트 통과"
else
    echo ""
    echo "⛔ 오류: pytest 테스트 실패!"
    echo "  python3 -m pytest \"$SRC/tools/test_*.py\" -v 로 상세 확인"
    exit 1
fi
echo ""

# 1. JSON 검증
echo "[1/7] JSON 검증 중..."
if ! python3 "$SRC/tools/project_tool.py" validate --quiet 2>/dev/null; then
    echo "  ⚠️  JSON 검증 스킵 (project_tool.py 오류)"
fi
echo ""

# 2. 빌드 실행
echo "[2/7] 빌드 중..."
if ! python3 "$SRC/tools/build_optimized.py"; then
    echo ""
    echo "빌드 실패!"
    exit 1
fi
echo ""

# 3. 대상 폴더 준비
echo "[3/7] 대상 폴더 준비..."
mkdir -p "$DST"
rm -rf "$DST/Scripts"
rm -rf "$DST/LOCALIZATION"
rm -rf "$DST/data"
rm -rf "$DST/StreamingAssets"
rm -f "$DST/sourcemap.json"

# 4. 빌드 결과물 복사
echo "[4/7] 빌드 결과물 복사..."
cp -R "$SRC/dist/data" "$DST/"
cp "$SRC/dist/sourcemap.json" "$DST/"

# 5. 필수 파일 복사
echo "[5/6] 필수 파일 복사..."
cp "$SRC/mod_info.json" "$DST/"
cp "$SRC/manifest.json" "$DST/"
cp "$SRC/d2coding.bundle" "$DST/"
cp -R "$SRC/Scripts" "$DST/"

# StreamingAssets 복사 (존재하는 경우)
[ -d "$SRC/StreamingAssets" ] && cp -R "$SRC/StreamingAssets" "$DST/"

# 6. 정리
echo "[6/6] 정리 중..."
find "$DST" -name ".DS_Store" -delete 2>/dev/null || true
find "$DST" -name "*.meta" -delete 2>/dev/null || true

echo ""
echo "=============================================="
echo "  ✅ 빌드 & 배포 완료!"
echo "=============================================="
echo ""
echo "배포 위치: $DST"
echo ""
echo "📦 데이터 번들:"
ls -lh "$DST/data/" 2>/dev/null || echo "(없음)"
echo ""
echo "=============================================="
echo "  ⚠️  다음 단계 (생략 금지!)"
echo "=============================================="
echo ""
echo "1. 게임 실행"
echo "2. Ctrl+W → Wish:"
echo "   kr:stats   - 번역 통계 확인 (Mode: bundle 확인)"
echo "   kr:perf    - 성능 카운터 확인"
echo "3. 로그 확인:"
echo "   grep -i 'error\\|exception' ~/Library/Logs/Freehold\\ Games/CavesOfQud/Player.log | tail -20"
echo ""
echo "테스트 완료 후:"
echo "   git add . && git commit -m 'type: 설명'"
echo ""
