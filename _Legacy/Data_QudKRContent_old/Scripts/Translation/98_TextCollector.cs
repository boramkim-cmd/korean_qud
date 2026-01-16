/*
 * 파일명: 98_TextCollector.cs
 * 분류: [Tool] 텍스트 수집기
 * 역할: 게임 내 모든 UI 텍스트를 수집하여 번역 대상 목록을 생성합니다.
 * 수정일: 2026-01-14
 */

using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace QudKRContent
{
    public static class TextCollector
    {
        private static HashSet<string> collectedTexts = new HashSet<string>();
        private static string outputPath = "/Users/ben/Desktop/qud_ui_texts.txt";
        private static bool isCollecting = true;
        
        public static void CollectFromComponent(Component component, string screenName)
        {
            if (!isCollecting || component == null) return;
            
            try
            {
                var texts = component.GetComponentsInChildren<TMP_Text>(true);
                int newCount = 0;
                
                foreach (var t in texts)
                {
                    if (!string.IsNullOrEmpty(t.text))
                    {
                        string text = t.text.Trim();
                        
                        // 빈 문자열, 숫자만, 너무 긴 텍스트 제외
                        if (text.Length == 0 || text.Length > 100) continue;
                        if (int.TryParse(text, out _)) continue;
                        
                        // 새로운 텍스트만 추가
                        if (collectedTexts.Add($"{screenName}|{text}"))
                        {
                            newCount++;
                        }
                    }
                }
                
                if (newCount > 0)
                {
                    Debug.Log($"[TextCollector] {screenName}: {newCount}개 새 텍스트 발견 (총 {collectedTexts.Count}개)");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TextCollector] 오류: {e.Message}");
            }
        }
        
        public static void SaveToFile()
        {
            if (collectedTexts.Count == 0)
            {
                Debug.Log("[TextCollector] 수집된 텍스트가 없습니다.");
                return;
            }
            
            try
            {
                // 화면별로 그룹화
                var grouped = new Dictionary<string, List<string>>();
                
                foreach (var entry in collectedTexts)
                {
                    string[] parts = entry.Split('|');
                    if (parts.Length != 2) continue;
                    
                    string screen = parts[0];
                    string text = parts[1];
                    
                    if (!grouped.ContainsKey(screen))
                    {
                        grouped[screen] = new List<string>();
                    }
                    grouped[screen].Add(text);
                }
                
                // 파일에 저장
                using (StreamWriter writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("===========================================");
                    writer.WriteLine("Caves of Qud - UI 텍스트 수집 결과");
                    writer.WriteLine($"수집 시간: {DateTime.Now}");
                    writer.WriteLine($"총 {collectedTexts.Count}개 텍스트");
                    writer.WriteLine("===========================================");
                    writer.WriteLine();
                    
                    foreach (var screen in grouped.Keys)
                    {
                        writer.WriteLine($"### {screen} ({grouped[screen].Count}개)");
                        writer.WriteLine();
                        
                        foreach (var text in grouped[screen])
                        {
                            writer.WriteLine($"  - {text}");
                        }
                        
                        writer.WriteLine();
                    }
                }
                
                Debug.Log($"[TextCollector] {outputPath}에 {collectedTexts.Count}개 텍스트 저장 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TextCollector] 파일 저장 실패: {e.Message}");
            }
        }
        
        public static void StopCollecting()
        {
            isCollecting = false;
            SaveToFile();
        }
    }
}
