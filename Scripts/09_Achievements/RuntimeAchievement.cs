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

        // 저장 키 결정 (일일 퀘스트 vs 영구 업적)
        string progressKey = GetProgressKey();
        string completeKey = GetCompleteKey();
        string rewardKey = GetRewardKey();

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

    // ⭐ 저장 키 생성 (일일 퀘스트는 DAILY_QUEST_ 접두사)
    public string GetProgressKey()
    {
        if (original.isDailyQuest)
            return "DAILY_QUEST_" + original.id + "_PROGRESS";
        else
            return "ACH_PROGRESS_" + original.id;
    }
    
    public string GetCompleteKey()
    {
        if (original.isDailyQuest)
            return "DAILY_QUEST_" + original.id + "_COMPLETED";
        else
            return "ACH_" + original.id;
    }
    
    public string GetRewardKey()
    {
        if (original.isDailyQuest)
            return "DAILY_QUEST_" + original.id + "_REWARDED";
        else
            return "ACH_REWARD_" + original.id;
    }
    
    // 다국어 제목/설명 가져오기
    public string GetTitle() => original.GetLocalizedTitle();
    public string GetDescription() => original.GetLocalizedDescription();
}