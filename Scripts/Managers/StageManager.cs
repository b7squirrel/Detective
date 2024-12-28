using UnityEngine;

public class StageManager : MonoBehaviour
{
    public StageContents[] stageContents;

    StageTime stageTime;
    StageEvenetManager stageEventManager;
    StageAssetManager stageAssetManager;
    SpawnGemsOnStart spawnGemsOnStart;
    PoolManager poolManager;
    FieldItemSpawner fieldItemSpawner;

    void Awake()
    {
        stageTime = GetComponent<StageTime>();
        stageEventManager = GetComponent<StageEvenetManager>();
        stageAssetManager = GetComponent<StageAssetManager>();
        spawnGemsOnStart = GetComponent<SpawnGemsOnStart>();
        poolManager = FindObjectOfType<PoolManager>();
        fieldItemSpawner = FindObjectOfType<FieldItemSpawner>();
    }

    void Start()
    {
        int currentStageNum = FindAnyObjectByType<PlayerDataManager>().GetCurrentStageNumber();
        StageContents contents = stageContents[currentStageNum - 1];

        stageTime.Init(contents.WallDuration);

        poolManager.InitPools();

        stageEventManager.Init(contents.stageDataText,
                                       contents.enemyData, contents.enemyNumForNextEvent,
                                       contents.stageMusicType);

        fieldItemSpawner.Init(contents.eggSpawnTimes);

        spawnGemsOnStart.InitGemData(contents.gemToSpawn,
                                                   contents.numbersOfGemToSpawn,
                                                   contents.innerRadius, contents.outerRadius);
        spawnGemsOnStart.InitChestData(contents.chestPrefab,
                                                    contents.innerRadiusForChest, contents.outerRadiusForChest);
        spawnGemsOnStart.GenGemsAndChest();

        stageAssetManager.Init(contents.enemies, contents.bossPrefab, contents.effects, contents.bossEffects);

        // pool Manager에서 stage Asset manager를 참조하니까 가장 마지막에 실행
        poolManager.InitEnemyPools();
    }
}
