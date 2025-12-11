using System;
using UnityEngine;

public class RuntimeAchievement
{
    public AchievementSO original;
    public int progress;
    public bool isCompleted;
    public bool isRewarded;
    
    public event System.Action<RuntimeAchievement> OnProgressChanged;
    public event System.Action<RuntimeAchievement> OnCompleted;
    
    public RuntimeAchievement(AchievementSO so)
    {
        original = so;
        progress = PlayerPrefs.GetInt("ACH_PROGRESS_" + so.id, 0);
        isCompleted = PlayerPrefs.GetInt("ACH_" + so.id, 0) == 1;
        isRewarded = PlayerPrefs.GetInt("ACH_REWARD_" + so.id, 0) == 1;
    }
    
    public void AddProgress(int amount)
    {
        if (isCompleted) return;
        
        progress += amount;
        OnProgressChanged?.Invoke(this);
        
        if (progress >= original.targetValue)
        {
            isCompleted = true;
            OnCompleted?.Invoke(this);
        }
    }
    
    public void Reward()
    {
        isRewarded = true;
    }
    
    // 다국어 제목/설명 가져오기
    public string GetTitle() => original.GetLocalizedTitle();
    public string GetDescription() => original.GetLocalizedDescription();
}