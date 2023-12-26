using UnityEngine;
using System.IO;

[System.Serializable]
public class StageNumberData
{
    public int currentStageNumber;
    public bool isNewStage;
}

public class PlayerDataManager : MonoBehaviour
{
    [SerializeField] StageNumberData stageNumberData; // ����׸� ���� ����ȭ
    string filePath;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "stageNumberData.json");
        LoadStageNumberData();
        Debug.Log("LOAD");
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
    public void LoadCurrentStageNumber()
    {
        LoadStageNumberData();
    }

    public bool IsNewStage() { return stageNumberData.isNewStage; }
    public void SetIsNewStage(bool isNew) 
    { 
        stageNumberData.isNewStage = isNew;
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
                currentStageNumber = 1 // �ʱ� �������� �ѹ� ����
            };
            SaveStageNumberData();
        }
    }
}