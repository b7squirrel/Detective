using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeeklyRewardItemUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] Image rewardIcon;

    [Header("보상 아이콘")]
    [SerializeField] Sprite gemIcon;
    [SerializeField] Sprite coinIcon;

    public void Bind(RuntimeAchievement ra)
    {
        titleText.text = ra.GetTitle();
        rewardText.text = ra.original.rewardNum.ToString();

        if (rewardIcon != null)
        {
            rewardIcon.sprite = ra.original.rewardType == RewardType.GEM
                ? gemIcon
                : coinIcon;
        }
    }
}