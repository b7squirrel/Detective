using UnityEngine;
using DG.Tweening;
using System.Collections;

public class LaunchSlotAction : MonoBehaviour
{
    [SerializeField] protected LaunchSlotType currentSlotType;

    public void Onclick()
    {
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

        // 전체 애니메이션 완료까지 대기
        yield return new WaitForSeconds(0.2f);

        ActionType();
    }

    void ActionType()
    {
        if (currentSlotType == LaunchSlotType.Up)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            LaunchManager launchManager = GetComponentInParent<LaunchManager>();
            launchManager.SetAllFieldTypeOf("Weapon", cardData);
            return;
        }
        if (currentSlotType == LaunchSlotType.Field)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            LaunchManager launchManager = GetComponentInParent<LaunchManager>();
            launchManager.UpdateLead(cardData);
            return;
        }
        if (currentSlotType == LaunchSlotType.None)
            return;
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
