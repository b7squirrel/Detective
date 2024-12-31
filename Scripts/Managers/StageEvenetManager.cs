using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEvenetManager : MonoBehaviour
{
    [SerializeField] List<StageEvent> stageEvents;
    [SerializeField] int enemyNumForNextEvent; // 다음 이벤트를 시작하기 위한 최대 적 수
    [SerializeField] StageMusicType stageMusicType;

    ReadStageData readStageData;
    Spawner spawner;

    WallManager wallManager;

    float duration;
    bool isWaiting;
    bool onStopWatchEffect;

    int eventIndexer;
    bool forceSpawn;
    int forceSpawnIndex;

    int subBossNums;

    MusicManager musicManager;
    public bool IsWinningStage { get; set; }
    Coroutine winStageCoroutine;

    public void Init(TextAsset _stageTextData, EnemyData[] _enemyDatas, int _enemyNumForNextEvent, StageMusicType _stageMusicType)
    {
        enemyNumForNextEvent = _enemyNumForNextEvent;
        stageMusicType = _stageMusicType;

        readStageData = GetComponent<ReadStageData>();
        readStageData.Init(_stageTextData, _enemyDatas);

        List<int> segmentsLengths = new();
        int length = 0;
        foreach (var item in readStageData.GetStageEventsList())
        {
            this.stageEvents.Add(item);
            length++;

            if (item.eventType == StageEventType.SpawnSubBoss)
            {
                subBossNums++;
                segmentsLengths.Add(length);
                length = 0;
            }
            else if (item.eventType == StageEventType.SpawnEnemyBoss)
            {
                segmentsLengths.Add(length);
            }
        }
        spawner = FindObjectOfType<Spawner>();

        IsWinningStage = false;
        winStageCoroutine = null;

        GameManager.instance.musicCreditManager.Init();
        GameManager.instance.progressionBar.Init(subBossNums, stageEvents.Count, segmentsLengths);
        GameManager.instance.progressionBar.UpdateProgressBar(false);

        wallManager = FindObjectOfType<WallManager>();
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
        int enemyNums = spawner.GetCurrentEnemyNums();

        if (eventIndexer > stageEvents.Count - 1) return; // 이벤트를 다 소진하면(보스가 등장했다면) 더 이상 아무 일도 안 함.

        if (stageEvents[eventIndexer].eventType == StageEventType.Incoming)
        {
            forceSpawn = true;
            forceSpawnIndex = 5; // 적들이 몰려옵니다 경고 이후 5개의 이벤트는 강제로 진행
        }

        // 적들이 몰려옵니다 이벤트가 아닐 때만 적의 수에 따라 이벤트 실행
        if (forceSpawnIndex <= 0)
        {
            if (enemyNums > enemyNumForNextEvent) return; // 적이 너무 많이 남아 있다면 이벤트 없음.
        }


        if (isWaiting) return; // 이벤트가 진행 중일 동안은 다음 이벤트를 진행하지 않고 기다림.

        duration = stageEvents[eventIndexer].time;

        Debug.Log(stageEvents[eventIndexer].eventType.ToString() + forceSpawn);

        StartCoroutine(TriggerEvent(duration, forceSpawn));
    }
    IEnumerator TriggerEvent(float _duration, bool _forceSpawn)
    {
        isWaiting = true;
        yield return new WaitForSeconds(_duration);

        if (GameManager.instance.IsPlayerDead == false)
            isWaiting = false;

        if (onStopWatchEffect) yield break; // 스톱위치가 작동 중이면 이벤트 홀드

        bool isSubBoss = stageEvents[eventIndexer].eventType == StageEventType.SpawnSubBoss ? true : false;
        GameManager.instance.progressionBar.UpdateProgressBar(isSubBoss);

        switch (stageEvents[eventIndexer].eventType)
        {
            case StageEventType.SpawnEnemy:
                for (int i = 0; i < stageEvents[eventIndexer].count; i++)
                {
                    spawner.Spawn(stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.enemy, _forceSpawn);
                }
                break;

            case StageEventType.SpawnEnemyGroup:
                SpawnEnemyGroup(stageEvents[eventIndexer].count);
                break;

            case StageEventType.SpawnSubBoss:
                SpawnSubBoss(_forceSpawn);
                break;

            case StageEventType.SpawnEnemyBoss:
                spawner.SpawnBoss(stageEvents[eventIndexer].enemyToSpawn);
                break;

            case StageEventType.Incoming:
                GameManager.instance.GetComponent<IncomingWarningPanel>().Init();
                break;
            default:
                break;
        }

        eventIndexer++;
        forceSpawnIndex--;
        if (forceSpawnIndex <= 0) forceSpawn = false;

        // ???源?
        SendStageEventIndex(eventIndexer);
    }

    IEnumerator WinStage()
    {
        yield return new WaitForSeconds(3f);
        GameManager.instance.GetComponent<WinStage>().OpenPanel();
    }
    void SpawnSubBoss(bool _forceSpawn)
    {
        spawner.Spawn(stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.subBoss, _forceSpawn);
    }
    
    void SpawnEnemyGroup(int number)
    {
        spawner.SpawnEnemyGroup(stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.enemyGroup, number);
    }

    public void PasueStageEvent(bool _pause)
    {
        onStopWatchEffect = _pause;
    }
    public StageMusicType GetStageMusicType()
    {
        return stageMusicType;
    }

    #region ???源?
    void SendStageEventIndex(int _index)
    {
        DebugManager debugManager = FindObjectOfType<DebugManager>();
        debugManager.SetStageEventIndex(_index);
    }
    #endregion
}
