/*
 * 파일명: 00_00_ModEntry.cs
 * 분류: [Core] 시스템 진입점
 * 역할: Harmony 라이브러리를 초기화하고 분할된 모든 패치 파일을 실행합니다.
 * 수정일: 2026-01-14
 */

using System;
using HarmonyLib;
using UnityEngine;

namespace QudKRContent
{
    public class ModEntry
    {
        public static void Main()
        {
            try
            {
                Debug.Log("[Qud-KR] 모드 초기화 시작...");
                
                // 로그 자동 복사 시스템 초기화
                LogCopier.Initialize();
                
                // 시스템 기능 테스트 (한 번만 실행)
                SystemCapabilityTest.RunTests();
                
                var harmony = new Harmony("com.boram.qud.content");
                
                // 패치 적용 전 타입 및 메서드 확인
                Debug.Log("[Qud-KR] 타입 및 메서드 확인 중...");
                InspectType("Qud.UI.OptionsScreen");
                InspectType("Qud.UI.InventoryAndEquipmentStatusScreen");
                InspectType("Qud.UI.TradeScreen");
                InspectType("Qud.UI.CharacterStatusScreen");
                
                Debug.Log("[Qud-KR] Harmony PatchAll 실행 중...");
                harmony.PatchAll();
                
                // 적용된 패치 확인
                var patches = harmony.GetPatchedMethods();
                int count = 0;
                foreach (var method in patches)
                {
                    count++;
                    Debug.Log($"[Qud-KR] 패치됨: {method.DeclaringType?.Name}.{method.Name}");
                }
                Debug.Log($"[Qud-KR] 총 {count}개 메서드 패치 완료");
                
                // 메인 메뉴 데이터를 직접 번역
                Patch_MainMenu.OverwriteMenuData();
                
                Debug.Log("[Qud-KR] 한글화 모드 로드 완료 (System: 00_XX / Menu: 10_XX / Play: 20_XX)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Qud-KR] 로드 실패: {e.ToString()}");
            }
        }
        
        private static void InspectType(string typeName)
        {
            var type = AccessTools.TypeByName(typeName);
            if (type != null)
            {
                Debug.Log($"[Qud-KR] ✓ {typeName} 발견");
                
                // 모든 public 메서드 나열
                var methods = type.GetMethods(System.Reflection.BindingFlags.Public | 
                                             System.Reflection.BindingFlags.Instance | 
                                             System.Reflection.BindingFlags.DeclaredOnly);
                
                if (methods.Length > 0)
                {
                    // LINQ 없이 메서드 이름 목록 생성
                    var methodNames = new System.Collections.Generic.List<string>();
                    int maxCount = methods.Length > 10 ? 10 : methods.Length;
                    for (int i = 0; i < maxCount; i++)
                    {
                        methodNames.Add(methods[i].Name);
                    }
                    Debug.Log($"[Qud-KR]   사용 가능한 메서드: {string.Join(", ", methodNames.ToArray())}");
                }
                else
                {
                    Debug.Log($"[Qud-KR]   public 인스턴스 메서드 없음");
                }
            }
            else
            {
                Debug.LogWarning($"[Qud-KR] ✗ {typeName} 찾을 수 없음");
            }
        }
    }
}