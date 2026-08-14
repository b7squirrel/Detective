using System.Collections;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public StageContents[] stageContents;

    public static int CurrentSessionStageNum { get; private set; }

    [Header("Speed Settings")]
    [SerializeField] float gameSpeedMultiplier = 1.25f;
    float originalFixedDeltaTime;

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
    GeneralFuctions generalFuctions = new GeneralFuctions();

    void Awake()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;

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

        CurrentSessionStageNum = currentStageNum;

        StageContents contents = stageContents[currentStageNum - 1];

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
        // GenGemsAndChest()는 StageStartEvents의 애니메이션 이벤트(프레임 151)에서 호출됨

        stageAssetManager.Init(contents.bossPrefab, contents.effects, contents.bossEffects);

        stageGroundManager.InitGround(contents.stageGroundType);
        stageGroundEffectManager.Init(contents.stageGroundType);

        // poolManager가 stageAssetManager를 참조하니까 먼저 초기화하면 안 됨
        WarmUpPoolsByStage(currentStageNum, contents); // ⭐ contents 추가

        ApplyGameSpeed();

        StartCoroutine(UpdateTimeUI());
    }

    void ApplyGameSpeed()
    {
        Time.timeScale = gameSpeedMultiplier;
        Time.fixedDeltaTime = originalFixedDeltaTime * gameSpeedMultiplier;

        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        if (pauseManager != null)
        {
            pauseManager.SetNormalTimeScale(gameSpeedMultiplier);
        }

        Logger.Log($"[StageManager] Game speed: {gameSpeedMultiplier}x");
    }

    void OnDisable()
    {
        ResetGameSpeed();
    }

    void OnDestroy()
    {
        ResetGameSpeed();
    }

    void ResetGameSpeed()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        if (pauseManager != null)
        {
            pauseManager.SetNormalTimeScale(1.0f);
        }
    }

    // ⭐ 파라미터에 StageContents contents 추가
    void WarmUpPoolsByStage(int stageNum, StageContents contents)
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

        // ⭐ 추가: 젬/상자도 미리 워밍업 (Instantiate 비용을 여기서 미리 지불)
        poolManager.WarmUpMiscPool(contents.gemToSpawn, contents.numbersOfGemToSpawn);
        poolManager.WarmUpMiscPool(contents.chestPrefab, 1);
    }

    IEnumerator UpdateTimeUI()
    {
        int stageNum = playerDataManager.GetCurrentStageNumber();
        timeWaveUI.InitStageUI(stageNum.ToString());

        while (true)
        {
            if (timeWaveUI != null)
            {
                float currentTime = stageTime.GetElapsedTime();
                string timeFormatted = generalFuctions.FormatTime(currentTime);
                timeWaveUI.InitTimeUI(timeFormatted);
            }
            yield return new WaitForSeconds(.1f);
        }
    }
}