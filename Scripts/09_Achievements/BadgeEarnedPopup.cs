using UnityEngine;
using TMPro;

/// <summary>
/// 배지 보상을 받는 순간(AchievementManager.OnAnyRewarded, rewardType == BADGE) 팝업으로
/// "어떤 스탯이 몇 % 오르는지"와 함께, 로비 전시용 배지 비주얼을 그대로 생성해서 보여준다.
/// 활성화/비활성화는 전부 PanelTween이 담당 (SetActive를 이 스크립트에서 직접 건드리지 않음).
/// </summary>
public class BadgeEarnedPopup : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] PanelTween panelTween;      // ⭐ 변경: GameObject 대신 PanelTween 참조 (New Badge Popup 안의 "Panel")
    [SerializeField] TextMeshProUGUI titleText;  // 예: "새 배지 획득!"
    [SerializeField] TextMeshProUGUI statText;   // 예: "공격력 +0.5%"
    [SerializeField] Transform badgeVisualParent; // 계층상 "Badge" 오브젝트 - 여기 밑에 전시용 프리팹을 생성

    [Header("데이터")]
    [SerializeField] BadgeBonusConfig badgeBonusConfig;   // Character.cs와 같은 에셋을 연결
    [SerializeField] BadgeIconDatabase badgeIconDatabase; // BadgeDisplayManager와 같은 에셋을 연결

    GameObject spawnedBadgeVisual; // 이전에 생성한 비주얼을 지우기 위해 기억해둠

    void Awake()
    {
        if (panelTween != null) panelTween.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAnyRewarded += OnAnyRewarded;
    }

    void OnDisable()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAnyRewarded -= OnAnyRewarded;
    }

    void OnAnyRewarded(RuntimeAchievement ra)
    {
        if (ra.original.rewardType != RewardType.BADGE) return;
        Show(ra.original);
    }

    void Show(AchievementSO badge)
    {
        if (panelTween == null)
        {
            Logger.LogWarning("[BadgeEarnedPopup] panelTween이 연결되지 않았습니다.");
            return;
        }
        if (badgeBonusConfig == null)
        {
            Logger.LogWarning("[BadgeEarnedPopup] BadgeBonusConfig가 연결되지 않았습니다.");
            return;
        }

        float percent = badgeBonusConfig.GetPercent(badge.badgeCategory);
        string categoryName = GetCategoryDisplayName(badge.badgeCategory);

        if (titleText != null)
        {
            titleText.text = LocalizationManager.Game != null
                ? LocalizationManager.Game.badgeEarnedTitle
                : "새 배지 획득!"; // LocalizationManager 초기화 전 폴백
        }
        if (statText != null) statText.text = $"{categoryName} +{percent}%";

        SpawnBadgeVisual(badge.id);

        panelTween.ShowWithScale(); // ⭐ 변경: 활성화 + 튀는 애니메이션을 PanelTween이 전담
    }

    void SpawnBadgeVisual(string badgeId)
    {
        if (spawnedBadgeVisual != null)
        {
            Destroy(spawnedBadgeVisual);
            spawnedBadgeVisual = null;
        }

        if (badgeVisualParent == null)
        {
            Logger.LogWarning("[BadgeEarnedPopup] badgeVisualParent가 연결되지 않았습니다.");
            return;
        }
        if (badgeIconDatabase == null)
        {
            Logger.LogWarning("[BadgeEarnedPopup] BadgeIconDatabase가 연결되지 않았습니다.");
            return;
        }
        if (!badgeIconDatabase.TryGetPrefab(badgeId, out var prefab))
        {
            Logger.LogWarning($"[BadgeEarnedPopup] 배지 id '{badgeId}'에 매칭되는 프리팹이 없습니다.");
            return;
        }

        spawnedBadgeVisual = Instantiate(prefab, badgeVisualParent);
    }

    // LocalizationManager.Game의 배지 카테고리 표기 필드를 사용
    string GetCategoryDisplayName(BadgeCategory category)
    {
        if (LocalizationManager.Game == null) return category.ToString(); // 초기화 전 폴백

        switch (category)
        {
            case BadgeCategory.Attack: return LocalizationManager.Game.badgeCategoryAttack;
            case BadgeCategory.Armor: return LocalizationManager.Game.badgeCategoryArmor;
            case BadgeCategory.Speed: return LocalizationManager.Game.badgeCategorySpeed;
            case BadgeCategory.Magnet: return LocalizationManager.Game.badgeCategoryMagnet;
            case BadgeCategory.Critical: return LocalizationManager.Game.badgeCategoryCritical;
            case BadgeCategory.Knockback: return LocalizationManager.Game.badgeCategoryKnockback;
            default: return "";
        }
    }

    // 필요하면 코드에서 직접 닫을 때 사용 (지금은 Button Close가 PanelTween.HidePanel()을 직접 호출하므로 필수는 아님)
    public void ClosePopup()
    {
        if (panelTween != null) panelTween.HidePanel();
    }
}