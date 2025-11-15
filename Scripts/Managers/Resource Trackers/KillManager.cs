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
    public void UpdateCurrentKills(EnemyType enemyType, bool isSubBoss, bool isBoss)
    {
        currentKills++;

        OnSlimeKilled(enemyType, isSubBoss, isBoss);

        OnKill?.Invoke();
    }

    /// <summary>
    /// 도전 과제. 슬라임 킬
    /// </summary>
    void OnSlimeKilled(EnemyType enemyType, bool isSubBoss, bool isBoss)
    {
        AchievementManager.Instance.AddProgressByID("KILL_100", 1);
        AchievementManager.Instance.AddProgressByID("SLIME_500", 1);
        AchievementManager.Instance.AddProgressByID("TEST_SLIME_10", 1);

        if (enemyType == EnemyType.Ranged) AchievementManager.Instance.AddProgressByID("RANGED_200", 1);
        if (isBoss) AchievementManager.Instance.AddProgressByID("FIRST_BOSS", 1);
        if (isSubBoss) AchievementManager.Instance.AddProgressByID("MIDBOSS_5", 1);
    }
    public int GetCurrentKills() => currentKills;
}