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
            // 상단 카테고리
            { "General", "일반" },
            { "Graphics", "그래픽" },
            { "Audio", "오디오" },
            { "Gameplay", "게임플레이" },
            { "Controls", "조작" },
            { "Interface", "인터페이스" },
            { "Keybinds", "단축키" },
            { "Debug", "디버그" },
            { "Modding", "모딩" },
            
            // 일반 설정
            { "Language", "언어" },
            { "Autosave", "자동 저장" },
            { "Show Tutorials", "튜토리얼 표시" },
            
            // 그래픽 설정
            { "Resolution", "해상도" },
            { "Fullscreen", "전체 화면" },
            { "UI Scale", "UI 크기" },
            { "Tile Set", "타일셋" },
            
            // 공통 버튼 및 라벨
            { "Apply", "적용" },
            { "Back", "뒤로" },
            { "Default", "기본값" },
            { "Cancel", "취소" },
            { "Reset to Defaults", "초기화" },
            
            // 툴팁/설명 (예시)
            { "Adjust the volume of the sound effects.", "효과음의 음량을 조절합니다." },
            { "Toggle whether the game automatically saves at key points.", "주요 시점에서 게임을 자동으로 저장할지 여부를 설정합니다." }
        };
    }
}
