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

public class PlayerDataManager : SingletonBehaviour<PlayerDataManager>
{
    [SerializeField] PlayerData playerData;
    string filePath;
    bool isStageCleared;

    [Header("게임 모드")]
    [SerializeField] GameMode currentGameMode;

    public event System.Action OnCurrencyChanged;

    // ⭐ 추가: 데이터 로드 완료 플래그
    public static bool IsDataLoaded { get; private set; } = false;

    // ⭐ SingletonBehaviour의 Init()을 override하여 초기화
    protected override void Init()
    {
        base.Init(); // ⭐ 반드시 base.Init() 호출하여 Instance 설정

        filePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        LoadPlayerData();
        IsDataLoaded = true;
        Logger.Log("[PlayerDataManager] 데이터 로드 완료");
    }

    void OnApplicationQuit()
    {
        IsDataLoaded = false;
    }

    void LoadPlayerData()
    {
        if (File.Exists(filePath))
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                playerData = JsonUtility.FromJson<PlayerData>(jsonData);
                Logger.Log($"[PlayerDataManager] 플레이어 데이터 로드: Stage {playerData.currentStageNumber}");
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[PlayerDataManager] 데이터 로드 오류: {e.Message}");
                CreateDefaultPlayerData();
            }
        }
        else
        {
            Logger.Log("[PlayerDataManager] 저장된 데이터 없음, 기본값 생성");
            CreateDefaultPlayerData();
        }
    }

    void CreateDefaultPlayerData()
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

    void SavePlayerData()
    {
        try
        {
            string jsonData = JsonUtility.ToJson(playerData, true);
            File.WriteAllText(filePath, jsonData);
        }
        catch (System.Exception e)
        {
            Logger.LogError($"[PlayerDataManager] 데이터 저장 오류: {e.Message}");
        }
    }

    void NotifyCurrencyChanged() => OnCurrencyChanged?.Invoke();


    // --- Stage ---
    public int GetCurrentStageNumber()
    {
        if (playerData == null)
        {
            Logger.LogWarning("[PlayerDataManager] playerData is null");
            return 1;
        }

        return Mathf.Max(1, playerData.currentStageNumber);
    }
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

    // UI 업데이트 없이 실제 값만 증가
    public void SetCoinNumberAsSilent(int amount)
    {
        playerData.currentCoinNumber = amount;
        SavePlayerData();
        // NotifyCurrencyChanged() 호출 안 함
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

    public void SetGameMode(GameMode mode)
    {
        currentGameMode = mode;
    }
    public GameMode GetGameMode()
    {
        return currentGameMode;
    }
}