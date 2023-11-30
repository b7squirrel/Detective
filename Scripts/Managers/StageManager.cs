using UnityEngine;
using System.IO;

[System.Serializable]
public class StageNumberData
{
    public int currentStageNumber;
}

public class StageManager : MonoBehaviour
{
    StageNumberData stageNumberData;
    string filePath;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "stageNumberData.json");
        LoadStageNumberData();
    }

    public int GetCurrentStageNumber()
    {
        return stageNumberData.currentStageNumber;
    }

    public void SetCurrentStageNumber(int stageNumber)
    {
        stageNumberData.currentStageNumber = stageNumber;
        
        SaveStageNumberData();
    }

    void SaveStageNumberData()
    {
        string jsonData = JsonUtility.ToJson(stageNumberData);
        Debug.Log("Saving Current Stage number = " + stageNumberData.currentStageNumber);
        File.WriteAllText(filePath, jsonData);
    }

    void LoadStageNumberData()
    {
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            stageNumberData = JsonUtility.FromJson<StageNumberData>(jsonData);
        }
        else
        {
            stageNumberData = new StageNumberData
            {
                currentStageNumber = 1 // 초기 스테이지 넘버 설정
            };
            SaveStageNumberData();
        }
    }
}