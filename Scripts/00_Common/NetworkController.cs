using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

/// <summary>
/// 네트워크 시간 동기화 및 API 통신 관리
/// </summary>
public class NetworkController : SingletonBehaviour<NetworkController>
{
    [Header("시간 API 설정")]
    [SerializeField] private string timeAPIUrl = "https://worldtimeapi.org/api/ip";
    [SerializeField] private float requestTimeout = 10f; // 타임아웃 설정
    
    [Header("캐싱 설정")]
    [SerializeField] private bool enableTimeCaching = true;
    [SerializeField] private float cacheValidDuration = 300f; // 5분간 캐시 유효
    
    private DateTime? cachedServerTime = null;
    private float cachedTime = 0f;
    private bool isRequestInProgress = false;
    
    [System.Serializable]
    struct TimeDataWrapper
    {
        public string datetime;
        public long unixtime;
        public string utc_datetime;
    }
    
    protected override void Init()
    {
        base.Init();
        Logger.Log("[NetworkController] 초기화 완료");
    }
    
    /// <summary>
    /// 현재 서버 시간 가져오기 (캐싱 지원)
    /// </summary>
    public async Task<DateTime> GetCurrentDateTime(bool forceRefresh = false)
    {
        // ⭐ 캐시 사용 (네트워크 요청 절약)
        if (enableTimeCaching && !forceRefresh && cachedServerTime.HasValue)
        {
            float elapsedSinceCached = Time.realtimeSinceStartup - cachedTime;
            
            if (elapsedSinceCached < cacheValidDuration)
            {
                DateTime estimatedTime = cachedServerTime.Value.AddSeconds(elapsedSinceCached);
                Logger.Log($"[NetworkController] 캐시된 시간 사용: {estimatedTime} (캐시된 지 {elapsedSinceCached:F1}초)");
                return estimatedTime;
            }
        }
        
        // ⭐ 중복 요청 방지
        if (isRequestInProgress)
        {
            Logger.LogWarning("[NetworkController] 시간 요청이 이미 진행 중입니다. 대기 중...");
            
            // 진행 중인 요청이 끝날 때까지 대기
            while (isRequestInProgress)
            {
                await Task.Yield();
            }
            
            // 요청이 끝났으면 캐시된 값 반환
            if (cachedServerTime.HasValue)
            {
                return cachedServerTime.Value;
            }
        }
        
        isRequestInProgress = true;
        
        try
        {
            DateTime serverTime = await FetchServerTime();
            
            if (serverTime != DateTime.MinValue)
            {
                // ⭐ 캐시 저장
                cachedServerTime = serverTime;
                cachedTime = Time.realtimeSinceStartup;
                Logger.Log($"[NetworkController] 서버 시간 업데이트: {serverTime}");
            }
            
            return serverTime;
        }
        finally
        {
            isRequestInProgress = false;
        }
    }
    
    /// <summary>
    /// 서버 시간 요청 (실제 네트워크 통신)
    /// </summary>
    private async Task<DateTime> FetchServerTime()
    {
        Logger.Log($"[NetworkController] 서버 시간 요청 시작: {timeAPIUrl}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(timeAPIUrl))
        {
            // ⭐ 타임아웃 설정
            request.timeout = (int)requestTimeout;
            
            var operation = request.SendWebRequest();
            
            // ⭐ 타임아웃 체크
            float elapsedTime = 0f;
            while (!operation.isDone)
            {
                await Task.Yield();
                elapsedTime += Time.deltaTime;
                
                if (elapsedTime > requestTimeout)
                {
                    Logger.LogError($"[NetworkController] 요청 타임아웃 ({requestTimeout}초)");
                    return GetFallbackTime();
                }
            }
            
            // ⭐ 에러 처리
            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Logger.LogError($"[NetworkController] 네트워크 오류: {request.error}");
                Logger.LogError($"[NetworkController] Response Code: {request.responseCode}");
                return GetFallbackTime();
            }
            
            // ⭐ JSON 파싱
            try
            {
                string jsonResponse = request.downloadHandler.text;
                Logger.Log($"[NetworkController] 응답: {jsonResponse.Substring(0, Mathf.Min(100, jsonResponse.Length))}...");
                
                TimeDataWrapper timeData = JsonUtility.FromJson<TimeDataWrapper>(jsonResponse);
                DateTime currentDateTime = ParseDateTime(timeData.datetime);
                
                Logger.Log($"[NetworkController] 파싱 성공: {currentDateTime}");
                return currentDateTime;
            }
            catch (Exception e)
            {
                Logger.LogError($"[NetworkController] JSON 파싱 오류: {e.Message}");
                return GetFallbackTime();
            }
        }
    }
    
    /// <summary>
    /// 날짜/시간 문자열 파싱
    /// </summary>
    private DateTime ParseDateTime(string dateTimeString)
    {
        try
        {
            // 2024-11-03T20:37:11.321767-05:00
            string date = Regex.Match(dateTimeString, @"^\d{4}-\d{2}-\d{2}").Value;
            string time = Regex.Match(dateTimeString, @"\d{2}:\d{2}:\d{2}").Value;
            
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
            {
                Logger.LogError($"[NetworkController] 날짜/시간 파싱 실패: {dateTimeString}");
                return GetFallbackTime();
            }
            
            return DateTime.Parse($"{date} {time}");
        }
        catch (Exception e)
        {
            Logger.LogError($"[NetworkController] DateTime 파싱 오류: {e.Message}");
            return GetFallbackTime();
        }
    }
    
    /// <summary>
    /// 서버 시간을 가져올 수 없을 때 폴백 처리
    /// </summary>
    private DateTime GetFallbackTime()
    {
        // ⭐ 옵션 1: 디바이스 시간 사용 (보안 취약하지만 게임은 계속 진행)
        DateTime deviceTime = DateTime.Now;
        Logger.LogWarning($"[NetworkController] 폴백: 디바이스 시간 사용 ({deviceTime})");
        return deviceTime;
        
        // ⭐ 옵션 2: DateTime.MinValue 반환 (더 안전하지만 기능 제한)
        // Logger.LogError("[NetworkController] 서버 시간을 가져올 수 없습니다.");
        // return DateTime.MinValue;
    }
    
    /// <summary>
    /// 캐시 초기화 (디버그용)
    /// </summary>
    [ContextMenu("캐시 초기화")]
    public void ClearCache()
    {
        cachedServerTime = null;
        cachedTime = 0f;
        Logger.Log("[NetworkController] 시간 캐시 초기화됨");
    }
    
    /// <summary>
    /// 서버 시간과 디바이스 시간 차이 확인 (치트 감지용)
    /// </summary>
    public async Task<TimeSpan> GetTimeDifference()
    {
        DateTime serverTime = await GetCurrentDateTime(forceRefresh: true);
        DateTime deviceTime = DateTime.Now;
        
        TimeSpan difference = serverTime - deviceTime;
        
        Logger.Log($"[NetworkController] 서버 시간: {serverTime}");
        Logger.Log($"[NetworkController] 디바이스 시간: {deviceTime}");
        Logger.Log($"[NetworkController] 차이: {difference.TotalSeconds:F1}초");
        
        // ⭐ 시간 차이가 크면 경고 (치트 의심)
        if (Math.Abs(difference.TotalMinutes) > 5)
        {
            Logger.LogWarning($"[NetworkController] ⚠️ 시간 차이가 {difference.TotalMinutes:F1}분입니다. 시간 조작 의심!");
        }
        
        return difference;
    }
}