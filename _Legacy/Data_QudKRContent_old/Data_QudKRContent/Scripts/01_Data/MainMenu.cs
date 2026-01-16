/*
 * 파일명: MainMenu.cs
 * 분류: [Data] 메인 메뉴 텍스트
 * 역할: 메인 메뉴 화면의 텍스트를 정의합니다.
 * 작성일: 2026-01-15
 * 출처: 기존 Data_QudKRContent 프로젝트에서 마이그레이션
 */

using System.Collections.Generic;
using static QudKRTranslation.Core.G;  // G._ 사용

namespace QudKRTranslation.Data
{
    /// <summary>
    /// 메인 메뉴 텍스트 - Glossary 시스템 사용
    /// </summary>
    public static class MainMenuData
    {
        public static Dictionary<string, string> Translations
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    // 왼쪽 메뉴 (초간단!)
                    { "New Game", _("ui.newGame") },
                    { "Continue", _("ui.continue") },
                    { "Load Game", _("ui.loadGame") },
                    { "Options", _("ui.options") },
                    { "Mods", _("ui.mods") },
                    { "Quit", _("ui.quit") },
                    
                    // 나머지는 하드코딩 유지
                    { "Records", "기록실" },
                    { "Daily Challenge", "일일 도전" },
                    { "Weekly Challenge", "주간 도전" },
                    { "Redeem Code", "코드 입력" },
                    { "Modding Toolkit", "모딩 도구" },
                    { "Credits", "제작진" },
                    { "Help", "도움말" },
                    { "Overlay UI", "오버레이 UI" },
                    { "System", "시스템" },
                    { "Library", "라이브러리" },
                    { "Save Game", "게임 저장하기" },
                    { "Quit to Desktop", "게임 종료" },
                    { "Quit to Main Menu", "메인 메뉴로" },
                    { "Classic", "클래식" },
                    { "Roleplay", "롤플레이" },
                    { "Wander", "방랑" },
                    { "Press any key to continue", "아무 키나 눌러 계속하기" },
                    { "Loading...", "로딩 중..." },
                    { "Are you sure you want to quit?", "정말 종료하시겠습니까?" },
                    { "Redeem a Code", "코드 입력" },
                    { "That save file looks like it's from an older save format revision", "이 저장 파일은 이전 버전의 형식인 것으로 보입니다" },
                    { "You can probably change to a previous branch in your game client and get it to load if you want to finish it off.", "게임 클라이언트에서 이전 브랜치로 변경하면 불러올 수 있을 것입니다." },
                    { "Game Deleted!", "게임이 삭제되었습니다!" }
                };
            }
        }
    }
}
