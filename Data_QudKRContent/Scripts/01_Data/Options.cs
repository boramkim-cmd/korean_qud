/*
 * 파일명: OptionsData.cs
 * 분류: [Data] 설정 화면 텍스트
 * 역할: 게임 설정(Options) 화면의 카테고리 및 설정 항목 텍스트를 정의합니다.
 */

using System.Collections.Generic;

namespace QudKRTranslation.Data
{
    public static class OptionsData
    {
        public static Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            // === [1] 카테고리 (좌측 사이드바 & 헤더) ===
            { "Sound", "사운드" },
            { "SOUND", "사운드" },
            { "Display", "디스플레이" },
            { "DISPLAY", "디스플레이" },
            { "Controls", "조작" },
            { "CONTROLS", "조작" },
            { "Accessibility", "접근성" },
            { "ACCESSIBILITY", "접근성" },
            { "UI", "UI" },
            { "Legacy UI", "레거시 UI" },
            { "Automation", "자동화" },
            { "Autoget", "자동 습득" },
            { "Prompts", "알림" },
            { "Mods", "모드" },
            { "MODS", "모드" },
            { "Performance", "성능" },
            { "App Settings", "앱 설정" },
            { "Debug", "디버그" },
            { "DEBUG", "디버그" },

            // === [2] 사운드 (Sound) 항목 ===
            { "Main volume", "주 볼륨" },
            { "Sound effects", "효과음" },
            { "Sound effects volume", "효과음 볼륨" },
            { "Music", "음악" },
            { "Music volume", "음악 볼륨" },
            { "Ambient sounds", "환경음" },
            { "Ambient volume", "환경음 볼륨" },
            { "Interface sounds", "인터페이스 소리" },
            { "Interface volume", "인터페이스 볼륨" },
            { "Combat sounds", "전투 소리" },
            { "Combat volume", "전투 볼륨" },
            { "Fire crackling sounds", "불 타는 소리" },

            // === [3] 디스플레이 (Display) 항목 ===
            { "Brightness", "밝기" },
            { "Contrast", "대비" },
            { "Fullscreen", "전체 화면" }, // 이미 작동 중
            { "Fullscreen resolution", "전체 화면 해상도" },
            { "Frame rate", "프레임 레이트" },
            { "Enable tile graphics", "타일 그래픽 활성화" },
            { "Display vignette effect", "비네트 효과 표시" },
            { "Display scanlines", "스캔라인 표시" },
            { "Display combat animations", "전투 애니메이션 표시" },
            { "Display floating damage numbers", "피해 수치 표시" },
            { "Display modern visual effects", "최신 시각 효과 표시" },
            { "Use pixel font", "픽셀 폰트 사용" }, 
            
            // === [4] 상단 & 하단 네비게이션 ===
            { "Show advanced options", "고급 설정 표시" },
            { "<search>", "검색" }, // 검색창 플레이스홀더
            { "navigate", "이동" },
            { "Collapse All", "모두 접기" },
            { "Expand All", "모두 펼치기" },
            { "Toggle Visibilty", "보기 전환" }, // 오타 고려 (Visibilty)
            { "Toggle Visibility", "보기 전환" }, // 정타
            { "Back", "뒤로" },
            { "Apply", "적용" },
            { "Default", "기본값" },

            // === [5] 기타 누락 가능성 높은 항목들 ===
            { "Language", "언어" },
            { "Autosave", "자동 저장" },
            { "Show Tutorials", "튜토리얼 표시" },
            { "Zoom level", "확대 레벨" },
            { "Sidebar width", "사이드바 너비" },
            { "Message log opacity", "메시지 로그 투명도" },
            { "Minimap", "미니맵" },
            { "Show overlay map", "오버레이 지도 표시" },
            { "Overlay map opacity", "오버레이 지도 투명도" }
        };
    }
}
