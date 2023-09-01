using UnityEngine;
using DG.Tweening;
using System.Collections;

public enum SlotType { Field, Up, Mat, None };
public class SlotAction : MonoBehaviour
{
    [SerializeField] protected SlotType currentSlotType;
    protected UpPanelManager upPanelManager;

    void Awake()
    {
        upPanelManager = FindAnyObjectByType<UpPanelManager>();
    }

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
        slotRec.DOScale(initialValue, .07f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(.066f);

        ActionType();
        
    }
    void ActionType()
    {
        if (currentSlotType == SlotType.Field)
        {
            upPanelManager.AcquireCard(GetComponent<CardSlot>().GetCardData());
            return;
        }
        if (currentSlotType == SlotType.Up)
        {
            upPanelManager.GetIntoAllField();
            GetComponent<CardSlot>().EmptySlot();
            return;
        }
        if (currentSlotType == SlotType.Mat)
        {
            upPanelManager.BackToMatField();
            GetComponent<CardSlot>().EmptySlot();
            return;
        }
        if (currentSlotType == SlotType.None)
            return;
    }
}
