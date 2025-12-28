using System;
using UnityEngine;

[Serializable]
public class StageContents
{
    [Header("Stage Number")]
    public string Name;

    [Header("Stage Events")]
    public int enemyNumForNextEvent;
    public StageMusicType stageMusicType;

    [Header("Stage Data")]
    public TextAsset stageDataText;

    [Header("Enemy Data")]
    public EnemyData[] enemyData;

    [Header("Stage Asset Data")]
    public GameObject[] enemies;
    public GameObject bossPrefab;
    public GameObject[] effects;
    public GameObject[] bossEffects;

    [Header("Spawn Gem On Start")]
    public int numbersOfGemToSpawn;
    public GameObject gemToSpawn;
    public float innerRadius;
    public float outerRadius;

    [Header("Chest")]
    public GameObject chestPrefab;
    public float innerRadiusForChest;
    public float outerRadiusForChest;

    [Header("Wall")]
    public Vector2[] startPositions = new Vector2[4];
}
