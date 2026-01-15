/*
 * 파일명: 00_04_TranslationDB_Controls.cs
 * 분류: [System] 번역 데이터베이스 - 조작
 * 역할: 조작 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // Controls 카테고리
        public static Dictionary<string, string> Options_Controls = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "Controls", "조작" },
            { "CONTROLS", "조작" },
            { "<color=#77BFCFFF>CONTROLS</color>", "<color=#77BFCFFF>조작</color>" },
            
            // 키 설정
            { "Control Mapping", "키 매핑" },
            { "Key repeat delay", "키 반복 지연" },
            { "Key repeat rate", "키 반복 속도" },
            
            // 마우스 설정
            { "Allow mouse input", "마우스 입력 허용" },
            { "Allow mouse movement", "마우스 이동 허용" },
            { "Allow scroll wheel to zoom", "스크롤 휠 줌 허용" },
            
            // 기타
            { "Limit the input buffer to two commands", "입력 버퍼를 2개 명령으로 제한" }
        };
    }
}
