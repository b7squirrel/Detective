using System.Collections.Generic;
using UnityEngine;

// 런타임용 업적 데이터
[System.Serializable]
public class RuntimeAchievement
{
    public AchievementSO originalSO; // 원본 참조
    public int currentValue;
    public bool isCompleted;

    public RuntimeAchievement(AchievementSO so)
    {
        originalSO = so;
        currentValue = 0;
        isCompleted = false;
    }

    public void AddProgress(int amount = 1)
    {
        if (isCompleted) return;

        currentValue += amount;
        if (currentValue >= originalSO.targetValue)
        {
            currentValue = originalSO.targetValue;
            isCompleted = true;
        }
    }
}

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("업적 리스트 (SO)")]
    public List<AchievementSO> achievements; // 에디터에서 할당

    // 런타임 인스턴스
    private Dictionary<string, RuntimeAchievement> runtimeAchievements = new Dictionary<string, RuntimeAchievement>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeRuntimeAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // SO → RuntimeAchievement 생성 및 저장된 진행도 로드
    private void InitializeRuntimeAchievements()
    {
        foreach (var so in achievements)
        {
            RuntimeAchievement runtime = new RuntimeAchievement(so);

            // 저장된 값 불러오기
            runtime.isCompleted = PlayerPrefs.GetInt("ACH_" + so.id, 0) == 1;
            runtime.currentValue = PlayerPrefs.GetInt("ACH_PROGRESS_" + so.id, 0);

            runtimeAchievements[so.id] = runtime;
        }
    }

    // 진행도 저장
    private void SaveAchievement(RuntimeAchievement runtime)
    {
        PlayerPrefs.SetInt("ACH_" + runtime.originalSO.id, runtime.isCompleted ? 1 : 0);
        PlayerPrefs.SetInt("ACH_PROGRESS_" + runtime.originalSO.id, runtime.currentValue);
    }

    // 진행 추가
    public void AddProgress(string id, int amount = 1)
    {
        if (!runtimeAchievements.TryGetValue(id, out var runtime)) return;
        if (runtime.isCompleted) return;

        runtime.AddProgress(amount);

        SaveAchievement(runtime);

        if (runtime.isCompleted)
        {
            OnAchievementCompleted(runtime);
        }
    }

    // 업적 완료 처리
    private void OnAchievementCompleted(RuntimeAchievement runtime)
    {
        var so = runtime.originalSO;

        // 예: 보상 지급, UI 팝업
        Debug.Log($"🏆 업적 달성: {so.title} (+{so.rewardGem} 보석)");
    }

    // UI용 전체 리스트 반환
    public List<RuntimeAchievement> GetAllRuntimeAchievements()
    {
        return new List<RuntimeAchievement>(runtimeAchievements.Values);
    }

    // 테스트용: 모든 업적 초기화
    [ContextMenu("Reset All Achievements")]
    public void ResetAllAchievements()
    {
        foreach (var runtime in runtimeAchievements.Values)
        {
            runtime.currentValue = 0;
            runtime.isCompleted = false;
            SaveAchievement(runtime);
        }

        Debug.Log("✅ 모든 업적 초기화 완료!");
    }
}