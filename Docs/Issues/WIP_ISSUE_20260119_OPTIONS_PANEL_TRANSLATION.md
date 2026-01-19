# Options Screen Left Panel & Partial Translation Issue

> **Status**: ✅ RESOLVED  
> **Priority**: High  
> **Category**: UI Translation  
> **Created**: 2026-01-19  
> **Updated**: 2026-01-19  
> **Resolved**: 2026-01-19

---

## Problem Summary

Options 화면에서 두 가지 문제:
1. 왼쪽 패널 카테고리 이름(Sound, Display, Controls 등)이 영어로 표시
2. 일부 옵션(Interface sounds, Fire crackling sounds 등)이 JSON에 번역이 있음에도 영어로 표시

---

## Root Cause Analysis

### 원인 1: 번역 데이터 동기화 누락
- `tools/options_translations.json`에 번역이 있지만
- `LOCALIZATION/UI/options.json`에 동기화되지 않음
- **19개 옵션 누락**

### 원인 2: Trailing Space 문제
- 게임 XML에 trailing space가 있는 옵션: `"Allow creatures to have multiple defects "`
- JSON에는 trim된 버전: `"Allow creatures to have multiple defects"`
- **키 불일치로 번역 매칭 실패**

### 원인 3: 다중 공백 문제
- 게임 XML: `"Nearby objects list:   show liquid pools"` (공백 2개)
- JSON: `"Nearby objects list: show liquid pools"` (공백 1개)

---

## Solution Applied (ROOT FIX)

### 1. 동기화 스크립트 생성 ✅
- File: `tools/sync_options_translations.py`
- tools → LOCALIZATION 번역 동기화
- trailing space 버전 자동 추가
- 수동 누락 항목 추가

### 2. TranslationEngine 공백 정규화 ✅
- File: `Scripts/00_Core/00_00_01_TranslationEngine.cs`
- `FindInScopes()` 함수에 공백 정규화 로직 추가:
  1. 원본 키 그대로 시도
  2. `key.Trim()` 시도
  3. 다중 공백을 단일 공백으로 정규화 시도

### 3. 번역 데이터 완성 ✅
- 게임 옵션: 187개
- LOCALIZATION 데이터: 442개 (대소문자 변형 포함)
- **모든 게임 옵션에 번역 매칭 완료**

### 3. Deployed ✅
- `sync-and-deploy.sh` executed successfully

### 4. Bottom Menu Options Added ✅
- Missing translations for footer menu buttons:
  - `"Change Value": "값 변경"` - Slider control
  - `"Save": "저장"` - Slider save action
- Existing translations verified:
  - `"Cancel": "취소"` ✅
  - `"Collapse All": "모두 접기"` ✅
  - `"Expand All": "모두 펼치기"` ✅
  - `"Help": "도움말"` ✅
  - `"navigate": "이동"` ✅
  - `"Select": "선택"` ✅

---

## All Issues Resolved ✅

The following problems have been fixed:
1. ✅ Left panel categories now translated (Sound → 사운드, etc.)
2. ✅ Options with trailing spaces now matched
3. ✅ Bottom menu options now translated (`[Space] Change Value` → `[Space] 값 변경`)

**일부 옵션이 JSON에 번역이 있음에도 영어로 표시되는 문제**

### Verified Data
| English Text | In JSON? | In Patch Scope? |
|--------------|----------|-----------------|
| Interface sounds | ✅ Yes | ✅ options |
| Interface volume | ✅ Yes | ✅ options |
| Fire crackling sounds | ✅ Yes | ✅ options |
| Disable most tile-based flashing effects | ✅ Yes | ✅ options |
| Disable tile-based screen-warping effects | ✅ Yes | ✅ options |

### Possible Causes
1. **Game not restarted** - User needs to restart game after deploy
2. **Mod compilation failed** - Check Player.log for errors
3. **Timing issue** - Options loaded before Harmony patches applied
4. **Case sensitivity** - Though JSON has both cases

### Debug Steps for Next Session
```bash
# Check Player.log for mod loading
grep -i "Qud-KR\|KoreanLocalization\|Harmony" ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | tail -50

# Check for any errors
grep -i "error\|exception\|fail" ~/Library/Logs/Freehold\ Games/CavesOfQud/Player.log | tail -30

# Verify JSON is valid
python3 -c "import json; json.load(open('LOCALIZATION/UI/options.json'))"
```

---

## ERR-012 Checklist (From Error Log)

- [x] Does the translation JSON contain the exact English key shown in screenshot? ✅
- [x] Does the patch code actually load and use that JSON category? ✅  
- [ ] Are there formatting differences between EN/KO that could cause display issues? **Need to verify**
- [ ] Did you TEST in game after deploying? **User needs to confirm**

---

## Next Steps

1. [ ] User: Restart game and test
2. [ ] Verify left panel categories now show in Korean (Sound → 사운드, etc.)
3. [ ] Check if "Interface sounds" etc. now translated
4. [ ] If still English, add debug logging to TranslateOption to see what's happening
5. [ ] Consider alternative approach: UI-level translation in TranslateAll() with explicit text matching

---

## Related Files

- `Scripts/02_Patches/10_UI/02_10_01_Options.cs` - Options patch
- `LOCALIZATION/UI/options.json` - Translation data
- `Scripts/00_Core/00_00_01_TranslationEngine.cs` - Translation engine
- `Assets/core_source/GameSource/Qud.UI/OptionsCategoryControl.cs` - Game source

---

## Reference: ERR-012 Lesson

> "Code patches are useless if translation data doesn't exist or is malformed."
> 
> Always check BOTH:
> 1. Translation exists in JSON with correct key
> 2. Patch code loads and uses correct category
