using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning($"[DailyRewardButton] Animator가 없습니다!");
        }
    }

    public void UpdateDailyRewardButton(int coinReward, int cristalReward, int currentDay, int buttonDay, bool hasClaimed)
    {
        Debug.Log($"[DailyRewardButton] 버튼 {buttonDay} 업데이트: 현재일={currentDay}, 수령={hasClaimed}");
        
        // // postIt 날짜 표시
        // if (dayText != null)
        // {
        //     dayText.text = buttonDay.ToString();
        //     Debug.Log($"[DailyRewardButton] {buttonDay}일차 dayText 설정");
        // }
        // else
        // {
        //     Debug.LogError($"[DailyRewardButton] X 버튼 {buttonDay} dayText가 null!");
        // }
        
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
                if(anim != null) anim.SetTrigger("Ready");
                bool shouldShowPostIt = !hasClaimed;
                
                postIt.SetActive(shouldShowPostIt);

                // circle은 포스트잇이 보일 때만 (즉, 아직 안 받았을 때만)
                if (availableIcons != null)
                {
                    availableIcons.SetActive(shouldShowPostIt);
                }

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
                if(anim != null) anim.SetTrigger("Idle");                
                // circle 숨김 (아직 수령할 날이 아님)
                if (availableIcons != null)
                {
                    availableIcons.SetActive(false);
                }
                
                Debug.Log($"[DailyRewardButton] {buttonDay}일차 포스트잇 (미래): true, circle: false");
            }
        }
        else
        {
            Debug.LogError($"[DailyRewardButton] 버튼 {buttonDay} postIt이 null!");
        }
    }

    // ⭐ 새로 추가: 포스트잇 떨어지는 애니메이션 재생
    public IEnumerator PlayClaimAnimation()
    {
        if (anim != null)
        {
            Debug.Log($"[DailyRewardButton] Claim 애니메이션 시작");
            anim.SetTrigger("Claim");
            
            // 애니메이션 길이만큼 대기 (애니메이션 클립 길이에 맞게 조정)
            yield return new WaitForSeconds(0.4f); // 애니메이션 길이에 맞게 수정
        }
        else
        {
            Debug.LogWarning($"[DailyRewardButton] Animator 없음, 애니메이션 스킵");
            yield return null;
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