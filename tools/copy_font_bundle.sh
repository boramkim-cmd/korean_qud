#!/bin/bash
# Unity AssetBundle 자동 복사 스크립트
# 사용법: ./copy_font_bundle.sh

# Unity 프로젝트 AssetBundle 경로
UNITY_BUNDLE_DIR="/Users/ben/My project/Assets/AssetBundles"
# Qud 모드 폴더 내 폰트 리소스 경로 (필요시 수정)
QUDFONT_DIR="/Users/ben/Desktop/qud_korean/StreamingAssets/Mods/YourMod/Fonts"

# 폴더가 없으면 생성
mkdir -p "$QUDFONT_DIR"

# AssetBundle 파일 복사 (모든 번들 파일)
cp -v "$UNITY_BUNDLE_DIR"/* "$QUDFONT_DIR"/

echo "AssetBundle 복사 완료: $UNITY_BUNDLE_DIR → $QUDFONT_DIR"
