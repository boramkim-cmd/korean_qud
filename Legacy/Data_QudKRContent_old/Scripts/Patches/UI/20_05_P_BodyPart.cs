/*
 * 파일명: 20_05_P_BodyPart.cs
 * 분류: [UI] 신체 부위 및 장비 슬롯 이름 번역
 * 역할: "Left Hand" -> "좌측 손", "Missile Weapon" -> "원거리" 등 신체 부위 이름을 한국어로 번역하고, UI 겹침 방지를 위해 간소화합니다.
 * 수정일: 2026-01-14 (Fix Critical Syntax Errors)
 */

using HarmonyLib;
using XRL.World.Anatomy;
using System.Collections.Generic;
using System.Text;

namespace QudKRContent
{
    [HarmonyPatch(typeof(BodyPart))]
    public static class Patch_BodyPart
    {
        // 신체 부위 이름 매핑 (Description -> Korean)
        static Dictionary<string, string> BodyPartNames = new Dictionary<string, string>
        {
            { "Hand", "손" },
            { "Feet", "발" },
            { "Head", "머리" },
            { "Body", "몸통" },
            { "Arm", "팔" },
            { "Face", "얼굴" },
            { "Back", "등" },
            { "Floating Nearby", "부유" }, // "주변 부유" -> "부유" (공간 절약)
            { "Missile Weapon", "원거리" }, // "원거리 무기" -> "원거리"
            { "Thrown Weapon", "투척" },    // "투척 무기" -> "투척"
            { "Light Source", "조명" },    // "광원" -> "조명"
            { "Hands", "손" }, // Worn on Hands -> Hands
            { "Finger", "손가락" },
            { "Roots", "뿌리" },
            { "Tail", "꼬리" },
            { "Fin", "지느러미" },
            { "Tread", "궤도" }, // Tank treads
            { "Wheel", "바퀴" }
        };

        // 방향 접두사 매핑 (공간 절약을 위해 2글자 선호)
        static Dictionary<string, string> CardinalPrefixes = new Dictionary<string, string>
        {
            { "Left", "좌측" },    // 왼쪽 -> 좌측
            { "Right", "우측" },   // 오른쪽 -> 우측
            { "Upper", "상단" },   // 위쪽 -> 상단
            { "Lower", "하단" },   // 아래쪽 -> 하단
            { "Middle", "중단" },  // 가운데 -> 중단
            { "Front", "전면" },
            { "Rear", "후면" },
            { "Dorsal", "등쪽" },
            { "Ventral", "배쪽" },
            { "Anterior", "앞쪽" },
            { "Posterior", "뒤쪽" },
            { "Medial", "안쪽" },
            { "Lateral", "바깥쪽" },
            { "Proximal", "근위" },
            { "Distal", "원위" }
        };

        [HarmonyPatch("GetCardinalDescription")]
        [HarmonyPostfix]
        static void GetCardinalDescription_Postfix(BodyPart __instance, ref string __result)
        {
            if (string.IsNullOrEmpty(__result)) return;

            // 이미 번역된 경우 패스 (무한 루프 방지)
            if (__result.Contains("좌측") || __result.Contains("우측") || __result.Contains("상단")) return;

            // 1. 방향 접두사 처리 (String Match & Replace)
            // GetCardinalDescription은 "Left Hand" 형태의 문자열을 반환함.
            // 우리는 이를 "좌측 손"으로 바꾸고 싶음.

            StringBuilder sb = new StringBuilder(__result);

            // 방향 교체
            foreach (var kvp in CardinalPrefixes)
            {
                if (__result.StartsWith(kvp.Key + " "))
                {
                    sb.Replace(kvp.Key + " ", kvp.Value + " ");
                    break; // 하나만 매칭되면 종료
                }
            }

            // 부위 이름 교체
            // sb는 현재 "좌측 Hand" 또는 "Left Hand" (방향 매칭 실패 시) 상태일 수 있음.
            
            // "Hands"가 "Hand"보다 먼저 처리되도록 길이에 따라 정렬하거나 명시적으로 처리
            // Dictionary 순서가 보장되지 않으므로, 키 길이 역순으로 정렬하여 처리합니다.
            var sortedKeys = new List<string>(BodyPartNames.Keys);
            sortedKeys.Sort((a, b) => b.Length.CompareTo(a.Length)); // 긴 단어 먼저

            foreach (string key in sortedKeys)
            {
                if (sb.ToString().Contains(key))
                {
                    sb.Replace(key, BodyPartNames[key]);
                }
            }
            
            // Worn on Hands 예외 처리
            sb.Replace("Worn on ", ""); 
            // "Worn on Hands" -> "Hands" -> "손" (위 루프에서 처리됨)

            __result = sb.ToString();
        }
    }
}
