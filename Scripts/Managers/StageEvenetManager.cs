using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEvenetManager : MonoBehaviour
{
    [SerializeField] StageData stageData;
    Spawner spawner;

    StageTime stageTime;
    int eventIndexer;
    PlayerWinManager winManager;

    void Awake()
    {
        stageTime = GetComponent<StageTime>();
        spawner = FindObjectOfType<Spawner>();
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
                        spawner.Spawn(stageData.stageEvents[eventIndexer].enemyToSpawn, 0);
                    }
                    break;

                case StageEventType.SpawnObject:
                    for (int i = 0; i < stageData.stageEvents[eventIndexer].count; i++)
                    {
                        spawner.SpawnObject(GameManager.instance.player.transform.position, 
                        stageData.stageEvents[eventIndexer].objectToSpawn);
                    }
                    break;

                case StageEventType.WinStage:
                    WinStage();
                    break;

                case StageEventType.SpawnEnemyBoss:
                    SpawnEnemyBoss();
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
    void SpawnEnemyBoss()
    {
        spawner.Spawn(stageData.stageEvents[eventIndexer].enemyToSpawn, 1);
        
    }
}
