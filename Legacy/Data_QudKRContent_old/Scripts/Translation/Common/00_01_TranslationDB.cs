/*
 * 파일명: 00_01_TranslationDB.cs
 * 분류: [Core] 데이터베이스
 * 역할: 모든 화면의 번역 데이터 관리 + 능력치 설명 추가
 * 수정일: 2026-01-14 (Update)
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static class DictDB
    {
        // [20_02] 상태창 & 능력치 (Attributes)
        public static Dictionary<string, string> Status = new Dictionary<string, string>()
        {
            // 하단 메뉴
            { "Show Effects", "효과 보기" }, { "Buy Mutation", "변이 구매" },
            { "Skill", "스킬" }, { "Powers", "권능" },
            { "Reputation", "평판" }, { "Cybernetics", "사이버네틱스" },
            { "Inventory", "인벤토리" }, { "Equipment", "장비" },
            
            // 능력치 이름 (색상 태그 무시하고 번역됨)
            { "Strength", "힘" },
            { "Agility", "민첩" },
            { "Toughness", "건강" },
            { "Intelligence", "지능" },
            { "Willpower", "의지력" },
            { "Ego", "자아" },
            { "Hit Points", "생명력" },
            { "XP", "경험치" },
            { "Level", "레벨" },

            // 헤더
            { "MAIN ATTRIBUTES", "주요 능력치" }
        };

        // ✅ [Descriptions] 긴 설명문 번역 (앞부분만 일치해도 번역되게 처리함)
        public static Dictionary<string, string> StatusDescriptions = new Dictionary<string, string>()
        {
            { "Strength determines", "힘은 근접 공격 관통력, 강제 이동 저항력, 그리고 최대 소지 무게를 결정합니다." },
            { "Agility determines", "민첩은 근접 및 원거리 무기 명중률과 공격 회피 능력을 결정합니다." },
            { "Toughness determines", "건강은 최대 생명력, 자연 회복률, 그리고 독과 질병에 대한 저항력을 결정합니다." },
            { "Intelligence determines", "지능은 스킬 포인트(SP) 획득량, 유물 감정 능력, 그리고 기술 조작(팅커링) 능력을 결정합니다." },
            { "Willpower determines", "의지력은 정신 공격 저항력(MA)과 사용 기술의 재사용 대기시간 회복 속도를 결정합니다." },
            { "Ego determines", "자아는 정신 변이의 위력, 상인과의 흥정 능력, 그리고 생명체를 포섭하는 능력을 결정합니다." }
        };

        // [공통] 일반 UI 텍스트
        public static Dictionary<string, string> UICommon = new Dictionary<string, string>() { 
            { "Yes", "예" }, { "No", "아니오" }, { "OK", "확인" }, { "Cancel", "취소" }, 
            { "Accept", "수락" }, { "Ignore", "무시" }, { "Keep", "유지" }, { "Delete", "삭제" },
            { "Back", "뒤로" }, { "None", "없음" }, { "Unknown", "알 수 없음" }
        };
        public static Dictionary<string, string> Options = new Dictionary<string, string>() { { "Video", "비디오" }, { "Audio", "오디오" }, { "Controls", "조작" }, { "Interface", "인터페이스" }, { "Automation", "자동화" }, { "Prompts", "알림" }, { "Prerelease Content", "베타 콘텐츠" }, { "Debug", "디버그" }, { "Back", "뒤로" }, { "Restore Defaults", "기본값 복원" } };
        public static Dictionary<string, string> Inventory = new Dictionary<string, string>() { { "Inventory", "인벤토리" }, { "Equipment", "장비" }, { "Nearby", "주변" }, { "Weight", "무게" }, { "Value", "가치" }, { "Category", "분류" } };
        public static Dictionary<string, string> Trade = new Dictionary<string, string>() { { "Inventory", "인벤토리" }, { "Trade", "거래" }, { "Offer", "제안" }, { "Checkout", "계산하기" }, { "Weight", "무게" }, { "Value", "가치" }, { "Category", "분류" }, { "Cost", "비용" } };
        // ✅ [Descriptions] 긴 설명문 번역 (앞부분만 일치해도 번역되게 처리함)
        public static Dictionary<string, string> StatusDescriptions = new Dictionary<string, string>()
        {
            { "Strength determines", "힘은 근접 공격 관통력, 강제 이동 저항력, 그리고 최대 소지 무게를 결정합니다." },
            { "Agility determines", "민첩은 근접 및 원거리 무기 명중률과 공격 회피 능력을 결정합니다." },
            { "Toughness determines", "건강은 최대 생명력, 자연 회복률, 그리고 독과 질병에 대한 저항력을 결정합니다." },
            { "Intelligence determines", "지능은 스킬 포인트(SP) 획득량, 유물 감정 능력, 그리고 기술 조작(팅커링) 능력을 결정합니다." },
            { "Willpower determines", "의지력은 정신 공격 저항력(MA)과 사용 기술의 재사용 대기시간 회복 속도를 결정합니다." },
            { "Ego determines", "자아는 정신 변이의 위력, 상인과의 흥정 능력, 그리고 생명체를 포섭하는 능력을 결정합니다." }
        };

        // (기존 사전들 유지)
        public static Dictionary<string, string> Popup = new Dictionary<string, string>() { { "Yes", "예" }, { "No", "아니오" }, { "OK", "확인" }, { "Cancel", "취소" }, { "Accept", "수락" }, { "Ignore", "무시" }, { "Keep", "유지" }, { "Delete", "삭제" } };
    }
}