using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnItem {enemy, subBoss, enemyGroup, bossSlime}

public class StageEvenetManager : MonoBehaviour
{
    [SerializeField] List <StageEvent> stageEvents;
    [SerializeField] AudioClip stageMusic;
    [SerializeField] float eventFrequency; // 몇 초 간격으로 이벤트를 발동시키려고 할 것인지
    [SerializeField] int enemyNumForNextEvent; // 다음 이벤트를 시작하기 위한 최대 적 수
    ReadStageData readStageData;
    Spawner spawner;
    SpawnItem spawnItem;

    float nextEventTime;

    StageTime stageTime;
    int eventIndexer;

    MusicManager musicManager;
    public bool IsWinningStage {get; set;}
    Coroutine winStageCoroutine;

    void Start()
    {
        readStageData = GetComponent<ReadStageData>();
        foreach (var item in readStageData.GetStageEventsList())
        {
            this.stageEvents.Add(item);
        } 
        stageTime = GetComponent<StageTime>();
        spawner = FindObjectOfType<Spawner>();
        musicManager = FindObjectOfType<MusicManager>();
        musicManager.InitBGM(stageMusic); 
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

        if (Time.time >= nextEventTime)
        {
            //nextEventTime += 2f;

            if (spawner.GetCurrentEnemyNums() > enemyNumForNextEvent) return; // 적이 너무 많이 남아 있다면 이벤트 없음.
            if (eventIndexer > stageEvents.Count - 1) return; // 이벤트를 다 소진하면(보스가 등장했다면) 더 이상 아무 일도 안 함.

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
        yield return new WaitForSeconds(3f);
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
