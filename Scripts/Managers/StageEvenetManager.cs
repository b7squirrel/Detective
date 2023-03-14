using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnItem {enemy, subBoss, bossSlime}

public class StageEvenetManager : MonoBehaviour
{
    [SerializeField] StageData stageData;
    Spawner spawner;
    SpawnItem spawnItem;

    StageTime stageTime;
    int eventIndexer;
    PlayerWinManager winManager;

    MusicManager musicManager;

    void Awake()
    {
        stageTime = GetComponent<StageTime>();
        spawner = FindObjectOfType<Spawner>();
        musicManager = FindObjectOfType<MusicManager>();
        musicManager.MusicOnStart = stageData.stageMusic; // Mucic manager.Start에서 Play하므로 this.awake에서 초기화 
    }

    void Start()
    {
        winManager = FindObjectOfType<PlayerWinManager>();
    }

    void Update()
    {
        if (eventIndexer >= stageData.stageEvents.Count)
            return;

        if (stageTime.time > stageData.stageEvents[eventIndexer].time)
        {
            
            switch (stageData.stageEvents[eventIndexer].eventType)
            {
                case StageEventType.SpawnEnemy:
                    for (int i = 0; i < stageData.stageEvents[eventIndexer].count; i++)
                    {
                        spawner.Spawn(stageData.stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.enemy);
                    }
                    break;

                case StageEventType.SpawnObject:
                    for (int i = 0; i < stageData.stageEvents[eventIndexer].count; i++)
                    {
                        spawner.SpawnObject(stageData.stageEvents[eventIndexer].objectToSpawn);
                    }
                    break;

                case StageEventType.WinStage:
                    WinStage();
                    break;

                case StageEventType.SpawnSubBoss:
                    SpawnSubBoss();
                    break;

                case StageEventType.SpawnEnemyBoss:
                    spawner.SpawnBoss(stageData.stageEvents[eventIndexer].enemyToSpawn);
                    break;
                    
                default:
                    break;
            }
            eventIndexer++;
        }
    }

    void WinStage()
    {
        winManager.Win();
    }
    void SpawnSubBoss()
    {
        spawner.Spawn(stageData.stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.subBoss);
    }
    void SpawnEnemyBoss()
    {
        spawner.Spawn(stageData.stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.bossSlime);
    }
}
