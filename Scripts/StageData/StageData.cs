using System;
using System.Collections.Generic;
using UnityEngine;

public enum StageEventType
{
    SpawnEnemy,
    SpawnEnemyGroup,
    SpawnSubBoss,
    SpawnEnemyBoss,
    SpawnEggBox,
    SpawnObject,
    WinStage,
    Incoming
}

[Serializable]
public class StageEvent
{
    public StageEventType eventType;

    public float time;
    public string message;

    public EnemyData enemyToSpawn;
    public GameObject objectToSpawn;
    public int count;
}

[CreateAssetMenu]
public class StageData : ScriptableObject
{
    public AudioClip stageMusic;
    public List<StageEvent> stageEvents;
}
