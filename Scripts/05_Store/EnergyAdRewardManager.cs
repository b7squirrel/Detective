using UnityEngine;
using System;
using System.Threading.Tasks;

public class EnergyAdRewardManager : SingletonBehaviour<EnergyAdRewardManager>
{
    [Header("설정")]
    [SerializeField] private float cooldownMinutes = 30f;
    [SerializeField] private bool useServerTime = true;

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;

    private const string LAST_CLAIM_TIME_KEY = "EnergyAd_LastClaimTime";

    protected override void Init()
    {
        base.Init();

        if (showDebugLogs)
            Logger.Log($"[EnergyAdRewardManager] 초기화 완료 (쿨다운 {cooldownMinutes}분, 일일 횟수 제한 없음)");
    }

    private async Task<DateTime> GetCurrentTime()
    {
        if (useServerTime && NetworkController.Instance != null)
            return await NetworkController.Instance.GetCurrentDateTime();
        else
            return DateTime.Now;
    }

    /// <summary>
    /// 광고를 볼 수 있는지 확인 (비동기 - 서버 시간 기반, 실제 소모 직전 체크용)
    /// </summary>
    public async Task<bool> CanClaimAsync()
    {
        if (!PlayerPrefs.HasKey(LAST_CLAIM_TIME_KEY))
            return true;

        string lastClaimTimeStr = PlayerPrefs.GetString(LAST_CLAIM_TIME_KEY);
        if (!DateTime.TryParse(lastClaimTimeStr, out DateTime lastClaimTime))
        {
            PlayerPrefs.DeleteKey(LAST_CLAIM_TIME_KEY);
            return true;
        }

        DateTime currentTime = await GetCurrentTime();
        TimeSpan elapsed = currentTime - lastClaimTime;

        return elapsed.TotalMinutes >= cooldownMinutes;
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
        int minutes = Mathf.CeilToInt(seconds / 60f); // 올림 처리 (아래 이유 참고)
        return $"{minutes}분";
    }

    /// <summary>
    /// 광고 시청 완료 시 호출 — 쿨다운 갱신
    /// </summary>
    public async Task OnAdClaimedAsync()
    {
        DateTime currentTime = await GetCurrentTime();
        string currentTimeStr = currentTime.ToString("o");

        PlayerPrefs.SetString(LAST_CLAIM_TIME_KEY, currentTimeStr);
        PlayerPrefs.Save();

        if (showDebugLogs)
            Logger.Log($"[EnergyAdRewardManager] 광고 시청 기록: {currentTime}, 다음 가능: {currentTime.AddMinutes(cooldownMinutes)}");
    }
}