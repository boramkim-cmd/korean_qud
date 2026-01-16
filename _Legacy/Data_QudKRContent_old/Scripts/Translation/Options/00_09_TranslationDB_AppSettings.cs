/*
 * 파일명: 00_09_TranslationDB_AppSettings.cs
 * 분류: [System] 번역 데이터베이스 - 앱 설정
 * 역할: 앱 설정 관련 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // App Settings 카테고리
        public static Dictionary<string, string> Options_AppSettings = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "App Settings", "앱 설정" },
            { "APP SETTINGS", "앱 설정" },
            { "<color=#77BFCFFF>APP SETTINGS</color>", "<color=#77BFCFFF>앱 설정</color>" },
            
            // 앱 설정
            { "Show advanced options", "고급 옵션 표시" },
            { "Keep Caves of Qud active in background", "Caves of Qud를 백그라운드에서 활성 상태로 유지" },
            { "Number of rolling backup saves", "롤링 백업 저장 개수" },
            { "Send anonymous gameplay statistics", "익명 게임플레이 통계 전송" },
            { "Disable input warning", "입력 경고 비활성화" }
        };
    }
}
