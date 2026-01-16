/*
 * 파일명: 00_10_TranslationDB_Performance.cs
 * 분류: [System] 번역 데이터베이스 - 성능
 * 역할: 성능 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // Performance 카테고리
        public static Dictionary<string, string> Options_Performance = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "Performance", "성능" },
            { "PERFORMANCE", "성능" },
            { "<color=#77BFCFFF>PERFORMANCE</color>", "<color=#77BFCFFF>성능</color>" },
            
            // 성능 설정
            { "Check memory usage and show a warning when it's high", "메모리 사용량 확인 및 높을 때 경고 표시" },
            { "Flush zones to cache early (for low memory environments)", "구역을 캐시에 조기 플러시 (저메모리 환경용)" },
            { "Garbage collect after each zone flush (for low memory environments)", "각 구역 플러시 후 가비지 수집 (저메모리 환경용)" },
            { "Do not generate floor texture objects", "바닥 텍스처 오브젝트 생성 안 함" },
            { "Throttle animations (for low CPU environments)", "애니메이션 제한 (저CPU 환경용)" }
        };
    }
}
