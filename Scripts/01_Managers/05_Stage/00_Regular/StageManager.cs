using System.Collections;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public StageContents[] stageContents;

    // ⭐ 추가: 이번 세션에서 실제로 시작한 스테이지 번호 (다른 스크립트에서 참조용)
    public static int CurrentSessionStageNum { get; private set; }

    [Header("Speed Settings")] // ⭐ 추가
    [SerializeField] float gameSpeedMultiplier = 1.25f; // ⭐ 추가: 일반모드 배속
    float originalFixedDeltaTime; // ⭐ 추가

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
        originalFixedDeltaTime = Time.fixedDeltaTime; // ⭐ 추가: 원본 저장

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

        // ⭐ 추가: static 값에 기록
        CurrentSessionStageNum = currentStageNum;
        
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

        ApplyGameSpeed(); // ⭐ 추가: 일반모드 배속 적용

        StartCoroutine(UpdateTimeUI());
    }

    // ⭐ 추가: 무한모드의 ApplyGameSpeed와 동일한 패턴
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

    // ⭐ 추가: 씬을 나갈 때 원래 배속으로 복구 (무한모드의 ResetGameSpeed와 동일)
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
