using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CountdownTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI timeText;

    [Header("Sound Clips")]
    [SerializeField] AudioClip tickingLoopClip;   // 지속 틱틱
    [SerializeField] AudioClip secondTickClip;    // 초 변경 띵
    [SerializeField] AudioClip endClip;           // (선택) 종료음

    float duration;
    float remainingTime;
    int lastSecond;

    bool isRunning;
    Action onComplete;

    public void StartTimer(float seconds, Action _onComplete = null)
    {
        duration = seconds;
        remainingTime = seconds;
        lastSecond = Mathf.CeilToInt(seconds);
        onComplete = _onComplete;

        isRunning = true;
        gameObject.SetActive(true);

        // 지속 ticking 시작
        if (tickingLoopClip != null)
        {
            SoundManager.instance.PlayLoop(tickingLoopClip, 0.6f);
        }

        UpdateUI();
    }

    void Update()
    {
        if (!isRunning) return;

        remainingTime -= Time.unscaledDeltaTime;
        remainingTime = Mathf.Max(remainingTime, 0f);

        UpdateUI();
        HandleSecondTick();

        if (remainingTime <= 0f)
        {
            CompleteTimer();
        }
    }

    void UpdateUI()
    {
        if (timeText != null)
            timeText.text = remainingTime.ToString("F2");
    }

    void HandleSecondTick()
    {
        int currentSecond = Mathf.CeilToInt(remainingTime);
        if (currentSecond != lastSecond)
        {
            lastSecond = currentSecond;

            if (secondTickClip != null)
                SoundManager.instance.Play(secondTickClip);
        }
    }

    void CompleteTimer()
    {
        isRunning = false;

        // 루프 정지
        if (tickingLoopClip != null)
            SoundManager.instance.StopLoop(tickingLoopClip);

        // 종료음
        if (endClip != null)
            SoundManager.instance.Play(endClip);

        onComplete?.Invoke();
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        // 안전장치
        if (tickingLoopClip != null)
            SoundManager.instance.StopLoop(tickingLoopClip);

        isRunning = false;
    }
}
