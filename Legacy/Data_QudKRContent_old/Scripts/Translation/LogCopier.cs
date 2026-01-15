/*
 * 파일명: LogCopier.cs
 * 분류: [Tool] 로그 복사 도구
 * 역할: 게임 종료 시 Player.log를 모드 폴더에 자동 복사 (강제 종료 대응)
 */

using System;
using System.IO;
using UnityEngine;

namespace QudKRContent
{
    public class LogCopier : MonoBehaviour
    {
        private static string logSourcePath = "/Users/ben/Library/Logs/Freehold Games/CavesOfQud/Player.log";
        private static string logDestFolder = "/Users/ben/Library/Application Support/com.FreeholdGames.CavesOfQud/Mods/Data_QudKRContent/Logs";
        private static bool initialized = false;
        private static float lastBackupTime = 0f;
        private static float backupInterval = 300f; // 5분마다 자동 백업
        
        public static void Initialize()
        {
            if (initialized) return;
            
            try
            {
                // Logs 폴더 생성
                if (!Directory.Exists(logDestFolder))
                {
                    Directory.CreateDirectory(logDestFolder);
                    Debug.Log($"[LogCopier] Logs 폴더 생성: {logDestFolder}");
                }
                
                // GameObject 생성하여 Update 사용
                GameObject logCopierObj = new GameObject("LogCopier");
                logCopierObj.AddComponent<LogCopierBehaviour>();
                UnityEngine.Object.DontDestroyOnLoad(logCopierObj);
                
                // 게임 종료 시 로그 복사
                Application.quitting += OnApplicationQuit;
                
                // 초기 백업
                CopyLog("startup");
                
                initialized = true;
                Debug.Log("[LogCopier] 로그 자동 복사 시스템 초기화 완료 (5분마다 자동 백업)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LogCopier] 초기화 실패: {e.Message}");
            }
        }
        
        // MonoBehaviour를 위한 별도 클래스
        private class LogCopierBehaviour : MonoBehaviour
        {
            void Update()
            {
                // 5분마다 자동 백업
                if (Time.time - lastBackupTime > backupInterval)
                {
                    lastBackupTime = Time.time;
                    CopyLog("auto");
                }
            }
            
            void OnApplicationFocus(bool hasFocus)
            {
                // 포커스를 잃을 때 백업 (강제 종료 대비)
                if (!hasFocus)
                {
                    CopyLog("focus_lost");
                }
            }
            
            void OnDestroy()
            {
                // GameObject가 파괴될 때 백업
                CopyLog("destroy");
            }
        }
        
        private static void OnApplicationQuit()
        {
            CopyLog("quit");
        }
        
        private static void CopyLog(string reason)
        {
            try
            {
                // 타임스탬프 생성
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string destPath = Path.Combine(logDestFolder, $"Player_{timestamp}_{reason}.log");
                
                // 로그 파일 복사
                if (File.Exists(logSourcePath))
                {
                    File.Copy(logSourcePath, destPath, true);
                    
                    // quit일 때만 로그 출력 (너무 많은 로그 방지)
                    if (reason == "quit" || reason == "startup")
                    {
                        Debug.Log($"[LogCopier] 로그 복사 완료 ({reason}): {Path.GetFileName(destPath)}");
                    }
                    
                    // 오래된 로그 정리
                    if (reason == "quit" || reason == "auto")
                    {
                        CleanOldLogs();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LogCopier] 로그 복사 실패 ({reason}): {e.Message}");
            }
        }
        
        private static void CleanOldLogs()
        {
            try
            {
                var logFiles = Directory.GetFiles(logDestFolder, "Player_*.log");
                
                if (logFiles.Length > 20)
                {
                    // 파일을 날짜순으로 정렬
                    Array.Sort(logFiles);
                    
                    // 오래된 파일 삭제 (최근 20개만 유지)
                    int deleteCount = logFiles.Length - 20;
                    for (int i = 0; i < deleteCount; i++)
                    {
                        File.Delete(logFiles[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LogCopier] 로그 정리 실패: {e.Message}");
            }
        }
        
        // 수동으로 로그 복사 (디버깅용)
        public static void CopyLogNow()
        {
            CopyLog("manual");
            Debug.Log("[LogCopier] 수동 로그 복사 완료");
        }
    }
}
