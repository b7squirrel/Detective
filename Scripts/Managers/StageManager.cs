using UnityEngine;

public class StageManager : MonoBehaviour
{
    public StageContents[] stageContents;

    StageTime stageTime;
    StageEvenetManager stageEventManager;
    ReadStageData readStageData;
    StageEnemyData stageEnemyData;
    StageAssetManager stageAssetManager;
    SpawnGemsOnStart spawnGemsOnStart;

    void Awake()
    {
        stageTime = GetComponent<StageTime>();
        stageEventManager = GetComponent<StageEvenetManager>();
        readStageData = GetComponent<ReadStageData>();
        stageEnemyData = GetComponent<StageEnemyData>();
        stageAssetManager = GetComponent<StageAssetManager>();
        spawnGemsOnStart = GetComponent<SpawnGemsOnStart>();
    }

    public void SetStage(int _stageNum)
    {
        StageContents currentContents = stageContents[_stageNum];

    }
}
