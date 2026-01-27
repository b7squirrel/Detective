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
    NONE
}

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Achievement/New Achievement", order = 0)]
public class AchievementSO : ScriptableObject
{
    [Header("기본 정보")]
    public string id;                 // 업적 고유 ID (다국어 Key로 사용)

    // 일일 퀘스트 구분
    [Tooltip("true면 일일 퀘스트 (매일 리셋), false면 영구 업적")]
    public bool isDailyQuest = false;
    
    // 레거시 필드 (더 이상 사용 안 함, 하지만 기존 데이터 보존용으로 남겨둠)
    [HideInInspector] public string title;              
    [HideInInspector] public string description;
    
    public string icon;               // UI 아이콘

    [Header("보상 정보")]
    public int rewardNum;             // 보상 (보석 개수 등)
    public RewardType rewardType;     // 보상 타입

    [Header("진행 정보")]
    public AchievementType type;      // 업적 타입
    public int targetValue;           // 목표 값
    
    // 다국어 제목/설명 가져오기 (런타임에서만 사용)
    public string GetLocalizedTitle()
    {
        if (LocalizationManager.Achievement == null)
            return id;
        return LocalizationManager.Achievement.GetTitle(id);
    }
    
    public string GetLocalizedDescription()
    {
        if (LocalizationManager.Achievement == null)
            return "";
        return LocalizationManager.Achievement.GetDescription(id);
    }
}