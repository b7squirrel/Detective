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
        // 터치하면 일단 또잉또잉
        RectTransform slotRec = GetComponent<RectTransform>();
        float initialValue = slotRec.transform.localScale.x;
        slotRec.transform.localScale = new Vector2(initialValue * 1.1f, initialValue * 1.1f);
        slotRec.DOScale(initialValue, .04f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(.066f);

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
