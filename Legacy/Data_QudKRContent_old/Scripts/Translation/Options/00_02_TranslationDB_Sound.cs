/*
 * 파일명: 00_02_TranslationDB_Sound.cs
 * 분류: [System] 번역 데이터베이스 - 사운드
 * 역할: 사운드 관련 설정 옵션 번역
 */

using System.Collections.Generic;

namespace QudKRContent
{
    public static partial class DictDB
    {
        // Sound 카테고리
        public static Dictionary<string, string> Options_Sound = new Dictionary<string, string>()
        {
            // 메인 카테고리 (모든 변형 포함)
            { "Sound", "사운드" },
            { "SOUND", "사운드" },
            { "<color=#77BFCFFF>SOUND</color>", "<color=#77BFCFFF>사운드</color>" },
            { "<color=#77BFCFFF>[-]</color>", "<color=#77BFCFFF>[-]</color>" },
            { "<color=#77BFCFFF>[+]</color>", "<color=#77BFCFFF>[+]</color>" },
            
            // 볼륨 설정
            { "Main volume", "메인 볼륨" },
            { "Sound effects", "효과음" },
            { "Sound effects volume", "효과음 볼륨" },
            { "Music", "음악" },
            { "Music volume", "음악 볼륨" },
            { "Ambient sounds", "환경음" },
            { "Ambient volume", "환경음 볼륨" },
            { "Interface sounds", "인터페이스 사운드" },
            { "Interface volume", "인터페이스 볼륨" },
            { "Combat sounds", "전투 사운드" },
            { "Combat volume", "전투 볼륨" },
            { "Fire crackling sounds", "불 타는 소리" }
        };
    }
}
