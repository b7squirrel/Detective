using System;
using System.Collections;
using UnityEngine;

public class WeeklyResetManager : SingletonBehaviour<WeeklyResetManager>
{
    public static event Action OnWeeklyReset;
    public static event Action OnWeeklyResetWithUnclaimed;

    private PlayerDataManager playerDataManager;
    public static bool IsInitialized { get; private set; } = false;

    protected override void Init()
    {
        base.Init();
        StartCoroutine(WaitForPlayerDataManager());
    }

    private IEnumerator WaitForPlayerDataManager()
    {
        while (!PlayerDataManager.IsDataLoaded)
            yield return null;

        playerDataManager = PlayerDataManager.Instance;

        // ⭐ CheckWeeklyReset은 여기서 호출하지 않음
        // GameInitializer에서 AchievementManager 초기화 후 호출

        IsInitialized = true;
        Logger.Log("[WeeklyResetManager] 초기화 완료");
    }

    public void CheckWeeklyReset()
    {
        string currentWeek = GetCurrentWeekString();
        string lastWeek = PlayerPrefs.GetString("WEEKLY_LAST_RESET", "");

        Logger.Log($"[WeeklyResetManager] 현재 주: {currentWeek}");
        Logger.Log($"[WeeklyResetManager] 마지막 리셋: {lastWeek}");
        Logger.Log($"[WeeklyResetManager] 주가 다른가: {currentWeek != lastWeek}");

        if (string.IsNullOrEmpty(lastWeek))
        {
            Logger.Log("[WeeklyResetManager] 첫 실행 - 리셋 없음");
            PlayerPrefs.SetString("WEEKLY_LAST_RESET", currentWeek);
            PlayerPrefs.Save();
            return;
        }

        if (currentWeek != lastWeek)
        {
            Logger.Log("[WeeklyResetManager] 주 변경 감지!");

            var unclaimed = AchievementManager.Instance?.GetUnclaimedCompletedWeeklyQuests();
            Logger.Log($"[WeeklyResetManager] 미수령 완료 업적 수: {unclaimed?.Count ?? -1}");

            if (unclaimed != null && unclaimed.Count > 0)
            {
                foreach (var ra in unclaimed)
                {
                    Logger.Log($"[WeeklyResetManager] 미수령 업적: {ra.original.id}, 완료: {ra.isCompleted}, 수령: {ra.isRewarded}");
                }
                Logger.Log("[WeeklyResetManager] OnWeeklyResetWithUnclaimed 이벤트 발동!");
                OnWeeklyResetWithUnclaimed?.Invoke();

                // ⭐ 추가: 비활성화된 팝업도 직접 찾아서 호출
                var popup = FindObjectOfType<WeeklyRewardPopup>(true);
                if (popup != null)
                    popup.Show();
                else
                    Logger.LogError("[WeeklyResetManager] WeeklyRewardPopup을 찾을 수 없습니다!");
            }
            else
            {
                Logger.Log("[WeeklyResetManager] 미수령 없음 - 바로 리셋");
                PerformWeeklyReset();
            }
        }
        else
        {
            Logger.Log("[WeeklyResetManager] 같은 주 - 리셋 없음");
        }
    }

    public void PerformWeeklyReset()
    {
        PlayerPrefs.SetString("WEEKLY_LAST_RESET", GetCurrentWeekString());
        PlayerPrefs.Save();

        OnWeeklyReset?.Invoke();
        Logger.Log("[WeeklyResetManager] 주간 리셋 완료");
    }

    public static string GetCurrentWeekString()
    {
        DateTime now = DateTime.Now;
        int week = GetIso8601WeekOfYear(now);
        return $"{now.Year}-W{week:D2}";
    }

    private static int GetIso8601WeekOfYear(DateTime date)
    {
        System.Globalization.CultureInfo ci =
            System.Globalization.CultureInfo.InvariantCulture;
        return ci.Calendar.GetWeekOfYear(
            date,
            System.Globalization.CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);
    }

    public static string GetTimeUntilNextMondayString()
    {
        int seconds = GetSecondsUntilNextMonday();
        int days = seconds / 86400;
        int hours = (seconds % 86400) / 3600;
        int minutes = (seconds % 3600) / 60;
        return $"{days}일 {hours:D2}:{minutes:D2}";
    }

    public static int GetSecondsUntilNextMonday()
    {
        DateTime now = DateTime.Now;
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        DateTime nextMonday = now.Date.AddDays(daysUntilMonday);
        return (int)(nextMonday - now).TotalSeconds;
    }

    [ContextMenu("Debug: Simulate Next Week")]
    void DebugSimulateNextWeek()
    {
        PlayerPrefs.SetString("WEEKLY_LAST_RESET", "1970-W01");
        Logger.Log("[DEBUG] 주간 리셋을 오래 전으로 설정. 앱 재시작 시 리셋됩니다.");
    }
}