/*
 * 파일명: 00_08_TranslationDB_Mods.cs
 * 분류: [System] 번역 데이터베이스 - 모드
 * 역할: 모드 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // Mods 카테고리
        public static Dictionary<string, string> Options_Mods = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "Mods", "모드" },
            { "MODS", "모드" },
            { "<color=#77BFCFFF>MODS</color>", "<color=#77BFCFFF>모드</color>" },
            
            // 모드 설정
            { "Enable mods", "모드 활성화" },
            { "Allow scripting mods (scripting mods may contain malicious code!)", "스크립팅 모드 허용 (악성 코드 포함 가능!)" },
            { "Write compiled mod assemblies to disk", "컴파일된 모드 어셈블리를 디스크에 저장" },
            { "Enable Harmony debug output", "Harmony 디버그 출력 활성화" }
        };
    }
}
