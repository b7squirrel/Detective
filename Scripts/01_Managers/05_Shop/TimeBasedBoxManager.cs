using UnityEngine;
using System;
using System.Threading.Tasks;

public class TimeBasedBoxManager : SingletonBehaviour<TimeBasedBoxManager>
{
    [Header("설정")]
    [SerializeField] private float cooldownMinutes = 60f;
    [SerializeField] private bool useServerTime = true; // ⭐ 서버 시간 사용 여부
    
    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;
    
    private const string LAST_CLAIM_TIME_KEY = "LastTimeBoxClaimTime";
    
    protected override void Init()
    {
        base.Init();
        
        if (showDebugLogs)
        {
            Logger.Log("[TimeBasedBoxManager] 초기화 완료");
            Logger.Log($"[TimeBasedBoxManager] 서버 시간 사용: {useServerTime}");
        }
    }
    
    /// <summary>
    /// 현재 시간 가져오기 (서버 또는 디바이스)
    /// </summary>
    private async Task<DateTime> GetCurrentTime()
    {
        if (useServerTime && NetworkController.Instance != null)
        {
            return await NetworkController.Instance.GetCurrentDateTime();
        }
        else
        {
            return DateTime.Now;
        }
    }
    
    /// <summary>
    /// 상자를 열 수 있는지 확인 (비동기)
    /// </summary>
    public async Task<bool> CanClaimBoxAsync()
    {
        if (!PlayerPrefs.HasKey(LAST_CLAIM_TIME_KEY))
        {
            return true;
        }
        
        string lastClaimTimeStr = PlayerPrefs.GetString(LAST_CLAIM_TIME_KEY);
        DateTime lastClaimTime;
        
        if (!DateTime.TryParse(lastClaimTimeStr, out lastClaimTime))
        {
            Logger.LogError($"[TimeBasedBoxManager] 시간 파싱 오류: {lastClaimTimeStr}");
            ResetCooldown();
            return true;
        }
        
        DateTime currentTime = await GetCurrentTime();
        TimeSpan elapsed = currentTime - lastClaimTime;
        
        return elapsed.TotalMinutes >= cooldownMinutes;
    }
    
    /// <summary>
    /// 상자를 열 수 있는지 확인 (동기 - UI용)
    /// </summary>
    public bool CanClaimBox()
    {
        if (!PlayerPrefs.HasKey(LAST_CLAIM_TIME_KEY))
        {
            return true;
        }
        
        string lastClaimTimeStr = PlayerPrefs.GetString(LAST_CLAIM_TIME_KEY);
        DateTime lastClaimTime;
        
        if (!DateTime.TryParse(lastClaimTimeStr, out lastClaimTime))
        {
            return true;
        }
        
        // ⭐ 동기 버전은 디바이스 시간 사용 (UI 업데이트용)
        DateTime currentTime = DateTime.Now;
        TimeSpan elapsed = currentTime - lastClaimTime;
        
        return elapsed.TotalMinutes >= cooldownMinutes;
    }
    
    /// <summary>
    /// 남은 시간 반환 (초 단위)
    /// </summary>
    public float GetRemainingSeconds()
    {
        if (!PlayerPrefs.HasKey(LAST_CLAIM_TIME_KEY))
        {
            return 0f;
        }
        
        string lastClaimTimeStr = PlayerPrefs.GetString(LAST_CLAIM_TIME_KEY);
        DateTime lastClaimTime;
        
        if (!DateTime.TryParse(lastClaimTimeStr, out lastClaimTime))
        {
            return 0f;
        }
        
        DateTime currentTime = DateTime.Now;
        TimeSpan elapsed = currentTime - lastClaimTime;
        float remainingSeconds = (float)(cooldownMinutes * 60 - elapsed.TotalSeconds);
        
        return Mathf.Max(0f, remainingSeconds);
    }
    
    /// <summary>
    /// 상자를 열었을 때 호출 (서버 시간 기록)
    /// </summary>
    public async Task OnBoxClaimedAsync()
    {
        DateTime currentTime = await GetCurrentTime();
        string currentTimeStr = currentTime.ToString("o"); // ISO 8601 형식
        
        PlayerPrefs.SetString(LAST_CLAIM_TIME_KEY, currentTimeStr);
        PlayerPrefs.Save();
        
        Logger.Log($"[TimeBasedBoxManager] 상자 열림 시간 기록: {currentTime}");
        Logger.Log($"[TimeBasedBoxManager] 다음 가능 시간: {currentTime.AddMinutes(cooldownMinutes)}");
    }
    
    public void ResetCooldown()
    {
        PlayerPrefs.DeleteKey(LAST_CLAIM_TIME_KEY);
        PlayerPrefs.Save();
        Logger.Log("[TimeBasedBoxManager] 쿨다운 리셋됨");
    }
    
    public string GetRemainingTimeFormatted()
    {
        float seconds = GetRemainingSeconds();
        int hours = Mathf.FloorToInt(seconds / 3600);
        int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        
        return $"{hours:00}:{minutes:00}:{secs:00}";
    }
}