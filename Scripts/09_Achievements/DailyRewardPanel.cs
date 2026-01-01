using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 일일 출석 보상 패널
/// </summary>
public class DailyRewardPanel : MonoBehaviour
{
    [Header("보상 데이터")]
    [SerializeField] private DailyRewardData rewardData;
    
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI dayText;           // "3일차"
    [SerializeField] private TextMeshProUGUI coinAmountText;    // 코인 개수
    [SerializeField] private TextMeshProUGUI gemAmountText;     // 크리스탈 개수
    [SerializeField] private Button claimButton;                // 보상 받기 버튼
    [SerializeField] private GameObject coinRewardGroup;        // 코인 보상 그룹
    [SerializeField] private GameObject gemRewardGroup;         // 크리스탈 보상 그룹
    [SerializeField] private GameObject specialBadge;           // 7일차 특별 표시
    [SerializeField] private GameObject alreadyClaimedPanel;    // "이미 받음" 표시
    
    [Header("보상 이펙트")]
    [SerializeField] private RectTransform rewardEffectPos;     // 이펙트 시작 위치
    [SerializeField] private GemCollectFX gemCollectFX;
    
    [Header("사운드")]
    [SerializeField] private AudioClip clipClaimReward;
    
    private PlayerDataManager playerDataManager;
    
    private void OnEnable()
    {
        if (playerDataManager == null)
            playerDataManager = PlayerDataManager.Instance;
        
        UpdateUI();
    }
    
    private void Start()
    {
        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(ClaimReward);
        }
        
        if (gemCollectFX == null)
            gemCollectFX = FindObjectOfType<GemCollectFX>();
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (playerDataManager == null || rewardData == null) return;
        
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
        
        // 코인 보상 표시
        if (coinRewardGroup != null)
        {
            bool hasCoin = reward.coinReward > 0;
            coinRewardGroup.SetActive(hasCoin);
            
            if (hasCoin && coinAmountText != null)
                coinAmountText.text = reward.coinReward.ToString();
        }
        
        // 크리스탈 보상 표시
        if (gemRewardGroup != null)
        {
            bool hasGem = reward.gemReward > 0;
            gemRewardGroup.SetActive(hasGem);
            
            if (hasGem && gemAmountText != null)
                gemAmountText.text = reward.gemReward.ToString();
        }
        
        // 특별 보상 뱃지
        if (specialBadge != null)
            specialBadge.SetActive(reward.isSpecial);
        
        // 버튼 상태
        if (claimButton != null)
            claimButton.interactable = !hasClaimed;
        
        // "이미 받음" 표시
        if (alreadyClaimedPanel != null)
            alreadyClaimedPanel.SetActive(hasClaimed);
    }
    
    /// <summary>
    /// 보상 수령
    /// </summary>
    private void ClaimReward()
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