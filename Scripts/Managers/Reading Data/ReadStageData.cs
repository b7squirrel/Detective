using System.Collections.Generic;
using UnityEngine;

public class ReadStageData : MonoBehaviour
{
    public TextAsset stageTextData;
    ReadData readData;
    string[,] data;
    List <StageEvent> stageEvents;
    EnemyData[] enemyDatas;

    StageEventType GetStageEventType(string stageEventType)
    {
        if (stageEventType == "Enemy")
            return StageEventType.SpawnEnemy;
        if (stageEventType == "EnemyGroup")
            return StageEventType.SpawnEnemyGroup;
        if (stageEventType == "SubBoss")
            return StageEventType.SpawnSubBoss;
        if (stageEventType == "EggBox")
            return StageEventType.SpawnEggBox;
        if (stageEventType == "Boss")
            return StageEventType.SpawnEnemyBoss;
        if (stageEventType == "Object")
            return StageEventType.SpawnObject;
        if (stageEventType == "Incoming")
            return StageEventType.Incoming;

        return StageEventType.WinStage;
    }

    EnemyData GetEnemyType(string enemyType)
    {
        if (enemyType == "LV1")
            return enemyDatas[0];
        if (enemyType == "LV1_SubBoss")
            return enemyDatas[1];
        if (enemyType == "LV2")
            return enemyDatas[2];
        if (enemyType == "LV2_SubBoss")
            return enemyDatas[3];
        if (enemyType == "LV3")
            return enemyDatas[4];
        if (enemyType == "LV3_SubBoss")
            return enemyDatas[5];
        if (enemyType == "LV4")
            return enemyDatas[6];
        if (enemyType == "LV4_SubBoss")
            return enemyDatas[7];
        if (enemyType == "LV5")
            return enemyDatas[8];
        if (enemyType == "LV5_SubBoss")
            return enemyDatas[9];
        if (enemyType == "Boss")
            return enemyDatas[10];
        if (enemyType == "Group") 
            return enemyDatas[11];
        if (enemyType == "Egggulp_LV1") 
            return enemyDatas[12];
        if (enemyType == "Egggulp_LV2") 
            return enemyDatas[13];
        if (enemyType == "Egggulp_LV3") 
            return enemyDatas[14];
        if (enemyType == "Egggulp_LV4") 
            return enemyDatas[15];
        if (enemyType == "Egggulp_LV5") 
            return enemyDatas[16];
        return enemyDatas[0]; // 일단 채워넣었음
    }
    
    public void Init(TextAsset _stageTextData, EnemyData[] _enemyDatas)
    {
        stageTextData = _stageTextData;
        enemyDatas = _enemyDatas;
    }

    public List<StageEvent> GetStageEventsList()
    {
        readData = new ReadData();
        data = readData.GetText(stageTextData);
        stageEvents = new List<StageEvent>();
        int length = data.GetLength(0);

        for (int i = 0; i < length; i++)
        {
            StageEvent stageEvent = new StageEvent();
            stageEvent.eventType = GetStageEventType(data[i, 0]);
            stageEvent.time = int.Parse(data[i, 1]);
            stageEvent.enemyToSpawn = GetEnemyType(data[i, 2]);
            stageEvent.count = int.Parse(data[i, 3]);

            stageEvents.Add(stageEvent);
        }
        return stageEvents;
    }
}
