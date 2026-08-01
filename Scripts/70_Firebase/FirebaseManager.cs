using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Analytics;

/// <summary>
/// Firebase Analytics 초기화 전담 매니저
/// 게임 데이터 초기화(GameInitializer)와는 독립적으로, 별도 코루틴에서 병렬 실행됩니다.
/// </summary>
public static class FirebaseManager
{
    public static bool IsInitialized { get; private set; } = false;
    public static bool InitializationFailed { get; private set; } = false;

    public static IEnumerator Initialize()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        if (dependencyTask.Exception != null)
        {
            Debug.LogError($"[FirebaseManager] 초기화 실패 (Exception): {dependencyTask.Exception}");
            InitializationFailed = true;
            yield break;
        }

        DependencyStatus status = dependencyTask.Result;

        if (status == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

            IsInitialized = true;
            Debug.Log("[FirebaseManager] ✅ Firebase Analytics 초기화 완료");
        }
        else
        {
            Debug.LogError($"[FirebaseManager] ❌ Firebase 의존성 문제: {status}");
            InitializationFailed = true;
        }
    }

    /// <summary>
    /// 커스텀 이벤트 로깅 (초기화 안 됐으면 무시하고 조용히 리턴 — 게임 흐름에 영향 없도록)
    /// </summary>
    public static void LogEvent(string eventName)
    {
        if (!IsInitialized) return;
        FirebaseAnalytics.LogEvent(eventName);
    }

    public static void LogEvent(string eventName, string paramName, string paramValue)
    {
        if (!IsInitialized) return;
        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }

    // ⭐ 신규 추가: 숫자(long) 파라미터 오버로드 — step_number 등 정수 값 기록용
    public static void LogEvent(string eventName, string paramName, long paramValue)
    {
        if (!IsInitialized) return;
        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }
}