using UnityEngine;
using DG.Tweening;
using System.Collections;

public class LaunchSlotAction : MonoBehaviour
{
    [SerializeField] protected LaunchSlotType currentSlotType;

    public void Onclick()
    {
        Debug.Log($"[LaunchSlotAction] Onclick 호출됨. gameObject={gameObject.name}, currentSlotType={currentSlotType}");
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
        Debug.Log($"[LaunchSlotAction] ActionType 실행. currentSlotType={currentSlotType}");

        if (currentSlotType == LaunchSlotType.Up)
    {
        CardData cardData = GetComponent<CardSlot>().GetCardData();
        Debug.Log($"[LaunchSlotAction] Up 분기 진입. cardData={(cardData == null ? "NULL" : cardData.Name)}");

        LaunchManager launchManager = GetComponentInParent<LaunchManager>();
        Debug.Log($"[LaunchSlotAction] GetComponentInParent<LaunchManager> 결과: {(launchManager == null ? "NULL!!" : "찾음")}");

        launchManager.SetAllFieldTypeOf("Weapon", cardData);
        return;
    }
        if (currentSlotType == LaunchSlotType.Field)
        {
            Debug.Log("[LaunchSlotAction] Field 분기 진입");
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            LaunchManager launchManager = GetComponentInParent<LaunchManager>();
            launchManager.UpdateLead(cardData);
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
}
