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
    [SerializeField] PlayerData playerData;
    string filePath;
    bool isStageCleared;

    // 재화 값이 변경될 때 UI 등에서 구독하는 이벤트
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
                currentCandyNumber = 100,
                currentHighCoinNumber = 50
            };

            SavePlayerData();
        }
    }

    void SavePlayerData()
    {
        string jsonData = JsonUtility.ToJson(playerData);
        File.WriteAllText(filePath, jsonData);
    }

    void NotifyCurrencyChanged()
    {
        OnCurrencyChanged?.Invoke();
    }

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

    public void SetCurrentStageCleared()
    {
        isStageCleared = true;
    }

    // --- High Coin ---
    public int GetCurrentHighCoinNumber() => playerData.currentHighCoinNumber;

    public void AddHighCoin(int amount)
    {
        playerData.currentHighCoinNumber += amount;
        SavePlayerData();
        NotifyCurrencyChanged();
    }

    public void SetCristalNumberAs(int amount)
    {
        playerData.currentHighCoinNumber = amount;
        SavePlayerData();
        NotifyCurrencyChanged();
    }

    // --- Lightning ---
    public int GetCurrentLightningNumber() => playerData.currentLightningNumber;

    public void AddLightning(int amount)
    {
        playerData.currentLightningNumber += amount;
        SavePlayerData();
        NotifyCurrencyChanged();
    }

    // --- Candy ---
    public int GetCurrentCandyNumber() => playerData.currentCandyNumber;

    public void AddCandyNumber(int amount)
    {
        playerData.currentCandyNumber += amount;
        SavePlayerData();
        NotifyCurrencyChanged();
    }

    public void SetCandyNumberAs(int amount)
    {
        playerData.currentCandyNumber = amount;
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
        SetCandyNumberAs(coinNum);

        int hiCoinNum = FindObjectOfType<CristalManager>().GetCurrentCristals();
        SetCristalNumberAs(hiCoinNum);

        FindObjectOfType<PauseManager>().PauseGame();
    }
}