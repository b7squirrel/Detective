using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PlayerPersistentUpgrades
{
    HP, //최대체력
    DAMAGE, // 공격력
    Armor, // 방어력
    ProjectileSpeed, // 투사체 속도
    ProjectileAmount, // 투사체 양
    MagnetRange, // 자석 범위
    MoveSpeed, // 이동 속도
    CoolDown, // 쿨타임
    Area, // 공격 범위
    knockBackChance // 낙백 확률
}

[Serializable]
public class PlayerUpgrades
{
    public PlayerPersistentUpgrades playerPersistanceUpgrades;
    public int level = 0;
    public int max_level = 10;
    public int costToUpgrades = 100;
}

[CreateAssetMenu]
public class DataContainer : ScriptableObject
{
    public int coins;
    public int gold;
    public int lightning;
    public int kills;
    public List<PlayerUpgrades> upgrades;
    public List<bool> stageCompletion;
    
    public void StageComplete(int i)
    {
        stageCompletion[i] = true;
    }

    public int GetUpgradeLevel(PlayerPersistentUpgrades persistantUpgrade)
    {
        return upgrades[(int)persistantUpgrade].level;
    }
}
