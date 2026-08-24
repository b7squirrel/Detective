using UnityEngine;
using System.IO;
using System;
using System.Collections;

[System.Serializable]
public class PlayerData
{
    public int currentStageNumber;
    public bool isNewStage;
    public int currentCoinNumber;       // 기존 Candy → Coin
    public int currentCristalNumber;    // 기존 HighCoin → Cristal
    public int currentLightningNumber;
    public long lastLightningUpdateTicks; // 마지막 번개 갱신 시각 (DateTime.Ticks)
    public int currentKillNumber;
    public int bestWave; // 무한모드 웨이브 최고 기록
    public float bestSurvivalTime; // 무한 모드 시간 최고 기록

    // 일일 시스템 필드
    public string lastLoginDate;        // "2025-12-30" 형식
    public int consecutiveDays;         // 연속 출석일
    public bool hasTakenDailyReward;    // 오늘 출석 보상 수령 여부

    // 무한모드 해금
    public bool isInfiniteModeUnlocked;

    // ⭐ 추가: 첫 크리스탈 구매 2배 보너스 수령 여부
    public bool firstCristalBonusClaimed;

    // ⭐ 추가: 첫 번째 동료 슬롯 해금 안내 오버레이를 이미 보여줬는지 (한 번만 표시하기 위함)
    public bool firstCompanionSlotAnnouncementShown;

    // ⭐ 추가: 동료 슬롯별로 "새로 해금됨" 빨간 점 배지를 이미 확인했는지 (인덱스 0~3). false면 아직 안 봄 = 배지 표시
    public bool[] companionSlotBadgeSeen = new bool[4];
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

    // 무한모드 해금
    public bool IsInfiniteModeUnlocked() => playerData.isInfiniteModeUnlocked;

    [Header("번개 설정")]
    [SerializeField] int maxLightningNumber = 25;
    [SerializeField] int lightningRechargeSeconds = 300; // 5분
    public int GetMaxLightningNumber() => maxLightningNumber;

    [Header("초기 재화 설정")]
    [SerializeField] int defaultCoinNumber = 10000;
    [SerializeField] int defaultCristalNumber = 250;

    // ⭐ SingletonBehaviour의 Init()을 override하여 초기화
    protected override void Init()
    {
        base.Init(); // ⭐ 반드시 base.Init() 호출하여 Instance 설정
        filePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        LoadPlayerData();
        ApplyLightningRegen(); // ← 앱 시작 시 오프라인 동안 쌓인 회복분 반영
        IsDataLoaded = true;
        StartCoroutine(LightningRegenLoop()); // ← 실행 중 매초 체크
        stageInfo = FindObjectOfType<StageInfo>();
        if (stageInfo == null)
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
            currentLightningNumber = maxLightningNumber,
            lastLightningUpdateTicks = DateTime.UtcNow.Ticks,
            currentCoinNumber = defaultCoinNumber,
            currentCristalNumber = defaultCristalNumber
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

    // UI 업데이트 없이 실제 값만 증가 (보상 이펙트용 - 오버캡 허용, 별도 제한 없음)
    public void AddLightningSilent(int amount)
    {
        playerData.currentLightningNumber += amount;
        SavePlayerData();
        // NotifyCurrencyChanged() 호출 안 함
    }

    IEnumerator LightningRegenLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            ApplyLightningRegen();
        }
    }

    public bool TryConsumeLightning(int amount)
    {
        ApplyLightningRegen(); // 체크 직전에 최신 상태로 갱신
        if (playerData.currentLightningNumber < amount) return false;
        playerData.currentLightningNumber -= amount;
        SavePlayerData();
        NotifyCurrencyChanged();
        return true;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus) // 백그라운드에서 돌아왔을 때
            ApplyLightningRegen();
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
        // ⭐ 수정: currentStage 증가/isStageCleared 리셋 이전에
        //          "이번 세션에 실제로 클리어했는지" 값을 먼저 고정해둔다.
        //          (기존 코드는 currentStage++ 시점에 isStageCleared를 false로
        //           바꿔버린 뒤 그 값으로 clearBonus 여부를 판단하고 있어서,
        //           죽었을 때(isStageCleared == false)도 조건이 참이 되어
        //           클리어 보너스가 잘못 지급되는 버그가 있었음)
        bool wasCleared = isStageCleared;

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

        // CoinManager 값 + GoldRewardManager 보상 합산
        int coinNum = FindObjectOfType<CoinManager>().GetCurrentCoins();
        int killGold = GoldRewardManager.Instance.GetKillGold();
        // ⭐ 수정: wasCleared 기준으로 판단 (죽었으면 0, 클리어했을 때만 지급)
        int clearBonus = wasCleared ? GoldRewardManager.Instance.GetClearBonus(currentStage - 1) : 0;
        SetCoinNumberAs(coinNum + killGold + clearBonus);

        int cristalNum = FindObjectOfType<CristalManager>().GetCurrentCristals();
        SetCristalNumberAs(cristalNum);

        // ⭐ 생존 시간 업적 (일반 모드)
        if (AchievementManager.Instance != null)
        {
            StageTime stageTime = FindObjectOfType<StageTime>();
            if (stageTime != null)
            {
                int survivedMinutes = Mathf.FloorToInt(stageTime.GetElapsedTime() / 60f);
                if (survivedMinutes > 0)
                    AchievementManager.Instance.AddSurviveMinutes(survivedMinutes, false);
            }
        }

        FindObjectOfType<PauseManager>().PauseGame();
    }

    // 최고 웨이브 기록, 최고 생존 시간 기록, 골드, 크리스탈 기록 저장
    public void SaveInfiniteModeResources()
    {
        InfiniteStageManager infiniteManager = FindObjectOfType<InfiniteStageManager>();
        int currentWave = infiniteManager.GetCurrentWave();
        int clearedWaves = infiniteManager.GetClearedWaves();
        float currentTime = infiniteManager.GetSurvivalTime();

        // 최고 기록 갱신 체크
        bool isNewRecord = currentTime > playerData.bestSurvivalTime;

        if (currentWave > playerData.bestWave) SetBestWave(currentWave);
        if (currentTime > playerData.bestSurvivalTime) SetSurvivalTime(currentTime);

        // CoinManager 값 + 무한모드 골드 합산
        CoinManager coinManager = FindObjectOfType<CoinManager>();
        if (coinManager != null)
        {
            int coinNum = coinManager.GetCurrentCoins();
            int killGold = GoldRewardManager.Instance.GetKillGold();
            int infiniteGold = GoldRewardManager.Instance.CalculateInfiniteGold(clearedWaves, isNewRecord) + killGold;
            SetCoinNumberAs(coinNum + infiniteGold);
        }

        CristalManager cristalManager = FindObjectOfType<CristalManager>();
        if (cristalManager != null)
        {
            int cristalNum = cristalManager.GetCurrentCristals();
            SetCristalNumberAs(cristalNum);
        }

        // ⭐ 생존 시간 업적 (무한 모드)
        if (AchievementManager.Instance != null)
        {
            int survivedMinutes = Mathf.FloorToInt(currentTime / 60f);
            if (survivedMinutes > 0)
                AchievementManager.Instance.AddSurviveMinutes(survivedMinutes, true);
        }

        PauseGame();

        InfiniteMissionTracker tracker = FindObjectOfType<InfiniteMissionTracker>();
        tracker?.OnGameEnd();
    }

    // 패배 시 생존 시간만 업적에 누적 (코인/크리스탈 저장 없음)
    public void SaveSurviveTimeOnGameOver()
    {
        if (AchievementManager.Instance == null) return;

        if (currentGameMode == GameMode.Regular)
        {
            StageTime stageTime = FindObjectOfType<StageTime>();
            if (stageTime != null)
            {
                int survivedMinutes = Mathf.FloorToInt(stageTime.GetElapsedTime() / 60f);
                if (survivedMinutes > 0)
                    AchievementManager.Instance.AddSurviveMinutes(survivedMinutes, false);
            }
        }
        else
        {
            InfiniteStageManager infiniteManager = FindObjectOfType<InfiniteStageManager>();
            if (infiniteManager != null)
            {
                int survivedMinutes = Mathf.FloorToInt(infiniteManager.GetSurvivalTime() / 60f);
                if (survivedMinutes > 0)
                    AchievementManager.Instance.AddSurviveMinutes(survivedMinutes, true);
            }
        }
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

    // 경과 시간만큼 번개 회복 계산 (오프라인 회복 포함)
    void ApplyLightningRegen()
    {
        if (playerData.currentLightningNumber >= maxLightningNumber)
        {
            // 꽉 차 있을 땐 디스크에 안 쓰고 메모리에서만 시각 갱신
            playerData.lastLightningUpdateTicks = DateTime.UtcNow.Ticks;
            return;
        }

        DateTime lastTime = playerData.lastLightningUpdateTicks > 0
            ? new DateTime(playerData.lastLightningUpdateTicks)
            : DateTime.UtcNow;

        TimeSpan elapsed = DateTime.UtcNow - lastTime;
        int recoveredAmount = Mathf.FloorToInt((float)elapsed.TotalSeconds / lightningRechargeSeconds);

        if (recoveredAmount > 0)
        {
            playerData.currentLightningNumber = Mathf.Min(
                playerData.currentLightningNumber + recoveredAmount, maxLightningNumber);

            // 나머지 시간(못 채운 초)은 버리지 않고 다음 회복에 이어지도록
            int usedSeconds = recoveredAmount * lightningRechargeSeconds;
            playerData.lastLightningUpdateTicks = lastTime.AddSeconds(usedSeconds).Ticks;

            if (playerData.currentLightningNumber >= maxLightningNumber)
                playerData.lastLightningUpdateTicks = DateTime.UtcNow.Ticks;

            SavePlayerData();
            NotifyCurrencyChanged();
        }
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

    public void ReloadFromDisk()
    {
        LoadPlayerData();
        Logger.Log("[PlayerDataManager] 디스크에서 데이터 재로드 완료");
    }

    public void UnlockInfiniteMode()
    {
        if (playerData.isInfiniteModeUnlocked) return;
        playerData.isInfiniteModeUnlocked = true;
        SavePlayerData();
        Logger.Log("[PlayerDataManager] 무한모드 해금 완료");
    }

    // --- 첫 크리스탈 구매 보너스 ---
    public bool HasClaimedFirstCristalBonus() => playerData.firstCristalBonusClaimed;
    public void SetFirstCristalBonusClaimed(bool claimed)
    {
        playerData.firstCristalBonusClaimed = claimed;
        SavePlayerData();
    }

    // ⭐ 추가: 첫 번째 동료 슬롯 해금 안내
    public bool HasShownFirstCompanionSlotAnnouncement() => playerData.firstCompanionSlotAnnouncementShown;
    public void SetFirstCompanionSlotAnnouncementShown(bool shown)
    {
        playerData.firstCompanionSlotAnnouncementShown = shown;
        SavePlayerData();
    }

    // ⭐ 추가: 동료 슬롯별 "새로 해금됨" 배지 확인 여부
    public bool IsCompanionSlotBadgeSeen(int companionIndex)
    {
        if (playerData.companionSlotBadgeSeen == null) return false;
        if (companionIndex < 0 || companionIndex >= playerData.companionSlotBadgeSeen.Length) return false;
        return playerData.companionSlotBadgeSeen[companionIndex];
    }

    public void SetCompanionSlotBadgeSeen(int companionIndex)
    {
        if (playerData.companionSlotBadgeSeen == null || playerData.companionSlotBadgeSeen.Length < 4)
            playerData.companionSlotBadgeSeen = new bool[4];
        if (companionIndex < 0 || companionIndex >= playerData.companionSlotBadgeSeen.Length) return;
        playerData.companionSlotBadgeSeen[companionIndex] = true;
        SavePlayerData();
    }
}