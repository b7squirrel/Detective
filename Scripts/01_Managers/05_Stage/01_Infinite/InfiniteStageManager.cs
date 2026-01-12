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
    }

    void Start()
    {
        StartCoroutine(WaitAndInitialize());
    }

    void Update()
    {
        if (!isInitialized) return;
        if (isSpawnPaused) return;

        survivalTime += Time.deltaTime;
        UpdateChestSpawn();
    }

    // ⭐ Public Getter 메서드들
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
    /// ⭐ 적이 죽었을 때 호출 (외부에서)
    /// </summary>
    public void OnEnemyKilled()
    {
        currentWaveEnemiesKilled++;

        // UI 업데이트
        if (enemyCountUI != null)
        {
            enemyCountUI.UpdateWaveProgress(
                currentWaveEnemiesKilled.ToString(),
                currentWavePlannedEnemies.ToString()
            );
        }
    }

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

        // 무한 모드 속도 적용
        ApplyGameSpeed();

        // 음악 초기화
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

    // ⭐ 새로운 메서드: 게임 속도 적용
    void ApplyGameSpeed()
    {
        Time.timeScale = gameSpeedMultiplier;
        Time.fixedDeltaTime = originalFixedDeltaTime * gameSpeedMultiplier;

        // ⭐ PauseManager에게 알려주기
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

    // ⭐ 게임 종료/비활성화 시 원래대로
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

        // ⭐ PauseManager도 원래대로
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
                string timeFormatted = new GeneralFuctions().FormatTime(currentTime);
                timeWaveUI.InitTimeWaveUI(timeFormatted, currentWave.ToString());
            }

            if (enemyCountUI != null)
            {
                enemyCountUI.InitDebugCurrentEnemies(GetCurrentEnemyCount().ToString());
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    // ⭐⭐⭐ 핵심: 웨이브 루프 (보스 시스템 통합)
    IEnumerator WaveLoop()
    {
        while (true)
        {
            currentWave++;
            
            currentWaveEnemiesSpawned = 0;
            currentWaveEnemiesKilled = 0;
            
            // ⭐ 6의 배수면 보스 웨이브
            bool isBossWave = (currentWave % 6 == 0);
            
            if (isBossWave)
            {
                Logger.Log($"[InfiniteStage] ========== BOSS Wave {currentWave} Start ==========");
                
                // 보스 1마리만
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
                
                // 일반 적 수 계산
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

            // 휴식
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

    // ⭐ 일반 웨이브 스폰 (일반 적 + SubBoss)
    IEnumerator SpawnNormalWave(int normalEnemyCount)
    {
        float spawnDelay = baseSpawnInterval / currentDifficulty;
        spawnDelay = Mathf.Max(spawnDelay, minSpawnInterval);

        // 1. 일반 적 스폰
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

        // 2. SubBoss 순서대로 스폰
        SpawnSubBossInOrder();
        currentWaveEnemiesSpawned++;

        Logger.Log($"[InfiniteStage] Spawned {normalEnemyCount} normal + 1 sub-boss");
    }

    // ⭐ 보스 웨이브 스폰 (보스만)
    IEnumerator SpawnBossWave()
    {
        SpawnBossInOrder(currentWave);
        currentWaveEnemiesSpawned++;

        Logger.Log($"[InfiniteStage] Spawned boss for wave {currentWave}");

        yield return null;
    }

    // ⭐ 일반 적만 랜덤 스폰
    void SpawnRandomNormalEnemy()
    {
        int enemyIndex = GetWeightedRandomNormalEnemyIndex();

        if (enemyIndex >= 0 && enemyIndex < enemyConfigs.Length)
        {
            spawner.SpawnForInfiniteMode(enemyConfigs[enemyIndex].data, enemyIndex);
        }
    }

    // ⭐ SubBoss 순서대로 스폰 (Wave 1-5, 7-11, 13-17...)
    void SpawnSubBossInOrder()
    {
        int waveInCycle = currentWave % 6;
        if (waveInCycle == 0) waveInCycle = 6;
        int subBossOrder = waveInCycle - 1;  // 0~4

        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsSubBoss() &&
                enemyConfigs[i].orderIndex == subBossOrder)
            {
                spawner.SpawnForInfiniteMode(enemyConfigs[i].data, i);
                Logger.Log($"[InfiniteStage] SubBoss[{subBossOrder}] spawned: {enemyConfigs[i].data.Name}");
                return;
            }
        }

        Logger.LogWarning($"[InfiniteStage] No SubBoss found for order {subBossOrder}!");
    }

    // ⭐ Boss 순서대로 스폰 (Wave 6, 12, 18...)
    void SpawnBossInOrder(int wave)
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
            return;
        }

        int bossOrder = bossNumber % bossCount;

        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsStageBoss() &&
                enemyConfigs[i].orderIndex == bossOrder)
            {
                // ⭐ isBoss = true 전달
                spawner.SpawnForInfiniteMode(enemyConfigs[i].data, i, isBoss: true);

                Logger.Log($"[InfiniteStage] Boss[{bossOrder}] spawned: {enemyConfigs[i].data.Name} (Wave {wave})");
                return;
            }
        }

        Logger.LogWarning($"[InfiniteStage] No Boss found for order {bossOrder}!");
    }

    // ⭐ 일반 적만 가중치 랜덤 (보스 제외)
    int GetWeightedRandomNormalEnemyIndex()
    {
        List<int> availableIndices = new List<int>();
        List<int> availableWeights = new List<int>();

        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            // Normal 타입만 필터링
            if (enemyConfigs[i].IsNormalEnemy() &&
                enemyConfigs[i].IsAvailableAtWave(currentWave))
            {
                availableIndices.Add(i);
                availableWeights.Add(enemyConfigs[i].weight);
            }
        }

        if (availableIndices.Count == 0)
        {
            Logger.LogError("[InfiniteStage] No normal enemies available!");
            return 0;
        }

        int totalWeight = 0;
        foreach (int w in availableWeights)
        {
            totalWeight += w;
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < availableWeights.Count; i++)
        {
            currentWeight += availableWeights[i];
            if (randomValue < currentWeight)
            {
                return availableIndices[i];
            }
        }

        return availableIndices[0];
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
        return new GeneralFuctions().GetRandomPointInRing(Vector2.zero, outerRadius, innerRadius);
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