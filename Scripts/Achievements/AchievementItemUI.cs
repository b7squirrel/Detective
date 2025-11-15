using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItemUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] TextMeshProUGUI rewardAmount;
    [SerializeField] Image rewardIcon;

    public void InitAchievementItem(string description, int rewardAmount, Image rewardIcon)
    {
        this.description.text = description;
        this.rewardAmount.text = rewardAmount.ToString();
        this.rewardIcon = rewardIcon;
    }
}
