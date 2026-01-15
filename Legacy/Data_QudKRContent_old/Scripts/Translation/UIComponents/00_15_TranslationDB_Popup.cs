/*
 * 파일명: 00_15_TranslationDB_Popup.cs
 * 분류: [System] 번역 데이터베이스 - 팝업
 * 역할: 알림, 질문, 선택지 등의 팝업 텍스트 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        public static Dictionary<string, string> Popup = new Dictionary<string, string>()
        {
            { "Yes", "예" }, { "No", "아니오" },
            { "OK", "확인" }, { "Cancel", "취소" },
            { "Accept", "수락" }, { "Ignore", "무시" },
            { "Keep", "유지" }, { "Delete", "삭제" },
            { "Confirm", "확인" }, { "Close", "닫기" },
            
            { "Continue", "계속하기" },
            { "Dismiss", "닫기" },

            { "Are you sure?", "정말입니까?" },
            { "Are you sure you want to quit?", "정말 게임을 종료하시겠습니까?" },
            
            // UI 조작
            { "<up for more...>", "<위로 더 보기...>" },
            { "<down for more...>", "<아래로 더 보기...>" },
            { "Thinking...", "생각 중..." },

            // Main Menu / ESC Menu options
            { "Control Mapping", "키 설정" },
            { "Key Mapping", "키 설정" },
            { "Game Info", "게임 정보" },
            { "Save and Quit", "저장하고 종료" },
            { "Abandon Character", "캐릭터 포기" },
            { "Quit", "종료" },
            { "Resume", "계속하기" },

            // Journal Popups
            { "Entry text", "내용 입력" },
            { "Are you sure you want to delete this entry?", "이 항목을 삭제하시겠습니까?" },
            { "You can't delete automatically recorded chronology entries.", "자동으로 기록된 연대기는 삭제할 수 없습니다." },
            { "Enter a new name for ", "새로운 이름 입력: " },
            { "Enter a name for this location.", "이 장소의 이름 입력:" },
            { "You stop calling this location '", "이 장소를 '" },
            { "You start calling this location '", "이 장소를 '" },

            // Popup Button Prompts (Exact formatting)
            { "{{W|[Esc]}} {{y|Cancel}}", "{{W|[Esc]}} {{y|취소}}" },
            { "{{W|[C]}} {{y|Copy}}", "{{W|[C]}} {{y|복사}}" },
            { "{{W|[L]}} {{y|Look}}", "{{W|[L]}} {{y|보기}}" },
            { "{{W|[space]}} {{y|Continue}}", "{{W|[스페이스]}} {{y|계속}}" },
            { "{{W|[y]}} {{y|Yes}}", "{{W|[y]}} {{y|예}}" },
            { "{{W|[n]}} {{y|No}}", "{{W|[n]}} {{y|아니오}}" },
            { "{{y|Yes}}", "{{y|예}}" },
            { "{{y|No}}", "{{y|아니오}}" },
            { "{{y|Accept}}", "{{y|수락}}" },
            { "{{W|[Enter]}} {{y|Accept}}", "{{W|[Enter]}} {{y|수락}}" },
            { "{{y|Submit}}", "{{y|제출}}" },
            { "{{W|[F1]}} {{y|Color}}", "{{W|[F1]}} {{y|색상}}" },
            { "{{y|Hold to Accept}}", "{{y|길게 눌러 수락}}" },
            
            // Core System Popups (XRLCore)
            { "Are you sure you want to delete this?\n\n", "정말 삭제하시겠습니까?\n\n" },
            { "Your health has dropped below", "체력이 다음 아래로 떨어졌습니다" },
            { "You can only set your checkpoint in settlements.", "체크포인트는 정착지에서만 설정할 수 있습니다." },
            { "You can only restore your checkpoint outside settlements.", "체크포인트 복구는 정착지 밖에서만 가능합니다." },
            { "Are you sure you want to restore your checkpoint?", "체크포인트를 복구하시겠습니까?" },
            { "This saved game predates world seed info.", "이 저장된 게임은 월드 시드 정보가 없습니다." },
            { "You cannot do that on the world map.", "월드맵에서는 할 수 없습니다." },
            { "You haven't found any points of interest nearby.", "근처에서 흥미로운 지점을 찾지 못했습니다." },
            { "There are hostiles nearby!", "근처에 적이 있습니다!" },
            { "Are you sure you want to go to the world map?", "정말 월드맵으로 이동하시겠습니까?" },
            { "Would you like to walk to the nearest stairway up?", "가장 가까운 위쪽 계단으로 이동하시겠습니까?" },
            { "Would you like to walk to the nearest stairway down?", "가장 가까운 아래쪽 계단으로 이동하시겠습니까?" },
            { "You cannot examine things while you are enraged.", "격분 상태에서는 조사할 수 없습니다." },
            { "You cannot examine things while you are confused.", "혼란 상태에서는 조사할 수 없습니다." },
            { "Are you sure you want to save and quit?", "정말 저장하고 종료하시겠습니까?" }
        };
    }
}
