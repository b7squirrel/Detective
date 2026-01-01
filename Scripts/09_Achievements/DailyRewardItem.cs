using UnityEngine;

/// <summary>
/// 하루치 출석 보상 데이터
/// </summary>
[System.Serializable]
public class DailyRewardItem
{
    [Header("보상 날짜")]
    [Tooltip("1~7일차")]
    public int day;
    
    [Header("코인 보상")]
    public int coinReward;
    
    [Header("크리스탈 보상")]
    public int gemReward;
    
    [Header("특별 보상 여부")]
    [Tooltip("7일차 등 특별한 날")]
    public bool isSpecial;
}

/// <summary>
/// 7일 출석 보상 데이터 (ScriptableObject)
/// </summary>
[CreateAssetMenu(fileName = "DailyRewardData", menuName = "Daily System/Daily Reward Data")]
public class DailyRewardData : ScriptableObject
{
    [Header("7일 출석 보상")]
    [Tooltip("7개 항목 필수")]
    public DailyRewardItem[] rewards = new DailyRewardItem[7];
    
    /// <summary>
    /// 특정 날짜의 보상 가져오기
    /// </summary>
    public DailyRewardItem GetReward(int day)
    {
        if (day < 1 || day > 7)
        {
            Logger.LogError($"[DailyRewardData] 잘못된 day 값: {day} (1~7만 가능)");
            return null;
        }
        
        return rewards[day - 1]; // 배열은 0부터 시작
    }
    
    /// <summary>
    /// 모든 보상의 유효성 검사
    /// </summary>
    [ContextMenu("Validate Rewards")]
    void ValidateRewards()
    {
        if (rewards.Length != 7)
        {
            Logger.LogError("[DailyRewardData] 보상은 정확히 7개여야 합니다!");
            return;
        }
        
        for (int i = 0; i < 7; i++)
        {
            if (rewards[i] == null)
            {
                Logger.LogError($"[DailyRewardData] {i + 1}일차 보상이 null입니다!");
                continue;
            }
            
            if (rewards[i].day != i + 1)
            {
                Logger.LogWarning($"[DailyRewardData] {i + 1}일차의 day 필드가 {rewards[i].day}로 설정되어 있습니다. {i + 1}로 수정하세요.");
            }
            
            Logger.Log($"[DailyRewardData] {i + 1}일차: 코인 {rewards[i].coinReward}, 크리스탈 {rewards[i].gemReward}");
        }
        
        Logger.Log("[DailyRewardData] 검증 완료!");
    }
}