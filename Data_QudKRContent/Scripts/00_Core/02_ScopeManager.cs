/*
 * 파일명: 02_ScopeManager.cs
 * 분류: [Core] 범위 관리 시스템
 * 역할: Stack 기반으로 현재 활성 번역 범위를 관리합니다.
 *       각 화면이 열릴 때 PushScope, 닫힐 때 PopScope를 호출하여
 *       의도하지 않은 번역을 방지합니다.
 * 작성일: 2026-01-15
 */

using System.Collections.Generic;
using UnityEngine;

namespace QudKRTranslation
{
    /// <summary>
    /// Stack 기반 범위 관리 시스템
    /// 화면이 중첩될 때 (팝업 → 인벤토리 → 메인 메뉴) 각 범위를 올바르게 관리합니다.
    /// </summary>
    public static class ScopeManager
    {
        private static Stack<Dictionary<string, string>[]> scopeStack = new Stack<Dictionary<string, string>[]>();
        
        /// <summary>
        /// 새로운 번역 범위를 추가합니다.
        /// </summary>
        /// <param name="scopes">우선순위 순서대로 딕셔너리 배열 (첫 번째가 최우선)</param>
        public static void PushScope(params Dictionary<string, string>[] scopes)
        {
            if (scopes == null || scopes.Length == 0)
            {
                Debug.LogWarning("[ScopeManager] PushScope called with null or empty scopes");
                return;
            }
            
            scopeStack.Push(scopes);
            Debug.Log($"[ScopeManager] Pushed scope (depth: {scopeStack.Count})");
        }
        
        /// <summary>
        /// 현재 번역 범위를 제거합니다.
        /// </summary>
        public static void PopScope()
        {
            if (scopeStack.Count > 0)
            {
                scopeStack.Pop();
                Debug.Log($"[ScopeManager] Popped scope (depth: {scopeStack.Count})");
            }
            else
            {
                Debug.LogWarning("[ScopeManager] Attempted to pop empty stack!");
            }
        }
        
        /// <summary>
        /// 현재 활성 번역 범위를 반환합니다.
        /// </summary>
        /// <returns>현재 범위의 딕셔너리 배열, 없으면 null</returns>
        public static Dictionary<string, string>[] GetCurrentScope()
        {
            return scopeStack.Count > 0 ? scopeStack.Peek() : null;
        }
        
        /// <summary>
        /// 현재 Stack 깊이를 반환합니다. (디버깅용)
        /// </summary>
        public static int GetDepth()
        {
            return scopeStack.Count;
        }
        
        /// <summary>
        /// 모든 범위를 초기화합니다. (에러 복구용)
        /// </summary>
        public static void ClearAll()
        {
            int count = scopeStack.Count;
            scopeStack.Clear();
            if (count > 0)
            {
                Debug.LogWarning($"[ScopeManager] Cleared {count} scopes (emergency cleanup)");
            }
        }
    }
}
