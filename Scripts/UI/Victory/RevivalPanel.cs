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
    [SerializeField] Button adButton;
    [SerializeField] Button cristalButton;
    [SerializeField] Button giveUpButton;

    [Header("사운드")]
    [SerializeField] AudioClip revivalPanelSound;

    [Header("설정")]
    [SerializeField] int countdownSeconds = 8;
    [SerializeField] int cristalCost = 30;

    Character character;
    Coroutine countdownCoroutine;
    bool isRevived = false;
    bool hasUsedRevival = false;
    PanelTween panelTween;

    void Start()
    {
        panel.SetActive(false);
        panelTween = panel.GetComponent<PanelTween>();

        cristalCostText.text = $"{cristalCost}개로 부활";

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

        character = _character;
        isRevived = false;
        panelTween.ShowWithScale();
        adButton.interactable = AdsManager.IsRewardedAdReady;

        // 여기서 반드시 정지 보장
        GameManager.instance.pauseManager.PauseGame();

        countdownCoroutine = StartCoroutine(CountdownCo());

        // 패널 효과음은 먼저 재생한 뒤 나머지 사운드를 멈춤
        // (효과음을 Pause 후에 재생하면 Pause 상태에 걸리기 때문)
        if (revivalPanelSound != null)
            SoundManager.instance.Play(revivalPanelSound);

        // ── 부활 팝업 등장 시 모든 사운드 일시 정지 ──
        SoundManager.instance.PauseAllSounds();
    }

    IEnumerator CountdownCo()
    {
        int remaining = countdownSeconds;
        while (remaining > 0)
        {
            countdownText.text = $"{remaining}초";

            if (remaining > 5)
                countdownText.color = Color.yellow;
            else if (remaining > 2)
                countdownText.color = new Color(1f, 0.5f, 0f);
            else
                countdownText.color = Color.red;

            yield return new WaitForSecondsRealtime(1f);
            remaining--;
        }

        // 타임아웃 → 게임오버 (사운드 재개 없음)
        if (isRevived == false)
        {
            Logger.Log("[RevivalPanel] 카운트다운 종료 → 게임오버");
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
                    Logger.Log("[RevivalPanel] 광고 닫힘 → 부활");
                    DoRevive();
                }
                else
                {
                    Logger.Log("[RevivalPanel] 광고 미완료 → 게임오버");
                    Hide(resumeSounds: false);
                    character.ProcessDeath();
                }
            }
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
            Logger.LogWarning("[RevivalPanel] 크리스탈 부족");
            cristalButton.interactable = true; // 부족하면 다시 활성화
            return;
        }

        PlayerDataManager.Instance.AddCristal(-cristalCost);
        DoRevive();
    }

    void OnGiveUpButtonClicked()
    {
        Logger.Log("[RevivalPanel] 포기 선택 → 게임오버");

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        // 게임오버이므로 사운드 재개 없이 패널만 닫음
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

        // 부활 시에만 사운드 재개
        Hide(resumeSounds: true);
        character.Revive();
    }

    /// <summary>
    /// resumeSounds: true → 부활(사운드 재개) / false → 게임오버(재개 안 함)
    /// </summary>
    void Hide(bool resumeSounds)
    {
        countdownText.color = Color.yellow;
        panelTween.HideWithScale();

        if (resumeSounds)
            SoundManager.instance.ResumeAllSounds();
    }
}