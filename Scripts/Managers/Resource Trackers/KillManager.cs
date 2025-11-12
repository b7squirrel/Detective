using System;
using UnityEngine;

public class KillManager : MonoBehaviour
{
    int currentKills;
    public event Action OnKill;

    void Start()
    {
        OnKill?.Invoke();
    }
    public void UpdateCurrentKills()
    {
        currentKills++;

        OnSlimeKilled();

        OnKill?.Invoke();
    }

    /// <summary>
    /// 도전 과제. 슬라임 킬
    /// </summary>
    void OnSlimeKilled()
    {
        AchievementManager.Instance.AddProgress("KILL_100", 1);
        AchievementManager.Instance.AddProgress("Kill_1000", 1);
        AchievementManager.Instance.AddProgress("Kill_10000", 1);
        AchievementManager.Instance.AddProgress("TEST_SLIME_10", 1);
    }
    public int GetCurrentKills() => currentKills;
}