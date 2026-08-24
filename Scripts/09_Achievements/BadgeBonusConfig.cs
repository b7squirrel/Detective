using UnityEngine;

/// <summary>
/// 카테고리별, 단계별 배지 스탯 보너스 퍼센트를 담는 공용 설정.
/// Character.cs(스탯 계산)와 BadgeEarnedPopup.cs(팝업 표시)가 이 하나의 에셋을 공유해서 참조한다.
/// 배열 인덱스 = 단계(0번째 = 1단계, 1번째 = 2단계, ...) - 나중에 단계가 늘어나면 배열 길이만 늘리면 됨.
/// </summary>
[CreateAssetMenu(fileName = "BadgeBonusConfig", menuName = "Badge/Bonus Config")]
public class BadgeBonusConfig : ScriptableObject
{
    [Header("배지 단계별 스탯 보너스 (%) - 인덱스 0=1단계, 1=2단계...")]
    public float[] attackPercents = new float[] { 0.5f, 1f };
    public float[] armorPercents = new float[] { 0.5f, 1f };
    public float[] speedPercents = new float[] { 0.5f, 1f };
    public float[] magnetPercents = new float[] { 0.5f, 1f };
    public float[] criticalPercents = new float[] { 0.5f, 1f };
    public float[] knockbackPercents = new float[] { 0.5f, 1f };

    float[] GetTierArray(BadgeCategory category)
    {
        switch (category)
        {
            case BadgeCategory.Attack: return attackPercents;
            case BadgeCategory.Armor: return armorPercents;
            case BadgeCategory.Speed: return speedPercents;
            case BadgeCategory.Magnet: return magnetPercents;
            case BadgeCategory.Critical: return criticalPercents;
            case BadgeCategory.Knockback: return knockbackPercents;
            default: return null;
        }
    }

    // Character.cs가 스탯 계산할 때 사용 - 받은 배지 개수만큼 단계별 값을 전부 합산
    public float GetTotalPercent(BadgeCategory category, int earnedCount)
    {
        float[] tiers = GetTierArray(category);
        if (tiers == null) return 0f;

        float total = 0f;
        int n = Mathf.Min(earnedCount, tiers.Length);
        for (int i = 0; i < n; i++) total += tiers[i];
        return total;
    }

    // BadgeEarnedPopup.cs가 "방금 받은 배지 하나"의 증가분만 표시할 때 사용 (tierIndex는 0부터 시작)
    public float GetPercentAtTier(BadgeCategory category, int tierIndex)
    {
        float[] tiers = GetTierArray(category);
        if (tiers == null || tierIndex < 0 || tierIndex >= tiers.Length) return 0f;
        return tiers[tierIndex];
    }
}