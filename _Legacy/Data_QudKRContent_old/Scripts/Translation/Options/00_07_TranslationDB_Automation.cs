/*
 * 파일명: 00_07_TranslationDB_Automation.cs
 * 분류: [System] 번역 데이터베이스 - 자동화
 * 역할: 자동화 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // Automation 카테고리
        public static Dictionary<string, string> Options_Automation = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "Automation", "자동화" },
            { "AUTOMATION", "자동화" },
            { "<color=#77BFCFFF>AUTOMATION</color>", "<color=#77BFCFFF>자동화</color>" },
            
            // 자동 행동
            { "Automatically drink fresh water when thirsty", "목마를 때 자동으로 깨끗한 물 마시기" },
            { "Thirst threshold for automatic drinking", "자동 음수 갈증 임계값" },
            { "Automatically douse and light torches", "횃불 자동 끄기/켜기" },
            { "Automatically disassemble scrap", "고철 자동 분해" },
            
            // 적 무시
            { "Difficulty threshold for ignoring hostile creatures", "적대 생물 무시 난이도 임계값" },
            { "Range threshold for ignoring hostile creatures", "적대 생물 무시 거리 임계값" },
            { "Attack ignored hostiles that you move adjacent to during autoexplore", "자동 탐색 중 인접한 무시된 적 공격" },
            { "Attack ignored hostiles that you move adjacent to during autoexplore ", "자동 탐색 중 인접한 무시된 적 공격" },
            
            // 이동 중단
            { "Interrupt held movement when hostiles are nearby", "적이 근처에 있을 때 이동 중단" },
            { "Interrupt held movement when entering an unexplored zone", "미탐험 구역 진입 시 이동 중단" },
            { "Navigate inventory and equipment panes with left/right pagination instead of movement binds", "이동 키 대신 좌/우 페이지 전환으로 인벤토리 및 장비 창 탐색" },
            { "Navigate inventory and equipment panes with left/right pagination instead of movement binds ", "이동 키 대신 좌/우 페이지 전환으로 인벤토리 및 장비 창 탐색" },
            { "Use text autoact interrupt indicator instead of flashing red box", "깜빡이는 빨간 상자 대신 텍스트 자동 행동 중단 표시 사용" },
            
            // 기타
            { "Search containers while autoexploring", "자동 탐험 중 컨테이너 검색" },
            { "Automatically dig through walls by moving into them when wielding a digging implement", "굴착 도구 장착 시 벽으로 이동하여 자동 굴착" },
            { "Automatically save when moving to a different zone", "다른 구역으로 이동 시 자동 저장" },
            { "Maximum automove cells/sec and autoattack actions/sec", "최대 자동 이동 셀/초 및 자동 공격 행동/초" },
            
            // 갈증 수준
            { "Dehydrated", "탈수됨" },
            { "Parched", "매우 목마름" },
            { "Thirsty", "목마름" },
            { "Quenched", "갈증 해소됨" },
            { "Tumescent", "수분 과잉" },
            
            // 난이도 및 거리
            { "None", "없음" },
            { "Easy", "쉬움" },
            { "Average", "보통" },
            { "Tough", "어려움" },
            { "Very Tough", "매우 어려움" },
            { "Impossible", "불가능" }
        };
    }
}
