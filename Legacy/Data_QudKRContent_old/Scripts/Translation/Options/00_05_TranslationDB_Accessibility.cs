/*
 * 파일명: 00_05_TranslationDB_Accessibility.cs
 * 분류: [System] 번역 데이터베이스 - 접근성
 * 역할: 접근성 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // Accessibility 카테고리
        public static Dictionary<string, string> Options_Accessibility = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "Accessibility", "접근성" },
            { "ACCESSIBILITY", "접근성" },
            { "<color=#77BFCFFF>ACCESSIBILITY</color>", "<color=#77BFCFFF>접근성</color>" },
            
            // 색맹 지원
            { "Disable fullscreen color effects (for color blindness)", "전체화면 색상 효과 비활성화 (색맹용)" },
            { "Display hits with alphanumerics instead of dots (for color blindness)", "점 대신 영숫자로 타격 표시 (색맹용)" },
            
            // 효과 비활성화
            { "Disable most tile-based flashing effects", "대부분의 타일 기반 깜빡임 효과 비활성화" },
            { "Disable tile-based screen-warping effects", "타일 기반 화면 왜곡 효과 비활성화" },
            { "Disable fullscreen screen-warping effects", "전체화면 왜곡 효과 비활성화" },
            
            // 탐색
            { "Navigate", "탐색" },
            { "Collapse All", "모두 접기" },
            { "Expand All", "모두 펼치기" },
            { "Toggle Visibility", "표시 전환" }
        };
    }
}
