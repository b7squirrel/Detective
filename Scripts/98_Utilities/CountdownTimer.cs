using UnityEngine;
using TMPro;
using System;
using DG.Tweening;

public class CountdownTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI secondText;
    [SerializeField] TextMeshProUGUI fractionText;
    [SerializeField] float popScale = 1.5f; // 팝 최대 크기
    [SerializeField] float popDuration = 0.2f; // 애니메이션 시간

    [Header("Final Second Effect")]
    [SerializeField] RectTransform timeRoot;
    [SerializeField] float shakeAmount = 6f;
    [SerializeField] float shakeSpeed = 25f;
    [SerializeField] float pulseScale = 1.15f;

    [Header("Final Second Color")]
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color finalColor = Color.red;
    [SerializeField] float colorDuration = 0.2f; // 색상 전환 시간

    Vector3 originalPos;
    Vector3 originalScale;
    bool isFinalSecond;

    [Header("Sound Clips")]
    [SerializeField] AudioClip tickingLoopClip;   // 지속 틱틱
    [SerializeField] AudioClip secondTickClip;    // 초 변경 띵
    [SerializeField] AudioClip endClip;           // (선택) 종료음

    float duration;
    float remainingTime;
    int lastSecond;
    bool wasPaused; // 이전 프레임의 일시정지 상태

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

        // 파이널 세컨드 이펙트 초기값 저장
        originalPos = timeRoot.anchoredPosition;
        originalScale = timeRoot.localScale;
        isFinalSecond = false;

        UpdateUI();
    }

    void Update()
    {
        if (!isRunning) return;

        // 일시정지 상태 체크
        HandlePauseState();

        // 일시정지 중이면 카운트다운 멈춤
        if (GameManager.instance.IsPaused) return;

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(remainingTime, 0f);

        UpdateUI();
        HandleSecondTick();
        HandleFinalSecondEffect();

        if (remainingTime <= 0f)
        {
            CompleteTimer();
        }
    }

    void HandlePauseState()
    {
        bool isPaused = GameManager.instance.IsPaused;

        // 일시정지 시작
        if (isPaused && !wasPaused)
        {
            if (tickingLoopClip != null)
            {
                SoundManager.instance.StopLoop(tickingLoopClip);
            }
        }
        // 일시정지 해제
        else if (!isPaused && wasPaused)
        {
            if (tickingLoopClip != null)
            {
                SoundManager.instance.PlayLoop(tickingLoopClip, 0.6f);
            }
        }

        wasPaused = isPaused;
    }

    void UpdateUI()
    {
        int seconds = Mathf.FloorToInt(remainingTime);
        int fraction = Mathf.FloorToInt((remainingTime - seconds) * 100);

        secondText.text = seconds.ToString();
        fractionText.text = "." + fraction.ToString("00");
    }

    void HandleSecondTick()
    {
        int currentSecond = Mathf.CeilToInt(remainingTime);

        if (currentSecond != lastSecond)
        {
            // 초가 바뀌었을 때
            lastSecond = currentSecond;

            // 팝 애니메이션
            secondText.transform.DOKill(); // 기존 애니메이션 제거
            secondText.transform.localScale = Vector3.one; // 초기화
            secondText.transform.DOScale(popScale, popDuration / 2f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    secondText.transform.DOScale(Vector3.one, popDuration / 2f)
                        .SetEase(Ease.InQuad);
                });

            // 초 바뀌는 사운드
            if (secondTickClip != null)
            {
                SoundManager.instance.Play(secondTickClip);
            }
        }
    }

    void CompleteTimer()
    {
        isRunning = false;

        // 위치/스케일 원복
        timeRoot.anchoredPosition = originalPos;
        timeRoot.localScale = originalScale;

        // 색상 원복
        secondText.color = normalColor;

        // DoTween 색상 제거
        secondText.DOKill();

        // 루프 사운드 정지
        if (tickingLoopClip != null)
        {
            SoundManager.instance.StopLoop(tickingLoopClip);
        }

        // 종료 사운드 재생 (선택)
        if (endClip != null)
            SoundManager.instance.Play(endClip);

        // 콜백 호출
        if (onComplete != null)
            onComplete.Invoke();
    }

    void HandleFinalSecondEffect()
    {
        // 마지막 1초 진입 체크
        if (remainingTime <= 1f && !isFinalSecond)
        {
            isFinalSecond = true;

            // 깜빡임 시작
            if (!DOTween.IsTweening(secondText))
            {
                secondText.DOColor(finalColor, colorDuration)
                    .SetLoops(-1, LoopType.Yoyo); // 빨간색 ↔ 원래색 반복
            }
        }

        if (!isFinalSecond) return;

        // 흔들림 효과
        float shakeX = Mathf.Sin(Time.unscaledTime * shakeSpeed) * shakeAmount;
        timeRoot.anchoredPosition = originalPos + new Vector3(shakeX, 0f, 0f);

        // 펄스 스케일
        float scale = 1f + Mathf.Sin(Time.unscaledTime * shakeSpeed) * (pulseScale - 1f);
        timeRoot.localScale = originalScale * scale;
    }

    void OnDisable()
    {
        // 안전장치
        if (tickingLoopClip != null)
            SoundManager.instance.StopLoop(tickingLoopClip);

        isRunning = false;
    }
}
