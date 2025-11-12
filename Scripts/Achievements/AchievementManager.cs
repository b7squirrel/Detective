using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("업적 리스트")]
    public List<AchievementSO> achievements; // 에디터에서 모든 업적 ScriptableObject 할당

    private void Awake()
    {
        // 싱글톤 + DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 업적 진행도 불러오기
    private void LoadAchievements()
    {
        foreach (var ach in achievements)
        {
            ach.isCompleted = PlayerPrefs.GetInt("ACH_" + ach.id, 0) == 1;
            ach.currentValue = PlayerPrefs.GetInt("ACH_PROGRESS_" + ach.id, 0);
        }
    }

    // 업적 진행도 저장
    private void SaveAchievement(AchievementSO ach)
    {
        PlayerPrefs.SetInt("ACH_" + ach.id, ach.isCompleted ? 1 : 0);
        PlayerPrefs.SetInt("ACH_PROGRESS_" + ach.id, ach.currentValue);
    }

    /// <summary>
    /// 업적 진행 추가
    /// id로 해당 업적 찾아서 AddProgress 호출
    /// </summary>
    public void AddProgress(string id, int amount = 1)
    {
        AchievementSO ach = achievements.Find(x => x.id == id);
        if (ach == null || ach.isCompleted) return;

        ach.AddProgress(amount);

        // 진행도 저장
        SaveAchievement(ach);
    }

    /// <summary>
    /// 업적이 완료되면 호출되는 함수
    /// 보상 지급 및 팝업 처리
    /// </summary>
    public void OnAchievementCompleted(AchievementSO ach)
    {
        // // 1. 보상 지급
        // CurrencyManager.Instance.AddGem(ach.rewardAmount);

        // // 2. 저장
        // SaveAchievement(ach);

        // // 3. UI 팝업 표시
        // PopupUI.Instance.ShowAchievement(ach.title, ach.icon);

        // Debug.Log($"🏆 업적 달성: {ach.title} (+{ach.rewardAmount} 보석)");
    }

    /// <summary>
    /// 업적 전체 리스트 반환 (UI에서 사용)
    /// </summary>
    public List<AchievementSO> GetAchievements()
    {
        return achievements;
    }

    /// <summary>
    /// 테스트용: 모든 업적 초기화
    /// </summary>
    [ContextMenu("Reset All Achievements")]
    public void ResetAllAchievements()
    {
        foreach (var ach in achievements)
        {
            ach.currentValue = 0;
            ach.isCompleted = false;
        }

        Debug.Log("✅ 모든 업적 초기화 완료!");
    }
}
