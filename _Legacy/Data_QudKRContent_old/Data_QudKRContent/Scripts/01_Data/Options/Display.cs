/*
 * 파일명: Display.cs
 * 분류: [Data] 옵션 - 디스플레이
 * 역할: 디스플레이 설정 옵션 텍스트를 정의합니다.
 * 작성일: 2026-01-15
 * 출처: 기존 Data_QudKRContent 프로젝트에서 마이그레이션
 */

using System.Collections.Generic;

namespace QudKRTranslation.Data.Options
{
    public static class DisplayData
    {
        public static Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            // 카테고리
            { "Display", "디스플레이" },
            { "DISPLAY", "디스플레이" },
            { "<color=#77BFCFFF>DISPLAY</color>", "<color=#77BFCFFF>디스플레이</color>" },
            
            // 화면 설정
            { "Brightness", "밝기" },
            { "Contrast", "대비" },
            { "Fullscreen", "전체화면" },
            { "Fullscreen resolution", "전체화면 해상도" },
            { "Frame rate", "프레임 레이트" },
            { "Screen unset", "화면 미설정" },
            { "Unset", "미설정" },
            { "Screen", "스크린" },
            { "Unlimited", "무제한" },
            { "VSync", "수직동기화" },
            
            // 그래픽 효과
            { "Enable tile graphics", "타일 그래픽 활성화" },
            { "Display vignette effect", "비네트 효과 표시" },
            { "Display scanlines", "스캔라인 표시" },
            { "Display combat animations", "전투 애니메이션 표시" },
            { "Display floating damage numbers", "떠다니는 데미지 숫자 표시" },
            { "Display modern visual effects", "현대적 시각 효과 표시" },
            { "Display ASCII visual effects", "ASCII 시각 효과 표시" },
            
            // 비활성화 옵션
            { "Disable floor textures", "바닥 텍스처 비활성화" },
            { "Disable blood splatters", "피 튀김 비활성화" },
            { "Disable smoke effects", "연기 효과 비활성화" },
            { "Disable modern, tile-based graphical effects", "현대적 타일 기반 그래픽 효과 비활성화" },
            
            // 기타
            { "Main menu background art", "메인 메뉴 배경 아트" }
        };
    }
}
