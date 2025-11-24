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
public enum RewardType
{
    GEM,
    COIN,
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
    public int rewardNum;             // 보상 (보석 개수 등)
    public RewardType rewardType;     // 보상 타입


    [Header("진행 정보")]
    public AchievementType type;      // 업적 타입
    public int targetValue;           // 목표 값
}
