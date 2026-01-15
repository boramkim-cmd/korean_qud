/*
 * 파일명: 00_11_TranslationDB_Autoget.cs
 * 분류: [System] 번역 데이터베이스 - 자동 획득
 * 역할: 자동 획득 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // Autoget 카테고리
        public static Dictionary<string, string> Options_Autoget = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "Autoget", "자동 획득" },
            { "AUTOGET", "자동 획득" },
            { "<color=#77BFCFFF>AUTOGET</color>", "<color=#77BFCFFF>자동 획득</color>" },
            
            // 자동 획득 항목
            { "Autoget ammo", "탄약 자동 획득" },
            { "Autoget primitive ammo", "원시 탄약 자동 획득" },
            { "Autoget copper, silver, and gold nuggets", "구리, 은, 금 덩어리 자동 획득" },
            { "Autoget trade goods", "교역품 자동 획득" },
            { "Autoget food", "음식 자동 획득" },
            { "Autoget fresh water", "깨끗한 물 자동 획득" },
            { "Autoget liquids from containers you've dropped", "버린 컨테이너의 액체 자동 획득" },
            { "Autoget special items", "특수 아이템 자동 획득" },
            { "Autoget artifacts", "유물 자동 획득" },
            { "Autoget scrap", "고철 자동 획득" },
            { "Autoget books", "책 자동 획득" },
            { "Autoget weightless items", "무게 없는 아이템 자동 획득" },
            { "Autoget if hostiles are nearby", "적이 근처에 있어도 자동 획득" },
            { "Autoget from adjacent cells", "인접 셀에서 자동 획득" }
        };
    }
}
