using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PlayerPersistentUpgrades
{
    HP,
    DAMAGE
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
