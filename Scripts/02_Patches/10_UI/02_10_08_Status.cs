/*
 * 파일명: 10_08_P_Status.cs
 * 분류: [UI Patch] 상태창(StatusScreensScreen) 번역 스코프 관리
 * 역할: 상태창(인벤토리, 장비, 캐릭터 시트 등 포함)이 열릴 때 
 *       관련 번역 스코프(status, inventory)를 활성화합니다.
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using Qud.UI;
using QudKRTranslation.Core;
using UnityEngine;
using QudKRTranslation;

namespace QudKRTranslation.Patches.UI
{
    [HarmonyPatch(typeof(StatusScreensScreen))]
    public static class Patch_StatusScreensScreen_Scope
    {
        private static bool _scopePushed = false;

        // 화면이 열리는 진입점 (Static show 메서드)
        [HarmonyPatch("show")]
        [HarmonyPrefix]
        static void Show_Prefix()
        {
            PushStatusScope();
        }

        // 화면이 닫히는 지점 (Instance Exit 메서드)
        [HarmonyPatch("Exit")]
        [HarmonyPostfix]
        static void Exit_Postfix()
        {
            PopStatusScope();
        }
        
        // 안전 장치: 예기치 않게 닫혔을 때를 대비해 Cleanup 등에도 훅을 걸 수 있음
        // 하지만 StatusScreensScreen은 Singleton이므로 Exit이 주로 불림.

        static void PushStatusScope()
        {
            if (_scopePushed) return; // 이미 푸시됨

            var statusDict = LocalizationManager.GetCategory("status");
            var inventoryDict = LocalizationManager.GetCategory("inventory");
            
            // 유효한 딕셔너리만 골라서 Push
            var dictsToPush = new List<Dictionary<string, string>>();
            if (statusDict != null) dictsToPush.Add(statusDict);
            if (inventoryDict != null) dictsToPush.Add(inventoryDict);

            if (dictsToPush.Count > 0)
            {
                ScopeManager.PushScope(dictsToPush.ToArray());
                _scopePushed = true;
            }
        }

        static void PopStatusScope()
        {
            if (_scopePushed)
            {
                ScopeManager.PopScope();
                _scopePushed = false;
            }
        }
    }
}
