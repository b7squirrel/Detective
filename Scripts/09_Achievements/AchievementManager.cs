using System.Collections.Generic;
using UnityEngine;
using System;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("Resources 폴더 경로 (예: Resources/Achievements)")]
    [SerializeField] private string resourcePath = "Achievements";

    // 자동 로드된 업적 리스트
    public List<AchievementSO> achievementSOList = new();

    // 런타임 저장소
    public Dictionary<string, RuntimeAchievement> runtimeDict = new();

    // 글로벌 이벤트
    public event Action<RuntimeAchievement> OnAnyProgressChanged;
    public event Action<RuntimeAchievement> OnAnyCompleted;
    public event Action<RuntimeAchievement> OnAnyRewarded;

    [SerializeField] private GemCollectFX gemCollectFX;
    PlayerDataManager playerDataManager; // 크리스탈의 실제 값을 더해주기 위해

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        // ⭐ 일일 리셋 이벤트 구독
        DailyResetManager.OnDailyReset += ResetDailyQuests;
    }

    private void OnDisable()
    {
        // ⭐ 이벤트 구독 해제
        DailyResetManager.OnDailyReset -= ResetDailyQuests;
    }

    private void Initialize()
    {
        LoadAllSO();

        runtimeDict.Clear();

        foreach (var so in achievementSOList)
        {
            if (runtimeDict.ContainsKey(so.id))
            {
                Debug.LogError($"[AchievementManager] 중복 업적 ID: {so.id}");
                continue;
            }

            RuntimeAchievement ra = new RuntimeAchievement(so);

            ra.OnProgressChanged += r => OnAnyProgressChanged?.Invoke(r);
            ra.OnCompleted += r => OnAnyCompleted?.Invoke(r);

            runtimeDict.Add(so.id, ra);
        }

        if (gemCollectFX == null)
            gemCollectFX = FindObjectOfType<GemCollectFX>();
    }


    // ★ Resources 폴더에서 AchievementSO 자동 로딩
    private void LoadAllSO()
    {
        achievementSOList.Clear();

        AchievementSO[] loaded = Resources.LoadAll<AchievementSO>(resourcePath);

        if (loaded.Length == 0)
            Debug.LogWarning($"[AchievementManager] Resources/{resourcePath} 에 업적이 없습니다.");

        achievementSOList.AddRange(loaded);
    }


    public void AddProgressByID(string id, int amount = 1)
    {
        if (runtimeDict.TryGetValue(id, out var ra))
        {
            ra.AddProgress(amount);
            SaveAchievement(ra);
        }
    }

    public void AddProgress(AchievementType type, int amount = 1)
    {
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.type == type && !ra.isCompleted)
            {
                ra.AddProgress(amount);
                SaveAchievement(ra);
            }
        }
    }

    public void Reward(string id, RectTransform pos, RewardType rewardType)
    {
        if (!runtimeDict.TryGetValue(id, out var ra)) return;
        if (ra.isRewarded) return;

        ra.Reward(); // 보상을 수령했음을 기록
        SaveAchievement(ra);

        OnAnyRewarded?.Invoke(ra);

        // 먼저 실제 데이터에 보석 혹은 코인을 모두 추가 (UI 업데이트 없이)
        RewardType rType = rewardType;
        if (playerDataManager == null) playerDataManager = FindObjectOfType<PlayerDataManager>();

        if (rType == RewardType.GEM)
        {
            int currentValue = playerDataManager.GetCurrentCristalNumber();
            playerDataManager.SetCristalNumberAsSilent(currentValue + ra.original.rewardNum);

            if (gemCollectFX != null)
                gemCollectFX.PlayGemCollectFX(pos, ra.original.rewardNum, true);
        }
        else
        {
            int currentValue = playerDataManager.GetCurrentCoinNumber();
            playerDataManager.SetCoinNumberAsSilent(currentValue + ra.original.rewardNum);

            if (gemCollectFX != null)
                gemCollectFX.PlayGemCollectFX(pos, ra.original.rewardNum, false);
        }
    }

    public void SaveAchievement(RuntimeAchievement ra)
    {
        // ⭐ 동적 키 사용
        PlayerPrefs.SetInt(ra.GetCompleteKey(), ra.isCompleted ? 1 : 0);
        PlayerPrefs.SetInt(ra.GetProgressKey(), ra.progress);
        PlayerPrefs.SetInt(ra.GetRewardKey(), ra.isRewarded ? 1 : 0);
    }

    public List<RuntimeAchievement> GetAll()
    {
        return new List<RuntimeAchievement>(runtimeDict.Values);
    }

    // ⭐ 일일 퀘스트만 가져오기
    public List<RuntimeAchievement> GetDailyQuests()
    {
        List<RuntimeAchievement> dailyQuests = new();
        foreach (var ra in runtimeDict.Values)
        {
            if (ra.original.isDailyQuest)
                dailyQuests.Add(ra);
        }
        return dailyQuests;
    }

    // ⭐ 영구 업적만 가져오기
    public List<RuntimeAchievement> GetPermanentAchievements()
    {
        List<RuntimeAchievement> permanentAchievements = new();
        foreach (var ra in runtimeDict.Values)
        {
            if (!ra.original.isDailyQuest)
                permanentAchievements.Add(ra);
        }
        return permanentAchievements;
    }

    // ⭐ 일일 퀘스트 리셋 (매일 자정 호출됨)
    public void ResetDailyQuests()
    {
        Logger.Log("[AchievementManager] 일일 퀘스트 리셋 시작");
        
        foreach (var ra in runtimeDict.Values)
        {
            // 일일 퀘스트만 리셋
            if (ra.original.isDailyQuest)
            {
                ra.progress = 0;
                ra.isCompleted = false;
                ra.isRewarded = false;

                // PlayerPrefs에 저장
                PlayerPrefs.SetInt(ra.GetCompleteKey(), 0);
                PlayerPrefs.SetInt(ra.GetProgressKey(), 0);
                PlayerPrefs.SetInt(ra.GetRewardKey(), 0);

                // UI 갱신 이벤트
                OnAnyProgressChanged?.Invoke(ra);
            }
        }

        PlayerPrefs.Save();
        Logger.Log("[AchievementManager] 일일 퀘스트 리셋 완료");
    }

    // ⭐ 모든 업적 리셋 (디버그용)
    public void ResetAllAchievements()
    {
        foreach (var ra in runtimeDict.Values)
        {
            ra.progress = 0;
            ra.isCompleted = false;
            ra.isRewarded = false;

            PlayerPrefs.SetInt(ra.GetCompleteKey(), 0);
            PlayerPrefs.SetInt(ra.GetProgressKey(), 0);
            PlayerPrefs.SetInt(ra.GetRewardKey(), 0);

            OnAnyProgressChanged?.Invoke(ra);
        }

        PlayerPrefs.Save();
    }
}