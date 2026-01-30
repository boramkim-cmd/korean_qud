/*
 * 파일명: 02_10_16_InventoryCategoryPatch.cs
 * 분류: [UI Patch] 인벤토리/상점 카테고리 헤더 번역
 * 역할: GameObject.GetInventoryCategory() 반환값을 한글 카테고리로 변환합니다.
 *       필터 비교도 동일 메서드를 사용하므로 양쪽 모두 번역되어 일관성 유지됩니다.
 */

using System.Collections.Generic;
using HarmonyLib;
using XRL.World;
using QudKRTranslation.Core;

namespace QudKRTranslation.Patches.UI
{
    [HarmonyPatch(typeof(GameObject), nameof(GameObject.GetInventoryCategory))]
    public static class Patch_InventoryCategory
    {
        private static Dictionary<string, string> _categoryDict;

        [HarmonyPostfix]
        static void Postfix(ref string __result)
        {
            if (string.IsNullOrEmpty(__result)) return;

            if (_categoryDict == null)
            {
                _categoryDict = LocalizationManager.GetCategory("categories");
                if (_categoryDict == null) return;
            }

            if (_categoryDict.TryGetValue(__result, out var translated))
            {
                __result = translated;
            }
        }
    }
}
