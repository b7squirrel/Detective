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
    [SerializeField] float waveInterval = 3f;  // ⭐ 웨이브 클리어 후 짧은 휴식
    
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

    // ⭐ 웨이브 추적 변수들
    int currentWaveEnemiesSpawned = 0;     // 현재 웨이브에서 스폰된 적 수
    int currentWaveEnemiesKilled = 0;      // 현재 웨이브에서 처치한 적 수
    int currentWavePlannedEnemies = 0;     // 현재 웨이브 목표 적 수

    // 성과
    float survivalTime = 0f;

    // 스폰 일시정지
    bool isSpawnPaused = false;

    void Awake()
    {
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
    
    /// <summary>
    /// 현재 살아있는 적의 수
    /// </summary>
    public int GetCurrentEnemyCount()
    {
        if (spawner == null) return 0;
        return spawner.GetCurrentEnemyNums();
    }
    
    /// <summary>
    /// 현재 웨이브에서 처치한 적의 수
    /// </summary>
    public int GetCurrentWaveKilledCount()
    {
        return currentWaveEnemiesKilled;
    }
    
    /// <summary>
    /// 현재 웨이브에서 스폰될 예정인 총 적 수
    /// </summary>
    public int GetCurrentWavePlannedCount()
    {
        return currentWavePlannedEnemies;
    }
    
    /// <summary>
    /// 현재 웨이브 진행도 (0.0 ~ 1.0)
    /// </summary>
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

        // ⭐ 디버그 로그
        Logger.Log($">>> [Kill] Wave {currentWave}: {currentWaveEnemiesKilled} / {currentWavePlannedEnemies}");

        // ⭐ 오버플로우 경고
        if (currentWaveEnemiesKilled > currentWavePlannedEnemies)
        {
            Logger.LogWarning($">>> [Kill] WARNING: Killed ({currentWaveEnemiesKilled}) > Planned ({currentWavePlannedEnemies})!");
        }

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

        isInitialized = true;

        StartCoroutine(WaveLoop());
        StartCoroutine(UpdateUI());

        Logger.Log("[InfiniteStage] Initialization complete!");
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

            if(enemyCountUI != null)
            {
                enemyCountUI.InitDebugCurrentEnemies(GetCurrentEnemyCount().ToString());
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    // ⭐⭐⭐ 핵심 변경: 웨이브 루프
    IEnumerator WaveLoop()
    {
        while (true)
        {
            currentWave++;

            // 웨이브 시작 시 카운터 초기화
            currentWaveEnemiesSpawned = 0;
            currentWaveEnemiesKilled = 0;

            // 이번 웨이브 목표 적 수 계산
            float rawCount = baseEnemiesPerWave * Mathf.Pow(enemyGrowthRate, currentWave - 1);
            currentWavePlannedEnemies = Mathf.Min(Mathf.RoundToInt(rawCount), maxEnemiesPerWave);

            // 이제 UI 업데이트 (계산 후!)
            if (enemyCountUI != null)
            {
                enemyCountUI.UpdateWaveProgress("0", currentWavePlannedEnemies.ToString());
            }

            Logger.Log($"[InfiniteStage] ========== Wave {currentWave} Start ==========");
            Logger.Log($"[InfiniteStage] Target: {currentWavePlannedEnemies} enemies");

            // 1단계: 적 스폰
            yield return StartCoroutine(SpawnWave());

            Logger.Log($"[InfiniteStage] Spawning complete. Waiting for wave clear...");
            
            // 2단계: 모든 적 처치 대기
            yield return StartCoroutine(WaitForWaveClear());
            
            Logger.Log($"[InfiniteStage] ========== Wave {currentWave} Complete ==========");
            Logger.Log($"[InfiniteStage] Killed: {currentWaveEnemiesKilled} / {currentWavePlannedEnemies}");

            // 3단계: 짧은 휴식
            float waitedTime = 0f;
            while (waitedTime < waveInterval)
            {
                if (!isSpawnPaused)
                {
                    waitedTime += Time.deltaTime;
                }
                yield return null;
            }

            // 4단계: 난이도 증가
            IncreaseDifficulty();
        }
    }

    // 새로운 메서드: 웨이브 클리어 대기
    IEnumerator WaitForWaveClear()
    {
        // 모든 적이 죽을 때까지 대기
        while (GetCurrentEnemyCount() > 0)
        {
            // 플레이어가 죽었으면 중단
            if (GameManager.instance != null && GameManager.instance.IsPlayerDead)
            {
                Logger.Log("[InfiniteStage] Player dead during wave clear");
                yield break;
            }
            
            yield return new WaitForSeconds(0.5f);  // 0.5초마다 체크
        }
        
        Logger.Log("[InfiniteStage] All enemies cleared!");
    }

    IEnumerator SpawnWave()
    {
        int enemiesToSpawn = currentWavePlannedEnemies;
        
        float spawnDelay = baseSpawnInterval / currentDifficulty;
        spawnDelay = Mathf.Max(spawnDelay, minSpawnInterval);

        Logger.Log($"[InfiniteStage] Spawning {enemiesToSpawn} enemies with {spawnDelay:F2}s delay");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (GameManager.instance != null && GameManager.instance.IsPlayerDead)
            {
                Logger.Log("[InfiniteStage] Player dead, stopping spawn");
                yield break;
            }

            while (isSpawnPaused)
            {
                yield return null;
            }

            SpawnRandomEnemy();
            currentWaveEnemiesSpawned++;  // ⭐ 카운트 증가

            yield return new WaitForSeconds(spawnDelay);
        }
        
        Logger.Log($"[InfiniteStage] Spawned {currentWaveEnemiesSpawned} / {currentWavePlannedEnemies}");
    }

    void SpawnRandomEnemy()
    {
        int enemyIndex = GetWeightedRandomEnemyIndex();

        if (enemyIndex >= 0 && enemyIndex < enemyConfigs.Length)
        {
            spawner.SpawnForInfiniteMode(enemyConfigs[enemyIndex].data, enemyIndex); // 무한 모드 전용 Spawner 메서드
        }
        else
        {
            Logger.LogError($"[InfiniteStage] Invalid enemy index: {enemyIndex}");
        }
    }

    int GetWeightedRandomEnemyIndex()
    {
        List<int> availableIndices = new List<int>();
        List<int> availableWeights = new List<int>();

        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsAvailableAtWave(currentWave))
            {
                availableIndices.Add(i);
                availableWeights.Add(enemyConfigs[i].weight);
            }
        }

        if (availableIndices.Count == 0)
        {
            Logger.LogError("[InfiniteStage] No available enemies for current wave!");
            return 0;
        }

        int totalWeight = 0;
        for (int i = 0; i < availableWeights.Count; i++)
        {
            totalWeight += availableWeights[i];
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