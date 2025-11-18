using UnityEngine;
using System.IO;
[System.Serializable]
public class PlayerData
{
    public int currentStageNumber;
    public bool isNewStage;

    public int currentCoinNumber;       // 기존 Candy → Coin
    public int currentCristalNumber;    // 기존 HighCoin → Cristal
    public int currentLightningNumber;
    public int currentKillNumber;
}
public class PlayerDataManager : MonoBehaviour
{
    [SerializeField] PlayerData playerData;
    string filePath;
    bool isStageCleared;

    public event System.Action OnCurrencyChanged;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        LoadPlayerData();
    }

    void LoadPlayerData()
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
                currentStageNumber = 1,
                currentLightningNumber = 60,
                currentCoinNumber = 100,
                currentCristalNumber = 50
            };

            SavePlayerData();
        }
    }

    void SavePlayerData()
    {
        string jsonData = JsonUtility.ToJson(playerData);
        File.WriteAllText(filePath, jsonData);
    }

    void NotifyCurrencyChanged() => OnCurrencyChanged?.Invoke();


    // --- Stage ---
    public int GetCurrentStageNumber() => playerData.currentStageNumber;
    public void SetCurrentStageNumber(int stageNumber)
    {
        playerData.currentStageNumber = stageNumber;
        SavePlayerData();
    }

    public bool IsNewStage() => playerData.isNewStage;
    public void SetIsNewStage(bool isNew)
    {
        playerData.isNewStage = isNew;
        SavePlayerData();
    }

    public void SetCurrentStageCleared() => isStageCleared = true;


    // --- Coin ---
    public int GetCurrentCoinNumber() => playerData.currentCoinNumber;
    public void AddCoin(int amount)
    {
        playerData.currentCoinNumber += amount;
        SavePlayerData();
        NotifyCurrencyChanged();
    }
    public void SetCoinNumberAs(int amount)
    {
        playerData.currentCoinNumber = amount;
        SavePlayerData();
        NotifyCurrencyChanged();
    }


    // --- Cristal ---
    public int GetCurrentCristalNumber() => playerData.currentCristalNumber;
    public void AddCristal(int amount)
    {
        playerData.currentCristalNumber += amount;
        SavePlayerData();
        NotifyCurrencyChanged();
    }
    public void SetCristalNumberAs(int amount)
    {
        playerData.currentCristalNumber = amount;
        SavePlayerData();
        NotifyCurrencyChanged();
    }

    // UI 업데이트 없이 실제 값만 증가
    public void SetCristalNumberAsSilent(int amount)
{
    playerData.currentCristalNumber = amount;
    SavePlayerData();
    // NotifyCurrencyChanged() 호출 안 함
}


    // --- Lightning ---
    public int GetCurrentLightningNumber() => playerData.currentLightningNumber;
    public void AddLightning(int amount)
    {
        playerData.currentLightningNumber += amount;
        SavePlayerData();
        NotifyCurrencyChanged();
    }


    // --- 게임 종료 전 저장 ---
    public void SaveResourcesBeforeQuitting()
    {
        StageInfo stageinfo = GetComponent<StageInfo>();
        int currentStage = GetCurrentStageNumber();

        if (stageinfo.IsFinalStage(currentStage) == false)
        {
            if (isStageCleared)
            {
                currentStage++;
                SetCurrentStageNumber(currentStage);
                isStageCleared = false;
            }
        }

        int coinNum = FindObjectOfType<CoinManager>().GetCurrentCoins();
        SetCoinNumberAs(coinNum);

        int cristalNum = FindObjectOfType<CristalManager>().GetCurrentCristals();
        SetCristalNumberAs(cristalNum);

        FindObjectOfType<PauseManager>().PauseGame();
    }
}