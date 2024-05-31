using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnItem {enemy, subBoss, enemyGroup, bossSlime}

public class StageEvenetManager : MonoBehaviour
{
    [SerializeField] List<StageEvent> stageEvents;
    [SerializeField] AudioClip stageMusic;
    [SerializeField] int enemyNumForNextEvent; // 다음 이벤트를 시작하기 위한 최대 적 수
    ReadStageData readStageData;
    Spawner spawner;

    float duration;
    bool isWaiting;
    bool onStopWatchEffect;

    int eventIndexer;

    MusicManager musicManager;
    public bool IsWinningStage { get; set; }
    Coroutine winStageCoroutine;

    void Start()
    {
        readStageData = GetComponent<ReadStageData>();
        foreach (var item in readStageData.GetStageEventsList())
        {
            this.stageEvents.Add(item);
        }
        spawner = FindObjectOfType<Spawner>();
        musicManager = FindObjectOfType<MusicManager>();
        musicManager.InitBGM(stageMusic);
        IsWinningStage = false;
        winStageCoroutine = null;
    }

    void Update()
    {
        //if (onStopWatchEffect) return; // 스톱위치가 작동 중이면 이벤트 홀드

        if (IsWinningStage)
        {
            if (winStageCoroutine == null)
            {
                winStageCoroutine = StartCoroutine(WinStage());
            }
            return;
        }

        if (spawner.GetCurrentEnemyNums() > enemyNumForNextEvent) return; // 적이 너무 많이 남아 있다면 이벤트 없음.
        if (eventIndexer > stageEvents.Count - 1) return; // 이벤트를 다 소진하면(보스가 등장했다면) 더 이상 아무 일도 안 함.

        if (isWaiting) return;
        duration = stageEvents[eventIndexer].time;
        StartCoroutine(TriggerEvent(duration));
    }
    IEnumerator TriggerEvent(float _duration)
    {
        isWaiting = true;
        yield return new WaitForSeconds(_duration);
        isWaiting = false;

        if (onStopWatchEffect) yield break; // 스톱위치가 작동 중이면 이벤트 홀드

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

    public void PasueStageEvent(bool _pause)
    {
        onStopWatchEffect = _pause;
    }
}
