using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// 날짜 추적 및 일일 리셋 관리
/// </summary>
public class DailyResetManager : SingletonBehaviour<DailyResetManager>
{
    // 이벤트
    public static event Action OnDailyReset;           // 날짜가 바뀌었을 때
    public static event Action OnConsecutiveDayBroken; // 연속 출석 끊겼을 때

    private PlayerDataManager playerDataManager;
    public static bool IsInitialized { get; private set; } = false;

    protected override void Init()
    {
        base.Init();
        // PlayerDataManager 초기화 대기
        StartCoroutine(WaitForPlayerDataManager());
    }

    /// <summary>
    /// PlayerDataManager 초기화를 기다림
    /// </summary>
    private IEnumerator WaitForPlayerDataManager()
{
    // PlayerDataManager.IsDataLoaded가 true가 될 때까지 대기
    while (!PlayerDataManager.IsDataLoaded)
    {
        yield return null;
    }

    playerDataManager = PlayerDataManager.Instance;

    if (playerDataManager == null)
    {
        Logger.LogError("[DailyResetManager] PlayerDataManager를 찾을 수 없습니다!");
        yield break;
    }

    Logger.Log("[DailyResetManager] PlayerDataManager 연결 완료");
    CheckDailyReset();
    
    // ⭐ 이 줄 추가!
    IsInitialized = true;
    Logger.Log("[DailyResetManager] 초기화 완료");
}
    
    /// <summary>
    /// 앱 실행 시 날짜 변경 확인
    /// </summary>
    public void CheckDailyReset()
    {
        string today = GetTodayString();
        string lastLogin = playerDataManager.GetLastLoginDate();
        
        Logger.Log($"[DailyResetManager] 오늘: {today}, 마지막 로그인: {lastLogin}");
        
        // 첫 실행 (lastLogin이 비어있음)
        if (string.IsNullOrEmpty(lastLogin))
        {
            Logger.Log("[DailyResetManager] 첫 실행");
            playerDataManager.SetLastLoginDate(today);
            playerDataManager.SetConsecutiveDays(1);
            return;
        }
        
        // 날짜 비교
        DateTime todayDate = DateTime.Parse(today);
        DateTime lastLoginDate = DateTime.Parse(lastLogin);
        
        int daysDifference = (todayDate - lastLoginDate).Days;
        
        if (daysDifference == 0)
        {
            // 같은 날 (리셋 불필요)
            Logger.Log("[DailyResetManager] 오늘 이미 접속함");
        }
        else if (daysDifference == 1)
        {
            // 하루 지남 (연속 출석 유지)
            Logger.Log("[DailyResetManager] 연속 출석 유지!");
            PerformDailyReset(true);
        }
        else
        {
            // 2일 이상 지남 (연속 출석 끊김)
            Logger.Log($"[DailyResetManager] {daysDifference}일 만에 접속, 연속 출석 초기화");
            PerformDailyReset(false);
            OnConsecutiveDayBroken?.Invoke();
        }
        
        // 마지막 로그인 날짜 갱신
        playerDataManager.SetLastLoginDate(today);
    }
    
    /// <summary>
    /// 일일 리셋 수행
    /// </summary>
    private void PerformDailyReset(bool maintainStreak)
    {
        // 연속 출석일 갱신
        if (maintainStreak)
        {
            int currentStreak = playerDataManager.GetConsecutiveDays();
            
            // 7일 사이클이므로 7일 넘으면 1로 리셋
            if (currentStreak >= 7)
                playerDataManager.SetConsecutiveDays(1);
            else
                playerDataManager.SetConsecutiveDays(currentStreak + 1);
        }
        else
        {
            // 연속 출석 끊김
            playerDataManager.SetConsecutiveDays(1);
        }
        
        // 출석 보상 수령 여부 리셋
        playerDataManager.SetHasTakenDailyReward(false);
        
        // 일일 퀘스트 리셋 (다음 단계에서 구현)
        OnDailyReset?.Invoke();
        
        Logger.Log($"[DailyResetManager] 일일 리셋 완료! 연속 출석: {playerDataManager.GetConsecutiveDays()}일");
    }
    
    /// <summary>
    /// 오늘 날짜 문자열 반환 (YYYY-MM-DD)
    /// </summary>
    public static string GetTodayString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }
    
    /// <summary>
    /// 다음 리셋까지 남은 시간 (초)
    /// </summary>
    public static int GetSecondsUntilMidnight()
    {
        DateTime now = DateTime.Now;
        DateTime midnight = now.Date.AddDays(1); // 다음 날 00:00
        return (int)(midnight - now).TotalSeconds;
    }
    
    /// <summary>
    /// 다음 리셋까지 남은 시간 문자열 (HH:mm:ss)
    /// </summary>
    public static string GetTimeUntilMidnightString()
    {
        int seconds = GetSecondsUntilMidnight();
        int hours = seconds / 3600;
        int minutes = (seconds % 3600) / 60;
        int secs = seconds % 60;
        return $"{hours:D2}:{minutes:D2}:{secs:D2}";
    }

    // 디버그 테스트용
    [ContextMenu("Debug: Simulate Next Day")]
    void DebugSimulateNextDay()
    {
        if (playerDataManager == null)
        {
            Logger.LogError("[DEBUG] PlayerDataManager가 없습니다!");
            return;
        }

        string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        playerDataManager.SetLastLoginDate(yesterday);
        Logger.Log($"[DEBUG] 마지막 로그인을 어제로 설정: {yesterday}");
        Logger.Log("[DEBUG] 앱을 재시작하면 '연속 출석 유지' 메시지가 나옵니다.");
    }

    [ContextMenu("Debug: Break Streak (3 days ago)")]
    void DebugBreakStreak()
    {
        if (playerDataManager == null)
        {
            Logger.LogError("[DEBUG] PlayerDataManager가 없습니다!");
            return;
        }

        string threeDaysAgo = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
        playerDataManager.SetLastLoginDate(threeDaysAgo);
        Logger.Log($"[DEBUG] 마지막 로그인을 3일 전으로 설정: {threeDaysAgo}");
        Logger.Log("[DEBUG] 앱을 재시작하면 '연속 출석 초기화' 메시지가 나옵니다.");
    }
    // 특정 날짜로 설정
    [ContextMenu("Debug: Set Day 1")]
    void DebugSetDay1()
    {
        if (playerDataManager == null) return;

        playerDataManager.SetConsecutiveDays(1);
        playerDataManager.SetLastLoginDate(DailyResetManager.GetTodayString());
        Logger.Log("[DEBUG] 연속 출석 1일로 설정");
    }

    [ContextMenu("Debug: Set Day 7")]
    void DebugSetDay7()
    {
        if (playerDataManager == null) return;

        playerDataManager.SetConsecutiveDays(7);
        playerDataManager.SetLastLoginDate(DailyResetManager.GetTodayString());
        Logger.Log("[DEBUG] 연속 출석 7일로 설정");
    }

    // 일일 퀘스트만 리셋 (연속 출석은 유지)
    [ContextMenu("Debug: Reset Daily Quests Only")]
    void DebugResetDailyQuestsOnly()
    {
        AchievementManager.Instance?.ResetDailyQuests();
        Logger.Log("[DEBUG] 일일 퀘스트만 리셋");
    }
}