/*
 * 파일명: Status.cs
 * 분류: [Data] 캐릭터 상태창 텍스트
 * 역할: 캐릭터 상태창의 텍스트를 정의합니다.
 * 작성일: 2026-01-15
 * 출처: 기존 Data_QudKRContent 프로젝트에서 마이그레이션
 */

using System.Collections.Generic;

namespace QudKRTranslation.Data
{
    public static class StatusData
    {
        public static Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            // 하단 메뉴
            { "Show Effects", "효과 보기" },
            { "Buy Mutation", "변이 구매" },
            { "Skill", "스킬" },
            { "Skills", "스킬" },
            { "Powers", "권능" },
            { "Power", "권능" },
            { "Reputation", "평판" },
            { "Cybernetics", "사이버네틱스" },
            { "Inventory", "인벤토리" },
            { "Equipment", "장비" },
            
            // 능력치
            { "Level", "레벨" },
            { "Experience", "경험치" },
            { "Health", "체력" },
            { "Strength", "힘" },
            { "Agility", "민첩" },
            { "Toughness", "건강" },
            { "Intelligence", "지능" },
            { "Willpower", "의지력" },
            { "Ego", "자아" },
            
            // 속성 헤더
            { "MAIN ATTRIBUTES", "주요 속성" },
            { "SECONDARY ATTRIBUTES", "보조 속성" },
            { "RESISTANCES", "저항력" },
            { "MUTATION", "변이" },
            
            // 약어
            { "QN", "속도" },
            { "MS", "이속" },
            { "AV", "장갑" },
            { "DV", "회피" },
            { "MA", "정신" },
            { "AR", "산성" },
            { "ER", "전기" },
            { "CR", "냉기" },
            { "HR", "열기" },
            
            // 변이 타입
            { "Physical Mutation", "신체적 변이" },
            { "Mental Mutation", "정신적 변이" },
            
            // 상태 표시
            { "[Unlearned]", "[미습득]" },
            { "{{R|[Unlearned]}}", "{{R|[미습득]}}" },
            { "{{W|[Unlearned]}}", "{{W|[미습득]}}" },
            { "[Learned]", "[습득함]" },
            { "{{G|[Learned]}}", "{{G|[습득함]}}" },
            { "[Morphotype]", "[형태군]" },
            { "Compute Power (CP)", "연산 능력 (CP)" },
            { "Collapse", "접기" },
            { "Expand", "펼치기" },
            { "-Buy", "-구매" },
            
            // 탭 이름
            { "Stats", "능력치" },
            { "Factions", "세력" },
            { "Journal", "일지" },
            { "SKILLS", "스킬" },
            { "Attributes", "속성" },
            { "Attributes & Powers", "속성 및 권능" },
            { "Attributes && Powers", "속성 및 권능" },
            { "ATTRIBUTES & POWERS", "속성 및 권능" },
            { "EQUIPMENT", "장비" },
            { "TINKERING", "팅커링" },
            { "JOURNAL", "일지" },
            { "QUESTS", "퀘스트" },
            { "REPUTATION", "평판" },
            { "MESSAGE LOG", "메시지 기록" },
            { "Message Log", "메시지 기록" },
            { "Messages", "메시지" },
            { "Active Effects", "활성 효과" },
            { "Log", "로그" },
            
            // 일지 카테고리
            { "Locations", "장소" },
            { "Gossip and Lore", "소문과 전승" },
            { "Sultan Histories", "술탄의 역사" },
            { "Village Histories", "마을의 역사" },
            { "Chronology", "연대기" },
            { "General Notes", "일반 노트" },
            { "Recipes", "요리법" },
            
            // 일지 메시지
            { "You have no knowledge of any villages.", "아는 마을이 없습니다." },
            { "You have no knowledge of the sultans.", "아는 술탄이 없습니다." },
            { "You have no map notes.", "지도 노트가 없습니다." },
            { "You have no history. That's pretty weird to be honest.", "기록된 역사가 없습니다. 솔직히 좀 이상하네요." },
            { "You have made no observations.", "관찰한 기록이 없습니다." },
            { "You have learned no recipes.", "배운 요리법이 없습니다." },
            
            // 팅커링
            { "Build", "제작" },
            { "Mod", "개조" },
            { "switch to modifications", "개조 모드로 전환" },
            { "switch to build", "제작 모드로 전환" },
            { "~<none>", "~<없음>" },
            
            // 세력
            { "Highest reputation", "평판 높은 순" },
            { "Lowest reputation", "평판 낮은 순" },
            { "Alphabetical", "가나다순" },
            { "Expand All", "모두 펼치기" },
            { "Collapse All", "모두 접기" },
            
            // 스킬 상세
            { "STR:", "힘:" },
            { "AGI:", "민첩:" },
            { "TOU:", "건강:" },
            { "INT:", "지능:" },
            { "WIL:", "의지:" },
            { "EGO:", "자아:" },
            { "Ex:", "제외:" },
            { "[none]", "[없음]" }
        };
    }
}
