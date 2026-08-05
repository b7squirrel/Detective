using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RevivalPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] TextMeshProUGUI cristalCostText;
    [SerializeField] TextMeshProUGUI adButtonLabel; // ⭐ 추가: 광고 버튼 안의 텍스트
    [SerializeField] Button adButton;
    [SerializeField] Button cristalButton;
    [SerializeField] Button giveUpButton;

    [Header("사운드")]
    [SerializeField] AudioClip revivalPanelSound;

    [Header("설정")]
    [SerializeField] int countdownSeconds = 8;
    [SerializeField] int cristalCost = 30;

    const string AD_LOADING_TEXT = "광고 로드 중..."; // ⭐ 추가

    Character character;
    Coroutine countdownCoroutine;
    bool isRevived = false;
    bool hasUsedRevival = false;
    PanelTween panelTween;

    string adButtonDefaultText; // ⭐ 추가
    int remainingCountdown; // ⭐ 추가: 카운트다운을 필드로 관리 (재개 시 이어서 진행)

    void Start()
    {
        panel.SetActive(false);
        panelTween = panel.GetComponent<PanelTween>();

        cristalCostText.text = $"{cristalCost}개로 부활";

        if (adButtonLabel != null)
            adButtonDefaultText = adButtonLabel.text; // ⭐ 추가

        adButton.onClick.AddListener(OnAdButtonClicked);
        cristalButton.onClick.AddListener(OnCristalButtonClicked);
        giveUpButton.onClick.AddListener(OnGiveUpButtonClicked);
    }

    public void Show(Character _character)
    {
        if (hasUsedRevival)
        {
            _character.ProcessDeath();
            return;
        }

        // ⭐ 추가: 광고가 준비 안 되어 있으면 패널 자체를 띄우지 않고 즉시 사망 처리
        if (!AdsManager.IsRewardedAdReady)
        {
            Debug.Log("[RevivalPanel] 광고 미준비 - 부활 패널 스킵, 즉시 게임오버 처리");
            FirebaseManager.LogEvent("revival_panel_skipped_ad_not_ready");
            AdsManager.Instance.RequestRewardedAdLoad(); // 다음 기회를 위해 재로드 트리거
            _character.ProcessDeath();
            return;
        }

        character = _character;
        isRevived = false;
        remainingCountdown = countdownSeconds; // ⭐ 추가
        panelTween.ShowWithScale();

        SyncAdButtonState(AdsManager.IsRewardedAdReady); // ⭐ 변경
        AdsManager.OnRewardedAdReadyChanged += SyncAdButtonState; // ⭐ 추가: 실시간 구독

        GameManager.instance.popupManager.BlockForRevival();

        GameManager.instance.pauseManager.PauseGame();

        countdownCoroutine = StartCoroutine(CountdownCo());

        if (revivalPanelSound != null)
            SoundManager.instance.Play(revivalPanelSound);

        SoundManager.instance.PauseAllSounds();
    }

    // ⭐ 추가: 광고 버튼의 interactable 상태와 텍스트를 함께 동기화
    void SyncAdButtonState(bool isReady)
    {
        if (adButton != null)
            adButton.interactable = isReady;

        if (adButtonLabel != null)
            adButtonLabel.text = isReady ? adButtonDefaultText : AD_LOADING_TEXT;
    }

    IEnumerator CountdownCo()
    {
        while (remainingCountdown > 0) // ⭐ 변경: 필드 기반으로 동작 (멈췄다 재개해도 값 유지)
        {
            countdownText.text = $"{remainingCountdown}";

            if (remainingCountdown > 5)
                countdownText.color = Color.yellow;
            else if (remainingCountdown > 2)
                countdownText.color = new Color(1f, 0.5f, 0f);
            else
                countdownText.color = Color.red;

            yield return new WaitForSecondsRealtime(1f);
            remainingCountdown--;
        }

        if (isRevived == false)
        {
            Debug.Log("[RevivalPanel] 카운트다운 종료 → 게임오버");
            Hide(resumeSounds: false);
            character.ProcessDeath();
        }
    }

    void OnAdButtonClicked()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        GameManager.instance.pauseManager.PauseGame();
        bool rewarded = false;

        AdsManager.Instance.ShowDailyFreeGemRewardedAd(
            onRewarded: () => { rewarded = true; },
            onClosed: () =>
            {
                if (rewarded)
                {
                    Debug.Log("[RevivalPanel] 광고 닫힘 → 부활");
                    DoRevive();
                }
                else
                {
                    Debug.Log("[RevivalPanel] 광고 미완료 → 게임오버");
                    Hide(resumeSounds: false);
                    character.ProcessDeath();
                }
            },
            onAdNotReady: () => // ⭐ 추가
            {
                Debug.LogWarning("[RevivalPanel] 광고 재생 불가 → 카운트다운 이어서 재개");
                countdownCoroutine = StartCoroutine(CountdownCo()); // remainingCountdown 값 그대로 이어서 진행
            },
            placement: "revival"
        );
    }

    void OnCristalButtonClicked()
    {
        cristalButton.interactable = false;
        adButton.interactable = false;
        giveUpButton.interactable = false;

        int currentCristal = PlayerDataManager.Instance.GetCurrentCristalNumber();
        if (currentCristal < cristalCost)
        {
            Debug.LogWarning("[RevivalPanel] 크리스탈 부족");
            cristalButton.interactable = true;
            return;
        }

        PlayerDataManager.Instance.AddCristal(-cristalCost);
        DoRevive();
    }

    void OnGiveUpButtonClicked()
    {
        Debug.Log("[RevivalPanel] 포기 선택 → 게임오버");

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        Hide(resumeSounds: false);
        character.ProcessDeath();
    }

    void DoRevive()
    {
        if (isRevived) return;
        isRevived = true;
        hasUsedRevival = true;

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        Hide(resumeSounds: true);
        GameManager.instance.popupManager.UnblockAfterRevival();
        character.Revive();
    }

    /// <summary>
    /// resumeSounds: true → 부활(사운드 재개) / false → 게임오버(재개 안 함)
    /// </summary>
    void Hide(bool resumeSounds)
    {
        AdsManager.OnRewardedAdReadyChanged -= SyncAdButtonState; // ⭐ 추가: 구독 해제

        countdownText.color = Color.yellow;
        panelTween.HideWithScale();

        if (resumeSounds)
            SoundManager.instance.ResumeAllSounds();
    }

    void OnDestroy() // ⭐ 추가: 오브젝트 파괴 시 안전장치
    {
        AdsManager.OnRewardedAdReadyChanged -= SyncAdButtonState;
    }
}