using UnityEngine;

public enum AchievementType
{
    SURVIVE,
    KILL,
    WEAPON,
    SHOP,
    PLAY,
    ETC,
    WAVE,
    STAGE_CLEAR,   // ← 추가
    STAGE_REACH,   // ← 추가
    BOSS_DEFEAT,   // ← 추가
    AD_DRAW        // ← 추가
}

public enum RewardType
{
    GEM,
    COIN,
    ENERGY,
    NONE,
    BADGE       // ⭐ 추가: 배지 보상
}

// ⭐ 추가: 배지 카테고리 (Character.cs 스탯 6종과 1:1 매칭)
public enum BadgeCategory
{
    None,       // 배지가 아닌 업적(기존 GEM/COIN/ENERGY 보상용)에는 이 값을 씀
    Attack,     // 공격력 (DamageBonus)
    Armor,      // 방어력 (Armor)
    Speed,      // 속도 (MoveSpeed)
    Magnet,     // 자력 (MagnetSize)
    Critical,   // 치명타 (CriticalDamageChance)
    Knockback   // 넉백 (knockBackChance)
}

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Achievement/New Achievement", order = 0)]
public class AchievementSO : ScriptableObject
{
    [Header("기본 정보")]
    public string id;                 // 업적 고유 ID (다국어 Key로 사용)

    // 일일 퀘스트 구분
    [Tooltip("true면 일일 퀘스트 (매일 리셋), false면 영구 업적")]
    public bool isDailyQuest = false;

    [Tooltip("true면 주간 퀘스트 (매주 월요일 리셋)")]
    public bool isWeeklyQuest = false;

    [Tooltip("true면 무한모드 전용 임무 (업적 탭에서 숨김)")]
    public bool isInfiniteMode = false;

    // 레거시 필드 (더 이상 사용 안 함, 하지만 기존 데이터 보존용으로 남겨둠)
    [HideInInspector] public string title;
    [HideInInspector] public string description;

    public string icon;               // UI 아이콘

    [Header("보상 정보")]
    public int rewardNum;             // 보상 (보석 개수 등)
    public RewardType rewardType;     // 보상 타입

    // ⭐ 추가: rewardType이 BADGE일 때만 의미 있는 필드
    [Tooltip("rewardType이 BADGE일 때, 이 업적이 어떤 스탯 카테고리 배지인지 지정")]
    public BadgeCategory badgeCategory = BadgeCategory.None;

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