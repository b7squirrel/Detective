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

    [Header("설정")]
    [SerializeField] int countdownSeconds = 8;
    [SerializeField] int cristalCost = 30;

    Character character;
    Coroutine countdownCoroutine;
    bool isRevived = false;
    bool hasUsedRevival = false; // 이번 판에 부활을 이미 사용했는지

    void Start()
    {
        panel.SetActive(false);
        cristalCostText.text = $"{cristalCost}개로 부활";

        adButton.onClick.AddListener(OnAdButtonClicked);
        cristalButton.onClick.AddListener(OnCristalButtonClicked);
        giveUpButton.onClick.AddListener(OnGiveUpButtonClicked);
    }

    public void Show(Character _character)
    {
        // 이미 부활을 사용했으면 바로 게임오버
        if (hasUsedRevival)
        {
            _character.ProcessDeath();
            return;
        }

        character = _character;
        isRevived = false;
        panel.SetActive(true);
        adButton.interactable = AdsManager.IsRewardedAdReady;
        countdownCoroutine = StartCoroutine(CountdownCo());
    }

    IEnumerator CountdownCo()
    {
        int remaining = countdownSeconds;
        while (remaining > 0)
        {
            countdownText.text = $"{remaining}초";

            // 남은 시간에 따라 색상 변경
            if (remaining > 5)
            {
                countdownText.color = Color.yellow;
            }
            else if (remaining > 2)
            {
                countdownText.color = new Color(1f, 0.5f, 0f); // 주황
            }
            else
            {
                countdownText.color = Color.red;
            }

            yield return new WaitForSecondsRealtime(1f);
            remaining--;
        }

        // 타임아웃
        if (isRevived == false)
        {
            Logger.Log("[RevivalPanel] 카운트다운 종료 → 게임오버");
            Hide();
            character.ProcessDeath();
        }
    }

    void OnAdButtonClicked()
    {
        // 광고 시작 전 게임 일시정지
        GameManager.instance.pauseManager.PauseGame();

        AdsManager.Instance.ShowDailyFreeGemRewardedAd(() =>
        {
            Logger.Log("[RevivalPanel] 광고 시청 완료 → 부활");
            // 광고 종료 후 게임 재개
            GameManager.instance.pauseManager.UnPauseGame();
            DoRevive();
        });
    }

    void OnCristalButtonClicked()
    {
        int currentCristal = PlayerDataManager.Instance.GetCurrentCristalNumber();
        if (currentCristal < cristalCost)
        {
            Logger.LogWarning("[RevivalPanel] 크리스탈 부족");
            // 추후 부족 알림 UI 추가 가능
            return;
        }

        PlayerDataManager.Instance.AddCristal(-cristalCost);
        Logger.Log($"[RevivalPanel] 크리스탈 {cristalCost}개 소모 → 부활");
        DoRevive();
    }

    void OnGiveUpButtonClicked()
    {
        Logger.Log("[RevivalPanel] 포기 선택 → 게임오버");

        // 카운트다운 코루틴 중지
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        Hide();
        character.ProcessDeath();
    }

    void DoRevive()
    {
        if (isRevived) return;
        isRevived = true;
        hasUsedRevival = true; // ⭐ 추가

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        Hide();
        character.Revive();
    }

    void Hide()
    {
        countdownText.color = Color.yellow; // 색상 초기화
        panel.SetActive(false);
    }
}