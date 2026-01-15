/*
 * 파일명: MainMenu.cs
 * 분류: [Data] 메인 메뉴 텍스트
 * 역할: 메인 메뉴 화면의 텍스트를 정의합니다.
 * 작성일: 2026-01-15
 */

using System.Collections.Generic;

namespace QudKRTranslation.Data
{
    /// <summary>
    /// 메인 메뉴 텍스트
    /// </summary>
    public static class MainMenuData
    {
        public static Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            // 메인 메뉴 항목
            { "New Game", "새 게임" },
            { "Continue", "계속하기" },
            { "Load Game", "게임 불러오기" },
            { "Save Game", "게임 저장하기" },
            { "Options", "옵션" },
            { "Mods", "모드" },
            { "Credits", "크레딧" },
            { "Quit", "종료" },
            { "Quit to Desktop", "게임 종료" },
            { "Quit to Main Menu", "메인 메뉴로" },
            
            // 게임 모드
            { "Classic", "클래식" },
            { "Roleplay", "롤플레이" },
            { "Wander", "방랑" },
            
            // 기타
            { "Press any key to continue", "아무 키나 눌러 계속하기" },
            { "Loading...", "로딩 중..." }
        };
    }
}
