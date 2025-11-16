using System.Collections.Generic;
using UnityEngine;
using System;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("업적 리스트 (SO)")]
    public List<AchievementSO> achievementSOList;

    // 런타임 저장소
    public Dictionary<string, RuntimeAchievement> runtimeDict = new();

    // 글로벌 이벤트
    public event Action<RuntimeAchievement> OnAnyProgressChanged;
    public event Action<RuntimeAchievement> OnAnyCompleted;
    public event Action<RuntimeAchievement> OnAnyRewarded;

    [SerializeField] GemCollectFX gemCollectFX;

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

    private void Initialize()
    {
        foreach (var so in achievementSOList)
        {
            RuntimeAchievement ra = new RuntimeAchievement(so);

            // 개별 이벤트 = 매니저 이벤트로 다시 전달
            ra.OnProgressChanged += runtime =>
                OnAnyProgressChanged?.Invoke(runtime);

            ra.OnCompleted += runtime =>
                OnAnyCompleted?.Invoke(runtime);

            runtimeDict.Add(so.id, ra);
        }

        if(gemCollectFX == null) gemCollectFX = FindObjectOfType<GemCollectFX>();
    }

    // ID 기반 진행 증가
    public void AddProgressByID(string id, int amount = 1)
    {
        if (runtimeDict.TryGetValue(id, out var ra))
        {
            ra.AddProgress(amount);
            SaveAchievement(ra);
        }
    }

    // 타입 기반 진행 증가
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

    // 보상 지급
    public void Reward(string id, RectTransform pos)
    {
        if (!runtimeDict.TryGetValue(id, out var ra)) return;
        if (ra.isRewarded) return;

        ra.Reward(); // "보상을 받았음"으로 체크해서 보상이 중복되지 않도록
        SaveAchievement(ra);

        OnAnyRewarded?.Invoke(ra);

        gemCollectFX.PlayGemCollectFX(pos, ra.original.rewardGem);
    }

    public void SaveAchievement(RuntimeAchievement ra)
    {
        string id = ra.original.id;
        PlayerPrefs.SetInt("ACH_" + id, ra.isCompleted ? 1 : 0);
        PlayerPrefs.SetInt("ACH_PROGRESS_" + id, ra.progress);
        PlayerPrefs.SetInt("ACH_REWARD_" + id, ra.isRewarded ? 1 : 0);
    }

    // 전체 조회
    public List<RuntimeAchievement> GetAll()
    {
        return new List<RuntimeAchievement>(runtimeDict.Values);
    }

    // 디버그 버튼으로 호출
    // 모든 도전과제 초기화
    public void ResetAllAchievements()
    {
        foreach (var ra in runtimeDict.Values)
        {
            // 런타임 값 초기화
            ra.progress = 0;
            ra.isCompleted = false;
            ra.isRewarded = false;

            // PlayerPrefs 초기화
            string id = ra.original.id;
            PlayerPrefs.SetInt("ACH_" + id, 0);
            PlayerPrefs.SetInt("ACH_PROGRESS_" + id, 0);
            PlayerPrefs.SetInt("ACH_REWARD_" + id, 0);

            // 이벤트 호출 (원하면)
            OnAnyProgressChanged?.Invoke(ra);
        }
        
        PlayerPrefs.Save();
    }
}