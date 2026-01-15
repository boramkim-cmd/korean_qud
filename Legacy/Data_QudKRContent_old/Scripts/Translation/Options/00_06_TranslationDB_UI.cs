/*
 * 파일명: 00_06_TranslationDB_UI.cs
 * 분류: [System] 번역 데이터베이스 - UI
 * 역할: UI 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // UI 카테고리
        public static Dictionary<string, string> Options_UI = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "UI", "UI" },
            { "<color=#77BFCFFF>UI</color>", "<color=#77BFCFFF>UI</color>" },
            
            // UI 설정
            { "Enable modern UI", "현대적 UI 활성화" },
            { "UI scale", "UI 크기" },
            { "Enable modern UI character sheet", "현대적 UI 캐릭터 시트 활성화" },
            { "Character sheet scale percentage", "캐릭터 시트 크기 비율" },
            { "Character sheet size", "캐릭터 시트 크기" },
            { "Play area scale", "플레이 영역 크기" },
            { "Pixel perfect tile graphic scale", "픽셀 퍼펙트 타일 그래픽 크기" },
            
            // 표시 옵션
            { "Ability bar mode", "능력 바 모드" },
            { "Show minimap", "미니맵 표시" },
            { "Show nearby objects list", "주변 오브젝트 목록 표시" },
            { "Nearby objects list: show only current cell's contents", "주변 오브젝트 목록: 현재 셀 내용만 표시" },
            { "Nearby objects list: show liquid pools", "주변 오브젝트 목록: 액체 웅덩이 표시" },
            { "Nearby objects list: show plants", "주변 오브젝트 목록: 식물 표시" },
            { "Nearby objects list: show only show takeable objects", "주변 오브젝트 목록: 획득 가능한 오브젝트만 표시" },
            { "Message log font size (pt)", "메시지 로그 글꼴 크기 (pt)" },
            { "Dock message log & minimap", "메시지 로그 & 미니맵 도킹" },
            { "Dock background opacity", "도킹 배경 불투명도" },
            
            // 마우스/커서
            { "Mouse cursor appearance", "마우스 커서 모양" },
            { "Automatically hide mouse cursor when inactive", "비활성 시 마우스 커서 자동 숨김" },
            { "Gamepad button appearance", "게임패드 버튼 모양" },
            
            // 캐릭터 표시
            { "Color your character's glyph based on health status", "체력 상태에 따라 캐릭터 글리프 색상 변경" },
            { "Ignore all effects that recolor your character's glyph other than health status", "체력 상태 외 캐릭터 글리프 색상 변경 효과 무시" },
            { "Allow mutations to recolor your character's glyph", "돌연변이가 캐릭터 글리프 색상 변경 허용" },
            
            // 기타 UI
            { "Display overlay keyboard for input on supported devices", "지원 기기에서 입력용 오버레이 키보드 표시" },
            { "Zoom sensitivity", "줌 감도" },
            { "Tooltip delay (ms)", "툴팁 지연 (ms)" },
            { "Display mouse-clickable zone transition arrows", "마우스 클릭 가능한 구역 전환 화살표 표시" },
            { "Pressing shift hides the sidebar", "Shift 누르면 사이드바 숨김" },
            { "When zoomed out to max zoom, remove the UI frame from the main gameplay screen", "최대 줌아웃 시 메인 게임플레이 화면에서 UI 프레임 제거" },
            { "Always highlight stairs", "계단 항상 강조" },
            { "Add a line separator at the end of each round during combat", "전투 중 각 라운드 끝에 구분선 추가" },
            { "Indent body parts by attachment on the Equipment screen", "장비 화면에서 부착 위치에 따라 신체 부위 들여쓰기" },
            { "Display detailed weapon penetration and damage in weapon names", "무기 이름에 상세한 관통력과 데미지 표시" },
            { "Take corpses when using the 'take all' command", "'모두 가져가기' 명령 사용 시 시체도 가져가기" },
            { "Enable a 'drop all' interaction on items in your inventory", "인벤토리 아이템에 '모두 버리기' 상호작용 활성화" },
            { "Navigate inventory and equipment panes with left/right pagination instead of movement binds", "이동 키 대신 좌/우 페이지 전환으로 인벤토리 및 장비 창 탐색" },
            { "Equip/unequip highlighted item when pressing right or left, respectively, on the Equipment screen", "장비 화면에서 우/좌 키로 강조된 아이템 장착/해제" },
            { "Expand the equipment or inventory pane when focused", "포커스 시 장비 또는 인벤토리 창 확장" },
            { "Move tutorial to the end of the game mode list", "튜토리얼을 게임 모드 목록 끝으로 이동" },
            { "Show collector's seals on the main menu", "메인 메뉴에 수집가 인장 표시" },
            { "Show advanced options", "고급 옵션 표시" },
            { "Toggle Option", "옵션 전환" },
            { "Toggle", "전환" },
            { "Select", "선택" },
            
            // 옵션 값
            { "Full", "전체" },
            { "Compact", "간략" },
            { "Full Compact", "전체 간략" },
            { "System", "시스템" },
            { "Default", "기본" },
            { "Alternate", "대체" },
            { "System Default Alternate", "시스템 기본 대체" },
            { "Auto", "자동" },
            { "KBM", "키보드/마우스" },
            { "XBox", "XBox" },
            { "PS", "PS" },
            { "XBox Filled", "XBox 채움" },
            { "PS Filled", "PS 채움" },
            { "Auto KBM XBox PS XBox Filled PS Filled", "자동 키보드/마우스 XBox PS XBox 채움 PS 채움" },
            { "Standard", "표준" },
            { "Full Height", "전체 높이" },
            { "Standard Full Height Full", "표준 전체 높이 전체" },
            { "Fit", "맞춤" },
            { "Cover", "덮기" },
            { "Pixel Perfect", "픽셀 퍼펙트" },
            { "Modern", "현대적" },
            { "Classic", "클래식" },
            { "Modern Classic", "현대적 클래식" }
        };
    }
}
