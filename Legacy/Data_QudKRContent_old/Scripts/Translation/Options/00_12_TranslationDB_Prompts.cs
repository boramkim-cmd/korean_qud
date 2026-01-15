/*
 * 파일명: 00_12_TranslationDB_Prompts.cs
 * 분류: [System] 번역 데이터베이스 - 알림
 * 역할: 알림/프롬프트 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // Prompts 카테고리
        public static Dictionary<string, string> Options_Prompts = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "Prompts", "알림" },
            { "PROMPTS", "알림" },
            { "<color=#77BFCFFF>PROMPTS</color>", "<color=#77BFCFFF>알림</color>" },
            
            // 프롬프트 설정
            { "Prompt before moving to the world map", "월드맵 이동 전 확인" },
            { "Prompt before autowalking to stairs", "계단 자동 이동 전 확인" },
            { "Prompt before swimming", "수영 전 확인" },
            { "Prompt before drinking and moving into certain dangerous liquids", "위험한 액체 마시기/이동 전 확인" },
            { "Prompt list of items when getting from the ground, even if there's only one", "바닥에서 줍기 시 항목 목록 표시 (1개여도)" },
            { "Threshold for low hitpoint warning", "낮은 체력 경고 임계값" },
            { "Show popups when you dismember or decapitate someone", "절단/참수 시 팝업 표시" },
            { "Show ability-on-cooldown prompts as message log entries instead of popups", "능력 쿨다운 알림을 팝업 대신 메시지 로그로 표시" },
            { "Show scavenged items as message log entries instead of popups", "수집한 아이템을 팝업 대신 메시지 로그로 표시" },
            { "Display popups when noting information in your journal", "일지 기록 시 팝업 표시" },
            { "Display verbose level up messages for companions", "동료 레벨업 시 상세 메시지 표시" },
            { "Default the selection on death prompts in Roleplay mode to 'View final messages' instead of 'Reload checkpoint'", "롤플레이 모드 사망 시 기본 선택을 '체크포인트 재로드' 대신 '마지막 메시지 보기'로 설정" }
        };
    }
}
