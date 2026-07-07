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
    [SerializeField] Sprite lightningIcon;

    public void Bind(RuntimeAchievement ra)
    {
        titleText.text = ra.GetTitle();
        rewardText.text = ra.original.rewardNum.ToString();

        if (rewardIcon != null)
        {
            switch (ra.original.rewardType)
            {
                case RewardType.GEM:
                    rewardIcon.sprite = gemIcon;
                    break;
                case RewardType.COIN:
                    rewardIcon.sprite = coinIcon;
                    break;
                case RewardType.ENERGY: // ← 추가
                    rewardIcon.sprite = lightningIcon;
                    break;
            }
        }
    }
}