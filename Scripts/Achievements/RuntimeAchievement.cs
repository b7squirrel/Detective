using System;
using UnityEngine;

[Serializable]
public class RuntimeAchievement
{
    public AchievementSO original;   // SO 원본 참조
    public int progress;
    public bool isCompleted;
    public bool isRewarded;

    // 이벤트
    public event Action<RuntimeAchievement> OnProgressChanged;
    public event Action<RuntimeAchievement> OnCompleted;

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
        progress = Mathf.Clamp(progress, 0, original.targetValue);

        OnProgressChanged?.Invoke(this);

        if (progress >= original.targetValue && !isCompleted)
        {
            isCompleted = true;
            OnCompleted?.Invoke(this);
        }
    }

    public void Reward()
    {
        if (!isCompleted || isRewarded) return;

        isRewarded = true;
    }
}