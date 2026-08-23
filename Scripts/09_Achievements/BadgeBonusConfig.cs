using UnityEngine;

/// <summary>
/// 배지 1개당 스탯 보너스 퍼센트를 담는 공용 설정.
/// Character.cs(스탯 계산)와 BadgeEarnedPopup.cs(팝업 표시)가 이 하나의 에셋을 공유해서 참조한다.
/// 값이 바뀌어도 두 군데를 따로 수정할 필요 없이 이 에셋 하나만 바꾸면 됨.
/// </summary>
[CreateAssetMenu(fileName = "BadgeBonusConfig", menuName = "Badge/Bonus Config")]
public class BadgeBonusConfig : ScriptableObject
{
    [Header("배지 1개당 스탯 보너스 (%)")]
    public float attackPercent = 0.5f;
    public float armorPercent = 0.5f;
    public float speedPercent = 0.5f;
    public float magnetPercent = 0.5f;
    public float criticalPercent = 0.5f;
    public float knockbackPercent = 0.5f;

    public float GetPercent(BadgeCategory category)
    {
        switch (category)
        {
            case BadgeCategory.Attack: return attackPercent;
            case BadgeCategory.Armor: return armorPercent;
            case BadgeCategory.Speed: return speedPercent;
            case BadgeCategory.Magnet: return magnetPercent;
            case BadgeCategory.Critical: return criticalPercent;
            case BadgeCategory.Knockback: return knockbackPercent;
            default: return 0f;
        }
    }
}