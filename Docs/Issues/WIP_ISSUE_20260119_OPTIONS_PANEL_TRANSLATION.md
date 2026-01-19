# Options Screen Left Panel & Partial Translation Issue

> **Status**: 🟡 WIP  
> **Priority**: High  
> **Category**: UI Translation  
> **Created**: 2026-01-19  
> **Updated**: 2026-01-19

---

## Problem Summary

Options 화면에서 두 가지 문제:
1. 왼쪽 패널 카테고리 이름(Sound, Display, Controls 등)이 영어로 표시
2. 일부 옵션(Interface sounds, Fire crackling sounds 등)이 JSON에 번역이 있음에도 영어로 표시

---

## Screenshots

스크린샷 참조:
- Options 화면에서 왼쪽 패널 영어
- SOUND, DISPLAY, CONTROLS 등 카테고리 헤더 영어
- 일부 옵션: Interface sounds, Fire crackling sounds, Disable most tile-based... 영어

---

## Work Done (This Session)

### 1. Added category translations to options.json ✅
```json
"Sound": "사운드",
"Display": "디스플레이", 
"Controls": "조작",
"Accessibility": "접근성",
"UI": "UI",
"Legacy UI": "레거시 UI",
"Automation": "자동화",
"Autoget": "자동 획득",
"Prompts": "프롬프트",
"Mods": "모드",
"Performance": "성능",
"App Settings": "앱 설정",
"Debug": "디버그"
```

### 2. Added OptionsCategoryControl patch ✅
- File: `Scripts/02_Patches/10_UI/02_10_01_Options.cs`
- Added `Patch_OptionsCategoryControl` class with `Render_Postfix`
- Translates category title after original Render()

### 3. Deployed ✅
- `sync-and-deploy.sh` executed successfully

---

## Remaining Issue

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
