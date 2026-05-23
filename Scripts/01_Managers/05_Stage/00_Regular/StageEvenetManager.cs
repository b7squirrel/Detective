using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEvenetManager : MonoBehaviour, ISpawnController
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
    int subBossSpawnedCount; // 현재 스테이지에서 몇 번째 서브보스인지 추적

    MusicManager musicManager;
    public bool IsWinningStage { get; set; }
    Coroutine winStageCoroutine;
    bool winStageDone; // winStage 초기화가 update에서 일어나는데 반복해서 하지 않도록


    public void Init(TextAsset _stageTextData, EnemyData[] _enemyDatas, int _enemyNumForNextEvent, StageMusicType _stageMusicType)
    {
        enemyNumForNextEvent = _enemyNumForNextEvent;
        stageMusicType = _stageMusicType;

        readStageData = GetComponent<ReadStageData>();
        readStageData.Init(_stageTextData, _enemyDatas);

        subBossSpawnedCount = 0;

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
        if (onStopWatchEffect) return; // 스톱위치가 작동 중이면 이벤트 홀드
        if (IsWinningStage)
        {
            if (winStageDone == false)
            {
                winStageDone = true;
                WinStage();
            }
            return;
        }
        int enemyNums = spawner.GetCurrentEnemyNums();

        if (eventIndexer > stageEvents.Count - 1) return; // 이벤트를 다 소진하면(보스가 등장했다면) 더 이상 아무 일도 안 함.

        if (stageEvents[eventIndexer].eventType == StageEventType.Incoming
            || stageEvents[eventIndexer].eventType == StageEventType.SubBossIncoming)
        {
            forceSpawn = true;
            forceSpawnIndex = 5; // 적들이 몰려옵니다 또는 보스가 옵니다 경고 이후 5개의 이벤트는 강제로 진행
        }

        // 적들이 몰려옵니다 이벤트가 아닐 때만 적의 수에 따라 이벤트 실행
        if (forceSpawnIndex <= 0)
        {
            if (enemyNums > enemyNumForNextEvent) return; // 적이 너무 많이 남아 있다면 이벤트 없음.
        }


        if (isWaiting) return; // 이벤트가 진행 중일 동안은 다음 이벤트를 진행하지 않고 기다림.

        duration = stageEvents[eventIndexer].time;

        // Logger.Log(stageEvents[eventIndexer].eventType.ToString() + "Force Spawn = " + forceSpawn);

        StartCoroutine(TriggerEvent(duration, forceSpawn));
    }
    IEnumerator TriggerEvent(float _duration, bool _forceSpawn)
    {
        isWaiting = true;

        float remaining = _duration;
        while (remaining > 0f)
        {
            if (!onStopWatchEffect)
                remaining -= Time.deltaTime;
            yield return null;
        }

        // ⭐ 수정: IsPlayerDead 조건 제거
        isWaiting = false;

        bool isSubBoss = stageEvents[eventIndexer].eventType == StageEventType.SpawnSubBoss;
        GameManager.instance.progressionBar.UpdateProgressBar(isSubBoss);

        switch (stageEvents[eventIndexer].eventType)
        {
            case StageEventType.SpawnEnemy:
                for (int i = 0; i < stageEvents[eventIndexer].count; i++)
                    spawner.Spawn(stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.enemy, _forceSpawn);
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
            case StageEventType.SubBossIncoming:
                break;
        }

        eventIndexer++;
        forceSpawnIndex--;
        if (forceSpawnIndex <= 0) forceSpawn = false;

        SendStageEventIndex(eventIndexer);
    }

    void WinStage()
    {
        GameManager.instance.GetComponent<WinStage>().OpenPanel();
    }
    void SpawnSubBoss(bool _forceSpawn)
    {
        // ✅ 기존: spawner.Spawn(..., (int)SpawnItem.subBoss, ...)  → enemyPools 공유
        // ✅ 변경: SpawnSubBossEnemy() → subBossPools 사용
        spawner.SpawnSubBossEnemy(stageEvents[eventIndexer].enemyToSpawn, _forceSpawn);

        // ✅ 수정: EnemyData.Name 대신 Localization SO에서 접두사 포함 이름 가져오기
        string subBossName = GetLocalizedSubBossName();
        GameManager.instance.bossWarningPanel.Init(subBossName);

        subBossSpawnedCount++; // ✅ 추가
    }

    // ✅ 추가: 현재 스테이지와 서브보스 등장 순서로 로컬라이즈된 이름 반환
    string GetLocalizedSubBossName()
    {
        if (LocalizationManager.Game == null ||
            LocalizationManager.Game.stageBossName == null)
        {
            // fallback: EnemyData 이름 사용
            return stageEvents[eventIndexer].enemyToSpawn.Name;
        }

        PlayerDataManager pdm = FindObjectOfType<PlayerDataManager>();
        if (pdm == null)
            return stageEvents[eventIndexer].enemyToSpawn.Name;

        int stageIndex = pdm.GetCurrentStageNumber();
        int set = (stageIndex - 1) / 6;           // 세트 번호 (0, 1, 2, 3, 4)
        int nameIndex = set * 6 + subBossSpawnedCount; // 배열 인덱스

        string[] names = LocalizationManager.Game.stageBossName;
        if (nameIndex >= 0 && nameIndex < names.Length)
            return names[nameIndex];

        Debug.LogWarning($"[StageEvenetManager] stageBossName 범위 초과: {nameIndex} / {names.Length}");
        return stageEvents[eventIndexer].enemyToSpawn.Name;
    }

    void SpawnEnemyGroup(int number)
    {
        spawner.SpawnEnemyGroup(stageEvents[eventIndexer].enemyToSpawn, (int)SpawnItem.enemyGroup, number);
    }

    // 인터페이스 구현
    public void PauseSpawn(bool pause)
    {
        onStopWatchEffect = pause;
    }

    // 기존 메서드 유지
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
