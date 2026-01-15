/*
 * 파일명: 00_14_TranslationDB_Debug.cs
 * 분류: [System] 번역 데이터베이스 - 디버그
 * 역할: 디버그 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // Debug 카테고리
        public static Dictionary<string, string> Options_Debug = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "Debug", "디버그" },
            { "DEBUG", "디버그" },
            { "<color=#77BFCFFF>DEBUG</color>", "<color=#77BFCFFF>디버그</color>" },
            
            // 게임 모드 설정
            { "Don't delete Classic mode saved games on death", "클래식 모드 사망 시 저장 파일 삭제 안 함" },
            { "Enable save and load", "저장 및 불러오기 활성화" },
            { "Get prompted to confirm deaths", "사망 확인 프롬프트 표시" },
            { "Enable unfinished and unsupported content", "미완성 및 미지원 콘텐츠 활성화" },
            { "Allow creatures to have multiple defects ", "생물이 여러 결함 가질 수 있도록 허용" },
            
            // UI 디버그
            { "Strip color formatting from UI text", "UI 텍스트에서 색상 서식 제거" },
            { "Enable color menu for text input", "텍스트 입력용 색상 메뉴 활성화" },
            { "Disable achievements", "업적 비활성화" },
            { "Show quickstart option during character creation", "캐릭터 생성 중 빠른 시작 옵션 표시" },
            
            // 시스템 디버그
            { "Disable brain hijacking on player-controlled characters", "플레이어 제어 캐릭터의 뇌 하이재킹 비활성화" },
            { "Disable zone caching", "구역 캐싱 비활성화" },
            { "Disable limit on zone build tries", "구역 생성 시도 제한 비활성화" },
            { "Pregenerate zone names for wish support", "위시 지원을 위한 구역 이름 사전 생성" },
            
            // 정보 표시
            { "Show reputation with a creature's factions when looking at them", "생물 조사 시 해당 생물의 세력과의 평판 표시" },
            { "Show XML conversation and node names during conversation", "대화 중 XML 대화 및 노드 이름 표시" },
            { "Show the full zone ID during zone creation", "구역 생성 중 전체 구역 ID 표시" },
            
            // 전투 디버그
            { "Show damage penetration debug text", "데미지 관통 디버그 텍스트 표시" },
            { "Show saving throw debug text", "내성 굴림 디버그 텍스트 표시" },
            { "Show lost chance debug text", "상실 확률 디버그 텍스트 표시" },
            { "Show debug text for stat shifts", "능력치 변화 디버그 텍스트 표시" },
            { "Show encounter chance debug chance", "조우 확률 디버그 정보 표시" },
            { "Show travel speed debug text", "이동 속도 디버그 텍스트 표시" },
            
            // 오브젝트 디버그
            { "Show debug info on objects", "오브젝트 디버그 정보 표시" },
            { "Show attitude info on objects", "오브젝트 태도 정보 표시" },
            
            // 월드맵 디버그
            { "Indicate special encounters on the worldmap", "월드맵에 특수 조우 표시" },
            { "Indicate biomes on the worldmap", "월드맵에 생물군계 표시" },
            
            // 맵 생성 디버그
            { "Draw population placement hint maps when building zones", "구역 생성 시 개체군 배치 힌트 맵 그리기" },
            { "Draw population placement regions when building zones", "구역 생성 시 개체군 배치 영역 그리기" },
            { "Draw creature pathfinding", "생물 경로 탐색 그리기" },
            { "Draw creature pathfinding ", "생물 경로 탐색 그리기" },  // 공백 포함 버전
            { "Wait for keypress when drawing pathfinding", "경로 탐색 그리기 시 키 입력 대기" },
            { "Draw navigation weight maps on each step", "각 단계마다 탐색 가중치 맵 그리기" },
            { "Draw cellular automata when used in map generation", "맵 생성 시 셀룰러 오토마타 그리기" },
            { "Draw reachability maps on zone generation", "구역 생성 시 도달 가능성 맵 그리기" },
            { "Draw semantic tag info when building zones", "구역 생성 시 의미 태그 정보 그리기" },
            
            // 시각화 디버그
            { "Draw visibility flood fill", "시야 범람 채우기 그리기" },
            { "Draw audibility flood fill", "청각 범람 채우기 그리기" },
            { "Draw olfaction flood fill", "후각 범람 채우기 그리기" },
            { "Draw electrical arc history for last few rounds", "최근 몇 라운드의 전기 아크 이력 그리기" },
            
            // 사운드 디버그
            { "Write played sound file names to message log", "재생된 사운드 파일 이름을 메시지 로그에 기록" },
            { "Disable preloading of sounds", "사운드 사전 로딩 비활성화" },
            
            // 기타 디버그
            { "Perform inventory consistency checks", "인벤토리 일관성 검사 수행" },
            { "Show error popups", "오류 팝업 표시" },
            { "Disable conflict checking when rebinding controls", "컨트롤 재바인딩 시 충돌 검사 비활성화" },
            { "Enable workaround for Steam Deck shift key bug", "Steam Deck Shift 키 버그 우회 활성화" }
        };
    }
}
