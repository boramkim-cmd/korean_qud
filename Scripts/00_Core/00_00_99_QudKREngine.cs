/*
 * 파일명: 00_99_QudKREngine.cs
 * 분류: [Core] 엔진 확장 유틸리티
 * 역할: 한국어 폰트 강제 적용, 조사(Josa) 처리 로직 등 엔진 레벨의 기능을 제공합니다.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using XRL.Messages;
using XRL.Language;
using XRL.World.Parts;
using Qud.UI;

// GameObject 명시적 지정
using GameObject = XRL.World.GameObject;

namespace QudKRTranslation.Core
{
    // =================================================================
    // 1. 폰트 매니저
    // =================================================================
    public static class FontManager
    {
        public static bool IsFontLoaded { get; private set; } = false;
        
        private static bool _patched = false;
        private static TMP_FontAsset _koreanTMPFont = null;

        /// <summary>
        /// 한글 TMP 폰트 에셋 반환 (외부에서 접근용)
        /// </summary>
        public static TMP_FontAsset GetKoreanFont() => _koreanTMPFont;

        // 시스템 폰트 검색 우선순위 (TMP fallback용)
        public static string[] TargetFontNames = { 
            "Cafe24PROSlimMax SDF",  // TMP 번들 폰트
            "Cafe24PROSlimMax",      // 시스템 설치 폰트
            "Cafe24 PRO SlimMax",    // 대체 이름
            "AppleGothic",
            "NeoDunggeunmo-Regular", 
            "NeoDunggeunmo",         
            "neodgm",                
            "Apple SD Gothic Neo",
            "Noto Sans CJK KR",
            "Arial" 
        };

        public static void ApplyKoreanFont()
        {
            // If we've already successfully loaded the font, nothing to do
            if (IsFontLoaded) return;

            // prevent re-entrancy during a load attempt
            if (_patched) return;
            _patched = true;

            // 1. 모든 TMP 폰트 이름, 한글 지원 여부, fallback 목록을 로그로 출력
            var allTMPFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            int fontIdx = 0;
            foreach (var fontAsset in allTMPFonts)
            {
                if (fontAsset == null) continue;
                bool hasKorean = false;
                try { hasKorean = fontAsset.HasCharacter('가'); } catch { }
                Debug.Log($"[Qud-KR][TMPFont] #{fontIdx}: '{fontAsset.name}' (Korean: {hasKorean})" );
                fontIdx++;
            }

            // Try to find and load our AssetBundle (qudkoreanfont) 
            // First try mod folder's StreamingAssets, then game's StreamingAssets/Mods
            try
            {
                TMP_FontAsset loadedFont = null;
                var searchPaths = new System.Collections.Generic.List<string>();

                // 1. Try mod folder's StreamingAssets first
                try
                {
                    var mod = XRL.ModManager.GetMod("KoreanLocalization");
                    if (mod != null && !string.IsNullOrEmpty(mod.Path))
                    {
                        string modStreamingAssets = System.IO.Path.Combine(mod.Path, "StreamingAssets", "Mods");
                        if (System.IO.Directory.Exists(modStreamingAssets))
                        {
                            searchPaths.Add(modStreamingAssets);
                            Debug.Log($"[Qud-KR] Added mod StreamingAssets to search: {modStreamingAssets}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Qud-KR] Error getting mod path: {ex.Message}");
                }

                // 2. Also try game's StreamingAssets/Mods as fallback
                string gameStreamingMods = System.IO.Path.Combine(Application.streamingAssetsPath, "Mods");
                if (System.IO.Directory.Exists(gameStreamingMods))
                {
                    searchPaths.Add(gameStreamingMods);
                }

                foreach (var modsPath in searchPaths)
                {
                    if (loadedFont != null) break;
                    
                    // Look for qudkoreanfont in any subfolder
                    var candidates = System.IO.Directory.GetFiles(modsPath, "qudkoreanfont*", System.IO.SearchOption.AllDirectories);
                    foreach (var candidate in candidates)
                    {
                        // Skip manifest files
                        if (candidate.EndsWith(".manifest") || candidate.EndsWith(".meta")) continue;
                        
                        Debug.Log($"[Qud-KR] Attempting to load font bundle: {candidate}");
                        var bundle = AssetBundle.LoadFromFile(candidate);
                        if (bundle == null) 
                        {
                            Debug.LogWarning($"[Qud-KR] Failed to load bundle from: {candidate}");
                            continue;
                        }

                        try
                        {
                            var fonts = bundle.LoadAllAssets<TMP_FontAsset>();
                            if (fonts != null && fonts.Length > 0)
                            {
                                loadedFont = fonts[0];
                                Debug.Log($"[Qud-KR] Loaded TMP_FontAsset '{loadedFont.name}' from bundle.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[Qud-KR] Error reading assets from bundle: {ex.Message}");
                        }
                        // Keep assets in memory but unload bundle container
                        bundle.Unload(false);

                        if (loadedFont != null) break;
                    }
                }

                if (loadedFont != null)
                {
                    _koreanTMPFont = loadedFont;
                }
                else
                {
                    // No bundle; try to find an existing TMP font asset that contains Korean glyphs
                    foreach (var targetName in TargetFontNames)
                    {
                        foreach (var fontAsset in allTMPFonts)
                        {
                            if (fontAsset == null) continue;
                            if (!fontAsset.name.Equals(targetName, StringComparison.OrdinalIgnoreCase)) continue;
                            bool hasKorean = false;
                            try { hasKorean = fontAsset.HasCharacter('가'); } catch { }
                            if (hasKorean)
                            {
                                _koreanTMPFont = fontAsset;
                                Debug.Log($"[Qud-KR] Selected existing TMP_FontAsset '{fontAsset.name}' as fallback (matched preferred name '{targetName}').");
                                break;
                            }
                        }
                        if (_koreanTMPFont != null) break;
                    }

                    // If still nothing, pick any font that has Korean
                    if (_koreanTMPFont == null)
                    {
                        foreach (var fontAsset in allTMPFonts)
                        {
                            if (fontAsset == null) continue;
                            bool hasKorean = false;
                            try { hasKorean = fontAsset.HasCharacter('가'); } catch { }
                            if (hasKorean)
                            {
                                _koreanTMPFont = fontAsset;
                                Debug.Log($"[Qud-KR] Selected existing TMP_FontAsset '{fontAsset.name}' as fallback (first available Korean-supporting font).");
                                break;
                            }
                        }
                    }

                    if (_koreanTMPFont == null)
                    {
                        // Reset _patched so future attempts (e.g., after bundle copy) can retry
                        _patched = false;
                        Debug.LogWarning("[Qud-KR] Korean font bundle not found and no existing TMP_FontAsset with Korean glyphs was detected. Will retry on next ApplyKoreanFont() call.");
                    }
                }

                if (_koreanTMPFont != null)
                {
                    // Ensure TMP_Settings fallback list exists and contains our font (insert at front)
                    if (TMP_Settings.fallbackFontAssets == null)
                        TMP_Settings.fallbackFontAssets = new System.Collections.Generic.List<TMP_FontAsset>();

                    if (!TMP_Settings.fallbackFontAssets.Contains(_koreanTMPFont))
                    {
                        TMP_Settings.fallbackFontAssets.Insert(0, _koreanTMPFont);
                        Debug.Log($"[Qud-KR] Inserted '{_koreanTMPFont.name}' into TMP_Settings.fallbackFontAssets.");
                    }

                    // Set Korean font as default for consistent rendering
                    if (TMP_Settings.defaultFontAsset != _koreanTMPFont)
                    {
                        Debug.Log($"[Qud-KR] Changing default font from '{TMP_Settings.defaultFontAsset?.name}' to '{_koreanTMPFont.name}'");
                        TMP_Settings.defaultFontAsset = _koreanTMPFont;
                    }

                    // Add as fallback to all existing TMP font assets
                    // AND add existing fonts as fallback to Korean font (Bi-directional)
                    if (_koreanTMPFont.fallbackFontAssetTable == null)
                        _koreanTMPFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();

                    foreach (var fontAsset in allTMPFonts)
                    {
                        if (fontAsset == null || fontAsset == _koreanTMPFont) continue;
                        
                        // 1. Existing -> Korean
                        if (fontAsset.fallbackFontAssetTable == null)
                            fontAsset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                        if (!fontAsset.fallbackFontAssetTable.Contains(_koreanTMPFont))
                            fontAsset.fallbackFontAssetTable.Add(_koreanTMPFont);

                        // 2. Korean -> Existing (for English chars in Korean font context)
                        if (!_koreanTMPFont.fallbackFontAssetTable.Contains(fontAsset))
                            _koreanTMPFont.fallbackFontAssetTable.Add(fontAsset);
                    }

                    // Force refresh of existing TMP components so fallback takes effect immediately
                    ApplyFallbackToAllTMPComponents();

                    IsFontLoaded = true;
                    Debug.Log("[Qud-KR] Korean font successfully loaded and applied (Bi-directional fallback).");

                    // --- 진단 로그: 로드된 폰트 글리프 커버리지 확인 ---
                    try
                    {
                        var kf = _koreanTMPFont;
                        if (kf != null)
                        {
                            bool hasKorean = false, hasA = false, has0 = false, hasBang = false;
                            try { hasKorean = kf.HasCharacter('가'); } catch { }
                            try { hasA = kf.HasCharacter('A'); } catch { }
                            try { has0 = kf.HasCharacter('0'); } catch { }
                            try { hasBang = kf.HasCharacter('!'); } catch { }

                            Debug.Log($"[Qud-KR][Diag] Loaded TMP_FontAsset: '{kf.name}' (가: {hasKorean}, A: {hasA}, 0: {has0}, !: {hasBang})");
                            
                            // Extended lowercase test for "character creation" issue
                            string testChars = "characterion";
                            var missing = new System.Collections.Generic.List<char>();
                            foreach (char c in testChars)
                            {
                                bool has = false;
                                try { has = kf.HasCharacter(c); } catch { }
                                if (!has) missing.Add(c);
                            }
                            if (missing.Count > 0)
                                Debug.LogWarning($"[Qud-KR][Diag] MISSING lowercase glyphs in font: [{string.Join(", ", missing)}]");
                            else
                                Debug.Log($"[Qud-KR][Diag] All lowercase glyphs for 'character creation' present in font.");
                        }

                        // TMP_Settings.fallbackFontAssets 상태 출력
                        if (TMP_Settings.fallbackFontAssets != null)
                        {
                            int idx = 0;
                            foreach (var fb in TMP_Settings.fallbackFontAssets)
                            {
                                if (fb == null) continue;
                                bool hasK = false;
                                try { hasK = fb.HasCharacter('가'); } catch { }
                                Debug.Log($"[Qud-KR][Diag][Fallback] #{idx}: '{fb.name}' (가: {hasK})");
                                idx++;
                            }
                        }

                        // Legacy UnityEngine.UI.Text 검사
                        try
                        {
                            var legacyTexts = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Text>();
                            int totalLegacy = legacyTexts?.Length ?? 0;
                            int missingKorean = 0;
                            foreach (var lt in legacyTexts)
                            {
                                try
                                {
                                    if (lt?.font == null || !lt.font.HasCharacter('가')) missingKorean++;
                                }
                                catch { missingKorean++; }
                            }
                            Debug.Log($"[Qud-KR][Diag] Legacy UnityEngine.UI.Text total: {totalLegacy}, missing Korean glyphs: {missingKorean}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[Qud-KR][Diag] Failed to scan legacy Text components: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[Qud-KR][Diag] Font diagnostics exception: {ex.Message}");
                    }

                    // 메인 메뉴 텍스트 번역 시도
                    MainMenu_Show_Patch.TranslateMainMenuOptions();

                    // --- Legacy UnityEngine.UI.Text 자동 한글 폰트 적용 ---
                    try
                    {
                        var legacyTexts = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Text>();
                        int patched = 0;
                        foreach (var lt in legacyTexts)
                        {
                            if (lt == null) continue;
                            var f = lt.font;
                            bool needPatch = false;
                            try { if (f == null || !f.HasCharacter('가')) needPatch = true; } catch { needPatch = true; }
                            if (needPatch)
                            {
                                // 시스템 설치된 Cafe24PROSlimMax 우선 사용 (TMP 번들과 동일한 폰트)
                                Font fallback = null;
                                string[] candidates = { 
                                    "Cafe24PROSlimMax",      // 설치된 한글 폰트 (TMP와 동일)
                                    "Cafe24 PRO SlimMax",    // 대체 이름
                                    "Apple SD Gothic Neo", 
                                    "AppleGothic", 
                                    "Arial" 
                                };
                                foreach (var cname in candidates)
                                {
                                    try { fallback = Font.CreateDynamicFontFromOSFont(cname, lt.fontSize > 0 ? lt.fontSize : 14); } catch { }
                                    if (fallback != null && fallback.HasCharacter('가')) break;
                                }
                                if (fallback != null)
                                {
                                    lt.font = fallback;
                                    patched++;
                                }
                            }
                        }
                        Debug.Log($"[Qud-KR][LegacyPatch] Patched {patched} legacy UI.Text components to Korean system font.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[Qud-KR][LegacyPatch] Exception: {ex.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                _patched = false;
                Debug.LogWarning($"[Qud-KR] Exception while loading font bundle: {e.Message}");
            }

            // 2. TMP_Settings.fallbackFontAssets 목록 출력
            if (TMP_Settings.fallbackFontAssets != null)
            {
                int fbIdx = 0;
                foreach (var fb in TMP_Settings.fallbackFontAssets)
                {
                    if (fb == null) continue;
                    bool hasKorean = false;
                    try { hasKorean = fb.HasCharacter('가'); } catch { }
                    Debug.Log($"[Qud-KR][Fallback] #{fbIdx}: '{fb.name}' (Korean: {hasKorean})");
                    fbIdx++;
                }
            }

            Debug.Log($"[Qud-KR] Font diagnostic complete. (총 TMP 폰트: {fontIdx})");
        }

        // Apply Korean font to a single TMP text component
        // FORCE REPLACE: 모든 TMP 컴포넌트의 폰트를 한국어 폰트로 강제 교체
        // 이렇게 해야 영어 텍스트도 Cafe24 폰트로 표시됨
        public static void ApplyFallbackToTMPComponent(TMPro.TMP_Text txt, bool forceLog = false)
        {
            if (txt == null) return;
            var k = _koreanTMPFont;
            if (k == null) return;

            try
            {
                var currentFont = txt.font;
                
                // 아이콘/심볼 폰트는 교체하지 않음
                if (currentFont != null)
                {
                    string fname = currentFont.name;
                    if (fname.Contains("Filled") || 
                        fname.Contains("Outlined") || 
                        fname.Contains("Icons") ||
                        fname.Contains("Cursor") ||
                        fname.Contains("Dingbats") ||
                        fname.Contains("PC-"))  // 컨트롤러 아이콘 폰트
                    {
                        // 아이콘 폰트는 fallback만 추가
                        if (currentFont.fallbackFontAssetTable == null)
                            currentFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                        if (!currentFont.fallbackFontAssetTable.Contains(k))
                            currentFont.fallbackFontAssetTable.Insert(0, k);
                        return;
                    }
                }
                
                // 폰트 강제 교체 (영어 + 한글 모두 Cafe24로 통일)
                if (txt.font != k)
                {
                    txt.font = k;
                }
            }
            catch
            {
                // 크래시 방지: 모든 예외 무시
            }
        }

        // Apply fallback to all TMP components currently loaded
        public static void ApplyFallbackToAllTMPComponents()
        {
            var uguis = Resources.FindObjectsOfTypeAll<TMPro.TextMeshProUGUI>();
            int uiCount = 0;
            foreach (var t in uguis)
            {
                ApplyFallbackToTMPComponent(t);
                uiCount++;
            }

            var texts = Resources.FindObjectsOfTypeAll<TMPro.TextMeshPro>();
            int textCount = 0;
            foreach (var t in texts)
            {
                ApplyFallbackToTMPComponent(t);
                textCount++;
            }
            
            Debug.Log($"[Qud-KR] Applied fallback to {uiCount} UGUIs and {textCount} 3D TMPs.");
        }
        
        public static TMP_FontAsset GetKoreanTMPFont()
        {
            return _koreanTMPFont;
        }
    }

    // =================================================================
    // 2. Harmony Patches
    // =================================================================

    [HarmonyPatch(typeof(Qud.UI.UIManager), "Init")]
    public static class UILoadPatch
    {
        static void Postfix()
        {
            FontManager.ApplyKoreanFont();
        }
    }

    // 메인 메뉴 표시 시 강제 적용
    [HarmonyPatch(typeof(Qud.UI.MainMenu), "Show")]
    public static class MainMenu_Show_Patch
    {
        // 1. 메뉴 표시 전: 텍스트 번역 (Show 메서드가 이 리스트를 사용함)
        static void Prefix()
        {
            TranslateMainMenuOptions();
        }

        // 2. 메뉴 표시 후: 폰트 적용 및 이벤트 등록
        static void Postfix()
        {
            // 폰트가 로드되지 않았으면 로드 시도
            if (!FontManager.IsFontLoaded)
                FontManager.ApplyKoreanFont();
            
            // 전역 Fallback 적용
            FontManager.ApplyFallbackToAllTMPComponents();

            // 메인 메뉴 Scroller 아이템 폰트 강제 적용 (이벤트 훅)
            if (FontManager.IsFontLoaded && Qud.UI.MainMenu.instance != null)
            {
                var menu = Qud.UI.MainMenu.instance;
                RegisterFontHook(menu.leftScroller);
                RegisterFontHook(menu.rightScroller);
                
                // 이미 생성된 아이템들에 대해서도 즉시 적용
                ApplyFontToScroller(menu.leftScroller);
                ApplyFontToScroller(menu.rightScroller);
            }
        }

        static void RegisterFontHook(XRL.UI.Framework.FrameworkScroller scroller)
        {
            if (scroller == null) return;
            
            // 기존 리스너 제거 (중복 방지)
            scroller.PostSetup.RemoveListener(OnPostSetup);
            // 새 리스너 등록
            scroller.PostSetup.AddListener(OnPostSetup);
            Debug.Log("[Qud-KR] Registered PostSetup hook for scroller.");
        }

        static void OnPostSetup(XRL.UI.Framework.FrameworkUnityScrollChild child, XRL.UI.Framework.ScrollChildContext context, XRL.UI.Framework.FrameworkDataElement data, int index)
        {
            if (child == null) return;
            var tmps = child.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            var krFont = FontManager.GetKoreanTMPFont();
            if (krFont == null) return;

            foreach (var tmp in tmps)
            {
                // fallback 추가 방식 (폰트 강제 교체 안 함)
                FontManager.ApplyFallbackToTMPComponent(tmp);
            }
        }

        static void ApplyFontToScroller(XRL.UI.Framework.FrameworkScroller scroller)
        {
            if (scroller == null || scroller.childRoot == null) return;
            
            var tmps = scroller.childRoot.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            var krFont = FontManager.GetKoreanTMPFont();
            if (krFont == null) return;

            foreach (var tmp in tmps)
            {
                // fallback 추가 방식 (폰트 강제 교체 안 함)
                FontManager.ApplyFallbackToTMPComponent(tmp);
            }
        }

        public static void TranslateMainMenuOptions()
        {
            Debug.Log("[Qud-KR] TranslateMainMenuOptions called.");
            var krMap = new Dictionary<string, string>
            {
                { "New Game", "새로 시작" },
                { "Continue", "이어하기" },
                { "Records", "기록" },
                { "Options", "설정" },
                { "Mods", "모드" },
                { "Redeem Code", "코드 입력" },
                { "Modding Toolkit", "모딩 툴킷" },
                { "Credits", "크레딧" },
                { "Help", "도움말" },
                { "Quit", "종료" }
            };

            bool changed = false;

            if (Qud.UI.MainMenu.LeftOptions != null)
            {
                foreach (var opt in Qud.UI.MainMenu.LeftOptions)
                {
                    if (krMap.ContainsKey(opt.Text)) 
                    {
                        opt.Text = krMap[opt.Text];
                        changed = true;
                    }
                }
            }

            if (Qud.UI.MainMenu.RightOptions != null)
            {
                foreach (var opt in Qud.UI.MainMenu.RightOptions)
                {
                    if (krMap.ContainsKey(opt.Text)) 
                    {
                        opt.Text = krMap[opt.Text];
                        changed = true;
                    }
                }
            }
            
            if (changed) Debug.Log("[Qud-KR] MainMenu options translated.");

            // 이미 메인 메뉴가 떠 있다면 강제 갱신
            try
            {
                if (Qud.UI.MainMenu.instance != null && Qud.UI.MainMenu.instance.gameObject.activeInHierarchy)
                {
                    Debug.Log("[Qud-KR] Refeshing active MainMenu instance...");
                    var menu = Qud.UI.MainMenu.instance;
                    if (menu.leftScroller != null)
                        menu.leftScroller.BeforeShow(null, Qud.UI.MainMenu.LeftOptions);
                    if (menu.rightScroller != null)
                        menu.rightScroller.BeforeShow(null, Qud.UI.MainMenu.RightOptions);
                    
                    // Force refresh layout?
                    menu.UpdateMenuBars();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Qud-KR] Failed to refresh MainMenu: {ex.Message}");
            }
        }
    }

    // 게임 코어 시작 시 폰트 로드 시도 (가장 빠른 시점)
    [HarmonyPatch(typeof(XRL.Core.XRLCore), "Start")]
    public static class XRLCore_Start_Patch
    {
        static void Postfix()
        {
            if (!FontManager.IsFontLoaded)
            {
                FontManager.ApplyKoreanFont();
            }
        }
    }

    // UITextSkin.Apply() 패치 - 모든 UI 텍스트에 한국어 폰트 강제 적용
    // UITextSkin은 TextMeshProUGUI를 감싸는 래퍼로, 대부분의 게임 UI 텍스트에 사용됨
    [HarmonyPatch(typeof(XRL.UI.UITextSkin), "Apply")]
    public static class UITextSkin_Apply_Patch
    {
        static void Postfix(XRL.UI.UITextSkin __instance)
        {
            if (!FontManager.IsFontLoaded) return;
            
            var krFont = FontManager.GetKoreanTMPFont();
            if (krFont == null) return;
            
            try
            {
                // UITextSkin 내부의 TMP 컴포넌트에 접근
                var tmp = __instance.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmp != null && tmp.font != krFont)
                {
                    tmp.font = krFont;
                    tmp.SetAllDirty();
                }
            }
            catch { }
        }
    }

    // 모든 TMP 컴포넌트가 활성화될 때 한글 폰트 fallback 적용
    // 메인 메뉴, 동적 생성 UI 등 모든 영역에 적용됨
    [HarmonyPatch(typeof(TMPro.TextMeshProUGUI), "OnEnable")]
    public static class TextMeshProUGUI_OnEnable_Patch
    {
        static void Postfix(TMPro.TextMeshProUGUI __instance)
        {
            if (!FontManager.IsFontLoaded) return;
            FontManager.ApplyFallbackToTMPComponent(__instance);
        }
    }

    // TextMeshPro (3D) 패치도 추가
    [HarmonyPatch(typeof(TMPro.TextMeshPro), "OnEnable")]
    public static class TextMeshPro_OnEnable_Patch
    {
        static void Postfix(TMPro.TextMeshPro __instance)
        {
            if (!FontManager.IsFontLoaded) return;
            FontManager.ApplyFallbackToTMPComponent(__instance);
        }
    }

    // TextMeshProUGUI.Awake 패치 - 최초 생성 시점에도 적용
    [HarmonyPatch(typeof(TMPro.TextMeshProUGUI), "Awake")]
    public static class TextMeshProUGUI_Awake_Patch
    {
        static void Postfix(TMPro.TextMeshProUGUI __instance)
        {
            if (!FontManager.IsFontLoaded) return;
            FontManager.ApplyFallbackToTMPComponent(__instance);
        }
    }

    [HarmonyPatch(typeof(MessageQueue), "AddPlayerMessage", new Type[] { typeof(string), typeof(string), typeof(bool) })]
    public static class MessageLogPatch
    {
        static void Prefix(ref string Message)
        {
            if (!string.IsNullOrEmpty(Message))
                Message = KoreanTextHelper.ResolveJosa(Message);
        }
    }

    [HarmonyPatch(typeof(Grammar), "IndefiniteArticle", new Type[] { typeof(string), typeof(bool) })]
    public static class ArticleKillerPatch
    {
        static bool Prefix(ref string __result)
        {
            __result = "";
            return false;
        }
    }

    [HarmonyPatch(typeof(Grammar), "Pluralize")]
    public static class PluralizeKillerPatch
    {
        static bool Prefix(string word, ref string __result)
        {
            __result = word;
            return false;
        }
    }

    [HarmonyPatch]
    public static class NameOrderPatch
    {
        static MethodBase TargetMethod()
        {
            MethodInfo bestMatch = null;
            int bestParamCount = -1;
            foreach (MethodInfo method in typeof(GameObject).GetMethods())
            {
                if (method.Name != "GetDisplayName") continue;
                ParameterInfo[] pars = method.GetParameters();
                if (pars.Length > 0 && pars[0].ParameterType == typeof(int))
                {
                    if (pars.Length > bestParamCount)
                    {
                        bestParamCount = pars.Length;
                        bestMatch = method;
                    }
                }
            }
            return bestMatch;
        }

        static void Postfix(ref string __result)
        {
            if (__result != null && __result.Contains("에 착용") && !__result.EndsWith("에 착용"))
            {
                string[] parts = __result.Split(new string[] { "에 착용" }, StringSplitOptions.None);
                if (parts.Length > 1) __result = parts[1].Trim() + "에 착용";
            }
        }
    }

    [HarmonyPatch(typeof(Description), "GetShortDescription")]
    public static class DescriptionPatch
    {
        static void Postfix(ref string __result)
        {
            if (__result != null && __result.StartsWith("You see "))
            {
                string content = __result.Substring(8).TrimEnd('.');
                __result = KoreanTextHelper.ResolveJosa(content + "{을/를} 본다.");
            }
        }
    }

    // =================================================================
    // 4. TextMeshPro 글로벌 폰트 적용 패치
    // =================================================================
    [HarmonyPatch(typeof(TMPro.TextMeshProUGUI), "OnEnable")]
    public static class TMPUGUIOnEnablePatch
    {
        static void Postfix(TMPro.TextMeshProUGUI __instance)
        {
            try
            {
                QudKRTranslation.Core.FontManager.ApplyFallbackToTMPComponent(__instance);
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(TMPro.TextMeshPro), "OnEnable")]
    public static class TMPOnEnablePatch
    {
        static void Postfix(TMPro.TextMeshPro __instance)
        {
            try
            {
                QudKRTranslation.Core.FontManager.ApplyFallbackToTMPComponent(__instance);
            }
            catch { }
        }
    }

    // =================================================================
    // 3. 한국어 유틸리티
    // =================================================================
    public static class KoreanTextHelper
    {
        public static bool HasJongsung(char c)
        {
            if (c < 0xAC00 || c > 0xD7A3) return false;
            return (c - 0xAC00) % 28 != 0;
        }
        public static string ResolveJosa(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('{') == -1) return text;
            StringBuilder sb = new StringBuilder(text);
            ProcessPattern(sb, "{을/를}", "을", "를");
            ProcessPattern(sb, "{이/가}", "이", "가");
            ProcessPattern(sb, "{은/는}", "은", "는");
            ProcessPattern(sb, "{와/과}", "과", "와");
            ProcessPattern(sb, "{으로/로}", "으로", "로");
            return sb.ToString();
        }
        private static void ProcessPattern(StringBuilder sb, string pattern, string josaWith, string josaWithout)
        {
            string str = sb.ToString();
            int offset = 0;
            while (true)
            {
                int idx = str.IndexOf(pattern, offset);
                if (idx == -1) break;

                // 조사 바로 앞 글자 찾기
                char target = ' ';
                if (idx > 0)
                {
                    target = str[idx - 1];
                    
                    // 만약 앞 글자가 '}'라면, 색상 태그({{...}})가 닫히는 부분인지 확인
                    if (target == '}')
                    {
                        // 색상 태그 내부의 마지막 글자를 찾는다
                        // 예: {{w|검}}{을/를} -> '검'이 target이 되어야 함
                        
                        // 정규식으로 현재 위치 바로 앞의 색상 태그 추출
                        // pattern: {{[a-zA-Z]\|([^}]+)}}
                        string sub = str.Substring(0, idx);
                        var tagMatch = Regex.Match(sub, @"\{\{[a-zA-Z]\|([^}]+)\}\}$");
                        
                        if (tagMatch.Success)
                        {
                            string innerContent = tagMatch.Groups[1].Value;
                            if (!string.IsNullOrEmpty(innerContent))
                            {
                                // 태그 내용물 중 마지막 문자를 target으로 설정
                                // (보통 마지막 글자가 한글일 가능성이 높음)
                                target = innerContent[innerContent.Length - 1];
                            }
                        }
                    }
                }

                string replacement = HasJongsung(target) ? josaWith : josaWithout;
                sb.Remove(idx, pattern.Length);
                sb.Insert(idx, replacement);
                
                // 문자열 변경됨, 다시 문자열 갱신 (비효율적이지만 안전함)
                str = sb.ToString();
                offset = idx + replacement.Length;
            }
        }
    }
}
