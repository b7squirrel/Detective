using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteStageManager : MonoBehaviour, ISpawnController
{
    [Header("Enemy Configuration")]
    [SerializeField] InfiniteEnemyConfig[] enemyConfigs;

    [Header("Wave Settings")]
    [SerializeField] float baseSpawnInterval = 2f;
    [SerializeField] float waveInterval = 10f;
    [SerializeField] int baseEnemiesPerWave = 5;
    [SerializeField] float difficultyMultiplier = 1.2f;
    [SerializeField] int enemiesIncreasePerWave = 2;

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

    // 게임 상태
    int currentWave = 0;
    float currentDifficulty = 1f;
    float chestSpawnTimer = 0f;
    bool isInitialized = false;

    // 성과
    float survivalTime = 0f; // 생존 시간. 초 단위

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

        // 스폰이 일시정지되었으면 상자 스폰도 중지
        if (isSpawnPaused) return;

        // 생존 시간 증가
        survivalTime += Time.deltaTime;

        UpdateChestSpawn();
    }

    public int GetCurrentWave() => currentWave;
    public float GetSurvivalTime() => survivalTime;

    // ISpawnController 구현
    public void PauseSpawn(bool pause)
    {
        isSpawnPaused = pause;
        Logger.Log($"[InfiniteStage] Spawn {(pause ? "paused" : "resumed")}");
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

        // 유효성 검사
        if (!ValidateConfiguration())
        {
            Logger.LogError("[InfiniteStage] Invalid configuration! Check Inspector settings.");
            return;
        }

        // 필수 매니저 재확인
        if (!ValidateManagers())
        {
            Logger.LogError("[InfiniteStage] Required managers missing!");
            return;
        }

        // 벽 설정
        if (wallManager != null)
        {
            wallManager.SetWallSize();
        }

        // 기본 풀 초기화
        poolManager.InitPools();

        // 무한 모드용 적 풀 초기화
        InitializeEnemyPools();

        // 초기 보석 스폰
        SpawnInitialResources();

        // UI 초기화
        timeWaveUI = FindObjectOfType<TimeWaveUI>();

        // 초기화 완료
        isInitialized = true;

        // 웨이브 시작
        StartCoroutine(WaveLoop());
        StartCoroutine(UpdateUI());

        Logger.Log("[InfiniteStage] Initialization complete!");
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
            yield return new WaitForSeconds(.1f); // 0.1초마다 업데이트
        }
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
        // enemyConfigs에서 프리팹만 추출
        GameObject[] prefabs = new GameObject[enemyConfigs.Length];
        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            prefabs[i] = enemyConfigs[i].prefab;
        }

        // 풀 초기화
        poolManager.InitInfiniteEnemyPools(prefabs);

        Logger.Log($"[InfiniteStage] Initialized {prefabs.Length} enemy types");
    }

    void SpawnInitialResources()
    {
        // 초기 보석 스폰
        for (int i = 0; i < initialGemCount; i++)
        {
            Vector2 pos = GetRandomSpawnPosition();
            GameObject gem = poolManager.GetMisc(gemPrefab);
            if (gem != null)
            {
                gem.transform.position = pos;
            }
        }

        // 초기 상자 스폰
        if (chestPrefab != null && fieldItemSpawner != null)
        {
            Vector2 chestPos = GetRandomPointInRing(chestInnerRadius, chestOuterRadius);
            fieldItemSpawner.SpawnEggBox(chestPos);
        }

        Logger.Log($"[InfiniteStage] Spawned {initialGemCount} gems and 1 chest");
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            currentWave++;
            Logger.Log($"[InfiniteStage] === Wave {currentWave} Starting ===");

            yield return StartCoroutine(SpawnWave());

            Logger.Log($"[InfiniteStage] === Wave {currentWave} Complete ===");

            // 웨이브 간 휴식도 일시정지 영향 받음
            float waitedTime = 0f;
            while (waitedTime < waveInterval)
            {
                if (!isSpawnPaused)
                {
                    waitedTime += Time.deltaTime;
                }
                yield return null;
            }

            // 난이도 증가
            IncreaseDifficulty();
        }
    }

    IEnumerator SpawnWave()
    {
        int enemiesToSpawn = baseEnemiesPerWave + (currentWave - 1) * enemiesIncreasePerWave;
        float spawnDelay = baseSpawnInterval / Mathf.Max(1f, currentDifficulty * 0.5f);

        Logger.Log($"[InfiniteStage] Wave {currentWave}: Spawning {enemiesToSpawn} enemies (delay: {spawnDelay:F2}s)");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (GameManager.instance != null && GameManager.instance.IsPlayerDead)
            {
                Logger.Log("[InfiniteStage] Player dead, stopping wave");
                yield break;
            }

            // 스폰 일시정지 체크
            while (isSpawnPaused)
            {
                yield return null;
            }

            SpawnRandomEnemy();

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnRandomEnemy()
    {
        int enemyIndex = GetWeightedRandomEnemyIndex();

        if (enemyIndex >= 0 && enemyIndex < enemyConfigs.Length)
        {
            spawner.Spawn(enemyConfigs[enemyIndex].data, enemyIndex, false);
        }
        else
        {
            Logger.LogError($"[InfiniteStage] Invalid enemy index: {enemyIndex}");
        }
    }

    int GetWeightedRandomEnemyIndex()
    {
        // 현재 웨이브에서 사용 가능한 적들만 필터링
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

        // 가중치 기반 랜덤 선택
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
        Logger.Log($"[InfiniteStage] Difficulty increased to {currentDifficulty:F2}");
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

            Logger.Log($"[InfiniteStage] Spawned chest at {chestPos}");
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
    [ContextMenu("Force Next Wave")]
    void DebugForceNextWave()
    {
        StopAllCoroutines();
        StartCoroutine(WaveLoop());
    }

    [ContextMenu("Print Current State")]
    void DebugPrintState()
    {
        Logger.Log($"=== Infinite Stage State ===");
        Logger.Log($"Current Wave: {currentWave}");
        Logger.Log($"Difficulty: {currentDifficulty:F2}");
        Logger.Log($"Available Enemies: {GetAvailableEnemyCount()}");
    }

    int GetAvailableEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < enemyConfigs.Length; i++)
        {
            if (enemyConfigs[i].IsAvailableAtWave(currentWave))
            {
                count++;
            }
        }
        return count;
    }
    #endregion
}