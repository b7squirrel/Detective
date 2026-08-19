using UnityEngine;
using DG.Tweening;
using System.Collections;

public class LaunchSlotAction : MonoBehaviour
{
    [SerializeField] protected LaunchSlotType currentSlotType;

    // ⭐ 추가: 이 슬롯이 스쿼드에서 몇 번 자리인지 (Up 타입 슬롯에서만 사용)
    // 0 = 리드, 1~4 = 동료 1~4번. 인스펙터에서 슬롯마다 직접 지정.
    [SerializeField] int squadSlotIndex = 0;

    public void Onclick()
    {
        Debug.Log($"[LaunchSlotAction] Onclick 호출됨. gameObject={gameObject.name}, currentSlotType={currentSlotType}, squadSlotIndex={squadSlotIndex}");
        StartCoroutine(OnClickCo());
    }
    IEnumerator OnClickCo()
    {
        RectTransform slotRec = GetComponent<RectTransform>();
        float initialValue = slotRec.transform.localScale.x;

        // 부드럽게 크기 증가 후 감소
        Sequence clickSequence = DOTween.Sequence();
        clickSequence.Append(slotRec.DOScale(initialValue * 1.1f, 0.08f).SetEase(Ease.OutQuad))
        .Append(slotRec.DOScale(initialValue, 0.12f).SetEase(Ease.OutBack));

        RectTransform cardRec = GetComponent<RectTransform>();
        FindObjectOfType<CardEffect>().SetEffectPosition(cardRec);

        // 전체 애니메이션 완료까지 대기
        yield return new WaitForSeconds(0.2f);

        ActionType();
    }

    void ActionType()
    {
        Debug.Log($"[LaunchSlotAction] ActionType 실행. currentSlotType={currentSlotType}, squadSlotIndex={squadSlotIndex}");

        if (currentSlotType == LaunchSlotType.Up)
        {
            // ⭐ 변경: 리드/동료 슬롯(Up)을 탭하면 자신의 squadSlotIndex와 함께 LaunchManager에 알림
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            Debug.Log($"[LaunchSlotAction] Up 분기 진입. cardData={(cardData == null ? "NULL" : cardData.Name)}");

            LaunchManager launchManager = GetComponentInParent<LaunchManager>();
            Debug.Log($"[LaunchSlotAction] GetComponentInParent<LaunchManager> 결과: {(launchManager == null ? "NULL!!" : "찾음")}");

            launchManager.OpenPickerForSlot(squadSlotIndex, cardData);
            return;
        }
        if (currentSlotType == LaunchSlotType.Field)
        {
            // ⭐ 변경: 필드 안의 카드를 고르면, 지금 편집 중인 슬롯(리드든 동료든)에 배정하도록 LaunchManager에 위임
            Debug.Log("[LaunchSlotAction] Field 분기 진입");
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            LaunchManager launchManager = GetComponentInParent<LaunchManager>();
            launchManager.AssignPickedCard(cardData);
            return;
        }
        if (currentSlotType == LaunchSlotType.None)
        {
            Debug.LogWarning("[LaunchSlotAction] currentSlotType이 None입니다. 아무 동작도 하지 않습니다.");
            return;
        }
    }
    public void SetSlotType(LaunchSlotType launchSlotType)
    {
        currentSlotType = launchSlotType;
    }
    public LaunchSlotType GetSlotType()
    {
        return currentSlotType;
    }

    // ⭐ 추가: 필요 시 코드에서 동적으로 슬롯 번호를 재설정할 수 있도록 제공 (기본은 인스펙터 값 사용)
    public void SetSquadSlotIndex(int index)
    {
        squadSlotIndex = index;
    }
    public int GetSquadSlotIndex()
    {
        return squadSlotIndex;
    }
}