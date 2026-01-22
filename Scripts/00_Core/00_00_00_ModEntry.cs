/*
 * 파일명: 00_ModEntry.cs
 * 분류: [Core] 모드 진입점
 * 역할: 모드 로드 시 LocalizationManager를 초기화하고 모든 Harmony 패치를 어셈블리에서 찾아 실행합니다.
 */

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation
{
    public class ModEntry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Main()
        {
            try
            {
                Debug.Log("=================================================");
                Debug.Log("[Qud-KR Translation] 모드 초기화 시작...");
                Debug.Log("[Qud-KR Translation] Version: 0.2.1 (Safe-Patch)");
                Debug.Log("=================================================");
                
                // Harmony 인스턴스 생성
                var harmony = new Harmony("com.boram.qud.translation");

                // 번역 데이터 초기화
                QudKRTranslation.Core.LocalizationManager.Initialize();
                
                // 패치 적용 전 타입 검증 (선택적)
                VerifyPatchTargets();
                
                // 현재 어셈블리의 모든 HarmonyPatch 클래스 찾기
                var assembly = Assembly.GetExecutingAssembly();
                var patchTypes = assembly.GetTypes()
                    .Where(t => t.IsDefined(typeof(HarmonyAttribute), inherit: true))
                    .ToArray();

                Debug.Log($"[Qud-KR Translation] 총 {patchTypes.Length}개의 패치 클래스 발견. 개별 적용 시작...");

                int successCount = 0;
                foreach (var type in patchTypes)
                {
                    try
                    {
                        // 개별 클래스 단위로 패치 적용 (하나가 실패해도 나머지는 진행됨)
                        harmony.CreateClassProcessor(type).Patch();
                        successCount++;
                        Debug.Log($"[Qud-KR Translation] ✓ 패치 성공: {type.Name}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Qud-KR Translation] ❌ 패치 실패: {type.Name}");
                        Debug.LogError($"[Qud-KR Translation] 원인: {ex.GetType().Name} - {ex.Message}");
                    }
                }
                
                Debug.Log("=================================================");
                Debug.Log($"[Qud-KR Translation] 패치 완료: {successCount}/{patchTypes.Length} 성공");
                Debug.Log("[Qud-KR Translation] 모드 로드 완료!");
                Debug.Log("=================================================");
            }
            catch (Exception e)
            {
                Debug.LogError("=================================================");
                Debug.LogError("[Qud-KR Translation] ❌ 치명적 로드 실패!");
                Debug.LogError($"[Qud-KR Translation] 에러: {e.Message}");
                Debug.LogError($"[Qud-KR Translation] 스택 트레이스:\n{e.StackTrace}");
                Debug.LogError("=================================================");
            }
        }
        
        /// <summary>
        /// 패치 대상 타입 및 메서드를 검증합니다. (선택적)
        /// [FIX Issue 13] Document hardcoded type names - these may change with game updates.
        /// When game updates break patches, check these types first.
        /// </summary>
        private static void VerifyPatchTargets()
        {
            // 주요 타겟 타입 존재 여부만 로그로 남김
            // NOTE: These are version-sensitive. Update if game structure changes.
            // Last verified: Caves of Qud 2026.1.x
            string[] criticalTypes = {
                "ConsoleLib.Console.ScreenBuffer",  // Console rendering
                "XRL.UI.UITextSkin",                 // Modern UI text
                "Qud.UI.MainMenu"                   // Main menu (may also be XRL.UI.MainMenu in older versions)
            };

            foreach(var typeName in criticalTypes) {
                if(AccessTools.TypeByName(typeName) == null)
                    Debug.LogWarning($"[Qud-KR Translation] 경고: 핵심 타입 '{typeName}'을 찾을 수 없습니다.");
            }

            // TextConsole 폰트 시스템 조사
            InvestigateTextConsoleFont();
        }

        /// <summary>
        /// TextConsole 폰트 시스템 조사 (한글 지원을 위한 정보 수집)
        /// </summary>
        private static void InvestigateTextConsoleFont()
        {
            try
            {
                // ex 타입 찾기 (ConsoleLib.Console.ex)
                var exType = AccessTools.TypeByName("ConsoleLib.Console.ex");
                if (exType == null)
                {
                    Debug.LogWarning("[Qud-KR][FontInvestigate] ex type not found");
                    return;
                }

                Debug.Log($"[Qud-KR][FontInvestigate] ex type found: {exType.FullName}");

                // 모든 필드 출력 (Font 관련)
                var fields = exType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    string fieldTypeName = field.FieldType.Name;
                    if (field.Name.ToLower().Contains("font") ||
                        fieldTypeName.Contains("Font") ||
                        fieldTypeName.Contains("Sprite") ||
                        fieldTypeName.Contains("Texture"))
                    {
                        Debug.Log($"[Qud-KR][FontInvestigate] ex.{field.Name} : {field.FieldType.FullName}");
                    }
                }

                // 모든 속성 출력 (Font 관련)
                var props = exType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    string propTypeName = prop.PropertyType.Name;
                    if (prop.Name.ToLower().Contains("font") ||
                        propTypeName.Contains("Font") ||
                        propTypeName.Contains("Sprite") ||
                        propTypeName.Contains("Texture"))
                    {
                        Debug.Log($"[Qud-KR][FontInvestigate] ex.{prop.Name} (prop) : {prop.PropertyType.FullName}");
                    }
                }

                // SpriteManager 조사
                var spriteManagerType = AccessTools.TypeByName("ConsoleLib.Console.SpriteManager");
                if (spriteManagerType != null)
                {
                    Debug.Log($"[Qud-KR][FontInvestigate] SpriteManager found: {spriteManagerType.FullName}");
                    var smFields = spriteManagerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    foreach (var field in smFields)
                    {
                        if (field.Name.ToLower().Contains("font") || field.FieldType.Name.Contains("Font"))
                        {
                            Debug.Log($"[Qud-KR][FontInvestigate] SpriteManager.{field.Name} : {field.FieldType.FullName}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Qud-KR][FontInvestigate] Error: {e.Message}");
            }
        }
    }
}
