using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public int currentStageNumber;
    public bool isNewStage;

    public int currentCandyNumber;

    public int currentLightningNumber;
    public int currentKillNumber;
}

public class PlayerDataManager : MonoBehaviour
{
    [SerializeField] PlayerData playerData; // ����׸� ���� ����ȭ
    string filePath;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        LoadStageNumberData();
        LoadCandyNumberData();
    }

    #region Stage
    public int GetCurrentStageNumber()
    {
        return playerData.currentStageNumber;
    }

    public void SetCurrentStageNumber(int stageNumber)
    {
        playerData.currentStageNumber = stageNumber;

        SaveStageNumberData();
    }
    void SaveStageNumberData()
    {
        string jsonData = JsonUtility.ToJson(playerData);
        File.WriteAllText(filePath, jsonData);
    }

    void LoadStageNumberData()
    {
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            playerData = JsonUtility.FromJson<PlayerData>(jsonData);
        }
        else
        {
            playerData = new PlayerData
            {
                currentStageNumber = 1 // �ʱ� �������� �ѹ� ����
            };
            SaveStageNumberData();
        }
    }
    public bool IsNewStage() { return playerData.isNewStage; }
    public void SetIsNewStage(bool isNew)
    {
        playerData.isNewStage = isNew;
        SaveStageNumberData();
    }
    #endregion

    #region Candy
    public int GetCurrentCandyNumber()
    {
        return playerData.currentCandyNumber;
    }
    public void SetCurrentCandyNumber(int candyNumber)
    {
        playerData.currentCandyNumber = candyNumber;

        SaveCandyNumberData();
    }

    void SaveCandyNumberData()
    {
        string jsonData = JsonUtility.ToJson(playerData);
        File.WriteAllText(filePath, jsonData);
    }
    void LoadCandyNumberData()
    {
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            playerData = JsonUtility.FromJson<PlayerData>(jsonData);
        }
        else
        {
            playerData = new PlayerData
            {
                currentCandyNumber = 1 // �ʱ� �������� �ѹ� ����
            };
            SaveCandyNumberData();
        }
    }
    #endregion
}