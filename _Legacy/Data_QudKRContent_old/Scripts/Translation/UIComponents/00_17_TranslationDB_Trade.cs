/*
 * 파일명: 00_17_TranslationDB_Trade.cs
 * 분류: [System] 번역 데이터베이스 - 거래
 * 역할: 거래창의 UI 텍스트 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        public static Dictionary<string, string> Trade = new Dictionary<string, string>()
        {
            { "Inventory", "인벤토리" }, { "Trade", "거래" },
            { "Offer", "제안" }, { "Checkout", "계산하기" },
            { "Weight", "무게" }, { "Value", "가치" },
            { "Category", "분류" }, { "Cost", "비용" },
            
            { "Buy", "구매" }, { "Sell", "판매" },


            // Trade Menu Options
            { "Filter", "필터" },
            { "Toggle Sort", "정렬 전환" },
            { "toggle sort", "정렬 전환" },
            { "offer", "제안" },
            { "Add One", "하나 추가" },
            { "add one", "하나 추가" },
            { "Remove One", "하나 제거" },
            { "remove one", "하나 제거" },
            { "Toggle All", "모두 전환" },
            { "toggle all", "모두 전환" },
            { "Vendor Actions", "상인 행동" },
            { "vendor actions", "상인 행동" },
            { "Close Menu", "메뉴 닫기" }, // Override or Ensure

            // Trade UI Strings
            { "trade", "거래" },
            { "transfer", "이동" },
            { "sort: ", "정렬: " },
            { "a-z", "가나다" },
            { "by class", "분류별" },
            { "Add how many ", "몇 개를 추가합니까 " },
            { " to trade.", " 거래에 추가." },
            
            // Currency & Weight
            { "drams", "드램" },
            { "lbs.", "파운드" },
            { "Your Offer", "당신의 제안" }, { "Their Offer", "상대의 제안" },
            { "Total", "합계" }, { "Balance", "잔액" },
            { "Not enough currency", "화폐 부족" },
            { "Deal", "거래 완료" }, { "No Deal", "거래 취소" },

            // 거래 UI 고유
            { "drams →", "드람 →" },
            { "← drams", "← 드람" },
            { "They have no more money.", "그들은 더 이상 돈이 없습니다." },
            { "You have no more money.", "당신의 자금이 부족합니다." }
        };
    }
}
