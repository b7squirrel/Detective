using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeeklyRewardPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform rewardListContent;
    [SerializeField] private GameObject rewardItemPrefab;
    [SerializeField] private Button claimAllButton;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("이펙트")]
    [SerializeField] private GemCollectFX gemCollectFX;
    [SerializeField] private RectTransform effectStartPos;

    [Header("사운드")]
    [SerializeField] private AudioClip clipOpen;
    [SerializeField] private AudioClip clipClaim;

    private List<RuntimeAchievement> unclaimedList = new();
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        claimAllButton.onClick.AddListener(ClaimAll);
    }

    private void OnEnable()
    {
        WeeklyResetManager.OnWeeklyResetWithUnclaimed += Show;
    }

    private void OnDisable()
    {
        WeeklyResetManager.OnWeeklyResetWithUnclaimed -= Show;
    }

    public void Show()
    {
        Logger.Log("[WeeklyRewardPopup] Show() 호출됨!");

        unclaimedList = AchievementManager.Instance.GetUnclaimedCompletedWeeklyQuests();
        Logger.Log($"[WeeklyRewardPopup] 미수령 목록 수: {unclaimedList.Count}");

        if (unclaimedList.Count == 0)
        {
            Logger.Log("[WeeklyRewardPopup] 미수령 없음 - 팝업 안 뜸");
            return;
        }

        // 기존 목록 제거
        foreach (Transform child in rewardListContent)
            Destroy(child.gameObject);

        // ✅ 수정: WeeklyRewardItemUI.Bind() 사용
        foreach (var ra in unclaimedList)
        {
            var item = Instantiate(rewardItemPrefab, rewardListContent);
            var ui = item.GetComponent<WeeklyRewardItemUI>();
            if (ui != null)
                ui.Bind(ra);
        }

        gameObject.SetActive(true);
        if (clipOpen != null) SoundManager.instance.Play(clipOpen);
    }

    private void ClaimAll()
    {
        StartCoroutine(ClaimAllCoroutine());
    }

    private IEnumerator ClaimAllCoroutine()
    {
        claimAllButton.interactable = false;

        if (clipClaim != null) SoundManager.instance.Play(clipClaim);

        if (gemCollectFX == null)
            gemCollectFX = FindObjectOfType<GemCollectFX>();

        PlayerDataManager pdm = PlayerDataManager.Instance;

        // 보상 타입별 합산
        int totalGem = 0;
        int totalCoin = 0;

        foreach (var ra in unclaimedList)
        {
            if (ra.original.rewardType == RewardType.GEM)
                totalGem += ra.original.rewardNum;
            else if (ra.original.rewardType == RewardType.COIN)
                totalCoin += ra.original.rewardNum;

            ra.Reward();
            AchievementManager.Instance.SaveAchievement(ra);
        }

        // 보석 지급
        if (totalGem > 0)
        {
            pdm.SetCristalNumberAsSilent(pdm.GetCurrentCristalNumber() + totalGem);
            if (gemCollectFX != null)
                gemCollectFX.PlayGemCollectFX(effectStartPos, totalGem, true);

            yield return new WaitForSeconds(0.4f);
        }

        // 코인 지급
        if (totalCoin > 0)
        {
            pdm.SetCoinNumberAsSilent(pdm.GetCurrentCoinNumber() + totalCoin);
            if (gemCollectFX != null)
                gemCollectFX.PlayGemCollectFX(effectStartPos, totalCoin, false);

            yield return new WaitForSeconds(0.4f);
        }

        yield return new WaitForSeconds(0.5f);

        // 주간 리셋 진행
        WeeklyResetManager.Instance.PerformWeeklyReset();

        // 팝업 닫기
        if (anim != null)
        {
            anim.SetTrigger("Down");
            yield return new WaitForSeconds(0.5f);
        }

        gameObject.SetActive(false);
    }
}