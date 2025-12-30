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

    public int bestWave; // 무한모드 웨이브 최고 기록
    public float bestSurvivalTime; // 무한 모드 시간 최고 기록

    // 일일 시스템 필드
    public string lastLoginDate;        // "2025-12-30" 형식
    public int consecutiveDays;         // 연속 출석일
    public bool hasTakenDailyReward;    // 오늘 출석 보상 수령 여부
}

public class PlayerDataManager : SingletonBehaviour<PlayerDataManager>
{
    [SerializeField] PlayerData playerData;
    string filePath;
    bool isStageCleared;
    StageInfo stageInfo;

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

        stageInfo = FindObjectOfType<StageInfo>();
        if(stageInfo == null)
        {
            Logger.LogWarning("[PlayerDataManager] StageInfo를 찾을 수 없습니다.");
        }
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

    // Wave
    public int GetBestWave() => playerData.bestWave;
    public void SetBestWave(int wave)
    {
        playerData.bestWave = wave;
        SavePlayerData();
    }

    // Survival Time
    public float GetBestSurvivalTime() => playerData.bestSurvivalTime;
    public void SetSurvivalTime(float survivalTime)
    {
        playerData.bestSurvivalTime = survivalTime;
        SavePlayerData();
    }

    // --- 게임 종료 전 저장 ---
    // 최고 스테이지, 골드, 크리스탈 기록 저장
    public void SaveResourcesBeforeQuitting()
    {
        Logger.Log($"[PlayerDataManager] {currentGameMode} 모드 리소스 저장 시작");

        // ⭐ 모드별 분기
        if (currentGameMode == GameMode.Regular)
        {
            SaveRegularModeResources();
        }
        else // Infinite
        {
            SaveInfiniteModeResources();
        }
    }
    void SaveRegularModeResources()
    {
        int currentStage = GetCurrentStageNumber();
        if (stageInfo.IsFinalStage(currentStage) == false)
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
    // 최고 웨이브 기록, 최고 생존 시간 기록, 골드, 크리스탈 기록 저장
    public void SaveInfiniteModeResources()
    {
        InfiniteStageManager infiniteManager = FindObjectOfType<InfiniteStageManager>();
        int currentWave = infiniteManager.GetCurrentWave();
        float currentTime = infiniteManager.GetSurvivalTime();

        // 최고 기록과 현재 기록 비교. 최고 기록 갱신
        if (currentWave > playerData.bestWave) SetBestWave(currentWave);
        if (currentTime > playerData.bestSurvivalTime) SetSurvivalTime(currentTime);

        SaveCoinsAndCristals();
        PauseGame();
    }
    
    void SaveCoinsAndCristals()
    {
        CoinManager coinManager = FindObjectOfType<CoinManager>();
        if (coinManager != null)
        {
            int coinNum = coinManager.GetCurrentCoins();
            SetCoinNumberAs(coinNum);
        }

        CristalManager cristalManager = FindObjectOfType<CristalManager>();
    if (cristalManager != null)
        {
            int cristalNum = cristalManager.GetCurrentCristals();
            SetCristalNumberAs(cristalNum);
        }
    }

    void PauseGame()
    {
        PauseManager pauseManager = FindObjectOfType<PauseManager>();
        if (pauseManager != null)
        {
            pauseManager.PauseGame();
        }
    }
    public void SetGameMode(GameMode mode)
    {
        currentGameMode = mode;
    }
    public GameMode GetGameMode()
    {
        return currentGameMode;
    }

    // --- Daily System ---
    public string GetLastLoginDate() => playerData.lastLoginDate ?? "";
    public void SetLastLoginDate(string date)
    {
        playerData.lastLoginDate = date;
        SavePlayerData();
    }

    public int GetConsecutiveDays() => playerData.consecutiveDays;
    public void SetConsecutiveDays(int days)
    {
        playerData.consecutiveDays = days;
        SavePlayerData();
    }

    public bool HasTakenDailyReward() => playerData.hasTakenDailyReward;
    public void SetHasTakenDailyReward(bool taken)
    {
        playerData.hasTakenDailyReward = taken;
        SavePlayerData();
    }
}