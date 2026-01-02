using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyRewardPanel : MonoBehaviour
{
    [Header("보상 데이터")]
    [SerializeField] private DailyRewardData rewardData;

    [SerializeField] DailyRewardButton[] dailyButtons;
    PlayerDataManager playerDataManager;

    [Header("UI 요소")]
    [SerializeField] TextMeshProUGUI dayText;
    [SerializeField] GameObject alreadyClaimedMessagePanel; // 이미 받았습니다. 설명
    [SerializeField] GameObject claimRewardMessagePanel; // 매일 매일 선물을 받아가세요. 설명

    [Header("보상 이펙트")]
    [SerializeField] private RectTransform rewardEffectPos;     // 이펙트 시작 위치
    [SerializeField] private GemCollectFX gemCollectFX;

    [Header("사운드")]
    [SerializeField] private AudioClip clipClaimReward;

    void OnEnable()
    {
        GameInitializer.OnGameInitialized += OnGameReady;
    }
    void OnDisable()
    {
        GameInitializer.OnGameInitialized -= OnGameReady;
    }

    void Start()
    {
        if (gemCollectFX == null)
            gemCollectFX = FindObjectOfType<GemCollectFX>();
    }

    // 게임 초기화 완료 시 호출됨
    void OnGameReady()
    {
        Logger.Log("[DailyRewardPanel] 게임 초기화 완료, UI 업데이트 시작");

        if (playerDataManager == null)
            playerDataManager = PlayerDataManager.Instance;

        UpdateUI();
    }

    void UpdateUI()
    {
        if (playerDataManager == null || rewardData == null)
        {
            Logger.LogError($"Player Data Manager 혹은 reward Data가 널입니다");
            return;
        }

        int currentDay = playerDataManager.GetConsecutiveDays();
        bool hasClaimed = playerDataManager.HasTakenDailyReward();

        // 보상 데이터 가져오기
        DailyRewardItem reward = rewardData.GetReward(currentDay);
        if (reward == null) return;

        // 날짜 표시
        if (dayText != null)
        {
            string dayKey = GetDayKey(currentDay);
            dayText.text = LocalizationManager.Game?.GetDayText(currentDay) ?? $"{currentDay}일차";
        }

        // 날짜 버튼 업데이트
        for (int i = 0; i < dailyButtons.Length; i++)
        {
            DailyRewardItem item = rewardData.GetReward(i + 1);
            int buttonDay = i + 1;

            dailyButtons[i].UpdateDailyRewardButton(item.coinReward, item.gemReward, currentDay, buttonDay, hasClaimed);

            // 해당 날짜의 버튼이고 아직 수령하지 않았다면 버튼 이벤트 등록
            if (currentDay == buttonDay && hasClaimed == false)
            {
                Button button = dailyButtons[i].GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(ClaimReward);
                }
                else
                {
                    Logger.LogError($"버튼 컴포넌트를 찾을 수 없습니다.");
                }
            }
        }

        alreadyClaimedMessagePanel.SetActive(hasClaimed); // 받았으면 받았습니다 메시지
        claimRewardMessagePanel.SetActive(!hasClaimed); // 안 받았으면 받아가세요 메시지
    }

    void ClaimReward()
    {
        if (playerDataManager == null || rewardData == null) return;

        // 이미 받았는지 확인
        if (playerDataManager.HasTakenDailyReward())
        {
            Logger.LogWarning("[DailyRewardPanel] 이미 오늘 출석 보상을 받았습니다.");
            return;
        }

        int currentDay = playerDataManager.GetConsecutiveDays();
        DailyRewardItem reward = rewardData.GetReward(currentDay);

        if (reward == null) return;

        // 사운드
        if (clipClaimReward != null)
            SoundManager.instance?.Play(clipClaimReward);

        // 코인 보상
        if (reward.coinReward > 0)
        {
            int currentCoin = playerDataManager.GetCurrentCoinNumber();
            playerDataManager.SetCoinNumberAsSilent(currentCoin + reward.coinReward);

            if (gemCollectFX != null && rewardEffectPos != null)
                gemCollectFX.PlayGemCollectFX(rewardEffectPos, reward.coinReward, false);
        }

        // 크리스탈 보상
        if (reward.gemReward > 0)
        {
            int currentGem = playerDataManager.GetCurrentCristalNumber();
            playerDataManager.SetCristalNumberAsSilent(currentGem + reward.gemReward);

            if (gemCollectFX != null && rewardEffectPos != null)
                gemCollectFX.PlayGemCollectFX(rewardEffectPos, reward.gemReward, true);
        }

        // 보상 수령 기록
        playerDataManager.SetHasTakenDailyReward(true);

        // UI 갱신
        UpdateUI();

        // 론치 패널 뱃지 갱신
        LaunchManager launchManager = FindObjectOfType<LaunchManager>();
        if (launchManager != null)
        {
            launchManager.UpdateDailyRewardBadge();
        }

        Logger.Log($"[DailyRewardPanel] {currentDay}일차 출석 보상 수령 완료!");
    }

    /// <summary>
    /// 날짜 텍스트 키 (다국어용)
    /// </summary>
    private string GetDayKey(int day)
    {
        return $"day_{day}";
    }
}