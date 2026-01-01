using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DailyRewardButton : MonoBehaviour
{
    [SerializeField] GameObject coinRewardGroup; // 코인 보상 그룹
    [SerializeField] GameObject cristalRewardGroup; // 크리스탈 보상 그룹
    [SerializeField] TextMeshProUGUI coinAmountText; // 코인 수
    [SerializeField] TextMeshProUGUI cristalAmountText; // 크리스탈 수
    [SerializeField] GameObject postIt;
    [SerializeField] TextMeshProUGUI dayText;
    Button dailyButton;

    public void UpdateDailyRewardButton(int coinReward, int CristalRewards, int currentDay, int buttonDay, bool hasClaimed)
    {
        // postIt 날짜 표시
        dayText.text = buttonDay.ToString();

        // 코인 보상 표시
        bool hasCoin = coinReward > 0;
        coinRewardGroup.SetActive(hasCoin);
        if (hasCoin && coinAmountText != null) coinAmountText.text = coinReward.ToString();

        // 크리스탈 보상 표시
        bool hasGem = CristalRewards > 0;
        cristalRewardGroup.SetActive(hasGem);
        if (hasGem && cristalAmountText != null) cristalAmountText.text = CristalRewards.ToString();

        // 날짜별 포스트잇 설정
        dailyButton = GetComponent<Button>();
        if (currentDay == buttonDay) // 해당 일자
        {
            postIt.SetActive(!hasClaimed);
            Logger.LogError($"{buttonDay}일자 버튼 {!hasClaimed}, 포스트잇 : {!hasClaimed}");
        }
        else if (currentDay > buttonDay) // 지난 날짜들은 포스트잇 제거. 버튼 비활성화
        {
            postIt.SetActive(false);
            Logger.LogError($"현재 날짜 : {currentDay}, {buttonDay}일자 포스트잇 : 비활성화");
        }
        else // 지나지 않은 날짜들은 포스트잇 활성화. 버튼 비활성화.
        {
            postIt.SetActive(true);
            Logger.LogError($"현재 날짜 : {currentDay}, {buttonDay}일자 포스트잇 : 활성화");
        }
    }
}
