using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteStageManager : MonoBehaviour, ISpawnController
{
    [Header("Enemy Configuration")]
    [SerializeField] InfiniteEnemyConfig[] enemyConfigs;

    [Header("Wave Settings - Exponential Growth")]
    [SerializeField] int baseEnemiesPerWave = 15;
    [SerializeField] float enemyGrowthRate = 1.35f;
    [SerializeField] int maxEnemiesPerWave = 120;

    [Header("Spawn Timing")]
    [SerializeField] float baseSpawnInterval = 1.0f;
    [SerializeField] float minSpawnInterval = 0.25f;
    [SerializeField] float waveInterval = 3f;

    [Header("Difficulty")]
    [SerializeField] float difficultyMultiplier = 1.15f;

    [Header("Initial Setup")]
    [SerializeField] int initialGemCount = 15;
    [SerializeField] GameObject gemPrefab;
    [SerializeField] float spawnRadius = 20f;

    [Header("Chest Spawn")]
    [SerializeField] GameObject chestPrefab;
    [SerializeField] float chestSpawnInterval = 30f;
    [SerializeField] float chestInnerRadius = 10f;
    [SerializeField] float chestOuterRadius = 18f;

    [Header("Speed Settings")]
    [SerializeField] float gameSpeedMultiplier = 1.5f;
    float originalTimeScale = 1.0f;
    float originalFixedDeltaTime = 0.02f;

    // 캐시된 매니저들
    PoolManager poolManager;
    Spawner spawner;
    WallManager wallManager;
    FieldItemSpawner fieldItemSpawner;
    StageInfo stageInfo;

    // GetWeightedRandomNormalEnemyIndex에서 매번 new List 하지 않도록 캐싱
    List<int> cachedIndices = new List<int>(16);
    List<int> cachedWeights = new List<int>(16);

    // UpdateUI, GetRandomPointInRing에서 매번 new 하지 않도록 캐싱
    GeneralFuctions generalFuctions = new GeneralFuctions();

    [Header("UI")]
    TimeWaveUI timeWaveUI;
    EnemyCountUI enemyCountUI;

    // 게임 상태
    int currentWave = 0;
    float currentDifficulty = 1f;
    float chestSpawnTimer = 0f;
    bool isInitialized = false;

    // 웨이브 추적 변수들
    int currentWaveEnemiesSpawned = 0;
    int currentWaveEnemiesKilled = 0;
    int currentWavePlannedEnemies = 0;
    public int GetClearedWaves() => Mathf.Max(0, currentWave - 1);

    // 성과
    float survivalTime = 0f;

    // 스폰 일시정지
    bool isSpawnPaused = false;

    // 웨이브 완료 이벤트
    public event System.Action<int> OnWaveComplete;

    void Awake()
    {
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;

        poolManager = FindObjectOfType<PoolManager>();
        spawner = FindObjectOfType<Spawner>();
        wallManager = FindObjectOfType<WallManager>();
        fieldItemSpawner = FindObjectOfType<FieldItemSpawner>();
        stageInfo = FindObjectOfType<StageInfo>();
    }

    void Start()
    {
        GameManager.instance.progressionBar.DeactivateProgressBar(); // 무한모드는 진행바 없애기
        StartCoroutine(WaitAndInitialize());
    }

    void Update()
    {
        if (!isInitialized) return;
        if (isSpawnPaused) return;

        survivalTime += Time.deltaTime;
        UpdateChestSpawn();
    }

    #region Public Getter 메서드들
    public int GetCurrentWave() => currentWave;
    public float GetSurvivalTime() => survivalTime;

    public int GetCurrentEnemyCount()
    {
        if (spawner == null) return 0;
        return spawner.GetCurrentEnemyNums();
    }

    public int GetCurrentWaveKilledCount()
    {
        return currentWaveEnemiesKilled;
    }

    public int GetCurrentWavePlannedCount()
    {
        return currentWavePlannedEnemies;
    }

    public float GetCurrentWaveProgress()
    {
        if (currentWavePlannedEnemies == 0) return 0f;
        return (float)currentWaveEnemiesKilled / currentWavePlannedEnemies;
    }

    public void PauseSpawn(bool pause)
    {
        isSpawnPaused = pause;
        Logger.Log($"[InfiniteStage] Spawn {(pause ? "paused" : "resumed")}");
    }

    /// <summary>
    /// 적이 죽었을 때 EnemyBase.Die()에서 호출
    /// </summary>
    public void OnEnemyKilled()
    {
        currentWaveEnemiesKilled++;

        if (enemyCountUI != null)
        {
            enemyCountUI.UpdateWaveProgress(
                currentWaveEnemiesKilled.ToString(),
                currentWavePlannedEnemies.ToString()
            );
        }
    }
    #endregion

    IEnumerator WaitAndInitialize()
    {
        Logger.Log("[InfiniteStage] Waiting for Essential scene...");

        int maxWaitFrames = 300;
        int waitedFrames = 0;

        while ((PickupSpawner.Instance == null ||
                GameManager.instance == null ||
                Player.instance == null ||
                poolManager == null) &&
               waitedFrames < maxWaitFrames)
        {
            yield return null;
            waitedFrames++;
        }

        if (waitedFrames >= maxWaitFrames)
        {
            Logger.LogError("[InfiniteStage] Timeout waiting for Essential scene!");
            yield break;
        }

        Logger.Log($"[InfiniteStage] Essential ready after {waitedFrames} frames");

        // WaitAndInitialize에서 한 번 더 캐싱 (Awake보다 늦게 준비되는 경우 대비)
        poolManager = FindObjectOfType<PoolManager>();
        spawner = FindObjectOfType<Spawner>();
        wallManager = FindObjectOfType<WallManager>();
        fieldItemSpawner = FindObjectOfType<FieldItemSpawner>();

        Initialize();
    }

    #region 초기화
    void Initialize()
    {
        Logger.Log("[InfiniteStage] Initializing Infinite Mode...");

        if (!ValidateConfiguration())
        {
            Logger.LogError("[InfiniteStage] Invalid configuration!");
            return;
        }

        if (!ValidateManagers())
        {
            Logger.LogError("[InfiniteStage] Required managers missing!");
            return;
        }

        if (wallManager != null)
        {
            wallManager.SetWallSize();
        }

        poolManager.InitPools();
        InitializeEnemyPools();
        SpawnInitialResources();

        timeWaveUI = FindObjectOfType<TimeWaveUI>();
        enemyCountUI = FindObjectOfType<EnemyCountUI>();

        ApplyGameSpeed();

        if (GameManager.instance != null && GameManager.instance.musicCreditManager != null)
        {
            GameManager.instance.musicCreditManager.Init();
            Logger.Log("[InfiniteStage] MusicCreditManager.Init() called");
        }

        isInitialized = true;

        StartCoroutine(WaveLoop());
        StartCoroutine(UpdateUI());

        Logger.Log("[InfiniteStage] Initialization complete!");
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

        Logger.Log($"[InfiniteStage] Game speed: {gameSpeedMultiplier}x");
    }

    bool ValidateConfiguration()
    {
        if (enemyConfigs == null || enemyConfigs.Length == 0)
        {
            Logger.LogError("[InfiniteStage] No enemy configs assigned!");
            return false;
        }

        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].prefab == null)
            {
                Logger.LogError($"[InfiniteStage] Enemy config {i} has null prefab!");
                return false;
            }
            if (enemyConfigs[i].data == null)
            {
                Logger.LogError($"[InfiniteStage] Enemy config {i} has null data!");
                return false;
            }
        }

        return true;
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
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        if (pauseManager != null)
        {
            pauseManager.SetNormalTimeScale(1.0f);
        }

        Logger.Log("[InfiniteStage] Game speed reset to normal");
    }

    bool ValidateManagers()
    {
        bool allValid = true;

        if (poolManager == null)
        {
            Logger.LogError("[InfiniteStage] PoolManager is null!");
            allValid = false;
        }

        if (spawner == null)
        {
            Logger.LogError("[InfiniteStage] Spawner is null!");
            allValid = false;
        }

        if (PickupSpawner.Instance == null)
        {
            Logger.LogError("[InfiniteStage] PickupSpawner.instance is null!");
            allValid = false;
        }

        if (GameManager.instance == null)
        {
            Logger.LogError("[InfiniteStage] GameManager.instance is null!");
            allValid = false;
        }

        if (Player.instance == null)
        {
            Logger.LogError("[InfiniteStage] Player.instance is null!");
            allValid = false;
        }

        return allValid;
    }

    void InitializeEnemyPools()
    {
        GameObject[] prefabs = new GameObject[enemyConfigs.Length];
        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            prefabs[i] = enemyConfigs[i].prefab;
        }

        poolManager.InitInfiniteEnemyPools(prefabs);
        Logger.Log($"[InfiniteStage] Initialized {prefabs.Length} enemy types");
    }

    void SpawnInitialResources()
    {
        for (int i = 0; i < initialGemCount; i++)
        {
            Vector2 pos = GetRandomSpawnPosition();
            GameObject gem = poolManager.GetMisc(gemPrefab);
            if (gem != null)
            {
                gem.transform.position = pos;
            }
        }

        if (chestPrefab != null && fieldItemSpawner != null)
        {
            Vector2 chestPos = GetRandomPointInRing(chestInnerRadius, chestOuterRadius);
            fieldItemSpawner.SpawnEggBox(chestPos);
        }

        Logger.Log($"[InfiniteStage] Spawned {initialGemCount} gems and 1 chest");
    }
    #endregion

    IEnumerator UpdateUI()
    {
        while (true)
        {
            if (timeWaveUI != null)
            {
                float currentTime = GetSurvivalTime();
                string timeFormatted = generalFuctions.FormatTime(currentTime); // 캐싱된 인스턴스 재사용
                timeWaveUI.InitTimeWaveUI(timeFormatted, currentWave.ToString());
            }

            if (enemyCountUI != null)
            {
                enemyCountUI.InitDebugCurrentEnemies(GetCurrentEnemyCount().ToString());
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    // 핵심: 웨이브 루프 (보스 시스템 통합)
    IEnumerator WaveLoop()
    {
        while (true)
        {
            currentWave++;

            currentWaveEnemiesSpawned = 0;
            currentWaveEnemiesKilled = 0;

            if (timeWaveUI != null)
                timeWaveUI.PunchWaveText();

            // 6의 배수면 보스 웨이브
            bool isBossWave = (currentWave % 6 == 0);

            if (isBossWave)
            {
                Logger.Log($"[InfiniteStage] ========== BOSS Wave {currentWave} Start ==========");

                currentWavePlannedEnemies = 1;

                if (enemyCountUI != null)
                {
                    enemyCountUI.UpdateWaveProgress("0", "1");
                }

                yield return StartCoroutine(SpawnBossWave());
            }
            else
            {
                Logger.Log($"[InfiniteStage] ========== Wave {currentWave} Start ==========");

                float rawCount = baseEnemiesPerWave * Mathf.Pow(enemyGrowthRate, currentWave - 1);
                int normalEnemies = Mathf.Min(Mathf.RoundToInt(rawCount), maxEnemiesPerWave);

                // 일반 적 + SubBoss 1마리
                currentWavePlannedEnemies = normalEnemies + 1;

                if (enemyCountUI != null)
                {
                    enemyCountUI.UpdateWaveProgress("0", currentWavePlannedEnemies.ToString());
                }

                Logger.Log($"[InfiniteStage] Target: {normalEnemies} enemies + 1 sub-boss");

                yield return StartCoroutine(SpawnNormalWave(normalEnemies));
            }

            Logger.Log($"[InfiniteStage] Spawning complete. Waiting for wave clear...");

            yield return StartCoroutine(WaitForWaveClear());

            Logger.Log($"[InfiniteStage] ========== Wave {currentWave} Complete ==========");
            Logger.Log($"[InfiniteStage] Killed: {currentWaveEnemiesKilled} / {currentWavePlannedEnemies}");

            OnWaveComplete?.Invoke(currentWave);
            Logger.Log($"[InfiniteStage] OnWaveComplete event triggered for wave {currentWave}");

            // 웨이브 휴식
            float waitedTime = 0f;
            while (waitedTime < waveInterval)
            {
                if (!isSpawnPaused)
                {
                    waitedTime += Time.deltaTime;
                }
                yield return null;
            }

            IncreaseDifficulty();
        }
    }

    IEnumerator WaitForWaveClear()
    {
        while (GetCurrentEnemyCount() > 0)
        {
            if (GameManager.instance != null && GameManager.instance.IsPlayerDead)
            {
                Logger.Log("[InfiniteStage] Player dead during wave clear");
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        Logger.Log("[InfiniteStage] All enemies cleared!");
    }

    // 일반 웨이브 스폰 (일반 적 + SubBoss)
    IEnumerator SpawnNormalWave(int normalEnemyCount)
    {
        float spawnDelay = baseSpawnInterval / currentDifficulty;
        spawnDelay = Mathf.Max(spawnDelay, minSpawnInterval);

        // 일반 적 스폰
        for (int i = 0; i < normalEnemyCount; i++)
        {
            if (GameManager.instance != null && GameManager.instance.IsPlayerDead)
                yield break;

            while (isSpawnPaused)
                yield return null;

            SpawnRandomNormalEnemy();
            currentWaveEnemiesSpawned++;

            yield return new WaitForSeconds(spawnDelay);
        }

        // 경고 팝업 후 서브보스 스폰
        string subBossName = GetSubBossName();
        GameManager.instance.bossWarningPanel.Init(subBossName);
        yield return new WaitForSecondsRealtime(2f);

        SpawnSubBossInOrder();
        currentWaveEnemiesSpawned++;

        Logger.Log($"[InfiniteStage] Spawned {normalEnemyCount} normal + 1 sub-boss");
    }

    // 보스 웨이브 스폰 (보스만)
    IEnumerator SpawnBossWave()
    {
        string bossName = GetBossName(currentWave);
        GameManager.instance.bossWarningPanel.Init(bossName);
        yield return new WaitForSeconds(2f);

        SpawnBossInOrder(currentWave);
        currentWaveEnemiesSpawned++;
        yield return null;
    }

    #region 보스 이름 얻기
    string GetSubBossName()
    {
        int waveInCycle = currentWave % 6;
        if (waveInCycle == 0) waveInCycle = 6;
        int subBossOrder = waveInCycle - 1; // 0~4

        if (stageInfo != null) // 필드 캐싱 사용
        {
            int stageIndex = subBossOrder + 1;
            return stageInfo.GetStageInfo(stageIndex).Title;
        }

        // stageInfo가 없으면 EnemyData.Name으로 폴백
        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsSubBoss() &&
                enemyConfigs[i].orderIndex == subBossOrder)
            {
                return enemyConfigs[i].data.Name;
            }
        }
        return "";
    }

    string GetBossName(int wave)
    {
        int bossNumber = (wave / 6) - 1;
        int bossCount = 0;

        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsStageBoss())
                bossCount++;
        }

        if (bossCount == 0) return "";
        int bossOrder = bossNumber % bossCount;

        if (stageInfo != null) // 필드 캐싱 사용 (기존의 지역변수 FindObjectOfType 제거)
        {
            int stageIndex = (bossOrder + 1) * 6;
            return stageInfo.GetStageInfo(stageIndex).Title;
        }

        // stageInfo가 없으면 EnemyData.Name으로 폴백
        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsStageBoss() &&
                enemyConfigs[i].orderIndex == bossOrder)
            {
                return enemyConfigs[i].data.Name;
            }
        }
        return "";
    }
    #endregion

    // 일반 적만 랜덤 스폰
    void SpawnRandomNormalEnemy()
    {
        int enemyIndex = GetWeightedRandomNormalEnemyIndex();

        if (enemyIndex >= 0 && enemyIndex < enemyConfigs.Length)
        {
            spawner.SpawnForInfiniteMode(enemyConfigs[enemyIndex].data, enemyIndex);
        }
    }

    // SubBoss 순서대로 스폰 (Wave 1-5, 7-11, 13-17...)
    string SpawnSubBossInOrder()
    {
        int waveInCycle = currentWave % 6;
        if (waveInCycle == 0) waveInCycle = 6;
        int subBossOrder = waveInCycle - 1;

        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsSubBoss() &&
                enemyConfigs[i].orderIndex == subBossOrder)
            {
                spawner.SpawnForInfiniteMode(enemyConfigs[i].data, i);
                Logger.Log($"[InfiniteStage] SubBoss[{subBossOrder}] spawned: {enemyConfigs[i].data.Name}");
                return enemyConfigs[i].data.Name;
            }
        }

        Logger.LogWarning($"[InfiniteStage] No SubBoss found for order {subBossOrder}!");
        return "";
    }

    // Boss 순서대로 스폰 (Wave 6, 12, 18...)
    string SpawnBossInOrder(int wave)
    {
        int bossNumber = (wave / 6) - 1;
        int bossCount = 0;

        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsStageBoss())
                bossCount++;
        }

        if (bossCount == 0)
        {
            Logger.LogError("[InfiniteStage] No boss configured!");
            return "";
        }

        int bossOrder = bossNumber % bossCount;

        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsStageBoss() &&
                enemyConfigs[i].orderIndex == bossOrder)
            {
                spawner.SpawnForInfiniteMode(enemyConfigs[i].data, i, isBoss: true);
                Logger.Log($"[InfiniteStage] Boss[{bossOrder}] spawned: {enemyConfigs[i].data.Name}");
                return enemyConfigs[i].data.Name;
            }
        }

        Logger.LogWarning($"[InfiniteStage] No Boss found for order {bossOrder}!");
        return "";
    }

    // 일반 적만 가중치 랜덤 (보스 제외). List를 캐싱하여 GC 방지
    int GetWeightedRandomNormalEnemyIndex()
    {
        cachedIndices.Clear();
        cachedWeights.Clear();

        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsNormalEnemy() &&
                enemyConfigs[i].IsAvailableAtWave(currentWave))
            {
                cachedIndices.Add(i);
                cachedWeights.Add(enemyConfigs[i].weight);
            }
        }

        if (cachedIndices.Count == 0)
        {
            Logger.LogError("[InfiniteStage] No normal enemies available!");
            return 0;
        }

        int totalWeight = 0;
        for (int i = 0; i < cachedWeights.Count; i++)
        {
            totalWeight += cachedWeights[i];
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < cachedWeights.Count; i++)
        {
            currentWeight += cachedWeights[i];
            if (randomValue < currentWeight)
            {
                return cachedIndices[i];
            }
        }

        return cachedIndices[0];
    }

    void IncreaseDifficulty()
    {
        currentDifficulty *= difficultyMultiplier;
        Logger.Log($"[InfiniteStage] Difficulty: {currentDifficulty:F2}x");
    }

    void UpdateChestSpawn()
    {
        if (chestPrefab == null || fieldItemSpawner == null) return;
        if (GameManager.instance.IsBossStage) return;

        chestSpawnTimer += Time.deltaTime;

        if (chestSpawnTimer >= chestSpawnInterval)
        {
            Vector2 chestPos = GetRandomPointInRing(chestInnerRadius, chestOuterRadius);
            fieldItemSpawner.SpawnEggBox(chestPos);
            chestSpawnTimer = 0f;
        }
    }

    Vector2 GetRandomSpawnPosition()
    {
        return new Vector2(
            Random.Range(-spawnRadius, spawnRadius),
            Random.Range(-spawnRadius, spawnRadius)
        );
    }

    Vector2 GetRandomPointInRing(float innerRadius, float outerRadius)
    {
        return generalFuctions.GetRandomPointInRing(Vector2.zero, outerRadius, innerRadius); // 캐싱된 인스턴스 재사용
    }

    #region Debug
    [ContextMenu("Print Wave Preview (Next 10 Waves)")]
    void DebugPrintWavePreview()
    {
        Logger.Log("=== Wave Preview ===");
        for (int wave = 1; wave <= 10; wave++)
        {
            float rawCount = baseEnemiesPerWave * Mathf.Pow(enemyGrowthRate, wave - 1);
            int count = Mathf.Min(Mathf.RoundToInt(rawCount), maxEnemiesPerWave);
            float difficulty = Mathf.Pow(difficultyMultiplier, wave - 1);
            float spawnDelay = Mathf.Max(baseSpawnInterval / difficulty, minSpawnInterval);
            float duration = count * spawnDelay;

            Logger.Log($"Wave {wave}: {count} enemies, {spawnDelay:F2}s interval, ~{duration:F0}s spawn time");
        }
    }

    [ContextMenu("Print Current State")]
    void DebugPrintState()
    {
        Logger.Log($"=== Infinite Stage State ===");
        Logger.Log($"Current Wave: {currentWave}");
        Logger.Log($"Wave Progress: {currentWaveEnemiesKilled} / {currentWavePlannedEnemies}");
        Logger.Log($"Enemies Alive: {GetCurrentEnemyCount()}");
        Logger.Log($"Difficulty: {currentDifficulty:F2}x");
        Logger.Log($"Survival Time: {survivalTime:F1}s");
    }
    #endregion
}