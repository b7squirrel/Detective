using System.Collections;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    float normalTimeScale = 1.0f;
    Coroutine watchdogCoroutine;

    public float NormalTimeScale => normalTimeScale;

    // ⭐ 추가: 지금 패널(업그레이드/부활 등)이 의도적으로 timeScale=0을 유지 중인지
    public bool IsPausedByPanel => watchdogCoroutine != null;

    void Start()
    {
        UnPauseGame();
    }

    public void SetNormalTimeScale(float timeScale)
    {
        normalTimeScale = timeScale;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        GameManager.instance.SetPauseState(true);
        StartTimeScaleWatchdog();
    }

    public void UnPauseGame()
    {
        Logger.Log($"[PauseManager] UnPauseGame 호출됨. Stack: {System.Environment.StackTrace}");
        StopTimeScaleWatchdog();
        Time.timeScale = normalTimeScale;
        GameManager.instance.SetPauseState(false);
    }

    public void SetTimeScale(float timeScale, float waitingTime)
    {
        StartCoroutine(SlowMotion(timeScale, waitingTime));
    }

    IEnumerator SlowMotion(float desiredTimeScale, float waitingTime)
    {
        Time.timeScale = desiredTimeScale;
        yield return new WaitForSecondsRealtime(waitingTime);
        UnPauseGame();
    }

    void StartTimeScaleWatchdog()
    {
        if (watchdogCoroutine != null) StopCoroutine(watchdogCoroutine);
        watchdogCoroutine = StartCoroutine(EnforceZeroTimeScale());
    }

    void StopTimeScaleWatchdog()
    {
        if (watchdogCoroutine != null)
        {
            StopCoroutine(watchdogCoroutine);
            watchdogCoroutine = null;
        }
    }

    IEnumerator EnforceZeroTimeScale()
    {
        while (true)
        {
            if (Time.timeScale != 0f)
            {
                Time.timeScale = 0f;
            }
            yield return null;
        }
    }
}