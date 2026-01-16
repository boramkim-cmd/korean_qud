/*
 * 파일명: Common.cs
 * 분류: [Data] 공통 UI 텍스트
 * 역할: 모든 화면에서 사용되는 기본 UI 텍스트를 정의합니다.
 * 작성일: 2026-01-15
 */

using System.Collections.Generic;

namespace QudKRTranslation.Data
{
    /// <summary>
    /// 공통 UI 텍스트 - 모든 화면에서 사용되는 기본 텍스트
    /// </summary>
    public static class CommonData
    {
        public static Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            // 기본 버튼
            { "Yes", "예" },
            { "No", "아니오" },
            { "OK", "확인" },
            { "Cancel", "취소" },
            { "Accept", "수락" },
            { "Decline", "거절" },
            { "Ignore", "무시" },
            { "Keep", "유지" },
            { "Delete", "삭제" },
            { "Back", "뒤로" },
            { "Close", "닫기" },
            { "Continue", "계속" },
            { "Skip", "건너뛰기" },
            
            // 상태
            { "None", "없음" },
            { "Unknown", "알 수 없음" },
            { "Loading", "로딩 중" },
            { "Saving", "저장 중" },
            { "Error", "오류" },
            
            // 방향
            { "Up", "위" },
            { "Down", "아래" },
            { "Left", "왼쪽" },
            { "Right", "오른쪽" },
            
            // 수량
            { "All", "전체" },
            { "Some", "일부" },
            { "Many", "다수" },
            { "Few", "소수" }
        };
    }
}
