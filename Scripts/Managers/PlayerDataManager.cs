using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public int currentStageNumber;
    public bool isNewStage;

    public int currentCandyNumber;
    public int currentHighCoinNumber;

    public int currentLightningNumber;
    public int currentKillNumber;
}

public class PlayerDataManager : MonoBehaviour
{
    [SerializeField] PlayerData playerData; // 디버그를 위해 직렬화
    string filePath;

    bool isStageCleared;

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

        SavePlayerData();
    }

    void SavePlayerData()
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
                currentStageNumber = 1 // 초기 스테이지 넘버 설정
                , currentLightningNumber = 60
                , currentCandyNumber = 100
                , currentHighCoinNumber = 50
            };
            SavePlayerData();
        }
    }
    public bool IsNewStage() { return playerData.isNewStage; }
    public void SetIsNewStage(bool isNew)
    {
        playerData.isNewStage = isNew;
        SavePlayerData();
    }
    public void SetCurrentStageCleared()
    {
        isStageCleared = true;
    }
    #endregion

    public int GetCurrentHighCoinNumber()
    {
        return playerData.currentHighCoinNumber; ;
    }
    public void AddHighCoin(int highCoinNumToAdd)
    {
        playerData.currentHighCoinNumber += highCoinNumToAdd;
        SavePlayerData();
    }

    public int GetCurrentLightningNumber()
    {
        return playerData.currentLightningNumber;
    }
    public void AddLightning(int lightningToAdd)
    {
        playerData.currentLightningNumber += lightningToAdd;
        SavePlayerData();
    }

    #region Candy
    public int GetCurrentCandyNumber()
    {
        return playerData.currentCandyNumber;
    }
    public void AddCandyNumber(int candyNumberToAdd)
    {
        playerData.currentCandyNumber += candyNumberToAdd;
        Debug.Log("Add Candy Number " + candyNumberToAdd);
        SavePlayerData();
    }
    public void SetCandyNumberAs(int candyNumberToSet)
    {
        playerData.currentCandyNumber = candyNumberToSet;

        SavePlayerData();
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
                currentCandyNumber = 1 // 초기 스테이지 넘버 설정
            };
            SavePlayerData();
        }
    }
    #endregion

    #region 나가기 전 재화 저장
    /// <summary>
    /// 동전과 스테이지 저장
    /// </summary>
    public void SaveResourcesBeforeQuitting()
    {
        StageInfo stageinfo = GetComponent<StageInfo>();
        int currentStage = GetCurrentStageNumber();

        // 스테이지 저장. 최종 스테이지가 아니라면 스테이지 수를 올리기
        if (stageinfo.IsFinalStage(currentStage) == false)
        {
            if(isStageCleared)
            {
                Debug.Log("현재 스테이지 = " + currentStage);
                currentStage++;
                SetCurrentStageNumber(currentStage);
                Debug.Log("다음 스테이지 = " + currentStage);
                isStageCleared = false;
            }
        }

        // 동전 갯수 저장
        int coinNum = FindObjectOfType<CoinManager>().GetCurrentCoins();
        SetCandyNumberAs(coinNum);
        FindObjectOfType<PauseManager>().PauseGame();
    }
    #endregion
}