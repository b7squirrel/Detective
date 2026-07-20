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
    GeneralFuctions generalFuctions = new GeneralFuctions(); // 필드로 선언

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

        // ⭐ 추가: 스테이지 시작 이벤트
        FirebaseManager.LogEvent("stage_start", "stage_number", currentStageNum.ToString());

        wallManager.SetWallSize(contents.startPositions);

        poolManager.InitPools();

        stageEventManager.Init(contents.stageDataText,
    contents.enemyData,
    contents.stageMusicType);

        spawnGemsOnStart.InitGemData(contents.gemToSpawn,
                                                   contents.numbersOfGemToSpawn,
                                                   contents.innerRadius, contents.outerRadius);
        spawnGemsOnStart.InitChestData(contents.chestPrefab,
                                                    contents.innerRadiusForChest, contents.outerRadiusForChest);
        spawnGemsOnStart.GenGemsAndChest();

        stageAssetManager.Init(contents.bossPrefab, contents.effects, contents.bossEffects);

        stageGroundManager.InitGround(contents.stageGroundType);
        stageGroundEffectManager.Init(contents.stageGroundType);

        // poolManager가 stageAssetManager를 참조하니까 먼저 초기화하면 안 됨
        WarmUpPoolsByStage(currentStageNum);

        StartCoroutine(UpdateTimeUI());
    }

    void WarmUpPoolsByStage(int stageNum)
    {
        // ✅ 이 두 줄이 반드시 WarmUp보다 먼저 와야 함
        poolManager.InitEnemyPools();
        poolManager.InitSubBossPools();

        int warmUpCount;
        switch (stageNum)
        {
            case 1: warmUpCount = 30; break;
            case 2: warmUpCount = 60; break;
            case 3: warmUpCount = 80; break;
            case 4: warmUpCount = 100; break;
            case 5: warmUpCount = 130; break;
            case 6:
            default: warmUpCount = 180; break;
        }

        poolManager.WarmUpEnemyPools(warmUpCount);
        poolManager.WarmUpSubBossPools(2);
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
                string timeFormatted = generalFuctions.FormatTime(currentTime);
                timeWaveUI.InitTimeUI(timeFormatted);
            }
            yield return new WaitForSeconds(.1f); // 0.1초마다 업데이트
        }
    }
}
