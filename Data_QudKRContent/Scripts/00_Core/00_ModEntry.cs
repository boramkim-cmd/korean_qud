/*
 * 파일명: 00_ModEntry.cs
 * 분류: [Core] 시스템 진입점
 * 역할: Harmony 라이브러리를 초기화하고 모든 패치를 적용합니다.
 *       에러 핸들링 및 로깅을 담당합니다.
 * 작성일: 2026-01-15
 */

using System;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation
{
    public class ModEntry
    {
        public static void Main()
        {
            try
            {
                Debug.Log("=================================================");
                Debug.Log("[Qud-KR Translation] 모드 초기화 시작...");
                Debug.Log("[Qud-KR Translation] Version: 0.2.0");
                Debug.Log("=================================================");
                
                // Harmony 인스턴스 생성
                var harmony = new Harmony("com.boram.qud.translation");
                
                // 패치 적용 전 타입 검증 (선택적)
                VerifyPatchTargets();
                
                // 모든 Harmony 패치 적용
                Debug.Log("[Qud-KR Translation] Harmony PatchAll 실행 중...");
                harmony.PatchAll();
                
                // 적용된 패치 확인
                var patches = harmony.GetPatchedMethods();
                int count = 0;
                foreach (var method in patches)
                {
                    count++;
                    Debug.Log($"[Qud-KR Translation] ✓ 패치됨: {method.DeclaringType?.Name}.{method.Name}");
                }
                
                Debug.Log("=================================================");
                Debug.Log($"[Qud-KR Translation] 총 {count}개 메서드 패치 완료");
                Debug.Log("[Qud-KR Translation] 모드 로드 완료!");
                Debug.Log("=================================================");
            }
            catch (Exception e)
            {
                Debug.LogError("=================================================");
                Debug.LogError("[Qud-KR Translation] ❌ 로드 실패!");
                Debug.LogError($"[Qud-KR Translation] 에러: {e.Message}");
                Debug.LogError($"[Qud-KR Translation] 스택 트레이스:\n{e.StackTrace}");
                Debug.LogError("=================================================");
            }
        }
        
        /// <summary>
        /// 패치 대상 타입 및 메서드를 검증합니다. (선택적)
        /// </summary>
        private static void VerifyPatchTargets()
        {
            Debug.Log("[Qud-KR Translation] 패치 대상 검증 중...");
            
            // 주요 타입 확인
            InspectType("XRL.UI.ScreenBuffer");
            InspectType("Qud.UI.InventoryScreen");
            InspectType("Qud.UI.TradeScreen");
            InspectType("Qud.UI.CharacterStatusScreen");
            InspectType("XRL.UI.Popup");
            
            Debug.Log("[Qud-KR Translation] 패치 대상 검증 완료");
        }
        
        /// <summary>
        /// 타입의 존재 여부와 주요 메서드를 확인합니다.
        /// </summary>
        private static void InspectType(string typeName)
        {
            var type = AccessTools.TypeByName(typeName);
            if (type != null)
            {
                Debug.Log($"[Qud-KR Translation]   ✓ {typeName} 발견");
                
                // 주요 메서드 나열 (최대 5개)
                var methods = type.GetMethods(
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.DeclaredOnly
                );
                
                if (methods.Length > 0)
                {
                    int maxCount = methods.Length > 5 ? 5 : methods.Length;
                    for (int i = 0; i < maxCount; i++)
                    {
                        var paramTypes = methods[i].GetParameters();
                        string paramStr = paramTypes.Length > 0 
                            ? string.Join(", ", Array.ConvertAll(paramTypes, p => p.ParameterType.Name))
                            : "";
                        Debug.Log($"[Qud-KR Translation]     - {methods[i].Name}({paramStr})");
                    }
                    if (methods.Length > 5)
                    {
                        Debug.Log($"[Qud-KR Translation]     ... 외 {methods.Length - 5}개");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[Qud-KR Translation]   ✗ {typeName} 찾을 수 없음");
            }
        }
    }
}
