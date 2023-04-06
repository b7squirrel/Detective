using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnItem {enemy, subBoss, enemyGroup, bossSlime}

public class StageEvenetManager : MonoBehaviour
{
    [SerializeField] List <StageEvent> stageEvents;
    [SerializeField] AudioClip stageMusic;
    ReadStageData readStageData;
    Spawner spawner;
    SpawnItem spawnItem;

    StageTime stageTime;
    int eventIndexer;

    MusicManager musicManager;
    public bool IsWinningStage {get; set;}
    Coroutine winStageCoroutine;

    void Awake()
    {
        readStageData = GetComponent<ReadStageData>();
        foreach (var item in readStageData.GetStageEventsList())
        {
            this.stageEvents.Add(item);
        } 
        stageTime = GetComponent<StageTime>();
        spawner = FindObjectOfType<Spawner>();
        musicManager = FindObjectOfType<MusicManager>();
        musicManager.MusicOnStart = stageMusic; // Mucic manager.Start에서 Play하므로 this.awake에서 초기화 
        IsWinningStage = false;
        winStageCoroutine = null;
    }

    void Update()
    {
        if (IsWinningStage)
        {
            if (winStageCoroutine == null)
            {
                winStageCoroutine = StartCoroutine(WinStage());
            }
            return;
        }
        if (eventIndexer >= stageEvents.Count)
            return;

        if (stageTime.time > stageEvents[eventIndexer].time)
        {

            switch (stageEvents[eventIndexer].eventType)
            {
                case StageEventType.SpawnEnemy:
                    for (int i = 0; i < stageEvents[eventIndexer].count; i++)
                    {
                        spawner.Spawn(stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.enemy);
                    }
                    break;

                case StageEventType.SpawnEnemyGroup:
                    SpawnEnemyGroup(stageEvents[eventIndexer].count);
                    break;

                case StageEventType.SpawnObject:
                    for (int i = 0; i < stageEvents[eventIndexer].count; i++)
                    {
                        spawner.SpawnObject(stageEvents[eventIndexer].objectToSpawn);
                    }
                    break;

                case StageEventType.WinStage:
                    WinStage();
                    break;

                case StageEventType.SpawnSubBoss:
                    SpawnSubBoss();
                    break;

                case StageEventType.SpawnEnemyBoss:
                    spawner.SpawnBoss(stageEvents[eventIndexer].enemyToSpawn);
                    break;
                    
                default:
                    break;
            }
            eventIndexer++;
        }
    }

    IEnumerator WinStage()
    {
        yield return new WaitForSeconds(7f);
        GameManager.instance.GetComponent<WinStage>().OpenPanel(); 
    }
    void SpawnSubBoss()
    {
        spawner.Spawn(stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.subBoss);
    }
    void SpawnEnemyGroup(int number)
    {
        spawner.SpawnEnemyGroup(stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.enemyGroup, number);
    }
}
