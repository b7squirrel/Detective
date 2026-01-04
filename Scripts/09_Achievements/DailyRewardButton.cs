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
    [SerializeField] GameObject availableIcons;
    
    Button dailyButton;
    Animator anim;

    public void UpdateDailyRewardButton(int coinReward, int cristalReward, int currentDay, int buttonDay, bool hasClaimed)
    {
        Debug.Log($"[DailyRewardButton] 버튼 {buttonDay} 업데이트: 현재일={currentDay}, 수령={hasClaimed}");
        
        // postIt 날짜 표시
        if (dayText != null)
        {
            dayText.text = buttonDay.ToString();
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
            if (currentDay == buttonDay) // 오늘
            {
                bool shouldShowPostIt = !hasClaimed;
                

                postIt.SetActive(shouldShowPostIt);

                // ⭐ circle은 포스트잇이 보일 때만 (즉, 아직 안 받았을 때만)
                if (availableIcons != null)
                {
                    availableIcons.SetActive(shouldShowPostIt);
                }

                // 포스트잇 랜덤 회전
                // if (shouldShowPostIt)
                // {
                //     ApplyRandomRotation(postIt);
                // }

                Debug.Log($"[DailyRewardButton] {buttonDay}일차 포스트잇: {shouldShowPostIt}, circle: {shouldShowPostIt}");
            }
            else if (currentDay > buttonDay) // 과거
            {
                postIt.SetActive(false);

                // circle도 숨김
                if (availableIcons != null)
                {
                    availableIcons.SetActive(false);
                }

                Debug.Log($"[DailyRewardButton] {buttonDay}일차 포스트잇 (과거): false");
    }
    else // 미래
    {
        postIt.SetActive(true);
        
        // circle 숨김 (아직 수령할 날이 아님)
        if (availableIcons != null)
        {
            availableIcons.SetActive(false);
        }
        
        // 포스트잇 랜덤 회전
        // ApplyRandomRotation(postIt);
        
        Debug.Log($"[DailyRewardButton] {buttonDay}일차 포스트잇 (미래): true, circle: false");
    }
}
else
{
    Debug.LogError($"[DailyRewardButton] 버튼 {buttonDay} postIt이 null!");
}
    }

    void ApplyRandomRotation(GameObject postIt)
    {
        // Z축 기준 -5도 ~ +5도 사이 랜덤 회전
        float randomAngle = Random.Range(-5f, 5f);

        // 기존 회전값 유지하고 Z축만 변경
        Vector3 currentRotation = postIt.transform.localEulerAngles;
        postIt.transform.localEulerAngles = new Vector3(
            currentRotation.x,
            currentRotation.y,
            randomAngle
        );
    }
}