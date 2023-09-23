using UnityEngine;
using DG.Tweening;
using System.Collections;

public enum LaunchSlotType { Field, Up, None }
public class LaunchSlotAction : MonoBehaviour
{
    [SerializeField] protected LaunchSlotType currentSlotType;
    [SerializeField] LaunchManager launchManager;

    public void Onclick()
    {
        StartCoroutine(OnClickCo());
    }
    IEnumerator OnClickCo()
    {
        // 터치하면 일단 또잉또잉
        RectTransform slotRec = GetComponent<RectTransform>();
        float initialValue = slotRec.transform.localScale.x;
        slotRec.transform.localScale = new Vector2(initialValue * 1.3f, initialValue * 1.3f);
        slotRec.DOScale(initialValue, .07f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(.066f);

        ActionType();
    }
    
    void ActionType()
    {
        if (currentSlotType == LaunchSlotType.Up)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            launchManager.SetAllFieldTypeOf("Weapon", cardData);
            return;
        }
        if (currentSlotType == LaunchSlotType.Field)
        {
            CardData cardData = GetComponent<CardSlot>().GetCardData();
            return;
        }
        if (currentSlotType == LaunchSlotType.None)
            return;
    }
    public void SetSlotType(LaunchSlotType launchSlotType)
    {
        currentSlotType = launchSlotType;
    }
}
