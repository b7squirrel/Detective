using UnityEngine;

public enum AchievementType
{
    SURVIVE,
    KILL,
    WEAPON,
    SHOP,
    PLAY,
    ETC
}

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Achievement/New Achievement", order = 0)]
public class AchievementSO : ScriptableObject
{
    [Header("기본 정보")]
    public string id;                 // 업적 고유 ID
    public string title;              // 업적 이름
    [TextArea] public string description; // 설명
    public string icon;               // UI 아이콘

    [Header("보상 정보")]
    public int rewardGem;             // 보상 (보석 개수 등)

    [Header("진행 정보")]
    public AchievementType type;      // 업적 타입
    public int targetValue;           // 목표 값
    public int currentValue = 0;      // 현재 진행 상황
    public bool isCompleted = false;  // 달성 여부

    /// <summary>
    /// 업적에 진척이 생길 때 호출 (예: 적 처치 +1, 보석 획득 +1)
    /// </summary>
    public void AddProgress(int amount = 1)
    {
        if (isCompleted) return;

        currentValue += amount;

        if (currentValue >= targetValue)
        {
            currentValue = targetValue;
            CompleteAchievement();
        }
    }

    /// <summary>
    /// 업적 강제 완료 처리
    /// </summary>
    public void CompleteAchievement()
    {
        // if (isCompleted) return;

        // isCompleted = true;

        // // 업적 매니저 호출
        // AchievementManager.Instance.OnAchievementCompleted(this);
    }
}
