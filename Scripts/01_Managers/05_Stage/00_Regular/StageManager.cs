using System.Collections;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public StageContents[] stageContents;

    StageEvenetManager stageEventManager;
    StageAssetManager stageAssetManager;
    SpawnGemsOnStart spawnGemsOnStart;
    PoolManager poolManager;
    FieldItemSpawner fieldItemSpawner;
    WallManager wallManager;
    StageGroundManager stageGroundManager;
    StageGroundEffectManager stageGroundEffectManager;
    PlayerDataManager playerDataManager;
    TimeWaveUI timeWaveUI;
    StageTime stageTime;

    void Awake()
    {
        stageEventManager = GetComponent<StageEvenetManager>();
        stageAssetManager = GetComponent<StageAssetManager>();
        spawnGemsOnStart = GetComponent<SpawnGemsOnStart>();
        poolManager = FindObjectOfType<PoolManager>();
        fieldItemSpawner = FindObjectOfType<FieldItemSpawner>();
        wallManager = FindObjectOfType<WallManager>();
        stageGroundManager = GetComponent<StageGroundManager>();
        stageGroundEffectManager = GetComponent<StageGroundEffectManager>();
    }

    void Start()
    {
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        timeWaveUI = FindObjectOfType<TimeWaveUI>();
        stageTime = FindObjectOfType<StageTime>();

        int currentStageNum = playerDataManager.GetCurrentStageNumber();
        StageContents contents = stageContents[currentStageNum - 1];

        wallManager.SetWallSize(contents.startPositions);

        poolManager.InitPools();

        stageEventManager.Init(contents.stageDataText,
                                       contents.enemyData, contents.enemyNumForNextEvent,
                                       contents.stageMusicType);

        spawnGemsOnStart.InitGemData(contents.gemToSpawn,
                                                   contents.numbersOfGemToSpawn,
                                                   contents.innerRadius, contents.outerRadius);
        spawnGemsOnStart.InitChestData(contents.chestPrefab,
                                                    contents.innerRadiusForChest, contents.outerRadiusForChest);
        spawnGemsOnStart.GenGemsAndChest();

        stageAssetManager.Init(contents.enemies, contents.bossPrefab, contents.effects, contents.bossEffects);

        stageGroundManager.InitGround(contents.stageGroundType);
        stageGroundEffectManager.Init(contents.stageGroundType);

        // poolManager가 stageAssetManager를 참조하니까 먼저 초기화하면 안 됨
        poolManager.InitEnemyPools();

        StartCoroutine(UpdateTimeUI());
    }


    IEnumerator UpdateTimeUI()
    {
        // 스테이지
        int stageNum = playerDataManager.GetCurrentStageNumber();
        timeWaveUI.InitStageUI(stageNum.ToString());

        // 시간
        while (true)
        {
            if (timeWaveUI != null)
            {
                float currentTime = stageTime.GetElapsedTime();
                string timeFormatted = new GeneralFuctions().FormatTime(currentTime);
                timeWaveUI.InitTimeUI(timeFormatted);
            }
            yield return new WaitForSeconds(.1f); // 0.1초마다 업데이트
        }
    }
}
