using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DailyRewardButton : MonoBehaviour
{
    [SerializeField] GameObject coinRewardGroup;
    [SerializeField] GameObject cristalRewardGroup;
    [SerializeField] TextMeshProUGUI coinAmountText;
    [SerializeField] TextMeshProUGUI cristalAmountText;
    [SerializeField] GameObject postIt;
    [SerializeField] TextMeshProUGUI dayText;
    
    Button dailyButton;

    public void UpdateDailyRewardButton(int coinReward, int cristalReward, int currentDay, int buttonDay, bool hasClaimed)
    {
        Debug.Log($"[DailyRewardButton] 버튼 {buttonDay} 업데이트: 현재일={currentDay}, 수령={hasClaimed}");
        
        // postIt 날짜 표시
        if (dayText != null)
        {
            dayText.text = buttonDay.ToString() + "일차";
            Debug.Log($"[DailyRewardButton] {buttonDay}일차 dayText 설정");
        }
        else
        {
            Debug.LogError($"[DailyRewardButton] X 버튼 {buttonDay} dayText가 null!");
        }
        
        // 코인 보상 표시
        bool hasCoin = coinReward > 0;
        if (coinRewardGroup != null)
        {
            coinRewardGroup.SetActive(hasCoin);
            Debug.Log($"[DailyRewardButton] {buttonDay}일차 코인그룹: {hasCoin}");
        }
        
        if (hasCoin && coinAmountText != null)
            coinAmountText.text = coinReward.ToString();
        
        // 크리스탈 보상 표시
        bool hasGem = cristalReward > 0;
        if (cristalRewardGroup != null)
        {
            cristalRewardGroup.SetActive(hasGem);
            Debug.Log($"[DailyRewardButton] {buttonDay}일차 크리스탈그룹: {hasGem}");
        }
        
        if (hasGem && cristalAmountText != null)
            cristalAmountText.text = cristalReward.ToString();
        
        // 버튼 상태
        dailyButton = GetComponent<Button>();
        if (dailyButton != null)
        {
            bool isClickable = (currentDay == buttonDay) && !hasClaimed;
            dailyButton.interactable = isClickable;
            Debug.Log($"[DailyRewardButton] {buttonDay}일차 버튼 클릭가능: {isClickable}");
        }
        else
        {
            Debug.LogError($"[DailyRewardButton] X 버튼 {buttonDay} Button 컴포넌트 없음!");
        }
        
        // 포스트잇 표시
        if (postIt != null)
        {
            if (currentDay == buttonDay)
            {
                postIt.SetActive(!hasClaimed);
                Debug.Log($"[DailyRewardButton] {buttonDay}일차 포스트잇 (오늘): {!hasClaimed}");
            }
            else if (currentDay > buttonDay)
            {
                postIt.SetActive(false);
                Debug.Log($"[DailyRewardButton] {buttonDay}일차 포스트잇 (과거): false");
            }
            else
            {
                postIt.SetActive(true);
                Debug.Log($"[DailyRewardButton] {buttonDay}일차 포스트잇 (미래): true");
            }
        }
        else
        {
            Debug.LogError($"[DailyRewardButton] X 버튼 {buttonDay} postIt이 null!");
        }
    }
}