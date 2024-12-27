using System;
using UnityEngine;

[Serializable]
public class StageContents
{
    [Header("Stage Time")]
    public float WallDuration;

    [Header("Stage Events")]
    public int enemyNumForNextEvent;
    public StageMusicType stageMusicType;

    [Header("Egg Spawn Time")]
    public float[] eggSpawnTimes;

    [Header("Stage Data")]
    public TextAsset stageDataText;

    [Header("Enemy Data")]
    public EnemyData[] enemyData;

    [Header("Stage Asset Data")]
    public GameObject[] enemies;
    public GameObject bossPrefab;
    public GameObject[] effects;
    public GameObject[] bossEffects;

    [Header("Wall")]
    public Vector2[] startPositions = new Vector2[4];
}
