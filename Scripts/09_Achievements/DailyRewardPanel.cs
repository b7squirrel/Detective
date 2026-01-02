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
    [SerializeField] GameObject alreadyClaimedMessagePanel;
    [SerializeField] GameObject claimRewardMessagePanel;

    [Header("보상 이펙트")]
    [SerializeField] private RectTransform rewardEffectPos;
    [SerializeField] private GemCollectFX gemCollectFX;

    [Header("사운드")]
    [SerializeField] private AudioClip clipClaimReward;

    // ⭐ 디버그용 플래그
    private bool hasSubscribed = false;
    private bool hasGameInitialized = false;

    void OnEnable()
    {
        Debug.Log("[DailyRewardPanel] OnEnable 시작");
        
        if (!hasSubscribed)
        {
            GameInitializer.OnGameInitialized += OnGameReady;
            hasSubscribed = true;
            Debug.Log("[DailyRewardPanel] 이벤트 구독 완료");
        }
        
        // ⭐ 이미 초기화되었다면 바로 UpdateUI
        if (GameInitializer.IsInitialized && !hasGameInitialized)
        {
            Debug.Log("[DailyRewardPanel] GameInitializer 이미 초기화됨, 바로 UpdateUI 호출");
            OnGameReady();
        }
    }

    void OnDisable()
    {
        Debug.Log("[DailyRewardPanel] OnDisable");
        GameInitializer.OnGameInitialized -= OnGameReady;
        hasSubscribed = false;
    }

    void Start()
    {
        Debug.Log("[DailyRewardPanel] Start");
        
        if (gemCollectFX == null)
            gemCollectFX = FindObjectOfType<GemCollectFX>();
    }

    void OnGameReady()
    {
        Debug.Log("[DailyRewardPanel] ⭐ OnGameReady 호출됨!");
        hasGameInitialized = true;
        
        if (playerDataManager == null)
        {
            playerDataManager = PlayerDataManager.Instance;
            Debug.Log($"[DailyRewardPanel] PlayerDataManager 연결: {playerDataManager != null}");
        }
        
        Debug.Log($"[DailyRewardPanel] rewardData: {rewardData != null}");
        Debug.Log($"[DailyRewardPanel] dailyButtons: {dailyButtons?.Length ?? 0}개");
        Debug.Log($"[DailyRewardPanel] dayText: {dayText != null}");
        
        UpdateUI();
    }

    void UpdateUI()
    {
        Debug.Log("[DailyRewardPanel] UpdateUI 시작");
        
        if (playerDataManager == null)
        {
            Debug.LogError("[DailyRewardPanel] X playerDataManager가 null!");
            return;
        }
        
        if (rewardData == null)
        {
            Debug.LogError("[DailyRewardPanel] X rewardData가 null!");
            return;
        }

        int currentDay = playerDataManager.GetConsecutiveDays();
        bool hasClaimed = playerDataManager.HasTakenDailyReward();

        Debug.Log($"[DailyRewardPanel] 현재 날짜: {currentDay}일차");
        Debug.Log($"[DailyRewardPanel] 수령 여부: {hasClaimed}");

        // 보상 데이터 가져오기
        DailyRewardItem reward = rewardData.GetReward(currentDay);
        if (reward == null)
        {
            Debug.LogError($"[DailyRewardPanel] X {currentDay}일차 보상 데이터가 null!");
            return;
        }
        
        Debug.Log($"[DailyRewardPanel] 보상: 코인 {reward.coinReward}, 크리스탈 {reward.gemReward}");

        // 날짜 표시
        if (dayText != null)
        {
            string text = LocalizationManager.Game?.GetDayText(currentDay) ?? $"{currentDay}일차";
            dayText.text = text;
            Debug.Log($"[DailyRewardPanel] dayText 설정: '{text}'");
        }
        else
        {
            Debug.LogError("[DailyRewardPanel] X dayText가 null!");
        }

        // 날짜 버튼 업데이트
        if (dailyButtons == null || dailyButtons.Length == 0)
        {
            Debug.LogError("[DailyRewardPanel] X dailyButtons가 비어있음!");
            return;
        }
        
        Debug.Log($"[DailyRewardPanel] 버튼 업데이트 시작 (총 {dailyButtons.Length}개)");
        
        for (int i = 0; i < dailyButtons.Length; i++)
        {
            if (dailyButtons[i] == null)
            {
                Debug.LogError($"[DailyRewardPanel] X dailyButtons[{i}]가 null!");
                continue;
            }
            
            DailyRewardItem item = rewardData.GetReward(i + 1);
            int buttonDay = i + 1;

            Debug.Log($"[DailyRewardPanel] 버튼 {buttonDay} 업데이트: 코인 {item.coinReward}, 크리스탈 {item.gemReward}");
            
            dailyButtons[i].UpdateDailyRewardButton(
                item.coinReward, 
                item.gemReward, 
                currentDay, 
                buttonDay, 
                hasClaimed
            );

            // 해당 날짜의 버튼이고 아직 수령하지 않았다면 버튼 이벤트 등록
            if (currentDay == buttonDay && hasClaimed == false)
            {
                Button button = dailyButtons[i].GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(ClaimReward);
                    Debug.Log($"[DailyRewardPanel] ✓ {buttonDay}일차 버튼 클릭 이벤트 등록!");
                }
                else
                {
                    Debug.LogError($"[DailyRewardPanel] X {buttonDay}일차 Button 컴포넌트 없음!");
                }
            }
        }

        // 메시지 패널
        if (alreadyClaimedMessagePanel != null)
        {
            alreadyClaimedMessagePanel.SetActive(hasClaimed);
            Debug.Log($"[DailyRewardPanel] alreadyClaimedMessagePanel: {hasClaimed}");
        }
        else
        {
            Debug.LogError("[DailyRewardPanel] X alreadyClaimedMessagePanel이 null!");
        }
        
        if (claimRewardMessagePanel != null)
        {
            claimRewardMessagePanel.SetActive(!hasClaimed);
            Debug.Log($"[DailyRewardPanel] claimRewardMessagePanel: {!hasClaimed}");
        }
        else
        {
            Debug.LogError("[DailyRewardPanel] X claimRewardMessagePanel이 null!");
        }
        
        Debug.Log("[DailyRewardPanel] ✓ UpdateUI 완료!");
    }

    void ClaimReward()
    {
        Debug.Log("[DailyRewardPanel] ⭐ ClaimReward 호출!");
        
        if (playerDataManager == null || rewardData == null)
        {
            Debug.LogError("[DailyRewardPanel] X ClaimReward: manager가 null!");
            return;
        }

        if (playerDataManager.HasTakenDailyReward())
        {
            Debug.LogWarning("[DailyRewardPanel] 이미 오늘 출석 보상을 받았습니다.");
            return;
        }

        int currentDay = playerDataManager.GetConsecutiveDays();
        DailyRewardItem reward = rewardData.GetReward(currentDay);

        if (reward == null)
        {
            Debug.LogError("[DailyRewardPanel] X ClaimReward: reward가 null!");
            return;
        }

        Debug.Log($"[DailyRewardPanel] {currentDay}일차 보상 수령 시작");

        // 사운드
        if (clipClaimReward != null)
            SoundManager.instance?.Play(clipClaimReward);

        // 코인 보상
        if (reward.coinReward > 0)
        {
            int currentCoin = playerDataManager.GetCurrentCoinNumber();
            playerDataManager.SetCoinNumberAsSilent(currentCoin + reward.coinReward);
            Debug.Log($"[DailyRewardPanel] 코인 {reward.coinReward} 지급");

            if (gemCollectFX != null && rewardEffectPos != null)
                gemCollectFX.PlayGemCollectFX(rewardEffectPos, reward.coinReward, false);
        }

        // 크리스탈 보상
        if (reward.gemReward > 0)
        {
            int currentGem = playerDataManager.GetCurrentCristalNumber();
            playerDataManager.SetCristalNumberAsSilent(currentGem + reward.gemReward);
            Debug.Log($"[DailyRewardPanel] 크리스탈 {reward.gemReward} 지급");

            if (gemCollectFX != null && rewardEffectPos != null)
                gemCollectFX.PlayGemCollectFX(rewardEffectPos, reward.gemReward, true);
        }

        // 보상 수령 기록
        playerDataManager.SetHasTakenDailyReward(true);
        Debug.Log("[DailyRewardPanel] 보상 수령 기록 저장");

        // UI 갱신
        UpdateUI();

        // 론치 패널 뱃지 갱신
        LaunchManager launchManager = FindObjectOfType<LaunchManager>();
        if(launchManager != null)
        {
            launchManager.UpdateDailyRewardBadge();
        }

        Debug.Log($"[DailyRewardPanel] ✓ {currentDay}일차 출석 보상 수령 완료!");
    }

    private string GetDayKey(int day)
    {
        return $"day_{day}";
    }
}