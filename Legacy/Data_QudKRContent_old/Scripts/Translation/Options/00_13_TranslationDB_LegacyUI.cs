/*
 * 파일명: 00_13_TranslationDB_LegacyUI.cs
 * 분류: [System] 번역 데이터베이스 - 레거시 UI
 * 역할: 레거시 UI 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // Legacy UI 카테고리
        public static Dictionary<string, string> Options_LegacyUI = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "Legacy UI", "레거시 UI" },
            { "LEGACY UI", "레거시 UI" },
            { "<color=#77BFCFFF>LEGACY UI</color>", "<color=#77BFCFFF>레거시 UI</color>" },
            
            // 레거시 UI 설정
            { "Display ability icons on the sidebar", "사이드바에 능력 아이콘 표시" },
            { "Display contents of current cell in a popup", "현재 셀 내용을 팝업으로 표시" },
            { "Color hit hearts per the target's remaining HP", "대상의 남은 HP에 따라 하트 색상 변경" },
            { "Show number of items in each inventory category", "각 인벤토리 카테고리의 아이템 수 표시" },
            { "Display location instead of name on the sidebar", "사이드바에 이름 대신 위치 표시" },
            { "Always map directions to numpad", "방향키를 항상 넘패드에 매핑" },
            { "Map Shift+directions to menu pagination binds (doesn't work for numpad bindings)", "Shift+방향키를 메뉴 페이지 전환에 매핑 (넘패드 바인딩에서는 작동 안 함)" },
            { "Always pass A-Z hotkeys through to the legacy UI", "A-Z 단축키를 항상 레거시 UI로 전달" }
        };
    }
}
