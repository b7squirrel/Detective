using UnityEngine;

[System.Serializable]
public class DailyRewardItem
{
    public int day;
    public int coinReward;
    public int gemReward;
    public bool isSpecial;
    
    public DailyRewardItem()
    {
        day = 0;
        coinReward = 0;
        gemReward = 0;
        isSpecial = false;
    }
}

[CreateAssetMenu(fileName = "DailyRewardData", menuName = "Daily System/Daily Reward Data")]
public class DailyRewardData : ScriptableObject
{
    [Header("7일 출석 보상")]
    public DailyRewardItem[] rewards = new DailyRewardItem[7];
    
    void OnEnable()
    {
        if (rewards == null || rewards.Length != 7)
        {
            rewards = new DailyRewardItem[7];
        }
        
        for (int i = 0; i < 7; i++)
        {
            if (rewards[i] == null)
            {
                rewards[i] = new DailyRewardItem();
                rewards[i].day = i + 1;
            }
        }
    }
    
    public DailyRewardItem GetReward(int day)
    {
        if (day < 1 || day > 7)
        {
            Debug.LogError($"[DailyRewardData] 잘못된 day 값: {day}");
            return null;
        }
        
        return rewards[day - 1];
    }
}