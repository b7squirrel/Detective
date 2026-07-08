using UnityEngine;
using System;
using System.Threading.Tasks;

public class EnergyAdRewardManager : SingletonBehaviour<EnergyAdRewardManager>
{
    [Header("설정")]
    [SerializeField] private float cooldownMinutes = 30f;
    [SerializeField] private int maxDailyCount = 5;
    [SerializeField] private bool useServerTime = true;

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;

    private const string LAST_CLAIM_TIME_KEY = "EnergyAd_LastClaimTime";
    private const string DAILY_COUNT_KEY = "EnergyAd_DailyCount";
    private const string DAILY_COUNT_DATE_KEY = "EnergyAd_DailyCountDate";

    protected override void Init()
    {
        base.Init();

        // 날짜가 바뀌었으면 카운트 리셋 (DailyResetManager 이벤트도 별도 구독)
        ValidateDailyCountDate();

        if (showDebugLogs)
            Logger.Log($"[EnergyAdRewardManager] 초기화 완료 (쿨다운 {cooldownMinutes}분, 일일 {maxDailyCount}회)");
    }

    void OnEnable()
    {
        DailyResetManager.OnDailyReset += ResetDailyCount;
    }

    void OnDisable()
    {
        DailyResetManager.OnDailyReset -= ResetDailyCount;
    }

    private async Task<DateTime> GetCurrentTime()
    {
        if (useServerTime && NetworkController.Instance != null)
            return await NetworkController.Instance.GetCurrentDateTime();
        else
            return DateTime.Now;
    }

    // ⭐ 자정이 지났는데 DailyResetManager 이벤트를 놓쳤을 경우 대비 (앱 재시작 시 안전장치)
    void ValidateDailyCountDate()
    {
        string today = DailyResetManager.GetTodayString();
        string savedDate = PlayerPrefs.GetString(DAILY_COUNT_DATE_KEY, "");

        if (savedDate != today)
        {
            PlayerPrefs.SetInt(DAILY_COUNT_KEY, 0);
            PlayerPrefs.SetString(DAILY_COUNT_DATE_KEY, today);
            PlayerPrefs.Save();
        }
    }

    void ResetDailyCount()
    {
        PlayerPrefs.SetInt(DAILY_COUNT_KEY, 0);
        PlayerPrefs.SetString(DAILY_COUNT_DATE_KEY, DailyResetManager.GetTodayString());
        PlayerPrefs.Save();

        if (showDebugLogs)
            Logger.Log("[EnergyAdRewardManager] 일일 횟수 리셋됨");
    }

    public int GetTodayClaimedCount()
    {
        ValidateDailyCountDate();
        return PlayerPrefs.GetInt(DAILY_COUNT_KEY, 0);
    }

    public int GetRemainingCountToday()
    {
        return Mathf.Max(0, maxDailyCount - GetTodayClaimedCount());
    }

    /// <summary>
    /// 광고를 볼 수 있는지 확인 (비동기 - 서버 시간 기반, 실제 소모 직전 체크용)
    /// </summary>
    public async Task<(bool canClaim, string reason)> CanClaimAsync()
    {
        if (GetRemainingCountToday() <= 0)
            return (false, "daily_limit");

        if (!PlayerPrefs.HasKey(LAST_CLAIM_TIME_KEY))
            return (true, "");

        string lastClaimTimeStr = PlayerPrefs.GetString(LAST_CLAIM_TIME_KEY);
        if (!DateTime.TryParse(lastClaimTimeStr, out DateTime lastClaimTime))
        {
            PlayerPrefs.DeleteKey(LAST_CLAIM_TIME_KEY);
            return (true, "");
        }

        DateTime currentTime = await GetCurrentTime();
        TimeSpan elapsed = currentTime - lastClaimTime;

        if (elapsed.TotalMinutes < cooldownMinutes)
            return (false, "cooldown");

        return (true, "");
    }

    /// <summary>
    /// 남은 쿨다운 시간(초) - UI 표시용 (디바이스 시간 기준)
    /// </summary>
    public float GetRemainingCooldownSeconds()
    {
        if (!PlayerPrefs.HasKey(LAST_CLAIM_TIME_KEY))
            return 0f;

        string lastClaimTimeStr = PlayerPrefs.GetString(LAST_CLAIM_TIME_KEY);
        if (!DateTime.TryParse(lastClaimTimeStr, out DateTime lastClaimTime))
            return 0f;

        TimeSpan elapsed = DateTime.Now - lastClaimTime;
        float remainingSeconds = (float)(cooldownMinutes * 60 - elapsed.TotalSeconds);

        return Mathf.Max(0f, remainingSeconds);
    }

    public string GetRemainingCooldownFormatted()
    {
        float seconds = GetRemainingCooldownSeconds();
        int minutes = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{minutes:00}:{secs:00}";
    }

    /// <summary>
    /// 광고 시청 완료 시 호출 — 쿨다운 갱신 + 일일 카운트 증가
    /// </summary>
    public async Task OnAdClaimedAsync()
    {
        DateTime currentTime = await GetCurrentTime();
        string currentTimeStr = currentTime.ToString("o");

        PlayerPrefs.SetString(LAST_CLAIM_TIME_KEY, currentTimeStr);

        ValidateDailyCountDate(); // 자정 넘어간 사이 광고 볼 수도 있으니 먼저 검증
        int count = PlayerPrefs.GetInt(DAILY_COUNT_KEY, 0);
        PlayerPrefs.SetInt(DAILY_COUNT_KEY, count + 1);
        PlayerPrefs.Save();

        if (showDebugLogs)
            Logger.Log($"[EnergyAdRewardManager] 광고 시청 기록: {currentTime}, 오늘 {count + 1}/{maxDailyCount}회");
    }
}